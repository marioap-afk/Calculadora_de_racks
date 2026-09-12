# I-51 — ID15 — Discovery (G1): RACKDUPLICAR hoy, y lo que exige duplicar varios origenes

> **Esto es G1: un informe de caracterizacion.** No cambia codigo productivo ni pruebas. Las
> «respuestas de diseno» de la seccion 12 son **insumo para G2**, no un contrato: la implementacion
> sigue bloqueada, y la seccion 16 declara lo que necesita **revision de Arquitecto** y **decision del
> dueno** antes de G2.
>
> Contrato: [I-51-rackduplicar-multiples-origenes.md](I-51-rackduplicar-multiples-origenes.md).

## 0. Reconciliacion de G2 (2026-09-12)

Este informe se escribio en G1 (`c6fbfbc`) y **se conserva como evidencia de ese momento**: no se reescribe
para aparentar que G1 conocia decisiones posteriores. G2 lo reconcilia con las decisiones PD-1..PD-7 y con
la revision de Arquitecto (`AGREED WITH CHANGES`), registradas en
[`docs/automation/decisions/I-51.md`](../automation/decisions/I-51.md).

Cada afirmacion que G2 corrige lleva junto al texto original una marca **`[G2-Dn]`**; las marcas **`[G2]`**
solo anotan el estado posterior. **Donde este informe y el contrato difieran, manda el contrato.**

| Marca | Correccion |
|---|---|
| `[G2-D1]` | `ProjectVariableScanProjection` **no** clasifica fuentes de I-51: convierte un sobre sin `Id` en `UnreadableEnvelope` (`ProjectVariableScanProjection.cs:37-42`), lo que contradice PD-6. La clasificacion es propia, en cuatro clases (contrato, INV-03). `SelectiveAuthoredAuthority.IsSameAuthority` **si** se reutiliza |
| `[G2-D2]` | La clave logica es un valor **discriminado**: `RackId` (sin distinguir mayusculas) o `Definition(handle)` cuando el `Id` es null, vacio o solo espacios (PD-6). El handle vive solo dentro del lote y nunca se persiste como RackId |
| `[G2-D3]` | La entrada con identidad explicita recibe **`Guid newId`**, no un string, con los invariantes NI-1..NI-6 del contrato; la firma historica de dos argumentos delega en ella |
| `[G2-D4]` | No hay estructura `OldRackId → NewRackId`: el plan de cada destino asigna `{LogicalSourceKey, NewRackId, CopyName}`; la correspondencia definicion → clon es local a la transaccion del Plugin |
| `[G2-D5]` | Estado posterior de las paralelas: **I-49** reclamada y bootstrapeada (`f2d28a2`); su contrato extiende `ProjectVariable.Definition` y **excluye ID21**. **I-50** cerro G1 en `fdaf2bc`: autoridad rack × tipo de vista **en el diseno**, metadata por instancia rechazada (`CD-01`, `CD-08`) |
| `[G2-D6]` | PD-1..PD-7 estan **cerradas**: ya no son preguntas abiertas |
| `[G2-D7]` | El titulo incorpora el ID del Owner, **ID15** |

## 1. Baseline inspeccionado y preflight

```text
Rama             = feature/rackduplicar-multiples-origenes
Worktree         = C:\Users\alejandra-mendoza\.codex\worktrees\feature-rackduplicar-multiples-origenes
BASE_SHA         = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main = main, "Merge I-48")
CLAIM_SHA        = 3ffd2ca21778b29bd5ccac2e6d171971769ad8ca   (commit vacio)
Claim-Id         = ad4b9e47-63c0-48c3-9406-9ae2db8a121a
BOOTSTRAP_SHA    = 5a5c12aaf04bb9f3edfd861aad9fc266dfb89cf2   (contrato + fila ROADMAP, solo docs)
Codigo auditado  = a4d88f1: src/ y tests/ byte-identicos, porque los commits de I-51 son solo docs
```

Preflight del 2026-09-12 (13:06 -06:00, repetido a las 13:19):

- `git fetch --prune origin`: `main` = `origin/main` = `a4d88f1`, divergencia `0/0`, arbol limpio.
- Worktrees registrados: el principal (`main`) y, desde las 13:05, el de I-50
  (`~/.codex/worktrees/feature-cotas-independientes-por-vista`). Ninguno de I-51.
- Ramas remotas: `main` y `feature/cotas-independientes-por-vista` (I-50: reclamo `97cc0ef`, contrato
  `4e22d91`, fila de ROADMAP `a2fba4f`). **Ninguna de I-49 ni de I-51.**
- `git stash list` vacio. Sin `MERGE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`,
  `rebase-merge`, `rebase-apply` ni `sequencer`.
- `I-49`, `I-50`, `I-51` en `main`: cero coincidencias en todo el repositorio.
- Reclamo: primer `git push -u` aceptado sin force (`* [new branch]`).

Metodo: lectura de codigo, historia (`git log --follow`) y busqueda de consumidores. **No** se ejecuto
AutoCAD ni ninguna suite: G1 no cambia codigo. Toda cita es `archivo:linea` sobre `a4d88f1`.

## 2. Respuestas cortas

| # | Pregunta | Respuesta | Detalle |
|---|---|---|---|
| 1 | Flujo actual | `GetEntity` de **una** referencia → snapshot en una transaccion de lectura → punto base → bucle de destinos. Por destino, **una** transaccion: `RestampEnvelope` (GUID nuevo **generado dentro**) → comprobar → `CloneDefinition` → `new BlockReference` en `Position + desplazamiento UCS→WCS` → commit. Sin Regen | §3 |
| 2 | ¿Una vista seleccionada duplica solo esa vista o todas sus hermanas? | **Solo esa vista**, siempre. Lo confirman el codigo, la historia y tres documentos; es decision del dueno del 2026-07-09 | §5.5 |
| 3 | Seleccion multiple agrupada sin perder vistas | Snapshot plano por referencia → **grupo por RackId** → **definiciones deduplicadas** → **todas las referencias elegidas** **[G2-D2]** | §12.1 |
| 4 | UN RackId nuevo para las hermanas copiadas de A | El GUID debe asignarlo el **llamador** (hoy nace en `RackEnvelopeRestamp.cs:36`), y cada definicion del grupo se re-estampa con el **mismo** `(newId, copyName)`. La mitad interior ya recibe `newId` (`IRackKindHandler.cs:75`) **[G2-D3]** | §12.2 |
| 5 | ¿Hace falta `OldRackId → NewRackId`? | **Si, efimero y por punto de destino**, como tabla de asignacion. **No** para reescribir contenido: hoy ningun diseno referencia a otro rack. Cambia si llega ID21 **[G2-D4]** | §12.3 |
| 6 | Atomicidad | Preflight completo sin escribir + **una transaccion por punto de destino** que cubre todos los grupos, definiciones y referencias. Los puntos ya confirmados quedan, como hoy | §12.4 |
| 7 | N racks × M puntos | `M × N` RackIds; `M × Σ definiciones` clones; `M × Σ referencias` referencias; un nombre por (rack, punto) | §12.5 |
| 8 | Riesgos con copias enlazadas | El mayor: `RACKLISTA` y `RACKBOMTOTAL` cuentan copias como el **MAX de referencias directas** por RackId; clonar por referencia en vez de por definicion cambia ese conteo en silencio. Una copia Selectivo con vistas divergentes sale del BOM y bloquea las operaciones de las variables que consume | §11 |
| 9 | Conflictos con I-49/I-50 | Ningun archivo productivo comun **previsto**; mismo grupo caliente `*Commands*.cs` que I-50; acoplamiento semantico si I-50 guarda politica por vista en la **referencia**; I-49 sin rama **[G2-D5]** | §13 |
| 10 | Piezas pequenas reutilizables | Sobrecarga de `RestampEnvelope` con `newId`; un planificador puro en Application; reutilizar `ProjectVariableScanProjection`, `SelectiveAuthoredAuthority`, `KindHandlerDispatch.TryResolveIgnoreCase`, `RackCloner` e `InDocumentTransaction` **[G2-D1]** **[G2-D3]** | §14 |
| — | ¿Decision arquitectonica material? | **SI**: AM-1, AM-2 y AM-3; AM-4 condicionada a I-50 **[G2]** AM-1..AM-3 reconciliadas; AM-4 cerrada como no material | §16 |

