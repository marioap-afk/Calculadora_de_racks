# Project Handoff

> Estado vivo de RackCad para continuidad entre sesiones. Actualizado: **2026-07-21**.
> La arquitectura se consulta en [ARCHITECTURE.md](ARCHITECTURE.md), el proceso en
> [WORKFLOW.md](WORKFLOW.md), el plan en [ROADMAP.md](ROADMAP.md), los procedimientos en
> [guias/](guias/) y la historia anterior en
> [archivo/transicion-2026-07/handoff-historial-2026-07.md](archivo/transicion-2026-07/handoff-historial-2026-07.md).

## 1. Resumen y estado actual

RackCad es un plugin de AutoCAD 2025 (.NET 8, C#/WPF) para diseñar y dibujar racks industriales
con BOM. El trunk único es `main`; Domain y Application son puros, UI usa WPF sin AutoCAD y Plugin
es el único adaptador de la API de AutoCAD.

El producto mantiene cuatro familias operativas: cabecera, selectivo, dinámico modular y cama de
rodamiento. Comparten identidad por GUID embebida en DWG, edición round-trip y vistas ligadas. El
dinámico modular de I-02 y la instalación segura de I-04 están integrados.

I-06 (`docs/reestructura`) está cerrada e integrada desde el **2026-07-17**. Entregó
`ARCHITECTURE.md`, nueve Context Packs, guías vigentes, archivo histórico y este HANDOFF reducido.
La iniciativa reorganizó documentación y no cambió comportamiento de producto.

I-26 (`refactor/test-catalog-ids`) está integrada desde el **2026-07-19**. Centraliza las
expectativas canónicas de tests, añade un guardián de IDs y relaciones esenciales y publica
cobertura Cobertura como artifact; no cambia producto ni catálogos distribuidos.

I-13 quedó integrada el **2026-07-20** mediante `architecture/referencias-autocad-ci`. CI compila
ahora `RackCad.Plugin` sin AutoCAD instalado con
referencias condicionales compile-only, versiones y origen fijados y guardas que impiden copiar o
publicar material Autodesk. ADR-0003 registra la única excepción autorizada a la política cero
NuGet. I-29 concluyó con decisión B, aprobada con catorce restricciones para uso interno como
aceptación interna de riesgo; no constituye conclusión jurídica ni autorización expresa de Autodesk.

I-09 (`refactor/plugin-commands`) quedó integrada el **2026-07-20**. Partió la clase única
`RackFrameCommands` (12 archivos parciales) en clases de comando públicas por área (`RackMenuCommands`,
`RackCabeceraCommands`, `RackSelectivoCommands`, `RackDinamicoCommands`, `RackCamaCommands`,
`RackDuplicarCommands`, `RackInventarioCommands`, `RackLayoutCommands`, `RackAyudaCommands`) y promovió
los helpers cruzados a tipos `internal static` (`RackBlockFinder` con el escaneo de envelopes
unificado, `RackCloner`, `LayerHelper`, `InDocumentTransaction`, `RackCommandSupport`,
`RackEnvelopeRestamp`). Es un refactor mecánico **sin cambio de comportamiento**: los 26 comandos y
alias, prompts, mensajes, switches por `Kind`, DrawServices, GUID/restamp, layers y flujos se
conservan; solo cambia la clase contenedora. AutoCAD descubre las clases públicas sin
`[assembly: CommandClass]`.

I-08 (`architecture/system-registry`) quedó integrada el **2026-07-21**. Introdujo un `SystemDescriptor`
y un `SystemRegistry` puro en `RackCad.Application` (fuente única de los cinco `RackSystemKind`, sin
reflexión ni escaneo) y migró `RackProjectStore`, la validación genérica y `RackDesignLibrary` para
despachar por el registro: mueren el `if/else` de `Serialize` y los `switch` de
`BuildProject`/`ValidateProject`, y **el enum paralelo `RackDesignKind` y su `MapKind` quedaron
eliminados por completo** (`RackDesignLibraryEntry.Kind` pasa a `RackSystemKind`; la etiqueta visible
proviene del descriptor). Es un cambio **sin comportamiento observable nuevo**: formato JSON PascalCase,
schema `2.0`, nombres de enum, fallback legacy sin `kind`, `Kind: 999` a cabecera, reconstrucción física,
reglas laxas de cama/larguero y etiquetas de biblioteca se conservan idénticos. La adaptación de UI se
limitó a las comparaciones de `RackMainMenuWindow`. I-10 e I-16 quedaron fuera de alcance.

I-16 (`refactor/draw-services`) quedó integrada el **2026-07-21** (merge `--no-ff`; rebaseada sobre el `main`
con I-08 antes de integrar; validada en AutoCAD, ver §2 y §5). Colapsa la duplicación de los `*DrawService`
del Plugin **sin cambio de comportamiento**: extrae la infraestructura compartida (`RackCatalogLoader`,
`BlockPlacement`), uniforma la regeneración condicional (`SystemBlockWriter.ApplyRegen`) y colapsa la
orquestación de los siete servicios de vista en `ViewBlockDraw`, conservando las siete fachadas públicas,
`LateralHeaderDrawService` como servicio especializado, y los invariantes observables (nombres de bloque y
sufijos, mensajes, `postIndex`, `DynamicRackEnd`, caso all-loose, payload/GUID, BOM, geometría, persistencia
y el único `Regen` final multivista).

I-10 (`architecture/kind-handlers`) quedó integrada el **2026-07-21** (merge `--no-ff`; `origin/main` no
avanzó desde la base `c5a4082`, sin rebase final). Introduce `IRackKindHandler` y un registro explícito
`KindHandlerRegistry` en `RackCad.Plugin` (los cuatro Kinds embebidos —`selective`, `dynamic`, `cabecera`,
`cama`— en orden canónico, sin reflexión; `Larguero` no tiene sobre ni handler) y migra a él los tres
despachos por el string del sobre: RACKEDITAR, RACKBOMTOTAL (`BuildRackBom` + `KindLabel`) y el restamp de
copias independientes (consumido por RACKDUPLICAR y RACKLAYOUT). Es un refactor **sin cambio de
comportamiento observable** salvo una excepción autorizada: un `Kind` sin handler produce un error visible
(el mismo mensaje de "tipo de rack no reconocido", inalcanzable con los cuatro Kinds reales). El
`SystemRegistry` de I-08 permanece en Application (persistencia/validación/biblioteca, por `RackSystemKind`);
`KindHandlerRegistry` es el registro del Plugin para las operaciones AutoCAD y **no** se unifica con él ni
con `RackListBuilder` (Application, RACKLISTA), que queda intacto por la dirección de dependencias. Cierra la
pista B del Plugin (I-09→I-16→I-10).

## 2. Última validación real

La última validación manual de comportamiento sigue siendo I-02 sobre `b0de31d`, después del rebase
sobre `main`: el dueño cargó el DLL Debug del worktree en AutoCAD 2025 y confirmó el checklist
completo del dinámico modular, incluidos vistas, seguridad, BOM, persistencia, round-trip, escenario
legacy y rendimiento. No se realizó ni se requiere una validación nueva en AutoCAD para I-06 porque
su alcance es documental.

La guía vigente para futuras validaciones está en
[guias/validacion-manual-autocad.md](guias/validacion-manual-autocad.md).

I-26 no requirió validación en AutoCAD. El dueño confirmó el CI de rama, incluidos tests y build UI,
y descargó el artifact de cobertura con el XML esperado antes de autorizar su integración.

I-13 tampoco cambia dibujo ni comportamiento de runtime. El dueño autorizó la integración después
de verificar el build limpio del Plugin y la aplicación documental de I-29; no se requiere una
validación adicional mediante NETLOAD.

I-09 no cambia dibujo ni comportamiento de runtime (refactor de la superficie de comandos del
Plugin). No requirió validación en AutoCAD: la equivalencia se sostiene con el inventario de los 26
`[CommandMethod]`, la revisión mecánica del refactor y el CI verde de la rama. AutoCAD: no ejecutado;
no requerido por contrato al conservar comportamiento mediante equivalencia mecánica, builds y CI.

I-08 no cambia dibujo ni BOM (refactor de persistencia/biblioteca de Application con adaptación mínima
de UI). No requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca con ✋). La
**owner-validation quedó aprobada** por el dueño: recorrió el checklist de la biblioteca de diseños —las
cinco etiquetas, abrir un diseño de cada tipo con su editor correcto, una cabecera legacy sin `Kind`— y
confirmó que no cambió dibujo, BOM ni edición posterior. AutoCAD: no ejecutado como validación formal de
geometría; no requerido por contrato. La equivalencia se sostiene con la caracterización golden/round-trip
y el CI verde de la rama.

