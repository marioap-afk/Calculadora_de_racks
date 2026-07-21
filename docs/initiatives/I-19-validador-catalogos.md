---
schema: rackcad-initiative/v1
id: I-19
title: Validador de catálogos
type: feature
status: in-progress
branch: feature/validador-catalogos
base_branch: main
priority:
size: M
depends_on: []
conflicts_with: []
context_packs:
  - catalogs-data
  - delivery-validation
automation_state_path: docs/automation/state/I-19.yml
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# Validador de catálogos

## 1. Objetivo

Un validador **puro** en `RackCad.Application` que revise un `RackCatalog` cargado (más las filas
crudas de `secciones.csv`) y produzca **un diagnóstico único** con severidades para cinco categorías:

1. **IDs duplicados** dentro de un catálogo y dentro de la hoja unificada `secciones.csv`.
2. **Referencias y relaciones inválidas** (FKs colgantes: ménsula, punto de conexión, pieza; claves de
   relación repetidas que quedan ensombrecidas por el `FirstOrDefault` de los lookups).
3. **Bloques o vistas faltantes** (nombre de bloque vacío; vista referenciada que no existe en `views`).
4. **Filas descartadas por rol, con aviso** (una fila de `secciones.csv` con `rol` desconocido/vacío hoy
   se descarta en silencio en el proveedor; el validador la reporta como advertencia).
5. **Manifiesto previsto para `blocks-library.dwg`**: construir el manifiesto **esperado** desde el
   catálogo (lista de bloques + parámetros dinámicos derivados de los datos + huella/versión) y
   compararlo contra un manifiesto **real** provisto como dato, señalando bloques esperados que faltan,
   bloques extra y parámetros esperados ausentes.

El resultado verificable es la biblioteca de validación con pruebas positivas y negativas por categoría
y severidad, más una prueba de **integridad** que fija el estado del catálogo distribuido y una prueba
**golden** que demuestra que el reparto por rol del proveedor no cambió.

## 2. Problema

