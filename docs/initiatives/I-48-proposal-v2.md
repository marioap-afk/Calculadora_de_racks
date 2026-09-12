# I-48 — Proposal V2: edicion vinculable reusable

> # ⚠ COORDINATOR PROPOSAL V2 — ARCHITECT CHANGES RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```
> Coordinator = PROPOSED V2
> Architect   = NOT REVIEWED V2
> Consensus   = NOT REACHED
> ```
>
> **V2 reconcilia la Architect Review de V1.1. NO es un acuerdo.** El Arquitecto reviso V1.1 y
> termino en `NOT AGREED` con **1 BLOCKER, 5 MATERIAL y 2 MINOR**. Este documento registra que se
> acepto, que se refino y donde V2 **se aparta** de lo que el Arquitecto pidio — porque una
> reconciliacion que solo dijera «aceptado» a todo no seria una reconciliacion.
>
> La revision de V2 **no ha ocurrido**. Ninguna `R-NN` es firme hasta que el Arquitecto se pronuncie
> sobre **esta** version.
>
> ```
> Base:                a06114862448d59b1752298633f3c5b063a2b86f  (Proposal V1.1 revisada)
> Discovery:           edacf7d7b280715c4192c779a4c38748d33a56ef  (G1)
>                      7a9471f0d5abd815519069f67171800ec29337c9  (G1.1 addendum)
> Codigo auditado:     e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, sin avanzar)
> Historial:           I-48-proposal-v1.md queda INTACTA (V1 -> V1.1). Este documento NO la sustituye
>                      en el archivo: la conserva como registro de lo que se reviso.
> Estado de gates:     G0 · G1 · G1.1 · G2A · G2A.1 · G2A.2 · Architect Review V1.1 · **G2B EN CURSO**
>                      · G3 consenso PENDIENTE
> ```

## 0. Como leer V2

V2 **no reescribe** `D-01`..`D-18`. Las conserva y las **modifica por excepcion**: cada `R-NN` dice
que decision toca y como queda. Lo que ninguna `R-NN` toca, sigue vigente **tal como esta en V1.1**.

Se conservan sin cambio: `D-01` (proof + legado `0.0` caracterizado, no migrado), `D-02`
(`PropertyId` estable/Ordinal/system-qualified), `D-06` (resolver N-propiedades, sin orden de
`Dictionary`, sin cross-write, fail-closed), `D-07` (algoritmo N→1, **una** `RackMutation` por rack),
`D-09`/`D-09-bis` (frontera de la UI), `D-14` (capas), `D-15` (ID22B/ID21 fuera), `D-16`
(`PropertyValue<T>` no se fuerza), `D-17` (KPI, con `R-15`).

---

## 1. Tabla de reconciliacion

| Hallazgo de la Architect Review V1.1 | Severidad | Resuelto en | Resultado |
|---|---|---|---|
| **`D-05` — criterio de aceptacion ciego a `H1` y `H7`** | **BLOCKER** | **`R-01`** | **ACEPTADO**, y el criterio se sustituye por completo |
| `D-13` — «intent final unico» no expresa 20.13 | MATERIAL | **`R-03`**, **`R-04`**, **`R-10`** | ACEPTADO con refinamiento |
| `D-11` — `LostFocus` no debe comprometer referencias | MATERIAL | **`R-05`** | ACEPTADO |
| `D-10` — el valor no desambigua homonimos | MATERIAL | **`R-06`** | ACEPTADO |
| Integracion con `CommitPendingEditors` (C4) ausente | MATERIAL | **`R-07`** | ACEPTADO |
| `D-18` — es promesa, no mecanismo | MATERIAL | **`R-08`** | ACEPTADO y reforzado |
| `D-08` — el fix debe lanzar, no defaultear | MINOR | **`R-09`** | ACEPTADO (fail-loud) |
| `D-04` — «mecanico» necesita definicion estructural | MINOR | **`R-02`** | ACEPTADO **con correccion**: V2 **no** adopta el limite literal de miembros |
| `PC-8` — clausula anti-metrica | (propuesta) | **`R-15`** | ACEPTADO, y el AFTER pasa a ser **empirico** |
| `OQ-1` — campo gobernado deja de ser read-only | (abierta) | **`R-11`** | RESUELTA |
| `OQ-2` — donde vive el conjunto `P` | (abierta) | **`R-12`** | RESUELTA |
| `OQ-3` — forma de la source guard | (abierta) | **`R-13`** | RESUELTA |
| `OQ-4` — `Reference(X) → Reference(X)` | (abierta) | **`R-14`** | RESUELTA |

**Todo lo que el Arquitecto marco como bloqueante o material queda aceptado.** El unico punto donde
V2 **no** hace lo que la review pedia literalmente es `R-02`, y se argumenta ahi.

---

## 2. Decisiones de reconciliacion

### R-01 — `D-05` BLOCKER: **ACEPTADO**. El criterio del token se RETIRA

**El defecto, con la evidencia del Arquitecto.** V1.1 proponia como criterio primario que el token
`SelectiveVerticalClearance` dejara de aparecer en las seis superficies. Medido:

| Superficie | token | campo |
|---|---|---|
| **`SelectiveEffectiveDesignResolver`** (`H1`) | **0** | **2** (l.51, l.69) |
| **`ProjectVariablesWorkspace.FindBroken`** (`H7`) | **0** | **1** (l.292) |
| `Preflight` (`Unlink` + `Materialize`) | 1 | 2 |
| `WithDesign` | 1 | 1 |
| `SelectiveEditorOpen` | 2 | 2 |

Los dos peores hotspots **ya cumplen hoy** el criterio de V1.1 estando integramente hardcodeados **al
campo**. Era un criterio capaz de **certificar como terminada una implementacion corrupta**. Se
retira, sin sustituto parcial.

**Criterio primario nuevo: MATRIZ DE COMPORTAMIENTO por cada descriptor registrado**, sobre las seis
superficies.

**Y la condicion que le da valor: un ORACLE DE TEST INDEPENDIENTE del descriptor.** Quedan
**prohibidos los tests tautologicos**: una prueba que use **el getter/setter del propio descriptor**
para decidir que campo debia cambiar no prueba nada — pasaria igual con el descriptor mal cableado.

El oracle declara la correspondencia **por su cuenta**, como minimo:

```text
selective.verticalClearance  →  VerticalClearance
selective.palletTolerance    →  PalletTolerance
```

y **su conjunto de `PropertyId` debe coincidir EXACTAMENTE con el catalogo productivo** — ni de mas
(oracle para algo que no existe) ni de menos (propiedad registrada sin caso independiente). Esa
igualdad de conjuntos es, ella misma, una prueba.

Para **cada** `PropertyId` del catalogo se comprueba:

1. **resolucion efectiva al campo correcto**;
2. **no-cross-write** — resolver esta propiedad no altera el campo de ninguna otra;
3. **freeze de su propio literal**;
4. **`Unlink` materializa su propio effective**;
5. **`UnlinkAllAndDelete` materializa TODAS las propiedades aplicables**;
6. **`FindBroken` obtiene el `StoredLiteral` correcto**;
7. **`EditorOpen` obtiene el estado correcto**.

**La source guard baja a defensa ESTRUCTURAL SECUNDARIA.** Puede existir, pero **nunca** sustituye a
las pruebas de comportamiento y **nunca** vuelve a ser un criterio sobre el token (`R-13`).

---

### R-02 — Descriptor (`D-03`/`D-04`): **SE MANTIENE, REFORZADO** — y V2 corrige a la review en un punto

**Se mantiene el catalogo**: Selective-specific, **cerrado**, **inmutable**, en **Application**.

**Capacidades semanticas minimas** (esto es lo que el descriptor debe poder expresar; **no** es una
firma):

- `PropertyId`;
- `VariableType`;
- acceso **tipado y mecanico** get/set al **literal persisted/authored**;
- acceso **tipado y mecanico** get/set al **effective/domain**.

**Prohibiciones, que es donde vive la garantia:** `internal`; `sealed`/inmutable donde corresponda;
**fuertemente tipado**; **sin** registro en runtime; **sin** descubrimiento por reflexion; **sin**
configuracion ni CSV; **sin** `IServiceProvider` ni servicios; **sin** bolsas `object`; **sin**
callbacks generales de negocio; **sin** WPF; **sin** AutoCAD; **sin** parsing; **sin** formulas.

**Dos aclaraciones que V2 hace explicitas porque la review las dejaba ambiguas:**

- **Los accesores tipados mecanicos SI estan permitidos.** Son el mecanismo, no el riesgo.
- **Pasar un descriptor COMO VALOR a un helper puro SI esta permitido, y eso NO es service locator.**
  Service locator es *pedir* dependencias a un contenedor; pasar un dato inmutable a una funcion pura
  es lo contrario.

> **Aqui V2 se aparta de la review.** `PC-7` pedia «exactamente cuatro miembros» y «nunca pasarlo como
> parametro». V2 **no adopta** ninguna de las dos:
> - **No se fija una cantidad literal de miembros C#.** Es una restriccion de forma que no protege de
>   nada —cuatro miembros pueden ser cuatro lambdas arbitrarias— e impediria detalles legitimos.
>   Lo que protege es la **lista de prohibiciones** de arriba, que es sobre *naturaleza*, no sobre conteo.
> - **La prohibicion de pasarlo como parametro se retira**, por la razon del parrafo anterior.
>
> Se somete a revision precisamente por ser un apartamiento.

**Evidencia del Arquitecto que se INCORPORA al expediente** (V1.1 no la citaba, y sostiene `D-04`):
resolver mediante **clones document-shaped** —escribir el efectivo en un clon del documento y llamar
`ToDomain()`— **no sustituye** estos accesores, por dos hechos verificados:

1. `ProjectVariableCloning.Clone` es un **round-trip JSON completo** (`Serialize` + `Deserialize`), y
   `Resolve` corre **en bucle por rack** en `ChangeValue`, `UnlinkAllAndDelete`, `FindBroken` y el BOM.
2. `ToDomain()` contiene **semantica legacy/default** que **no** equivale a un accesor mecanico de
   campo: `PalletDepth = PalletDepth > 0.0 ? PalletDepth : SelectiveRackDefaults.DefaultPalletDepth`
   (l.341) y el default de `DepthCount` (l.342). Un futuro `selective.palletDepth` con efectivo `0`
   se convertiria en el default **en silencio**.

Las dos formas —documento y dominio— son **genuinamente distintas**. Por eso son dos parejas de
accesores y no una.

---

### R-03 — `D-13` / `PC-2`: **ACEPTADO CON REFINAMIENTO**

El estado declarativo **FINAL** por propiedad pasa conceptualmente a:

```text
LinkedPropertyEditState
    CommittedLiteral : double
    Source           : LinkedPropertySource

