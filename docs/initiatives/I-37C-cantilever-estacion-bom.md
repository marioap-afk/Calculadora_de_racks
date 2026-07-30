---
schema: rackcad-initiative/v1
id: I-37C
title: Estacion Cantilever y BOM por componentes
type: architecture
status: implementing
branch: architecture/cantilever-estacion-bom
base_branch: main
priority:
size: L
depends_on: [I-36A, I-36B, I-36C, I-36D, I-37A, I-37B]
conflicts_with: [I-37D]
context_packs: [architecture-kernel, catalogs-data, ui-editors, persistence, documentation-governance, delivery-validation]
automation_state_path: docs/automation/state/I-37C.yml
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

# Estacion Cantilever y BOM por componentes

> **Tercera subiniciativa de I-37.** I-37A fundo columna y base; I-37B fundo el brazo. I-37C **compone**
> las dos: una **estacion** completa de gondola **sencilla** o **doble**, con niveles ajustados
> obligatoriamente a los troqueles de la columna, altura **automatica** o **manual**, y un **BOM por
> componentes atornillables**.
>
> **No recalcula ninguna geometria de I-37A ni de I-37B.** Los consume: construye sus disenos, los resuelve
> con sus resolvers y compone el resultado. Lo unico que se **extrae** es la retica regular de troqueles,
> mecanicamente y con caracterizacion previa, porque la estacion necesita elegir indices **antes** de
> conocer la altura final.
>
> **No dibuja nada**, igual que las dos anteriores: sin vistas, preview, editor, persistencia de proyecto,
> registros ni AutoCAD, y por tanto sin validacion manual.

## 1. Objetivo

Que sea posible, puro en Domain y Application:

1. recibir un diseno editable de estacion;
2. resolver una estacion **sencilla** en cualquiera de los dos lados;
3. resolver una estacion **doble** con **una** columna, **dos** bases y brazos en ambos lados;
4. compartir la lista de niveles entre ambos lados;
5. elegir un brazo predeterminado;
6. aplicar **overrides por celda**;
7. calcular los indices de troquel de cada nivel;
8. ajustar cada nivel **obligatoriamente hacia arriba** a la reticula;
9. respetar un **claro libre** solicitado, medido cuerpo a cuerpo;
10. recalcular la **altura minima** de la columna;
11. admitir altura **automatica** o **manual**;
12. dejar al menos el **margen superior** parametrizado sobre el ultimo nivel;
13. producir un **BOM por componentes**: una columna con su base o bases, y brazos individuales agrupados
    por receta;
14. producir una estacion **inmutable y determinista**;
15. hacerlo sin UI, AutoCAD, persistencia de proyecto ni linea longitudinal.

## 2. Problema

I-37A e I-37B resolvieron piezas; nadie las compone. Al componerlas aparecen cinco preguntas que ninguna de
las dos tenia:

1. **Que es una gondola doble.** Dos estaciones no es la respuesta: hay **una** columna fisica y **una**
   placa inferior de columna, con **dos** bases espejadas.
2. **Donde vive la altura.** El template de columna-base no puede llevar una altura, porque la estacion la
   calcula; guardar las dos es guardar dos autoridades para el mismo numero.
3. **Una dependencia circular real.** La altura de la columna determina cuantos troqueles regulares existen,
   los troqueles determinan donde caen los niveles, y los niveles determinan la altura minima.
4. **Que mide el claro libre.** Cuerpo a cuerpo en el plano de conexion, y no desde ejes, centros de
   troquel ni bordes de placa.
5. **Que es un componente del BOM.** Lo atornillable: una columna con su base o bases, y cada brazo.

Las cinco se responden en [ADR-0026](../adr/0026-estacion-cantilever-niveles-altura-y-bom.md).

## 3. Alcance

- **Domain** (`src/RackCad.Domain/Systems/Cantilever/`): el diseno de estacion y sus partes —modo de cara,
  modo y diseno de altura, nivel, **template** de columna-base y **template** de brazo—, con los ids de
  seccion como **texto**, como manda ADR-0024 D1.
- **Application** (`src/RackCad.Application/Systems/Cantilever/`): la autoridad de la reticula regular
  **extraida** de I-37A, el resolver de layout de niveles, la autoridad de espejo de base lateral, la
  matriz pura de brazos, el resolver de estacion, el subensamble compuesto y el BOM por componentes.
- **Extension aditiva** de los enums y tokens de pieza de I-37A/I-37B. **Ningun valor existente cambia de
  nombre ni de numero.**
- **Extraccion mecanica** de `CantileverColumnRegularPunchGrid` desde `CantileverColumnBaseResolver`, con
  **caracterizacion previa** y equivalencia numerica demostrada.
- **Pruebas** en `tests/RackCad.Tests`, incluidas guardas de fuente focalizadas y autoverificables.
- **Documentacion**: este contrato, ADR-0026, el estado de automatizacion, la fila del ROADMAP, la decision
  versionada de I-37 y el glosario.

## 4. Fuera de alcance

Cada uno es **condicion de detencion**:

varias estaciones; separacion longitudinal entre estaciones; linea Cantilever; separadores; arriostres;
vista frontal, lateral, planta e isometrica; preview; UI; editor WPF; persistencia de proyecto;
`RackSystemKind.Cantilever`; los tres registros; biblioteca; AutoCAD; comandos; Xrecords; bloques; peso;
costo; materiales; tornilleria; anclas; soldaduras; calculo estructural; capacidades; preparacion de
extremos; CNC; shop drawings; familias nuevas de catalogo; un PTR nuevo; y **cambios funcionales de
comportamiento en I-36, I-37A o I-37B**.

