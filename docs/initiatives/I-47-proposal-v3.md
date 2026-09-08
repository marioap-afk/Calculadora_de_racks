# I-47 — Proposal V3: contrato de variables de proyecto (ID22A)

> **Documental. No autoriza implementacion.** No toca `src/` ni `tests/`. **No se pidio Architect
> Review** en este gate.
>
> ```
> PROPOSAL VERSION:   V3
> Sustituye a:        I-47-proposal-v2.md  (commit e08431f)   — que a su vez supersedio a V1
> Base del analisis:  9e25d5291daa13c4846112241be6e52526429a47  (Discovery)
> origin/main:        306e18ed4676e5e96b54d59402c9a230efb137d3  (sin avanzar)
> Premisas:           C-1..C-6 (Gate C) + C2-1..C2-9 (Gate C2) + C3-1..C3-8 (Gate C3)
> ```
>
> **V3 corrige ocho puntos de V2 y no toca nada mas.** Su efecto principal es que **el contrato deja de
> tener decisiones de arquitectura pendientes**: el dueno cerro D-07 y D-10, y retiro del terreno de las
> preguntas abiertas la viabilidad de la transaccion unica y la numeracion de H3. Lo que queda vivo es
> **verificacion e implementacion**, no diseno. La confirmacion explicita esta en §12.

## 0. Premisas — no se comparan

Las seis decisiones del dueno son **entradas**, no opciones. Ninguna alternativa de este documento las
contradice; cuando una alternativa razonable queda excluida **por** una premisa, se dice cual y por
que, en vez de omitirla.

| # | Premisa |
|---|---|
| C-1 | DWG sin `ProjectVariables` = **registro vacio**; **sin migracion**; **literales preservados** |
| C-2 | Propiedad = **`Literal`** o **`ProjectVariableReference`**; la referencia **gobierna el valor efectivo** |
| C-3 | Cambiar una variable **propaga y redibuja todos sus consumidores en UNA operacion** |
| C-4 | **RACKDUPLICAR conserva el mismo `VariableId`** |
| C-5 | **ID22A no unifica** `ClearHeight` de otros sistemas; el slice es **Selectivo** |
| C-6 | **`defaults.json` convive**; **no** se convierte en `ProjectVariables` |

Diferido por el dueno: **WBLOCK y copia entre dibujos**. Minimo exigible: **guardar, cerrar y
reabrir el mismo DWG** (§9.2).

### Premisas anadidas en el Gate C2

| # | Premisa |
|---|---|
| C2-1 | **ID22B = formulas/expresiones entre variables.** No se disenan aqui |
| C2-2 | **ID21 = referencias a propiedades de racks**, previstas como **`RackId` + `PropertyId`**. No se disena aqui |
| C2-3 | Se separan tres conceptos distintos: **`ProjectVariable`**, **`PropertyValue<T>`** y **`PropertyId`** (§1-bis) |
| C2-4 | La referencia persistida es **tipada**; ID22A acepta **solo** `kind: projectVariable` |
| C2-5 | `[JsonExtensionData]` anadido ahora **no** protege frente a una version pre-I-47 **ya compilada** |
| C2-6 | `SystemBlockWriter.RedrawInPlace` **commitea por bloque**: el lote **no** es todo-o-nada |
| C2-7 | Un round-trip del Store **no** prueba `SelectiveKindHandler.RestampDesign` |
| C2-8 | La transferencia/fusion de variables entre dibujos **no** pertenece a ID22B: es futuro separado |
| C2-9 | El contrato y la fila de ROADMAP deben actualizarse **antes de produccion** si UI/comando forman parte del plan final (§14) |

### Premisas anadidas en el Gate C3

| # | Premisa |
|---|---|
| C3-1 | **D-07 queda decidido: F2.** F3 se **rechaza** por una razon semantica, no de formato (D-07) |
| C3-2 | F2 se corrige a **promocion de schema PEGAJOSA**, no «major segun contenido actual» (D-07) |
| C3-3 | **D-10 queda decidido: F1.** Borrar una variable con consumidores **se bloquea** y se listan; «desvincular todos materializando + borrar» solo como **accion explicita separada** |
| C3-4 | `RedefineSystemBlock` usa la **`Transaction` del llamador**; transaccion **unica** para `ProjectVariables` + geometria + payloads |
| C3-5 | **`BlockLibraryImporter.EnsureForPlan` NO forma parte del preflight ni del lote**: puede mutar el `Database` **fuera** de la transaccion. La propagacion **verifica** que las definiciones ya existan y **aborta** si falta alguna; **no importa ni repara** la biblioteca como efecto colateral |
| C3-6 | La **viabilidad** de la transaccion unica **deja de ser pregunta de contrato**. La validacion real en AutoCAD es **gate de implementacion / Owner Validation**, no condicion previa de Consensus Freeze |
| C3-7 | **WORKFLOW §2 manda sobre CF-2**: el **contrato** de iniciativa si refleja el alcance final antes de produccion; **ROADMAP no se modifica en un gate intermedio** y se actualiza **al integrar/cerrar**. ROADMAP sale del Consensus Freeze y pasa al **checklist de cierre** |
| C3-8 | Se retiran de «preguntas abiertas»: F2 vs F3, D-10, viabilidad conceptual de la transaccion unica, y asignar numero a H3. El **coste de propagacion** queda como **riesgo/metrica de implementacion**, no como decision de arquitectura |

## 0-bis. Tabla exacta V2 → V3

| Decision | V2 decia | V3 dice | Motivo |
|---|---|---|---|
| **D-07** | F2 **recomendada**, F3 «alternativa viva»; la eleccion se dejaba al dueno | **F2 DECIDIDA.** F3 **rechazada** con razon propia: una version vieja puede *preservar* el binding en el sobre pero **no entiende su semantica**, asi que podria editar y redibujar con el **literal** y dejar geometria **incoherente** con la autoridad que una version nueva resolvera despues | C3-1 |
| **D-07 (mecanica)** | «major condicional» segun si el documento lleva `PropertyValues` | **Promocion PEGAJOSA**: 1.x nunca vinculado sigue en 1.x; introducir `PropertyValues` promueve a **2.0**; **una vez 2.x, nunca vuelve a 1.x**; desvincular materializa y borra la referencia **sin degradar el schema**; minors posteriores se preservan segun `SchemaVersionPolicy` | C3-2 |
| **D-10** | F1 **recomendada**, seguia listada como pregunta abierta | **F1 DECIDIDA.** Se bloquea y se listan los consumidores; el atajo comodo existe **solo** como accion explicita separada | C3-3 |
| **D-13** | «conviene resolver `EnsureForPlan` en el preflight» | **Fuera del preflight y del lote.** `EnsureBlocks` **abre y commitea su propia transaccion** y luego llama a `WblockCloneObjects`, que muta el `Database` **fuera** de la del llamador. La propagacion **verifica** que las definiciones existan y **aborta antes** de tocar la variable o los consumidores | C3-5 |
| **D-13 (transaccion)** | «se propone» una frontera con transaccion del llamador | **Se exige**: `RedefineSystemBlock` recibe la `Transaction` del llamador y **una sola** transaccion cubre registro + geometria + payloads | C3-4 |
| **§12** | 4 preguntas abiertas (F2/F3, transaccion unica, coste, numero de H3) | **1 sola**, y no es de arquitectura. Las otras tres se retiran; el coste pasa a §13 como metrica | C3-6, C3-8 |
| **§13** | P9 = «la atomicidad podria no ser alcanzable» | P9 reformulado: la **viabilidad** no se discute; lo que se verifica en AutoCAD es la **implementacion**. Coste de propagacion entra como **metrica** | C3-6, C3-8 |
| **§14** | CF-2 exigia actualizar ROADMAP antes de produccion; CF-5 exigia contestar la viabilidad **antes** del Freeze | **CF-2 retirado del Freeze** y movido al **checklist de cierre** (WORKFLOW §2: ROADMAP no se toca en un gate intermedio). **CF-5 reformulado**: la validacion en AutoCAD es gate de **implementacion / Owner Validation** | C3-6, C3-7 |

La tabla **V1 → V2** vive en la §0-bis de [V2](I-47-proposal-v2.md); no se repite aqui.

**No se reabren** D-01..D-06, D-08, D-09, D-11, D-12, D-14..D-19, ni §1-bis, §10 y §11 salvo por las
entradas que estas ocho correcciones anaden.


## 1. Metodo, y una regla que gobierna todo el documento

**Ninguna opcion gana por existir ya.** El Discovery (§2.1) demostro que la autoridad de nivel dibujo
es **mecanismo nuevo**: cero `NamedObjectsDictionary`, cero `SummaryInfo`/`Ldata`/`RegAppTable`/`XData`
en `src/`, cero `WorkingDatabase` en todo el repositorio. Por tanto **no hay un camino barato**, y la
tentacion de elegir el unico almacen existente —el Xrecord de `RackBlockData` sobre la definicion de
bloque— seria elegir por coste de escritura, no por adecuacion. Ese razonamiento se rechaza
explicitamente en D-01.

Cada decision se presenta como: **alternativas → tradeoffs → recomendacion → por que se descarta el
resto**. Lo que no esta decidido se dice en §12, no se disimula.

Toda cita de codigo esta verificada contra el SHA de la cabecera.

---

## 1-bis. Tres conceptos separados (C2-3)

V1 los mezclaba: hablaba del «vinculo» sin distinguir **lo que la variable es** de **como una propiedad
la usa**. La separacion no es cosmetica — es lo que permite que ID22B e ID21 crezcan **sin chocar entre
si**, porque **extienden tipos distintos**.

```
ProjectVariable                         ← el registro del DIBUJO
    VariableId : string   (GUID)          identidad estable, nunca derivada del nombre  (D-02)
    Name       : string                   solo etiqueta; renombrar no toca a nadie       (D-09)
    Type       : VariableType             ID22A: Length (pulgadas)                       (D-03)
    Definition : VariableDefinition       ID22A: Literal(valor)
                                          ── ID22B anadira: Expression(...)              (C2-1)

PropertyValue<T>                        ← como una PROPIEDAD toma su valor
    Literal(T)                            el escalar de siempre                          (C-1)
    ProjectVariableReference(VariableId)  la referencia gobierna el efectivo             (C-2)
                                          ── ID21 anadira: RackPropertyReference(RackId, PropertyId)  (C2-2)

PropertyId : string estable             ← que propiedad es, de forma persistente
    slice de ID22A: "selective.verticalClearance"
```

**El punto arquitectonico, y la razon de separarlos ahora:**

