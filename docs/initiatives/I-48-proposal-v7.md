# I-48 — Proposal V7: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V7 — SIXTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V7
> Architect   = NOT REVIEWED V7
> Consensus   = NOT REACHED
> ```
>
> **V7 reconcilia la SEXTA Architect Review, la de V6.** Esa review termino:
>
> ```
> Architect = NOT AGREED
> BLOCKER  = 0
> MATERIAL = 2   (semantica heredada de duplicados inconsistente por superficie:
>                 el STOP de V6-R01 no podia dispararse;
>                 FindBroken presenta filas reparables en racks con FATAL)
> MINOR    = 1   (conversion mecanica del Type: prohibicion sin mecanismo)
> ```
>
> **Los tres se ACEPTAN.** En `V7-R01` el Coordinador **no adopta literalmente** el cambio minimo que
> el Arquitecto propuso —elegir explicitamente first-wins o last-wins— y adopta una respuesta **mas
> fuerte**: la identidad ambigua es **fail-closed**. Se argumenta en §2.
>
> La revision de V7 **no ha ocurrido**.
>
> ```
> Base:              d64e511df44619a09570c415efc52054465a1e07  (Proposal V6 revisada)
> Cadena previa:     fef7b97 (V5) · 24c004e (V4) · b03a7ae (V3) · 7f6680d (V2) · a061148 (V1.1)
> Codigo auditado:   e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:         I-48-proposal-v1..v6 quedan INTACTAS
> Estado de gates:   G0 · G1 · G1.1 · G2A/.1/.2 · Rev V1.1 · G2B · Rev V2 · G2C · Rev V3 · G2D
>                    · Rev V4 · G2E · Rev V5 · G2F · Rev V6 · **G2G EN CURSO** · G3 PENDIENTE
> ```

## 0. Decisiones CERRADAS que V7 no reabre

```text
AGREE V4-R01 resolver type compatibility
AGREE V4-R02 test seam
AGREE RepairBrokenRack rack-scoped
AGREE R-02
```

Se conserva sin cambio todo lo de V2..V6 que ninguna `V7-R` toque.

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review de V6 | Severidad | Resuelto en |
|---|---|---|
| **semantica de duplicados inconsistente por superficie** | MATERIAL | **`V7-R01` + `V7-R02`** — el Coordinador elige **fail-closed por identidad ambigua**, **NO** first-wins/last-wins |
| **`FindBroken` ofrece MISSING accionable con `FATAL` presente** | MATERIAL | **`V7-R03`** |
| conversion del `Type` sin mecanismo | MINOR | **`V7-R04`** |
| `OQ6-1` — diagnostico de rack bloqueado | (abierta) | **`V7-R05`** |
| `OQ6-2` — `TryFind` | (abierta) | **CERRADA** (§6): converge al lookup unificado |

---

## 2. `V7-R01` — `VariableId` duplicado: IDENTIDAD AMBIGUA, fail-closed

### La evidencia heredada, completa y verificada

El Arquitecto encontro tres superficies con dos reglas. Verificado sobre el codigo, son **CINCO
superficies con TRES comportamientos distintos**:

| Superficie | Codigo | Comportamiento |
|---|---|---|
| `SelectiveEffectiveDesignResolver.Index` | `index[variable.Id] = variable;` (l.157) | **LAST wins** |
| `ProjectVariableMutationPreflight.TryFind` | `return true` al primer match | **FIRST wins** |
| `SelectiveEditorOpen.BoundName` | `return variable.Name` al primer match | **FIRST wins** |
| `RegistryMutation` `Rename` / `ChangeValue` | `Find(next)` = `List.Find` (l.100, 110, 133) | **FIRST wins** |
| `RegistryMutation` `Remove` | `RemoveAll(...)` (l.124) | **elimina TODOS** |

### Conclusion contractual

> **No existe una semantica heredada UNICA de `VariableId` duplicado que pueda preservarse.**

Y por eso **V7 no elige**. Elegir first-wins o last-wins seria inventar una politica nueva **y
presentarla como continuidad**, cuando cualquiera de las dos **cambiaria** al menos dos de las cinco
superficies. V7 adopta:

```text
duplicate VariableId
  → AMBIGUOUS IDENTITY
  → UsableProjectVariablesRegistry construction FAILS
  → fail-closed
  → zero mutation
