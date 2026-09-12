# I-48 — Proposal V6: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V6 — FIFTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V6
> Architect   = NOT REVIEWED V6
> Consensus   = NOT REACHED
> ```
>
> **V6 reconcilia la QUINTA Architect Review, la de V5.** Esa review termino:
>
> ```
> Architect = NOT AGREED
> BLOCKER  = 0
> MATERIAL = 2   (V5-R01 y V5-R02 no componen: el snapshot incompatible no tenia
>                 camino hasta InspectBinding;
>                 FATAL_UNKNOWN_PROPERTY sin dueno bajo la firma de V5-R03)
> MINOR    = 3   (mismatch tras G4D; unicidad de PropertyId en el descriptor set;
>                 VariableId duplicado en la via pura)
> ```
>
> **Los cinco se ACEPTAN. V6 no introduce ningun apartamiento.** Los dos materiales eran, otra vez, de
> la misma familia: **dos piezas correctas por separado que V5 no dijo COMO se encuentran**, y **una
> autoridad declarada cuya firma le impedia ejercerla**.
>
> La revision de V6 **no ha ocurrido**.
>
> ```
> Base:              fef7b97009c241df1ac69ef20c025dc9a19d7dfd  (Proposal V5 revisada)
> Cadena previa:     24c004e (V4) · b03a7ae (V3) · 7f6680d (V2) · a061148 (V1.1) · edacf7d · 7a9471f
> Codigo auditado:   e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:         I-48-proposal-v1..v5 quedan INTACTAS
> Estado de gates:   G0 · G1 · G1.1 · G2A/.1/.2 · Rev V1.1 · G2B · Rev V2 · G2C · Rev V3 · G2D
>                    · Rev V4 · G2E · Rev V5 · **G2F EN CURSO** · G3 consenso PENDIENTE
> ```

## 0. Decisiones CERRADAS que V6 no reabre

```text
AGREE V4-R01 resolver type compatibility
AGREE V4-R02 test seam
AGREE RepairBrokenRack rack-scoped
AGREE R-02
```

Se conserva sin cambio todo lo de V2/V3/V4/V5 que ninguna `V6-R` toque.

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review de V5 | Severidad | Resuelto en |
|---|---|---|
| **snapshot / usableRegistry no componen** | MATERIAL | **`V6-R01`** |
| **`FATAL_UNKNOWN_PROPERTY` sin dueno** | MATERIAL | **`V6-R02`** |
| mismatch sigue inalcanzable tras `G4D` | MINOR | **`V6-R04`** |
| `PropertyId` duplicada en el descriptor set | MINOR | **`V6-R03`** |
| `VariableId` duplicado en el synthetic registry | MINOR | **`V6-R05`** |
| `OQ5-1` — proyeccion de produccion | (abierta) | **CERRADA** (§6) |
| `OQ5-2` — `BindingInspection` transporta el snapshot | (abierta) | **detalle permitido / preferido** (§7) |

---

## 2. `V6-R01` — Composicion de `VariableTargetSnapshot` y `UsableProjectVariablesRegistry`

**Aceptado `PC5-1`.** El Arquitecto demostro que la unica proyeccion documento → variables tipadas que
existe es `ToProjectVariables()`, y pasa por `ProjectVariable.Create(...)`, que **lanza** ante un tipo
no soportado. Con una sola via de construccion, el snapshot incompatible **no tenia donde ponerse**.

`UsableProjectVariablesRegistry` tiene conceptualmente **DOS vias de construccion claramente
separadas**.

### A. Factory PRODUCTIVA

Solo acepta un `ProjectVariablesReadResult` con **`Absent`** o **`Readable`**.
**`PresentButUnreadable`, `IncompatibleMajor` y `null`/no-consultado NUNCA llegan a un usable
registry.**

Proyecta los `VariableTargetSnapshot` **desde el `ProjectVariablesDocument` YA ACREDITADO por el
store**. **No necesita pasar por `ProjectVariable.Create()`.**

**Esta proyeccion es MECANICA.** **NO** vuelve a decidir:

- schema compatibility;
- JSON validity;
- persisted type support;
- definition kind support;
- si el registro es `Readable`.

> **`ProjectVariablesStore` sigue siendo la UNICA autoridad de lectura. La factory es un TESTIGO de ese
> veredicto, no un segundo juez.**

Puede convertir el `Type` **persistido ya validado** a `VariableType`, pero **no** ejecutar una segunda
politica independiente de tipos.

### B. Factory interna/pura para TEST

Puede construir `UsableProjectVariablesRegistry` **directamente** desde `VariableTargetSnapshot[]`,
**sin** `ProjectVariablesReadResult`, **sin** store y **sin** `ProjectVariable`.

Esta via:

- **solo** existe para kernel/tests o ambito interno equivalente;
- **no** es alcanzable desde Plugin/UI;
- **no** interpreta `null` como registro vacio;
- **no** decide schema/JSON/persisted compatibility;
- **permite** un `VariableType` sintetico/no soportado **para probar la desigualdad**.

**Invariantes minimos del fixture:**

```text
VariableId no vacio
LiteralValue finito
VariableId unico
```

Un `VariableId` duplicado en esta via es un **fixture invalido**.

### Duplicados productivos — acotacion deliberada del alcance

> **NO convertir I-48 silenciosamente en un fix de duplicados persistidos.**

El store actual **no** declara los duplicados como `unreadable` —sus cinco validaciones son nombre,
tipo, definicion presente, clase de definicion y valor finito— y el comportamiento heredado los
**colapsa en el lookup** (ultimo gana). Por tanto:

- la via **TEST** **si** prohibe ids duplicados (`V6-R05`);
- la factory **PRODUCTIVA** **NO** introduce una nueva politica de rechazo de duplicados **por
  iniciativa propia**;
- **si implementar la nueva arquitectura exige cambiar la semantica productiva de `VariableId`
  duplicados: STOP y escalar** como cambio material / deuda heredada.

**V6 NO declara resuelta esa deuda.**

---

## 3. `V6-R02` — La primitiva posee tambien `PropertyId → descriptor`

**Aceptado `PC5-2`.** La firma de V5 recibia un `descriptor` **ya resuelto** y a la vez declaraba a la
primitiva unica autoridad sobre `PropertyId`: las dos cosas eran conjuntamente insatisfacibles, y
`FATAL_UNKNOWN_PROPERTY` quedaba sin dueno.

La primitiva **ya NO recibe un descriptor previamente resuelto como unica forma**. Conceptualmente
recibe:

```text
InspectBinding(
    rawPropertyToken,
    rawReference,
    descriptorSet,
    usableRegistry)
        → BindingInspection