| Iniciativa | Que extiende | Que NO toca |
|---|---|---|
| **ID22A** (esta) | funda los tres | — |
| **ID22B** — formulas | **`ProjectVariable.Definition`**: `Literal` → tambien `Expression` | `PropertyValue<T>` no cambia: una propiedad sigue apuntando a un `VariableId` y le da igual como esa variable calcule su valor |
| **ID21** — refs a propiedades de racks | **`PropertyValue<T>`**: un tercer caso `RackPropertyReference(RackId, PropertyId)` | `ProjectVariable` no cambia: una referencia rack→rack **no** pasa por el registro del dibujo |

Es decir: **ID22B vive dentro de la variable; ID21 vive dentro de la propiedad.** Si ambos conceptos
compartieran un solo tipo —como insinuaba V1—, cada una de las dos iniciativas obligaria a tocar lo de
la otra. Separados, cada una crece por su lado.

**ID22A no implementa ninguno de los dos.** `Definition` tiene hoy un solo caso (`Literal`) y
`PropertyValue<T>` dos (`Literal`, `ProjectVariableReference`). Lo unico que ID22A se compromete a
hacer es **dejar los dos puntos de extension abiertos y persistidos de forma discriminada**, para que
anadir un caso sea aditivo y no una ruptura de esquema.

### `PropertyId`: por que es un concepto y no una cadena cualquiera

`PropertyId` identifica **la propiedad**, no el campo C# ni la caja de la UI. Es estable, se persiste y
sobrevive a renombrados de codigo. Para el slice: **`selective.verticalClearance`**.

Existe por dos razones concretas, y la segunda es la que lo hace obligatorio ahora:

1. Es la clave del mapa persistido de D-04.
2. **ID21 lo necesita como la mitad de su par `(RackId, PropertyId)`.** Una referencia a «la holgura
   vertical de aquel rack» solo se puede escribir si esa propiedad tiene un identificador estable e
   independiente del rack. Introducirlo despues obligaria a reescribir lo ya persistido.

Los ids se declaran como **constantes en un solo sitio**, siguiendo la disciplina que
`CatalogBlockParameters` ya usa para los nombres de parametro de bloque
(`SelectiveRackDefaults.LengthParam`, `PeralteParam`, `PalletAltoParam`), para que un renombrado de C#
no pueda romperlos en silencio.

---

## 2. Fundacion

### D-01 — Autoridad unica `ProjectVariables` por DWG

**Requisito**: exactamente **un** registro por dibujo, estructurado, que sobreviva a guardar/reabrir y
que no dependa de que el usuario no toque nada.

| Alt | Mecanismo | A favor | En contra |
|---|---|---|---|
| **A1** | **NOD**: `Database.NamedObjectsDictionaryId` abierto como `DBDictionary` → sub-`DBDictionary` `RACKCAD_PROJECT` → `Xrecord` con JSON troceado | Unicidad **estructural**: hay exactamente un NOD por `Database`. Es el sitio que AutoCAD define para datos de aplicacion a nivel dibujo. **No entra en la `BlockTable`**, asi que `RackBlockFinder.ScanEnvelopes` no lo ve y no necesita exclusion nueva. El troceado a <=255 ya esta probado en `RackBlockData` | API nueva en el Plugin (crear el sub-diccionario y `AddNewlyCreatedDBObject`). No lo cubre ninguna suite (R2) |
| **A2** | Diccionario de extension del **BTR de Model Space**, reutilizando `RackBlockData` tal cual | Coste de escritura casi nulo | La unicidad es **por convencion** («siempre elegimos Model Space»), no estructural: cada layout de papel es otro BTR. Dice «esto pertenece al bloque Model Space», que es **falso**. Elegirla seria exactamente el error que §1 prohibe |
| **A3** | Definicion de bloque dedicada (`RACKCAD_PROJECT_VARS`) con `RackBlockData` | Reutilizacion total | **`ScanEnvelopes` la leeria** (no es layout, ni anonima, ni xref) y la tomaria por un rack: exige una exclusion nueva en el barrido. El usuario puede renombrarla, explotarla o purgarla. Peor opcion |
| **A4** | `Database.SummaryInfo` (propiedades personalizadas) | Nivel dibujo por definicion; visible en las propiedades del DWG | Mapa plano `string→string` **editable por el usuario** desde la UI de AutoCAD: un registro tipado se corrompe en silencio. Sin estructura |
| **A5** | Archivo sidecar junto al DWG | Testeable sin AutoCAD | **Deja de existir en cuanto el DWG se copia solo.** Contradice el objetivo: el registro debe viajar dentro del dibujo |

**Recomendacion: A1.** No por reutilizacion —no hay ninguna— sino porque es **la unica cuya unicidad
por dibujo es estructural** y la unica que no obliga a tocar el barrido existente. A2 se descarta
aunque sea la mas barata: su unicidad depende de una convencion que nadie puede hacer cumplir, y §1
prohibe elegir por coste.

**Reparto por capas** (AGENTS conv. 1, [ADR-0006](../adr/0006-autocad-solo-en-plugin.md)):

```
RackCad.Application/Persistence/   ProjectVariablesDocument (DTO) + ProjectVariablesStore
                                   (Serialize/Deserialize)  ← PURO, cubierto por la suite Core
RackCad.Plugin/                    ProjectVariablesData.Read/Write(Transaction, Database)
                                   ← unico codigo que toca el NOD; deliberadamente minimo
```

Ese reparto es la **mitigacion de R2**: el almacen del DWG no tiene hoy ninguna prueba de
comportamiento (§2.3 del Discovery), asi que todo lo que pueda vivir fuera del Plugin, vive fuera.

**Precision de API (C2 punto 7).** El acceso correcto es
**`Database.NamedObjectsDictionaryId`** —un `ObjectId`— abierto dentro de la transaccion como
`DBDictionary`; sobre el se crea o se lee la entrada `RACKCAD_PROJECT`. Es una correccion de
**nomenclatura**, no de decision: A1 sigue siendo la recomendacion.

> Nota: la afirmacion negativa del Discovery §2.1 **no se ve afectada**. La busqueda uso la cadena
> `NamedObjectsDictionary`, que es **subcadena** de `NamedObjectsDictionaryId`, asi que habria
> encontrado ambas formas. Los ceros siguen siendo ceros.

**A verificar cuando haya AutoCAD** (no se afirma aqui): que `PURGE` no toque una entrada del NOD con
Xrecord propio. Es la unica duda operativa de A1 dentro del alcance minimo de §9.2.

### D-02 — `VariableId` estable e independiente de `Name`

| Alt | Forma | Tradeoff |
|---|---|---|
| **B1** | **GUID** (`Guid.NewGuid().ToString()`) | Estable por construccion; **precedente directo**: `RackEmbedDocument.Id` es exactamente eso para la identidad del rack. Ilegible a ojo |
| B2 | Slug derivado del nombre (`holgura-vertical`) | Legible; **pero renombrar rompe las referencias**, que es justo lo que el requisito prohibe. Descartada por definicion |
| B3 | Entero incremental | Compacto; colisiona en cuanto dos conjuntos de variables se encuentren, y no gana legibilidad real |

**Recomendacion: B1**, comparado **`OrdinalIgnoreCase`** —la misma politica que `FindRackBlocks` usa
para el GUID del rack (`RackCommandSupport.cs:124`)—, para no introducir una segunda regla de
comparacion de identidades en el mismo repositorio.

`Name` es **solo etiqueta**: se muestra, se edita libremente y **no participa en ninguna resolucion**.

### D-03 — `Type` y `Definition` separados; ID22A solo `Length` literal en pulgadas

> **Corregido en V2 (C2-1).** V1 justificaba el discriminador diciendo que «ID22B agregara tipos». Eso
> era **incorrecto**: ID22B son **formulas**. La justificacion real es otra —y mas fuerte—, y ademas
> obliga a **partir el modelo en dos ejes** que V1 tenia fundidos en uno.

Una variable tiene **dos** cosas que pueden crecer por separado, y confundirlas es lo que V1 hacia:

```
Type       — QUE es el valor            ID22A: Length (pulgadas)
Definition — DE DONDE sale el valor     ID22A: Literal(valor)
                                        ID22B: Expression(...)   ← el eje que crece con formulas
```

| Alt | Modelo | Tradeoff |
|---|---|---|
| **C1** | **`Type` discriminado + `Definition` discriminada**, ejes independientes | Additivo en los dos ejes por separado; un tipo o una definicion desconocidos son **detectables** (D-08 los reporta, no los ignora) |
| C2 | `object` + etiqueta de tipo | Flexible y opaco; obliga a `switch` de casteo en cada consumidor |
| C3 | **Sin tipo**: todo `double` literal | Lo mas simple hoy y lo que bloquea **las dos** continuaciones (ver abajo) |
| C4 | Una coleccion por tipo | Sin casteos; multiplica el formato persistido por cada tipo nuevo |
| C5 | `Type` discriminado pero **valor literal fijo** (sin `Definition`) | Suficiente para ID22A; **ID22B tendria que romper el esquema** para meter una expresion donde hoy hay un escalar |

**Recomendacion: C1.** ID22A acepta **exactamente un** `Type` —`Length`, en **pulgadas**, coherente
con [ADR-0005](../adr/0005-estrategia-de-unidades.md), sin introducir conversion— y **exactamente una**
`Definition`: `Literal`.

**Por que el discriminador de `Type` sigue siendo necesario, ahora que ID22B no lo justifica:**

1. **Candidatos no numericos ya identificados.** El Discovery §4.7 senala `DimensionStyle` —una cadena,
   elegida de una tabla de nivel dibujo y hoy guardada por rack— como el candidato mas claro despues
   del slice. Un modelo sin tipo obliga a romper el esquema para admitirlo.
2. **Una formula necesita operandos tipados.** ID22B no puede evaluar `a + b` sin saber si suma
   longitudes o concatena texto, ni comprobar coherencia de unidades. El tipo es **precondicion** de
   ID22B, aunque ID22B no agregue tipos.

**Y `Definition` es el punto de extension que ID22B ocupa.** Persistirla discriminada desde el primer
dia es lo que permite que `Expression` entre **como un caso mas**, aditivo, sin subir el `SchemaVersion`
mayor y sin tocar `PropertyValue<T>` (§1-bis).

C3 y C5 se descartan aunque hoy bastarian: son las decisiones que mas caro salen de corregir, y cada
una bloquea una continuacion distinta ya definida por el dueno.

### D-04 — `Literal` vs `ProjectVariableReference`: la forma de la propiedad

Premisa C-2: si hay referencia, **la referencia gobierna**. La pregunta abierta es **como se persiste
el vinculo**.

