# I-39B — Decisiones técnicas sobre el alcance interno restante

> Conclusiones **medidas**, no diferimientos. Ninguna abre una iniciativa nueva. Donde la conclusión es
> «no se hace», va acompañada de la diferencia funcional concreta que lo impide y de qué parte del
> contrato **sí** queda cumplida sin ello.

## B. `EditorAction`, `EditorActionBar`, `EditorStatusPresenter`

### Qué necesitaba cada consumidor, y qué sabe expresar la infraestructura

| Necesidad aparecida en I-39B | ¿La expresa hoy? | Resultado |
|---|---|---|
| Motivo visible de acción bloqueada (Dinámico, Cama) | **Sí** — `EditorAction.DisabledReason` + `ToolTipService.ShowOnDisabled` | ver abajo |
| Escala de severidad Info/Success/**Warning**/Error (Cantilever) | **Sí** — `EditorStatusSeverity` + `EditorStatusPalette` sobre los tokens `ShellStatus*Brush` | **ADOPTADA** |
| Acción **por defecto** (Enter) | **No** | bloqueante |
| Acción de **cancelación** (Escape) | **No** | bloqueante |
| Si la acción **cierra** la ventana | **No** | bloqueante |

### Decisión 1 — `EditorStatusPalette` **se adopta**, y ya tiene consumidor productivo

La severidad de Cantilever es exactamente el caso que el booleano de `UiSupport.SetStatus` no sabía
expresar: un diagnóstico **no bloqueante** —la línea sí resolvió— se pintaba con el rojo de error.

`UiSupport.SetStatus` gana una sobrecarga que toma `EditorStatusSeverity` y resuelve el color por
`EditorStatusPalette`, es decir por los tokens `ShellStatus*Brush`. **No es una paleta nueva**: es la
del shell, que hasta ahora no tenía ningún consumidor productivo. La sobrecarga booleana se conserva
porque para la mayoría de los mensajes la distinción real sigue siendo error/ok, y cambiarlos movería
el color de decenas de mensajes ya validados sin que ningún incumplimiento lo pida.

Con esto **deja de ser cierto** que la infraestructura de status no tenga consumidores productivos.

### Decisión 2 — `EditorAction` y `EditorActionBar` **no se adoptan en I-39B**

No por estética ni por comodidad: por una **diferencia funcional demostrada**.

`EditorActions.Button` construye un `Button` con `Content`, `IsEnabled`, `Style`, `Margin` y el tooltip
del motivo. **No fija `IsDefault` ni `IsCancel`**, y `EditorAction` no tiene forma de declararlos.
Sustituir por ella los botones de los seis editores **rompería el contrato de teclado que I-39B acaba
de fijar**: el Dinámico y la Cama perderían su acción por defecto —Enter dejaría de recalcular— y las
seis perderían la de cancelación, con lo que Escape dejaría de llegar a `OnClosing` y la política de
cierre que esta misma subiniciativa implementa quedaría inalcanzable por teclado.

Evolucionar `EditorAction` para expresar «por defecto», «cancelación» y «cierra/no cierra» es posible y
neutral, pero **su único beneficiario sería una migración cosmética**: los motivos de bloqueo que
I-39B necesitaba ya se resuelven con `ToolTipService.ShowOnDisabled` + `ToolTip`, que es lo que el
Dinámico y la Cama usan y lo que `EditorActions.Button` haría por dentro. Añadir tres propiedades para
después reescribir la barra de acciones de seis ventanas es exactamente la migración cosmética masiva
que el contrato de I-39B prohíbe.

**Qué queda cumplido sin adoptarla:** ADR-0029 **D6** —toda acción importante deshabilitada expone su
motivo— se cumple hoy en el Dinámico, la Cama y Cantilever por el mecanismo que ya usaban las ventanas
migradas. La adopción declarativa es una refactorización de forma, no un requisito del contrato.

## C. Cama → `RackEditorVisualShell`: **no se migra**

### Lo medido

