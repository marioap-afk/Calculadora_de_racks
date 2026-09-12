# I-48 — Proposal V5: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V5 — FOURTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V5
> Architect   = NOT REVIEWED V5
> Consensus   = NOT REACHED
> ```
>
> **V5 reconcilia la CUARTA Architect Review, la de V4.** Esa review termino:
>
> ```
> Architect = NOT AGREED
> BLOCKER  = 0
> MATERIAL = 2   (FATAL_INCOMPATIBLE_TARGET inverificable con la forma actual;
>                 REPAIRABLE_MISSING_TARGET sin cualificar degrada
>                 present-but-unreadable a absent)
> MINOR    = 3   (chequeo inerte hasta G4D; anti-fuga como enumeracion;
>                 Escape inalcanzable en el mensaje)
> ```
>
> **Los cinco hallazgos se ACEPTAN. V5 no introduce ningun apartamiento.** Los dos materiales eran de
> la **misma familia** —una definicion correcta a la que le faltaba decir **sobre que estado es
> valida**— y V5 los cierra fijando esas dos precondiciones como fronteras de tipo, no como
> convenciones.
>
> La revision de V5 **no ha ocurrido**.
>
> ```
> Base:              24c004ecd39ab1bc6a0c34978a052c6b71a35144  (Proposal V4 revisada)
> Cadena previa:     b03a7ae (V3) · 7f6680d (V2) · a061148 (V1.1) · edacf7d (G1) · 7a9471f (G1.1)
> Codigo auditado:   e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:         I-48-proposal-v1.md, -v2.md, -v3.md y -v4.md quedan INTACTAS
> Estado de gates:   G0 · G1 · G1.1 · G2A/.1/.2 · Rev V1.1 · G2B · Rev V2 · G2C · Rev V3 · G2D
>                    · Rev V4 · **G2E EN CURSO** · G3 consenso PENDIENTE
> ```

## 0. Decisiones CERRADAS que V5 no reabre

```text
AGREE V4-R01 resolver type compatibility     (cerrada en la review de V4)
AGREE V4-R02 test seam                       (cerrada en la review de V4)
AGREE RepairBrokenRack rack-scoped           (cerrada en V3)
AGREE R-02                                   (cerrada en V2)
```

Se conservan sin cambio todas las `R` de V2/V3/V4 que ninguna `V5-R` toque, y `D-01`..`D-18` de V1.1
en lo que ninguna `R` haya alterado.

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review de V4 | Severidad | Resuelto en | Resultado |
|---|---|---|---|
| `FATAL_INCOMPATIBLE_TARGET` inverificable (`PC4-1`) | MATERIAL | **`V5-R01`** | **ACEPTADO, con precision del Coordinador** |
| `REPAIRABLE_MISSING_TARGET` sin cualificar (`PC4-2`) | MATERIAL | **`V5-R02`** | **ACEPTADO, con precision del Coordinador** |
| Chequeo de tipo inerte hasta `G4D` (`PC4-3`) | MINOR | **`V5-R04`** | ACEPTADO |
| Anti-fuga como enumeracion (`PC4-4`) | MINOR | **`V5-R05`** | ACEPTADO |
| Escape inalcanzable (`PC4-5`) | MINOR | **`V5-R06`** | ACEPTADO |
| `OQ4-1` — ¿tipo incompatible en `G4H`? | (abierta) | **`V5-R07`** | **CERRADA**: test automatico, **no** validacion manual |
| `OQ4-2` — clasificacion por binding o por documento | (abierta) | **`V5-R03`** | **CERRADA**: primitiva por binding + scanner comun |

**«Con precision del Coordinador» no es un apartamiento**: el Arquitecto pidio el *qué* y V5 fija el
*sobre qué*, que es lo que faltaba. Ninguna de las dos precisiones contradice su propuesta.

---

## 2. `V5-R01` — `PC4-1`: el snapshot del target

**Aceptado.** Y **no basta con decir «modelo tipado minimo»**: sin fijar su forma, la implementacion
natural volveria a `ProjectVariable` y la rama seguiria siendo inejecutable.

Se fija **conceptualmente** un snapshot interno equivalente a:

```text
VariableTargetSnapshot
    VariableId
    VariableType
    LiteralValue
