---
schema: rackcad-initiative/v1
id: I-20
title: Estado del editor Selectivo
type: refactor
status: integrated
branch: refactor/selective-editor-state
base_branch: main
priority:
size: M
depends_on: [I-15]
conflicts_with: [I-22]
context_packs:
  - architecture-kernel
  - ui-editors
  - system-selective
  - persistence
  - delivery-validation
automation_state_path: docs/automation/state/I-20.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-20 — Estado del editor Selectivo

## 1. Objetivo

Extraer el **estado propio** del editor selectivo y sus **operaciones** desde
`RackSelectiveWindow.xaml.cs` (~2,452 líneas, archivo caliente) a clases **puras y testeables** de
`RackCad.Application`, de modo que la ventana quede **observando ese estado y pintando** (matriz y
previews) en lugar de alojar el documento del editor en campos privados de la Window. Concretamente,
según el ROADMAP (Fase 5, I-20) extraer `FondoMatrix`/`Cell`/`ApplyScope`/`BuildDesign` a Application:

- `SelectiveEditorCell` (la celda editable: frente, alto, conteo, larguero, peralte y overrides) y
  `SelectiveEditorFondoMatrix` (una matriz de niveles por fondo: bahías, `larguero a piso`, alturas,
  tramos, fondo y override de cabecera) — hoy tipos anidados privados `Cell`/`FondoMatrix`;
- `SelectiveEditorState`: dueño único de la matriz de trabajo (bahías × niveles), las matrices por
  fondo, la selección (frente/nivel/fondo) y las cabeceras/peraltes por poste, más las operaciones
  puras: `InitMatrix`, snapshot/restore de la matriz de trabajo, `SaveWorkingToSelected`/`LoadFondo`
  (transiciones por fondo), `CloneAligned`, `ResizeBays`, add/remove de nivel, `ClampSelection`,
  `ApplyScope` (celda/nivel/frente/todas), `MaxFrenteCount`, `SyncPostCabeceras` y `BuildDesign`;
- `SelectiveDesignInputs`: el contrato de entradas escalares (poste, peralte, tolerancias, fondos,
  toggles de dibujo, cotas, seguridad) que la ventana lee de sus controles y pasa a
  `SelectiveEditorState.BuildDesign`, manteniendo puro el ensamblado del `SelectivePalletDesign`.

Resultado verificable cuando: la solución compila; `RackCad.Tests` cubre el estado puro (matriz,
cambios de fondo, redimensionado, selección/alcances, construcción equivalente del diseño,
recomputación coalescida y casos límite) y pasa; `RackCad.UI.Tests` cubre el cableado WPF que no puede
cubrirse desde Application (adopción del estado por la ventana real, STA) y pasa; los builds Debug de
UI y Plugin quedan en 0 errores propios; CI queda verde sobre la punta publicada. El comportamiento
observable —UI visible e interacción, geometría resuelta, BOM, GUID, inserción/actualización,
persistencia y metadatos I-11, catálogos, compatibilidad legacy y round-trip— es **idéntico** al
vigente.

## 2. Problema

La auditoría 2026-07 (hallazgos U1 y U3) y el ROADMAP dividen el «Editor Shell» en tres: controles
(I-14, integrada), shell (I-15, integrada) y **extracción del estado por editor** (I-20/I-21). Tras
I-15, la ventana selectiva **adopta el shell** (catálogo, identidad GUID+nombre, recomputación
coalescida y contrato de inserción vía `RackEditorSession`), pero su **estado propio sigue en campos
privados de la Window**:

- **U1 — MVVM nominal.** El documento del editor (la matriz `bays`/`floorBeams`/`bayHeights`/
  `baySegments`, las matrices por fondo `fondoMatrices`, la selección `selBay`/`selLevel`/
  `selectedFondo`, las cabeceras/peraltes por poste) vive en campos privados de `RackSelectiveWindow`,
  entremezclado con el pintado. Las operaciones (`ApplyScope`, `BuildDesign`, `SnapshotWorking`/
  `RestoreWorkingFrom`/`SaveWorkingToSelected`/`LoadFondo`, `ResizeBays`, `CloneAligned`) están
  incrustadas en la ventana.
- **U3 — UI sin tests por construcción.** El proyecto de pruebas puro (`RackCad.Tests`, net8.0) no
  puede referenciar la UI (net8.0-windows), así que toda esa lógica de estado es hoy inalcanzable por
  pruebas unitarias. Extraerla a Application (net8.0) la hace testeable.

I-20 es el paso previo obligado a I-22 (colocación de seguridad por familia), que toca el mismo código
del selectivo: **orden fijo, I-20 primero** (ROADMAP).

## 3. Alcance

Autorizado por el ROADMAP (Fase 5, I-20) y por el objetivo de `ARCHITECTURE.md` (estado del editor en
Application):

