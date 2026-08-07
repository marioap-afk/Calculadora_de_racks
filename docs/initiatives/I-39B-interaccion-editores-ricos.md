---
schema: rackcad-initiative/v1
id: I-39B
title: Adopcion del contrato funcional comun en los seis editores ricos
type: architecture
status: implementing
branch: architecture/interaccion-editores-ricos
base_branch: main
priority:
size: M
depends_on: [I-39A]
conflicts_with: [I-39C, I-39D]
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-39B.yml
decision_paths:
  - docs/automation/decisions/I-39.md
  - docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-39B — Adopción del contrato funcional común en los seis editores ricos

> Alcance autorizado por `docs/ROADMAP.md` (Fase 3, fila I-39B) y por **ADR-0029**, aceptado. **Este
> contrato NO amplía el ROADMAP y NO reabre ADR-0019 ni ADR-0029.** La auditoría de apertura
> ([evidencia](../automation/evidence/I-39B-auditoria-editores-ricos.md)) ajusta **cómo** se implementa,
> no **qué** debe entregarse.

## 1. Objetivo

Que los seis editores ricos adopten el contrato funcional común de ADR-0029, sobre una red de
caracterización que hoy no existe, y sin cambiar reglas de producto.

Verificable al cerrar:

1. Existen pruebas que fijan, para las **seis**, el comportamiento de Enter, Escape, caminos de cierre,
   dirty declarado, acciones y sus motivos, autoridad y frescura del preview, y foco inicial.
2. **Ningún camino de cierre pierde trabajo en silencio.** Botón, Escape, X y `Alt+F4` atraviesan una
   política coherente por ventana; la que no tiene ámbito pendiente real cierra directo.
3. El configurador de cabecera respeta al cerrar el ámbito que **ya declara**
   (`HasUnsavedManualEdits`), reutilizando `ConfirmDiscard` en vez de un segundo modelo.
4. Push Back resuelve su cierre sin reducirlo a `IsCancel`, sobre las autoridades existentes.
5. `RackEditorVisualShell` resuelve sus seis tokens sin depender del consumidor, **sin cambiar el
   aspecto de ninguna de las cuatro ventanas ya validadas**.
6. `Editor/` no contiene ningún `using` hacia un namespace de sistema, y una guarda lo impide.
7. Cero cambios de geometría, BOM, persistencia, wire format, GUID, catálogos y reglas de producto.

## 2. Problema

La auditoría de apertura midió el cumplimiento real de ADR-0029 en las seis y encontró dos clases de
incumplimiento que **no** se pueden atacar juntas.

**Lo que se puede corregir sin que el usuario lo note** es poco y urgente:

- **Cobertura de interacción: cero.** Ninguna de las seis tiene una sola aserción sobre Enter, Escape,
  foco, tabulación o cierre. La única cobertura de ese tipo en el repositorio es la que I-39A escribió
  para su piloto. Sin caracterización previa, cualquier adopción del contrato es un cambio a ciegas
  sobre las ventanas de producción.
- **`RackEditorVisualShell` arrastra la misma dependencia latente de recursos** que I-39A corrigió en
  el shell acotado: seis tokens `DynamicResource` de clave de cadena —fondo, tipografía, tamaño de
  fuente, `ShellZoneSpacing`, `ShellSidebarWidth`, `ShellPreviewMinHeight`— que **no** caen al
  diccionario de tema del ensamblado. Hoy está enmascarada porque los cuatro consumidores mergean
  `AppStyles` en `Window.Resources`; se manifestaría en el primer consumidor construido en código, y
  ya se manifiesta en las pruebas que instancian el shell suelto.
- **`Editor/` conoce dos sistemas**: `RecomputeGate.cs` declara `using RackCad.UI.Systems.Selective;` y
  `RecomputeDebouncer.cs` y `DispatcherRecomputeScheduler.cs` declaran `using RackCad.UI.RackFrames;`,
  los tres solo para resolver un `<see cref>` de documentación. Viola D12 y ninguna guarda lo cubre.

**Lo observable** es lo que da sentido a la subiniciativa, y por eso también está aquí:

- **Ninguna de las seis tenía `OnClosing`.** No había punto de intercepción en el proyecto entero.
- **El configurador de cabecera** tiene `IsCancel`, `HasUnsavedManualEdits` y `ConfirmDiscard`, y **el
  cierre no consulta ese dirty**: hoy Escape y la X descartan ediciones manuales sin preguntar, mientras
  la misma ventana sí pregunta al restaurar.
- **Push Back** no lleva `IsCancel`, de modo que Escape no la cierra; y el ámbito perdible al cerrar
  **excede** `ModuleSession`.
- El **Dinámico** conserva un preview obsoleto sin marcarlo y deja materializar desde él.
- **Cama** y **Cabecera** dejan habilitadas acciones que no pueden producir salida válida.
- **Cantilever** pinta un aviso con severidad de error.
- `EditorAction` no sabe expresar acción por defecto, cancelación, si la acción cierra, ni motivo de
  bloqueo, y por eso ninguna ventana la consume.

