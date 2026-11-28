# PB-T3c: revisión directa de la sesión (intento 1, Worker `R20261120T135026Z-80e5`)

**Qué se revisa:** el RED `e0940a0efa314ea26777c59ab4e55c09d019bff6` y el GREEN `938a35f0f1636b4306d844eb9e34000e8967c05e` sobre la BASE `7dfc7c21` (F4B-E47). Se lee por objetos git desde el checkout principal, sin tocar el worktree. La verificación del Controller corre en paralelo.

**Resultado: conforme.** Sin hallazgos bloqueantes ni importantes. Dos observaciones van para la revisión independiente.

| Qué | Resultado |
|---|---|
| **El RED** (`7dfc7c21..e0940a0e`) | Tres archivos nuevos, +661: la fachada (15 líneas, igual al parche `a81de435…`; el relevo lo comprueba por blob con CRLF→LF) y las dos clases de Core. WR no cambia |
| **El GREEN** (`e0940a0e..938a35f0`) | Solo WR (+43/−5) y la fachada (+28/−1). Ningún archivo de `tests/` |
| **Las CT** | Cinco `[Fact]`, uno por CT (CT-PBT3C-01 a 04 en `PushBackViewEraseGuardTests`, CT-PBT3C-05 en `PushBackWritePathPluginGuardTests`). Ninguna `[Theory]`, ningún `Skip`, ningún `Assert.Fail` |
| **CI del RED** (`44754080713`) | `tests` en failure; `ui-tests` y `build-ui` en success; `build-plugin` omitido. Core 19 129 = 19 119 + 4 Failed + 6 Skipped; UI 2 451 = 2 430 + 21. Es la única rama prevista (F4B-E46) |
| **CI del GREEN** (`44757675821`) | 4/4. Core 19 129 = 19 123 + 0 + 6; UI 2 451 = 2 430 + 21. Exacto |
| **D-1** | Los seis `Skip` siguen en el RED y en el GREEN (S-1 a S-6, una fila cada uno) |
| **INV-PBT3C-03** | En WR, sobre el GREEN: ninguna aparición de `GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite`, `UpgradeOpen` ni `.Name =`. `Purge` sigue una sola vez (`SystemBlockWriter.PurgeAfterCommit`, en POST). `EraseUnit(` y `EraseInTransaction(` no casan con `Erase(` |
| **INV-PBT3C-04** | La fachada no contiene la cadena `PushBack`, ni siquiera en el comentario nuevo |
| **C13, el censo** | Las cinco declaraciones nuevas son las previstas (las dos `_erasures`, `EraseUnit`, la clase y su método) |
| **Trailer** | `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` en los dos commits |

**Lo que hace el GREEN:**
- **La fachada:** abre la definición en lectura y rechaza un layout con `InvalidOperationException`; el clásico lo salta en silencio, y el comentario lo justifica: el llamador aborta entero. Captura las definiciones anidadas antes de borrar. Borra las referencias directas (`directOnly: true`), también las que están dentro de otras definiciones, y luego la definición (`UpgradeOpen` + `Erase`). Devuelve las anidadas. No toma el bloqueo, no abre ámbito, no confirma ni purga.
- **WR:**
  - PREPARE resuelve las claves de `plan.Erasures` antes del bucle de los miembros; una clave ausente falla antes de importar nada (limitación declarada por el Worker: el paquete no fija ese orden);
  - MUTATE recibe `_erasures` por el constructor (cuatro argumentos más uno, su único uso en WR:156 → 169);
  - `RackPilotMutationScope` implementa `IPushBackPilotViewEraser` con `EraseUnit`: comprueba el ámbito, busca la clave, cuenta la unidad (la inyección de fallo de depuración la cubre) y llama a la fachada; las anidadas van a `_staleDefinitions`, que POST purga;
  - la relectura de MUTATE incluye las claves a borrar en `matches`.

**Observaciones para la revisión independiente (sin hallazgo de la sesión):**
1. **La relectura de MUTATE** escribe en el mismo diccionario `matches` las claves preparadas y las de borrado. Si una clave estuviera en los dos, ganaría la de borrado. El diseño de PB-T3 lo excluye: `Erasures` solo lleva `ObsoleteLateral`, que no es miembro. Que lo confirme.
2. **El layout como error,** frente al salto silencioso del clásico: ¿lo admite D-PBT3-11 tal como está escrito?

**Sin AutoCAD:** el borrado real en el dibujo, con las referencias, las anidadas, la purga de POST y el deshacer, queda para el Smoke-PB (D-PB-09). El Worker lo declara.
