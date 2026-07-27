---
schema: rackcad-initiative/v1
id: I-23
title: Namespaces finales por sistema
type: refactor
status: implementing
branch: refactor/namespaces-sistemas
base_branch: main
priority:
size: M
depends_on: [I-08, I-15, I-16, I-20, I-21, I-22]
conflicts_with: [I-20, I-21, I-22, I-24, I-25, I-30, I-31, I-32, I-33, I-34, I-35]
context_packs:
  - architecture-kernel
  - system-selective
  - system-dynamic-flowbed
  - autocad-plugin
  - delivery-validation
automation_state_path: docs/automation/state/I-23.yml
decision_paths: []
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

# I-23 — Namespaces finales por sistema

## 1. Objetivo

Cerrar el hallazgo **E8** de la [auditoria 2026-07](../auditoria-arquitectura-2026-07.md): el namespace
`Systems` es plano y multi-sistema, con nombres fosiles que ya no describen lo que contienen. Al
terminar, cada sistema vive en su propio namespace y su propia carpeta, el `Shared` contiene solo lo
que es de verdad compartido, y una regla comprobable impide que el namespace y la ruta vuelvan a
divergir.

Es un refactor **mecanico**: mueve archivos y reescribe declaraciones de namespace y `using`. No
cambia una sola linea de logica.

I-23 **cierra la Fase 5** y depende de todas sus predecesoras (ROADMAP, Fase 5).

## 2. Problema

La auditoria lo enuncia asi:

> **E8** — Namespace `Systems` plano multi-sistema con nombres engañosos (`DynamicSystemPlan` lo usan
> todos; el selectivo consume constantes `DynamicRackDefaults.Separator*`).

Medido sobre la base `b43b5d1`:

- `RackCad.Application.Systems` concentra **101 archivos** de cinco sistemas distintos sin ninguna
  separacion fisica ni de namespace;
- `RackCad.Domain.Systems` (22) y `RackCad.Plugin.Systems` (12) repiten el mismo patron plano;
- `DynamicSystemPlan` **no es el plan del sistema dinamico**: es el plan de corridas de cabecera y lo
  consumen los cuatro sistemas (Selectivo, Dinamico, Push Back y cama). El nombre miente;
- `RackCad.Application.Headers` mezcla el vocabulario de materializacion compartido
  (`HeaderBlockInstance`, `LateralHeaderLayout`) con los builders de la cabecera fisica;
- `RackCad.UI` repite el MISMO namespace plano multi-sistema: las cinco ventanas de sistema, el
  configurador de cabecera y sus modelos conviven en la raiz `RackCad.UI`;
- `RackCad.Plugin.Headers` no contiene el modelo fisico de la cabecera sino adapters e infraestructura
  de dibujo (`BlockPlacement`, `BlockLibraryImporter`, `RackBlockRenamer`, los drawers y sus
  resultados);
- no existe `.editorconfig`, asi que nada impide que el proximo archivo nazca en el lugar equivocado.

El costo es concreto: un lector no puede saber a que sistema pertenece un tipo sin abrirlo, y el
sistema N+1 hereda un namespace plano que ya tiene 101 archivos.

### 2.1 Defectos de la primera ronda de I-23 (corregidos en la segunda)

La primera ronda excluyo dos areas **sin autorizacion**, y este contrato las registraba como si
fueran decisiones legitimas. No lo eran:

1. **`Plugin.Headers` se preservo por una analogia falsa** con `RackFrames`. La analogia no se
   sostiene: ese subarbol no modela la cabecera fisica, la DIBUJA, y la mitad de sus tipos
   (`BlockPlacement`, `BlockLibraryImporter`, `RackBlockRenamer`) no son de cabecera en absoluto.
2. **`RackCad.UI` quedo entera fuera de alcance**, pese al alcance normativo del Owner, y el
   inventario omitio la clasificacion de UI y de los dos proyectos de prueba.

Ambas exclusiones quedan **eliminadas**; la seccion 4 ya no las contiene.

## 3. Alcance

Autorizado por el ROADMAP (fila I-23, Fase 5) y por el encargo:

1. **Separacion por sistema** de los namespaces `Systems` de los **cuatro** proyectos de producto
   (Domain, Application, **UI** y Plugin) en `Selective`, `Dynamic`, `PushBack`, `FlowBed`,
   `Larguero` y `Shared`. La ruta en disco acompaña al namespace.
