---
schema: rackcad-initiative/v1
id: I-39D
title: Dialogos del arquetipo C, utilitarias del D y papel final de RackDialogWindow
type: architecture
status: implementing
branch: architecture/dialogos-y-utilitarias
base_branch: main
priority:
size: L
depends_on: [I-39A, I-39B, I-39C]
conflicts_with: []
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-39D.yml
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

# I-39D — Diálogos del arquetipo C, utilitarias del D y papel final de `RackDialogWindow`

> Alcance autorizado por `docs/ROADMAP.md` (Fase 3, fila I-39D) y por **ADR-0029**, aceptado. **Este
> contrato NO amplía el ROADMAP y NO reabre ADR-0019 ni ADR-0029.** La
> [auditoría de apertura](../automation/evidence/I-39D-auditoria-dialogos-y-utilitarias.md) ajusta
> **cómo** se implementa, no **qué** debe entregarse. **Con esta subiniciativa se cierra I-39.**

## 1. Objetivo

Que los **diez** diálogos del arquetipo C y las **seis** utilitarias del D queden bajo el contrato
funcional común, sobre una red de caracterización que hoy **no existe**, y que `RackDialogWindow`
deje de ser una infraestructura sin adoptantes.

Verificable al cerrar:

1. Existen pruebas que fijan, para las **dieciséis**, la declaración de acción por defecto y de
   cancelación, los caminos de cierre, el dirty declarado, las acciones y sus motivos, el foco
   inicial, la tabulación emergente, el tamaño real de apertura y el ownership.
2. **`RackDialogWindow` tiene papel final resuelto**, con evidencia y no por analogía. Sus dos
   mitades van a donde sirven: el **chrome** se generaliza a un contrato de ventana que se adopta por
   composición, y la **barra de acciones** se retira por ser un **modelo paralelo** de
   `EditorActions.Button`, que la decisión 28 del Owner prohíbe.
3. El chrome del arquetipo C vive en **una sola fuente** y no en diez constructores.
4. El contrato de tamaño de C y D se declara **donde la evidencia ya converge**, y **con su excepción
   escrita** donde no: cuatro ventanas calculan su tamaño de los datos y tres usan `SizeToContent`.
5. `EditorActions.Button` tiene consumidores productivos en el arquetipo C, **sin** perder etiqueta,
   métrica, teclado ni resultado.
6. Ninguna acción queda habilitada sin efecto ni bloqueada sin motivo visible.
7. Cero cambios de geometría, BOM, persistencia, wire format, GUID, catálogos y reglas de producto.

## 2. Problema

La auditoría midió las dieciséis y encontró un arquetipo **coherente por repetición, no por
contrato**: los diez diálogos de C son code-only, ninguno hereda de `RackDialogWindow`, los diez
repiten a mano el mismo bloque de chrome, los diez reconstruyen su propia barra Aceptar/Cancelar —en
**tres formas distintas**—, ninguno declara foco inicial ni `TabIndex`, y **ninguno** tiene
`OnClosing`.

Lo que hace que esto **no** se pueda unificar a la ligera son las excepciones, todas medidas:

- **`SafetyDefensaGridWindow` es la única con el bloque de chrome incompleto**: no asigna
  `FontFamily` ni `Background`. Cualquier fuente común **le cambia el aspecto**: es una corrección,
  no un refactor.
- **La etiqueta primaria no es «Aceptar» en dos**: «Colocar» y «Calcular».
- **Concordancia de género cruzada** en los pares Todos/Ninguno frente a Todas/Ninguna: es regla de
  producto, no ruido.
- **`SelectiveSegmentsWindow` tiene tres terminaciones**, no dos: «Sin medio frente» cierra con éxito
  y resultado vacío.
- **Cinco ventanas declaran `CenterOwner` sin que pueda existir `Owner`**, porque su único llamador es
  un comando del Plugin sin ventana padre WPF.
- **Nueve de las dieciséis no se construyen jamás en una prueba**, y **toda** la cobertura existente
  es funcional: ni una aserción de teclado, cierre, foco, tabulación, tamaño u ownership.

Y `RackDialogWindow` sigue con **cero subclases productivas** desde I-14, por razones estructurales
que la auditoría midió: no entrega tamaño, no coloca la barra que fabrica, no tiene `OnClosing`, su
`CreateActionBar` no admite la invocación natural de las cuatro rejillas, y asigna `Background` y
`FontFamily` como **valor local**, lo que en precedencia WPF **impide** que un futuro estilo de
ventana del arquetipo C cambie ninguno de los dos.

