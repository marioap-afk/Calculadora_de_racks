# Mapa 5. La categoría de la purga (F4-T4B)

**Base medida:** HEAD `7c29a5e5` del worktree de arquitectura. El código de producto es idéntico a `c3ae1c97`.
**Método:** solo lectura de archivos y `git show`/`git log`. No hubo build, ni pruebas, ni AutoCAD.
**Pregunta del Owner:** «solo registro, salvo que el fallo de la purga deje en duda que el estado authored sea correcto. El paquete tiene que probar qué categoría aplica».

## Veredicto

**Categoría (b): es limpieza *best effort* de basura y no devuelve estado. Por tanto, solo registro.**

El veredicto **depende de un invariante de orden**. La purga corre en su propia transacción, después de que la transacción authored se confirma, y una sola vez. Si la purga compartiera la transacción authored, se anidara en ella o se intercalara entre las `WriteUnit`, la categoría pasaría a ser (a) (§2.3). Por eso el paquete tiene que declarar el orden como invariante y protegerlo con una guarda. La justificación actual de §9 I-5 («devuelve `void` y no se toca») describe el síntoma, pero no lo prueba.

## 1. Qué purga usan los caminos de redibujo

### 1.1 Las funciones (solo hay una implementación)

| Papel | Símbolo | Ubicación | Firma y retorno |
|---|---|---|---|
| Produce las candidatas (dentro de la transacción) | `LateralHeaderDrawer.RedefineSystemBlock` | `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:88-182` | `LateralHeaderDrawOutcome RedefineSystemBlock(Database, Transaction, ObjectId blockId, HeaderRunPlan, out IReadOnlyCollection<ObjectId> staleDefs)` |
| Las propaga | `SystemBlockWriter.RedefineInTransaction` | `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:126-138` | `out staleDefinitions`. Escribe el payload en `:136` |
| Las propaga | `ViewBlockDraw.RedrawInTransaction` | `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:97-109` | delegación directa |
| Fachadas de Push Back que usaría el piloto | `PushBack{System,Frontal,Planta}DrawService.RedrawInTransaction` | `Systems/PushBack/PushBackSystemDrawService.cs:64-66`, `PushBackFrontalDrawService.cs:62-64`, `PushBackPlantaDrawService.cs:56-58` | **No purgan.** Solo devuelven `staleDefinitions` |
| **Entrada única** | `SystemBlockWriter.PurgeAfterCommit` | `SystemBlockWriter.cs:147-150` | `internal static void PurgeAfterCommit(Database, IReadOnlyCollection<ObjectId>)`. Reenvía a la implementación |
| **Implementación** | `LateralHeaderDrawer.PurgeUnreferenced` | `LateralHeaderDrawer.cs:193-238` | `internal static void PurgeUnreferenced(Database db, IReadOnlyCollection<ObjectId> candidates)`, que devuelve **`void`** |

Usan la misma implementación, pero no como redibujo:
- `BlockPlacement.EraseUnreferencedDefinitionCore`, para la limpieza de un Insertar cancelado (`Drawing/BlockPlacement.cs:182-215`, `:213`);
- `RackCommandSupport.EraseViewBlocks` (`RackCommandSupport.cs:218-283`, `:274`).

`EraseViewBlocks` es un borrado **authored** de vistas obsoletas, y R-T4-11 lo prohíbe al piloto. Se distingue en §3.

### 1.2 Qué borra y cómo se eligen las candidatas

**Las candidatas se forman en la transacción authored** (`LateralHeaderDrawer.cs`):
1. **Antes de borrar el contenido de la vista:** cada `BlockTableRecord` referenciado por un `BlockReference` hijo directo del BTR de la vista, y el bloque `*D` de cada cota antigua (`:97-113`).
2. **Se resta** todo lo que referencia el contenido nuevo: las definiciones nuevas de cabecera (`:133`) y cada definición de catálogo que se vuelve a referenciar (`AppendInstance`, `:306`, llamado en `:137` y `:157`).
3. **El resultado** sale en `staleDefs` (`:177-178`). Son las definiciones ARRAY anidadas de la pasada anterior, con nombres `<prefijo>_<Rol>_<n>` (`src/RackCad.Application/Drawing/HeaderInstanceGrouper.cs:96`), los `*D` antiguos y las definiciones de catálogo o representaciones dinámicas que el contenido nuevo ya no usa.

**La purga**, en su propia transacción (`LateralHeaderDrawer.cs:202`):
1. Descarta las nulas o borradas (`:207-210`).
2. Solo admite `BlockTableRecord` que no sean layout (`:212-215`).
3. `db.Purge(ids)` reduce la colección a las definiciones **con 0 referencias** (`:220`).
4. Llama a `Erase()` **solo sobre esas** (`:221-227`).
5. Hace `Commit` (`:230`).

