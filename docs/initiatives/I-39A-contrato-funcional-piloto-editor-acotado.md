---
schema: rackcad-initiative/v1
id: I-39A
title: Contrato funcional de ventanas WPF - fundacion y piloto de editor acotado
type: architecture
status: implementing
branch: architecture/contrato-funcional-ventanas-wpf
base_branch: main
priority:
size: M
depends_on: [I-14, I-15, I-24, I-30, I-31, I-37D]
conflicts_with: [I-39B, I-39C, I-39D]
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-39A.yml
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

# I-39A — Fundación del contrato funcional y piloto de editor acotado

> Alcance autorizado por `docs/ROADMAP.md` (Fase 3, filas I-39 e I-39A) y por las decisiones
> vinculantes del Owner en [`decisions/I-39.md`](../automation/decisions/I-39.md). Primera
> subiniciativa de la línea I-39. **Este contrato NO amplía el ROADMAP y NO reabre ADR-0019.**

## 1. Objetivo

Demostrar sobre **una** ventana real y de bajo riesgo que el contrato funcional de ADR-0029 es
adoptable sin cambiar producto, y dejar el arquetipo B con un hogar neutral que un segundo consumidor
no-Cantilever ya usa.

Verificable al cerrar:

1. `RackCad.UI.Shell.RackBoundedEditorShell` existe con las siete zonas del arquetipo B y **ninguna**
   referencia a Cantilever, a ningún sistema, a `RackSystemKind`, a AutoCAD ni a persistencia.
2. `StructuralSectionInspectorWindow` se compone sobre él y **su comportamiento observable es el
   mismo que en la base**, fijado por caracterización escrita y verde **antes** de migrar.
3. `CantileverComponentEditorShell` sigue existiendo en su ruta y namespace, y los cuatro XAML de
   componente Cantilever tienen **diff vacío**.
4. Existe cobertura de Enter, Escape, foco inicial, orden de tabulación y caminos de cierre para el
   piloto, donde hoy no existe ninguna.

## 2. Problema

El repositorio tiene **29 clases concretas derivadas de `System.Windows.Window`** en `src/`: 28
productivas y `RackDialogWindow`, que es infraestructura. Ningún censo por nombre de archivo las
encuentra —`SafetyPerPostWindow` vive dentro de `SelectiveSafetyWindow.cs`— y ningún censo por
`x:Name` distingue el **tipo** `RackCad.UI.Controls.PreviewCanvas`, que tiene **un** consumidor
productivo, de las diez ventanas que dibujan sobre un `Canvas` ordinario llamado igual.

ADR-0019 resolvió la composición visual de los editores ricos. No existe contrato de comportamiento:
`IsDefault` está presente en unas ventanas, ausente en otras y **prohibido por comentario** en el
Selectivo con su causa escrita; `RackPushBackSystemWindow` es la única de las nueve ventanas de rack
sin `IsCancel`; `EditorAction`, `EditorActionBar`, `EditorStatusPresenter` y sus cuatro severidades
tienen **cero** consumidores productivos; `RackDialogWindow` no tiene ninguna subclase productiva
mientras diez diálogos reconstruyen a mano su par Aceptar/Cancelar; y `tests/RackCad.UI.Tests` no
ejercita Enter, Escape, foco ni cierre en ninguna ventana.

Lo específico de esta subiniciativa: el shell de los cuatro editores de componente Cantilever es
**neutral como tipo** —siete `DependencyProperty` de tipo `object`, cero ramas, sus únicos `using`
son `System.Windows` y `System.Windows.Controls`— pero vive en
`RackCad.UI.Systems.Cantilever.Components`, `Themes/Generic.xaml` declara un `xmlns` hacia ese
namespace de sistema, y cinco pruebas de I-37D enumeran esa carpeta por ruta literal. La neutralidad
está **probada en el código y desmentida por la ubicación**.

## 3. Alcance

### 3.1 Censo por tipo y taxonomía

