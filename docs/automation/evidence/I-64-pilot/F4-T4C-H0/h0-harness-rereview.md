# I-64 F4-T4C-H0: revisión focalizada de las correcciones del arnés

**Veredicto: APTO PARA OPERAR.**
- **Los 16 hallazgos de la revisión anterior están cerrados.** 15 se resuelven en el código o en la nota. H-14 se cierra con esta revisión y con el SHA-256 que midió la sesión.
- **Las correcciones dejan 7 hallazgos nuevos, todos MENORES:** 0 BLOQUEANTES y 0 IMPORTANTES.
- **Recomiendo incorporar los MENORES al texto de la guía antes de dársela al Owner.** No piden un build nuevo, y los tres de código (N-01, N-03 y N-04) pueden esperar.

**Qué se revisó:**
- el parche regenerado, `h0-harness.patch`: 1820 líneas, SHA-256 `f2a48bec…2ad2672f5`, igual al que declara la nota;
- la nota actualizada, con su §9, «Correcciones tras la revisión»;
- los fuentes del worktree desechable.

**Método, solo lectura.** No compilé, no ejecuté pruebas, no abrí AutoCAD y no escribí en el worktree.
- **El parche y el worktree:**
  - la parte de los archivos con seguimiento coincide exactamente con `git diff`;
  - los 4 archivos nuevos están enteros en el parche;
  - los dos archivos que no cambian respecto de la primera versión conservan su índice: el escritor, `9cad7782`, y `H0HostTrace.cs`, `83715a52`.
- **Release:** repetí mi comprobación independiente, `review_release_check.py`. El parche no borra ninguna línea, y cada una de sus 1460 líneas añadidas es una directiva DEBUG o cae en una región que solo compila DEBUG. Lo confirma en los 11 archivos.
- **El DLL:**
  - el `RackCad.Plugin.dll` de la carpeta de salida tiene SHA-256 `611808426baa…fdae790`, el que midió la sesión;
  - los otros tres DLL coinciden con la tabla del §6 de la nota;
  - los DLL son posteriores a la última modificación de los fuentes: el Plugin a las 03:28:35, y el último fuente a las 03:28:18.
- **La notación:** `patch Lnnn` es una línea del parche nuevo; `nota Lnnn`, una línea de la nota nueva; y `archivo:nnn`, una línea de la base `b15072ff`.

---

## 1. Los 16 hallazgos, uno por uno