```

**No se fijan nombres ni firma C# exactos.**

**Es la UNICA autoridad semantica para:**

1. interpretar/validar el `PropertyId`;
2. **resolver `PropertyId → descriptor`**;
3. validar el reference **kind**;
4. parsear el `VariableId`;
5. determinar la **existencia** del target;
6. comprobar `target.Type` vs `descriptor.VariableType`.

Asi puede producir **por si misma** los cinco resultados:

```text
HEALTHY
REPAIRABLE_MISSING_TARGET
FATAL_INCOMPATIBLE_TARGET
FATAL_UNKNOWN_PROPERTY
FATAL_MALFORMED_REFERENCE
```

> **`FATAL_UNKNOWN_PROPERTY` debe nacer AQUI.**

El scanner:

```text
InspectBindings(authored, descriptorSet, usableRegistry)
```

**solo**: itera las entradas · llama a `InspectBinding` · conserva y ordena resultados.

**NO hace `PropertyId → descriptor` semantico por su cuenta.**

---

## 4. `V6-R03` — DescriptorSet valido y unico

**Aceptado `PC5-4`.** Todo conjunto de descriptores usado por el kernel —**productivo o sintetico**—
debe estar validado conceptualmente como:

```text
PropertyId unicos, comparacion Ordinal
```

Una `PropertyId` duplicada es **configuracion invalida del descriptor set** y se **rechaza antes de
inspeccionar bindings**.

**No usar** normalizacion · case folding · aliases. **La identidad sigue siendo ordinal y
case-sensitive** (`D-02`).

El catalogo productivo cerrado y los sets sinteticos obedecen **la misma** invariancia de unicidad.
**Esto NO convierte el catalogo en DI ni en configuracion de runtime** (`R-02`, `V4-R02`).

---

## 5. `V6-R04` — Alcance EXACTO de `G4D`

**Aceptado `PC5-3`.** Se corrige el lenguaje de V5, que sugeria que tras `G4D` el mismatch productivo
pasaba a ser alcanzable.

| Gate | Lo que queda cierto |
|---|---|
| **`G4B`** | `kernel type compatibility` = **implementada + testeada** |
| **`G4D`** | `persisted Type authority` = **correcta** — `ToProjectVariables()` deja de fabricar `Length` y pasa a respetar / `fail-loud` sobre el tipo persistido |

**PERO:**

> **`G4D` NO vuelve alcanzable un mismatch productivo en I-48.**

Mientras este build soporte unicamente `VariableType.Length` **y el store rechace cualquier otro
tipo**, el estado

```text
existing productive target with incompatible supported type
```

**sigue siendo inalcanzable**. La rama incompatible **existe y esta probada en el kernel** para
preservar el contrato futuro, pero **no se inventa un segundo tipo productivo**.

**No se planifica prueba productiva ni manual de mismatch en `G4D` ni en `G4H`.**

## `V6-R05` — `VariableId` duplicado en el synthetic registry

**Aceptado `PC5-5`, con este alcance.** La factory pura/sintetica **DEBE rechazar `VariableId`
duplicados**, para que el kernel no pruebe existencia sobre un target ambiguo. **Es invariancia del
FIXTURE / SEAM.**

> **NO se afirma que I-48 corrija o cambie el comportamiento persistido heredado de duplicados.**

La diferencia queda registrada explicitamente **para evitar una ampliacion accidental de alcance**
(§2, «Duplicados productivos»).

---

## 6. `OQ5-1` — Proyeccion en produccion: CERRADA

La factory productiva **puede proyectar snapshots directamente** del `ProjectVariablesDocument`
acreditado por `Absent`/`Readable`, **sin depender de `ProjectVariable.Create()`**. **No debe
introducir una segunda autoridad de validacion.**

**`ToProjectVariables()` continua existiendo**, y `G4D` corrige su deuda de `Type` porque **otros
consumidores/contratos lo requieren** — no porque la factory dependa de ella.

Si conviene compartir internamente el codigo **MECANICO** de proyeccion, es **detalle de
implementacion**. **No se crean dos politicas de interpretacion del `Type`.**

## 7. `OQ5-2` — `BindingInspection` y el snapshot: detalle permitido, con preferencia

`BindingInspection(HEALTHY)` **PUEDE** transportar el `VariableTargetSnapshot` ya encontrado, para que
el resolver **no haga un segundo lookup**. **Es preferible si reduce duplicacion.**

Pero es **contractual** que, si existe un lookup posterior, **no vuelva a interpretar**:

- existencia;
- tipo;
- identidad.

**No puede producir una segunda autoridad.**

---

## 8. Clasificacion consolidada

Con **`usableRegistry` y `descriptorSet` validos**:

```text
raw PropertyId desconocido                → FATAL_UNKNOWN_PROPERTY
reference kind / VariableId / payload invalido → FATAL_MALFORMED_REFERENCE
target ausente                            → REPAIRABLE_MISSING_TARGET
target presente + Type incompatible       → FATAL_INCOMPATIBLE_TARGET
target presente + Type compatible         → HEALTHY
```

**Fuera de este dominio, y ANTES:**

```text
PresentButUnreadable                → FAIL-CLOSED antes
IncompatibleMajor                   → FAIL-CLOSED antes
registry no consultado              → FAIL-CLOSED antes
descriptor set duplicado/invalido   → FAIL antes de inspeccionar documentos
```

**Todo FATAL: `zero plan` / sin fallback. Solo `REPAIRABLE_MISSING_TARGET` entra en `B`.**

---

## 9. Tests contractuales

Se anaden/refinan como minimo:

```text
1. synthetic usableRegistry + target snapshot Type != descriptor.VariableType
     → FATAL_INCOMPATIBLE_TARGET

