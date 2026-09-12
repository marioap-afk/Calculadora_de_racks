# I-48 — Proposal V3: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V3 — SECOND ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V3
> Architect   = NOT REVIEWED V3
> Consensus   = NOT REACHED
> ```
>
> **V3 reconcilia la SEGUNDA Architect Review, la de V2.** Esa review termino:
>
> ```
> Architect = NOT AGREED
> BLOCKER  = 1   (RepairBroken: bloqueo permanente con dos vinculos rotos)
> MATERIAL = 4   (Resolve documento-como-predicado; orden de gates N->1;
>                 LostFocus destruye vinculos; disposicion RED de G4E1)
> MINOR    = 3   (origen y orden de P; auto-seleccion; Cerrar gateado)
> AGREE R-02
> ```
>
> La revision de V3 **no ha ocurrido**. Ninguna `V3-RNN` es firme.
>
> ```
> Base:              7f6680d21a0533dc1d8c65ca487ba0569457142f  (Proposal V2 revisada)
> Cadena previa:     a061148 (V1.1 revisada) · edacf7d (G1) · 7a9471f (G1.1)
> Codigo auditado:   e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:         I-48-proposal-v1.md e I-48-proposal-v2.md quedan INTACTAS
> Estado de gates:   G0 · G1 · G1.1 · G2A · G2A.1 · G2A.2 · Review V1.1 · G2B · Review V2
>                    · **G2C EN CURSO** · G3 consenso PENDIENTE
> ```

## 0. Que se conserva de V2

**`R-02` queda CERRADO tal como esta en V2.** El Arquitecto emitio `AGREE R-02` y retiro las dos
restricciones de su propia `PC-7` (conteo literal de miembros; prohibicion de pasar el descriptor como
parametro). **No se reabre.** `V3-R10` solo anade una distincion que la review pidio como aclaracion,
no como desacuerdo.

Se conservan sin cambio: el catalogo inmutable Selective-specific en Application (`R-02`), el estado
declarativo `{CommittedLiteral, Source}` (`R-03`), la resolucion de 20.13 —**`Link` congela el ultimo
literal COMPROMETIDO**— (`R-04`), la regla de que el popup interno no es `LostFocus` del control
compuesto (`R-04`), el filtrado sin resolucion por nombre (`R-06`), `fail-loud` de tipos (`R-09`), la
idempotencia de `Reference(X) → Reference(X)` (`R-14`), y `D-01`..`D-18` de V1.1 en todo lo que
ninguna `R` toca.

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review de V2 | Severidad | Resuelto en | Resultado |
|---|---|---|---|
| **`RepairBroken` bloqueo permanente con dos rotas** | **BLOCKER** | **`V3-R01`**, **`V3-R02`** | **Diagnostico ACEPTADO; solucion MODIFICADA** (ver §3) |
| `Resolve(document)` como predicado por propiedad | MATERIAL | **`V3-R03`** | ACEPTADO — matriz explicita |
| Orden de gates: N→1 despues de activar | MATERIAL | **`V3-R13`** | ACEPTADO |
| `LostFocus` destruye un vinculo | MATERIAL | **`V3-R06`**, **`V3-R07`** | ACEPTADO |
| `G4E1` deja un SHA remoto en RED | MATERIAL | **`V3-R14`** | ACEPTADO |
| Origen y orden de `P` | MINOR | **`V3-R05`** | ACEPTADO |
| Auto-seleccion con candidato unico | MINOR | **`V3-R08`** | ACEPTADO |
| `Cerrar` gateado por C4 | MINOR | **`V3-R09`** | ACEPTADO |
| `OQ2-1` — catalogo vs descriptor | (abierta) | **`V3-R10`** | RESUELTA |
| `OQ2-2` — `FindBroken` expone una sola | (abierta) | **`V3-R11`** | RESUELTA |
| `OQ2-3` — `Link`/`Unlink` ante rota ajena | (abierta) | **`V3-R12`** | RESUELTA |
| Consecuencia sobre la metrica | (derivada) | **`V3-R15`** | AFTER corregido |

**Todo lo bloqueante y material queda aceptado.** El unico punto donde V3 **no** adopta literalmente
lo que el Arquitecto propuso es la **forma** de la solucion del BLOCKER, y se argumenta en §3.

---

## 2. Apartamiento declarado: `PC2-1` no se adopta en su forma de reparacion parcial

> **`PC2-1` no se adopta en su forma de reparacion parcial; se adopta el BLOCKER pero la solucion pasa
> a repair-batch rack-scoped, porque una reparacion parcial no puede producir un `EffectiveOutput`
> completo.**

**El diagnostico del Arquitecto es correcto y se acepta entero:** con dos bindings rotos en el mismo
rack, `RepairBroken(propertyA)` retira A en un clon pero el resolver completo **sigue fallando por B**,
y el plan queda vacio; simetricamente con B. El rack queda ineditable e irreparable.

**Lo que no se adopta es el criterio de exito propuesto:**

```text
repair A → success dejando B broken        ← RECHAZADO como criterio
```

**Por que, y esta sobre codigo verificado.** `RackMutation` exige un `EffectiveOutput`
(`MutationPlan.cs:158`) y el executor **lo usa para resolver la geometria y redibujar TODAS las
vistas**:

```csharp
// ProjectVariableMutationExecutor.cs:167
var system = new SelectiveGeometryResolver().Resolve(rack.EffectiveOutput, catalog);
```

Mientras B siga rota **no existe un diseno efectivo completo** que entregarle. Una reparacion parcial
tendria que inventar un `EffectiveOutput` incompleto —o dejar el rack sin redibujar— y las dos cosas
rompen el contrato del executor. **El problema no es que reparar A este mal: es que la unidad de
reparacion no es la propiedad, es el RACK.**

---

## 3. `V3-R01` — `RepairBrokenRack`: rack-scoped y atomico

**Contrato: la reparacion es rack-scoped y atomica sobre el conjunto completo `B` de bindings rotos
REPARABLES del rack.**

```text
 1. resolver la authored authority del rack
 2. inspeccionar CADA binding conocido de forma property-specific
 3. clasificar cada uno:
      - healthy
      - broken por VariableId inexistente        = REPARABLE
      - malformed / unknown / uninterpretable    = FATAL
 4. si existe alguno FATAL:  abort / zero plan
 5. B = TODOS los bindings reparable-broken del rack
 6. B debe ser NO VACIO
 7. presentar al usuario TODOS los PropertyIds, VariableIds y StoredLiterals de B
 8. exigir confirmacion explicita DEL BATCH
 9. clonar el authored UNA vez