I-16 (`refactor/draw-services`) SÍ cambia la superficie de dibujo del Plugin y por eso se validó en AutoCAD:
el dueño cargó por `NETLOAD` el DLL Debug del worktree I-16 (build sobre `2d276a6`, SHA-256 `6AEF0F4D…906B`)
en AutoCAD 2025 y **aprobó** la matriz por familia (selectivo, dinámico con `postIndex` y entrada/salida,
cama, cabecera, cancelación del jig, persistencia y edición posterior) **sin observaciones**; registro en
[initiatives/I-16-autocad-validation.md](initiatives/I-16-autocad-validation.md). El avance del trunk por
I-08 cambia solo Application/UI y **no toca la superficie de dibujo del Plugin**, por lo que esa validación
se conserva tras el rebase (WORKFLOW §6).

I-10 (`architecture/kind-handlers`) **no** cambia dibujo, geometría, BOM funcional, GUID ni persistencia: es
un refactor del despacho por el `Kind` del sobre en el Plugin cuyos handlers son fachadas delgadas hacia el
código de edición/BOM/restamp **sin cambiar** (no toca la superficie de dibujo, a diferencia de I-16). No
requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca con ✋) ni owner-validation
(`requires_owner_validation: false`), por analogía directa con I-09: la equivalencia se sostiene con el
inventario mecánico (26 `[CommandMethod]` idénticos a `origin/main`, 0 `switch`/cadena por el `Kind` del
sobre restante, 5 `Guid.NewGuid`, 7 `Regen`, mensajes y etiquetas verbatim), la suite completa y el CI verde
de la rama. AutoCAD: no ejecutado; no requerido por contrato.

