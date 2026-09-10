# I-48 — Proposal V8: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V8 — SEVENTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V8
> Architect   = NOT REVIEWED V8
> Consensus   = NOT REACHED
> ```
>
> **V8 reconcilia la SEPTIMA Architect Review, la de V7.** Esa review termino:
>
> ```
> Architect = NOT AGREED
> BLOCKER  = 0
> MATERIAL = 1   (la compuerta de semantic-usability no cubre la re-lectura de
>                 commit del executor, y V7-R02 afirma que si la cubre)
> MINOR    = 0
> ```
>
> **Un solo material, aceptado sin apartamientos.** V8 es la Proposal mas corta de la cadena porque
> **no reabre nada**: cierra un unico hueco y declara fuera de alcance la unica pregunta abierta.
>
> La revision de V8 **no ha ocurrido**.
>
> ```
> Base:              18f9438a8692bc17cadb63882fe6043dad5b8738  (Proposal V7 revisada)
> Codigo auditado:   e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:         I-48-proposal-v1..v7 quedan INTACTAS
> Estado de gates:   G0 · G1 · G1.1 · G2A..G2G · siete Architect Reviews
>                    · **G2H EN CURSO** · G3 consenso PENDIENTE
> ```

## 0. Decisiones CERRADAS que V8 no reabre

```text
AGREE V7-R01 duplicate VariableId fail-closed
AGREE V7-R03 rack repairability atomica
AGREE V7-R04 primitiva unica del Type persistido
AGREE V7-R05
AGREE cierre OQ6-2 / unified VariableId lookup

AGREE V4-R01 resolver type compatibility
AGREE V4-R02 test seam
AGREE RepairBrokenRack rack-scoped
AGREE R-02
```

Todo lo demas de V2..V7 se conserva sin cambio.

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review de V7 | Severidad | Resuelto en |
|---|---|---|
| **la re-lectura de commit del executor esquiva la semantic-usability** | MATERIAL | **`V8-R01`** + **`V8-R02`** + **`V8-R03`** + **`V8-R04`** |
| `OQ7-1` — comparar registro de preflight vs commit | (abierta) | **`V8-R05`** — **FUERA DE ALCANCE** |

---

## 2. `V8-R01` — La semantic-usability tambien en la re-lectura de COMMIT

**Aceptado `PC7-1`.** El defecto concreto de V7, sobre codigo verificado:

```text
PRE-FLIGHT
  registry A
  → semantic-usability acreditada
  → plan

COMMIT
  registry B = ProjectVariablesRegistry.Read(...)        // l.254
  → RegistryMutation.ApplyTo(registry B.Document)        // l.260
```

**`registry B` es una lectura NUEVA. La acreditacion realizada sobre `A` NO acredita `B`.** `V7-R02`
afirmaba que `RegistryMutation` nunca recibe identidad ambigua «porque `V7-R01` lo bloquea antes de
planificar»; esa frase es **falsa para este camino**, y el store no rechaza duplicados, asi que `B`
pasa como `Readable`.

### La regla, extendida

> **Todo `ProjectVariablesDocument` que vaya a convertirse en entrada de lookup semantico o de mutacion
> identificada por `VariableId` debe provenir de una acreditacion de semantic-usability correspondiente
> a ESA lectura concreta.**

Aplica **tanto en planificacion como en commit**.

### Secuencia obligatoria del executor

Cuando `plan.RegistryMutation.Kind != None`:

```text
1. lastRead = ProjectVariablesRegistry.Read(...)

2. semanticRead = acreditar semantic-usability DE lastRead
                  usando la MISMA frontera/primitive de V7-R01

3. si semanticRead FAIL:
     → abortar la transaccion
     → razon visible
     → NO llamar RegistryMutation.ApplyTo
     → NO escribir registry
     → NO escribir ni redibujar destinations

4. si semanticRead SUCCESS:
     → RegistryMutation.ApplyTo(semanticRead.Document)
     → ProjectVariablesRegistry.TryWrite(..., lastRead, changedDocument, ...)
