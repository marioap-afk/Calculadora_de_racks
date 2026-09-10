# I-48 — Proposal V1.1: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V1.1 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> **Esto es la posicion del COORDINADOR, no un acuerdo.** `Coordinator` y `Architect` **NO** estan
> `AGREED`, y mientras no lo esten **sobre esta misma version** no hay autorizacion para escribir una
> sola linea de produccion (contrato de I-48, §12: *NO IMPLEMENTATION BEFORE CONSENSUS*).
>
> Ninguna decision `D-NN` de este documento es firme. Todas estan **sujetas a Architect Review**, y la
> §20 enumera expresamente donde el Coordinador **espera** ser corregido.
>
> Un hallazgo del Discovery no es una autorizacion. Esta Proposal tampoco.
>
> ```
> Base del analisis:  edacf7d7b280715c4192c779a4c38748d33a56ef  (G1  Discovery)
>                     7a9471f0d5abd815519069f67171800ec29337c9  (G1.1 addendum)
> Codigo auditado:    e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Estado de gates:    G0 hecho · G1 hecho · G1.1 hecho · **G2A.1 EN CURSO** · G3 consenso PENDIENTE
> Version sometida:   V1.1  (sustituye a V1, que NO llego a revisarse)
> ```

### Que cambia en V1.1 respecto a V1 — leer esto antes de revisar

V1.1 es una **correccion previa a la Architect Review**: V1 se publico y se corrigio **antes** de ser
revisada, asi que no hay revision que invalidar. Dos cambios, y **ningun otro**:

| # | Cambio | Alcance |
|---|---|---|
| 1 | **`D-09` corregido y ampliado con `D-09-bis`.** V1 afirmaba que la UI «sigue sin ver `VariableId`». Era **incorrecto** —la UI ya lo transporta hoy, y **debe** seguir haciendolo para que los homonimos sigan siendo distinguibles— y ademas **se contradecia con `D-10`**. `D-09-bis` fija la frontera real: **transportar identidad opaca** SI, **ejercer semantica** NO. `D-10` y `D-14` quedan alineados con esa redaccion. | Aclaracion de frontera. **No cambia que decide** ninguna D-NN |
| 2 | **Nueva pregunta `20.13`** al Arquitecto: que literal se congela cuando el usuario **edita el numero y despues vincula** en la misma apertura (`4` o `7`). Apunta a `D-13` y **no se decide aqui**. | Pregunta nueva. `D-13` **no** se modifica |

**`D-01`..`D-18` conservan sus decisiones**, salvo la aclaracion de frontera del punto 1. `Coordinator`
sigue `PROPOSED`, `Architect` sigue `NOT REVIEWED`, la implementacion sigue **BLOQUEADA**.

## 0. Que decide esta Proposal, y que no

**Decide** una forma para el mecanismo de edicion vinculable reusable que I-48 debe fundar, y la
somete a revision.

**No decide** —y cualquier lectura que lo suponga es incorrecta—: que se implemente, cuando, ni en que
orden. Tampoco reabre nada de I-47: la semantica aceptada en
[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) es **entrada**, no opcion. Esta
Proposal cambia **por donde** se ejerce esa semantica; **nunca** la semantica.

Todo lo que sigue se apoya en [I-48-discovery.md](I-48-discovery.md) y su addendum G1.1. Cuando una
decision existe **por** un hallazgo, se cita el hotspot (`H1`..`H10`) o la seccion.

---

## 1. Alcance y objeto de prueba

### D-01 — `selective.palletTolerance` se mantiene como proof of generality

El Discovery (§6) la audito de punta a punta y la encontro **funcional real, no campo muerto**: el
resolver la lee en la linea **anterior** a `clearance` (l.95-96), viaja por la **misma** `ResolveFondo`
y gobierna `BeamLength = frente*count + tolerance*(count+1)`, que llega al **BOM** mas directamente que
la propia holgura.

Es buena prueba porque es **parecida donde debe serlo** —mismo `VariableType` `Length`, misma
granularidad de **escalar global de rack**, mismo editor, override por celda simetrico
(`BeamLengthOverride` ↔ `ClearOverride`)— y **distinta donde muerde**: escribe en **otro campo** del
diseno, que es exactamente el eje sobre el que colapsan `H1`, `H3`, `H4` y `H7`. Una propiedad que
tambien escribiera `VerticalClearance` no probaria nada.

**Prueba obligatoria que se deriva de elegirla: NO-CROSS-WRITE.** Vincular, cambiar y desvincular una
de las dos **no debe alterar** el campo de la otra, en ninguna de las seis superficies de `D-05`.

**Su legado `0.0` sin sentinel NO se corrige en I-48** (`H10`). `PalletTolerance` es `double` plano: un
documento antiguo sin el campo deserializa a `0.0`, y `0` es un valor **valido** segun la validacion
vigente, de modo que legado y tecleado son indistinguibles. Se **caracteriza** —con prueba que fije el
comportamiento actual— y se **preserva**. Corregirlo es cambio de semantica de datos y no es de I-48.

### D-15 — ID22B y ID21 siguen FUERA DE ALCANCE

Sin **parser**, sin **AST**, sin expresiones, sin grafo de dependencias, sin deteccion de ciclos, sin
persistencia de expresiones (**ID22B**); y sin **referencias rack→rack**, sin selector de rack y sin
persistencia nueva para ese caso (**ID21**).

El caracter `=` que `D-10` introduce en la UI **no es un parser**: es un **discriminador de modo** de un
solo caracter en posicion inicial. Que un diseno futuro admita `=Holgura General + 2` es **direccion,
no permiso**, exactamente como lo dejo I-47.

### D-16 — `PropertyValue<T>` no se fuerza al camino real