1. Crear en `src/RackCad.Application/Systems/` la lógica pura del estado del editor selectivo,
   separada de la vista WPF:
   - `SelectiveEditorCell` (equivalente exacto del `Cell` anidado: mismos defaults, `Clone`,
     `CopyFrom`, `HasOverride`);
   - `SelectiveEditorFondoMatrix` (equivalente del `FondoMatrix` anidado: bahías, `FloorBeams`,
     `BayHeights`, `BaySegments`, `Depth`, `CabeceraOverride`);
   - `SelectiveApplyScope` (enum `Cell`/`Row`/`Column`/`All`);
   - `SelectiveDesignInputs` (las entradas escalares + separadores + toggles + cotas + seguridad ya
     filtrada que la ventana lee de sus controles);
   - `SelectiveEditorState` (dueño de la matriz de trabajo, las matrices por fondo, la selección y las
     cabeceras/peraltes por poste; con las operaciones puras enumeradas en §1). Sin WPF, sin AutoCAD,
     sin dependencia del catálogo (el resolver/builder siguen en la ventana).
2. Adaptar `RackSelectiveWindow.xaml.cs` para que **componga** `SelectiveEditorState`, observe sus
   colecciones (propiedades de acceso sobre el estado) y delegue las operaciones en él; la ventana
   conserva el pintado (`RenderMatrix`/`DrawPreview`/`DrawLateralPreview`), el editor de celda
   (lectura/carga de controles), el cableado de eventos, la recomputación coalescida (I-15, intacta),
   la orquestación de carga (`LoadDesign`/`LoadExisting`/`LoadForNew`) y la lectura de escalares para
   `BuildDesign`. Las propiedades públicas y la superficie que consume el Plugin no cambian.
3. Añadir pruebas de estado puro en `tests/RackCad.Tests` y, solo para el cableado que no se puede
   cubrir desde Application, pruebas STA en `tests/RackCad.UI.Tests`. Verificar explícitamente
   equivalencia de diseños (resuelto idéntico antes/después) y round-trip.

## 4. Fuera de alcance

- **I-21 (estado del editor dinámico)** y **su worktree**: no se toca `RackDynamicSystemWindow` ni su
  estado; las integraciones son serializadas.
- **I-22 (colocación de seguridad por familia)**: no se migran servicios de colocación de seguridad,
  no se crean subtipos de `SelectiveSafetySelection` ni se adopta `SelectionMatrix` en las rejillas.
  La selección de seguridad se transporta tal cual (la ventana filtra y `DeepCopy` como hoy).
- Rediseñar el XAML, los controles o la identidad visual; cambiar etiquetas, tooltips u orden.
- Cambiar geometría, resolvers, builders, BOM o Draw Services (I-16); cambiar formatos persistidos,
  DTO, envelope, Xrecord, el round-trip o los metadatos I-11.
- Corregir hallazgos adyacentes «de paso», **incluida la asimetría vigente de estilos de cota**.
- Dependencias NuGet nuevas (política cero-NuGet, ADR-0003) o paquetes de test nuevos.

## 5. Contexto requerido

- `AGENTS.md` (convenciones, dirección de dependencias Domain←Application←UI←Plugin, definición de
  terminado, regla en un solo sitio, archivos calientes).
- `docs/WORKFLOW.md` (ciclo de iniciativa, archivos calientes §7, cierre §5) y `docs/ROADMAP.md`
  (I-20, dependencia I-15, conflicto I-22 con orden fijo).
- `docs/ARCHITECTURE.md` (capas, estado editable vs geometría resuelta) y
  `docs/auditoria-arquitectura-2026-07.md` (hallazgos U1/U3).
- Context Packs: `architecture-kernel`, `ui-editors`, `system-selective`, `persistence`,
  `delivery-validation`.
- Precedentes: I-14 (controles + proyecto `RackCad.UI.Tests` + runner STA) e I-15 (`RackEditorSession`
  y adopción del shell por la ventana selectiva; deja explícitamente para I-20 la matriz por fondo y
  `BuildSystem`).
- Código: `src/RackCad.UI/RackSelectiveWindow.xaml{,.cs}`, `src/RackCad.UI/Editor/*` (shell I-15,
  intacto), `src/RackCad.Domain/Systems/SelectivePalletDesign.cs`, `SelectiveRackDefaults.cs`,
  `src/RackCad.Application/Systems/Selective*` (resolver/builders/BOM, sin cambio),
  `src/RackCad.Plugin/RackSelectivoCommands.cs` (consumidor, sin cambio).

## 6. Dependencias

- **Integradas requeridas:** I-15 (`architecture/editor-shell`, integrada 2026-07-21 en `main`
  `bfda406`). También I-08/I-14 (transitivas de I-15) están en `main`.