| Id | Estado | Evidencia | Comentario |
|---|---|---|---|
| H-01 | **Resuelto** | La guarda es un `CompareExchange` sobre `_inFlightId` (patch L1063-1088), y `LeaveFlight` solo libera la guarda de su propia sonda (patch L1090-1094). Se toma antes de fijar la fase (patch L975 y L980; L1022 y L1027). Se libera en todos los caminos sin continuación: `not-requested` (patch L1013-1016 y L1055-1058), `call-threw` (L1155), `result-null` (L1174), `no-continuation` (L1200-1204), `launch-threw` (L1212), `schedule-failed` y `raw-threw` (L1226 y L1231), y `completed` y `continuation-threw` (L1385 y L1397) | Un segundo clic ya no pide nada, y la fase global ya no se reetiqueta. Lo que queda: N-01 |
| H-02 | **Resuelto** | `Focusable = false` e `IsTabStop = false` en los botones (patch L1774-1781) y en la casilla (L1733-1740). Las reglas del foco y el clic en la línea de comandos antes de `U` y `REDO` (nota L159-161 y L191-193) | Enter y la barra espaciadora ya no pueden volver a pulsar una sonda. Queda un residuo de foco, que es N-02 |
| H-03 | **Resuelto** | `ThreadFacts` solo lee AutoCAD con `probe.Dispatcher.CheckAccess()` (patch L1402-1419). La usan `continuation.raw` (L1223-1225) y `continuation.ran` (L1380-1384). `DetachFrames` y el texto del panel pasan por `OnPanelThread`, que hace `BeginInvoke` fuera del hilo (L1388-1392 y L1422-1443) | Repasé los caminos que pueden correr en el pool: `Schedule`, `OnCompleted` y `LeaveFlight`. Solo hacen lecturas administradas (`IsCompleted`, `GetResult`, que es vacío) y `H0Trace`. No queda ninguna llamada nativa fuera del hilo |
| H-04 | **Resuelto** | Las tres variables (patch L925-928). `harness.attach.injection` dice si cada una está puesta en el proceso, en el usuario y en la máquina, sin su valor (L956-962). `T0 … injection=` (L992 y L1034; L1558-1578). La comprobación de la sesión (nota L129-131) | Solo se registra, no se impide. Ver N-04 |
| H-05 | **Resuelto** | Las reglas de protección: «Cargar una vez», solo la copia, y ni informes ni recuperación (nota L154-157). Lo que mide la sesión al terminar (nota L148-150) | — |
| H-06 | **Resuelto** | La segunda sonda, a prioridad `Normal` (patch L1334-1357). La regla de lectura de tres condiciones (nota L100) | Ver el §2.4. Falta la regla con los fotogramas apagados (N-05 c) |
| H-07 | **Resuelto** | `row2-prediction`, calculado con el `SynchronizationContext.Current` del final del callback (patch L1278-1284). `getResult=returned(no-op)` (L1368-1378). La aclaración de `IsCompleted` en `call.returned` (L1165-1169). `acCoreMgdFileVersion` (L948-953). Los hechos del IL, las reglas y las consecuencias para el paquete (nota L27-55) | La predicción es correcta: según el IL, `Continuation` corre en línea en ese mismo hilo, justo después de que vuelva el callback |
| H-08 | **Resuelto** | El tope: 200 líneas de `Object*` por segmento, con un resumen por fuente y tipo que se escribe antes de la línea que cierra el segmento (patch L643-653 y L681-750). Los `Object*` del puente pasan por `ObjectEvent` (L395-439 y L477-493). El detalle se evalúa solo si la línea se escribe (L711). La espera de hasta 2 minutos (nota L166) | El orden respecto de las líneas que no son `Object*` se conserva. Lo que queda es una regla de lectura (N-05 a) |
| H-09 | **Resuelto** | `AtRest` aplica la regla de reposo de `PassGate` (`CTL:367-373`): `IsQuiescent` y sin comando activo para el puente (patch L1096-1108). Está en los dos botones (L1003 y L1044) | Introduce una interacción nueva, N-03 |
| H-10 | **Resuelto** | `released`, en un `finally` solo de Debug, después de los dos `using`, en `SyncName` (patch L28-35) y en `Traverse` (L301-305). La lectura de `top=OPEN` (nota L98) | El `finally` también registra `released` cuando el `LockDocument` lanzó. Ver N-05 b |
| H-11 | **Resuelto** | La línea de base de Deshacer (nota L168); el contexto vacío sin selección y con ella (pasos 4 y 5); el Actualizar con el campo limpio (paso 6); y la repetición (nota L174-197) | Falta anotar la selección en los pasos 6 y 7 (N-06) |
| H-12 | **Resuelto** | El §4.1 de la nota (nota L123-150) y la secuencia `D7` (patch L760) | La expectativa `loaded=4` puede dar una alarma falsa (N-04) |
| H-13 | **Resuelto en lo esencial** | Las 5 reglas de privacidad (nota L209-217). La nota ya no lleva rutas con el nombre del usuario (nota L8) | Faltan dos marcadores para las rutas nuevas de `harness.attach*` (N-07) |
| H-14 | **Cerrado con esta revisión** | El build nuevo; el SHA-256 que midió la sesión, igual al del archivo; la comprobación de Release repetida por el autor y por mí; y esta revisión del delta (nota L327) | Queda para la custodia registrar el orden: build 1, revisión, correcciones, build 2 y esta revisión |
| H-15 | **Resuelto** | Las llamadas de traza del puente van en su propio `try` (patch L329-439). `Attach` va en un `try` del controlador que registra con `RackLog` (L554-561). Los ensamblados se registran con su ubicación, su SHA-256 y `folders=` (L1525-1556) | Queda un residuo aceptable. Con un juego mezclado, los métodos de producto que nombran `H0Trace` directamente fallarían en la compilación JIT: `Traverse`, `SyncName`, el escritor y `NotifyStateChanged`. Pero el fallo de `Attach` se registra (`[H0] harness.attach FAILED`), y los ganchos no se asignan, así que se detecta antes de la primera sonda |
| H-16 | **Resuelto** | `CountFrames` y la casilla (patch L1697-1698, L1732-1741 y L1132-1133). La declaración en la nota (L298-299) y la repetición sin fotogramas del paso 10 (L195) | — |