El Discovery (§2.1, §11.2) encontro que `PropertyValue<T>` es generico, limpio y **no lo usa nadie** en
el camino del Selectivo: lo persistido es `SelectivePropertyValueDocument` y el resolver produce un
`double`.

**Que exista no es razon para adoptarlo.** Migrar el camino a `PropertyValue<T>` seria un cambio
transversal de tipos sin problema que lo justifique — cosmetico, y por tanto riesgo sin contrapartida.
Si el Arquitecto identifica una razon **concreta** (p. ej. que el descriptor de `D-04` lo necesite para
expresar authored/effective sin duplicar), se reconsidera **con esa razon escrita**; no antes.

---

## 2. La autoridad `PropertyId → campo`

Esta es la parte central de la Proposal, y la que responde a `H1`, `H3`, `H4`, `H5`, `H7`.

### D-02 — `PropertyId` se mantiene estable, **Ordinal** y **system-qualified**

Sin cambios respecto a I-47. En concreto:

- **Estable**: el token es lo persistido; renombrar un campo C# no puede moverlo.
- **Ordinal**: comparacion exacta, case-sensitive, sin alias, sin trim, sin normalizacion. Un id
  desconocido es **error visible**, jamas un fallback silencioso.
- **System-qualified**: el prefijo `selective.` **se conserva y es obligatorio**. El Discovery (§6) dio
  la evidencia de por que: existen **tres** `PalletTolerance` independientes —Selectivo
  (`SelectivePalletDesign` l.27), Dinamico (`DynamicRackDesign` l.36) y Push Back
  (`PushBackEditorInputs` l.23)— con defaults y trato del legado distintos. Un `palletTolerance` a secas
  nombraria **tres** propiedades diferentes.

*(Citar esos sistemas no los incorpora: `D-14` y el contrato mantienen que I-48 **no** generaliza a
otros sistemas.)*

### D-03 — Un catalogo **INMUTABLE** y **SELECTIVE-SPECIFIC** de propiedades vinculables

**Propuesta:** declarar en `RackCad.Application`, dentro del area del Selectivo, un **catalogo estatico
e inmutable** de descriptores de propiedad vinculable — uno por `PropertyId` que el build conoce.

Lo que **es**:

- **Inmutable**: se fija en compilacion. Sin registro en arranque, sin `Add`, sin mutacion en runtime.
- **Selective-specific**: describe propiedades **del Selectivo**. No es una tabla de todos los sistemas.
- **Cerrado y enumerable**: preguntarle «que propiedades conoces» devuelve una lista fija, que es lo que
  hoy hace `ProjectPropertyIds.IsKnown` con una igualdad (`H5`).

Lo que **NO es**, y se declara para que no derive hacia ello:

- **No** es un registry global ni un contenedor DI.
- **No** es un service locator: nadie le pide *servicios*, solo **acceso mecanico a un campo**.
- **No** se puebla desde configuracion, catalogo CSV, reflexion ni atributos.
- **No** cruza sistemas.

`ProjectPropertyIds` deja de ser una igualdad y pasa a **derivar** de este catalogo: `IsKnown` se
responde por pertenencia. Esa es la palanca de `H5`, y por eso `D-17` fija su orden.

> **Tension declarada, no resuelta:** un catalogo cerrado en Application es una **autoridad nueva**, y
> el repositorio ha pagado caro las autoridades nuevas. La §20.1 pregunta explicitamente al Arquitecto
> si existe una arquitectura **materialmente menor** que cierre `H1`-`H7` sin fundar esta.

### D-04 — Que declara **como minimo** cada descriptor, y que tiene **prohibido** declarar

**Minimo obligatorio — cuatro cosas, ni una mas:**

| Campo | Para que | Quien lo consume |
|---|---|---|
| `PropertyId` | Identidad persistida; clave del mapa de vinculos | Todos |
| `VariableType` | Tipo que la propiedad **exige** de la variable | `D-08` (validacion de compatibilidad) |
| Acceso **mecanico** al **literal authored** (get/set) | Congelar, materializar, leer el literal almacenado | `WithDesign`, `Unlink`, materializacion, `FindBroken` |
| Acceso **mecanico** al valor **effective** (get/set) | Escribir el valor resuelto en el diseno de dominio | `SelectiveEffectiveDesignResolver` |

«**Mecanico**» es la palabra que gobierna esta decision: cada accesor es **exactamente** un campo, sin
logica, sin redondeo, sin default, sin validacion y sin efecto lateral. Es el reemplazo tipado de las
lineas que hoy estan cableadas (`resolver` l.51/69, `WithDesign` l.326, `Unlink` l.273, `Materialize`
l.369, `FindBroken` l.292).

**Prohibido en el descriptor**, y esto es lo que evita que se convierta en un framework:

- **Labels, tooltips o cualquier texto de WPF.** El descriptor no sabe como se llama la propiedad en
  pantalla.
- **Nada de AutoCAD.**
- **Nada de parsing ni de formateo.** El parseo vive en `LocalizedNumberParser` y sigue ahi (`D-10`).
- **Formulas.** Ver `D-15`.
- **Callbacks arbitrarios de negocio**: sin validadores, sin reglas de rango, sin hooks, sin
  `Func<>` de proposito general. Un descriptor que aceptara un delegado arbitrario **es** el service
  locator que `D-03` dice no ser.

> **Nota deliberada:** el rango y la validacion de `ClearanceBox`/`ToleranceBox` (`>= 0`, Discovery
> §5.1) **se quedan donde estan**. Moverlos al descriptor lo convertiria en autoridad de negocio.

### D-05 — **Una** autoridad, consumida por las **seis** superficies

La misma autoridad `PropertyId → campo` de `D-03`/`D-04` debe ser consumida, **sin una segunda copia**,
por:

| # | Superficie | Que deja de estar cableado | Hotspot |
|---|---|---|---|
| 1 | `SelectiveEffectiveDesignResolver` | l.51 y l.69: el destino del valor resuelto | `H1` |
| 2 | `SelectivePalletDesignDocument.WithDesign` | l.324-327: que literal se congela | `H4` |
| 3 | `Preflight.Unlink` | l.273: que campo se materializa | `H3` |
| 4 | `UnlinkAllAndDelete` / materializacion | l.369-370: campo **y** constante | `H2` |
| 5 | `ProjectVariablesWorkspace.FindBroken` | l.292: de donde sale `StoredLiteral` | `H7` |
| 6 | `SelectiveEditorOpen` | l.121, l.173, l.176: el DTO exclusivo | `H6` |

**Criterio de exito del gate:** tras I-48, el token `SelectiveVerticalClearance` **no** debe aparecer en
ninguna de las seis. Es una condicion verificable —y por tanto una guarda posible— no una aspiracion.

### D-06 — El resolver soporta **varias propiedades simultaneas**, sin orden y sin cross-write

Hoy `Resolve` (l.44-73) recorre todos los bindings pero **cada iteracion pisa la misma local** y el
resultado se vuelca **solo** en `design.VerticalClearance` (`H1`). Lo unico que lo hace seguro es que
`IsKnown` admite una sola propiedad.

**Se propone que el resolver satisfaga tres propiedades, las tres verificables:**

1. **Multi-propiedad**: N bindings resueltos en una pasada, cada uno escrito en **su** campo via el
   accesor effective de su descriptor.
2. **Independencia del orden**: el resultado **no puede depender** del orden de enumeracion de
   `Dictionary<string, …>`. Hoy, con dos ids conocidos, ganaria el ultimo, y el orden de un `Dictionary`
   de .NET **no es contractual**. La propiedad exigida es: para el mismo conjunto de bindings, el mismo
   diseno efectivo, **sea cual sea el orden**.
3. **Cero cross-write**: resolver la propiedad `A` **no toca** el campo de `B`. Es la prueba que `D-01`
   hace obligatoria.

**Lo que NO cambia:** la resolucion sigue teniendo **un solo hogar** —sigue siendo `H1` el unico sitio
donde una referencia se vuelve numero—, y los cinco fallos nombrados de `TryResolveBinding`
(`UnknownPropertyId`, `UnknownReferenceKind`, `MalformedReference`, `BrokenProjectVariableReference`)
se conservan tal cual, incluida la regla de que un roto **aborta** en vez de degradar.

---

## 3. `UnlinkAllAndDelete` y la cardinalidad N→1

### D-07 — El algoritmo, y por que no son N llamadas independientes

`UnlinkAllAndDelete(X)` debe operar asi, **por rack**:

```text
1. Determinar el CONJUNTO P = { p : p es propiedad del rack y su binding apunta a X }
2. Resolver el diseno EFECTIVO actual  ...................... UNA vez
3. Clonar el authored  ..................................... UNA vez
4. Para CADA p en P, sobre ESE MISMO clon:
      materializar el effective actual de p en su literal authored
      retirar el binding de p
5. Eliminar X del registro
6. Resolver el resultado FINAL sobre el registro ya sin X
7. Emitir UNA RackMutation por rack, con ese authored y ese effective
```

**Se mantiene UNA `RackMutation` por rack.** No se propone emitir varias: dos `RackMutation` para el
mismo rack serian **doble escritura sobre las mismas vistas**, no composicion (Discovery §4.4-bis,
hecho 7 y razon 2).

**Atomicidad:** cualquier fallo en cualquier paso, para cualquier `p` de cualquier rack, **aborta la
operacion completa con plan vacio**. Ni el registro, ni un rack, ni una propiedad. Es la regla vigente
del preflight («o plan COMPLETO o nada»), aplicada al caso general.

**Por que NO se modela como N llamadas independientes a `Materialize` desde el authored original** —y
esto es el hallazgo de G1.1, no una preferencia—:

- `Materialize` arranca con `ProjectVariableCloning.Clone(source)` **sobre el `source` recibido**. Dos
  llamadas singulares partirian **ambas del original** y la segunda **descartaria** el trabajo de la
  primera.
- El eje que falta **no es «que propiedad» sino «que CONJUNTO»**. `ProjectVariableConsumer` no lleva
  ningun `PropertyId` (hecho 6) y `Probe` colapsa N coincidencias en un `bool` (hecho 4), aunque
  `Summarize` demuestre que el conjunto **se sabe calcular** (hecho 5).

**El caso que esto cierra**, y que hoy falla en silencio:

```text
Rack A
  selective.verticalClearance -> Variable X
  selective.palletTolerance   -> Variable X
```

Hoy `UnlinkAllAndDelete(X)` materializa **solo** clearance, retira **solo** esa clave, borra `X`, y deja
`palletTolerance` apuntando a una variable inexistente: **fabrica un vinculo roto** —justo lo que
`Delete` existe para impedir—, devuelve `Success` y commitea. El dano aparece despues: `RACKEDITAR` de
ese rack **deja de abrir** y el BOM lo reporta como `BrokenProjectVariableReference`.

**Alcance acotado:** `Delete` solo **bloquea** y su mensaje ya usa `Summarize` (plural correcto);
`ChangeValue` **no** reescribe literales y le basta re-resolver el rack entero. El unico que materializa
por rack es `UnlinkAllAndDelete`.

> **Pregunta abierta real:** de donde sale `P`. `Summarize` ya lo calcula, pero es una **proyeccion de
> presentacion**; usarla como entrada de una mutacion mezcla papeles. §20.4.

---

## 4. Compatibilidad de tipo

### D-08 — **Application** valida `property.VariableType ↔ variable.Type`