| Alt | Forma | A favor | En contra |
|---|---|---|---|
| **D1** | Campo hermano por propiedad: `VerticalClearance` (double, existente) **+** `VerticalClearanceVariableId` (string, nuevo nullable) | Minimo, explicito, **cero cadenas magicas**, trivial de probar | **Un campo nuevo por cada propiedad vinculable**: ID22B con N propiedades = N campos |
| D2 | Sustituir el escalar por un objeto `{ mode, literal, variableId }` | Expresivo | **Rompe la compatibilidad del campo escalar existente** y exigiria migracion ⇒ **prohibida por C-1**. Descartada por premisa |
| ~~D3~~ | ~~`Bindings: { "<slot>": "<variableId>" }`~~ — **RETIRADA en V2** | Un solo campo | **El valor es una cadena desnuda**: no dice **que clase** de referencia es, asi que ID21 (`RackId` + `PropertyId`) no cabe sin cambiar el tipo del valor. Sustituida por D4 |
| **D4** | **`PropertyValues: { "<PropertyId>": { "kind": "projectVariable", "variableId": "<id>" } }`** — referencia **tipada** y discriminada | Un solo campo **y** un punto de extension explicito: `kind` es exactamente donde ID21 anade `rackProperty`. Un `kind` desconocido es **detectable** (D-08) | Formato algo mas verboso que una cadena |

**Recomendacion: D4 (corregida en V2 por C2-3 y C2-4).**

V1 recomendaba D3. **Se retira** porque su valor era una cadena desnuda: `slot → variableId` codifica
«esta propiedad apunta a **una variable**» como una suposicion implicita del formato. En cuanto ID21
introduce `RackPropertyReference(RackId, PropertyId)` —dos datos, no uno—, D3 obliga a cambiar el tipo
del valor del mapa, que **es** una ruptura de esquema. D4 lo evita reconociendo desde el principio que
lo persistido es un **caso de `PropertyValue<T>`**, no un id suelto.

**Contrato de D-04:**

1. **El escalar legacy se conserva como el literal.** `SelectivePalletDesignDocument.VerticalClearance`
   sigue existiendo, con su tipo y su nombre. No se sustituye, no se migra, no se vacia (C-1). Esto
   descarta D2 por premisa, igual que en V1.
2. **La referencia vive en un mapa aparte**, tecleado por `PropertyId` (§1-bis) y con valor
   **discriminado por `kind`**.
3. **ID22A acepta un solo `kind`: `projectVariable`.** Ningun otro se implementa.
4. **Un `kind` desconocido es error visible**, nunca se ignora ni cae al literal (D-08).
5. **ID21 anadira `rackProperty`** —`{ "kind": "rackProperty", "rackId": "...", "propertyId": "..." }`—
   como **caso adicional**, sin tocar los existentes ni subir el `SchemaVersion` mayor. **No se
   implementa aqui**, y este documento no disena su resolucion.

Forma persistida del slice:

```jsonc
{
  "VerticalClearance": 6.0,                       // literal legacy: intacto, sigue siendo el fallback
  "PropertyValues": {
    "selective.verticalClearance": { "kind": "projectVariable", "variableId": "9f3c…" }
  }
}
```

**Las claves son constantes, no literales dispersos.** Es la disciplina que `CatalogBlockParameters`
ya usa para los nombres de parametro de bloque (`SelectiveRackDefaults.LengthParam`, `PeralteParam`,
`PalletAltoParam`), y neutraliza el unico argumento serio contra un mapa tecleado por cadena.

**D1 sigue siendo defendible si el dueno prefiere el minimo absoluto** para una sola propiedad, pero su
coste sube con la correccion: con ID21 definido, D1 necesitaria **dos** campos hermanos nuevos por
propiedad (`…VariableId` y despues `…RackId`/`…PropertyId`), o un campo hermano cuyo tipo cambia. El
tradeoff pasa de «cuando pago el escalar» a «cuantas veces rompo el esquema».

**Semantica del valor efectivo:**

```
valor efectivo(propiedad) =
    hay entrada en PropertyValues  →  se resuelve segun su kind      (C-2: la referencia gobierna)
                                      ID22A: projectVariable → valor de esa variable
    no hay entrada                 →  el literal persistido          (C-1: literal preservado)
```

El literal **nunca se borra** al vincular: se conserva y es lo que D-11 devuelve al desvincular.

---

## 3. El vertical slice

### D-05 — `SelectivePalletDesign.VerticalClearance`, y nada mas

C-5 fija el slice. En ID22A **una sola propiedad** es vinculable:

```csharp
// src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:29-30
/// <summary>Vertical clearance ("holgura") above a pallet inside its clear opening (in). Editable; default 6".</summary>
public double VerticalClearance { get; set; } = 6.0;
```

Es un buen slice por tres razones verificadas en el Discovery: existe **solo** en el Selectivo (9
lineas, 6 archivos), tiene **un unico consumidor geometrico** (`SelectiveGeometryResolver.cs:96`), y su
efecto es **visible en el dibujo**, asi que la validacion del dueno puede confirmarlo a ojo.

**Fuera del slice, por C-5**: `ClearHeight` (Dinamico/Push Back, por nivel), `RequestedClearHeight`
(Cantilever), `PalletTolerance` y `PalletDepth`. El Discovery §4.2-4.4 sigue describiendo esas
asimetrias como **hechos del arbol**; ID22A no las toca.

### D-06 — `ClearOverride` conserva **exactamente** su precedencia actual

La cadena vigente, verificada:

```csharp
// src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:395-402
private static double SeparationFor(SelectiveCell cell, double palletAltoBelow, double clearance, double paso)
{
    if (cell.ClearOverride.HasValue && cell.ClearOverride.Value > 0.0)
    {
        return Math.Max(paso, RoundUpToMultiple(cell.ClearOverride.Value, paso));
    }
    return Separation(palletAltoBelow, clearance, cell.BeamPeralte, paso);
}
```

**La variable sustituye el argumento `clearance`, y nada mas.** Es decir:

```
ClearOverride de la celda   (manual, por celda)        ← GANA, sin cambios
    ↓ si no hay
valor efectivo del rack     (variable si hay binding, si no el literal)
```

**Alternativa considerada y descartada**: que la variable de proyecto tambien pise `ClearOverride`.
Se descarta porque invierte una precedencia que hoy existe y esta probada, convertiria un override
manual en un valor ignorado sin aviso, y **ninguna premisa la pide**. C-2 dice que la referencia
gobierna **el valor de la propiedad**, no que el proyecto gobierne las excepciones por celda.

Consecuencia practica y deseable: `ClearOverride` sigue siendo la **valvula de escape** para el caso
en que una celda concreta deba desviarse del estandar del proyecto.

---

## 4. Persistencia

### D-07 — Compatibilidad hacia atras, y ausencia del registro = vacio

Dos limites distintos, y **el segundo tiene un agujero verificado**.

**Limite 1 — el registro del dibujo.** C-1: un DWG sin entrada en el NOD **no es un error ni un
dibujo a migrar**; es un proyecto con **cero variables**. Se lee como registro vacio, todos los racks
siguen siendo literales, y nada cambia en el dibujo. Es tambien lo que ocurre con un DWG creado por
una version anterior, sin ningun trabajo adicional.

**Limite 2 — el diseno del rack.** Aqui esta el hallazgo:

```
DTO con [JsonExtensionData]:  RackEmbedDocument, RackProjectDocument, PushBackDesignDocument,
                              FlowBedDocument, CantileverLineDocument, LargueroDocument
DTO SIN [JsonExtensionData]:  SelectivePalletDesignDocument      ← el del slice
```

`SelectivePalletDesignDocument` **tiene su propio `SchemaVersion` (`"1.0"`) pero no preserva campos
desconocidos**. Consecuencia concreta: si una version nueva escribe el vinculo dentro del diseno
selectivo y una version **anterior** abre ese dibujo, edita el rack y lo vuelve a guardar, **el
vinculo desaparece en silencio** y el rack queda como literal sin que nadie lo note. Es exactamente la
clase de fallo silencioso que el proyecto persigue desde I-03.

> ### ⚠ Correccion de V2 (C2-5): V1 se equivocaba aqui
>
> V1 recomendaba anadir `[JsonExtensionData]` a `SelectivePalletDesignDocument` **«para evitar la
> perdida silenciosa»**. Eso es **falso**, y no es un matiz: un binario **pre-I-47 ya compilado** no
> tiene ese atributo y **nunca lo tendra**. Anadirlo hoy no cambia el comportamiento de ninguna version
> que ya exista. La perdida silenciosa que V1 decia evitar **seguiria ocurriendo exactamente igual**.

### Dos compatibilidades distintas, que V1 fundia en una

| | Que es | ¿Se puede garantizar desde I-47? |
|---|---|---|
| **Backward compatibility** | Una version **nueva** lee un documento **viejo** | **SI**, y ya esta resuelto: campo nullable ausente = legado, `SchemaVersionPolicy.IsReadable` acepta major menor o ausente |
| **Downgrade / forward round-trip** | Una version **vieja, ya compilada**, lee un documento **nuevo**, lo edita y lo vuelve a guardar | **Solo con mecanismos que esa version vieja YA tenga compilados.** Nada que se anada ahora al codigo la alcanza |

**La consecuencia manda sobre el diseno:** el unico control que I-47 tiene sobre una version anterior
es **lo que esa version ya ejecuta**. Y hay exactamente dos cosas ya compiladas y verificadas:

```csharp
// src/RackCad.Application/Persistence/SelectivePalletDesignStore.cs:43  — YA compilado en versiones anteriores
SchemaGuard.CheckReadable(document?.SchemaVersion, SelectivePalletDesignDocument.CurrentSchemaVersion, "El diseño del selectivo");
```
```csharp
// src/RackCad.Application/Persistence/SchemaGuard.cs  — LANZA, no devuelve null
throw new InvalidOperationException(
    what + " fue creado con una versión más nueva de RackCad (esquema " + storedVersion +
    "); actualiza la aplicación para abrirlo.");
```
```csharp
// src/RackCad.Application/Persistence/RackEmbedComposer.cs  — YA compilado: el SOBRE si preserva desconocidos
ExtensionData = source?.ExtensionData
```

### Alternativas, y por que F3 se rechaza

| Alt | Donde vive la referencia | Que hace una version **pre-I-47** con un rack vinculado |
|---|---|---|
| F1 | Diseno selectivo, `SchemaVersion` sigue `1.0` | **Lo desvincula en silencio.** Inaceptable |
| **F2 — DECIDIDA** | Diseno selectivo, con **promocion pegajosa de schema** | **Se niega a abrirlo**, con el mensaje ya compilado de `SchemaGuard`. Falla **visible**, cero corrupcion |
| ~~F3~~ | En el **sobre** (`RackEmbedDocument`) | **RECHAZADA en V3.** Lo conserva... y ese es justamente el problema (abajo) |
| E3 (V1) | Diseno, sin nada mas | Igual que F1 |

**Por que F3 se rechaza (C3-1).** El argumento a su favor era que una version vieja **preserva** el
binding, gracias a `[JsonExtensionData]` + `RackEmbedComposer`, ya compilados. Es cierto — y es
**insuficiente**, porque preservar bytes no es entender semantica:

> Una version pre-I-47 **conserva** la referencia en el sobre, pero **no sabe que existe**. Al abrir
> ese rack lee la propiedad de su **literal**, y con el literal **edita y redibuja**. El resultado es
> geometria **coherente con el literal** y **contradictoria con la autoridad** que una version nueva
> resolvera despues: el dibujo muestra una cosa y el modelo dice otra, sin que nada falle.

F3 convierte un fallo ruidoso en una **divergencia silenciosa entre dibujo y autoridad**, que es peor
que lo que intentaba evitar y es exactamente lo que el proyecto persigue desde I-03. **La preservacion
sin comprension no es compatibilidad.**

> **Nota conservada de V2, por honestidad:** V1 objetaba a F3 que «el vinculo quedaria duplicado por
> cada vista». Ese argumento estaba mal calibrado —el diseno entero ya viaja duplicado en el Xrecord de
> cada bloque-vista— y **no** es el motivo del rechazo. F3 cae por semantica, no por duplicacion.

### Contrato de compatibilidad — DECIDIDO (F2 con promocion pegajosa)

**F2 queda decidida** (C3-1): la referencia vive en el **diseno selectivo**, y el `SchemaVersion` del
documento se **promueve de forma pegajosa** (C3-2). «Pegajosa» significa **monotona**: sube y no baja.

```
documento 1.x que NUNCA ha llevado PropertyValues   ->  se sigue escribiendo 1.x
introducir PropertyValues                           ->  PROMUEVE a 2.0
documento ya almacenado como 2.x                    ->  se escribe 2.x SIEMPRE
    aunque se elimine el ultimo binding             ->  NO vuelve a 1.x
desvincular (D-11)                                  ->  materializa el literal y borra la referencia,
                                                        pero NO degrada el schema
minor posterior del mismo major                     ->  se preserva segun SchemaVersionPolicy
```

**Por que pegajosa y no «segun el contenido actual».** Una regla que mira solo el contenido hace que el
documento **oscile** `1.0 -> 2.0 -> 1.0` al vincular y desvincular. Eso tiene tres consecuencias malas y
ninguna buena:

1. **«Que versiones pueden abrir este rack» pasaria a depender de estado transitorio**, imposible de
   explicar a un usuario.
2. **El dibujo ya ha sido tocado por la semantica nueva.** Aunque este rack quede sin vinculos, el DWG
   puede tener un registro `ProjectVariables` y otros racks vinculados; devolverlo a 1.x afirma una
   compatibilidad que el archivo, como conjunto, ya no tiene.
3. Un ciclo de bajada y subida es una fuente clasica de perdidas sutiles. Monotono no la tiene.

**Coste aceptado, dicho sin adornos:** un rack que se vinculo **una vez** queda fuera del alcance de las
versiones anteriores **para siempre**, incluso despues de desvincularlo. Es deliberado.

**Lo que F2 garantiza, exactamente:**

- Un DWG sin registro de variables se lee como **registro vacio** (C-1). Sin migracion.
- Un rack **que nunca se vinculo** conserva `1.x` y **cualquier version anterior lo sigue abriendo y
  editando** con normalidad. El caso comun no se degrada.
- Un rack **promovido** es **ilegible** para una version anterior, que lo dice con un mensaje claro y
  **no lo modifica** — ni lo desvincula, ni lo redibuja con el literal.
- Una version I-47+ lee y escribe ambos.

**Lo que F2 NO garantiza:**

- **No** permite que una version anterior edite un rack promovido. Lo impide a proposito.
- **No** es reversible: la promocion no se deshace (es el punto).
- **No** hace nada retroactivo sobre documentos que una version anterior ya guardo.
- **No** protege campos anadidos **dentro** de tipos anidados sin version propia.

### Que politica de escritura nueva necesita el Selectivo (no se implementa)

Hoy la version de escritura sale de `SchemaVersionPolicy.ResolveWriteVersion(stored, current)`, donde
`current` es **una constante del build**. Verificado:

```csharp
// src/RackCad.Application/Persistence/SchemaVersionPolicy.cs — ResolveWriteVersion
if (!TryParse(storedVersion, out var storedMajor, out var storedMinor)
    || !TryParse(currentVersion, out var currentMajor, out var currentMinor)
    || storedMajor != currentMajor)
{
    return currentVersion;            // <- majors distintos: DEVUELVE la constante del build
}
return storedMinor > currentMinor ? storedVersion.Trim() : currentVersion;
```

Esa forma **no puede** expresar la promocion pegajosa: con `current = "1.0"` y `stored = "2.0"` los
majors difieren y devuelve **`"1.0"`**, que es precisamente el downgrade prohibido. Y subir la constante
a `"2.0"` promoveria **todos** los documentos, incluidos los que nunca se vincularon.

Hace falta, por tanto, **un helper nuevo** —aditivo, puro y testeable en la suite Core— con esta forma:

```
ResolveStickyWriteVersion(storedVersion, hasPropertyValues) :

    MajorOf(stored) >= 2   ->  ResolveWriteVersion(stored, "2.0")   // pegajoso: nunca baja; respeta minors
    hasPropertyValues      ->  "2.0"                                 // promocion
    en otro caso           ->  ResolveWriteVersion(stored, "1.0")    // camino legado, intacto
```

Tres consecuencias que la implementacion debe asumir y que se declaran aqui en vez de darse por hechas:

1. **`SelectivePalletDesignDocument` necesita dos constantes de linea** (la legada `1.x` que sigue
   escribiendo y la promovida `2.0`), no una sola `CurrentSchemaVersion`.
2. **La version de LECTURA del build I-47+ debe ser la `2.x`**, o `SchemaGuard.CheckReadable` rechazaria
   los documentos que el propio build acaba de promover. Leer y escribir dejan de compartir constante,
   y eso debe ser explicito.
3. **`hasPropertyValues` se evalua sobre el documento que se va a ESCRIBIR**, no sobre el que se leyo;
   si no, vincular y guardar en el mismo paso no promoveria.

`SchemaVersionPolicy.ResolveWriteVersion` **no se modifica**: el helper nuevo lo **usa**. Los otros
cuatro stores siguen exactamente igual.

### Se conserva `[JsonExtensionData]` en el DTO selectivo

**Si, pero con la garantia correcta y por un motivo distinto del que daba V1.**

- **No** protege frente a versiones pre-I-47. Ninguna afirmacion de este documento depende ya de eso.
- **Si** protege de I-47 en adelante: una version N preservara los campos que escriba una version N+1
  del mismo major.
- Elimina una **asimetria real**: cinco DTO hermanos lo tienen y el del slice no.

Es, por tanto, una mejora **justificada por si misma**, no un prerequisito del que dependa la
compatibilidad. Si el dueno prefiere no tocar el DTO selectivo, **F2 sigue funcionando igual**.

### D-08 — `VariableId` inexistente = **error visible**, jamas fallback silencioso

Un binding que apunta a una variable que ya no existe es alcanzable (borrado, dibujo tocado por otra
version, edicion manual). **Nunca** debe resolverse cayendo al literal: eso cambiaria la geometria en
silencio.

| Alt | Comportamiento | Tradeoff |
|---|---|---|
| A | Caer al literal | Silencioso y **por eso inaceptable**: el dibujo cambia y nadie se entera |
| B | Excepcion cruda | Visible, pero rompe la operacion sin mensaje util |
| **C** | **Preflight** que aborta **toda** la operacion con un mensaje que **nombra el rack y la variable** | Consistente con lo que el repositorio ya hace |

**Recomendacion: C**, y con los precedentes ya escritos, no con maquinaria nueva:

```csharp
// src/RackCad.Application/Persistence/KindDispatch.cs:137  — el mensaje vive en Application, es puro y testeable
public static string NotRecognized(string kind) => "RackCad: tipo de rack no reconocido (" + kind + ").";
```

```csharp
// src/RackCad.Plugin/RackCommandSupport.cs — PreflightInnerSources
// "...If ANY block carries an incompatible-MAJOR or wrong-kind inner design, the whole edit ABORTS
//  with a visible message and NO block is modified (no partial update)."
```

El mensaje de «variable inexistente» debe vivir en **Application** (como `KindDispatchMessages`), de
modo que la suite Core pueda probarlo sin AutoCAD, y el Plugin solo lo muestre.

---

## 5. Ciclo de vida de una variable

### D-09 — Renombrar es por `Id`

Consecuencia directa de D-02: renombrar cambia `Name` y **nada mas**. Ningun consumidor se toca,
ningun rack se redibuja, no hace falta preflight. Es la razon de ser de un id independiente del
nombre, y por eso no tiene alternativas que comparar.

### D-10 — Borrar: bloquear vs referencias rotas

> Aviso de notacion: las etiquetas `F1`/`F2`/`F3` de esta decision son **locales a D-10** y no tienen
> relacion con las `F1`/`F2`/`F3` de D-07. El dueno decidio **F1 aqui** y **F2 alli**.

| Alt | Comportamiento | Tradeoff |
|---|---|---|
| **F1** | **Bloquear** mientras existan consumidores, **listandolos** | Ningun estado roto se crea nunca. Exige el barrido de consumidores — que **ya existe** (D-12). El usuario debe desvincular antes: mas pasos, pero ninguno sorpresa |
| F2 | Permitir y dejar referencias rotas | «Barato» al borrar y caro despues: traslada el fallo a un momento futuro y a otra persona |
| F3 | Borrar y **desvincular automaticamente** todos los consumidores | Comodo, pero **reescribe en silencio el diseno persistido de N racks** como efecto colateral de un borrado. Destructivo y sorprendente |

**DECIDIDA: F1** (C3-3). No es ya una recomendacion: borrar una variable **con consumidores se
bloquea**, y la operacion **lista los consumidores** que lo impiden. Con dos matices que forman parte
de la decision:

1. **F3 puede existir SOLO como accion explicita y separada** —«desvincular todos materializando» y
   despues borrar—, **nunca** como efecto automatico del borrado. El usuario tiene el camino comodo
   **y** ve lo que va a pasar. El desvinculado de esa accion es el de D-11: **materializa** el valor
   efectivo, asi que ningun rack cambia de geometria al ejecutarla.
2. **La ruta de error de F2 se implementa igualmente.** Aunque el borrado se bloquee, una referencia
   colgante sigue siendo alcanzable por otros caminos (otra version, edicion externa), asi que D-08 no
   es opcional por haber elegido F1.

### D-11 — Desvincular: materializar el valor efectivo

| Alt | Al desvincular, el literal queda… | Efecto geometrico inmediato |
|---|---|---|
| **G1** | **= valor efectivo actual** (el de la variable) | **Ninguno.** El rack sigue dibujando exactamente igual |
| G2 | = el literal anterior al vinculo | **Salto visible**: el rack cambia de forma al desvincular |
| G3 | = default o cero | Peor version de G2 |