### 1.3 Transacción y excepciones

**La transacción:** siempre es propia (`:202`). Hoy todos los llamadores la invocan **después** del `Commit` authored y fuera de su `using`:

| Llamador | Commit | Purga |
|---|---|---|
| `SystemBlockWriter.RedrawInPlace` | `:87` | `:91` |
| `LateralHeaderDrawService.RedrawInPlace` | `Drawing/LateralHeaderDrawService.cs:141` | `:145` |
| `ProjectVariableMutationExecutor` | `ProjectVariableMutationExecutor.cs:312` | `:316` |
| El `post` de I-55 | `Systems/Shared/SiblingRedrawTransaction.cs:23` | `RackSelectivoInsertIntegration.cs:253`, vía `SiblingRedrawTransaction.cs:55-66` |

Hay dos guardas que fijan este orden, y otra que fija la entrada única:
- `tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs:197-211`: el commit va antes de la purga;
- `:214-231`: la purga va antes del regen;
- `:233-238`: la purga tiene una sola entrada.

**Ojo:** `StartTransaction` se **anida** si hay una `TopTransaction` abierta. «Propia» solo es verdad si se llama con `TopTransaction == null`.

**Las excepciones:**
- `catch (Exception ex)` en `:233-237`. El archivo no importa `Autodesk.AutoCAD.Runtime` (`:1-8`), así que es `System.Exception` y captura todo.
- Registra con `RackLog.Exception("Purga de definiciones anidadas", ex)`, que **nunca lanza** (`src/RackCad.Application/Diagnostics/RackLog.cs:23-24`).
- No relanza y no devuelve estado.
- La intención está documentada: «Best effort — a lingering def is preferable to failing the redraw» (`:191`, `:235`). Y en `SystemBlockWriter.cs:144-145`: «A failure here is a DIAGNOSTIC, not a semantic rollback».

### 1.4 ¿Puede tocar el resultado authored? No, por construcción

| Lo authored | Por qué la purga no lo toca |
|---|---|
| **El BTR de la vista** | Nunca es candidato, porque las candidatas son lo que la vista referencia, no la vista misma. Además, lo referencian sus copias en el espacio modelo, así que `Database.Purge` lo filtraría |
| **El payload y la identidad** (Xrecord `RACKCAD_SELECTIVE` con el GUID, el Name y el diseño) | Viven en el diccionario de extensión del BTR de la vista (`Systems/Shared/RackBlockData.cs:20-51`, escrito en `SystemBlockWriter.cs:136`). La purga solo llama a `BlockTableRecord.Erase()` sobre definiciones de basura; no abre ni modifica Xrecords, diccionarios, referencias ni nombres |
| **Las definiciones nuevas** (anidadas y de catálogo) | Las excluye dos veces: la resta de `:177`, y el filtro de 0 referencias de `:220`, porque el contenido nuevo ya confirmado las referencia |
| **Las vistas hermanas y los bloques de otros racks** | Cualquier definición que otro bloque referencie sobrevive al filtro (`:187-190`, `:220`). Una sola pasada de `Database.Purge` tampoco borra una definición referenciada desde otra definición no insertada |

## 2. Qué deja un fallo de la purga

### 2.1 Con el orden actual (después del commit, en su propia transacción)

| Caso | Efecto |
|---|---|
| Una excepción a mitad, en `GetObject`, `Purge`, el `Erase` k-ésimo o el `Commit` | Sale del `using` sin `Commit` y la transacción **de la purga** se aborta. **No persiste ningún borrado.** El repo ya se apoya en esa semántica (`ProjectVariableMutationExecutor.cs:293-295`; `Views/RackProjectionWriteScope.cs:15`). Después se registra |
| Una definición que sigue referenciada | `Database.Purge` la saca de la colección y no se borra. No es un fallo |
| Un borrado parcial | No llega a persistir. Aunque llegara, solo afectaría a definiciones de 0 referencias |
| El estado authored | Ya estaba confirmado en otra transacción **antes**, y no cambia |

**El estado resultante** es el authored confirmado más las definiciones obsoletas: **basura**.

**Lo que conviene declarar del residuo:**
- **Es permanente:** ninguna pasada posterior lo vuelve a recoger, porque las candidatas solo salen de los hijos actuales de la vista. Se queda hasta un PURGE manual.
- **Afecta a los nombres anidados:** fuerza sufijos `_1`, `_2` (`UniqueBlockName`, `LateralHeaderDrawer.cs:395-413`). Es cosmético.
- **Es invisible para los lectores de racks:** `RackSiblingScan` salta las definiciones sin payload (`Views/RackSiblingScan.cs:119-122`), y lo mismo hace `RackBlockFinder` (`RackBlockFinder.cs:71-74`). Las cuentas de referencias son sobre las definiciones de vista (`RackSiblingScan.cs:129-135`). Por tanto, el residuo no altera la relectura de POST ni la StaleBase.

