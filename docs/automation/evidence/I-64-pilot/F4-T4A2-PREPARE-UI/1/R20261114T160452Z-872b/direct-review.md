# I-64 F4-T4A2-PREPARE-UI, intento 1: revisión directa de la sesión

**Objeto:** el diff `4eb7e924..9333396d` (RED `1fe36cf9`, GREEN `9333396d`), contra el paquete v3 y el contrato.

**Veredicto:** el producto cumple el contrato. No hay bloqueantes ni importantes; hay 3 menores.

## Comprobado

- **Alcance:** las 15 rutas exactas de la lista blanca. El GREEN solo toca `WorkspacePilotFieldViewModel.cs` y `WorkspaceHostCore.cs`; ningún archivo de `tests/` cambia entre el RED y el GREEN.
- **El host (D-T4A2-04):** las dos líneas exactas, justo después de `ConfirmBase`; `Confirmed(admission, hint)` y `Conflict(DescribeDifferences(confirm.Differences), admission, hint)`. Nada más cambia en el host (`WorkspaceHostCore.cs`, +6/−3). `EditConfirmOutcome` gana `ClassicEditorHint` y los parámetros opcionales; los resúmenes de M-8 se corrigieron.
- **La prueba existente declarada:** solo cambia `WorkspacePilotPrepareTests.cs:993`, a `Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, ...)`, sin renombrar la prueba.
- **El XAML:** los cinco botones con su `x:Name`, su `Content`, su `AutomationProperties.Name` y su `Click` de la tabla de T-A2. Los tres de la reconciliación llevan su propio `Visibility`; Sí y No van en `BrowserFieldChoicePanel` sin `Visibility` propio. No hay `IsDefault`, `IsCancel`, teclado ni tamaños fijos. `BrowserFieldReconcileText` y `BrowserFieldChoiceText` enlazan lo que dice la tabla.
- **El código subyacente:** cinco manejadores de la forma exacta `=> Contained(() => _viewModel.Browser.Field.X());`.
- **El VM del campo:**
  - **la oferta:** `OffersCheck` (pestaña fijada, borrador, sin fallo, sin elección; Clean, Dirty, Conflict o Unknown con CommitThrew) y `OffersChoice` (sin diagnóstico persistente; Conservar solo en Conflict con `IsProtected` y sin pendiente). `OffersChoice` no incluye la cláusula de la elección pendiente, que se añade solo en `Show*Button`;
  - **el despacho:** `CheckNow` lee la vista y despacha `TryCheckNow` (Unknown con CommitThrew) o `TryConfirmBase` (Clean, Dirty o Conflict), una vez, con `IsMine` tras la lectura;
  - **sin falso limpio:** `UnchangedText` solo se devuelve en `Confirmed` sin diagnóstico y con la vista releída en Clean o Dirty; Conflict con la vista releída da A2-02; otro estado, A2-35; sin borrador, A2-37. `RecoveredText` solo con `Recovered`. Cualquier otro estado da el prefijo y « No se comprobó.»;
  - **la elección:** el enumerado privado; la instantánea (estado, `IsProtected`, origen, pendiente y texto comprometido); la revalidación en `ConfirmChoice` antes de llamar, con A2-34 si difiere; una sola llamada a la operación elegida; Reloaded y Kept solo para la suya. `CancelChoice` no llama al puerto. La retirada en `Apply` pone A2-34; en `Fail` y `Clear` es silenciosa;
  - **la caja:** de solo lectura con una elección pendiente o con un diagnóstico persistente (`EditableOf`), y sin «Editar» en esos casos;
  - **el diagnóstico persistente:** `ClearNotices` no lo toca; solo lo sustituyen `Confirmed` o `Conflict` (que ponen el suyo o ninguno) y `Recovered` o `Conflict` de `TryCheckNow` (que lo retiran). Lo quitan también Reloaded, Kept, `Show` de otra superficie, `Clear` y `Apply` sin borrador. `NotAdmitted` de la elección lo pone;
  - **`ReconcileText`:** A2-10 con A2-14 solo con pendiente, sin diagnóstico persistente y sin elección; A2-11 y A2-12;
  - los textos son los de T-A2, en constantes; no hay `+=`, `event`, `Dispatcher`, `Task`, teclado ni foco.

## Menores

| Id | Contenido | Disposición propuesta |
|---|---|---|
| D-1 | En `PresentChoice`, un `Reloaded` que llegara para una elección de Conservar (o un `Kept` para Descartar y recargar) se presenta como «No se aplicó la elección: … No se hizo nada.», aunque el host sí habría cambiado el borrador. Es la rama defensiva que pide el paquete (A2-33), inalcanzable con el host real, porque cada operación responde su propio estado | Se acepta como está. Se anota por si F4-T4B cambia esas respuestas |
| D-2 | `CheckNow`, si el estado cambió entre la oferta del botón y la lectura de la vista, no llama al puerto y pone A2-34. El paquete dice «sin la condición de oferta, no hace nada». No contradice el contrato: no se lee el dibujo y el aviso es el de la situación cambiada | Se acepta: es más informativo y conservador |
| D-3 | La retirada por `Reloaded` o `Kept` del diagnóstico persistente no es alcanzable por la UI, porque las opciones no se ofrecen con un diagnóstico persistente. El Worker lo declara en `KnownLimitations` | Se acepta como defensa sin prueba propia, declarada |
