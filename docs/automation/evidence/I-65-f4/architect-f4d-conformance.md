# I-65 — F4-D: revisión de conformidad del Architect

Revisión del Architect de I-65 (`SAME-SESSION ROLE`, subagente `claude-opus-5-5`/`high`, solo lectura; cesión 2026-10-16T16:19:36Z - 16:27:58Z;
prompt `architect-f4d.prompt.md`) sobre `e886b557..51fc82f1`. Texto del informe tal como lo entregó el Architect.

---

**Veredicto: CONFORMANT WITH OBSERVATIONS.** El diff no tiene ningún hallazgo REQUIRED; registro 11 observaciones OPTIONAL. Dos de ellas son precondiciones de cierre que no son defectos del diff: los controles nc1-nc3 y la custodia del intento 7 (A65-F4D-08).

**pwd informado:** `/d/Documentos/Codex/Calculadora de racks`. Es el checkout principal y no lo leí ni lo usé. Todas las lecturas fueron con `git -C <worktree>` (`show`, `diff`, `grep`, `log`, `rev-parse`, `check-attr`) o con rutas absolutas bajo `artifacts/` del worktree. No escribí nada, no compilé, no ejecuté pruebas ni intérpretes, y no hice fetch.

Alcance revisado: `e886b557..51fc82f1`, seis commits. RED `fe9b6720` y GREEN `d548fb0b` del intento 6; `2a1ed99a` y `c487262f` solo tocan `docs/automation`; RED `2188a1d7` y GREEN `51fc82f1` del intento 7.

## 1. Tabla

- **Blob idéntico:** `HEAD:assets/.../section-material-map.json` = `e886b557:docs/automation/evidence/I-65-f4/f4d-table-target.json` = `d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2`. El blob anterior era `81532867…`. El archivo es UTF-8 sin BOM, con LF y sin ningún CR.
- **Solo cambian los campos de D-INV-01:** `ramSection.table` y `.country`, los cinco campos de `expectedMaterial`, `status`, `verifiedBy` y `verifiedOn`.
  - `sectionId`, `ramSection.name`, `materialId`, `ramMaterial`, el esquema y el número de filas (2) no cambian.
  - Lo único adicional es formato: los objetos que estaban en una línea pasan a varias.
- **Valores frente a la orden literal (D-01..D-09):** coinciden.
  - Tabla `W` / `HSS_SQR` y país `United States`.
  - E = 29000 ksi, `poissonRatio` 0.3 y `shearModulus` null.
  - `unitWeight` 490 pcf. DIF-01 no se codificó como valor de RAM (486.94).
  - `diff` entre `d08-rows-final.json` y la tabla en HEAD: solo difieren las 4 líneas de `verifiedBy`/`verifiedOn`. Por tanto los `source` son la Variante B íntegra.
  - `verifiedBy` es idéntico byte a byte en las dos filas y en `.original.txt`: `C3 A9` (é) y `E2 80 94` (raya). `verifiedOn` es `2026-10-16` en ambas.
- **DIF-01 y limitación del catálogo:** ambas están en los dos `source` («DIF-01: RAM unit weight -0.62 %, informative only…», «RAM catalog formal edition not verified»).
- **Nada presenta la tabla como validación estructural ni como certificación del material.**
  - Los `source` dicen «modeling conventions… primary citation not read in I-65».
  - Busqué en las líneas añadidas de pruebas, assets, decisiones y evidencia los términos certif, validación estructural, resistencia, safe y similares: cero coincidencias.

## 2. Freeze, ADR-0050 y fronteras

- `git diff --name-only e886b557..HEAD -- src tools '*.csproj' RackCad.sln docs/adr docs/initiatives assets/catalogs .github` devuelve 0 archivos.
- El último commit de ADR-0050 es `ed8154c3` y el de la Proposal V3.1 es `a9d7080b` (Freeze); ninguno tiene cambios posteriores.
- Domain, UI y Workspace no se tocan; tampoco los `.csproj`.
- **Tolerancias:** sin cambios. `StructuralReadbackComparison.cs:407-419` está intacto, y las propiedades de material siguen en `NotEvaluated` con nota de hoja firmada (`:765-766`).
- **Oráculo de material:** ν = 0.3 con `shearModulus` null en las dos filas; no hay segunda fuente de G.
- **INV-04:** la huella sale de `AnalysisInput(model, adapterVersion)`. `MappingRows` usa el `CanonicalText` de la fila (`RamSectionMapRow.cs:130-147`), que incluye todos los campos.
- **INV-14 y guardas:** solo cambian cinco archivos de prueba, todos en el AllowedWriteScope. Ningún archivo de guarda se tocó. Las líneas añadidas no contienen tokens de intérprete ni del módulo del proveedor, salvo espacios de nombres ya usados en pruebas existentes.