```

**No se fija el nombre exacto.**

**Contrato:**

- **NO** es `ProjectVariable`;
- **NO** es una nueva entidad de dominio;
- **NO** cambia la persistencia;
- **NO** se expone a UI ni a Plugin;
- **produccion** lo proyecta desde variables reales **ya validadas**;
- los **tests del kernel** pueden construirlo **directamente**;
- **su construccion NO llama a `VariableTypes.IsSupported()`.**

**Por que esto desbloquea la prueba.** Permite ejercitar

```text
target.Type != descriptor.VariableType
  → FATAL_INCOMPATIBLE_TARGET
```

usando un **valor sintetico / no soportado** de `VariableType` **dentro del snapshot de test**, sin:

- anadir un segundo tipo soportado;
- modificar `VariableTypes.IsSupported`;
- debilitar `ProjectVariable.Create`;
- modificar el store.

> **Que prueba exactamente ese test, dicho para que nadie lo malinterprete:** prueba **la desigualdad
> fail-closed**. **NO** declara que ese tipo sintetico sea productivamente soportado, ni abre la puerta
> a un segundo `VariableType`. La evidencia del Arquitecto sigue en pie: `ProjectVariable.Create` lanza
> ante un tipo no soportado, y por eso el snapshot **no puede** ser un `ProjectVariable`.

---

## 3. `V5-R02` — `PC4-2`: la frontera del registro utilizable

**Aceptado.** `REPAIRABLE_MISSING_TARGET` **solo existe contra un registro semanticamente utilizable**,
y eso deja de ser una convencion por call site para convertirse en una **frontera de tipo**.

### Disposicion por estado de lectura

```text
ProjectVariablesReadResult

Absent                 → usable EMPTY registry
Readable               → usable POPULATED registry
PresentButUnreadable   → FAIL-CLOSED
IncompatibleMajor      → FAIL-CLOSED
null / no consultado   → FAIL-CLOSED
```

La primitiva por binding recibe un valor equivalente a **`UsableProjectVariablesRegistry`**, que
**solo puede obtenerse desde `Absent` o `Readable`**.

> **PROHIBIDO que un `ProjectVariablesDocument` nulo signifique «registro vacio» dentro del kernel.**
> Es exactamente el paso que convertiria un registro presente-e-ilegible en uno ausente.

### Definicion consolidada

```text
REPAIRABLE_MISSING_TARGET =
      reference conocida y bien formada
    + usable registry
    + target VariableId ausente
```

Mientras que **`PresentButUnreadable`**, **`IncompatibleMajor`** y **registro no consultado** fallan
**ANTES** de la clasificacion por binding y **nunca producen `B`**.

**Queda explicitamente preservado el invariante heredado de I-47:**

```text
present-but-unreadable  !=  absent / empty
```

*(La asimetria que lo hace necesario, verificada por el Arquitecto en el codigo: `Absent()` construye
`ProjectVariablesDocument.CreateNew()` —vacio pero **valido**—, mientras que `Unreadable(...)` e
`IncompatibleMajor(...)` construyen `Document = null`. Sin la frontera de tipo, indexar ese `null`
daria indice vacio y **todos** los bindings del rack se clasificarian como reparables.)*

---

## 4. `V5-R03` — `OQ4-2`: primitiva por binding + scanner comun

**Cerrada asi:**

```text
InspectBinding(descriptor, reference, usableRegistry)
    → BindingInspection
```

Es la **UNICA autoridad** para:

- `PropertyId`;
- reference **kind**;
- `VariableId`;
- **existencia** del target;
- **compatibilidad de tipo**.

Y un agregador equivalente:

```text
InspectBindings(authored, descriptors, usableRegistry)
    → ordered BindingInspection[]
