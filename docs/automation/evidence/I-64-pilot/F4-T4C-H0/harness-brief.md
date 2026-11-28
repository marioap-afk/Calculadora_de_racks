# Encargo: escribir y compilar el arnés de la sonda de host T4C-H0 (I-64 F4-T4C)

Escribes el arnés instrumentado de una sonda de host, solo de diagnóstico. **No es producto.** No se hace commit, no se integra y no cuenta como evidencia de cumplimiento. Lo operará en AutoCAD el Owner, después de que un revisor independiente lo revise.

## Dónde

- **El worktree desechable**, el ÚNICO sitio donde puedes escribir código: `<worktree-desechable>`. Está desacoplado (detached HEAD) en `b15072ff`, el producto de `4314c180`.
- **La salida del build:** `<salida>`, fuera del repositorio.
- **Prohibido escribir** en el worktree de I-64 (`<worktree-de-I-64>`) y en el checkout principal (`<checkout-principal>`). Puedes leerlos, si te sirve.
- **Tus notas y scripts** van en `<scratchpad>\hs65\t4c\h0\`.

## Qué manda

- **D-T4C-16** del paquete de F4-T4C, con su tabla de 12 filas «Lo que mide, y qué pasa con cada resultado». Está en `<scratchpad>\hs65\t4c\I-64-f4-t4c-gate-contract-package.md`. Léelo entero, sobre todo D-T4C-02 a D-T4C-04, D-T4C-09, D-T4C-15 y D-T4C-16.
- **Los mapas** M1 a M4 de esa carpeta (`map-*.md`), para saber dónde vive cada pieza.
- **La disposición del Owner:** `owner-f4t4c-package-decisions.md`, O-01 «H0 con dispensas».

## Qué hace el arnés (todo solo en Debug)

Todo el código va bajo `#if DEBUG` o en archivos que solo compila Debug. **El build Release tiene que quedar idéntico al de la base:** compruébalo con un diff de los fuentes efectivos o razonándolo, y dilo.

1. **Un botón de sonda en el panel del Workspace:** «H0: Actualizar (sonda)».
   - El clic llega al Plugin en el **contexto de aplicación**, igual que hoy cualquier botón de la paleta. Con un comando registrado no serviría.
   - Como la UI no puede referenciar AutoCAD, la llamada pasa por un gancho de Debug: por ejemplo, un delegado estático que asigna el Plugin.
   - **Al pulsar, el arnés:**
     1. registra en `RackLog`, con hora de alta resolución y el id del hilo, el instante T0, `SynchronizationContext.Current` y el documento activo, por su id nativo;
     2. llama a `ExecuteInCommandContextAsync` con un callback;
     3. al volver la llamada, registra la hora y `Task.IsCompleted`;
     4. engancha una continuación, con el planificador del contexto capturado, que registra la hora y el hilo al completar.
   - **Dentro del callback**, registra:
     - la hora de entrada, `MdiActiveDocument` (y si es el admitido), `CommandInProgress`, `Editor.IsQuiescent` y si `TopTransaction` es nula;
     - después toma `using (document.LockDocument())` de ese documento, construye el escritor real de T4B (`RackPilotUpdateWriter`) y llama a `TryUpdatePilot(request, writer)` del núcleo con la sesión y el rack del borrador que el operador tenga en Dirty en el panel;
     - si hace falta alcanzar el núcleo, usa un accesor solo-Debug;
     - el `EditUpdateOutcome` completo (estado, causa, diferencias y avisos) y la hora de salida.
   - **Para la fila 11** (¿bombea mensajes de WPF?), al entrar en el callback programa un `BeginInvoke` a prioridad `Background` que registre cuándo corre: antes o después de salir del callback.
   - **El callback nunca deja salir una excepción:** la captura y la registra.
2. **Un segundo botón:** «H0: contexto vacío». Llama a `ExecuteInCommandContextAsync` con un callback que solo registra la entrada y la salida, sin escribir nada. Es para la fila 7: el paso de Undo vacío.
3. **La traza de eventos del puente.** En los manejadores existentes de `AutoCadEventBridge`, solo en Debug, registra una línea por evento con la hora, el tipo y, en los de comando, `GlobalCommandName`. Los eventos son `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed`, `Object*` (con el tipo del objeto, si se puede sin abrir nada) y `EnteringQuiescentState`, más las pistas y el drenaje.
   - Así se miden las filas 4 y 5: los `*Cancel*` previos y el orden respecto de la relectura de POST.
   - Marca en la traza el inicio y el fin de cada fase de la corrida, si lo puedes hacer sin tocar la lógica de T4B: por ejemplo, con trazas de Debug en los ámbitos del escritor (`Begin`, `ReadForPrepare`, `PrepareUnits`, `BeginMutation`, `Seal`, `Post`).
4. **La reentrada del bloqueo** (fila 9): registra si los `LockDocument` anidados, el de `CaptureDrawing` y el de `SyncName` bajo el exterior, lanzan o funcionan.
5. **La fila 12:** no toques el `Debug.Assert` del drenaje del controlador; el operador dirá si salta.

## Reglas

- **No cambies la lógica de T4B ni del producto.** El arnés solo añade, en Debug, los botones, el gancho, el accesor y las trazas.
- **Sin commit y sin push.** No crees ramas. El worktree se queda desacoplado.
- **No ejecutes la suite completa de UI** (F4-E59 y H-1: escribe en el perfil real). Si quieres comprobar que compila, basta con el build. Si ejecutas pruebas, solo de Core y con filtro.
- **El build** usa el SDK de nivel de usuario: `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`, porque el `dotnet` del PATH no resuelve `global.json`.
  - `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -p:UseAutoCADNuGetReferences=true -o <salida>`
  - Registra los avisos y los errores. Calcula el SHA-256 del `RackCad.Plugin.dll` de salida y de los demás DLL de RackCad que haya allí.
- **No abras AutoCAD.**
- **Todo lo que escribas de registro** va a `RackLog`, donde va hoy el registro del Plugin.
  - Averigua dónde escribe `RackLog` en disco y dilo, para que la sesión recoja la traza.
  - Pon un prefijo fijo, `[H0]`, en cada línea.

## Lo que entregas

1. El parche: `git -C <worktree-desechable> diff > <scratchpad>/h0/h0-harness.patch`, con la ruta completa de la carpeta de notas.
2. Una nota, `h0-harness-notes.md` en la carpeta de notas:
   - qué archivos cambiaste y por qué;
   - qué fila de la tabla de D-T4C-16 mide cada traza;
   - cómo se usa: los pasos del operador, en orden y en español sencillo;
   - dónde queda la traza;
   - el build, con sus avisos, errores y SHA-256;
   - la razón por la que Release no cambia;
   - lo que el arnés NO mide, y lo que tiene que observar el operador (Undo, Redo, el Assert).

**Tu respuesta final** es una sola línea: el estado del build, el SHA-256 del DLL del Plugin y la ruta de la nota.