**Las 16 filas están cerradas.**

---

## 2. ¿Las correcciones introducen defectos nuevos?

### 2.1 Release

**No hay ningún cambio.**
- Ninguna línea borrada.
- Todas las añadidas son solo de Debug (§ «Método»).
- Los nuevos `try`/`finally` de `SyncName` y `Traverse` están dentro de `#if DEBUG`.
- El autor repite la comparación de los fuentes efectivos (11 de 11 IDÉNTICOS) y la de los DLL Release byte a byte.

### 2.2 La guarda de vuelo

**Lo que está bien:**
- **Todos los caminos de salida liberan la guarda** (§1, H-01).
- **`LeaveFlight` compara con el id propio**, así que una compleción tardía no libera la guarda de otra sonda.
- **Un clic rechazado ya no toca `Phase`**, porque la guarda va antes.

**El residuo** (N-01):
- **El abandono solo se declara en el clic siguiente** (patch L1073-1083). El recuadro no dirá nunca «abandonada» por sí solo.
- **Tras el abandono, la guarda queda libre, pero el callback de la sonda abandonada no se neutraliza:**
  - `Callback` ejecuta `body` siempre (patch L1253-1265);
  - si AutoCAD lo tenía encolado y corre después, haría un Actualizar en medio de otra sonda.
- **No hay riesgo para el dibujo:** la ranura de T4B impide la concurrencia. Pero mezclaría las trazas.
- **La nota ya manda parar tras un abandono.**

### 2.3 El control de hilo

**Correcto y completo** (§1, H-03).
- `IsPanelThread` está dentro de un `try` y devuelve falso si falla.
- `OnPanelThread` contiene las excepciones en los dos caminos.
- No queda ninguna lectura de AutoCAD fuera del hilo del panel.

### 2.4 El tope de líneas

**No hay riesgo nuevo en el host.**
- **El cerrojo global `Gate`** serializa `H0Trace` y anida el cerrojo propio de `RackDiagnosticsLog`, siempre en ese orden (patch L686-689 y L706-718; `RackDiagnosticsLog.cs:42`). No hay ningún orden inverso, así que no hay interbloqueo.
- **El hilo principal evalúa el detalle de un `Object*` con `Gate` tomado:** `ObjectId.Handle` y lecturas puras del núcleo (patch L711 y L487). Ningún otro hilo espera al principal mientras tanto.
- **Una excepción dentro del `lock`** libera el cerrojo y queda en el `try` exterior.

**Para la validez:**
- **el orden se conserva:** el resumen va justo antes de la línea que cierra el segmento;
- **pero el resumen se fecha y se etiqueta al cerrar.** Su `phase=` y su `t=` son los de ese momento, y la fase cambia **antes** de las líneas de transición (`callback.enter` en patch L1258-1264, `callback.exit` en L1276-1284). Un resumen que cierra en `callback.enter` saldría con `in-callback` aunque sus eventos fueran anteriores (N-05 a).
- **En la práctica el efecto es mínimo:** la supresión solo empieza tras 200 eventos de un mismo segmento, y eso ocurre en la importación o en el sello, dentro de una misma fase.

### 2.5 La segunda sonda de la fila 11 (prioridad `Normal`)

**Es correcta y no crea reentradas:** solo escribe una línea.
- **La prueba que da:** las operaciones `Normal` de WPF no se aplazan por la entrada pendiente, así que un «BEFORE» con `sameThreadAsClick=True` solo puede venir de un bombeo dentro del callback.
- **Su efecto secundario:** añade un mensaje del despachador durante el callback. Pero no crea el bombeo: solo lo revela, igual que los fotogramas.
- **Entre las dos sondas y los fotogramas** se cubren las dos clases de prioridad de WPF, la de primer plano y la de fondo.
- **La regla de la nota** (L100) está bien para la corrida con fotogramas. Con la casilla apagada no hay `inCallback`, y la nota no dice cómo se decide entonces (N-05 c).