## 3. Flujo exacto actual: seleccion → clon → restamp → traslado → commit

`src/RackCad.Plugin/RackDuplicarCommands.cs` (213 lineas): comando `RACKDUPLICAR` (l.28-29), alias `RD`
(l.19).

```text
RackDuplicar()                                                    l.29-129   try/catch → Report (l.125-128)
 1 PickDuplicateSource                                            l.41 → l.144-176
     GetEntity + AddAllowedClass(BlockReference, exactMatch:false) l.149-153  UNA sola entidad
     InDocumentTransaction.Run (lectura, con commit)              l.159-172
       snapshot: DefinitionId = reference.BlockTableRecord        l.164
                 Position, Rotation, ScaleFactors, LayerId        l.165-168
       Payload = RackBlockData.Read(tx, DefinitionId)             l.170      la identidad vive en la DEFINICION
     RackEmbedStore.Deserialize(Payload)                          l.174
 2 embed nulo o Design vacio → mensaje y fin                      l.46-50
 3 KindHandlerDispatch.TryResolveIgnoreCase (gate antes de copiar) l.55-58
 4 GetPoint "Punto base"                                          l.60-67    coordenadas del UCS actual
 5 baseName = embed.Name o "Rack"; multiple = true; placed = 0    l.68-70
 6 bucle de destinos                                              l.72-117
     GetPoint destino, UseBasePoint, AllowNone, [Unica/Multiple]  l.74-84
     palabra clave → cambia el modo y sigue                       l.86-93
     Enter o Esc → fin                                            l.95-98
     placed++; copyName = base + " - copia" o " - copia N"        l.100-103
     displacement = (destino − base).TransformBy(CurrentUCS)      l.105-107  VECTOR, UCS → WCS
     position = source.Position + displacement                    l.108      WCS
     PlaceIndependentCopy                                         l.110 → l.181-210
       InDocumentTransaction.Run (escritura, UNA por destino)     l.185
         RestampEnvelope(source.Payload, copyName)                l.190      GUID nuevo en CADA llamada
         !IsSuccess → throw, sin commit: nada clonado             l.192-195
         RackCloner.CloneDefinition(..., labelFrom: embed.Name, labelTo: copyName)   l.197
         new BlockReference(position, clon) { Rotation, ScaleFactors, LayerId }       l.201-206
         AppendEntity + AddNewlyCreatedDBObject, commit           l.207-208
     modo Unica → fin tras esta copia                             l.113-116
 7 resumen "N copia(s) independiente(s)"                          l.119-123
```

Colaboradores directos:

- **`RackEnvelopeRestamp.RestampEnvelope`** (`RackEnvelopeRestamp.cs:32-49`): deserializa el sobre,
  **`embed.Id = Guid.NewGuid()`** (l.36) y `embed.Name = copyName` (l.37); re-estampa el diseno interior
  con **ese mismo** id (l.39 → `RestampDesign`, l.58-72, que resuelve el handler con
  `TryGetIgnoreCase` en l.66 y **lanza** si no lo hay en l.68); devuelve el fallo como valor (l.41-45) o
  el sobre serializado (l.47-48). Sin `catch` (I-47 G14, l.13-23).
- **`RackCloner.CloneDefinition`** (`RackCloner.cs:29-68`): BTR nuevo con nombre unico y el mismo
  `Origin` (l.34-36); `DeepCloneObjects` de las entidades **sin** clonar las definiciones anidadas
  ARRAY, que se comparten (l.38-50); renombra el `DBText` igual al nombre de origen (l.53-64); y
  **sustituye el payload** del clon (l.66). Politica de nombre propia `" (n)"` (l.71-87).
- **`InDocumentTransaction.Run`** (`InDocumentTransaction.cs:20-40`): bloqueo + transaccion + commit.
  Una excepcion en el cuerpo sale **sin commit**, es decir, con rollback.
- **Sin `Regen`**, fijado por la linea base de I-16 (`docs/initiatives/I-16-draw-services-baseline.md:117`).
- **Sin guardia de unidades**, por diseno de I-05 (`docs/initiatives/I-05-guardrail-unidades.md:105`).

### 3.1 Mapa de archivos

«I-51» dice lo que se **preve**, sin fijarlo: la lista vinculante la decide G2.