- **Conflictos que deben permanecer inactivos:** I-22 (`refactor/safety-placement`) — no en curso;
  el orden es fijo, I-20 primero. I-21 (`refactor/dynamic-editor-state`) toca otra ventana (dinámico)
  y no comparte archivo caliente con I-20: puede estar en curso en su propio worktree sin
  reconciliación; sus integraciones son serializadas.
- **Entradas del dueño:** ninguna decisión requerida para arrancar. La validación en AutoCAD y la
  owner-validation son responsabilidad del dueño al cierre (gates abiertos, §10).

## 7. Archivos esperados

Crear (producto, `src/RackCad.Application/Systems/`):

- `SelectiveEditorCell.cs`, `SelectiveEditorFondoMatrix.cs`, `SelectiveApplyScope.cs`,
  `SelectiveDesignInputs.cs`, `SelectiveEditorState.cs`.

Crear (pruebas, `tests/RackCad.Tests/`):

- `SelectiveEditorStateTests.cs` (matriz, cambios de fondo, redimensionado, selección/alcances,
  construcción equivalente del diseño, casos límite); complementos de round-trip donde aplique.

Crear/ampliar (pruebas, `tests/RackCad.UI.Tests/`):

- `SelectiveEditorStateAdoptionTests.cs` (STA): la ventana real compone el estado y produce el mismo
  `DesignToInsert`/geometría resuelta tras load→build (caracterización antes/después).

Modificar (acotado, sin cambio de comportamiento observable):

- `src/RackCad.UI/RackSelectiveWindow.xaml.cs` (compone y delega en `SelectiveEditorState`; conserva
  pintado, editor de celda, eventos, recompute, carga y superficie pública). Se puede añadir un seam
  `internal` de prueba (como el `Session` de I-15).
- `docs/initiatives/README.md` (índice), `docs/initiatives/I-20-selective-editor-state.md` (este
  contrato), `docs/automation/state/I-20.yml` (estado versionado).

