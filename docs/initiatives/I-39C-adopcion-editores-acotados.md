---
schema: rackcad-initiative/v1
id: I-39C
title: Adopcion del contrato funcional comun en los editores acotados
type: architecture
status: integrated
branch: architecture/adopcion-editores-acotados
base_branch: main
priority:
size: M
depends_on: [I-39A, I-39B]
conflicts_with: [I-39D]
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-39C.yml
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

# I-39C — Adopción del contrato funcional común en los editores acotados

> Alcance autorizado por `docs/ROADMAP.md` (Fase 3, fila I-39C) y por **ADR-0029**, aceptado. **Este
> contrato NO amplía el ROADMAP y NO reabre ADR-0019 ni ADR-0029.** Hereda lo que I-39A y I-39B
> dejaron explícitamente asignado a esta subiniciativa; no abre alcance nuevo.

## 1. Objetivo

Cerrar el **arquetipo B** completo: sus **seis** ventanas comparten un shell, un contrato de tamaño y
un contrato de acciones, y ninguna de las tres deudas que I-39A e I-39B le asignaron sigue viva.

Verificable al cerrar:

1. **Ningún** XAML nombra `CantileverComponentEditorShell`, el tipo **ya no existe**, y las guardas que
   lo vigilaban quedan **reapuntadas al tipo real**, no debilitadas.
2. `RackBoundedEditorShell` resuelve sus tokens **sin depender del consumidor** y **sin sombrear** el
   diccionario del consumidor, con el mismo patrón que I-39B fijó para el shell rico.
3. El **contrato de tamaño del arquetipo B** deja de ser letra muerta: las seis ventanas obtienen
   tamaño inicial y mínimos de los tokens `BoundedEditor*`, ninguna declara literales propios y ninguna
   aplica `EditorShellWindowStyle`, que es el contrato del arquetipo **A**.
4. `EditorAction` sabe describir **acción por defecto** y **acción de cancelación**, y tiene al menos un
   **consumidor productivo real** que las usa. Es exactamente el motivo por el que el piloto de I-39A no
   pudo adoptar `EditorActions.Button` y por el que I-39B declaró la desviación.
5. `RackLargueroWindow` está **caracterizada antes de migrar** y adopta el shell del arquetipo B sin
   perder `sourceProject`, los metadatos JSON desconocidos de I-11, el guardado en biblioteca, el
   catálogo, el BOM modal ni el comportamiento para su llamador.
6. Las **seis** ventanas del arquetipo B declaran, con pruebas, autoridad y frescura de su preview, sus
   acciones con motivo visible al bloquearse, su camino de cierre, su foco inicial y su tamaño.
7. Cero cambios de geometría, BOM, persistencia, wire format, GUID, catálogos y reglas de producto.

## 2. Problema

El arquetipo B quedó a medias **por decisión deliberada y registrada**, no por descuido:

- **La fachada `CantileverComponentEditorShell` existe para que cuatro XAML ya validados no se
  tocaran.** I-39A movió el shell a infraestructura compartida y dejó una subclase vacía en la ruta
  vieja: al no sobrescribir `DefaultStyleKeyProperty` hereda la clave de estilo del tipo base, así que
  los cuatro XAML renderizan igual **sin diff**. Es un andamio con fecha de retiro escrita.
- **El shell acotado mergea el diccionario compartido incondicionalmente**, que es justo lo que I-39B
  midió como defecto en el shell rico: sombrea el diccionario del consumidor. Hoy no se observa —ningún
  consumidor del arquetipo B declara un solo `x:Key`—, pero la asimetría entre los dos shells es real.
- **El contrato de tamaño del arquetipo B es letra muerta.** Las cuatro ventanas Cantilever aplican
  `EditorShellWindowStyle` —el contrato del editor **rico**— y declaran `Width`/`Height` locales sin
  mínimos propios. En precedencia WPF el valor local gana al setter del estilo, pero `MinWidth` y
  `MinHeight` vienen del estilo y **clampean**: las cuatro declaran un ancho que **nunca** se produce.
  Los tokens `BoundedEditor*` existen desde I-39A y **nadie los lee**: el piloto repite sus cuatro
  números a mano.