Los catálogos CSV/JSON son la fuente de verdad de perfiles, bloques, conexiones y seguridad, y los
editan ingenieros en Excel. El proveedor es tolerante por diseño: un `rol` mal escrito descarta la fila
sin aviso (`JsonRackCatalogProvider.SplitSecciones`), un FK colgante o un ID duplicado no se detecta
hasta que el dibujo sale incompleto o un lookup toma la fila equivocada. No existe una revisión previa
que reúna estos problemas en un solo diagnóstico ni un contrato de compatibilidad entre el catálogo y
`blocks-library.dwg` (ideas-futuras #14 y #15). I-18 (Push Back) meterá filas nuevas al catálogo; tener
el validador antes reduce el riesgo de esa y de futuras ampliaciones.

## 3. Alcance

- Nuevo espacio `RackCad.Application.Catalogs.Validation`: modelo de severidad/categoría/incidencia,
  reporte agregado (con formato de diagnóstico único y modo estricto) y el motor `CatalogValidator`.
- Modelo de manifiesto de bloques (`CatalogBlockManifest`) con: construcción del esperado desde el
  catálogo, huella estable (SHA-256, BCL), (de)serialización JSON y comparación contra un manifiesto real.
- Extracción de la clasificación por rol a una única fuente compartida (`SeccionRoles`) consumida por el
  proveedor y por el validador (regla en un solo sitio, AGENTS.md §2), **preservando el comportamiento**
  del reparto actual.
- Costura aditiva en `JsonRackCatalogProvider` para exponer las filas crudas de `secciones` sin cambiar
  `Load()`.
- Pruebas xUnit positivas y negativas por categoría/severidad, prueba de filas descartadas, pruebas de
  manifiesto (build/huella/JSON/comparación), prueba de integridad del catálogo distribuido y prueba
  golden del reparto por rol.
- Documentación: este contrato, nota en la guía de catálogos, hallazgo fuera de alcance en
  `ideas-futuras.md`, estado de automatización.

## 4. Fuera de alcance

- **Corrección automática de catálogos**: el validador sólo reporta; nunca edita un CSV/JSON.
- **Modificaciones al DWG de bloques**: no se lee ni se escribe `blocks-library.dwg` (no está versionado
  y pertenece al dueño). Sólo se define/compara el manifiesto como dato.
- **Push Back** (I-18) y cualquier fila/bloque nuevo de producto.
- **Cambios en reglas de producto**: la clasificación por rol se extrae sin alterar su comportamiento;
  no se cambia cómo el catálogo se convierte en dibujo/BOM.
- **Logging de I-03** y escrituras atómicas.
- **Ampliaciones del esquema de persistencia** (`*Document`, versiones, Xrecord).
- **Cableado en UI y comando de AutoCAD** (el "diagnóstico único en UI" y el modo estricto de despliegue
  de la idea #14): se entrega el motor puro y su reporte con formato; la superficie WPF/Plugin y la
  emisión del manifiesto real leyendo el DWG son un seguimiento posterior (requieren build de Plugin y
  validación en AutoCAD). Se anota en `ideas-futuras.md`.

## 5. Contexto requerido

- `AGENTS.md` (§2 regla en un solo sitio, §7 catálogos, dependencias sin NuGet), `docs/WORKFLOW.md`,
  Context Pack `catalogs-data` y `delivery-validation`, `docs/guias/catalogos-y-plantillas.md`,
  `docs/guias/modelo-de-datos.md`.
- Código: `src/RackCad.Application/Catalogs/*` (entradas, lector CSV, proveedor, extensiones/lookups),
  `src/RackCad.Application/RackFrames/CatalogIds.cs`, `assets/catalogs/*.csv`.
- Pruebas existentes que no se deben duplicar sino complementar: `CatalogCanonicalIdsTests`
  (guardián de IDs canónicos, I-26), `SeccionesCatalogTests`, `JsonRackCatalogProviderTests`,
  `TestCatalogIds`.

## 6. Dependencias

Sin dependencias previas integradas requeridas (ROADMAP: "—"). Sin conflictos declarados. No requiere
entradas del dueño previas. I-12 e I-14 pueden estar activas en worktrees separados; no se tocan.

## 7. Archivos esperados

Crear:

- `src/RackCad.Application/Catalogs/SeccionRoles.cs`
- `src/RackCad.Application/Catalogs/Validation/CatalogValidationIssue.cs`
- `src/RackCad.Application/Catalogs/Validation/CatalogValidationReport.cs`
- `src/RackCad.Application/Catalogs/Validation/CatalogValidator.cs`
- `src/RackCad.Application/Catalogs/Validation/CatalogBlockManifest.cs`
- `tests/RackCad.Tests/CatalogValidatorTests.cs`
- `tests/RackCad.Tests/CatalogBlockManifestTests.cs`
- `tests/RackCad.Tests/ShippedCatalogIntegrityTests.cs`

Modificar (aditivo / preservando comportamiento):

- `src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs` (costura de filas crudas + uso de
  `SeccionRoles` en `SplitSecciones`).
- `docs/initiatives/I-19-validador-catalogos.md`, `docs/initiatives/README.md`,
  `docs/guias/catalogos-y-plantillas.md`, `docs/ideas-futuras.md`, `docs/automation/state/I-19.yml`.

Una desviación material respecto de esta lista obliga a detenerse.

## 8. Fases

1. **Reclamo + contrato + estado** (rama, worktree, commit vacío, este contrato, `state/I-19.yml`).
2. **Motor de validación**: `SeccionRoles`, costura del proveedor, incidencia/reporte, `CatalogValidator`
   con las cuatro categorías de catálogo + filas descartadas.
3. **Manifiesto**: `CatalogBlockManifest` (esperado, huella, JSON, comparación) integrado al reporte.
4. **Pruebas** positivas/negativas por categoría, filas descartadas, manifiesto, integridad y golden.
5. **Gates + evidencia**: build, suite completa, integridad, revisión de diff, hash/estado del DWG,
   documentación, commits lógicos y push.

## 9. Pruebas y builds

- `dotnet build src/RackCad.Application/RackCad.Application.csproj` (0 errores/0 advertencias propias).
- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` (toda la suite verde).
- CI en la rama (tres jobs) verde sobre los commits publicados.
- Build de Plugin: no aplica (sin cambios en el Plugin). Build de UI: no aplica.

## 10. Validación manual

AutoCAD: **no aplica** (sin cambios de dibujo, geometría, bloques ni persistencia). La validación del
dueño consiste en revisar el diagnóstico del validador sobre el catálogo real y confirmar que ni los
catálogos ni `blocks-library.dwg` fueron modificados por la iniciativa.

**Owner-validation APROBADA (2026-07-21):** el dueño aceptó como baseline conocido el diagnóstico del
catálogo real (1 error `DUPLICATE_ID` en `TROQUEL_TOPE` + 2 advertencias `UNRESOLVED_BLOCK_PIECE` de
`TARIMA_GENERICA`) y confirmó que `assets/catalogs/*` y `blocks-library.dwg` permanecen intactos. Estado:
**integration-ready**. Falta sólo la integración serializada manual (rebase sobre `main` vigente + CI verde +
merge), fuera del alcance de esta corrida.

## 11. Criterios de aceptación

- El validador reporta, con severidad, cada una de las cinco categorías, con pruebas positivas y negativas.
- Las filas descartadas por rol se reportan como advertencia (antes silenciosas).
- El manifiesto esperado se construye desde el catálogo, tiene huella estable y round-trip JSON, y la
  comparación produce las incidencias de bloque/parámetro faltante o extra.
- La prueba de integridad fija el estado del catálogo distribuido y la golden demuestra reparto por rol
  inalterado.
- Suite completa y CI verdes. `main` no modificada. Sin ediciones a `assets/catalogs/*` ni a ningún DWG.

## 12. Condiciones para detenerse

- Necesidad de editar un catálogo o el DWG para "arreglar" un hallazgo (fuera de alcance: se documenta).
- Que el reparto por rol del proveedor cambie de comportamiento (la golden debe seguir verde).
- Divergencia de `origin/main` que exija rebase con conflictos no mecánicos.
- Cualquier expansión hacia UI/Plugin/Push Back/persistencia.

## 13. Estado versionado y entrega del Pull Request

Estado canónico: `docs/automation/state/I-19.yml`. El merge automático está prohibido; la integración es
manual. No se abre un segundo Pull Request para la iniciativa. `completed` no significa integrada.

## 14. Evidencia final

Evidencia reproducible y verbatim en
[`docs/automation/evidence/I-19-validador-catalogos.md`](../automation/evidence/I-19-validador-catalogos.md).

- **Build**: `RackCad.Application` con 0 errores / 0 advertencias.
- **Pruebas**: suite completa **842/842** verde (51 nuevas sobre la línea base 791 en `de72287`; r1 822,
  r2 839, r3 841, r4 842, r5 842).

### Ronda 2 — correcciones de la revisión (2026-07-21)

1. **Exactitud del manifiesto**: los parámetros esperados se derivan por **uso exacto** de bloque
   (pieceId+view), nunca por PieceId global; los `paramX`/`paramY` sólo aplican a la misma pieza y vista.
2. **Parámetros dinámicos reales**: fuente compartida `CatalogBlockParameters` (nombres desde
   `SelectiveRackDefaults`/`SelectiveSafetyDefaults`, los mismos que usan los productores) que incluye
   `LONGITUD` del riel/postes/separadores, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`/`FONDO`. Se consolidaron
   los literales dispersos de nombre de parámetro hacia el dominio (sin cambiar geometría ni reglas). Una
   guardia (`Manifest_ExpectsEveryParameterTheRailBuilderActuallyApplies`) cruza la salida real del builder
   del riel contra el manifiesto para prevenir divergencias.
3. **Versión/huella operativas**: `Compare` aborta ante esquema incompatible
   (`MANIFEST_SCHEMA_INCOMPATIBLE`) y marca huella ausente o alterada (`MANIFEST_FINGERPRINT_MISMATCH`).
4. **Campos obligatorios vacíos**: `ConnectionLayoutEntry` con `PieceId`/`ConnectionPointId`/`View` vacío es
   error (`EMPTY_LAYOUT_FIELD`); el bloque genérico sin pieza sigue siendo advertencia.

Archivos nuevos de la ronda 2: `src/RackCad.Application/Catalogs/CatalogBlockParameters.cs`,
`tests/RackCad.Tests/CatalogBlockParametersTests.cs`. Editados: los cuatro productores que definían nombres
de parámetro por literal (`SelectiveRackDefaults` del dominio, `SelectiveSafetyPlacement`,
`LateralHeaderParameters`, `DynamicSystemLateralBuilder`, `FlowBedLateralBuilder`). La huella esperada del
catálogo distribuido cambió a `0352c75e…` (bloques: 90).

### Ronda 3 — correcciones de la revisión (2026-07-21)

1. **Separador por vista**: recibe `LONGITUD` en FRONTAL (`SeparatorView`) y PLANTA; ya **no** se exige en
   LATERAL (ningún builder de producción lo escribe ahí).
2. **Protectores tipo `LATERAL`**: parametrizados con `LONGITUD` en las tres vistas
   (`SelectiveSafetyPlacement.Piece`), sin afectar a las botas `BOTA`.
3. **Guardia integral builder→manifiesto** (`CatalogManifestGuardTests`): corre los builders reales de las
   trece familias (poste, placa, larguero, celosía, separador, riel, tarima, tope, parrilla, protector
   lateral, desviador, guía, defensa) por vista y verifica que cada clave escrita esté en el manifiesto del
   mismo PieceId+View+BlockName; regresiones explícitas contra requisitos falsos (separador LATERAL,
   ménsulas, botas). Ya no depende sólo del riel.
4. **Larguero lateral**: refleja `PERALTE` (intermedio) y `LONGITUD` (in/out) — ambos grips en las vistas
   dibujadas.

Archivo nuevo de la ronda 3: `tests/RackCad.Tests/CatalogManifestGuardTests.cs`. Editados: el mapa de
`CatalogBlockParameters` (separador, protector lateral, larguero) y los XML-doc de `CatalogBlockManifest`
(`Parameters` y `BuildExpected`: los parámetros proceden de `paramX`/`paramY` **y** de los requisitos reales
de los builders). Sin nuevos literales (nombres siguen en el dominio). La huella esperada cambió a
`50f8a460…` (bloques: 90).

### Ronda 4 — correcciones de la revisión (2026-07-21)

1. **Largueros en LATERAL**: la regla `Beam` deja de añadir `LONGITUD`+`PERALTE` en cualquier vista. En
   LATERAL el larguero selectivo/intermedio exige sólo `PERALTE`; el in/out (`LARGUERO_IN_OUT_C6`) es un
   subtipo `InOutBeam` que **no exige nada** en LATERAL (`MakeLoadBeam` no escribe parámetros). La placa en
   LATERAL tampoco exige (el `PERALTE` sólo lo escribe el override manual, fuera del flujo estándar).
2. **Guardia por igualdad exacta**: agrupa lo escrito por los builders por `PieceId`+`View`+`BlockName` y
   compara por igualdad contra `ExpectedParameters(...)` **y** contra la entrada del manifiesto (detecta
   faltantes y de más); fixtures enfocados activan los caminos condicionales (tope).
3. **Cobertura verificable**: matriz explícita por `PieceId`+`View`+`Parámetro` de las 13 familias; la prueba
   falla si una familia deja de observarse (sin colapsar seguridad bajo `Safety/<vista>/LONGITUD`).
4. **Validación de entrada del manifiesto**: instancia parametrizada con `BlockName` vacío o sin entrada en el
   manifiesto → falla (no se omite cuando `TryGetValue` devuelve `false`).

Ronda 4: sólo se editaron `CatalogBlockParameters` (subtipo in/out; larguero/placa LATERAL exactos) y se
reescribió `CatalogManifestGuardTests` como comparación exacta + matriz de cobertura. Sin nuevos literales.
La huella esperada cambió a `ad1f6063…` (bloques: 90).

### Ronda 5 — correcciones de la revisión (2026-07-21)

1. **Override real de la placa LATERAL**: el manifiesto exige `PERALTE` en la placa LATERAL (camino de
   producción soportado — `LateralHeaderLayoutBuilder` lo escribe cuando `LeftBasePlate`/`RightBasePlate.
   PeralteOverride` es positivo). Fixture enfocado con override que corre el builder real y observa `PERALTE`;
   sin tocar el builder ni la regla de producto.
2. **Agrupación por triple exacto**: la guardia se key-ea por `(PieceId, View, BlockName)` (la r4 lo afirmaba
   pero el código agrupaba por `PieceId`+`View`); cada triple compara por igualdad contra
   `ExpectedParameters(...)` **y** la entrada exacta del manifiesto; sin acumular varios `BlockName` bajo una
   entrada; `BlockName` vacío o sin entrada sigue fallando.
3. **Matriz de cobertura completa**: añadidos `GUIA_ENTRADA`+`LATERAL` y la placa LATERAL con `PERALTE`;
   revisión en **ambas direcciones** (declarado⇒observado y observado-con-parámetro⇒declarado); casos vacíos
   relevantes declarados explícitamente.

Ronda 5: se editó `CatalogBlockParameters` (placa `PERALTE` en las tres vistas) y `CatalogManifestGuardTests`
(triple keying + fixture de override + matriz bidireccional). Sin nuevos literales. La huella esperada cambió a
`1a31c1a9…` (bloques: 90). El intento de corrección en el estado versionado sube a `attempts: 4`.
- **Categorías cubiertas** con prueba positiva y negativa (matriz en la evidencia): ids duplicados,
  referencias/relaciones inválidas, bloques/vistas faltantes, filas descartadas por rol y manifiesto,
  más modo estricto y el paso extremo a extremo por la costura del proveedor.
- **Integridad del catálogo real**: el validador reporta exactamente 1 error conocido (`DUPLICATE_ID`
  `TROQUEL_TOPE`) y 2 advertencias esperadas (`UNRESOLVED_BLOCK_PIECE` `TARIMA_GENERICA`); fijado por
  `ShippedCatalogIntegrityTests`. Golden del reparto por rol sin cambios.
- **`assets/catalogs/` sin modificar** (`git diff origin/main -- assets/catalogs` vacío) y
  **`blocks-library.dwg` intacto** (no versionado, ningún `.dwg` rastreado o en disco, el código nunca abre
  un DWG).
- **`main` no modificada**: todo el trabajo vive en `feature/validador-catalogos`.

Hallazgo fuera de alcance registrado en `docs/ideas-futuras.md`: el id `TROQUEL_TOPE` duplicado en
`connection-points.csv`. Validación del dueño pendiente (revisar el diagnóstico). El detalle transitorio
(SHAs, fase, siguiente acción) vive en `docs/automation/state/I-19.yml`.