## 3. Alcance

### 3.1 Caracterización antes de tocar (ADR-0029 D13)

Pruebas que fijan el comportamiento **actual** de las dieciséis en las dimensiones del contrato.
Prioridad a las **nueve que no se construyen nunca**. Se caracteriza también el **orden y el texto
exacto de las validaciones** de las dos ventanas de almacén, que hoy es su único contrato observable
y no lo fija nada.

**La caracterización previa es inmutable.** Si un cambio la contradice, la prueba original **no se
reescribe**: se conserva intacta con `Skip` como evidencia versionada y el contrato nuevo vive en una
clase separada.

### 3.2 Papel final de `RackDialogWindow`

Se resuelve por **medición**, no por analogía, y la medición descarta un ancestro único para las diez:
encajan cuatro y no encajan seis. Sus dos mitades se separan.

- El **chrome** —`FontFamily`, fondo y el diccionario compartido— se generaliza a un contrato de
  ventana del arquetipo C, hermano de los de A y B, que se adopta **por composición** y en una línea.
  Deja de ser valor local, que es lo que hoy bloquearía cualquier contrato de tamaño.
- La **barra de acciones** se retira: es un **modelo paralelo** de `EditorActions.Button`, y la
  decisión 28 del Owner prohíbe los modelos paralelos. `EditorActions.Button` es el que sobrevive,
  porque desde I-39C transporta el rol de teclado y desde siempre transporta el motivo de bloqueo.
- Con ambas mitades reubicadas, el tipo queda **sin contenido propio** y se retira. El censo baja de
  29 a 28 clases y su guarda se **reapunta, no se debilita**.

### 3.3 Chrome del arquetipo C en una sola fuente

Las **nueve** ventanas que hoy asignan las cuatro propiedades adoptan el contrato de ventana. El diff
visual es **vacío y verificable**, porque el contrato lleva exactamente los valores que ya usan.
`SafetyDefensaGridWindow` queda **fuera** de esta fase: completarle el chrome es cambio observable.

### 3.4 Contrato de tamaño de C y D

Solo donde la evidencia ya converge. En **D**, tres ventanas declaran literalmente el mismo tamaño:
se tokeniza. En **C**, el contrato lleva **solo** apariencia y tipografía, y **no** mínimos: cuatro
ventanas calculan su tamaño de los datos y tres usan `SizeToContent`. Imponer mínimos comunes
reproduciría en C exactamente la anomalía que I-39A midió en Cantilever y que I-39C acaba de cerrar.

### 3.5 Adopción de `EditorActions.Button`

En los diálogos de **dos** botones sin acción deshabilitada ni tercer grupo, conservando
explícitamente etiqueta, métrica, teclado y resultado. **No** se adoptan las cuatro rejillas —sus
Todos/Ninguno cruzarían la ventana—, ni `SelectiveSegmentsWindow` —tercera terminación—, ni los diez
botones-tarjeta del menú, cuyo contenido no es texto.

### 3.6 Lo observable, uno a uno

Cada punto es una decisión independiente y no un lote: chrome de `SafetyDefensaGridWindow`, ubicación
de las cinco ventanas sin padre WPF posible, unificación de la paleta de estado, foco inicial donde
hoy recae sobre una acción de escritura masiva, motivo visible en la barra de selección masiva, y
limpieza del diagnóstico obsoleto.

## 4. Fuera de alcance

Cada uno es **condición de detención**:

- Arquetipos **A** y **B**, cerrados por I-39B e I-39C.
- **Unificar los mapeos `SafetySide`**: dos de ellos no ofrecen «Ninguno» a propósito y colapsan los
  valores intermedios. Es regla de producto.
