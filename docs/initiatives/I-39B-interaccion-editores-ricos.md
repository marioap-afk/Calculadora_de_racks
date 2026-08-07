---
schema: rackcad-initiative/v1
id: I-39B
title: Caracterizacion y fundacion de interaccion de los editores ricos
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
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-39B — Caracterización y fundación de interacción de los editores ricos

> Alcance autorizado por `docs/ROADMAP.md` (Fase 3, fila I-39B) y por las decisiones vinculantes del
> Owner en [`decisions/I-39.md`](../automation/decisions/I-39.md), incluida la **decisión de partición
> del 2026-08-07** que separó la mitad no observable de los editores ricos de la mitad que cambia lo
> que el usuario ve. **Este contrato NO amplía el ROADMAP y NO reabre ADR-0019 ni ADR-0029.**

## 1. Objetivo

Dejar los seis editores ricos **caracterizados** y la infraestructura común **sin dependencias
latentes**, de modo que la adopción observable del contrato pueda hacerse después sobre una red de
seguridad que hoy no existe.

Verificable al cerrar:

1. Existen pruebas que fijan, para las **seis** ventanas del arquetipo A, el comportamiento actual de
   Enter, Escape, caminos de cierre, dirty declarado, acciones y sus motivos, autoridad y frescura del
   preview, y foco inicial. Hoy la cobertura de interacción de esas seis es **cero**.
2. `RackEditorVisualShell` resuelve sus seis tokens sin depender de que el consumidor haya mergeado
   `AppStyles`, **sin cambiar el aspecto de ninguna de las cuatro ventanas ya validadas**.
3. `Editor/` no contiene ningún `using` hacia un namespace de sistema, y una guarda lo impide.
4. **Ninguna** de las seis ventanas cambia comportamiento observable.

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

**Lo que no se puede corregir sin que el usuario lo note** es mucho, y por eso no está aquí: la
política de cierre unificada, Push Back, la Cabecera, el preview obsoleto del Dinámico, `Insertar`
habilitado sin modelo válido, la adopción del shell en Cama y Cabecera y la adopción de `EditorAction`.
Todo eso es la iniciativa hermana.

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

## 4. Fuera de alcance

Cada uno es **condición de detención**:

- **La política de cierre unificada** y todo `OnClosing`. Decisión del Owner ya tomada para cuando se
  implemente —solo preguntan las ventanas que **declaran** un ámbito transaccional—, pero la
  implementación es de la hermana.
- **Push Back**: no se le añade `IsCancel` ni nada que cambie su cierre.
- **Cabecera**: ni foco inicial, ni `ConfirmDiscard` al cerrar, ni su contrato de inserción paralelo
  —que ADR-0029 excluye expresamente—, ni la adopción del shell.
- **Cama**: no se adopta el shell ni se cambia su contrato de tamaño.
- **Dinámico**: no se toca el preview obsoleto ni la habilitación de sus acciones de dibujo.
- **Cantilever**: no se cambia la severidad con que pinta sus avisos.
- **`EditorAction`, `EditorActionBar`, `EditorStatusPresenter`**: no se adoptan en ninguna de las seis.
- Arquetipos **B**, **C** y **D**: I-39C e I-39D.
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

**`no aplica`.** `requires_autocad: false`, `requires_owner_validation: false`. I-39B **no cambia
comportamiento observable**: añade pruebas, hace que un token resuelva por una vía distinta al mismo
valor, y retira tres `using` que solo servían a comentarios. El ROADMAP no la marca con ✋. La
equivalencia se demuestra por caracterización, no se afirma.

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