## 3. Problemas y riesgos activos

- `ParrillaFrente` y `ParrillaCantidad` siguen siendo globales al rack; una configuración
  heterogénea puede requerir overrides por frente o nivel en una iniciativa futura.
- En medio frente, la cantidad de parrilla es por tramo; el comportamiento es intencional, pero
  debe comprobarse contra el uso real.
- El build del Plugin puede emitir los `MSB3277` conocidos de las referencias de AutoCAD y falla al
  copiar DLL si AutoCAD los mantiene cargados.
- `RackDynamicSystemWindow.xaml.cs` conserva deuda de code-behind; I-21 define su migración futura.
- El fallback legacy del dinámico conserva cabeceras sin procedencia como personalizadas para evitar
  pérdida de datos.
- Los catálogos de producto y los overrides del usuario aún comparten ubicación; I-04 preserva el
  DWG de bloques, pero la separación de capas de datos sigue diferida.
- La compilación del Plugin en GitHub-hosted runners depende de la excepción limitada de ADR-0003.
  Debe revisarse a más tardar el 2027-07-20 o antes ante cambios de proyecto, versiones, source,
  runner, caching, artifacts, audiencia, finalidad o documentación incompatible de Autodesk.
- GitHub Actions advierte que las acciones actuales basadas en Node.js 20 se ejecutan forzadamente
  sobre Node.js 24; es una deuda de infraestructura separada de I-13.

## 4. Siguiente acción

