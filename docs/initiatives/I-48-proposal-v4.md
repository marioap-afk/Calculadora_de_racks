# I-48 — Proposal V4: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V4 — THIRD ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V4
> Architect   = NOT REVIEWED V4
> Consensus   = NOT REACHED
> ```
>
> **V4 reconcilia la TERCERA Architect Review, la de V3.** Esa review termino:
>
> ```
> Architect = NOT AGREED
> BLOCKER  = 0
> MATERIAL = 2   (incompatible target cae en healthy;
>                 G4B no puede ejercitar RepairBrokenRack multi-broken)
> MINOR    = 3   (mensaje de InvalidDraft; fuga de metrica hacia atras; evidencia RED)
> AGREE RepairBrokenRack rack-scoped
> AGREE R-02 remains
> ```
>
> **Primera ronda sin bloqueantes.** La revision de V4 **no ha ocurrido**.
>
> ```
> Base:              b03a7ae81f1a28ce4e3117b3eddd02fb9906c0a2  (Proposal V3 revisada)
> Cadena previa:     7f6680d (V2 revisada) · a061148 (V1.1 revisada) · edacf7d (G1) · 7a9471f (G1.1)
> Codigo auditado:   e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:         I-48-proposal-v1.md, -v2.md y -v3.md quedan INTACTAS
> Estado de gates:   G0 · G1 · G1.1 · G2A/.1/.2 · Review V1.1 · G2B · Review V2 · G2C · Review V3
>                    · **G2D EN CURSO** · G3 consenso PENDIENTE
> ```

## 0. Decisiones CERRADAS que V4 no reabre

- **`AGREE RepairBrokenRack rack-scoped`** — el Arquitecto **retiro** su `PC2-1` parcial tras verificar
  que el executor consume `rack.EffectiveOutput` para redibujar. **La solucion rack-scoped de V3 queda
  CERRADA y no se reabre.**
- **`AGREE R-02`** se mantiene (catalogo inmutable Selective-specific; un descriptor puede viajar como
  valor). `V4-R02` solo precisa la costura de prueba; no lo contradice.
- Se conservan sin cambio: `V3-R05` (`P` tras la authority), `V3-R06`/`R07`/`R08`/`R09` (UI y tipos),
  `V3-R11`, `V3-R12`, `V3-R14`, `R-03`/`R-04` de V2 (estado declarativo y 20.13), y todo `D-01`..`D-18`
  que ninguna `R` toque.

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review de V3 | Severidad | Resuelto en |
|---|---|---|
| **`incompatible target` caeria en `healthy`** | MATERIAL | **`V4-R01`** |
| **`G4B` no puede ejercitar `RepairBrokenRack` multi-broken; `V3-R10` bloquea la costura** | MATERIAL | **`V4-R02`**, **`V4-R03`** |
| Mensaje de bloqueo para referencia en `InvalidDraft` | MINOR | **`V4-R06`** |
| Fuga de metrica hacia atras | MINOR | **`V4-R04`** |
| Evidencia RED de `G4E` | MINOR | **`V4-R05`** |
| `OQ3-1` — que muestra el campo con draft pendiente | (abierta) | **`V4-R07`** |
| `OQ3-2` — agrupacion de `FindBroken` | (abierta) | **`V4-R08`** |
| `OQ3-3` — firma del intent de Repair | (abierta) | **`V4-R09`** |

**Los dos materiales y los tres menores quedan aceptados. V4 no introduce ningun apartamiento.**

---

## 2. `V4-R01` — Inspeccion de bindings y compatibilidad de tipo

**Se acepta MATERIAL-1 y se refuerza.** La review demostro que `SelectiveEffectiveDesignResolver`
**no comprueba el tipo** —su ultima linea es `value = variable.Definition.LiteralValue;` (l.132)— y que
por tanto una clasificacion derivada de «¿resolvio?» etiquetaria un binding de tipo incompatible como
**`healthy`**, que es el bucket mas peligroso de los tres.

**Queda prohibida una clasificacion exhaustiva de solo `healthy / repairable / fatal` sin nombrar la
causa.** La primitiva property-specific de Application debe distinguir **conceptualmente**:

| Estado | Condicion | Disposicion |
|---|---|---|
| **`HEALTHY`** | propiedad conocida · reference valida · target existe · **`target.Type == descriptor.VariableType`** | resuelve |
| **`REPAIRABLE_MISSING_TARGET`** | el `VariableId` del target **no existe** | **UNICA** categoria que entra automaticamente en `B` |
| **`FATAL_INCOMPATIBLE_TARGET`** | el target **existe** pero **`target.Type != descriptor.VariableType`** | **abort / zero plan** |
| **`FATAL_UNKNOWN_PROPERTY`** | `PropertyId` desconocida | **abort / zero plan** |
| **`FATAL_MALFORMED_REFERENCE`** | kind desconocido · `VariableId` ilegible · payload invalido | **abort / zero plan** |

**Todo `FATAL` es `abort / zero plan`. Sin fallback al stored literal.**

### La comprobacion de tipo NO pertenece solo a Repair

Esta es la parte que **extiende** lo que la review pidio, y se declara como extension para que la
proxima revision la contraste con los invariantes:

> **La inspeccion/resolucion property-specific que usa el effective resolver tambien debe exigir
> `variable.Type == descriptor.VariableType`.** Una variable **existente pero incompatible** **NUNCA**
> puede resolver effective ni alimentar geometry/BOM.

En la practica esto **anade un sexto modo de fallo** a la taxonomia del resolver, junto a
`UnknownPropertyId`, `UnknownReferenceKind`, `MalformedReference` y
`BrokenProjectVariableReference`. Es estrictamente **mas** fail-closed que hoy y no contradice
[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md), que no dice nada sobre el tipo en
el momento de resolver.

### Una sola nocion de compatibilidad

**La clasificacion NO puede derivarse de `Resolve(document).IsSuccess` ni de «el `VariableId`
existe».** Debe existir una **primitiva/autoridad property-specific comun**, suficiente para que
**resolver, `FindBroken` y Repair no desarrollen tres nociones distintas de compatibilidad**. Es el
mismo principio que ya gobierna la resolucion: **un solo hogar**.

**No se fija todavia el nombre de la clase ni del metodo.**

### Anadido al oracle contractual

```text
known property + existing incompatible variable
  → fail-closed
  → nunca healthy
  → nunca repairable
  → ningun effective silencioso
  → zero mutation