```

que **solo llama a la primitiva** y **no reimplementa ninguna regla**.

**`Resolver`, `FindBroken` y `RepairBrokenRack` consumen esa MISMA clasificacion.** Es lo que impide
las «tres nociones de compatibilidad» que la review de V4 identifico como el defecto a evitar un nivel
por debajo del que I-48 vino a resolver.

---

## 5. Clasificacion consolidada

**Con `usable registry` ya acreditado** (`V5-R02`):

| Estado | Condicion | Disposicion |
|---|---|---|
| **`HEALTHY`** | known property · well-formed reference · target exists · `target.Type == descriptor.VariableType` | resuelve |
| **`REPAIRABLE_MISSING_TARGET`** | known property · well-formed reference · **target missing** | **UNICO** que entra en `B` |
| **`FATAL_INCOMPATIBLE_TARGET`** | target exists · `target.Type != descriptor.VariableType` | **abort / zero plan** |
| **`FATAL_UNKNOWN_PROPERTY`** | `PropertyId` desconocida | **abort / zero plan** |
| **`FATAL_MALFORMED_REFERENCE`** | kind desconocido · `VariableId` ilegible · payload invalido | **abort / zero plan** |

**Todo `FATAL`: `abort / zero plan`.** **Solo `REPAIRABLE_MISSING_TARGET` entra en `B`.**

---

## 6. `V5-R04` — `PC4-3`: el chequeo de tipo en `G4B` frente a `G4D`

En **`G4B`**:

```text
kernel type compatibility = IMPLEMENTADA + TESTEADA
```

**Pero se documenta que, productivamente, sigue siendo INERTE / no alcanzable**, por tres razones
concurrentes:

1. el **store** solo acepta `Length` (`ProjectVariablesStore.IsKnownType` rechaza el documento entero);
2. **`ProjectVariable`** solo admite tipos soportados (`Create` lanza);
3. **`ToProjectVariables()`** todavia **fabrica `Length`**.

**`G4D`** corrige la autoridad del `Type` **persistido**. **NO se cambia el orden de gates**: el
Arquitecto ya valido que el chequeo inerte no es incorrecto, y que `G4D` aterriza antes de `G4E`, o
sea antes de que exista una segunda propiedad.

Lo que esta linea evita es que alguien lea «`G4B`: compatibilidad de tipo ✓» y **crea que produccion ya
esta protegida**.

---

## 7. `V5-R05` — `PC4-4`: anti-fuga como PROPIEDAD, no como enumeracion

**No se usa una enumeracion exhaustiva** — repetiria el error de metodo que `V3-R04` ya corrigio.

> **REGLA.** Antes de `PRE_PROOF_SHA`, **ningun artefacto PRODUCTIVO NUEVO puede introducir soporte de
> binding especifico `PalletTolerance ↔ ProjectVariable`.**

**SI pueden existir legitimamente:**

- `SelectivePalletDesign.PalletTolerance` **(ya existe)**;
- toda la **mecanica historica** de `PalletTolerance`;
- **infraestructura genuinamente generica**.

**NO pueden adelantarse especificamente para el binding:** simbolos · recursos · labels · DTOs ·
helpers · branches · XAML · commands · tokens · descriptors · **cualquier otra forma equivalente**.

> **La lista es ILUSTRATIVA, no exhaustiva.** Lo que decide es la propiedad, no la pertenencia a la
> lista: la regla se acota a **wiring de binding**, no a la palabra `PalletTolerance`.

---

## 8. `V5-R06` — `PC4-5`: restauracion de foco en la frontera C4

Si una frontera C4 es bloqueada por un `LinkedPropertyEditor`:

```text
1. identificar el PRIMER editor ofensor
2. devolver el foco al editor
3. conservar el draft
4. presentar la indicacion
```

**Solo entonces** el mensaje puede decir:

| Situacion | Mensaje |
|---|---|
| `=Hol` sin seleccion | «Selecciona una variable de la lista o cancela este cambio con Escape.» |
| `Reference → DraftLiteral` | «Confirma el cambio a literal con Enter o cancelalo con Escape.» |

> **Solo se menciona Escape como accion inmediata si el foco YA fue devuelto al editor.** Si restaurar
> el foco falla, el mensaje **debe ofrecer una accion realmente alcanzable**. Un mensaje que nombra una
> salida que en ese instante no funciona es peor que no nombrarla.

**`Cerrar` / `Cancelar` sigue FUERA de C4** (`V3-R09`).

---

## 9. `V5-R07` — `OQ4-1`: el tipo incompatible no va a Owner Validation

**No se exige un caso de tipo incompatible en `G4H`.**

Con **un solo tipo productivo**, ese estado queda cubierto **obligatoriamente** por los tests
automaticos del kernel/Application (`V5-R01`). **No se fabrica corrupcion manual** para la validacion
del dueno: pedirle que construya a mano un estado que produccion no puede alcanzar no valida nada y
arriesga su dibujo.

---

## 10. Gates de V5

> **PROPUESTOS, NO AUTORIZADOS.** Ocho, refinados.

| Gate | Contenido |
|---|---|
| **G4A** | catalogo + **frontera de `usable registry`** + **target snapshot** + primitivas de inspeccion property-specific. **`VerticalClearance` ONLY** |
| **G4B** | kernel generico de Application + fachada `VerticalClearance` + **inspection/scanner compartidos** + **N→1** + **`FindBroken` all** + **`RepairBrokenRack`** + **tests multi-propiedad sinteticos** + **test de tipo incompatible del kernel**. **SIN wiring de binding de `PalletTolerance`** |
| **G4C** | `LinkedPropertyEditor` + reconciler + transiciones de `Source` + **C4 / restauracion de foco** + **estado pendiente visible** + seleccion explicita de autocomplete |
| **G4D** | autoridad del `Type` **persistido** + `ToProjectVariables` **fail-loud** + camino de compatibilidad **productivo** |
| **G4E** | **proof REAL unico de `PalletTolerance`**: baseline GREEN · tests primero → **RED local** · **no commit** · wiring de binding de producto · **GREEN** · commit/push **solo en GREEN** |
| **G4F** | proof real completo de dos propiedades. **`PROOF_SHA` avanza hasta que TODOS los contratos de proof esten GREEN; los fixes cuentan en el AFTER** |
| **G4G** | candidato completo + CI |
| **G4H** | **Owner Validation en AutoCAD 2025**, **sin** caso fabricado de tipo incompatible (`V5-R07`) |

### Tests contractuales explicitos que V5 anade

```text
Absent usable registry + missing target
  → repairable

