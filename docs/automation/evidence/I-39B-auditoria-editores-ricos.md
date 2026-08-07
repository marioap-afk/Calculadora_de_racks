# I-39B — Auditoría de cumplimiento de ADR-0029 en los seis editores ricos

> Evidencia **medida** sobre la base `3853cd4`. Son hallazgos técnicos, **no decisiones**: ajustan
> **cómo** se implementa I-39B, no **qué** debe entregar. El alcance lo fija su fila del ROADMAP.
> Contrato: [`../../initiatives/I-39B-interaccion-editores-ricos.md`](../../initiatives/I-39B-interaccion-editores-ricos.md).

## 1. Dependencia latente de recursos del shell rico — CONFIRMADO, ya corregido

`RackEditorVisualShell` consume **seis** tokens `DynamicResource` de clave de cadena que **no** caen al
diccionario de tema del ensamblado: `ShellWindowBackgroundBrush`, `ShellFontSizeBase`,
`ShellFontFamily`, `ShellZoneSpacing` (caería a `Thickness(0)`), `ShellSidebarWidth` (a `NaN`) y
`ShellPreviewMinHeight` (a `0`). Es el mismo defecto que I-39A midió y corrigió en el shell acotado.

Está **enmascarado** hoy: los cuatro consumidores son ventanas XAML que mergean `AppStyles` en
`Window.Resources`, cada `x:Key` del repositorio se define **una sola vez** y ninguno de los cuatro
declara ninguna clave propia.

**Hallazgo que solo apareció al implementar:** mergear siempre el diccionario compartido —lo que hace
el shell acotado— **sombrea** el de la ventana también para el contenido inyectado en los slots, y ese
contenido pasa a resolver **otra instancia** del mismo `Style`. Lo detectó
`PushBackShellAdoptionTests.ActionButtons_UseTheCommonStyles_AndShowReasonsWhenDisabled`, que fija
`PrimaryButtonStyle` **por identidad**. Se corrigió el diseño —respaldo en `OnApplyTemplate`, y solo
cuando el token **no** resuelve— en vez de tocar esa prueba.

**Deuda registrada:** `RackBoundedEditorShell` conserva el merge incondicional y hoy **nadie lo
detecta**, porque sus consumidores no fijan estilos por identidad. No se toca en I-39B: es arquetipo B.

## 2. Push Back no era el único caso de cierre relevante — CONFIRMADO

Push Back es la única de las seis **sin** `IsCancel`, de modo que hoy Escape **no la cierra**. Las
otras cinco sí lo llevan y **ninguna** tiene guarda.

## 3. El configurador de cabecera es el caso grave — CONFIRMADO

Tiene las tres piezas y ninguna se aplica al cerrar:

- `IsCancel="True"` en su botón `Cerrar`;
- `HasUnsavedManualEdits` en `RackFrameConfiguratorViewModel`, documentado como «True when discarding
  the model (Restore / Generate) would lose manual edits or exceptions»;
- `ConfirmDiscard(string)`, un `MessageBox` Sí/No, invocado desde «Restaurar estándar» y «Generar
  cabecera».

**Hoy Escape y la X descartan ediciones manuales de cabecera sin preguntar**, mientras la misma ventana
sí pregunta al restaurar. La protección existe y el cierre la evita.

## 4. El ámbito perdible de Push Back excede `ModuleSession` — CONFIRMADO

`ModuleSession.HasPendingChanges` cubre **solo** los módulos longitudinales. Nada llega al DWG hasta
Insertar o Actualizar, así que cerrar descarta además matriz, seguridad, topes y la cabecera
personalizada escenificada. Añadir `IsCancel` **sin** política convertiría Escape en un descarte de
todo eso, mientras la ventana muestra «Cambios pendientes: confirma o cancela».

Además, la escenificación se pierde **sin aviso** por una vía ajena al cierre: un cambio estructural
que altere la firma de módulos provoca `ReseedModuleSession` y anula lo escenificado. Es un defecto de
producto **preexistente e independiente del cierre**; se registra y no se corrige aquí.

## 5. Cero ventanas con `OnClosing` — CONFIRMADO al auditar

Ninguna de las seis declaraba `OnClosing`. El único override de ciclo de vida era `OnClosed` en el
configurador, que es **post-cierre y no cancelable**. No existía punto de intercepción en ninguna.

## 6. Incumplimientos observables y no observables — CONFIRMADO

Seis incumplimientos son corregibles sin que el usuario lo note y diez cambian algo visible. La
distinción **no** reduce el alcance: determina qué exige validación manual y en qué orden se toca cada
cosa. Los no observables se hicieron primero, con caracterización previa, para que la adopción
observable ocurra sobre una red de seguridad.

## 7. Puntos con exclusión real y previa — REGISTRADOS, no convertidos en iniciativa

- **Contrato de inserción paralelo del configurador de cabecera.** ADR-0029, en «Lo que este ADR NO
  decide», excluye expresamente autorizar cambios de persistencia e identidad. La ventana mantiene
  `InsertRequested`/`InsertView` propios en vez de `RackEditorSession`. **No se toca en I-39B**; si
  proteger el cierre lo exigiera, se detiene y se registra.
- **Defecto de reseed de Push Back** (punto 4): pérdida silenciosa ajena al cierre, preexistente.
- **Merge incondicional del shell acotado** (punto 1).

Ninguno abre una iniciativa nueva por decisión propia.
