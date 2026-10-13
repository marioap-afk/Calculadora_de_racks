# I-64 — Paquete de cierre delta de F1-T3B-HOST-UI-C1 (colocación inicial de la paleta del Workspace)

Preparado por la sesión responsable el 2026-10-13 por orden del Coordinator («I-64 — Smoke-1 STOP disposition /
F1-T3B-HOST-UI-C1 authorized», literal en `coordinator-authorization.md`). Para **COORDINATOR_T3B_C1_CLOSE_REQUIRED**.

Este paquete:
- **no** declara el cierre de C1, Candidato de Smoke-1 ni FINAL_CANDIDATE_SHA;
- no ejecutó AutoCAD, no pide otra ocurrencia de Smoke-1 y no inicia F2.

La tarea la ejecutó directamente la sesión responsable, sin invocaciones de modelos: la orden no nombró Worker, Controller ni celdas.

## 1. Identidad

| Hecho | Valor |
|---|---|
| Tarea | `F1-T3B-HOST-UI-C1`, corrección estrecha de producto tras el STOP CONFIRMED de Smoke-1 `R20261013T011654Z-184b` (SM-03) |
| Base | `30d600adc1b030d1ca391cb9aae3c73e9876e2c5` (custodia de Smoke-1); producto previo = `src/` y `tests/` de `e0205b5a` |
| **RED** | `0d2fddf800168c7bca0253edb377263815e924bd`; CI 38341291918 (push): `Tests (Domain + Application)` = failure con **exactamente las 2 pruebas nuevas en FAIL** (Failed 2, Passed 13124, Total 13126); `Build Plugin without AutoCAD` = skipped (depende de Tests); `Build UI` y `UI Tests` = success |
| **GREEN / producto** | `b874705e26fff203e8727f6ed2362eca497b5860`; CI 38341458322 (push) **4/4 success** |
| DLL nuevo | `RackCad.Plugin.dll` `FB9523EEA2EDA1DDB53D1CAD1FCB0D6743F4B0F81AC7198645226B96984180EF` (Debug, `b874705e`) |
| Candidato anterior | `4d79e91036acdaa3e62b7b54677f394593f581a0` = **HISTORICAL_SMOKE1_STOP_CANDIDATE**; no se reutiliza (DLL `E769219D…7512`) |

## 2. Inspección de la API pública de `PaletteSet` en AutoCAD 2025

Metadatos de `acmgd.dll` de la instalación (SHA-256 `7AA8BF5F…BC8C`), leídos con `System.Reflection.Metadata`: sin cargar el
ensamblado, sin AutoCAD y sin reflexión sobre AutoCAD. Inventario completo en `paletteset-api-acad2025.txt` y herramienta en
`apidump-Program.cs`. La CI compila contra AutoCAD.NET 25.0.1, la misma API 25.0. Miembros relevantes:

| Miembro | Tipo | Uso en C1 |
|---|---|---|
| `DeviceIndependentSize` (get/set) | `System.Windows.Size` | tamaño inicial en unidades independientes de los DPI |
| `DeviceIndependentMinimumSize` (get/set) | `System.Drawing.Size` | tamaño mínimo |
| `InitializeFloatingPosition(Rect)` | `System.Windows.Rect` | rectángulo flotante inicial, para cuando el usuario la desacople |
| `Dock` (get/set) | `DockSides` (None 0, Left 4096, Top 8192, Right 16384, Bottom 32768) | acople inicial a la derecha |
| `DockEnabled`, `Style`, `KeepFocus` | — | sin cambios |
| `Size`, `MinimumSize`, `Location`, `DeviceIndependentLocation`, `TitleBarLocation`, `RolledUp`, `AutoRollUp`, `FloatControl`, `RecalculateDockSiteLayout` | — | no se usan |
| eventos `Load`, `Save`, `Saving`, `PaletteSetMoved`, `SizeChanged`… | — | no se usan |
| constructores con `Guid` | — | no se usan; sigue `new PaletteSet("RackCad")` |

Se eligieron las variantes `DeviceIndependent*` porque son las que AutoCAD escala con los DPI de la pantalla. Las variantes en píxeles
(`Size`, `Location`) dependen de la escala, y la sospecha del hallazgo de SM-03 es un arrastre que no respeta la escala.

## 3. Cambio (archivos)

`git diff --stat 30d600ad b874705e`: **2 archivos, +166 / −1**. Nada más cambia en `src/` ni `tests/` respecto a `e0205b5a`.