```

**No se fijan nombres exactos.** El punto contractual es uno:

> **PROHIBIDO `ApplyTo(lastRead.Document)` sin acreditar ESA `lastRead`.**

El documento entregado a `ApplyTo` debe proceder **conceptualmente del resultado exitoso de la
acreditacion semantica**. Notese que `TryWrite` sigue recibiendo `lastRead` —es la lectura fisica que
su escritura necesita— mientras que **`ApplyTo` recibe el documento acreditado**: son dos papeles
distintos y no se confunden.

Con esto la garantia pasa a ser **por construccion**, tambien durante el commit:

```text
RegistryMutation nunca opera sobre identidad VariableId ambigua
```

---

## 3. `V8-R02` — Disposicion exacta en commit

```text
Absent                          → semantic usable empty  → el commit puede continuar
Readable + unique VariableIds   → semantic usable        → el commit puede continuar
Readable + duplicate VariableId → semantic-integrity failure → ABORT
PresentButUnreadable            → ABORT
IncompatibleMajor               → ABORT
null / not consulted            → ABORT
```

En particular, **`Readable + duplicate VariableId` NO llega a**:

- `RegistryMutation.Rename`;
- `RegistryMutation.ChangeValue`;
- `RegistryMutation.Remove`;
- `RegistryMutation.Add`;
- `ApplyTo`.

**Asi las semanticas historicas `FIRST wins`, `LAST wins` y `RemoveAll` son INALCANZABLES cuando la
identidad es ambigua.** Esto es lo que cierra el escenario que la review nombro: `Remove` sobre un
documento duplicado usa `RemoveAll` y **borraria ambas entradas** — perdida de datos bajo identidad
ambigua, por el unico camino que la compuerta no cubria.

**No se repara el registro. No se elige first/last.**

---

## 4. `V8-R03` — Frontera Plugin / Application

La semantica de **«¿este `ReadResult` es semanticamente usable?»** **sigue siendo de la capa
Application/pura** definida por `V7-R01`.

**El executor (Plugin):**

- realiza la **lectura fisica**;
- **entrega `lastRead`** a esa misma frontera de Application;
- **consume** success/failure;
- **aborta la transaccion** ante failure.

**NO reimplementa en Plugin:** busqueda de duplicados · mapping de `Type` · reglas de registro ·
identidad.

> **Debe reutilizar la MISMA acreditacion semantica que usan las demas superficies** (`V7-R02`). Una
> segunda implementacion en el Plugin seria exactamente la duplicacion de autoridad que toda esta
> cadena de Proposals viene eliminando.

## 5. `V8-R04` — Zero mutation ante fallo en commit

Si la re-acreditacion de `lastRead` falla:

```text
MutationExecutionResult   = Aborted
transaction               = no commit
registry write            = 0
destination/view writes   = 0
RegistryMutation.ApplyTo  = NO debe ejecutarse
```

El mensaje de error **debe ser visible y preservar la razon de semantic-integrity failure**.

El executor **ya posee** un camino de abort —`return MutationExecutionResult.Aborted(registryError)`
(l.265)— y V8 **reutiliza conceptualmente ese mecanismo**: es una comprobacion mas en un punto que ya
sabe abortar, no una reestructuracion.

**No se introduce partial commit.**

---

## 6. `V8-R05` — `OQ7-1` concurrencia: FUERA DE ALCANCE

**I-48 NO introduce:**

- comparacion entre el registro de preflight y el de commit;
- version token;
- optimistic concurrency;
- hash / ETag;
- rechazo de plan stale por cualquier cambio del registro.

**La relectura de commit se conserva** tal como esta.

> **Contrato:** si la `lastRead` de commit **cambio** pero **sigue siendo semanticamente usable**, I-48
> **no la rechaza por el mero hecho de haber cambiado**.

V8 solo exige que **esa lectura mas reciente** pase las mismas invariantes de seguridad necesarias
antes de lookup o mutacion. **La concurrencia general queda fuera del alcance de I-48.**

Si durante la implementacion aparece evidencia de que esta politica existente produce un defecto
material **independiente de la identidad**: **STOP y escalar**. **No se amplia ahora por hipotesis.**

---

## 7. Tests contractuales adicionales

```text
1. preflight sobre registry unique/usable
   + commit reread Readable CON duplicate VariableId
     → semantic accreditation FAIL
     → execution abort
     → ApplyTo NO alcanzado
     → registry write 0
     → view writes 0

