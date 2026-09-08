# I-47 — Proposal V1: contrato de variables de proyecto (ID22A)

> **Documental. No autoriza implementacion.** No toca `src/` ni `tests/`. Compara alternativas y
> **recomienda** un contrato; **elegir sigue siendo del dueno** y el gate `owner-decision` sigue
> abierto. **No se pidio Architect Review** en este gate.
>
> ```
> Base del analisis:  9e25d5291daa13c4846112241be6e52526429a47  (Discovery)
> origin/main:        306e18ed4676e5e96b54d59402c9a230efb137d3  (sin avanzar)
> Premisas:           C-1..C-6 del Gate C (docs/automation/decisions/I-47.md)
> ```

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

## 2. Fundacion

### D-01 — Autoridad unica `ProjectVariables` por DWG

**Requisito**: exactamente **un** registro por dibujo, estructurado, que sobreviva a guardar/reabrir y
que no dependa de que el usuario no toque nada.

| Alt | Mecanismo | A favor | En contra |
|---|---|---|---|
| **A1** | **NOD** (`Database.NamedObjectsDictionary`) → sub-`DBDictionary` `RACKCAD_PROJECT` → `Xrecord` con JSON troceado | Unicidad **estructural**: hay exactamente un NOD por `Database`. Es el sitio que AutoCAD define para datos de aplicacion a nivel dibujo. **No entra en la `BlockTable`**, asi que `RackBlockFinder.ScanEnvelopes` no lo ve y no necesita exclusion nueva. El troceado a <=255 ya esta probado en `RackBlockData` | API nueva en el Plugin (crear el sub-diccionario y `AddNewlyCreatedDBObject`). No lo cubre ninguna suite (R2) |
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

### D-03 — Modelo de tipos extensible; ID22A solo `Length` en pulgadas

| Alt | Modelo | Tradeoff |
|---|---|---|
| **C1** | Discriminador `Type` (string) + valor tipado | Additivo y explicito; un tipo desconocido es **detectable** (y por D-08 se reporta, no se ignora) |
| C2 | `object` + etiqueta de tipo | Flexible y opaco; obliga a `switch` de casteo en cada consumidor |
| C3 | **Sin tipo**: todo `double` | Lo mas simple **hoy** y lo que bloquea manana: §4.7 del Discovery ya identifica `DimensionStyle` —**no numerico**— como candidato. Un modelo sin tipo obliga a romper el esquema para admitirlo |
| C4 | Una coleccion por tipo | Sin casteos; multiplica el formato persistido por cada tipo nuevo |

**Recomendacion: C1.** ID22A acepta **exactamente un** tipo: `Length`, en **pulgadas**, coherente con
[ADR-0005](../adr/0005-estrategia-de-unidades.md) (la pulgada es la unidad geometrica interna y **no**
se introduce conversion). Persistir el discriminador **desde el primer dia** es lo que permite que
ID22B agregue tipos **sin cambiar el `SchemaVersion` mayor**.

C3 se descarta aunque hoy bastaria: es la decision que mas caro sale corregir.

### D-04 — `Literal` vs `ProjectVariableReference`: la forma de la propiedad

Premisa C-2: si hay referencia, **la referencia gobierna**. La pregunta abierta es **como se persiste
el vinculo**.

| Alt | Forma | A favor | En contra |
|---|---|---|---|
| **D1** | Campo hermano por propiedad: `VerticalClearance` (double, existente) **+** `VerticalClearanceVariableId` (string, nuevo nullable) | Minimo, explicito, **cero cadenas magicas**, trivial de probar | **Un campo nuevo por cada propiedad vinculable**: ID22B con N propiedades = N campos |
| D2 | Sustituir el escalar por un objeto `{ mode, literal, variableId }` | Expresivo | **Rompe la compatibilidad del campo escalar existente** y exigiria migracion ⇒ **prohibida por C-1**. Descartada por premisa |
| **D3** | Mapa de vinculos: `Bindings: { "<slot>": "<variableId>" }`, con los **slots como constantes** declaradas en un solo sitio | **Un solo campo** que escala a ID22B sin tocar el esquema; el literal permanece intacto y no se toca | El slot es una cadena; un renombrado descuidado la rompe |

**Recomendacion: D3, con la salvedad de sus claves.** El riesgo de «cadena magica» **ya esta resuelto
en este repositorio** y no hay que inventar nada: `CatalogBlockParameters` fija los nombres de
parametro de bloque en constantes (`SelectiveRackDefaults.LengthParam`, `PeralteParam`,
`PalletAltoParam`) precisamente para que un renombrado no pueda romperlos en silencio. La misma
disciplina aplicada a los slots (`ProjectVariableSlots.SelectiveVerticalClearance =
"selective.verticalClearance"`) neutraliza el unico argumento en contra de D3.