Hoy **nadie valida el tipo en Application**. El Discovery (§7, addendum) lo confirmo: `TryFind` (l.413)
solo comprueba **existencia** y **descarta** el `Type`; quien evita una vinculacion incompatible es
`SelectiveBindingOptions.ForLength`, es decir **un filtro de UI**.

**Se propone:**

1. **Application valida la compatibilidad** en el preflight de `Link`, usando el `VariableType` que el
   descriptor declara (`D-04`) contra el `Type` de la variable. Una vinculacion incompatible se
   **rechaza** ahi.
2. **El filtro de UI es COMODIDAD, no frontera de seguridad.** Sigue existiendo —no ofrecer lo que no
   se puede vincular es buena UX— pero deja de ser lo unico que impide el error. Una peticion construida
   sin pasar por la lista debe fallar igual.
3. **Consecuencia obligada:** `ProjectVariablesDocument.ToProjectVariables()` pasa en esta Proposal a
   **`FIX REQUIRED`**. Hoy fabrica `VariableType.Length` fijo (l.81) ignorando `entry.Type`; en cuanto
   ese `Type` sea el dato con el que **alguien decide**, el metodo estaria **afirmando** algo falso.
   Un proyector inocuo puede mentir; una autoridad no.

   *Esto es exactamente la rama que el Discovery §7 anticipo como disparador del cambio de veredicto.
   Se toma aqui, y por eso el veredicto cambia: **`DEFER SAFE` → `FIX REQUIRED BY I-48`**.*

4. **NO se anade ningun segundo `VariableType` en I-48.** El fix es que el metodo **lea** el tipo
   persistido en vez de inventarlo — no que aparezcan tipos nuevos. `VariableTypes.IsSupported` y la
   guarda de lectura del store siguen admitiendo solo `Length`.

---

## 5. Apertura del editor

### D-09 — `SelectiveEditorOpen` deja de exponer un DTO exclusivo

Hoy expone `VerticalClearanceBindingState`, con la propiedad del resultado llamada `VerticalClearance`
y la constante cableada en `BoundName` (l.121) y en `Resolve` (l.173) (`H6`).

**Se propone** sustituirlo por **estado reutilizable direccionado por `PropertyId`**: el mismo trio de
datos que hoy viaja —`IsBound`, `EffectiveValue`, `BoundVariableName` (solo display)— pero **por
propiedad**, de modo que anadir una no cambie la forma del resultado.

**Lo que NO cambia, y es lo importante:**

- **La apertura sigue pudiendo NEGARSE**, con las mismas causas: referencia rota, `Kind` del futuro, id
  ilegible, y registro `PresentButUnreadable`/`IncompatibleMajor` **aunque el rack no este vinculado**.
- **Reparar sigue sin estar aqui** (`D-12`).
- **La UI no adquiere autoridad semantica.** La frontera exacta es `D-09-bis`, y no es «la UI no ve
  `VariableId`».

### D-09-bis — La frontera de la UI respecto a `VariableId` (CORRECCION V1.1)

> **Correccion sobre V1.** V1 afirmaba que la UI «sigue sin ver `VariableId` para decidir nada». Esa
> redaccion era **incorrecta y ademas se contradecia con `D-10`**: la UI **ya transporta hoy** el
> `VariableId` —`ProjectVariableOption.Id`, que `LinkClearance_Click` envia como `option.Id`— y **tiene
> que seguir haciendolo**. Se sustituye por la frontera de abajo, que es la que de verdad importa.

La distincion correcta **no** es «ver o no ver el id». Es **transportar identidad opaca** frente a
**ejercer semantica**.

**La UI PUEDE — y debe:**

- **Recibir y transportar `VariableId`** como **identidad OPACA** de una opcion. Opaca significa: la
  trata como una etiqueta sin estructura, no la interpreta, no la compara contra el registro, no deriva
  nada de ella.
- **Devolver ese `VariableId`** dentro de una **intencion semantica** (`D-13`).

**Y esto no es una concesion, es un REQUISITO.** Es lo unico que mantiene en pie la invariante vigente
de I-47 de que **los nombres homonimos siguen siendo distinguibles**: si la UI devolviera un nombre, dos
variables llamadas igual serian indistinguibles en el viaje de vuelta y se vincularia la equivocada, en
silencio y de forma permanente. Devolver el id **es** el mecanismo que lo impide (`D-10`, regla de
homonimos).

**La UI NO PUEDE:**

| Prohibido | Por que |
|---|---|
| Resolver `VariableId → valor` | La resolucion tiene **un solo hogar**: `SelectiveEffectiveDesignResolver` (`H1`) |
| Consultar el registro `ProjectVariables` | El registro se lee en el limite del Plugin, en transaccion; la UI recibe lo ya proyectado |
| Resolver una variable **por nombre** | El nombre no es identidad; buscar por nombre es exactamente el bug de los homonimos |
| Decidir compatibilidad `Property ↔ Variable` | Es de Application (`D-08`); el filtro de UI es **comodidad, no frontera de seguridad** |
| Decidir la semantica de `Link` / `Unlink` / materializacion | Es de Application (`D-12`); congelar, materializar y negarse ante un roto son respuestas ya probadas |
| Escribir `PropertyValues` | Guarda vigente `NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS` (`D-14`) |

**El nombre sigue siendo SOLO DISPLAY.** `BoundVariableName` y el nombre de cada opcion existen para que
una persona lea; **ninguna decision de la maquina** —ni en la UI, ni en el Plugin, ni en Application—
se toma a partir de esa cadena.

*(Coherencia: `D-10` exige que la identidad interna y persistida sea siempre `VariableId` y que los
homonimos nunca se resuelvan «tomando el primero»; `D-13` hace que la intencion que la ventana devuelve
lleve ese id; `D-14` mantiene que la UI **describe intencion** y Application **decide semantica**. Las
tres dicen lo mismo que `D-09-bis`, y ya no hay contradiccion con `D-09`.)*

