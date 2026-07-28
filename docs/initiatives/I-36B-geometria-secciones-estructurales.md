---
schema: rackcad-initiative/v1
id: I-36B
title: Geometria y representacion prismatica de secciones estructurales
type: architecture
status: review-ready
branch: architecture/geometria-secciones-estructurales
base_branch: main
priority:
size: L
depends_on: [I-36A, I-16, I-23]
conflicts_with: []
context_packs: [architecture-kernel, catalogs-data, ui-editors, autocad-plugin, delivery-validation, documentation-governance]
automation_state_path: docs/automation/state/I-36B.yml
decision_paths: [docs/automation/decisions/I-36B.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Geometria y representacion prismatica de secciones estructurales

> **Segunda iniciativa de la Fase 6.** I-36A entrego 983 secciones con sus dimensiones tabuladas y,
> deliberadamente, ninguna forma de VERLAS. I-36B convierte esos datos en geometria: contornos
> parametricos, longitud arbitraria, orientacion, vistas y una insercion en AutoCAD.
>
> **No dibuja ningun miembro.** Una instancia prismatica es geometria con longitud, no un poste, un
> brazo ni un larguero. I-37 Cantilever consumira esta infraestructura en una iniciativa separada.

## 1. Objetivo

Que una `StructuralSection` se pueda **ver y colocar**, con:

- geometria transversal **parametrica generada en codigo** desde el catalogo neutral, en dos niveles de
  detalle y con una **fidelidad declarada**;
- una **instancia prismatica** que aporte longitud, orientacion, rotacion y espejo, sin tocar la
  seccion;
- **proyeccion ortografica** a vistas de seccion, longitudinal X, longitudinal Y, isometrica y
  personalizada;
- un **plan neutral unico** que consumen igual el preview de la UI y el adaptador de AutoCAD;
- una **superficie minima de inspeccion** y el comando `RACKSECCION`.

Resultado verificable: las **983** secciones generan geometria sin excepcion, con centroide en el
origen y contornos cerrados; el error de area por familia esta **medido y documentado**; y ni
`secciones.csv`, ni `blocks.csv`, ni `blocks-library.dwg`, ni ningun sistema vigente cambian.

## 2. Problema

I-36A separo la seccion del miembro y de la pieza comercial, pero dejo el catalogo **sin
representacion**. Hoy no hay forma de comprobar visualmente que una `W12X26` importada es realmente una
W12X26, ni de colocarla en un dibujo, ni de que I-37 la use.

El patron vigente del repositorio no sirve aqui. Los cuatro sistemas resuelven cada pieza a un bloque de
`blocks-library.dwg` mediante `blocks.csv`: aplicarlo exigiria **983 bloques dibujados a mano**, uno por
designacion, atados a un archivo que no se versiona, y perderia justo lo que hace util un perfil
estandar —que su contorno es derivable de sus dimensiones—.

Tres preguntas mas no tienen respuesta previa: donde esta el origen de una seccion que AISC publica con
distancias «desde el borde designado»; donde vive la longitud sin convertir el catalogo en una fila por
medida; y que dibujar cuando la fuente publica `kdes` pero no publica el radio del filete.

## 3. Alcance

Autorizado por las decisiones vinculantes 1-25 del dueno
([`decisions/I-36B.md`](../automation/decisions/I-36B.md)) y por ADR-0022.

1. **ADR-0022** previo a implementar, en estado `propuesto` hasta la validacion del dueno.
2. **Primitivas neutrales ADITIVAS** en `RackCad.Application.Geometry`: vector 2D, limites,
   transformacion con rotacion y espejo, segmento, arco circular, trayectoria, contorno cerrado y
   punto/vector/marco 3D. Se reutiliza `Point2D`; no se mueve ni renombra nada existente.
3. **Geometria por familia** en `RackCad.Application.StructuralSections.Geometry`: W, HSS
   rectangular/cuadrado, C y L, en detalle `Simplified` y `Tabulated`, con fidelidad declarada y
   diagnosticos de degradacion.
4. **Instancia prismatica** con longitud, marco local ortonormal, rotacion alrededor de Z y espejo.
5. **Proyeccion ortografica** y **plan neutral** con roles semanticos, limites, fidelidad, diagnosticos
   y firma determinista.
6. **Cache perezosa** por seccion y nivel de detalle, invalidada por la firma del catalogo.
7. **Inspector y control de preview** en `RackCad.UI`, reutilizando `PreviewProjection` y los controles
   vigentes.
8. **Comando `RACKSECCION`** y materializacion como **bloque interno del dibujo**.
9. **Pruebas** puras sobre las 983 secciones, sentinelas, invariantes, area, centroides, proyeccion y
   UI.
10. **Documentacion**: ARCHITECTURE, README, indices, guia nueva y evidencia. **ROADMAP no**: ver
    seccion 7.

## 4. Fuera de alcance

Estricto. Cualquiera de estos exige detenerse (seccion 12):

I-37 Cantilever e I-38; `StructuralMember`, postes, brazos, largueros, celosias y bases; conexiones,
troqueles, perforaciones, placas, soldaduras, cortes de extremo y fabricacion; materiales por miembro,
cargas, capacidades y seleccion automatica; persistencia de miembros y round-trip de la representacion
insertada; solidos 3D, `Region`, `Solid3d`, extrusion y sweep; bloques dinamicos; edicion o migracion
de catalogos; cambios en sistemas existentes, en `blocks-library.dwg` o filas nuevas en `blocks.csv`;
descarga de fuentes; y familias nuevas.

Los hallazgos relacionados se documentan en `docs/ideas-futuras.md`; **no se corrigen de paso**.

## 5. Contexto requerido

- `AGENTS.md`; `docs/WORKFLOW.md`; `docs/ROADMAP.md` Fase 6; `docs/ARCHITECTURE.md` 4.4.1 y 7.
- ADR-0005 (unidades), ADR-0012 (cero NuGet), ADR-0020 y ADR-0021 (catalogo neutral e identidad), y el
  nuevo ADR-0022. `OWNER-DECISIONS.md` no existe en el repositorio.
- Context Packs: `architecture-kernel`, `catalogs-data`, `ui-editors`, `autocad-plugin`,
  `delivery-validation`, `documentation-governance`.
- Contrato, decision, estado y evidencia de **I-36A**, y `docs/guias/secciones-estructurales.md`.
- Codigo: `Application/StructuralSections` completo, `Application/Geometry`, `Application/Drawing`,
  `UI/Controls` y `UI/Preview`, `Plugin/Drawing`, `ViewBlockDraw`, `BlockPlacement`,
  `SystemBlockWriter`, el registro de comandos y las guardas de namespaces de I-23.

## 6. Dependencias

- **I-36A integrada**: aporta las 983 secciones, sus dimensiones tipadas por familia y la carga
  validada. Sin ella no hay de donde derivar geometria.
- **I-16 integrada**: aporta el patron de materializacion (`BlockPlacement`, `SystemBlockWriter`,
  `ApplyRegen`) que el adaptador reutiliza en vez de copiar.
- **I-23 integrada**: fija la regla comprobable «namespace = carpeta», que los tres namespaces nuevos
  respetan.
- **Sin conflictos activos**: al reclamar, `origin` solo tenia `main`.

## 7. Archivos esperados

Una desviacion material exige detenerse.

**Nuevos — primitivas** (`src/RackCad.Application/Geometry/`): vector, limites, transformacion 2D,
segmento, arco, trayectoria, contorno cerrado y tipos 3D de orientacion.

**Nuevos — geometria de secciones** (`src/RackCad.Application/StructuralSections/Geometry/`): nivel de
detalle, fidelidad, diagnostico, resultado de seccion, builders por familia, factoria con cache,
instancia prismatica, vistas, proyector, plan neutral y teselacion.

**Nuevos — UI** (`src/RackCad.UI/StructuralSections/`): control de preview e inspector.

**Nuevos — Plugin** (`src/RackCad.Plugin/Drawing/StructuralSections/`): materializacion del plan.

**Nuevos — comando**: registro de `RACKSECCION`.

**Nuevos — pruebas**: en `tests/RackCad.Tests` y `tests/RackCad.UI.Tests`.

**Nuevos — docs**: `docs/adr/0022-*.md`, `docs/guias/geometria-secciones-estructurales.md`,
`docs/initiatives/I-36B-*.md` (este), `docs/automation/decisions/I-36B.md`,
`docs/automation/state/I-36B.yml`, `docs/automation/evidence/I-36B-*.md`.

**Modificados**: `docs/ARCHITECTURE.md`, `README.md` (WORKFLOW section 8 obliga a actualizarlo en la
misma rama cuando cambian los comandos de AutoCAD),
`docs/adr/README.md`, `docs/initiatives/README.md`, `docs/guias/secciones-estructurales.md`.

**Prohibido tocar**: `assets/catalogs/**` (incluidos `secciones.csv` y `blocks.csv`),
`blocks-library.dwg`, `src/RackCad.Domain`, los sistemas vigentes de UI y Plugin, `deploy/`, `.github/`.

**`docs/ROADMAP.md` y `docs/HANDOFF.md` tampoco**, y esto corrige la version inicial de este contrato,
que listaba ROADMAP entre los modificados. [`WORKFLOW.md`](../WORKFLOW.md) seccion 8 lo dice sin
ambiguedad —«Nunca HANDOFF/ROADMAP desde ramas paralelas»— y WORKFLOW tiene precedencia sobre este
documento (seccion 10 de WORKFLOW). La fila de I-36B queda `pendiente` y su actualizacion pertenece a
la sesion de integracion. I-36A si escribio en ROADMAP, pero con **autorizacion expresa** del dueño
registrada en su decision; I-36B no la tiene y no la pide.

## 8. Fases

| # | Fase | Evidencia de cierre |
|---|---|---|
| 1 | Reclamo | Commit vacio con `Initiative-Id`, `Claim-Id` y `Co-Authored-By`; primer push aceptado; mapa de reutilizacion y viabilidad geometrica medidos |
| 2 | Contrato, decision y ADR-0022 | ADR `propuesto`; contrato y estado versionado iniciales |
| 3 | Primitivas y contrato geometrico | `Application.Geometry` ampliado de forma aditiva, con pruebas puras |
| 4 | Builders W y HSS | Contornos simplificado y tabulado; radios derivados; fidelidad declarada |
| 5 | Builders C y L | Traslado por `x`/`y`; orientacion canonica; angulo desigual probado |
| 6 | Instancia prismatica y proyeccion | Marco ortonormal; cinco vistas; teselacion determinista; plan neutral con firma |
| 7 | Preview e inspector | Control y ventana sobre `PreviewProjection`; pruebas de UI |
| 8 | Materializacion y `RACKSECCION` | Bloque interno; transaccion segura; sin `blocks.csv` ni `blocks-library.dwg` |
| 9 | Pruebas completas | 983 secciones, sentinelas, invariantes, area, centroides, proyeccion |
| 10 | Documentacion, evidencia y estado | Guia nueva, docs actualizados, evidencia reproducible, `review-ready` con gate `owner-validation` |

## 9. Pruebas y builds

```powershell
dotnet build src/RackCad.Application/RackCad.Application.csproj -c Debug -v:minimal
dotnet test  tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -v:minimal
dotnet test  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug -v:minimal
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug -v:minimal
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -v:minimal
pwsh deploy/build-bundle.ps1
```

Mas: validacion del catalogo de I-36A, generacion geometrica de las 983 secciones, pruebas de area y
centroide, guardas de namespaces y **CI completa verde** sobre el SHA publicado.

## 10. Validacion manual

**AutoCAD: SI aplica** (`requires_autocad: true`, `requires_owner_validation: true`). I-36B dibuja, asi
que las pruebas no pueden cerrar el gate: solo el dibujo real lo hace. El checklist de doce puntos vive
en la evidencia y cubre las cuatro familias, las cuatro vistas, rotacion ortogonal y no ortogonal,
espejo, los dos niveles de detalle, envelope y eje, dos longitudes distintas, y la confirmacion de que
la escala es en pulgadas, el bloque interno es seleccionable, no hay dependencia de
`blocks-library.dwg`, no hay filas nuevas en `blocks.csv`, cancelar no deja entidades y los sistemas
existentes no cambian.

## 11. Criterios de aceptacion

1. Las **983** secciones generan geometria `Simplified` sin excepcion, y `Tabulated` generan o degradan
   **explicitamente**.
2. Ninguna salida contiene `NaN` ni infinito; todos los contornos cierran; los limites son positivos; el
   centroide esta en el origen; la familia del builder coincide; **ningun diagnostico es silencioso**.
3. El error de area por familia esta **medido y documentado**, sin manipular la geometria para forzarlo.
4. Las sentinelas de las cuatro familias, incluido un **angulo desigual**, verifican dimensiones,
   orientacion, centroide, huecos, espesor nominal, radios derivados, fidelidad y limites.
5. Las invariantes se cumplen: area positiva, rotacion y espejo la conservan, la longitud no altera la
   seccion transversal, el peso escala linealmente, y la firma del plan es determinista.
6. El preview y AutoCAD consumen **el mismo plan**; ninguno recalcula una dimension.
7. `RACKSECCION` inserta un bloque **interno**, sin `blocks-library.dwg` ni filas en `blocks.csv`.
8. Suites completas verdes, builds Debug de UI y Plugin con 0 errores propios, bundle verificado y **CI
   verde** sobre el SHA publicado.
9. `git diff` sin una linea en `assets/`, `blocks-library.dwg`, Domain, los sistemas vigentes, `deploy/`
   ni `.github/`.

## 12. Condiciones para detenerse

I-36A no carga o valida; falta una dimension obligatoria no cubierta por la degradacion; la semantica de
`kdes`, `x`, `y`, `h` o `b` no puede acreditarse; un radio solo se obtiene inventando una regla; una
familia no puede centrarse; el area tabulada excede de forma material la tolerancia **sin causa
acreditada**; se requiere un solido 3D, modificar un sistema existente, cambiar el catalogo AISC o
`blocks-library.dwg`; conflicto material con otra iniciativa; CI base roja; `origin/main` avanza y el
rebase produce un conflicto semantico; o el alcance intenta expandirse hacia I-37.

**La ausencia de un radio NO bloquea la iniciativa** cuando la politica de degradacion permite una
representacion simplificada honesta.

## 13. Estado versionado y entrega

Estado canonico: [`docs/automation/state/I-36B.yml`](../automation/state/I-36B.yml), actualizado al
terminar **cada** ejecucion. `state` recorre `claimed` -> `implementing` -> `review-ready`; el gate
final es `owner-validation`. No se abre Pull Request: el repositorio integra por `git merge --no-ff`
desde una sesion de integracion. `completed` no significa integrada.

## 14. Evidencia final

En [`docs/automation/evidence/I-36B-geometria-secciones-estructurales.md`](../automation/evidence/I-36B-geometria-secciones-estructurales.md):
preflight real, base, rama/worktree/Claim-Id, primitivas reutilizadas y anadidas, contrato de ejes,
politica de fidelidad, reglas por familia, **errores de area maximos por familia**, resultados de
centroides, conteo de geometrias completas/derivadas/degradadas, vistas, teselacion, preview, comando,
materializacion, pruebas, builds, CI, commits, diff, el checklist para el dueno y la confirmacion
expresa de que no hubo I-37, miembros, calculo resistente, solidos 3D, migracion, cambios en sistemas
existentes, cambios en `blocks-library.dwg` ni filas nuevas en `blocks.csv`, y de que **`main` quedo
intacta**.