**D1 es una recomendacion defendible si el dueno prefiere el minimo absoluto** para una sola
propiedad. El tradeoff real y unico entre D1 y D3 es **cuando** se paga el coste de escalar: D1 lo
aplaza a ID22B y lo cobra con intereses (N campos y N tests de round-trip); D3 lo paga hoy, una vez.
Dado que el dueno pide explicitamente **evolucion limpia a ID22B** (§11), D3 es la coherente.

**Semantica, en cualquiera de las dos formas:**

```
valor efectivo(propiedad) =
    binding presente  →  valor de la variable referida     (C-2: la referencia gobierna)
    binding ausente   →  el literal persistido             (C-1: literal preservado)
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

| Alt | Donde vive el vinculo | Tradeoff |
|---|---|---|
| **E1** | En `SelectivePalletDesignDocument`, **anadiendole `[JsonExtensionData]`** como prerequisito | El vinculo vive junto al diseno, que es su sitio natural. El prerequisito es **aditivo** y alinea al DTO con **cinco DTO hermanos** que ya lo tienen. Es un cambio de produccion, y por tanto **no** de esta fase |
| E2 | En el **sobre** (`RackEmbedDocument`), que ya preserva desconocidos | Cero prerequisitos. Pero el sobre es **agnostico del kind** y se repite por **cada vista** del rack: el vinculo quedaria duplicado N veces con riesgo de divergencia entre vistas |
| E3 | En el diseno **sin** arreglar el DTO | Descartada: acepta a sabiendas la perdida silenciosa |

**Recomendacion: E1**, declarando el `[JsonExtensionData]` de `SelectivePalletDesignDocument` como
**prerequisito explicito de la implementacion**, no como mejora opcional. E2 es el plan B si el dueno
prefiere no tocar el DTO selectivo.

**`SchemaVersion` no sube de mayor.** Un campo nuevo nullable es aditivo; subir el mayor haria que
[`SchemaVersionPolicy.IsReadable`](../../src/RackCad.Application/Persistence/SchemaVersionPolicy.cs)
declarara ilegibles esos documentos para versiones anteriores, que es justo lo contrario de lo que
pide C-1.

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

| Alt | Comportamiento | Tradeoff |
|---|---|---|
| **F1** | **Bloquear** mientras existan consumidores, **listandolos** | Ningun estado roto se crea nunca. Exige el barrido de consumidores — que **ya existe** (D-12). El usuario debe desvincular antes: mas pasos, pero ninguno sorpresa |
| F2 | Permitir y dejar referencias rotas | «Barato» al borrar y caro despues: traslada el fallo a un momento futuro y a otra persona |
| F3 | Borrar y **desvincular automaticamente** todos los consumidores | Comodo, pero **reescribe en silencio el diseno persistido de N racks** como efecto colateral de un borrado. Destructivo y sorprendente |

**Recomendacion: F1**, con dos matices que importan:

1. **F3 se ofrece como accion explicita y separada** —«desvincular todos los consumidores» y despues
   borrar—, nunca como efecto automatico del borrado. Asi el usuario tiene el camino comodo **y** ve
   lo que va a pasar.
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

### D-13 — Preflight, todo-o-nada, lote y **un solo** `Regen`

Las tres piezas existen; lo que no existe es la costura sin dialogo (Discovery §3.5, H-03).

| Pieza | Ya existe | Ubicacion |
|---|---|---|
| Preflight de lote que aborta | **si** | `KindHandlerDispatch.TryResolveAll` (`KindHandlerDispatch.cs:49`) |
| Aborto sin modificar nada | **si** | `RackCommandSupport.PreflightInnerSources` |
| Redibujo en sitio | **si** | `SystemBlockWriter.RedrawInPlace(..., regen: false)` |
| **Un solo** `Regen` | **si** | `SystemBlockWriter.ApplyRegen` es el unico punto que lo dispara |
| **Aplicar diseno y redibujar SIN ventana** | **NO** | — es lo que hay que extraer |

**Orden recomendado**, que es el mismo de los cinco RACKEDITAR pero sobre N racks:

```
1. escanear y agrupar por RackId
2. seleccionar los consumidores de la variable cambiada
3. PREFLIGHT de todos: handler resoluble + diseno legible + variables resueltas   → si algo falla, ABORTA
4. por cada rack, por cada vista: RedrawInPlace(..., regen: false)
5. UN solo document.Editor.Regen()
```

**Alternativa descartada: propagacion perezosa** (marcar los racks y redibujar cuando se abran).
Evitaria el barrido, pero contradice C-3 —«en una operacion»— y dejaria el dibujo en un estado
intermedio donde dos racks vinculados a la misma variable muestran valores distintos.

**Riesgo abierto y honesto**: no hay medicion de cuanto tarda esto en un dibujo grande. El repositorio
sabe que fijar parametros dinamicos por referencia es lento y por eso existe el patron ARRAY; una
propagacion sobre muchos racks es **trabajo nuevo de la misma familia**. No se estima aqui.

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

**Consecuencia que refuerza D-04 y D-07:** el vinculo tiene que ser un **campo declarado** del DTO que
lo transporte. Confiar en `[JsonExtensionData]` no basta aqui —y de hecho el DTO selectivo ni siquiera
lo tiene—: un vinculo no declarado sobreviviria a guardar y reabrir, pero **moriria en la primera
duplicacion**. Merece prueba propia, precisamente porque es una omision silenciosa.

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

**H3 es la respuesta correcta a largo plazo** y se nombra explicitamente como territorio de ID22B: la
fusion de conjuntos de variables entre dibujos es una iniciativa propia, no un detalle de exportacion.

**Aviso de implementacion**: el guardado del selectivo escribe
`RackProject.ForSelectiveRack(document)` con **el mismo `SelectivePalletDesignDocument`** que se
embebe en el DWG. Materializar es, por tanto, un paso **explicito antes de componer el proyecto de
biblioteca** — no ocurre solo.

---

## 7. Interfaz de usuario

> **Nota de alcance.** La fila de I-47 en ROADMAP lista «UI» y «comando nuevo» como fuera de alcance.
> Esa fila describe lo que **no se construye**, y sigue siendo cierta: esta seccion **disena en papel**
> y no toca `src/`. Se senala en §12 por si el dueno quiere que la fila lo diga con mas precision.

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
| Supervivencia del vinculo al restamp de RACKDUPLICAR (D-14) | Prueba pura sobre `SelectivePalletDesignStore` round-trip | **Core** |
| Ventana de variables y estado visual (D-16) | `StaTestRunner` + `EditorWindowTestSupport`, sin mostrar la ventana | **UI** |
| Entrada de menu y `MainMenuAction` | UI tests (precedente: `RackMainMenuStructuralSectionTests`) | **UI** |
| `[CommandMethod]` y la llamada al NOD | **Solo guarda de texto fuente** — precedente literal: `MainMenuStructuralSectionAccessGuardTests` afirma sobre el `.cs` leido como texto | Core (guarda) |
| **El NOD escribe y lee de verdad** | **Imposible automatizar** | **Dueno, en AutoCAD** |

**Consecuencia de diseno, no de pruebas**: cuanto mas codigo viva en Application, mas cae del lado
verificable. Es la misma razon por la que D-01 deja en el Plugin solo el acceso al diccionario.

**Aviso**: `SelectiveWindowTestSupport` es **el unico sitio autorizado** para construir
`RackSelectiveWindow` (lo impone `SelectiveWindowConstructionGuardTests`). Cualquier prueba de UI del
slice pasa por ahi.

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

**Advertencia de honestidad: no se que son ID22B ni ID21.** `git grep` sobre todo el repositorio no
devuelve **ninguna** aparicion de esos identificadores; viven en la numeracion del dueno, igual que
ID22A antes de esta iniciativa. Por tanto **no se disena para ellos**: se enuncian las propiedades que
el contrato conserva para no bloquear una continuacion razonable, sea cual sea.

| Propiedad que el contrato preserva | Que desbloquea | Gracias a |
|---|---|---|
| Tipos extensibles con discriminador persistido | Variables no numericas (p. ej. `DimensionStyle`, §4.7) | D-03 |
| Un solo campo de vinculos con slots constantes | Vincular N propiedades **sin** cambiar el esquema | D-04 (D3) |
| Id independiente del nombre | Renombrar libremente para siempre | D-02 |
| Preflight todo-o-nada ya generalizado a N racks | Propagar variables que afecten a varios sistemas | D-13 |
| `defaults.json` intacto y conviviendo | Decidir mas tarde, sin prisa, si hay capas instalacion → dibujo → rack | C-6 |
| Materializacion como operacion nombrada | Exportar, desvincular y borrar comparten un solo mecanismo | D-11, D-15 |

**Lo que ID22A deja deliberadamente sin resolver, y no es deuda sino alcance**: unificar `ClearHeight`
(C-5), fusionar conjuntos de variables entre dibujos (D-15/H3), WBLOCK, y formulas o dependencias
entre variables —excluidas desde el encargo original—.

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
| C3 — Sin modelo de tipos | Bloquea variables no numericas ya identificadas |
| D2 — Sustituir el escalar por objeto | Exigiria migracion: **prohibida por C-1** |
| E3 — Vinculo en el DTO selectivo sin `[JsonExtensionData]` | Perdida silenciosa en round-trip de version anterior |
| «Variable pisa `ClearOverride`» | Invierte una precedencia probada; ninguna premisa lo pide |
| A (D-08) — Fallback al literal | Cambia la geometria en silencio |
| F2 / F3 (D-10) | Traslada el fallo al futuro / reescribe N racks como efecto colateral |
| G2 / G3 (D-11) | Salto geometrico visible al desvincular |
| Propagacion perezosa | Contradice C-3 y permite estados intermedios inconsistentes |
| H2 (D-15) | Importaciones que fallan en casi cualquier dibujo destino |

---

## 12. Preguntas realmente abiertas

Solo lo que **no** esta decidido. Ni las premisas C-1..C-6, ni lo que esta Proposal recomienda —una
recomendacion no es una decision—.

1. **¿Se acepta el contrato recomendado?** El gate `owner-decision` sigue abierto sobre D-01..D-19 en
   bloque. Dentro de el, las tres elecciones con alternativa realmente defendible son: **D-04**
   (mapa de vinculos `D3` frente a campo hermano `D1`), **D-07** (`E1` anadir `[JsonExtensionData]` al
   DTO selectivo frente a `E2` llevar el vinculo al sobre) y **D-10** (`F1` bloquear el borrado). Las
   demas tienen, a mi juicio, una sola respuesta razonable.
2. **¿Que son ID22B e ID21?** `git grep` no devuelve **ninguna** aparicion en el repositorio: viven en
   la numeracion del dueno. §10 enuncia que propiedades conserva el contrato para no bloquearlas, pero
   **no puede** confirmar que sean las que hacen falta sin saber que son.
3. **Coste de la propagacion en un dibujo grande.** No hay medicion. El repositorio ya sabe que fijar
   parametros dinamicos por referencia es lento —de ahi el patron ARRAY—, y propagar sobre N racks es
   trabajo de la misma familia. Podria exigir su propia estrategia, y eso solo se sabe midiendo.
4. **Nota de proceso, no de producto.** La fila de I-47 en ROADMAP lista «UI» y «comando nuevo» como
   fuera de alcance, redactada para la fase DISCOVERY. La Proposal **disena** UI sin construirla, asi
   que no la contradice; pero si el dueno quiere que la fila lo diga explicitamente, el momento
   legitimo de editarla es el cierre (WORKFLOW seccion 2, momento 3), no este gate.

## 13. Riesgos de esta propuesta

Distintos de los del Discovery: aquellos describen el arbol, estos son de **lo que se propone**.

| # | Riesgo | Mitigacion incorporada |
|---|---|---|
| P1 | **El acceso al NOD no lo cubre ninguna suite** y no lo cubrira nunca | D-01 deja en el Plugin solo el acceso al diccionario; DTO, store, resolucion, mensajes y materializacion viven en Application y **si** se prueban |
| P2 | **`PURGE` sobre una entrada del NOD**: comportamiento no verificado | Se declara como pendiente de comprobar en AutoCAD (D-01). **No se afirma en ningun sentido** |
| P3 | **Desvinculado silencioso al guardar un rack** desde RACKEDITAR | D-14 lo nombra como el riesgo principal del editor y exige que desvincular sea accion explicita |
| P4 | **Perdida silenciosa del vinculo** por round-trip de version anterior, o por el restamp de RACKDUPLICAR | D-07 (`[JsonExtensionData]` como prerequisito) y D-14 (campo **declarado** en el DTO, con prueba propia) |
| P5 | **Un tercer significado del campo vacio** en la UI | D-16b elige adorno adyacente (K3), no ausencia de valor |
| P6 | **Coste de propagacion desconocido** | Declarado en §12.3. No se estima ni se promete |
| P7 | **Archivos calientes**: el slice toca `SelectivePalletDesign.cs` y `RackSelectiveWindow.xaml.cs`, ambos en la tabla de WORKFLOW seccion 7 | La implementacion debe **serializarse** con cualquier otra iniciativa del Selectivo |
| P8 | **Sobre-ingenieria**: D-03 (tipos) y D-04 (mapa de vinculos) construyen para ID22A mas de lo que ID22A necesita | Es una apuesta **consciente** por «evolucion limpia a ID22B», que el dueno pidio. Si prefiere el minimo, D1 y C3 son las alternativas, y estan escritas con su coste |