---

## 6. El control WPF

### D-10 — Un control reusable `LinkedPropertyEditor`

**Propuesta de forma** (no de implementacion): **un mismo campo** acepta literal o referencia.

| Estado | Se ve como |
|---|---|
| Literal | `6` · `6.5` |
| Referencia | `=Holgura General` |

- **`=` en posicion inicial inicia un draft de referencia** y ofrece sugerencias.
- **Las opciones llegan YA FILTRADAS por Application** (`D-08`): el control **no** decide que es
  compatible ni consulta el registro.
- **La identidad interna y persistida es SIEMPRE `VariableId`.** El control lo **recibe y lo devuelve**
  como **identidad opaca** —eso es exactamente lo que `D-09-bis` autoriza, y lo que hace distinguibles a
  las homonimas—, pero **no lo interpreta, no lo resuelve y no consulta el registro con el**. Nunca el texto. Renombrar la variable
  deja el vinculo intacto y el control solo vuelve a dibujar `=NuevoNombre`.
- **Los nombres duplicados NO pueden resolverse tomando el primero.** Es la regla dura de este control:
  I-47 permite homonimos deliberadamente, asi que «el primero que coincida» vincularia la variable
  equivocada, en silencio y de forma permanente.
- **Las sugerencias homonimas deben poder distinguirse**: mostrando el **valor** y/o un **sufijo corto
  del id**. La eleccion concreta de presentacion es de la implementacion; lo que la Proposal fija es que
  dos homonimas tienen que ser **distinguibles por el usuario**.

**Reutilizacion, con un limite explicito:**

- **`LocalizedNumberParser` se reutiliza** tal cual: es lo que hace deterministas `96.5` y `96,5` sin
  tratar la coma como separador de millares.
- **Comportamiento visual ya probado se reutiliza donde sea razonable** — el idioma de error de
  `NumericField` (borde `#B00020` restaurando la procedencia del brush) es el candidato obvio.
- **NO se hereda ingenuamente de `NumericField`.** `NumericField` **es** un `TextBox` cuyo contrato es
  «esto es un numero»: expone `Value`, `IntegerOnly`, `Minimum`/`Maximum` y valida como medida. `=Nombre`
  **no es entrada numerica**, y heredar obligaria a que un control numerico considere valido algo que no
  lo es. Si se comparte codigo, se comparte **por composicion o por extraccion**, no por herencia.

> **Contexto que conviene tener presente:** `RackSelectiveWindow` **no usa `NumericField` en absoluto**
> (Discovery §5.2), aunque Cantilever y Push Back si (7 archivos). El editor que tiene la propiedad
> vinculable es justo el que no adopto el control. **Adoptar `NumericField` en el resto del Selectivo NO
> es de I-48** (§20.11 lo pregunta).

### D-11 — Estado pendiente vs comprometido del control

Hoy **no existe** para estos campos: `ClearanceBox`/`ToleranceBox` estan **fuera** de `pendingAll`, el
`TextBox.Text` **es** el modelo, y **no hay Escape** (Discovery §5.1).

**Cinco estados, y ninguno colapsable con otro:**

| Estado | Significado |
|---|---|
| `CommittedLiteral` | Vale el literal, y esta resuelto |
| `CommittedReference` | Gobierna una variable, identificada por `VariableId`, y esta resuelto |
| `DraftLiteral` | El usuario escribio un numero y aun no lo confirmo |
| `DraftReference` | El usuario escribio `=…` y aun no lo confirmo |
| `InvalidDraft` | Lo escrito **no** es aceptable: ni numero valido, ni referencia identificable |

**Gestos:**

- **Enter** compromete un draft **valido**.
- **Escape** restaura lo **comprometido** — el gesto que hoy no existe.
- **LostFocus** puede comprometer un **literal valido** o una referencia **inequivocamente
  identificada**.
- **Una referencia inexistente o AMBIGUA permanece invalida.** No se revierte sola y **no se elige en
  silencio**. Esta es la contrapartida directa de la regla de homonimos de `D-10`: preferir un estado
  invalido visible a una eleccion arbitraria invisible.

*(Existe `PendingTextField<T>` (I-43/ADR-0032) con commit en dos fases, pero es `internal` y estos dos
campos quedaron fuera del conjunto pendiente. Si se reutiliza o no es §20.11.)*

### D-12 — Transiciones

| Transicion | Semantica exigida |
|---|---|
| **Literal → Reference** | Es un **`Link`**: el literal authored **se CONGELA**, no se reemplaza. Invariante 1 de I-47. |
| **Reference → Reference** | **Conserva el literal congelado.** Cambiar de variable no es desvincular: el literal que la reparacion necesitaria sigue siendo el original. |
| **Reference → Literal(valor)** | Application debe preservar la semantica logica **`Unlink` (materializa el effective actual) → `SetLiteral(valor)`** dentro de **UNA operacion atomica**, **sin persistir el estado intermedio**. |

Sobre la tercera, que es la delicada: I-47 fijo que desvincular **materializa** y que pasar a literal es
**explicito**, nunca un fallback. Lo que `D-12` anade es que, cuando el usuario **ya escribio otro
numero**, no debe quedar en disco un estado en el que el rack tiene el efectivo materializado pero
todavia no el numero tecleado. La secuencia logica se conserva; **lo que se prohibe es su
observabilidad**.

**Un vinculo roto NO entra por esta via.** `RACKEDITAR` sigue **fail-closed** —no abre— y `RepairBroken`
sigue viviendo **exclusivamente** en `RACKVARIABLES`, explicito y advertido. Que el control tenga ahora
un estado `InvalidDraft` **no** lo convierte en una via de reparacion.

