# Project Handoff

> Documento canonico de continuidad entre sesiones (Claude, Codex o un desarrollador nuevo).
> Actualizado: **2026-07-17**. **`main` es el trunk unico** (migracion Git I-00 ejecutada; ver seccion 8).
> **I-02 ejecuto ADR-0002 opcion A**: `codex/dinamico-modular` fue renombrada a
> `feature/dinamico-modular`, rebasada sobre `main` y validada post-rebase (suite combinada
> **635/635**, builds Debug verdes, CI verde y checklist manual del dueno OK en AutoCAD 2025);
> este commit documental es el ultimo de la rama antes del merge `--no-ff` (ver secciones 8-12).
> El arbol incluye el sistema dinamico modular multivista completo, las variantes de seguridad
> (Bota C, Lateral C, Poste tope, desviadores A/L) y los 6 arreglos de la revision exhaustiva.
> Regla de mantenimiento: este archivo describe ESTADO y CONTEXTO; las convenciones estables viven en
> [AGENTS.md](../AGENTS.md) y la vista general en [README.md](../README.md). Al cerrar una sesion de trabajo
> significativa, actualizar las secciones 8-12 de este archivo.

## 1. Resumen ejecutivo

**RackCad** es un plugin de AutoCAD 2025 (.NET 8, C#/WPF) para **disenar y dibujar racks industriales**
y obtener su **BOM** (lista de materiales) para cotizacion. Lo usa el dueno del repo (ingenieria de racks)
como herramienta interna de dibujo tecnico.

Maneja **cuatro tipos de rack** (cabecera/marco, sistema dinamico pallet-flow, cama de rodamiento y
selectivo), cada uno con ventana editora WPF, dibujo por bloques en AutoCAD y **round-trip de edicion**
(seleccionar un rack dibujado y reabrir su editor con `RACKEDITAR`). Los dos modulos ricos son el
**selectivo** (matriz frentes x niveles dirigida por tarima, tres vistas ligadas por GUID, doble
profundidad, medio frente, cotas, seguridad y BOM por componentes con export CSV/XLSX) y, desde I-02,
el **dinamico modular multivista** (matriz frente x nivel con fondos y niveles variables, laterales
por poste, frontales de salida/entrada, planta, camas integradas, largueros IN/OUT e intermedios,
seguridad multivista y BOM por componentes).

**Estado**: activo y funcional. El arbol actual tiene **635/635 tests verdes** y build Debug completo con 0 errores;
solo aparecen los `MSB3277` conocidos de las referencias de AutoCAD. El sistema dinamico modular esta validado
post-rebase por el dueno en AutoCAD 2025 (checklist completo OK, seccion 9). La base publicada conserva su validacion
manual de parrilla, larguero tope, rejilla, persistencia, biblioteca y rendimiento; Bota C 4/6, Poste tope,
Lateral C 4/6 y los siete desviadores A/L siguen verificados con sus bloques DWG reales.

## 2. Estado comprobado

| Aspecto | Estado | Evidencia |
|---|---|---|
| Compilacion Debug (Domain/Application/UI/Plugin) | **OK, 0 errores** | builds UI y Plugin post-rebase; 2 familias MSB3277 conocidas, 2026-07-17 |
| Pruebas unitarias | **635/635 verdes, 0 omitidas** | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug`, 2026-07-17 (arbol rebasado de I-02) |
| CI (workflow `CI`: tests ubuntu + build UI windows) | **Success** | confirmado por el dueno sobre `b0de31d` |
| Carga en AutoCAD (NETLOAD del Debug post-rebase) | **OK, confirmado por el dueno** | dinamico multivista completo, seguridad, BOM, round-trip y legacy; 2026-07-17 (seccion 9) |
| Commits `bd76444`..`b8b4db5` (sistema dinamico modular multivista, I-02) | Implementado, testeado y verificado en AutoCAD post-rebase | Ver secciones 8-9 |
| Commits `b1cfce2`..`95e25f2` (tope medio-frente, parrilla) | Implementado, testeado y verificado en AutoCAD | Ver seccion 9 |
| Commits `aa42986`, `c11a267`, `a9f1c13` (variantes de seguridad y desviadores) | Implementados, testeados, verificados en AutoCAD y publicados | Ver secciones 8-9 |
| Release build / bundle de despliegue | No reconstruido en la ultima sesion | `deploy/install-bundle.ps1 -Build` requiere AutoCAD cerrado |

## 3. Estado por funcionalidad

| Funcionalidad | Estado | Codigo principal | Pruebas | Observaciones |
|---|---|---|---|---|
| Cabecera (marco): editor + dibujo + round-trip | completo | `src/RackCad.UI/RackFrameConfiguratorWindow*`, `src/RackCad.Plugin/Headers/` | `tests/RackCad.Tests/*Frame*` | Peralte de placa editable por placa |
| Sistema dinamico (pallet flow): matriz frente x nivel, fondos/niveles variables, 4 vistas (laterales por poste, frontal salida, frontal entrada, planta), camas integradas, IN/OUT e intermedios, BOM | completo y verificado en AutoCAD post-rebase (I-02) | `RackDynamicSystemWindow`, `DynamicRackDesign` -> `DynamicRackSystemResolver`, `Dynamic*Builder`, `DynamicFlowBedLateralBuilder`, `SystemBomBuilder` | `Dynamic*Tests`, `DynamicSystemMultiViewBuilderTests`, `SystemBomBuilderTests` | Cama del nivel reutiliza `FlowBedLateralBuilder`; patron ARRAY para perf; deuda de code-behind -> I-21 |
| Seguridad del dinamico: botas/laterales/desviadores compartidos + **defensa de montacargas** y **guia de entrada** | completo y verificado en AutoCAD post-rebase (I-02) | `DynamicSafetyMultiViewBuilder`, `DynamicForkliftDefensePlan`, `DynamicEntranceGuidePlan`, `SafetyDefensaGridWindow`, `SafetyGuiaEntradaGridWindow` | `DynamicSafetyDefaultsTests`, `DynamicForkliftDefensePlanTests`, `DynamicEntranceGuidePlanTests` | Inventario fisico sin duplicar por vista; desviador de entrada con orientacion correcta |
| Cama de rodamiento (flow bed) | completo | `RackFlowBedWindow`, `FlowBed*` | `FlowBed*Tests` | Rodillos por paso minimo; capacidad = fase futura |
| Selectivo: matriz, 3 vistas, doble profundidad, medio frente, cotas | completo | `RackSelectiveWindow`, `src/RackCad.Application/Systems/Selective*` | `Selective*Tests` | El modulo mas activo |
| BOM por componentes + consolidado (`RACKBOMTOTAL`) + export CSV/XLSX | completo | `SelectiveBomBuilder`, `RackBomWindow`, `XlsxWriter` | `SelectiveBomBuilderTests`, `Xlsx*Tests` | XLSX es OOXML escrito a mano, sin dependencias |
| Seguridad: bota, protector lateral, separador, tope de larguero | completo | `SelectiveSafetyPlacement`, `SelectiveSafetyWindow`, `SafetyTopeGridWindow` | `SelectiveSafetyTests` | Catalogo en `assets/catalogs/seguridad.csv` |
| Seguridad: **Bota C 4/6 + Poste tope** | completo y verificado en AutoCAD | `SelectiveSafetyFamilies`, `SelectiveSafetyWindow`, builders existentes | `SelectiveSafetyTests`, `SelectivePerFondoTests` | Variantes por `ElementId`; una seleccion exclusiva por familia |
| Seguridad: **Lateral C 4/6** | completo y verificado en AutoCAD | catalogos + regla `LATERAL` existente | `SelectiveSafetyTests` | Reutiliza colocacion, reemplazo de botas, BOM y selector del Lateral H |
| Seguridad: **Desviadores A/L** | completo y verificado en AutoCAD | `SelectiveDesviadorPlan`, `SelectiveDesviadorDrawing`, `SafetyDesviadorGridWindow` | `SelectiveDesviadorTests` | 7 variantes por `ElementId`; una regla/rejilla comun; lado Izquierdo/Derecho/Ambas |
| Seguridad: **parrilla / deck** (una por tarima, frente y cantidad manuales, cuenta en vivo) | completo y verificado en AutoCAD | `SelectiveFrontalBuilder.ParrillaRow`, `SelectiveParrillaPlan`, `SafetyParrillaGridWindow` | `SelectiveSafetyTests` (Parrilla_*), `SelectiveMedioFrenteTests` | Ver secciones 8-9 |
| Tarima como referencia visual (frontal + lateral, sin BOM) | completo | `SelectiveFrontalBuilder.AddPallets`, bloque `TARIMA_GENERICA` | `SelectiveFrontalBuilderTests` | Planta = futuro |
| Identidad + round-trip (GUID en Xrecord, `RACKEDITAR`, `RACKDUPLICAR`, `RACKLISTA`) | completo | `RackEmbedDocument`, `RackFrameCommands` | round-trip tests por store | Convencion Actualizar/Insertar en las 4 ventanas |
| Layout de almacen v1 (`RACKLAYOUT`, `RACKRELLENAR`) | completo (v1) | `WarehouseGridPlanner`, comandos en Plugin | `Warehouse*Tests` | Solo motor de colocacion; el optimizador IA es meta futura |
| Bibliotecas (disenos + bloques DWG) | completo | `RackDesignLibrary*`, settings en `%APPDATA%\RackCad\settings.json` | — | `blocks-library.dwg` NO esta en el repo (ver seccion 6) |
| Validacion de cargas / capacidad | **diferido a proposito** | — | — | Ira con RAM Elements; NO re-proponer |
| Guardas traseras (seguridad) | pendiente, prioridad final | — | — | No retomarlas hasta cerrar las areas funcionales prioritarias |
| Rejilla de seguridad vs niveles resueltos | **corregido, testeado y verificado en AutoCAD** | `SelectiveSafetyGrid`, `RackSelectiveWindow.Safety_Click` | `SelectiveSafetyGridTests` | Ver secciones 8-10 |

## 4. Arquitectura

Cuatro proyectos en `RackCad.sln`, con dependencias en una sola direccion:

```
RackCad.Domain (net8.0, puro)          modelo: disenos, tarimas, selecciones
        ^
RackCad.Application (net8.0, puro)     geometria/calculo/BOM/persistencia; catalogos CSV; TESTEABLE
        ^
RackCad.UI (net8.0-windows, WPF)       ventanas editoras; NO referencia AutoCAD
        ^
RackCad.Plugin (net8.0-windows)        UNICO proyecto que toca la API de AutoCAD (adaptador delgado)
```

- **Flujo del selectivo** (el patron a imitar): `SelectivePalletDesign` (Domain, lo que el usuario tecleo)
  -> `SelectiveGeometryResolver.Resolve` -> `SelectiveRackSystem` (resuelto) -> builders puros
  (`SelectiveFrontalBuilder` / `SelectiveLateralBuilder` / `SelectivePlantaBuilder`) -> lista de
  `HeaderBlockInstance` -> drawers del Plugin que materializan bloques de AutoCAD.
- **Persistencia**: el diseno viaja EN el DWG — sobre `RackEmbedDocument` (JSON + GUID + nombre) en un
  Xrecord del diccionario de extension de la definicion de bloque. Stores por tipo
  (`SelectivePalletDesignStore`, `RackProjectStore`, `FlowBedConfigurationStore`) con version de esquema.
- **Catalogos**: CSV editables en Excel en `assets/catalogs/` (encoding con fallback Latin-1, cache por
  mtime). Se copian junto al ensamblado; `JsonRackCatalogProvider.FromBaseDirectory()` los resuelve en
  runtime. Detalle: [catalogos-y-plantillas.md](catalogos-y-plantillas.md) y [modelo-de-datos.md](modelo-de-datos.md).
- **Bloques**: cada pieza tiene un bloque por vista (`blocks.csv`); los que faltan se importan de una
  biblioteca DWG externa. Los parametros dinamicos se fijan por nombre case-insensitive
  (`ApplyDynamicParameters` + `RecordGraphicsModified`).
- Sin base de datos, sin servicios externos, sin red. Todo local.

## 5. Mapa del repositorio

| Ruta | Que es |
|---|---|
| `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | Modelo del selectivo, incl. `SelectiveSafetySelection` (flags de seguridad) |
| `src/RackCad.Application/Systems/` | Resolver, builders de vistas, BOM, geometria (todo lo interesante del selectivo) |
| `src/RackCad.Application/Persistence/` | Documentos DTO + stores (round-trip con fallbacks legacy) |
| `src/RackCad.Application/Catalogs/` | Carga de catalogos CSV/JSON -> `RackCatalog` |
| `src/RackCad.UI/` | Ventanas WPF (una por tipo de rack + dialogos de seguridad/BOM/biblioteca) |
| `src/RackCad.Plugin/` | Comandos de AutoCAD, drawers, jigs, embed en DWG |
| `assets/catalogs/` | Los CSV/JSON de datos (fuente de verdad de perfiles/bloques/seguridad) |
| `tests/RackCad.Tests/` | xUnit; corre sin AutoCAD ni Windows-only APIs |
| `deploy/` | Bundle del Autoloader + `install-bundle.ps1` |
| `docs/` | Documentacion; empezar por `00-indice-contexto.md` |

## 6. Configuracion y entorno

- **Requisitos**: Windows, .NET 8 SDK (o superior con TFM net8.0), AutoCAD 2025 completo (no LT) solo
  para probar el plugin. Las pruebas y los proyectos Domain/Application corren en cualquier OS.
- **RackCad no requiere variables de entorno ni secretos.** La unica configuracion runtime de la aplicacion es
  `%APPDATA%\RackCad\settings.json` (ruta de la biblioteca de bloques DWG; se edita desde el menu `RACKCAD`).
- En esta workstation, el SDK 8.0.423 esta instalado por usuario en `%LOCALAPPDATA%\Microsoft\dotnet`; el perfil
  PowerShell de usuario antepone esa ruta y define `DOTNET_ROOT` porque `C:\Program Files\dotnet` solo tiene runtime.
- **`blocks-library.dwg` NO esta versionado**: es el DWG del usuario con los bloques reales
  (p. ej. `TARIMA_GENERICA`, `PARRILLA_GENERICA_*`). Sin el, el dibujo reporta piezas faltantes y las
  omite (no aborta). Los nombres deben coincidir con `blockName` de `blocks.csv`.
- **Trampa conocida**: con AutoCAD abierto y el plugin cargado, los DLL del bin quedan bloqueados y el
  build falla en el paso de copia (MSB3021/MSB3027). Cerrar AutoCAD para reconstruir; para validar solo
  codigo, compilar a una carpeta temporal.
- Referencias de AutoCAD: apuntan a `C:\Program Files\Autodesk\AutoCAD 2025\` (avisos MSB3277 esperados).

## 7. Decisiones tecnicas

| Decision | Motivo | Estado |
|---|---|---|
| Solo `RackCad.Plugin` toca la API de AutoCAD | Testeabilidad: geometria/BOM puros en Application | vigente |
| Catalogos = CSV Excel-first (no BD) | El usuario los edita en Excel; cache por mtime; fallback Latin-1 | vigente |
| Un solo `secciones.csv` con columna `rol` (POSTE/CELOSIA/LARGUERO/SEPARADOR) | Una hoja para todos los perfiles; el provider los separa (API intacta) | vigente (2026-07-10) |
| Identidad por GUID embebido en el DWG (Xrecord), no por nombre de bloque | El nombre no es estable; el rack viaja con el archivo | vigente |
| Convencion Actualizar (redibuja en sitio) / Insertar (vista nueva enlazada) | Permanente en las 4 ventanas | vigente |
| Parametros dinamicos por patron ARRAY (HeaderGroup) al insertar | Fijar params por referencia era el cuello de botella | vigente |
| Cero dependencias NuGet en el codigo de producto (el XLSX es OOXML a mano) | Despliegue simple del plugin | vigente |
| **Parrilla: una por tarima; la regla de conteo vive en UN sitio** (`SelectiveFrontalBuilder.ParrillaRow`), consumida por frontal, BOM, lateral y editor | Dibujo, BOM y UI concuerdan por construccion, no por coincidencia | vigente (2026-07-14) |
| Copia de `SelectiveSafetySelection` centralizada en `DeepCopy` | Evita agregar un flag en resolver/UI/vista y olvidarlo en otro camino; el DTO sigue explicito por compatibilidad | vigente (2026-07-15) |
| Entrada numerica localizada sin separadores de miles | Acepta punto/coma sin convertir `96,5` en `965` | vigente (2026-07-15) |
| Cantidad de parrilla forzada: el dialogo la RECHAZA si no cabe; el builder ademas la ACOTA | El dialogo valida contra la matriz de ese momento; angostar despues degrada en vez de dibujar fuera del marco | vigente |
| Validacion de cargas DIFERIDA (ira con RAM Elements) | Decision explicita del usuario; no re-proponer | vigente |
| Optimizador IA de layout = meta futura; `RACKLAYOUT` es solo el motor de colocacion | El BOM ya da el costo; falta el optimizador beneficio/costo | planeado |

## 8. Trabajo realizado recientemente

**I-02 — integracion del sistema dinamico modular (`feature/dinamico-modular`) — completada el 2026-07-17:**

- Ejecuto ADR-0002 opcion A en una sesion (de las 3 permitidas): la punta validada `9f19a8c` se
  preservo con el tag `archive/dinamico-modular-pre-rebase-9f19a8c` (NO eliminar: punto de
  recuperacion permanente) y con `origin/codex/dinamico-modular` congelada; la rama se renombro a
  `feature/dinamico-modular` en el mismo worktree, se reclamo con commit vacio (Claim-Id
  `102a5306-1ec0-4582-806c-066007a5e851`) y se rebaso sobre `origin/main` (`cc43235`).
- **Conflictos: SOLO los 7 docs previstos por la evidencia de I-01** (README, docs/00, docs/01,
  docs/03, HANDOFF, catalogos-y-plantillas, ideas-futuras); cero conflictos de codigo o catalogos.
  Resolucion: estructura y estado de `main` canonicos; el contenido funcional del dinamico se
  conservo; se descarto el estado obsoleto (conteos viejos, `release/claude-review`).
- **Los 6 arreglos de la revision exhaustiva quedaron intactos** (sus archivos no estan en el diff
  de la rama): los 4 campos de cabecera + `SchemaGuard` en `RackFrameProjectDocument/Store`,
  `CsvCatalogReader`, el acote del paso en `FlowBedLateralBuilder`, `SelectiveBomBuilder` sin
  decoracion y `RackFrameCommands.List`.
- **Catalogos byte-identicos a la rama validada** (+38 filas append-only en 6 CSV; unica edicion
  no-append: `displayName` de `MENSULA_3_REMACHES_CAL_10` en `mensulas.csv`, intencional; encoding
  Latin-1/UTF-8 intacto, sin caracteres de reemplazo).
- Publicada con `--force-with-lease` en `b0de31d`; `range-diff` contra el tag sin diferencias fuera
  de la resolucion documental y el commit de reclamo.
- **Validacion automatizada post-rebase** (2026-07-17, SDK 8.0.423): suite completa **635/635
  verdes, 0 fallos, 0 omitidas**; subconjunto dinamico **138/138**; dirigidas:
  `RackFrameProjectStore` 11/11, `RackProjectStore` 21/21, `CsvCatalogReader` 9/9, `FlowBed` 21/21,
  `SystemBomBuilder` 16/16, `SelectiveBomBuilder` 7/7, `CatalogStandardConsistency` 5/5; build UI
  Debug **0 errores / 0 advertencias**; build Plugin Debug **0 errores** (solo las 2 familias
  `MSB3277` conocidas); `git diff --check` y working tree limpios. **CI Success sobre `b0de31d`**
  confirmado por el dueno.
- **Validacion manual post-rebase** (dueno, AutoCAD 2025, NETLOAD del DLL de `b0de31d`): checklist
  completo de 13 puntos OK — detalle en la seccion 9. Es la validacion que habilita el merge
  `--no-ff` (WORKFLOW seccion 4.5.3).

**Migracion Git I-00 — ejecutada y cerrada el 2026-07-17:**

- **`main` es el trunk unico de integracion** (rama por defecto en GitHub, protegida contra
  force-push y borrado, sin required checks). `release/claude-review` quedo retirada.
- Checkpoint 1 validado sobre `e47e81e`: merge de planificacion `99ebfa1` (auditoria + WORKFLOW +
  ROADMAP + ADRs) mas `global.json` (SDK .NET 8 fijado) incluido en `e47e81e`. Tests **554/554**
  verdes; build UI con **0 errores y 0 advertencias**; build Plugin con **0 errores** y las 2
  familias `MSB3277` conocidas.
- Tags de recuperacion creados en origin ANTES de la migracion:
  `archive/pre-i00-main-2026-07-17`, `archive/pre-i00-release-claude-review-2026-07-17` y
  `archive/catalogos-dinamico-local-pre-i00-2026-07-17` (este ultimo preserva los seis CSV
  relacionados con el dinamico encontrados en el worktree principal antes de I-00; NO integrar —
  I-01 verifico que es subconjunto estricto de la rama del dinamico, sin contenido exclusivo).
- Retiradas en esta misma operacion (todas archivadas o contenidas en `main`):
  `release/claude-review`, `claude/rackcad-architecture-audit-cd4046` (rama + worktree de la
  auditoria/planificacion), `codex/seguridad-variantes-topes-botas` (zombie: contenido ya
  integrado con otros hashes; arbol verificado equivalente a `c11a267` y punta preservada en
  `archive/seguridad-variantes-topes-botas-2026-07-17`) y `codex/app-tooling-catalogs-logging`
  (tooling historico de la era MVP, preservado en `archive/app-tooling-catalogs-logging-2026-07-17`).
- `codex/dinamico-modular` quedo entonces como el unico worktree de agente restante, intacta en
  `9f19a8c`, resuelta por [ADR-0002](adr/0002-secuencia-dinamico-modular.md) (**aceptado, opcion
  A**). I-02 la renombro a `feature/dinamico-modular` y la integro (ver la entrada de I-02 arriba).

**I-01 — decision ADR-0002 (Paso 0 + aceptacion) — completada el 2026-07-17:**

- Rama de la iniciativa: `docs/decision-dinamico-modular` (solo documentacion). La evidencia
  completa vive en [adr/0002-paso0-evidencia.md](adr/0002-paso0-evidencia.md).
- **Evidencia automatizada** sobre `codex/dinamico-modular` en `9f19a8c` (AutoCAD cerrado,
  verificado por proceso): restore OK; suite completa **627/627 verde, 0 omitidas**; subconjunto
  dinamico **138/138**; build UI Debug **0 errores / 0 advertencias**; build Plugin Debug
  **0 errores** (solo las 2 familias `MSB3277` conocidas); `git diff --check` y status limpios
  tras los builds. Ningun defecto automatizado.
- **Evidencia manual**: el dueno recorrio personalmente el checklist de 17 pruebas en AutoCAD 2025
  (NETLOAD del DLL Debug compilado dentro del worktree dinamico) y las informo **17/17 OK**:
  editor multi-frente, geometria de frentes/fondos/niveles variables, camas, largueros IN/OUT e
  intermedios, frontales de salida y entrada, laterales por poste, planta, seguridad, **BOM
  coherente**, **persistencia sin perdida**, **RACKEDITAR correcto**, **actualizacion en sitio
  correcta**, legacy, y el **desviador de la frontal de entrada con orientacion correcta** (el
  unico pendiente que la rama declaraba). Sin fallos bloqueantes.
- **La validacion de I-01 fue PRE-rebase** (sobre `9f19a8c`, sin los 8 commits de `main`): valio
  para la decision. I-02 re-valido despues sobre el arbol ya rebasado (WORKFLOW seccion 4.5.3);
  la baseline vigente es la post-rebase de la seccion 12.
- **Decision del dueno (2026-07-17): opcion A** — integrar primero mediante I-02. El alcance de
  I-02 y la contingencia (si no estabilizaba en 3 sesiones: detener y redactar un ADR nuevo que
  propusiera reemplazar ADR-0002 por la opcion B) quedaron en el ADR y en la evidencia. I-02 la
  ejecuto en una sola sesion de estabilizacion (entrada de I-02 arriba).

1. **`b1cfce2` Tope medio-frente**: los topes de larguero siguen los tramos de un frente partido
   (frontal + planta), un tope por tramo cargado.
2. **`38572c6` Parrilla v1**: elemento de seguridad PARRILLA (deck) dibujable en frontal + lateral,
   rejilla frente x nivel + toggles por vista, BOM por tramo.
3. **`66d03c7` Parrilla por tarima**: correccion de requisito — va UNA parrilla POR TARIMA (no por nivel);
   campo "Frente" manual; regla unica `ParrillaRow`; fix de decks fantasma en la lateral
   (`ParrillaExistsAt` antes del dedup por Y). Revision adversarial multi-agente confirmo y se corrigio.
4. **`95e25f2` Cantidad manual + cuenta en vivo**: campo "Cantidad" (conserva el ancho de la tarima),
   cuenta calculada por celda + total en el dialogo (`SelectiveParrillaPlan`), rechazo de cantidades que
   no caben, y fix del aviso "no cabe ninguna" que se contradecia con la celda (el aviso ahora sale del
   numero que se pinta; `MaxCountIn` ignora filas inherentemente vacias).

Verificacion realizada: 489 tests verdes; los tests de regresion se verificaron **fallando** con el fix
desactivado antes de darlos por buenos; dos revisiones adversariales multi-agente (los hallazgos
confirmados se corrigieron; el resto fue refutado con evidencia empirica).

**Lote cerrado 2026-07-15 (publicado, build/tests verdes):**

- `SelectiveSafetyGrid.LevelCounts` deriva las filas de niveles RESUELTOS y expone el maximo real entre fondos;
  elimina la fila de tarima de piso cuando no existe larguero.
- `AllCellsOff` ignora duplicados/indices legacy invalidos; evita descartar una seleccion por conteo inflado.
- Builders y BOM convierten `OffCells` una vez a `HashSet`; dejan de recorrer toda la lista por cada bloque/celda.
- `SelectiveSafetySelection.DeepCopy` centraliza la copia usada por resolver, vista por fondo y UI. Persistencia
  permanece explicita y con fallback.
- `LocalizedNumberParser` acepta punto/coma, prohibe agrupadores ambiguos y se usa en seguridad/layout.
- UX: textos de seguridad corregidos, aviso cuando la geometria no permite conteo, tipos/defaults sin literales
  repetidos; docs y manifest de comandos alineados.
- 14 casos nuevos (`LocalizedNumberParserTests`, `SelectiveSafetyGridTests`), total **503/503 verdes**.
- Regresion verificada con el fix temporalmente desactivado: 5/14 casos dirigidos fallaron (multi-fondo,
  duplicados/indices invalidos y parsing decimal/agrupadores); despues de restaurarlo, 503/503 verdes.

**Lote cerrado 2026-07-15 — variantes Bota C / Lateral C / Poste tope (publicado):**

- Se incorporaron los bloques `PROTECTOR_BOTA_C_4`, `PROTECTOR_BOTA_C_6` y `POSTE_3_1_5_8_TOPE` en las tres vistas.
- `seguridad.csv` los tipa como variantes `BOTA` / `TOPE`; reutilizan sin duplicar las reglas existentes.
- `SelectiveSafetyFamilies` centraliza la exclusividad y hace que dibujo, BOM y UI elijan el mismo `ElementId`.
- El dialogo muestra `Ninguno` + variantes en un combo por familia; los ids legacy H/larguero reabren seleccionados.
- Regresion comprobada antes del fix: 6 pruebas fallaron sin catalogacion; tras agregar los CSV pasaron. Luego 2
  pruebas de duplicados fallaron dibujando simultaneamente ambas variantes; con la regla central pasan. Total 511/511.
- UI y Plugin Debug compilan; el usuario confirmo en AutoCAD dibujo, seleccion y funcionamiento de Bota C 4/6 y Poste tope.
- Se agregaron `PROTECTOR_LATERAL_BOTA_C_4` y `_C_6` en las tres vistas. Las filas recibidas como `BOTA` se
  corrigieron a `LATERAL`, porque deben compartir exactamente la regla, reemplazo de botas y BOM del Lateral H.
- Regresion de Lateral C comprobada antes de catalogar: 4 de 8 casos dirigidos fallaron; despues pasan los 8 y la
  suite completa queda en 516/516. Plugin Debug compila y el usuario confirmo ambos laterales en AutoCAD.

**Lote cerrado 2026-07-15 — familia de desviadores A/L (publicado):**

- `seguridad.csv` cataloga `DESVIADOR_A_3`, `DESVIADOR_A_4`, `DESVIADOR_L_3`, `_L_3_5`, `_L_4`, `_L_4_5` y
  `_L_5`; cada id tiene bloques `FRONTAL`, `LATERAL` y `PLANTA`. El usuario confirmo que los siete reutilizan
  exactamente el mismo contrato de colocacion, parametros, espejo y BOM.
- `SelectiveDesviadorPlan` centraliza la rejilla poste x nivel, incluidos postes intermedios de medio frente, las dos
  caras exteriores expuestas al pasillo, el espejo posterior, el primer nivel sobre troquel y los niveles superiores
  6" debajo del larguero. El selector `Izquierdo/Derecho/Ambas` filtra la cara frontal/posterior o conserva ambas;
  dibujo, BOM y UI consumen el mismo plan y legacy cae en `Ambas`.
- `LONGITUD` y la altura del primer nivel son globales, 18" por defecto y aceptan solo enteros pares mayores de 8".
  La rejilla muestra una nota cuando el claro seleccionado es menor que la longitud y sugiere un par menor.
- Dibuja los bloques catalogados en frontal/lateral/planta; en planta el origen del desviador coincide con el origen
  del poste (sin offset `TROQUEL_LARGUERO`). El BOM cuenta las piezas fisicas de ambos frentes aunque frontal/planta
  colapsen proyecciones superpuestas. `ElementId`, dimensiones y celdas apagadas hacen round-trip.
- Regresion comprobada con la resolucion multi-variante desconectada: A4 y los cinco L dieron cantidad 0 en vez de
  12 (6/7 fallos esperados); restaurada la regla por `type`, pasan 30/30 casos dirigidos y 546/546 en la suite.
- Solucion y Plugin Debug compilan con 0 errores (solo las 2 familias MSB3277 conocidas). El usuario valido en
  AutoCAD los siete bloques, selector, rejilla, lados, dibujo, BOM y round-trip el 2026-07-15.

**Revision exhaustiva de codigo (2026-07-15, multi-agente + verificacion en linea; 554 tests):**

1. **Lector CSV — fila en blanco antes del header**: `CsvCatalogReader` tomaba `rows[0]` como header
   incondicionalmente; una fila en blanco (o de solo comas) que Excel deja arriba vaciaba TODO el catalogo en
   silencio (y `secciones.Count > 0` suprimia el fallback legacy). Ahora el header es la primera fila con
   contenido. Tests: `Read_SkipsLeadingBlankRows_BeforeTheHeader`, `Read_AllBlankRows_ReturnsEmpty`.
2. **Cama — paso de rodillo sin acotar**: un override diminuto ("0.01") generaba cientos de miles de
   instancias y congelaba la UI. El paso manual se acota al troquel (`Math.Max(grid, override)`).
3. **Persistencia de cabecera — 4 campos perdidos**: `RackFrameProjectDocument` no guardaba
   `DiagonalDoubleSpacingTroqueles`, `HorizontalDoubleOffsetTroqueles`, `PasoTroquel` ni `PanelClear`
   (geometria real; el clone del dinamico pasa por este store). Mapeados con fallback legacy + round-trip test.
4. **`RackFrameProjectStore` sin guardas**: ahora aplica `SchemaGuard.CheckReadable` + `IsUsableHeader`
   como sus hermanos ("{}" cargaba una cabecera degenerada de alto 0 en silencio).
5. **Perf del BOM**: las vistas de conteo se construyen SIN decoracion (tarimas/cotas/numeracion) — un rack
   grande con "Mostrar tarimas" materializaba miles de instancias solo para descartarlas, multiplicado por
   `RACKBOMTOTAL`. Con test de equivalencia (BOM identico y flags del caller restaurados).
6. **`RACKLISTA` — "Copias" coherente con el BOM**: sumaba las referencias de todas las vistas (frontal + 2
   cortes + planta = "4 copias"); ahora usa el maximo entre vistas, la misma agregacion que `RACKBOMTOTAL`.

Los hallazgos diferidos y los NO verificados (la verificacion adversarial agoto su limite de sesion a mitad
de la corrida) quedaron registrados en `docs/ideas-futuras.md` ("Hallazgos de la revision de codigo
2026-07-15"), junto con 14 features nuevas no mapeadas antes (items 21-34).

## 9. Ultima validacion manual

**`feature/dinamico-modular` en `b0de31d` (arbol YA rebasado sobre `main`) — 2026-07-17, AutoCAD
2025, NETLOAD del DLL Debug del worktree dinamico (SHA-256 en la seccion 12): el dueno recorrio el
checklist post-rebase de 13 puntos y los informo TODOS OK.** Es la validacion que habilita el merge:

- Carga (NETLOAD + `RACKCAD`), menu y editor dinamico correctos.
- Sistema multi-frente con fondos y niveles variables correcto.
- Camas dinamicas y paso de rodillos correctos (acote de `main` vigente).
- **Los cuatro campos de cabecera restaurados por `main` conservaron su valor en el round-trip.**
- Guardar, cerrar, reabrir y `RACKEDITAR` conservaron la configuracion; actualizar en sitio correcto.
- BOM del dinamico coherente; `RACKBOMTOTAL` combinado con otros sistemas coherente.
- Frontales (salida/entrada), laterales por poste y planta generados correctamente.
- Elementos de seguridad correctos; **el desviador de entrada mantuvo la orientacion correcta**.
- Escenario legacy probado correctamente; sin fallos bloqueantes, sin perdida de datos, sin bloques
  faltantes atribuibles al codigo; rendimiento aceptable.

**Validaciones anteriores (historia):**

**Rama `codex/dinamico-modular` en `9f19a8c` — 2026-07-17 (I-01 Paso 0, PRE-rebase):** 17/17 OK
informadas por el dueno (detalle en [adr/0002-paso0-evidencia.md](adr/0002-paso0-evidencia.md));
superada por la validacion post-rebase de arriba.

**Trunk `main` — AutoCAD 2025, NETLOAD del Debug, 2026-07-15: OK confirmado por el usuario.**
- Biblioteca: disponible y reportada correctamente por `RACKCAD`.
- Rejilla sin larguero a piso: filas resueltas correctas, sin fila muerta ni desplazamiento.
- Parrilla: frente/cantidad, posicion en frontal/lateral y BOM correctos.
- Tope: rejilla, posicion y opciones correctas.
- Variantes de seguridad: selectores exclusivos, Bota C 4/6, Poste tope, Lateral C 4/6 y los siete desviadores A/L
  funcionan correctamente en sus vistas y BOM.
- Persistencia/round-trip: configuracion conservada tras `RACKEDITAR` y actualizar.
- Rendimiento: sin degradacion perceptible en el escenario probado.

## 10. Problemas conocidos y deuda tecnica

1. **Desfase rejilla de seguridad <-> niveles resueltos: corregido, testeado y verificado en AutoCAD**. La UI usa
   `SelectiveSafetyGrid.LevelCounts(resolved)`; sin larguero a piso, 3 filas de diseno producen 2 filas reales.
   Compatibilidad: los builders siempre interpretaron `Level` como indice RESUELTO, por lo que se conserva el
   significado efectivo de `OffCells`; referencias a la antigua fila superior muerta se ignoran y desaparecen al
   volver a guardar. Tests y validacion visual de tope/parrilla estan verdes.
2. **`ParrillaFrente`/`ParrillaCantidad` son un valor unico para TODO el rack**: en racks con frentes de
   anchos distintos, un ancho que no cabe en el frente angosto deja ese frente sin parrilla (consistente
   en dibujo y BOM, y el dialogo lo avisa). Si el usuario necesita valores por frente/nivel, mover el
   override a la rejilla (deuda de diseno, no bug).
3. **Cantidad de parrilla es POR TRAMO en medio frente** (cada tramo es su propia posicion de carga):
   documentado en tooltip y docs; puede sorprender.
4. Avisos **MSB3277** al compilar el Plugin (conflictos de version de las refs de AutoCAD): conocidos,
   no bloquean.
5. Documentos historicos amplios en `docs/` (arquitectura, MVP, analisis VBA): utiles como referencia de
   decisiones, pero NO reflejan el estado actual; el indice los marca como historicos.
6. **`RackDynamicSystemWindow.xaml.cs` crecio a ~3,318 lineas de code-behind** (deuda heredada a
   sabiendas por ADR-0002; se paga en I-21 con la migracion al editor shell, igual que el selectivo).
7. **Fallback legacy conservador del dinamico**: una cabecera dinamica de un documento antiguo no
   declaraba si era calculada; se abre como personalizada para evitar perdida de datos.
   `Restaurar estandar` o `Calculada` vuelve a derivarla.

## 11. Siguientes tareas recomendadas

> El plan de ejecucion por fases e iniciativas vive en [ROADMAP.md](ROADMAP.md); esta seccion
> apunta a lo INMEDIATO. I-00, I-01 e I-02 ya estan cerradas (seccion 8).

1. **Elegir la siguiente iniciativa de la Fase 1 conforme a [ROADMAP.md](ROADMAP.md)** (quedan
   I-03/I-04/I-05/I-06/I-07/I-13/I-26): la eleccion es del dueno. Con I-02 integrada, las
   dependencias de I-08/I-09/I-11/I-14/I-16/I-17 quedaron satisfechas (ninguna esta iniciada);
   la limpieza post-merge de I-02 (rama, worktree) sigue las reglas de borrado seguro de
   WORKFLOW seccion 3 — el tag `archive/dinamico-modular-pre-rebase-9f19a8c` NO se elimina.
2. **Overrides de parrilla por frente/nivel** (item 2 de la seccion 10): mantener a mediano plazo; hoy los valores
   globales son suficientes, pero el control por celda puede aportar valor en configuraciones heterogeneas.
3. **Guardas traseras**: mantener pendientes hasta el final; no son prioridad de producto.

Quedan diferidos sin prioridad actual: tarima/parrilla en PLANTA, integracion BOM -> cotizador, distribucion formal
de `blocks-library.dwg` y reconstruccion del bundle Release. El BOM actual es suficiente; el cotizador real es un
Excel delicado y no justifica el riesgo de integracion. No es necesario desplegar la aplicacion todavia.

No tomar sin confirmar con el usuario: validacion de cargas (diferida a RAM Elements) y el optimizador IA
de layout (meta futura, no inmediata).

## 12. Verificacion del proyecto (ultima ejecucion real)

**Baseline vigente — arbol rebasado de I-02, `feature/dinamico-modular` en `b0de31d` (la
validacion POST-rebase que habilita el merge):**

| Verificacion | Comando | Resultado | Fecha/entorno |
|---|---|---|---|
| Suite completa | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` | **635/635 verdes, 0 fallos, 0 omitidas** | 2026-07-17, Windows 11, SDK 8.0.423 por usuario |
| Subconjunto dinamico | filtro `Dynamic\|RackProjectStore\|SystemBomBuilder\|CatalogStandardConsistency` | **138/138 verdes** | 2026-07-17 |
| Dirigidas por clase | `RackFrameProjectStore` / `RackProjectStore` / `CsvCatalogReader` / `FlowBed` / `SystemBomBuilder` / `SelectiveBomBuilder` / `CatalogStandardConsistency` | **11/11, 21/21, 9/9, 21/21, 16/16, 7/7, 5/5** | 2026-07-17 |
| Build UI Debug | `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` | **OK, 0 errores, 0 advertencias** | 2026-07-17 |
| Build Plugin Debug | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` | **OK, 0 errores**, solo las 2 familias MSB3277 conocidas | 2026-07-17 |
| Higiene del arbol | `git diff --check` + `git status` tras builds | limpios, sin cambios versionados generados | 2026-07-17 |
| CI (workflow `CI`) | tests ubuntu + build UI windows, push de `b0de31d` | **Success, confirmado por el dueno** | 2026-07-17 |
| Validacion manual AutoCAD | NETLOAD del DLL Debug del worktree dinamico (`src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`, SHA-256 `44CA6C759BCD6E7C36796B771C66DFA9CD54A9BE4C511C33A91F78ACDBDCA6F6`) + checklist post-rebase | **13/13 puntos OK informados por el dueno** (seccion 9) | 2026-07-17, AutoCAD 2025 |

**Corridas anteriores (historia; NO son la baseline actual):**

| Verificacion | Comando | Resultado | Fecha/entorno |
|---|---|---|---|
| **[I-01 Paso 0, PRE-rebase, `9f19a8c` — superada]** Suite completa | `dotnet test` en el worktree del dinamico | 627/627 verdes, 0 omitidas | 2026-07-17 (I-01 Paso 0) |
| **[I-01 Paso 0, PRE-rebase, `9f19a8c` — superada]** Subconjunto dinamico / builds / manual | filtro dinamico; builds UI y Plugin; checklist de 17 pruebas | 138/138; UI 0/0; Plugin 0 errores; 17/17 OK del dueno | 2026-07-17; detalle en adr/0002-paso0-evidencia.md |
| Build Debug (todo) | `dotnet build RackCad.sln -c Debug -v:minimal` | **OK, 0 errores, 2 advertencias MSB3277 conocidas** | 2026-07-15, Windows 11, SDK 8.0.423 por usuario |
| Build Plugin Debug | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -v:minimal` | **OK, 0 errores, 2 advertencias MSB3277 conocidas** | 2026-07-15 |
| Pruebas | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` | **554/554 verdes**, 0 omitidas | 2026-07-15 (revision exhaustiva) |
| Regresion de la revision exhaustiva (CSV header en blanco / paso de rodillo / DTO cabecera) | filtro `SkipsLeadingBlankRows\|TinyPitchOverride\|PreservesGridAndDoubleMember` con los fixes en stash | **3 fallos esperados de 3**; fixes restaurados y suite completa verde | 2026-07-15 |
| Regresion con fix desactivado | filtro `SelectiveSafetyGridTests|LocalizedNumberParserTests` | **5 fallos esperados de 14**; fix restaurado y suite completa verde | 2026-07-15 |
| Regresion Desviador A3 con integracion desconectada | `SelectiveDesviadorTests.Drawing_ProjectsTheSamePlanInThreeViews_AndBomKeepsPhysicalLevelCount` | **1 fallo esperado:** frontal 0 vs 6; fix restaurado, 17/17 dirigidas y suite verde | 2026-07-15 |
| Regresion origen A3 en planta | mismo caso dirigido, exigiendo insercion igual al origen del poste | **1 fallo esperado:** 4/4 referencias conservaban offset del troquel; fix restaurado y suite verde | 2026-07-15 |
| Regresion lado A3 | `SelectiveDesviadorTests.SideChoice_FiltersAisleFacesForDrawingAndBom` | **2 fallos esperados:** Izquierdo/Derecho daban 12 piezas en vez de 6; fix restaurado, 3/3 y suite verde | 2026-07-15 |
| Regresion variantes A/L | `SelectiveDesviadorTests.EveryVariant_ReusesTheSamePlanDrawingAndBomRule` | **6/7 fallos esperados:** A4/L daban 0 piezas al limitar la resolucion a la primera variante; restaurado, 7/7 verdes | 2026-07-15 |
| Validacion estatica auxiliar | Roslyn de PowerShell + referencias .NET 8/xUnit en cache; parse XML | **OK:** sintaxis 233 C#; semantica Domain/Application, UI y tests; 12 XML/XAML bien formados | 2026-07-15; no sustituye `dotnet build/test` ni la generacion XAML actual |
| Lint / format | — | no aplica (no hay linters configurados) | — |
| Release / bundle | `pwsh deploy/install-bundle.ps1 -Build` | **no ejecutada** en esta sesion (requiere AutoCAD cerrado y no era necesaria) | — |
| Verificacion manual AutoCAD | NETLOAD + `RACKCAD`/`RACKSELECTIVO`/`RACKEDITAR` | Bota C/Tope/Lateral C + siete desviadores A/L **OK** | 2026-07-15 |

## 13. Preguntas abiertas

1. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global? (mediano plazo, segun uso real)

(La pregunta sobre el alcance del cierre del dinamico y la integracion de sus camas quedo resuelta
por I-02: la cama esta compuesta dentro del sistema via `DynamicFlowBedLateralBuilder`, con BOM por
componente `Cama` sin despiece, y validada en pruebas y en AutoCAD.)

## 14. Como reanudar el trabajo

1. Clonar `https://github.com/marioap-afk/Calculadora_de_racks.git` — la rama por defecto es
   **`main`** (el trunk unico). NUNCA reanudar desde `release/claude-review` (retirada en I-00).
2. `git fetch origin && git log --oneline -5` y comparar con la seccion 8 de este archivo
   (¿hubo push nuevo?); `git branch -r` para ver las iniciativas en curso.
3. Leer en orden: este archivo -> [README.md](../README.md) -> [AGENTS.md](../AGENTS.md) ->
   [docs/WORKFLOW.md](WORKFLOW.md) -> [docs/00-indice-contexto.md](00-indice-contexto.md).
4. `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` (debe descubrir 635+ y quedar verde).
5. Tomar la primera tarea de la seccion 11 que siga abierta, abriendo rama + worktree por
   iniciativa segun [WORKFLOW.md](WORKFLOW.md) (nunca desarrollar directo sobre `main`).

**Prompt de reanudacion (copiar en un chat nuevo de Claude o Codex):**

```
Trabajo en RackCad (D:\Documentos\Codex\Calculadora de racks), plugin de AutoCAD 2025 en C#/.NET8.
El trunk es main (nunca release/claude-review, que fue retirada). Lee primero docs/HANDOFF.md,
luego README.md, AGENTS.md y docs/WORKFLOW.md; verifica el estado real con git log y dotnet test
(la suite completa debe quedar verde; el conteo vive en la seccion 12). El contexto de estado,
bugs conocidos y siguientes tareas esta en las secciones 9-11 del HANDOFF. I-02
(feature/dinamico-modular, el sistema dinamico modular) ya esta integrada; despues de su merge, el
dueno elige la siguiente iniciativa de la Fase 1 conforme a docs/ROADMAP.md — no reclames ninguna
sin esa eleccion. Respeta las convenciones de AGENTS.md (en especial: DeepCopy + DTO para flags de
seguridad, tests de regresion verificados fallando, y no integrar a main sin verificacion en
AutoCAD).
```
