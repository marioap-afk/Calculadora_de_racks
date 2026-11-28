# I-64 F4-T4A2-PREPARE-UI, intento 2 (corrección): revisión directa de la sesión

**Objeto:** el diff `f8ccef46..ae0a7b40` (RED `f2bb4d6f`, GREEN `ae0a7b40`), contra el contrato del intento 2 y el `analysis.md` de `872b`.

**Veredicto:** la corrección cumple el contrato del intento 2. No hay bloqueantes ni importantes; hay 2 menores.

## Comprobado

- **Alcance:** 7 rutas, todas dentro de las 15. El RED solo cambia pruebas y guardas (6 archivos); el GREEN solo cambia `WorkspacePilotFieldViewModel.cs`, y ningún archivo de `tests/` cambia entre el RED y el GREEN.
- **I-1:** la positiva de la regla 5 ya no fija cuentas exactas. Exige al menos una llamada a `_authorize` dentro de `PilotIntentReader` y al menos una fuera.
- **M-1:** en `PresentConfirm`, `PresentCheck` y `PresentChoice`, si `Render` deja el campo en fallo, el resultado no se presenta. En `PresentChoice` el aviso se calcula antes y solo se asigna después de la relectura. Las tres pruebas nuevas de UI fallan en el RED, según la entrega.
- **M-4:** la regla 1 exige la invocación `_port.TryX(request)` dentro de su cuerpo. Sus autopruebas capturan cada una de las cuatro operaciones como grupo de métodos por un solo `Replace` del texto real.
- **M-5:** la autoprueba mueve la referencia de `UnchangedText` fuera del caso Confirmed sin quitar la etiqueta. Las de las mitades son un `Replace` del XAML real, la carpeta enumera todos los `.xaml` de `src/RackCad.UI/Workspace/`, y la regla 3 comprueba que la pregunta va antes de Sí dentro del panel.
- **M-8:** `AssertHeld` compara la base y el estado actual por referencia, y el inicial con el comparador del piloto. El Worker lo declara como desviación porque `InitialDraftState` se entrega como copia en cada lectura, por contrato del núcleo, y `Drafts/` está prohibido. Se acepta: es la igualdad que el propio borrador usa para decidir Dirty.
- **M-9:** se retira la cuenta exacta no declarada; la de M-3 se mantiene.
- **M-2, M-3, M-6 y M-7:** las pruebas y declaraciones pedidas, según la entrega; la re-revisión independiente las comprueba una por una.

## Menores

| Id | Contenido | Disposición propuesta |
|---|---|---|
| D2-1 | Con la salida temprana de M-1, si la relectura falla después de un Conflict, un Reloaded o un Kept del host, que sí cambian el borrador, `_changed` no se llama y la tira de pestañas del navegador puede quedar desfasada hasta la siguiente presentación. Es una ruta defensiva: el puerto declara que `GetField` no lanza | Se acepta y se anota; el campo queda en su Error, que es lo conservador |
| D2-2 | La desviación de M-8 (el inicial por comparador y no por referencia) contradice la letra de INV-F4T4A2-13 del intento 2 («por referencia (Assert.Same)») | Se acepta como desviación declarada y justificada: la letra pedía algo imposible sin tocar `Drafts/` |