2. **`RackFrames` se conserva** para la cabecera fisica, en Domain, Application **y UI**: el
   configurador y sus modelos son la cabecera fisica, no un sistema de rack.
3. **`Application.Headers` se disuelve**: los artefactos de **materializacion** pasan a
   `Application.Drawing`; los builders de la **cabecera fisica** pasan a `Application.RackFrames`.
4. **`Plugin.Headers` se disuelve** en `Plugin.Drawing`: son adapters e infraestructura de dibujo y
   materializacion, no el modelo fisico de la cabecera. Deja los dos proyectos simetricos
   (`Application.Drawing` y `Plugin.Drawing`).
5. **Renombre fosil autorizado**: `DynamicSystemPlan` pasa a `Drawing.HeaderRunPlan`.
6. **`.editorconfig`** con reglas comprobables de namespace/ruta, mas **source guards focalizados**,
   incluida la frontera WPF (x:Class, pack URIs y construccion real de las ventanas).
7. **Barrido de referentes** de las rutas movidas en la documentacion viva (WORKFLOW seccion 8).

### 3.1 Regla de clasificacion (objetiva y comprobable)

Un archivo pertenece al namespace del sistema que su tipo de primer nivel **nombra y modela**. Solo
hay tres excepciones, y cada una exige evidencia:

| Excepcion | Criterio | Destino |
|---|---|---|
| Neutral de nombre **y** de contenido | no nombra ningun sistema y no ramifica por ninguno | `Shared` |
| Vocabulario de materializacion | describe piezas/instancias/planos de bloque que consumen todos | `Drawing` |
| Nombre neutral con contenido de un sistema | el nombre no dice el sistema pero el contenido y sus llamadores si | ese sistema |

Consumir un contrato de otro sistema **no** mueve un tipo: la composicion entre sistemas es legal
(ROADMAP, principio 5). Por eso `SelectiveDesviadorPlan` permanece en `Selective` aunque el Dinamico y
Push Back lo consuman, y `FlowBedLateralBuilder` permanece en `FlowBed` aunque los dos lo compongan.

### 3.2 Por que `HeaderRunPlan` y no `SystemPlan`

El ROADMAP anota el renombre como `DynamicSystemPlan` a `SystemPlan`. **No se aplica ese nombre**:
en el arbol actual es ambiguo, porque `SystemPlan` sugiere "el plan de un sistema" y colisiona
conceptualmente con `SystemBomBuilder`, `SystemDescriptor`, `SystemRegistry` y `SystemBlockWriter`, que
si son por sistema.

La evidencia del tipo dice otra cosa: agrupa `HeaderGroup` (definiciones distintas de cabecera) con sus
`HeaderPlacement` (donde se coloca cada una a lo largo de la corrida) mas las instancias sueltas. Es el
**plan de corridas de cabecera**, y lo consumen los cuatro sistemas — exactamente lo que E8 señala. Se
renombra a **`HeaderRunPlan`** y se aloja en `Drawing` junto al vocabulario que produce y consume.

## 4. Fuera de alcance

**Congelacion funcional total.** No se cambia:

- logica, algoritmos ni geometria;
- firmas, salvo el namespace y el unico nombre autorizado (`DynamicSystemPlan` a `HeaderRunPlan`);
- accesibilidad de ningun miembro o tipo;
- wire format, JSON, enums persistidos, schemas, `SchemaVersion`, fallback legacy;
- GUID, identidad, metadata I-11, Xrecords;
- comandos de AutoCAD, aliases, prompts ni textos visibles;
- BOM, catalogos, bloques DWG, `assets/` ni `deploy/`.

Tampoco entra: **I-25** (guardas traseras, diferida), la **defensa** que I-34 dejo como candidato
independiente, el **preview visual** que I-18 difirio, `DesviadorCellsAreByPost`, ni ninguna de las
mejoras que siguen en [`../ideas-futuras.md`](../ideas-futuras.md). **Push Back v1 queda estable**: I-23
no lo toca funcionalmente.

### 4.1 Excepciones justificadas (lo unico que NO se reparte por sistema)

Estas son excepciones **con evidencia**, no exclusiones de conveniencia. Las dos exclusiones que la
primera ronda si tomo sin autorizacion —`Plugin.Headers` y `RackCad.UI`— quedaron **eliminadas**.

