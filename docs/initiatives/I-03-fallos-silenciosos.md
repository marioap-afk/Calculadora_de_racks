---
schema: rackcad-initiative/v1
id: I-03
title: Fallos silenciosos
type: refactor
status: integrated
branch: refactor/fallos-silenciosos
base_branch: main
priority:
size: M
depends_on: []
conflicts_with: [I-11]
context_packs:
  - persistence
  - autocad-plugin
  - catalogs-data
  - architecture-kernel
automation_state_path: docs/automation/state/I-03.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-03 — Fallos silenciosos

## 1. Objetivo

Hacer **diagnosticables** los fallos que hoy se tragan en silencio, **sin cambiar comportamiento
funcional** (el cambio es aditivo: se añade un rastro, no se altera el flujo). En concreto (hallazgos
P1 y D2 de la auditoría 2026-07):

- **Logger mínimo** a `%AppData%\RackCad\logs`: un archivo diario, best-effort (nunca lanza),
  seguro entre hilos. Vive en `RackCad.Application` (capa común a Plugin y Persistence).
- **Excepciones del Plugin y de Persistence registradas con stack trace**: los 14 `catch` silenciosos
  del Plugin y los `catch` best-effort de Persistence dejan un registro; `Report()` deja de tirar el
  stack (lo escribe al log, conservando el mismo mensaje en la línea de comandos).
- **Escrituras atómicas** (temp + `File.Replace`) en los **4 stores** que hoy usan `File.WriteAllText`
  directo: `RackProjectStore`, `RackFrameProjectStore`, `UserTemplateStore`, `UserSettingsStore`.
- **Carga que distingue "no existe" de "ilegible"**: los stores best-effort que hoy resetean a
  defaults en silencio (settings pierde la ruta de biblioteca sin aviso) ahora separan el archivo
  ausente (default silencioso, normal) del archivo presente-pero-ilegible (registro con stack +
  archivo movido a `.bad` para preservar el dato y hacerlo diagnosticable, luego default).
- **Aviso de catálogo vacío**: `RackCatalogLoader` registra por qué un catálogo roto degradó a
  "faltan bloques" (la "pista" que hoy no existe) y anota el caso de catálogo vacío.

Resultado verificable: ante un fallo antes silencioso queda un registro con contexto + tipo + mensaje
+ stack en `%AppData%\RackCad\logs`; las escrituras de los 4 stores no dejan un archivo a medias que
destruya la copia previa; y una carga corrupta ya no borra el dato del usuario en silencio.
**Compatibilidad I-11 preservada de forma estricta** (versiones, metadata, geometría, BOM, GUID,
comandos, alias, formatos, fallback legacy y comportamiento observable).

## 2. Problema

- **P1 — 14 catch silenciosos y cero logging**: un catálogo roto degrada a "faltan bloques" sin pista;
  `Report()` sólo muestra `ex.Message` y tira el stack trace. No hay ningún archivo de registro: un
  fallo best-effort (bloque ajeno omitido, redibujo parcial, restamp fallido, etc.) desaparece sin
  rastro y no es diagnosticable después.
- **D2 — Escrituras no atómicas y cargas que resetean en silencio**: cuatro stores hacen
  `File.WriteAllText` directo (sin temp + rename), de modo que una interrupción a media escritura
  puede dejar el archivo corrupto y perder la copia buena anterior; y `UserSettingsStore`/
  `UserTemplateStore` capturan cualquier error de carga y devuelven defaults sin avisar, así que un
  `settings.json` corrupto hace que el usuario **pierda la ruta de su biblioteca sin enterarse**.

## 3. Alcance

