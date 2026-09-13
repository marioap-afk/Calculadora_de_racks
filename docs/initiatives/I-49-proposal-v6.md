# I-49 — Proposal V6: motor de expresiones paramétricas (Expression Engine, ID22B)

> # ⚠ COORDINATOR PROPOSAL V6 — ARCHITECT FINAL RE-REVIEW V5 RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V6
> Coordinator      = AGREED WITH V6
> Architect        = PENDING FINAL CHECK
>
> CR-1..CR-5       = CLOSED
> FR-1             = CLOSED
> RR-1             = CLOSED IN V6
>
> A-03             = CLOSED
> Open A           = CLOSED
> Open B           = CLOSED
> Schema           = V-0
>
> ADR              = REQUIRED, not yet written
> Implementation   = BLOCKED
> ```
>
> Gate **G2F**. V6 reconcilia la **Architect Final Re-review de V5** (`Architect = CHANGES REQUIRED`) sobre la base
> exacta de V5. La re-revisión dio FR-1 = SATISFIED, la regla de dependencias transitivas = ACCEPTED y N-1 = SATISFIED,
> y encontró un único hallazgo nuevo, **RR-1 (LOW, material)**: `RepairBrokenRack` no protegía las premisas que
> deciden qué fuentes elimina. El Coordinador **acepta** RR-1 (§0.1).
>
> - V6 cambia **únicamente** cómo el `PlanReadSet` protege las fuentes que retira `RepairBrokenRack` y los textos y
>   pruebas que derivan directamente de ello, más el cambio opcional de vocabulario de N-1, sin cambio de semántica.
> - CR-1…CR-5, FR-1 y N-1 siguen cerrados. Ninguna otra decisión arquitectónica se reabre (§0.2, §0.11).
>
> ```text
> Base V5          = 2783accd0c9400ed1308ab5c40072eadcd388f36   (I-49-proposal-v5.md; CI 34733545685 success)
> Proposal V4      = ccf21c69d9aa6e8d9b622e6389fb61f430d53f03   (historial)
> Proposal V3      = 4df9480638bbeb9a95549a690db098b7f32a28ea   (historial)
> Proposal V2      = 9aef7d04d9e65b4225af2cb36750ee636a6689d5   (historial)
> Proposal V1      = b15e40a076d7157ce8ca73537af9659049bf8576   (historial)
> Discovery G1     = 4cf02b167f183fb66d93988c9843296838277dc4   (I-49-discovery.md)
> Código auditado  = a4d88f18a1f42263d366c44dc05dd18a6786f152   (base de la rama; la rama no contiene producción)
> main observado   = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (merge de I-50; cambia 4 archivos citados, §12.1)
> Override vigente = OWNER_OVERRIDE_I49_I50_PARALLEL   (docs/automation/decisions/I-49.md)
> Paralelas        = §12 (tips observados al abrir G2F)
> Estado de gates  = G0 · G1 HECHAS · V1–V5 historial · G2F V6 EN CURSO · G3 PENDIENTE
> ```

---

## 0. V5 → Architect Final Re-review → V6 Reconciliation

```text
CR-1..CR-5 = CLOSED
FR-1       = CLOSED
N-1        = CLOSED
RR-1       = ACCEPTED / CLOSED IN V6
```

V6 no reabre ninguna otra decisión.

### 0.1 Tabla de reconciliación de RR-1

| # | Severidad | Regla V5 | Hallazgo | Decisión del Architect | Decisión del Coordinador | Regla V6 final |
|---|---|---|---|---|---|---|
| **RR-1** | LOW (material) | P21.6: regla de derivación y fila de `RepairBrokenRack`; T-V4-09 | V5 exige observar todo resultado que el preflight usa «para decidir el plan», pero la fila de `RepairBrokenRack` solo observaba lo que leen las fuentes **conservadas**. Las premisas que deciden qué fuentes se **eliminan** (P24.6: `Upstream`, `Domain`, `Intrinsic` o id ausente) quedaban sin proteger. Si el id roto reaparecía entre preflight y commit, el commit borraba `Holgura + 2`, que ya evaluaba a 6, y dejaba gobernar el literal congelado | Proteger la premisa de cada fuente retirada, dejar explícito el caso del id ausente, ajustar T-V4-09 y añadir pruebas | **ACEPTADA** | `PlanReadObservation = SymbolResultObservation \| RepairDecisionObservation`. Cada fuente que retira `RepairBrokenRack` lleva una `RepairDecisionObservation {RackId, PropertyId, ExpectedRepairReason}`, producida por la misma inspección pura del preflight (`InspectBinding`, P24.6). En commit se recomputa sobre la re-lectura acreditada, antes de validar las fuentes conservadas; si la decisión difiere → `ABORT BEFORE WRITE`. Las razones se comparan estructuradas, sin texto: `MissingTarget(ids)`, `Intrinsic(firma)`, `Upstream(firma de causa raíz)` y `Domain`. Las fuentes conservadas siguen con las `SymbolResultObservation` de V5, y la regla transitiva no cambia (§0.3, P21.6, P21.10, P24.6, §5.2, §7.2, T-V4-09, T-V6-01 a T-V6-09) |

**Resultado**: RR-1 queda cerrado en V6. No queda nada abierto.

**Vocabulario de N-1** (opcional, sin cambio de semántica): «Condición de promesa» pasa a «Condición para mostrar la
sugerencia de recuperación», y «promete» pasa a «indica» (§5.2 decisión 8, §7.2, P22.9, T-V4-06, §8.3 punto 14 y
§9). N-1 sigue SATISFIED.

### 0.2 Veredictos de la re-revisión final que V6 conserva

| Tema | Veredicto de la re-revisión final | Estado en V6 | Dónde |
|---|---|---|---|
| FR-1: modelo de observación, antes y después, comparación de `Failed`, `Delete`, `UnlinkAllAndDelete`, fallos previos de R2 y planes sin `RegistryMutation` | SATISFIED | CLOSED, sin cambio | §0.6, P20.8, P21.6, §7.1, T-V5-01 a T-V5-09 |
| Regla transitiva: las dependencias de un símbolo observado no se observan aparte | ACCEPTED | Sin cambio. V6 la aplica también a las decisiones de reparación: se compara la razón, no cada valor intermedio | P21.6, ALT-41, T-V6-08 |
| N-1: «permitiría» no garantiza la corrección | SATISFIED | CLOSED; solo cambia el vocabulario opcional | §7.2 |
| CR-1…CR-5 y decisiones cerradas | Sin regresiones | Sin cambio | §0.11 |
| RR-1: premisas de las fuentes que retira `RepairBrokenRack` | LOW, material | CLOSED IN V6 | §0.1, §0.3, P21.6 |
| Estado paralelo | Coordinación, sin cambio de contrato | Actualizado en §12 | §12 |

### 0.3 `RepairDecisionObservation` — premisas de las fuentes que retira la reparación **[V6 · RR-1]**

**Qué cambia.** `RepairBrokenRack` es la única operación cuyo plan destruye datos authored a partir de una
clasificación semántica: retira las fuentes que `InspectBinding` (P24.6) clasificó como reparables. V5 protegía lo que
leen las fuentes conservadas, pero no la premisa que decidía retirar las demás. V6 amplía el modelo de observación:

```text
PlanReadObservation
    = SymbolResultObservation      // V5, sin cambio: SymbolId, Phase, ExpectedResult
    | RepairDecisionObservation    // V6: la decisión de retirar una fuente

RepairDecisionObservation
{
    RackId,
    PropertyId,
    ExpectedRepairReason
}

ExpectedRepairReason
    = MissingTarget(missingIds)
    | Intrinsic(stableDiagnosticSignature)
    | Upstream(rootCauseSignature)
    | Domain
```

- **Regla final de derivación.** Todo dato o clasificación semántica que el preflight usó para justificar una
  decisión destructiva del plan queda representado por una observación comparable hasta el commit. En
  `RepairBrokenRack`, cada fuente que se **elimina** lleva una `RepairDecisionObservation`, y cada fuente que se
  **conserva** y cuyo efectivo importa lleva las `SymbolResultObservation` de lo que realmente lee, como en V5. No se
  añaden observaciones porque sí.
- **Una sola autoridad.** La razón esperada la produce la misma inspección pura que usó el preflight, sobre la misma
  entrada de fuente, que el plan transporta como dato. En commit se recomputa con esa inspección sobre la re-lectura
  acreditada: no hay una segunda autoridad y no se re-ejecuta el preflight.
- **Comparación estructurada.** Nunca se compara texto en español, texto del formatter, posiciones, mensajes de UI ni
  la traza opt-in.
- **Por razón:**
  - `MissingTarget(ids)`: si siguen ausentes exactamente esos ids, coincide. Si reaparece alguno, desaparece otro o
    cambia la clasificación, no coincide y aborta. Un id ausente no se convierte en símbolo ni en
    `SymbolResultObservation`.
  - `Upstream`: si la variable leída sigue fallando por la misma causa raíz estructurada, coincide. Si se recupera o
    cambia la causa raíz, aborta. No se observan aparte sus dependencias transitivas.
  - `Intrinsic`: si la firma estable del fallo propio sigue igual, coincide aunque cambien valores intermedios
    (`A / (B - C)` pasa de `B = C = 5` a `B = C = 6` y sigue en `DivisionByZero`). Si cambia la firma o la fuente
    evalúa, aborta.
  - `Domain`: si el efectivo sigue fuera de dominio, coincide. Si vuelve al dominio (`A - 10` con A de 7 a 15),
    aborta: no se borra una fórmula que ya volvió a ser válida.
- **Orden en commit.** Re-lectura y acreditación; recomputar cada `RepairDecisionObservation`; validar las
  `SymbolResultObservation` de las fuentes conservadas; escribir la `RackMutation`. Todo en la misma transacción y
  antes de la primera escritura de destino.
- **T-V4-09.** Toda reparación retira al menos una fuente, así que lleva al menos una `RepairDecisionObservation` y
  re-lee, aunque no conserve ningún símbolo. Solo un plan sin ninguna observación prescinde de la re-lectura.

El contrato normativo completo está en P21.6.

### 0.4 V4 → Architect Final Review → V5 Reconciliation (heredada de V5)

La tabla de V5 se conserva como historial de la reconciliación de FR-1 y N-1. Sus reglas siguen vigentes; V6 solo
añade las decisiones de reparación de §0.3.

| # | Severidad | Regla V4 | Hallazgo | Decisión del Architect | Decisión del Coordinador | Regla V5 final |
|---|---|---|---|---|---|---|
| **FR-1** | MEDIUM | P21.6 (contenido, valor esperado y pseudocódigo de commit), P21.2 paso 9, §5.2 decisión 9, P27.2 | El commit abortaba si **cualquier** símbolo del `PlanReadSet` fallaba, y comparaba contra valores esperados tomados solo de «después». **(a)** Contradecía R2: un fallo previo de `I` con la definición intacta, que el preflight admite, abortaba siempre el commit. **(b)** Rompía las eliminaciones: X está en `I` pero no existe en «después», así que `Delete(X)` y `UnlinkAllAndDelete(X)` abortaban siempre; y si se excluía X, el valor que `UnlinkAllAndDelete` materializa, leído en «antes», quedaba sin proteger | Cada entrada con su lado (antes o después) y su resultado esperado (`Success(valor)` o `Failed(código)`); comparar en el mismo lado en que se leyó; abortar solo si el resultado difiere; un fallo idéntico no aborta; un `Delete` sin racks solo incluye lo que el preflight leyó; alinear P21.2, §5.2 y las pruebas | **ACEPTADA** | El `PlanReadSet` es un conjunto de **observaciones** `(SymbolId, Phase = Before \| After, ExpectedResult = Success(valor) \| Failed(fallo tipado))` que el preflight **realmente** usó. En commit, las `Before` se evalúan sobre la re-lectura acreditada, antes de `ApplyTo`, y las `After` sobre el documento cambiado. Se aborta antes de escribir **solo si el resultado observado difiere del esperado**; un fallo idéntico no aborta. `Delete(X)` no lleva observaciones; `UnlinkAllAndDelete(X)` protege el valor materializado con `Before(X)` y nunca tiene `After(X)`. Las dependencias transitivas de un símbolo observado no se observan aparte, porque su resultado ya las incorpora (§0.6, DR-8, P19.7, P20.8, P21.2, P21.3, P21.6, §5.2, §7.1, P27.2, T-V5-01 a T-V5-09) |
| **N-1** | No material | §7.2, puntos 2 y 6 | «Permitiría» se definía como «R1 no bloqueará la corrección». Es demasiado fuerte: R1 sigue aplicando los dominios de las propiedades con los valores nuevos | Precisar la redacción sin cambiar la regla | **ACEPTADA** | «Permitiría» solo significa que ninguna otra fuente inválida conocida mantiene por sí sola bloqueada la recuperación. La corrección concreta sigue sujeta a la validación normal del estado resultante: R1 con los dominios, R2, contrato raíz y `PlanReadSet`. R1 no se relaja (§7.2) |

**Resultado**: FR-1 quedó cerrado en V5 y N-1 incorporada. La re-revisión final de V5 los dio por satisfechos (§0.2).

### 0.5 Veredictos de la revisión final de V4 que V5 conservó (heredado de V5)

| Tema | Veredicto de la revisión final | Estado en V5 | Dónde |
|---|---|---|---|
| CR-1 — autoridad de unidades solo con conversiones genéricas | SATISFIED | CLOSED, sin cambio | P9.5–P9.7, P28.4, T-V4-10 |
| CR-2 — contrato raíz con casos A–D | SATISFIED | CLOSED, sin cambio | §0.9, P8.10, T-V4-01 |
| CR-3 — `MaxBoundExpressionDepth = 24` | SATISFIED | CLOSED, sin cambio | P1.9, T-V4-02 a T-V4-04 |
| CR-4 — mensaje de recuperación condicional | SATISFIED | CLOSED; N-1 precisa la redacción de §7.2 sin cambiar la regla | §7.2, T-V4-05, T-V4-06, T-V4-11 |
| CR-5 — revisión acotada de V8-R05 | SATISFIED | CLOSED: no cambian la regla exacta, el alcance con y sin `RegistryMutation`, las exclusiones ni la exigencia de ADR y aceptación del Owner. FR-1 corrige cómo se forman y se comparan las lecturas | DR-8, §2.3, P21.6, §9, §13, T-V3-10, T-V4-07 a T-V4-09 |
| Decisiones cerradas: A-03, OPEN A, OPEN B, V-0, operaciones, modelo híbrido, `=X`, unidades, funciones, `#GUID`, tokens, aislamiento de estado, `RegistryEvaluation`, costuras, un plan y un commit | Coherentes, salvo la interacción de FR-1 con R2 y con las eliminaciones | Sin cambio | §0.11 |
| Estado paralelo: I-50 ya edita `RackSelectiveWindow.xaml/.cs`, I-53 lo tocará en su G5 e I-49 en su G10 | Nota de coordinación, no hallazgo | Actualizado en §12, sin cambio de contrato | §12 |

### 0.6 `PlanReadSet` — observaciones con fase y resultado esperado **[V5 · FR-1]**

**Qué cambia.** En V4, el `PlanReadSet` era una lista plana de símbolos con valores esperados tomados de «después», y
el commit abortaba si cualquiera fallaba. En V5 es el conjunto de **observaciones que el preflight realmente usó**:

```text
PlanReadObservation
{
    SymbolId,
    Phase:          Before | After,
    ExpectedResult: Success(double value) | Failed(typedFailure)
}
```

- **`Before`**: un resultado que el preflight usó **antes** de aplicar la `RegistryMutation`. En commit se evalúa
  sobre la re-lectura acreditada, antes de `ApplyTo`.
- **`After`**: un resultado que el preflight usó sobre el estado hipotético «después». En commit se evalúa sobre el
  documento cambiado. Aquí están, entre otros, los valores con los que se construyen los racks finales.
- **El mismo `SymbolId`** puede tener una observación en cada fase si el plan realmente lo usa en las dos.
- **Solo lo usado.** No se mete de forma automática todo `I` en las dos fases. Las precondiciones de la mutación
  (legibilidad, acreditación, identidad y existencia de los ids que la operación necesita) conservan su propio
  contrato y no se convierten en lecturas de valor. Las dependencias transitivas de un símbolo observado tampoco se
  observan aparte: su resultado ya las incorpora, y V4 las enumeraba en la lista plana (ALT-41).
- **Comparación por resultado.** `Success` compara el valor exacto. `Failed` compara el código y sus datos
  estructurados estables, como los ids y la causa. Nunca se compara texto localizado.
- **Regla de commit: si el resultado observado difiere del esperado → `ABORT BEFORE WRITE`.** Un fallo no aborta por
  sí mismo. El mismo fallo tipado coincide. Abortan `Failed → Success`, `Success → Failed`, `Success(5) → Success(6)`,
  `Failed(causa A) → Failed(causa B)` y un símbolo que ya no existe en esa fase.
- **Eliminaciones.** `Delete(X)` no lee ningún valor: su plan no lleva observaciones, no hay `After(X)` y el commit
  termina con normalidad. `UnlinkAllAndDelete(X)` materializa el valor de X leído **antes** de eliminarlo y lo protege
  con `Before(X, Success(valor materializado))`: si X cambia o desaparece, aborta; si no, hace commit. Nunca hay
  `After(X)`.
- **R2 con fallos previos.** Con `X = 5` y `D = X + #<id-roto>`, D falla antes y después por la misma causa.
  `ChangeDefinition(X, 6)` pasa R2, y el plan guarda el fallo de D como resultado esperado. Si D sigue idéntico, el
  commit escribe. Si D se recupera o falla por otra causa, aborta antes de escribir.
- **Lo que no cambia de CR-5**: la regla exacta, que V5 solo precisa («resultado observado» donde decía «valor»); el
  alcance con y sin `RegistryMutation`; la ausencia de hash, token global, comparación completa, locks y rechazo por
  cambios fuera del `PlanReadSet`; y la exigencia de ADR y de aceptación del Owner.

El contrato normativo completo, con la regla de derivación por operación, la comparación de fallos y el pseudocódigo
de commit, está en P21.6.

**[V6 · RR-1]** V6 amplía este modelo con `RepairDecisionObservation`, que protege la premisa de cada fuente que
retira `RepairBrokenRack` (§0.3). Las `SymbolResultObservation` descritas aquí no cambian.

### 0.7 V3 → Architect Re-review → V4 Reconciliation (heredada de V4)

La tabla de V4 se conserva como historial de la reconciliación de CR-1…CR-5. Sus reglas siguen vigentes; la de CR-5
se aplica con la semántica de observaciones de §0.6.

| # | Severidad | Regla V3 | Hallazgo | Decisión del Architect | Decisión del Coordinador | Regla V4 final |
|---|---|---|---|---|---|---|
| **CR-1** | MEDIUM | P9.6, P9.7, guarda de unidades de P28.4, G6 y G11, §12.1, §13, R12, §10.2 | V3 amplió A-13 a `FootInches`, `CommercialFoot` y el `12.0` de Cantilever, que son constantes de dominio. Tocaba sistemas que P30.15 excluye, añadía una colisión con I-50 y proponía una guarda sobre «factores de 12» imposible de verificar | Retirar la ampliación: alias solo en `StructuralSectionUnits`, las tres constantes siguen locales y la guarda se limita | **ACEPTADA** | La autoridad neutral contiene solo `MillimetersPerInch = 25.4` e `InchesPerFoot = 12`; `StructuralSectionUnits` conserva su API y alía sus dos factores; `FootInches`, `CommercialFoot` y el `12.0` de Cantilever quedan locales; la guarda prohíbe solo parsers o tokens de unidad, un `25.4` duplicado y una segunda autoridad genérica pies↔pulgadas (P9.6, P9.7, P28.4, G6, §12, §13) |
| **CR-2** | LOW | P8.10, P21.2 paso 6 | Un símbolo del cierre que fallaba y obtiene valor tras la mutación podía quedar con raíz `≤ 0` sin que ninguna regla lo bloqueara | Añadir el caso: el valor nuevo cumple el contrato raíz o el plan queda vacío | **ACEPTADA** | Casos A–D del contrato raíz; el escenario `A` roto → `B = -90` falla el preflight (P8.10, P21.2, T-V3-16, T-V4-01) |
| **CR-3** | LOW | P1.9, P1.10, P17.6 | «Anidamiento» no estaba definido, y una cadena binaria larga sin paréntesis podía aceptarse y no ser serializable | Profundidad normativa del `BoundExpression` contando cada nodo; guardas del parser separadas; prueba del peor caso por descubrimiento de los puntos de serialización | **ACEPTADA** | `MaxBoundExpressionDepth = 24` con definición exacta y ejemplos normativos; validación tras el bind y al leer; guardas del parser aparte (P1.9, P1.10, P17.6, T-V4-02 a T-V4-04) |
| **CR-4** | LOW | §7.2 (aviso de recuperación), P21.3 R1, P24.12 | El aviso «corregir X recuperaría esta fórmula» podía ser falso: R1 bloquea la corrección mientras el rack conserve otras fuentes inválidas | Aviso condicional; no relajar R1 | **ACEPTADA** | Mensaje de recuperación solo si es demostrable en el estado diagnosticado; si no, mensaje de bloqueo con la fórmula canónica (§7.2, P21.2 paso 8, P22.9, T-V3-13, T-V4-05, T-V4-06, T-V4-11) |
| **CR-5** | LOW | P21.6, P21.12, fila de V8-R01 en §2.3 | V3 afirmaba que la política V8-R05 de I-48 quedaba intacta; el `PlanReadSet` la revisa, también para planes solo con referencias directas | Declarar la revisión acotada en §2.3 y §9 | **ACEPTADA** | Revisión **acotada** de V8-R05: un valor que participó en el `PlanReadSet` y cambia entre preflight y commit aborta antes de escribir, con o sin `RegistryMutation`; nada más; exige ADR y aceptación del Owner antes de implementar (DR-8, §2.3, P21.6, P21.12, §5.2, §9, §13, T-V3-10, T-V4-07 a T-V4-09) |

**Resultado**: 5 de 5 cerrados en V4. Ninguno queda abierto. La revisión final de V4 los dio por satisfechos (§0.5).

### 0.8 Veredictos de la re-revisión de V3 que V4 conservó (heredado de V4)

| Tema | Veredicto de la re-revisión | Estado en V4 | Dónde |
|---|---|---|---|
| A-01…A-12, A-14…A-18 | SATISFIED | Sin cambio | §0.10 |
| A-13 | NOT SATISFIED (ampliación de V3) | **ACCEPTED sin ampliación** por CR-1 | §0.10, P9.7 |
| A-03 modificado | ACCEPTED | CLOSED; CR-2 completa su aplicación en propagación | §0.9, P8.10 |
| P1 `#GUID` solo diagnóstico | ACCEPTED | Sin cambio | P7.1, §4 |
| P2 contrato raíz en propagación | CHANGES REQUIRED | Cerrado por CR-2 | P8.10 |
| P3 profundidad 24 | CHANGES REQUIRED | Cerrado por CR-3 | P1.9 |
| P4 `PlanReadSet` sin `RegistryMutation` | CHANGES REQUIRED | Cerrado por CR-5 | P21.6 |
| P5 autoridad de unidades ampliada | CHANGES REQUIRED | Cerrado por CR-1 | P9.7 |
| P6 tabla de tokens de `Type` | ACCEPTED | Sin cambio | P17.10 |

### 0.9 A-03 — remedio modificado (ACCEPTED; CR-2 completa su aplicación)

**No hay dominio físico dentro del motor de expresiones.** La validez se reparte en tres fronteras:

```text
Expression Engine
  → cualquier double finito; sin dominio físico

ProjectVariable adapter
  → aplica el contrato vigente de su VariableType al RESULTADO RAÍZ

Property consumer
  → aplica el dominio de esa propiedad al valor efectivo
```

- **El motor sigue siendo adimensional.** Los valores intermedios de una expresión pueden ser negativos o nulos.
  `VariableType.Length` **no** gobierna ninguna álgebra.
- **Una variable definida por expresión no puede saltarse las reglas de validez que tendría definida por
  literal.** La validez no depende de la sintaxis. Por eso **no se admite** `A = -2` como expresión cuando el mismo
  `A = -2` literal sería inválido bajo el contrato vigente.
- **Contrato vigente de `Length`** (hechos):
  - al leer, el store y la acreditación exigen un valor **finito** (Discovery §11.4);
  - al escribir desde el producto, la ventana de RACKVARIABLES exige **`> 0`** («El valor tiene que ser un
    número mayor que cero», `RackProjectVariablesWindow.xaml.cs:215-224`; Discovery §10.3, §11.4).
- **Cómo lo aplica V6** (P8.10) **[V4 · CR-2]**. Para cada símbolo del cierre `I`, igual para literal y expresión:

  | Caso | Antes | Después | Regla |
  |---|---|---|---|
  | **A** | Símbolo mutado X (`Create`, `ChangeDefinition`) | `Success` | La raíz de X cumple el contrato |
  | **B** | `Success` y cumplía el contrato | `Success` | Tiene que seguir cumpliéndolo |
  | **C** | `Failed` o sin valor | `Success` | El valor nuevo **tiene que** cumplir el contrato; si no, `OutOfRange` y plan vacío |
  | **D** | Valor evaluable que ya incumplía por corrupción externa, con la definición intacta | `Success` | No bloquea por sí sola; R1 sigue impidiendo que un efectivo inválido llegue a una propiedad |

  La comprobación de la ventana se conserva como aviso temprano. **Al leer**, el contrato histórico (finito) se
  aplica igual a las dos sintaxis: ningún valor persistido se re-valida con más rigor por ser expresión, ni ningún
  literal con más rigor que hoy.
- **Variables escalares o con signo**: si hacen falta en el futuro, exigen **otro `VariableType` u otra
  iniciativa**. I-49 no las introduce de forma implícita.
- **Propiedades** (P24.5): el dominio de la propiedad se aplica al valor efectivo de las tres fuentes,
  preservando lo histórico donde aplica:

  | Fuente | Al escribir (editor, reconciliación) | Al resolver lo persistido (lectura, preflight, BOM) |
  |---|---|---|
  | `Literal` | **Ruta histórica**: la ventana rechaza `< 0` al construir el diseño (`RackSelectiveWindow.xaml.cs:2481-2505`); un literal canónico escrito con `=` sigue la misma regla (A-18) | **Heredado**: no se re-valida |
  | `ProjectVariableReference` | Dominio sobre el efectivo | Dominio sobre el efectivo: `OutOfRange`. **Endurecimiento declarado**: una referencia a una variable negativa, solo alcanzable por edición externa, deja de resolver y falla cerrada; su geometría existente no se toca |
  | `Expression` | Dominio sobre el efectivo: aviso en la sesión y autoridad en el reconciliador | Dominio sobre el efectivo: `OutOfRange` |

- **Ejemplos de la regla:**

  | Caso | Resultado V6 |
  |---|---|
  | `Ajuste = -2` escrito como literal | Rechazado: contrato de `Length` (caso A) |
  | `Ajuste = AlturaA - AlturaB` con raíz `-2` | Rechazado: mismo contrato en la raíz (caso A) |
  | `AlturaFinal = Base + (AlturaA - AlturaB)` con intermedio `-2` y raíz `> 0` | Válido: el motor no tiene dominio |
  | `palletTolerance = A - 10` con efectivo `-3` | `OutOfRange` del consumidor |
  | Cambiar `A` de modo que una variable dependiente de `I`, que cumplía, quede con raíz `≤ 0` | Preflight rechazado: `OutOfRange` de esa variable (caso B) |
  | **Escenario del Architect**: `A` con referencia rota, `B = A - 100`, `AlturaFinal = B + 200`; `ChangeDefinition(A, "=Base")` con `Base = 10` | `A = 10` cumple, pero `B` pasa de `Failed` a `-90`: **el preflight falla** con `OutOfRange` de `B` y plan vacío (caso C) |
  | Raíz `≤ 0` persistida por edición externa, literal o expresión | Se lee como hoy en las dos sintaxis; con su definición intacta no bloquea por sí sola, y nunca llega a una propiedad fuera de dominio (caso D) |

### 0.10 V2 → Architect Review → Coordinator Reconciliation (heredada de V3)

La tabla de V3 se conserva como historial de la reconciliación de A-01…A-18. En V4 solo cambió A-13, que quedó
**ACCEPTED sin ampliación**. En V5 solo se precisó la regla vigente de A-05 (FR-1), y en V6 esa regla gana las
decisiones de reparación (RR-1).

| # | Severidad (Architect) | Regla V2 | Decisión del Architect | Decisión del Coordinador | Regla vigente en V6 |
|---|---|---|---|---|---|
| **A-01** | BLOCKER | V2 §5.4 (cuarto punto), P24.6, P21.10, P14.8 | Retirar `FatalUnevaluable` como estado no reparable: un fallo semántico de una fuente es reparable por rack y solo lo estructural bloquea | **ACEPTADA** | P24.6, P21.10, §5, §7.2 |
| **A-02** | HIGH | P20.4 | `UnlinkAllAndDelete` no convierte fórmulas en literales: bloquea si una fórmula referencia X | **ACEPTADA**, con el contrato final de §7.1 | P20.4, §7.1 |
| **A-03** | HIGH | P21.7, P17.7 | Sin dominio en variables; dominio de la propiedad sobre el efectivo de toda fuente no literal | **ACEPTADA EN PRINCIPIO y MODIFICADA**; la re-revisión la **acepta** y CR-2 completa su aplicación en propagación (§0.9) | P5.5, P8.10, P21.7, P23.6, P24.5 |
| **A-04** | HIGH | P22.7 | Enlazar contra el snapshot del workspace; el intent lleva ids; preflight y commit validan ids | **ACEPTADA** | P7.7, P22.7 |
| **A-05** | MEDIUM | P21.6 | La re-lectura de commit compara el conjunto leído del plan, no solo `I` | **ACEPTADA**: `PlanReadSet` contractual, declarado en V4 como revisión acotada de V8-R05 (CR-5) y formado en V5 por observaciones con fase y resultado esperado (FR-1); en V6 protege además la premisa de cada fuente que retira `RepairBrokenRack` (RR-1) | P21.6 |
| **A-06** | MEDIUM | P5.1 | Quitar `ConsumerType` del núcleo | **ACEPTADA** | P5.1, P8.9, P26.1 |
| **A-07** | MEDIUM | P2.8, P17.6 | Una forma no canónica es error semántico, no estructural | **ACEPTADA** | P2.8, P17.6–P17.7, §5 |
| **A-08** | MEDIUM | P17.6 | Campos desconocidos en `expression` = error estructural; regla de evolución | **ACEPTADA con alcance precisado**: solo dentro de los payloads de Expression; la política histórica de `ExtensionData` fuera de ellos no cambia | P17.6, P17.12, §6.2–§6.3 |
| **A-09** | MEDIUM | P23.15 | No fijar `"2.0"`: calcular la versión con `SelectiveDesignSchema.ResolveWriteVersion` | **ACEPTADA** como preservación de invariante, no como deuda lateral | P23.15 |
| **A-10** | MEDIUM | P24.6, P21.10 | Confirmación de reparación consciente de la fuente | **ACEPTADA**, en la misma operación `RepairBrokenRack`; CR-4 hace condicional el aviso de recuperación | §7.2, P22.9 |
| **A-11** | LOW | P18.4–P18.5 | Justificar V-0 por el comportamiento de los lectores, no por los archivos tocados | **ACEPTADA** | P18.3–P18.7, §6 |
| **A-12** | LOW | P21.9 | P21.9 contradecía P23.15: materializar se limita a `UnlinkAllAndDelete` y a la exportación; el reconciliador escribe el literal tecleado | **ACEPTADA** | P21.9 |
| **A-13** | LOW | P9.7 | `StructuralSectionUnits` toma sus factores de la autoridad neutral; `FootInches` y `CommercialFoot` no cambian | **ACEPTADA sin ampliación** **[V4 · CR-1]**: se retira la ampliación de V3 | P9.6–P9.7 |
| **A-14** | LOW | P16.4, P23.10 | `RegistryEvaluation` es el único dueño de los valores evaluados | **ACEPTADA** | P14.9, P16.4, P23.10 |
| **A-15** | LOW | P23.15 | Precisar «un fallo no muta nada», o clonar | **ACEPTADA y ELEVADA a MEDIUM práctico**: estado aislado obligatorio; se distinguen «no hubo persistencia» y «no hubo mutación en memoria» | P23.15–P23.16 |
| **A-16** | LOW | P1.9 | Límites normativos por nodos, anidamiento y argumentos; texto de entrada ≥ 4000 | **ACEPTADA**; CR-3 define la profundidad | P1.9 |
| **A-17** | LOW | P17.4, P17.10 | Tokens persistidos en tablas cerradas; nunca nombres C# | **ACEPTADA** | P2.4, P10.9, P17.10 |
| **A-18** | LOW | P2.8, P22.3 | Un literal canónico obedece la regla de literales de su superficie | **ACEPTADA** | P2.8, P22.3, P23.6 |

### 0.11 Decisiones cerradas (no se reabren en V6)

| Tema | Estado | Dónde |
|---|---|---|
| CR-1…CR-5 | CLOSED — SATISFIED en la revisión final de V4 | §0.5, §0.7 |
| FR-1: fases de observación, comparación `Success`/`Failed`, `Delete`, `UnlinkAllAndDelete` | CLOSED — SATISFIED en la re-revisión final de V5 | §0.2, §0.4, §0.6, P21.6 |
| Regla de dependencias transitivas | CLOSED — ACCEPTED en la re-revisión final de V5 | P21.6, ALT-41 |
| N-1: «permitiría» sin garantía | CLOSED — SATISFIED | §7.2 |
| A-03 modificado (tres fronteras) | CLOSED — ACCEPTED | §0.9, P8.10 |
| OPEN A — sintaxis de homónimos, con `#GUID` sin nombre solo para referencias rotas | CLOSED | §4 |
| OPEN B — estructural frente a semántico, R1–R3 | CLOSED | §5 |
| Versión de schema | CLOSED: **V-0** con cuatro condiciones | §6 |
| `UnlinkAllAndDelete` con expresiones | CLOSED: bloquea fórmulas y dependientes | §7.1 |
| `RepairBrokenRack` con expresiones | CLOSED: una sola operación, conjunto semántico ampliado, confirmación por fuente | §7.2 |
| Tabla explícita de tokens de `VariableType` | CLOSED | P17.10 |
| Motor adimensional; `[mm]`, `[in]`, `[ft]`; `MIN`, `MAX`, `ABS`; `ROUND`, `CEILING`, `FLOOR` diferidas | CLOSED | P5, P9, P10 |
| `PlanReadSet`, también sin `RegistryMutation` | CLOSED; FR-1 corrigió en V5 cómo se forman y se comparan sus lecturas, sin cambiar la regla exacta de CR-5; RR-1 añade en V6 las decisiones de reparación | P21.6 |
| Aislamiento de estado (A-15) | CLOSED | P23.16 |
| Texto de entrada ≥ 4000; límites de nodos, argumentos y profundidad | CLOSED | P1.9 |
| `RegistryEvaluation` como única autoridad de valores | CLOSED | P14.9 |
| Costuras para ID20, ID23, ID28 e ID29 | CLOSED | P25–P27 |

### 0.12 Qué conserva V6 de V5

Todo lo que no enumeran §0.1 y §0.3:

- las fases de observación de FR-1 y la comparación `Success`/`Failed`; la regla de dependencias transitivas
  (ACCEPTED); el commit de `Delete` y de `UnlinkAllAndDelete`; el `PlanReadSet` con y sin `RegistryMutation`;
- CR-1…CR-5, tal como la revisión final de V4 los dio por satisfechos; N-1;
- A-03 con sus tres fronteras y los contratos raíz (casos A–D); OPEN A; OPEN B con R1–R3; V-0 con sus cuatro
  condiciones; `MaxBoundExpressionDepth = 24`;
- los veredictos de `UnlinkAllAndDelete` y de `RepairBrokenRack` como **una** operación, con la confirmación
  consciente de la fuente;