2. el snapshot incompatible REALMENTE VIAJA:
     synthetic snapshots → synthetic usableRegistry → InspectBinding
     → FATAL_INCOMPATIBLE_TARGET

3. descriptor set con PropertyId duplicada (Ordinal)
     → construccion FAIL

4. raw PropertyId desconocida → InspectBinding
     → FATAL_UNKNOWN_PROPERTY

5. scanner con unknown PropertyId
     → delega en InspectBinding
     → NO implementa lookup paralelo

6. synthetic usableRegistry con VariableId duplicado
     → construccion FAIL

7. production usable registry SOLO desde Absent/Readable

8. PresentButUnreadable / IncompatibleMajor
     → nunca usable registry
     → nunca MISSING / B
```

> El test **2** es el que cierra de verdad el material de la ronda anterior: no basta con que el
> snapshot exista y con que la rama exista; hay que demostrar que **hay camino**.

**Se mantienen todos los contratos de V5** (registro utilizable, `RepairBrokenRack`, N→1, `FindBroken`
all, reconciler, pending/`Source`, oracle independiente y `production oracle set == production
catalogue set`).

---

## 10. Gates de V6

> **PROPUESTOS, NO AUTORIZADOS.** Ocho.

| Gate | Contenido |
|---|---|
| **G4A** | descriptor/catalogo · **descriptor set validado** · **factories de usable-registry (A y B)** · **target snapshot** · **primitiva `InspectBinding`**. **`VerticalClearance` ONLY productivo** |
| **G4B** | kernel generico de Application · scanner · resolver enrutado · **N→1** · **`P` tras la authority** · **`FindBroken` all** · **`RepairBrokenRack`** · **proof multi-propiedad sintetico** · **proof de target incompatible sintetico** · **proof de propiedad de `FATAL_UNKNOWN_PROPERTY`**. **SIN wiring de binding de `PalletTolerance`** |
| **G4C** | `LinkedPropertyEditor` · reconciler · transiciones de `Source` · **restauracion de foco C4** · estado pendiente · seleccion explicita de autocomplete |
| **G4D** | **autoridad del `Type` persistido correcta** · `ToProjectVariables` **fail-loud / proyeccion correcta** · **NO se afirma que el mismatch productivo pase a ser alcanzable** |
| **G4E** | **proof REAL unico de `PalletTolerance`** · `PRE_PROOF_SHA` · baseline GREEN · tests primero · **RED local** · **no commit** · wiring de binding de producto · **GREEN** · commit/push **solo en GREEN** |
| **G4F** | proof real completo de dos propiedades · **los fixes cuentan en el AFTER** · `PROOF_SHA` final en verde |
| **G4G** | candidato completo + CI |
| **G4H** | **Owner Validation en AutoCAD 2025** · **sin caso fabricado de tipo incompatible** |

---

## 11. Estado

```text
COORDINATOR PROPOSAL V6 — FIFTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V6
Architect   = NOT REVIEWED V6
Consensus   = NOT REACHED

