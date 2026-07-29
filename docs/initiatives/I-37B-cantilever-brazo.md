---
schema: rackcad-initiative/v1
id: I-37B
title: Fundacion Cantilever - brazo y conexion a columna
type: architecture
status: implementing
branch: architecture/cantilever-brazo
base_branch: main
priority:
size: M-L
depends_on: [I-36A, I-36B, I-36C, I-36D, I-37A]
conflicts_with: [I-37C, I-37D]
context_packs: [architecture-kernel, catalogs-data, documentation-governance, delivery-validation]
automation_state_path: docs/automation/state/I-37B.yml
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

# Fundacion Cantilever - brazo y conexion a columna

> **Segunda subiniciativa de I-37.** I-37A fundo base y columna; I-37B funda el **brazo**: cuerpo sencillo o
> compuesto, placa de conexion, patron de troqueles **consumido** de la columna, pendiente, tapa final y
> tope.
>
> **Extiende los contratos de I-37A de forma ADITIVA y no reabre ninguno.** El brazo es un
> `CantileverStructuralMemberPlan` con un rol nuevo, sus placas son `CantileverPlatePlan` con tipos nuevos y
> sus troqueles son `CantileverPunchPlan` con una superficie nueva.
>
> **No dibuja nada**, igual que I-37A: sin vistas, preview, editor, persistencia de proyecto, registros ni
> AutoCAD, y por tanto sin validacion manual.

## 1. Objetivo

Que sea posible, puro en Domain y Application:

1. recibir un diseno editable de brazo;
2. seleccionar una combinacion registrada de seccion y arreglo;
3. resolver **cuerpo sencillo**, **canal doble encontrado** y **canal doble espalda con espalda**;
4. colocar el cuerpo en lado `+Y` o `-Y`;
5. aplicar una pendiente parametrizable, **incluido cero**;
6. seleccionar un conjunto **contiguo** de troqueles regulares **ya existentes** de la columna;
7. generar una placa de conexion coincidente;
8. generar una tapa final opcional y extenderla como tope;
9. producir un `CantileverArmAssembly` determinista;
10. demostrar la coincidencia geometrica con la columna por **datum**, no por centros 3D;
11. hacerlo sin UI, AutoCAD, bloques ni lectura directa de CSV.

## 2. Problema

I-37A dejo la columna con su reticula regular resuelta y nadie que la consuma. El brazo es la pieza que la
consume, y al hacerlo aparecen cuatro preguntas que la base no tenia: una pieza que puede ser **dos**
perfiles; que mide exactamente la longitud que el usuario captura; de donde salen los agujeros del brazo; y
que hacer cuando el perfil es mas alto que su patron de agujeros. Las cuatro se responden en
[ADR-0025](../adr/0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md).

## 3. Alcance

- **Domain** (`src/RackCad.Domain/Systems/Cantilever/`): `CantileverArmDesign` y sus partes —cuerpo, placa de
  conexion, placa final—, con los enums de arreglo, modo de placa final y lado. Ids de seccion como
  **texto**, como manda ADR-0024 D1.
- **Application** (`src/RackCad.Application/Systems/Cantilever/`): la politica de elegibilidad del brazo, la
  autoridad de arreglos, la autoridad de marcos del brazo, el plan de cuerpo, el patron de conexion
  brazo-columna, el subensamble y sus diagnosticos.
- **Extension aditiva** de tres enums de I-37A (`CantileverMemberRole`, `CantileverPlateKind`,
  `CantileverPunchSurface`) y de los tokens de pieza. **Ningun valor existente cambia de nombre ni de
  numero.**
- **Pruebas** en `tests/RackCad.Tests`, incluidas guardas de fuente focalizadas.
- **Documentacion**: este contrato, ADR-0025, el estado de automatizacion, la fila del ROADMAP, la decision
  versionada de I-37 y el glosario.

## 4. Fuera de alcance

Cada uno es **condicion de detencion**:

estacion completa; doble cara como sistema; lista de niveles; aplicacion masiva de brazos; separadores;
arriostres; linea completa; BOM; peso; persistencia de `RackProject`; `RackSystemKind.Cantilever`; los tres
registros; biblioteca; UI; editor; preview; vistas; AutoCAD; comandos; Xrecords; bloques; calculo
estructural; capacidad; soldaduras; tornillos; anclas; CNC; shop drawings; familias nuevas de catalogo; un
PTR nuevo en el catalogo; y **cambios funcionales a I-36 o a I-37A**.

