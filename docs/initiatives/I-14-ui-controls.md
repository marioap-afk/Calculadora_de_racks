---
schema: rackcad-initiative/v1
id: I-14
title: Controles comunes de UI
type: architecture
status: in-progress
branch: architecture/ui-controls
base_branch: main
priority:
size: M
depends_on: [I-02]
conflicts_with: [I-15, I-17]
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
automation_state_path: docs/automation/state/I-14.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-14 — Controles comunes de UI

## 1. Objetivo

Entregar cinco controles WPF reutilizables en `RackCad.UI`, nacidos con pruebas, que concentren los
patrones de UI hoy repetidos entre ventanas:

- `SelectionMatrix`: rejilla de selección/toggle fila × columna construida en código (hoy duplicada
  en el selectivo y en las rejillas de seguridad);
- `NumericField`: campo de entrada numérica localizada (punto o coma, sin agrupadores) con rango y
  estado de error, sobre el `LocalizedNumberParser` puro de Application;
- `CatalogCombo`: combo ligado a `CatalogOption` de catálogo (DisplayName visible / Id en el modelo);
- `RackDialogWindow`: clase base de ventana de diálogo (recursos compartidos, tema, fondo, tipografía,
  ubicación, estado y resultado Aceptar/Cancelar);
- `PreviewCanvas`: lienzo de vista previa con **proyección** (mundo→lienzo) y **paleta** compartidas,
  cerrando el hueco que dejó `PreviewCanvasPainter` (que hoy comparte solo el dibujo, no la proyección
  ni la paleta).

Además, crear el proyecto `tests/RackCad.UI.Tests` (`net8.0-windows`), agregarlo a la solución y
publicar su **gate de CI** dedicado. Los controles y su lógica testeable nacen cubiertos por pruebas.

El resultado es verificable cuando: la solución compila, la suite `RackCad.Tests` sigue verde, el
nuevo proyecto de pruebas de UI compila y pasa, y CI ejecuta el nuevo job de UI tests además de los
tres existentes.

## 2. Problema

La auditoría 2026-07 (recomendación 9, «Editor Shell») y el ROADMAP dividen ese trabajo en tres:
controles (I-14), shell (I-15) y extracción de estado por editor (I-20/I-21). Hoy, antes de I-14:

- las rejillas de selección se construyen a mano en varias ventanas (selectivo y familias de
  seguridad); el ROADMAP proyecta 5-6 rejillas tras I-02;
- el parsing numérico localizado y su validación de rango se repiten como llamadas sueltas a
  `UiSupport.TryNum`/`TryInt`/`TryOptionalNum` seguidas de validación ad hoc por ventana;
- cada ventana puebla sus combos de catálogo con el mismo patrón;
- cada ventana redeclara fondo, tipografía, ubicación, merge de `AppStyles.xaml`, barra de estado y
  botonera Aceptar/Cancelar;
- el preview comparte primitivas de dibujo (`PreviewCanvasPainter`) pero **cada ventana reimplementa
  la proyección mundo→lienzo y su propia paleta**, que es exactamente lo que I-14 debe unificar.

Sin controles comunes, I-15 (shell), I-20/I-21 (editores) e I-22 (seguridad adopta `SelectionMatrix`)
no tienen sobre qué apoyarse y seguirían duplicando UI.

## 3. Alcance

Autorizado por el ROADMAP (Fase 3, I-14) y por el objetivo §7.3 de `ARCHITECTURE.md`:

1. Crear los cinco controles en `src/RackCad.UI` bajo un espacio propio (`Controls/`), separando en
   cada uno una **parte lógica testeable sin árbol visual** (modelo/validación/proyección/paleta) de
   la **vista WPF**.
2. Reutilizar lo que ya existe y es la fuente única: `LocalizedNumberParser` (Application),
   `CatalogOption`, `UiSupport.ToOptions`, `PreviewCanvasPainter`, `ObservableObject` y los estilos de
   `Themes/AppStyles.xaml`. No se duplican reglas ni se reescribe geometría.
3. Crear `tests/RackCad.UI.Tests` (`net8.0-windows`, xUnit + Test SDK, con un runner STA propio y sin
   paquetes nuevos) y agregarlo a `RackCad.sln`.
4. Agregar un job de CI en `.github/workflows/ci.yml` que compile y ejecute el proyecto de pruebas de
   UI en `windows-latest`.
5. Preservar el comportamiento y la apariencia funcional vigentes: los controles nacen **junto** a las
   ventanas actuales (patrón strangler); ninguna ventana existente se migra en esta iniciativa, así que
   no hay cambio observable de producto.