| Commit | Archivo | Cambio |
|---|---|---|
| RED `0d2fddf8` | `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs` | +132: 4 guardas nuevas (sección 4) |
| GREEN `b874705e` | `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` | +34 / −1 |

**GREEN, en `WorkspacePaletteHost`:**
- Seis constantes privadas: `InitialWidth` 320, `InitialHeight` 560, `MinimumWidth` 240, `MinimumHeight` 320, `FloatingLeft` 80 y
  `FloatingTop` 80, en unidades independientes de los DPI.
- Un método privado `ApplyInitialPlacement()`, llamado **una sola vez** desde el constructor (que corre una vez por proceso, en el
  primer `RACKPANEL`), después de `AddVisual` y antes de las suscripciones. Hace:
  - `DeviceIndependentMinimumSize = new System.Drawing.Size(240, 320)`;
  - `DeviceIndependentSize = new System.Windows.Size(320, 560)`;
  - `InitializeFloatingPosition(new System.Windows.Rect(80, 80, 320, 560))`: cabe en una pantalla principal de 1024 × 768;
  - `Dock = DockSides.Right`, uno de los lados de `DockEnabled`.
- La colocación está **contenida**: `try` / `catch (System.Exception)` → `LogFault` (RackLog). Si falla, la paleta se muestra igual.
- `Show()` **no cambia**: solo `_palette.Visible = true`. Un `RACKPANEL` repetido no vuelve a colocar la paleta, así que el usuario
  conserva durante el proceso sus movimientos y acoples.
- La documentación XML de la clase menciona la colocación inicial.

## 4. Pruebas

**Guardas nuevas** (fuente, en `WorkspacePaletteHostSourceGuardTests`, como las de T3B):

| Prueba | Exigencia de la orden | RED |
|---|---|---|
| `TheFirstCreationConfiguresAnExplicitDeviceIndependentPlacementAndDocksThePalette` | colocación y acople iniciales explícitos: tamaño, mínimo y rectángulo flotante con constantes dentro de rangos usables (240–600 × 300–700; mínimo ≤ tamaño; rectángulo dentro de 1024 × 768); acople a la derecha después del tamaño; colocación contenida | **FAIL** (no existía) |
| `RepeatedShowNeverReappliesTheFirstPlacementAndTheUserKeepsMovesAndDocking` | `Show()` no reaplica ni reinicia la geometría: una declaración y una llamada, en el constructor, tras `AddVisual` y antes de las suscripciones; `Show()` = solo `Visible = true`; ningún miembro de geometría fuera de la colocación inicial | **FAIL** (no existía) |
| `TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism` | sin GUID ni `toolID`; sin `Load`/`Save`/`Saving`; sin registro, ajustes, `System.IO`, variables de entorno o del sistema, `TRUSTEDPATHS`, `SECURELOAD`, perfil, `ApplicationPlugins` ni carga automática; las mismas 2 suscripciones; sin seguir movimientos ni tamaños; los mismos 6 archivos en `Plugin/Workspace` | PASS (guarda) |
| `TheExistingStyleDockingCapabilityAndKeepFocusOfThePaletteAreIntact` | `new PaletteSet("RackCad")` único; `Style` exacto (`ShowPropertiesMenu`, `ShowAutoHideButton`, `ShowCloseButton`); `DockEnabled = Left \| Right`; `KeepFocus = true`; `AddVisual("Workspace", _root)` | PASS (guarda) |

Las 41 guardas anteriores de T3B y las 43 de T3A siguen en verde sin cambios, incluidas la de una sola entrada a `RackLog`, la
superficie pública `GetOrCreate` / `IsCreated` / `Show`, sin temporizador ni `Dispatcher`, y G1.

**Verificación local sobre `b874705e`** (después del commit, árbol limpio antes y después, AutoCAD cerrado, SDK 8.0.423):

| Suite o build | Resultado |
|---|---|
| Core enfocada (`FullyQualifiedName~RackCad.Tests.Workspace`) | PASS 531/531 |
| UI enfocada (`FullyQualifiedName~Workspace`) | PASS 66/66 |
| Core Full | PASS 13126/13126 (13122 + 4 nuevas) |
| UI Full | PASS 1701/1718 (17 Skip existentes, 0 con error) |
| Debug UI build | PASS (0 advertencias) |
| Debug Plugin build | PASS (0 errores; solo MSB3277, conocidas de las referencias de AutoCAD) |
| CI del SHA exacto | 38341458322 push 4/4 |