### D-13 — El protocolo de intents: acumular por `PropertyId`, plegar en Application

Hoy `BindingIntent` es **un** intent y `AskBinding` **cierra la ventana** al registrarlo (`H8`). Con dos
propiedades vinculables eso deja de ser una restriccion de pantalla y pasa a ser **de protocolo**: no se
pueden expresar dos gestos.

**Se propone** que la ventana **acumule intenciones semanticas FINALES por `PropertyId`** —una por
propiedad, la ultima que el usuario expreso— y que **Application las pliegue al confirmar**.

Reglas que lo acotan:

- **La UI NO escribe `PropertyValues`.** Ni ahora ni despues. La guarda
  `NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS` sigue valiendo tal cual.
- **«Final» significa final**: la ventana no transporta un historial de gestos, solo el estado ultimo
  por propiedad. No es un log ni un undo.
- **Plegar es de Application**: decidir que significa el conjunto, en que orden y con que atomicidad, no
  es de la ventana.

> **Limite explicito, y no es retorica:** esto **NO autoriza** una reescritura completa del editor, ni
> una migracion a MVVM, ni `DataTemplate`s de propiedades, ni tocar el Editor Shell. Es un cambio del
> **protocolo de salida** de la ventana. El contrato de I-48 pone «reescribir editores completos» fuera
> de alcance y esta decision **no** lo reabre. Alternativa **D** de la §19, rechazada.

---

## 7. Capas

### D-14 — La separacion vigente se mantiene, sin excepciones

| Capa | Papel | Lo que NO hace |
|---|---|---|
| **UI** | **Describe intencion** — y para eso **transporta `VariableId` como identidad opaca** (`D-09-bis`) | No resuelve `VariableId → valor`, no consulta el registro, no resuelve por nombre, no valida compatibilidad, no decide la semantica de Link/Unlink/materializacion, no escribe `PropertyValues`, no conoce AutoCAD |
| **Application** | **Decide semantica** | No dibuja, no abre transacciones, no conoce AutoCAD |
| **Plugin** | **Orquesta** | No re-decide semantica ni re-resuelve |
| **Frontera fisica vigente** | **Escritura y transaccion** | Sin cambios |

**No se introduce acceso a AutoCAD en UI ni en Application pura.** Las guardas de
`ProjectVariablesConformanceTests` que hoy lo fijan —`LA_UI_ENTERA_SIGUE_SIN_CONOCER_AUTOCAD`,
`EL_DOMINIO_ENTERO_SIGUE_SIN_CONOCER_LAS_VARIABLES`, `SIGUE_HABIENDO_UN_SOLO_RESOLVEDOR_EFECTIVO`,
`NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS`— **siguen valiendo sin modificacion**, y esta
Proposal no pide relajar ninguna.

`ProjectVariableMutationExecutor` sigue siendo el **unico** escritor fisico, y el Discovery confirmo por
grep que no conoce ninguna propiedad. `D-07` **no** lo toca.

---

## 8. Metricas

### D-17 — KPI, orden y una hipotesis que hay que someter

**KPI arquitectonico primario: `BEFORE_PRODUCT_FILES = 9`.** Es la cifra que mide el coste de wiring y,
por tanto, si I-48 logro su objetivo.

**Los tests se reportan APARTE y NO se minimizan.** `BEFORE_TEST_FILES = 10` (+5 extendido) mide
**evidencia y cobertura**. Una reduccion artificial de tests **no** es una mejora arquitectonica: si una
propiedad nueva exige **mas** pruebas porque hay mas casos que demostrar, eso puede ser correcto.

**Objetivo de diseno — HIPOTESIS, no criterio de aceptacion.** Anadir una **tercera** propiedad `Length`
comparable deberia requerir aproximadamente:

1. declarar el `PropertyId`;
2. registrar su descriptor en el catalogo de `D-03`;
3. colocar el control reusable de `D-10`;

**sin editar por propiedad** el resolver, `Unlink`, `UnlinkAllAndDelete`, el workspace,
`SelectiveEditorOpen` ni el Plugin.

> **Esto NO es todavia un «criterio de 3 archivos», y no debe endurecerse aqui.** Es una **hipotesis
> verificable** que se somete al Arquitecto (§20.10). Convertirla en criterio rigido antes del consenso
> invitaria a disenar **para la metrica** en vez de para el problema — que es la forma mas facil de
> obtener un 3 que no significa nada.

**El AFTER reportara producto y tests por separado**, nunca sumados en un indicador unico.

---

## 9. Orden de las compuertas

### D-18 — `IsKnown` se relaja **al final**, no al principio

No es una preferencia de secuencia: es seguridad. Hoy `ProjectPropertyIds.IsKnown` (`H5`) es **lo unico**
que vuelve inofensivos `H1` y `H3`, porque impide que un segundo `PropertyId` llegue siquiera al
resolver.

**Consecuencia:** admitir la segunda propiedad **antes** de que la autoridad de `D-05` gobierne las seis
superficies crearia una ventana —posiblemente un solo commit— en la que un rack **puede** vincular
`palletTolerance` y el sistema **resolveria mal en silencio**, escribiendo en `VerticalClearance`.

La Proposal fija el **invariante**, no el plan de gates (eso es G4+, y sigue bloqueado): **en ningun
commit puede existir un estado donde una segunda propiedad sea reconocible pero se resuelva,
materialice o congele mal.** Como se garantiza es §20.12.

---

## 19. Alternativas consideradas