LinkedPropertySource  (hoy)
    Literal
    ProjectVariableReference(VariableId)
```

- **NO usar `bool IsVariable`.** Un booleano no admite una tercera variante y volveria a cerrar la
  puerta que `D-15` quiere dejar abierta sin implementar.
- **NO transportar historial de gestos.**

**`CommittedLiteral` significa el ultimo literal REALMENTE COMPROMETIDO:**

- con `Source = Literal`, es el **valor activo**;
- con `Source = Reference`, es el **literal congelado detras de la referencia**.

**La UI describe este estado final. Application lo RECONCILIA contra el authored inicial y produce la
semantica.** Esto **sustituye, en el camino de `RACKEDITAR`**, la idea de transportar operaciones
sucesivas `Unlink → SetLiteral → Link`.

**Las operaciones explicitas de `RACKVARIABLES` permanecen con sus contratos vigentes** (`Create`,
`Rename`, `ChangeValue`, `Delete`, `Link`, `Unlink`, `RepairBroken`, `UnlinkAllAndDelete`). V2 **no**
las toca: lo que cambia es por donde entra el **editor**.

---

### R-04 — Pregunta 20.13: **RESUELTA**

**Regla adoptada: `Link` congela el ultimo literal COMPROMETIDO.**

```text
Caso 1
  inicial = 4
  el usuario escribe 7
  7 NO comprometido
  el usuario selecciona Reference(X)
  → frozen = 4

