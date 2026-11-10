# I-64 — F3 Browser Session: paquete del contrato de gate (propuesta, NO emitido)

Preparado por la sesión al cerrar F2 (D-09). **No está emitido ni autorizado.** Reúne lo necesario para emitirlo en cuanto se decida lo de la sección 2.

## 1. Alcance (Freeze V5 §5 D-08, §11 fila F3)

Pestañas preview y fijadas, cierre sin escritura, registros ligeros y K superficies (K = 1) con la superficie de resumen. Depende solo de F1. No hay hito de host.

Obligaciones del gate: INV-02 con pestañas, la regla de preview de INV-04, INV-09, INV-15 e INV-38 (pestaña), y los arneses automatizados de los escenarios 4 (cambio de rack), 5 (cambio de pestaña) y 6 (10 pestañas ligeras).

Fuera de F3: borradores, dirty real y edición (F4), migración de editores (F5), navegador e inventario (F6).

## 2. Decisiones necesarias antes de emitir

| Id | Decisión | Recomendación |
|---|---|---|
| C-F3-01 | **Presupuesto.** `attempts` está en 3 de 3, así que cualquier corrección sería S-11. El arranque de F2 lo resolvió una disposición expresa («los attempts de F1 no pasan a F2») | Disposición expresa del Owner o del Coordinator: F3 arranca con `attempts` = 1 de 3 |
| C-F3-02 | **Origen de nombre y Kind de la pestaña.** La selección solo aporta RackId y el panel no lee por eventos (INV-F2T2B-13) | Crear la preview solo con RackId. Leer nombre, Kind y vistas una vez al materializar la superficie de la pestaña activa, con una consulta de resumen sobre `NavigationTargets` (INV-38), con caché por (sesión, RackId) |
| C-F3-03 | **A qué rack apunta la navegación** cuando hay pestaña activa | Al de la pestaña activa. Una selección Uno(R) activa o crea su pestaña, así que coinciden salvo que el usuario cambie de pestaña |
| C-F3-04 | **«Abrir en editor clásico»** (D-08, sistemas sin integrar) necesita lanzar un comando desde el panel, y D-08 de esta noche evitó precisamente el comando interno | Diferirlo a F5. En F3 la superficie muestra el resumen y el estado |
| C-F3-05 | **Vida de las pestañas** | Por `SessionId`, solo en memoria; terminan con la sesión |
| C-F3-06 | **Contextos que no son Uno(R)** | Selección (N), None y diagnósticos no crean ni cambian pestañas (D-09 del Freeze) |

## 3. Tareas propuestas

**F3-T1-BROWSER-CORE (Application, puro).** `BrowserSession` y su registro por `SessionId`: registros (RackId, nombre, Kind, preview o fijada, estado), activación, reemplazo de la preview, fijar y cerrar. Sin tipos de AutoCAD ni de WPF. Pruebas en `tests/RackCad.Tests/Workspace/`.

**F3-T2-BROWSER-UI (WPF).** `WorkspaceBrowserViewModel`, tira de pestañas sin anchos fijos, una sola superficie de resumen viva (K = 1) y navegación ligada a la pestaña activa. Arneses de los escenarios 4, 5 y 6 en `tests/RackCad.UI.Tests/Workspace/`.

**Archivos previstos.** Nuevos: `src/RackCad.Application/Workspace/BrowserSession.cs`, `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs` y sus pruebas. Candidatos: `WorkspacePanelViewModel.cs`, `WorkspacePanelView.xaml(.cs)`, `WorkspaceNavigationViewModel.cs`, `IWorkspaceNavigationPort.cs`, `WorkspaceNavigationHost.cs` y las guardas afectadas.

## 4. Invariantes propuestos

1. Un registro de pestaña no retiene árboles WPF, sistemas resueltos, BOM, objetos de AutoCAD ni suscripciones.
2. Como máximo una preview por sesión; una selección Uno(R) activa la pestaña de R si existe y, si no, crea o reemplaza la preview.
3. Cerrar una pestaña nunca escribe el DWG. Hay cero llamadas de escritura en todo el ciclo.
4. K = 1: con N pestañas hay exactamente una superficie viva.
5. Las pestañas no añaden suscripciones a eventos de AutoCAD.
6. La pestaña usa la misma autoridad de objetivos que el contexto.
7. Ningún cambio de contexto o de pestaña dispara lectura salvo la consulta de resumen de la superficie que se materializa, una por materialización y con caché.
8. Sin anchos fijos: la tira y la superficie funcionan en barra estrecha y en superficie amplia.

## 5. Advertencia heredada de F2

Cualquier acción de panel que escriba selección o vista debe probarse en AutoCAD real (D-08). Los arneses de F3 no sustituyen esa comprobación.