| | Alternativa | Veredicto del Coordinador |
|---|---|---|
| **A** | **Descriptor Selective-specific + `LinkedPropertyEditor`** (`D-03`+`D-10`) | **RECOMENDADA.** Cierra `H1`-`H7` en Application con una autoridad **cerrada y enumerable**, y da a la UI un control unico. El coste es real y se declara: **funda una autoridad nueva**. |
| **B** | **Registry global universal** de propiedades vinculables, para todos los sistemas | **RECHAZADA (provisional).** Riesgo de **service locator** y de **framework especulativo**: resolveria hoy un problema que solo tiene el Selectivo, con una abstraccion dimensionada para sistemas que **nadie ha pedido vincular**. Contradice ademas «no generalizar otros sistemas» del contrato. Reconsiderable **el dia que un segundo sistema lo necesite de verdad**. |
| **C** | **Solo el control WPF generico**, sin tocar Application | **INSUFICIENTE, y peligrosa.** Deja intactos `H1`-`H7`: el resolver seguiria colapsando en `VerticalClearance`, `Unlink` materializando el campo fijo, `WithDesign` congelando uno solo y `FindBroken` mintiendo el literal. Un control bonito sobre semantica rota es **peor** que el estado actual, porque hace facil pedir lo que el nucleo hace mal. |
| **D** | **Rewrite MVVM / `DataTemplate` del editor** | **RECHAZADA.** *Scope explosion*: convierte I-48 en una reforma del editor Selectivo. El contrato pone «reescribir editores completos» expresamente fuera de alcance. `D-13` toma deliberadamente el camino minimo. |

---

## 20. Riesgos y preguntas explicitas para el Arquitecto

**El Coordinador espera ser corregido aqui.** Estas no son preguntas retoricas: cada una puede cambiar
la Proposal, y varias pueden invalidar `D-03`.

**20.1 — ¿Es realmente necesario el descriptor, o existe una arquitectura materialmente menor?**
`D-03` funda una autoridad nueva en un repositorio que ha pagado caro las autoridades nuevas. ¿Se pueden
cerrar `H1`-`H7` con algo **menor** —un `switch` exhaustivo, un par de funciones puras por propiedad, un
tipo suma— que no sea una tabla? Si la respuesta es si, `D-03` cae.

**20.2 — ¿El ownership y la capa del descriptor son correctos?** Se propone en `RackCad.Application`,
area del Selectivo. ¿Deberia estar mas cerca del documento persistido? ¿Del dominio? ¿Rompe alguna
direccion de dependencia que hoy vale?

**20.3 — ¿Como se impide que el descriptor derive en service locator?** `D-04` lo prohibe por
enumeracion (sin labels, sin parsing, sin callbacks). ¿Es suficiente una prohibicion escrita, o hace
falta una **guarda estructural** —al estilo de las de `ProjectVariablesConformanceTests`— que lo fije?

**20.4 — ¿Es correcto mantener UNA `RackMutation` por rack en el caso N→1?** `D-07` lo mantiene por el
argumento de la doble escritura. ¿Se sostiene? Y sobre todo: **¿de donde debe salir el conjunto `P`?**
`Summarize` ya lo calcula, pero es una **proyeccion de presentacion**; usarla como entrada de una
mutacion mezcla papeles. ¿Nuevo tipo? ¿`ProjectVariableConsumer` gana el conjunto? ¿El probe deja de
devolver `bool`?

**20.5 — ¿Debe Application validar el tipo, haciendo obligatorio el fix de `ToProjectVariables()`?**
`D-08` dice que si y acepta el coste. La pregunta inversa es legitima: ¿basta con el filtro de UI y se
deja la deuda en `DEFER SAFE`? El Coordinador cree que **no** —un filtro de UI no es una frontera— pero
reconoce que esto **anade** trabajo a I-48 por una via que I-47 dejo abierta a proposito.

**20.6 — ¿Esta `Reference → Literal(valor)` correctamente modelado?** `D-12` pide preservar la semantica
`Unlink → SetLiteral` en una operacion atomica sin estado intermedio observable. ¿Es esa la
formulacion correcta, o deberia ser una operacion **propia** con su propio nombre y su propio preflight,
en vez de una composicion?

**20.7 — ¿Debe `LostFocus` comprometer REFERENCIAS?** `D-11` lo permite solo si la referencia es
**inequivocamente identificada**. Es discutible: comprometer un vinculo al salir de un campo es un gesto
mas debil que Enter, y un vinculo es mas consecuente que un numero. ¿Solo Enter para referencias?

**20.8 — ¿Como tratar los homonimos?** `D-10` prohibe «tomar el primero» y exige distinguibilidad por
valor y/o sufijo del id. ¿Es suficiente? ¿Debe el control **negarse** a comprometer ante ambiguedad
—que es lo que `D-11` propone— o debe **forzar** una desambiguacion explicita del usuario?

**20.9 — ¿Sigue siendo `PalletTolerance` la prueba adecuada?** `D-01` dice que si, con evidencia. El
riesgo declarado es su legado `0.0` sin sentinel: ¿lo hace mala prueba, o al contrario, la hace **mejor**
porque expone una asimetria real que un caso comodo esconderia?

**20.10 — ¿Es la hipotesis de `D-17` la correcta?** «Declarar id + registrar descriptor + colocar
control» como coste de la tercera propiedad. ¿Es alcanzable? ¿Es **deseable** como objetivo, o invita a
disenar para la metrica?

**20.11 — ¿Cual es el cambio minimo suficiente en la UI?** ¿Basta el batch de intents de `D-13`?
**Ver tambien `20.13`, que expone un caso concreto donde ese batch podria perder intencion.**
¿Debe el Selectivo adoptar `NumericField` dentro de I-48 o queda fuera? ¿Se reutiliza
`PendingTextField<T>` —hoy `internal`— o el control lleva su propio estado?

