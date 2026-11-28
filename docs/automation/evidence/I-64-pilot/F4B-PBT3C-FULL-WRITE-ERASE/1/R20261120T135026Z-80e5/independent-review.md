# Revisión independiente del código — I-64 F4B PB-T3c (`F4B-PBT3C-FULL-WRITE-ERASE`), intento 1

- WorkRunId `R20261120T135026Z-80e5`.
- BaseSha = ChainBaseSha `7dfc7c21`. RedSha `e0940a0e`. CurrentSha (GREEN) `938a35f0`.
- Sin reparación del RED: `rv/rr/` está vacío, `Deviations` está vacío y la cadena son dos commits. `e0940a0e` es hijo directo de `7dfc7c21`, y `938a35f0` de `e0940a0e`.
- Autoridades leídas en `938a35f0`:
  - el contrato `docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json`;
  - el paquete `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` v4.7: D-PBT3-11/12/14, §4, §4.1, §5.0, §5.4, §6.0 y §6.3;
  - la entrada de D-2 en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md:311-350`.
- Método:
  - el código, solo por objetos git desde el checkout principal;
  - la entrega, desde la copia `rv/`;
  - los TRX, leídos con Python `-I`;
  - los metadatos de las tres CI (RED, GREEN y la línea base `44707482956`), con `gh run view`;
  - los artefactos de la CI, descargados con `gh run download` a una carpeta temporal fuera del repositorio, para comparar sus hashes.
- No he compilado ni ejecutado pruebas, y no he leído el worktree de la rama.

## Veredicto

**ACEPTABLE CON MENORES.**

**Recuento: B 0 / I 0 / M 6.** Además, 3 hallazgos OWNER, que son riesgos del borrado real para el Smoke-PB y no defectos de la entrega.

El código hace lo que dice D-PBT3-11, punto por punto:
- el RED es exactamente el del contrato;
- el GREEN solo toca WR y la fachada;
- las guardas de F4 siguen en verde;
- los totales exactos de las dos CI coinciden con los previstos, en una sola rama.

Los menores son de precisión de las guardas nuevas y de dos líneas no prescritas, sin efecto observable.

## Tabla de hallazgos

| Id | Sev | Dónde (`938a35f0`) | Hallazgo | Sugerencia |
|---|---|---|---|---|
| PBT3C-IR-M01 | M | `tests/RackCad.Tests/PushBackViewEraseGuardTests.cs:175-182` | CT-01 fija «una clase y un solo método» contando `\b(?:internal\|private\|public\|protected)\b` (`declaraciones=2`), y `clase=`/`metodo=` solo por los dos nombres prescritos. Un método, un campo o un tipo declarado **sin** modificador de acceso (privado implícito) no lo ve. Hoy la lista cerrada de PBT3C-C13 la sostiene solo el censo de la entrega (`rv/inv/inv-final.txt`), no una guarda duradera. | Aceptar como límite declarado de CT-01, o en una tarea futura contar declaraciones por sintaxis y no por modificador. |
| PBT3C-IR-M02 | M | `PushBackViewEraseGuardTests.cs:359-371` y `:396-403` | **CT-04:** la regla «la llamada está dentro de `EraseUnit`» (`fuera-de-EraseUnit`, `:370`) no tiene una autoprueba que la aísle. Las tres autopruebas del §5.4 suben siempre el total a 2 (`llamadas=2`), así que «saltan» aunque esa rama estuviera rota. Una llamada **movida** de `EraseUnit` a otro método de WR (total 1) depende de una rama sin autoprueba. Además, el censo solo ve el literal `RackViewBlockEraser.EraseInTransaction(`: un `using static` o un grupo de métodos se le escapan. Cumple la letra del §5.4, que pide solo esas tres. | Registrar el límite. En una tarea futura, una cuarta autoprueba: la única llamada movida fuera de `EraseUnit`. |
| PBT3C-IR-M03 | M | `PushBackViewEraseGuardTests.cs:324` y `:330` | **CT-03, menos preciso de lo que dice su nombre:**<ul><li>`Failed-con-el-texto-de-hoy=2` se cuenta en todo `PrepareUnits`, no dentro del bucle de `plan.Erasures`;</li><li>`asignaciones=2` en `Reread` no comprueba que la comparación de las claves a borrar sea con `pair.Value` (el `ObjectId` resuelto).</li></ul>El código real sí lo hace (WR:117-118 y :277-278). | Solo registro. |
| PBT3C-IR-M04 | M | `tests/RackCad.Tests/PushBackWritePathPluginGuardTests.cs:206-216` y `:178-182` | **CT-05, autopruebas de las reglas:**<ul><li>usa 8 de los 15 ejemplos de CT-PBT1B-14 (a, b, c, e, f1, h1, i, j). Faltan f2, f3, g1, g2, h2, k1 y k2: g1/g2 son los `as`/`is` de la regla 1b, y k1/k2 los únicos que ejercen la regla 2b (alias + molde, la lección I01);</li><li>es el mismo subconjunto que CT-PBT3B-13, y el paquete no fija la lista;</li><li>las reglas son copia byte a byte de las originales: `diff` vacío entre `PushBackSharedReferenceGuardTests.cs:33-172` y `PushBackWritePathPluginGuardTests.cs:36-175`.</li></ul>`Inject` lanzaría `ArgumentOutOfRangeException` sobre un archivo ausente (`LastIndexOf('}', -2)`); no se alcanza, porque la fachada existe desde el RED. | Solo registro. |
| PBT3C-IR-M05 | M | `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:112`, `:114-121` y `:227` | **Tres decisiones del Worker que D-PBT3-11 no prescribe** (cambios de cuerpo, no declaraciones, así que C13 no las alcanza):<ul><li>`_erasures.Clear()` al empezar `PrepareUnits`;</li><li>el bucle de las claves a borrar **antes** del de los miembros;</li><li>`_erasures.Clear()` en `RackPilotUpdateScope.Dispose`.</li></ul>Las tres copian el patrón de `_prepared` y no cambian nada observable: el piloto lleva `Erasures` vacío (`PushBackPilotUpdatePlan.cs:290`). El orden está declarado (`KnownLimitations[1]`); la línea de `Dispose`, no. | Que el cierre las registre como decisiones de implementación sin efecto observable. |
| PBT3C-IR-M06 | M | `src/RackCad.Plugin/Views/RackViewBlockEraser.cs:9-12` y `:19-20`; `src/RackCad.Plugin/RackCommandSupport.cs:235-238` | La fachada no lleva el salto del clásico `definitionId.IsNull \|\| definitionId.IsErased → continue`. Con un id nulo o borrado, `GetObject` lanza una excepción de AutoCAD, no la `InvalidOperationException` del paso 1. Es **conforme al paquete**: el paso 1 de D-PBT3-11 no incluye ese salto, y no se alcanza, porque `Reread` exige `matches` para cada clave (UR:523-529). Pero el comentario de la clase dice «It does what the classic erasure does for one definition, in the same order», y eso no es exacto: hay dos diferencias, el `IsLayout`, que lanza (lo reconoce `:18`), y este salto. | Solo registro, para la entrada de D-2 al cerrar. |

## Alcance y forma (punto 1)

**Cadena** (`git diff --name-status`):
- `7dfc7c21..e0940a0e` añade 3 archivos: la fachada `src/RackCad.Plugin/Views/RackViewBlockEraser.cs` y las dos clases nuevas `tests/RackCad.Tests/PushBackViewEraseGuardTests.cs` y `PushBackWritePathPluginGuardTests.cs`. WR no cambia.
- `e0940a0e..938a35f0` solo toca WR (M) y la fachada (M).
- `7dfc7c21..938a35f0` son 4 rutas, exactamente el `AllowedWriteScope`.

**Esqueleto del RED, byte a byte:**
- el parche custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt3/pbt3c-skeleton.patch` tiene sha256 `a81de435e70d62a4…`, el que se esperaba;
- su línea `index 0000000..b4224b3` coincide con el blob `b4224b3b` de la fachada en `e0940a0e`, y el contenido es idéntico (15 líneas, solo `throw new NotSupportedException();`, sin comentario).

**INV-PBT3C-01:** `git diff 7dfc7c21 938a35f0 -- src/RackCad.Application src/RackCad.UI src/RackCad.Domain src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackCommandSupport.cs` está vacío (0 líneas). `tests/RackCad.UI.Tests` tampoco cambia.

**PBT3C-C13, mi propio censo leyendo `git show` de WR:**
- WR pasa de 40 a 43 declaraciones, y la fachada de 0 a 2;
- la diferencia son las cinco prescritas: `RackPilotUpdateScope._erasures` (:77), `RackPilotMutationScope._erasures` (:250), `RackPilotMutationScope.EraseUnit` (:323), la clase `RackViewBlockEraser` (:14) y `EraseInTransaction` (:16);
- el único cambio de firma es el constructor interno de `RackPilotMutationScope`, que gana `erasures` (:255-267). Tiene un solo uso, WR:169;
- coincide con `rv/inv/inv-final.txt`.

**Cambios de cuerpo en miembros existentes:**
- prescritos: `PrepareUnits`, `BeginMutation` y `Reread`, y los dos comentarios (:26-27 y :238-240);
- no prescritos: `Dispose` (:227) y los `Clear()` (M05).
- `Post` no cambia.

**Base:** `src/` y `tests/` de `7dfc7c21` son iguales a los de `fe1c67f4` y a los de `c7582785` (diff vacío).

## Diseño → código (D-PBT3-11), en `938a35f0`

| Punto del diseño | Código | Estado |
|---|---|---|
| Fachada, firma `internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)` en `internal static class RackViewBlockEraser` | RVBE:14-16 | Cumple |
| 1. `GetObject(definitionId, OpenMode.ForRead)` como BTR; nula o `IsLayout` → `InvalidOperationException` | RVBE:19-20 | Cumple (M06: sin el salto `IsNull`/`IsErased` del clásico, conforme al paquete) |
| 2. Captura de las anidadas (el `BlockTableRecord` de cada `BlockReference` hija), **antes** de borrar | RVBE:23-28 | Cumple |
| 3. `GetBlockReferenceIds(directOnly: true, forceValidity: true)`, cada una `ForWrite` y `Erase()` | RVBE:31-34 | Cumple |
| 4. `UpgradeOpen()` y `Erase()` de la definición | RVBE:36-37 | Cumple |
| 5. Devuelve las anidadas | RVBE:38 | Cumple |
| Sin `LockDocument`, `StartTransaction`, `StartOpenCloseTransaction`, `.Commit(`, `Purge`, `Regen`, `SyncName`, `PushBack`, ni nombrar el piloto | RVBE completo: 0 apariciones de cada término y `NamesThePilot` sin coincidencia, en texto crudo | Cumple (INV-04) |
| WR: `PrepareUnits` resuelve `plan.Erasures` en `_definitions` hacia `_erasures`; si falta, el `Failed` con el texto de hoy; sin `PrepareRedraw` | WR:112-121; los tres `PrepareRedraw` siguen solo en el bucle de miembros, :136, :139 y :142 | Cumple (orden y `Clear`: M05) |
| `BeginMutation` pasa `_erasures` | WR:169 | Cumple |
| `RackPilotMutationScope : IPushBackPilotMutationScope, IPushBackPilotViewEraser` | WR:242 | Cumple |
| `EraseUnit`, en orden: `EnsureOwned();`, la búsqueda en `_erasures` que lanza, `_units++;`, `FailUnitIfRequested(_units);`, la fachada con `_transaction`, las anidadas a `_staleDefinitions`, `new PushBackPilotUnitResult(Array.Empty<string>())` | WR:325-333 | Cumple. Sin `SyncName`, purga, regeneración ni `Commit` en su cuerpo |
| `Reread` calcula `matches` sobre `_prepared` y `_erasures` | WR:275-278 | Cumple |
| POST no cambia (purga de la unión de `_staleDefinitions`; nombres solo de `_prepared`) | WR:173-220, sin diff | Cumple |
| Los dos comentarios que decían lo contrario | WR:26-27 y :238-240; el segundo lleva el texto literal del paquete más una frase | Cumple |
| INV-03: tres clases `RackPilot*`, 1 `StartTransaction`, 1 `.Commit(`, 1 purga y 2 `[Conditional("DEBUG")]`; 0 de `GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` y `UpgradeOpen`; 0 `.Name =` | `grep -F` sobre el blob en base y en GREEN: 3/1/1/1/2 y 0 en todos, en los dos | Cumple |
| CT-04: `RackViewBlockEraser.EraseInTransaction(` una sola vez en `src/`, dentro de `EraseUnit` | WR:332, la única (`git grep` en `938a35f0`) | Cumple |
| G-9 (IntegrationGuard:679-699): WR mantiene sus cuentas; `.EraseInTransaction(` no es `\.Erase\s*\(` | La CI la pasa: `WorkspacePilotUpdateIntegrationGuardTests` 123 Passed + 2 Skipped de D-1 | Cumple |
| Un plan con borrados y sin miembros haría lanzar a `BeginMutation` (`_prepared.Count == 0`, WR:162) | `PrepareFull` rechaza `AllObsolete` antes (`PushBackPilotUpdatePlan.cs:629`) y usa `plans[0]` (`:633`) | No se alcanza. Sin hallazgo |

## D2 y su límite (punto 3)

- **El diff de `src/`** (84 líneas `+`/`-`) no contiene ninguna de estas cadenas: `PushBackSystem`, `RackModuleReconciliation`, las ocho fichas de D2, `.Name =`, `Structure` ni `Modules`. Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado, y ninguna toca un copiador del estado.
- **Los objetos que se comparten por referencia** (`_erasures`, `Dictionary<string, ObjectId>`, entre el ámbito de la corrida y el de MUTATE) son estado propio del escritor, con valores inmutables (`string` y `ObjectId`). Siguen el patrón ya existente de `_prepared` y `_staleDefinitions`. No son copias del estado del borrador, así que quedan fuera de D2.
- **CT-PBT3C-05** está en verde en el RED y en el GREEN, y CT-PBT1B-14/15 en verde (CT-15 sigue en 14 archivos: la fachada no contiene `PushBack`).
- **INV-PBT3C-14 se cumple, y C17 no se dispara.**

## Los CT (punto 4)

Cada CT es un `[Fact]`, sin `Theory`, y acaba en un `Assert.Equal` de las líneas unidas por `\n`:

| CT | Método | ¿Fija lo del §5.4? | Lectura del texto | Autopruebas | Falla en el RED por |
|---|---|---|---|---|---|
| 01 | `CT_PBT3C_01_TheFacadeErasesOneView…` (:184-254) | Sí: firma, clase, orden de 11 pasos, cuentas (`erase=2`, `upgradeopen=1`, `referencias=1`), prohibiciones y `NamesThePilot` copiado literal de IntegrationGuard:679 | Prohibiciones sobre el **texto crudo**, comentarios incluidos (más estricto que G-9 y que CT-PBT1B-15, que miran el código limpio; coherente con «el archivo no contiene la cadena `PushBack`»). Orden y forma sobre el código sin comentarios | Discriminan:<ul><li>9 prohibiciones inyectadas en el texto real y en un modelo (18 «falla»);</li><li>un comentario con `Purge`;</li><li>6 mutantes de orden y contenido sobre el modelo, con su control.</li></ul>En el RED todas dieron lo esperado (`rv/tests/t0/t.trx`: solo difieren `orden` y `erase`). Límite: M01 | `fachada orden=[]` y `erase=0 upgradeopen=0 referencias=0` (aserción) |
| 02 | `CT_PBT3C_02_EraseUnit…` (:272-301) | Sí: la interfaz, `EraseUnit=1`, los 8 pasos en orden, 1 llamada, sin purga, renombrado, regeneración ni confirmación, y 2 `[Conditional("DEBUG")]` | Código sin comentarios; `Conditional` sobre el crudo | No las pide el §5.4 | `ambito-implementa-IPushBackPilotViewEraser=False` (aserción) |
| 03 | `CT_PBT3C_03_PrepareResolves…` (:305-347) | Sí, con la precisión de M03 | Código sin comentarios | No las pide el §5.4 | `_erasures-declarado=0` (aserción) |
| 04 | `CT_PBT3C_04_TheCensusOfTheFacade…` (:377-406) | Sí: 1 llamada en `src/`, en WR y dentro de `EraseUnit` | Código sin comentarios, `src/*.cs` sin `bin`/`obj`, orden ordinal | Las tres del §5.4 (2.ª llamada en WR, en RPC y en un archivo nuevo) discriminan el total; la rama de ubicación no está aislada (M02) | `censo llamadas=0 archivos=[]` (aserción) |
| 05 | `CT_PBT3C_05_NeitherTheWriterNorTheFacade…` (`PushBackWritePathPluginGuardTests.cs:191-236`) | Sí:<ul><li>las 8 fichas de `PushBackSharedReferenceGuardTests.cs:35-36`;</li><li>`Rule1`, `Rule1b` y `Rule1c`, y la regla 2, copiadas byte a byte;</li><li>sobre WR y la fachada, y exige que existan las dos.</li></ul> | Reglas sobre `Clean` (sin comentarios y con los literales vaciados, como CT-PBT1B-14); fichas sobre el texto crudo (más estricto) | Discriminan: 8 reglas × 2 archivos y 8 fichas × 2 archivos. Subconjunto de ejemplos: M04 | No falla: pasa en el RED (ExpectRed false), como pide el §5.4 |

**Determinismo:**
- sin hora, sin aleatoriedad y sin estado estático mutable: solo `static readonly` de `string[]`, `Regex` y tuplas;
- los números con `InvariantCulture`, los booleanos con `ToString()` invariante y el orden ordinal en CT-04;
- solo lectura de archivos, sin escritura;
- la RR-3 no aplica, porque no hubo reparación (el relevo no lleva `TablaDeRR3`).

## El RED (punto 5)

- **La CI del RED** es el run `44754080713`. Con `gh run view`: `headSha e0940a0e`, `Tests` en failure, `UI Tests` y `Build UI` en success, `Build Plugin` en skipped, y `completed`.
- **Los TRX de la copia** tienen el mismo sha256 que los artefactos de ese run, descargados y comparados: Core `a55b0b30…` y UI `86e54c42…`.
- **Core:** 19 129 filas = 19 119 Passed + 4 Failed + 6 NotExecuted (Skipped). Las 4 fallidas son CT-01 a 04, todas con `Assert.Equal() Failure: Strings differ`, por aserción y no por excepción. CT-05 pasa. **C04 no se dispara.**
- **Los 6 omitidos** son los de D-1, con su motivo literal: 3 de `WorkspacePilotPrepareGuardTests`, 2 de `WorkspacePilotUpdateIntegrationGuardTests` y 1 de `WorkspacePilotUpdateGuardTests`. Es el mismo conjunto en la línea base de CI `44707482956` (`c7582785`), en el RED y en el GREEN.
- **UI:** 2 451 = 2 430 Passed + 21 Skipped. Es el mismo conjunto de nombres y de omitidos que la línea base, sin ningún cambio de resultado.
- **Totales exactos (F4B-E46, una sola rama):** coinciden con los previstos del contrato, tanto el total como cada parte.
- **Todos los filtros de `RequiredTests`**, contados sobre los TRX de la CI, superan su `MinSelected`. El de `ExpectRed` está entero en Failed.
- **Línea base de CI frente al GREEN:** solo se añaden los 5 nombres nuevos; ningún resultado cambia.

## El GREEN y la estabilidad (punto 6)

- **La CI del GREEN** es el run `44757675821`, sobre `headSha 938a35f0`, con los 4 jobs en success, también `Build Plugin without AutoCAD`.
- **Los TRX** coinciden en sha256 con los artefactos: Core `b3f7c9a1…` y UI `517e1e7e…`.
- **Core:** 19 129 = 19 123 Passed + 0 Failed + 6 Skipped. **UI:** 2 451 = 2 430 + 21. Son exactamente los previstos.
- **El GREEN no toca `tests/`:** `git diff e0940a0e 938a35f0 -- tests` está vacío.
- **Builds locales** (`rv/tests/b0` y `g5`): las mismas cuentas de avisos que la línea base (Core 132, Plugin 104 MSB3277 y UI 28 líneas de aviso) y 0 errores. Ningún aviso nombra los archivos nuevos.
- **Las tres corridas en verde del filtro con `ExpectRed` sobre el GREEN** (g1, g2 y g3) están en la copia.

## Contención (punto 7)

- **Pruebas:** solo guardas de texto de Core, sin ventanas, modales, `MessageBox`, configurador, «Insertar», BOM ni «Guardar en biblioteca». Sin escrituras: las autopruebas trabajan sobre texto en memoria.
- **UI en local:** 24 órdenes, que son los 12 filtros de UI del contrato × 2 (RED y GREEN), todas con su filtro de clase. Ninguna corre la suite de UI completa ni filtra por método.
- **La corrida diagnóstica `t0`:** usó el filtro de las dos clases nuevas. Cambió temporalmente el `Assert.Equal` final y añadió un `TmpDump.cs` que, según la entrega, no entró en ningún commit. Los commits lo confirman: los dos archivos de prueba son iguales en RED y GREEN, y no hay ningún archivo extra.
- **Las corridas de Core completas** (b0, r3 y g4) están permitidas por INV-10.
- **Lo que no se toca:** Application, Domain, el anfitrión (`src/RackCad.Plugin/Workspace/`), el puerto del panel y AutoCAD. `inv/inv-check.ps1` es de solo lectura (solo `git cat-file` y `git show`).
- **Disco y perfil:** sin escrituras en los archivos de prueba. La salida del Plugin local fue a `plugin-out` dentro de los artefactos.

## Hallazgos para el Owner (punto 8)

Ninguno es un defecto de la entrega: la fachada reproduce el clásico (RCS:233-266). Son riesgos del borrado real que el Smoke-PB debería mirar y que el paquete no declara de forma explícita (§5.5).

### PBT3C-IR-OWNER-1: referencias fuera de las copias del rack, borradas sin aviso

`GetBlockReferenceIds(directOnly: true, …)` devuelve todas las referencias directas de la vista: las del espacio modelo, **las de los layouts de papel** y las anidadas en **definiciones ajenas al rack**, por ejemplo un bloque del usuario. La fachada las borra todas (RVBE:31-34) sin ningún aviso, y con eso modifica definiciones que no son del rack.

- El paquete declara las anidadas (BM:54-60 y «con una referencia anidada» en el Smoke), pero no las de espacio papel ni el silencio.
- La barrera de capas bloqueadas sí las cubre: el escaneo recoge la capa de cada referencia directa, de layout o anidada (`RackSiblingScan.cs:141-146`), y UR la aplica también a las claves a borrar (UR:665-670).

**Para el Smoke-PB:** una referencia de la vista obsoleta en un layout de papel y otra anidada en un bloque del usuario. Comprobar que las dos desaparecen con un solo `U`, y que el Owner decida si quiere un aviso.

### PBT3C-IR-OWNER-2: la purga de las anidadas es de un nivel

Las candidatas son solo las hijas directas de la definición borrada (RVBE:23-28). POST hace una sola pasada de `Database.Purge` sobre ellas (`LateralHeaderDrawer.cs:195-239`). Las definiciones de segundo nivel, por ejemplo piezas de catálogo dentro de los grupos ARRAY, pueden quedar sin referencias y sin purgar tras «Actualizar». Es el mismo resultado que el clásico: residuo, no un error.

**Para el Smoke-PB:** contar las definiciones de bloque, o los candidatos de `PURGE`, antes y después de un «Actualizar» con borrado.

### PBT3C-IR-OWNER-3 (baja): bloques dinámicos

`GetBlockReferenceIds(directOnly: true)` no devuelve las referencias a las representaciones anónimas `*U` de un bloque dinámico, y el escaneo ignora las definiciones anónimas (`RackSiblingScan.cs:131`). Si alguna vista de los dibujos reales fuera dinámica, su borrado dejaría referencias anónimas. Las vistas de RackCad parecen estáticas, así que es una confirmación de una línea en el Smoke.

### Comprobado, sin hallazgo

- **El puente de eventos:** trata `ObjectErased` igual que `ObjectModified` (`EventBridgeCore.cs:346-353`). Dentro de la mutación propia, el borrado es una pista propia.
- **El origen borrado (V2-01), el `U` único y `LOCKED_LAYER`** ya están en el §5.5.

## Lo no verificado

- **No he compilado ni ejecutado nada,** por regla. La compilación y las pruebas las acredita la CI: verifiqué sus metadatos con `gh` y los TRX por hash contra sus artefactos. Las corridas locales solo constan por sus TRX, logs y `commands.txt` de la copia.
- **El comportamiento real en AutoCAD:** el borrado, la purga, el `U` y los eventos son del Smoke-PB. OWNER-2 y OWNER-3 se basan en la semántica conocida de la API, no en una medición.
- **El contenido de `TmpDump.cs` y la modificación temporal de `t0`:** se borraron. Solo veo su TRX y su log: las trazas apuntan a la línea del `Assert` final y los mensajes son las líneas ACTUAL/EXPECTED.
- **`clean-before.txt` y `clean-after.txt`:** no están en la copia, así que la identidad del configurador y de la biblioteca antes y después es una afirmación de la entrega.
- **Los avisos «nuevos»:** los comparé por cuentas y por la ausencia de los archivos nuevos en los logs, no línea a línea, porque el orden de los avisos varía entre compilaciones.