10. retirar TODOS los bindings de B
11. NO cambiar sus stored literals
12. conservar intactos TODOS los bindings healthy
13. resolver el documento completo
14. si no resuelve:  abort / zero plan
15. emitir UNA RackMutation
```

**No hay reparacion parcial que deje otro roto vivo.**

El **nombre exacto del metodo no se fija todavia**; semanticamente es `RepairBrokenRack` / rack-scoped.

**`RACKVARIABLES` debe comunicar que la accion reparara N bindings rotos DEL RACK**, y no presentar
una confirmacion falsa sobre una sola propiedad. Hoy el mensaje de `!confirmed` afirma que la variable
de la propiedad elegida no existe — con dos bindings eso puede ser **factualmente falso**.

**Tests contractuales:**

| Escenario | Resultado exigido |
|---|---|
| `A healthy + B broken` → repair batch | `B` se desvincula usando su **stored literal**; **`A` sigue vinculada** |
| `A broken + B broken` → repair batch | **ambas** se desvinculan; **ambos stored literals gobiernan**; effective completo |
| `broken + malformed/unknown` | **zero plan** |
| `confirmed = false` | **zero plan** |

**Rechazado expresamente como criterio:** *«repair one of two broken succeeds leaving the other
broken»*.

### `V3-R02` — `FindBroken` expone TODAS las rotas

V2 y G1 dependian del **primer fallo del resolver**. Con multiples propiedades, `FindBroken` debe
inspeccionar los bindings **property-by-property** y devolver **todas** las referencias
reparable-broken del rack.

- **No resolver una propiedad por el fallo de otra.**
- **malformed/unknown permanece fail-closed** y **NO** se convierte automaticamente en «broken
  reparable».
- La UI **puede mostrar varias filas**, pero la operacion de Repair es **rack-scoped sobre el conjunto
  `B` completo**.

---

## 4. `V3-R03` — Matriz explicita: pregunta de PROPIEDAD vs pregunta de DOCUMENTO

El Arquitecto mostro que el patron «`Resolve(document).IsSuccess` como predicado por propiedad» es
**sistemico**: seis llamadas en el preflight, y el resolver **corta al primer fallo**. V3 lo separa en
dos clases y obliga a declarar cual aplica.

### Property-scoped

Usar inspeccion/resolucion **property-specific** para:

- determinar si **UN** binding es `healthy` / `broken` / `malformed`;
- obtener su `VariableId`;
- obtener su effective concreto **cuando sea resolvible**;
- obtener el `StoredLiteral` correcto;
- construir `FindBroken`;
- construir el conjunto `B` de Repair;
- construir `P` para una variable objetivo **despues** de la authority.

**NO usar `Resolve(document).IsSuccess` como sustituto de ninguna de estas preguntas.**

### Document-scoped

**Toda operacion que vaya a producir una `RackMutation` exige un effective final COMPLETO**, porque el
executor lo consume para redibujar (§2):

- `Link`;
- `Unlink`;
- el reconciler de estado final del editor;
- `ChangeValue`;
- `UnlinkAllAndDelete`;
- `RepairBrokenRack` **DESPUES** de retirar todo `B`.

### Contrato deliberado que se deriva

**`Link` y `Unlink` estan fail-closed ante un roto AJENO del mismo rack.** Si cualquier otra propiedad
del rack esta rota, esas operaciones **abortan**, porque no pueden producir un `EffectiveOutput`
completo. **El usuario debe reparar el rack primero.** No es deuda ni efecto accidental (`V3-R12`).

---

## 5. `V3-R04` — *Property-sensitive semantic surfaces*, no «seis superficies»

Las seis superficies de G1 siguen siendo los seis **hotspots/hardcodes LEGACY** —`EffectiveResolver`,
`WithDesign`, `Unlink`, `UnlinkAllAndDelete`/materializacion, `FindBroken`, `EditorOpen`— pero **ya no
se presentan como el inventario completo de consumidores semanticos**. La review de V2 lo demostro
encontrando uno mas.

La arquitectura de V3 debe cubrir **ademas, como minimo**:

- `Link` / compatibilidad de tipo;
- **`RepairBrokenRack`**;
- el **reconciler de estado final** de `RACKEDITAR`.

Se adopta el concepto **`property-sensitive semantic surfaces`**, y **el oracle/matriz de contratos
cubre COMPORTAMIENTOS, no un numero fijo de clases**.

---

## 6. `V3-R05` — `P` nace DESPUES de `AuthoredAuthority`

`ProjectVariableConsumerProbe` se **mantiene** como `Positive / Negative / Indeterminate` para decidir
**alcance**. No se convierte en productor de conjuntos.

```text
probe siblings
  → todos Positive
  → SelectiveAuthoredAuthority.Resolve
  → authority = Single
  → derivar P desde authority.Authored.PropertyValues
  → P = bindings cuyo VariableId == target
  → ordenar P por PropertyId con comparacion Ordinal
  → P no vacio