### 2.2 Las semánticas de AutoCAD que se asumen

`Database.Purge` filtra por referencias, y `Dispose` sin `Commit` aborta. Son las semánticas en las que el código ya confía. Este mapa no las midió físicamente.

### 2.3 Si la purga compartiera la transacción authored: categoría (a)

**Precedente:** la forma anterior a `f2ab66bb` (2026-07-11) era exactamente esa:
- `PurgeStaleNestedDefinitions(tr, …)` dentro de `RedefineSystemBlock` (`git show f2ab66bb^:src/RackCad.Plugin/Headers/LateralHeaderDrawer.cs`, `:194` y `:208`);
- usaba `GetBlockReferenceIds(directOnly:false, forceValidity:true)`;
- y **no tenía `try/catch`**.

El commit `f2ab66bb` la sacó a «post-commit» («PERF-C — purga más barata y correcta»).

**Lo que pasaría con esa forma:**
1. **Una excepción sin capturar** abortaría **todo** el redibujo. Sería un fallo falso por una limpieza.
2. **Una excepción capturada** dejaría la transacción confirmable con borrados parciales.
3. **La corrección** dependería de cómo ven `Database.Purge` y `GetBlockReferenceIds` las referencias borradas pero no confirmadas, y las nuevas sin confirmar. El repo no lo mide.
4. **Intercalada entre `WriteUnit`** (es el único escenario de daño authored), una definición podría quedarse sin referencias tras la unidad k y hacerle falta a la unidad k+1. La unidad k+1 la tenía presente o importada en PREPARE. Si se borra antes de su `AppendInstance`, hay dos salidas:
   - la omite como faltante (`LateralHeaderDrawer.cs:293-303`), y la geometría queda incompleta;
   - o referencia un BTR borrado.

Con esa forma, el fallo **sí** podría dejar el rack mal escrito.

## 3. Cómo tratan hoy los fallos de la purga los caminos clásicos

### RACKEDITAR Push Back (`RackPushBackCommands.cs`)

**El recorrido:** cada vista pasa por `RedrawInPlace` de su fachada (`:279`, `:297`, `:336`), después por `ViewBlockDraw.RedrawInPlace` (`ViewBlockDraw.cs:114-143`) y después por `SystemBlockWriter.RedrawInPlace` (`SystemBlockWriter.cs:67-102`). Cada vista tiene su propia transacción y su `Commit` (`:83-88`). Después vienen la purga (`:91`) y `Success = true` (`:96`), **pase lo que pase con la purga**. Después, `SyncName` (`RackPushBackCommands.cs:282`, `:300`, `:339`), las cuentas y el mensaje de éxito (`:376-383`).

**Un fallo de la purga** solo queda en el registro.

**Los cortes obsoletos** van por `EraseViewBlocks` (`:349-352`). Es un borrado **authored**, y su fallo es **visible** (`RackCommandSupport.cs:277-280`). Su purga anidada (`:274`) vuelve a ser solo registro. El repo ya separa así los dos casos: el borrado authored informa y la basura se registra.

**La rama de Insertar de RACKEDITAR** (`:369-373`) no redibuja ni purga.

### Insertar (I-55)

**El recorrido:**
- **La aplicación** es `RedrawInTransaction`, dentro de `SiblingRedrawTransaction.Mutate` (`RackSelectivoInsertIntegration.cs:241-250`; `SiblingRedrawTransaction.cs:18-24`).
- **El `post`** es `PurgeAfterCommit` + `SyncName` (`RackSelectivoInsertIntegration.cs:251-255`), y corre después del commit (`AutoCadSiblingRedrawPort.cs:37-46`).
- **En el modo diferido**, se aplica dentro de la transacción del jig (`RackSelectivoInsertIntegration.cs:175-180`; `BlockPlacement.cs:251`), y el `post` va en `Complete`, después del commit (`Views/RackViewPlacement.cs:137-141`).

**La propia corrida** trata una excepción de `Post` como `Committed` con `POST_FAILED`, nunca como descartada (`src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:110-118`). La purga ni siquiera llega ahí, porque no lanza.

### RACKPROYECTAR

No redibuja ni purga: «no erase, purge or cleanup is written by hand» (`RackProjectionWriteScope.cs:12-16`). `RackSiblingScan` solo lee.

### El precedente caller-owned más cercano al piloto

`ProjectVariableMutationExecutor.cs:262-320`:
- junta la **unión** de las `staleDefinitions` de todas las vistas (`:263`, `:306-309`);
- hace **un** `Commit` (`:312`);
- purga **una vez**, fuera del `using` (`:316`);
- y devuelve `Applied` (`:319-320`), sin que la purga influya.