### 2.6 Lo demás

- **`AtRest` y el reposo del puente** (N-03). Si el comando interno no da `EnteringQuiescentState`, que es justo el caso C12 de la fila 5, `bridgeCmdActive` queda en `True` y todas las sondas siguientes se rechazan con `T0.refused-not-quiescent`.
  - No es un defecto: el `PassGate` del producto haría lo mismo, y es un dato.
  - Pero el operador quedaría atascado sin saber qué hacer.
  - Además, un clic rechazado por reposo deja `Phase` en `Pn:requested` (patch L980 y L1003; L1027 y L1044).
- **Los botones que no toman el foco** quitan el riesgo del Enter, pero cambian dos cosas (N-02):
  - **el foco de teclado se queda donde estaba.** Tras el paso 7.1, sigue en el cuadro «Escala de anotación» durante toda la sonda;
  - **el clic ya no provoca el `LostFocus` del cuadro, que hacía `CommitPending()`** (`WorkspacePanelView.xaml.cs:170`). Si el operador olvida el Enter del paso 7.1, el Actualizar da `PendingText` y no escribe. Es seguro, pero la corrida no sirve para las filas 5 y 6.
  - **Si algo se escribe en el cuadro editable,** Enter lo confirma y Esc lo descarta sin restaurar el valor anterior (`:139-147`).
- **`CheckAssembliesOnce`** calcula una sola vez, en el primer `T0`, los SHA-256 de los cuatro DLL, unos 4 MB (patch L1550-1556). Está entre `T0` y `call.before`, así que no afecta a la medida de la fila 1.

---

## 3. Los pasos del operador (§4 de la nota)

**Son seguros:**
- solo la copia, con su SHA-256;
- «Cargar una vez»;
- ni informes ni recuperación;
- cerrar sin guardar;
- clic en la línea de comandos antes de escribir;
- nada de Enter ni barra espaciadora en el panel;
- una sonda a la vez;
- solo en reposo.

**Ningún paso puede escribir fuera de la copia ni dejar estado persistente.**

**Están casi completos.** Faltan cinco aclaraciones de texto, sin build nuevo:
1. **Qué hacer si a los 2 minutos no sale «terminado».** Hoy solo dice «si dice abandonada, para», y eso solo aparece tras otro clic. Propongo: «si a los 2 minutos no dice «terminado», no pulses nada más: anótalo y avisa a la sesión» (N-01).
2. **Qué hacer con «no está en reposo» y la línea de comandos libre.** Es un dato de la fila 5: anotarlo, no insistir y avisar a la sesión (N-03).
3. **Un punto de control de la sesión entre el paso 3 (`RACKPANEL`) y el paso 4.** La sesión lee en el registro `harness.attach.injection`, `harness.attach.assembly.summary` y `acCoreMgdFileVersion`, y da el visto bueno antes de la primera sonda. Hoy el §4.1 pide esas líneas, pero no dice que se lean **antes** de operar (N-04).
4. **En los pasos 6 y 7:** «antes de pulsar, anota si el rack quedó seleccionado (con pinzamientos) al abrirlo en el navegador». El estado de la selección cambia los `*Cancel*` de la fila 4 (N-06).
5. **Sobre el foco:**
   - «tras el paso 7.1, comprueba que el cuadro no quedó con texto a medio escribir»;
   - «si al escribir `U` o `REDO` el texto aparece en el panel, no pulses Enter: bórralo y vuelve a hacer clic en la línea de comandos» (N-02).

---

## 4. Hallazgos nuevos