Registro de pasos en `local-verification-steps.txt`. Los registros completos de dotnet llevan rutas locales y no se versionan.

## 5. Sin persistencia ni seguridad nuevas

- **Identidad:** la paleta sigue sin GUID ni `toolID`, construida como `new PaletteSet("RackCad")`, así que AutoCAD no tiene con qué
  persistir su estado. La colocación se aplica en memoria en cada proceso nuevo.
- **Escrituras:** ninguna en registro, perfil, ajustes de RackCad, variables de entorno ni sistema; no se usan `Load` ni `Save`.
  Guardas: la tercera de la sección 4 y las de T3B (`ThePaletteHasNoLoadNorSaveHandlers`,
  `ThePaletteHostNeverReadsOrWritesTheDrawingNorTheCommandLineNorTheDisk`).
- **Seguridad:** nada de `TRUSTEDPATHS`, `SECURELOAD`, `ApplicationPlugins`, bundles ni carga automática.
- **Suscripciones:** las mismas 2 de T3B. No se sigue ningún movimiento ni tamaño de la paleta.
- **Fuera de alcance, sin cambios:** puente, sesiones, controlador, lector de selección, comando `RACKPANEL`, UI y DWG.
- **Verificación de host pendiente:** el diff de perfil de T3C (S1-11) en el próximo Smoke-1 lo comprobará en el host. Una clave de
  paleta nueva seguiría siendo REPORT; con la aceptación de I-52, `REPORTED_ACCEPTED`.

## 6. Comparación con D-01 (Freeze V5)

| Requisito de D-01 | Antes de C1 (`e0205b5a`) | Después de C1 (`b874705e`) |
|---|---|---|
| un único `PaletteSet` por proceso, `AddVisual` de un solo control raíz WPF | sí | sí, sin cambios |
| sin `toolID` ni manejadores `Load`/`Save`; RackCad no pide persistir estado | sí | sí; la colocación es en memoria |
| **acoplable y flotante** | `DockEnabled` Left \| Right, pero sin tamaño ni posición: en el host la paleta flotó y salió de la pantalla (SM-03) | **acoplada a la derecha** al crearse, con tamaño y mínimo definidos y un rectángulo flotante en pantalla para cuando se desacople; sigue acoplable a izquierda o derecha |
| se muestra con `RACKPANEL` | sí | sí; `Show()` sin cambios |
| cerrar la paleta la oculta; las sesiones siguen | sí | sí, sin cambios |
| foco: solo `KeepFocus` público | sí | sí, sin cambios |
| contingencia (b) `ShowModelessWindow` | no usada | no usada (exigiría una A-n material) |

## 7. Evidencia de Smoke-1 conservada

- `git diff --quiet 30d600ad HEAD -- docs/automation/evidence/I-64-pilot/F1-SMOKE-1` es vacío: la custodia de
  `R20261013T011654Z-184b` (STOP en SM-03) y de `R20261012T195007Z-4141` (PRECHECK_ABORTED) no se toca.
- SM-03 no se reetiqueta.
- El STOP queda **CONFIRMED** por el Coordinator.

## 8. Límites y residuales

- **Comportamiento real en el host, sin verificar:** no se lanzó AutoCAD. No se ha comprobado que AutoCAD 2025 respete `Dock` asignado
  antes del primer `Visible = true`. Si lo ignorara, la paleta aparecería flotante en el rectángulo inicial (80, 80, 320 × 560
  unidades independientes), que está dentro de la pantalla. Lo comprobará el próximo Smoke-1, en SM-03 y SM-17.
- **Causa del arrastre errático de SM-03, sin demostrar:** la escala de la pantalla es una hipótesis. C1 evita depender del arrastre
  inicial (la paleta nace acoplada) y fija el tamaño en unidades independientes de los DPI.
- **Candidato de un próximo Smoke-1:** lo decide el Coordinator. Este paquete añade un commit documental sobre `b874705e`. Según la regla
  del Candidato (guía §7.1), el SHA que se autorice necesitará su propia evidencia local posterior al commit y su CI exacta.
- **Cosmético:** en la documentación XML de la clase, una línea quedó más larga que las demás. Es solo un comentario: no se corrige para
  no crear otro SHA de producto.

## 9. Siguiente

STOP en **COORDINATOR_T3B_C1_CLOSE_REQUIRED**. Sin AutoCAD, sin otra ocurrencia de Smoke-1 y sin F2.
