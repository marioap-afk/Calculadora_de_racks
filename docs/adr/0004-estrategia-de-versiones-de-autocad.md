# ADR-0004: Estrategia de versiones de AutoCAD

- **Estado:** aceptado
- **Fecha:** 2026-07-21 (propuesta y aceptación)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó); redactado por Claude (I-12)
- **Iniciativa relacionada:** I-12 (`refactor/versionado`)

## Contexto

`RackCad.Plugin` es `net8.0-windows` y compila contra los ensamblados administrados de AutoCAD 2025
(R25.0), la primera versión de AutoCAD sobre .NET 8. Cada versión anual de AutoCAD puede cambiar el
runtime .NET y la API administrada; históricamente un plugin compilado para una versión no carga en
otra sin recompilar. El manifiesto del Autoloader (`PackageContents.xml`) declara el rango soportado
con `RuntimeRequirements SeriesMin`/`SeriesMax`: si `SeriesMax` falta, AutoCAD intentará cargar el
bundle en versiones futuras no probadas, y un cambio de runtime o de API fallaría en tiempo de
ejecución ante el usuario.

Hasta I-12, el manifiesto declaraba solo `SeriesMin="R25.0"` y la versión estaba escrita a mano en dos
lugares. I-12 centraliza la versión y genera el manifiesto desde una fuente única; falta fijar la
política del rango de series que esa fuente expresa. [ADR-0003](0003-referencias-autocad-para-ci.md) ya
limitó su excepción de compilación en CI a AutoCAD 2025 y .NET 8 y exige nueva revisión para 2026/2027.

## Decisión

RackCad declara soporte de **una sola serie de AutoCAD a la vez**. Hoy: `SeriesMin = SeriesMax =
R25.0` (AutoCAD 2025). El rango vive en una fuente única —las propiedades `RackCadAutoCADSeriesMin` y
`RackCadAutoCADSeriesMax` de `Directory.Build.props`— y el build las inyecta en `PackageContents.xml`
desde la plantilla; el rango nunca se escribe a mano en el manifiesto.

Soportar una versión nueva de AutoCAD es un acto **deliberado y anual**: se recompila el Plugin contra
los ensamblados de esa versión, se valida en ella, se sube `SeriesMax` (y `SeriesMin` si se abandona la
anterior) en esa única fuente, y se ejecuta la nueva revisión de ADR-0003 que las referencias de
compilación exigen. No se amplía `SeriesMax` a una versión que no se haya recompilado y validado.

## Alternativas consideradas

- **Omitir `SeriesMax` (rango abierto)** — AutoCAD cargaría el bundle en versiones futuras no probadas;
  un cambio de runtime .NET o de API podría fallar en ejecución ante el usuario. El límite explícito es
  más seguro y honesto.
- **Multi-targeting (varias series a la vez)** — mantener varias compilaciones y un manifiesto con
  `RuntimeRequirements`/`ComponentEntry` por serie. Multiplica mantenimiento y validaciones sin demanda
  actual; se puede reconsiderar cuando convivan 2025 y 2026.
- **Fijar el rango a mano en el manifiesto** — vuelve a duplicar el dato que I-12 unificó.

## Consecuencias

- Positivas: el bundle solo se carga donde fue compilado y validado (AutoCAD 2025); subir de versión es
  un cambio de una línea en la fuente única más recompilación y validación; alineado con ADR-0003.
- Negativas / costos aceptados: al salir AutoCAD 2026/2027 el bundle actual no cargará ahí hasta la
  recompilación anual deliberada; hay que recordar subir `SeriesMax` y revisar ADR-0003 en ese momento.

## Referencias

- I-12 (`refactor/versionado`): versión única, manifiesto generado, bundle por `dotnet publish`.
- [ADR-0003](0003-referencias-autocad-para-ci.md): limita la excepción de compilación a AutoCAD 2025 y
  .NET 8; exige nueva revisión para 2026/2027.
- `Directory.Build.props` (`RackCadAutoCADSeriesMin`/`Max`),
  `deploy/RackCad.bundle/PackageContents.template.xml`, `docs/guias/despliegue.md`.
- ROADMAP, fila I-12 (hallazgos G5, G8, G9).

## Notas posteriores

- **2026-07-21 — Aceptado por Mario Pérez** (dueño del repositorio). Decisión confirmada: RackCad
  soporta una sola serie de AutoCAD a la vez; hoy `SeriesMin = SeriesMax = R25.0`, exclusivamente
  AutoCAD 2025; soportar una versión futura exige recompilar, validar en ella, actualizar el rango y
  revisar nuevamente ADR-0003. El contenido de la sección Decisión no cambia (ADR aceptado inmutable).