Censo de las clases concretas derivadas de `System.Windows.Window` en `src/`, por **tipo**, con su
arquetipo A/B/C/D y sus casos discutibles, publicado como evidencia. Una guarda nueva lo mantiene
vivo: detecta ventanas code-only y varias clases `Window` declaradas en un mismo archivo, y falla si
aparece una ventana que el censo no registra.

### 3.2 Shell neutral del arquetipo B

`src/RackCad.UI/Shell/RackBoundedEditorShell.cs`, control lookless con las siete zonas `Header`,
`Parameters`, `SectionPicker`, `Preview`, `Diagnostics`, `BomSummary` y `Actions`, todas `object`.
Su `Style` y su `ControlTemplate` pasan a `Themes/Generic.xaml` bajo el prefijo `shell:` ya
declarado, con el cuerpo de la plantilla **sin cambios funcionales**.

### 3.3 Fachada Cantilever y guardas de I-37D

`CantileverComponentEditorShell` se reduce a `: RackBoundedEditorShell` **sin miembros propios**: al
no sobrescribir `DefaultStyleKeyProperty`, hereda la clave de estilo del tipo base y con ella la
plantilla, de modo que **no necesita `Style` propio ni re-declarar ninguna `DependencyProperty`**. Los
cuatro XAML de componente siguen nombrando `components:CantileverComponentEditorShell` y sus siete
property-elements, y quedan **con diff vacío**.

En consecuencia `Themes/Generic.xaml` **deja de declarar el `xmlns` hacia el namespace de Cantilever**
y el archivo queda libre de nombres de sistema.

`CantileverRoundTwoSourceGuardTests.ElShellDeComponenteTieneSusSieteRanuras` se **reapunta** al
archivo que hoy declara las siete ranuras, conservando íntegra su aserción. No se escribe código para
satisfacerla: la fachada no re-declara nada. Las cuatro reglas restantes que barren la carpeta de
componentes siguen pasando sin tocarse.

### 3.4 Piloto: `StructuralSectionInspectorWindow`

Se compone sobre el shell neutral conservando su comportamiento observable. El reparto de zonas es:

| Zona | Contenido del piloto |
|---|---|
| `Header` | vacía |
| `Parameters` | el panel de captura completo, **en su orden actual** |
| `SectionPicker` | vacía — el buscador y la lista del inspector viven en su panel de captura y **no se separan**, porque moverlos a esta zona invertiría el orden visual |
| `Preview` | `StructuralSectionPreview` |
| `Diagnostics` | resumen, fidelidad, advertencia de autoridad y diagnósticos |
| `BomSummary` | vacía — el inspector no tiene BOM (decisión 23 de I-36D) |
| `Actions` | «Insertar» y «Cerrar» |

### 3.5 Contrato de tamaño del arquetipo B

Tokens propios en `Themes/AppStyles.xaml` y un `Style` de ventana propio del arquetipo, con **los
valores vigentes del piloto**, de modo que su tamaño observable no cambie. Los valores definitivos
para las cuatro ventanas Cantilever son decisión del Owner y se cierran en I-39C.

### 3.6 Adopción parcial de la infraestructura existente

Se adopta lo que no cambia comportamiento. **No** se adopta `EditorActions.Button` para las dos
acciones del piloto: esa fábrica no fija `IsDefault` ni `IsCancel`, y sustituir los botones actuales
**rompería Enter y Escape**, que la caracterización fija. Queda registrado como deuda de I-39C, junto
con la evolución de `EditorAction` para describir la acción por defecto y la de cancelación.

## 4. Fuera de alcance

Cada uno es **condición de detención**:

- **Larguero**: `RackLargueroWindow` no se toca — es I-39C.
- **Los seis editores ricos**: no se tocan — es I-39B. `RackSelectiveWindow.xaml.cs` y
  `RackFrameConfiguratorViewModel.cs` son archivos calientes de `WORKFLOW.md` §7 y **no deben
  aparecer en el diff**.