Readable usable registry + missing target
  → repairable

PresentButUnreadable
  → falla ANTES de la inspeccion por binding
  → repair zero plan

IncompatibleMajor
  → falla ANTES de la inspeccion por binding
  → repair zero plan

target snapshot type mismatch
  → FATAL_INCOMPATIBLE_TARGET
  → no effective
  → no B
  → zero mutation
```

Se mantiene **todo** el oracle de V3/V4, incluida la independencia del oracle productivo respecto del
descriptor y la igualdad `production oracle set == production catalogue set`.

---

## 11. Estado

```text
COORDINATOR PROPOSAL V5 — FOURTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V5
Architect   = NOT REVIEWED V5
Consensus   = NOT REACHED

Origen: Architect Review de V4 @ 24c004ecd39ab1bc6a0c34978a052c6b71a35144
        (NOT AGREED · 0 BLOCKER · 2 MATERIAL · 3 MINOR)

CERRADO y no reabierto:
  AGREE V4-R01 resolver type compatibility
  AGREE V4-R02 test seam
  AGREE RepairBrokenRack rack-scoped
  AGREE R-02

V5 NO introduce ningun apartamiento respecto de la review de V4.

Siguiente paso: Architect Review sobre V5.
Los dos puntos que V5 somete son las DOS PRECISIONES: el target snapshot que hace
ejecutable FATAL_INCOMPATIBLE_TARGET sin tocar tipos, store ni ProjectVariable.Create;
y la frontera UsableProjectVariablesRegistry, que convierte
present-but-unreadable != absent en una garantia de TIPO en vez de una
convencion por call site.
```

**Historial conservado:** [I-48-proposal-v1.md](I-48-proposal-v1.md),
[I-48-proposal-v2.md](I-48-proposal-v2.md), [I-48-proposal-v3.md](I-48-proposal-v3.md) e
[I-48-proposal-v4.md](I-48-proposal-v4.md) quedan **intactas**. Cada una debe poder leerse tal como fue
revisada.