## 4. Conclusión y regla para el paquete T4B

### La prueba de la categoría (b)

| | Qué se prueba | Dónde |
|---|---|---|
| P1 | Las candidatas son solo definiciones referenciadas por el contenido **antiguo** de la vista, menos las que referencia el nuevo | `LateralHeaderDrawer.cs:97-113`, `:128-161`, `:177` |
| P2 | El filtro `Database.Purge` sobre el estado confirmado salva todo lo referenciado: el resultado authored, las vistas hermanas y los otros racks | `:220` |
| P3 | La purga solo llama a `BTR.Erase()` sobre basura de 0 referencias. Nunca toca el BTR de la vista, su payload ni su identidad | `:221-227`; `RackBlockData.cs:20-51` |
| P4 | Un fallo aborta solo la transacción de la purga, se registra y no se propaga. Deja authored + basura | `:200-237`; `RackLog.cs:23-24` |
| P5 | Ningún camino clásico convierte hoy un fallo de purga en fallo de la operación | §3 |
| P6 | La basura que queda es invisible para la relectura de racks | `RackSiblingScan.cs:119-135` |

### El texto propuesto para el paquete

Sustituye la justificación de §9 I-5 y la fila de Post de D-T4B-03.

**INV-PURGA (categoría b, solo registro):**
1. La purga de las `staleDefinitions` es limpieza de basura sin estado.
   - **No produce aviso, ni fallo, ni un «purgado» en el resultado.**
   - El desenlace (`Applied`, `AppliedWithWarnings` o `AppliedPostRereadFailed`) no depende de ella. Su fallo solo va a `RackLog`.
   - No se inventa ningún contrato de éxito o fallo, y `LateralHeaderDrawer` no se toca.
2. **El orden:**
   - el escritor llama **solo** a `SystemBlockWriter.PurgeAfterCommit`, que es la entrada única (nunca a `LateralHeaderDrawer.PurgeUnreferenced` directamente);
   - lo hace **solo en `Post`**, **una vez**, con la **unión** de las `staleDefinitions` de todas las `WriteUnit`;
   - y **después** de que `Seal` (`Commit()`) haya vuelto **y** de que se haya liberado el ámbito de mutación.
3. **Nunca:** en `WriteUnit`, en `Seal`, dentro del ámbito de mutación ni entre dos `WriteUnit`.
4. **La precondición de `Post`:** `TopTransaction == null`. Si no se cumple (por ejemplo, la fila «`Seal` vuelve y el `Dispose` de `m` lanza» de D-T4B-02), **se omite la purga y se registra**. No es un fallo, porque solo deja basura.
5. **Sin purga en las ramas NotApplied y CommitThrew**, porque `Post` solo corre con `sealedReturned`.
6. **La relectura de POST** (`CaptureDrawing`) va **después** de la purga, como ya ordena D-T4B-03. Así el comparador de D-T4B-05 observa el estado posterior a la purga.
7. **El residuo declarado:** si la purga falla, las definiciones obsoletas se quedan hasta un PURGE manual. Ningún redibujo posterior las recoge, y los lectores de racks no las ven. Es análogo al residuo OM-5 de la importación.
8. **La cláusula de cambio de categoría:** cualquier diseño (T4B o T4C) que haga compartir, anidar o intercalar la purga con la transacción authored la convierte en categoría (a). Entonces tendría que ir dentro de MUTATE, con su fallo como NotAppliedFailure sin `Seal`, y habría que medir su corrección dentro de la transacción (§2.3). **El paquete prohíbe esa forma**, en lugar de admitirla.

### Las guardas propuestas, textuales, sobre `RackPilotUpdateWriter.cs`

- `PurgeAfterCommit` aparece **solo** en el cuerpo de `Post`, y una vez.
- La clase del ámbito de mutación no contiene ni `PurgeAfterCommit`, ni `PurgeUnreferenced`, ni `.Purge(`.
- `Post` comprueba `TopTransaction` antes de purgar (se enlaza con §9 I-7).

### Una nota menor, sin peso

- **El orden clásico** es purga → `SyncName` (`SystemBlockWriter.cs:91`, y después `RackPushBackCommands.cs:282`; `RackSelectivoInsertIntegration.cs:253-254`).
- **D-T4B-03 lista** `SyncName` → purga.
- **No tiene efecto:** las definiciones anidadas llevan siempre el sufijo `_<Rol>_<n>` (`HeaderInstanceGrouper.cs:96`), así que no pueden ocupar el nombre objetivo de la vista.
- **Aun así**, alinear el orden con el clásico evitaría declarar una divergencia.