Estrictamente la fila I-03 del ROADMAP (recomendación 6 de la auditoría: "Logging mínimo + catch
silenciosos + escrituras atómicas"), sin ampliaciones laterales:

1. **Logger** (`RackCad.Application/Diagnostics/`): `RackLog` (fachada pública estática usada por Plugin
   y Persistence), `RackDiagnosticsLog` (escritor por instancia con directorio, best-effort, thread-safe)
   y `RackLogFormatter` (formato puro, testeable). Destino por defecto `%AppData%\RackCad\logs`
   (`rackcad-AAAAMMDD.log`). Nunca lanza (un fallo de logging jamás rompe el flujo).
2. **`Report()`** (`RackCad.Plugin/RackCommandSupport.cs`): registra la excepción completa (tipo +
   mensaje + stack) al log y **conserva idéntico** el mensaje de la línea de comandos
   (`"\nRackCad error: " + ex.Message`). Esto cubre de paso todos los `catch (ex) => Report(ex)`.
3. **Los 14 `catch` silenciosos del Plugin** registran contexto + excepción antes de tragar; el flujo
   de control (seguir best-effort) **no cambia**. Un `catch` sin variable pasa a `catch (Exception ex)`
   sólo para poder registrarlo.
4. **Persistence best-effort** (`UserSettingsStore`, `UserTemplateStore`): la carga distingue archivo
   ausente (default silencioso) de ilegible (registro con stack + movido a `<archivo>.bad`, luego
   default). El guardado registra el fallo antes de tragarlo (settings) — sin dejar de ser best-effort.
5. **Escritura atómica** (`RackCad.Application`): helper `AtomicFile.WriteAllText` (temp en el mismo
   directorio + `File.Replace` si el destino existe, `File.Move` si no) cableado en los `Save` de los
   4 stores. Se conserva la precondición de directorio de cada store (los que ya lo creaban lo siguen
   creando; los que no, siguen fallando igual si falta).
6. **Catálogo** (`RackCad.Plugin/RackCatalogLoader.cs`): registra la excepción cuando el catálogo no
   carga y anota el caso de catálogo vacío. Sin escrituras a la línea de comandos a mitad de dibujo.
7. **Pruebas** positivas y negativas en `tests/RackCad.Tests`. `InternalsVisibleTo(RackCad.Tests)` en
   Application para probar los seams internos (formatter, escritor, `AtomicFile`, overloads de path).

## 4. Fuera de alcance

- **Rediseño de UI** y cualquier cambio del texto/UX visible (los mensajes de línea de comandos se
  conservan verbatim; el logging es adicional y no visible salvo el archivo de log).
- **Cambios de schema / formato / DTO** y **reglas de producto** y **datos de catálogo** (CSV/JSON).
- **Telemetría remota**, rotación/retención de logs más allá del archivo diario, o un framework de
  logging configurable.
- **Correcciones ajenas**: no se arreglan bugs no relacionados de los archivos tocados; sólo se añade
  el registro que es el alcance de I-03.
- **I-17** (clon único de cabecera).
- **Las lecturas tolerantes de embeds de I-11** (`FlowBedConfigurationStore`,
  `SelectivePalletDesignStore`): su "tragado" es **diseño intencional** de I-11 (un bloque ajeno/ilegible
  se omite, no aborta el escaneo) — no son "fallos silenciosos" en el sentido de D2; no se tocan.
- **`RackFrameProjectDocument`** y su preservación de metadata (deuda diferida de I-11).
- **`UiSupport.LoadCatalogSafe`** (capa UI): el ROADMAP acota I-03 a Plugin + Persistence; se registra
  como idea futura, no se toca aquí.

## 5. Contexto requerido

- **Global**: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md` (fila I-03), `docs/ARCHITECTURE.md`,
  `docs/auditoria-arquitectura-2026-07.md` (P1, D2, recomendación 6) y este contrato.
- **Context Packs**: [`persistence`](../context-packs/persistence.md),
  [`autocad-plugin`](../context-packs/autocad-plugin.md),
  [`catalogs-data`](../context-packs/catalogs-data.md),
  [`architecture-kernel`](../context-packs/architecture-kernel.md).
- **Contrato previo**: [`I-11`](I-11-persistencia-uniforme.md) — §4 declara explícitamente que la
  escritura atómica, el logging y los archivos ilegibles quedan **fuera de I-11 y son de I-03**. I-03
  no toca los cuatro límites de preservación de I-11 (sólo cambia CÓMO se escribe el archivo, no QUÉ).
- **Código a leer antes de editar**: `Persistence/RackProjectStore.cs`, `RackFrameProjectStore.cs`,
  `RackFrames/UserTemplateStore.cs`, `Settings/UserSettings.cs`, `Catalogs/JsonRackCatalogProvider.cs`,
  `Plugin/RackCommandSupport.cs`, `Plugin/RackCatalogLoader.cs` y los archivos del Plugin con los 14
  `catch` silenciosos (ver §7).

## 6. Dependencias

- **Depende de**: nada (ROADMAP: "Depende de" = —).
- **Conflicto declarado ("se estorba con")**: **I-11** (`architecture/persistencia-uniforme`), hoy
  **integrada** — sin rama activa, sin conflicto real. I-03 **completa** lo que I-11 dejó fuera por
  contrato (escritura atómica + logging + archivos ilegibles), tocando la Persistence en un plano
  ortogonal (CÓMO se escribe / se registra, no el contenido del documento ni su versión/metadata).
- Sin decisiones ni entradas del dueño previas (`requires_owner_decision: false`).

## 7. Archivos esperados

**Crear** (`src/RackCad.Application/`):

- `Diagnostics/RackLog.cs` (fachada pública), `Diagnostics/RackDiagnosticsLog.cs` (escritor por
  instancia), `Diagnostics/RackLogFormatter.cs` (formato puro), `Diagnostics/CorruptFile.cs`
  (registro + cuarentena `.bad`), `Persistence/AtomicFile.cs` (escritura atómica).
- `Properties/AssemblyInfo.cs` **o** un `ItemGroup` en el `.csproj` con `InternalsVisibleTo(RackCad.Tests)`.
- Pruebas nuevas en `tests/RackCad.Tests/`.

**Modificar**:

- `src/RackCad.Application/Persistence/RackProjectStore.cs` y `RackFrameProjectStore.cs` (`Save` →
  `AtomicFile`).
- `src/RackCad.Application/RackFrames/UserTemplateStore.cs` (`Save` → `AtomicFile`; `Load` distingue
  ausente/ilegible + cuarentena + log).
- `src/RackCad.Application/Settings/UserSettings.cs` (`Save` → `AtomicFile` + log; `Load` distingue
  ausente/ilegible + cuarentena + log; overloads internos por path para pruebas).
- `src/RackCad.Plugin/RackCommandSupport.cs` (`Report()` registra el stack).
- `src/RackCad.Plugin/RackCatalogLoader.cs` (registro de fallo de carga + catálogo vacío).
- Los 14 `catch` silenciosos del Plugin registran: `Headers/BlockLibraryImporter.cs` (×2),
  `Headers/BlockPlacement.cs`, `Headers/LateralHeaderDrawer.cs`, `Headers/LateralHeaderDrawService.cs`,
  `Headers/RackBlockRenamer.cs`, `RackCommandSupport.cs` (ReadDimensionStyleNames), `RackEnvelopeRestamp.cs`,
  `RackInventarioCommands.BomTotal.cs`, `RackLayoutCommands.cs`, `RackLayoutCommands.Fill.cs` (×3).
  (`RackCatalogLoader.cs` es el 14º; se trata en el punto anterior.)

Una **desviación material** —cambiar geometría, BOM, GUID, un formato/DTO persistido, la clave del
Xrecord, catálogos, Draw Services, o el texto visible de un mensaje existente— obliga a **detenerse** (§12).

## 8. Fases

- **F0 — Reclamo**: worktree desde `origin/main` (`f674bd4`), commit vacío de reclamo con `Claim-Id`,
  primer push aceptado sin force. **Sin código.**
- **F1 — Contrato**: este archivo + `docs/automation/state/I-03.yml` + entrada en el índice.
- **F2 — Pruebas que fallan antes del cambio**: formatter (stack + contexto), escritor real a archivo,
  `AtomicFile` (crear/reemplazar/sin temp huérfano/directorio ausente), y la distinción
  ausente-vs-ilegible con cuarentena `.bad` en settings/templates. Deben **fallar** antes de F3.
- **F3 — Implementación**: la solución mínima que satisface F2 (logger, `AtomicFile`, cuarentena,
  cableado de los 4 stores, distinción de carga, `Report()`, los 14 catch, catálogo).
- **F4 — Validación local**: `git diff --check`, suite completa, builds sln/UI/Plugin Debug, búsquedas
  de regresión (formatos/claves/GUID/mensajes ajenos intactos).
- **F5 — Push + CI por SHA**: push de la rama (respaldo autorizado), CI verde sobre el HEAD final.

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` **verde** (toda la suite; baseline 981).
- Las pruebas F2 **fallan** antes de F3 y pasan después; no se debilitan pruebas existentes.
- Build Debug de `RackCad.sln`, `RackCad.UI` y `RackCad.Plugin` con **0 errores** (sólo los 2 `MSB3277`
  conocidos del Plugin). Sin paquetes NuGet nuevos.
- CI verde sobre el commit final (`requires_ci: true`).

## 10. Validación manual

- **AutoCAD: NO requerido** (`requires_autocad: false`; el ROADMAP **no** marca I-03 con ✋). El cambio
  es aditivo (logging + escritura atómica) y no altera geometría, BOM, comandos ni mensajes visibles.
- **Owner-validation: NO requerido** (`requires_owner_validation: false`). La cobertura la dan las
  pruebas puras (formatter, escritor real, `AtomicFile`, distinción de carga) y los builds.
- **Smoke test opcional** (recomendado, no bloqueante): con el DLL Debug del worktree en AutoCAD 2025,
  provocar un error de comando y confirmar que aparece una entrada con stack en
  `%AppData%\RackCad\logs\`; confirmar que settings y biblioteca siguen guardando/cargando sin cambio.

## 11. Criterios de aceptación

- Existe un logger a `%AppData%\RackCad\logs` que registra tipo + mensaje + stack, nunca lanza, y es
  consumido por `Report()`, los 14 catch del Plugin, el catálogo y los stores best-effort.
- Los 4 stores escriben de forma atómica (temp + `File.Replace`/`Move`) sin dejar temporales huérfanos.
- La carga distingue archivo ausente (default silencioso) de ilegible (log + `.bad` + default), con
  test que falla sin el cambio.
- **Invariantes I-11**: JSON conocido, nombres/casing, enums, versiones/metadata, GUID, geometría, BOM,
  fallback legacy y la clave del Xrecord **idénticos**; comandos, alias y mensajes visibles **idénticos**.
- Suite verde + builds UI/Plugin/sln Debug con 0 errores. Implementada ≠ integrada.

## 12. Condiciones para detenerse

- Rama remota de I-03 reclamada por otra sesión, o worktree con otra sesión activa.
- Sería inevitable cambiar geometría, BOM, GUID, un formato/DTO persistido, la clave del Xrecord,
  catálogos, Draw Services o el texto de un mensaje visible existente.
- Un cambio rompería la compatibilidad obligatoria con I-11 (versiones, metadata, enums, fallback legacy).
- Tres intentos fallidos de pruebas/build/CI; CI no disponible para el SHA final; árbol sucio del
  usuario; falta de permisos GitHub.

## 13. Estado versionado y entrega del Pull Request

Ejecución **dirigida** (la automatización nocturna está pausada). Estado canónico en
`docs/automation/state/I-03.yml`. Identidad del reclamo: `initiative: I-03`,
`branch: refactor/fallos-silenciosos`, `claim_id: 55a87e52-b49e-442e-ad66-fbd86774f834`. No se abre PR
por CLI (Git basta); si se abre, es **único** y **draft**, sin auto-merge. **Merge automático
prohibido.** `in-progress`/`completed` no significan integrada: la integración es manual del dueño.

## 14. Evidencia final

Commits de la rama, archivos, pruebas (positivas y negativas), builds, CI por SHA, invariantes I-11
verificados y validación manual pendiente (sólo el smoke test opcional) se registran en el reporte de
cierre y en el cuerpo de los commits. `main` **no** se modifica; la integración serializada la ejecuta
el dueño (rebase final, CI, HANDOFF §8-12 + ROADMAP, merge `--no-ff`).