1. **Infraestructura transversal de UI**: `UI.Controls`, `UI.Editor`, `UI.Preview`, `UI.Shell` y
   `UI/Themes` no pertenecen a ningun sistema y no se reparten. De `Preview` sale un unico tipo,
   `PushBackPreviewRenderer`, cuyo unico consumidor es Push Back; `EditorPreviewSurface`/`Parts`/
   `Palette` los comparten el Dinamico y Push Back y se quedan.
2. **Dialogos compartidos de seguridad**: `SelectiveSafetyWindow` y los cinco `Safety*GridWindow`
   permanecen en la raiz de `RackCad.UI`. `SelectiveSafetyWindow` la abren las ventanas del
   **Selectivo, del Dinamico y de Push Back**; los `Safety*GridWindow` son agnosticos a
   `RackSystemKind` desde I-34. **Un dialogo compartido no se asigna a un sistema por numero de
   consumidores**: su nombre es un fosil, su contenido es neutral, y renombrarlos no esta autorizado,
   asi que la raiz es donde esa neutralidad queda visible.
3. **Superficie transversal de producto**: menu, biblioteca, los dos BOM, lista, ayuda, las dos
   ventanas de almacen, `UiSupport`, `ObservableObject`, `CatalogOption`, `EnumDisplayConverter` y
   `PreviewCanvasPainter` se quedan en la raiz.
4. **Los dos proyectos de prueba conservan un unico namespace de ensamblado**
   (`RackCad.Tests`, `RackCad.UI.Tests`). Es la unica excepcion a la regla de carpeta, y es
   **explicita y comprobable**:

   - medido sobre este arbol, **92 de 220 archivos de prueba (42 %) ejercitan mas de un sistema** y 48
     tocan tres o mas: los golden comparan Selectivo contra Dinamico contra Push Back en el mismo
     archivo. Asignarles un propietario seria arbitrario justo donde la regla de I-23 exige que sea
     **inequivoco**;
   - `FullyQualifiedName~` es la interfaz operativa de verificacion del repo. Mover los namespaces
     cambiaria el significado de cada filtro dirigido registrado en la evidencia, y un filtro que pasa
     a coincidir con **cero** pruebas no avisa (AGENTS.md);
   - las fixturas compartidas (`EditorWindowTestSupport`, `StaTestRunner`, `TestCatalogIds`) las
     consumen todos los sistemas;
   - xUnit descubre por **ensamblado**, no por namespace: no hay beneficio en tiempo de ejecucion.

   La excepcion **no es una exencion**: la vigila
   `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`, que exige exactamente
   un namespace por archivo e igual a la raiz del ensamblado. El inventario declara igualmente el
   **propietario** de cada tipo de prueba (el sistema o sistemas que ejercita).

## 5. Contexto requerido

- [AGENTS.md](../../AGENTS.md), [WORKFLOW.md](../WORKFLOW.md), [ROADMAP.md](../ROADMAP.md) Fase 5,
  [HANDOFF.md](../HANDOFF.md), [ARCHITECTURE.md](../ARCHITECTURE.md) seccion 7.4.
- [Auditoria 2026-07](../auditoria-arquitectura-2026-07.md), hallazgo **E8** y recomendacion BAJA 16.
- Context Packs: `architecture-kernel`, `system-selective`, `system-dynamic-flowbed`,
  `autocad-plugin`, `delivery-validation`.
- [`guias/agregar-un-sistema.md`](../guias/agregar-un-sistema.md): el patron que estos namespaces deben
  dejar legible para el sistema N+1.
- ADR-0006 (AutoCAD solo en el Plugin) y ADR-0011 (patron ARRAY): fijan que el movimiento no puede
  cruzar capas ni alterar el agrupamiento de bloques.
- El inventario tipo por tipo de esta iniciativa:
  [`I-23-inventario-namespaces.md`](I-23-inventario-namespaces.md).

## 6. Dependencias

Integradas y verificadas en `main` = `b43b5d1`: **I-08, I-15, I-16, I-20, I-21, I-22**, y ademas
I-24, I-30, I-31, I-32, I-33, I-34 e I-35, que llegaron despues de escrita la fila del ROADMAP y
tambien mueven codigo dentro de `Systems`.