**No se toca `docs/HANDOFF.md`** ni se corrigen los hallazgos adyacentes de `docs/ideas-futuras.md`.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/ARCHITECTURE.md`; `docs/guias/agregar-un-sistema.md`;
`docs/guias/glosario.md`; `docs/guias/secciones-estructurales.md`;
`docs/guias/geometria-secciones-estructurales.md`; ADR-0020 a **ADR-0025**, y **ADR-0026**;
`docs/automation/decisions/I-37.md`; los contratos, estados y cierres de I-37A e I-37B. Context Packs:
`architecture-kernel`, `catalogs-data`, `ui-editors` (solo por el patron de estado puro y matriz),
`persistence` (solo como referencia de futuro), `documentation-governance`, `delivery-validation`.

**Lectura obligatoria de codigo antes de extraer la reticula:**
`src/RackCad.Application/Systems/Cantilever/CantileverColumnBaseResolver.cs`, metodo
`BuildRegularPunches`. Es la unica fuente de la formula, y la extraccion tiene que **conservar sus
resultados**.

**Precedente obligatorio de BOM por componentes:** `src/RackCad.Application/Bom/BomBuilder.Components`
—agrupacion por firma de piezas— y `AddPlate`, donde una placa entra con `Length = 0`.

**Precedente obligatorio de matriz pura y alcances:** `SelectiveEditorState.ApplyScope` con
`SelectiveApplyScope`.

## 6. Dependencias

I-36A a I-36D, **I-37A** e **I-37B** integradas en `origin/main` (merge `0610adb`), con **ADR-0024 y
ADR-0025 aceptados**. Se estorba con I-37D, que aun no existe.

## 7. Archivos esperados

**Crear** — `src/RackCad.Domain/Systems/Cantilever/CantileverStation*.cs` y los templates;
`src/RackCad.Application/Systems/Cantilever/CantileverColumnRegularPunchGrid.cs`,
`CantileverStation*.cs`; pruebas nuevas en `tests/RackCad.Tests/`; `docs/adr/0026-*.md`;
`docs/automation/state/I-37C.yml`; este contrato.

**Modificar** — `CantileverColumnBaseResolver.cs` **solo** para consumir la reticula extraida, sin cambiar
un resultado; los enums y tokens (**solo anadiendo**); `docs/ROADMAP.md`; `docs/adr/README.md`;
`docs/initiatives/README.md`; `docs/automation/decisions/I-37.md`; `docs/guias/glosario.md`;
`docs/ARCHITECTURE.md` solo si hace falta.

**No tocar** — `docs/HANDOFF.md`; `src/RackCad.UI`; `src/RackCad.Plugin`;
`src/RackCad.Application/StructuralSections/**`; `assets/`; `deploy/`; `.github/`; `RackCad.sln`; los cinco
sistemas vigentes; el **contrato compartido de BOM** salvo necesidad objetiva demostrada; y el
**comportamiento** de I-37A y I-37B.

## 8. Fases

1. Reclamo.
2. Contrato, ADR-0026, ROADMAP, decision versionada y estado.
3. Contratos de Domain: diseno de estacion, modos y templates.
4. Caracterizacion de I-37A y extraccion de la reticula regular.
5. Composicion de columna y bases, y layout de niveles.
6. Altura automatica y manual, y el resolver de estacion.
7. Matriz pura de brazos y sus alcances.
8. BOM por componentes.
9. Pruebas, regresiones en rojo y guardas de fuente.
10. Evidencia final.

## 9. Pruebas y builds

Builds Debug de Domain, Application, UI y Plugin con cero errores propios; pruebas focalizadas de I-37A,
I-37B **e** I-37C; guardas de fuente; pruebas de BOM; validacion del catalogo estructural; suites completas
de `RackCad.Tests` y `RackCad.UI.Tests`; CI verde en la rama.

Regresion **verificada fallando** para, al menos: implementar la doble como dos subensambles columna-base
completos; duplicar la columna en doble; redondear el claro sin usar troqueles; permitir ajuste hacia abajo;
calcular la doble usando solo un lado; permitir compartir troqueles entre niveles; ignorar la placa al
comprobar traslape; ignorar `TopClearFactor`; normalizar una altura manual insuficiente; generar dos
componentes columna-base en doble; separar brazos identicos por lado en el BOM; calcular el BOM desde el
diseno; duplicar la formula del pitch regular fuera de la autoridad; y usar una altura provisional magica.

## 10. Validacion manual

**No aplica.** I-37C no cambia el dibujo ni la interfaz — igual que I-37A e I-37B, el gate se resuelve
**sobre el codigo**.

## 11. Criterios de aceptacion

Los quince puntos del objetivo, cubiertos por prueba; las invariantes de sencilla, doble, niveles, matriz,
altura, BOM y determinismo; la **equivalencia numerica** de I-37A tras la extraccion, con sus 81 pruebas
focalizadas intactas; guardas de fuente activas; y **cero** cambios de comportamiento en I-36, I-37A,
I-37B, los cinco sistemas vigentes, catalogos, UI y Plugin.

## 12. Condiciones para detenerse

Necesitar un valor sin default aprobado; necesitar un id de produccion o una familia de catalogo que no
existe —incluido un PTR—; que la extraccion de la reticula cambie **cualquier** resultado de I-37A o
I-37B; que el pase final de resolucion difiera del layout previo; o necesitar tocar UI, Plugin o el
contrato compartido de BOM sin una necesidad objetiva demostrada.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-37C.yml`. Sin Pull Request; el merge automatico esta prohibido.

## 14. Evidencia final

Commits de la rama, archivos, pruebas, builds, CI y confirmacion de que `main` no fue modificada.