| Id | Gravedad | Hallazgo | Evidencia | Corrección |
|---|---|---|---|---|
| N-01 | MENOR | **El abandono** se declara solo en el clic siguiente, y el callback de una sonda abandonada no se neutraliza: si corre tarde, hace un Actualizar en medio de otra sonda | patch L1073-1083 y L1253-1265; nota L166 | Una marca `Abandoned` en la sonda: `Callback` registra `callback.late-after-abandon` y no ejecuta `body`. En la guía, la instrucción de los 2 minutos (§3, punto 1) |
| N-02 | MENOR | **El foco que queda tras H-02:** el cuadro del campo conserva el foco durante la sonda, y el clic ya no hace `CommitPending` por `LostFocus`. Un Enter olvidado da `PendingText`. Una tecla mal dirigida va al cuadro, donde Esc no restaura el valor | patch L1774-1781; `WorkspacePanelView.xaml.cs:139-147` y `:170`; `WorkspacePaletteHost.cs:52` | Las frases del §3, punto 5. La sesión descarta para las filas 5 y 6 las corridas con `run.outcome status=PendingText` |
| N-03 | MENOR | **`AtRest` y el reposo del puente:** sin `EnteringQuiescentState` tras el comando interno, todas las sondas siguientes se rechazan. Es un dato de la fila 5, que predice que `PassGate` bloquearía el Actualizar siguiente, pero el operador no tiene instrucción. Además, un clic rechazado deja `Phase` en `Pn:requested` | patch L1096-1108, L980/L1003 y L1027/L1044; `CTL:367-373` | La instrucción del §3, punto 2, y la regla de lectura «`T0.refused-not-quiescent bridgeCmdActive=True` tras una sonda = sin entrada en reposo del comando interno». Opcional: no fijar `Phase` hasta pasar `AtRest` |
| N-04 | MENOR | **Las comprobaciones de arranque** solo se registran: con una variable `ARMED`, la sonda pide igual. `CheckAssembliesOnce` mira una sola vez. Y `loaded=4` puede ser `loaded=3 notLoadedYet=[RackCad.Domain]` al crear el controlador, porque Domain se carga con retraso. Sería una alarma falsa | patch L956-962, L1543-1547 y L1550-1556; nota L131 y L134-135 | El punto de control de la sesión del §3, punto 3. La regla: `folders=1` siempre, y `loaded=3` con solo Domain pendiente es válido en `harness.attach`. Opcional: rechazar las sondas con `ARMED` (`T0.refused-injection-armed`) y repetir `T0.assembly` en el primer Actualizar |
| N-05 | MENOR | **Faltan reglas de lectura:** (a) el `phase=` y el `t=` de `event.Object.suppressed` son los de la línea que cierra el segmento, no los de sus eventos; (b) un `released` sin `held` antes significa que el bloqueo no llegó a tomarse; (c) con los fotogramas apagados, la fila 11 se decide solo con las dos sondas `row11.*`; (d) en el paso 6, un borrador que no existe da `NoDraft` antes que `NothingToApply`, y vale igual para la fila 7 | patch L681-690, L1258 y L1276; L28-35 y L301-305; nota L100; nota L182-185; `WorkspaceHostCore.cs:819-825` | Añadir las cuatro reglas a la nota §3 |
| N-06 | MENOR | **Los pasos 6 y 7 no registran** si el rack queda seleccionado tras abrirlo en el navegador. La selección cambia la fila 4 | nota L182-190 | La frase del §3, punto 4 |
| N-07 | MENOR | **Privacidad:** `harness.attach` y `*.assembly` llevan rutas locales nuevas, las de la carpeta de salida (`location=`) y de la instalación de AutoCAD (`acCoreMgd=`). La regla 2 de la nota no tiene marcadores para ellas | patch L952 y L1535-1538; nota L211-214 | Añadir `<salida>` y `<autocad>` a la regla 2 |

**Recuento:** 0 BLOQUEANTES, 0 IMPORTANTES y 7 MENORES.

---

## 5. Veredicto

**APTO PARA OPERAR.**

**Lo que está bien:**
- **las 16 correcciones están hechas, y con evidencia.** H-14 se cierra con esta revisión;
- **Release sigue idéntico a la base;**
- **no hay ningún defecto nuevo de seguridad en el host:**
  - la guarda es correcta;
  - el control de hilo es completo;
  - el tope no puede crear un interbloqueo;
  - la sonda `Normal` solo registra;
- **el DLL que se va a cargar** es el que midió la sesión.

**Antes de dar la guía al Owner, recomiendo:**
- **las cinco aclaraciones del §3,** que son solo texto;
- **las reglas de lectura de N-05,** para quien lea la traza;
- **el marcador de N-07,** para la custodia.

Los tres cambios de código opcionales (N-01, N-03 y N-04) no condicionan la operación. Si se aplican, piden otro build, otro SHA-256 medido por la sesión y una revisión de ese delta.
