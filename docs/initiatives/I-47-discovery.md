# I-47 — Informe de Discovery: variables de proyecto y autoridad drawing-level

> **Fase DISCOVERY. Sin cambios de produccion.** Este documento **describe** el arbol; no propone
> implementacion, no elige mecanismo y no disena formulas ni propagacion **rack a rack**. Esas cuatro
> cosas quedan fuera por el encargo del dueno, registrado en
> [`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md).
>
> **Actualizado en el Gate C (2026-09-08):** la §6 se reescribio porque seis de sus siete preguntas
> **ya no estan abiertas** — el dueno las decidio. Las secciones 2 a 5 **no se tocaron**: siguen
> describiendo el arbol en el mismo SHA. Aviso sobre la exclusion de arriba: la decision **C-3** manda
> propagar de la variable a **sus consumidores**, que es propagacion **proyecto → rack**; la que sigue
> excluida es la de **rack a rack** (un rack heredando de otro), que es cosa distinta.
>
> ```
> Rama:     architecture/project-variables-foundation
> Reclamo:  6e17bd5   Claim-Id 73672933-f052-46af-b3c0-8091f0add299
> Bootstrap: 51a75b3
> Base:     origin/main 306e18ed4676e5e96b54d59402c9a230efb137d3
> ```

## 1. Metodo y limite de la evidencia

Todo lo afirmado aqui se comprobo **leyendo el arbol en la punta de esta rama**, que es identica a
`origin/main` `306e18e` salvo por los documentos de la propia iniciativa. Las afirmaciones negativas
—«no existe X»— se sostienen con un `git grep` sobre **todo** el repositorio, no solo sobre `src/`.

**No se ejecuto AutoCAD.** Por tanto este informe **no** afirma nada sobre comportamiento en runtime
que no este escrito en el codigo: la supervivencia de un registro a `WBLOCK`, a copiar y pegar entre
dibujos o a `PURGE` es una **pregunta abierta**, no un hallazgo. Se marca como tal en la seccion 6.

---

## 2. D1 — Autoridad drawing-level disponible

### 2.1 El hallazgo principal: hoy no existe

**RackCad no tiene ninguna autoridad de nivel dibujo.** Un `ProjectVariables` unico por DWG seria un
mecanismo **nuevo**, no la reutilizacion de uno existente. Las cuatro comprobaciones, sobre el
repositorio completo:

| Mecanismo candidato | Ocurrencias |
|---|---|
| `NamedObjectsDictionary` / `NamedObjectDictionary` | **0** |
| `SummaryInfo`, `.Ldata`, `RegAppTable`, `RegisterApp`, `XData` en `src/` | **0** |
| `HostApplicationServices.WorkingDatabase` | **0** |
| Diccionario propio creado a nivel `Database` | **0** |

La unica mencion de XData o extension dictionaries como estrategia vive en un documento **archivado**
de la epoca del MVP (`docs/archivo/mvp-inicial/arquitectura-autocad-racks.md`); ningun codigo la
implementa.

### 2.2 Lo unico que hoy vive dentro del DWG, y de quien cuelga

Hay **un solo** almacen embebido, y su dueno **no es el dibujo**:

`src/RackCad.Plugin/Systems/Shared/RackBlockData.cs` — `internal static class RackBlockData`

```csharp
public const string DictKey = "RACKCAD_SELECTIVE";   // linea 16
...
var entity = (DBObject)transaction.GetObject(entityId, OpenMode.ForWrite);
if (entity.ExtensionDictionary.IsNull) { entity.CreateExtensionDictionary(); }
var dictionary = (DBDictionary)transaction.GetObject(entity.ExtensionDictionary, OpenMode.ForWrite);
```

El payload es el JSON de `RackEmbedDocument` troceado en cadenas de <= 255 caracteres
(`ChunkSize = 255`) dentro de un `Xrecord`.

**El `entityId` que recibe es siempre la DEFINICION del bloque (`BlockTableRecord`), nunca la
referencia.** Se verificaron los **doce** puntos de llamada de `Write`/`Read` en `src/`: todos pasan
un `DefinitionId`, un `blockId` de definicion o `reference.BlockTableRecord`. La intencion esta
escrita en `src/RackCad.Plugin/Drawing/LateralHeaderDrawService.cs:206`:

```csharp
// Payload on the DEFINITION so every reference/copy shares it and the cabecera can be reopened.
```

Y en `src/RackCad.Plugin/RackCommandSupport.cs:91-93`, incluso cuando el usuario **pica una
referencia**, la lectura salta a la definicion:

```csharp
var reference = (BlockReference)transaction.GetObject(selection.ObjectId, OpenMode.ForRead);
var definitionId = reference.BlockTableRecord;
return (BlockId: definitionId, Json: RackBlockData.Read(transaction, definitionId));
```

> **Hallazgo documental (H-01).** El comentario XML de la propia clase `RackBlockData` dice que
> escribe en *«a block **reference's** extension dictionary»*. Es **falso** para los doce llamadores
> reales. Importa para I-47 porque la pregunta «un unico registro por DWG» se decide exactamente
> sobre quien posee el diccionario, y la unica prosa que hay sobre ese punto miente. Se registra
> como hallazgo, **no se corrige aqui**.

### 2.3 Consecuencias para «un unico registro por DWG»

1. **La clave `"RACKCAD_SELECTIVE"` es la unica reservada por RackCad dentro de un DWG**, y esta
   acotada al diccionario de extension de una definicion. Una clave de nivel dibujo no puede
   colisionar con ella.
2. **La granularidad actual es «uno por definicion de bloque», y eso es lo contrario de «uno por
   dibujo»**: un DWG con N racks tiene N (o mas, una por vista) copias del almacen.
3. `RackBlockData.Write` **no puede borrar** un valor: retorna en silencio con payload nulo o vacio
   (`RackBlockData.cs:22-25`), asi que no sirve como precedente para «restablecer al valor por
   defecto» si eso llegara a hacer falta.
4. **El almacen del DWG no tiene ninguna prueba automatizada.** Los unicos archivos de `tests/` que
   nombran `RackBlockData` son **guardas que leen el texto fuente** —el patron ya conocido del
   repositorio—, no pruebas de comportamiento. Es una consecuencia estructural de
   [ADR-0006](../adr/0006-autocad-solo-en-plugin.md): lo que toca AutoCAD vive en el Plugin, y el
   Plugin no lo cubre ninguna suite.

### 2.4 Lo unico «de nivel dibujo» que el codigo ya lee

Existen dos lecturas de estado del dibujo, ambas **de solo lectura** y ninguna es un almacen:

- `src/RackCad.Plugin/RackUnitsGuard.cs` — `WarnIfNotInches(Document)` lee `document.Database.Insunits`.
  Su comentario declara ser *el unico sitio de RackCad que lee INSUNITS*. Es un aviso; nunca convierte
  ([ADR-0005](../adr/0005-estrategia-de-unidades.md)).
- `src/RackCad.Plugin/RackCommandSupport.cs:138` — `ReadDimensionStyleNames(Document)` enumera
  `document.Database.DimStyleTableId`.

La segunda es relevante: **el estilo de cota se elige de una tabla de nivel dibujo pero se guarda por
rack** (`SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs`). Es, hoy, el caso mas
claro de un valor de proyecto almacenado en el sitio equivocado — y **no** es uno de los tres del
vertical slice.

### 2.5 Donde se toca AutoCAD

`Application.DocumentManager.MdiActiveDocument` aparece en **24 puntos**, todos dentro de
`src/RackCad.Plugin`, conforme a [ADR-0006](../adr/0006-autocad-solo-en-plugin.md). La envoltura
transaccional comun es `src/RackCad.Plugin/InDocumentTransaction.cs` (`InDocumentTransaction.Run`).
Cualquier lectura/escritura de nivel dibujo tendria que entrar por ahi, y su DTO tendria que vivir en
`RackCad.Application/Persistence` junto a `RackEmbedDocument` para ser testeable.

---

## 3. D2 — Descubrimiento y redibujo de consumidores

### 3.1 El descubrimiento ya es de nivel dibujo, y es uno solo

`src/RackCad.Plugin/RackBlockFinder.cs` — `RackBlockFinder.ScanEnvelopes(Transaction, Database, bool)`
(linea 57) es **el** barrido del dibujo. Todo camino de descubrimiento desemboca ahi.

```csharp
var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
foreach (ObjectId id in blockTable)
{
    var record = (BlockTableRecord)transaction.GetObject(id, OpenMode.ForRead);
    if (record.IsLayout || record.IsAnonymous || record.IsFromExternalReference) { continue; }
    var json = RackBlockData.Read(transaction, id);
```

Recorre **definiciones**, no entidades del espacio modelo. Devuelve `RackEnvelopeScan`
(`DefinitionId`, `Embed`, `DirectReferenceCount`): datos por valor, ningun objeto vivo de AutoCAD
escapa de la transaccion del llamador.

**Para I-47 esto significa que la mitad «encontrar a todos» ya existe y no necesita cambios.**

### 3.2 La resolucion por GUID (multi-vista)

`src/RackCad.Plugin/RackCommandSupport.cs:109` — `FindRackBlocks(Document, string rackId)` filtra el
barrido por `embed.Id` con `OrdinalIgnoreCase`. Es lo que hace que RACKEDITAR reabra **todas las
vistas del mismo rack**. Sus llamadores: RACKEDITAR de los cinco sistemas
(`RackSelectivoCommands.cs:90`, `RackDinamicoCommands.cs:178`, `RackPushBackCommands.cs:184`,
`RackCantileverCommands.cs:234`, `RackCabeceraCommands.cs:238`) mas RACKLAYOUT
(`RackLayoutCommands.cs:71`) y RACKRELLENAR (`RackLayoutCommands.Fill.cs:59`).

> **Dos politicas de filtro conviven, y no son la misma (H-02).** `FindRackBlocks` exige **solo** el
> GUID y tolera un `Kind` ausente; RACKLISTA y RACKBOMTOTAL exigen `Id` **y** `Kind` no vacios
> (`RackInventarioCommands.BomTotal.cs:53`). Un barrido «todos los racks del dibujo» tendria que
> elegir una de las dos, y hoy **no existe** un helper que agrupe **todos** los racks por GUID: el
> agrupamiento esta escrito tres veces.

### 3.3 El redibujo: la primitiva y el orquestador

- **Primitiva.** `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:88` —
  `RedefineSystemBlock(...)` vacia y repuebla la definicion conservando id y nombre. Incluye el
  arreglo de extents obsoletos ya conocido del proyecto:

  ```csharp
  foreach (ObjectId referenceId in systemDef.GetBlockReferenceIds(directOnly: true, forceValidity: true))
  {
      var reference = (BlockReference)tr.GetObject(referenceId, OpenMode.ForWrite);
      reference.RecordGraphicsModified(true);
  }
  ```

- **Orquestador.** `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:45` — `RedrawInPlace(...)`
  redefine, reescribe el payload, purga las definiciones anidadas huerfanas y llama a `ApplyRegen`.
- **Contrato de producto.** [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md):
  *Actualizar* redibuja en sitio conservando el GUID; *Insertar* crea una definicion nueva **con el
  mismo GUID** mas una referencia colocada con jig.

### 3.4 La primitiva de lote ya existe

`SystemBlockWriter.ApplyRegen(Document, bool regen)` (linea 87) es **el unico** punto donde se dispara
`Editor.Regen()` en el camino de escritura. El idioma establecido es: `regen: false` en cada vista y
**un solo** `Editor.Regen()` al final. Se ve en los cinco RACKEDITAR
(`RackSelectivoCommands.cs:195`, `RackCabeceraCommands.cs:291`, `RackDinamicoCommands.cs:275`,
`RackPushBackCommands.cs:323`, `RackCantileverCommands.cs:357`):

```csharp
if (updatedFrontal + updatedLateral + updatedPlanta + erasedPhantoms > 0)
{
    document.Editor.Regen(); // ONE regeneration refreshes every redefined (and drops every erased) view-block
}
```

### 3.5 El precedente mas cercano a un barrido que ESCRIBE: no existe

**Hoy ningun comando redibuja varios racks.** Los precedentes reales, en orden de cercania:

| Comando | Que hace | Por que no es el precedente completo |
|---|---|---|
| `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs`) | Barre el dibujo, agrupa por GUID, resuelve handler por kind y **aborta todo** si a algun kind le falta handler | Es **de solo lectura**: no redibuja nada |
| `RACKLISTA` (`RackInventarioCommands.cs`) | Mismo barrido, forma de inventario | Solo lectura |
| `RACKLAYOUT` / `RACKRELLENAR` | Insertan N referencias en una transaccion y un solo `Regen()` | Es **insercion**, no redibujo de lo existente |
| `RACKDUPLICAR` | Coloca copias repetidas | Lote solo en la UX |

`RACKBOMTOTAL` aporta el esqueleto **enumerar -> agrupar por GUID -> resolver handler -> actuar por
rack, todo-o-nada**, con dos compuertas de aborto ya escritas: `KindHandlerDispatch.TryResolveAll` y
`RackCommandSupport.PreflightInnerSources`.

> **Lo que falta es la costura de escritura sin ventana (H-03).** Los seis `Edit*` estan escritos
> alrededor de una ventana WPF modal: deserializan, **abren el editor**, y solo despues redibujan
> leyendo `window.UpdateOnly` / `window.InsertRequested`. No existe hoy un «aplicar este diseno y
> redibujar» sin dialogo. Es el elemento que un cambio de valor de proyecto necesitaria y que el
> arbol **no** ofrece.

### 3.6 La identidad se fragmenta al copiar

`src/RackCad.Plugin/RackEnvelopeRestamp.cs:20` — `RestampEnvelope(payload, copyName)` acuna un
`Guid.NewGuid()` nuevo. Lo usan RACKDUPLICAR y la variante «copia independiente» de RACKLAYOUT. Por
tanto «todos los racks afectados» en un dibujo real son **N GUIDs distintos**, no N referencias a uno.
Las copias **enlazadas**, en cambio, comparten definicion: editar una edita todas, gratis.

### 3.7 Censo de comandos

**31 atributos `[CommandMethod]` en `src/RackCad.Plugin` = 16 comandos + 15 alias** (`RACKSECCION` es
el unico sin alias). Verificado por conteo:

```
QUICKCABECERA/QCB  QUICKCAMA/QCM  RACKAYUDA/RA  RACKBOMTOTAL/RB  RACKCABECERA/RCB
RACKCAD/RK  RACKCANTILEVER/RCT  RACKDUPLICAR/RD  RACKEDITAR/RED  RACKLAYOUT/RLY
RACKLISTA/RL  RACKPUSHBACK/RPB  RACKRELLENAR/RR  RACKSECCION  RACKSELECTIVO/RS
RACKSISTEMADINAMICO/RSD
```

> **Hallazgo (H-04).** `src/RackCad.UI/RackCommandReference.cs:28` se declara *«la unica fuente de
> verdad»* de la referencia de comandos que muestra RACKAYUDA, y **omite `RACKPUSHBACK`/`RPB` y
> `RACKSECCION`**. Coincide con lo ya registrado en
> [ideas-futuras.md](../ideas-futuras.md) sobre dos comandos ausentes de la referencia.

### 3.8 Lectura de la configuracion por rack

Dos capas:

1. **Sobre**: `src/RackCad.Application/Persistence/RackEmbedDocument.cs` — `RackEmbedStore.Deserialize`
   **nunca lanza**: un documento de major superior devuelve `null` por `SchemaVersionPolicy.IsReadable`.
   Conserva campos desconocidos con `[JsonExtensionData]`.
2. **Diseno por kind**, dentro de `embed.Design`: `SelectivePalletDesignStore` (selectivo),
   `RackProjectStore` (dinamico, pushback, cantilever, cabecera) y `FlowBedConfigurationStore` (cama).
   **No se comportan igual ante JSON invalido**: los dos primeros **lanzan**, el tercero devuelve
   `null`. Cada llamador envuelve los primeros en `try/catch`.

---

## 4. D3 — Vertical slice: `VerticalClearance`, `PalletTolerance`, `PalletDepth`

El dueno eligio tres variables **como muestra**. El resultado confirma la sospecha que motivo el
ejercicio: **no se comportan igual**, y sus diferencias son de tres tipos distintos —de **nombre**, de
**granularidad** y de **vigencia**—.

### 4.1 Tabla comparativa

| | `VerticalClearance` | `PalletTolerance` | `PalletDepth` |
|---|---|---|---|
| **Existe con ese nombre** | **solo en Selectivo** | Selectivo, Dinamico, Push Back | Selectivo, Dinamico, Cama (FlowBed) |
| **El mismo concepto en otros sistemas** | si, con **otro nombre**: `ClearHeight`, `RequestedClearHeight` | si, mismo nombre | si, con **otro nombre**: `PalletSpecification.Depth` |
| **Copias independientes** | 3 (Selectivo / Dinamico+PushBack / Cantilever) | 2 (Selectivo / Dinamico+PushBack) | **4** (Selectivo / Dinamico+PushBack / FlowBed / prompt del Plugin) |
| **Granularidad propia** | por rack **+ override por celda** (`ClearOverride`) | **por rack** | por rack **+ override por fondo** |
| **Granularidad del gemelo** | `ClearHeight` es **por NIVEL de cada frente** | igual | `Pallet.Depth` es **una por rack**, sin override |
| **Vigencia geometrica** | vigente en los tres | vigente en Selectivo, **MUERTA** en Dinamico/Push Back | vigente en los cuatro |
| **Default** | `6.0` (inicializador de campo) | `4.0` (inicializador de campo) | `SelectiveRackDefaults.DefaultPalletDepth = 48.0` |
| **Guarda legado (Selectivo)** | **no** | **no** | **si** (`> 0.0 ? ... : default`) |
| **Guarda legado (Dinamico)** | **si** (`ClearHeight ?? default`) | **si** (`?? default`, DTO nullable) | **no** (`double` directo) |
| **Consumidor geometrico** | `SelectiveGeometryResolver.cs:96` | idem, `:95` | `SelectiveDepthLayout`, builders lateral/planta |

### 4.2 `VerticalClearance` — el mismo concepto con dos nombres y dos granularidades

Declaracion unica, en Domain:

```csharp
// src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:29-30
/// <summary>Vertical clearance ("holgura") above a pallet inside its clear opening (in). Editable; default 6".</summary>
public double VerticalClearance { get; set; } = 6.0;
```

`git grep VerticalClearance -- src/` devuelve **9 lineas en 6 archivos, todos del Selectivo**. Un
unico consumidor geometrico: `SelectiveGeometryResolver.cs:96`.

El **mismo concepto fisico** existe en Dinamico, Push Back y Cantilever bajo otro nombre y **con otra
granularidad**:

```csharp
// src/RackCad.Domain/Systems/Dynamic/DynamicRackFront.cs:169  (dentro de DynamicRackLevel)
public double ClearHeight { get; set; } = DynamicRackDefaults.DefaultClearHeight;   // = 6.0
```

`ClearHeight` aparece en **53 archivos** y es **por nivel de cada frente**, no por rack. Push Back lo
copia nivel a nivel (`PushBackSideDesign.cs:127`) y Cantilever tiene el suyo con un tercer nombre,
`RequestedClearHeight` (`CantileverStationDesign`, `CantileverLineDesign`).

**Pero el Selectivo no es puramente por rack**: tiene una **valvula de escape por celda** que gana
sobre el valor del rack.

```csharp
// src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs — SelectiveCell
/// <summary>Manual override for the clear/separation BELOW this level's beam (in) ... Null = auto.</summary>
public double? ClearOverride { get; set; }
```

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

**Esta es la asimetria central del slice**: el mismo numero (6") con el mismo significado fisico esta
modelado en **tres granularidades distintas** —valor del rack con excepcion por celda (Selectivo),
dato por nivel sin valor de rack (Dinamico/Push Back), y campo propio (Cantilever)—. Un unico valor de
proyecto no puede sustituir a los tres sin decidir antes que pasa con la granularidad fina, y esa
decision es del dueno.

**Nota util**: el patron `null = automatico, valor = manual` de `ClearOverride` es exactamente la forma
«general con excepcion local» que una variable de proyecto necesitaria un nivel mas arriba. El
Selectivo ya lo tiene resuelto **entre rack y celda**; lo que no existe es el escalon **entre proyecto
y rack**.

### 4.3 `PalletTolerance` — vigente en un sistema, legado declarado en otro

```csharp
// src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:26-27
/// <summary>Horizontal tolerance per gap between/around pallets (in). Editable; default 4".</summary>
public double PalletTolerance { get; set; } = 4.0;
```

```csharp
// src/RackCad.Domain/Systems/Dynamic/DynamicRackDefaults.cs
/// <summary>Legacy persisted input; dynamic beam cuts now use the fixed BFR contract.</summary>
public const double DefaultPalletTolerance = 4.0;
```

En el Dinamico **el propio codigo declara la variable legado**: los cortes de larguero pasaron al
contrato BFR fijo (`BfrAllowance = 2.0`). Sigue viva en `DynamicRackDesign.cs:36`,
`DynamicRackSystem.cs:33` y en el DTO (9 menciones en `DynamicRackSystemDocument.cs`) **porque esta
persistida**, no porque gobierne el dibujo.

**«Legado» se queda corto: es geometricamente MUERTA, y aun asi obligatoria.** Comprobado en el arbol:

```csharp
// src/RackCad.Application/Systems/Dynamic/DynamicFrontGeometry.cs
public static double AutoBeamLength(double palletFront, int palletCount, double tolerance)
{
    var count = Math.Max(1, palletCount);
    return Bfr(palletFront) * count + DynamicRackDefaults.InOutBeamLengthAllowance;   // 'tolerance' NO se lee
}
```

```csharp
// src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:471-472
if (design.PalletTolerance <= 0.0)
    throw new ArgumentException("La holgura transversal debe ser mayor que cero.", nameof(design));
```

El parametro `tolerance` entra en la funcion y **no aparece en el cuerpo**. Aun asi el resolver
**rechaza** un diseno con tolerancia <= 0. Y Push Back **si** expone el campo al usuario
(`RackPushBackSystemWindow.xaml:89`, etiqueta «Tolerancia»), mientras el Dinamico **no lo expone** y lo
fija a la constante en la ventana (`RackDynamicSystemWindow.xaml.cs:892-894`). Resultado: en Push Back
el usuario edita un campo obligatorio, positivo y persistido **que no cambia el dibujo**.

El contraste con el Selectivo es total: alli `AutoBeamLength(cell, tolerance)` **si** la consume
(`SelectiveGeometryResolver.cs:387-390`, regla documentada `LONGITUD = Frente*Count + Tolerance*(Count+1)`).

**Consecuencia para I-47**: promover `PalletTolerance` a variable de proyecto la elevaria a autoridad
global **justo donde ya no manda** — y le daria alcance de dibujo a un campo que en dos sistemas de
tres es decorativo. Es el caso que mejor demuestra que la lista de variables **no puede salir de una
busqueda por nombre**.

### 4.4 `PalletDepth` — la que ya tiene dos niveles de autoridad

```csharp
// src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:35-36
/// <summary>Pallet depth / fondo (in): the depth of the cabeceras in the LATERAL view. Editable.</summary>
public double PalletDepth { get; set; } = SelectiveRackDefaults.DefaultPalletDepth;   // 48.0
```

Ya convive con un **override por fondo** (`ExtraFondoDepths`/`FondoDepths`: `<= 0` significa «hereda el
fondo 0»), que es exactamente la forma «valor general + excepcion local» que una variable de proyecto
necesitaria. Es **el precedente interno mas util del slice**.

Es tambien **la mas fragmentada de las tres: cuatro declaraciones independientes bajo tres nombres**.

| Copia | Simbolo | Ubicacion | Default |
|---|---|---|---|
| Selectivo | `PalletDepth` + `ExtraFondoDepths` | `SelectivePalletDesign.cs:36,59` | `DefaultPalletDepth = 48.0` (const) |
| Dinamico / Push Back | `PalletSpecification.Depth` | `src/RackCad.Domain/Systems/Shared/PalletSpecification.cs:25` | **ninguno** (0.0) |
| Cama (FlowBed) | `FlowBedConfiguration.PalletDepth` | `src/RackCad.Domain/Systems/FlowBed/FlowBedConfiguration.cs:16` | **ninguno** — y `0.0` es **valido** para una cama Pushback |
| Prompt del Plugin | literal | `src/RackCad.Plugin/RackCamaCommands.cs:80` | **`48.0` escrito a mano** |

La ultima merece atencion: dos prompts hermanos del mismo Plugin resuelven el mismo numero de forma
distinta —`RackCabeceraCommands.cs:79` usa `SelectiveRackDefaults.DefaultPalletDepth`, y
`RackCamaCommands.cs:80` escribe `DefaultValue = 48.0`—. Cambiar la constante hoy **no** cambia el
segundo prompt.

Ademas es la unica de las tres con **derivacion** encima —la regla cabecera = tarima − 6"—, y esa
regla esta **duplicada** entre Application y la UI:

```csharp
// src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs:173-174
var pallet = palletDepth > 0.0 ? palletDepth : SelectiveRackDefaults.DefaultPalletDepth;
var cabecera = pallet - SelectiveRackDefaults.CabeceraFondoAllowance;
```

reproducida en `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs:1260-1261` y `:2398`.
Eso **contradice la convencion 2 de AGENTS** («regla en un solo sitio»). Se registra; no se corrige
aqui.

### 4.5 Las tres, lado a lado, en el mismo resolver

El contraste de autoridades es literal — tres lineas consecutivas de
`src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:94-96`:

```csharp
var paso = SelectiveRackDefaults.TroquelPaso;   // constante: NO configurable sin recompilar
var tolerance = design.PalletTolerance;         // del diseno del rack
var clearance = design.VerticalClearance;       // del diseno del rack
```

En tres renglones conviven **dos** de los modelos de autoridad de RackCad.

### 4.5-bis. El tercer modelo ya existe — y esta en el alcance equivocado

`assets/catalogs/defaults.json` es un archivo editable que el catalogo carga y que **cambia numeros de
producto sin recompilar**:

```json
{ "...": "...", "defaultHeaderHeight": 132.0, "headerEndAllowance": 6.0 }
```

Lo deserializa `JsonRackCatalogProvider` (`DefaultsFile = "defaults.json"`, linea 30;
`Defaults = ReadObject(DefaultsFile, new RackDefaults())`, linea 126) sobre
`src/RackCad.Application/Catalogs/RackDefaults.cs`, cuyo comentario lo dice sin rodeos:

> *«Global default ids/values for the standard frame, loaded from `defaults.json`. Property
> initializers fall back to `CatalogIds` so a missing file still yields a usable standard. Editing
> the JSON changes the "standard recipe" without recompiling.»*

**Esto importa mucho para I-47, y en dos direcciones opuestas:**

1. **A favor**: el patron que la iniciativa persigue —valor global, editable, con fallback si el
   archivo falta— **ya esta implementado y probado** en este repositorio. No hay que inventarlo.
2. **En contra**: su alcance es **por instalacion** (el archivo del catalogo), no **por dibujo**. Dos
   DWG distintos abiertos en la misma maquina comparten forzosamente esos valores, y el mismo DWG
   abierto en otra maquina puede leer otros. Ademas el catalogo compartido es de **solo lectura** en
   la practica del proyecto, asi que no es un sitio donde un proyecto escriba lo suyo.

Los tres modelos vigentes, entonces:

| Modelo | Alcance | Ejemplo | Editable en runtime |
|---|---|---|---|
| `const` en Domain | compilacion | `TroquelPaso = 2.0` | **no** |
| `defaults.json` | **instalacion** | `headerEndAllowance = 6.0` | si (archivo) |
| Campo del diseno | **rack** (+ celda/fondo) | `VerticalClearance` | si (editor) |

**El escalon que falta es exactamente el que pide I-47: el dibujo.** No es un hueco entre nada y algo,
sino un escalon ausente en una escalera que ya tiene tres peldanos.

### 4.6 Persistencia y fallback legado

En `src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs` las tres son `double` **no
anulables**, y `ToDomain` las trata distinto:

```csharp
PalletTolerance = PalletTolerance,                       // sin guarda
VerticalClearance = VerticalClearance,                   // sin guarda
PalletDepth = PalletDepth > 0.0 ? PalletDepth : SelectiveRackDefaults.DefaultPalletDepth, // legacy docs had no fondo
```

Un `double` no anulable ausente en el JSON deserializa como **`0.0`** y el inicializador de campo del
Domain (`= 4.0`, `= 6.0`) **queda pisado** por el inicializador de objeto. Hoy eso **no es un defecto
activo**: `git log -S` confirma que `PalletTolerance` y `VerticalClearance` estan en el DTO desde su
**primer commit** (`d15371d`, 2026-07-08), asi que no existe documento legado sin ellas. Pero **no
cumplen** el patron que exige la convencion 4 de AGENTS (nullable + fallback explicito), y
`PalletDepth` —vecina inmediata en el mismo bloque— **si** lo hace.

**Y el Dinamico, para las mismas dos variables, si cumple:**

```csharp
// src/RackCad.Application/Persistence/DynamicRackSystemDocument.cs:688-690
ClearHeight = ClearHeight.HasValue && ClearHeight.Value >= 0.0
    ? ClearHeight.Value
    : DynamicRackDefaults.DefaultClearHeight,
```

`DynamicRackSystemDocument.PalletTolerance` es `double?` con `?? DynamicRackDefaults.DefaultPalletTolerance`
en dos sitios. Push Back hereda ese DTO y por tanto ese fallback.

**La disciplina defensiva no difiere por variable: difiere por SISTEMA.** El Dinamico protege
`ClearHeight` y `PalletTolerance` y deja `PalletDepth` sin guarda; el Selectivo hace exactamente lo
contrario. Ninguna regla explica el reparto. Para I-47 esto significa que **no existe un contrato
unico de «valor ausente»** del que colgar el de proyecto: habria que fijarlo, y eso es decision.

### 4.7 Constantes que son variables de proyecto disfrazadas

Fuera de las tres, el arbol tiene valores que **solo** se cambian recompilando y que responden a la
descripcion de «variable de proyecto». Se listan como **inventario del Discovery**, no como propuesta:

- `src/RackCad.Domain/Systems/Selective/SelectiveRackDefaults.cs`: `TroquelPaso = 2.0`,
  `CabeceraFondoAllowance = 6.0`, `DefaultFloorBeamRise = 4.0`, `DefaultSeparator = 12.0`,
  `MaxDepthCount = 4`, `DefaultBeamPeralte = 4.0`.
- `src/RackCad.Domain/Systems/Dynamic/DynamicRackDefaults.cs`: `BfrAllowance = 2.0`,
  `InOutBeamLengthAllowance = 6.0`, `FlowBedLengthClearance = 4.0`, `HeaderEndAllowance = 6.0`.
- `src/RackCad.Domain/Systems/FlowBed/FlowBedDefaults.cs`: `BrakeClearanceOverPallet = 1.0`.
- `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs`: `TopeLengthAllowance = 0.25`,
  `LateralLengthAllowance = 4.0`.

`TroquelPaso` es el caso extremo: se consume como `const double paso = SelectiveRackDefaults.TroquelPaso;`
en `SelectiveLateralBuilder.cs:295` y `:344` y en `SelectiveFrontalBuilder.cs:205` — un `const` local,
literalmente inalcanzable en runtime.

> **Duplicacion detectada (H-05).** El valor 6" existe con **tres declaraciones independientes**:
> `HeaderEndAllowance` en `RackDefaults.cs` **y** en `DynamicRackDefaults.cs` (dos copias del mismo
> nombre), y `CabeceraFondoAllowance` en `SelectiveRackDefaults.cs:37`. **Y las dos copias del mismo
> nombre no tienen el mismo alcance**: la de `RackDefaults` se sobrescribe desde `defaults.json`
> (§4.5-bis) y la de `DynamicRackDefaults` es `const`. Editar el JSON mueve una y deja la otra donde
> estaba. Se registra; no se unifica aqui.

Y un valor de nivel dibujo guardado por rack: **`DimensionStyle`**
(`SelectivePalletDesignDocument.cs:82`, `DynamicRackSystemDocument.cs:63`), elegido de una
`DimStyleTable` que pertenece al dibujo (§2.4).

---

## 5. Riesgos

| # | Riesgo | Evidencia | Por que importa |
|---|---|---|---|
| R1 | **Mecanismo nuevo, no reutilizado** | §2.1: cero NOD, cero SummaryInfo/XData | Toda estimacion que asuma «ya existe donde guardarlo» es falsa |
| R2 | **Cero cobertura automatizada del almacen en DWG** | §2.3 punto 4 | Cualquier autoridad nueva nace en la mitad no testeable del sistema (ADR-0006). Mitigacion natural: DTO+serializador en Application, solo el acceso a AutoCAD en el Plugin |
| R3 | **Supervivencia no verificada** | §1 | WBLOCK, copiar/pegar entre dibujos y PURGE no se probaron. Un almacen de nivel dibujo que no sobreviva a copiar el DWG no cumple el objetivo |
| R4 | **La granularidad no coincide entre sistemas** | §4.2: `VerticalClearance` por rack + `ClearOverride` por celda, vs `ClearHeight` por nivel, vs `RequestedClearHeight` de Cantilever | Un valor unico por DWG no puede sustituir a un dato por nivel sin una decision de producto |
| R5 | **Elevar una variable MUERTA** | §4.3, verificado: `DynamicFrontGeometry.AutoBeamLength` no lee su parametro `tolerance`, y el resolver aun exige `> 0` | La lista de variables no puede derivarse de coincidencias de nombre. Push Back expone al usuario un campo obligatorio que no cambia el dibujo |
| R5-bis | **Ya existe un modelo global, en el alcance equivocado** | §4.5-bis: `defaults.json` + `RackDefaults` | El patron esta resuelto **por instalacion**; confundirlo con «por dibujo» daria una solucion que no cumple el objetivo, y el catalogo compartido ademas es de solo lectura |
| R5-ter | **No hay contrato unico de «valor ausente»** | §4.6: el Dinamico protege `ClearHeight`/`PalletTolerance` y no `PalletDepth`; el Selectivo al reves | El fallback del valor de proyecto no puede heredarse de una regla existente: no la hay |
| R6 | **No existe redibujo sin dialogo** | §3.5 (H-03) | «Cambiar un valor y que el dibujo reaccione» exige extraer una costura que hoy no existe en ninguno de los seis `Edit*` |
| R7 | **La identidad se fragmenta al copiar** | §3.6 | «Todos los racks afectados» son N GUIDs distintos; las copias enlazadas se actualizan gratis y las independientes no |
| R8 | **Dos politicas de filtro y tres agrupamientos por GUID** | §3.2 (H-02) | Un barrido nuevo debe elegir una politica, y elegir mal cambia en silencio que racks entran |
| R9 | **Precedencia sin definir entre proyecto y rack** | §4.6 y §4.4 | El unico precedente interno de «general + excepcion» es `FondoDepths`; extenderlo es decision del dueno |
| R10 | **Archivos calientes** | WORKFLOW §7 | Los tres editores grandes y `SelectivePalletDesign.cs` (776 lineas) estan en la tabla de archivos calientes. Una implementacion futura los toca y debe serializarse con cualquier otra iniciativa de esos sistemas |

> **Estado tras el Gate C** (§6.1): **R4 se acota** —C-5 saca de ID22A la unificacion de granularidades,
> asi que sigue siendo un hecho del arbol pero no un riesgo de esta iniciativa—; **R5-bis se cierra**
> —C-6 prohibe convertir `defaults.json` en `ProjectVariables`—; y **R6 se activa** —C-3 exige propagar
> en una sola operacion, asi que la costura sin dialogo pasa de hipotetica a obligatoria—. Los demas
> siguen vigentes tal cual.

---

## 6. Decisiones del dueno y lo que sigue abierto

> **Actualizado en el Gate C (2026-09-08).** Esta seccion planteaba siete preguntas. **Seis quedaron
> resueltas por decision del dueno** y ya **no** son preguntas: presentarlas como abiertas seria
> falso. Se listan como **premisas** con su decision literal, y solo despues lo que de verdad sigue
> sin decidir. El registro vinculante vive en
> [`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md) §«Decisiones vinculantes del
> Gate C»; si esta prosa y aquel registro discreparan, **manda el registro**.

### 6.1 Premisas fijadas — no se reabren

| # | Decision del dueno | Que pregunta cierra |
|---|---|---|
| **C-1** | Un DWG **sin** `ProjectVariables` tiene un **registro vacio**; **no hay migracion** y los **literales existentes se preservan** | «dibujos heredados» |
| **C-2** | Una propiedad es **`Literal`** o **`ProjectVariableReference`**; cuando es referencia, **la referencia gobierna el valor efectivo** | «precedencia proyecto vs rack» |
| **C-3** | Cambiar una variable **propaga y redibuja a todos sus consumidores en UNA operacion** | «retroactividad» |
| **C-4** | **RACKDUPLICAR conserva el mismo `VariableId`** | «copias independientes» |
| **C-5** | **ID22A no unifica** el `ClearHeight` de otros sistemas; el vertical slice es **Selectivo** | «granularidad» y el alcance de la lista **para ID22A** |
| **C-6** | **`defaults.json` convive** y **no** se convierte en `ProjectVariables` | «relacion con defaults.json» |

Tres consecuencias que conviene leer junto al resto del informe, porque **desactivan** parte de lo que
este documento planteo como problema:

- **C-5 acota el riesgo R4.** La divergencia de granularidad entre `VerticalClearance` (Selectivo),
  `ClearHeight` (Dinamico/Push Back, **por nivel**) y `RequestedClearHeight` (Cantilever) sigue siendo
  **un hecho del arbol** —§4.2 no cambia—, pero **deja de ser un problema de ID22A**: nada se unifica
  aqui. R4 pasa de riesgo activo a **contexto para una iniciativa posterior**.
- **C-3 activa el riesgo R6.** La costura «aplicar diseno y redibujar **sin dialogo**», que §3.5
  documenta como inexistente, **deja de ser hipotetica**: propagar en una operacion la exige.
- **C-6 cierra la puerta a reutilizar `defaults.json`.** Junto con D1 (§2.1), significa que la
  autoridad de nivel dibujo es **mecanismo nuevo por partida doble**: ni existe, ni se va a construir
  reciclando el unico mecanismo global que hay.

### 6.2 Diferido por decision expresa

**Supervivencia a `WBLOCK` y a copiar/pegar entre dibujos.** El dueno la declara **cuestion
diferible**: no es requisito de ID22A. Sigue siendo **no verificada** (§1: no se ejecuto AutoCAD), y
por tanto este informe no afirma nada sobre ella en ningun sentido. Lo que si es requisito minimo —y
tambien esta sin verificar— es la supervivencia a **guardar, cerrar y reabrir el mismo DWG**.

### 6.3 Lo que sigue realmente abierto

Solo esto, y ninguna de las dos es una pregunta que el Discovery pudiera contestar:

1. **El contrato concreto** que implementa las seis premisas: identidad, tipos, formato persistido,
   semantica de borrado y desvinculado, descubrimiento, orden de redibujo y superficie de UI. Es el
   objeto de la **Proposal V4** ([I-47-proposal-v4.md](I-47-proposal-v4.md)), que lo compara y lo
   recomienda; **elegir** sigue siendo del dueno y el gate `owner-decision` sigue abierto.
2. **La lista de variables mas alla del slice.** C-5 la fija **para ID22A** (Selectivo,
   `VerticalClearance`). §4.7 inventaria al menos una docena de candidatos para despues —incluido
   `DimensionStyle`, que **no es numerico** y por eso presiona el modelo de tipos—. Fuera de alcance
   aqui.

---

## 7. Hallazgos fuera de alcance

Se registran; **no** se corrigen en esta iniciativa. Su destino documental es
[ideas-futuras.md](../ideas-futuras.md) cuando el dueno lo autorice, ya que no forman parte del
entregable del Discovery.

| ID | Hallazgo | Ubicacion |
|---|---|---|
| H-01 | El comentario XML de `RackBlockData` dice «block **reference's** extension dictionary»; los doce llamadores pasan la **definicion** | `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:7-11` |
| H-02 | Tres agrupamientos por GUID con **dos** politicas de filtro distintas; no hay helper unico | `RackCommandSupport.cs:109`, `RackInventarioCommands.cs:46`, `RackInventarioCommands.BomTotal.cs:53` |
| H-03 | No existe costura «aplicar diseno y redibujar» sin ventana WPF modal | los seis `Edit*` del Plugin |
| H-04 | `RackCommandReference.cs` se declara fuente unica y omite `RACKPUSHBACK`/`RPB` y `RACKSECCION` | `src/RackCad.UI/RackCommandReference.cs:28` |
| H-05 | El valor 6" duplicado con tres nombres (`HeaderEndAllowance` x2, `CabeceraFondoAllowance`) | `RackDefaults.cs:20`, `DynamicRackDefaults.cs:45`, `SelectiveRackDefaults.cs:37` |
| H-06 | La regla cabecera = tarima − 6" esta duplicada entre Application y la UI (contradice AGENTS conv. 2) | `SelectiveDepthLayout.cs:173-174` vs `RackSelectiveWindow.xaml.cs:1260-1261, 2398` |
| H-07 | `PalletTolerance` y `VerticalClearance` no siguen el patron nullable + fallback de AGENTS conv. 4, y su vecina `PalletDepth` si (latente, no activo; ningun test cubre la ruta del cero legado) | `SelectivePalletDesignDocument.cs:30-31, 170-171` |
| H-09 | **`PalletTolerance` es geometricamente muerta en Dinamico/Push Back y aun asi obligatoria**: `AutoBeamLength` recibe `tolerance` y no lo lee; el resolver lanza si es `<= 0`; Push Back la expone al usuario | `DynamicFrontGeometry.cs:34-38`, `DynamicRackSystemResolver.cs:471-472`, `RackPushBackSystemWindow.xaml:89` |
| H-10 | Dos prompts hermanos del Plugin resuelven el mismo fondo de tarima de forma distinta: uno usa la constante, el otro un literal `48.0` | `RackCabeceraCommands.cs:79` vs `RackCamaCommands.cs:80` |
| H-11 | El termino de claro `topCell.Pallet.Height + topCell.ClearHeight` —con su mismo preambulo `ordered`/`top`/`topCell`— esta escrito **dos veces**. Las alturas de cabecera que lo rodean **si** difieren, asi que es duplicacion del termino, no de la formula entera | `DynamicHeaderHeightCalculator.cs:129` y `PushBackHeaderHeight.cs:58` |
| H-08 | **Marcador de conflicto de merge sin resolver commiteado en `main`**: la linea `\|\|\|\|\|\|\| 085ca2f` | `docs/ideas-futuras.md:813` |

> H-08 se detecto al buscar el ID del dueno en la documentacion. **No pertenece a I-47** y no se toca
> desde esta rama: `ideas-futuras.md` no es archivo caliente, pero corregir un defecto ajeno desde la
> rama de otra iniciativa contradice el alcance. Se reporta para que el dueno decida donde arreglarlo.