## 3. Alcance

### 3.1 Caracterización de las seis

Pruebas que fijan el comportamiento **actual**, no el deseable. Cubren, por ventana: presencia y
contenido del botón por defecto y del de cancelación; qué hace hoy cada camino de cierre; qué ámbito
dirty declara, si alguno; qué acciones se deshabilitan y cuáles exponen motivo; qué hace el preview
ante entrada inválida; foco inicial declarado y ausencia de `TabIndex` explícito.

Se caracteriza también, explícitamente, lo que **está mal** y no se corrige aquí, para que la hermana
lo herede fijado: que Push Back no cierra con Escape, que la Cabecera descarta ediciones manuales sin
preguntar, que el Dinámico conserva un preview obsoleto sin marcarlo mientras deja insertar, y que
ninguna de las seis tiene `OnClosing`.

### 3.2 Dependencia de recursos de `RackEditorVisualShell`

El shell mergea el diccionario compartido en sus **propios** recursos, como ya hace
`RackBoundedEditorShell`, reutilizando `ShellResources.Shared`. Regresión verificada **en rojo**.

Medido antes de tocar, y por eso el cambio no es observable: cada `x:Key` del repositorio se define
**una sola vez** y en `AppStyles.xaml`; los cuatro consumidores **no declaran ni un solo `x:Key`**, de
modo que el diccionario del control no puede sombrear un valor distinto.

### 3.3 `Editor/` sin nombres de sistema

Se retiran los tres `using` y se ajustan los `<see cref>` que los motivaban. Una guarda nueva impide
que vuelvan, con el mismo patrón que la de `Shell/`.

### 3.4 Política común de cierre

Infraestructura **neutral** en `Shell/`: un ámbito de trabajo pendiente que una ventana **declara** y un
punto único por el que pasan botón, Escape, X y `Alt+F4`, con una costura de confirmación testeable
—hoy `MessageBox.Show` no la tiene—. Una ventana **sin** ámbito pendiente real cierra directo:
ADR-0029 D8 admite `NotApplicable`, y **no** se inventa un dirty global que el producto no tiene.

### 3.5 Configurador de cabecera y Push Back

La Cabecera reutiliza `HasUnsavedManualEdits` y `ConfirmDiscard`. Push Back resuelve su cierre sobre
`ModuleSession` y las autoridades existentes, **sin** reducirlo a `IsCancel` y **sin** inventar
persistencia ni un modelo de sesión nuevo.

### 3.6 Dinámico, Cama, Cabecera y Cantilever

Preview obsoleto clasificado por autoridad y frescura, con la materialización bloqueada cuando el
contrato lo exija y **sin** borrar un preview válido que el producto hoy conserva. Acciones que no
pueden producir salida válida, bloqueadas **con motivo** mediante la infraestructura común. Severidades
de Cantilever corregidas **sin** tocar lógica de I-37.

### 3.7 Evolución neutral de `EditorAction`

Solo lo que haga falta para expresar acción por defecto, cancelación, si cierra y motivo de bloqueo,
**agnóstico al sistema y con pruebas**. Sin migración cosmética masiva y sin modelos paralelos.

## 4. Fuera de alcance

Cada uno es **condición de detención**:

- **El contrato de inserción paralelo del configurador de cabecera**, que ADR-0029 excluye
  expresamente. Si proteger su cierre lo exigiera, se detiene y se registra.
- **La adopción del shell en Cama y Cabecera** solo se hace si la auditoría demuestra que es compatible,
  caracterizable y sin reestructurar lógica de producto; si no, se registra la desviación concreta.
- Arquetipos **B**, **C** y **D**: I-39C e I-39D. Larguero, los cuatro componentes Cantilever, los
  diálogos y las utilitarias **no se tocan**.