| Dimensión | Cama hoy | Shell (arquetipo A) |
|---|---|---|
| Tamaño inicial | `1080×640` | `1280×720` |
| Mínimo | `860×520` | `1120×672` |
| Fondo y tipografía | `#EEF2F6` y `Segoe UI` literales en el XAML | tokens |
| Composición | `Grid` propio de tres columnas | sidebar + matriz opcional + preview + status + 4 categorías |
| Matriz | **no tiene** | slot opcional |

### La diferencia que lo impide

Adoptar el shell impone el contrato de tamaño del arquetipo A, y su mínimo (`1120×672`) es **más
grande que el tamaño inicial completo de la Cama** (`1080×640`). No es un ajuste de píxeles: la ventana
pasaría a no poder mostrarse en el tamaño en que hoy se usa, y su contenido —que cabe holgadamente en
tres columnas sin barra lateral ni matriz— tendría que redistribuirse en zonas pensadas para un editor
con matriz.

Eso es una **remodelación visual fuera del objetivo funcional de I-39B**, que es el comportamiento:
teclado, cierre, dirty, acciones, preview y estado. Ninguno de esos siete depende de estar sobre el
shell.

### Qué contrato cumple la Cama **sin** el shell

`CenterOwner`; foco inicial declarado; `IsCancel` en Cerrar; `IsDefault` en «Actualizar vista», que es
un recálculo y por tanto una acción por defecto **segura**; ningún camino de cierre materializa; sin
ámbito transaccional declarado, luego cierra directo, que D8 admite como «no aplicable»; e **Insertar
bloqueado con su motivo** cuando no hay modelo, que es lo que I-39B añadió.

**Desviación explícita registrada** frente a ADR-0019 (migración progresiva al shell) y ADR-0029 D9
(contrato de tamaño por arquetipo): la Cama conserva su composición y su contrato de tamaño propios.
No se reabre ningún ADR: ADR-0019 fija la migración como **progresiva**, y ADR-0029 D9 dice
expresamente que un arquetipo **no hereda** las restricciones de tamaño de otro — que es justamente el
argumento por el que forzar aquí los mínimos del editor rico sería incorrecto.

## D. Cabecera → `RackEditorVisualShell`: **no se migra**

### Lo medido, además de lo de la Cama

- Tamaño `1320×820`, mínimo `1160×560`: su **mínimo de alto es menor** que el del arquetipo A (`672`),
  de modo que adoptarlo reduciría el rango de uso de la ventana más grande del repositorio.
- **Layout de paneles persistido**: la ventana guarda y restaura la posición de sus columnas
  (`ApplySavedLayout` al abrir, `SaveCurrentLayout` en `OnClosed`) y ofrece «Restablecer paneles». Las
  columnas redimensionables **son** su composición; el shell impone una barra lateral de ancho
  tokenizado y un área de trabajo fija.
- **Diez estilos propios** declarados en el XAML raíz.
- Un árbol de modelo, una rejilla de propiedades y un preview conviven en tres zonas que no se
  corresponden con los slots del shell.

### La diferencia que lo impide

Migrar exigiría **retirar o reinterpretar el layout persistido**, que es una autoridad de producto de
la ventana —el usuario coloca sus paneles y espera encontrarlos igual— y una funcionalidad que el shell
no ofrece. No es una diferencia visual aceptable: es la pérdida de una capacidad.

### Qué contrato cumple la Cabecera **sin** el shell

`CenterOwner`; **foco inicial declarado** (añadido por I-39B, apuntando al árbol del modelo, que no es
una acción destructiva ni bloqueable); `IsCancel` en Cerrar; **política de cierre** que consulta su
ámbito real (`HasUnsavedManualEdits`) por las cuatro rutas reutilizando `ConfirmDiscard`; ningún camino
de cierre materializa; motivos de bloqueo en sus acciones deshabilitables, que ya era la mejor cobertura
de D6 del repositorio.

**Fuera de alcance por exclusión previa y expresa:** su contrato de inserción paralelo
(`InsertRequested`/`InsertView`/`UpdateOnly` en vez de `RackEditorSession`). ADR-0029 no autoriza tocar
persistencia ni identidad, y proteger el cierre **no lo requirió**: `OnClosing` distingue el cierre por
inserción leyendo `InsertRequested`, sin modificarlo.

**Desviación explícita registrada**, misma base que la de la Cama.