| Archivo | Papel hoy | I-51 |
|---|---|---|
| `src/RackCad.Plugin/RackDuplicarCommands.cs` | Comando: seleccion, bucle de destinos, transformacion, una transaccion por destino | **Tocaria** |
| `src/RackCad.Plugin/RackEnvelopeRestamp.cs` | Restamp compartido con `RACKLAYOUT`; **genera el GUID** | **Tocaria**: sobrecarga con `newId` |
| `src/RackCad.Plugin/RackCloner.cs` | Clon de una definicion con payload nuevo y etiqueta renombrada | Reutilizar |
| `src/RackCad.Plugin/InDocumentTransaction.cs` | Bloqueo + transaccion + commit de una fase | Reutilizar |
| `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs` | Payload en Xrecord de la **definicion** | No |
| `src/RackCad.Plugin/RackBlockFinder.cs` | Escaneo de sobres por definicion | No |
| `src/RackCad.Plugin/RackCommandSupport.cs` | `PickRackBlock`, `FindRackBlocks` | No |
| `src/RackCad.Plugin/KindHandlers/IRackKindHandler.cs`, `KindHandlerRegistry.cs`, `KindHandlerDispatch.cs` | Contrato y resolucion por kind | No |
| `src/RackCad.Plugin/KindHandlers/<Kind>KindHandler.cs` (las seis implementaciones) | Restamp de la identidad interior | No |
| `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | Sobre y su store | No |
| `src/RackCad.Application/Persistence/RestampResult.cs` | `RestampResult` y `SelectiveAuthoredRestamp` | No |
| `src/RackCad.Application/Persistence/RackListBuilder.cs` | Agrupacion pura por GUID | Precedente de forma |
| `src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs` | Clasificacion sin inventar RackId | Reutilizar **[G2-D1]**: NO como clasificador de fuentes de I-51 |
| `src/RackCad.Application/ProjectVariables/SelectiveAuthoredAuthority.cs` | Igualdad authored entre hermanas | Reutilizar |
| `src/RackCad.Application/ProjectVariables/ProjectVariableConsumerDiscovery.cs` | Aborta ante consumidores divergentes | No |
| `src/RackCad.Application/Bom/BomAuthoredAuthority.cs` | Autoridad del BOM por rack | No |
| `src/RackCad.Plugin/RackInventarioCommands.cs`, `RackInventarioCommands.BomTotal.cs` | Conteo de copias (MAX de referencias) | No |
| `src/RackCad.Plugin/RackLayoutCommands.cs`, `RackLayoutCommands.Fill.cs` | Consumidores de restamp y clon | No |
| `src/RackCad.Plugin/ProjectVariableMutationExecutor.cs` | Precedente PREPARE/MUTATE/POST | No |
| `src/RackCad.UI/RackCommandReference.cs` | Texto de ayuda del comando | Posible |
| `tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs`, `PushBackRoundTripSourceGuardTests.cs` | Guardas de texto sobre el comando y el restamp | **Reapuntar** (§15) |
| Application + `tests/RackCad.Tests` | Planificador puro y sus pruebas | **Nuevos** |
| `docs/guias/despliegue.md`, `README.md`, `docs/ideas-futuras.md`, `docs/guias/validacion-manual-autocad.md` | Documentacion del comando | Docs |

## 4. Seleccion actual

- **Una sola referencia**, por `Editor.GetEntity` (`RackDuplicarCommands.cs:149-153`). En **ningun**
  comando del Plugin hay `GetSelection`, `SelectImplied` ni `CommandFlags.UsePickSet` (busqueda vacia):
  la seleccion multiple es terreno nuevo, sin patron propio que reutilizar.
- `AddAllowedClass(typeof(BlockReference), exactMatch: false)` (l.151) admite subclases, `MInsertBlock`
  incluido; la copia de un MINSERT sale como referencia simple (§17, L-4).
- Un bloque **sin** payload y uno cuyo sobre **no deserializa** —JSON invalido o MAJOR futuro, que
  `RackEmbedStore.Deserialize` convierte en `null` (`RackEmbedDocument.cs:86-118`)— acaban en el
  **mismo** mensaje: «ese bloque no tiene datos de rack para duplicar» (`RackDuplicarCommands.cs:46-49`).
- **No se exige `embed.Id`**: un sobre legado sin id se duplica hoy, porque el restamp le da uno.
- `RACKEDITAR`, `RACKLAYOUT` y `RACKRELLENAR` usan otro selector con la misma forma,
  `RackCommandSupport.PickRackBlock` (`RackCommandSupport.cs:74-99`). `RACKDUPLICAR` tiene el suyo
  porque tambien necesita posicion, rotacion, escala y capa.

## 5. Vistas hermanas e identidad

### 5.1 Donde vive la identidad

El payload se guarda en el diccionario de extension de la **definicion** (`BlockTableRecord`), clave
`RACKCAD_SELECTIVE` (`RackBlockData.cs:16`). Todos los escritores pasan un id de definicion
(`LateralHeaderDrawService.cs:258`, `SystemBlockWriter.cs:33` y `:114`, `RackCantileverCommands.cs:156`
y `:324`, `RackCloner.cs:66`) y todos los lectores leen la definicion (`RackDuplicarCommands.cs:170`,
`RackCommandSupport.cs:93`, `RackBlockFinder.cs:71`, `RackLayoutCommands.cs:191`).

Consecuencia: **todas las referencias de una definicion comparten identidad**, y una referencia no
tiene identidad propia. El `<summary>` de `RackBlockData` dice «block reference's extension
dictionary» (l.8-11) y esta desactualizado (§17, L-1).

### 5.2 RackId exterior

`RackEmbedDocument` (`RackEmbedDocument.cs:14-65`): `SchemaVersion` (l.35), `Kind` (l.38), `View` (l.41;
vacio = frontal), `Section` (l.47; -1 = no seccionada), **`Id`** (l.50: «stable identity (GUID) of the
rack; kept across edits»), `Name` (l.53), `Design` (l.56) y `ExtensionData` (l.63-64, preservado en ida
y vuelta). **Un rack es el conjunto de definiciones cuyo sobre tiene el mismo `Id`.**

### 5.3 RackId interior, por kind

| Kind | Identidad interior | Re-estampado | Cita |
|---|---|---|---|
| `selective` | `SelectivePalletDesignDocument.Id` + `Name` | Round-trip del **documento**: `Id = newId`, `Name = copyName` | `SelectiveKindHandler.cs:79-80`; `RestampResult.cs:57-83` (l.71-74) |
| `cantilever` | `CantileverLineDesign.Id` (Guid) + `Name`: «one GUID per LINE, shared by its three views» | `Id = Guid.TryParse(newId)` o uno nuevo; `Name = copyName` | `CantileverKindHandler.cs:73-75`, `:80-110` (l.105-106) |
| `cabecera` | Solo `Header.Name`, sin id | `Header.Name = copyName` | `CabeceraKindHandler.cs:44-66` (l.64) |
| `dynamic`, `cama`, `pushback` | Ninguna | JSON intacto | `DynamicKindHandler.cs:49`, `CamaKindHandler.cs:52`, `PushBackKindHandler.cs:75` |

La coherencia entre identidad exterior e interior esta garantizada **dentro de una llamada**
(`RackEnvelopeRestamp.cs:36-39`), **no entre llamadas**: dos llamadas producen dos GUID distintos.

### 5.4 Como se encuentran las hermanas

- `RackCommandSupport.FindRackBlocks(document, rackId)` (`RackCommandSupport.cs:109-134`) recorre
  definiciones con `RackBlockFinder.ScanEnvelopes` y compara `Id` con **`OrdinalIgnoreCase`** (l.124).
  Lo usan los `RACKEDITAR` de Selectivo (`RackSelectivoCommands.cs:125`), Dinamico
  (`RackDinamicoCommands.cs:178`), Push Back (`RackPushBackCommands.cs:184`), Cantilever
  (`RackCantileverCommands.cs:234`) y Cabecera (`RackCabeceraCommands.cs:238`); tambien `RACKLAYOUT`
  (`RackLayoutCommands.cs:71`) y el ejecutor de variables (`ProjectVariableMutationExecutor.cs:147`).
  **La Cama no**: su `RACKEDITAR` redibuja solo la definicion elegida (`RackCamaCommands.cs:202-238`).
- `RackBlockFinder.ScanEnvelopes` (`RackBlockFinder.cs:57-91`) salta layouts, anonimos y xrefs
  (l.66-69); el conteo de referencias sale del **registro**, nunca del payload (l.79-86).
- Agrupan con el mismo criterio `RackListBuilder.Build` (`RackListBuilder.cs:55-57`), `RACKLISTA`
  (`RackInventarioCommands.cs:40-72`) y `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs:73-114`).
- Vistas por sistema (`docs/guias/validacion-manual-autocad.md:115-121`): Selectivo, frontal por fondo
  (`Section` = fondo), cortes laterales por poste (`Section` = poste) y planta; Dinamico, laterales por
  poste, frontales de salida y entrada, y planta; Cabecera, lateral y planta. En Push Back la seccion
  frontal codifica lado y extremo (`PushBackRoundTripSourceGuardTests.cs:225-231`); Cantilever tiene
  tres vistas (`CantileverKindHandler.cs:73-75`).
- `RACKEDITAR → Insertar` crea **otra definicion** con el mismo GUID (`RackSelectivoCommands.cs:246-259`),
  asi que un rack puede tener dos definiciones de la misma (View, Section).

### 5.5 Historicamente: solo la vista clicada

Tres evidencias independientes:

1. **Codigo.** Se toma `reference.BlockTableRecord` de la entidad elegida (l.164) y se clona **esa**
   definicion (l.197). `RackDuplicarCommands.cs` no llama a `FindRackBlocks`.
2. **Historia** (`git log --follow`). `1547254` (2026-07-09): «Duplicates the clicked block's view».
   `31a608e` (2026-07-14), estilo COPY: «CLONA la definicion de la vista clicada». Despues, solo
   refactors y robustez: I-09 (`e3d3542`, `b2d0428`, `25e58b8`, `6a4cc3f`), post-I-10 (`7211027`), I-18b
   (`2ff0994`), I-23 (`99c7350`) e I-47 G14 (`31cbb79`). Ninguno cambio el alcance de vistas.
3. **Documentos.** Decision del 2026-07-09 en `docs/ideas-futuras.md:129-131` («duplicar SOLO la vista
   clicada ... no se duplican todas las vistas»); `docs/guias/despliegue.md:276` («Duplica la vista
   clicada»); `docs/initiatives/I-47-proposal-v4.md:2527-2529` («las vistas hermanas del mismo rack no se
   copian»).

Consecuencia vigente: **cada invocacion produce un rack de una sola vista**. Duplicar dos vistas de `A`
en dos invocaciones da **dos racks distintos**, con dos GUID, no dos vistas de una misma copia.

## 6. JSON authored y bindings de I-47/I-48

- El Selectivo re-estampa a traves del **documento**, no del dominio (`RestampResult.cs:57-83`):
  sobreviven el literal congelado, `PropertyValues` (los bindings), la `SchemaVersion` promocionada
  (`2.0`) y `ExtensionData`. Lo prueban `SelectiveDuplicationFailClosedTests.cs:90-149`.
- **C-4 de I-47**: la copia conserva el `VariableId` (`docs/automation/decisions/I-47.md:86`). «The
  rack's identity changes; the variable's does not» (`RestampResult.cs:51-55`).
- **I-47 G14**: el fallo de restamp es un valor y se mira antes de clonar (`RackEnvelopeRestamp.cs:13-23`,
  `RackDuplicarCommands.cs:187-195`).

**Lo que la seleccion multiple anade y hoy no existe: una copia con varias vistas.** Tres autoridades
comparan las vistas de un rack Selectivo y exigen igualdad estructural del documento persistido,
**incluidos `Id` y `Name`** (son propiedades publicas del DTO, sin `JsonIgnore`):

- `SelectiveAuthoredAuthority.IsSameAuthority` / `Resolve` (`SelectiveAuthoredAuthority.cs:96-157`):
  compara el arbol completo; `SchemaVersion` y `ExtensionData` tambien participan (l.72-77).
- `BomAuthoredAuthority.Resolve` (`BomAuthoredAuthority.cs:84-125`): un rack divergente **queda fuera**
  de `RACKBOMTOTAL`, con aviso (`RackInventarioCommands.BomTotal.cs:150-157`). Para los kinds que no son
  Selectivo aprueba la primera vista sin comparar (l.106-110).
- `ProjectVariableConsumerDiscovery.DiscoverConsumers` (`ProjectVariableConsumerDiscovery.cs:100-155`):
  si un rack consumidor tiene vistas que discrepan sobre el vinculo (l.137-142) o vistas divergentes
  (l.144-149), **aborta la operacion de la variable para todo el dibujo**.

⇒ **Invariante que I-51 tiene que crear**: todas las vistas de una copia se re-estampan con el mismo
`(newId, copyName)` a partir de documentos equivalentes. Un GUID o un nombre por vista fabrican una
copia que **nace divergente**.

## 7. Transformacion: posicion, rotacion, escala, capa y UCS→WCS

| Aspecto | Hoy en `RACKDUPLICAR` | Cita |
|---|---|---|
| Punto base y destino | `GetPoint` en coordenadas del **UCS actual**, con liga elastica desde el base | l.60-61, l.74-84 |
| Desplazamiento | `(destino − base).TransformBy(CurrentUserCoordinateSystem)`: un **vector**, solo parte lineal. Llego en `31a608e` como hallazgo 1 de su revision | l.105-107 |
| Posicion | `source.Position + displacement`, en **WCS**; el clon conserva el `Origin` del BTR | l.108; `RackCloner.cs:34` |
| Rotacion y escala (espejo incluido) | Copiadas de la referencia de origen | l.203-204 |
| Capa | Copiada: paridad con COPY, hallazgo 2 de `31a608e` | l.205 |
| Espacio | **Siempre Model Space**, aunque el origen este en un layout | l.199-200 |
| **No se copia** | `Normal` —y con ella el OCS—, color, tipo y escala de linea, grosor, transparencia, visibilidad, XData y diccionario de extension **de la referencia**, atributos | l.201-206 |

Para varios origenes, la semantica COPY es directa: **un solo desplazamiento** para todas las
referencias, cada una con su propia `Position`, `Rotation`, `ScaleFactors` y `LayerId`. Nada de lo que
el codigo calcula hoy depende de que el origen sea uno.

## 8. Transacciones y atomicidad actuales

| Paso | Transaccion | Commit | Si falla |
|---|---|---|---|
| Snapshot de la seleccion | Una, de lectura (l.159) | Si | Nada que deshacer |
| Cada destino | Una por destino (l.185) | Si, al terminar el destino | Excepcion sin commit: ese destino no deja nada; el `catch` del comando (l.125) **termina el bucle**, y los destinos anteriores **quedan** |
| Regen | Ninguno | — | — |

Precedentes de atomicidad por lote en el repositorio:

- `RACKLAYOUT` coloca toda la rejilla en **una** transaccion y un restamp fallido deshace el lote entero
  (`RackLayoutCommands.cs:219-264`, comentario l.234-235).
- El ejecutor de variables separa **PREPARE** puro, **MUTATE** en una transaccion y **POST** (purga y un
  Regen) (`ProjectVariableMutationExecutor.cs:138-297`).
- La decision C2-6 de I-47 exige «preflight completo antes de mutar, coherencia conjunta, commit unico o
  rollback real» (`docs/automation/decisions/I-47.md:113`).

**No se verifico en G1** si `UNDO` revierte el comando completo de una vez: requiere AutoCAD.

## 9. Modo Multiple/Unica y nombres

- `multiple = true` por defecto (l.69). `[Unica]` hace que el **siguiente** punto coloque una copia y
  termine; `[Multiple]` lo revierte (l.81-93). Enter o Esc terminan (l.95-98).
- El nombre sale del **ordinal de colocacion dentro de la invocacion**: «&lt;base&gt; - copia» y
  «&lt;base&gt; - copia N» (l.100-103). Otra invocacion vuelve a empezar en «- copia», asi que los nombres
  pueden repetirse; no son identidad (Context Pack `persistence`: «el nombre visible no es identidad
  estable»).
- El BTR se llama como `copyName`, saneado y con `" (n)"` si ya existe (`RackCloner.cs:71-87`). La copia
  de un corte lateral se llama «X - copia», no «X - copia - lateral 3», hasta que el primer `RACKEDITAR`
  de la copia la sincroniza (`RackBlockRenamer.SyncName`, p. ej. `RackSelectivoCommands.cs:205-206`).

## 10. RACKLAYOUT: reutilizacion real

| Pieza | ¿Compartida con `RACKDUPLICAR`? | Cita |
|---|---|---|
| `RackEnvelopeRestamp.RestampEnvelope` | **Si**, en copias independientes | `RackLayoutCommands.cs:236` |
| `RackCloner.CloneDefinition` | **Si**, en copias independientes | `RackLayoutCommands.cs:243` |
| Gate `KindHandlerDispatch.TryResolveIgnoreCase(editor, embed.Kind, out _)` | **Si**, literal | `RackLayoutCommands.cs:64` |
| Selector | No: `PickRackBlock` | l.49 |
| Hermanas | `FindRackBlocks` **solo** para localizar la **planta**, de la que toma una referencia semilla | l.71, l.182 |
| Transformacion | Posiciones **absolutas** en WCS del planificador; `Rotation` y `ScaleFactors` del semilla; **Z = 0**; **sin capa** (usa la actual); sin UCS | l.250-254 |
| Transaccion | **Una** para toda la rejilla; un restamp fallido deshace el lote | l.219-264 |
| Regen | Uno al final | l.266 |
| Copias enlazadas | Referencian la **misma** definicion, sin clon | l.230 |
| `RACKRELLENAR` | Solo enlazadas: sin restamp ni clon | `RackLayoutCommands.Fill.cs:454-457` |

**Conclusion**: la reutilizacion real son **dos helpers** (`RestampEnvelope` y `CloneDefinition`) y el
gate de kind. No hay flujo, seleccion, transformacion ni multi-vista que heredar de `RACKLAYOUT`. Como
su llamada usa la firma de dos argumentos, **I-51 no necesita tocar `RackLayoutCommands.cs`** si la
asignacion del id entra como sobrecarga.

## 11. Copias enlazadas y multiples referencias del mismo RackId: riesgos

| # | Riesgo | Por que es real | Mitigacion para G2 |
|---|---|---|---|
| R1 | **Conteo de copias alterado** | `RACKLISTA` y `RACKBOMTOTAL` cuentan copias como el **MAX de referencias directas** entre las vistas de un RackId (`RackInventarioCommands.cs:72`; `RackInventarioCommands.BomTotal.cs:109-113`). Si dos referencias de la misma definicion se clonan en **dos** definiciones con el mismo RackId nuevo, el rack pasa de «2 copias» a «1» | Deduplicar por **definicion de origen** dentro del grupo: un clon por definicion y N referencias al clon |
| R2 | **Copia divergente** (Selectivo) | §6: un GUID o nombre por vista, o vistas de origen ya divergentes, dejan la copia fuera del BOM y **abortan**, en todo el dibujo, las operaciones de las variables que la copia consume | Un `(newId, copyName)` por grupo; preflight de igualdad authored de las vistas seleccionadas de cada grupo Selectivo |
| R3 | **Copia parcial** | Lo historico es copiar lo seleccionado, no las hermanas (§5.5). Un rack copiado sin laterales es valido: `RACKEDITAR` redibuja las vistas que existan | Decision de producto PD-1 **[G2-D6]** |
| R4 | **Ambiguedad de intencion** | Dos referencias de `A` pueden ser dos racks fisicos enlazados. ¿Su copia son dos racks enlazados, con un RackId, o dos independientes? | Decision PD-2; la exigencia «las hermanas de A comparten UN RackId» apunta a enlazados **[G2-D6]** |
| R5 | Referencias de `A` **no** seleccionadas | No se tocan: el clon es un BTR nuevo (`RackCloner.cs:34-36`) y el payload de origen no se escribe | Ninguna: conservar |
| R6 | Dos definiciones con la misma (View, Section) bajo `A` | Posible por «Insertar» (§5.4); `RACKEDITAR` las redibuja todas | Reproducirlas tal cual, sin fusionar |
| R7 | Sobre sin `Id` (legado) | Hoy se duplica; agruparlo exigiria inventar un RackId, y eso esta prohibido (`ProjectVariableScanProjection.cs:17-21`) | Decision PD-6 **[G2-D6]** **[G2-D2]** |
| R8 | Mismo RackId con `Kind` distinto en dos definiciones | Corrupcion; con una sola vista no podia manifestarse | Abortar en el preflight |
| R9 | Referencia en espacio papel | La copia cae en Model Space (§7) | Decision PD-7 **[G2-D6]** |
| R10 | Rack ilegible confundido con «no es rack» | Mismo mensaje (§4) | Clasificar en el preflight y abortar si una referencia elegida es un rack ilegible |
| R11 | Cama con varias definiciones del mismo RackId | Su `RACKEDITAR` no busca hermanas (§5.4): dos definiciones no se editan juntas | Deduplicar por definicion reproduce la estructura del origen y no crea definiciones de mas |

## 12. Respuestas de diseno (insumo para G2, no contrato)

### 12.1 Representacion de la seleccion fisica multiple

Tres niveles de datos planos. Ningun objeto de AutoCAD sale de la transaccion
(`InDocumentTransaction.cs:11-15`).

```text
SeleccionFisica = [ ReferenciaSeleccionada ]          una por BlockReference elegida, unica por handle
  ReferenciaSeleccionada { ReferenceHandle, DefinitionHandle, Position, Rotation, Scale, LayerId, ... }