Caso 2
  inicial = 4
  el usuario escribe 7
  7 COMPROMETIDO con Enter / LostFocus valido
  despues selecciona Reference(X)
  → frozen = 7
```

**La frontera es COMMIT vs DRAFT, no el orden temporal.** Es **una sola regla**, sin casos
especiales: `Link` congela `CommittedLiteral` (`R-03`), sea cual sea su valor en ese momento.

**No hace falta `LiteralCandidate`.** El concepto se descarta: `CommittedLiteral` ya lo cubre, y
anadir un tercer campo para drafts reintroduciria historial por la puerta de atras.

> **Detalle del control COMPUESTO, y no es menor.** `LinkedPropertyEditor` es un control con textbox
> **y** popup/lista de sugerencias. **Mover el foco entre el textbox y su propio popup NO cuenta como
> `LostFocus` del control completo** y **NO compromete accidentalmente un literal**. Sin esta regla, el
> Caso 1 se convertiria en el Caso 2 por el mero hecho de abrir la lista — que es exactamente la
> inferencia silenciosa que la restriccion 1 prohibe.

---

### R-05 — `D-11` / `PC-3`: **ACEPTADO**

**Literal valido:**

- **Enter** puede comprometer;
- **`LostFocus` DEL CONTROL COMPUESTO** puede comprometer (con la salvedad de `R-04`).

**Referencia:**

- **`LostFocus` NUNCA crea un vinculo.**
- Una referencia se compromete **solo** mediante **accion explicita sobre una sugerencia que porta
  `VariableId`**.
- **Seleccion con raton = explicita.** **Seleccion con teclado + Enter = explicita.**
- **Enter SIN candidato seleccionado NO resuelve por texto.**

**Escape** restaura el ultimo estado comprometido.
**Un draft invalido o no resuelto PERMANECE pendiente**; no se descarta en silencio (`R-07`).

---

### R-06 — `D-10` / `PC-4`: **ACEPTADO**

- El texto `=Nombre` sirve para **FILTRAR**, **nunca** para resolver identidad.
- El commit de una referencia ocurre **solo** mediante una **sugerencia seleccionada** que porta el
  `VariableId`.
- **El nombre nunca es identidad.**

**Nombres duplicados:**

- **sufijo corto del `VariableId` OBLIGATORIO** como desambiguador visual;
- el **valor** puede mostrarse ademas;
- **el valor solo NO es suficiente**, porque dos variables homonimas **pueden tener el mismo valor**.

Con esto el invariante 5 deja de depender de una convencion de presentacion y pasa a ser estructural:
si la unica via de commit porta el id, la ambiguedad **no puede** plantearse como pregunta de
resolucion.

---

### R-07 — Integracion C4 / `PC-5`: **ACEPTADO**

`LinkedPropertyEditor` **tendra drafts reales** (`R-03`, `R-05`) y por tanto **debe participar del
protocolo vigente de pending editors**.

**No se obliga a reutilizar `PendingTextField<T>`**: no modela referencias, y forzarlo deformaria el
control o el helper.

**Si se exige:**

- implementar **`IPendingTextField`** o un **adapter equivalente al mismo contrato de dos fases**;
- **incluir los `LinkedPropertyEditor` en `pendingAll`**;
- **Stage valida SIN mutar**;
- **Apply ocurre solo despues** de que todos los pendientes requeridos hayan validado;
- **`InvalidDraft` o referencia no seleccionada BLOQUEA** las fronteras de escritura/navegacion que
  hoy llaman `CommitPendingEditors`;
- **nunca perder un draft en silencio**.

**No se reescribe el editor completo ni se migra a MVVM.**

---

### R-08 — `D-18` / `PC-6`: **ACEPTADO Y REFORZADO**

**El enunciado «`IsKnown` se relaja al final» NO basta**: nada lo hace fallar. La implementacion
futura debe tener **pruebas independientes preparadas ANTES** de activar la segunda propiedad.

Secuencia conceptual:

1. infraestructura generica funcionando **todavia solo con `VerticalClearance`**;
2. las **seis superficies ya enrutadas** por la autoridad;
3. **editor reusable + pending protocol**;
4. **type compatibility**;
5. **tests/oracle de `PalletTolerance` preparados**;
6. **SOLO AL FINAL**, activar/registrar `selective.palletTolerance`.

**No puede existir un commit donde `PalletTolerance` sea conocida pero se resuelva, materialice o
congele mal.**

**El mecanismo que lo hace cumplir:** el conjunto del **oracle** y el conjunto del **catalogo** deben
**coincidir** (`R-01`). Registrar una propiedad sin su caso independiente **queda rojo**. Y **no se usa
el propio descriptor como oracle**.

---

### R-09 — `D-08`: **ACEPTADO + fail-loud**

- **Application valida** `property.VariableType == variable.Type`.
- **`ToProjectVariables()` sigue siendo `FIX REQUIRED BY I-48`** y debe **proyectar el `Type`
  persistido real**.
- Si llega un `Type` **no parseable o no soportado** donde por contrato ya deberia haberse validado
  (`ProjectVariablesStore.IsKnownType` rechaza el documento entero en lectura): **fail loud**.
- **PROHIBIDO** cualquier fallback del tipo `?? VariableType.Length`. Recrearia el bug en un sitio
  nuevo, y esta vez con apariencia de correccion.
- **No se anade un segundo `VariableType`.**

---

### R-10 — `D-12`: **REFORMULADO PARA EL EDITOR**

**En `RACKEDITAR` NO se modela `Reference → Literal` como dos comandos persistibles sucesivos.**

El **reconciler** de Application recibe:

- el **authored inicial**;
- el **estado final declarativo por `PropertyId`** (`R-03`);

y produce **un resultado final atomico**.

**Invariantes observables que debe conservar:**

| Transicion | Resultado exigido |
|---|---|
| `Literal → Reference` | **Congela el ultimo literal comprometido** (`R-04`) |
| `Reference → Reference` | **Conserva el literal congelado** |
| `Reference → Literal(value)` | Termina **sin binding**, `literal = value`, `effective = value` |
| `Reference(X) → Reference(X)` | **NO-OP idempotente** para esa propiedad (`R-14`) |
| Broken binding | **Fuera de este camino** y **fail-closed** |

**No hay estado intermedio persistido.**

---

### R-11 — `OQ-1`: **RESUELTA**

Se acepta **deliberadamente** que el campo gobernado **deje de ser read-only** en `RACKEDITAR`.

- El mismo `LinkedPropertyEditor` muestra `=Nombre` y **permite iniciar edicion**.
- **Teclear un numero NO desvincula inmediatamente**: crea un **`DraftLiteral`**.
- **Solo al commit valido** se expresa `Reference → Literal`.
- **No se exige un boton `Unlink` previo.**

`RACKVARIABLES` **conserva** sus operaciones explicitas y `RepairBroken`.

*(Es un cambio de comportamiento consciente respecto a `ApplyVerticalClearanceBinding`, que hoy pone
el campo en `IsReadOnly`. Queda registrado como tal para la validacion del dueno.)*

---

### R-12 — Conjunto `P` / `OQ-2`: **RESUELTA**

Para la familia **target-variable**, el discovery/probe debe producir el conjunto **EXACTO y NO
VACIO**:

```text
P = PropertyIds del rack que apuntan al VariableId objetivo
```

- **Ese conjunto debe VIAJAR hasta el preflight/materializacion.**
- **NO se reutiliza `Summarize` como input de mutacion**; `Summarize` **se deriva** del conjunto
  descubierto. La presentacion lee el modelo, no al reves.
- **NO se modela «`P` no aplica» como lista vacia dentro de un consumer comun** si eso crea un estado
  semanticamente ambiguo — «vacio porque no aplica» y «vacio porque no hay» no son lo mismo.

**V2 prefiere un tipo o wrapper especifico** para el consumer/uso target-variable, que incluya:

- `RackId`;
- **authored authority**;
- `Siblings`;
- **`P` = matching PropertyIds**.

**La familia B (`ResolveTargetRack`) conserva su semantica sin inventar `P`.**

El **nombre exacto** del tipo puede decidirse en implementacion; **la separacion semantica, no**.

---

### R-13 — `OQ-3`: **RESUELTA**

- **Behavioral tests = criterio de aceptacion.**
- **Source guard = opcional / secundaria / estructural.**
- **No disenar para `grep`.**
- Si existe source guard, debe **detectar dependencia property-specific en los consumidores**, no
  sustituir la prueba funcional.

---

### R-14 — `OQ-4`: **RESUELTA**

`Reference(X) → Reference(X)` es **NO-OP idempotente**:

- **no** es error;
- **no** hay reescritura innecesaria para esa propiedad;
- **no** cambia el literal congelado.

**Otros cambios del mismo batch pueden continuar** — la idempotencia es por propiedad, no aborta el
conjunto.

---

### R-15 — `PC-8` / metricas: **ACEPTADO**

Se mantiene **`BEFORE_PRODUCT_FILES = 9`** como **KPI historico**.

**Clausula anti-gaming:** alcanzar un numero bajo mediante **reflexion, configuracion en runtime, CSV,
service locator o reduccion de tests es FALLO, no exito.**

**El AFTER pasa a ser EMPIRICO, no estimado:**

```text
PRE_PROOF_SHA  = el SHA justo ANTES de activar PalletTolerance,
                 con toda la infraestructura generica terminada