- **`EditorAction` no sabe declarar acción por defecto ni cancelación.** Es el motivo textual por el que
  el piloto de I-39A no adoptó `EditorActions.Button` y por el que I-39B declaró la desviación en los
  editores ricos. Mientras siga así, la infraestructura no puede tener consumidores sin romper el
  contrato de teclado que ADR-0029 D7 fija.
- **`RackLargueroWindow` no tiene una sola prueba de interfaz.** Es la última ventana del arquetipo B
  sin shell y sin cobertura, con un chrome escrito a mano —colores literales `#1F2933`, `#9AA7B4`,
  `#617080`, `#D8DEE6`— que no pasa por ningún token compartido.

## 3. Alcance

### 3.1 Caracterización antes de migrar (ADR-0029 D13)

Pruebas que fijan el comportamiento **actual** de las seis ventanas del arquetipo B en las dimensiones
del contrato —acción por defecto y de cancelación, caminos de cierre, dirty declarado, acciones y sus
motivos, autoridad y frescura del preview, foco inicial y tamaño real de apertura—, donde hoy solo
existe la del piloto. La cobertura vigente de los cuatro componentes Cantilever es **funcional** —
geometría, recetas, identidad de inserción— y no toca ninguna de esas dimensiones.

**La caracterización previa es inmutable.** Si una migración cambia un comportamiento caracterizado, la
prueba original **no se reescribe**: se conserva como evidencia versionada y el contrato nuevo vive en
una clase separada, de modo que se lea entera la transición *base → ADR → contrato*.

### 3.2 Retirada de la fachada

Los cuatro XAML de componente pasan a nombrar `shell:RackBoundedEditorShell`; el archivo
`CantileverComponentEditorShell.cs` se **elimina**. Las dos guardas que hoy lo vigilan —la de las siete
ranuras de I-37D y la de que la fachada no re-declara nada— se **reapuntan al tipo real**: la primera
sigue vigilando las siete ranuras, la segunda pasa a vigilar que **ningún** XAML vuelva a nombrar un
shell con nombre de sistema.

### 3.3 Recursos del shell acotado

Mismo patrón que I-39B: respaldo **solo cuando el token no resuelve**, en `OnApplyTemplate`, en vez de
merge incondicional en el constructor. Simetría exacta entre los dos shells, con su regresión en rojo.

### 3.4 Contrato de tamaño del arquetipo B

Un estilo de ventana propio del arquetipo B, hermano de `EditorShellWindowStyle`, alimentado por los
tokens `BoundedEditor*` que I-39A ya dejó escritos. Las cuatro ventanas Cantilever lo aplican y
**retiran sus literales**; el piloto lee los tokens en vez de repetir sus cuatro números.

El tamaño observable de las cuatro Cantilever **cambia**, y ése es el punto: hoy abren en un tamaño que
no es el que declaran. La accesibilidad real de acciones y diagnóstico en el mínimo del arquetipo se
**mide** con pruebas de layout y se **valida** en AutoCAD.

### 3.5 Evolución y adopción de `EditorAction`

`EditorAction` gana la capacidad de declarar **acción por defecto** y **acción de cancelación**;
`EditorActions.Button` las honra. Es evolución **neutral**: sin conocimiento de sistema y con pruebas.

La adopción se hace donde **no** rompe nada: el piloto construye sus botones en código y ya declara
`IsDefault`/`IsCancel` a mano, así que es el consumidor natural. **No** se convierten a código los
botones declarados en XAML: llevan `x:Name` del que dependen su propio code-behind y las pruebas, y
sustituirlos sería una reescritura sin ganancia para el usuario. La desviación, si se mantiene, se
registra medida.

### 3.6 Larguero al shell

Migración a `RackBoundedEditorShell` conservando **instancias, manejadores y orden**: el formulario a
`Parameters`, el lienzo a `Preview`, el estado a `Diagnostics`, los tres botones a `Actions`. Se
preservan `sourceProject`, `LoadExisting`, el catálogo, el guardado en biblioteca con los metadatos
desconocidos de I-11 y el BOM modal.

### 3.7 Preview y acciones de las seis

