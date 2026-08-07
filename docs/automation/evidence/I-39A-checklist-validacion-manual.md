# I-39A — Checklist de validación manual en AutoCAD 2025 (Owner)

> Estado: **PENDIENTE**. Este documento describe qué validar y sobre qué binario; **no** contiene ningún
> resultado. El veredicto lo escribe el Owner.
>
> Contrato: [`../../initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md`](../../initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md) ·
> ADR: [`../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md) ·
> Decisiones: [`../decisions/I-39.md`](../decisions/I-39.md) · Estado: [`../state/I-39A.yml`](../state/I-39A.yml)

## Artefacto a validar

| Campo | Valor |
|---|---|
| Iniciativa | I-39A — Fundación del contrato funcional y piloto de editor acotado |
| Rama | `architecture/contrato-funcional-ventanas-wpf` |
| Claim-Id | `fa57f5d5-197c-4b68-9b24-4d481cb15933` |
| SHA candidato | `d1149e5` (último commit de código; los posteriores son documentales y no cambian el binario) |
| DLL a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| Worktree | `.claude\worktrees\architecture-contrato-funcional-ventanas-wpf` |

**El DLL es el del worktree de la iniciativa, no el del worktree principal.** La ruta de `CLAUDE.md`
apunta al principal y no sirve para validar una rama. **Cerrar AutoCAD antes de cada recompilación**:
con el plugin cargado el DLL queda bloqueado.

## Qué cambió y qué NO cambió

**Cambió la composición de una ventana**: `StructuralSectionInspectorWindow` —la que abren `RACKSECCION`
y el botón del menú principal— pasa a componerse sobre el shell común del arquetipo B. Sus controles son
**los mismos objetos, con los mismos handlers y en el mismo orden**; lo que se mueve es dónde viven los
diagnósticos y las acciones.

**Deltas visuales esperados, y por qué:**

1. **Diagnósticos y acciones cruzan la ventana** en vez de vivir solo bajo el preview. Es el objetivo de
   la migración: quedan siempre fuera del scroll de parámetros y por tanto siempre alcanzables.
2. **La columna de captura mide 330 px** en vez de 320: es la del shell compartido.
3. **Un margen exterior de 10 px** alrededor del área de trabajo, que es el espaciado de zona del shell.
4. **Fondo y tipografía** pasan a los tokens compartidos del shell.

**No cambió nada más.** Ni búsqueda, ni filtrado, ni familias, ni lista, ni longitud por defecto, ni
vista, ni detalle, ni representación, ni rotación, ni espejo, ni ejes, ni envolvente, ni la geometría, ni
el resultado que recibe el llamador, ni la cancelación. Sigue sin persistencia y sin BOM.

## Checklist

| # | Punto | Resultado |
|---|---|---|
| 1 | `RACKSECCION` abre el inspector | |
| 2 | El botón del menú principal abre el mismo inspector | |
| 3 | La lista muestra el catálogo completo y las familias son las mismas, en el mismo orden | |
| 4 | Buscar por designación filtra igual que antes; un texto sin coincidencias deja la lista vacía | |
| 5 | Longitud, vista, detalle, representación, rotación, espejo, eje y envolvente producen el mismo dibujo que antes | |
| 6 | La advertencia de geometría visual derivada sigue visible, con el mismo texto, en una sección S | |
| 7 | El preview se redibuja al cambiar entradas y al redimensionar la ventana | |
| 8 | **Insertar** materializa exactamente la sección aceptada, con el mismo mensaje en la línea de comandos | |
| 9 | **Enter** dispara Insertar y **Escape** cierra, como antes | |
| 10 | **Cerrar** y la **X** no materializan nada en el dibujo | |
| 11 | El aviso de unidades sigue apareciendo una sola vez y después del inspector | |
| 12 | **Al tamaño mínimo y al redimensionar, las acciones y el diagnóstico siguen visibles y sin recorte** | |
| 13 | Los cuatro editores de componente Cantilever (columna-base, brazo, separador, tensor) abren y funcionan igual | |
| 14 | Los cinco sistemas vigentes no muestran regresión | |

El punto **12** es el que sustituye a fijar una resolución mínima: por decisión del Owner no se fija
todavía un número, se valida la accesibilidad real.

El punto **13** es el que protege la fachada: si los cuatro configuradores Cantilever se ven o se
comportan distinto, la fachada no está haciendo su trabajo y la subiniciativa se detiene.

## Qué se decide con este veredicto

Con la aprobación de este checklist el Owner resuelve **dos** gates a la vez:

1. `owner-validation` y `autocad` de I-39A;
2. la **aceptación o rechazo de ADR-0029**, que nace `propuesto` y cuyo texto no puede aceptarse a
   ciegas: se acepta después de ver la ventana real, con el precedente de ADR-0023 en I-36D.

Si el veredicto es de rechazo, I-39A no se integra y ADR-0029 permanece `propuesto` y editable.