Conflicto activo: **ninguno**. Al reclamar, la unica rama en `origin` era `main`, asi que no habia
iniciativa funcional en curso. **I-25 queda diferida** y no debe reclamarse mientras I-23 este viva:
la fila del ROADMAP declara que I-23 se estorba con toda la Fase 5.

Entradas del dueño requeridas: **ninguna**. El ROADMAP no marca I-23 con validacion en AutoCAD.

## 7. Archivos esperados

**176 archivos movidos** con `git mv` (142 en la primera ronda + 8 del Plugin + 26 de la UI), mas la
reescritura de `namespace`/`using` en los archivos que los consumen. Mapa final REAL, tal como quedo
en el arbol (el desglose tipo por tipo, con consumidores y propietario, esta en el
[inventario](I-23-inventario-namespaces.md)):

| Proyecto | Frontera | Archivos |
|---|---|---|
| Domain | `Systems.{Selective,Dynamic,PushBack,FlowBed,Larguero,Shared}` | 4 / 7 / 4 / 3 / 1 / 3 |
| Domain | `RackFrames` (cabecera fisica, sin cambios) | 18 |
| Application | `Systems.{Selective,Dynamic,PushBack,FlowBed,Larguero,Shared}` | 28 / 34 / 26 / 2 / 1 / 9 |
| Application | `Drawing` (materializacion; recibe `HeaderRunPlan`) | 5 |
| Application | `RackFrames` (recibe los builders de cabecera de `Headers`) | 14 |
| Application | `Persistence`, `Catalogs`, `Bom`, `Layout`, `Geometry`, `Diagnostics`, … | sin cambios |
| **UI** | `Systems.{Selective,Dynamic,PushBack,FlowBed,Larguero}` | 2 / 1 / 6 / 1 / 1 |
| **UI** | `RackFrames` (configurador y sus modelos) | 9 |
| **UI** | `Controls`, `Editor`, `Preview`, `Shell`, `Themes` (transversal) | 13 / 11 / 3 / 7 / — |
| **UI** | raiz (menu, biblioteca, BOM, lista, dialogos compartidos de seguridad) | 21 |
| Plugin | `Systems.{Selective,Dynamic,PushBack,FlowBed,Shared}` | 2 / 3 / 3 / 1 / 3 |
| **Plugin** | `Drawing` (recibe entero el disuelto `Plugin.Headers`) | 8 |
| Plugin | `KindHandlers` y raiz de comandos | 8 / 21 |
| Tests | `RackCad.Tests` (excepcion, seccion 4.1) | 159 |
| Tests | `RackCad.UI.Tests` (excepcion, seccion 4.1) | 61 |

Cinco namespaces se disuelven por completo: `Domain.Systems`, `Application.Systems`, `Plugin.Systems`
(planos), `Application.Headers` y `Plugin.Headers`.

Se crean: `.editorconfig`, la guarda de namespace/ruta `NamespaceFolderGuardTests` (7 aserciones) y la
guarda de frontera WPF `UiSystemBoundaryGuardTests` (3 aserciones: construccion real de las seis
ventanas migradas, `x:Class` contra carpeta y code-behind, y pack URIs).

Se actualizan por barrido de rutas: los dos Context Packs por sistema, los ADR 0009/0011/0013/0014/0016
(enlaces), `guias/catalogos-y-plantillas.md`, `guias/generacion-cabecera-lateral.md`,
`guias/agregar-un-sistema.md` (seccion 0.bis), `ARCHITECTURE.md` 7.4 y las rutas de codigo citadas en
`AGENTS.md` y `WORKFLOW.md` seccion 7. `docs/archivo/` **no** se toca (es historia) y
`HANDOFF.md`/`ROADMAP.md` se tocan solo en la sesion de integracion (WORKFLOW 4.5.4).

Una desviacion material de este inventario obliga a detenerse.

## 8. Fases

1. **Reclamo** — rama, worktree, commit vacio con `Claim-Id`, push sin force. **Hecha** (`ed81d0a`).
2. **Contrato e inventario** — este documento y el inventario tipo por tipo con consumidores; captura
   de la baseline completa antes de mover nada.
3. **Movimiento** — `git mv` por frontera (Domain, Application, Plugin, `Headers`), reescritura de
   `namespace`/`using`, renombre autorizado. Un commit por frontera.