```

**Hoy solo existe `VariableType.Length` y NO se anade un segundo tipo para probarlo.** Puede probarse
con **estado construido de forma controlada en tests** si hace falta.

---

## 3. `V4-R02` — Costura de kernel puro con conjunto de descriptores

**Se acepta MATERIAL-2.** La review mostro que `ProjectVariableConsumerProbe` devuelve `Indeterminate`
ante una `PropertyId` desconocida (l.72-75), de modo que con un solo `PropertyId` conocido **«dos
bindings repairable-broken» es estructuralmente inalcanzable**, y que la letra de `V3-R10` impedia la
unica costura que permitiria probar el algoritmo.

### Prohibido en produccion

- catalogo **configurable** desde Plugin/UI;
- **DI** del catalogo;
- `IEnumerable<Descriptor>` **suministrado desde capas externas**;
- **runtime registration**;
- **reemplazo** del catalogo;
- **config / CSV / reflection discovery**.

**La autoridad productiva sigue siendo `SelectiveLinkedProperties.All`, cerrada y estatica en
Application.**

### Permitido internamente

Un **algoritmo/kernel PURO** puede recibir explicitamente:

```text
IReadOnlyList<SelectiveLinkedPropertyDescriptor>
```

o forma equivalente.

- Los **entrypoints PRODUCTIVOS** le pasan **siempre** el catalogo cerrado.
- Los **tests** pueden invocar directamente ese kernel con un **descriptor set TEST-ONLY**.

**Eso es una costura de prueba / funcion pura, NO extensibilidad en runtime ni service locator.**

### Los descriptor sets de prueba

- **no** se registran en `ProjectPropertyIds`;
- **no** modifican `SelectiveLinkedProperties.All`;
- **no** son visibles a Plugin/UI;
- **no** se cargan desde configuracion;
- **existen solo en tests**.

*(Se fijan aqui los dos primeros nombres concretos de toda la cadena de Proposals —
`SelectiveLinkedProperties.All` y `SelectiveLinkedPropertyDescriptor`— porque la costura no se puede
describir sin nombrar lo que la atraviesa. El resto de nombres sigue sin fijarse.)*

---

## 4. `V4-R03` — `G4B`: kernel proof **vs** production proof

Resuelve explicitamente la contradiccion de gates que la review encontro.

En `G4B`, todavia con **`VerticalClearance` ONLY en produccion**:

### DEBEN quedar GREEN, con descriptores sinteticos TEST-ONLY

```text
N→1:
  una VariableId usada por DOS PropertyIds sinteticas
  P contiene AMBAS
  un clone
  materializa AMBAS
  una RackMutation conceptual/final

Repair batch:
  healthy + broken
  two broken
  malformed  fatal
  incompatible fatal