```

**No se fija el nombre exacto del outcome.**

> **Por que esto es MAS fuerte que el cambio minimo que pidio la review.** `PC6-1` pedia **declarar**
> la regla elegida y admitir que cambia alguna superficie. Pero con **tres** comportamientos —uno de
> ellos «elimina todos»— cualquier eleccion es arbitraria, y una eleccion arbitraria **silenciosa**
> sobre una identidad ambigua es justo lo que I-47 prohibe en todos los demas puntos: la identidad
> viaja por `VariableId`, y un `VariableId` que designa dos cosas **no es una identidad**. Fallar es
> la unica respuesta coherente con la doctrina que I-48 hereda.

### Lo que NO se hace

- **NO** modificar automaticamente el documento;
- **NO** eliminar duplicados;
- **NO** escoger first/last;
- **NO** declarar la deuda persistida «reparada»;
- **NO** cambiar el formato del registro.

### `ProjectVariablesStore` sigue siendo la autoridad de lectura

Continua decidiendo **JSON · schema · tipos persistidos soportados · definition kind · readability**.

> **La nueva comprobacion de unicidad es una PRECONDICION SEMANTICA de la autoridad de identidad,
> POSTERIOR a `Readable`.** No es una segunda politica de lectura.

Se distingue expresamente:

```text
ProjectVariablesReadOutcome.Readable      ← persistence-readable
              vs