Plan (puro, en Application)
  GrupoPorRack { OldRackId, Kind, BaseName }           clave RackId con OrdinalIgnoreCase, como FindRackBlocks
    DefinicionOrigen { DefinitionHandle, Payload }     deduplicada por handle
      Referencias [ ReferenceHandle ... ]              TODAS las elegidas: ninguna se descarta al agrupar
```

> **[G2-D2]** `OldRackId` no sirve de clave: los sobres legacy no tienen `Id`. La clave es un valor
> discriminado, `RackId` o `Definition(handle)`, y el handle vive solo dentro del lote.

- **Agrupar por RackId** responde a «un RackId por rack origen»; **deduplicar por definicion**
  responde a R1; **conservar cada referencia** responde a «sin perder las vistas realmente
  seleccionadas».
- Las claves son el `Handle` como texto, la convencion que ya usan las proyecciones puras
  (`RackSelectivoCommands.cs:291`, `RackInventarioCommands.BomTotal.cs:80`).
- La clasificacion puede reutilizar `ProjectVariableScanProjection.Project`
  (`ProjectVariableScanProjection.cs:30-65`), que nunca inventa un RackId y ya distingue sobre
  ilegible, rack de otro kind y Selectivo legible. **[G2-D1]** Incorrecto para I-51: esa proyeccion
  trata un sobre sin `Id` como ilegible (l.37-42) y dejaria fuera los legacy que PD-6 incluye.

### 12.2 Un RackId nuevo compartido por las hermanas copiadas de A

- **Hoy es imposible sin cambio**: el GUID nace dentro de `RestampEnvelope` (`RackEnvelopeRestamp.cs:36`)
  y cada llamada crea uno.
- El cambio minimo es que el **llamador** asigne el id: una sobrecarga
  `RestampEnvelope(payload, copyName, newId)`, con la firma actual delegando en ella con un GUID nuevo,
  de modo que `RACKLAYOUT` no cambia. La mitad interior ya lo admite:
  `IRackKindHandler.RestampDesign(designJson, newId, copyName)` (`IRackKindHandler.cs:75`).
  **[G2-D3]** La entrada recibe `Guid newId`, no un string, con NI-1..NI-6 del contrato.
- Cada definicion se re-estampa desde **su propio** payload, para conservar su `View`, su `Section` y su
  `ExtensionData` por vista (`RackEmbedDocument.cs:41-64`), pero con el `(newId, copyName)` **del
  grupo**. No conviene componer un sobre nuevo desde un diseno comun: perderia los metadatos por vista.
- La **asignacion** `(grupo, punto) → (newId, copyName)` debe vivir en codigo **puro y probado**. El
  Plugin no se carga en las suites (ADR-0003), asi que un invariante que solo existiera en su bucle solo
  podria vigilarse con guardas de texto.

### 12.3 ¿Hace falta `OldRackId → NewRackId`?

> **[G2-D4]** Se sustituye por una **asignacion** en el plan de cada destino,
> `{LogicalSourceKey, NewRackId, CopyName}`, sin estructura `OldRackId → NewRackId`. La correspondencia
> definicion → clon es local a la transaccion del Plugin. Nada se persiste ni se remapea.

**Si, como tabla efimera por punto de destino.** Es lo que hace que las N definiciones de un grupo
reciban el mismo id y que dos grupos no compartan nunca uno. Va acompanada de
`DefinitionHandle de origen → definicion clonada`, que resuelve R1.

**No, para reescribir contenido.** Las referencias persistidas de I-47/I-48 apuntan a `VariableId`
(C-4), e **ID21 —referencias a propiedades de otros racks— no esta implementado**
(`docs/HANDOFF.md:1524`). Esto **cambia si llega ID21**, que I-47 preve como `RackId + PropertyId`
(C2-2, `docs/automation/decisions/I-47.md:109-110`): copiar un grupo obligaria entonces a decidir si una
referencia de la copia de A a B apunta a la copia de B o sigue apuntando a B. Es AM-3.

### 12.4 Atomicidad recomendada y su alcance

1. **Preflight sin escribir**, antes del punto base: snapshot de toda la seleccion en una lectura;
   clasificacion en no-rack, rack ilegible y rack; gate de kind **por grupo** con
   `TryResolveIgnoreCase` —no con `TryResolveAll`, que es sensible a mayusculas
   (`KindHandlerDispatch.cs:45-58` frente a `:31-43`)—; igualdad authored de las vistas seleccionadas de
   cada grupo Selectivo; y un **ensayo de restamp** por definicion distinta. El restamp es determinista
   salvo por el GUID y el nombre, que no pueden hacerlo fallar, asi que un ensayo que sale bien prueba
   que saldra bien. Cualquier fallo: **cero escrituras**.
2. **Un punto de destino = una transaccion** que cubre todos los grupos, definiciones y referencias del
   punto: asignar ids y nombres → re-estampar todo → si algo falla, salir **sin commit** → clonar →
   anadir referencias → commit. Nunca queda una copia con media vista ni con vistas de ids distintos.
3. **Entre puntos, commits independientes.** Es la semantica COPY actual —cada clic se ve— y mantener
   una transaccion abierta a traves de peticiones interactivas no tiene precedente en el repositorio.
4. **Alcance**: el grupo es indivisible dentro del punto y el punto es indivisible; el comando entero
   no lo es (decision PD-4). Sin Regen, como hoy. Sin tocar el registro de variables.

### 12.5 N racks × M puntos

```text
RackIds nuevos      = M × N
Definiciones nuevas = M × Σ_g (definiciones distintas del grupo g)
Referencias nuevas  = M × Σ_g (referencias seleccionadas del grupo g)
Nombre              = BaseName_g + " - copia" en el punto 1, o + " - copia k" en el punto k
                      (UN nombre por grupo y punto, compartido por todas sus vistas)