```

**Esto demuestra el ALGORITMO.**

### Pero NO se afirma que el proof productivo este cerrado en `G4B`

Los siguientes contratos **esperan necesariamente** al catalogo **REAL** de dos propiedades, en
`G4E`/`G4F`:

```text
selective.verticalClearance + selective.palletTolerance
same VariableId
real resolver fields
real WithDesign
real FindBroken
real EditorOpen
real RepairBrokenRack
real BOM / drawing / persistence / UI
```

**No se activa `PalletTolerance` antes de `G4E`.**

### La igualdad de conjuntos, con precision

```text
independent PRODUCTION oracle PropertyIds  ==  PRODUCTION catalogue PropertyIds
```

**Aplica al catalogo PRODUCTIVO.** **No** se exige que el conjunto sintetico de prueba coincida con el
catalogo productivo — son universos distintos y confundirlos era justamente lo que hacia irresoluble la
contradiccion.

---

## 5. `V4-R04` — Las pruebas sinteticas no filtran el proof (regla anti-fuga de la metrica)

**ANTES de `PRE_PROOF_SHA` queda PROHIBIDO en codigo PRODUCTIVO cualquier wiring especifico de
`PalletTolerance`:**

- declaracion del token / `PropertyId` productivo para binding;
- **descriptor productivo** de `PalletTolerance`;
- cualquier **branch especifico**;
- **placement/wiring de UI**;
- **binding option** especifica;
- **source guard** especifica.

**Ademas, los fixtures genericos de `G4B` NO deben usar el token productivo
`selective.palletTolerance`.** Usan `PropertyId` **sinteticas** de tests.

Pueden usar **dos campos fisicos distintos** del documento/dominio para demostrar no-cross-write si
hace falta —esos campos **ya existen**— pero **no** deben constituir el descriptor productivo ni el
token persistido del proof.

**Los tests PRODUCTIVOS especificos de `selective.palletTolerance` se escriben por primera vez dentro
de `G4E`**, despues de fijar `PRE_PROOF_SHA = parent de G4E`. Asi el diff
`PRE_PROOF_SHA..PROOF_SHA` captura **tanto el wiring productivo como el test proof especifico**.

---

## 6. `V4-R05` — Evidencia RED de `G4E`

**Antes** de introducir los tests nuevos:

```text
PRE_PROOF_SHA = HEAD
```

Ejecutar la suite/focal requerida sobre ese SHA limpio → **`BASELINE GREEN`**, y registrar:

- **SHA**;
- **comando exacto**;
- **total PASS/FAIL**;
- **resultado GREEN**.

Despues, en el **mismo worktree y SIN COMMIT**:

1. escribir los tests/oracle **productivos** de `PalletTolerance`;
2. ejecutarlos;
3. obtener el **RED esperado**.

**La evidencia RED debe registrar:**

- el **mismo `PRE_PROOF_SHA`** como base;
- el **comando exacto**;
- los **nombres de tests/aserciones nuevas** que fallan;
- que los fallos pertenecen **exclusivamente** a expectativas nuevas de I-48;
- la **ausencia de fallos preexistentes o no relacionados**.

Despues implementar activacion/wiring, ejecutar de nuevo y exigir **GREEN antes de cualquier
commit/push**. **No se versiona un SHA deliberadamente rojo.**

---

## 7. Reglas de UI cerradas en V4

### `V4-R06` — Mensaje de `InvalidDraft` de referencia

Para texto como `=Hol` **sin opcion seleccionada**, una frontera C4 debe **bloquear** con un mensaje
equivalente a:

> «Selecciona una variable de la lista o cancela este cambio con Escape.»

**No sugerir que Enter por si solo resolvera el texto** — eso reintroduciria la resolucion por nombre
que `V3-R08` prohibe.

Para `Reference → DraftLiteral` sin confirmacion explicita:

> «Confirma el cambio a literal con Enter o cancelalo con Escape.»

Los textos exactos pueden localizarse en implementacion; **la informacion contractual debe
conservarse**.

### `V4-R07` — Presentacion de `Reference → DraftLiteral` (cierra `OQ3-1`)

Si el estado comprometido es `Reference(X)` y el usuario escribe `7`:

- **el textbox conserva visualmente `7`**, porque es su draft;
- pero **debe existir indicacion visible** de que la referencia **SIGUE gobernando hasta el commit**,
  equivalente a:

```text
Pendiente: sigue gobernado por =Nombre.
Enter para usar 7 como literal.
Escape para cancelar.
```

- **`LostFocus` no cambia `Source`** (`V3-R06`).
- **No** reemplazar automaticamente el draft por `=Nombre`: pareceria haber **descartado** lo escrito.
- **No** presentar `7` como si ya fuera el effective.

### `V4-R08` — `FindBroken` / confirmacion del batch (cierra `OQ3-2`)

**Agrupar filas por rack es decision de PRESENTACION y NO un requisito arquitectonico.**

**Si es contractual:**

- `FindBroken` expone **todas** las filas repairable-broken;
- una accion Repair iniciada **desde cualquier fila** tiene **scope RACK**;
- **antes de ejecutar** se presenta **TODO** el conjunto `B` del rack;
- la confirmacion **nombra todos** los `PropertyId` / `VariableId` / `StoredLiteral` afectados.

**No puede parecer que solo se reparara la fila clicada.**

### `V4-R09` — Intent de Repair rack-scoped (cierra `OQ3-3`)

- La solicitud **semantica** de Repair a Application es **rack-scoped**. El **`RackId`** determina el
  rack.
- **Ningun `PropertyId` recibido desde la UI puede limitar que elementos entran en `B`.**
- Si la implementacion conserva una `PropertyId` como **metadata de origen** de la UI, es **solo
  metadata diagnostica/presentacional** y **NO participa en el scope de mutacion**.
- **Preferencia arquitectonica:** `RepairBroken(rackId, confirmed)` o equivalente semanticamente
  rack-scoped. **No se fija la firma exacta** si una forma menor preserva la misma garantia.

---

## 8. Gates de V4

> **PROPUESTOS, NO AUTORIZADOS.** Ocho gates, refinados.

| Gate | Contenido |
|---|---|
| **G4A** | Descriptor/catalogo + **primitivas de inspeccion property-specific**. **`VerticalClearance` ONLY productivo** |
| **G4B** | **Kernel generico de Application + fachada de producto `VerticalClearance` ONLY**: effective resolver enrutado · `WithDesign` · `Link`/`Unlink` · **N→1** · **`P` tras la authority** · **`FindBroken` all** · **`RepairBrokenRack`** · `EditorOpen` · **compatibilidad de tipo** · caracterizacion de authored-authority. **El ALGORITMO multi-propiedad se prueba aqui con descriptores sinteticos de TEST. Sin wiring de producto de `PalletTolerance`** |
| **G4C** | `RACKEDITAR`: `LinkedPropertyEditor` · final-state reconciler · transiciones de `Source` · pending/C4 · **estado pendiente visible** (`V4-R07`) · autocomplete de seleccion explicita |
| **G4D** | **Autoridad del tipo persistido**: `ToProjectVariables` · **fail-loud** · camino de compatibilidad completo. **Al terminar `G4D` NO puede existir wiring de producto especifico de `PalletTolerance`** |
| **G4E** | **SINGLE REAL PROOF GATE.** `PRE_PROOF_SHA` = parent · **baseline GREEN** · escribir **primero** los tests/oracle REALES de `PalletTolerance` · **RED local documentado** · **NO COMMIT** · anadir `PropertyId` de producto + descriptor + placement/wiring de UI + el wiring de proof especifico requerido · **GREEN** · commit/push **solo en GREEN** |
| **G4F** | **Proof REAL completo de dos propiedades**: simultaneas · misma `VariableId` N→1 · repair `healthy+broken` · `two-broken` batch · **incompatible fatal** · BOM/dibujo · persistencia · `EditorOpen`/UI · homonimos · C4/navegacion. **Cualquier fix requerido cuenta en el AFTER.** `PROOF_SHA` = SHA final donde todos los contratos de proof estan GREEN |
| **G4G** | Candidato completo + CI |
| **G4H** | **Owner Validation en AutoCAD 2025** |

---

## 9. Adiciones al oracle

Se mantiene **todo** el oracle de V3 y se anade explicitamente:

```text
existing target + incompatible type
  → fail-closed