usable for unique VariableId authority    ← semantic-usable
```

**Un documento puede ser persistence-readable y NO semantic-usable** si contiene ids duplicados.

### Disposicion de la factory productiva

```text
Absent                          → usable empty
Readable + unique VariableIds   → usable
Readable + duplicate VariableId → semantic-integrity failure / NOT usable
PresentButUnreadable            → NOT usable
IncompatibleMajor               → NOT usable
null / not consulted            → NOT usable
```

La **factory sintetica** mantiene igualmente `VariableId` unico (`V6-R05`).

**El flujo normal no genera duplicados**, porque la creacion usa `VariableId.New()`. Si aparece un
registro historico o editado a mano con duplicados, **la accion correcta de I-48 es BLOQUEAR y
DIAGNOSTICAR, no repararlo automaticamente**.

---

## 3. `V7-R02` — Todas las autoridades de `VariableId` convergen

Consecuencia directa de `V7-R01`: cualquier superficie de I-48 que pregunte

> «¿que variable representa este `VariableId`?»

**debe consumir `UsableProjectVariablesRegistry` o la misma primitiva de lookup.** **No puede conservar
un lookup independiente first/last.**

Incluye **como minimo** las superficies que I-48 toque:

- effective resolver;
- preflight / el actual `TryFind`;
- `SelectiveEditorOpen` / el actual `BoundName`;
- Workspace / opciones de variables;
- `FindBroken`;
- `RepairBrokenRack`;
- `Link`/`Unlink` y toda operacion que necesite resolver el target.

**No se exige reescribir codigo del repositorio que no participe en estos contratos**, pero **ninguna
superficie migrada por I-48 puede mantener su propio criterio de lookup**.

**`RegistryMutation` no debe recibir nunca un estado con id duplicado usable**, porque `V7-R01` lo
bloquea **antes** de planificar una mutacion. Por tanto **no hace falta decidir first/last dentro de
`RegistryMutation` como semantica productiva nueva** — y su `RemoveAll` heredado deja de ser
alcanzable por esta via.

---

## 4. `V7-R03` — Reparabilidad ATOMICA por rack

**Aceptado `PC6-2`.** Tras `InspectBindings(rack completo)` debe existir una evaluacion rack-scoped
equivalente a `AssessRackRepairability(inspections)`. **No se fijan nombres.**

| Estado | Condicion |
|---|---|
| **`HEALTHY`** | ningun `MISSING`, ningun `FATAL` |
| **`REPAIRABLE`** | `B` = todos los `REPAIRABLE_MISSING_TARGET`, `B` **no vacio**, **cero `FATAL`** |
| **`BLOCKED`** | **uno o mas `FATAL`** |

> **REGLA ABSOLUTA: cualquier `FATAL` ⇒ `CanRepair = false` para el rack ENTERO**, aunque ese mismo
> rack contenga tambien `MISSING`.

**`FindBroken` / Workspace:**

- completan **TODO** el scan del rack **antes** de presentar una accion;
- **pueden** mostrar todas las incidencias;
- un `MISSING` dentro de un rack `BLOCKED` **puede mostrarse como DIAGNOSTICO**;
- **NO** se presenta como accion reparable;
- Repair queda **deshabilitado / no ofrecido** para **TODO** el rack;
- el **motivo fatal debe ser visible**;
- **no se expone una lista parcial accionable mientras quedan entradas por clasificar**.

Cuando el rack sea `REPAIRABLE`, `B` = **todos** los `MISSING`, y una accion iniciada **desde cualquiera
de sus filas** sigue siendo **rack-scoped** y confirma **TODO `B`** (`V4-R08`, `V4-R09`).

**`RepairBrokenRack` vuelve a usar o recalcular la MISMA assessment antes de mutar.**

### Casos contractuales

```text
healthy + missing              → REPAIRABLE
two missing                    → REPAIRABLE
missing + incompatible         → BLOCKED
missing + unknown property     → BLOCKED
missing + malformed reference  → BLOCKED
fatal only                     → BLOCKED
all healthy                    → HEALTHY
```

**Todo `BLOCKED`: `zero plan`.**

---

## 5. `V7-R04` — Una UNICA autoridad del token `Type`

**Aceptado `PC6-3`, y se le da el mecanismo que faltaba.** Debe existir **una unica primitiva/mapping**
de token persistido → `VariableType` soportado, **compartida OBLIGATORIAMENTE** por:

```text
ProjectVariablesStore
production UsableProjectVariablesRegistry factory
ProjectVariablesDocument.ToProjectVariables()
```

Conceptualmente algo equivalente a `TryParseSupportedVariableType(token, out type)`. **No se fija el
nombre exacto.** **La misma tabla/regla alimenta las tres superficies.**

**Semantica:**

- el **Store** la usa para decidir si el token persistido es soportado y, por tanto, **si el documento
  puede ser `Readable`**;
- la **factory productiva** recibe un documento **ya `Readable`** y usa **ESA MISMA** conversion de
  forma **mecanica**;
- **`ToProjectVariables()`** usa **ESA MISMA** conversion en `G4D`.

> **NO mantener `Store.IsKnownType` con una tabla y otra tabla o `switch` copiado en la factory o en
> `ToProjectVariables()`.**

**Compartir el mapping NO convierte a la factory ni a `ToProjectVariables()` en autoridades de
readability.** Y tras un `Readable`, un fallo de esa misma conversion seria **invariant violation /
fail-loud**, **nunca** una reinterpretacion silenciosa del documento (`V4-R09`).

## `V7-R05` — Diagnostico de rack bloqueado (cierra `OQ6-1`)

Contractualmente **basta** con:

- mostrar que el rack **NO es reparable**;
- mostrar **la razon o razones fatales** suficientes para actuar o diagnosticar;
- mantener visibles los `MISSING` **si ayudan a explicar el estado**.

**Copiar o exportar el diagnostico es presentacion opcional y queda FUERA.** **No se introducen nuevas
acciones de reparacion para `FATAL`.**

---

## 6. `OQ6-2` — `TryFind`: CERRADA

**El actual `TryFind` NO queda como autoridad independiente.** En `G4B` debe **migrar o quedar
sustituido** por el lookup semantico de `UsableProjectVariablesRegistry` para **cualquier operacion de
I-48 que resuelva un `VariableId`**.

**Misma regla para `BoundName`:** el nombre visible debe pertenecer **al mismo target acreditado que
gobierna el effective**. Que el editor muestre el nombre de una variable y el dibujo tome el valor de
otra es exactamente el fallo que `V7-R01` cierra por arriba.

---

## 7. Tests contractuales adicionales

```text
 1. Readable registry + duplicate VariableId
      → production usable-registry construction FAIL
      → razon de semantic-integrity visible
      → zero mutation

 2. duplicate registry
      → InspectBinding NO elige target por first-wins ni last-wins

 3. synthetic registry con VariableId duplicado
      → construction FAIL

 4. todas las superficies migradas de lookup de VariableId
      → consumen la semantica de lookup unificada
      → sin bucles independientes first/last

 5. rack: healthy + missing        → REPAIRABLE · B contiene el missing
 6. rack: two missing              → REPAIRABLE · B contiene ambos
 7. rack: missing + FATAL_INCOMPATIBLE_TARGET
      → BLOCKED · sin afordancia de Repair · zero plan
 8. rack: missing + FATAL_UNKNOWN_PROPERTY      → BLOCKED
 9. rack: missing + FATAL_MALFORMED_REFERENCE   → BLOCKED