Origen: Architect Review de V5 @ fef7b97009c241df1ac69ef20c025dc9a19d7dfd
        (NOT AGREED · 0 BLOCKER · 2 MATERIAL · 3 MINOR)

CERRADO y no reabierto:
  AGREE V4-R01 resolver type compatibility
  AGREE V4-R02 test seam
  AGREE RepairBrokenRack rack-scoped
  AGREE R-02

V6 NO introduce ningun apartamiento respecto de la review de V5.

Siguiente paso: Architect Review sobre V6.
Los dos puntos que V6 somete: la SEPARACION de las dos factories —que es lo que
hace que el snapshot incompatible tenga camino sin que un registro ilegible
pueda volverse vacio— y el traslado del lookup PropertyId -> descriptor DENTRO
de la primitiva, que es lo que da dueno a FATAL_UNKNOWN_PROPERTY.
Se somete tambien la acotacion deliberada del alcance sobre duplicados
persistidos: V6 los prohibe en el FIXTURE y NO los declara resueltos en
produccion.
```

**Historial conservado:** [I-48-proposal-v1.md](I-48-proposal-v1.md),
[I-48-proposal-v2.md](I-48-proposal-v2.md), [I-48-proposal-v3.md](I-48-proposal-v3.md),
[I-48-proposal-v4.md](I-48-proposal-v4.md) e [I-48-proposal-v5.md](I-48-proposal-v5.md) quedan
**intactas**. Cada una debe poder leerse tal como fue revisada.