- las dos superficies de expresión con el mismo control; el modelo híbrido con `=X` canónico; `#GUID` solo como
  diagnóstico; las tablas explícitas de tokens persistidos;
- el núcleo neutral y adimensional; `[mm]`, `[in]` y `[ft]`; `MIN`, `MAX` y `ABS`;
- el aislamiento de estado; `RegistryEvaluation` como única autoridad de valores; las dependencias y los dependientes
  inversos; las costuras de ID20, ID23, ID28 e ID29;
- un plan, una transacción, un commit y un `Regen`;
- los 21 escenarios de P28.5, los 11 de P28.6 y los 9 de P28.7; T-V4-09 cambia solo su descripción (RR-1);
- ADR REQUIRED sin número.

**Cambios editoriales**: las menciones a la Proposal vigente pasan de V5 a V6; las secciones heredadas de §0 se
renumeran de §0.4 a §0.12; P28.8 y P28.9 pasan a P28.9 y P28.10; se aplica el vocabulario opcional de N-1; y los
hechos de coordinación se actualizan tras la integración de I-50 en `main` (§1.1, P23.13, P23.16, P24.5, §8, §9,
§12 y §13), sin cambio de contrato. Las
marcas `[V4 · CR-n]` y `[V5 · FR-1]`, los escenarios `T-V4-xx` y `T-V5-xx` y las tablas heredadas conservan su
versión.

---

## 1. Cómo leer esta Proposal

### 1.1 Qué es y qué no es

- **Es** la Proposal V6 del Coordinador, con `Coordinator = AGREED WITH V6`, sometida a la **comprobación final** del
  Arquitecto. Cada regla está **propuesta** hasta que haya consenso.
- **No es** consenso, **no** autoriza implementación, **no** es un ADR y **no** modifica V1, V2, V3, V4, V5 ni el
  Discovery. La compuerta **NO IMPLEMENTATION BEFORE CONSENSUS** del contrato (§12) sigue vigente.
- **Parte de hechos**: toda afirmación sobre el código remite al [Discovery G1](I-49-discovery.md) o a una ruta
  verificada en `a4d88f1`. Hasta `46fcac2`, ninguno de los archivos citados cambió en `main`. La integración de I-50
  (`f8deb67`) cambió `RackSelectiveWindow.xaml` y `.xaml.cs`, `SelectivePalletDesignDocument.cs` y
  `SelectiveGeometryResolver.cs`: sus citas de línea siguen siendo de `a4d88f1`, y ninguna premisa de V6 cambia
  (§12.1). Lo que era INFERENCE sigue siéndolo.

### 1.2 Convenciones