2. commit reread Readable + unique
     → accreditation SUCCESS
     → la mutacion puede continuar

3. commit reread Absent
     → usable empty
     → comportamiento permitido cuando la mutacion concreta lo soporte

4. commit reread PresentButUnreadable   → abort ANTES de ApplyTo
5. commit reread IncompatibleMajor      → abort ANTES de ApplyTo

6. executor / commit path
     → consume la MISMA semantic-usability primitive de Application
     → NO hay logica de duplicate-id paralela en Plugin

7. RegistryMutation None
     → V8 NO exige una relectura nueva unicamente para esta compuerta
```

> **Sobre el test 1, y es el punto que decide si el contrato quedo probado:** la evidencia critica
> **no** es solo que falle `TryWrite`. Debe probarse que **la acreditacion semantica falla ANTES de
> `ApplyTo`**, porque **`TryWrite` por si solo no detecta un `VariableId` duplicado** — el store no lo
> valida. Un test que solo compruebe el resultado final pasaria igual con la compuerta ausente.

**Se mantienen todos los tests contractuales de V7 no sustituidos.**

---

## 8. Gates de V8

> **PROPUESTOS, NO AUTORIZADOS.** Ocho, sin cambios de estructura respecto de V7.

| Gate | Contenido |
|---|---|
| **G4A** | semantic registry · integridad de `VariableId` duplicado · descriptor set · target snapshots · mapping del `Type` · `InspectBinding` |
| **G4B** | kernel · **unified `VariableId` lookup** · rack repairability · resolver · `FindBroken` · `RepairBrokenRack` · N→1 · proofs sinteticos · **+ re-acreditacion de la re-lectura de commit (ver abajo)** |
| **G4C** | `LinkedPropertyEditor` / C4 |
| **G4D** | persisted `Type` / `ToProjectVariables` |
| **G4E** | proof REAL de `PalletTolerance`, RED→GREEN local |
| **G4F** | proof real completo |
| **G4G** | candidato + CI |
| **G4H** | Owner Validation |

**Obligacion anadida explicitamente a `G4B`** —o al gate donde caiga la migracion del executor segun
la division final—:

```text
commit re-read
  → semantic-usability re-accreditation
  → ANTES de RegistryMutation.ApplyTo
```

> **Esta proteccion NO se deja para `G4G`/CI.** Forma parte de la **implementacion funcional de la
> autoridad unificada**: es la que hace verdadera la frase de `V7-R02`, no un chequeo de calidad
> posterior.

---

## 9. Estado

```text
COORDINATOR PROPOSAL V8 — SEVENTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V8
Architect   = NOT REVIEWED V8
Consensus   = NOT REACHED

Origen: Architect Review de V7 @ 18f9438a8692bc17cadb63882fe6043dad5b8738
        (NOT AGREED · 0 BLOCKER · 1 MATERIAL · 0 MINOR)

V8 NO introduce ningun apartamiento.

V8 NO reabre:
  V7-R01 duplicate-id fail-closed · V7-R03 rack repairability
  V7-R04 shared Type mapping · V7-R05 · unified VariableId lookup
  R-02 · RepairBrokenRack rack-scoped · resolver type compatibility · test seam

Siguiente paso: Architect Review sobre V8.
Lo que V8 somete es una sola cosa: que la garantia de V7-R01 pase de valer
"en planificacion" a valer "en toda lectura que se convierta en entrada de
lookup o de mutacion", incluida la re-lectura de commit del executor.
```

**Historial conservado:** [V1](I-48-proposal-v1.md) · [V2](I-48-proposal-v2.md) ·
[V3](I-48-proposal-v3.md) · [V4](I-48-proposal-v4.md) · [V5](I-48-proposal-v5.md) ·
[V6](I-48-proposal-v6.md) · [V7](I-48-proposal-v7.md) quedan **intactas**.