PROOF_SHA      = el SHA de la implementacion exitosa que la activa

AFTER = diff productivo real de  PRE_PROOF_SHA..PROOF_SHA
```

- Se **reportan productivos y tests POR SEPARADO**.
- La hipotesis «`PropertyId` + descriptor + placement» **sigue siendo hipotesis, no gate rigido**.

---

## 3. Gates propuestos para la implementacion futura

> **PROPUESTOS, NO AUTORIZADOS.** Ninguno de estos gates puede ejecutarse hasta que exista consenso
> `Coordinator = AGREED` **y** `Architect = AGREED` sobre la misma version.

| Gate | Contenido |
|---|---|
| **G4A** | Descriptor/catalogo + **solo `VerticalClearance`** |
| **G4B** | Enrutar las **seis** superficies semanticas; comportamiento de I-47 **sin cambio** |
| **G4C** | `LinkedPropertyEditor` + protocolo de estado final + `pendingAll`/C4 |
| **G4D** | Autoridad de tipo + `ToProjectVariables` **fail-loud** |
| **G4E1** | Tests/oracle de `PalletTolerance` en **RED**; **todavia NO registrada ni conocida** |
| **G4E2** | **Activar** `PalletTolerance` como segundo descriptor + colocacion en UI |
| **G4F** | Cross-property / N→1 / BOM / persistencia / integracion de UI |
| **G4G** | Candidato completo + CI |
| **G4H** | **Owner Validation en AutoCAD 2025** |

**`G4E2` define `PRE_PROOF_SHA` como su commit padre y `PROOF_SHA` como el SHA de su implementacion
exitosa**, para la medicion AFTER de `R-15`.

---

## 4. Arquitectura futura

- **ID22B (formulas) e ID21 (rack→rack) siguen FUERA.**
- **`LinkedPropertySource` PUEDE ganar otra variante en el futuro** —por eso `R-03` prohibe el
  `bool`— pero **NO se implementa `Formula` ni `RackReference` ahora**.
- **`PropertyId` se mantiene compatible con un futuro `(RackId, PropertyId)`**: estable, Ordinal,
  system-qualified, sin normalizacion ni alias.

---

## 5. Estado

```text
COORDINATOR PROPOSAL V2 — ARCHITECT CHANGES RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Coordinator = PROPOSED V2
Architect   = NOT REVIEWED V2
Consensus   = NOT REACHED

Origen: Architect Review de V1.1 @ a06114862448d59b1752298633f3c5b063a2b86f
        (Architect = NOT AGREED · 1 BLOCKER · 5 MATERIAL · 2 MINOR)

Siguiente paso: Architect Review sobre V2.
Punto que V2 somete con atencion especial: R-02, donde V2 NO adopta dos
restricciones de PC-7 (conteo literal de miembros; prohibicion de pasar el
descriptor como parametro) y argumenta por que.
```

**Historial conservado:** [I-48-proposal-v1.md](I-48-proposal-v1.md) (V1 → V1.1) queda **intacta**.
V2 no la modifica ni la marca supersedida en su propio archivo: la revision de V1.1 es parte del
expediente y su documento debe leerse tal como se reviso.