| Marca | Significado |
|---|---|
| **Pn.m** | Regla propuesta *m* del punto *n* de §3. Solo sería vinculante tras G3 |
| **[V6 · RR-1]** | Regla nueva o modificada en V6 para cerrar el hallazgo RR-1 de la Architect Final Re-review de V5 |
| **[V5 · FR-1]** | Regla nueva o modificada en V5 para cerrar el hallazgo FR-1 de la Architect Final Review de V4 |
| **[V5 · N-1]** | Aclaración no material de V5, pedida en la Architect Final Review de V4 |
| **[V4 · CR-n]** | Regla nueva o modificada en V4 para cerrar el cambio CR-n de la Architect Re-review de V3 |
| **[V3 · A-xx]** | Regla nueva o modificada en V3 para reconciliar el hallazgo A-xx de la Architect Review de V2 |
| **[V3 · OPEN A]**, **[V3 · OPEN B]** | Regla que cerró la cuestión correspondiente en V3 |
| **DR-n** | Decisión rectora (§2) |
| **Discovery §x** | Hecho verificado en G1; remite a la sección *x* de [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` |
| **REQUISITO DEL OWNER** | Requisito del contrato del Owner fijado en el encargo de G2A.1 |
| **OWNER INPUT** | Aclaración del Owner recibida en G1 (ID20, ID23, ID28/ID29) |
| **INFERENCE** | Estimación o deducción, no hecho |
| Nombres de tipos y miembros | **Ilustrativos**: el contrato es el comportamiento |

### 1.3 Entradas vinculantes

- **Contrato de I-49** — [`I-49-motor-expresiones-parametricas.md`](I-49-motor-expresiones-parametricas.md): las
  invariantes de I-47/I-48 no se reabren sin ADR y decisión del Owner (§3.2); quedan fuera Custom BOM, ID20
  productivo e ID21 (§4). Su §11.2 prevé salir de `ProjectVariable.Definition` con «ADR nuevo aceptado por el Owner»:
  ese ADR es obligatorio (§9) y la alineación del texto del contrato queda para G4.
- **[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)**, aceptado e inmutable.
- **REQUISITOS DEL OWNER** (encargo de G2A.1), sin cambio respecto de V2.
- **Architect Review de V2** (`CHANGES REQUIRED`) y **decisiones del Coordinador de G2C**, reconciliadas en §0.10.
- **Architect Re-review de V3** (`CHANGES REQUIRED`, CR-1…CR-5) y **decisiones del Coordinador de G2D**,
  reconciliadas en §0.7.
- **Architect Final Review de V4** (`CHANGES REQUIRED`: CR-1…CR-5 SATISFIED, hallazgo FR-1 y nota N-1) y
  **decisiones del Coordinador de G2E**, reconciliadas en §0.4.
- **Architect Final Re-review de V5** (`CHANGES REQUIRED`: FR-1 SATISFIED, regla transitiva ACCEPTED, N-1 SATISFIED y
  hallazgo RR-1) y **decisiones del Coordinador de G2F**, reconciliadas en §0.1.
- **OWNER INPUT de G1** (Discovery §1): ID20, ID23 e ID28/ID29.
- **`OWNER_OVERRIDE_I49_I50_PARALLEL`** — [`decisions/I-49.md`](../automation/decisions/I-49.md), con su procedimiento
  de colisión (§8 de ese registro).

---

## 2. Decisiones rectoras y arquitectura en una página

### 2.1 Decisiones rectoras

- **DR-1 — Dos superficies productivas de expresión** (REQUISITO DEL OWNER): la **definición de variable**
  (`literal | expression`) y la **fuente de una propiedad vinculable** en el **mismo** `LinkedPropertyEditor`
  (`Literal | ProjectVariableReference(VariableId) | Expression(BoundExpression)`). La referencia directa se
  conserva y `=X` tiene una sola representación persistida. ID21 sigue fuera.
- **DR-2 — Núcleo neutral** `RackCad.Application.Expressions`: sin dependencia de Project Variables, persistencia,
  sistemas, BOM, catálogos, `StructuralSections`, Domain, UI, Plugin ni AutoCAD, y **sin ningún tipo de
  `ProjectVariables`** en sus entradas **[V3 · A-06]**. Las conversiones de unidades vienen de **una** autoridad
  neutral que contiene **solo** las conversiones genéricas milímetros↔pulgadas y pies↔pulgadas; las constantes de
  dominio que valen 12 no forman parte de ella **[V4 · CR-1]**.
- **DR-3 — Un solo dueño de los valores evaluados**: `RegistryEvaluation`, una vez por snapshot acreditado. Las
  expresiones de propiedad se evalúan en el resolver único, sobre esos valores. UI y sesión solo transportan
  valores para presentación **[V3 · A-14]**.
- **DR-4 — Identidad por id y una forma canónica por significado.** El nombre solo interviene al escribir y al
  mostrar, con la sintaxis de §4 **[V3 · OPEN A]**.
- **DR-5 — Motor adimensional con tres fronteras de validez** **[V3 · A-03 modificada]**: el motor acepta
  cualquier `double` finito; el adaptador de Project Variables aplica a la raíz el contrato de su `VariableType`,
  también a los símbolos que pasan de fallar a tener valor **[V4 · CR-2]**; el consumidor aplica el dominio de su
  propiedad.
- **DR-6 — Fail-closed tipado** (ADR-0034 §8): cada fallo es un resultado tipado, un plan fallido es vacío, y no
  hay fallback a literal, a cero ni a un valor anterior.
- **DR-7 — Estructural ≠ semántico** **[V3 · OPEN B]**: lo que la build no entiende no se lee ni se repara; lo que
  entiende pero no puede evaluar se diagnostica y se corrige, sin fallback.
- **DR-8 — Sin efectos parciales y con una revisión ACOTADA de la concurrencia de I-48** **[V3 · A-05, A-15 ·
  V4 · CR-5 · V5 · FR-1 · V6 · RR-1]**:
  - ninguna transición deja mutado en memoria el estado de quien la llama;
  - I-49 **revisa de forma acotada** la política V8-R05 de I-48: si un valor que participó en el `PlanReadSet` cambia
    entre preflight y commit, el commit aborta antes de escribir, con o sin `RegistryMutation`;
  - el `PlanReadSet` está formado por **observaciones** con fase (`Before` o `After`) y resultado esperado. «Cambia»
    significa que el resultado observado difiere del esperado, y un fallo idéntico al esperado **no** aborta
    **[V5 · FR-1]**;
  - toda clasificación semántica que justifica una decisión destructiva del plan también se observa: cada fuente que
    retira `RepairBrokenRack` lleva su decisión de reparación, que se recomputa antes de escribir **[V6 · RR-1]**;
  - no introduce hash de snapshot, token global, comparación completa del registro, rechazo por cambios fuera del
    `PlanReadSet` ni locks. Los cambios ajenos al `PlanReadSet` **no** abortan;
  - es una extensión consciente de I-48 que el ADR de I-49 tiene que formalizar y que requiere la aceptación del
    Owner antes de implementar (§9, §13).

### 2.2 Tuberías

```text
Lectura y consumo
NOD RACKCAD_PROJECT (Plugin)          único lector/escritor físico                         (sin cambio)
  → ProjectVariablesStore             legibilidad ESTRUCTURAL: payload de Expression cerrado (P17)
  → UsableProjectVariablesRegistry    identidad: VariableId único                          (sin cambio)
  → RegistryEvaluation                grafo, ciclos, orden, valores y fallos semánticos    (P6–P16)
  → SelectiveEffectiveDesignResolver  resolver único: referencias → valor evaluado;
                                      expresiones → evaluadas aquí; dominio del consumidor (P14.8, P24)
  → geometría / BOM / preview         solo efectivo; nada roto se dibuja                   (§5)
```

```text
Escritura explícita — RACKVARIABLES (definiciones de variable)
texto tecleado → parse → bind contra el snapshot del workspace → validate → evaluate → canonicalizar
  → intent con la forma enlazada (ids, nunca texto)       (P7.7, P22.7)
  → preflight sobre la MISMA lectura: R1–R3, contrato raíz (casos A–D) y dominio   (P21.2–P21.3, P8.10)
  → plan + PlanReadSet: observaciones Before/After con su resultado esperado                      (P21.6)
  → commit: re-lectura, re-acreditación e ids; Before sobre la re-lectura, After sobre el documento
    cambiado → escribir, o ABORT si un resultado observado difiere del esperado                   (P21.6)
```

```text
Escritura explícita — RACKEDITAR (fuentes de propiedad)
texto tecleado → parse → bind contra el snapshot del comando → validate → evaluate (aviso) → canonicalizar
  → sesión comprometida → frontera C4 → estados finales      (P23.6, P23.9)
  → reconciliador sobre estado aislado: árbol por ids, un resolve del rack, versión calculada
  → escribir o fallar sin persistencia ni mutación en memoria   (P23.15–P23.16)
```

```text
RackCad.Application.Expressions  (núcleo neutral, DR-2)
  texto          → lexer/parser             → sintaxis con posiciones                  (P1, P2, §4)
  sintaxis       → binder + SymbolResolver  → BoundExpression (ids, funciones, unidades) (P7, P9, P10)
  BoundExpression→ comprobación semántica   → existencia, aridad, ámbito, forma, profundidad (P7.5, P1.9)
  BoundExpression→ dependencias → grafo → ciclos → orden                                (P11–P14)
  orden + tabla  → evaluador (double)       → EvaluationResult + traza opt-in           (P14, P16)
  BoundExpression→ formatter                → texto editable mínimo e inequívoco        (P4, §4)
```

### 2.3 Lo que V6 extiende o revisa

| Invariante o contrato vigente | Extensión o revisión | Punto |
|---|---|---|
| ADR-0034 §3: definición **literal** | `literal` **o** `expression`; `VariableType` sigue siendo solo `Length` | P2.6, P5.5 |
| ADR-0034 §4: `Literal(T)` o `ProjectVariableReference(VariableId)` | Tercer caso `Expression(BoundExpression)` | P2.7, P17.3, P24 |
| ADR-0034 §15: `Definition` para ID22B; `PropertyValue<T>` para ID21 | ID22B extiende también la fuente de propiedad; ID21 sigue fuera | §9 |
| ADR-0034 §7: profundidad 1 | Cierre transitivo; un plan, una transacción, un commit y un `Regen`; `PlanReadSet` | P21 |
| ADR-0034 §11: `Delete` bloqueado con consumidores; reparación de bindings rotos | `Delete` y `UnlinkAllAndDelete` también bloqueados por fórmulas; `RepairBrokenRack` repara también fallos semánticos, con aviso de recuperación condicional, y no escribe si la premisa de alguna fuente que retira cambió antes del commit | P20, §7 |
| ADR-0034 §13, C4-8 y C4-9 | Kinds `expression` fail-closed en builds anteriores; **V-0**; payload de Expression cerrado | §6 |
| Decisiones de I-48 §4 (V8-R01): re-lectura y re-acreditación en commit | La re-lectura también valida ids y compara las observaciones del `PlanReadSet` en su fase —`Before` sobre la re-lectura, `After` sobre el documento cambiado—, y ocurre también en planes sin `RegistryMutation` que escriben racks. En `RepairBrokenRack` recomputa además la decisión de cada fuente que retira (RR-1) | P21.6 |
| **I-48 Proposal V8 §6 (V8-R05)**: concurrencia fuera de alcance; sin comparación entre preflight y commit, sin concurrencia optimista y sin rechazo de plan obsoleto **[V4 · CR-5]** | **Revisión ACOTADA**: un valor que participó en el `PlanReadSet` y cambia entre preflight y commit → `ABORT BEFORE WRITE`. Desde V5, «cambia» significa que el resultado observado difiere del esperado en su fase, y un fallo idéntico no aborta (FR-1). Afecta también a planes que ya existían, como un cambio de valor con solo referencias directas. Sin hash, token global, comparación completa, locks ni rechazo por cambios fuera del `PlanReadSet`. **Exige ADR y aceptación del Owner** | DR-8, P21.6, §9, §13 |
| Contrato del editor de I-48 (HANDOFF §1; pines G20) | `=` abre un borrador de expresión; `CASO_6` y `CASO_8` cambian; se conservan borrador ≠ comprometido, Escape, LostFocus, C4, no auto-selección y desambiguador | P23.12 |
| Regla `> 0` de variables (hoy en la ventana) | La aplica también el adaptador, a la raíz, en las dos sintaxis (casos A–D) | P8.10 |
| Dominio de propiedades (hoy en la ventana) | Se aplica además al efectivo de referencias y expresiones al resolver | P24.5 |
| Contrato de I-49 §1, §11.2 y §12 | La capacidad sale de `Definition` por decisión del Owner, por la vía de ADR que el contrato prevé | §9, §13 |

**No se toca**: autoridad y nodo del registro, identidad `VariableId`, authored ≠ effective, congelado del literal
comprometido (20.13), autoridad multi-vista, identidad ambigua fail-closed, mapping único del `Type`, reparación
atómica por rack, la regla R1 y la decisión C4-13 de I-47 (consumidor irresoluble ⇒ abortar), las constantes de
dominio de los sistemas de rack y los censos de comandos y ventanas.

---

## 3. Los 30 puntos

### 3.0 Tabla de control

| # | Punto | Propuesta en una línea | Cambios |
|---|---|---|---|
| 1 | grammar | Aritmética, llamadas, unidades tras números y referencias `Nombre`, `{Nombre}` y `…#GUID`; profundidad normativa del `BoundExpression` | OPEN A · A-16 · **CR-3** |
| 2 | AST/model | Sintaxis, `BoundExpression` y resultado; formas canónicas; forma no canónica = semántica | A-07 · A-17 · A-18 |
| 3 | identity | `VariableId` intacto; `SymbolId = (namespace, key)`; cualificador de GUID completo | OPEN A |
| 4 | source text | Formatter único con forma mínima inequívoca; `#GUID` para referencias rotas | OPEN A |
| 5 | symbol model | Sin `VariableType` en el núcleo; motor adimensional; tres fronteras de validez | A-06 · A-03 |
| 6 | ExpressionContext | Inmutable por operación, desde un snapshot; los errores semánticos no invalidan el registro | OPEN B |
| 7 | SymbolResolver/binder | Única ruta nombre→id; enlace contra el snapshot mostrado; nunca contra una lectura posterior | OPEN A · A-04 |
| 8 | project-variable symbol binding | `SymbolId → VariableType` en el adaptador; contrato raíz con casos A–D | A-06 · A-03 · **CR-2** |
| 9 | units | `[mm]`, `[in]`, `[ft]`; autoridad neutral solo de conversiones genéricas; constantes de dominio locales | A-13 · **CR-1** |
| 10 | function registry | `MIN`, `MAX`, `ABS`; diferidas `ROUND`, `CEILING`, `FLOOR`; tokens en tablas cerradas | A-17 · OPEN A |
| 11 | dependency extraction | Función pura, igual para las dos superficies | — |
| 12 | dependency graph | Derivado; dependencias y dependientes; conjuntos afectado y leído | A-05 |
| 13 | cycle detection | Iterativa y determinista; ciclos persistidos según OPEN B | OPEN B |
| 14 | evaluation order | Topológico determinista; `RegistryEvaluation`, único dueño | A-14 |
| 15 | diagnostics | Catálogo cerrado actualizado con los códigos de OPEN A y OPEN B | OPEN A · OPEN B |
| 16 | EvaluationResult | `Success`/`Failed` con `double`, diagnósticos y traza opt-in | A-14 |
| 17 | persistence | Dos formas; payload de Expression cerrado; frontera estructural fijada, profundidad incluida | A-07 · A-08 · A-17 · **CR-3** |
| 18 | schema evolution | **V-0** con cuatro condiciones | A-11 · CLOSED |
| 19 | rename | Registry-only; formatter cualificado ante homónimos; permitido con errores semánticos, sin observaciones en commit | OPEN A · OPEN B · **FR-1** |
| 20 | delete | `Delete` y `UnlinkAllAndDelete` bloqueados por fórmulas y dependientes; commit sin `After` del símbolo eliminado | A-02 · **FR-1** |
| 21 | propagation | R1–R3; contrato raíz A–D; `PlanReadSet` de observaciones `Before`/`After` y de decisiones de reparación como revisión acotada de V8-R05; reparación generalizada; un plan y un `Regen` | A-01 · A-03 · A-05 · A-12 · OPEN B · **CR-2** · **CR-4** · **CR-5** · **FR-1** · **RR-1** |
| 22 | RACKVARIABLES UX | Enlace contra el snapshot; diagnóstico y reparación con aviso de recuperación veraz | A-04 · A-10 · A-18 · OPEN B · **CR-4** |
| 23 | LinkedPropertyEditor UX | Sintaxis cerrada; `CASO_8` fijado; estado aislado; versión calculada | OPEN A · A-09 · A-15 |
| 24 | direct-reference compatibility | Resultados de `InspectBinding` reparables o fatales; dominio sobre referencias; la misma inspección recomputa en commit la decisión de reparación | A-01 · A-03 · OPEN B · **RR-1** |
| 25 | ID20 extension point | Validado; `Rack.*` y `Project.*` reservados en la gramática | OPEN A |
| 26 | ID23 extension point | Validado con A-06 | A-06 |
| 27 | ID28/29 information preservation | `BoundExpression`, diagnósticos, traza opt-in, dependientes y conjuntos afectado y leído; el leído, con fase y resultado esperado o con su razón de reparación | A-05 · **FR-1** · **RR-1** |
| 28 | tests | 21 escenarios de G2C (tres ajustados), 11 de G2D, 9 de G2E y 9 de G2F; guarda de unidades acotada | V3 · **CR-1…CR-5** · **FR-1** · **RR-1** |
| 29 | migration | Sin migración de datos ni de versión | — |
| 30 | non-goals | Actualizado con las decisiones cerradas, CR-1, CR-4, CR-5, FR-1 y RR-1 | V3 · **CR-1** · **CR-4** · **CR-5** · **FR-1** · **RR-1** |

### P1 — grammar

**Base.** No existe ningún parser de expresiones en el repositorio (Discovery §15, búsqueda 1). La coma ya tiene
dos significados: decimal en la regla localizada de ADR-0015 y separador de lista en otras superficies (Discovery
§11.7). El editor vinculable parsea literales en cultura invariante y RACKVARIABLES con la regla localizada
(Discovery §11.2–§11.3). No existe sintaxis de unidades (Discovery §11.7).

- **P1.1 — Lenguaje.** Expresiones aritméticas sobre números (con unidad opcional), referencias y llamadas a
  función. Sin sentencias, asignaciones, comparaciones, condicionales, booleanos ni cadenas.
- **P1.2 — Gramática** (EBNF) **[V3 · OPEN A]**. La producción `reference` queda cerrada con la sintaxis de §4:

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
  qualifier      = "#" , guid ;                             (* forma D completa: 8-4-4-4-12, 36 caracteres *)
  ```

  `letter` es un carácter de categoría letra Unicode; `digit`, un dígito ASCII `0`–`9`.
- **P1.3 — Precedencia.** `*` y `/` sobre `+` y `-`; asociatividad por la izquierda; el unario liga más fuerte
  que cualquier binario.
- **P1.4 — Números.** Forma invariante con `.` como único separador decimal. Sin exponente, sin agrupadores, sin
  punto inicial ni final (`.5`, `5.`), sin signo dentro del número (el signo es operador unario) y sin `NaN` ni
  `Infinity`.
- **P1.5 — Coma.** `,` es **solo** separador de argumentos. Una coma entre dos dígitos (`1,5`) es el error léxico
  `AmbiguousDecimalComma`: nunca se lee como decimal ni como dos argumentos, de modo que `MAX(Holgura, 1,5)` no
  puede convertirse en silencio en `MAX(Holgura, 1, 5)`.
- **P1.6 — Unidades** (REQUISITO DEL OWNER):
  - un sufijo `[mm]`, `[in]` o `[ft]` se admite **solo** tras un literal numérico: `100[mm]`, `4[in]`, `20[ft]`;
  - los tokens de unidad son exactamente `mm`, `in` y `ft`. Cualquier otro (`[cm]`, `[MM]`, `[m]`) es
    `UnknownUnit`;
  - un sufijo tras una referencia, una llamada o un paréntesis (`Holgura[mm]`, `{Holgura}[mm]`,
    `Holgura#<id>[mm]`, `(2 + 3)[mm]`) es `UnitNotAllowedHere` **[V3 · OPEN A]**;
  - las demás notaciones (`100 mm`, `12"`, `10'6"`, `1 1/8`) son `UnitSyntaxNotSupported`. `1/2` no es una
    fracción: es una división válida.

  Los corchetes quedan **exclusivamente** para unidades. El significado numérico de cada unidad está en P9.
- **P1.7 — Espacios y reglas léxicas** **[V3 · OPEN A]**.
  - Los espacios no son significativos entre tokens: `100 [mm]` se acepta y su forma canónica es `100[mm]`.
  - Dos excepciones: dentro de un `bare-name`, un espacio **simple** entre dos palabras forma parte del nombre; dentro
    de llaves, todo carácter forma parte del nombre.
  - Una `word` seguida de `(` es una llamada (P10).
  - Una `word` seguida de `.` es sintaxis de namespace reservada para ID20 y da `UnknownNamespace` (P25).
  - Un `bare-name` cuyo texto completo coincide, sin distinguir mayúsculas, con un nombre de función del registro,
    con `Rack` o con `Project` es `ReservedName`: esos nombres se escriben con llaves (`{MIN}`, `{Rack}`).
  - Una llave sin cerrar es `UnterminatedName`. Un cualificador que no sea un GUID completo en forma D
    (`#3f2b1c9e`) es `InvalidQualifier`: un fragmento de GUID **nunca** se parsea.
- **P1.8 — El `=` no pertenece a la gramática.** Es la marca de superficie que indica «esto es una expresión», en
  RACKVARIABLES (P22) y en el editor vinculable (P23). Nunca se persiste.
- **P1.9 — Límites** **[V3 · A-16 · V4 · CR-3]**.

  | Límite | Valor | Naturaleza | Al escribir | Al leer lo persistido |
  |---|---|---|---|---|
  | Nodos por árbol | ≤ 256 | **Normativo** | `LimitExceeded` | Estructural (§5) |
  | Profundidad del `BoundExpression` (`MaxBoundExpressionDepth`) | ≤ 24 | **Normativo** | `LimitExceeded` | Estructural (§5) |
  | Argumentos por llamada | ≤ 16 | **Normativo** | `LimitExceeded` | Estructural (§5) |
  | Texto de entrada | ≤ 4000 caracteres (nunca menos) | Guarda del parser | `LimitExceeded` | No aplica: lo persistido no tiene texto |
  | Anidamiento sintáctico (paréntesis, unarios y llamadas) | Valor de implementación: inicial 64, nunca menor que 24 | Guarda del parser | `LimitExceeded` | No aplica |

  Superar un límite **nunca** trunca. La longitud del texto no es un límite del árbol: una referencia cualificada
  ocupa más de 36 caracteres (§4) y un límite solo textual rechazaría fórmulas legítimas.

  **Definición normativa de profundidad** **[V4 · CR-3]**:

  ```text
  depth(expression) = número máximo de nodos en cualquier camino root→leaf del BoundExpression,
                      contando la raíz y la hoja
  ```

  Ejemplos normativos:

  | `BoundExpression` | Texto | `depth` |
  |---|---|---|
  | `Number` | `1` | 1 |
  | `Reference` | `A` | 1 |
  | `Negate(Number)` | `-1` | 2 |
  | `Add(1, 2)` | `1 + 2` | 2 |
  | `Add(Add(1, 2), 3)` | `(1 + 2) + 3`, formateado `1 + 2 + 3` | 3 |
  | `Call(ABS, A)` | `ABS(A)` | 2 |
  | `Call(MIN, A, Add(B, C))` | `MIN(A, B + C)` | 3 |

  - **La asociatividad cuenta.** El parser construye cadenas binarias por la izquierda (P1.3), así que una suma sin
    paréntesis de *n* operandos tiene profundidad *n*. `1 + 1 + … + 1` con 24 operandos pasa; con 25, la cadena tiene
    25 niveles y **excede el límite aunque el texto no tenga paréntesis**. Agrupar con paréntesis puede reducir la
    profundidad, y el formatter conserva esa estructura (P2.5).
  - **Dónde se valida.** Tras el bind y antes de comprometer, en las dos superficies (P7.5, P23.6); otra vez sobre la
    forma enlazada que llega al preflight (P21.2); y al leer un `BoundExpression` persistido. Al escribir, el exceso
    es `LimitExceeded`; al leer, es estructural (`MalformedExpression` con submotivo de límite, P15.9).
  - **Guardas del parser, separadas.** La longitud del texto y el anidamiento sintáctico se comprueban **antes** de
    descender y solo protegen al parser. No sustituyen a la profundidad: un texto dentro de las guardas puede exceder
    `MaxBoundExpressionDepth` tras el bind, y los paréntesis redundantes no suman profundidad. La guarda sintáctica
    nunca es menor que 24, para que el texto canónico de cualquier árbol legal vuelva a parsear (P2.5).
  - **Contractual.** `MaxBoundExpressionDepth` decide qué payloads persistidos son legibles: cambiarlo exige
    revisión, la prueba de G8 y ADR.

  **Presupuesto de profundidad JSON.** Ningún `JsonSerializerOptions` de `src/RackCad.Application` fija `MaxDepth`
  (búsqueda en `src/` en `a4d88f1`), así que rige el máximo por defecto de `System.Text.Json`, 64 niveles, también en
  `JsonDocument.Parse`. Un nodo `call` consume dos niveles (`Args` y el nodo). El registro añade cuatro niveles antes
  del nodo raíz (raíz, `Variables`, entrada y `Definition`) y `PropertyValues`, tres (raíz, mapa y entrada); el diseño
  viaja como cadena dentro del sobre (`RackEmbedDocument.cs:55-56`), así que el sobre no suma. La biblioteca anida el
  diseño como objeto (`RackProjectDocument.SelectiveRack`), pero solo guarda exportaciones literales (P24.9).
  - El anidamiento ≤ 64 de V2 no era persistible, y un límite sin definir dejaba pasar cadenas binarias largas.
  - Con profundidad 24 y llamadas en todos los niveles, el peor caso ocupa `5 + 2 × 23 = 51` niveles en el registro
    y 50 en el diseño; una cadena binaria de profundidad 24 ocupa 28.
  - **Prueba del peor caso en G8, por descubrimiento.** La prueba localiza en `src/` los puntos reales que
    serializan, deserializan, clonan o parsean `ProjectVariablesDocument`, `SelectivePalletDesignDocument`,
    `SelectivePropertyValueDocument` o el JSON del diseño. Por cada uno ejecuta el árbol que agota a la vez
    profundidad, nodos y argumentos. Un punto nuevo sin cobertura hace fallar la prueba. Hoy aparecen, entre otros,
    los dos stores, `ProjectVariableCloning`, `SelectiveAuthoredAuthority.cs:160` y el restamp. Esa lista es un
    resultado de la prueba, no una parte cerrada del contrato.
- **P1.10 — Implementación.** Parser escrito a mano, determinista e independiente de la cultura, con las guardas de
  texto y de anidamiento sintáctico de P1.9 comprobadas **antes** de descender. La profundidad normativa se valida
  sobre el `BoundExpression`, no en el parser (P1.9). Prohibidos terceros (ADR-0012), `NCalc`, `DataTable.Compute`,
  `System.Linq.Expressions` y Roslyn (Discovery §15, búsqueda 3). **Un solo** lexer reconoce unidades, nombres y
  cualificadores: el del núcleo.
- **P1.11 — Texto sin `=`.** Sigue siendo un literal parseado **exactamente como hoy** en cada superficie: regla
  localizada de ADR-0015 en RACKVARIABLES e invariante en el editor vinculable. V6 no resuelve esa divergencia
  vigente (Discovery §11.2) ni admite unidades sin `=`. La gramática invariante de P1.4 solo rige dentro de una
  expresión, y esa excepción acotada a ADR-0015 tiene que constar en el ADR (§9).

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
- **P2.2 — Comprobación semántica separada.** Existencia de referencias, aridad, unidades, ámbito y forma
  canónica se comprueban sobre un `BoundExpression` **contra un contexto** (P7.5), no dentro del árbol. El mismo
  árbol leído de persistencia se re-comprueba en cada snapshot.
- **P2.3 — Igualdad estructural por valor.** Ordinal para tokens y `double.Equals` para números, igual que la
  igualdad exacta vigente (Discovery §11.4). La unidad forma parte de la igualdad: `100[mm]` ≠ `3.937…`.
- **P2.4 — Conjunto de nodos cerrado y tokens explícitos** **[V3 · A-17]**.
  - Cada kind, nodo, operador, unidad, función y namespace tiene su token persistido declarado en una **tabla
    cerrada y explícita**, en los dos sentidos (P17.10).
  - **Nunca** se persiste semántica con `enum.ToString()`, `Type.ToString()`, `nameof` ni el nombre de una clase
    C#: renombrar un miembro no puede cambiar lo que se escribe ni lo que se lee.
  - Añadir un tipo de nodo, una unidad o una función cambia lo que las builds anteriores pueden leer (P18.8).
- **P2.5 — Canonicidad.** Para todo `BoundExpression` canónico `b` **sin referencias rotas** y un mismo snapshot:
  `Canonicalize(Bind(Parse(Format(b)))) == b`. Se prueba como propiedad (P28), también con homónimos y con nombres
  que exigen llaves, porque la sintaxis de §4 representa cualquier identidad. Un árbol con una referencia rota se
  formatea como `#<id>` y **no** enlaza (`BrokenReference`): ese texto solo sirve para mostrar (P4.5).
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
  | Solo un número **sin unidad** (`Number`, o `Negate(Number)` sin unidad) | `Literal(±v)` | `Literal(±v)` |
  | Cualquier otro árbol: operación, función o **unidad explícita** | `Expression` | `Expression` |

  Así `=Holgura` y la selección de `Holgura` en la lista persisten lo mismo, y `=6` persiste lo mismo que `6`.
  `=4[in]` es `Expression` aunque valga 4, porque lleva conversión explícita (REQUISITO DEL OWNER).

  - **Un literal canónico obedece la regla de literales de su superficie** **[V3 · A-18]**. Canonicalizar no es una
    vía para eludirla. En RACKVARIABLES, `=0` y `=-2` se rechazan igual que `0` y `-2` (`> 0`, P22.3), que es además
    el contrato raíz de `Length` (P8.10). En el editor vinculable, `=-3` sigue la ruta histórica del literal `-3`
    (P24.5).
  - **Una forma no canónica leída de persistencia es error SEMÁNTICO `NonCanonicalForm`** **[V3 · A-07]**, por
    símbolo o por fuente. Es no canónica una definición `expression` que solo sea un número sin unidad, o una fuente
    `expression` que solo sea una referencia o solo un número sin unidad. No se evalúa, no vuelve ilegible el
    registro y se corrige con `ChangeDefinition` o con la reparación del rack (§7.2). **Nunca** se normaliza en
    silencio y **ningún** escritor la produce: todo camino de escritura canonicaliza, y G8–G10 lo prueban.

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
- **P3.4** — `FunctionId` es un token canónico en **mayúsculas** (`MIN`, `MAX`, `ABS`) de una tabla cerrada
  (P10.9), comparado en `Ordinal`; la entrada no distingue mayúsculas.
- **P3.5** — El nombre **nunca** es identidad: el árbol persistido no contiene nombres (P17) y la única resolución
  por nombre ocurre al escribir, contra el snapshot mostrado (P7.3).
- **P3.6** — Un `VariableId` duplicado da `AmbiguousIdentity` **antes** de construir cualquier tabla de símbolos
  (decisiones de I-48 §4): nada se enlaza ni se evalúa sobre un registro ambiguo.
- **P3.7** — Una variable que se referencia a sí misma forma un ciclo de longitud 1 (P13).
- **P3.8** — Una expresión de propiedad **tampoco** tiene identidad propia: pertenece a `(RackId, PropertyId)`,
  igual que hoy un binding. Ninguna expresión puede referenciarla (P13.6).
- **P3.9 — Cualificador textual de identidad** **[V3 · OPEN A]**.
  - `#` seguido del `VariableId` **completo** en forma D (36 caracteres), con los hexadecimales sin distinguir
    mayúsculas, igual que la igualdad de `VariableId`.
  - El formatter lo emite siempre en forma D en minúsculas.
  - Un fragmento **nunca** se parsea (`InvalidQualifier`). Los 8 caracteres que muestran las listas son solo una
    ayuda visual, porque dos ids con el mismo primer grupo se verían iguales (Discovery §13.2–§13.3).

### P4 — source text

**Base.** HANDOFF §4 exige persistir la referencia y «nunca el texto» (`docs/HANDOFF.md:1579`). `"0.###"` es formato
de **presentación** y redondea (Discovery §11.5, hallazgo lateral L5).

- **P4.1 — Tres textos, tres papeles**, en las dos superficies:

  | Texto | Origen | ¿Autoridad? | ¿Se persiste? |
  |---|---|---|---|
  | Texto tecleado | Usuario | No | No |
  | Forma canónica persistida (literal, referencia o `BoundExpression`) | Binder + canonicalización | **Sí** | Sí (P17) |
  | Texto de edición y presentación | Formatter, desde la forma canónica y los nombres **actuales** | No | No |

- **P4.2 — Un solo formatter.** Determinista e invariante:
  - números en su representación más corta que hace *round-trip* (nunca `"0.###"`), con la unidad pegada
    (`100[mm]`);
  - un espacio a cada lado de un operador binario, `MIN(a, b)` y funciones en mayúsculas;
  - paréntesis **mínimos** según precedencia y asociatividad;
  - referencias en la forma **mínima inequívoca dentro del snapshot** **[V3 · OPEN A]** (§4.3):

    | Situación de la referencia | Texto emitido |
    |---|---|
    | Id ausente del registro | `#<id>` |
    | Nombre único y seguro sin llaves | `Nombre` |
    | Nombre único con cualquier otro carácter, o reservado | `{Nombre}`, con `}` escapado como `}}` |
    | Homónimo | La forma del nombre + `#<id>` |

- **P4.3 — Normalización visible.** Espacios, paréntesis redundantes, mayúsculas de funciones, llaves innecesarias y
  cualificadores innecesarios no se conservan. Tras comprometer, el usuario ve la forma canónica.
- **P4.4 — El texto tecleado no se persiste.** Sería una segunda fuente capaz de contradecir al árbol y quedaría
  obsoleta con cada rename (ADR-0034 §2).
- **P4.5 — Referencia rota** **[V3 · OPEN A]**. Se muestra como `#<id>`, sin tomar un nombre de ningún otro sitio.
  Ese texto se puede **mostrar** y copiar, pero **no** se puede comprometer como referencia nueva: al enlazar da
  `BrokenReference` (P7.1).
- **P4.6** — Una referencia directa se muestra como `=` seguido de la referencia formateada (hoy `=Nombre`,
  Discovery §9.2; con un homónimo, `=Holgura#<id>`). Una expresión, como `=` seguido del formatter.
- **P4.7 — La forma mostrada depende del snapshot actual** **[V3 · OPEN A]**. Si un nombre único pasa a ser
  ambiguo tras un `Create` o un `Rename`, la siguiente presentación lo cualifica; si deja de serlo, lo muestra sin
  cualificador. Lo persistido no cambia en ningún caso.

### P5 — symbol model

**Base.** No existe `SymbolId`, scope ni namespace, y el registro acreditado se indexa solo por `VariableId`
(Discovery §15, búsquedas 7–9; §16.2).

- **P5.1** **[V3 · A-06]** — `SymbolEntry { SymbolId, SymbolScope, DisplayName, Definition }`, con
  `Definition = Literal(double) | Expression(BoundExpression)`. **Sin** `ConsumerType` ni `VariableType`: el núcleo
  no nombra ningún tipo de `ProjectVariables`, e ID23 puede construir su tabla de símbolos sin esa dependencia
  (P26.1). La relación `SymbolId → VariableType` vive en el adaptador (P8.9).
- **P5.2 — Namespaces.** Hay **uno** activo, `projectVariable`. Quedan **reservados conceptualmente, sin registrar y
  sin resolución**: `rack` y `project` (ID20, P25). El binder devuelve `UnknownNamespace` para cualquier namespace
  no registrado.
- **P5.3 — Ámbitos.** Existe **uno** activo, `Project`: un valor por dibujo. Queda reservado `Rack`: un valor por
  rack (ID20). Una definición de ámbito `Project` **no puede** depender de un símbolo `Rack` (`ScopeViolation`). Una
  expresión de propiedad se evalúa en el contexto de su rack y en V6 solo ve símbolos `Project`.
- **P5.4 — Dominio de valores adimensional** (REQUISITO DEL OWNER). Todo valor del motor es un `double` finito.
  **No existe** `ExpressionValueType`, ni `Scalar`, ni `DimensionMismatch`, ni `ResultTypeMismatch`, ni reglas del
  tipo `Length × Length` o `Length ÷ Length`.
- **P5.5 — `VariableType` fuera del álgebra** **[V3 · A-03 modificada]**. `VariableType` sigue siendo exactamente
  `{ Length = 1 }`, y las guardas G6–G10 no cambian (Discovery §14.3).
  - Dice cómo interpreta el número quien lo consume: pulgadas.
  - **No** gobierna ninguna álgebra dentro del motor.
  - Vive solo en el adaptador de Project Variables, que aplica su contrato al **resultado raíz** (P8.10).
- **P5.6 — Sin comprobador dimensional ni de signo en el motor.** Son válidas, y lo fijan pruebas de comportamiento
  (P28.3): `6 + 2`, `A + 2`, `A * B`, `A / B`, `100[mm] + 2`, `MAX(A, 4[in])` y `Base + (A - B)` con un intermedio
  negativo. El motor no rechaza **ninguna** operación por dimensiones ni por signo.
- **P5.7 — Tres fronteras de validez** **[V3 · A-03 modificada]** (DR-5; §0.9):

  | Frontera | Qué valida | Dónde | Resultado si falla |
  |---|---|---|---|
  | Motor de expresiones | Todo nodo da un `double` finito; sin dominio físico | Núcleo | `DivisionByZero`, `NonFiniteResult` |
  | Adaptador de Project Variables | Contrato vigente del `VariableType` sobre el **resultado raíz**, igual para literal y expresión | Preflight de las mutaciones de variables | `OutOfRange`, cuyo dueño es la variable |
  | Consumidor de propiedad | Dominio de la propiedad sobre el **valor efectivo** | Resolver único; ruta histórica de la ventana para literales | `OutOfRange`, cuyo dueño es `(RackId, PropertyId)` |

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
  contexto del snapshot y los valores **ya evaluados** de `RegistryEvaluation` (P14.9). En V6 no añade símbolos
  propios del rack.
- **P6.8 — Un error semántico no invalida el contexto** **[V3 · OPEN B]**. Los símbolos en error siguen en la tabla,
  con su resultado fallido, y el registro sigue `Usable`. Solo un fallo **estructural** o de identidad impide
  construir el contexto (P6.4).

### P7 — SymbolResolver/binder

**Base.** Hoy **ninguna** ruta de producción resuelve nombre→id. El editor filtra por *contains* `OrdinalIgnoreCase`
y compromete una referencia solo por `VariableId` (Discovery §13.3; `TrySelect` es «the ONLY way a reference is ever
committed», `LinkedPropertyEditSession.cs:302-306`). Los homónimos son legales (Discovery §13.1).

- **P7.1 — `SymbolResolver`** **[V3 · OPEN A]**. Traduce una referencia textual en la sintaxis de §4 a **exactamente
  un** `SymbolId`, o a un fallo tipado. **Nunca** elige uno entre varios ni completa un nombre parcial.

  | Entrada | Resultado |
  |---|---|
  | `Nombre#<id>` o `{Nombre}#<id>`, id presente y nombre igual al actual | Ese id |
  | `…#<id>` con id ausente | `BrokenReference`, no comprometible |
  | `Nombre#<id>` con id presente y nombre distinto del actual | `QualifiedNameMismatch`: el nombre se valida, nunca resuelve |
  | `#<id>` sin nombre, id presente | `NameRequired`: esa forma solo la emite el formatter para referencias rotas |
  | Nombre sin cualificador que coincide exactamente con uno | Ese id |
  | Nombre sin cualificador sin coincidencias | `UnknownSymbol` |
  | Nombre sin cualificador con dos o más coincidencias | `AmbiguousName`, con **cada** candidato en forma cualificada |
  | `bare-name` reservado | `ReservedName` |
  | Tramo contiguo, sin llaves ni espacios, con operadores, que coincide con el nombre de una variable (`Holgura-Base`) | `OperatorInName`: nunca se reinterpreta como resta |
  | `palabra.` | `UnknownNamespace` |

  **Precisión del Coordinador** sobre el veredicto del Architect: `#<id>` sin nombre **nunca** enlaza, ni siquiera
  con un id presente. Así el texto que representa un estado roto no puede comprometer una identidad que el usuario
  no ve nombrada.
- **P7.2 — Comparador de nombres** (CERRADO): igualdad exacta `OrdinalIgnoreCase`, sin recortes ni normalización
  Unicode, coherente con los comparadores vigentes (Discovery §13.3). El *contains* del editor sigue siendo un
  **filtro** y no una regla de resolución (Discovery §13.4).
- **P7.3 — Única ruta nombre→id de producción: solo al escribir y solo contra el snapshot que el usuario ve**
  **[V3 · A-04]**. El binder corre únicamente al comprometer texto del usuario en las dos superficies (P22, P23).
  Lectura, evaluación, rename, delete, propagación, resolver, re-lectura de commit y BOM **nunca** resuelven nombres.
- **P7.4 — Binder total y fail-closed.** Devuelve un árbol solo con **cero** diagnósticos; si no, la lista de
  diagnósticos, en orden determinista y con un tope (20, ajustable).
- **P7.5 — Comprobación semántica** (P2.2): existencia de referencias, aridad (`InvalidArguments`), unidad conocida
  (P9), ámbito (P5.3), autorreferencia (P3.7) y forma canónica (P2.8), además de los límites normativos del árbol
  —nodos, argumentos y profundidad del `BoundExpression`— de P1.9 **[V4 · CR-3]**. Sobre un árbol leído de
  persistencia corre **sin** resolver nombres.
- **P7.6 — Dónde vive.** En Application. El control WPF **no** resuelve nombres: la guarda G19 sigue exigiendo que
  `LinkedPropertyEditor.cs` no compare nombres (Discovery §14.3).
- **P7.7 — Enlace contra el snapshot mostrado** **[V3 · A-04]**. Corrige P22.7 de V2, que decía «contra el registro
  re-leído, nunca contra el snapshot de la ventana». Eso era falso respecto del código y habría dejado que un nombre
  cruzara una frontera entre lecturas actuando como identidad (ADR-0034 §2).

  ```text
  RACKVARIABLES (definiciones)
    snapshot del workspace = LastRegistry, la lectura que construyó la ventana
    → binder puro de Application          → forma enlazada: literal o BoundExpression con ids
    → intent con la forma enlazada        (nunca texto humano)
    → preflight: valida los ids contra LastRegistry (la misma lectura)
    → commit: re-lectura, re-acreditación, ids y observaciones del PlanReadSet (P21.6)

  RACKEDITAR (fuentes de propiedad)
    snapshot del comando = el registro leído antes de abrir la ventana
    → sesión: binder puro contra ese snapshot → estado comprometido con BoundExpression
    → frontera C4                         → LinkedPropertyFinalStates
    → reconciliador: re-comprueba el árbol por ids contra el MISMO snapshot, sin nombres
  ```

  - **FACT**: el bucle de RACKVARIABLES corre el preflight sobre `LastRegistry`/`LastEntries`, la misma lectura que
    construyó la ventana (`src/RackCad.Plugin/RackVariablesCommands.cs:59-60,121-122`).
  - **FACT**: en RACKEDITAR, una sola lectura del registro sirve para abrir, ofrecer opciones y reconciliar
    (`src/RackCad.Plugin/RackSelectivoCommands.cs:66,80-81,144-145`).
  - **Regla**: una lectura posterior **solo** valida ids (presentes y sin identidad ambigua) y las observaciones del
    `PlanReadSet` (P21.6). Nunca vuelve a resolver texto humano.

### P8 — project-variable symbol binding

**Base.** El `Type` persistido tiene un mapping único compartido (decisiones de I-48 §4; `VariableTypes.TryParseToken`,
Discovery §4.1). La compatibilidad de tipo al vincular vive en `InspectBinding` y `Link` (Discovery §4.1, §8.1).

- **P8.1** — Namespace `projectVariable`; clave = el texto del `VariableId` de la entrada del registro; comparación
  `OrdinalIgnoreCase`.
- **P8.2** — Fuente única: el registro acreditado (P6.4), con el mapping único del `Type`. **No** se crea un segundo
  mapping.
- **P8.3** — `DisplayName = Name`, solo para mostrar y para resolver al escribir.
- **P8.4** — `Definition` = el `Literal` o la `Expression` de la entrada.
- **P8.5** — Un árbol que referencia un id ausente produce `BrokenReference`. Al escribir, el binder lo rechaza
  (P7.1). En un árbol persistido es un error **semántico** (§5): la variable queda `Failed` y una fuente de rack,
  `RepairableMissingTarget`.
- **P8.6** — Una referencia directa de rack (`ProjectVariableReference`) **no** es un símbolo de expresión: es un
  consumidor que lee el valor evaluado de su variable (P24).
- **P8.7 — Referencia directa.** Conserva **sin cambio** la comprobación vigente entre el `VariableType` de la
  variable y el de la propiedad (V4-R01 de I-48).
- **P8.8 — Expresión de propiedad o de definición.** Puede referenciar cualquier variable de `VariableType`
  numérico; en V6 todos lo son, porque solo existe `Length`. El resultado es un `double` que el consumidor
  interpreta según **su** `VariableType` declarado. Qué pasa al combinar tipos distintos se decidirá cuando exista
  un segundo `VariableType`, con ADR.
- **P8.9 — La relación `SymbolId → VariableType` vive en el adaptador** **[V3 · A-06]**. Se construye desde el
  registro acreditado con el mapping único del `Type` y **fuera** del núcleo. La usan P8.7 y P8.10.
- **P8.10 — Contrato raíz del `VariableType`** **[V3 · A-03 modificada · V4 · CR-2]** (§0.9). El adaptador lo
  aplica al **resultado raíz** de la definición, con las mismas reglas para literal y expresión.
  - **`Length` al escribir.** El contrato vigente del producto es `> 0`: RACKVARIABLES rechaza cualquier otro valor
    con «El valor tiene que ser un número mayor que cero.» (`RackProjectVariablesWindow.xaml.cs:215-224`; Discovery
    §10.3, §11.4). En V6 se aplica a **cada símbolo del cierre `I`** del estado «después», según su estado «antes»:

    | Caso | Estado antes | Estado después | Regla |
    |---|---|---|---|
    | **A** | Símbolo mutado X (`Create`, `ChangeDefinition`) | `Success` | La raíz de X cumple `> 0`, sea literal o expresión |
    | **B** | `Success` y cumplía el contrato | `Success` | Tiene que seguir cumpliéndolo: un cambio de `A` no puede dejar `Ajuste = A - B` en `-2` |
    | **C** | `Failed` o sin valor | `Success` | El valor nuevo **tiene que** cumplir el contrato |
    | **D** | Valor evaluable que ya incumplía por corrupción externa, con la definición intacta | `Success` | No bloquea por sí sola, según la política correctiva de V3; R1 sigue impidiendo que un efectivo inválido llegue a una propiedad (P24.5) |

    Si falla A, B o C: `OutOfRange`, cuyo dueño es la variable, y plan vacío. Que un símbolo pase de `Success` a
    `Failed`, o siga `Failed`, lo gobierna R2 (P21.3), no el contrato raíz.
  - **Escenario normativo del caso C** (Architect Re-review de V3):

    ```text
    antes:   A = #<id-roto> + 1      → Failed (BrokenReference)
             B = A - 100             → Failed (DependencyFailed)
             AlturaFinal = B + 200   → Failed (DependencyFailed)
    intent:  ChangeDefinition(A, "=Base")   con Base = 10
    después: A = 10    → caso A, cumple
             B = -90   → caso C, INCUMPLE → OutOfRange(B)
    resultado: el preflight FALLA y el plan queda vacío
    ```

    Sin el caso C, R2 no bloqueaba a `B` porque ya fallaba, R3 no lo detectaba porque al leer una raíz `≤ 0` no es un
    error, y R1 pasaba si ningún rack leía `B` directamente. La salida sigue existiendo: redefinir primero el símbolo
    que incumpliría (`B`) o elegir otra definición de `A`. Queda fijado en T-V3-16 y en T-V4-01.
  - **`Length` al leer.** Rige el contrato histórico de lectura, que exige un valor finito
    (`ProjectVariablesStore.cs:199-204`; `VariableTargetSnapshot.cs:72-76`; Discovery §11.4), **igual para las dos
    sintaxis**. Ningún valor persistido se re-valida con más rigor por ser expresión, y ningún literal con más rigor
    que hoy.
  - **Autoridad.** La comprobación de la ventana se conserva como aviso temprano. La autoridad pasa al preflight de
    Application, fijada por pruebas; hoy ninguna prueba fija `> 0` (Discovery §11.4).
  - **Resultado.** Para el mismo valor, una variable definida por literal y otra definida por expresión tienen la
    misma validez al escribir y al leer: la validez no depende de la sintaxis.
  - **Variables escalares o con signo.** Exigen otro `VariableType` u otra iniciativa (P30.5). I-49 no las introduce
    de forma implícita.

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
- **P9.5 — Una sola autoridad neutral de conversión** **[V4 · CR-1]**. Un único componente de Application
  (ilustrativo: `RackCad.Application.Units.LengthUnits`) declara la tabla cerrada de unidades, sus tokens y
  **únicamente** las conversiones genéricas:

  ```text
  LengthUnits.MillimetersPerInch = 25.4
  LengthUnits.InchesPerFoot      = 12
  ```

  No depende de nada: ni del núcleo, ni de `StructuralSections`, ni de sistemas. El núcleo la consume; **no** existe
  otro parser de unidades en UI, Domain ni Plugin.
- **P9.6 — Clasificación de las constantes existentes** **[V4 · CR-1]** (hechos verificados en `a4d88f1`). Una
  constante pertenece a la autoridad si su **papel** es convertir una magnitud entre unidades, no porque valga lo
  mismo que un factor de conversión.

  | Constante | Ubicación | Papel | Clase | Tratamiento en V6 |
  |---|---|---|---|---|
  | `InchesToMillimeters = 25.4`, pública | `src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs:18-19` | Conversión pulgadas↔milímetros de datos de sección | **Conversión genérica** | Conserva nombre y API; se declara como alias de `LengthUnits.MillimetersPerInch` |
  | `InchesPerFoot = 12d`, privada | `StructuralSectionUnits.cs:27` | Conversión de longitud en pulgadas a pies para el peso por longitud (`lengthInches / InchesPerFoot`, `:80`) | **Conversión genérica** | Se declara como alias de `LengthUnits.InchesPerFoot` |
  | `FootInches = 12.0`, pública | `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:31` | Paso de redondeo de la altura del Selectivo; solo lo usa `RoundUpToFoot` (`:297,326,422`) | **Regla de dominio** | **Local, sin cambio** |
  | `CommercialFoot = 12.0`, pública | `src/RackCad.Application/Systems/Dynamic/DynamicHeaderHeightCalculator.cs:48-49` | «Inches in a commercial foot (heights are sold in whole feet)»: módulo del redondeo comercial (`:146-155`) y de la pendiente «7/16" por pie comercial» (`:143-144`), que `PushBackBedSlope` lee a propósito para que la regla exista una vez (`PushBackBedSlope.cs:26-38`) | **Regla de dominio** | **Local, sin cambio** |
  | `12.0` en línea | `src/RackCad.Application/Systems/Cantilever/CantileverArmFrameResolver.cs:48` | Denominador de la fórmula aceptada `angle = atan(SlopeRisePer12 / 12)` (ADR-0025 D4) | **Notación de dominio** | **Local, sin cambio** |

  Tampoco son conversiones, aunque valgan 12: `SelectiveRackDefaults.DefaultSeparator`
  (`src/RackCad.Domain/Systems/Selective/SelectiveRackDefaults.cs:33`), `DynamicForkliftDefensePlan.EdgeLength`
  (`src/RackCad.Application/Systems/Dynamic/DynamicForkliftDefensePlan.cs:14`) y las medidas de pantalla de la UI.
  El factor lb/ft→kg/m duplicado en `tools/` (Discovery §12.3) está fuera de los ensamblados de producto y se registra
  como seguimiento en G4.
- **P9.7 — Autoridad real, sin ampliación** **[V3 · A-13 · V4 · CR-1]**.
  - `LengthUnits.MillimetersPerInch` y `LengthUnits.InchesPerFoot` son las **únicas** declaraciones de autoridad
    genérica de esas conversiones en `src/`.
  - `StructuralSectionUnits` conserva su API y declara `InchesToMillimeters` e `InchesPerFoot` como **alias** de la
    autoridad. Son constantes de compilación con el mismo valor, así que el comportamiento es idéntico bit a bit, los
    dorados vigentes no cambian y no queda una segunda constante semántica de conversión.
  - `SelectiveGeometryResolver.FootInches`, `DynamicHeaderHeightCalculator.CommercialFoot` y el `12.0` de
    `CantileverArmFrameResolver` **se quedan locales**. Son reglas o notaciones de dominio y cambian por razones de
    dominio, no porque cambie la definición del pie. I-49 **no** planifica ninguna modificación en esos tres sitios
    y, por tanto, no genera colisión con I-50 por `FootInches` (§12).
  - Dirección de dependencias: `StructuralSections` → `Units`. La autoridad no depende de nada, y el núcleo **nunca**
    referencia `StructuralSections` (guarda, P28.4).
  - **Guarda nueva** (defensa secundaria; el criterio son las pruebas de P9.3 y de paridad). Prohíbe **únicamente**:
    1. parsers, lexers o tablas de tokens de unidad `[mm]`, `[in]` o `[ft]` fuera de la autoridad del motor;
    2. un literal semántico de conversión `25.4` duplicado fuera de `LengthUnits`;
    3. una **segunda** autoridad genérica pies↔pulgadas: una constante o un conversor con papel de conversión
       genérica fuera de `LengthUnits`, salvo los alias de `StructuralSectionUnits` inicializados desde ella.

    La guarda se apoya en nombres y papeles declarados (por ejemplo `InchesPerFoot`, `FeetToInches` o
    `InchesToFeet`), **no** busca el número `12` de forma mecánica, y deja fuera por diseño las constantes de dominio
    de P9.6.
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
- **P10.3 — Conjunto inicial** (REQUISITO DEL OWNER; confirmado por el Architect):

  | Función | Aridad | Semántica |
  |---|---|---|
  | `MIN(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Menor valor |
  | `MAX(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Mayor valor |
  | `ABS(x)` | 1 | Valor absoluto |

- **P10.4** **[V3 · OPEN B]** — Al escribir: nombre desconocido → `UnknownFunction`; aridad errónea →
  `InvalidArguments`. En un árbol persistido: token de función desconocido → ilegibilidad **estructural** (P17.6);
  función conocida con aridad errónea → error **semántico** `InvalidArguments`.
- **P10.5** — Nombre sin distinguir mayúsculas a la entrada (`max`, `Max`); mayúsculas canónicas en el árbol y en
  el formatter.
- **P10.6 — Candidatos evaluados y DIFERIDOS: `ROUND`, `CEILING`, `FLOOR`.**

  | Pregunta que bloquea | Por qué no se decide en V6 |
  |---|---|
  | Regla del punto medio | `Math.Round` de .NET redondea al par por defecto y las hojas de cálculo suelen alejarse del cero: una u otra cambia resultados sin avisar |
  | Precisión o múltiplo | ¿Entero? ¿Dígitos? ¿Un múltiplo, como `CEILING(x, 6)`? Cada firma es un contrato persistido distinto |
  | Interacción con unidades | Tras `100[mm]`, redondear a pulgada entera no es lo que el usuario escribió en milímetros |
  | Negativos | Sentido de `CEILING` y `FLOOR` con valores negativos |
  | Consumidor natural | Las cantidades **enteras** del BOM (`Quantity`, Discovery §16.3) son de ID23, que tendrá que fijar su semántica |

  Pueden añadirse después como ampliación **cerrada** del registro, con revisión, y con ADR si cambian lo que se
  persiste. Una build anterior las rechaza de forma estructural (P18.8).
- **P10.7 — Excluidos salvo nueva razón aprobada**: `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings,
  trigonometría y macros.
- **P10.8** — Añadir una función es un cambio de código con revisión y, para las builds anteriores, un cambio de lo
  que pueden leer (P18.8).
- **P10.9 — Tabla de tokens y palabras reservadas** **[V3 · A-17 · OPEN A]**.
  - Los tokens persistidos `MIN`, `MAX` y `ABS` forman una tabla cerrada y explícita, comparada en `Ordinal`.
  - Esa misma tabla es la fuente de las palabras reservadas de §4: `{MIN}` es la variable llamada `MIN`, y `MIN`
    sin llaves es `ReservedName`.
  - Añadir una función amplía la lista de reservadas **solo para escrituras futuras**. Lo persistido no cambia,
    porque lleva ids, y el formatter pasa a mostrar con llaves la variable homónima de la función nueva.

### P11 — dependency extraction

**Base.** Hoy no existe ninguna relación variable→variable (Discovery §6.2). `Targets()` ordena por id
`OrdinalIgnoreCase` (Discovery §13.3).

- **P11.1** — `Dependencies(BoundExpression) → IReadOnlyList<SymbolId>`: referencias **directas**, sin repetición y
  en orden determinista (namespace `Ordinal`; después clave con el comparador de su namespace, el mismo orden que
  `Targets()`).
- **P11.2** — Opera sobre el árbol persistido **sin evaluar** y **sin nombres**. Funciona igual sobre un árbol
  estructuralmente legible que falla al evaluar (§5).
- **P11.3** — Un literal tiene cero dependencias. Una referencia directa de rack depende de su único `VariableId`.
- **P11.4** — La consumen el grafo (P12), `Delete` y `UnlinkAllAndDelete` (P20), la propagación y el `PlanReadSet`
  (P21), RACKVARIABLES (P22) y la preservación para ID28/ID29 (P27).
- **P11.5** — La **misma** función extrae las dependencias de una expresión de definición y de una expresión de
  propiedad: no hay una segunda implementación por superficie.

### P12 — dependency graph

**Base.** ADR-0034 rechazó persistir un grafo en ID22A porque «el grafo pertenece a ID22B/ID21» (Discovery §6.2). La
relación propiedad→variable vive en el authored de cada rack y se descubre barriendo el dibujo (Discovery §7).

- **P12.1 — Derivado, nunca persistido.** Se construye desde la tabla de símbolos de UN snapshot. El registro es la
  única autoridad: un grafo persistido sería una segunda fuente capaz de divergir.
- **P12.2** — Nodos: los símbolos de la tabla (en V6, las variables de proyecto). Aristas: dueña → dependencia, con
  índice inverso dependencia → dependientes.
- **P12.3 — Consumidores de rack fuera del grafo.** Las relaciones propiedad→variable, directas **o** por expresión,
  no entran en el grafo de variables: se obtienen del barrido vigente y se unen al componer el plan (P21.4). Es la
  separación que el Discovery dedujo para su pregunta 8 (Discovery §2.2).
- **P12.4** — Una arista hacia un id ausente se registra como `BrokenReference`, sin crear nodo.
- **P12.5** — Iteración en el orden de P11.1.
- **P12.6** — Coste `O(V + E)` por snapshot. Que los registros reales sean pequeños es **INFERENCE**; se mide en G11.
- **P12.7 — Conjuntos derivados que expone cada operación** **[V3 · A-05]**:
  - `Dependencies(s)` y `Dependents(s)`, directos y transitivos;
  - **conjunto afectado**: el cierre `I` y los racks que lo consumen (P21.2);
  - **conjunto leído**: las observaciones del `PlanReadSet` del plan, cada una con su fase o, si es una decisión de
    reparación, con su razón (P21.6).

  Todos se derivan en memoria y **ninguno** se persiste (P27).

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
- **P13.5 — Ciclo persistido** **[V3 · OPEN B]**. Con P13.3, esta build no escribe ciclos; uno leído de persistencia
  es un error **semántico**:
  - todos los miembros dan `Failed(Cycle)`, con la ruta completa en el orden de P11.1;
  - sus dependientes dan `DependencyFailed`, y las fuentes de rack que los leen, `RepairableSemanticFailure` con causa
    superior (P24.6);
  - el registro sigue `Usable` y RACKVARIABLES lo muestra en su panel de diagnóstico (P22.9);
  - se rompe con `ChangeDefinition` de **un** miembro, que tiene que evaluar bien (regla R2, P21.3);
  - borrar un miembro sigue bloqueado, porque tiene dependientes (P20.2).
- **P13.6** — Las expresiones de propiedad **no** forman ciclos: nada puede referenciar una propiedad en V6 (ID21
  fuera). Solo pueden quedar afectadas por un ciclo de variables.

### P14 — evaluation order

**Base.** Hoy el resolver escribe `Target.LiteralValue` y ningún valor se calcula a partir de otro (Discovery §6.2).
La igualdad es exacta (Discovery §11.4).

- **P14.1** — Orden topológico sobre la parte acíclica, con el desempate de P11.1, **independiente** del orden de las
  entradas en el registro. Una prueba lo demuestra con permutaciones (P28).
- **P14.2** — Las **variables** se evalúan de forma **ansiosa**, una vez por snapshot (`RegistryEvaluation`) y con
  memoización: cada una se evalúa una sola vez.
- **P14.3** — Cortocircuito: si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la
  causa, sin evaluarse.
- **P14.4 — Semántica numérica.** `double` IEEE-754 sin redondeos intermedios. `x ÷ 0`, incluido `0 ÷ 0`, es
  `DivisionByZero`. Cualquier resultado intermedio o final no finito es `NonFiniteResult`. Las unidades se
  convierten al evaluar su literal (P9.3).
- **P14.5 — Determinismo.** Sin paralelismo, cultura ni reloj: el mismo snapshot da resultados idénticos bit a bit.
- **P14.6** — Un literal se evalúa a su propio valor, sin cambio. Por eso todo dibujo de I-47/I-48 produce
  **exactamente** los mismos efectivos que hoy, con la única excepción declarada de P24.11.
- **P14.7 — Dónde.** Dentro de Application: las variables tras la acreditación, y las expresiones de propiedad dentro
  del resolver único (DR-3). **Nunca** en el Plugin, la UI, el ejecutor ni el comando de BOM (guardas extendidas,
  P28.4).
- **P14.8 — Expresiones de propiedad.** Se evalúan **una vez por rack** durante su resolución, sobre los valores de
  `RegistryEvaluation` de ese mismo snapshot. Un fallo de cualquier propiedad deja **sin efectivo** a todo el rack,
  igual que hoy un binding roto (Discovery §4.2: el resolver valida todo antes de escribir un campo).
- **P14.9 — Un solo dueño de los valores evaluados** **[V3 · A-14]**.
  - `RegistryEvaluation` es el **único** dueño de los valores de los símbolos: se calcula una vez por snapshot
    acreditado.
  - Targets acreditados, opciones del editor, workspace de RACKVARIABLES, preflight y RACKBOMTOTAL **transportan**
    sus valores y resultados. Ninguno vuelve a evaluar un símbolo como autoridad.
  - El efectivo de una propiedad lo produce el resolver único a partir de `RegistryEvaluation`.
  - La sesión del editor evalúa un **borrador** solo para presentarlo y avisar pronto (P23.6). La autoridad es la
    resolución del reconciliador, y el valor mostrado nunca se escribe.

### P15 — diagnostics

**Base.** La doctrina exige estados tipados (ADR-0034 §8). El texto de reparación ya vive en una sola capa
(`ProjectVariableRepairText`, Discovery §3.4).

- **P15.1** — Catálogo **cerrado** de códigos. Cada diagnóstico lleva código estable, clase, severidad, dueño (una
  variable, o un par `(RackId, PropertyId)`), posición opcional (solo al escribir) e ids relacionados. Un fallo
  por causa superior lleva además la cadena hasta la causa raíz.
- **P15.2 — Severidad.** En V6 todo diagnóstico es `Error`: no hay avisos que permitan continuar.
- **P15.3 — Catálogo de V6.**

  | Clase | Códigos | Cuándo |
  |---|---|---|
  | Sintaxis y límites | `EmptyExpression`, `UnexpectedCharacter`, `UnexpectedToken`, `UnbalancedParenthesis`, `UnterminatedName`, `InvalidNumber`, `AmbiguousDecimalComma`, `UnknownUnit`, `UnitNotAllowedHere`, `UnitSyntaxNotSupported`, `InvalidQualifier`, `LimitExceeded` | Al escribir. `LimitExceeded` también tras el bind, para los límites normativos del árbol (P1.9) |
  | Enlace | `UnknownSymbol`, `AmbiguousName`, `UnknownNamespace`, `UnknownFunction`, `ScopeViolation`, `ReservedName`, `NameRequired`, `QualifiedNameMismatch`, `OperatorInName` | Al escribir |
  | Semánticos | `BrokenReference`, `Cycle`, `DependencyFailed`, `InvalidArguments`, `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` | Al escribir y al evaluar lo persistido (§5) |
  | Contrato de frontera | `OutOfRange`, con dueño variable (adaptador, solo al escribir, P8.10) o propiedad (consumidor, P24.5) | Al escribir y, para propiedades, al resolver |

- **P15.4** — El núcleo devuelve códigos y datos. Los mensajes en español se producen en **una** capa de texto de
  Project Variables o de UI.
- **P15.5** — Cualquier `Error` implica: sin valor, sin resultado parcial y sin fallback.
- **P15.6** — Los estados de nivel registro conservan sus resultados vigentes (`PresentButUnreadable`,
  `IncompatibleMajor`, `AmbiguousIdentity`; Discovery §4.1). Los diagnósticos **no** los sustituyen.
- **P15.7** — Orden determinista: posición, código y dueño.
- **P15.8 — Cambios respecto de V2.**
  - Nuevos por OPEN A: `UnterminatedName`, `InvalidQualifier`, `ReservedName`, `NameRequired`,
    `QualifiedNameMismatch` y `OperatorInName`.
  - Por OPEN B: `ArityMismatch` pasa a llamarse `InvalidArguments`, igual al escribir y al leer.
  - Por A-07: `NonCanonicalForm`, semántico.
  - Por A-03 modificada: `OutOfRange` **se conserva** para la raíz de una variable, pero solo al escribir. El
    Architect proponía quitarlo de las variables; V3 no lo adoptó para las escrituras, por la regla vinculante de
    §0.9, y la re-revisión aceptó ese remedio. Al leer ninguna variable recibe `OutOfRange`, lo que coincide con la
    clasificación de OPEN B (§5.1).
  - Por CR-2: `OutOfRange` de variable también para un símbolo que pasa de `Failed` a `Success` con una raíz que
    incumple el contrato (caso C de P8.10).
- **P15.9 — Lo estructural no es un diagnóstico por símbolo** **[V3 · OPEN B]**. Da un resultado de nivel registro
  (`PresentButUnreadable`) o `FatalMalformedReference` en el rack. Su motivo tipado conserva `UnknownKind` y
  `UnreadableVariableId` y gana `MalformedExpression`, con submotivo: nodo, namespace, función o unidad desconocidos;
  campo desconocido o repetido; campo obligatorio ausente; número no finito; límite superado; dos autoridades. Los
  nombres son ilustrativos.

### P16 — EvaluationResult

**Base.** El target acreditado tiene hoy forma de literal (`VariableTargetSnapshot`, Discovery §4.1 y §4.4).

- **P16.1 — Por símbolo**: `EvaluationResult { SymbolId, Outcome = Success | Failed, Value, Diagnostics, Trace }`.
  `Value` es un `double` finito que solo existe con `Success`; acceder a él en otro caso **lanza**, con la misma
  disciplina que `LiteralValue`. `Diagnostics` no está vacío si y solo si `Failed`. **Sin** campo de tipo (DR-5).
- **P16.2 — Por snapshot**: `RegistryEvaluation { resultados por símbolo, grafo directo e inverso, ciclos, orden de
  evaluación }`, inmutable.
- **P16.3 — Traza opt-in** (P27): definición enlazada, dependencias directas con el valor que tenían y cadena de
  causas de un fallo. Tamaño acotado, solo en memoria y desactivable sin cambiar ningún resultado.
- **P16.4** **[V3 · A-14]** — Los consumidores leen `Value` solo tras comprobar `Success`. El target acreditado
  **expone** el resultado de `RegistryEvaluation` para su variable; no lleva una evaluación propia.
- **P16.5** — Una variable literal da `Success(literal)` con traza trivial.
- **P16.6 — Por propiedad de rack**: el resolver produce, de forma transitoria, un resultado equivalente por
  `(RackId, PropertyId)`. Lo consumen la resolución, el preflight, `InspectBinding` y el texto de estado del editor.

### P17 — persistence

**Base.** El registro es un `Xrecord` troceado en el NOD bajo `RACKCAD_PROJECT`, con propiedades en PascalCase,
`PropertyNameCaseInsensitive` y `JsonStringEnumConverter` (`ProjectVariablesStore.cs:249-262`; Discovery §5.1). Los
bindings del Selectivo viven en el mapa opcional `PropertyValues` del portador authored (Discovery §5.1). El store del
registro rechaza todo kind distinto de `literal` (Discovery §5.3), y `SelectivePropertyValueDocument` declara que un
kind desconocido «is NOT ignored and does NOT fall back to the literal»
(`src/RackCad.Application/Persistence/SelectivePropertyValueDocument.cs:17-20`).

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

  `sub`, `mul` y `div` tienen la forma de `add`; `{}` señala un nodo anidado. El número se persiste **tal como se
  escribió**, con su unidad: la conversión ocurre al evaluar (P9.3), así el texto editable conserva `100[mm]`.
- **P17.5 — El valor evaluado NO se persiste**, ni en el registro ni en el rack. Se deriva en cada snapshot (DR-3).
  Un valor cacheado sería una segunda autoridad capaz de quedar obsoleta, y no ayuda a las builds anteriores, que
  rechazan el kind (P18.2).
- **P17.6 — Validación ESTRUCTURAL** **[V3 · OPEN B · A-07 · A-08]**. La build no entiende el significado. En el
  registro, cualquier caso da `PresentButUnreadable` para el registro entero, como hoy con un kind desconocido; en el
  rack, el binding es `FatalMalformedReference` y el rack queda `Blocked`, sin reparación destructiva (§5):
  1. `Kind` desconocido, en la definición o en la fuente (sin cambio, C4-8);
  2. **dos autoridades** en la misma entrada: `expression` con `Value` o `VariableId`, o sin `Expression`;
     `literal` o `projectVariable` con `Expression`;
  3. `Node` o `Namespace` desconocido persistido;
  4. token de `Function` desconocido persistido;
  5. token de `Unit` desconocido persistido, o `Unit` en un nodo que no es `number`;
  6. id o AST mal formado: `Id` no parseable, campo obligatorio ausente, número no finito, o límites normativos de
     P1.9 superados: nodos, argumentos o profundidad del `BoundExpression` según su definición normativa, que cuenta
     cada nodo de las cadenas binarias **[V4 · CR-3]**;
  7. **payload de Expression con mundo cerrado** **[V3 · A-08]**: cualquier campo desconocido, o repetido con la
     comparación de nombres vigente, dentro de:
     - la definición de variable cuyo `Kind` es `expression`, en todo su objeto;
     - la entrada de `PropertyValues` cuyo `Kind` es `expression`, en toda la entrada;
     - cualquier nodo del árbol, a cualquier profundidad.

  **Ya no es estructural** la forma no canónica: pasa a ser el error semántico `NonCanonicalForm` (P2.8, A-07).
- **P17.7 — Los errores semánticos NO son errores de lectura.** `BrokenReference`, `Cycle`, `InvalidArguments`,
  `DivisionByZero`, `NonFiniteResult`, `DependencyFailed`, `NonCanonicalForm` y el `OutOfRange` del consumidor se
  detectan al evaluar o al resolver, dan un resultado por símbolo o por fuente y dejan el registro estructuralmente
  `Usable` (§5).
- **P17.8** — Leer nunca escribe (Discovery §5.7). Solo escriben una `RegistryMutation` o una `RackMutation` tras
  un preflight, o el reconciliador de RACKEDITAR.
- **P17.9** — Los números se serializan con `System.Text.Json` por defecto, así que `6.0` puede reescribirse como `6`,
  igual que hoy (Discovery §5.1).
- **P17.10 — Tablas de tokens y comparadores** **[V3 · A-17]**. Cada token persistido pertenece a una tabla
  **cerrada y explícita**, con sus dos sentidos declarados y fijados por prueba:

  | Token | Valores | Comparación al leer |
  |---|---|---|
  | `Kind` de definición | `literal`, `expression` | `OrdinalIgnoreCase`, como `literal` hoy |
  | `Kind` de binding | `projectVariable`, `expression` | `Ordinal`, como `projectVariable` hoy (Discovery §13.3) |
  | `Node` | `number`, `ref`, `neg`, `add`, `sub`, `mul`, `div`, `call` | `Ordinal` |
  | `Namespace` | `projectVariable` | `Ordinal` |
  | `Unit` | `mm`, `in`, `ft` | `Ordinal` |
  | `Function` | `MIN`, `MAX`, `ABS` | `Ordinal` |

  - Los nombres de propiedad JSON siguen sin distinguir mayúsculas (`PropertyNameCaseInsensitive`, Discovery §5.1).
    Un nombre repetido dentro de un payload de Expression es estructural (P17.6).
  - El `JsonStringEnumConverter` de los stores (`ProjectVariablesStore.cs:249-262`;
    `SelectivePalletDesignStore.cs:67-76`) **no** puede producir ningún token de Expression.
  - **Rama que I-49 reescribe.** El `Add` del registro escribe hoy `Type = Type.ToString()` (`MutationPlan.cs:100`),
    y el mapping único compara contra el nombre del miembro (`VariableType.cs:89`). Al extender esa rama, I-49
    escribe y lee `Type` desde una tabla explícita del **mismo** mapping único.
    - El lenguaje aceptado no cambia: el nombre declarado, sin distinguir mayúsculas, sin forma numérica y sin
      listas (`VariableType.cs:64-71`).
    - La salida es byte a byte `"Length"`.
    - Es preservación de la invariante de un solo mapping (decisiones de I-48 §4), no un arreglo lateral.
  - `Id`: el parseo y la igualdad vigentes de `VariableId` (Discovery §4.1).
- **P17.11 — Una sola autoridad de interpretación en el rack.** La lectura de una fuente `expression` vive en
  `LinkedPropertyInspection.InspectBinding` (Discovery §4.1), no dispersa por el portador authored ni por el Plugin.
  `SelectivePalletDesignDocument.cs` no cambia (§12).
- **P17.12 — Política histórica de `ExtensionData` fuera de los payloads de Expression** **[V3 · A-08]**. **No** se
  elimina de forma global. La raíz del registro, la entrada de variable, la definición `literal`, el binding
  `projectVariable`, el portador Selectivo y el sobre conservan su tolerancia y su preservación de campos
  desconocidos, exactamente como hoy (Discovery §5.4, §5.8). El mundo cerrado de P17.6.7 rige **solo** dentro de los
  payloads de Expression.

### P18 — schema evolution

> **CLOSED: V-0.** Sin cambio de versión en `ProjectVariablesDocument` ni en el diseño Selectivo (§6).

- **P18.1 — Política real** (hechos con fuente):
  1. **Un major protege a las builds que leerían MAL.** La línea 2.0 del Selectivo existe para que una build
     anterior «REFUSES to open it… instead of silently unbinding the rack»
     (`SelectivePalletDesignDocument.cs:26-32`; decisiones de I-47, C4-9).
  2. **Un minor mayor del mismo major se lee y se preserva** al re-guardar, con sus campos desconocidos (C4.6-8,
     C4-9; `SchemaVersionPolicy.ResolveWriteVersion`, Discovery §5.4).
  3. **Un kind discriminado hace de un caso nuevo algo que NO rompe el schema**, y un kind desconocido nunca se
     ignora. Binding: «Discriminating from the first day makes that a new case instead»
     (`SelectivePropertyValueDocument.cs:11-20`). Definición: kind desconocido = ERROR, «ID22B decidirá su
     evolución» (C4-8).
  4. **Monotonía, sin oscilación por contenido**: la versión sube y nunca baja; decidir el major por el contenido
     del momento se rechaza expresamente (`src/RackCad.Application/Persistence/SelectiveDesignSchema.cs:6-19`).
- **P18.2 — Qué hace hoy una build I-47/I-48** ante lo que V6 persiste (hechos del código en `a4d88f1`):

  | Dato nuevo | Comportamiento de la build anterior | Evidencia |
  |---|---|---|
  | Registro con una definición `expression` | Registro **entero** `PresentButUnreadable`; RACKEDITAR de todo Selectivo, RACKVARIABLES y RACKBOMTOTAL bloqueados | `ProjectVariablesStore.cs:193-197`; Discovery §5.3 |
  | Rack con una fuente `expression` | `FatalMalformedReference(UnknownKind)`; el resolver da `UnknownReferenceKind`; la sonda da `Indeterminate` y el descubrimiento **aborta** | `BindingInspection.cs:205`; `SelectiveEffectiveDesignResolver.cs:174-176`; `ProjectVariableConsumerProbe.cs:74,83`; `ProjectVariableConsumerDiscovery.cs:122` |

  Ninguna lee **mal**: las dos fallan cerradas en la entrada o fuente exacta. Coste real: en una build anterior, un
  rack con expresión bloquea las operaciones de variables que necesitan descubrimiento en **todo** el dibujo
  (INFERENCE sobre el código citado).
- **P18.3 — Decisión: V-0** **[V3 · A-11]**. Se justifica **por el comportamiento de los lectores**:
  - las builds I-47/I-48 ya rechazan el discriminador desconocido y no leen mal la semántica nueva (P18.2), así que
    un major no protege nada (política 1);
  - las expresiones son casos nuevos de discriminadores diseñados para eso (política 3);
  - con el mundo cerrado de P17.6.7, todo lo nuevo **dentro** del payload también falla cerrado por estructura
    (P18.8).
- **P18.4 — Por qué no V-1** (minor `1.1` / `2.1` por contenido). Ningún lector cambia de comportamiento: solo añade
  una etiqueta *sticky*, que L2 borraría si no se corrigiera (`LinkedPropertyReconciler.cs:218`). El argumento de V2
  de que V-1 obligaba a tocar `SelectivePalletDesignDocument.cs` era incorrecto: la constante podría vivir en
  `SelectiveDesignSchema.cs` (A-11).
- **P18.5 — Por qué no V-2** (major). Dejaría un dibujo fuera del alcance de las builds anteriores **para siempre**,
  aunque ya no quedara ninguna expresión, sin evitar ninguna lectura errónea.
- **P18.6 — Condiciones obligatorias de V-0** (§6.2):
  1. payload de Expression con mundo cerrado (P17.6.7);
  2. semántica nueva de Expression solo por discriminadores o tokens cerrados que la build conozca, o por un major
     que obligue a rechazar (§6.3);
  3. los escritores de I-49 **nunca** degradan ni fijan una versión: la calculan con la autoridad vigente (P23.15);
  4. pruebas de caracterización del comportamiento heredado **antes** de modificar los stores (G8, paso 1).
- **P18.7 — Qué NO justifica V-0.** El número de archivos tocados no es un argumento. Tampoco el precedente de I-50
  sobre el mismo portador, que V6 registra solo como hecho de coordinación (§12).
- **P18.8 — Futuro.** Todo lo nuevo dentro del payload de Expression —nodos, unidades, funciones, namespaces o
  campos— es fail-closed para esta build (P17.6), con cualquier versión. Cada adición exige revisión y, si cambia
  persistencia, ADR.
- **P18.9 — Mensajes.** Una build anterior muestra «declara una definición de clase desconocida» o una referencia de
  kind desconocido, no «versión más nueva» (Discovery §5.6). Es un coste de comunicación, no de integridad, y se
  documenta en el ADR y al usuario.

### P19 — rename

**Base.** `Rename` es registry-only: sin barrido, sin racks y sin redibujo (Discovery §8.1; decisiones de I-47
C4.6-7). Hoy valida reconstruyendo un literal (Discovery §5.5, fila 10).

- **P19.1** — Sigue siendo registry-only.
- **P19.2** — Ni las definiciones ni las fuentes de propiedad cambian, porque refieren ids. El formatter muestra el
  nombre nuevo de inmediato (P4).
- **P19.3** — La validación de `Rename` conserva la definición vigente, literal o expresión, en lugar de reconstruir
  un literal.
- **P19.4** **[V3 · OPEN A]** — Las reglas de nombre no cambian: no en blanco, homónimos legales y sin política de
  unicidad, mayúsculas ni caracteres (Discovery §13.1).
  - Un rename puede volver **ambiguo** un nombre para escrituras futuras. El formatter pasa a cualificarlo (P4.7) y
    `AmbiguousName` solo afecta a textos que se tecleen después.
  - Renombrar a una palabra reservada, como `MIN`, está permitido: el formatter la muestra con llaves.
  - Nunca cambia el significado de nada persistido.
- **P19.5** — El requisito «rename no rompe» de OPEN A se cumple por construcción.
- **P19.6** — Un rename no redibuja ningún rack, aunque lo consuma una expresión: su efectivo no cambia.
- **P19.7** **[V3 · OPEN B]** — `Rename` está permitido siempre que el registro sea legible y acreditado, también con
  errores semánticos: no cambia ninguna evaluación. Su plan no lleva observaciones: el commit re-lee, re-acredita y
  valida el id, pero no compara ningún resultado, así que un símbolo en error no lo aborta **[V5 · FR-1]** (P21.6).

### P20 — delete

**Base.** `Delete` solo cuenta consumidores propiedad→variable directos, y `UnlinkAllAndDelete` materializa los
consumidores directos (`ProjectVariableMutationPreflight.cs:220-298`; Discovery §8.1, §8.2). ADR-0034 §11 rechaza
borrar materializando automáticamente.

- **P20.1 — Consumidores de X**: toda propiedad cuya fuente sea `ProjectVariableReference(X)` **o** una `Expression`
  cuyas dependencias (P11) incluyan X.
- **P20.2** — `Delete(X)` queda **bloqueado** si X tiene consumidores **o** dependientes variable→variable (P12). Se
  informan ambas listas. Sin cascada y sin materializar nada.
- **P20.3** — Los dependientes salen del snapshot acreditado **sin** barrer el dibujo (Discovery §6.3). Los
  consumidores salen del barrido.
- **P20.4 — `UnlinkAllAndDelete(X)`** **[V3 · A-02]** (§7.1):

  | Consumidor o dependiente de X | Resultado |
  |---|---|
  | Referencia directa `ProjectVariableReference(X)` | Se materializa **como hoy**: el efectivo pasa a literal y se quita el binding |
  | Expresión de propiedad que depende de X | **BLOQUEADO** |
  | Definición de variable que depende de X | **BLOQUEADO** |

  - Sin conversión implícita de una fórmula en literal y sin sustituir el valor de X dentro de la fórmula.
  - El bloqueo informa los dependientes (variable y definición formateada) y los consumidores por expresión (rack,
    propiedad y fórmula canónica).
  - Si solo hay referencias directas, cada rack materializado tiene que resolver después (regla R1; hoy
    `ProjectVariableMutationPreflight.cs:287`).
- **P20.5** — Borrar una variable definida por expresión, sin consumidores ni dependientes, está permitido y es
  registry-only.
- **P20.6** — Resultados tipados `BlockedByConsumers`, `BlockedByDependents` y `BlockedByExpressionConsumers`, con sus
  listas. Los nombres son ilustrativos.
- **P20.7** **[V3 · OPEN B]** — Con errores semánticos en el registro rigen las mismas reglas.
  - Se puede borrar una variable en error que no tenga consumidores ni dependientes: eso quita un error y no
    escribe ninguno (R3).
  - No se puede borrar un miembro de ciclo, porque tiene dependientes.
  - Un borrado **nunca** fabrica una `BrokenReference`: los consumidores y dependientes lo bloquean antes.
- **P20.8 — Commit de las eliminaciones** **[V5 · FR-1]** (P21.6). X no existe en «después», así que ninguna
  eliminación lleva una observación `After(X)`:
  - `Delete(X)` no lee ningún valor. Su preflight acredita, comprueba el id y bloquea por consumidores o dependientes:
    son precondiciones, no lecturas de valor. El plan no lleva observaciones, y el commit re-lee, re-acredita, valida
    el id y escribe, también si X está en error (P20.7);
  - `UnlinkAllAndDelete(X)` con solo referencias directas materializa el valor de X leído **antes** de eliminarlo
    (`ProjectVariableMutationPreflight.cs:254-255,283`). Lleva `Before(X, Success(valor materializado))` y, en
    `After`, lo que leen los racks finales. Si X cambió o desapareció entre preflight y commit, aborta antes de
    escribir; si no, hace commit.

### P21 — propagation

**Base.** Traza vigente de `ChangeValue` en 14 pasos (Discovery §6.1). La profundidad 1 vive en el descubrimiento, el
plan, la resolución, `Delete` y RACKVARIABLES (Discovery §6.2). `MutationDestinationBinding` aborta si una definición
aparece dos veces (Discovery §6.3). RACKVARIABLES hace hoy un descubrimiento por variable al abrir (Discovery §7).

- **P21.1** — `ChangeDefinition(X, d)` generaliza `ChangeValue` a las cuatro transiciones de una definición:
  literal→literal, literal→expresión, expresión→literal y expresión→expresión.
- **P21.2 — Preflight** (puro, todo o nada), con la cadena que exige el Owner:
  1. acreditar el registro «antes», `LastRegistry` (sin cambio);
  2. **validar la forma enlazada** que trae el intent, nunca texto (P7.7): todo id existe en «antes»; aridad, ámbito
     y forma canónica correctos;
  3. «después» = aplicar la mutación a un clon del documento acreditado, acreditarlo y evaluarlo
     (`RegistryEvaluation`, P14);
  4. **cierre**: `I = {X} ∪ dependientes transitivos de X` en el grafo «después», en el orden de P11.1;
  5. **regla R2** (P21.3): X da `Success`; ningún símbolo que daba `Success` en «antes» falla en «después»; un
     símbolo que ya fallaba y cuya definición no cambia no bloquea. Un ciclo que pase por X da `Cycle` (P13.3);
  6. **contrato raíz** (P8.10, casos A–D) **[V4 · CR-2]**: la raíz de X cumple el contrato (A); toda raíz de `I`
     que cumplía antes sigue cumpliéndolo (B); toda raíz de `I` que fallaba antes y obtiene valor lo cumple (C); una
     raíz que ya lo incumplía por corrupción externa y conserva su definición no bloquea por sí sola (D);
  7. **consumidores de rack de cualquier variable de `I`**: referencias directas **y** expresiones de propiedad
     cuyas dependencias toquen `I`, sobre **una** proyección del barrido y deduplicados por `RackId` (P21.4);
  8. **regla R1**: cada rack se resuelve **una sola vez** contra el estado final, con todas sus propiedades, sus
     expresiones evaluadas y el dominio de cada propiedad (P24.5). Si alguno no resuelve, el plan queda vacío. Si
     el rack ya estaba afectado antes, el mensaje indica que hay que repararlo primero, lista sus fuentes inválidas
     y advierte qué fórmulas eliminaría la reparación, con la regla de §7.2 **[V4 · CR-4]**;
  9. **`PlanReadSet`** (P21.6) **[V5 · FR-1]**: las observaciones que los pasos 5, 6 y 8 realmente usaron, cada una
     con su fase y su resultado esperado. En `After`, cada símbolo de `I` y lo que leen los racks; en `Before`, cada
     símbolo de `I` distinto de X, cuyo estado previo comparan R2 y el contrato raíz. Un fallo que R2 admite se
     guarda como fallo tipado esperado, no como motivo de aborto;
  10. **un `MutationPlan` coherente**: **una** `RegistryMutation` (en el registro solo cambia X) + **una**
      `RackMutation` por rack, con efectivo completo y todas sus vistas.
- **P21.3 — Reglas de intents correctivos** **[V3 · OPEN B]** (§5.2, decisión 6):
  - **R1** — Todo rack que consuma el cierre tiene que resolver después; si no, plan vacío. Es la regla de I-47/I-48,
    sin cambios.
  - **R2** — El símbolo mutado evalúa bien; un símbolo sano no puede fallar después; los que ya fallaban, con su
    definición intacta, no bloquean. En commit, esos fallos admitidos son resultados esperados del `PlanReadSet`: si
    siguen idénticos, no abortan (P21.6) **[V5 · FR-1]**.
  - **R3** — Ningún intent escribe un error nuevo.

  Con R1, R2 y la reparación (§7.2) siempre hay salida, también con dos errores dentro de un ciclo.
- **P21.4 — Descubrimiento por conjunto.** La semántica de la sonda no cambia: tri-estado, `Indeterminate` aborta,
  positivos parciales abortan y se exige autoridad `Single` (Discovery §7).
  - Una fuente `expression` estructuralmente legible es positiva si sus dependencias (P11) cortan `I`, **aunque**
    falle al evaluar.
  - Un kind desconocido o un payload estructuralmente mal formado es `Indeterminate`.
  - Se calcula **una vez** sobre el barrido y tiene que ser **equivalente** a la unión de descubrimientos por variable;
    esa equivalencia se prueba como oráculo (P28). Evita el coste por variable del riesgo R5.
- **P21.5 — Una transacción, un commit, un `Regen`.** La estructura del ejecutor no cambia (PREPARE / MUTATE / POST;
  Discovery §6.1) y **no** hay regen por variable intermedia (REQUISITO DEL OWNER). El ejecutor **no** parsea ni
  evalúa (guarda G13 extendida, P28.4).
- **P21.6 — `PlanReadSet` y re-lectura de commit** **[V3 · A-05 · V4 · CR-5 · V5 · FR-1 · V6 · RR-1]**. V2 afirmaba
  que el efectivo de cada rack dependía solo de los valores de `I` y de su authored. Es falso: un rack planificado
  puede leer variables fuera de `I`. V4 hizo contractual el conjunto leído, pero como una lista plana de valores
  esperados de «después» que abortaba si cualquier símbolo fallaba. Eso contradecía R2 y rompía `Delete` y
  `UnlinkAllAndDelete` (FR-1). V5 lo sustituyó por observaciones, y V6 añade las decisiones de reparación de
  `RepairBrokenRack` (RR-1).
  - **Observaciones** (nombres ilustrativos):

    ```text
    PlanReadObservation
        = SymbolResultObservation
        | RepairDecisionObservation

    SymbolResultObservation                         // V5 · FR-1, sin cambio
    {
        SymbolId,
        Phase:          Before | After,
        ExpectedResult: Success(double value) | Failed(typedFailure)
    }

    RepairDecisionObservation                       // V6 · RR-1
    {
        RackId,
        PropertyId,
        ExpectedRepairReason
    }
    ```

    - **`SymbolResultObservation`** (V5, sin cambio):
      - `Before`: información semántica que el preflight usó **antes** de aplicar la `RegistryMutation`, sobre el
        registro «antes» acreditado.
      - `After`: información que el preflight usó sobre el estado hipotético «después», el que produce `ApplyTo`.
        Aquí viven, entre otros, los valores con los que se construyen los racks finales del plan.
      - `ExpectedResult` es el resultado de ese símbolo en la `RegistryEvaluation` de esa fase durante el preflight,
        transportado como dato.
      - Un mismo `SymbolId` puede tener una observación `Before` y otra `After` si el plan realmente lo usa en las
        dos.
    - **`RepairDecisionObservation`** **[V6 · RR-1]**: la decisión de retirar la fuente `(RackId, PropertyId)` en un
      plan de `RepairBrokenRack`.
      - `ExpectedRepairReason` es la clasificación reparable que dio `InspectBinding` (P24.6) sobre esa fuente
        durante el preflight. Hoy esa inspección es la única autoridad semántica sobre un binding persistido
        (`src/RackCad.Application/ProjectVariables/BindingInspection.cs:143-241`), y la reparación la aplica a todo
        el rack con `SelectiveLinkedPropertyKernel.Assess` (`SelectiveLinkedPropertyKernel.cs:152-156`;
        `ProjectVariableMutationPreflight.cs:475`).
      - El plan transporta como dato la entrada de fuente que retira: la misma que inspeccionó el preflight.
      - No tiene fase: el plan de reparación no tiene `RegistryMutation`, así que se evalúa sobre un único snapshot.
    - El `PlanReadSet` **no** es una lista plana de valores de «después» y **no** mete de forma automática todo `I` en
      las dos fases.
  - **Regla de derivación.** Una observación entra en el `PlanReadSet` si y solo si el preflight usó ese resultado o
    esa clasificación para decidir el plan o para producir algo que el plan escribe. **[V6 · RR-1]** En particular,
    **todo dato o clasificación semántica que el preflight usó para justificar una decisión destructiva del plan queda
    representado por una observación comparable hasta el commit**, con la forma de lo que se usó:
    - el resultado de un símbolo en una fase → `SymbolResultObservation`;
    - la clasificación reparable de una fuente que se retira → `RepairDecisionObservation`. Los resultados de símbolo
      que solo sirvieron para esa clasificación no se observan aparte: la razón los resume (razón de reparación
      comparable, abajo).

    | Operación | `RegistryMutation` | `SymbolResultObservation` `Before` | `SymbolResultObservation` `After` | `RepairDecisionObservation` | Nunca |
    |---|---|---|---|---|---|
    | `Create(X, d)` | `Add` | — | X, cuyo resultado examina el contrato raíz (caso A) | — | — |
    | `ChangeDefinition(X, d)` | Cambio de definición | Cada símbolo de `I` distinto de X: R2 y el contrato raíz (casos B–D) comparan su estado previo | Cada símbolo de `I` (R2 y contrato raíz) y cada símbolo que leen los racks planificados en su resolución final, en **todas** sus propiedades: referencias directas y dependencias de sus expresiones (R1 y efectivo) | — | — |
    | `Rename(X, n)` | `Rename` | — | — | — | Ningún valor: `Rename` no evalúa ni cambia evaluaciones (P19.6–P19.7) |
    | `Delete(X)` | `Remove` | — | — | — | `After(X)` y cualquier valor: el preflight solo acredita, comprueba el id y bloquea por consumidores o dependientes (P20.2, P20.8) |
    | `UnlinkAllAndDelete(X)` | `Remove` | X, cuyo valor evaluado se materializa en las referencias directas (§7.1); sin consumidores no hay materialización ni observación | Cada símbolo que leen los racks finales, ya sin X | — | `After(X)` |
    | `RepairBrokenRack(rack)` **[V6 · RR-1]** | Ninguna | — | Cada símbolo que leen las fuentes que el rack **conserva** y cuyo efectivo importa | Una por cada fuente que se **elimina**, con su razón reparable | `SymbolResultObservation` por lo que leen solo las fuentes eliminadas; una observación por cada valor intermedio de una fuente eliminada; un símbolo inventado para un id ausente |
    | `Link` y `Unlink` heredados (`SelectiveBindingIntentPreflight.Run`, sin disparador productivo; P29.6) | Ninguna | `Unlink`: la variable cuyo efectivo se materializa | Los demás símbolos que lee el rack final; en `Link`, también la variable enlazada | — | — |

    - **Decisiones destructivas** **[V6 · RR-1]**. La única retirada de datos authored que se decide con una
      clasificación semántica es la de las fuentes de `RepairBrokenRack`. En las demás operaciones, lo que justifica el
      plan ya está cubierto:
      - `Delete` se decide con precondiciones estructurales: consumidores y dependientes (P20.2);
      - `UnlinkAllAndDelete` y `Unlink` heredado, con el valor que materializan (`SymbolResultObservation`);
      - `ChangeDefinition` y `Create`, con R2 y el contrato raíz (`SymbolResultObservation`);
      - `Rename` no destruye ningún dato semántico.
    - **Sin dependencias transitivas por separado.** El resultado de un símbolo observado ya incorpora el de sus
      dependencias, porque `RegistryEvaluation` lo calcula a partir de ellas. Un cambio de una dependencia que altera
      lo que el plan usó cambia ese resultado y aborta; uno que no lo altera no toca nada de lo que el plan decidió o
      escribe. V4 añadía las dependencias transitivas a la lista (ALT-41). **[V6 · RR-1]** Lo mismo vale para las
      decisiones de reparación: se compara la razón, no cada valor intermedio (T-V6-08).
    - **Con `UnlinkAllAndDelete`**, los demás símbolos del rack no necesitan observación `Before`. Ninguno depende de
      X, porque un dependiente bloquea la operación (§7.1), así que su resultado es el mismo en las dos fases.
    - **Un id ausente no es un símbolo.** Su ausencia forma parte del fallo tipado del símbolo que lo referencia o, en
      una fuente que se retira, de su razón `MissingTarget(ids)` **[V6 · RR-1]**.
  - **Invariantes del plan**, verificados al construirlo:
    - ninguna `SymbolResultObservation` `After` nombra un símbolo que la `RegistryMutation` elimina;
    - no hay dos `SymbolResultObservation` con el mismo `SymbolId` y la misma fase;
    - cada `ExpectedResult` sale de la `RegistryEvaluation` de su fase en el preflight, nunca de una evaluación
      propia, y cada `ExpectedRepairReason` sale de la inspección que hizo el preflight **[V6 · RR-1]**;
    - **[V6 · RR-1]** cada fuente que retira `RepairBrokenRack` tiene exactamente una `RepairDecisionObservation`;
      ninguna fuente conservada la tiene, y ningún símbolo recibe una `SymbolResultObservation` solo porque lo lee una
      fuente retirada;
    - el orden es determinista: primero las `RepairDecisionObservation`, por `RackId` y por token de propiedad
      (`Ordinal`, como `SelectiveLinkedPropertyKernel.cs:117-139`); después las `Before`, y al final las `After`, cada
      fase en el orden de P11.1.
  - **Qué no es una observación.** Las precondiciones propias de la mutación se validan con su contrato
    correspondiente y no se convierten en lecturas de valor: legibilidad estructural, acreditación de identidad
    (V8-R01), existencia de los ids que el plan usa (P7.7, P22.7) y forma enlazada. **[V6 · RR-1]** Los ids que una
    `RepairDecisionObservation` espera ausentes no son ids que el plan usa: su ausencia o su presencia la decide esa
    observación, no la precondición de ids. El authored de los racks tampoco es una observación («Qué no cubre»,
    abajo).
  - **Resultado comparable.** `Success` compara el valor exacto (`double.Equals`). `Failed` compara, en el orden
    determinista de P15.7, el **código** de cada diagnóstico del símbolo y sus **datos estructurados estables**.
    Nunca compara mensajes, texto localizado, posiciones (solo existen al escribir, P15.1) ni la traza opt-in
    (P16.3):

    | Código (P15.3) | Datos estables que se comparan |
    |---|---|
    | `BrokenReference` | Los ids ausentes, en el orden de P11.1 |
    | `Cycle` | Los miembros del ciclo, en el orden de P11.1 |
    | `DependencyFailed` | La cadena de causas hasta la raíz (P15.1): el id de cada eslabón, y el código y los datos de la causa raíz |
    | `InvalidArguments` | El token de la función y el número de argumentos recibidos |
    | `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` | Solo el código |

    - `OutOfRange` no aparece en una observación. Para una variable solo existe al escribir, y un plan que lo produce
      queda vacío (P8.10). Para una propiedad lo produce el consumidor, que no es un símbolo.
    - Si la implementación necesita más datos para distinguir dos causas, los añade como datos estables, nunca como
      texto.
    - Por el determinismo de P14.5, una re-lectura sin cambios reproduce cada resultado bit a bit: la comparación no
      produce abortos espurios.
  - **Razón de reparación comparable** **[V6 · RR-1]**. `ExpectedRepairReason` sigue la precedencia de P24.6 y compara
    solo datos estructurados estables. Nunca compara texto en español, texto del formatter (`CanonicalExpression`),
    posiciones, mensajes de UI ni la traza opt-in:

    | Razón | Cuándo (P24.6) | Datos estables que se comparan |
    |---|---|---|
    | `MissingTarget(missingIds)` | Algún id referido ausente, en referencia o en expresión | Los ids ausentes de la fuente, en el orden de P11.1 |
    | `Intrinsic(stableDiagnosticSignature)` | La fuente falla por sí misma: `InvalidArguments`, `DivisionByZero`, `NonFiniteResult` o `NonCanonicalForm` | El código y los datos estables de cada diagnóstico propio, con la tabla de `Failed` de arriba: token y número de argumentos para `InvalidArguments`, y solo el código para los demás |
    | `Upstream(rootCauseSignature)` | Una variable leída falla | Por cada variable leída que falla, en el orden de P11.1: su `SymbolId` y su causa raíz, es decir, el símbolo donde empieza el fallo, su código y sus datos estables. Los eslabones intermedios no entran: la decisión depende de qué variable leída falla y por qué, no del camino |
    | `Domain` | La fuente evalúa, pero su efectivo queda fuera del dominio de la propiedad | Solo la razón: el valor fuera de dominio no se compara |

    - En commit, la misma `InspectBinding` inspecciona la misma entrada de fuente contra la re-lectura acreditada y su
      `RegistryEvaluation`. Solo coincide si da la misma razón con los mismos datos estables; otra razón, `Healthy`
      o un estado estructural no coinciden.
    - `MissingTarget`: siguen ausentes exactamente esos ids → coincide; reaparece alguno, desaparece otro o cambia la
      clasificación → no coincide.
    - `Upstream`: la misma variable sigue fallando por la misma causa raíz → coincide; se recupera o cambia la causa
      raíz → no coincide.
    - `Intrinsic`: la firma sigue igual → coincide aunque cambien valores intermedios (`A / (B - C)` pasa de
      `B = C = 5` a `B = C = 6` y sigue en `DivisionByZero`); cambia la firma o la fuente evalúa → no coincide.
    - `Domain`: sigue fuera de dominio → coincide; vuelve al dominio → no coincide.
  - **Secuencia en commit**, dentro de Application (`RegistryCommit.Prepare`, Discovery §3.1), no en el Plugin:

    ```text
    sin RegistryMutation y sin observaciones:              no re-lee                   // sin cambio
    lastRead   = Read(...)
    si lastRead no es estructuralmente legible:            ABORT BEFORE WRITE
    accredited = Accredit(lastRead)                         // sin cambio (V8-R01); si falla: ABORT BEFORE WRITE
    si algún id que el plan usa ya no está:                 ABORT BEFORE WRITE          // precondición, no observación

    before     = RegistryEvaluation(accredited)             // solo si hay observaciones Before o de reparación
    para cada RepairDecision(rack, propiedad, esperada):    // V6 · RR-1: solo RepairBrokenRack, sin RegistryMutation
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

    - `Result(evaluación, s)` es el resultado de `s` en esa evaluación, o **ausente** si `s` no existe en ese
      snapshot. Un resultado ausente siempre difiere.
    - El aborto es tipado: informa la primera observación que difiere, en el orden de los invariantes, con su tipo, su
      fase o su fuente, y lo esperado y lo observado. El texto lo produce la capa de P15.4.
  - **Secuencia de `RepairBrokenRack`** **[V6 · RR-1]**, que no tiene `RegistryMutation`:
    1. re-leer y acreditar el registro, con la precondición de ids;
    2. recomputar cada `RepairDecisionObservation` de las fuentes que el plan va a **retirar**;
    3. si alguna decisión de reparación difiere: `ABORT BEFORE WRITE`;
    4. validar las `SymbolResultObservation` que exigen las fuentes que el plan va a **conservar**;
    5. solo entonces, escribir la `RackMutation`.

    Todo ocurre en la misma transacción, antes del primer `destination.Write`. Application es dueña de la inspección,
    la evaluación y la comparación; el Plugin solo lee y escribe físicamente; y no se re-ejecuta el preflight.
  - **También sin `RegistryMutation`.** Hoy el ejecutor solo re-lee el registro cuando el plan lo escribe
    (`src/RackCad.Plugin/ProjectVariableMutationExecutor.cs:252-275`), y `RegistryCommit.Prepare` devuelve
    `Unchanged` sin mirar la re-lectura (`src/RackCad.Application/ProjectVariables/RegistryCommit.cs:92-95`). Desde
    V4, todo plan con observaciones re-lee, re-acredita y compara sus observaciones en la misma transacción **antes**
    de escribir la primera vista, aunque no escriba el registro. Un plan sin ninguna observación no re-lee por el
    `PlanReadSet`. **[V6 · RR-1]** Toda reparación retira al menos una fuente, porque sin fuentes reparables no hay
    plan (`ProjectVariableMutationPreflight.cs:484-489`). Por eso todo plan de `RepairBrokenRack` lleva al menos una
    `RepairDecisionObservation` y re-lee, aunque no conserve ningún símbolo: «sin fuentes conservadas» no equivale a
    «sin observaciones».
  - **Una sola autoridad.** La lógica vive en Application y el ejecutor del Plugin solo la invoca; el Plugin sigue
    siendo el único que lee el NOD (ADR-0006). Las observaciones `Before` se evalúan con la `RegistryEvaluation` de la
    re-lectura acreditada y las `After` con la del documento cambiado, una evaluación por snapshot, y se comparan
    contra los resultados esperados que el plan transporta como datos. Las `RepairDecisionObservation` se recomputan
    con la misma `InspectBinding` sobre la misma entrada de fuente **[V6 · RR-1]**. No hay una segunda autoridad de
    evaluación ni de clasificación, y el commit **no** vuelve a ejecutar el preflight: no re-aplica R1, R2 ni el
    contrato raíz, solo comprueba que lo que el preflight usó sigue igual.
  - **Revisión ACOTADA de la política de concurrencia de I-48 (V8-R05)** **[V4 · CR-5]**. La regla nueva es
    exactamente:

    ```text
    un valor que participó realmente en el PlanReadSet
    cambia entre preflight y commit
    → ABORT BEFORE WRITE
    ```

    - **Precisión de V5** **[V5 · FR-1]**. «Un valor que participó realmente en el `PlanReadSet`» es el resultado de
      una observación en su fase, y «cambia» significa que el resultado observado difiere del esperado. Por eso la
      regla operativa es:

      ```text
      si el resultado observado difiere del resultado esperado → ABORT BEFORE WRITE
      ```

      y **nunca** «si algún símbolo del `PlanReadSet` falla → ABORT», que contradiría R2 (P21.3).
    - **Precisión de V6** **[V6 · RR-1]**. En una `RepairDecisionObservation`, lo que «participó» es la razón
      reparable de la fuente que se retira, y «cambia» significa que la inspección recomputada da otra razón u otros
      datos estables.
    - Alcanza a los planes con `RegistryMutation` y a los planes sin `RegistryMutation` que escriben racks.
    - **No** introduce hash de snapshot, token global, comparación completa del registro, rechazo por cambios fuera
      del `PlanReadSet` ni locks adicionales.
    - Un cambio fuera de las observaciones: **NO ABORT**. Un error no bloquea el commit por sí mismo: fuera del
      `PlanReadSet`, nunca; dentro, solo si su resultado difiere del esperado (§5.2, decisión 9).
    - **Un fallo no aborta por sí mismo**:

      | Esperado | Observado en commit | Resultado |
      |---|---|---|
      | `Failed(BrokenReference de Y)` | `Failed(BrokenReference de Y)` | **MATCH**: el commit sigue |
      | `Failed` | `Success` | `ABORT BEFORE WRITE` |
      | `Success` | `Failed` | `ABORT BEFORE WRITE` |
      | `Success(5)` | `Success(6)` | `ABORT BEFORE WRITE` |
      | `Failed(causa A)` | `Failed(causa B)` | `ABORT BEFORE WRITE` |
      | Cualquiera | Ausente | `ABORT BEFORE WRITE` |

      Un fallo preexistente idéntico y admitido por R2 no bloquea el commit. Un fallo que cambia de causa, o que se
      recupera de forma inesperada respecto de la observación sobre la que se construyó el plan, aborta: el plan se
      decidió sobre otra premisa y el commit no vuelve a ejecutar el preflight.
    - **Qué revisa.** V8-R05 (I-48 Proposal V8 §6) excluía comparar el registro de preflight con el de commit, la
      concurrencia optimista y el rechazo de planes obsoletos. I-49 introduce desde V4 una versión acotada de las tres
      cosas, limitada al `PlanReadSet`, y la aplica también a planes que ya existían: cambiar el valor de X en un rack
      que además lee Y por referencia directa aborta si Y cambió. Se conserva el contrato central de V8-R05: una
      re-lectura que cambió pero sigue siendo usable **no** se rechaza por el mero hecho de haber cambiado.
    - **Qué no cubre.**
      - El authored de los racks no entra en el `PlanReadSet`: el bucle de RACKVARIABLES es modal y re-lee en cada
        vuelta (Discovery §10.1). Una `RepairDecisionObservation` re-inspecciona la entrada de fuente que transporta el
        plan, no la del dibujo **[V6 · RR-1]**.
      - Las precondiciones de la mutación conservan su propio contrato; no son observaciones.
      - Un cambio de estructura del registro que no altera ningún resultado observado, como un dependiente nuevo
        creado entre preflight y commit, sigue fuera de alcance, como en V8-R05.
      - El reconciliador de RACKEDITAR no produce un `MutationPlan` (P21.12).
    - **Formalización.** Es una extensión consciente de I-48: el ADR de I-49 tiene que formalizarla y el Owner tiene
      que aceptarla **antes** de implementar (§2.3, §9, §13).
  - **Escenarios normativos** **[V5 · FR-1 · V6 · RR-1]**:

    ```text
    R2 con un fallo previo
      antes:   X = 5;  D = X + #<id-roto>           → D: Failed(BrokenReference #<id-roto>)
      intent:  ChangeDefinition(X, 6)               → R2 admite el plan: D ya fallaba y su definición no cambia
      plan:    After(X, Success(6))
               Before(D, Failed(BrokenReference #<id-roto>))
               After(D,  Failed(BrokenReference #<id-roto>))
      commit:  D idéntico en las dos fases           → COMMIT
               D recuperado o con otra causa         → ABORT BEFORE WRITE

    Delete
      antes:   X sin consumidores ni dependientes
      plan:    Remove(X); ninguna observación; no existe After(X)
      commit:  re-lectura, re-acreditación e id de X → COMMIT

    UnlinkAllAndDelete con solo referencias directas
      antes:   rack R con verticalClearance → X (X = 40) y palletTolerance → Y (Y = 2)
      plan:    Remove(X); R con verticalClearance = 40 materializado
               Before(X, Success(40))
               After(Y, Success(2))
      commit:  X e Y sin cambio                      → COMMIT
               X cambió                              → ABORT BEFORE WRITE, en la fase Before
               X desapareció                         → ABORT BEFORE WRITE, por la precondición de ids
               X sin cambio e Y cambió               → ABORT BEFORE WRITE, en la fase After

    RepairBrokenRack con una fuente retirada y otra conservada                    [V6 · RR-1]
      antes:   Holgura = #<id-roto> + 1           → Failed(BrokenReference #<id-roto>)
               R: palletTolerance = Holgura + 2    → RepairableSemanticFailure(Upstream, Holgura)
                  verticalClearance → Y (Y = 40)   → Healthy
      plan:    retirar palletTolerance (gobierna su literal congelado); conservar verticalClearance → Y
               RepairDecision(R, palletTolerance, Upstream(Holgura: BrokenReference #<id-roto>))
               After(Y, Success(40))
      commit:  decisión e Y sin cambio                 → COMMIT
               el id roto reaparece: Holgura = 4       → la inspección da Healthy (6) → ABORT BEFORE WRITE
               Holgura falla por otra causa raíz       → ABORT BEFORE WRITE
               decisión sin cambio e Y cambió          → ABORT BEFORE WRITE, al validar la fuente conservada

    Otras razones                                                                  [V6 · RR-1]
      MissingTarget: palletTolerance → #<id-roto>; sigue ausente → COMMIT; reaparece → ABORT BEFORE WRITE
      Domain:        palletTolerance = A - 10; A de 7 a 8 (efectivo -2) → COMMIT; A de 7 a 15 (efectivo 5) → ABORT
      Intrinsic:     palletTolerance = A / (B - C); B = C = 5 → B = C = 6, sigue DivisionByZero → COMMIT,
                     sin observar A, B ni C
    ```

  - **Casos fijados por prueba**:
    - **A-05**: en el rack R, `verticalClearance = X` (X está en `I`) y `palletTolerance = Y + 1` (Y no). Si Y cambia
      entre preflight y commit, el commit aborta antes de escribir porque `After(Y)` ∈ `PlanReadSet`;
    - **referencias directas heredadas**: R con `verticalClearance → X` y `palletTolerance → Y`, sin expresiones;
      cambiar X con Y alterado en la re-lectura aborta;
    - **cambio ajeno**: una variable Z que ninguna observación nombra cambia en la re-lectura; el commit sigue;
    - **sin `RegistryMutation`**: una reparación que deja `palletTolerance = Y + 1` aborta antes de la primera vista
      si Y cambió. Un plan sin ninguna observación no re-lee; toda reparación lleva al menos una
      `RepairDecisionObservation` y re-lee, aunque no conserve símbolos **[V6 · RR-1]**;
    - **fases y fallos esperados** **[V5 · FR-1]**: fallo previo idéntico, fallo con otra causa y fallo recuperado;
      `Delete`; `UnlinkAllAndDelete` con X sin cambio y con X cambiada; orden de las fases; cambio fuera de toda
      observación; `Rename` de una variable en error (T-V5-01 a T-V5-09);
    - **decisiones de reparación** **[V6 · RR-1]**: fallo superior idéntico, recuperado o con otra causa; id ausente
      que sigue ausente o reaparece; dominio que sigue fuera o vuelve; fallo propio con valores intermedios
      cambiados; fuente retirada junto a una conservada (T-V6-01 a T-V6-09).
- **P21.7 — Tres fronteras de validez en el preflight** **[V3 · A-03 modificada]** (P5.7):
  - **motor**: acepta cualquier `double` finito, también intermedios negativos o nulos;
  - **adaptador de Project Variables**: el contrato raíz del `VariableType` (P8.10), igual para literal y expresión;
  - **consumidor**: el dominio de cada propiedad sobre el efectivo de sus fuentes (P24.5), declarado por el
    descriptor **como dato**, nunca como callback.

  Si una frontera falla: `OutOfRange` con su dueño y plan vacío.
- **P21.8 — `Create` con expresión.** Sigue siendo registry-only, porque una variable nueva no tiene consumidores ni
  dependientes. Pero su preflight recibe y acredita el registro para validar la forma enlazada, evaluar y aplicar el
  contrato raíz, algo que hoy `Create` no hace (Discovery §8.1).
- **P21.9 — Materialización** **[V3 · A-12]**. Solo materializan un efectivo `UnlinkAllAndDelete`, que ya solo lo hace
  con referencias directas (§7.1), y la exportación a biblioteca (P24.9). Ambos escriben el **valor evaluado exacto**,
  sin redondear, así que el efecto geométrico sigue siendo nulo; se acepta el ruido de coma flotante del riesgo R16.
  La transición expresión→literal del reconciliador **no** materializa: escribe el literal tecleado
  (`LinkedPropertyReconciler.cs:204-210`), como hoy desde una referencia. V2 decía lo contrario en P21.9.
- **P21.10 — `RepairBrokenRack`** **[V3 · A-01]**. Una sola operación, rack-scoped y explícita, con el conjunto
  reparable ampliado a los fallos semánticos y confirmación consciente de la fuente (§7.2). Su commit protege la
  premisa de cada fuente que retira con una `RepairDecisionObservation` (P21.6) **[V6 · RR-1]**.
- **P21.11** — Sin límite de profundidad más allá de la aciclicidad y de P1.9, y **nunca** K planes sucesivos
  (Discovery §2.2, pregunta 16).
- **P21.12 — Edición desde RACKEDITAR.**
  - Cambiar la fuente de una propiedad no toca el registro ni propaga a otros racks, porque una propiedad no es un
    símbolo.
  - El reconciliador resuelve **ese** rack una vez, contra el snapshot del comando (P7.7), sobre estado aislado
    (P23.16), y escribe de forma atómica.
  - **Sin `PlanReadSet`** **[V4 · CR-5]**. El reconciliador no produce un `MutationPlan`: resuelve contra la lectura
    única del comando y escribe dentro del mismo comando modal (`RackSelectivoCommands.cs:66,144-151`). V6 no le
    añade re-lectura. Es el alcance declarado de la revisión de V8-R05, no una garantía de concurrencia.

### P22 — RACKVARIABLES UX

**Base.** Comando y bucle con re-lectura en cada vuelta (Discovery §10.1). Workspace y filas con `LiteralValue`
(Discovery §10.2). Lista sin id, campos «Nombre» y «Valor (in)» y panel de referencias rotas fuera del panel
editable (Discovery §10.3). Intents con `double` (Discovery §10.4). Todo lo que asume literal, `Length` y
consumidores directos (Discovery §10.5).

- **P22.1** — **Ni comando nuevo ni ventana nueva**: los censos de 33 comandos y 29 ventanas no cambian (guardas G15
  y G16, Discovery §14.3).
- **P22.2 — Fila de la lista**: nombre · tipo · definición (literal o texto del formatter) · valor evaluado
  (`"0.###"`) **o** código de diagnóstico · consumidores (directos y por expresión) · dependientes.
- **P22.3 — Un solo campo «Definición (in)»** en lugar de «Valor (in)»:
  - un texto que **no** empieza por `=` es un literal, parseado con la regla de ADR-0015 y `> 0`, exactamente como
    hoy;
  - un texto que empieza por `=` es una expresión (P1), con unidades si hacen falta: `=AlturaBase + Holgura`,
    `=100[mm] + Base`;
  - **[V3 · A-18]** si la expresión se canoniza a literal (`=6`, `=0`, `=-2`), rige la misma regla `> 0` que para el
    literal tecleado;
  - **[V3 · A-03 modificada]** si es una expresión, su raíz cumple el mismo `> 0` (P8.10): `=AlturaA - AlturaB` con
    raíz `-2` se rechaza igual que `-2`.
- **P22.4 — Diagnóstico en vivo.** La ventana llama a una función pura de Application, expuesta por el DTO del
  workspace, que parsea y enlaza contra el **snapshot del workspace** y devuelve diagnósticos con posiciones. La
  ventana los muestra y **no** emite el intent mientras haya errores. La ventana **nunca** evalúa con autoridad,
  **nunca** resuelve nombres por sí misma y **no** referencia tipos del núcleo (guarda, P28.4).
- **P22.5 — Selector de referencias** **[V3 · OPEN A]**. Una lista con nombre, valor evaluado y desambiguador, igual
  que los candidatos del editor (Discovery §13.2), inserta en el cursor la **forma del formatter** del `VariableId`
  elegido: `Nombre`, `{Nombre}` o, con homónimos, la forma cualificada. **Nunca** auto-selecciona por el nombre
  tecleado. La forma cualificada también se puede teclear o pegar.
- **P22.6 — Detalle de la variable seleccionada**: «Depende de» (dependencias directas) y «La usan» (variables
  dependientes directas, más los racks y propiedades que la consumen, directamente o por expresión).
- **P22.7 — Intents con forma enlazada** **[V3 · A-04]**.
  - `Create(nombre, definiciónEnlazada)` y `ChangeDefinition(id, definiciónEnlazada)`. La definición enlazada es un
    resultado opaco de Application: un literal o un `BoundExpression` con ids, producido por el binder puro contra
    el snapshot del workspace.
  - `Rename`, `Delete`, `UnlinkAllAndDelete` y `RepairBroken` conservan su payload.
  - El preflight valida esos ids contra `LastRegistry`, la misma lectura (P21.2), y el commit los vuelve a validar
    antes de comparar las observaciones del `PlanReadSet` (P21.6).
  - **Nunca** se vuelve a resolver texto humano contra una lectura posterior. Si una vuelta del bucle re-lee el
    dibujo, la ventana nueva se construye con el workspace nuevo, y el texto se enlaza solo contra el snapshot de la
    ventana donde se escribió.
- **P22.8** — Los fallos de preflight (`Cycle`, `BlockedByDependents`, `BlockedByExpressionConsumers`, `OutOfRange`,
  `BrokenReference`, etc.) usan el mecanismo vigente del bucle (Discovery §10.1).
- **P22.9 — Panel de diagnóstico y reparación** **[V3 · OPEN B · A-10]**. Queda fuera del panel editable, como hoy
  «Referencias rotas» (Discovery §10.3), y muestra:
  - las variables en error, con su causa raíz, los miembros y la ruta de cada ciclo y sus dependientes;
  - las fuentes de rack afectadas: rack, propiedad, tipo de fuente, fórmula canónica y causa;
  - la reparación por rack con la confirmación consciente de la fuente de §7.2;
  - para cada fuente con causa superior, el mensaje de recuperación **condicional** de §7.2: solo indica que corregir
    la variable permitiría recuperar la fórmula cuando es demostrable en el estado diagnosticado **[V4 · CR-4]**;
  - con el registro estructuralmente ilegible, el bloqueo de hoy, sin escribir nada.

  RACKVARIABLES abre siempre que el registro sea estructuralmente legible y acreditado (§5.2, decisión 5).
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
  | Seleccionar un candidato dentro de una expresión mayor | Inserta la forma del formatter de X, **sin** comprometer |
  | Enter con un candidato navegado | Lo selecciona, como hoy (Discovery §9.3) |
  | Enter con `DraftLiteral` | Compromete el literal, también desde una referencia o una expresión, como hoy |
  | Enter con `DraftExpression` | Pipeline P23.6: si pasa, compromete la forma canónica; si no, rechaza con diagnósticos y conserva el borrador |
  | Escape | Vuelve exactamente al estado comprometido |
  | Perder el foco | P23.8 |
  | Frontera C4 | P23.9 |

- **P23.6 — Pipeline de commit explícito** (REQUISITO DEL OWNER):
  1. **parse** (P1): diagnósticos de sintaxis, nombres y unidades;
  2. **bind** (P7) contra el snapshot de la sesión, que es el registro acreditado del comando y su evaluación,
     transportados por las opciones de apertura (Discovery §9.1). Un nombre exacto y único enlaza; uno parcial,
     desconocido, ambiguo o reservado se rechaza (§4);
  3. **validate** (P7.5): existencia, aridad, unidad, ámbito, forma canónica y compatibilidad de tipo de una
     referencia directa (P8.7);
  4. **evaluate** (P14) sobre los valores de `RegistryEvaluation` del mismo snapshot, con el dominio de la propiedad
     (P24.5). Es aviso temprano y presentación; la autoridad es el paso 7 (P14.9);
  5. **canonicalizar** (P2.8) y comprometer en la sesión, **sin** mutar el dominio. **[V3 · A-18]** Un literal
     canónico sigue la ruta histórica del literal tecleado;
  6. **intent**: la frontera C4 de la ventana entrega los estados finales (`LinkedPropertyFinalStates`, Discovery
     §9.4);
  7. **mutación atómica**: el reconciliador trabaja sobre estado aislado (P23.16). Re-comprueba y re-evalúa el árbol
     **ya enlazado por ids**, sin volver a resolver nombres, contra el snapshot del comando (P7.7). Resuelve el rack
     una vez, calcula la versión con la autoridad vigente (P23.15) y persiste authored, `PropertyValues` y efectivo
     juntos. Cualquier fallo deja **cero** persistencia y **cero** mutación en memoria visible para quien llama.
- **P23.7 — Candidatos e inserción** **[V3 · OPEN A]**.
  - El filtro sigue siendo *contains* `OrdinalIgnoreCase` sobre el fragmento bajo el cursor: filtra, **no** resuelve
    (Discovery §13.3).
  - La lista muestra nombre, valor evaluado y desambiguador obligatorio (Discovery §13.2). Los 8 caracteres son solo
    visuales y **nunca** se parsean (P3.9).
  - Solo se ofrecen variables compatibles cuya evaluación es `Success` (P23.11).
  - **Sin auto-selección**: nunca se completa un fragmento parcial ni se elige entre homónimos.
  - El token insertado es la forma del formatter: `Nombre`, `{Nombre}` o la forma cualificada.
- **P23.8 — LostFocus.** Compromete **solo** si el borrador no cambia la fuente, la regla general vigente
  (`LinkedPropertyEditSession.cs:279-300`). Un `DraftLiteral` sobre una fuente literal se compromete. Un
  `DraftExpression` que no enlaza, o cuya forma canónica difiere de la comprometida, se rechaza y el borrador
  sobrevive. Mover el foco a la lista no es perder el foco (Discovery §9.3).
- **P23.9 — Frontera C4.** Dos fases, como hoy: `TryStage` de **todos** los editores y después `ApplyStaged` de
  todos.
  - Un `DraftLiteral` sin cambio de fuente da `Ready`.
  - **Cualquier** borrador que cambie la fuente da `Blocked` con «confirma con Enter», porque una frontera genérica
    nunca convierte un cambio de fuente en commit (`LinkedPropertyEditSession.cs:349-352`). Cuenta como cambio de
    fuente pasar entre literal y expresión, entre referencia y expresión, a otra expresión, o de expresión o
    referencia a literal.
  - Un `DraftExpression` cuya forma canónica es la comprometida da `Clean`.
  - Un bloqueo aborta la frontera entera y enfoca al primer editor culpable (Discovery §9.4).
- **P23.10 — Presentación y efectivo** **[V3 · A-14]**.
  - Literal → `"0.###"`; referencia → `=` + referencia formateada; expresión → `=` + formatter (P4).
  - El efectivo que se muestra de una referencia es el valor de `RegistryEvaluation`, y el de una expresión, su
    evaluación sobre esos valores. Los dos son solo presentación.
  - El texto de estado bajo el campo muestra el valor y, si hay expresión, la fórmula.
- **P23.11 — Opciones.** `LinkedPropertyOptions.ForProperty` ofrece las variables de tipo compatible cuya evaluación
  es `Success`, con su **valor evaluado** y una marca para las definidas por expresión. Las variables en error
  **no** se ofrecen; su diagnóstico vive en RACKVARIABLES (§5.2, decisión 2).
- **P23.12 — Qué cambia respecto del contrato de I-48**, y que el ADR tiene que formalizar (§9):

  | Comportamiento de I-48 | V6 | Pines afectados (Discovery §14.2–§14.3) |
  |---|---|---|
  | Todo `=` es consulta de referencia que solo filtra (`IsQuery`) | `=` abre un borrador de expresión; la lista sigue filtrando fragmentos; teclear sigue sin comprometer nada | `CASO_6_UNA_CONSULTA_DE_REFERENCIA_QUEDA_PENDIENTE_Y_NO_COMPROMETE_NINGUNA_REFERENCIA` (`"="`, `"=H"`, `"=Holg"`, `"=Holgura General"`; `LinkedPropertyEditSessionTests.cs:212-230`): la clase esperada pasa a `DraftExpression`; «queda pendiente y no compromete» se conserva |
  | Enter sobre `=…` siempre se rechaza (`LinkedPropertyEditSession.cs:269-271`) | Enter compromete una expresión que pasa el pipeline; un fragmento parcial, desconocido, ambiguo o reservado sigue rechazándose | Pin nuevo: una expresión válida se compromete con Enter |
  | Ni con Enter se resuelve un nombre tecleado; `TrySelect` es la única vía de una referencia | **[V3 · OPEN A]** Enter resuelve un nombre **exacto y único**, que canoniza a la referencia existente | `CASO_8_ENTER_SIN_CANDIDATO_SELECCIONADO_NO_RESUELVE_POR_TEXTO` (`:249-259`) se **reescribe**: «Enter resuelve un nombre exacto y único; nunca uno parcial ni ambiguo». Con el mismo fixture, `=Holgura General` + Enter compromete su `VariableId` con el literal congelado; `=Holgura` (parcial) y un homónimo se rechazan |
  | `=Holgura General + 2` no se puede comprometer (Discovery §9.6, INFERENCE) | Se compromete si enlaza y evalúa | Pin nuevo |
  | **Se conserva**: sin auto-selección; desambiguador obligatorio; LostFocus sin cambio de fuente; C4 sin conversión silenciosa; Escape; regla 20.13 | Sin cambio | Intactos: `CASO_7_UN_FILTRO_CON_UN_UNICO_CANDIDATO_NO_LO_SELECCIONA` (`:232-247`), porque `=Estrecha` no es un nombre exacto; `CASO_9` a `CASO_11` (`:278-374`), `EL_FILTRO_ES_UN_SUBSTRING_INSENSIBLE_A_CAJA_Y_NO_UNA_RESOLUCION` (`:261-274`) y `:55-89`, `:134` |

- **P23.13 — `RackSelectiveWindow`.** Su `Describe` usa hoy `Text.TrimStart('=')` como nombre de la variable
  (Discovery §9.6). V6 lo sustituye por el texto de estado que produce la sesión: un cambio de un miembro.
  `pendingAll`, C4 y los estados finales no cambian. Es un archivo caliente: I-50 ya lo modificó y lo integró en
  `main` (`f8deb67`, donde `Describe` pasa a `:2582-2600`), e I-53 prevé tocarlo en su G5. Antes de editarlo se
  reconcilia la rama con `main` y se aplica el procedimiento de colisión (§12).
- **P23.14** — Sin control nuevo: el censo de 29 ventanas no cambia (G16).
- **P23.15 — Transiciones del reconciliador y versión escrita** **[V3 · A-09]** (amplía Discovery §9.4):

  | Transición | Qué escribe |
  |---|---|
  | literal → expresión | `WriteAuthored(literal comprometido)` (congela) + `PropertyValues[p] = expression` |
  | referencia → expresión, expresión → referencia, expresión → otra expresión | Literal congelado conservado + `PropertyValues[p]` = fuente nueva |
  | expresión → literal | `WriteAuthored(b)` y se quita la entrada, como hoy desde una referencia |
  | Cualquiera | El rack se resuelve **una** vez con todas sus propiedades finales |

  **Versión.** Toda rama que I-49 modifique para escribir estado authored del Selectivo calcula la versión con
  `SelectiveDesignSchema.ResolveWriteVersion(stored, hasPropertyValues)`
  (`src/RackCad.Application/Persistence/SelectiveDesignSchema.cs:43-66`) o con la autoridad vigente equivalente.
  **Nunca** fija `"2.0"`. Hoy el reconciliador lo fija sin condición (`LinkedPropertyReconciler.cs:218`, hallazgo
  L2), y un diseño `2.3` volvería a `2.0` al editarlo. Como I-49 reescribe esa rama, sustituirla es **preservación de
  la invariante C4-9**, no deuda lateral. El mismo criterio ya usa `Link` (`ProjectVariableMutationPreflight.cs:356`).
- **P23.16 — Estado aislado en las transiciones nuevas** **[V3 · A-15, elevado a MEDIUM práctico]**.
  - **Hecho.** `WithDesign` comparte la **misma** instancia de `PropertyValues` (`SelectivePalletDesignDocument.cs:321`
    en `a4d88f1`; hallazgo L3). El reconciliador la llama sobre el authored inicial
    (`LinkedPropertyReconciler.cs:146`) y `Apply` la muta (`:206-218`). Hoy un fallo no persiste nada, porque el
    comando aborta (`RackSelectivoCommands.cs:147-150`), pero el objeto que recibió el reconciliador puede quedar
    mutado en memoria.
  - **Regla.** Las transiciones nuevas trabajan sobre estado **aislado**: el reconciliador clona el authored inicial
    por la forma persistida (`ProjectVariableCloning.Clone`, `MutationPlan.cs:235-258`) **antes** de `WithDesign` y
    de `Apply`. Así lo hacen ya `UnlinkAllAndDelete` y `RepairBrokenRack`
    (`ProjectVariableMutationPreflight.cs:272,514`).
    Ningún alias de `PropertyValues` alcanzable por la sesión, la ventana o el comando puede quedar mutado tras un
    preflight o una reconciliación fallidos.
  - **Dos garantías distintas, las dos obligatorias y probadas por separado:**

    | Garantía | Significado | Cómo se prueba |
    |---|---|---|
    | `no persistence happened` | Ni registro ni vistas cambian en el dibujo | Contadores de escritura a cero tras un fallo |
    | `no in-memory mutation happened` | El authored recibido y su `PropertyValues` son idénticos antes y después de un fallo, y un éxito tampoco los altera | Forma serializada antes y después; mutar la salida no altera la entrada |

  - `WithDesign` y `SelectivePalletDesignDocument.cs` **no** cambian: el aislamiento se consigue en la frontera de
    I-49, y el portador, que I-50 ya modificó e integró en `main`, no se toca (§12). L3 se registra en G4 como
    hallazgo del portador.
  - El camino `WithDesign → From(design)`, del que depende I-50 para transportar sus campos
    (`docs/initiatives/I-50-proposal-v1.2.md:302-304`, ya en `main`), se conserva intacto sobre el clon.

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
  el error semántico `NonCanonicalForm` (A-07).
- **P24.4 — Formas persistidas en el rack** (P17.3): sin entrada (literal), `projectVariable` o `expression`.
- **P24.5 — Resolución y dominio del consumidor** **[V3 · A-03 modificada]**. Referencia directa → valor de X en
  `RegistryEvaluation`; expresión → su evaluación (P14.8). Resolver único (G11). El dominio de la propiedad (hoy
  `>= 0` en las dos propiedades Selectivas, Discovery §9.5) lo declara el descriptor **como dato** y se aplica al
  **valor efectivo** de las tres fuentes, preservando el comportamiento heredado donde aplica:

  | Fuente | Al escribir (editor, reconciliador) | Al resolver lo persistido (apertura, preflight, BOM) |
  |---|---|---|
  | `Literal` | **Ruta histórica**: la ventana rechaza `< 0` al construir el diseño (`RackSelectiveWindow.xaml.cs:2481-2505`) | **Heredado**: no se re-valida |
  | `ProjectVariableReference` | La misma comprobación de la ventana, que ya se aplica al efectivo de la sesión (`TryGetEffectiveValue`, `:2492-2502`), y el reconciliador | `OutOfRange`: `RepairableSemanticFailure` de dominio |
  | `Expression` | Pipeline de la sesión (aviso), la misma comprobación de la ventana sobre el efectivo y el reconciliador (autoridad) | `OutOfRange`: `RepairableSemanticFailure` de dominio |

  - **Endurecimiento declarado**: una referencia directa persistida a una variable con valor negativo, alcanzable solo
    por edición externa porque RACKVARIABLES exige `> 0`, deja de resolver y falla cerrada.
  - **Una sola declaración del dominio**: el descriptor. La ruta histórica de la ventana conserva su comprobación. Una
    prueba de paridad fija que, para cada propiedad, la ventana y el resolver aceptan exactamente el mismo conjunto de
    efectivos. Unificar la comprobación de la ventana en el descriptor tocaría `RackSelectiveWindow`, un archivo
    caliente (§12), así que queda como seguimiento en G4.
- **P24.6 — Resultados de `InspectBinding`** **[V3 · A-01 · OPEN B]**. Es la única autoridad sobre una fuente de rack
  (nombres ilustrativos). `FatalUnevaluable` de V2 **desaparece**: un fallo semántico de una fuente es reparable.

  | Situación | Resultado | Clase | ¿Reparable en el rack? |
  |---|---|---|---|
  | Todos los ids presentes, tipo compatible, evaluación `Success` y efectivo dentro del dominio | `Healthy` | — | — |
  | Algún id referido ausente, en referencia o en expresión | `RepairableMissingTarget` | Semántica | Sí |
  | La fuente falla por sí misma: `InvalidArguments`, `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` | `RepairableSemanticFailure(Intrinsic)` | Semántica | Sí |
  | Una variable leída falla: `Cycle`, `DependencyFailed`, `BrokenReference` o un error propio de esa variable | `RepairableSemanticFailure(Upstream, variable)` | Semántica | Sí |
  | El efectivo queda fuera del dominio de la propiedad | `RepairableSemanticFailure(Domain)` | Semántica | Sí |
  | Referencia directa con tipo incompatible | `FatalIncompatibleTarget` | — | No (inalcanzable con un solo `VariableType`) |
  | `PropertyId` desconocido | `FatalUnknownProperty` | Estructural | No |
  | Kind desconocido, id ilegible, payload mal formado, campo desconocido o dos autoridades | `FatalMalformedReference` | Estructural | No |

  - **Precedencia**: lo estructural, en el orden vigente, gana; después, id ausente; después, tipo incompatible; y
    por último, fallo semántico (propio, superior y dominio).
  - `RackRepairability` conserva `Healthy | Repairable | Blocked`: `Blocked` si alguna fuente es estructural o de tipo
    incompatible; `Repairable` si alguna es reparable y ninguna bloquea.
  - **Nunca** `Healthy` para un fallo de evaluación ni para un efectivo fuera de dominio.
  - **[V6 · RR-1]** La misma inspección recomputa en commit la razón de cada fuente que retira `RepairBrokenRack`
    (P21.6). No hay una segunda clasificación: la `RepairDecisionObservation` es su resultado transportado como dato.
- **P24.7 — BOM** **[V3 · OPEN B]**. RACKBOMTOTAL evalúa una vez por snapshot con la misma tubería. **Aborta** el total
  entero si algún rack incluido en ese BOM está afectado, como hoy con una referencia rota (ADR-0034 §10). Una
  variable en error sin consumidores cotizados no bloquea por sí sola. Una fuente no sana **nunca** produce una
  cantidad.
- **P24.8 — Duplicación y restamp.** La copia conserva `PropertyValues`, entradas `expression` incluidas, con los
  mismos `VariableId` (ADR-0034 §12; Discovery §5.8). I-51, ya integrada en `main`, cambió la entrada del restamp en
  el Plugin. V6 no toca ese camino y prueba en un archivo **propio**, contra el restamp integrado, que una entrada
  `expression` sobrevive a la duplicación (§12.2).
- **P24.9 — Exportación a biblioteca.** Materializa el efectivo evaluado exacto, quita `PropertyValues` y escribe
  literal-only en la línea `1.0`, como hoy (Discovery §5.8). Un rack afectado no tiene efectivo, así que su exportación
  falla cerrada.
- **P24.10 — Autoridad multi-vista.** La comparación estructural de árboles persistidos incluye `PropertyValues`
  (Discovery §7): dos vistas hermanas con expresiones distintas son `Divergent` y fallan cerradas, igual que hoy con
  referencias distintas.
- **P24.11 — Dibujos de I-47/I-48.** Con solo literales y referencias directas, dan los mismos efectivos, conservan
  sus formas persistidas y no se reescriben al abrir. La única excepción es el endurecimiento declarado en P24.5,
  alcanzable solo por edición externa. En una build anterior, un dibujo con expresiones falla cerrado (P18.2).
- **P24.12 — Rack afectado y rack no afectado** **[V3 · OPEN B]** (§5.2, decisiones 2 y 3):
  - **rack no afectado**: funciona con normalidad;
  - **rack afectado**: sin efectivo. RACKEDITAR no lo abre y remite a RACKVARIABLES con la causa, y su geometría
    existente no se toca;
  - un rack afectado solo entra en un plan que lo deje resuelto: su propia reparación, o la corrección de la causa
    superior que consume (R1).

### P25 — ID20 extension point

**OWNER INPUT.** I-49 **no** implementa `Rack.Frentes`, `Rack.Niveles`, `Rack.Altura`, `Project.TotalRacks` ni
`Project.TotalFrentes`; prepara `SymbolId`, scopes/namespaces, `ExpressionContext`, resolución de símbolos y los
datos que necesita el evaluador. **Validado por el Architect.**

- **P25.1 — Entregado por I-49**: `SymbolId`, `SymbolNamespace` (solo `projectVariable` activo), `SymbolScope` (solo
  `Project`), `SymbolTable`/`SymbolEntry`, `ExpressionContext`, `SymbolResolver`, evaluador sobre datos puros y el
  contexto de evaluación de una propiedad de rack (P6.7).
- **P25.2 — Reservado conceptualmente, sin implementación productiva** (REQUISITO DEL OWNER): `Rack.*` y `Project.*`,
  el ámbito `Rack` y un caso de definición para valores calculados. La reserva consta en el ADR y **ningún** camino
  productivo los crea.
- **P25.3 — Reserva en la gramática** **[V3 · OPEN A]**.
  - `Rack` y `Project` sin llaves son `ReservedName`.
  - `palabra.` es sintaxis de namespace: `=Rack.Frentes * 2` da `UnknownNamespace` en producción.
  - `{Rack}` es el nombre de una variable.

  Así ID20 podrá añadir `Rack.*` y `Project.*` sin cambiar el significado de ningún texto que hoy se pueda
  comprometer ni de nada persistido.
- **P25.4** — La regla de ámbito de P5.3 se fija con una prueba del núcleo sobre entradas **sintéticas de test**, sin
  crear ningún símbolo `Rack` productivo.
- **P25.5** — Las expresiones de propiedad son el consumidor natural de un futuro ámbito `Rack`: el contexto de P6.7
  es donde ID20 añadiría símbolos del rack. V6 no añade ninguno.
- **P25.6 — Frontera de datos para ID20.** Los valores calculados tendrán que venir de snapshots puros proyectados
  desde el barrido del Plugin, igual que `ProjectVariableScanEntry` (Discovery §16.2). El núcleo **nunca** lee
  AutoCAD, sistemas de Domain ni catálogos.
- **P25.7 — Lo que ID20 tendrá que resolver, y V6 no**: `Rack.Frentes` es un conteo **por fondo**, `Rack.Niveles`
  admite al menos dos lecturas, `Rack.Altura` requiere catálogo y redondea al pie, no existe ningún agregado
  `Project.*` y `RackCount` solo cuenta racks con BOM (Discovery §16.2). Las preguntas de frontera son del Owner y de
  ID20 (contrato §12), no de esta Proposal.

### P26 — ID23 extension point

**OWNER INPUT.** ID23 debe poder reutilizar parser, evaluador y contexto **sin depender de ProjectVariables**.
**Validado por el Architect con A-06.**

- **P26.1** **[V3 · A-06]** — El núcleo no depende de `ProjectVariables`, `Persistence`, `Systems.*`, `Bom`,
  `Catalogs`, `StructuralSections`, Domain, UI, Plugin ni AutoCAD. `SymbolEntry` ya no transporta `VariableType`
  (P5.1). Lo fija una guarda de fuente (P28.4), y la dirección de dependencia es de Project Variables hacia el
  núcleo, **nunca** al revés.
- **P26.2 — Superficie reutilizable**: parsear, enlazar con un contexto que aporta quien llama, comprobar, extraer
  dependencias, grafo, ciclos, orden, evaluar, formatear y diagnosticar.
- **P26.3** — La autoridad de unidades (P9.5) es igual de neutral y reutilizable.
- **P26.4 — Lo que añadiría ID23, y V6 no**: sus namespaces y ámbitos, un resultado entero y la semántica de redondeo
  para `Quantity` (hoy `int`, Discovery §16.3; P10.6), la persistencia de fórmulas de BOM y su propio ADR.
- **P26.5** — Sin ensamblado separado en V6 (§10, ALT-17): la guarda marca la frontera.
- **P26.6** — Ningún tipo de I-49 lleva conceptos de BOM, y no se persiste ninguna fórmula de BOM.

### P27 — ID28/29 information preservation

**OWNER INPUT**, precisado por el encargo de G2A.1. Hoy la procedencia se pierde en cuanto el resolver escribe el
número en Domain, y nada del impacto se persiste (Discovery §16.4). **Validado por el Architect.**

- **P27.1 — ID28 (Explain)**. Se conserva por snapshot y en memoria, para cada variable y cada fuente `expression`
  de rack:
  - `BoundExpression` y `SymbolId` de cada referencia;
  - diagnósticos (P15), con la cadena hasta la causa raíz;
  - traza **opt-in** (P16.3).
- **P27.2 — ID29 (Impact Preview)**. Se expone **antes** de ejecutar:
  - dependencias directas y transitivas (P11, P12.7);
  - dependientes inversos variable→variable (P12.2) y consumidores de rack, directos o por expresión (P21.2);
  - **conjunto afectado**: el cierre `I`, con el valor de cada variable antes y después, y los racks con sus
    propiedades;
  - **conjunto leído**: las observaciones del `PlanReadSet`, cada una con su fase y su resultado esperado, sea un
    valor o un fallo tipado (P21.6) **[V5 · FR-1]**, y las decisiones de reparación con su razón **[V6 · RR-1]**.

  Hoy el preflight solo expone los racks (Discovery §16.4). V6 lo usa en los mensajes de RACKVARIABLES.
- **P27.3** — La cadena de Explain se deriva **sin** re-evaluar: propiedad → fuente (referencia o expresión) →
  `SymbolId` → `EvaluationResult` con traza opt-in, en el mismo snapshot.
- **P27.4** — **Nada se persiste**: ni trazas, ni grafo, ni valores, ni conjuntos.
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
  | G5 | Lexer y parser por tabla (válidos, inválidos, posiciones); unidades (`[mm]`, `[in]`, `[ft]`, `UnknownUnit`, `UnitNotAllowedHere`, `UnitSyntaxNotSupported`); `AmbiguousDecimalComma`; gramática de referencias de §4; guardas del parser (texto y anidamiento sintáctico); orden de diagnósticos; formatter canónico y *round-trip* P2.5 sobre un corpus con homónimos y nombres con llaves |
  | G6 | Resolver y binder con todos los resultados de P7.1; límites normativos tras el bind, con la profundidad del `BoundExpression` (P1.9); **motor adimensional**: `6 + 2`, `A + 2`, `A * B`, `A / B`, `100[mm] + 2`, `MAX(A, 4[in])` y un intermedio negativo, con valores exactos; conversiones bit a bit (`4[in]` = 4, `20[ft]` = 240, `100[mm]` = 100 ÷ 25.4); **autoridad de unidades**: los alias de `StructuralSectionUnits` valen lo mismo que la autoridad y las constantes de dominio de P9.6 no cambian; `MIN`, `MAX` y `ABS` (aridad y valores); semántica numérica; regla de ámbito sintética; `Rack.*` y `Project.*` no resuelven; tabla de canonicalización P2.8; tablas de tokens |
  | G7 | Extracción sin repetición y ordenada; grafo con diamante, autorreferencia, ciclo de 2, componente mayor y aristas rotas; orden independiente de permutaciones; cortocircuito `DependencyFailed`; conjuntos de P12.7 |
  | G8 | Paso 1, solo pruebas: caracterización de V-0 sobre la producción intacta. Paso 2: round-trip del store real de definiciones y fuentes `expression`; cada regla estructural de P17.6; `NonCanonicalForm` semántico; **fixture dorado**: el JSON literal-only que escribe la build nueva es idéntico al de `a4d88f1`; resultados de `InspectBinding` para los dos kinds; resolver con expresiones y dominio; `RegistryEvaluation` único; RACKBOMTOTAL con un solo snapshot; restamp que conserva una entrada `expression`; exportación que la materializa; prueba del peor caso de profundidad por descubrimiento de los puntos de serialización (P1.9) |
  | G9 | Las cuatro transiciones; cierre impactado; consumidores por expresión; una `RegistryMutation`; racks deduplicados y resueltos una vez; R1–R3; contrato raíz con los casos A–D; `PlanReadSet` por observaciones `Before`/`After` y aborto en commit antes de escribir solo si un resultado observado difiere del esperado, con y sin `RegistryMutation`; `Delete` y `UnlinkAllAndDelete` con consumidores por expresión y con dependientes, y su commit sin `After` del símbolo eliminado; `Rename` de una variable en error; `RepairDecisionObservation` de cada fuente que retira `RepairBrokenRack`, recomputada antes de validar las conservadas; ciclo rechazado; `Create` y `Rename` con expresiones; reparación generalizada con mensaje de recuperación condicional; oráculo: descubrimiento por conjunto = unión por variable |
  | G10 | Sesión del editor: todos los gestos de P23.5 con éxito y con fallo del pipeline; las garantías de P23.2 con borradores `=`; `=X` persiste lo mismo que seleccionar X; `=6` lo mismo que `6`; inserción de candidatos; sin auto-selección; reconciliador (P23.15–P23.16); workspace y ventana de RACKVARIABLES con intents enlazados; panel de diagnóstico; pruebas del control sin interfaz |
  | G11 | Pruebas reales de cadena X → Y → expresión de propiedad → geometría y firma de BOM sobre catálogo real; un rack con una propiedad por referencia y otra por expresión; suites de I-47/I-48 verdes, sin más cambios de aserción que los de P28.4 |

- **P28.4 — Evolución explícita de guardas y pines** (Discovery §14.3). Se hace en el **mismo** gate que el
  comportamiento, con RED→GREEN, y **nunca** renombrando tipos para esquivarlas:

  | Guarda o pin | Evolución propuesta | Gate |
  |---|---|---|
  | G1 `NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` | `RackPropertyReference`, `rackProperty` y `FormulaParser` siguen prohibidos en Application, Plugin y UI; `ExpressionParser` y `DependencyGraph` pasan a permitirse **solo** en Application (núcleo y adaptadores) y siguen prohibidos en Plugin y UI | G5, G7 |
  | G2 `LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO` | Se sustituye: exactamente `Literal = 1` y `Expression = 2`; `Formula = ` sigue prohibido | G8 |
  | G3 `"expression"` = error duro | Se sustituye **después** del paso 1 de G8: `"expression"` válido solo con payload estructuralmente válido; kinds desconocidos (`"formula"`, `"futureKind"`) siguen siendo error duro | G8 |
  | **Nueva** — kinds de binding | Exactamente `projectVariable` y `expression`; los fixtures `rackProperty` y `futureKind` siguen fallando cerrados (Discovery §15, búsqueda 10) | G8 |
  | G4 Domain sin variables | Sin cambio; se añade: Domain sin tokens del núcleo | G5 |
  | G5 capa pura sin AutoCAD | Se extiende al núcleo y a la autoridad de unidades | G5 |
  | G6–G10 gramática de `VariableType` | Sin cambio: la tabla explícita de P17.10 conserva el lenguaje aceptado | — |
  | G11 y G12 resolver único y BOM | Sin cambio; se añade: RACKBOMTOTAL no parsea ni evalúa | G8 |
  | G13 ejecutor | Se añaden `ExpressionParser`, `ExpressionEvaluator` y `DependencyGraph` como ausentes | G9 |
  | G14 comando RACKVARIABLES | Sin cambio; se añade: el comando no evalúa ni enlaza | G9 |
  | G15 y G16 censos | Sin cambio: 33 comandos y 29 ventanas | — |
  | G17 catálogo cerrado | Sin cambio: dos propiedades | — |
  | G18 Plugin y UI no interpretan vínculos | Se extiende: sin tipos del núcleo en Plugin ni UI; `RackSelectiveWindow.xaml.cs` sigue sin `VariableId`, resolver ni parser | G10 |
  | G19 el control no compara nombres | Sin cambio | — |
  | G20 pines de `=` | Evolucionan **exactamente** según P23.12: `CASO_6` cambia de clase, `CASO_8` se reescribe y `CASO_7` queda intacto | G10 |
  | **Nueva** — independencia del núcleo | El núcleo y la autoridad de unidades no nombran `ProjectVariables`, `Persistence`, `Systems`, `Bom`, `Catalogs`, `StructuralSections`, `RackCad.Domain`, `RackCad.UI`, `RackCad.Plugin` ni `Autodesk` | G5 |
  | **Nueva** — sin comprobador dimensional | El criterio son las pruebas de P5.6; defensa secundaria: el núcleo no declara tipos ni códigos dimensionales | G6 |
  | **Nueva** — autoridad de unidades **[V4 · CR-1]** | Prohíbe **solo**: parsers, lexers o tablas de tokens `[mm]`, `[in]` o `[ft]` fuera de la autoridad del motor; un literal de conversión `25.4` duplicado fuera de `LengthUnits`; una segunda autoridad genérica pies↔pulgadas (constante o conversor con ese papel) fuera de `LengthUnits`, salvo los alias de `StructuralSectionUnits`. No busca el número `12` y no afecta a las constantes de dominio de P9.6 | G6 |
  | **Nueva** — tokens explícitos | Ningún token persistido de I-49 se produce con `ToString()`, `nameof` ni el conversor de enums | G8 |

- **P28.5 — Escenarios obligatorios añadidos en V3** **[V3]**. Cada uno es una prueba de comportamiento con su gate:

  | # | Escenario | Qué fija | Gate |
  |---|---|---|---|
  | T-V3-01 | Gramática de OPEN A por tabla | `Nombre`, `Nombre Compuesto`, `{Nombre complejo}`, `{a}}b}`, `Nombre#<GUID>`, `{Nombre}#<GUID>`, `UnterminatedName`, `OperatorInName`, `UnitNotAllowedHere` tras referencia | G5, G6 |
  | T-V3-02 | Cualificación con GUID completo | Solo la forma D de 36 caracteres; `#3f2b1c9e` es `InvalidQualifier`; hexadecimales sin distinguir mayúsculas; `QualifiedNameMismatch` | G5, G6 |
  | T-V3-03 | `#GUID` de referencia rota | El formatter emite `#<id>` para un id ausente; ese texto da `BrokenReference` y no se compromete; `#<id>` sin nombre con id presente da `NameRequired` | G6, G10 |
  | T-V3-04 | Escape de nombres de función y reservados | `MIN`, `max`, `Rack` y `Project` sin llaves dan `ReservedName`; `{MIN}` y `{Rack}` enlazan a variables; `Rack.Frentes` da `UnknownNamespace` | G5, G6 |
  | T-V3-05 | Clasificación estructural frente a semántica | Cada fila de §5.1, en definición y en fuente | G8 |
  | T-V3-06 | Un registro con errores semánticos sigue inspeccionable | `Usable`; RACKVARIABLES abre y diagnostica; los racks no afectados abren y resuelven; las opciones no ofrecen variables en error | G8, G10 |
  | T-V3-07 | Reparación de una propiedad con fallo semántico | Causa propia, superior y de dominio: la reparación quita la fuente, gobierna el literal congelado y el rack resuelve | G9 |
  | T-V3-08 | Propiedad estructural no reparable de forma destructiva | `FatalMalformedReference`: rack `Blocked`, la reparación falla y no se escribe nada, aunque el rack tenga además fuentes reparables | G9 |
  | T-V3-09 | Reglas R1, R2 y R3 | Rack que no resuelve después → plan vacío; símbolo sano que pasaría a fallar → rechazo; error previo con definición intacta → no bloquea; ningún intent escribe un error nuevo; salida de un ciclo con dos errores | G9 |
  | T-V3-10 | Carrera del `PlanReadSet` (A-05) | `verticalClearance = X`, `palletTolerance = Y + 1`; Y cambia entre preflight y commit → aborto antes de escribir porque `After(Y)` ∈ `PlanReadSet`; los casos heredados, ajenos y sin `RegistryMutation` están en T-V4-07 a T-V4-09, y las fases y los fallos esperados, en T-V5-01 a T-V5-09 | G9 |
  | T-V3-11 | `UnlinkAllAndDelete` bloqueado por una expresión de propiedad | `palletTolerance = A + B`, `UnlinkAllAndDelete(A)` → bloqueado con rack, propiedad y fórmula; nada materializado | G9 |
  | T-V3-12 | `UnlinkAllAndDelete` bloqueado por un dependiente | `C = A + 1` → bloqueado con la lista de dependientes; con solo referencias directas materializa como hoy | G9 |
  | T-V3-13 | Confirmación de reparación consciente de la fuente | El texto incluye rack, `PropertyId`, tipo de fuente, expresión canónica, causa, literal congelado y causa superior; el mensaje de recuperación sigue la regla condicional de §7.2 (T-V4-05 y T-V4-06) | G9, G10 |
  | T-V3-14 | Diagnóstico semántico de forma no canónica | `{"Kind":"expression","Expression":{"Node":"number","Value":6}}` → `NonCanonicalForm` por símbolo o fuente, registro `Usable`; ningún escritor produce una forma no canónica | G8 |
  | T-V3-15 | Campo desconocido en un payload de Expression | Campo desconocido o repetido en la definición `expression`, en la entrada `expression` o en un nodo profundo → estructural; el mismo campo en raíz, entrada, `literal` o `projectVariable` se conserva como hoy | G8 |
  | T-V3-16 | Mismo contrato raíz para literal y expresión | `-2` y `=0 - 2` rechazados igual (caso A); `=Base + (A - B)` con intermedio negativo aceptado; un cambio de A que dejaría una dependiente sana en `≤ 0` se rechaza (caso B); un símbolo que pasa de `Failed` a `Success` con raíz `≤ 0` se rechaza (caso C, escenario de P8.10); una raíz ya inválida por corrupción externa con definición intacta no bloquea por sí sola (caso D); al leer, una raíz `≤ 0` persistida se trata igual en las dos sintaxis | G9, G10 |
  | T-V3-17 | Mismo dominio de propiedad para cualquier fuente | Paridad: literal (ruta de la ventana), referencia y expresión aceptan el mismo conjunto de efectivos; endurecimiento declarado de una referencia negativa persistida | G8, G10 |
  | T-V3-18 | `RegistryEvaluation` como única autoridad de evaluación | Cada símbolo se evalúa una vez por snapshot; opciones, workspace, targets, preflight y BOM exponen exactamente sus valores; la sesión no persiste un valor propio | G8, G10 |
  | T-V3-19 | Las transiciones nuevas no comparten `PropertyValues` | Tras un reconcile fallido y tras uno correcto, el authored recibido es idéntico en forma serializada; mutar la salida no altera la entrada; `no persistence happened` y `no in-memory mutation happened` se prueban por separado | G10 |
  | T-V3-20 | Tablas explícitas de tokens persistidos | Cada token de P17.10 en los dos sentidos; `Type` escrito como `"Length"` byte a byte; un token con otra grafía es estructural | G8 |
  | T-V3-21 | Caracterización de la build anterior bajo V-0 | Commit solo de pruebas, con CI, sobre la producción intacta: registro con `expression` → `PresentButUnreadable`; diseño con fuente `expression` → `FatalMalformedReference(UnknownKind)`, `UnknownReferenceKind`, `Indeterminate` y descubrimiento abortado | G8, paso 1 |

  Tras el paso 1 de G8, las aserciones de T-V3-21 sobre `expression` evolucionan con los stores. El rechazo de la
  build anterior queda probado por la CI de ese commit y, en G12, con el DLL de `a4d88f1` (P29.5). Los kinds
  desconocidos conservan sus pruebas de rechazo.
- **P28.6 — Escenarios obligatorios añadidos en V4** **[V4 · CR-1…CR-5]**. Cada uno es una prueba de comportamiento
  con su gate:

  | # | Escenario | Qué fija | CR | Gate |
  |---|---|---|---|---|
  | T-V4-01 | `Failed` → `Success` con raíz inválida ⇒ bloqueado | `A` con referencia rota, `B = A - 100`, `AlturaFinal = B + 200`; `ChangeDefinition(A, "=Base")` con `Base = 10` → `OutOfRange(B)`, plan vacío, registro y vistas sin escribir | CR-2 | G9 |
  | T-V4-02 | Profundidad exacta 24 | Un `BoundExpression` de profundidad 24 se compromete al escribir, se persiste y se vuelve a leer como válido | CR-3 | G6, G8 |
  | T-V4-03 | Profundidad 25 | Al escribir, `LimitExceeded` tras el bind; al leer un payload persistido de profundidad 25, estructural (`MalformedExpression`) | CR-3 | G6, G8 |
  | T-V4-04 | Cadena binaria plana, asociada por la izquierda | `1 + 1 + … + 1` sin paréntesis: con 24 operandos pasa y con 25 da `LimitExceeded`, aunque las guardas del parser no salten; los ejemplos normativos de P1.9 dan exactamente su profundidad | CR-3 | G5, G6 |
  | T-V4-05 | Mensaje de recuperación veraz con una sola causa superior | Rack con una única fuente inválida, `palletTolerance = Holgura + 2` con `Holgura` rota, y ningún otro rack del cierre con fallos propios → «Corregir Holgura permitiría recuperar esta fuente sin eliminarla.»; después, `ChangeDefinition(Holgura)` pasa R1 y la fuente se recupera sin reparar | CR-4 | G9, G10 |
  | T-V4-06 | El mensaje NO indica recuperación con otra falla propia | El mismo rack con además `verticalClearance = ABS(A, B)` → mensaje de bloqueo con la fórmula canónica `Holgura + 2`; `ChangeDefinition(Holgura)` queda bloqueado por R1 | CR-4 | G9, G10 |
  | T-V4-07 | Carrera del `PlanReadSet` con referencias directas heredadas | Rack con `verticalClearance → X` y `palletTolerance → Y`, sin expresiones; cambiar X con Y alterado en la re-lectura → `ABORT BEFORE WRITE` | CR-5 | G9 |
  | T-V4-08 | Un cambio fuera del `PlanReadSet` no aborta | Z, que ningún rack del plan lee, cambia en la re-lectura → el commit escribe registro y vistas | CR-5 | G9 |
  | T-V4-09 | `PlanReadSet` sin `RegistryMutation` | `RepairBrokenRack` que deja `palletTolerance = Y + 1`: si Y cambia, aborta en la misma transacción antes de la primera vista. Una reparación sin **ninguna** observación no requiere re-lectura por el `PlanReadSet`, pero toda reparación que retira una fuente lleva al menos una `RepairDecisionObservation` y re-lee aunque no conserve símbolos (descripción ajustada en V6, RR-1) | CR-5 | G9 |
  | T-V4-10 | La autoridad de unidades no toca constantes de dominio | `FootInches`, `CommercialFoot` y el `12.0` de `CantileverArmFrameResolver` quedan sin cambios; los alias de `StructuralSectionUnits` valen lo mismo que `LengthUnits`; la guarda detecta un parser de unidad fuera del motor, un `25.4` duplicado y una segunda autoridad pies↔pulgadas, y no marca las constantes de dominio | CR-1 | G6 |
  | T-V4-11 | Dos causas raíz independientes en el mismo rack | Rack con `palletTolerance = A + 1` y `verticalClearance = B + 1`, con `A` y `B` rotas por separado → ninguna fuente recibe el mensaje de recuperación; `ChangeDefinition(A)` y `ChangeDefinition(B)` quedan bloqueados por R1; la reparación muestra las dos fórmulas antes de eliminarlas | CR-4 | G9, G10 |

- **P28.7 — Escenarios obligatorios añadidos en V5** **[V5 · FR-1]**. Cada uno es una prueba de comportamiento con su
  gate. Los escenarios de CR-5 (T-V3-10 y T-V4-07 a T-V4-09) se mantienen:

  | # | Escenario | Qué fija | Gate |
  |---|---|---|---|
  | T-V5-01 | Fallo previo idéntico en el `PlanReadSet` | `X = 5`, `D = X + #<id-roto>`; `ChangeDefinition(X, 6)` pasa R2 y el plan lleva `Before(D)` y `After(D)` con `Failed(BrokenReference)` del mismo id. Con la re-lectura idéntica, `Failed` → mismo `Failed` → **COMMIT**: registro escrito, sin aborto por el mero hecho de que D falle. La igualdad se decide por código e ids, nunca por texto | G9 |
  | T-V5-02 | El fallo cambia de causa | El mismo plan. Entre preflight y commit, D pasa a fallar por otra causa (`BrokenReference` de otro id, o `DependencyFailed` con otra causa raíz): `Failed(causa A)` → `Failed(causa B)` → `ABORT BEFORE WRITE`, sin escribir registro ni vistas | G9 |
  | T-V5-03 | El fallo se recupera de forma inesperada | El mismo plan. Entre preflight y commit, el id roto vuelve a existir y D evalúa: `Failed` → `Success` → `ABORT BEFORE WRITE`, porque el plan se apoyó en la observación fallida | G9 |
  | T-V5-04 | `Delete(X)` | X sin consumidores ni dependientes, también si X está en error (P20.7). El plan no lleva observaciones y ninguna `After(X)`. El commit re-lee, re-acredita, valida el id y escribe, también si cambió otra variable. Si X ya no existe en la re-lectura, aborta por la precondición de ids, no por una observación | G9 |
  | T-V5-05 | `UnlinkAllAndDelete(X)` con X sin cambio | Rack con `verticalClearance → X` y `palletTolerance → Y`. El plan lleva `Before(X, Success(valor materializado))` y `After(Y)`, y ninguna `After(X)`. Con la re-lectura idéntica → **COMMIT**: X eliminada, literal materializado y vistas escritas | G9 |
  | T-V5-06 | `UnlinkAllAndDelete(X)` con X cambiada | El mismo rack. X cambia de valor entre preflight y commit → `ABORT BEFORE WRITE` en la fase `Before`, antes de cualquier escritura de registro o vistas. Si X desaparece, aborta igual, por la precondición de ids | G9 |
  | T-V5-07 | Orden de las fases | El plan de T-V5-05, con `Before(X)` y `After(Y)`: las `Before` se evalúan sobre la re-lectura, antes de `ApplyTo`, y las `After` sobre el documento cambiado. Con X e Y cambiados, el aborto informa `Before(X)`; con solo Y cambiado, `After(Y)`; con los dos sin cambio, commit, lo que prueba que `Before(X)` no se evaluó sobre un documento sin X | G9 |
  | T-V5-08 | Un cambio fuera de toda observación no aborta | En los planes de T-V5-01 y T-V5-05 cambia una variable W que ninguna observación nombra → **COMMIT** | G9 |
  | T-V5-09 | `Rename` de una variable en error | Se renombra un miembro de un ciclo persistido: el plan no lleva observaciones y el commit escribe, conforme a P19.7 | G9 |

- **P28.8 — Escenarios obligatorios añadidos en V6** **[V6 · RR-1]**. Cada uno es una prueba de comportamiento con su
  gate. Siguen vigentes los de CR-5 (T-V3-10 y T-V4-07 a T-V4-09, este último con la descripción ajustada) y los de
  FR-1 (T-V5-01 a T-V5-09):

  | # | Escenario | Qué fija | Gate |
  |---|---|---|---|
  | T-V6-01 | El fallo superior sigue idéntico | `palletTolerance = Holgura + 2` con `Holgura = #<id-roto> + 1`, y `RepairBrokenRack` confirmado. El plan lleva `RepairDecision(R, palletTolerance, Upstream(Holgura: BrokenReference #<id-roto>))`. Con la re-lectura idéntica, la decisión coincide → **COMMIT**: fuente retirada y literal congelado gobernando | G9 |
  | T-V6-02 | El fallo superior se recupera | El mismo plan. El id roto reaparece antes del commit, `Holgura` evalúa y la inspección da `Healthy` → `ABORT BEFORE WRITE`: la fórmula no se borra, y ni el registro ni las vistas se escriben | G9 |
  | T-V6-03 | El fallo superior cambia de causa | El mismo plan. `Holgura` pasa a fallar por otra causa raíz (otro id ausente, o `DivisionByZero`): `Upstream(causa X)` → `Upstream(causa Y)` → `ABORT BEFORE WRITE` | G9 |
  | T-V6-04 | El id ausente sigue ausente | `palletTolerance → #<id-roto>`, y la variante en expresión `#<id-roto> + 1`. `MissingTarget({id-roto})` coincide en commit → **COMMIT**. Para ese id no se crea ninguna `SymbolResultObservation` ni un símbolo inventado | G9 |
  | T-V6-05 | El id ausente reaparece | El mismo plan: reaparece `#<id-roto>` → `ABORT BEFORE WRITE`. Variante: en una fuente `#a + B`, B también desaparece, y `MissingTarget({a})` pasa a `MissingTarget({a, b})` → `ABORT BEFORE WRITE` | G9 |
  | T-V6-06 | El fallo de dominio sigue siendo de dominio | `palletTolerance = A - 10` con A de 7 a 8 (efectivo de -3 a -2): `Domain` coincide → **COMMIT** | G9 |
  | T-V6-07 | El fallo de dominio vuelve a ser sano | El mismo plan con A de 7 a 15 (efectivo 5): la inspección da `Healthy` → `ABORT BEFORE WRITE`; la fórmula no se borra | G9 |
  | T-V6-08 | Fallo propio con valores intermedios cambiados | `palletTolerance = A / (B - C)` con `B = C = 5` da `DivisionByZero`; en commit, `B = C = 6` sigue dando `Intrinsic(DivisionByZero)` → **COMMIT**. El plan no lleva `SymbolResultObservation` de A, B ni C: no se reintroduce la observación transitiva excesiva | G9 |
  | T-V6-09 | Fuente retirada junto a una conservada | Rack con `palletTolerance = Holgura + 2` (retirada) y `verticalClearance → Y` (conservada). El plan lleva la `RepairDecisionObservation` de la retirada y `After(Y)` de la conservada, y las dos se validan antes de escribir. Sin cambios → **COMMIT**; `Holgura` recuperada → aborta por la decisión; Y cambiada → aborta por la observación de símbolo; con los dos cambios, el aborto informa primero la decisión de reparación | G9 |

- **P28.9** — V6 no exige refactorizar los fixtures duplicados que el Discovery contó (≈25 clases, Discovery §14.1).
  Las pruebas nuevas pueden introducir un único constructor de registros y fuentes con expresiones.
- **P28.10** — Checklist de validación del Owner: §8.3.

### P29 — migration

**Base.** Leer nunca escribe y re-escribir no conserva bytes, pero sí versión, `ExtensionData` y contenido
(Discovery §5.7).

- **P29.1** — **Sin migración de datos**: ni registros, ni bindings de rack, ni bibliotecas se reescriben al abrir.
- **P29.2** — Sin conversión automática de literales o referencias en expresiones, ni de expresiones en literales, y
  sin detección de «valores iguales».
- **P29.3 — Versiones (V-0).** La primera expresión **no** cambia ninguna versión, y quitar todas las expresiones
  devuelve el dibujo a lo que una build anterior puede usar. Los escritores de I-49 nunca degradan una versión
  (P23.15).
- **P29.4** — En una build anterior, un dibujo con expresiones falla cerrado (P18.2). Se documenta en el ADR y en la
  comunicación al usuario; sin herramienta de degradación (P30).
- **P29.5** — La compatibilidad se verifica en G12 con el DLL de `a4d88f1`, sobre tres dibujos: sin expresiones, con
  expresiones en el registro y con expresiones de propiedad.
- **P29.6** — Los caminos heredados `SelectiveBindingOptions.ForLength` y `BindingIntent`, sin disparador en producción
  (Discovery §8.1), **no** se extienden (riesgo R15).

### P30 — non-goals

- **P30.1** — ID21: referencias rack→rack. `RackPropertyReference` y `rackProperty` siguen prohibidos.
- **P30.2** — ID20 productivo: símbolos `Rack.*` y `Project.*`, parámetros calculados y ámbito `Rack`; solo quedan
  reservados (P25.2–P25.3).
- **P30.3** — ID23: Custom / Calculated BOM, cantidades enteras y fórmulas de BOM.
- **P30.4** — UI de ID28 Explain y de ID29 Impact Preview.
- **P30.5** — Sistema de dimensiones físicas, comprobador dimensional, tipos de área, volumen, masa o ángulo, nuevos
  `VariableType` y **variables de usuario escalares o con signo**: exigen otro `VariableType` u otra iniciativa
  (§0.9).
- **P30.6** — Unidades distintas de `[mm]`, `[in]` y `[ft]`; unidades sobre referencias, llamadas o paréntesis;
  sintaxis de pies-pulgadas o de fracciones; unidades en literales sin `=`; conversión del DWG.
- **P30.7** — Funciones distintas de `MIN`, `MAX` y `ABS`: `ROUND`, `CEILING` y `FLOOR` quedan diferidas (P10.6), e
  `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings, trigonometría y macros, excluidos (P10.7).
- **P30.8** — Un `FormulaTextBox` o cualquier control de entrada paralelo (P23.1).
- **P30.9** — Parsers o evaluadores externos (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) y
  cualquier dependencia NuGet (ADR-0012).
- **P30.10** — Grafo, valores evaluados, trazas, conjuntos o texto tecleado persistidos.
- **P30.11** — Sintaxis localizada (`;` como separador y `,` decimal) y completar texto sin selección explícita o
  nombre exacto.
- **P30.12** — Control de concurrencia más allá de la revisión acotada del `PlanReadSet` (P21.6): hash de snapshot,
  token global, comparación completa del registro, rechazo por cambios fuera del `PlanReadSet`, locks adicionales o
  volver a ejecutar el preflight en commit.
- **P30.13** — Transferencia o fusión de variables entre dibujos (decisiones de I-47, C2-8).
- **P30.14** — Política de unicidad o de mayúsculas de nombres, y renombrado de variables existentes.
- **P30.15** — Comandos o ventanas nuevos, otros sistemas de rack (Dinámico, Push Back, Cama, Cantilever) y nuevas
  propiedades vinculables.
- **P30.16** — Herramientas de degradación o de exportación a versiones anteriores.
- **P30.17** — Corregir los hallazgos laterales L1–L5 (Discovery §7, §5.8, §8.1, §11.5). Se registran en
  `docs/ideas-futuras.md` (contrato §4) en G4. **Excepciones acotadas**, que no son arreglos laterales sino
  preservación de invariantes en código que I-49 reescribe:
  - L2 en la rama del reconciliador que I-49 extiende (P23.15, A-09);
  - la tabla explícita del token `Type` en la rama `Add` (P17.10, A-17).

  L3 no se corrige en el portador: I-49 aísla su propio estado (P23.16).
- **P30.18** — Hacer converger los parsers de literales del editor y de RACKVARIABLES (Discovery §11.2).
- **P30.19** — Cambiar invariantes de I-47/I-48 más allá de lo que enumera §2.3: es condición de parada, no alcance.
- **P30.20** — Una operación o un comando de reparación nuevos: `RepairBrokenRack` sigue siendo la única (§7.2).
- **P30.21** — Convertir de forma implícita una fórmula en literal, en `UnlinkAllAndDelete` o en cualquier otra
  operación (§7.1).
- **P30.22** — Relajar la validez de una variable definida por expresión respecto de la misma variable definida por
  literal (§0.9).
- **P30.23** — Centralizar en la autoridad de unidades constantes de dominio que valen 12
  (`SelectiveGeometryResolver.FootInches`, `DynamicHeaderHeightCalculator.CommercialFoot` y el `12.0` de
  `CantileverArmFrameResolver`), o buscar el número `12` de forma mecánica (P9.6–P9.7, CR-1).
- **P30.24** — Relajar R1 o la decisión C4-13 de I-47 para permitir corregir una variable mientras un rack que
  consume su cierre conserva otras fuentes inválidas (§7.2, CR-4).
- **P30.25** — Extender el `PlanReadSet` al reconciliador de RACKEDITAR, que no produce un `MutationPlan` (P21.12).
- **P30.26** — Abortar un commit porque un símbolo observado falle con el mismo resultado que el preflight admitió,
  esperar un valor «después» de un símbolo que la mutación elimina, observar valores que el preflight no usó o
  comparar texto de diagnósticos (P21.6, FR-1). Tampoco, en `RepairBrokenRack`, observar por separado cada valor
  intermedio de una fuente cuya razón reparable no cambió, convertir un id ausente en símbolo o comparar la razón de
  reparación por su texto (P21.6, RR-1).

---

## 4. OPEN A — CLOSED: sintaxis de homónimos

```text
Estado V2  = OPEN FOR ARCHITECT
Veredicto  = Architect Review V2, §3: nombres sin delimitar cuando es seguro, llaves para el resto, #GUID completo
Estado V3  = CLOSED — decisión de V3 con la sintaxis del Architect
Precisión  = #<GUID> sin nombre: solo lo emite el formatter para referencias rotas; nunca enlaza
Re-review  = precisión #<GUID> ACCEPTED; sin cambios en V4
V5         = sin cambios
V6         = sin cambios
```

### 4.1 Formas

```text
Nombre                              nombre único y seguro sin llaves
{Nombre complejo}                   nombre con cualquier otro carácter, o reservado
Nombre#<GUID completo>              homónimo, o identidad pegada o tecleada
{Nombre complejo}#<GUID completo>   homónimo con llaves
#<GUID completo>                    SOLO formatter de referencia rota
```

Gramática: producción `reference` de P1.2, con las reglas léxicas de P1.7.

### 4.2 Reglas

1. **GUID completo, nunca un fragmento.** Forma D de 36 caracteres (P3.9). `#3f2b1c9e` es `InvalidQualifier`.
2. **`Name` no es identidad.** Con cualificador, el id es la autoridad y el nombre solo se valida
   (`QualifiedNameMismatch`). Lo persistido son ids (P17).
3. **Las llaves delimitan nombres complejos**, y `}}` escapa `}`. Todo nombre que no sea `word {" " word}` va entre
   llaves: operadores, paréntesis, comas, corchetes, punto, dígito inicial, espacios dobles, iniciales o finales.
4. **Reservados sin llaves**: los nombres de función del registro (P10.9), `Rack` y `Project`, sin distinguir
   mayúsculas (`ReservedName`). `palabra.` es sintaxis de namespace para ID20 (`UnknownNamespace`).
5. **Un fragmento parcial nunca enlaza** (`UnknownSymbol`). La lista filtra e inserta; no resuelve.
6. **Un nombre exacto y único sí enlaza al comprometer**, con el comparador de P7.2.
7. **Un nombre ambiguo da diagnóstico** `AmbiguousName`, con cada candidato en su forma cualificada.
8. **La lista y el selector insertan la forma inequívoca** del formatter (P22.5, P23.7).
9. **La forma cualificada se puede pegar o teclear.**
10. **`[mm]`, `[in]` y `[ft]` solo después de números.** Tras una referencia dan `UnitNotAllowedHere`.
11. **Un `#GUID` roto se puede MOSTRAR, pero no comprometer** como referencia nueva a un id inexistente
    (`BrokenReference`). `#<GUID>` sin nombre con id presente da `NameRequired`.
12. **Seguridad con operadores.** Un tramo contiguo, sin llaves ni espacios, que coincide con el nombre de una
    variable (`Holgura-Base`) da `OperatorInName`; nunca se reinterpreta como resta.

### 4.3 Formatter

Forma mínima inequívoca dentro del snapshot (P4.2, P4.7):

| Situación | Texto |
|---|---|
| Id ausente | `#<id>` |
| Nombre único y seguro sin llaves | `Nombre` |
| Nombre único con cualquier otro carácter, o reservado | `{Nombre}`, con `}` escapado como `}}` |
| Homónimo | La forma del nombre + `#<id>` |

Si un nombre único pasa a ser ambiguo, la siguiente presentación lo cualifica; lo persistido no cambia.

### 4.4 Ejemplos

| Caso | Texto |
|---|---|
| Nombre único | `=Holgura + 2` |
| Espacios simples | `=Holgura General + 2` |
| Operadores, paréntesis, punto, dígito inicial o espacios dobles | `={Holgura-Base} * 2` · `={Alto (m)}` · `={Nivel 2}` · `={v1.2}` |
| Llave en el nombre | `={a}}b}` (nombre `a}b`) |
| Igual a una función o reservado | `={MIN} + 1` · `={Rack}` |
| Homónimo | `=Holgura#3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44 + 2` |
| Homónimo con llaves | `={Holgura General}#8c1d7e20-4b5a-4c6d-9e7f-102132435465` |
| Referencia rota (solo presentación) | `=#3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44 + 2`, no comprometible |
| Unidad | `=100[mm] + Holgura` |
| ID20 futuro | `=Rack.Frentes * 2` → `UnknownNamespace` |

### 4.5 `CASO_8` actualizado

`CASO_8_ENTER_SIN_CANDIDATO_SELECCIONADO_NO_RESUELVE_POR_TEXTO` se reescribe según este contrato: **«Enter
resuelve un nombre exacto y único; nunca uno parcial ni ambiguo»** (P23.12). `CASO_6` cambia de clase y `CASO_7`
queda intacto.

### 4.6 Los cuatro requisitos congelados

| Requisito (encargo de G2A.1) | Cómo se cumple |
|---|---|
| homónimo termina en `VariableId` inequívoco | El GUID completo no colisiona, y un homónimo sin cualificar no enlaza |
| `Name` no es autoridad | El nombre valida, nunca resuelve con cualificador; sin cualificador, solo resuelve si es exacto y único **en el snapshot mostrado** (P7.7) |
| formatter puede representar fórmula editable | P2.5 se prueba como propiedad, con homónimos y llaves |
| rename no rompe | Lo persistido son ids (P19.5) |

---

## 5. OPEN B — CLOSED: legibilidad estructural frente a evaluabilidad semántica

```text
Estado V2  = OPEN FOR ARCHITECT
Veredicto  = Architect Review V2, §4 (clasificación y diez decisiones)
Estado V3  = CLOSED — decisión de V3
Sustituye  = FatalUnevaluable de V2 (P24.6), retirado como callejón sin salida (A-01)
V4         = sigue CLOSED; CR-4 precisa las decisiones 8 y 10, y CR-5 la decisión 9
V5         = sigue CLOSED; FR-1 alinea la decisión 9 con R2 y con las eliminaciones
V6         = sigue CLOSED; RR-1 añade a las decisiones 9 y 10 la protección de las premisas de reparación
```

**Principio.** Un fallo **estructural** es algo cuyo significado la build no entiende: no se lee y no se repara de
forma destructiva. Un fallo **semántico** es una fuente que la build entiende pero de la que no obtiene valor: se
diagnostica y se corrige, sin fallback.

### 5.1 Clasificación

| Caso | Definición en el registro | Fuente de rack | Clase |
|---|---|---|---|
| `Kind`, `Node` o `Namespace` desconocido persistido | Registro **entero** `PresentButUnreadable`, como hoy con un kind desconocido | `FatalMalformedReference`: rack `Blocked`, sin reparación destructiva | **Estructural** |
| Token de función desconocido persistido | Igual | Igual | **Estructural** |
| Token de unidad desconocido persistido | Igual | Igual | **Estructural** |
| Id o AST mal formado: id ilegible, campo obligatorio ausente, número no finito, límites superados | Igual | Igual | **Estructural** |
| Campo desconocido o repetido dentro del payload de Expression (A-08) | Igual | Igual | **Estructural** |
| Dos autoridades en la misma entrada: estado imposible | Igual | Igual | **Estructural** |
| `BrokenReference` | Símbolo `Failed` | `RepairableMissingTarget` | Semántico |
| `Cycle` | Miembros `Failed(Cycle)`, con la ruta | Fuentes que los leen: `RepairableSemanticFailure(Upstream)` | Semántico |
| `InvalidArguments` | `Failed` | `RepairableSemanticFailure(Intrinsic)` | Semántico |
| `DivisionByZero`, `NonFiniteResult` | `Failed` | `RepairableSemanticFailure(Intrinsic)` | Semántico |
| `DependencyFailed` | `Failed` | `RepairableSemanticFailure(Upstream)` | Semántico |
| `NonCanonicalForm` (A-07) | `Failed` | `RepairableSemanticFailure(Intrinsic)` | Semántico |
| `OutOfRange` del consumidor | No aplica al leer: rige el contrato histórico de la variable (P8.10) | `RepairableSemanticFailure(Domain)` | Semántico |

Un fallo semántico da un resultado **por símbolo o por fuente**, y el registro sigue estructuralmente `Usable`.

Una fuente de rack estructuralmente mal formada se comporta como hoy un kind desconocido: la sonda da
`Indeterminate` y las mutaciones que necesitan descubrimiento abortan (P21.4). Un dato que la build no entiende nunca
se interpreta como «no consume». RACKVARIABLES muestra ese rack como bloqueado, sin reparación.

### 5.2 Decisiones

1. **Alcance del bloqueo: solo el cierre afectado.** Queda afectado lo que falla: los símbolos en error, sus
   dependientes transitivos y todo rack con al menos una fuente no sana. Con errores solo semánticos, el registro
   sigue `Usable`.
2. **Rack no afectado**: funciona con normalidad. Sus opciones no ofrecen variables en error (P23.11).
3. **Rack afectado** (P24.12):
   - sin efectivo;
   - RACKEDITAR bloqueado: no lo abre y remite a RACKVARIABLES con la causa;
   - geometría existente intacta;
   - RACKVARIABLES ofrece diagnóstico y reparación;
   - solo entra en un plan que lo deje resuelto: su reparación o la corrección de la causa superior (R1).
4. **RACKBOMTOTAL**: aborta si un rack incluido en ese BOM está afectado, como hoy con una referencia rota (ADR-0034
   §10). Una variable fallida sin consumidor cotizado no bloquea por sí sola (P24.7).
5. **RACKVARIABLES**: abre siempre que el registro sea estructuralmente legible y esté acreditado. Su panel de
   diagnóstico, fuera del panel editable, muestra la causa raíz, los miembros de cada ciclo, los dependientes y las
   fuentes de rack afectadas (P22.9). Con un error estructural sigue bloqueado como hoy, sin escribir.
6. **Intents correctivos**: se admiten todos los normales, más la reparación, gobernados por R1, R2 y R3 (P21.3).
   `Rename` siempre está permitido (P19.7).
7. **Ciclos persistidos**: todos los miembros dan `Cycle` con la ruta; se rompen con `ChangeDefinition` de un
   miembro; borrar un miembro sigue bloqueado (P13.5).
8. **Fórmulas de propiedad rotas**: dos vías. Corregir la variable superior, que las recupera sin quitarlas **solo
   cuando R1 lo permite**, o reparar el rack y volver a escribir la fórmula en RACKEDITAR. El diagnóstico nunca
   indica la primera vía si no es demostrable en el estado diagnosticado (§7.2) **[V4 · CR-4]**. Las estructurales
   no se reparan, para no destruir datos que esta build no entiende.
9. **Re-lectura de commit** (P21.6) **[V4 · CR-5 · V5 · FR-1 · V6 · RR-1]**:
   - re-lectura estructuralmente ilegible → aborto;
   - re-acreditar y validar ids;
   - en `RepairBrokenRack`, recomputar con la misma inspección la `RepairDecisionObservation` de cada fuente que se
     retira, y abortar antes de escribir si la decisión de reparación difiere **[V6 · RR-1]**;
   - comparar cada observación del `PlanReadSet` en su fase —`Before` sobre la re-lectura acreditada, antes de aplicar
     la mutación; `After` sobre el documento cambiado— y abortar antes de escribir **solo** si un resultado observado
     difiere del esperado, también en planes sin `RegistryMutation` que escriben racks;
   - un error no aborta por sí mismo: un fallo idéntico al esperado, admitido por R2, no bloquea, y los cambios y
     errores fuera del `PlanReadSet` tampoco;
   - es una revisión acotada de V8-R05 que exige ADR y aceptación del Owner (§2.3, §9).
10. **`RepairBrokenRack`**: una sola operación con el conjunto reparable ampliado, confirmación consciente de la
    fuente y mensaje de recuperación condicional (§7.2). Su commit aborta si cambió la premisa de alguna fuente que
    retira (P21.6) **[V6 · RR-1]**.

### 5.3 Los dos requisitos congelados

| Requisito (encargo de G2A.1) | Cómo se cumple |
|---|---|
| geometry/BOM jamás fallback | Un rack afectado no tiene efectivo, su geometría existente no se toca, RACKBOMTOTAL aborta si lo incluye y ninguna fuente no sana produce un valor (P24.7, P24.12) |
| RACKVARIABLES debe poder diagnosticar/corregir estados semánticamente rotos que esta build puede leer estructuralmente | El registro sigue `Usable`; RACKVARIABLES abre y diagnostica; R1–R3 y la reparación generalizada garantizan una salida para cada estado semántico (§5.2, decisiones 5, 6, 8 y 10) |

---

## 6. Schema — CLOSED: V-0

```text
Estado V2  = ARCHITECT REVIEW REQUIRED (V-0 propuesta)
Veredicto  = Architect Review V2, §5: V-0 con cuatro condiciones
Estado V3  = CLOSED — V-0
V4         = sin cambios
V5         = sin cambios
V6         = sin cambios
```

### 6.1 Decisión

**No cambia** la versión de `ProjectVariablesDocument` (sigue `1.0`, major soportado 1) ni la del diseño Selectivo
(`1.0` / `2.0` *sticky*, major de lectura 2).

**Justificación principal**: las builds I-47/I-48 ya fallan cerradas ante un discriminador desconocido y no leen mal
la semántica nueva (P18.2). Un major no protegería nada. V-1 no cambia el comportamiento de ningún lector y V-2
excluye a las builds anteriores para siempre (P18.4–P18.5). V-0 **no** se justifica por reducir archivos tocados
(P18.7).

### 6.2 Condiciones obligatorias

| # | Condición | Dónde se cumple | Cómo se verifica |
|---|---|---|---|
| C1 | Payload de Expression con mundo cerrado | P17.6.7 | T-V3-15 |
| C2 | Semántica nueva solo por discriminadores o tokens cerrados, o por un major | §6.3, en el ADR | Revisión de cada adición (P18.8) |
| C3 | Los escritores de I-49 nunca degradan ni fijan una versión | P23.15 | Prueba: un diseño `2.3` editado sigue en `2.3` |
| C4 | Caracterización del comportamiento heredado antes de modificar los stores | G8, paso 1: commit solo de pruebas con su CI | T-V3-21 |

### 6.3 Regla de evolución, para el ADR

La semántica futura de Expression entra **solo** por tokens o discriminadores explícitos que la build lectora
conozca, o por un cambio de major que obligue a las builds anteriores a rechazar el documento. **Nunca** por campos
que una build anterior pudiera ignorar. La regla vincula a las builds futuras en todo el documento, no solo dentro
del payload, donde además la impone el mundo cerrado (P17.6.7).

---

## 7. Veredictos de operación — CLOSED

### 7.1 `UnlinkAllAndDelete(X)`

```text
Direct reference to X                       → materializable como hoy
Property expression depending on X          → BLOCKED
ProjectVariable definition depending on X   → BLOCKED
```

- **Referencias directas**: se materializan como hoy. El efectivo, es decir el valor evaluado exacto de X, pasa a
  literal y se quita el binding (`ProjectVariableMutationPreflight.cs:263,272,283-287`).
- **Expresiones de propiedad** que dependen de X: la operación entera queda **bloqueada**.
- **Definiciones de variable** que dependen de X: la operación entera queda **bloqueada**.
- **Sin conversión implícita** de fórmulas en literales y sin sustituir el valor de X dentro de ninguna fórmula.
- **El bloqueo muestra** los dependientes (variable y definición formateada) y los consumidores que bloquean (rack,
  propiedad y fórmula canónica).
- **R1**: todo rack materializado tiene que resolver después; si no, bloqueado con «repara ese rack primero».
- **Commit** **[V5 · FR-1]** (P20.8, P21.6): el valor de X que se materializa se leyó en «antes» y queda protegido
  por `Before(X, Success(valor))`; X no tiene observación `After`. Si X cambió o desapareció entre preflight y commit:
  `ABORT BEFORE WRITE`. Si no, commit.
- **Salida para el usuario**: editar cada fórmula en RACKEDITAR y cada definición dependiente en RACKVARIABLES, y
  ejecutar después la operación.

**Por qué.** «Desvincular» materializa el efectivo de una propiedad que **sigue** a X, y una fórmula no sigue a X:
sigue a su fórmula. `palletTolerance = A + B` también depende de B, y materializarla cortaría una dependencia que
nadie pidió tocar. La operación vigente solo recorre bindings directos (`:263`). Es la misma regla que ya se aplica a
los dependientes, y ADR-0034 §11 rechaza reescribir un diseño authored como efecto colateral.

### 7.2 `RepairBrokenRack(rack)`

- **Una sola operación**: mismo nombre, mismo intent `RepairBroken(rackId, confirmed)`, rack-scoped, atómica,
  explícita y confirmada (ADR-0034 §11; `ProjectVariableMutationPreflight.cs:458-530`). **Ningún comando nuevo.**
- **Conjunto reparable ampliado** [V3 · A-01]: toda fuente `RepairableMissingTarget` o `RepairableSemanticFailure`,
  por causa propia, superior o de dominio (P24.6).
- **Efecto**: quita esas fuentes de `PropertyValues` y deja gobernar el literal congelado, sin tocarlo, sobre un clon
  (`:514-520`). El rack resuelve por construcción y produce **una** `RackMutation` (`MutationPlan.cs:150-168`); si
  aun así no resolviera, el plan queda vacío (`:522-529`).
- **No reparable**: lo estructural (`FatalMalformedReference`, `FatalUnknownProperty`) y `FatalIncompatibleTarget`.
  El rack queda `Blocked` entero aunque tenga además fuentes reparables (`:477-482`): no se destruyen datos que esta
  build no entiende.
- **Commit** **[V6 · RR-1]** (P21.6). Cada fuente que se quita lleva una `RepairDecisionObservation` con su razón
  reparable. En commit, la misma inspección la recomputa sobre la re-lectura, antes de validar las fuentes que se
  conservan. Si alguna fuente ya no es reparable por la misma razón —se recuperó, cambió su causa, reapareció el id,
  desapareció otro o volvió al dominio— → `ABORT BEFORE WRITE`, sin borrar ninguna fórmula. Solo después se validan
  las fuentes conservadas y se escribe la `RackMutation`.
- **Confirmación consciente de la fuente** [V3 · A-10]. Hoy solo lista `'propiedad' -> 'id' (literal almacenado X)`
  (`:491-511`), y al reparar una expresión el usuario perdería una fórmula que puede contener referencias válidas.
  La confirmación incluye, **por fuente**:

  | Campo | Contenido |
  |---|---|
  | `Rack` | Nombre e id del rack |
  | `PropertyId` | Propiedad afectada |
  | `SourceKind` | `projectVariable` o `expression` |
  | `CanonicalExpression` | Texto del formatter en el snapshot, con `#<id>` para referencias rotas |
  | `FailureCause` | Código y dueño: id ausente, causa propia, causa superior o dominio |
  | `FrozenLiteral` | Literal congelado que pasará a gobernar |
  | `UpstreamCause` | Si aplica: la variable superior y su código |
  | Mensaje de recuperación | Condicional, con la regla de abajo **[V4 · CR-4]** |

  Aparece en el panel de RACKVARIABLES antes de confirmar y en el mensaje de confirmación del preflight. La
  confirmación sigue siendo del **conjunto completo** del rack, como hoy.
- **Mensaje de recuperación condicional** **[V4 · CR-4]**. El diagnóstico nunca indica sin condiciones «corregir X
  recuperaría esta fórmula». R1 **no** se relaja, así que corregir una variable solo es posible si, después, todos
  los racks que consumen su cierre resuelven.
  1. **Clasificación.** Cada fuente fallida de un rack se clasifica por su causa: **superior corregible**, con su
     conjunto de causas raíz (variables cuya propia definición falla, o un ciclo tomado como una unidad), o **no
     corregible desde arriba**: id ausente, fallo propio, dominio inválido u otra causa que mantendría R1 bloqueado.
  2. **Condición para mostrar la sugerencia de recuperación.** Para una fuente con causa superior y causa raíz única
     X, el diagnóstico puede mostrar la sugerencia **solo** si, en el estado diagnosticado, todas las fuentes fallidas
     de todos los racks que consumen el cierre de X tienen causa superior con causa raíz exactamente X. Solo entonces
     ninguna otra fuente inválida conocida impide la recuperación, y un `ChangeDefinition` de X que supere la
     validación normal del estado resultante deja esos racks resueltos sin reparar **[V5 · N-1]**.
  3. **Si se cumple**, el mensaje es:

     ```text
     Corregir <X> permitiría recuperar esta fuente sin eliminarla.
     ```

     Si X es un ciclo, `<X>` nombra a sus miembros: corregir cualquiera de ellos rompe el ciclo.
  4. **Si no se cumple**, porque hay cualquier fuente con fallo propio, dominio inválido, id ausente u otra causa
     superior independiente que mantendría R1 bloqueado, el mensaje es:

     ```text
     La corrección de <X> no puede aplicarse mientras este rack
     mantenga otras fuentes inválidas.

     Reparar el rack eliminará también esta fórmula:
     <formula canónica>

     Guárdala si deseas volver a escribirla después.
     ```

     A continuación se listan las fuentes que bloquean, con rack, propiedad y causa, también cuando están en otro
     rack que consume el cierre de X.
  5. **Consecuencia declarada.** Con R1 intacto, un rack con dos fuentes que dependen de causas raíz independientes
     no se recupera corrigiendo una sola variable, porque cada corrección deja la otra fuente inválida. La salida es
     la reparación, que muestra antes las fórmulas que elimina.
  6. **Sin promesas futuras** **[V5 · N-1]**. El diagnóstico no afirma una recuperación que no pueda demostrar en el
     estado diagnosticado. «Permitiría» significa **solo** que ninguna otra fuente inválida conocida, de este rack o
     de otro rack que consuma el cierre de X, mantiene por sí sola bloqueada esta recuperación. **No** significa que
     R1 vaya a admitir cualquier corrección: la definición nueva de X sigue sujeta a la validación normal del estado
     resultante, que incluye R1 con los dominios de las propiedades evaluados con los valores nuevos, R2 y el contrato
     raíz (P8.10), y en commit el `PlanReadSet` (P21.6). R1 no se relaja.
  7. **Mismo texto en el rechazo.** Cuando R1 bloquea una corrección (P21.2, paso 8), el mensaje del preflight usa la
     misma clasificación: lista las fuentes que bloquean y advierte qué fórmulas eliminaría la reparación.

---

## 8. Gates G0–G12

> **Propuestos, NO autorizados.** Solo se abre el gate siguiente con evidencia revisable del anterior (contrato
> §8). La división de G5–G10 puede ajustarse en la comprobación final.

### 8.1 Secuencia

| Gate | Nombre | Entregable | Evidencia exigida | Estado |
|---|---|---|---|---|
| **G0** | Reclamo + bootstrap | Reclamo atómico `77262fe`; contrato, decisión del Owner y fila de ROADMAP `f2d28a2` | Push aceptado sin force | **HECHA** |
| **G1** | Discovery read-only | [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` | CI 4/4 en ese SHA | **HECHA** |
| **G2** | Proposal | V1 `b15e40a` → V2 `9aef7d0` (`Architect = CHANGES REQUIRED`) → V3 `4df9480` (re-revisión: `CHANGES REQUIRED`, CR-1…CR-5) → V4 `ccf21c6` (revisión final: `CHANGES REQUIRED`, FR-1) → V5 `2783acc` (re-revisión final: `CHANGES REQUIRED`, RR-1) → **V6** (G2F) → comprobación final del Architect, hasta `Architect = AGREED` | Architect Reviews | **EN CURSO** (G2F) |
| **G3** | Consenso + freeze | `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal, más la decisión del Owner | Registro en `decisions/I-49.md` | pendiente — **compuerta** |
| **G4** | ADR + alineación documental | ADR aceptado por el Owner, con número asignado **en ese momento**, que formaliza también la revisión acotada de V8-R05 (CR-5) con la semántica de observaciones de V5 (FR-1) y las decisiones de reparación de V6 (RR-1); contrato (§1, §3.2, §11.2) y ROADMAP alineados con la Proposal congelada; en `ideas-futuras.md`: L1–L5, la unificación del dominio de la ventana en el descriptor (P24.5) y el factor lb/ft→kg/m de `tools/` (P9.6) | ADR en estado `aceptado` | NO AUTORIZADA |
| **G5** | Núcleo sintáctico | Lexer con unidades, nombres y cualificadores; parser con la gramática de §4; sintaxis; formatter; guardas del parser (texto y anidamiento sintáctico); diagnósticos de sintaxis; guardas G1 (parcial), G4, G5 e independencia del núcleo | RED→GREEN; focal + impacto | NO AUTORIZADA |
| **G6** | Núcleo semántico | `SymbolId`, namespaces, ámbitos, `ExpressionContext`, tabla de símbolos sin `VariableType`, `SymbolResolver` y binder de §4, límites normativos del árbol con la profundidad del `BoundExpression` (P1.9), autoridad de unidades con alias **solo** en `StructuralSectionUnits` (P9.7), `FunctionRegistry` (`MIN`, `MAX`, `ABS`), tablas de tokens, evaluador adimensional y `EvaluationResult`; guarda de unidades acotada y guarda sin comprobador dimensional | RED→GREEN; dorados geométricos intactos | NO AUTORIZADA |
| **G7** | Núcleo de grafo | Extracción de dependencias, grafo, ciclos, orden, conjuntos derivados (P12.7) y `RegistryEvaluation`; guarda G1 completa | RED→GREEN | NO AUTORIZADA |
| **G8** | Persistencia + integración | **Paso 1**: commit solo de pruebas, con su CI, que caracteriza sobre la producción intacta el rechazo de un registro y de un diseño con `expression` (C4, T-V3-21). **Paso 2**: `VariableDefinition.Expression`, fuente `expression`, stores con mundo cerrado (P17.6) y tablas de tokens (P17.10), clasificación de §5, target que expone `RegistryEvaluation`, `InspectBinding` de P24.6, resolver con expresiones y dominio, BOM, restamp, exportación y prueba del peor caso de profundidad por descubrimiento de los puntos de serialización (P1.9); guardas G2, G3, G12, kinds de binding y tokens explícitos | RED→GREEN; fixture dorado literal-only; CI del paso 1 | NO AUTORIZADA |
| **G9** | Mutaciones y propagación | `ChangeDefinition`, `Create` y `Rename` con intents enlazados; contrato raíz con los casos A–D (P8.10); R1–R3; `PlanReadSet` por observaciones `Before`/`After` y re-lectura de commit, con y sin `RegistryMutation` (P21.6); `Delete` y `UnlinkAllAndDelete` con los bloqueos de §7.1 y su commit (P20.8); `RepairBrokenRack` generalizado con su confirmación, su mensaje de recuperación condicional y la `RepairDecisionObservation` de cada fuente que retira (§7.2, P21.6); descubrimiento por conjunto con oráculo; un resolve por rack; guardas G13 y G14 | RED→GREEN | NO AUTORIZADA |
| **G10** | UX | RACKVARIABLES con enlace contra el snapshot, campo «Definición», selector y panel de diagnóstico con mensaje de recuperación condicional (P22); sesión y control del `LinkedPropertyEditor` con la sintaxis de §4 y los pines de P23.12; reconciliador con estado aislado y versión calculada (P23.15–P23.16); `Describe` de la ventana Selectiva tras el procedimiento de colisión; guardas G18 y G20 | RED→GREEN; suite UI | NO AUTORIZADA |
| **G11** | Candidato | Pruebas reales de cadena hasta geometría y BOM; las dos suites en local; builds Debug de UI y Plugin; CI verde sobre el SHA exacto; medición de coste de la propagación (R5) | Clases de evidencia de AGENTS.md y WORKFLOW §4–§5 | NO AUTORIZADA |
| **G12** | Owner Validation + integración | Checklist de §8.3 en AutoCAD 2025; integración **serializada** con las iniciativas activas, sobre el `main` que ya integra I-50; cierre documental (HANDOFF y ROADMAP); CI posterior al merge | WORKFLOW §4.5 | NO AUTORIZADA |

### 8.2 Reglas de la secuencia

- G5 y G7 **no** tocan producción fuera del núcleo nuevo. G6 solo toca, fuera del núcleo, las dos declaraciones de
  `StructuralSectionUnits.cs`, con comportamiento idéntico. G8 es el primer gate que toca producción de Project
  Variables o de persistencia, y G10 el primero que toca UI.
- **Ningún** gate modifica `SelectiveGeometryResolver.cs`, `DynamicHeaderHeightCalculator.cs` ni
  `CantileverArmFrameResolver.cs` por motivos de unidades (P9.7, CR-1).
- En G8, el paso 1 (caracterización, solo pruebas) se integra en la rama con CI verde **antes** de cambiar ningún
  store (condición C4 de §6.2).
- G9 no implementa el `PlanReadSet` hasta que el ADR que formaliza la revisión acotada de V8-R05 esté aceptado por el
  Owner (G4, CR-5).
- Antes de editar **cualquier** archivo productivo, cada gate de G5 a G10 ejecuta el procedimiento de colisión de
  [`decisions/I-49.md`](../automation/decisions/I-49.md) §8 contra las iniciativas activas y comprueba el `main`
  vigente, que ya integra I-50 (§12).
- La rama se reconcilia con `main` según WORKFLOW antes del primer gate productivo (§12.2).
- El Plugin se toca solo donde G9 o G10 lo exijan, archivo por archivo y con la comprobación anterior.
- Un gate que descubra que necesita cambiar una invariante de I-47/I-48 más allá de §2.3 **se detiene** (§13).

### 8.3 Checklist propuesto de Owner Validation (G12, AutoCAD 2025)

1. Crear `AlturaBase` literal y `AlturaFinal = AlturaBase + Holgura` por expresión; comprobar el valor evaluado en
   RACKVARIABLES.
2. En RACKEDITAR, escribir en el **mismo** campo `6`, `=Holgura`, `=Holgura + 2` y `=(AlturaBase + Holgura) / 2`,
   confirmando cada uno con Enter; geometría y BOM reflejan el valor.
3. Teclear una expresión y perder el foco: **no** se aplica y el borrador sigue ahí. Pulsar Escape: vuelve lo
   comprometido.
4. Con dos campos con borradores, uno inválido, pulsar Actualizar: **no** se aplica ninguno y el foco va al culpable.
5. Unidades: `=100[mm] + 2`, `=20[ft]` y `=4[in]` dan los valores esperados en pulgadas.
6. `=Holgura` + Enter y seleccionar `Holgura` en la lista dejan **el mismo** vínculo guardado.
7. Nombres: `=Holg` + Enter se rechaza; con dos `Holgura General`, `=Holgura General` + Enter da un diagnóstico con
   las dos formas cualificadas, y `=Holgura General#<id>` enlaza ese id; `Holgura-Base` sin llaves da
   `OperatorInName` y la lista inserta `{Holgura-Base}`.
8. Cambiar `AlturaBase`: se redibujan los racks de `AlturaFinal` y los que usan una expresión con ella, con **un**
   solo `Regen`.
9. Renombrar `AlturaBase`: las expresiones muestran el nombre nuevo, sin redibujo. Crear un homónimo: la referencia
   pasa a mostrarse cualificada.
10. Contrato raíz: en RACKVARIABLES, `-2` y `=0 - 2` se rechazan igual; `=Base + (A - B)` con `A < B` y raíz
    positiva se acepta; un cambio de `A` que dejaría `AlturaFinal ≤ 0` se rechaza; con el fixture del caso C
    (`A` rota, `B = A - 100`), redefinir `A` como `=Base` con `Base = 10` se rechaza por `B`.
11. Intentar borrar `AlturaBase` con dependientes o con consumidores por expresión: bloqueado, con las listas.
    `UnlinkAllAndDelete` con una expresión consumidora: bloqueado, con rack, propiedad y fórmula. Borrar una variable
    sin consumidores ni dependientes y `UnlinkAllAndDelete` con solo referencias directas terminan y escriben (FR-1).
12. Intentar crear un ciclo, escribir una expresión con error de sintaxis o una suma de 25 términos sin paréntesis:
    rechazado, sin mutación.
13. Estado semántico roto preparado como fixture (`ABS(A, B)` en un rack): RACKEDITAR no lo abre y remite a
    RACKVARIABLES. RACKVARIABLES muestra la causa, y la confirmación de reparación muestra fórmula, causa y literal
    congelado. Tras reparar, el rack abre. Los racks no afectados funcionan, y RACKBOMTOTAL aborta solo si incluye el
    rack afectado.
14. Mensaje de recuperación: un rack cuya única fuente inválida depende de una variable rota muestra «Corregir
    <variable> permitiría recuperar esta fuente sin eliminarla.», y una corrección válida la recupera; si el rack
    tiene además una fuente con fallo propio, el mensaje no indica recuperación y muestra la fórmula que eliminaría
    la reparación.
15. Estado estructural roto preparado como fixture (nodo desconocido): registro ilegible, sin reparación ofrecida y
    sin escritura.
16. Guardar, cerrar y reabrir: definiciones, fuentes, valores y vínculos intactos.
17. Con el DLL de `a4d88f1`: un dibujo sin expresiones funciona como hoy; uno con expresiones en el registro o en un
    rack falla cerrado, sin escritura.
18. RACKBOMTOTAL con racks ligados por referencia y por expresión.

---

## 9. ADR REQUIRED — número NO asignado, todavía no escrito

- **Por qué es obligatorio.** V6 cambia contratos persistidos (P17), extiende ADR-0034 (§2.3), revisa de forma
  acotada una decisión de I-48 (V8-R05) y toma decisiones de arquitectura (DR-1 a DR-8). WORKFLOW §8 y el contrato
  §3.1 exigen que el ADR esté **aceptado antes de implementar**.
- **Número NO asignado.** Se asigna en G4 contra la numeración remota de ese momento. ADR-0035 es de I-50, ya
  **aceptado** e integrado en `main`; I-52 propone ADR-0036 e I-53 propone ADR-0037, cada una en su rama (§12.3). I-49
  no reserva ningún número de antemano.
- **No se escribe en G2F.**
- **Lo que tendrá que explicar de forma explícita**, tras G3:
  - **la extensión de ADR-0034 que exigen las fórmulas en fuentes de propiedad**: un tercer caso
    `Expression(BoundExpression)` junto a `Literal(T)` y `ProjectVariableReference(VariableId)` (ADR-0034 §4), por
    qué ID22B extiende también el lado de la propiedad que ADR-0034 §15 asociaba a ID21, cómo quedaría un futuro caso
    de ID21 y que la referencia directa se conserva con `=X` canónico;
  - las dos superficies de expresión y la revisión del contrato del editor de I-48, con `CASO_6` y `CASO_8` (P23.12);
  - el motor adimensional y las **tres fronteras de validez**: motor sin dominio, contrato raíz del `VariableType`
    igual para literal y expresión con sus casos A–D, y dominio del consumidor sobre toda fuente, con el
    endurecimiento declarado (§0.9, P8.10, P24.5);
  - la sintaxis de unidades y la autoridad neutral **limitada a las conversiones genéricas**, con las constantes de
    dominio fuera de ella y ADR-0005 intacto (P9);
  - el conjunto de funciones inicial y los candidatos diferidos (P10);
  - V-0, sus cuatro condiciones y la **regla de evolución** (§6);
  - la clasificación estructural/semántica, R1–R3 y el alcance del bloqueo (§5);
  - la sintaxis de referencias y la precisión sobre `#<GUID>` (§4);
  - los veredictos de `UnlinkAllAndDelete` y `RepairBrokenRack`, con el mensaje de recuperación condicional que no
    relaja R1 ni la decisión C4-13 de I-47 y cuyo «permitiría» no indica que R1 vaya a admitir la corrección (§7);
  - el enlace contra el snapshot mostrado (P7.7);
  - **la revisión ACOTADA de V8-R05**: el `PlanReadSet`, con y sin `RegistryMutation`, sin hash, token global,
    comparación completa, locks ni rechazo por cambios ajenos; es una extensión consciente de I-48 que **el Owner
    tiene que aceptar antes de implementar** (DR-8, P21.6). Tiene que fijar además la semántica de V5 (FR-1):
    - observaciones `(SymbolId, Before | After, resultado esperado)` derivadas de lo que el preflight realmente usó;
    - comparación en su fase y por resultado: valor exacto o fallo tipado estable, nunca texto;
    - un fallo idéntico admitido por R2 no aborta;
    - ninguna eliminación espera un valor «después» del símbolo eliminado;
    - y la de V6 (RR-1): cada fuente que retira `RepairBrokenRack` lleva una `RepairDecisionObservation`, recomputada
      con la misma inspección y comparada por razón estructurada (`MissingTarget`, `Intrinsic`, `Upstream`, `Domain`)
      antes de validar las fuentes conservadas, sin observar cada valor intermedio ni inventar símbolos para ids
      ausentes;
  - los límites normativos del árbol y la definición exacta de `MaxBoundExpressionDepth` (P1.9);
  - el mundo cerrado de los payloads de Expression, las tablas de tokens y la política de `ExtensionData` que no
    cambia (P17.6, P17.10, P17.12);
  - la preservación de C4-9 en las ramas que I-49 reescribe y el aislamiento de estado (P23.15–P23.16);
  - la gramática numérica invariante como excepción acotada a ADR-0015 (P1.11);
  - los namespaces y el ámbito reservados para ID20 (P25.2–P25.3);
  - las alternativas rechazadas (§10.1).
- **Relación con los ADR vigentes.** Extiende ADR-0034; no sustituye ADR-0005, ADR-0006, ADR-0012, ADR-0015,
  ADR-0021 ni ADR-0025. ADR-0034 está aceptado y es **inmutable**: no se edita.
- **Contrato de I-49.** Su §11.2 y su §12 prevén exactamente esta vía («salvo ADR nuevo aceptado por el Owner»). La
  implementación sigue bloqueada hasta que ese ADR esté aceptado, y el texto del contrato se alinea en G4.

---

## 10. Alternativas y coste

### 10.1 Alternativas consideradas

| # | Alternativa | Decisión en V6 |
|---|---|---|
| ALT-1 | Expresiones **solo** en `ProjectVariable.Definition` (DR-1 de V1) | **Rechazada**: contradice el requisito del Owner de dos superficies |
| ALT-2 | Un **`FormulaTextBox`** o control paralelo para fórmulas | **Rechazada** por requisito del Owner: se usa el mismo control de I-48 (P23.1) |
| ALT-3 | Cada expresión de propiedad como **variable implícita** del registro (opción b de Discovery §9.7) | **Rechazada**: llena el registro de variables que nadie creó, con identidad y ciclo de vida por rack, y hace que rename y delete dependan de propiedades |
| ALT-4 | Fuente de propiedad **solo `Expression`**, sin canonicalizar `=X` | **Rechazada**: `=X` tendría dos representaciones persistidas (P2.8) |
| ALT-5 | Persistir el **texto con nombres** | **Rechazada**: un rename rompería las expresiones y el nombre pasaría a ser autoridad (ADR-0034 §2, HANDOFF §4) |
| ALT-6 | Persistir **texto canónico con ids** en una cadena | **Rechazada**: acoplaría la persistencia a la sintaxis de §4 y exigiría un parser en cada lectura; el árbol JSON cerrado (P17.4, P17.6) no lo necesita |
| ALT-7 | **Cachear el valor evaluado** | **Rechazada**: segunda autoridad que puede quedar obsoleta (P17.5, P14.9) |
| ALT-8 | **Persistir el grafo** de dependencias | **Rechazada**: derivable, y persistido podría divergir (P12.1) |
| ALT-9 | **Sistema de tipos dimensional** (`Length \| Scalar`, `DimensionMismatch`), como proponía V1 | **Rechazada** por requisito del Owner |
| ALT-10 | Conversión de unidades vía **`StructuralSectionUnits`** | **Rechazada**: haría depender el núcleo de un helper de catálogo; la dirección es la inversa (P9.7) |
| ALT-11 | Un **parser de unidades por capa** (UI, evaluador, Domain) | **Rechazada** por requisito del Owner: un lexer y una autoridad (P1.10, P9.5) |
| ALT-12 | **`ROUND`, `CEILING` y `FLOOR`** ya en V6 | **Diferida**: semántica abierta (P10.6); confirmado por el Architect |
| ALT-13 | Versión **V-1** (minor por contenido) o **V-2** (major) | **Rechazada**: V-0 CLOSED (§6) |
| ALT-14 | Evaluador de **terceros** (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) | **Rechazada**: ADR-0012; semántica sensible a cultura o no acotada (P1.10) |
| ALT-15 | Núcleo **dentro** de `RackCad.Application.ProjectVariables` | **Rechazada**: arrastraría esa dependencia a ID23 (Discovery §16.3) |
| ALT-16 | **Borrado en cascada** o materialización automática de dependientes | **Rechazada**: reescribe en silencio definiciones del usuario (ADR-0034 §11; P20.2) |
| ALT-17 | Núcleo en un **ensamblado separado** | **Diferida**: frontera más dura, pero añade un proyecto a builds, CI y despliegue; V6 usa namespace + guarda (P26.5) |
| ALT-18 | Gramática **localizada** (`,` decimal y `;` separador) | **Rechazada**: doble significado de la coma (Discovery §11.7); mitigado con `AmbiguousDecimalComma` (P1.5) |
| ALT-19 | **`FatalUnevaluable`** no reparable para fallos semánticos de una fuente (V2) | **Rechazada** (A-01): deja un rack sin corrección posible |
| ALT-20 | **Materializar fórmulas** en `UnlinkAllAndDelete` (V2) | **Rechazada** (A-02): corta dependencias que nadie pidió tocar |
| ALT-21 | **Sin contrato en la raíz** de las variables definidas por expresión, con `A = -2` válido (remedio del Architect para A-03) | **Rechazada por el Coordinador** y aceptada así en la re-revisión: la validez dependería de la sintaxis (§0.9) |
| ALT-22 | **`VariableType` como álgebra** o dominio dentro del motor | **Rechazada**: el motor es adimensional (DR-5, P5.5) |
| ALT-23 | Enlazar el texto contra el **registro re-leído** (V2) | **Rechazada** (A-04): un nombre cruzaría una frontera entre lecturas (P7.7) |
| ALT-24 | Comparar en commit **solo los valores de `I`** (V2) | **Rechazada** (A-05): no cubre lo que el plan leyó (P21.6) |
| ALT-25 | Forma no canónica como **error estructural** (V2) | **Rechazada** (A-07): bloquearía todo el dibujo por un estado que la build entiende |
| ALT-26 | **Eliminar `ExtensionData` de forma global** | **Rechazada**: rompería la preservación histórica fuera de los payloads de Expression (P17.12) |
| ALT-27 | **Operación o comando nuevo** de reparación de fórmulas | **Rechazada**: `RepairBrokenRack` basta con el conjunto ampliado (§7.2) |
| ALT-28 | **`#<GUID>` sin nombre como entrada** que enlaza con un id presente | **Rechazada**: esa forma es la presentación de un estado roto (§4.2, regla 11); precisión aceptada en la re-revisión |
| ALT-29 | **Fragmento de GUID** como cualificador (8 caracteres) | **Rechazada**: dos ids con el mismo primer grupo colisionarían (P3.9) |
| ALT-30 | **Arreglar `WithDesign`** en el portador para no compartir `PropertyValues` | **Rechazada en I-49**: el portador es de I-50 y el aislamiento se consigue en la frontera de I-49 (P23.16); L3 queda registrado |
| ALT-31 | **Centralizar todas las constantes que valen 12** en la autoridad de unidades (ampliación de A-13 en V3) | **Rechazada** (CR-1): `FootInches`, `CommercialFoot` y el `12.0` de Cantilever son reglas o notaciones de dominio; la ampliación tocaba sistemas fuera de alcance, añadía una colisión con I-50 y exigía una guarda inverificable (P9.6–P9.7) |
| ALT-32 | Limitar el **anidamiento por paréntesis o por texto** en lugar de la profundidad del `BoundExpression` | **Rechazada** (CR-3): no cuenta las cadenas binarias y deja pasar árboles no serializables (P1.9) |
| ALT-33 | **Aviso de recuperación incondicional** en la reparación | **Rechazada** (CR-4): puede ser falso mientras R1 bloquea la corrección (§7.2) |
| ALT-34 | **Relajar R1** para corregir una variable mientras un rack que la consume sigue con otras fuentes inválidas | **Rechazada** (CR-4): reabriría la decisión C4-13 de I-47 (§2.3, P30.24) |
| ALT-35 | **Concurrencia optimista general**: hash de snapshot, token global, comparación completa del registro o locks | **Rechazada** (CR-5): la revisión de V8-R05 queda acotada al `PlanReadSet` (P21.6, P30.12) |
| ALT-36 | Presentar el `PlanReadSet` como compatible con **V8-R05 intacta** | **Rechazada** (CR-5): es una revisión acotada y tiene que declararse (§2.3, §9) |
| ALT-37 | **`PlanReadSet` como lista plana** de valores esperados de «después», con aborto si cualquier símbolo falla (V4) | **Rechazada** (FR-1): contradice R2, y haría abortar siempre `Delete`, `UnlinkAllAndDelete` y el `Rename` de una variable en error (P21.6) |
| ALT-38 | **Meter todo `I` en las dos fases** para cualquier operación | **Rechazada** (FR-1): crea lecturas que el preflight no usó, como un `After(X)` de un símbolo eliminado (P21.6) |
| ALT-39 | **Comparar el texto de los diagnósticos** en commit | **Rechazada** (FR-1): el texto localizado no es estable; se comparan código y datos estructurados (P21.6) |
| ALT-40 | **Volver a ejecutar el preflight completo** en commit (R1, R2 y contrato raíz sobre la re-lectura) | **Rechazada**: sería una segunda autoridad de decisión y una revisión de V8-R05 más amplia que la acotada; comparar las observaciones basta para saber si lo que el preflight usó sigue igual (P21.6, P30.12, ALT-35) |
| ALT-41 | **Observar también las dependencias transitivas** de cada símbolo observado, como enumeraba V4 | **Rechazada** (FR-1): el resultado observado ya detecta todo cambio de una dependencia que altere lo que el plan usó; observarlas aparte abortaría por cambios que no tocan nada de lo decidido o escrito (P21.6) |
| ALT-42 | **Observar mecánicamente los valores de todas las dependencias** de una fuente que la reparación retira | **Rechazada** (RR-1): abortaría por cambios que no alteran la razón reparable y reintroduciría la observación transitiva que V5 excluyó (ALT-41, T-V6-08) |
| ALT-43 | **Dejar fuera de alcance la premisa de la reparación** | **Rechazada** (RR-1): el commit podría borrar una fórmula que ya volvió a ser válida (P21.6, T-V6-02, T-V6-07) |
| ALT-44 | **Modelar un id ausente como símbolo** para observarlo | **Rechazada** (RR-1): inventaría un símbolo inexistente; la ausencia viaja en `MissingTarget(ids)` (P21.6, T-V6-04, T-V6-05) |
| ALT-45 | **Volver a ejecutar `RepairBrokenRack` completo** en commit | **Rechazada** (RR-1): re-ejecutaría el preflight (ALT-40); basta con recomputar la inspección de las fuentes que se retiran (P21.6) |

### 10.2 Coste estimado (INFERENCE, se valida gate a gate)

| Área | Superficie estimada | Base |
|---|---|---|
| Núcleo nuevo | ≈14–20 archivos en `RackCad.Application.Expressions`, más la autoridad de unidades | P1–P16, P9.5 |
| Project Variables y persistencia | ≈22–28 archivos existentes: las ubicaciones que asumen literal; sesión, fuente, estado, opciones y reconciliador del editor; inspección, kernel y sonda; descubrimiento, preflight, commit, plan, workspace e intents; `SelectivePropertyValueDocument`; `VariableType` | Discovery §5.5, §9.7, §3.1–§3.2 |
| Selectivo en Application | ≈4: resolver, apertura del editor, exportación y descriptores con dominio | Discovery §3.2; P24.5 |
| Alias de unidades | 1 archivo: `StructuralSectionUnits` (dos alias) | P9.7 |
| Plugin | 0–3, incluida la invocación del `PlanReadSet` en planes sin `RegistryMutation` | Discovery §3.3; P21.6 |
| UI | ≈4: `LinkedPropertyEditor` (inserción de candidatos), `RackProjectVariablesWindow.xaml` y `.cs` (campo y panel), texto de reparación; `RackSelectiveWindow`, un miembro | Discovery §3.4; P23.13 |
| Pruebas | Núcleo nuevo, ≈20 archivos existentes, la evolución de P28.4, los 21 escenarios de P28.5, los 11 de P28.6, los 9 de P28.7 y los 9 de P28.8 | Discovery §14 |
| Comandos / ventanas nuevos | 0 / 0 | P22.1, P23.14 |

**Costes que no se minimizan:**

- Nace la **primera** ruta productiva nombre→id, en dos superficies y con una sintaxis de identidad (P7, §4).
- Se revisa el contrato del editor de I-48 que el Owner validó: `CASO_6` y `CASO_8` (P23.12).
- El panel de diagnóstico y la confirmación de reparación de RACKVARIABLES crecen, con un mensaje de recuperación que
  exige clasificar las fuentes de los racks del cierre (P22.9, §7.2).
- El commit compara las observaciones del `PlanReadSet` con hasta dos evaluaciones, la de la re-lectura para las
  `Before` y la del documento cambiado para las `After`, también en planes sin `RegistryMutation`; eso revisa V8-R05
  (P21.6). En `RepairBrokenRack`, además, recomputa la inspección de cada fuente que retira (RR-1).
- Con R1 intacto, un rack con dos causas raíz independientes solo sale por reparación (§7.2).
- Una referencia directa persistida a una variable negativa, solo alcanzable por edición externa, deja de resolver
  (P24.5).
- En una build anterior, un rack con expresión bloquea en todo el dibujo las operaciones de variables que necesitan
  descubrimiento (P18.2).
- La propagación sobre dibujos grandes sigue **sin medir** hasta G11.
- Evolucionan varias guardas y pines vigentes, y nacen guardas nuevas (P28.4).
- `RackSelectiveWindow`, que I-50 ya modificó e integró en `main` e I-53 prevé tocar en su G5, necesita un cambio
  (P23.13, §12).

---

## 11. Reconciliación con el Discovery G1

### 11.1 Las 23 preguntas abiertas (Discovery §18)

| # | Pregunta del Discovery | Respondida en |
|---|---|---|
| 1 | Dónde vive una expresión | DR-1; P2.7–P2.8; P23; P24 |
| 2 | Representación persistida | P4; P17.2–P17.4; ALT-5, ALT-6 |
| 3 | Versión del registro | P18; §6 |
| 4 | Token y payload del nuevo kind; `ExtensionData`; `PropertyNameCaseInsensitive` | P17.2–P17.4, P17.6, P17.10, P17.12 |
| 5 | Evaluación: dónde, cuándo y si se persiste | DR-3; P14.2, P14.7–P14.9; P17.5; P21.2, P21.6 |
| 6 | Grafo y ciclos | P12; P13; P15.3 |
| 7 | Propagación transitiva | P21 |
| 8 | Ciclo de vida con dependientes | P19; P20; P21; §5; §7 |
| 9 | Tipos | DR-5; P5.4–P5.7; P8.7–P8.10 |
| 10 | Unidades | P1.6; P9 |
| 11 | Gramática numérica | P1.4–P1.5, P1.11; P22.3 |
| 12 | Nombres dentro de expresiones | P7; §4 |
| 13 | Editor | P23 |
| 14 | RACKVARIABLES | P22 |
| 15 | ID20 (preparar) | P5.2–P5.3; P25 |
| 16 | ID23 (independencia) | DR-2; P26 |
| 17 | ID28/ID29 (preservar) | P16.3; P27 |
| 18 | Guardas de fuente | P28.4 |
| 19 | Compatibilidad de DWG I-47/I-48 | P18.2; P24.11; P28.3 (G8); P28.5 (T-V3-21); P29; §8.3 (17) |
| 20 | ADR | §9 |
| 21 | Alcance de sistemas y propiedades | DR-2; P28.4 (G17); P30.15; P30.23 |
| 22 | Semántica numérica del resultado | P14.4; P8.10; P21.7; P24.5 |
| 23 | Hallazgos laterales L1–L5 | P30.17; G4 |

### 11.2 Los 16 riesgos (Discovery §17)

| Riesgo | Tratamiento en V6 |
|---|---|
| R1 — legibilidad en builds anteriores | V-0 con fail-closed por kind y mundo cerrado del payload (§6); coste declarado; caracterización en G8 y verificación con el DLL de `a4d88f1` (§8.3, punto 17) |
| R2 — segunda autoridad de valor | `RegistryEvaluation` como único dueño; expresiones de propiedad dentro del resolver único; sin valores cacheados (P14.9; P17.5; P24.5) |
| R3 — guardas de fuente | Evolución explícita, en el mismo gate y sin renombrar para esquivarlas; guarda de unidades acotada a papeles, no a números (P28.4) |
| R4 — profundidad 1 embebida | Cierre transitivo, consumidores de cualquier variable afectada, un resolve por rack y un plan (P21.2) |
| R5 — coste sin medir | Descubrimiento por conjunto en un barrido (P21.4); grafo `O(V + E)` (P12.6); medición en G11 |
| R6 — nombres en fórmulas | Sintaxis cerrada (§4); binder como única ruta nombre→id, solo al comprometer y solo contra el snapshot mostrado (P7) |
| R7 — gramática numérica divergente | Gramática invariante con `AmbiguousDecimalComma`; los literales sin `=` conservan su regla (P1.4–P1.5, P1.11); convergencia fuera de alcance (P30.18) |
| R8 — unidades | `[mm]`, `[in]` y `[ft]` como conversión numérica con una autoridad neutral limitada a las conversiones genéricas (P9) |
| R9 — acoplamiento al Selectivo | Núcleo y autoridad de unidades neutrales, sin `VariableType` en el núcleo, protegidos por guarda (DR-2; P5.1; P26) |
| R10 — semántica actual de `=` | **Revisada por requisito del Owner**, con las garantías de I-48 preservadas y la lista exacta de pines que cambian (P23.2, P23.12) |
| R11 — pérdida de trazabilidad | `BoundExpression`, `SymbolId`, diagnósticos con causa raíz, conjuntos afectado y leído y traza opt-in en memoria (P27) |
| R12 — colisión potencial con I-50 | Sin tocar `WithDesign` ni el portador; aislamiento en la frontera de I-49; `RackSelectiveWindow` limitado a un miembro; ninguna modificación de constantes de dominio, así que desaparece la colisión por `FootInches` (P17.11; P23.13; P23.16; P9.7; §12) |
| R13 — defectos laterales heredados | Fuera de alcance, con registro en G4, salvo la preservación de invariantes en ramas que I-49 reescribe: L2 (P23.15) y el token `Type` (P17.10) |
| R14 — extensión muerta | Pruebas por el camino operativo store → acreditación → evaluación → resolver → geometría y BOM, en las dos superficies (P28.3, G8 y G11) |
| R15 — caminos heredados sin disparador | No se extienden (P29.6) |
| R16 — materializar valores calculados | Solo en `UnlinkAllAndDelete` (referencias directas) y exportación, con el valor evaluado exacto (P21.9) |

---

## 12. Coordinación con las iniciativas activas

**Observado al abrir G2F** (`git fetch --all --prune`, 2026-09-12) y verificado de nuevo justo antes del commit:

```text
origin/architecture/motor-expresiones-parametricas = 2783accd0c9400ed1308ab5c40072eadcd388f36  (= Proposal V5)
origin/main                                        = f8deb675c6d1ef0e64693b157d69c4cc170d7b24  (merge de I-50)
I-50  integrada en main (f8deb67 = 46fcac2 + c9a5f0a); rama retirada
I-51  integrada en main (46fcac2); rama retirada
I-52  origin/feature/rackmirror-espejo-semantico     = 04457183fc2d7dcda5d6c1b988a4789ed4ea2f8f  (solo docs)
I-53  origin/feature/cabeceras-configurables-multidestino = d7f17addb47ea943b61ff50b4b4a5b23010d6fab  (solo docs)
I-54  origin/architecture/propiedades-personalizadas = 36c337b84c47f9ac7c97d860fce42d3a5f4370e8  (solo docs)
```

No hay más ramas remotas. Durante G2F, I-50 cerró su documentación (`c9a5f0a`) y quedó integrada en `main`
(`f8deb67`), e I-52 publicó su Proposal V2 (`0445718`). Ninguna novedad invalida una premisa de V6 ni obliga a cambiar
su contrato; en particular, la coordinación sobre `RackSelectiveWindow` no cambia RR-1. Lo que sigue es coordinación
para el preflight productivo y para la integración serializada.

### 12.1 I-50 (integrada en `main`)

- **FACT** — I-50 está integrada en `main` con el merge `f8deb67` («Merge I-50: cotas independientes por tipo de
  vista», padres `46fcac2` y `c9a5f0a`), y su rama remota ya no existe. El merge añade a `main` 64 archivos: 26 de
  `src/`, 26 de pruebas y 12 de `docs/`, con ADR-0035 aceptado
  (`docs/adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md`).
- **FACT** — Cambió cuatro archivos que V6 cita en `a4d88f1`: `RackSelectiveWindow.xaml` y `.xaml.cs`,
  `SelectivePalletDesignDocument.cs` (`DimensionViews`, C-06) y `SelectiveGeometryResolver.cs` (C-04). Ninguna premisa
  de V6 cambia:
  - `Describe(PropertyId, string)` sigue usando `Text.TrimStart('=')` (`RackSelectiveWindow.xaml.cs:2582,2600` en
    `f8deb67`; `:2537` en `a4d88f1`), así que P23.13 sigue siendo un cambio de un miembro;
  - `FootInches = 12.0` sigue local en `SelectiveGeometryResolver.cs:31`, así que CR-1 no cambia;
  - `PropertyValues` sigue siendo el mismo diccionario del DTO (`SelectivePalletDesignDocument.cs:138` en `f8deb67`),
    y `DimensionViews` es un miembro hermano: V-0 y el aislamiento de P23.16 no cambian.
- **FACT (doc de I-50)** — Su Proposal V1.2, congelada y ya en `main`, declara que **no** se tocan `WithDesign`, el
  reconciliador, `LinkedPropertyReconciler`, `SelectiveEffectiveDesignResolver`, `SelectiveAuthoredAuthority`,
  `SelectiveLibraryExport`, `RackEnvelopeRestamp`, `src/RackCad.Plugin`, `LinkedPropertyEditor`, Project Variables ni
  Expression Engine, y que ninguna `SchemaVersion` cambia
  (`docs/initiatives/I-50-proposal-v1.2.md:58-59,175,302-304,464-468`).
- **Consecuencias para I-49:**

  | Archivo | I-49 V6 | I-50 (en `main`) | Tratamiento |
  |---|---|---|---|
  | `RackSelectiveWindow.xaml.cs` | Un miembro, `Describe` (P23.13), en G10 | Integrado, sin tocar `Describe` ni los editores vinculables | I-49 reconcilia la rama con `main` antes del primer gate productivo y edita en G10 sobre la versión integrada; serialización con el G5 de I-53 (§12.3) |
  | `RackDynamicSystemWindow.xaml/.cs`, `RackPushBackSystemWindow.xaml/.cs` | **Ninguno** (P30.15) | Integrados | Sin cruce |
  | `SelectivePalletDesignDocument.cs` | **Ninguno**: V-0 y aislamiento en la frontera de I-49 (P23.16) | Integrado | Sin cruce. Las citas de V6 a `:316-330` y `:321` son de `a4d88f1`; en `main` se desplazan |
  | `SelectiveGeometryResolver.cs` | **Ninguno** (CR-1) | Integrado | Sin cruce |
  | `LinkedPropertyReconciler.cs`, `SelectiveEffectiveDesignResolver.cs`, `SelectiveAuthoredAuthority.cs`, `SelectiveLibraryExport.cs` | Sí (P23.15–P23.16, P24) | No cambiaron | Sin cruce. I-50 depende del camino `WithDesign → From(design)`, que V6 conserva sobre el clon |
  | `SelectivePropertyValueDocument.cs`, editor vinculable, Project Variables y núcleo | Sí | No cambiaron | Sin cruce |
  | `StructuralSectionUnits.cs` | Dos alias (P9.7), en G6 | No cambió | Sin cruce |

- **Prueba cruzada**: I-49 es la segunda en integrar, así que le toca añadir la prueba en la que un Selectivo con una
  fuente `expression` y `DimensionViews` presentes sobrevive a RACKEDITAR con los dos intactos.
- **`OWNER_OVERRIDE_I49_I50_PARALLEL`**: con I-50 integrada, el procedimiento de colisión contra su rama se sustituye
  por la reconciliación con el `main` que ya la contiene. G2F solo crea este documento, así que la condición de
  colisión **no se activa**.

### 12.2 I-51, I-50 y `main`

- **FACT** — I-51 está **INTEGRADA y CERRADA** en `main` (merge `46fcac2`), con su rama remota retirada. Su producción
  integrada (`RackDuplicationPlan.cs`, `RackDuplicarCommands.cs`, `RackEnvelopeRestamp.cs` y tres archivos de
  pruebas) no la cita ni la modifica V6.
- **FACT** — Desde V5, `main` avanzó una vez: el merge de I-50 (`f8deb67`, §12.1).
- **FACT (HANDOFF de `main`)** — Registra que «**I-49** —motor de expresiones— declara que duplicar y re-estampar no
  cambian» (`docs/HANDOFF.md:1564-1570` en `46fcac2`). V6 lo mantiene (P24.8).
- **Divergencia declarada**: la rama de I-49 va 9 commits por delante de `main`, contando este documento, y 30 por
  detrás tras el merge de I-50. WORKFLOW §4.2 pide rebasar al abrir una sesión cuando el trunk avanzó; esta ejecución,
  solo documental como G2D y G2E, **no** rebasa, porque un documento nuevo no necesita el rebase para continuar. La
  reconciliación con `main` queda pendiente antes del primer gate productivo y, en todo caso, antes de escribir la
  prueba de P24.8, que tiene que ejercitar el restamp integrado, y la prueba cruzada con `DimensionViews` (§12.1).
- **Condición de parada**: si un gate de I-49 necesitara cambiar la API o la semántica de `RackDuplicationPlan`, del
  restamp, de `SelectiveAuthoredAuthority`, del portador, del store del Selectivo o de la política de cotas de I-50,
  se detiene **antes de editar** y lo reporta (contrato §12): ya es código de `main` validado por el Owner.

### 12.3 I-52, I-53 e I-54 (solo documentación)

| Iniciativa | Estado observado | Cruce con I-49 | Tratamiento |
|---|---|---|---|
| I-52 — RACKMIRROR, espejo semántico | G2: Proposal V2 y ADR-0036 corregido, en estado `propuesto`, en su rama (`0445718`) | Conserva las entradas `expression` como portador, sin reimplementar su evaluación; si I-49 integra antes, RACKMIRROR invoca la autoridad efectiva integrada (PV-6, T-M19). Su fachada de G4 toca `RackDuplicationPlan.cs` con aviso previo a I-49, cuya condición de parada lo cubre (`docs/initiatives/I-52-proposal-v2.md:414-415,1054,1242,1317` en su rama) | Coordinación: el espejo conserva `expression` sin interpretarla, igual que la duplicación (P24.8). Si `RackDuplicationPlan` cambia en `main` antes del primer gate productivo de I-49, la prueba de P24.8 se escribe contra esa versión (§12.2). Su G7 añade RACKMIRROR y lleva el censo de comandos de 33 a 34 (`docs/initiatives/I-52-proposal-v2.md:96,1320`): I-49 sigue sin añadir comandos (P22.1), y si I-52 integra antes, sus guardas de censo se escriben contra el censo vigente al rebasar. Sin cambio de contrato |
| I-53 — cabeceras configurables multidestino | G2B: Proposal V2 y ADR-0037 en estado `propuesto`, en su rama (`d7f17ad`) | Vedaba editar `RackSelectiveWindow.xaml/.cs` mientras I-50 no se integrara (ya integrada), y registra el cruce futuro con el `Describe` que I-49 prevé en su G10 como archivo caliente que exige serialización (`docs/initiatives/I-53-proposal-v2.md:62-63,859,883,892` en su rama) | Sin cruce de contrato. Coordinación en `RackSelectiveWindow.xaml.cs` entre I-53 (G5) e I-49 (G10), sobre la versión que integró I-50. Cada gate ejecuta su procedimiento de colisión antes de editar, y la integración es serializada |
| I-54 — propiedades personalizadas (ID24) | G2C: Proposal V2 (`36c337b`) | Mantiene el punto de extensión hacia expresiones solo como restricción, sin sintaxis, tokens, namespaces ni ámbitos, y sin dependencia de I-49; respeta la reserva de `Rack`/`Project` para ID20, y su miembro del sobre es hermano de `Design`, así que no suma presupuesto de profundidad JSON con P1.9 (`docs/initiatives/I-54-proposal-v2.md:114,705-712,1172` en su rama) | Sin cruce: la gramática de §4 ya reserva `palabra.` como sintaxis de namespace. Un namespace futuro para ID24 exigiría su propio ADR |

### 12.4 Conflictos documentales previstos

- I-51 e I-50 ya integraron sus filas de ROADMAP y sus registros en `docs/ideas-futuras.md`. I-49, I-52, I-53 e I-54
  insertan las suyas sobre el `main` vigente.
- I-49 registrará sus seguimientos en `docs/ideas-futuras.md` en G4.
- Numeración de ADR: 0035 es de I-50, aceptado e integrado en `main`; I-52 propone 0036 e I-53 propone 0037 en sus
  ramas; I-49 e I-54 no reservan número (§9). `docs/adr/README.md` recibirá sus filas en orden numérico según integren.
- Precedente, solo como hecho de coordinación: I-50 tampoco cambia ninguna `SchemaVersion` sobre el mismo portador
  (P18.7).

La integración es **serializada**, y cada iniciativa que integre después se reconcilia con `main`.

---

## 13. Condiciones para detenerse

Se heredan todas las del contrato §12. Sobre la que dice «salir del punto de extensión de ADR-0034 §15: detenerse; eso
es ADR nuevo y decisión del Owner», V6 deja constancia de que **la decisión del Owner existe** (encargo de G2A.1) y de
que **el ADR es obligatorio** antes de cualquier producción (§9). Además:

- Si la comprobación final exige **cambiar** una invariante de I-47/I-48 más allá de lo que enumera §2.3: detenerse; eso
  es ADR y decisión del Owner.
- Si el ADR no formaliza la **revisión acotada de V8-R05** o el Owner no la acepta: no se implementa el `PlanReadSet`
  ni ningún gate que dependa de él; detenerse y escalar (DR-8, P21.6).
- Si la implementación del `PlanReadSet` no pudiera derivar sus observaciones de lo que el preflight realmente usa, o
  necesitara abortar por un fallo idéntico admitido por R2, esperar un valor «después» de un símbolo eliminado o
  comparar texto de diagnósticos: detenerse; contradiría FR-1 (P21.6, P30.26).
- Si la implementación no pudiera recomputar la decisión de reparación con la misma inspección pura sobre la misma
  entrada de fuente, o necesitara comparar su texto, observar cada valor intermedio o inventar un símbolo para un id
  ausente: detenerse; contradiría RR-1 (P21.6, P30.26).
- Si el núcleo o la autoridad de unidades no pueden quedar **independientes** de Project Variables y de
  `StructuralSections` (P26.1): detenerse.
- Si la autoridad de unidades exigiera modificar constantes de dominio (P9.6): detenerse; está fuera de alcance
  (P30.23).
- Si la evolución de una guarda debilitara la protección de **ID21** (P28.4): detenerse.
- Si aceptar expresiones en el editor exigiera renunciar a alguna garantía de P23.2: detenerse. No se relaja una
  garantía para acomodar la implementación.
- Si el contrato raíz no pudiera aplicarse igual a literal y expresión, con sus casos A–D (P8.10): detenerse. La
  validez no se relaja por la sintaxis.
- Si una corrección exigiera un comando nuevo de reparación, convertir una fórmula en literal de forma implícita o
  relajar R1 (§7): detenerse.
- Si la medición de G11 muestra un coste de propagación inaceptable: detenerse y escalar.
- Colisión productiva material con una iniciativa activa, o necesidad de cambiar código integrado por I-50 o I-51:
  detenerse **antes de editar** (§12).
- Cualquier edición de producción antes de G3 y del ADR aceptado en G4: **prohibida**.

---

## 14. Estado

```text
COORDINATOR PROPOSAL V6 — ARCHITECT FINAL RE-REVIEW V5 RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Proposal Version = V6
Coordinator      = AGREED WITH V6
Architect        = PENDING FINAL CHECK

CR-1..CR-5       = CLOSED
FR-1             = CLOSED
RR-1             = CLOSED IN V6

A-03             = CLOSED
Open A           = CLOSED
Open B           = CLOSED
Schema           = V-0

ADR              = REQUIRED, not yet written
Implementation   = BLOCKED

V5 → Architect Final Re-review → V6 Reconciliation (§0.1):
  RR-1 LOW     RepairDecisionObservation {RackId, PropertyId, ExpectedRepairReason} por cada fuente
               que retira RepairBrokenRack; misma inspección pura (InspectBinding) recomputada en
               commit antes de validar las fuentes conservadas; razones MissingTarget(ids),
               Intrinsic, Upstream y Domain comparadas sin texto; sin observación transitiva     ACEPTADO · CERRADO
  N-1          vocabulario opcional («indica»), sin cambio de semántica                            APLICADO

Re-revisión final de V5 conservada (§0.2):
  FR-1 = SATISFIED · regla transitiva = ACCEPTED · N-1 = SATISFIED · sin regresiones en decisiones cerradas

V4 → Architect Final Review → V5 Reconciliation (§0.4, heredada):
  FR-1 MEDIUM  PlanReadSet = observaciones (SymbolId, Before | After, resultado esperado);
               aborta solo si un resultado observado difiere del esperado;
               un fallo idéntico admitido por R2 no aborta; Delete sin observaciones;
               UnlinkAllAndDelete con Before(X) y sin After(X)                   ACEPTADO · CERRADO
  N-1          «permitiría» no indica que R1 vaya a admitir la corrección; R1 intacto
                                                                                 ACEPTADA · INCORPORADA

Revisión final de V4 conservada (§0.5):
  CR-1..CR-5 = SATISFIED · decisiones cerradas coherentes · coordinación paralela sin cambio de contrato

V3 → Architect Re-review → V4 Reconciliation (§0.7, heredada):
  CR-1 MEDIUM  autoridad de unidades solo con conversiones genéricas;
               FootInches, CommercialFoot y 12.0 de Cantilever locales          ACEPTADO · CERRADO
  CR-2 LOW     contrato raíz con casos A–D; Failed → Success debe cumplir       ACEPTADO · CERRADO
  CR-3 LOW     MaxBoundExpressionDepth = 24 con definición normativa            ACEPTADO · CERRADO
  CR-4 LOW     mensaje de recuperación condicional; R1 intacto                  ACEPTADO · CERRADO
  CR-5 LOW     PlanReadSet como revisión ACOTADA de V8-R05; exige Owner         ACEPTADO · CERRADO

Re-review V3 conservada (§0.8):
  A-01…A-12, A-14…A-18 = SATISFIED · A-13 = ACCEPTED sin ampliación (CR-1)
  A-03 = ACCEPTED · P1 #GUID = ACCEPTED · P6 token Type = ACCEPTED

Veredictos cerrados (§0.11):
  OPEN A          §4   sintaxis del Architect; #<GUID> sin nombre solo para referencias rotas
  OPEN B          §5   estructural ≠ semántico; bloqueo por cierre afectado; R1–R3
  Schema          §6   V-0 con cuatro condiciones y regla de evolución
  Operaciones     §7   UnlinkAllAndDelete bloquea fórmulas y dependientes;
                       RepairBrokenRack única, conjunto semántico ampliado, confirmación por fuente,
                       mensaje de recuperación condicional, premisas de reparación protegidas en commit (RR-1)
  Funciones       P10  MIN, MAX, ABS; ROUND, CEILING, FLOOR diferidas
  Unidades        P9   [mm] [in] [ft] como conversión numérica; autoridad solo de conversiones genéricas
  ID20 / ID23     P25 / P26   validados (ID23 con A-06)
  ID28 / ID29     P27  BoundExpression + diagnósticos + traza opt-in / dependencias + dependientes + conjuntos
  PlanReadSet     P21.6  observaciones Before/After y decisiones de reparación; comparación por resultado
                         o por razón estructurada; revisión acotada de V8-R05

30 puntos      = P1..P30 (§3)
Pruebas        = 21 escenarios de G2C (P28.5) + 11 de G2D (P28.6) + 9 de G2E (P28.7) + 9 de G2F (P28.8)
Gates          = G0..G12 (§8): G0 y G1 HECHAS · G2 EN CURSO (V6) · G3 PENDIENTE · G4..G12 NO AUTORIZADAS

Base: Proposal V5   @ 2783accd0c9400ed1308ab5c40072eadcd388f36
      Proposal V4   @ ccf21c69d9aa6e8d9b622e6389fb61f430d53f03 (historial)
      Proposal V3   @ 4df9480638bbeb9a95549a690db098b7f32a28ea (historial)
      Proposal V2   @ 9aef7d04d9e65b4225af2cb36750ee636a6689d5 (historial)
      Proposal V1   @ b15e40a076d7157ce8ca73537af9659049bf8576 (historial)
      Discovery G1  @ 4cf02b167f183fb66d93988c9843296838277dc4

Siguiente paso: comprobación final del Architect sobre V6. G2F no la solicita y no autoriza implementación.
```

**Historial conservado:** [V5](I-49-proposal-v5.md), [V4](I-49-proposal-v4.md), [V3](I-49-proposal-v3.md),
[V2](I-49-proposal-v2.md) y [V1](I-49-proposal-v1.md) quedan **intactas**.