Autoridad y frescura declaradas por ventana (D4) y motivo visible al bloquearse (D6), sobre las
autoridades que cada ventana **ya** tiene. **No** se inventa un modelo de frescura donde el producto no
lo exhibe: ADR-0029 D4 dice expresamente que una ventana no está obligada a implementar estados que hoy
no muestra.

## 4. Fuera de alcance

Cada uno es **condición de detención**:

- Arquetipos **A**, **C** y **D**. Los seis editores ricos —cerrados por I-39B—, los diez diálogos y las
  seis utilitarias **no se tocan**. `RackDialogWindow` es de I-39D.
- Geometría, resolvers, BOM, persistencia, wire format, GUID, catálogos, Domain, Application y Plugin.
- Reabrir cualquier regla de I-36 o I-37: las cuatro ventanas Cantilever cambian de **contenedor y de
  tamaño**, no de contenido ni de reglas.
- Paquetes NuGet nuevos (**ADR-0012**).
- `docs/HANDOFF.md` y `docs/ROADMAP.md` más allá de la fila autorizada: sesión de integración.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/ROADMAP.md` Fase 3; **ADR-0029** y **ADR-0019**;
`docs/automation/decisions/I-39.md`; el contrato, el estado y las evidencias de **I-39A** y **I-39B**;
los contratos de I-14, I-15, I-24, I-30, I-31 e I-37D. Context Packs: `ui-editors`,
`architecture-kernel`, `delivery-validation`, `documentation-governance`.

## 6. Dependencias

**I-39A e I-39B integradas.** ADR-0029 **aceptado**. Conflicto que debe permanecer inactivo: I-39D.
Cualquier iniciativa que toque `src/RackCad.UI/Shell/**`, `Themes/**` o los componentes Cantilever se
serializa con ésta.

## 7. Archivos esperados

**Crear**: este contrato; `docs/automation/state/I-39C.yml`;
`tests/RackCad.UI.Tests/BoundedEditorCharacterizationTests.cs`;
`docs/automation/evidence/I-39C-*.md`.

**Modificar**: los cuatro `.xaml` de componente Cantilever;
`src/RackCad.UI/Systems/Larguero/RackLargueroWindow.xaml{,.cs}`;
`src/RackCad.UI/Shell/{RackBoundedEditorShell,EditorAction}.cs`;
`src/RackCad.UI/StructuralSections/StructuralSectionInspectorWindow.cs`;
`src/RackCad.UI/Themes/{AppStyles,Generic}.xaml`;
`tests/RackCad.UI.Tests/BoundedEditorShellTests.cs`;
`tests/RackCad.Tests/CantileverRoundTwoSourceGuardTests.cs`; `docs/ROADMAP.md` (solo la fila);
`docs/initiatives/README.md`.

**Eliminar**: `src/RackCad.UI/Systems/Cantilever/Components/CantileverComponentEditorShell.cs`.

**Hotspots que NO deben aparecer en el diff**: los `.xaml`/`.xaml.cs` de las seis ventanas del arquetipo
A, los diez diálogos, las seis utilitarias, `src/RackCad.Plugin/**`, `src/RackCad.Application/**`,
`src/RackCad.Domain/**`, `assets/`, `deploy/`, `.github/`, `docs/HANDOFF.md`.

## 8. Fases

1. Reclamo y registro.
2. **Caracterización** de las seis del arquetipo B, verde sobre el árbol sin tocar.
3. Retirada de la fachada y reapuntado de guardas.
4. Recursos del shell acotado, con su regresión en rojo.
5. Contrato de tamaño del arquetipo B.
6. `EditorAction` y su adopción real.
7. Larguero al shell.
8. Cierre: suites, builds, CI, estado, evidencias y checklist de validación manual.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Rebuild **completo** (`--no-incremental`) donde se verifique una guarda en rojo: el build incremental no
recoge un cambio en el constructor de un control y produce falso verde en ambos sentidos (lección de
I-39A, confirmada en I-39B).

Regresión obligatoria sin cambios: goldens, persistencia, handlers, round-trip, validador de catálogos,
las cinco familias de sistema, y las líneas I-36, I-37, I-39A e I-39B.

## 10. Validación manual

**OBLIGATORIA.** `requires_autocad: true`, `requires_owner_validation: true`. I-39C cambia el **tamaño
observable** de cuatro ventanas de producción y **migra** una quinta a otro contenedor visual, así que
el gate es el veredicto del Owner en **AutoCAD 2025** sobre el DLL Debug del worktree, construido desde
el SHA candidato. El ROADMAP la marca con la mano.

Checklist acotado a lo observable: las cuatro ventanas de componente Cantilever abren con el tamaño del
arquetipo B, se redimensionan hasta su mínimo sin perder acciones ni diagnóstico y conservan preview,
receta, aceptación, cancelación e inserción; el **Larguero** conserva formulario, preview, estado, BOM
modal y guardado en biblioteca, incluido reabrir uno guardado; el inspector de secciones sigue igual; y
regresión de que la línea Cantilever completa sigue dibujando lo mismo.

## 11. Criterios de aceptación

1. `CantileverComponentEditorShell` no existe y ningún XAML lo nombra; las guardas quedan reapuntadas.
2. El shell acotado respalda sus tokens sin sombrear, con regresión en rojo.
3. Ninguna ventana del arquetipo B declara literales de tamaño ni aplica `EditorShellWindowStyle`.
4. `EditorAction` describe acción por defecto y cancelación, y tiene consumidor productivo.
5. Larguero migrada, con su caracterización previa verde y sus cinco capacidades intactas.
6. Las seis del arquetipo B tienen caracterización de contrato.
7. Cero geometría, BOM, persistencia, identidad, wire format y catálogos en el diff.

## 12. Condiciones para detenerse

- Retirar la fachada obliga a cambiar comportamiento y no solo el tipo nombrado.
- El contrato de tamaño del arquetipo B recorta acciones o diagnóstico en alguna de las cuatro.
- Migrar el Larguero exige reestructurar lógica de producto, persistencia o catálogo.
- Adoptar `EditorAction` obliga a romper el contrato de teclado o a perder un `x:Name` del que dependa
  código o pruebas.
- `origin/main` avanza sobre los hotspots declarados.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-39C.yml`, `schema: rackcad-automation-state/v1`. El agente nunca hace merge
automático; la integración es una sesión separada y autorizada.

## 14. Evidencia final

**I-39C queda INTEGRADA en `main` el 2026-08-07** por `git merge --no-ff`, sin squash, después de que el
Owner aprobara los **37 puntos** del checklist (`OWNER_APPROVED_I39C_MANUAL_VALIDATION`) sobre el
candidato `2401698a5801a01f3497b3bb27027f801b91960e`, con CI verde 4/4 (run `31223039489`).

- **Base vs contrato**: [`I-39C-caracterizacion-base-vs-contrato.md`](../automation/evidence/I-39C-caracterizacion-base-vs-contrato.md)
  — las **diez** caracterizaciones que cambiaron, conservadas con `Skip` como evidencia versionada.
- **Decisiones técnicas**: [`I-39C-decisiones-tecnicas.md`](../automation/evidence/I-39C-decisiones-tecnicas.md)
- **Validación manual**: [`I-39C-checklist-validacion-manual.md`](../automation/evidence/I-39C-checklist-validacion-manual.md)
- **Estado versionado**: [`I-39C.yml`](../automation/state/I-39C.yml)

`origin/main` **no avanzó** desde la base `da3cd4a`: sin rebase, el árbol validado es el integrado.
`main` no fue modificada en ninguna sesión anterior a la de integración.

**Alcance interno CERRADO, sin nada diferido.** Las cuatro deudas que I-39A e I-39B asignaron a esta
subiniciativa tienen conclusión, y las cinco dimensiones del contrato —acciones con motivo, entrada
inválida, foco inicial, preview y cierre— quedan medidas y probadas.

**Desviación explícita vigente y medida**: `EditorActionBar` **no** se adopta en el arquetipo B. Sus dos
aportaciones —las cuatro categorías neutrales y el envoltorio que no recorta— ya las resuelve el
`DockPanel` que las cuatro ventanas Cantilever tienen, y la prueba de mínimo demuestra que no recorta.
Su papel en los arquetipos **C** y **D** lo decide I-39D.

**Fuera por exclusión previa**: los arquetipos **C** y **D** completos, incluido el papel final de
`RackDialogWindow`, que sigue sin una sola subclase productiva. Son I-39D, con la que **se cerrará I-39**.
