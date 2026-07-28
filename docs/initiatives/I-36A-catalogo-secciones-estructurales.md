---
schema: rackcad-initiative/v1
id: I-36A
title: Nucleo y catalogo de secciones estructurales
type: architecture
status: implementing
branch: architecture/catalogo-secciones-estructurales
base_branch: main
priority:
size: M
depends_on: [I-19, I-26, I-23]
conflicts_with: []
context_packs: [architecture-kernel, catalogs-data, delivery-validation, documentation-governance]
automation_state_path: docs/automation/state/I-36A.yml
decision_paths: [docs/automation/decisions/I-36A.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Nucleo y catalogo de secciones estructurales

> **Primera iniciativa de la Fase 6.** Funda el catalogo NEUTRAL de secciones transversales que los
> sistemas nuevos —Cantilever el primero— van a consumir, importado de la fuente oficial **AISC
> Shapes Database v16.0**. No dibuja, no migra y no toca ningun sistema vigente.
>
> **Registro en el ROADMAP.** I-36A e I-36B no tenian fila en `docs/ROADMAP.md` al abrirse la
> iniciativa. El dueno **autorizo expresamente** su registro y exigio que lo hiciera el **primer
> commit sustantivo** de la rama; la Fase 6 se crea en ese mismo commit. No es una excepcion tacita:
> la autorizacion esta versionada en
> [`docs/automation/decisions/I-36A.md`](../automation/decisions/I-36A.md).
>
> **HANDOFF y el estado del ROADMAP.** WORKFLOW secciones 4.5.4 y 8 reservan `docs/HANDOFF.md` y la
> **columna Estado** del ROADMAP a la sesion de integracion, como ULTIMO commit de la rama. Esta
> sesion es de implementacion: **no** toca HANDOFF y **no** marca I-36A como integrada. Lo que si
> hace, por autorizacion expresa, es **crear las filas** de la Fase 6 con estado `pendiente`, porque
> sin ellas el plan no existe.

## 1. Objetivo

Un catalogo de secciones estructurales **neutral, completo, verificable y reproducible**, con:

- las **983** secciones de las cuatro familias autorizadas de AISC v16.0 importadas sin descarte
  silencioso;
- un nucleo puro en `RackCad.Application.StructuralSections`, independiente de `RackCatalog` y de
  `CatalogEntryBase`;
- un **importador reproducible** fuera del producto que convierte el XLSX oficial en CSV
  byte-identicos entre ejecuciones;
- **busqueda** por id y por designacion, **unidades** con su equivalencia calculada, **validacion**
  propia con severidades y **peso** por longitud;
- cero cambios de comportamiento en los cuatro sistemas vigentes.

Resultado verificable: `assets/catalogs/secciones.csv` byte-identico, las suites completas verdes, el
bundle conteniendo los archivos nuevos, y un manifiesto cuyos conteos y hashes coinciden con los
archivos distribuidos.

## 2. Problema

`assets/catalogs/secciones.csv` mezcla tres cosas en una fila: la **seccion transversal**, el **rol de
miembro** (`POSTE`/`CELOSIA`/`LARGUERO`/`SEPARADOR`) y la **pieza comercial** (`partNumber`,
`unitCost`, `mensula`). Mientras el catalogo describio perfiles propios de rack la mezcla no dolia:
cada fila era a la vez las tres cosas.

Cantilever rompe la coincidencia. Se arma con perfil estructural **estandar**: la misma `W12X28` puede
ser columna, brazo o base, y su designacion no es un SKU de RackCad sino una designacion de norma.
Modelarla por rol obligaria a duplicar la seccion una vez por rol y a meter el rol —dato del
miembro— dentro del dato de la seccion.

Ademas `CsvCatalogReader` es deliberadamente **tolerante**: una celda malformada deja el campo en su
valor por defecto y sigue. Correcto para un catalogo que el usuario edita en Excel; inaceptable para
983 filas de fuente oficial, donde un `0` por parseo fallido es un dato falso indistinguible de un
dato real.

Evidencia medida sobre el libro oficial (SHA-256 `82D0CEB9…3496`): 2 299 filas de datos, 13 tipos,
de los cuales las cuatro familias autorizadas suman **983** filas y las excluidas **1 316**.

## 3. Alcance

Autorizado por las decisiones vinculantes 1–24 del dueno
([`decisions/I-36A.md`](../automation/decisions/I-36A.md)) y por ADR-0020 y ADR-0021.

1. **ADRs previos a implementar**: ADR-0020 (catalogo neutral; **reemplaza a ADR-0008** solo en
   autoridad conceptual) y ADR-0021 (identidad, unidades y presentacion; **no** reemplaza ADR-0005,
   que solo recibe nota posterior).
2. **ROADMAP**: crear la **Fase 6 — Secciones estructurales y nuevos sistemas** con I-36A, I-36B,
   I-37 e I-38 y su cadena de dependencias.
3. **Nucleo** en `src/RackCad.Application/StructuralSections/`: familia, identidad, fuente,
   definicion, dimensiones por familia, propiedades, id, normalizador, unidades, formateador,
   catalogo, proveedor y validador.
4. **Lector CSV estricto dedicado**, con extraccion del parser lexico RFC-4180 actual a un helper
   compartido **preservando exactamente** el comportamiento de `CsvCatalogReader`.
5. **Siete archivos de catalogo** directamente bajo `assets/catalogs/` (cuatro CSV de familia
   generados, fuentes, overlay de estado y manifiesto).
6. **Importador** en `tools/RackCad.StructuralSections.Import/`: .NET 8, BCL puro, cero NuGet, cero
   Office Interop, acepta ruta local del XLSX, escribe en staging y produce salida determinista.
7. **Pruebas** puras y deterministas del importador (con XLSX sintetico generado en runtime), del
   modelo, del catalogo distribuido y de las regresiones existentes.
8. **Documentacion**: ARCHITECTURE, guias de catalogos y modelo de datos, indices de ADR e
   iniciativas, guia nueva `secciones-estructurales.md`, evidencia y estado versionado.

## 4. Fuera de alcance

Estricto. Cualquiera de estos exige detenerse (seccion 12):

I-36B y toda geometria (contornos, radios, filetes generados, polilineas, regiones, solidos, vistas
frontal/lateral/planta, longitudes prismaticas, bloques internos); AutoCAD; WPF, selector visual y
preview; Cantilever (I-37/I-38); persistencia de sistemas; migracion de `secciones.csv`; postes,
largueros, celosias, separadores, troqueles, conectores, mensulas y placas; BOM de sistemas; calculo
resistente, capacidad, pandeo o flecha; seleccion automatica; descarga automatica de fuentes en
runtime; perfiles personalizados; overrides distintos de `IsEnabled`; base SQL; paquetes NuGet; y
edicion de `blocks-library.dwg`.

Los hallazgos relacionados se documentan en `docs/ideas-futuras.md`; **no se corrigen de paso**.

## 5. Contexto requerido

- `AGENTS.md`; `docs/WORKFLOW.md` secciones 1–8; `docs/ROADMAP.md`; `docs/ARCHITECTURE.md` 4.4 y 7.6.
- ADR-0005 (unidades), ADR-0007 (CSV Excel-first), ADR-0008 (secciones por rol), ADR-0012 (cero
  NuGet), ADR-0017 (calculo de cargas diferido) y los nuevos ADR-0020 y ADR-0021.
- Context Packs: `architecture-kernel`, `catalogs-data`, `delivery-validation`,
  `documentation-governance`.
- I-19 (validador con severidades y manifiesto), I-26 (`TestCatalogIds` y guardian de catalogos
  distribuidos), I-23 (regla namespace = carpeta y sus guardas).
- Codigo: `src/RackCad.Application/Catalogs/` completo (incluido `Validation/`), los `.csproj` que
  copian `assets/catalogs`, `deploy/verify-bundle.ps1` y las pruebas de catalogo de
  `tests/RackCad.Tests`.
- Fuente: AISC Shapes Database v16.0, hoja `Readme` (glosario de variables, convencion EDI y lista de
  formas nuevas) y hoja `Database v16.0`.

## 6. Dependencias

- **I-19 integrada**: aporta el modelo de severidades que el validador nuevo reutiliza sin alterar
  `CatalogValidator`.
- **I-26 integrada**: aporta el patron de guardian contra los catalogos realmente distribuidos, que
  las pruebas del catalogo nuevo replican.
- **I-23 integrada**: fija la regla comprobable «namespace = carpeta», que el namespace nuevo respeta
  (`RackCad.Application.StructuralSections` en `src/RackCad.Application/StructuralSections/`).
- Entrada del dueno **existente y suficiente**: las 24 decisiones vinculantes. No hace falta ninguna
  decision adicional para implementar; si hace falta su **validacion** al cerrar.
- **Sin conflictos activos**: al reclamar, `origin` solo tenia `main`; ninguna otra iniciativa toca
  `Catalogs`, `assets/catalogs`, `tests/RackCad.Tests`, `RackCad.sln` ni los indices documentales.

## 7. Archivos esperados

Una desviacion material exige detenerse.

**Nuevos — nucleo** (`src/RackCad.Application/StructuralSections/`)

`StructuralSectionFamily.cs`, `StructuralSectionId.cs`, `StructuralSectionDesignationNormalizer.cs`,
`StructuralSectionIdentity.cs`, `StructuralSectionSource.cs`, `StructuralSectionProperties.cs`,
`IStructuralSectionDimensions.cs`, `WSectionDimensions.cs`, `HssRectangularSectionDimensions.cs`,
`ChannelSectionDimensions.cs`, `AngleSectionDimensions.cs`, `StructuralSectionDefinition.cs`,
`StructuralSectionCatalog.cs`, `IStructuralSectionCatalogProvider.cs`,
`CsvStructuralSectionCatalogProvider.cs`, `StructuralSectionCatalogValidator.cs`,
`StructuralSectionUnits.cs`, `StructuralSectionLabelFormatter.cs`, mas el lector estricto y sus tipos
de incidencia.

**Nuevos — datos** (`assets/catalogs/`)

`structural-sections-w.csv`, `structural-sections-hss-rect.csv`, `structural-sections-c.csv`,
`structural-sections-l.csv`, `structural-section-sources.csv`, `structural-section-status.csv`,
`structural-sections-manifest.json`.

**Nuevos — herramienta** (`tools/RackCad.StructuralSections.Import/`)

`.csproj` y sus fuentes (lector OOXML por ZIP/XML, clasificador de familia, mapeo por familia,
escritor determinista, manifiesto).

**Nuevos — docs**

`docs/adr/0020-*.md`, `docs/adr/0021-*.md`, `docs/guias/secciones-estructurales.md`,
`docs/initiatives/I-36A-catalogo-secciones-estructurales.md` (este),
`docs/automation/decisions/I-36A.md`, `docs/automation/state/I-36A.yml`,
`docs/automation/evidence/I-36A-catalogo-secciones-estructurales.md`.

**Modificados**

`docs/ROADMAP.md` (Fase 6, filas nuevas y grafo), `docs/adr/README.md` (indice y nota),
`docs/adr/0008-*.md` (estado + nota posterior; Decision intacta), `docs/adr/0005-*.md` (solo nota
posterior), `docs/ARCHITECTURE.md`, `docs/guias/catalogos-y-plantillas.md`,
`docs/guias/modelo-de-datos.md`, `docs/initiatives/README.md`, `RackCad.sln`, `.gitignore`,
`.gitattributes` (nuevo, alcance limitado a los archivos generados),
`src/RackCad.Application/Catalogs/CsvCatalogReader.cs` (solo delega su parser lexico; comportamiento
identico) y `tests/RackCad.Tests/` (pruebas nuevas y de regresion).

**Prohibido tocar**

`assets/catalogs/secciones.csv` y los demas catalogos vigentes, `blocks-library.dwg`,
`src/RackCad.Domain`, `src/RackCad.UI`, `src/RackCad.Plugin`, `deploy/`, `.github/`.

## 8. Fases

Cada fase termina con evidencia revisable y commit.

| # | Fase | Evidencia de cierre |
|---|---|---|
| 1 | Reclamo | Commit vacio con `Initiative-Id`, `Claim-Id` y `Co-Authored-By`; primer push aceptado sin force |
| 2 | Contrato, ROADMAP, decisiones y ADRs | ADR-0020/0021 escritos; ADR-0008 reemplazado; ADR-0005 anotado; Fase 6 registrada; estado versionado inicial |
| 3 | Herramienta de importacion y pruebas sinteticas | El importador compila y corre; XLSX sintetico generado en runtime demuestra las cuatro familias, los excluidos, el error por encabezado, el error por valor, la salida byte-identica y la colision de IDs |
| 4 | Modelo, proveedor y lector estricto | Nucleo compilando; parser lexico compartido con regresiones de `CsvCatalogReader` en verde |
| 5 | Validador, unidades, estado y busquedas | Validador con severidades; conversion y formateador probados; overlay `IsEnabled`; busquedas por id, EDI y familia |
| 6 | Importacion completa AISC y manifiesto | Los siete archivos generados; conteos 289/525/32/137 = 983; cero filas seleccionadas rechazadas; hashes en el manifiesto |
| 7 | Documentacion, evidencia y estado | Guia nueva, docs actualizados, evidencia reproducible, estado `review-ready` con gate `owner-validation` |

## 9. Pruebas y builds

Minimo exigido, todo sobre el SHA candidato:

```powershell
dotnet build tools/RackCad.StructuralSections.Import/RackCad.StructuralSections.Import.csproj -c Debug -v:minimal
dotnet build src/RackCad.Application/RackCad.Application.csproj -c Debug -v:minimal
dotnet test  tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -v:minimal
dotnet test  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug -v:minimal
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug -v:minimal
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -v:minimal
pwsh deploy/build-bundle.ps1 ; pwsh deploy/verify-bundle.ps1
```

Mas: CI completa verde 4/4 sobre el SHA publicado, y verificacion explicita de que los cuatro CSV
nuevos, `structural-section-sources.csv`, `structural-section-status.csv` y
`structural-sections-manifest.json` aparecen dentro del bundle.

Cobertura de pruebas exigida (detalle en la seccion 13 del encargo):

- **Importador**: XLSX OOXML sintetico generado en runtime con W, HSS rectangular, HSS cuadrado, HSS
  redondo, C, MC, L y 2L; solo entran las cuatro autorizadas; ninguna fila seleccionada se descarta;
  error por encabezado cambiado; error por valor invalido; dos ejecuciones byte-identicas; colision
  de ID detectada.
- **Modelo y catalogo**: lookup por id, lookup por EDI, normalizacion, habilitado/deshabilitado,
  filtro por familia, propiedades opcionales, duplicados, unidad nativa y conversion, formateador
  dual, peso por longitud.
- **Catalogo distribuido**: carga estricta de los siete archivos, validacion sin errores, conteos y
  hashes iguales al manifiesto, `SourceId`/revision uniformes, ningun id vacio, ninguna propiedad
  requerida en cero por parseo fallido, y **al menos dos sentinelas por familia** contrastadas a mano
  contra el libro oficial con su fila y celdas documentadas en la evidencia.
- **Regresiones**: `CsvCatalogReaderTests`, I-19, I-26, `SeccionesCatalogTests`,
  `ShippedCatalogIntegrityTests`, bundle e instalador, y `secciones.csv` sin cambio funcional.

La mayoria de las pruebas nuevas usa fixtures sinteticos; solo las del catalogo distribuido dependen
de ids AISC reales.

## 10. Validacion manual

**AutoCAD: no aplica.** I-36A no cambia dibujo, bloques, comandos ni DWG (`requires_autocad: false`).

**Validacion del dueno: SI aplica** (`requires_owner_validation: true`). Los siete puntos:

1. **Fuente y SHA** del libro utilizado, incluida la nota de que la URL `globalassets` del encargo ya
   no existe y de que se uso el enlace publicado por la propia pagina oficial.
2. **Conteos por familia**: W 289, HSS rectangular/cuadrado 525, C 32, L 137, total 983.
3. **Sentinelas** por familia contrastadas contra el libro.
4. **Etiquetas de peso** en el formato nativo-primero.
5. **Politica de IDs**, incluido el caso HSS (`AISC-HSS-RECT-HSS4X4X_250` para `HSS4X4X1/4`).
6. **Estado habilitado/deshabilitado** y su comportamiento en busqueda frente a `GetById`.
7. **Confirmacion** de que `secciones.csv` y los sistemas existentes no cambiaron.

## 11. Criterios de aceptacion

Implementada —**no** integrada— cuando todo lo siguiente sea observable:

1. Las cuatro familias importadas completas: **289 + 525 + 32 + 137 = 983**, con **cero** filas
   seleccionadas rechazadas y los excluidos reportados por tipo, no como error.
2. Dos ejecuciones del importador sobre el mismo XLSX producen archivos **byte-identicos**.
3. El manifiesto declara SHA-256 del libro, conteos y hash de cada CSV generado, sin timestamps, y no
   se incluye a si mismo en sus hashes.
4. `StructuralSectionCatalog` resuelve por id en O(1), por EDI normalizado y por designacion visible;
   filtra por familia; devuelve solo habilitadas por defecto; `GetById` sigue resolviendo las
   deshabilitadas; rechaza ids y designaciones ambiguas.
5. El lector estricto convierte en **error** con archivo, fila, columna e id cualquier encabezado
   faltante, duplicado o desconocido, id vacio, bool/enum/numero invalido, `NaN`/infinito y requerido
   ausente; los opcionales vacios quedan `null`, nunca cero.
6. El validador cubre las quince comprobaciones del encargo y no intenta validar resistencia,
   capacidad, pandeo, flecha ni diseno.
7. `assets/catalogs/secciones.csv` **byte-identico** a `origin/main` y `git diff --stat` sin una sola
   linea en los demas catalogos vigentes, `blocks-library.dwg`, Domain, UI, Plugin, `deploy/` ni
   `.github/`.
8. Suites completas verdes, builds Debug de UI y Plugin con 0 errores propios, bundle verificado con
   los archivos nuevos presentes, y **CI verde 4/4** sobre el SHA publicado.
9. ADRs, ROADMAP, guias, evidencia y estado versionado actualizados en esta misma rama.

## 12. Condiciones para detenerse

Detener y reportar con evidencia si ocurre cualquiera:

- la fuente oficial es inaccesible **antes** del reclamo (entonces se pide al dueno la ruta local y no
  se usa otra fuente);
- el libro no corresponde a la v16.0;
- la clasificacion de una fila seleccionada resulta ambigua;
- una fila W / HSS rectangular / C / L no puede importarse;
- hay colision de IDs;
- haria falta **inferir** datos geometricos no tabulados;
- haria falta Office Interop o un paquete NuGet;
- haria falta modificar sistemas existentes;
- haria falta implementar geometria;
- aparece un conflicto material con otra iniciativa activa;
- la CI de la base esta roja y no es atribuible;
- aparece una discrepancia no explicable entre los bloques de unidades US y metrico;
- el volumen obliga a cambiar el instalador de forma no prevista;
- cualquier expansion hacia I-36B, I-37 o I-38.

## 13. Estado versionado y entrega del Pull Request

Estado canonico: [`docs/automation/state/I-36A.yml`](../automation/state/I-36A.yml). Se actualiza al
terminar **cada** ejecucion, con `last_evidence_commit` apuntando al SHA completo que lo respalda.

No se abre Pull Request en esta corrida: el flujo del repositorio integra por `git merge --no-ff`
desde una sesion de integracion, no por PR (I-30, I-31, I-32, I-34 e I-35 se integraron sin PR). Si el
dueno pidiera uno, seria **uno solo** y nunca con merge automatico (`auto_merge: false`).

`state` recorre `claimed` → `implementing` → `review-ready`; el gate final de esta corrida es
`owner-validation`. `completed` no significa integrada.

## 14. Evidencia final

Se registra en
[`docs/automation/evidence/I-36A-catalogo-secciones-estructurales.md`](../automation/evidence/I-36A-catalogo-secciones-estructurales.md):
preflight real, base real, rama/worktree/Claim-Id, SHA-256 y metadatos del libro, ADRs creados y
reemplazados, esquema final, politica final de IDs, conteos exactos por familia, filas rechazadas,
tipos excluidos, archivos generados con sus hashes, sentinelas por familia con su fila y celdas de
origen, pruebas y builds, CI, commits y SHA de punta, diff resumido, validacion pendiente del dueno y
la confirmacion expresa de que **no** hubo I-36B, AutoCAD, UI, migracion de miembros, cambio funcional
de sistemas existentes ni cambio a `blocks-library.dwg`, y de que **`main` quedo intacta**.