repair classifier:
  missing target        → repairable
  incompatible target   → fatal
  unknown property      → fatal
  malformed reference   → fatal

test-only synthetic descriptor set
  → puede probar el kernel generico N→1 / repair
  → NO cambia el catalogo de producto

production oracle set  ==  production catalogue set
```

---

## 10. Estado

```text
COORDINATOR PROPOSAL V4 — THIRD ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V4
Architect   = NOT REVIEWED V4
Consensus   = NOT REACHED

Origen: Architect Review de V3 @ b03a7ae81f1a28ce4e3117b3eddd02fb9906c0a2
        (NOT AGREED · 0 BLOCKER · 2 MATERIAL · 3 MINOR)

CERRADO y no reabierto:
  AGREE RepairBrokenRack rack-scoped   (decision de V3)
  AGREE R-02                            (decision de V2)

V4 NO introduce ningun apartamiento respecto de la review de V3.

Siguiente paso: Architect Review sobre V4.
Punto que V4 somete con atencion especial: la EXTENSION de V4-R01 —la
comprobacion de tipo entra tambien en el camino de RESOLUCION, no solo en
Repair—, que anade un sexto modo de fallo al resolver y por tanto conviene
contrastarla contra los invariantes heredados de I-47.
```

**Historial conservado:** [I-48-proposal-v1.md](I-48-proposal-v1.md),
[I-48-proposal-v2.md](I-48-proposal-v2.md) e [I-48-proposal-v3.md](I-48-proposal-v3.md) quedan
**intactas**. Cada una debe poder leerse tal como fue revisada.