## 3. Pruebas

**Cobertura de D-INV-01..06 en `StructuralAnalysisSectionMapVerifiedTests`:**
- **Literales:** se comprueban todos, incluidos los dos `source` completos y `VerifiedBy.Length == 43`. El `.cs` es UTF-8 sin BOM.
- **Lector:** `Valid`, `Assert.Empty(Diagnostics)` y dos filas `Verified`.
- **REF-CANT-01:** `Valid` con el mismo conjunto `Code|Subject|Severity` que la tabla verified de fixture. Ambos conjuntos están vacíos, pero un diagnóstico adicional rompería la prueba.
- **Paquete:** `Valid` en memoria. La fábrica no hace E/S (`RamBuildPackageFactory.Build` solo arma bytes en memoria).
- **Huella:** cambiar `verifiedOn` de la primera fila cambia `AnalysisInputFingerprint`.

**Cobertura proposed:**
- `LaPrecedencia…` conserva `Invalid` con `FolderInvalid` + `SectionRowUnverified` e `ImpliedOutcome` `Unsupported`, sobre una tabla de fixture con una fila proposed.
- `UnResultadoQueNoEsValido…` conserva la propagación de `Unsupported`, sin plan.
- También se mantienen pruebas de fixture que ya existían, como `ProposedArmRow` (en `e886b557`, línea 60).

**RED honesto en los dos intentos:**
- Intento 6: CI `38921140743`, 13 fallos de 1795, todos por aserción.
- Intento 7: CI `38927278590`, 13 fallos de 1795 (8 `Assert.Equal`, 3 `Assert.All`, 1 «Se esperaba Valid», 1 precondición `verifiedOn`); ninguno de compilación.
- `git diff 2188a1d7 51fc82f1 -- tests` está vacío, y el diff neto `d548fb0b..51fc82f1` son solo las dos pruebas corregidas.

**Desviación D1 (el modelo se construye con la tabla del producto): aceptable y necesaria.**
- `CheckPlan` compara `MappingRowsSha256` del modelo con el del plan. Con un modelo de fixture (E 111.5, peso 222.5) la fábrica da `Invalid` por mapeo obsoleto, y eso es correcto.
- El caso de mapeo obsoleto sigue cubierto en `PackageValidationTests:124`.

**Carpeta fija `C:\RackCad-F4D-NoExiste\paquete-estacion-0`: limitación aceptable, no defecto.**
- `IsAbsoluteNormalisedFolder` es sintáctica e independiente del sistema operativo (`RamBuildPackageFactory.cs:160-198`), así que la cadena es válida en Windows y en Linux.
- Límites, en A65-F4D-03: en Linux `Directory.Exists` evalúa un nombre relativo y la comprobación es vacía. En Windows, la precondición fallaría (sin destruir nada) si esa carpeta existiera.

## 4. Proceso

- **REWORK por `Ci`: sin objeción.** El prompt del intento 6 (`8301/prompt.md:81-83,107,129`) ordenaba usar `Path.GetTempPath()`. Eso contradice A65-F3-07, así que el error es del Coordinator y no activa C-03.
  - La prueba de que la corrección funciona es el par de CI sobre la misma tabla: `38922575054` (`d548fb0b`, 3 fallos) y `38928508008` (`51fc82f1`, success). El RED `2188a1d7` solo vuelve a probar que las pruebas dependen de la tabla (A65-F4D-05).