- **El defecto de Push Back** (`CloseButton` sin `IsCancel`): se registra, **no se corrige** — es
  I-39B, después de que la política de Escape con cambios pendientes esté fijada.
- **Los diez diálogos y las seis utilitarias**: no se tocan — es I-39D.
- **Migración física de los cuatro XAML de componente Cantilever** y **retirada de la fachada**: I-39C.
- **`RackDialogWindow`** como migración general: I-39D.
- **El buscador y la lista propios del inspector no se sustituyen** por `StructuralSectionPicker`.
- **No se amplía la responsabilidad de producto del inspector**: ni familias, ni geometría, ni
  representación, ni persistencia, ni BOM, ni edición de miembros, ni identidad de rack.
- **No se unifican** los mecanismos de recomputación ni se corrigen las fugas de `using` de
  `RecomputeGate`, `RecomputeDebouncer` y `DispatcherRecomputeScheduler`: quedan registradas.
- Geometría, resolvers, BOM, persistencia, JSON, DWG, GUID, catálogos, reglas de producto, AutoCAD y
  Plugin: **prohibido**.
- Paquetes NuGet nuevos, incluido cualquier framework de pruebas de UI: prohibidos por **ADR-0012**.
- No se fija todavía una resolución ni un DPI mínimos como decisión arquitectónica.
- `docs/HANDOFF.md` y `docs/ROADMAP.md` (más allá de las dos filas autorizadas): los actualiza la
  sesión de integración.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md` §§1, 2, 4, 6, 7, 8; `docs/AUTOMATION_PLAN.md` §8;
`docs/ROADMAP.md` Fase 3; `docs/ARCHITECTURE.md` §7.3-7.4; `docs/adr/README.md`; **ADR-0019**,
**ADR-0012**, **ADR-0003**, **ADR-0022** §7 y **ADR-0023**; los contratos de I-14, I-15, I-24, I-30,
I-31 e I-37D; `docs/automation/decisions/I-36D.md` (decisiones 22 y 23), `decisions/I-37.md` y
`decisions/I-39.md`; `docs/guias/validacion-manual-autocad.md`. Context Packs: `ui-editors`,
`architecture-kernel`, `delivery-validation`, `documentation-governance`.

## 6. Dependencias

Integradas: I-14, I-15, I-24, I-30, I-31, I-37D. Decisiones vigentes que no se reabren: **ADR-0019**,
**ADR-0012**, **ADR-0003**. Las decisiones del Owner que autorizan este alcance están versionadas en
`docs/automation/decisions/I-39.md`. **ADR-0029 nace `propuesto`**; su aceptación es del Owner y se
resuelve junto al gate de validación manual, con el precedente de ADR-0023 en I-36D.

Conflictos que deben permanecer inactivos: I-39B, I-39C, I-39D, y cualquier iniciativa que toque
`Themes/Generic.xaml`, `Themes/AppStyles.xaml` o `src/RackCad.UI/Shell/**`.

## 7. Archivos esperados

**Crear**: `src/RackCad.UI/Shell/RackBoundedEditorShell.cs`;
`tests/RackCad.UI.Tests/BoundedEditorShellTests.cs`;
`tests/RackCad.UI.Tests/StructuralSectionInspectorWindowTests.cs`;
`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`; este contrato;
`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`;
`docs/automation/decisions/I-39.md`; `docs/automation/state/I-39A.yml`;
`docs/automation/evidence/I-39A-censo-ventanas.md`.

**Modificar**: `src/RackCad.UI/Systems/Cantilever/Components/CantileverComponentEditorShell.cs`
(pasa a fachada); `src/RackCad.UI/Themes/Generic.xaml`; `src/RackCad.UI/Themes/AppStyles.xaml`;
`src/RackCad.UI/StructuralSections/StructuralSectionInspectorWindow.cs`;
`tests/RackCad.Tests/CantileverRoundTwoSourceGuardTests.cs` (guarda reapuntada);
`tests/RackCad.UI.Tests/EditorVisualShellTests.cs`; `tests/RackCad.UI.Tests/EditorWindowTestSupport.cs`
(aditivo); `docs/ROADMAP.md` (solo las dos filas); `docs/initiatives/README.md`; `docs/adr/README.md`.

**Hotspots que NO deben aparecer en el diff de I-39A**: `RackSelectiveWindow.*`,
`RackDynamicSystemWindow.*`, `RackPushBackSystemWindow.*`, `RackCantileverWindow.*`,
`RackFlowBedWindow.*`, `RackFrameConfiguratorWindow.*` y su ViewModel, `RackLargueroWindow.*`, los
cuatro `Cantilever*Window.xaml{,.cs}`, los diez diálogos, las seis utilitarias,
`Controls/RackDialogWindow.cs`, `Controls/PreviewCanvas.cs`, `StructuralSectionPreview.cs`,
`UiSupport.cs`, `Editor/Recompute*.cs`, `src/RackCad.Plugin/**`, `src/RackCad.Application/**`,
`src/RackCad.Domain/**`, `assets/`, `deploy/`, `.github/`, `RackCad.sln`, `Directory.Build.*`,
`docs/HANDOFF.md`. Una desviación material exige detenerse.

## 8. Fases

1. **Registro y reclamo**: filas del ROADMAP, contrato, decisiones, ADR-0029 propuesto, índices;
   rama, worktree, commit de reclamo y primer push sin force.
2. **Censo y taxonomía**, con su evidencia y su guarda verificada en rojo.
3. **Shell neutral, fachada y guardas reapuntadas.** Suites verdes; los cuatro XAML con diff vacío.
4. **Caracterización del piloto**, escrita y verde **sobre el piloto todavía sin migrar**.
5. **Migración del piloto.** La caracterización de la fase 4 sigue verde **sin editarse**.
6. **Cierre**: builds Debug de UI y Plugin, CI, estado versionado, evidencia y DLL para el Owner.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Filtros dirigidos, cada uno debe descubrir al menos una prueba: censo de ventanas, shell acotado,
inspector estructural, shell visual de I-30, guardas de I-37D, secciones estructurales, goldens,
persistencia, handlers. **Ningún filtro con cero pruebas.**

### 9.1 Caracterización del piloto — escrita ANTES de migrar, no editada después

Sobre la ventana real, con el runner STA del repositorio: familias ofrecidas y su orden; búsqueda por
subcadena normalizada sobre etiqueta de manual y designación EDI; texto en blanco y texto que **no
normaliza** no filtran; filtro sin coincidencias deja sin selección y sin plan; longitud por defecto
sembrada en cultura actual; longitud inválida rechazada conservando la anterior y pintando aviso;
rotación inválida que pinta aviso y **no** repinta; vista, detalle, representación, rotación, espejo,
eje y envolvente llegan al plan; preview vacío sin selección; `Result` y `AcceptedSection` tras
aceptar; cancelar deja `Result` nulo; cierre por la X deja `Result` nulo; **«Insertar» siempre
habilitado y, sin selección, no-op silencioso**; **longitud o rotación inválidas no bloquean la
inserción**; tamaño y mínimos de ventana; `CenterScreen`; «Insertar» es la acción por defecto y
«Cerrar» la de cancelación; ningún control declara `TabIndex` explícito; ningún elemento declara foco
inicial.

Las tres últimas fijan el estado **actual** de Enter, Escape, foco y tabulación. Donde ADR-0029
describa un contrato distinto y no exista autorización para cambiarlo en I-39A, se conserva el actual
y la deuda se registra para la subiniciativa del arquetipo.

### 9.2 Shell acotado

Las siete zonas existen y aceptan cualquier contenido; las opcionales colapsan sin dejar hueco;
`Actions` está siempre presente; el archivo no contiene `Cantilever`, `RackSystemKind`, ningún
namespace de sistema, `Autodesk`, ni persistencia; se consume desde XAML —vía la fachada— y desde una
ventana construida en código; la fachada conserva su API observable y su plantilla.

### 9.3 Regresión obligatoria, sin cambios

Goldens, persistencia, handlers, round-trip, validador de catálogos, las cinco familias de sistema,
la línea I-36 y la línea I-37, incluidas las cinco reglas que barren la carpeta de componentes.

## 10. Validación manual

**OBLIGATORIA.** `requires_autocad: true`, `requires_owner_validation: true`. El piloto es una
ventana que el usuario abre desde `RACKSECCION` y desde el menú principal. El gate es el veredicto del
Owner en **AutoCAD 2025** sobre el **DLL Debug del worktree** construido desde el SHA candidato,
cargado por `NETLOAD`. Sin ese veredicto no se integra, y con él se resuelve también la aceptación de
ADR-0029.

Checklist: `RACKSECCION` y el botón del menú abren el inspector; familias, búsqueda y lista se
comportan igual; longitud, vista, detalle, representación, rotación, espejo, eje y envolvente
producen el mismo dibujo; la advertencia de geometría visual derivada sigue visible con el mismo
texto; el preview se comporta igual al cambiar entradas y al redimensionar; Insertar materializa la
misma sección con el mismo mensaje; Cerrar y la X no materializan nada; Enter inserta y Escape
cierra, como antes; **acciones y diagnósticos permanecen visibles y sin recorte** al tamaño mínimo y
al redimensionar; los cuatro editores de componente Cantilever abren y funcionan igual; el aviso de
unidades sigue apareciendo una sola vez y después del inspector; los cinco sistemas vigentes sin
regresión.

## 11. Criterios de aceptación

1. `RackBoundedEditorShell` existe en `RackCad.UI.Shell` con las siete zonas y cero acoplamiento por
   sistema, verificado por guarda.
2. El piloto se compone sobre él y la caracterización de la fase 4 pasa **sin haber sido editada**.
3. Los cuatro XAML de componente Cantilever tienen **diff vacío**.
4. `Themes/Generic.xaml` no contiene ningún nombre de sistema.
5. La guarda de las siete ranuras quedó **reapuntada, no debilitada**, y las otras cuatro reglas de
   la carpeta de componentes pasan sin tocarse.
6. Existe cobertura de Enter, Escape, foco inicial, tabulación y caminos de cierre para el piloto.
7. Cero geometría, BOM, persistencia, catálogos, identidad y wire format en el diff.
8. Cero paquetes NuGet nuevos.
9. Diff confinado a `src/RackCad.UI/{Shell,Themes,StructuralSections}`, la fachada de componentes,
   `tests/**` y la documentación de I-39A.

## 12. Condiciones para detenerse

- ADR-0029 rechazado, o aceptado con modificaciones que cambien la taxonomía o el arquetipo B.
- La migración exigiría tocar geometría, BOM, persistencia, catálogos, handlers o Plugin.
- El piloto no puede componerse sin cambiar su comportamiento observable.
- Una guarda de I-37D exigiría **debilitarse** en lugar de reapuntarse.
- La fachada obligaría a modificar alguno de los cuatro XAML de componente.
- Se necesitaría un paquete NuGet o un framework de pruebas de UI.
- Aparecería la necesidad de un segundo modelo de acciones, severidades o estado.
- `origin/main` avanza con cambios en los hotspots declarados: rebase y reevaluación.
- El alcance crece más allá de la fundación y el piloto.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-39A.yml`, con `schema: rackcad-automation-state/v1`. `initiative`, `branch` y
`claim_id` se copian del reclamo y no cambian. El merge automático está prohibido; el agente nunca
hace merge. No se abre un segundo Pull Request para la iniciativa.

## 14. Evidencia final

Commits, censo publicado, resultados de las suites y de los builds Debug, CI, ruta y huella del DLL
entregado al Owner, veredicto de la validación manual, intentos y confirmación de que `main` no fue
modificada.