```

**No se deriva `P` de un sibling antes de la authority.**

**Contrato / test de caracterizacion que se anade** —porque es una dependencia portante y hoy no esta
escrita en ninguna parte—:

```text
siblings identicos salvo PropertyValues distintos
  → SelectiveAuthoredAuthority = Divergent
```

El caso que la review planteo:

```text
Vista A: Clearance       → X
Vista B: PalletTolerance → X
```

**debe abortar por divergencia authored aunque ambas probes den `Positive`.** Hoy lo hace porque
`PropertyValues` participa en la comparacion estructural JSON; el test lo convierte en garantia.

Una `PropertyId` desconocida, un `Kind` desconocido o un `VariableId` ilegible **continuan produciendo
`Indeterminate` / fail-closed**.

---

## 7. Reglas de UI

### `V3-R06` — `LostFocus` NUNCA cambia `Source`

Refina `R-05`/`R-11` de V2, cuya **interaccion** creaba la asimetria que la review encontro.

```text
Committed Source = Literal
DraftLiteral valido
LostFocus DEL CONTROL COMPUESTO
  → PUEDE comprometer, porque Source sigue siendo Literal
```

```text
Committed Source = Reference
DraftLiteral
LostFocus
  → NO compromete
  → Source sigue siendo Reference
  → el draft sigue PENDIENTE