No se espera modificar el XAML de la ventana, `RackDynamicSystemWindow` ni su estado, el shell de I-15,
los resolvers/builders/BOM/DrawServices, los catálogos, la persistencia/DTO/envelope, `deploy/` ni
`.github/workflows`. Una desviación material obliga a detenerse (§12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato
   desde `TEMPLATE.md`, índice y estado versionado. (Evidencia: rama remota aceptada, este archivo.)
2. **Caracterización previa.** Seam `internal` de build + pruebas STA que fijan la geometría/diseño
   resuelto de escenarios representativos (load→build) sobre el código vigente. (Evidencia: suite UI
   verde sobre el código sin refactor.)
3. **Estado puro en Application.** `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`/
   `SelectiveApplyScope`/`SelectiveDesignInputs`/`SelectiveEditorState` + pruebas puras. (Evidencia:
   `RackCad.Tests` verde.)
4. **Adopción por la ventana.** `RackSelectiveWindow` compone y delega en el estado; superficie
   pública y comportamiento intactos. (Evidencia: builds Debug de UI y Plugin en 0 errores propios;
   caracterización STA sigue verde.)
5. **Gates automatizados.** Suite completa + suite UI + build de la solución + CI de la rama sobre el
   SHA publicado. (Evidencia: §14.)
6. **Cierre de sesión.** Revisión de diff, commits lógicos, push de la rama, estado versionado y gates
   manuales documentados como abiertos. (Evidencia: §14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` — suite completa verde (sin regresión) + las
  nuevas pruebas de estado.
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` — suite UI verde + la caracterización
  STA de adopción del estado.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (solo los
  `MSB3277` conocidos de AutoCAD; requiere AutoCAD 2025 para el build completo del Plugin).
- `dotnet build RackCad.sln -c Debug` — build completo de la solución.
- CI: los cuatro jobs (Tests, Build UI, Build Plugin sin AutoCAD, UI Tests) en verde sobre la punta
  publicada de la rama.

## 10. Validacion manual

Ambos gates **APROBADOS por el dueño** el **2026-07-21**, sin observaciones, sobre el DLL Debug del
worktree I-20 (punta aprobada de implementación `0f43087`;
`…-I-20-selective-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en
AutoCAD 2025. La confirmación normativa del dueño consta en la sesión de integración; **no se re-solicita**.

- **AutoCAD — editor selectivo — PASS.** `RACKSELECTIVO`: la matriz (clic en celda, −/+ niveles,
  altura, `Piso`, medio frente), «Aplicar a:» celda/nivel/frente/todas, «Editando fondo» (cambio de
  fondo con doble/triple profundidad), previews frontal y lateral, «Insertar frontal» y (con
  `RACKEDITAR`) «Actualizar» e «Insertar lateral/planta» dibujan **idéntico** a lo vigente; geometría,
  BOM y GUID sin diferencias; round-trip (reabrir con `RACKEDITAR`, mismo GUID) preservado; metadatos
  I-11 intactos al reabrir desde biblioteca (`LoadForNew`).
- **owner-validation — PASS.** El dueño confirmó que la apariencia (etiquetas, tooltips, orden,
  layout) y la interacción son idénticas a lo vigente.

Checklist manual detallado (para el dueño), sobre el DLL Debug del worktree:

- [ ] Matriz: seleccionar celda; editar frente/alto/conteo/larguero/peralte; ✎ aparece con overrides.
- [ ] Aplicar a: celda / nivel / frente / todas — el conteo de celdas afectadas coincide.
- [ ] Niveles por frente (−/+), altura por frente (auto y manual), `Piso` (larguero a piso).
- [ ] Medio frente (tramos) por frente; postes intermedios visibles en la preview.
- [ ] Número de fondos 1→2→3; «Editando fondo» conmuta y conserva la matriz de cada fondo; separadores.
- [ ] Cabecera por poste (Personalizar/Restablecer) y peralte por poste; preview del poste resaltado.
- [ ] Preview frontal y lateral; «Mostrar tarimas», placa base, cotas.
- [ ] Insertar frontal (rack nuevo); con `RACKEDITAR`: Actualizar en sitio e Insertar lateral/planta.
- [ ] Round-trip: reabrir con `RACKEDITAR` (mismo GUID) y desde biblioteca (`LoadForNew`, I-11).
- [ ] Guardar en biblioteca y BOM (Lista de materiales) sin diferencias.

## 11. Criterios de aceptacion

- Existe en `RackCad.Application` el estado del editor selectivo (`SelectiveEditorState` +
  `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`/`SelectiveApplyScope`/`SelectiveDesignInputs`),
  puro, sin referencias WPF/AutoCAD y sin duplicar reglas de geometría/BOM.
- `RackSelectiveWindow` **compone** ese estado, observa sus colecciones y delega las operaciones; el
  documento del editor ya no vive solo en campos privados de la Window. El pintado, el editor de celda
  y los eventos permanecen en la ventana.
- `BuildDesign` produce un `SelectivePalletDesign` **estructural y geométricamente equivalente** al
  vigente (mismo diseño resuelto, mismo BOM, mismo round-trip) para escenarios de fondo único, doble/
  triple profundidad, medio frente, cabeceras/peraltes por poste y seguridad.
- La superficie pública que consume el Plugin (`InsertRequested`, `SystemToInsert`, `DesignToInsert`,
  `RackId`, `RackName`, `InsertView`, `UpdateOnly`, `SetDimensionStyles`, `LoadExisting`,
  `LoadForNew`, `Session`) no cambia.
- `RackCad.Tests` cubre el estado puro y pasa; `RackCad.UI.Tests` cubre la adopción por la ventana
  real (STA) y pasa; builds Debug de UI, Plugin y solución en 0 errores propios; CI verde en la rama.
- La dirección de dependencias se conserva (Application no referencia UI/AutoCAD; la ventana observa el
  estado) y no hay dependencias NuGet nuevas.

## 12. Condiciones para detenerse

- Que preservar el comportamiento exija tocar geometría, resolvers, builders, BOM, DrawServices,
  persistencia/DTO/envelope o los metadatos I-11: detenerse antes de ampliar alcance.
- Que la extracción obligue a cambiar la superficie pública que consume el Plugin, o el XAML/controles.
- Que aparezca en `origin` otra rama tocando `RackSelectiveWindow`/`SelectivePalletDesign` de forma
  incompatible: no sobrescribir; entregar evidencia.
- Cualquier necesidad de un paquete NuGet nuevo (producto o test) o de tocar el editor dinámico/I-21.
- Que el alcance derive hacia I-22 (subtipos de seguridad, colocación por familia, `SelectionMatrix`).

## 13. Estado versionado y entrega del Pull Request

Estado canónico en `docs/automation/state/I-20.yml`. La automatización está pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesión. No
se abre un segundo Pull Request ni se activa auto-merge. Los gates `autocad` y `owner-validation`
quedan **abiertos** (pendientes del dueño); la integración a `main` (`git merge --no-ff`, WORKFLOW
§4.5) se realiza en la sesión de integración, no en esta rama.

## 14. Evidencia final

Se completa al cierre de la sesión: commits lógicos con trailer de procedencia, archivos creados/
modificados, resultados de `dotnet test` (suite completa y UI), builds de UI/Plugin/solución, evidencia
de CI sobre el SHA publicado, SHA base y punta de la rama, confirmación del push, gates manuales
abiertos (AutoCAD + owner-validation), invariantes comprobados (equivalencia de diseño y round-trip) y
confirmación de que `main` no fue modificada. `docs/HANDOFF.md` §8-12 y el estado en `docs/ROADMAP.md`
se actualizan **solo** en la sesión de integración (último commit de la rama), nunca desde esta rama.