## 4. Fuera de alcance

- I-15 (`RackEditorSession`, `IRackEditorModule`, registro de módulos): shell, no controles.
- Migración de los editores selectivo/dinámico a los controles nuevos (I-20/I-21).
- Adopción de `SelectionMatrix` por las rejillas de seguridad y colocación por familia (I-22).
- Clonación única de `RackFrameConfiguration` (I-17).
- Rediseño visual completo o cambio de la identidad visual (paleta/tipografía) vigente.
- Cualquier cambio de geometría, resolvers, builders, BOM o persistencia.
- Introducir AutoCAD fuera de `RackCad.Plugin` o WPF fuera de `RackCad.UI`.
- Dependencias NuGet de producto (política cero-NuGet, ADR-0003); tampoco paquetes de test nuevos sin
  acuerdo explícito del dueño (AGENTS §Dependencias).

## 5. Contexto requerido

- `AGENTS.md` (convenciones, dirección de dependencias, definición de terminado, dependencias).
- `docs/WORKFLOW.md` (ciclo de iniciativa, archivos calientes, cierre) y `docs/ROADMAP.md` (I-14, sus
  dependencias y estorbos).
- `docs/ARCHITECTURE.md` §5 (UI y adaptación AutoCAD) y §7.3 (Editor Shell objetivo: enumera los cinco
  controles).
- Context Packs: `ui-editors`, `architecture-kernel`, `delivery-validation`.
- Código existente que los controles reutilizan o del que heredan patrón: `src/RackCad.UI/UiSupport.cs`,
  `CatalogOption.cs`, `ObservableObject.cs`, `PreviewCanvasPainter.cs`, `EnumDisplayConverter.cs`,
  `Themes/AppStyles.xaml`, las ventanas de seguridad (`Safety*GridWindow.cs`,
  `SelectiveSegmentsWindow.cs`) y `src/RackCad.Application/Formatting/LocalizedNumberParser.cs`.

## 6. Dependencias

- **Integrada requerida:** I-02 (integrada 2026-07-17) — desbloquea la pista de UI.
- **Estorbos que deben permanecer inactivos:** I-15 e I-17 (tocan la misma capa/áreas de UI). En el
  arranque de esta iniciativa ninguna tenía rama en `origin` (no en curso). I-20/I-21/I-22 dependen de
  I-14 y son posteriores.
- **Entradas del dueño:** ninguna decisión ni validación en AutoCAD requerida (no cambia dibujo).

## 7. Archivos esperados

Crear (producto, `src/RackCad.UI/Controls/`):

- `SelectionMatrix.cs` + `SelectionMatrixModel.cs` (+ celda/elemento de fila si aplica);
- `NumericField.cs` (control) + validación pura (`NumericFieldValidation` o equivalente);
- `CatalogCombo.cs` (control sobre `CatalogOption`);
- `RackDialogWindow.cs` (clase base `Window`);
- `PreviewCanvas.cs` (control) + `PreviewProjection.cs` (proyección pura) + `PreviewPalette.cs` (paleta
  compartida).

Crear (pruebas):

- `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` (`net8.0-windows`, `UseWPF`), un `StaTestRunner`
  propio y los archivos de prueba por control.

Modificar (infraestructura, cambios acotados):

- `RackCad.sln` (agregar el proyecto de pruebas y su carpeta `tests`);
- `.github/workflows/ci.yml` (nuevo job de UI tests en `windows-latest`);
- `docs/initiatives/README.md` (índice del contrato) y `docs/automation/state/I-14.yml` (estado).