**20.12 — ¿Que hot files y que gates exige la implementacion?** `docs/WORKFLOW.md` §7 lista los archivos
calientes. Las seis superficies de `D-05` mas el editor tocan varios a la vez. ¿En que orden, con que
evidencia por gate, y como se garantiza el invariante de `D-18` —que ningun commit deje una segunda
propiedad reconocible pero mal resuelta—?

**20.13 — ¿QUE LITERAL SE CONGELA cuando el usuario edita el numero Y DESPUES vincula, en la misma
apertura?** *(nueva en V1.1; refina `20.11` y apunta directamente a `D-13`.)*

**El caso, concreto:**

```text
Estado inicial:
  selective.palletTolerance   literal authored = 4

Durante UNA MISMA apertura del editor:
  1. el usuario escribe            7
  2. despues selecciona            =VariableX
  3. despues confirma la ventana
```

**Pregunta: al ejecutar el `Link`, ¿que literal debe quedar CONGELADO — `4` o `7`?**

**Esta Proposal NO lo decide, y NO se inclina por ninguna de las dos.** Se somete abierta porque la
respuesta condiciona la forma minima de `D-13`, y fijarla por omision —o insinuarla en la
redaccion— seria decidir sin revision.

**El hecho de partida, del codigo vigente y no discutido:** el `Link` actual **congela por omision**.
`Preflight.Link` escribe **solo** en `PropertyValues` y **no toca el campo**, asi que el literal que
queda congelado es, exactamente, el que traia el authored que recibio. Esto describe el mecanismo; **no
dice cual de las dos semanticas de abajo es la correcta**.

**Las dos semanticas estan ABIERTAS. Ninguna es la preferida del Coordinador.**

**Semantica A — la referencia REEMPLAZA el draft literal.**
Seleccionar `=VariableX` **sustituye** el draft literal `7`. El estado final que el usuario expresa es
`Reference(X)` y nada mas: el `7` fue una edicion en curso que el propio usuario abandono al vincular.
En consecuencia el `Link` conserva el authored **previamente comprometido**, es decir **`4`**.
*Lectura natural:* el campo es uno solo y su ultimo estado expresado es una referencia; un draft sin
comprometer no es una intencion, es un intento.

**Semantica B — el draft literal FORMA PARTE de la intencion final.**
El usuario primero cambio el literal base a `7` y **despues** pidio vincularlo. El estado final debe
expresar algo equivalente a **«nuevo literal congelado `7` + `Reference(X)`»**.
*Lectura natural:* el usuario dejo dos cosas dichas sobre la misma propiedad —cual es su valor propio y
que ahora la gobierna X— y congelar `4` descartaria la primera.

**Restriccion que vale para AMBAS, y que no favorece a ninguna:** **NO se quiere transportar un
historial completo de gestos**. Eso convertiria la ventana en un log, daria a la UI una secuencia con
semantica y contradiria `D-13` y `D-09-bis`. Sea A o sea B, la representacion tiene que ser **de
estado final**, no de trayectoria.

**Lo que se pide al Arquitecto — cinco preguntas, todas abiertas:**

1. **¿Cual interpretacion corresponde mejor al contrato UX** de este editor, A o B?
2. **¿Como puede la UI representar la intencion final MINIMA** sin transportar historial?
3. **¿Basta que el estado final contenga unicamente `Source = Reference(X)`**, o necesita ademas algo
   como un **`LiteralCandidate`**?
4. **¿Que debe ocurrir si el usuario escribe un literal y despues selecciona referencia SIN haber
   comprometido ese literal** explicitamente con Enter o LostFocus? (Es decir: ¿un draft no
   comprometido cuenta o no cuenta?)
5. **¿Cambia la respuesta si el `7` YA habia sido comprometido** antes de seleccionar `=VariableX`?
   Si A y B convergen en ese caso y divergen en el otro, la frontera esta en el compromiso, y conviene
   decirlo.

**Condiciones que la respuesta debe satisfacer, sea cual sea:**

1. **No inferir en silencio una intencion que el usuario no expreso.**
2. **La UI sigue describiendo estado / intencion, no decidiendo semantica** (`D-09-bis`, `D-14`).
3. **Ningun estado intermedio se persiste** — misma exigencia que `D-12` impone a
   `Reference → Literal`.
4. **Application produce un unico resultado atomico** (`D-07`, `D-13`).

*(Notese la simetria con `20.6`: alli el caso es `Reference → Literal(valor)`; aqui es
`Literal(valor nuevo) → Reference`. Puede que ambas pidan la misma respuesta estructural, y puede que
no — decidirlo es parte de la revision.)*

---

## 21. Estado

```text
COORDINATOR PROPOSAL V1.1 — NOT CONSENSUS
Implementation remains BLOCKED

Version     : V1.1  (V1 se corrigio ANTES de revisarse; no hay revision invalidada)
Coordinator : PROPOSED  (este documento)
Architect   : NOT REVIEWED
Consenso    : NO ALCANZADO

Siguiente paso: Architect Review sobre ESTA version (V1.1).
El consenso exige Coordinator = AGREED y Architect = AGREED sobre la MISMA PLAN_VERSION,
sin desacuerdos abiertos. Hasta entonces no se escribe produccion.
```

**Cambios de veredicto que esta Proposal introduce respecto al Discovery**, para que no pasen
inadvertidos en la revision:

| Punto | Discovery (G1/G1.1) | Proposal V1.1 | Por que |
|---|---|---|---|
| `ToProjectVariables()` | `DEFER SAFE under current one-Type assumptions`, condicional hasta G2 | **`FIX REQUIRED BY I-48`** | `D-08` decide que Application valide tipo, que es justo el disparador que §7 del Discovery anticipo |

Ningun otro veredicto del Discovery cambia. Los conteos de §9 del Discovery (`9 / 10 / 19`, extendido
`+5`) **no** se tocan.