**Recomendacion: G1 (materializar).** Es la unica con efecto geometrico nulo en el instante de
desvincular, que es lo que hace la operacion segura y comprensible.

**Aviso de implementacion, porque G2 es el bug facil:** si al limpiar el binding se deja el campo
literal como estaba, el resultado **es G2 sin haberlo elegido**. Materializar es un paso **activo**
(escribir el valor efectivo en el literal antes de borrar el vinculo) y merece su propia prueba.

---

## 6. Propagacion (C-3)

### D-12 — Descubrimiento de consumidores, deduplicado por `RackId`

El barrido ya existe y ya es de nivel dibujo: `RackBlockFinder.ScanEnvelopes` recorre las
**definiciones** de la `BlockTable`. Pero devuelve **una entrada por bloque-vista**, y un rack tiene
varias, asi que hay que agrupar.

```csharp
// src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs — el precedente de agrupacion
if (embed == null || string.IsNullOrWhiteSpace(embed.Id) || string.IsNullOrWhiteSpace(embed.Kind)) { continue; }
var copies = envelope.DirectReferenceCount;
if (!byRack.TryGetValue(embed.Id, out var aggregate)) { byRack[embed.Id] = new RackAggregate { ... }; }
```

**Deduplicacion recomendada: por `embed.Id` (`OrdinalIgnoreCase`)** — un `RackId` = un rack logico,
aunque tenga frontal, lateral y planta.

**Y aqui hay una diferencia con el precedente que hay que decidir a proposito.** El Discovery (H-02)
registro que conviven **dos politicas de filtro**: `FindRackBlocks` exige solo el GUID;
`RACKBOMTOTAL`/`RACKLISTA` exigen `Id` **y** `Kind`, y **saltan en silencio** (`continue`) lo que no
cumple. Para un BOM, saltar un bloque ilegible produce un total incompleto y visible. **Para una
propagacion, saltar en silencio produce un dibujo incorrecto y mudo**: un rack vinculado que no se
redibuja queda mostrando el valor viejo.

**Recomendacion**: la propagacion **no hereda la politica de `continue`**. Un bloque sin `Id`, sin
`Kind` o cuyo diseno no deserializa **aborta la operacion** (D-13) en cuanto **pueda** ser consumidor;
solo se ignoran los bloques que demostrablemente no llevan payload de RackCad.

### D-13 — Preflight completo, commit unico y **un solo** `Regen`

> ### ⚠ Correccion de V2 (C2-6): V1 daba por atomico un lote que no lo es
>
> V1 listaba «redibujo en sitio» y «un solo `Regen`» como piezas ya existentes y presentaba el bucle
> sobre N racks como si heredara la atomicidad de RACKEDITAR. **No la hereda.** `RedrawInPlace`
> **abre y commitea su propia transaccion en cada llamada**:

```csharp
// src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs — RedrawInPlace
using (document.LockDocument())
{
    BlockLibraryImporter.EnsureForPlan(database, plan);
    using (var transaction = database.TransactionManager.StartTransaction())
    {
        outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan, out staleDefs);
        RackBlockData.Write(transaction, blockId, payloadJson);
        transaction.Commit();                       // ← COMMIT por bloque
    }
    LateralHeaderDrawer.PurgeUnreferenced(database, staleDefs);   // ← ademas, purga DESPUES del commit
    ApplyRegen(document, regen);
}
```

Y el metodo entero esta envuelto en `try/catch` que devuelve `HeaderPlacementResult.Failure(...)`. Por
tanto, un bucle sobre N racks que falle en el rack **k** deja **k-1 racks ya commiteados** y el resto
sin tocar: el dibujo queda con **dos valores distintos para la misma variable**, que es exactamente el
estado que C-3 prohibe. Esto **no** lo arregla anadir un preflight: el preflight evita empezar mal, no
deshace lo ya commiteado si algo falla a mitad.

**Lo que realmente existe, corregido:**

| Pieza | Estado real |
|---|---|
| Preflight de lote que aborta antes de trabajar | **existe** — `KindHandlerDispatch.TryResolveAll`, `RackCommandSupport.PreflightInnerSources` |
| Redibujo de **un** bloque | **existe** — `RedrawInPlace`, pero **con transaccion propia** |
| **Atomicidad sobre N bloques** | **NO existe** — y V1 decia que si |
| `Regen` unico | **existe** como convencion (`regen: false` + un `Regen()` final), no como garantia del writer |
| Aplicar diseno y redibujar **sin ventana** | **NO existe** (Discovery H-03) |

**Contrato exigido (los cuatro puntos, no negociables):**

1. **Preflight completo antes de mutar nada.** Escanear, agrupar por `RackId`, resolver handler,
   deserializar cada diseno y resolver **todas** las referencias implicadas. Cualquier fallo aborta
   **antes** de la primera escritura.
2. **Coherencia conjunta**: el registro `ProjectVariables` **y** todos los consumidores quedan en un
   estado mutuamente consistente. No se admite «variable ya cambiada, racks a medio redibujar».
3. **Commit unico, o un mecanismo equivalente con rollback real.** Una operacion, un punto de no
   retorno.
4. **Un solo `Regen`**, al final, despues del commit.

**Frontera arquitectonica propuesta (no se implementa aqui).** El seam necesario ya esta **un nivel mas
abajo**: `LateralHeaderDrawer.RedefineSystemBlock(database, transaction, blockId, plan, out staleDefs)`
**ya recibe la transaccion**. Quien la crea y la commitea es `SystemBlockWriter`. La frontera, por
tanto, es exactamente esa capa:

```
HOY     SystemBlockWriter.RedrawInPlace(document, …)            crea la transaccion, commitea, purga, regenera
                                                                 → atomicidad = 1 bloque

PROPUESTO  un writer de LOTE cuya transaccion es del LLAMADOR:
           - el llamador abre UNA transaccion (y el document lock)
           - por cada bloque: redefinir + escribir payload  → SIN commit
           - escribir tambien el registro ProjectVariables  → misma transaccion
           - UN commit
           - DESPUES del commit: purga de definiciones huerfanas acumuladas
           - UN Editor.Regen()
```

Tres detalles que esa frontera debe resolver, y que se nombran para que nadie los descubra tarde:

- **`RedefineSystemBlock` opera sobre la `Transaction` del llamador** (C3-4). Ya la recibe como
  parametro, asi que el cambio es de **quien la crea**, no de su firma. La transaccion unica cubre
  **las tres escrituras**: el registro `ProjectVariables`, la geometria redefinida y los payloads de
  cada bloque-vista. No hay tres commits, hay uno.
- **`PurgeUnreferenced` sale del bucle.** Hoy corre por bloque y **despues del commit**; en el modelo de
  lote se **acumulan** los `staleDefs` de todos los racks y se purga **una vez**, tras el commit unico.
- **`ApplyRegen` deja de decidirse por bloque.** El `regen: false` de hoy es una convencion del
  llamador; en el lote el writer no regenera nunca y el **unico** `Regen` es responsabilidad explicita
  del orquestador, al final.

#### La biblioteca de bloques queda FUERA del lote (C3-5)

**Correccion de V2**, que proponia «resolver `EnsureForPlan` en el preflight». Eso era incorrecto: esa
llamada **no puede formar parte ni del preflight ni del lote**, porque **muta el `Database` fuera de
cualquier transaccion del llamador**. Verificado:

```csharp
// src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs — EnsureBlocks
using (var transaction = db.TransactionManager.StartTransaction())   // <- abre la SUYA
{
    ...  // detecta los que faltan
    transaction.Commit();                                            // <- y la COMMITEA
}
...
source.WblockCloneObjects(ids, db.BlockTableId, mapping, DuplicateRecordCloning.Ignore, deferTranslation: false);
```

`WblockCloneObjects` escribe en la `BlockTable` del dibujo **por su cuenta**. Meterlo dentro de una
operacion que promete «commit unico o rollback real» romperia esa promesa en el primer rack que
necesitara importar algo.

**El contrato, entonces:**

1. La propagacion **VERIFICA** que las definiciones de bloque que cada consumidor necesita **ya existan**
   en el dibujo. La comprobacion es de **solo lectura** y es la misma que el importador usa para
   detectar lo que falta: `blockTable.Has(name)` sobre los nombres del plan
   (`plan.LooseInstances` + `plan.Headers.SelectMany(g => g.Instances)`).
2. **Si falta alguna, ABORTA** —antes de tocar la variable y antes de tocar ningun consumidor— y
   **nombra las que faltan**. Ningun cambio parcial, coherente con D-08.
3. **No se importa ni se repara la biblioteca como efecto colateral** de cambiar una variable. Reparar
   la biblioteca es una operacion propia, que el usuario pide a proposito; un cambio de valor no es el
   momento de descubrir que falta un DWG.

Hay una razon adicional, verificada, para no delegar en el importador: si el archivo de biblioteca **no
existe**, `EnsureBlocks` **devuelve 0 en silencio** (`if (!File.Exists(path)) return 0;`). Apoyarse en
el durante una propagacion convertiria una biblioteca ausente en **geometria incompleta sin aviso**.

#### Viabilidad de la transaccion unica

**Deja de ser una pregunta de contrato** (C3-6). El contrato es el de los cuatro puntos de arriba, y se
sostiene: la unica operacion que se sabia problematica —la importacion de bloques— queda **fuera** por
el punto anterior, y `RedefineSystemBlock` ya esta escrito para trabajar sobre una transaccion ajena.