Con I-08, I-09, I-16 e **I-10** integradas y limpias, la **pista B del Plugin queda cerrada** (la
serialización I-09 → I-16 → I-10 está completa). Las pistas abiertas son la **A de Application** (I-11
`architecture/persistencia-uniforme`, con dependencias satisfechas; estorba con I-03 e I-08) y la **C de UI**
(I-14 `architecture/ui-controls` → I-15 `architecture/editor-shell`). I-18 (Push Back) sigue bloqueada por
I-11, I-15, I-16 y los bloques DWG del dueño. El siguiente paso es abrir una de esas iniciativas respetando
dependencias y estorbos, o continuar I-07 (`docs/adr-retroactivos`) en su worktree ya reclamado.

La automatización permanece pausada: no hay ejecutor nocturno activo ni horarios programados. El
desarrollo posterior continúa manualmente bajo WORKFLOW hasta que el dueño apruebe otro mecanismo y
un nuevo piloto controlado.

## 5. Última verificación vigente

**Baseline integrada de I-10 — 2026-07-21:**

- punta de implementación revisada de `architecture/kind-handlers`:
  `532eb038306a3de68277496ce47457f270200944`; este documento **no inventa** el SHA del merge de `main` (vive
  en `git log --first-parent main`);
- `origin/main` no avanzó desde la base de I-10 (`c5a4082`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **694/694 verdes**, sin fallos ni omitidas (línea base y post-refactor idénticas;
  Domain+Application no se tocan);
- build UI Debug: **0 errores y 0 advertencias**; build solución completa Debug: **0 errores**, únicamente
  las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `532eb03` (run `29836270208`, `headSha 532eb038…0944`): los tres jobs (Tests,
  Build UI, **Build Plugin without AutoCAD**) en `success`;
- objetivo entregado: `IRackKindHandler` + `KindHandlerRegistry` en `src/RackCad.Plugin/KindHandlers/`
  (registro explícito, inmutable, sin reflexión, que rechaza handlers nulos, claves vacías y duplicadas;
  `TryGet` Ordinal + `TryGetIgnoreCase`) con los cuatro handlers embebidos (`selective`, `dynamic`,
  `cabecera`, `cama`) y `Default`; RACKEDITAR, RACKBOMTOTAL (`BuildRackBom` + `KindLabel`) y el restamp
  despachan por el registro; **0** `switch`/cadena por el `Kind` del sobre restante en alcance; `Larguero` no
  registrado;
- equivalencia mecánica: **26 `[CommandMethod]`** idénticos a `origin/main`, cero duplicados; **5**
  `Guid.NewGuid` (una en el restamp); **7** ubicaciones de `Regen`; mensaje "tipo de rack no reconocido" y
  etiquetas de BOM (Selectivo/Dinámico/Cabecera/Cama) **verbatim**; constantes de Kind/View intactas;
- frontera de registros: `SystemRegistry` (Application, I-08, `RackSystemKind`) y `KindHandlerRegistry`
  (Plugin, string del sobre) **no** se unifican; `RackListBuilder` (Application, RACKLISTA) queda intacto por
  la dirección de dependencias (Application no depende del Plugin);
- alcance: producto solo en `src/RackCad.Plugin` (6 archivos nuevos en `KindHandlers/` + 3 consumidores:
  `RackMenuCommands`, `RackInventarioCommands.BomTotal`, `RackEnvelopeRestamp`); documentación el contrato de
  I-10 y su índice; sin cambios en Domain/Application/UI/catálogos/deploy/csproj; sin dependencias nuevas;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`; ROADMAP no marca I-10
  con ✋); owner-validation no requerida (`requires_owner_validation: false`), por analogía directa con I-09;
  no se declara AutoCAD validado.

**Baseline integrada de I-08 — 2026-07-21:**

- punta de implementación revisada de `architecture/system-registry`:
  `997fb8e459af11f0d42ac0eb13029cb8c4b287d3`; este documento no inventa el SHA futuro del merge de `main`;
- `origin/main` no avanzó desde la base de I-08 (`0849152`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **686/686 verdes**, sin fallos ni omitidas; incluye la caracterización golden
  F1–F1.1 (wire format PascalCase, schema `2.0`, cinco nombres de enum, nulos, legacy sin `Kind`,
  reconstrucción física, `kind` sin payload, degenerado, versión futura, string desconocido, `Kind: 999`,
  reglas laxas de cama/larguero, etiquetas/precedencia/orden/tolerancia) verde tras el refactor;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama verde sobre `997fb8e`: los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) en
  success;
- objetivo entregado: `SystemDescriptor` + `SystemRegistry` en Application como fuente única de los cinco
  `RackSystemKind`; `RackProjectStore`, la validación genérica y `RackDesignLibrary` despachan por el
  registro; **`RackDesignKind` y `MapKind` eliminados por completo** (búsqueda global de `RackDesignKind`
  en `.cs`/`.xaml` = cero); sin `switch`/cadena por `RackSystemKind` en el store ni en la biblioteca; sin
  un segundo registro manual de los cinco sistemas;
- compatibilidad preservada: formato JSON, schema, nombres persistidos del enum, fallback legacy y
  etiquetas visibles idénticos; `RackSystemKind` intacto (sin renombrar ni reordenar);
- owner-validation **aprobada** por el dueño (checklist de biblioteca, cinco etiquetas, editores y
  cabecera legacy); AutoCAD no ejecutado ni requerido (`requires_autocad: false`); no se declara
  validación formal de geometría;
- alcance: producto en `RackProjectStore.cs`, `RackDesignLibrary.cs`, `RackMainMenuWindow.xaml.cs` y los
  tipos nuevos del registro (`SystemDescriptor`, `SystemRegistry`, `SystemRegistry.Default`); tests; el
  contrato de I-08 y su índice; sin cambios en `src/RackCad.Plugin`, `RackEmbedDocument`,
  `RackListBuilder`, DrawServices, DTOs, geometría, BOM ni catálogos; sin dependencias nuevas. **I-10 e
  I-16 fuera de alcance.**

**Baseline integrada de I-16 — 2026-07-21:**

- punta de rama integrada `f3a84bc44faf498c94dcd26b0d469f33e49a697a` (rebaseada sobre `origin/main` `549870b`
  tras integrarse I-08); integrada en `main` por **merge `--no-ff` `2c3bee734511740ab8636894c29a74687ab1cafd`**
  (primer padre `549870b`, segundo padre `f3a84bc`);
- suite `RackCad.Tests`: **694/694 verdes** sobre el árbol combinado (686 de `main`/I-08 + 8 golden de I-16),
  sin fallos ni omitidas (build local con el SDK de usuario);
- build UI Debug: **0 errores y 0 advertencias**; build Plugin Debug: **0 errores**, únicamente las dos
  familias `MSB3277` conocidas;
- CI de rama verde en los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) sobre la punta `f3a84bc`;
- validación manual en **AutoCAD 2025 aprobada** por el dueño sobre el DLL Debug del worktree I-16 (build de
  la punta F4, SHA-256 `6AEF0F4D5A49B89F6F5AAA35D4E287715473641E81D379B4BC671B55CC52906B`), matriz por
  familia (selectivo, dinámico con `postIndex`/entrada-salida, cama, cabecera, cancelación del jig,
  persistencia) sin observaciones (registro en `initiatives/I-16-autocad-validation.md`); el avance por I-08
  no toca la superficie de dibujo, así que la validación se conserva (WORKFLOW §6);
- equivalencia mecánica: las siete fachadas `*DrawService` conservan firmas públicas; infraestructura
  compartida extraída (`RackCatalogLoader`, `BlockPlacement`, `ViewBlockDraw`, `SystemBlockWriter.ApplyRegen`);
  invariantes preservados (nombres de bloque y sufijos, mensajes, `postIndex`, `DynamicRackEnd`, all-loose,
  payload/GUID, BOM, geometría, persistencia y **7 ubicaciones efectivas de `Regen`**);
- alcance: producto solo en `src/RackCad.Plugin`; golden solo en `tests/RackCad.Tests`; documentación el
  contrato, la línea base y el registro de validación de I-16; sin cambios en Domain/UI/catálogos/deploy;
- diff del merge contra su primer padre (`549870b`): únicamente el alcance acumulado de I-16 (19 archivos);
  **I-08 permanece intacta** (`RackDesignKind` eliminado, `SystemRegistry` presente).

**Baseline integrada de I-09 — 2026-07-20:**

- punta de implementación revisada de `refactor/plugin-commands`:
  `09de768cc7dfabdd29e313b4d8798abd783ec4a9`; este documento no inventa el SHA futuro del merge de
  `main`;
- `origin/main` no avanzó desde la base de I-09 (`6136fcb`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama verde sobre `09de768`: los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) en
  success;
- equivalencia mecánica verificada: **26 `[CommandMethod]`** con nombres idénticos a `origin/main`,
  cero duplicados, **13 principales + 13 aliases** con los mismos destinos; conjuntos idénticos de
  literales de código (prompts, keywords, mensajes, `SetRejectMessage`, nombres de bloque), `case`
  por `Kind`, DrawServices, stores, colores ACI, `directOnly`/`forceValidity`; conteos iguales de
  `Regen` (7), `Guid.NewGuid` (5), purgas (6) y `catch`/`Report` (18); el tipo y los archivos
  `RackFrameCommands` quedaron eliminados;
- alcance: producto solo en `src/RackCad.Plugin`; documentación solo el contrato de I-09 y su índice;
  sin cambios en Domain/Application/UI/tests/catálogos/deploy; sin dependencias nuevas; sin
  `[assembly: CommandClass]`;
- AutoCAD: no ejecutado; no requerido por contrato al conservar comportamiento mediante equivalencia
  mecánica, builds y CI (ROADMAP no marca I-09 con ✋).

**Baseline integrada de I-13 — 2026-07-20:**

- punta técnica final `849dff931ac5055c955ea2371c2388ec279b74b4`, contenida en `main` por
  `773feea3732497e04746c45451eb1b4e775d8961`;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama #64: tests, build UI y build Plugin without AutoCAD verdes; único artifact
  `rackcad-coverage-cobertura`, sin artifacts del Plugin ni material Autodesk;
- la validación post-merge #63 detectó que el job conservaba la condición temporal de la rama de
  promoción; I-13 la retiró antes de la limpieza para que el Plugin se compile en cada push;
- CI post-merge #65 verde sobre `773feea3732497e04746c45451eb1b4e775d8961`: ejecutó los tres
  jobs, incluido Build Plugin without AutoCAD, y publicó solo la cobertura Cobertura;
- las tres anotaciones de CI son la deprecación heredada de Node.js 20 en las acciones usadas;
- ADR-0003 aceptado con decisión I-29 B, matriz 14/14, rollback y nueva revisión obligatoria;
- AutoCAD: no ejecutado ni requerido porque la iniciativa cambia infraestructura de compilación y
  documentación, no dibujo ni runtime;
- evidencia experimental y respaldo pre-rebase conservados en las etiquetas
  `archive/i-13-experiment-final-4e084d2` y `archive/i-13-pre-rebase-a6febd2` antes de retirar las
  ramas y worktrees de I-13.

**Baseline integrada de I-26 — 2026-07-19:**

- punta de implementación validada de `refactor/test-catalog-ids`:
  `2cf3f12684dbe495403f0a16eeaa882e4873e3c6`;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- guardián de catálogos canónicos: verde contra IDs, bloques/vistas, conexiones, relaciones
  esenciales, defaults, plantillas y constantes equivalentes de producto;
- build UI Debug: **0 errores y 0 advertencias**;
- cobertura local observada: **91.77 % de líneas** y **75.26 % de ramas** en `RackCad.Domain` y
  `RackCad.Application`; es evidencia, no un umbral contractual;
- CI de rama #40: verde sobre la punta validada, según confirmación del dueño; el artifact
  `rackcad-coverage-cobertura` fue descargado y contiene `coverage.cobertura.xml`;
- diff bajo `src/`, `assets/catalogs/` y `deploy/`: vacío; no hubo cambios de producto ni datos;
- AutoCAD: no ejecutado ni requerido para esta iniciativa de infraestructura de pruebas;
- el commit documental final de integración requiere su propio CI antes del merge y no se declara
  verde anticipadamente.

**Baseline documental de I-06 que lleva este merge — 2026-07-17:**

- punta validada de `docs/reestructura`: `39cd54189457e8737f08cf95dbf948bc2e564dd3`;
- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente los `MSB3277` conocidos;
- `git diff origin/main --check`: limpio;
- documentación Markdown: **52 documentos**, **123 enlaces locales** y **0 enlaces rotos**;
- Context Packs: nueve IDs únicos, con rutas, globs, gates y exclusiones válidos;
- diff bajo `src/`: solo el comentario XML autorizado en `RackCommandReference.cs`;
- CI de rama: verde para `39cd54189457e8737f08cf95dbf948bc2e564dd3`, según la confirmación del
  dueño; la corrección administrativa posterior requiere repetir CI antes del nuevo merge;
- AutoCAD: no ejecutado ni requerido para esta iniciativa documental.

La baseline integrada anterior correspondía a I-04 (`8e52828` como punta de integración):

- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug y Release: **0 errores**, únicamente las familias `MSB3277` conocidas;
- harness del instalador: **25/25 verificaciones** en rutas temporales;
- CI de I-04: Success sobre `f82a49f`.

La evidencia técnica de la rama I-06 se conserva bajo [automation/runs/](automation/runs/). Este
documento no inventa el SHA futuro del merge de `main`.

## 6. Preguntas abiertas

1. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global según el
   uso real?

## 7. Decisiones vigentes pendientes de I-07

> **Conservación temporal obligatoria:** estas decisiones proceden de la antigua sección 7 y aún
> gobiernan el proyecto. Solo podrán retirarse de HANDOFF cuando I-07 integre los ADRs retroactivos
> correspondientes. I-06 no las convierte en ADR ni reabre su contenido.

| Decisión | Motivo / alcance vigente |
|---|---|
| Solo `RackCad.Plugin` toca AutoCAD | Geometría y BOM permanecen puros y testeables en Application. |
| Catálogos CSV Excel-first, sin base de datos | El usuario los edita en Excel; se conserva fallback Windows-1252 y caché por firma. |
| Un solo `secciones.csv` con columna `rol` | Postes, celosía, largueros y separadores comparten una hoja y FKs explícitas. |
| Identidad por GUID embebido en DWG | El nombre visible no es una identidad estable. |
| `Actualizar` redibuja; `Insertar` agrega una vista ligada | Convención permanente de los cuatro editores. |
| Parámetros dinámicos mediante patrón ARRAY | Evita fijar parámetros repetidamente por referencia. |
| Cero dependencias NuGet en producto, salvo ADR-0003 | La única excepción es `AutoCAD.NET [25.0.1]` condicional y compile-only en `RackCad.Plugin`, con versiones transitivas fijadas, sin runtime ni distribución y sujeta a nueva revisión. |
| Parrilla: una por tarima y regla en `SelectiveFrontalBuilder.ParrillaRow` | Dibujo, BOM y UI concuerdan por construcción. |
| Copia de `SelectiveSafetySelection` centralizada en `DeepCopy` | El DTO sigue explícito para compatibilidad y round-trip. |
| Entrada numérica localizada sin agrupadores | Acepta punto o coma decimal sin transformar valores ambiguos. |
| Cantidad de parrilla: UI rechaza si no cabe y builder acota | Evita dibujo fuera del marco y degrada de forma segura tras cambios. |
| Validación de cargas diferida a RAM Elements | No debe re-proponerse dentro del alcance actual. |
| Optimizador IA de layout diferido | `RACKLAYOUT` es el motor determinista vigente, no el optimizador futuro. |