**No se toca `docs/HANDOFF.md`** ni se corrigen los hallazgos adyacentes de `docs/ideas-futuras.md`.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/ARCHITECTURE.md`; `docs/guias/agregar-un-sistema.md`;
`docs/guias/glosario.md`; `docs/guias/secciones-estructurales.md`;
`docs/guias/geometria-secciones-estructurales.md`; ADR-0020 a **ADR-0024** y **ADR-0025**;
`docs/automation/decisions/I-37.md`; el contrato y el estado de I-37A. Context Packs:
`architecture-kernel`, `catalogs-data`, `documentation-governance`, `delivery-validation`.

**Lectura obligatoria de codigo antes de colocar un canal:**
`src/RackCad.Application/StructuralSections/Geometry/ChannelSectionGeometryBuilder.cs`. La orientacion
canonica se lee ahi y **no se supone por el nombre**.

## 6. Dependencias

I-36A a I-36D e **I-37A integradas en `origin/main`** (merge `e0f319f`), con **ADR-0024 aceptado**. Se
estorba con I-37C e I-37D, que aun no existen.

## 7. Archivos esperados

**Crear** — `src/RackCad.Domain/Systems/Cantilever/CantileverArm*.cs`;
`src/RackCad.Application/Systems/Cantilever/CantileverArm*.cs`; pruebas nuevas en `tests/RackCad.Tests/`;
`docs/adr/0025-*.md`; `docs/automation/state/I-37B.yml`; este contrato.

**Modificar** — los tres enums y los tokens de I-37A (**solo anadiendo**); `docs/ROADMAP.md`;
`docs/adr/README.md`; `docs/initiatives/README.md`; `docs/automation/decisions/I-37.md`;
`docs/guias/glosario.md`; `docs/ARCHITECTURE.md` solo si hace falta.

**No tocar** — `docs/HANDOFF.md`; `src/RackCad.UI`; `src/RackCad.Plugin`;
`src/RackCad.Application/StructuralSections/**`; `assets/`; `deploy/`; `.github/`; `RackCad.sln`; los cinco
sistemas vigentes; y la **logica** de I-37A.

## 8. Fases

1. Reclamo.
2. Contrato, ADR-0025, ROADMAP, decision versionada y estado.
3. Contratos de Domain.
4. Politica, autoridad de arreglos y autoridad de marcos del brazo.
5. Cuerpo, placa de conexion, patron, placa final y subensamble.
6. Pruebas de invariantes y guardas de fuente.
7. Evidencia final.

## 9. Pruebas y builds

Builds Debug de Domain, Application, UI y Plugin con cero errores propios; pruebas focalizadas de I-37A **y**
de I-37B; guardas de fuente; suites completas de `RackCad.Tests` y `RackCad.UI.Tests`; validacion del
catalogo estructural; CI verde en la rama.

Regresion **verificada fallando** para, al menos: doble canal reducido a un miembro; separacion distinta de
cero; pendiente invertida en el lado negativo; el brazo recalculando el pitch de la columna; filas creciendo
hacia abajo; placa aceptando un cuerpo mas alto que su patron; y tapa modificando el corte del perfil.

## 10. Validacion manual

**No aplica.** I-37B no cambia el dibujo ni la interfaz — igual que I-37A, el gate se resuelve **sobre el
codigo**.

## 11. Criterios de aceptacion

Los once puntos del objetivo, cubiertos por prueba; las invariantes de cuerpo simple, de los dos arreglos
dobles, de la placa de conexion, de los troqueles, de la placa final y de determinismo; guardas de fuente
activas; y **cero** cambios en I-36, en I-37A, en los cinco sistemas vigentes, en catalogos, en UI y en
Plugin.

## 12. Condiciones para detenerse

Necesitar un valor sin default aprobado; necesitar un id de produccion o una familia de catalogo que no
existe —incluido un PTR—; necesitar tocar I-36, I-37A, UI o Plugin; o descubrir que la geometria real
contradice la orientacion canonica del canal que I-36 documenta.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-37B.yml`. Sin Pull Request; el merge automatico esta prohibido.

## 14. Evidencia final

Commits de la rama, archivos, pruebas, builds, CI y confirmacion de que `main` no fue modificada.
