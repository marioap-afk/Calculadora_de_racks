# ADR-0019: Shell visual de editores por composición y slots, agnóstico al sistema

- **Estado:** aceptado
- **Fecha:** 2026-07-24 (propuesto); 2026-07-24 (aceptado por el dueño del repo, «Acepto ADR»)
- **Decisores:** dueño del repo (aceptó el 2026-07-24); Claude Opus 4.8 (redacción)
- **Iniciativa relacionada:** I-30 `architecture/editor-visual-shell` (y su continuación I-31)

## Contexto

`docs/ARCHITECTURE.md` §7.3 declara desde hace tiempo un «Editor Shell» objetivo con controles
comunes (`NumericField`, `CatalogCombo`, `SelectionMatrix`, `PreviewCanvas`, base
`RackDialogWindow`). I-14 los construyó, I-15 entregó `RackEditorSession` e I-20/I-21 sacaron el
estado de los editores a `RackCad.Application`. La auditoría de I-30 sobre `main` = `8a1bce5`
midió, sin embargo, que la **adopción no ocurrió**:

- `NumericField`, `CatalogCombo` y `RackDialogWindow` tienen **cero** consumidores en
  `src/RackCad.UI`; los `PreviewCanvas` de las tres ventanas ricas son un `<Canvas
  x:Name="PreviewCanvas">` homónimo, no el control; `SelectionMatrix` solo lo adoptaron los tres
  diálogos de seguridad de I-22.
- Hay **43 brushes privados** duplicados (16 selectivo, 17 dinámico, 10 cama) frente a los 8 de
  `PreviewPalette`, y `Themes/AppStyles.xaml` aporta apenas 6 claves con hex incrustados: **no
  existen tokens** de tamaño, tipografía ni espaciado.
- Cada ventana reimplementa su matriz imperativa, su proyección de preview, su barra de acciones y
  su convención de estado.

El resultado observable es el rechazo del gate manual de I-18 (hallazgo **PB-VAL-01**: «interfaz no
alineada con la del editor Dinámico… la mala interfaz impide detectar y validar otros errores»), que
obligó a tres rondas correctivas en `feature/push-back`. Sin una capa de composición común, cada
editor nuevo vuelve a pagar —y a fallar— la misma coherencia visual.

Falta decidir **cómo** se comparte lo visual. La decisión condiciona a los tres editores existentes,
a Push Back y a cualquier sistema N+1, y es cara de revertir una vez migrado un editor de 2 848
líneas: cumple los criterios 1 y 2 de `docs/adr/README.md`.

## Decisión

Los editores ricos se componen sobre un **shell visual común** (`RackEditorVisualShell`) con estas
reglas:

1. **Composición por slots, no herencia.** El shell expone slots (`SidebarHeader`,
   `SidePanelContent`, `MatrixContent`, `PreviewContent`, `ActionBarContent`, `StatusContent`) como
   `ContentControl`/`ContentPresenter`; cada editor **inyecta** su contenido. Los slots opcionales
   pueden quedar vacíos y el shell recoloca sin huecos (un editor sin matriz es un caso soportado).
   El **encabezado del panel es un slot neutral y opcional**: el shell reserva el espacio pero **no
   obliga a mostrar el GUID** ni impone un contrato de identidad; qué se muestra lo decide el editor.
2. **Sin herencia profunda de ventanas.** No se construye una jerarquía de `Window` base por editor;
   `RackDialogWindow` no se adopta como ancestro de los editores ricos.
3. **Agnosticismo respecto de `RackSystemKind`.** Los archivos del shell no referencian
   `RackSystemKind` ni ningún sistema concreto, y **no contienen ramas** por Selectivo, Dinámico o
   Push Back. La variación se expresa con contenido inyectado, `DataTemplate` y **modelos puros**.
4. **Separación estricta en tres capas.** El shell posee composición, tokens, severidades y las
   **categorías visuales neutrales** de acciones; `RackEditorSession` (I-15) posee identidad,
   catálogo, recompute coalescido e inserción; los estados puros de Application (I-20/I-21) poseen
   estructura, selección y ensamblado del diseño. El shell **no** envuelve ni sustituye a la sesión, y
   no toca geometría, BOM ni persistencia. La action bar se estructura en categorías **neutrales por
   posición** —`LeadingActions`, `SecondaryActions`, `PrimaryActions`, `TrailingActions`—; **el editor
   concreto decide qué acciones ofrece y en qué categoría las clasifica**.
