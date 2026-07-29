---
schema: rackcad-initiative/v1
id: I-37A
title: Fundacion Cantilever - base y columna
type: architecture
status: implementing
branch: architecture/cantilever-base-columna
base_branch: main
priority:
size: M-L
depends_on: [I-36A, I-36B, I-36C, I-36D]
conflicts_with: [I-37B, I-37C, I-37D]
context_packs: [architecture-kernel, catalogs-data, documentation-governance, delivery-validation]
automation_state_path: docs/automation/state/I-37A.yml
decision_paths: [docs/automation/decisions/I-37.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Fundacion Cantilever - base y columna

> **Primera subiniciativa de I-37.** Funda el primer MIEMBRO de RackCad sobre el catalogo neutral de
> secciones estructurales: el subensamble base-columna, puro, en Domain y Application.
>
> ADR-0020 separo la seccion transversal del miembro y dejo escrito que el rol pertenece a los
> configuradores futuros. I-36B anadio la instancia prismatica y declaro igual de expresamente que **no es
> un miembro**. I-37A es donde nace el miembro.
>
> **No dibuja nada.** Sin vistas, sin preview, sin editor, sin persistencia de proyecto y sin AutoCAD. El
> usuario no ve nada nuevo al integrarla: lo que cambia es que el producto sabe que es una columna, que es
> una base y como se conectan.

## 1. Objetivo

Una fundacion pura en Domain/Application capaz de:

1. recibir un diseno editable de base y columna;
2. resolver **independientemente** los perfiles estructurales de ambas;
3. validar la combinacion con una **politica Cantilever explicita e inyectable**;
4. colocar el perfil de la columna y el de la base en un sistema de coordenadas local declarado;
5. derivar toda dimension exterior de la geometria de I-36, siempre por `Bounds`;
6. resolver placa frontal, placa posterior, cartabon, placa inferior de columna y los cuatro juegos de
   troqueles (placa posterior, conexion de columna, regulares de columna, placa inferior);
7. producir un subensamble resuelto **determinista**;
8. demostrar que los patrones de conexion de la base y de la columna **coinciden**, comparando su datum
   logico y no sus centros 3D;
9. hacerlo sin WPF, sin AutoCAD, sin bloques DWG y sin leer un CSV.

## 2. Problema

Ninguna de las cuatro iniciativas de I-36 construyo un miembro, a proposito y con guardas que lo
comprueban. El catalogo entrega secciones y geometria; nadie ha compuesto todavia dos secciones en una
pieza que se conecta con otra.

El subensamble base-columna es el minimo que obliga a resolver, de una vez, las cuatro preguntas caras:
en que capa vive el diseno si el id vive en Application, que forma tiene un miembro resuelto, quien es
dueno del patron de conexion compartido, y que significa una longitud cuando la fabricacion esta fuera de
alcance. Las cuatro se responden en [ADR-0024](../adr/0024-fundacion-cantilever-base-columna.md).

## 3. Alcance

- **Domain** (`src/RackCad.Domain/Systems/Cantilever/`): contratos editables de intencion, con el id de
  seccion como **texto**, un tipo por pieza y los defaults aprobados por el Owner.
- **Application** (`src/RackCad.Application/Systems/Cantilever/`): la frontera unica de resolucion
  —parseo del id, lookup, politica de elegibilidad, geometria por pieza, el patron compartido, el
  subensamble, los diagnosticos y las firmas deterministas—.
- **Pruebas** en `tests/RackCad.Tests`, incluidas las **guardas de fuente** focalizadas sobre
  `Systems/Cantilever`.
- **Documentacion**: este contrato, la decision versionada del Owner, ADR-0024, el estado de automatizacion,
  la fila del ROADMAP, el glosario y la frontera nueva en `ARCHITECTURE.md`.

## 4. Fuera de alcance

Cada uno es **condicion de detencion**, no un olvido:

brazos y su pendiente; niveles; estacion completa; doble cara; separadores; arriostres; la linea
Cantilever; BOM; peso; persistencia de `RackProject`; `RackSystemKind.Cantilever`; `SystemRegistry`;
`EditorModuleRegistry`; `KindHandlerRegistry`; biblioteca; editor; preview; vistas frontal, lateral, planta
e isometrica; AutoCAD; comandos; Xrecords; GUID; actualizacion multivista; materiales; calculo estructural;
capacidad de agujeros; calculo de placas; soldaduras; anclas; tornilleria; CNC; shop drawings; cambios
funcionales en I-36; familias estructurales nuevas; bloques DWG.

**No se toca `docs/HANDOFF.md`**: se actualiza solo en la sesion de integracion.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/ARCHITECTURE.md`; `docs/guias/agregar-un-sistema.md`;
`docs/guias/glosario.md`; `docs/guias/secciones-estructurales.md`;
`docs/guias/geometria-secciones-estructurales.md`; ADR-0020, ADR-0021, ADR-0022, ADR-0023 y **ADR-0024**;
`docs/automation/decisions/I-37.md`. Context Packs: `architecture-kernel`, `catalogs-data`,
`documentation-governance`, `delivery-validation`.

## 6. Dependencias

I-36A, I-36B, I-36C e I-36D **integradas en `origin/main`** (merge `3c6ccf5`), que es el unico criterio que
cuenta. Mas las decisiones del Owner versionadas en `docs/automation/decisions/I-37.md`.

Se estorba con I-37B, I-37C e I-37D, que aun no existen.

## 7. Archivos esperados

**Crear** — `src/RackCad.Domain/Systems/Cantilever/**`, `src/RackCad.Application/Systems/Cantilever/**`,
pruebas nuevas en `tests/RackCad.Tests/`, `docs/adr/0024-*.md`,
`docs/automation/decisions/I-37.md`, `docs/automation/state/I-37A.yml`, este contrato.

**Modificar** — `docs/ROADMAP.md` (fila paraguas de I-37 + fila nueva de I-37A),
`docs/adr/README.md`, `docs/initiatives/README.md`, `docs/guias/glosario.md`, `docs/ARCHITECTURE.md`,
`tests/RackCad.Tests/NamespaceFolderGuardTests.cs` (bucket `Cantilever`).

**No tocar** — `docs/HANDOFF.md`; todo `src/RackCad.UI`; todo `src/RackCad.Plugin`;
`src/RackCad.Application/StructuralSections/**`; `assets/catalogs/**`; `deploy/`; `.github/`;
`RackCad.sln`; los cinco sistemas vigentes.

## 8. Fases

1. Reclamo.
2. Contrato, decision del Owner, ADR-0024, ROADMAP y estado de automatizacion.
3. Contratos de Domain.
4. Resolucion de Application: secciones, politica, colocacion y miembros.
5. Placas, cartabon, troqueles y el patron compartido.
6. Pruebas de invariantes y guardas de fuente.
7. Documentacion final y evidencia.

## 9. Pruebas y builds

`dotnet build` Debug de Domain, Application, UI y Plugin con cero errores propios; suite completa de
`RackCad.Tests` y de `RackCad.UI.Tests`; CI verde en la rama. Regresion **verificada fallando** para al
menos: los tres troqueles adicionales, la transicion de `2 in` a `4 in`, la autoridad compartida y el
patron simetrico de la placa inferior.

## 10. Validacion manual

**No aplica.** I-37A no cambia el dibujo ni la interfaz: no hay nada que ver en AutoCAD.

## 11. Criterios de aceptacion

Las 27 invariantes del objetivo verificable, cubiertas por prueba; guardas de fuente activas sobre
`Systems/Cantilever`; firma determinista estable ante la misma entrada; cero cambios en I-36, en los cinco
sistemas vigentes, en catalogos y en el Plugin.

## 12. Condiciones para detenerse

Necesitar un valor sin default aprobado; necesitar tocar un archivo de I-36, de la UI o del Plugin;
descubrir que la geometria real contradice el patron de pares simetricos de la placa inferior; o que
cualquier elemento de la seccion 4 resulte imprescindible para cerrar el alcance.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-37A.yml`. Sin Pull Request abierto por ahora; el merge automatico esta prohibido.

## 14. Evidencia final

Commits de la rama, archivos, pruebas, builds, CI y confirmacion de que `main` no fue modificada.