Lo que sigue siendo necesario es **comprobarlo ejecutando**, y eso es un **gate de implementacion y de
Owner Validation** (§14, CF-5'), no una condicion previa del Consensus Freeze ni una incognita de
diseno.

**Alternativa descartada: propagacion perezosa** (marcar los racks y redibujar cuando se abran).
Evitaria el barrido, pero contradice C-3 —«en una operacion»— y dejaria el dibujo en un estado
intermedio donde dos racks vinculados a la misma variable muestran valores distintos.

**Riesgo abierto y honesto**: no hay medicion de cuanto tarda esto en un dibujo grande. El repositorio
sabe que fijar parametros dinamicos por referencia es lento y por eso existe el patron ARRAY; una
propagacion sobre muchos racks es **trabajo nuevo de la misma familia**. Y una transaccion unica sobre
N racks **agrava** la cuestion, porque mantiene abierto mas tiempo un estado mas grande. No se estima
aqui.

### D-14 — RACKEDITAR y RACKDUPLICAR

**RACKEDITAR.** El editor abre un rack que puede estar vinculado. Dos exigencias:

1. El campo muestra el **valor efectivo** y se ve **vinculado** (D-16), no editable como literal.
2. **Guardar no debe desvincular en silencio.** Es el riesgo real: la ventana lee cajas de texto y
   arma un diseno (`RackSelectiveWindow.xaml.cs:2256` escribe `VerticalClearance = clearance`). Si el
   camino de guardado ignora el binding, cada edicion del rack lo **desvincula sin pedirlo**.
   Desvincular debe ser una **accion explicita** (D-11), nunca un efecto de guardar.

**RACKDUPLICAR.** C-4: la copia **conserva el `VariableId`**. El contrato queda:

```
RackId          →  NUEVO en la copia independiente   (comportamiento actual, no cambia)
VariableId(s)   →  IDENTICOS                          (C-4)
```

C-4 es coherente sin esfuerzo porque **una copia se queda en el mismo dibujo**, donde la variable
existe. Pero hay un detalle del arbol que la convierte en un requisito **activo**, no automatico:

```csharp
// src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs — RestampDesign
var store = new SelectivePalletDesignStore();
var design = store.Deserialize(designJson);
design.Id = newId;
design.Name = copyName;
return store.Serialize(design);
```

**El restamp del selectivo NO copia el JSON: lo re-serializa a traves del DTO.** Cualquier campo que
`SelectivePalletDesignDocument` no declare **se pierde en RACKDUPLICAR**. (Los kinds `dynamic`, `cama`
y `pushback` devuelven el JSON tal cual, asi que no comparten el problema; `cabecera` y `cantilever`
si.)

**Consecuencia que refuerza D-04 y D-07:** si la referencia viaja en el **diseno selectivo** (D-07 F2),
tiene que ser un **campo declarado** del DTO. Un campo no declarado sobreviviria a guardar y reabrir
pero **moriria en la primera duplicacion**, porque este restamp re-serializa a traves del DTO y el DTO
selectivo no preserva desconocidos.

> Precision heredada de V2, **actualizada**: este argumento era especifico de F2 y **F2 es ahora la
> opcion decidida** (D-07), asi que **aplica sin condiciones**. Bajo la rechazada F3 no habria hecho
> falta, porque `RestampEnvelope` re-serializa el mismo `RackEmbedDocument` y su `[JsonExtensionData]`
> si conserva lo desconocido — pero F3 cayo por semantica, no por esto. La decision de D-14 no cambia:
> el restamp **no debe tocar** las referencias, y eso **merece prueba de comportamiento propia**
> (D-17-bis), no un round-trip del Store.

**Detalle adicional del arbol**: RACKDUPLICAR clona **solo la definicion del bloque-vista que el
usuario pico**; las vistas hermanas del mismo rack no se copian. La propagacion (D-12) debe partir del
`RackId`, no de la vista, para no razonar sobre medio rack.

### D-15 — Guardar/exportar a la biblioteca un rack vinculado

Un `VariableId` **solo tiene sentido dentro de su dibujo**. La biblioteca
(`RackDesignLibrary`, ficheros `*.rackcad.json`) es **cruzada entre dibujos** por definicion, asi que
un diseno exportado con un vinculo llega a destino apuntando a nada.

| Alt | Al exportar | Tradeoff |
|---|---|---|
| **H1** | **Materializar**: la entrada de biblioteca guarda el **valor efectivo** como literal | La biblioteca sigue siendo portable y siempre valida. Se pierde la intencion «esto era una variable» |
| H2 | Conservar el vinculo y resolver o fallar al importar | Preserva la intencion; produce **importaciones que fallan** en cualquier dibujo que no tenga esa variable — la mayoria |
| H3 | Exportar el vinculo **y** una copia de la variable, fusionando al importar | Lo mas completo y lo mas caro: exige semantica de **fusion y conflicto** entre conjuntos de variables |

**Recomendacion: H1 para ID22A.** Y no solo por coherencia con D-11: **el repositorio ya trata la
frontera de la biblioteca exactamente asi**. Una entrada de biblioteca **nunca conserva la identidad
del rack**; al abrirla se adopta identidad nula y se acuna un GUID nuevo al insertar:

```csharp
// src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs
session.Identity.Adopt(null, document.Name); // a library template inserts as a NEW rack: no id yet, fresh GUID on insert (I-15)
```

Si el `RackId` —un identificador con alcance de dibujo— ya se descarta al cruzar a la biblioteca,
**un `VariableId` debe descartarse por la misma razon y con el mismo criterio**. H1 no inventa una
politica: aplica la existente a un identificador nuevo.

**H3 queda como futuro SEPARADO (corregido en V2 por C2-8).** V1 lo llamaba «territorio de ID22B», y
eso era un error de clasificacion: **ID22B son formulas entre variables**, un problema *dentro* de un
dibujo. Transferir o **fusionar conjuntos de variables entre dibujos** es un problema distinto —
identidad, colision de nombres, conflicto de valores, reconciliacion al importar— que no depende de
ID22B ni lo habilita. **No pertenece a ninguna de las iniciativas hoy definidas** (ID22A, ID22B, ID21)
y no se le asigna una: queda como futuro propio, sin numero.

**Aviso de implementacion**: el guardado del selectivo escribe
`RackProject.ForSelectiveRack(document)` con **el mismo `SelectivePalletDesignDocument`** que se
embebe en el DWG. Materializar es, por tanto, un paso **explicito antes de componer el proyecto de
biblioteca** — no ocurre solo.

---

## 7. Interfaz de usuario

> **Nota de alcance.** La fila de I-47 en ROADMAP —y el §3 del contrato— listan «UI» y «comando nuevo»
> como fuera de alcance. Describen lo que **no se construye**, y siguen siendo ciertas: esta seccion
> **disena en papel** y no toca `src/`. Si el dueno acepta D-16, el **contrato** debe reflejarlo antes
> de produccion (§14, CF-1) y la **fila de ROADMAP** se actualiza **al cerrar** (§14-bis), que es el
> unico momento que WORKFLOW seccion 2 permite.

### D-16a — Donde vive la ventana central

| Alt | Superficie | Tradeoff |
|---|---|---|
| **J1** | Entrada en `RackMainMenuWindow` + miembro nuevo en `MainMenuAction`, leido tras cerrar el modal | **Precedente exacto y reciente**: asi se resolvio la unica accion del menu que **no es un rack** |
| J2 | `[CommandMethod]` propio (p. ej. `RACKVARIABLES`) sin entrada de menu | Accesible pero **no descubrible** — el problema que I-36C existio para arreglar |
| J3 | Pestana dentro de cada editor de rack | Repetida N veces; el registro es **del dibujo**, no del rack |

**Recomendacion: J1 y J2 juntos** —entrada de menu **y** comando con alias, como tiene todo lo demas—.
El precedente literal:

```csharp
// src/RackCad.Plugin/RackMenuCommands.cs:36-40
if (menu.RequestedAction == MainMenuAction.GenerateStructuralSection)
{
    StructuralSectionCommandFlow.Run(document);
    return;
}
```

Y la regla de crecimiento del enum, que este caso **respeta** (un miembro mas, sin payload):

```csharp
// src/RackCad.UI/MainMenuAction.cs:17-18
/// This enum stays small on purpose. If it ever grows a family of variants with payloads, that is the
/// signal to give it its own typed hierarchy — not to widen the enum.
```

Dos consecuencias verificadas: la lista de botones del menu es **XAML fijo** (no hay registro de
entradas), y el comando nuevo debe anadirse a `RackCommandReference` —que ya arrastra el hueco H-04 del
Discovery—. Ademas, `WindowCensusGuardTests` obliga a **clasificar** cualquier ventana nueva en uno de
cuatro arquetipos; una ventana de variables encaja en `Utilities` o `ConfigurationDialogs`, y el
arquetipo elegido arrastra su contrato (`DialogWindowContractTests`).

### D-16b — Estado visual del vinculo: el problema real

**No existe ningun control de «heredado / sobrescrito» en el repositorio.** Lo que hay son cuatro
soluciones ad-hoc, y una de ellas es un aviso serio:

| Patron | Donde | Que ensena |
|---|---|---|
| Solo prosa | Selectivo: etiqueta «Manual (opcional · vacio = auto)» + tooltip | **Auto y manual se ven identicos.** Es el listón que hay que superar, no el ejemplo a copiar |
| Asterisco en la tarjeta | Push Back, `PushBackMatrixCardModel`: *«distinguir "hereda 4" de "pidio 4"»* | Marca minima y barata que **si** funciona |
| Estado «mixto» | Push Back compuesto: `field.SetNumber(null)` + `IsOptional = true` + **tabla lateral** `HashSet<NumericField>` | El campo **no puede** expresar el estado por si mismo |
| Boton explicito | Push Back «Restaurar fondo» + combo de alcance | Deshacer un override es **accion nombrada**, no un borrado implicito |

El tercero es la advertencia central. Push Back necesito una estructura paralela porque un `NumericField`
vacio ya significa dos cosas —«mixto» y «sin override»—. **Un estado «vinculado» seria un tercer
significado del mismo hueco**, y repetiria el mismo apano.

Ademas hay una colision tecnica concreta, verificada: `NumericField` **es dueno de su `BorderBrush`** y
guarda/restaura la procedencia del consumidor (valor local **o** binding) al entrar y salir del estado
de error. Un adorno de «vinculado» que pinte el borde **choca** con esa maquinaria, que asume ser la
unica escritora.

| Alt | Como se muestra el vinculo | Tradeoff |
|---|---|---|
| K1 | Caja vacia + tooltip | Repite el error del Selectivo: **invisible** |
| K2 | Pintar el borde del campo | **Colisiona** con `ApplyErrorBorder` |
| **K3** | **Adorno adyacente**: chip/etiqueta con el nombre de la variable junto al campo, y el campo en **solo lectura** mostrando el valor efectivo, mas un boton **«Desvincular»** explicito | No toca `BorderBrush`; el valor efectivo es visible; el estado es **inequivoco** (hay chip o no lo hay); y el desvinculado es accion nombrada, como «Restaurar fondo» |

**Recomendacion: K3.** Nace de las dos lecciones que el arbol ya pago: el estado tiene que **verse**
(leccion del asterisco de Push Back) y no puede vivir en la ausencia de valor (leccion del estado
mixto).

**Detalle util para el slice**: `ClearanceBox` es hoy un **`TextBox` plano**, no un `NumericField`, asi
que la colision K2 **no aplica todavia** a este campo concreto — pero aplicaria a cualquier campo que
ID22B vincule, y por eso la recomendacion no depende de ese accidente.

---

## 8. Verificacion

### D-17 — Que se prueba automaticamente y que exige AutoCAD

El reparto lo dicta un hecho duro del repositorio: **ningun proyecto de pruebas referencia
`RackCad.Plugin`** (`RackCad.Tests` → Domain + Application; `RackCad.UI.Tests` → Domain + Application +
UI). No hay forma de ejecutar un `[CommandMethod]` en CI, y no la va a haber: el Plugin referencia
AutoCAD ([ADR-0003](../adr/0003-referencias-autocad-para-ci.md)) y la CI no tiene AutoCAD.

| Pieza | Como se verifica | Suite |
|---|---|---|
| `ProjectVariablesDocument` + store (round-trip, ausencia = vacio, tipo desconocido) | Pruebas puras | **Core** |
| Resolucion del valor efectivo (D-04) y precedencia de `ClearOverride` (D-06) | Pruebas puras sobre `SelectiveGeometryResolver` | **Core** |
| Mensaje de variable inexistente (D-08) | Prueba pura del mensaje en Application, como `KindDispatchMessages` | **Core** |
| Materializar al desvincular (D-11) y al exportar (D-15) | Pruebas puras | **Core** |
| Supervivencia del vinculo al restamp de RACKDUPLICAR (D-14) | **Ver D-17-bis** — un round-trip del Store **no** sirve | **Core**, tras extraer |
| Ventana de variables y estado visual (D-16) | `StaTestRunner` + `EditorWindowTestSupport`, sin mostrar la ventana | **UI** |
| Entrada de menu y `MainMenuAction` | UI tests (precedente: `RackMainMenuStructuralSectionTests`) | **UI** |
| `[CommandMethod]` y la llamada al NOD | **Solo guarda de texto fuente** — precedente literal: `MainMenuStructuralSectionAccessGuardTests` afirma sobre el `.cs` leido como texto | Core (guarda) |
| **El NOD escribe y lee de verdad** | **Imposible automatizar** | **Dueno, en AutoCAD** |

**Consecuencia de diseno, no de pruebas**: cuanto mas codigo viva en Application, mas cae del lado
verificable. Es la misma razon por la que D-01 deja en el Plugin solo el acceso al diccionario.

**Aviso**: `SelectiveWindowTestSupport` es **el unico sitio autorizado** para construir
`RackSelectiveWindow` (lo impone `SelectiveWindowConstructionGuardTests`). Cualquier prueba de UI del
slice pasa por ahi.

### D-17-bis — Verificar C-4 de verdad: el restamp de RACKDUPLICAR

> ### ⚠ Correccion de V2 (C2-7)
>
> V1 proponia cubrir «la supervivencia del vinculo al restamp» con **un round-trip de
> `SelectivePalletDesignStore`**. Eso **no prueba nada del restamp**: solo prueba que el serializador
> serializa. La transformacion que C-4 exige —cambiar `RackId` y `Name`, **conservar** las
> referencias— vive en `SelectiveKindHandler.RestampDesign`, en el **Plugin**, que **ninguna suite
> puede cargar**. V1 confundia «el DTO declara el campo» con «la transformacion lo preserva».

**Lo que hay que verificar por COMPORTAMIENTO**, no por serializacion:

```
dado   un diseno selectivo con RackId=A, Name="Rack 1" y PropertyValues={ selective.verticalClearance → V }
cuando se aplica el restamp de una copia independiente con newId=B, copyName="Rack 1 (1)"
entonces  RackId  == B        (cambia)
          Name    == "Rack 1 (1)"  (cambia)
          PropertyValues  ==  { selective.verticalClearance → V }   (SE CONSERVA, mismo VariableId)
```

| Alt | Como se verifica | Tradeoff |
|---|---|---|
| L1 | Round-trip del Store (lo que decia V1) | **No prueba el restamp.** Descartada |
| L2 | Guarda de texto sobre `SelectiveKindHandler.cs` | Detecta que el codigo *menciona* algo; **no** que el comportamiento sea correcto. Insuficiente sola |
| **L3** | **Extraer la transformacion a Application** como funcion pura y probarla por comportamiento; el handler del Plugin queda como delegacion de una linea; **mas** una guarda de texto que impida que alguien la reimplemente en el Plugin | Es la unica que verifica **la transformacion**. Coste: un tipo nuevo en Application y tocar el handler |

**Recomendacion: L3.** Y no es un patron nuevo: el repositorio **ya movio a Application** la logica de
despacho por kind y sus mensajes —`KindDispatch` / `KindDispatchMessages` viven en
`RackCad.Application/Persistence/` y el Plugin solo los consume—. Aplicar lo mismo al restamp del
diseno es continuar una decision ya tomada, no inventar una frontera.

Forma sugerida (no se implementa):

```
RackCad.Application/Persistence/   SelectiveDesignRestamp.Restamp(json, newId, copyName) → json
                                   ← PURO. Aqui viven la transformacion y su prueba de comportamiento.

RackCad.Plugin/KindHandlers/       SelectiveKindHandler.RestampDesign  →  delega, una linea
                                   ← cubierto por guarda de texto: que delegue y no reimplemente
```

**Por que la guarda de texto sigue haciendo falta**: extraer la funcion no impide que alguien vuelva a
escribir la transformacion dentro del Plugin. La guarda —del mismo tipo que
`MainMenuStructuralSectionAccessGuardTests`— fija que el handler **delega**, que es lo unico que la
suite puede afirmar sobre codigo que no puede ejecutar.

**Alcance de la correccion**: esto **no reabre D-14**, cuya decision (el vinculo debe ser campo
declarado y el restamp no debe tocarlo) sigue intacta. Cambia **como se verifica**, no que se decide.

### D-18 — Supervivencia minima exigida

**Guardar, cerrar y reabrir el mismo DWG**, con el registro y los vinculos intactos y el dibujo
identico. Es el minimo del dueno y **no es automatizable**: exige AutoCAD, y por tanto al dueno
(WORKFLOW §6). **WBLOCK y copia entre dibujos quedan fuera**, diferidos.

---

## 9. ADR

### D-19 — Hace falta un ADR, y uno solo

Los criterios de [`docs/adr/README.md`](../adr/README.md) se cumplen sobradamente: se funda una
**autoridad de persistencia nueva** en el DWG, se fija una **identidad** nueva y se cambia el
**contrato de una propiedad** de diseno. Ademas, WORKFLOW §8 exige que el ADR se escriba **antes** de
implementar.

**Recomendacion: un unico ADR** que cubra autoridad (D-01), identidad y tipos (D-02, D-03) y forma del
vinculo con su precedencia (D-04, D-06). Partirlo en tres produciria tres documentos que solo se
entienden juntos.

Nace en estado **`propuesto`**. **Solo el dueno lo acepta**: es la regla del indice de ADR y la
practica del repositorio (I-45 cerro con ADR-0033 aun `propuesto`).

---

## 10. Evolucion a ID22B e ID21

> **Reescrita en V2.** V1 declaraba no saber que eran. El dueno los definio en el Gate C2 (C2-1, C2-2),
> asi que esta seccion pasa de «propiedades genericas que no bloquean nada» a **que extiende cada una y
> que le deja preparado ID22A**. **Ninguna de las dos se disena ni se implementa aqui.**

```
ID22B — formulas/expresiones entre variables      →  extiende  ProjectVariable.Definition
ID21  — referencias a propiedades de racks        →  extiende  PropertyValue<T>
        (RackId + PropertyId)
```

Extienden **tipos distintos** (§1-bis). Esa es la propiedad que hace que puedan avanzar en cualquier
orden, o a la vez, sin que una obligue a tocar lo de la otra.

### Lo que ID22A deja preparado para **ID22B** (formulas)

| Preparado | Por que ID22B lo necesita | Gracias a |
|---|---|---|
| `Definition` **discriminada** desde el primer dia, con un solo caso (`Literal`) | `Expression` entra como **caso adicional**, aditivo, sin subir el major | D-03 |
| `Type` explicito y separado de `Definition` | Una expresion necesita **operandos tipados** y coherencia de unidades; sin tipo no hay evaluacion correcta | D-03 |
| `VariableId` estable e independiente del nombre | Una formula referencia **variables por id**; si el id derivara del nombre, renombrar romperia formulas | D-02 |
| `PropertyValue<T>` **no** menciona como se calcula la variable | Una propiedad apunta a un `VariableId` y le da igual si es literal o expresion: **ID22B no toca las propiedades** | §1-bis |

**Lo que ID22A NO resuelve y ID22B tendra que resolver entero**: el grafo de dependencias entre
variables, la deteccion de ciclos, el orden de evaluacion y que ocurre al borrar una variable **usada
por una formula** (D-10 hoy solo cuenta consumidores **propiedad→variable**, no **variable→variable**).

### Lo que ID22A deja preparado para **ID21** (refs a propiedades de racks)

| Preparado | Por que ID21 lo necesita | Gracias a |
|---|---|---|
| Referencia persistida **tipada** con `kind` | `rackProperty` entra como **tercer `kind`**, sin cambiar el tipo del valor ni romper el esquema | D-04 |
| **`PropertyId` estable** como concepto de primera clase | Es **la mitad del par** `(RackId, PropertyId)`. Introducirlo despues obligaria a reescribir lo ya persistido | §1-bis |
| `RackId` ya existe y ya es estable | Es `RackEmbedDocument.Id`, el GUID que sobrevive a ediciones | Discovery §2.2 |
| Un `kind` desconocido es **error visible** | Un dibujo tocado por versiones distintas no degrada en silencio | D-08 |
| Descubrimiento agrupado por `RackId` | ID21 necesita saber **que racks dependen de que rack** | D-12 |

**Lo que ID22A NO resuelve y ID21 tendra que resolver entero**: la resolucion rack→rack, sus ciclos, el
orden de redibujo cuando un rack depende de otro que a su vez cambia, y **que pasa al exportar a la
biblioteca un rack con referencias a otro rack** — D-15/H1 hoy materializa referencias a **variables**,
y una referencia a **otro rack** plantea el mismo problema con una respuesta que puede no ser la misma.

### Un limite comun que conviene ver ahora

Hoy la propagacion (D-13) es de **profundidad 1**: variable → consumidores. **ID22B anade
variable→variable** y **ID21 anade rack→rack**, y ambas convierten el preflight en un recorrido de
**grafo**, no de lista. El contrato de D-13 **no debe asumir profundidad 1** en su forma persistida ni
en su modelo de errores. No se disena aqui; se senala para que la implementacion de ID22A no cierre esa
puerta por descuido.

**Lo que ID22A deja deliberadamente sin resolver, y no es deuda sino alcance**: unificar `ClearHeight`
(C-5), WBLOCK y copia entre dibujos (diferido), formulas (ID22B), referencias rack→rack (ID21), y la
**transferencia/fusion de conjuntos de variables entre dibujos** — que, corregido en V2, **no pertenece
a ID22B ni a ninguna iniciativa definida** (D-15).

---

## 11. Alternativas descartadas — resumen

| Descartada | Por que |
|---|---|
| A2 — Model Space BTR | Unicidad por convencion, no estructural; y seria elegir por coste (§1) |
| A3 — Bloque dedicado | `ScanEnvelopes` lo leeria como rack; el usuario puede purgarlo o explotarlo |
| A4 — `SummaryInfo` | Editable por el usuario desde AutoCAD; sin estructura |
| A5 — Sidecar | No viaja dentro del DWG |
| B2 — Id derivado del nombre | Renombrar rompe referencias: contradice el requisito |
| B3 — Entero incremental | Colisiona al juntar conjuntos; no gana legibilidad |
| C3 / C5 — Sin tipo / sin `Definition` discriminada | Cada una bloquea una continuacion ya definida: variables no numericas, y las formulas de ID22B |
| D2 — Sustituir el escalar por objeto | Exigiria migracion: **prohibida por C-1** |
| **D3 — `Bindings: slot → string`** (recomendada en V1) | **Retirada en V2**: el valor es una cadena desnuda, asi que el par `(RackId, PropertyId)` de ID21 no cabe sin cambiar el tipo del valor |
| F1 / E3 — Referencia en el diseno con major sin cambiar | Perdida **silenciosa** en el round-trip de una version pre-I-47, que `[JsonExtensionData]` **no** puede evitar |
| **F3 — Referencia en el sobre** (viva en V2) | **Rechazada en V3**: una version vieja la **preserva** pero **no la entiende**; editaria y redibujaria con el literal, dejando geometria **incoherente con la autoridad**. Preservar bytes no es compatibilidad |
| **Major «segun contenido actual»** (mecanica de F2 en V2) | Hace **oscilar** el schema al vincular y desvincular: que versiones abren el rack dependeria de estado transitorio. Sustituida por **promocion pegajosa** |
| `EnsureForPlan` dentro del preflight/lote (V2) | **Muta el `Database` fuera de la transaccion del llamador** y con biblioteca ausente devuelve 0 en silencio. Sustituido por **verificar y abortar** |
| L1 — Round-trip del Store como prueba del restamp | **No prueba el restamp**: solo prueba el serializador |
| «Variable pisa `ClearOverride`» | Invierte una precedencia probada; ninguna premisa lo pide |
| A (D-08) — Fallback al literal | Cambia la geometria en silencio |
| F2 / F3 **de D-10** (etiquetas locales, no las de D-07) | Traslada el fallo al futuro / reescribe N racks como efecto colateral de un borrado |
| G2 / G3 (D-11) | Salto geometrico visible al desvincular |
| Propagacion perezosa | Contradice C-3 y permite estados intermedios inconsistentes |
| H2 (D-15) | Importaciones que fallan en casi cualquier dibujo destino |

---

## 12. Preguntas realmente abiertas

### 12.1 Decisiones de producto y arquitectura: **NO queda ninguna abierta**

Se confirma explicitamente, que era lo que el Gate C3 pedia comprobar. El recorrido completo:

| Antes abierta | Estado | Donde |
|---|---|---|
| D-07: F2 frente a F3 | **CERRADA** — F2, con promocion pegajosa; F3 rechazada por semantica | C3-1, C3-2 |
| D-10: bloquear el borrado | **CERRADA** — F1; el atajo solo como accion explicita separada | C3-3 |
| Viabilidad conceptual de la transaccion unica | **RETIRADA** — deja de ser pregunta de contrato; pasa a verificacion | C3-6 |
| Numero de iniciativa para H3 | **RETIRADA** — H3 es futuro separado y **no necesita numero** para que ID22A cierre | C3-8 |
| Coste de la propagacion | **RECLASIFICADO** — riesgo y **metrica de implementacion**, no decision de arquitectura | C3-8 |
| D-04 (forma de la referencia) | Cerrada en el Gate C2 | C2-4 |

**Todas las decisiones D-01..D-19 estan tomadas.** Lo que queda no es diseno: es **aceptacion formal**
del contrato por el dueno (gate `owner-decision`), **verificacion en AutoCAD** y **ejecucion**.

### 12.2 Lo unico que sigue abierto, y no es una decision de producto

**La aceptacion formal del contrato por el dueno.** El gate `owner-decision` sigue abierto sobre
D-01..D-19 **en bloque**: no porque falte elegir entre alternativas —ya no hay ninguna viva— sino
porque una Proposal **recomienda** y solo el dueno **acepta**. Formalmente, eso ocurre con el ADR de
D-19, que nace `propuesto` y que **solo el dueno** pasa a `aceptado`.

Si al leer el contrato el dueno quisiera cambiar algo, eso abriria una decision nueva; hoy no hay
ninguna pendiente por parte de esta Proposal.

## 13. Riesgos de esta propuesta

Distintos de los del Discovery: aquellos describen el arbol, estos son de **lo que se propone**.

| # | Riesgo | Mitigacion incorporada |
|---|---|---|
| P1 | **El acceso al NOD no lo cubre ninguna suite** y no lo cubrira nunca | D-01 deja en el Plugin solo el acceso al diccionario; DTO, store, resolucion, mensajes y materializacion viven en Application y **si** se prueban |
| P2 | **`PURGE` sobre una entrada del NOD**: comportamiento no verificado | Se declara como pendiente de comprobar en AutoCAD (D-01). **No se afirma en ningun sentido** |
| P3 | **Desvinculado silencioso al guardar un rack** desde RACKEDITAR | D-14 lo nombra como el riesgo principal del editor y exige que desvincular sea accion explicita |
| P4 | **Perdida silenciosa del vinculo** por round-trip de version anterior, o por el restamp de RACKDUPLICAR | D-07 **F2** (major condicional: la version anterior se **niega** en vez de desvincular; `[JsonExtensionData]` **no** cubre esto) y D-17-bis (transformacion extraida y probada por comportamiento) |
| P5 | **Un tercer significado del campo vacio** en la UI | D-16b elige adorno adyacente (K3), no ausencia de valor |
| P6 | **Coste de propagacion desconocido** en un dibujo grande | **Riesgo y metrica de implementacion** (C3-8), no decision de arquitectura. Se **mide** cuando exista algo que medir; no se estima aqui ni condiciona el contrato. La transaccion unica **agrava** el perfil, porque mantiene mas estado abierto mas tiempo |
| **P9** | La transaccion unica de D-13 **hay que verificarla ejecutando** | **Ya no es una incognita de diseno** (C3-6): la unica operacion que se sabia problematica queda fuera del lote, y `RedefineSystemBlock` ya trabaja sobre una transaccion ajena. Se verifica como **gate de implementacion / Owner Validation** (CF-5) |
| **P10** | **Profundidad 1 asumida por descuido**: ID22B (variable→variable) e ID21 (rack→rack) convierten el preflight en recorrido de **grafo** | §10 lo senala como limite comun; la forma persistida y el modelo de errores de D-13 no deben cerrar esa puerta |
| P7 | **Archivos calientes**: el slice toca `SelectivePalletDesign.cs` y `RackSelectiveWindow.xaml.cs`, ambos en la tabla de WORKFLOW seccion 7 | La implementacion debe **serializarse** con cualquier otra iniciativa del Selectivo |
| P8 | **Sobre-ingenieria**: D-03 (tipos) y D-04 (mapa de vinculos) construyen para ID22A mas de lo que ID22A necesita | Es una apuesta **consciente** por «evolucion limpia a ID22B», que el dueno pidio. Si prefiere el minimo, D1 y C3 son las alternativas, y estan escritas con su coste |

---

## 14. Condiciones de Consensus Freeze (corregidas en V3)

El Consensus Freeze **no** se da por alcanzado mientras quede pendiente cualquiera de estas. Son
condiciones de **proceso**, no de diseno.

> **Correccion de V3 (C3-7), conforme a [WORKFLOW](../WORKFLOW.md) seccion 2.** V2 metia la
> actualizacion de `ROADMAP.md` en el Freeze, y eso **contradice el proceso**: ROADMAP se edita en
> **tres momentos** y ninguno es un gate intermedio. **Sale del Freeze** y pasa al checklist de cierre
> (§14-bis). El **contrato de iniciativa** si permanece: ese no tiene restriccion de momento y debe
> reflejar el alcance final **antes de produccion**.

| # | Condicion |
|---|---|
| **CF-1** | **El contrato de iniciativa refleja el alcance final antes de produccion.** Hoy [`I-47-project-variables-foundation.md`](I-47-project-variables-foundation.md) §3 excluye UI y comando, redactado para DISCOVERY; D-16 los incluye. Si el dueno acepta D-16, el contrato debe decirlo **antes** de que se escriba la primera linea de produccion |
| **CF-2** | **El ADR de D-19 esta escrito antes de implementar** (WORKFLOW seccion 8), en estado `propuesto`. **Solo el dueno lo acepta** — y esa aceptacion es la forma concreta de cerrar el gate `owner-decision` (§12.2) |
| **CF-3** | **Los prerequisitos declarados estan reconocidos como trabajo**, no dados por hecho: el helper de **promocion pegajosa** con sus dos constantes de linea y su version de lectura `2.x` (D-07), la **extraccion del restamp** a Application (D-17-bis), y el **writer de lote con transaccion del llamador** mas la **verificacion de definiciones de bloque** (D-13) |

**Eso es todo.** Tres condiciones, las tres documentales o de reconocimiento de alcance. Ninguna es un
cambio de produccion y ninguna depende de AutoCAD.

### Lo que V2 pedia aqui y en V3 ya NO es condicion de Freeze

| Antes | Ahora |
|---|---|
| CF-2 de V2: actualizar la fila de ROADMAP | **Checklist de cierre** (§14-bis). WORKFLOW seccion 2 lo prohibe en un gate intermedio |
| CF-4 de V2: resolver D-07 y D-10 | **Ya resueltos** por el Gate C3 |
| CF-5 de V2: contestar la viabilidad de la transaccion unica **antes** del Freeze | **Gate de implementacion / Owner Validation** (C3-6). Ver CF-5' abajo |

### CF-5' — donde vive ahora la validacion en AutoCAD

**No es condicion de Consensus Freeze. Es gate de implementacion.** Se verifica sobre el candidato ya
construido, conforme a WORKFLOW seccion 6, y cubre como minimo:

- La transaccion unica hace lo que el contrato dice: **commit unico**, y ante un fallo inducido
  **ningun** consumidor queda modificado.
- El registro sobrevive a **guardar, cerrar y reabrir el mismo DWG** (D-18).
- La propagacion **aborta** —sin tocar nada— cuando falta una definicion de bloque (D-13).
- `PURGE` no destruye la entrada del NOD (D-01).

## 14-bis. Checklist de cierre (no es Freeze)

Se ejecuta en la **sesion de integracion**, como ultimo commit de la rama, segun WORKFLOW seccion 4.5.4
y el momento 3 de su seccion 2:

- **`docs/ROADMAP.md`**: actualizar la fila de I-47 —hoy lista «UI, comando nuevo» entre lo expresamente
  fuera de alcance, redactado para DISCOVERY— y marcar el estado de cierre. **Este es el unico momento
  legitimo para tocarla.**
- `docs/HANDOFF.md` §8-12, conforme al proceso.