5. **Adoptar el shell NO exige sustituir los controles específicos existentes.** La migración de un
   editor **aloja sus controles actuales tal cual dentro de los slots**, preservando su parsing,
   eventos, `LostFocus`, recomputación y comportamiento. Sustituir esas entradas por los controles
   comunes de I-14 (`NumericField`, `CatalogCombo`) **no es consecuencia automática de este ADR**:
   puede hacerse **después**, mediante migraciones separadas o adaptaciones mínimas y justificadas,
   nunca como efecto colateral de adoptar el shell.
6. **Migración progresiva (strangler), no big-bang**: **Dinámico en I-30**, **Selectivo en I-31**, y
   **Push Back después**, cuando I-18 se rebase tras I-31. Ningún editor migra antes de que el shell
   exista con pruebas propias.

## Alternativas consideradas

- **Ventana base por herencia (`RackDialogWindow` como ancestro de los editores).** Descartada: la
  herencia fija el layout en el ancestro y obliga a `protected virtual` por cada variación; con tres
  editores de estructura distinta (uno sin matriz) degenera en ramas por sistema dentro de la base —
  exactamente lo que el criterio 3 prohíbe. Además `RackDialogWindow` nació para diálogos, no para
  editores ricos.
- **Estilos compartidos solamente (ampliar `AppStyles.xaml` sin shell).** Descartada: unifica color y
  tipografía pero no la **estructura**, que es lo que el Owner rechazó en PB-VAL-01 (matriz
  relegada, preview inútil, acciones escondidas). Los 43 brushes duplicados demuestran que un
  diccionario disponible no se adopta solo.
- **Framework MVVM con `DataTemplate` por sistema y una ventana única.** Descartada por ahora: exige
  reescribir los tres code-behind (5 569 líneas) de una vez, contra el principio 2 del ROADMAP
  («nada de big-bang») y el 3 («strangler»). El shell por slots no la impide: es un paso hacia ella.
- **No hacer nada y que cada editor copie al dinámico.** Descartada: es el statu quo que produjo el
  rechazo del gate de I-18 y tres rondas correctivas.

## Consecuencias

- **Positivas**: una sola definición de estructura, tokens, severidades y categorías de acción; el
  editor N+1 hereda coherencia sin copiar; el gate visual del Owner pasa a ser verificable contra un
  contrato escrito; las pruebas estructurales del shell cubren a todos sus adoptantes; **habilita**
  (sin forzar) una adopción posterior de los controles comunes de I-14.
- **Negativas / costos aceptados**: se migran editores grandes con riesgo de regresión visual
  (mitigado con pruebas estructurales, goldens intactos y validación del Owner sobre el DLL Debug
  del SHA exacto); la composición por slots es más verbosa que la herencia; queda **deuda temporal
  de coherencia**: entre I-30 e I-31, Selectivo y Push Back siguen fuera del shell; y los controles
  de captura del dinámico siguen siendo los actuales (I-14 no se adopta en I-30).
- **A vigilar**: que ningún `RackSystemKind` ni rama por sistema se filtre al shell (verificable por
  prueba/guard); que adoptar el shell **no** derive en una sustitución no escalada de los controles
  de captura del dinámico; que la migración de Push Back no se adelante a I-31; que los tokens no se
  conviertan en un segundo lugar donde vivan colores junto a `PreviewPalette`.

## Referencias

- Contrato: [`docs/initiatives/I-30-editor-visual-shell.md`](../initiatives/I-30-editor-visual-shell.md)
- ROADMAP Fase 5, filas I-30 e I-31 y secuencia `I-30 → I-31 → reanudación de I-18`
- `docs/ARCHITECTURE.md` §7.3 (Editor Shell objetivo)
- I-14 (controles), I-15 (`RackEditorSession`), I-20/I-21 (estados puros), I-24 (pruebas de editores)
- I-18 `feature/push-back`: hallazgo **PB-VAL-01** del gate manual del Owner y sus tres rondas
  correctivas (leído en solo lectura desde `origin/feature/push-back` = `b2d9e9d`)
- Criterios de creación: [`docs/adr/README.md`](README.md) §«Cuándo crear un ADR», puntos 1 y 2