- `MessageBox` y `SaveFileDialog` sin costura, y la migración de los botones-tarjeta del menú.
- `SelectionMatrix` en `SafetyDefensaGridWindow`: su celda no es booleana.
- Geometría, resolvers, BOM, persistencia, wire format, GUID, catálogos, Domain, Application.
- Paquetes NuGet nuevos (**ADR-0012**).
- `docs/HANDOFF.md` y `docs/ROADMAP.md` más allá de la fila autorizada: sesión de integración.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/AUTOMATION_PLAN.md`; `docs/ROADMAP.md` Fase 3; **ADR-0029** y
**ADR-0019**; `docs/automation/decisions/I-39.md` (decisiones 5-8, 26-28); los contratos, estados y
evidencias de **I-39A**, **I-39B** e **I-39C**. Context Packs: `ui-editors`, `architecture-kernel`,
`delivery-validation`, `documentation-governance`.

## 6. Dependencias

**I-39A, I-39B e I-39C integradas.** ADR-0029 **aceptado**. No hay conflictos activos: es la última
subiniciativa de la línea.

## 7. Archivos esperados

**Crear**: este contrato; `docs/automation/state/I-39D.yml`; la auditoría de apertura;
`tests/RackCad.UI.Tests/DialogWindowCharacterizationTests.cs` y su clase de contrato.

**Modificar**: los diez `.cs` del arquetipo C; los `.xaml`/`.xaml.cs` de las seis del D que lo
requieran; `src/RackCad.UI/Themes/AppStyles.xaml`; `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`;
`docs/automation/evidence/I-39A-censo-ventanas.md` (refresco de punteros); `docs/ROADMAP.md` (solo la
fila); `docs/initiatives/README.md`.

**Eliminar**: `src/RackCad.UI/Controls/RackDialogWindow.cs` y su suite, una vez reubicadas sus dos
mitades.

**Hotspots que NO deben aparecer en el diff**: los `.xaml`/`.xaml.cs` de las seis ventanas del
arquetipo A y las seis del B, `src/RackCad.Plugin/**`, `src/RackCad.Application/**`,
`src/RackCad.Domain/**`, `assets/`, `deploy/`, `.github/`, `docs/HANDOFF.md`.

## 8. Fases

0. Higiene: refresco del censo y retirada de claves de recurso muertas. Cero deltas visuales.
1. **Caracterización** de las dieciséis, verde sobre el árbol sin tocar.
2. Contrato de ventana del arquetipo C y adopción por las nueve completas.
3. Contrato de tamaño donde converge, con su excepción escrita.
4. Papel final de `RackDialogWindow`: chrome generalizado, barra retirada, tipo eliminado.
5. Adopción de `EditorActions.Button` en los diálogos de dos botones.
6. Lo observable, uno a uno.
7. Cierre: suites, builds, CI, estado, evidencias y checklist de validación manual.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Rebuild **completo** (`--no-incremental`) donde se verifique una guarda en rojo. `dotnet test` **no**
acepta ese modificador: se construye aparte y se prueba con `--no-build` (lección de I-39C).

Regresión obligatoria sin cambios: goldens, persistencia, handlers, round-trip, validador de
catálogos, las cinco familias de sistema, y las líneas I-36, I-37 y I-39A/B/C.

## 10. Validación manual

**OBLIGATORIA.** `requires_autocad: true`, `requires_owner_validation: true`. I-39D cambia chrome,
ubicación, paleta de estado y foco en ventanas de producción, así que el gate es el veredicto del
Owner en **AutoCAD 2025** sobre el DLL Debug del worktree.

El checklist se agrupa **por familias**, no ventana por ventana: varias comparten exactamente el mismo
contrato y repetir el punto once veces no añade evidencia.

## 11. Criterios de aceptación

1. Las dieciséis tienen caracterización de contrato, con las nueve sin cobertura previa cubiertas.
2. `RackDialogWindow` tiene papel final resuelto y ejecutado, con el censo y su guarda reapuntados.
3. El chrome del arquetipo C vive en una sola fuente, con diff visual vacío en las nueve.
4. El contrato de tamaño se declara donde converge y su excepción está escrita donde no.
5. `EditorActions.Button` tiene consumidores productivos en C sin pérdida de contrato.
6. Cero geometría, BOM, persistencia, identidad, wire format y catálogos en el diff.
7. Diff confinado a `src/RackCad.UI`, `tests/**` y la documentación de I-39D.

## 12. Condiciones para detenerse

- Retirar `RackDialogWindow` obliga a cambiar comportamiento y no solo su ubicación.
- Adoptar el contrato de ventana cambia el aspecto de alguna de las nueve.
- Una unificación exige tocar una regla de producto —los mapeos `SafetySide`, las etiquetas por
  concordancia de género, la tercera terminación de `SelectiveSegmentsWindow`—.
- `origin/main` avanza sobre los hotspots declarados.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-39D.yml`, `schema: rackcad-automation-state/v1`. El agente nunca hace merge
automático; la integración es una sesión separada y autorizada.