4. **Reglas** — `.editorconfig` y source guards focalizados de namespace/ruta.
5. **Verificacion** — suites completas, filtros dirigidos con conteo positivo, builds Debug de UI y
   Plugin, validador de catalogos, bundle y comparacion antes/despues.
6. **Publicacion** — revision del diff completo y push de la rama. **No** se integra.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Baseline capturada sobre `b43b5d1` **antes** de mover: **1612** pruebas en `RackCad.Tests` y **491** en
`RackCad.UI.Tests`, cero fallos. El refactor no añade ni quita pruebas de comportamiento: los mismos
conteos, mas las guardas nuevas de namespace/ruta, deben seguir en verde.

Ademas: filtros dirigidos con **conteo positivo** (un filtro que corre cero pruebas no prueba nada), los
**7 goldens** de `tests/RackCad.Tests/Golden/`, el validador de catalogos de I-19 y la verificacion del
bundle de I-12. **Ningun golden debe cambiar**; si uno cambia, el refactor dejo de ser mecanico y hay
que detenerse.

AutoCAD debe estar **cerrado** durante los builds (bloqueo de DLL, trampa conocida de AGENTS.md).

## 10. Validacion manual

**No aplica** como gate: el ROADMAP no marca I-23 con validacion del dueño y la iniciativa no cambia
dibujo, BOM, catalogos ni comandos. La equivalencia se sostiene con los goldens, la comparacion
antes/despues de planes, BOM, serializacion, legacy, metadata, handlers y comandos, y los builds Debug.

El riesgo de runtime residual se declara en la evidencia final, no se oculta.

## 11. Criterios de aceptacion

1. Los **cuatro** namespaces `Systems` (Domain, Application, UI y Plugin) quedan separados por sistema;
   **ningun archivo** permanece en la raiz plana de `Systems`.
2. `Application.Headers` y `Plugin.Headers` no existen; sus artefactos de materializacion y dibujo estan
   en `Drawing` y los builders de cabecera fisica en `RackFrames`.
3. `DynamicSystemPlan` ya no existe; `Drawing.HeaderRunPlan` ocupa su lugar con **la misma superficie
   publica** (mismos miembros, misma accesibilidad, mismo comportamiento).
4. `.editorconfig` y las guardas fallan si un archivo declara un namespace que no corresponde a su
   carpeta, si vuelve la raiz plana, si reaparece un namespace disuelto, si un `x:Class` deja de
   corresponder a su carpeta, si una URI de recurso deja de ser absoluta, o si un proyecto de prueba
   declara un namespace distinto de la raiz de su ensamblado. **Verificado en rojo bajo infraccion
   inyectada**, no solo en verde.
4.b Las **seis ventanas WPF migradas se construyen de verdad** en pruebas (lo que ejecuta
   `InitializeComponent` y resuelve la URI generada mas `AppStyles`).
5. Suites completas verdes con los conteos de la baseline mas las guardas nuevas; goldens **identicos**;
   builds Debug de UI y Plugin sin errores propios; CI verde sobre la punta de la rama.
6. La comparacion antes/despues de planes, BOM, serializacion, legacy, metadata, handlers y comandos no
   muestra **ninguna** diferencia.
7. `git log --stat` de la rama muestra renombres (`R`), no borrados mas altas: el historial de cada
   archivo se conserva.

## 12. Condiciones para detenerse

- Un golden cambia, o la comparacion antes/despues muestra cualquier diferencia.
- Un movimiento exige tocar logica para compilar (señal de que el corte de namespace esta mal elegido).
- Aparece un consumidor que resuelve tipos por nombre completo, reflexion o discriminador de JSON.
- El trunk avanza: hay que rebasar antes de seguir.
- Alguien reclama I-25 u otra iniciativa de Fase 5 en `origin`.
- Se descubre que un archivo no encaja en la regla de clasificacion de la seccion 3.1 sin inventar una
  categoria nueva.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en [`../automation/state/I-23.yml`](../automation/state/I-23.yml). No se abre Pull
Request: el flujo de este repo integra por `git merge --no-ff` en una sesion dedicada (WORKFLOW 4.5) y
el agente **nunca** hace merge. El merge automatico esta prohibido.

## 14. Evidencia final

Se completa al cerrar la sesion: SHA base, `Claim-Id`, inventario, mapa final, commits, diff por
frontera, pruebas, builds, CI y riesgos de runtime pendientes. `main` no se modifica en ningun momento.