```

Con `Unica`, `M = 1`. El coste dominante es un `DeepCloneObjects` por definicion y punto; las
definiciones anidadas ARRAY se comparten, no se duplican (`RackCloner.cs:46-49`).

## 13. Conflictos con I-49 e I-50

### 13.1 Estado observado

- **I-50** (`feature/cotas-independientes-por-vista`): **activa**, con reclamo `97cc0ef`, contrato
  `4e22d91`, fila de ROADMAP `a2fba4f` y worktree creado a las 13:05. **Sin codigo**: su contrato deja
  G1-G8 pendientes.
- **I-49**: **sin rama remota** en los dos chequeos (13:06 y 13:19) y sin mencion en `main`. La unica
  fuente es el contrato de I-50 (§6 y §11), que la asocia a `LinkedPropertyEditor`, Expression Engine y
  Project Variables. **Su alcance real no se conoce.**
- **[G2-D5]** Estado posterior a G1: **I-49** reclamada (`77262fe`) y bootstrapeada (`f2d28a2`); su
  contrato extiende `ProjectVariable.Definition` (ADR-0034 §15), **excluye ID21** (§4) y reconoce a I-51
  sin archivo productivo previsto en comun (§6.4). **I-50** cerro G1 en `fdaf2bc`: autoridad rack × tipo
  de vista en el diseno (`CD-01`), metadata por instancia rechazada (`CD-08`), e I-51 solo con conflicto
  documental de ROADMAP (`CD-11`). Ninguna de las dos toca `src/` ni `tests/`.

### 13.2 Archivos

| Area | I-51 (previsto, no fijado) | I-50 (inferido de su contrato) | Cruce |
|---|---|---|---|
| `src/RackCad.Plugin/RackDuplicarCommands.cs` | Si | Improbable | No previsto |
| `src/RackCad.Plugin/RackEnvelopeRestamp.cs` | Si: sobrecarga | Solo si guarda politica en el sobre **y** decide tocarlo; el round-trip ya preserva campos | **Posible** |
| `src/RackCad.Plugin/*Commands*.cs` (grupo caliente, WORKFLOW §7) | Solo `RackDuplicarCommands.cs` | Caminos de insercion y edicion por sistema | **Mismo grupo, archivos distintos** |
| Application: planificador nuevo | Archivo nuevo | — | No |
| `RackEmbedDocument.cs`, `RackEmbedComposer.cs`, DTOs de diseno (`Dimensions`) | No | Probable | No |
| `SelectiveDuplicationFailClosedTests.cs`, `PushBackRoundTripSourceGuardTests.cs` | Si: reapuntar guardas | Improbable | No previsto |
| `src/RackCad.UI/RackCommandReference.cs` | Texto de ayuda | Improbable | No previsto |
| `docs/ROADMAP.md` | Fila | Fila | **Si, textual**: las dos filas estan insertadas tras I-48 |
| `docs/guias/despliegue.md`, `README.md` | Fila del comando | Posible | Posible, textual |

Frente a I-49: **ningun archivo previsto en comun**. I-51 no toca `LinkedPropertyEditor`, expresiones
ni el registro de variables.

### 13.3 Acoplamientos semanticos (pesan mas que los de archivo)

- **I-50 → I-51.** Si la politica de cotas por vista vive en la **definicion** (sobre o diseno), clon y
  restamp la transportan sin cambios. Si vive en la **referencia** (XData o diccionario de extension de
  la `BlockReference`), la copia actual **la pierde** (§7) y la vista copiada mostraria cotas distintas
  de su origen. Conviene que el G2 de I-50 declare donde vive **antes** del G2 de I-51 (AM-4).
  **[G2-D5]** Declarado en `fdaf2bc` (`CD-01`, `CD-08`): vive en el diseno. AM-4 queda cerrada como no
  material mientras ese contrato siga vigente.
- **I-49 → I-51.** Si I-49 introduce **ID21**, la duplicacion de grupos necesita politica de remapeo
  (AM-3). **[G2-D5]** El contrato de I-49 (`f2d28a2`, §4) excluye ID21.
  Si cambia la representacion de los bindings en `SelectivePalletDesignDocument`, el restamp
  debe seguir preservandola; hoy lo vigilan las pruebas de C-4
  (`SelectiveDuplicationFailClosedTests.cs:109-117`).

**Recomendacion**: no declarar todavia `conflicts_with` en el contrato; **serializar el G3+ de I-51
despues del G2 de I-50**, o acordar que I-50 no modifica `RackEnvelopeRestamp.cs`. Queda reportado aqui,
como pide el contrato (§12).

## 14. Piezas pequenas reutilizables, sin framework general

| Pieza | Tipo | Por que basta |
|---|---|---|
| `RackEnvelopeRestamp.RestampEnvelope(payload, copyName, newId)` **[G2-D3]**: `Guid newId` | Sobrecarga en el Plugin | La mitad interior ya recibe `newId`; la firma de dos argumentos delega y `RACKLAYOUT` queda intacto |
| Planificador puro de duplicacion: una clase estatica y dos o tres registros inmutables | Nuevo, en Application | Agrupa por RackId, deduplica definiciones, conserva referencias, clasifica y asigna `(newId, copyName)` por (grupo, punto). Precedente de forma: `RackListBuilder` (`RackListBuilder.cs:44-79`) |
| `ProjectVariableScanProjection.Project` | Reutilizar **[G2-D1]**: NO para clasificar fuentes de I-51 | Clasifica sin inventar RackId |
| `SelectiveAuthoredAuthority.IsSameAuthority` | Reutilizar | Igualdad authored de las vistas de un grupo, sin comparador nuevo |
| `KindHandlerDispatch.TryResolveIgnoreCase` | Reutilizar, por grupo | Mantiene la sensibilidad a mayusculas historica |
| `RackCloner.CloneDefinition` | Reutilizar sin cambio | Ya clona una definicion con su payload y su etiqueta |
| `InDocumentTransaction.Run` | Reutilizar, una por punto | El cuerpo es de una fase: re-estampar, clonar, anadir |

Fuera, expresamente: un registro de operaciones, una seleccion generica para otros comandos, un «motor»
de transformaciones, cambios en `IRackKindHandler` y unificar el flujo con el de `RACKLAYOUT`.

## 15. Guardas de fuente y pruebas que el cambio tocaria

Guardas de **texto** sobre `RackDuplicarCommands.cs`, porque el Plugin no se carga en las pruebas:

- `SelectiveDuplicationFailClosedTests.GUARDA_RACKDUPLICAR_DECIDE_ANTES_DE_CLONAR` (l.247-254):
  `RestampEnvelope(` antes que `RackCloner.CloneDefinition`, y `IsSuccess`.
- `SelectiveDuplicationFailClosedTests.GUARDA_NINGUN_CAMINO_DE_COPIA_CAE_AL_PAYLOAD_DE_ORIGEN`
  (l.265-277): los literales `source.Payload, copyName)` y
  `CloneDefinition(database, transaction, source.DefinitionId, copyName, source.Payload`.
- `PushBackRoundTripSourceGuardTests.CopyAndLayout_AcceptPushBackViaIgnoreCaseLookup_WithNoPerKindBranch`
  (l.299-308): el literal `KindHandlerDispatch.TryResolveIgnoreCase(editor, embed.Kind, out _)`.

Sobre `RackEnvelopeRestamp.cs`: `GUARDA_EL_RESTAMP_COMPARTIDO_NO_TIENE_MEJOR_ESFUERZO`
(`SelectiveDuplicationFailClosedTests.cs:235-245`) y
`RackEnvelopeRestamp_ResolvesViaRegistryIgnoreCase_NoPushBackBranch`
(`PushBackRoundTripSourceGuardTests.cs:292-297`).

Una sobrecarga y un bucle por grupos pueden romper esos literales **sin** romper la propiedad que
protegen. G2 debe **reapuntarlas a la propiedad** —como hizo I-47 G14 con la guarda de Push Back,
`PushBackRoundTripSourceGuardTests.cs:119-127`—, nunca borrarlas.

## 16. Decisiones que G1 no puede tomar

### 16.1 Decision arquitectonica material: **SI, para revision de Arquitecto antes de G2**

- **AM-1 — Unidad de identidad de una copia.** Pasa de «una definicion = un RackId nuevo por llamada al
  restamp compartido» a «un rack origen × un punto = un RackId nuevo compartido por N definiciones».
  Saca la asignacion del GUID del helper que comparten `RACKDUPLICAR` y `RACKLAYOUT`
  (`RackEnvelopeRestamp.cs:36`) y **extiende el invariante indivisible de I-47 G14** de una definicion a
  un grupo y a un punto. Toca el mismo contrato que I-10 describio como «una sola generacion de GUID por
  copia independiente» (`docs/initiatives/I-10-kind-handlers.md:224-231`).
- **AM-2 — Estructura enlazada dentro de la copia.** Deduplicar por definicion de origen fija el conteo
  de copias de `RACKLISTA` y `RACKBOMTOTAL` y la autoridad authored entre hermanas; la alternativa lo
  cambia en silencio (R1). Es una decision de **identidad y de BOM**, no de UI.
- **AM-3 — Alcance del mapa `OldRackId → NewRackId`.** Hoy basta efimero y sin reescribir contenido;
  con ID21 (C2-2) haria falta una politica de remapeo. Conviene decidir de forma explicita que el mapa
  **no** reescribe contenido, para que ID21 no herede un supuesto implicito.
- **AM-4 — condicionada a I-50.** Transformar copiando propiedades, como hoy, o clonando la referencia y
  desplazandola, con paridad COPY de `Normal`, color, XData y diccionario de extension. Pasa a material
  si I-50 guarda politica por vista en la referencia.

> **[G2]** Revision de Arquitecto `AGREED WITH CHANGES`: AM-1, AM-2 y AM-3 **reconciliadas**; AM-4
> **cerrada como no material** por la decision vigente de I-50. Detalle en
> [`decisions/I-51.md`](../automation/decisions/I-51.md) seccion 3.3.

**No** son decisiones arquitectonicas nuevas, porque aplican precedentes existentes: el preflight con
transaccion por punto (C2-6, el ejecutor de variables, `RACKLAYOUT`), la reutilizacion de autoridades
puras y la ausencia de un registro nuevo.

### 16.2 Decisiones de producto para el dueno

> **[G2-D6]** Las siete estan **cerradas**; decisiones y precisiones en
> [`decisions/I-51.md`](../automation/decisions/I-51.md) seccion 2. Las preguntas se conservan tal como se
> formularon en G1.

- **PD-1.** Seleccionar una vista, ¿copia solo esa vista, como decidio el dueno el 2026-07-09, o todas sus
  hermanas? *Recomendacion tecnica*: conservar lo historico.
- **PD-2.** Dos referencias de la misma definicion, ¿dan una copia con dos referencias enlazadas y un
  RackId, o dos racks independientes? *Recomendacion tecnica*: enlazadas (AM-2).
- **PD-3.** Entidades que no son rack en la seleccion: ¿se ignoran con aviso o se rechaza la seleccion?
- **PD-4.** Si falla el punto k > 1: ¿se conservan los anteriores, como hoy, o se deshace todo?
- **PD-5.** Nombres con N racks × M puntos: ¿se mantiene «- copia» / «- copia k» por ordinal de punto?
- **PD-6.** Sobres sin `Id` (legado): ¿grupo propio por definicion, que es lo que pasa hoy, o rechazo?
- **PD-7.** Referencias en espacio papel: ¿se restringe a Model Space o se copia en su propio espacio?

Con estas decisiones abiertas, el contrato deberia pasar `requires_owner_decision` a `true` en G2 (la
metadata solo anade). **[G2]** Hecho.

## 17. Hallazgos laterales (no se arreglan en I-51)

- **L-1.** `RackBlockData.cs:8-11`: el `<summary>` dice «block reference's extension dictionary», pero
  todos los llamadores leen y escriben en la **definicion** (§5.1).
- **L-2.** `CantileverKindHandler.cs:70-78`: dos `<summary>` seguidos; la documentacion del restamp quedo
  pegada a `OutputBlockedReason` y `RestampDesign` no tiene la suya.
- **L-3.** `RackLayoutCommands.cs:250-254` y `RackLayoutCommands.Fill.cs:454-457`: las copias de
  `RACKLAYOUT` y `RACKRELLENAR` no conservan la **capa** del semilla y fijan **Z = 0**.
- **L-4.** `RackDuplicarCommands.cs:151`: `exactMatch: false` admite `MInsertBlock`, y la copia sale como
  referencia simple.

Estan **pendientes de registrar en `docs/ideas-futuras.md`**: este commit se limita al informe de G1, por
instruccion del dueno. El Discovery de I-48 tampoco toco `ideas-futuras.md`. **[G2]** Registrados sin
corregir en `docs/ideas-futuras.md`, seccion «I-51».

## 18. Conclusion G1

1. `RACKDUPLICAR` duplica hoy **una vista por invocacion**, con identidad nueva por copia y fail-closed.
   Su flujo es simple y **ya trabaja con desplazamiento**, no con posicion absoluta: extender la
   transformacion a varios origenes es natural.
2. Lo que **no** es natural es la **identidad**. El GUID nace dentro del helper compartido, y una copia
   multi-vista introduce por primera vez que N definiciones nazcan con el **mismo** id y el **mismo**
   nombre, bajo tres autoridades que abortan ante divergencia.
3. El riesgo principal no esta en AutoCAD sino en la **semantica de las copias enlazadas**: deduplicar
   por definicion es lo que conserva el conteo del BOM.
4. **Declaracion**: hay **decision arquitectonica material** (AM-1, AM-2 y AM-3; AM-4 condicionada a
   I-50), que necesita **revision de Arquitecto antes de G2**, y siete decisiones de producto (PD-1 a
   PD-7) para el dueno.
5. **G2 queda bloqueado** hasta esa revision. G0 y G1 no cambiaron codigo productivo.

> **[G2]** La revision llego (`AGREED WITH CHANGES`) y G2 la reconcilio: el contrato vinculante es
> [I-51-rackduplicar-multiples-origenes.md](I-51-rackduplicar-multiples-origenes.md).