- **Rechazo P-03: correcto.** `exactness.json` muestra `" .github/"` y `…f4-d.original.txt`. La reemisión `5fdd` sale exacta (A1-A8 `pass`), y no se consumieron intentos. Es el tercer caso del mismo modo de fallo (A65-F4D-09).
- **STOP P-02 como falso positivo: sin objeción.** `exit-reread.txt` registra `alive=[]` para el PID 36848, y `cession-start.txt` (15:43:47Z) es posterior a la foto (15:43:44Z). La reverificación `e795` da `Termination` en `pass` con esa evidencia mecánica.
  - Matiz: la reverificación recibió como entrada el análisis del Coordinator. Hay algo de riesgo de anclaje, pero lo atenúa que el releído es un hecho registrado antes de la cesión.
- **DEV-I65-F4-23 (`TmpProbe.cs`):** sin efectos en commits. La suite local (14390) coincide con la CI, y el prompt del intento 7 lo prohíbe expresamente (línea 69-70).
  - La huella `4defdcd4…` se midió con esa sonda, que no está versionada. No sirve como oráculo (A65-F4D-07).
- **Verificación final:** `EXECUTION_VERIFIED`, 14/14 `pass`, alcance mecánico `pass`, y RED y GREEN en CI sobre los SHA exactos. Lo que falta para cerrar F4-D:
  - **Controles negativos:** nc1-nc3 no se han ejecutado. Con Codex en 47 de 48, nc2 y nc3 exceden el tope (P-07).
  - **Custodia:** los registros del intento 7 aún no están en Git: las decisiones (§65) y la evidencia de `9e5c`, `e795` y `analysis-p02`.
  - **Estado:** el `next_action` de `state/I-65.yml` está desactualizado.

## 5. Siguiente paso: riesgos antes del segundo smoke (camino Valid, OV-I65-03) y de la primera generación válida

1. **Orden de pasos.** Primero nc1-nc3 o una disposición del Owner sobre ellos, y la custodia y cierre de F4-D. Después, la aprobación del Owner para usar el nuevo DLL. Por último, compilar con AutoCAD cerrado.
2. **Identidad de la tabla en el DLL (A65-F4D-01).** `core.autocrlf=true` y la tabla no tiene regla `-text`. Una copia nueva del repositorio da CRLF, y el `Content` de `RackCad.Plugin.csproj:27-29` lo copiaría tal cual. Hay que fijar qué se compara: el blob o el contenido con fin de línea normalizado.
3. **El camino Valid escribe.** `StructuralPackagePublisher.Write` escribe en disco, y la carpeta queda fijada en el script y en el manifiesto. El Owner debe definir antes si el paquete del smoke es desechable o si es el primer paquete válido; recuerda que prohibió generar el paquete definitivo.
4. **Datos de entrada pendientes.**
   - REF-CANT-01 en un DWG (OD-F4-09) y el archivo de cargas `P1-P3` (OD-F4-06), con su SHA.
   - La huella de las pruebas no es oráculo: el `rackId` y el origen del catálogo difieren.
5. **Coordinación con I-52.** Hace falta reserva de ventana y recomprobación de I-52, como en CD-I65-F4-92.
6. **En la primera comparación:**
   - DIF-01 queda fuera de ±0.5 %. Debe constar en la hoja firmada como reconocida (CD-I65-F4-47) y no como dentro de tolerancia.
   - Es la primera vez que un manifiesto lleva `verifiedBy` no ASCII; ver A65-F4D-10.

**Estado de las observaciones anteriores:**
- **A65-F4R-10: resuelta la parte de datos.** La tabla está verified, el lector y REF-CANT-01 dan `Valid` y el paquete es `Valid` en memoria. Falta recompilar y comprobarlo en AutoCAD.
- **A65-F4R-08: parcialmente satisfecha.** El camino `Unsupported` se aceptó en CD-I65-F4-92; el camino Valid, con publicación, nunca se ha ejecutado.
- **A65-F4R-06: abierta, sin cambios.** Hay que declarar las combinaciones como no leídas, salvo medición en E4, antes de la primera comparación. A65-F4R-07 (`RackRamElementsCommands.cs:116-118`) y A65-F4R-02 siguen abiertas.