```

- **`Reference → Literal` requiere Enter explicito.**
- **`LostFocus` nunca crea `Literal → Reference` ni `Reference → Reference`**: esas exigen seleccion
  explicita.

> **Regla general: `LostFocus` puede comprometer un valor solo si NO cambia `Source`.**

### `V3-R07` — C4 no puede convertir `Source` en silencio

Un **draft que cambia `Source`** y esta pendiente **NO** puede volverse commit solo porque una frontera
generica llame a `CommitPendingEditors()`.

Para un draft `Reference → Literal` **aun no confirmado**, `TryStage` (o el adapter equivalente) debe
**BLOQUEAR** la frontera con un mensaje equivalente a:

> «Confirma el cambio de fuente con Enter o cancelalo con Escape.»

**No se aplica automaticamente el cambio de `Source`.** Para `Literal → Literal` se mantiene el
comportamiento normal de dos fases / `LostFocus`.

### `V3-R08` — Autocomplete SIN auto-seleccion

Aunque el filtro produzca **un unico resultado**:

- **NO** se auto-selecciona de modo que `Enter` resuelva **solo por haber escrito texto**;
- el **texto libre unicamente FILTRA**.

**Seleccion explicita** significa: **clic humano** en una opcion; **o** navegacion de teclado explicita
que **mueve la seleccion** a una opcion **+ Enter**. La seleccion **porta el `VariableId`**. **No se
resuelve por nombre.**

### `V3-R09` — `Cerrar` / `Cancelar` NO es C4

`Cerrar` / `Cancelar` **sin guardar NO es frontera de escritura**. Un `InvalidDraft` **no puede atrapar
al usuario en la ventana**.

- Cerrar **puede descartar explicitamente** los cambios/pendientes de la sesion **sin persistir**.
- `Escape` dentro del `LinkedPropertyEditor` **restaura el committed del campo**.
- **No confundir** cerrar/cancelar con `Actualizar` / `Insertar` / navegacion, que **si** exigen
  reconciliar estado.

---

## 8. `V3-R10` — El catalogo COMPLETO no es dependencia ambiental

**`R-02` de V2 permanece aceptada**: un **descriptor individual** puede pasarse **como valor
inmutable** a helpers puros. Eso no es service locator.

**Pero se distingue explicitamente:** **NO** convertir el **catalogo completo** en dependencia
ambiental/inyectable enhebrada por Plugin/UI/grafo de llamadas.

- La autoridad productiva permanece **estatica y cerrada en Application**.
- Los consumidores **resuelven un descriptor** desde esa autoridad y pueden pasar **ESE** descriptor.
- **No introducir**: `IEnumerable<Descriptor>` configurable desde fuera; **DI del catalogo**;
  reemplazo del catalogo en runtime.
- Los **helpers internos del propio catalogo** pueden naturalmente operar sobre su coleccion cerrada.

### `V3-R11` — `OQ2-2` resuelta

`FindBroken` debe exponer **todas** las propiedades reparable-broken, no solo la primera. Es necesario
para un diagnostico honesto **y** para que `RepairBrokenRack` pueda presentar el conjunto `B` completo.

### `V3-R12` — `OQ2-3` resuelta

`Link` y `Unlink` ante un roto ajeno: **fail-closed deliberado.** **No es deuda ni efecto accidental.**
Justificacion: cualquier operacion que produce una `RackMutation` requiere un `EffectiveOutput`
completo; si otra propiedad esta rota **no existe un diseno completo seguro que redibujar**.

---

## 9. `V3-R13` — Orden de gates: N→1 **ANTES** de la segunda propiedad

**Se elimina la secuencia de V2** donde `G4F` implementaba N→1 **despues** de activar `PalletTolerance`.
**Toda la infraestructura generica N→1 debe estar implementada ANTES del proof.**

Entra en la generalizacion previa:

- transporte de `P`;
- **un** clone por rack;
- materializacion de **todas** las `P`;
- **una** `RackMutation`;
- **`RepairBrokenRack` batch**;
- **`FindBroken` all**.

## `V3-R14` — Sin SHA remoto en RED

**Se elimina `G4E1` como commit/gate remoto rojo.** Un solo gate de proof:

```text
G4E — SINGLE PROOF GATE
misma ejecucion / worktree:

1. escribir PRIMERO el oracle/tests de PalletTolerance
2. correrlos            → RED esperado, guardar evidencia
3. NO COMMIT
4. activar PropertyId / descriptor / placement
5. completar los cambios property-specific
6. correr focal/system  → GREEN
7. SOLO AHORA commit + push
```

**No se usan pruebas omitidas/en cuarentena solo para representar el RED.** **RED→GREEN es evidencia
LOCAL dentro del gate, no un estado remoto.**

## `V3-R15` — AFTER corregido

```text
PRE_PROOF_SHA = parent del PRIMER commit de G4E
```

Pero **`PROOF_SHA` NO es simplemente el commit de activacion**. `PROOF_SHA` es **el ultimo SHA
necesario para que TODO el proof contractual de `PalletTolerance` este GREEN**, incluidos
cross-property, N→1, Repair, BOM, persistencia y UI.

**Si el gate posterior de proof descubre que hacen falta fixes productivos, esos archivos forman parte
del coste marginal y `PROOF_SHA` avanza.**

Metrica: `PRE_PROOF_SHA..PROOF_SHA`. **Reportar product files y test files POR SEPARADO.** Se mantiene
la clausula anti-gaming de `R-15` (reflexion, config en runtime, CSV, service locator o reduccion de
tests = **FALLO**).

---

## 10. Oracle de V3

Se **conserva el oracle independiente** de V2 —declara la correspondencia `PropertyId → campo` por su
cuenta, y los tests **no** usan los accesores del descriptor para decidir que campo debia cambiar—. Se
**amplia** la matriz de contratos para incluir **como minimo**:

1. **effective field correcto**;
2. **no-cross-write**;
3. **freeze literal correcto**;
4. **`Unlink` materializa la propiedad correcta**;
5. **`UnlinkAllAndDelete` N→1**;
6. **`FindBroken` `StoredLiteral` correcto**;
7. **`EditorOpen` state correcto**;
8. **Repair**: `healthy + broken` · `two broken batch` · `malformed fatal`;
9. **final-state reconciler**: `Literal→Reference` · `Reference→Reference` · `Reference→Literal` ·
   `Reference(X)→Reference(X)`;
10. **contratos de pending / transicion de `Source`**.

> **«10» NO es un numero de aceptacion permanente**: es la matriz contractual **actual**. `V3-R04` ya
> fija que lo que se cubre son **comportamientos**, no un conteo.

**El set del oracle y el set del catalogo deben seguir siendo identicos** una vez el proof este
activado.

---

## 11. Gates de V3

> **PROPUESTOS, NO AUTORIZADOS.**

| Gate | Contenido |
|---|---|
| **G4A** | Descriptor/catalogo + primitivas property-specific. **`VerticalClearance` ONLY** |
| **G4B** | **Toda** la semantica de Application generalizada, todavia con **`VerticalClearance` ONLY**: resolver · `WithDesign` · `Link` compatibility · `Unlink` · **`UnlinkAllAndDelete` N→1** · **`P` desde authority** · **`FindBroken` all** · **`RepairBrokenRack` batch** · `EditorOpen` · caracterizacion de authored-authority |
| **G4C** | `RACKEDITAR`: `LinkedPropertyEditor` · final-state reconciler · transiciones de `Source` explicitas · `pendingAll`/C4 · autocomplete de seleccion explicita |
| **G4D** | Autoridad de tipo: `Type` persistido · `ToProjectVariables` **fail-loud** · compatibilidad en Application |
| **G4E** | **SINGLE PROOF GATE** (`V3-R14`): tests/oracle de `PalletTolerance` escritos primero → **RED local** → **sin commit** → activar `PropertyId` + descriptor + placement → **GREEN** → commit/push **solo en verde**. `PRE_PROOF_SHA` = parent del primer commit de `G4E` |
| **G4F** | **Validacion completa del proof**: propiedades simultaneas · misma `VariableId` N→1 · `healthy+broken` repair · `two-broken` batch repair · BOM/dibujo · persistencia · integracion del editor · homonimos · C4/navegacion. **`PROOF_SHA` = SHA final requerido para que todos los contratos de proof pasen** |
| **G4G** | Candidato completo + CI |
| **G4H** | **Owner Validation en AutoCAD 2025** |

> **`G4F` NO debe introducir infraestructura generica que ya debio existir antes de `G4E`.** Si
> descubre un defecto y requiere fix, **ese fix cuenta en el AFTER** (`V3-R15`).

---

## 12. Owner validation futura — se REGISTRA, no se ejecuta

`G4H` debera incluir **como minimo**:

- ver/editar literal;
- ver `=Variable`;
- seleccionar variable;
- **homonimos**;
- `Reference → Literal` con **Enter**;
- **`LostFocus` NO desvincula**;
- **el popup interno NO compromete literal**;
- dos propiedades simultaneas;
- **la misma `VariableId` gobernando ambas**;
- **copiar un rack con dos bindings a un DWG sin registro**;
- **`FindBroken` muestra ambas**;
- **repair batch confirma ambas y recupera el rack**.

**No ejecutar todavia.**

---

## 13. Estado

```text
COORDINATOR PROPOSAL V3 — SECOND ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V3
Architect   = NOT REVIEWED V3
Consensus   = NOT REACHED

Origen: Architect Review de V2 @ 7f6680d21a0533dc1d8c65ca487ba0569457142f
        (NOT AGREED · 1 BLOCKER · 4 MATERIAL · 3 MINOR · AGREE R-02)

R-02 queda CERRADO tal como esta en V2. No se reabre.

Siguiente paso: Architect Review sobre V3.
Punto que V3 somete con atencion especial: el apartamiento de la seccion 2
—PC2-1 no se adopta en su forma de reparacion parcial— con la evidencia de
ProjectVariableMutationExecutor.cs:167 y MutationPlan.cs:158.
```

**Historial conservado:** [I-48-proposal-v1.md](I-48-proposal-v1.md) (V1 → V1.1) e
[I-48-proposal-v2.md](I-48-proposal-v2.md) quedan **intactas**. Cada una debe poder leerse tal como fue
revisada.