- Geometría, resolvers, BOM, persistencia, wire format, GUID, catálogos, Domain, Application y Plugin.
- Paquetes NuGet nuevos (**ADR-0012**).
- `docs/HANDOFF.md` y `docs/ROADMAP.md` más allá de la fila autorizada: sesión de integración.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/AUTOMATION_PLAN.md`; `docs/ROADMAP.md` Fase 3;
**ADR-0029** y **ADR-0019**; `docs/automation/decisions/I-39.md`; el contrato, el estado y las
evidencias de **I-39A**; los contratos de I-14, I-15, I-24, I-30, I-31 e I-37D. Context Packs:
`ui-editors`, `architecture-kernel`, `delivery-validation`, `documentation-governance`.

## 6. Dependencias

**I-39A integrada.** ADR-0029 **aceptado**. Conflictos que deben permanecer inactivos: I-39C, I-39D y
la hermana de adopción observable. Cualquier iniciativa que toque `src/RackCad.UI/Shell/**`,
`src/RackCad.UI/Editor/**` o `Themes/**` se serializa con ésta.

## 7. Archivos esperados

**Crear**: `tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs`; este contrato;
`docs/automation/state/I-39B.yml`; `docs/automation/evidence/I-39B-auditoria-editores-ricos.md`.

**Modificar**: `src/RackCad.UI/Shell/RackEditorVisualShell.cs`; `src/RackCad.UI/Editor/RecomputeGate.cs`;
`src/RackCad.UI/Editor/RecomputeDebouncer.cs`; `src/RackCad.UI/Editor/DispatcherRecomputeScheduler.cs`;
`tests/RackCad.UI.Tests/EditorVisualShellTests.cs`; `docs/ROADMAP.md` (solo la fila);
`docs/initiatives/README.md`.

**Hotspots que NO deben aparecer en el diff**: los `.xaml` y `.xaml.cs` de las seis ventanas ricas
—incluidos `RackSelectiveWindow.xaml.cs` y `RackFrameConfiguratorViewModel.cs`, archivos calientes de
`WORKFLOW.md` §7—, los cuatro componentes Cantilever, Larguero, los diálogos, las utilitarias,
`src/RackCad.Plugin/**`, `src/RackCad.Application/**`, `src/RackCad.Domain/**`, `assets/`, `deploy/`,
`.github/`, `docs/HANDOFF.md`.

## 8. Fases

1. Registro y reclamo.
2. **Caracterización** de las seis, verde sobre el árbol sin tocar.
3. Fix de recursos del shell rico, con su regresión en rojo.
4. Limpieza de `using` en `Editor/` y su guarda.
5. Cierre: suites, builds, CI, estado y evidencia.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Rebuild **completo** donde se verifique una guarda en rojo: el build incremental no recoge un cambio en
el constructor de un control y produce falso verde en ambos sentidos (lección de I-39A).

Filtros dirigidos, todos con pruebas: caracterización de editores ricos, shell visual, shell acotado,
censo, guardas de I-37D, migraciones de Selectivo/Dinámico/Push Back, goldens, persistencia, handlers.

Regresión obligatoria sin cambios: goldens, persistencia, handlers, round-trip, validador de catálogos,
las cinco familias de sistema y las líneas I-36, I-37 y I-39A.

## 10. Validación manual

**OBLIGATORIA.** `requires_autocad: true`, `requires_owner_validation: true`. I-39B cambia
comportamiento observable de cierre y de acciones en ventanas de produccion, asi que el gate es el
veredicto del Owner en **AutoCAD 2025** sobre el DLL Debug del worktree, construido desde el SHA
candidato. El ROADMAP la marca con la mano.

Checklist acotado a lo observable: en **Cabecera**, con dirty real, Escape, X y boton de cierre, con
confirmacion aceptada y rechazada, y sin dirty el cierre normal; en **Push Back**, con edicion pendiente
relevante, Escape, X y Cerrar, confirmando y cancelando el descarte, y sin perdida silenciosa; en el
**Dinamico**, captura invalida con preview anterior, su estado visible y las acciones bloqueadas cuando
corresponda; en **Cama** y **Cabecera**, acciones bloqueadas con su razon visible, y el shell si se
migraron; y regresion de que las seis siguen abriendo y operando con normalidad.

## 11. Criterios de aceptación

1. Las seis ventanas tienen caracterización de Enter, Escape, cierre, dirty, acciones, preview y foco.
2. `RackEditorVisualShell` resuelve sus seis tokens sin depender del consumidor, con regresión en rojo.
3. Las pruebas de migración de Selectivo, Dinámico y Push Back siguen verdes **sin editarse**: son la
   prueba de que el aspecto no cambió.
4. `Editor/` sin `using` de sistema, con guarda verificada en rojo.
5. Cero cambios en los `.xaml`/`.xaml.cs` de las seis ventanas.
6. Cero geometría, BOM, persistencia, identidad, wire format y catálogos en el diff.
7. Diff confinado a `src/RackCad.UI/{Shell,Editor}`, `tests/**` y la documentación de I-39B.

## 12. Condiciones para detenerse

- La caracterización revela que una de las seis **ya** incumple algo de forma que corregirlo sea
  inevitable para que el resto pase.
- El fix de recursos cambia el aspecto de alguna de las cuatro ventanas migradas.
- Retirar un `using` obliga a cambiar código en vez de un comentario.
- Aparece la necesidad de tocar cualquiera de las seis ventanas.
- `origin/main` avanza sobre los hotspots declarados.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-39B.yml`, `schema: rackcad-automation-state/v1`. El agente nunca hace merge
automático; la integración es una sesión separada y autorizada.

## 14. Evidencia final

Commits, evidencia de la auditoría, resultados de suites y builds, CI, y confirmación de que `main` no
fue modificada.