10. FindBroken / Workspace
      → ninguna fila reparable accionable antes del assessment completo del rack

11. Store + production factory + ToProjectVariables
      → comparten la MISMA primitiva de mapping del Type persistido
```

**Se mantienen todos los tests contractuales de V6 no sustituidos**, incluido el **nº 2 de V6** —que el
snapshot incompatible **realmente viaja** hasta `InspectBinding`—.

---

## 8. Gates de V7

> **PROPUESTOS, NO AUTORIZADOS.** Ocho.

| Gate | Contenido |
|---|---|
| **G4A** | descriptor/catalogo · descriptor set validado · target snapshot · factories de usable-registry · **frontera de integridad semantica de `VariableId` duplicado** · **primitiva compartida de mapping del `Type` persistido** · `InspectBinding`. **`VerticalClearance` ONLY** |
| **G4B** | kernel generico · scanner · **assessment de reparabilidad a nivel de rack** · resolver enrutado · **superficies de lookup de `VariableId` unificadas, incluidos `TryFind`/`BoundName` donde aplique** · N→1 · `P` tras la authority · **`FindBroken` all diagnostics + `CanRepair` rack-scoped** · `RepairBrokenRack` · proofs sinteticos. **SIN wiring de binding de `PalletTolerance`** |
| **G4C** | `LinkedPropertyEditor` · reconciler · transiciones de `Source` · restauracion de foco C4 · estado pendiente · autocomplete explicito |
| **G4D** | **`ToProjectVariables` usa el mapping compartido del `Type`** · autoridad del `Type` persistido correcta · **sin afirmar que el mismatch productivo pase a ser alcanzable** |
| **G4E** | **proof REAL unico de `PalletTolerance`**, RED→GREEN local, commit solo en verde |
| **G4F** | proof real completo de dos propiedades |
| **G4G** | candidato completo + CI |
| **G4H** | **Owner Validation en AutoCAD 2025** |

---

## 9. Estado

```text
COORDINATOR PROPOSAL V7 — SIXTH ARCHITECT REVIEW RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V7
Architect   = NOT REVIEWED V7
Consensus   = NOT REACHED

Origen: Architect Review de V6 @ d64e511df44619a09570c415efc52054465a1e07
        (NOT AGREED · 0 BLOCKER · 2 MATERIAL · 1 MINOR)

CERRADO y no reabierto:
  AGREE V4-R01 resolver type compatibility
  AGREE V4-R02 test seam
  AGREE RepairBrokenRack rack-scoped
  AGREE R-02

APARTAMIENTO DECLARADO, y es el unico:
  V7-R01 NO adopta el cambio minimo de PC6-1 -declarar una regla first/last
  elegida-. Adopta fail-closed por IDENTIDAD AMBIGUA, que es mas fuerte. La
  razon esta en la evidencia ampliada: no son dos comportamientos heredados
  sino TRES sobre CINCO superficies, incluido un RemoveAll. Con ese reparto,
  cualquier eleccion es arbitraria, y una eleccion arbitraria silenciosa sobre
  una identidad ambigua contradice la doctrina que I-48 hereda de I-47.

Siguiente paso: Architect Review sobre V7.
```

**Historial conservado:** [V1](I-48-proposal-v1.md) · [V2](I-48-proposal-v2.md) ·
[V3](I-48-proposal-v3.md) · [V4](I-48-proposal-v4.md) · [V5](I-48-proposal-v5.md) ·
[V6](I-48-proposal-v6.md) quedan **intactas**. Cada una debe poder leerse tal como fue revisada.