No se espera modificar ninguna ventana existente, ni Domain/Application/Plugin, ni catálogos, ni
`deploy/`. Una desviación material de esta lista obliga a detenerse (§12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato
   creado desde `TEMPLATE.md`. (Evidencia: rama remota aceptada, este archivo.)
2. **Andamiaje de pruebas.** Proyecto `RackCad.UI.Tests` + `StaTestRunner` + alta en la solución;
   compila y ejecuta vacío/verde. (Evidencia: build + test del proyecto nuevo.)
3. **Controles con lógica pura.** `NumericField` (+validación), `CatalogCombo`, `SelectionMatrix`
   (+modelo), `PreviewProjection`, `PreviewPalette`, cada uno con sus pruebas de lógica. (Evidencia:
   suite de UI verde.)
4. **Controles WPF y base de ventana.** `SelectionMatrix`, `NumericField`, `CatalogCombo`,
   `PreviewCanvas` (vista) y `RackDialogWindow`, con pruebas de instanciación STA. (Evidencia: suite de
   UI verde; build UI Debug 0 errores.)
5. **Gate de CI.** Job de UI tests en `ci.yml`. (Evidencia: corrida de CI de la rama con el job nuevo
   en verde.)
6. **Cierre de sesión.** Gates completos, revisión de diff, commits lógicos, push de la rama y estado
   versionado actualizado. (Evidencia: §14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` — suite completa verde (sin regresión).
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` — nuevo proyecto verde.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (solo los
  `MSB3277` conocidos de AutoCAD; requiere AutoCAD 2025 para el build completo del Plugin).
- CI: los tres jobs existentes (Tests, Build UI, Build Plugin without AutoCAD) **más** el nuevo job de
  UI tests, todos en verde sobre la punta de la rama.

## 10. Validacion manual

`no aplica` para AutoCAD y para owner-validation. I-14 no wirea los controles a ninguna ventana
existente (patrón strangler): no cambia dibujo, geometría, BOM, persistencia ni la apariencia de las
ventanas actuales, por lo que no hay comportamiento nuevo que validar en AutoCAD (`requires_autocad:
false`; el ROADMAP no marca I-14 con ✋). La compatibilidad de apariencia/comportamiento para cuando
las iniciativas posteriores (I-15/I-20/I-21/I-22) adopten los controles se sostiene con las pruebas de
UI y con la reutilización de `AppStyles.xaml`, `LocalizedNumberParser`, `CatalogOption` y
`PreviewCanvasPainter`. La validación en AutoCAD corresponderá a la iniciativa que adopte cada control.

## 11. Criterios de aceptacion

- Existen los cinco controles en `src/RackCad.UI/Controls/`, con parte lógica testeable separada de la
  vista, sin referencias AutoCAD y sin duplicar reglas de Application.
- `PreviewCanvas` expone proyección (mundo→lienzo) y paleta **compartidas** y reutiliza
  `PreviewCanvasPainter` para el dibujo.
- Existe `tests/RackCad.UI.Tests` (`net8.0-windows`) en la solución, con pruebas por control y un
  runner STA propio (sin paquetes nuevos), verde.
- `.github/workflows/ci.yml` ejecuta un job dedicado de UI tests en `windows-latest`, verde en la rama.
- La suite `RackCad.Tests` permanece verde (sin regresión) y los builds de UI y Plugin siguen en 0
  errores propios.
- Ninguna ventana existente cambió de comportamiento ni de apariencia (no se migró ningún consumidor).
- La dirección de dependencias se conserva (UI no referencia AutoCAD; los controles no dependen del
  Plugin) y no hay dependencias NuGet nuevas.

## 12. Condiciones para detenerse

- Que I-15 o I-17 aparezcan activas en `origin` tocando los mismos archivos de UI de forma incompatible
  (entregar evidencia concreta; no sobrescribir cambios ajenos).
- Que preservar el comportamiento/apariencia exija migrar una ventana existente (eso es I-15/I-20/I-21/
  I-22): detenerse antes de ampliar alcance.
- Que las pruebas de UI exijan un paquete NuGet nuevo (p. ej. un runner STA de terceros): detenerse y
  pedir acuerdo; el plan es un runner STA propio sin dependencias.
- Que el gate de CI requiera cambiar la estrategia de disparo global de `ci.yml` más allá de agregar un
  job, o tocar el job del Plugin (fuera de alcance).
- Cualquier necesidad de cambiar Domain/Application/Plugin, catálogos, geometría, BOM o persistencia.

## 13. Estado versionado y entrega del Pull Request

Estado canónico en `docs/automation/state/I-14.yml`. La automatización está pausada
(`automation.enabled: false`), así que el ejecutor es manual y mantiene ese archivo al cierre de cada
sesión. No se abre un segundo Pull Request para la iniciativa ni se activa auto-merge. La integración a
`main` es una sesión manual posterior (WORKFLOW §4.5) y no forma parte de esta corrida.

## 14. Evidencia final

Se completa al cierre de la sesión: commits lógicos con trailer de procedencia, archivos creados/
modificados, resultados de `dotnet test` (suite completa y UI), builds de UI y Plugin, evidencia del
nuevo job de CI, SHA final de la rama, confirmación del push y confirmación de que `main` no fue
modificada. El detalle vivo del proyecto se actualiza en `docs/HANDOFF.md` §8-12 **solo** en la sesión
de integración (último commit de la rama), nunca desde esta rama en paralelo.