## Hallazgos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-F4D-01 | `.gitattributes`; `RackCad.Plugin.csproj:27-29` | La tabla no tiene regla `-text` y `core.autocrlf=true`: una copia nueva tiene CRLF y la que acompaña al DLL no coincidirá en bytes con el blob LF | OPTIONAL | Custodiar el DLL comparando el contenido con fin de línea normalizado (o el blob); valorar `-text` en una unidad autorizada |
| A65-F4D-02 | `SectionMapVerifiedTests.CambiarUnCampo…` | Solo varía `verifiedOn` (un metadato). Su RED falla por precondición, no por sensibilidad; es inherente porque no hay cambio de producción | OPTIONAL | En una unidad futura, añadir una variante con `unitWeight` o `source` |
| A65-F4D-03 | `PackageFolder` en dos archivos de prueba | En Linux `Directory.Exists` comprueba un nombre relativo y la aserción es vacía. D-INV-06 del contrato de gate aún menciona `Path.GetTempPath` | OPTIONAL | Registrarlo como limitación; D-INV-06 se cumple porque la fábrica no hace E/S |
| A65-F4D-04 | `PackageValidationTests.cs:309`; `TranslationTests.cs:447-454` | La variable `shipped` ahora contiene una tabla de fixture; `ProposedArmRow` está duplicado porque Fixtures está prohibido | OPTIONAL | Renombrar o factorizar cuando se autorice |
| A65-F4D-05 | RED `2188a1d7` | El RED de la corrección no demuestra el arreglo de la carpeta; lo demuestra el par de CI `38922575054` (fallo) y `38928508008` (éxito) | OPTIONAL | Decirlo así en la custodia |
| A65-F4D-06 | `RackRamElementsCommands.cs` (publicación) | El camino Valid escribe un paquete atado a su carpeta: el segundo smoke será la primera generación de un paquete | OPTIONAL | La autorización del Owner debe fijar si es desechable o el primer paquete válido, la ubicación fuera del repositorio y lo que se espera observar |
| A65-F4D-07 | Evidencia (4) de `8301` | La huella `4defdcd4…` se midió con una sonda no versionada y no es oráculo del comando (`rackId` y catálogo distintos) | OPTIONAL | No usarla como criterio del smoke |
| A65-F4D-08 | `state/I-65.yml`; decisiones/evidencia | Precondición de cierre (no defecto del diff): nc1-nc3 pendientes, nc2-nc3 por encima del tope de 48, y el intento 7 sin custodia | OPTIONAL | Disposición del Owner (P-07) y custodia antes de cerrar F4-D |
| A65-F4D-09 | Protocolo del Controller | El falso P-02 repetido costó la invocación 46; P-03 es el tercer caso de transcripción corrupta | OPTIONAL | Dar siempre al Controller el releído junto a la foto, y proyectar las colecciones mecánicamente |
| A65-F4D-10 | `RamBuildManifest.cs:78,208` | El manifiesto (no solo ASCII) llevará é y la raya; ninguna prueba lee de vuelta un manifiesto con verificador no ASCII (el fixture usa `TEST-FIXTURE`) | OPTIONAL | Observar los bytes del manifiesto en el smoke; añadir la prueba en el futuro |
| A65-F4D-11 | §4 del Freeze; `StructuralReadbackComparison.cs:765-766` | DIF-01 (−0.62 %) queda fuera de ±0.5 % y las propiedades de material son `NOT_EVALUATED` con hoja firmada | OPTIONAL | Que la hoja de la primera comparación registre DIF-01 como reconocida y fuera de tolerancia, sin peso propio |

## No comprobado sin ejecutar

- Que las pruebas pasen o fallen: me basé en los TRX, los `trx-summary` y las salidas del Controller.
- La identidad de las corridas de CI en GitHub: usé `run-*.json` y los TRX locales.
- Que la compilación copie la tabla junto al DLL, y su fin de línea en una copia nueva del repositorio.
- Los valores de huella y la ausencia de `C:\RackCad-F4D-NoExiste` en las máquinas o runners.
- Que el manifiesto con texto no ASCII se lea bien con `RamManifestReader` y con la herramienta de comparación.
- Los tiempos y PID de los procesos: solo constan en archivos.
- Los contadores de Codex e intentos.
- K1/K2 en el checkout principal y en otros worktrees: no los leí, por la regla de operación.
- El resultado de las pruebas de UI.
