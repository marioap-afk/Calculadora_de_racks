# I-39C — Checklist de validación manual en AutoCAD 2025 (Owner)

> Estado: **PENDIENTE**. Contrato:
> [`../../initiatives/I-39C-adopcion-editores-acotados.md`](../../initiatives/I-39C-adopcion-editores-acotados.md) ·
> Decisiones técnicas: [`I-39C-decisiones-tecnicas.md`](I-39C-decisiones-tecnicas.md) ·
> Base vs contrato: [`I-39C-caracterizacion-base-vs-contrato.md`](I-39C-caracterizacion-base-vs-contrato.md) ·
> Estado: [`../state/I-39C.yml`](../state/I-39C.yml)

## Artefacto a validar

| Campo | Valor |
|---|---|
| Iniciativa | I-39C — Adopción del contrato funcional común en los editores acotados |
| Rama | `architecture/adopcion-editores-acotados` |
| Claim-Id | `611ca9e5-4734-4615-a008-21d3e69338f8` |
| DLL a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| Worktree | `.claude\worktrees\architecture-adopcion-editores-acotados` |
| SHA candidato | *(se rellena al ejecutar)* |

**El DLL es el del worktree de la iniciativa, no el del principal.** Cierra AutoCAD antes de recompilar:
AutoCAD bloquea el DLL cargado.

## Qué cambió, y qué no

**Cambió, y es lo que hay que validar:**

1. **Tamaño de cinco ventanas.** Las cuatro de componente Cantilever abrían en `1120×700` y `1120×672`
   —que **no** era lo que declaraban— y ahora abren en `1040×680`, con mínimo `820×520`. El Larguero pasa
   de `720×440` al mismo contrato del arquetipo.
2. **El Larguero se compone sobre el shell**: cabecera, parámetros a la izquierda, preview a la derecha,
   estado y acciones abajo. Mismos campos, mismos botones, mismo dibujo.
3. **`Insertar` se apaga cuando no puede producir nada**, con el motivo en su ayuda emergente: en el
   separador y el tensor sin línea resuelta, en la columna-base y el brazo cuando la pieza no resuelve, y
   en el inspector de secciones sin selección. `Aceptar` **no** se apaga: una pieza bloqueada sigue siendo
   una intención que puedes conservar.
4. **Foco inicial declarado** en las seis, cada una en su primer control de captura.
5. **Los dos botones del inspector** usan ya los estilos compartidos (primario y secundario).

**No cambió**: geometría, BOM, persistencia, identidad, catálogos, reglas de producto, el contenido de
ninguna ventana ni lo que se dibuja en el DWG.

## Checklist

### A. Las cuatro ventanas de componente Cantilever (`RACKCANTILEVER` → configuradores)

| # | Punto | Resultado |
|---|---|---|
| 1 | Columna y base abre y su tamaño es cómodo; nada aparece recortado ni pegado a los bordes | |
| 2 | Brazo abre igual | |
| 3 | Separador abre igual | |
| 4 | Tensor abre igual | |
| 5 | Al reducir cualquiera de las cuatro hasta su tamaño mínimo, siguen viéndose **enteros** el diagnóstico, la receta y los cuatro botones | |
| 6 | El preview sigue dibujando lo mismo que antes en las tres vistas | |
| 7 | La receta (lista de materiales de la pieza) sigue diciendo lo mismo | |
| 8 | Cambiar secciones y parámetros sigue recalculando preview, diagnóstico y receta | |
| 9 | `Restaurar` vuelve a los valores con que se abrió la ventana | |
| 10 | `Aceptar` devuelve la pieza editada a la línea, como antes | |
| 11 | `Cancelar` y `Escape` cierran sin aplicar, como antes | |
| 12 | Al abrir, el cursor está en el primer campo de captura y **no** en un botón | |

### B. `Insertar sólo esta pieza`

| # | Punto | Resultado |
|---|---|---|
| 13 | En columna-base y brazo, con la pieza resuelta, `Insertar` está **activo** e inserta como antes | |
| 14 | En columna-base y brazo, con la pieza **sin resolver**, `Insertar` está **apagado** y al pasar el ratón dice por qué | |
| 15 | En el separador y el tensor **sin línea resuelta**, `Insertar` está **apagado** y dice que hay que resolver la línea primero | |
| 16 | En el separador y el tensor **con línea resuelta**, `Insertar` está activo e inserta como antes | |
| 17 | `Aceptar` sigue activo en todos los casos anteriores | |

### C. Larguero (menú principal `RACKCAD` → «Larguero»; no tiene comando propio)

| # | Punto | Resultado |
|---|---|---|
| 18 | La ventana abre con el nuevo aspecto y el cursor está en **Nombre** | |
| 19 | Los cinco campos están y funcionan: nombre, perfil, peralte, longitud, ménsula | |
| 20 | Elegir un perfil actualiza la lista de peraltes | |
| 21 | El esquema del preview se ve **claro y legible**, con sus rótulos | |
| 22 | Cambiar longitud redibuja el esquema; una longitud inválida lo rotula «(longitud)» | |
| 23 | `Ver lista de materiales` abre el BOM en su ventana, con el mismo contenido | |
| 24 | `Guardar en biblioteca` guarda y lo dice en la línea de estado | |
| 25 | Sin perfil o con longitud cero, guardar avisa en la línea de estado y no guarda | |
| 26 | Abrir un larguero **guardado** desde la biblioteca lo recarga con sus valores | |
| 27 | `Cerrar` y `Escape` cierran | |

### D. Inspector de secciones estructurales (`RACKSECCION`)

| # | Punto | Resultado |
|---|---|---|
| 28 | Abre con el mismo tamaño de siempre y el cursor en la caja de búsqueda | |
| 29 | Buscar, filtrar por familia y elegir una sección funcionan como antes | |
| 30 | Con una sección elegida, `Insertar` está activo e inserta como antes | |
| 31 | Con una búsqueda que no encuentra nada, `Insertar` está **apagado** y dice que elijas una sección | |
| 32 | Una longitud inválida **no** apaga `Insertar`: el campo avisa y se conserva el último valor válido | |
| 33 | `Enter` sigue disparando `Insertar` y `Escape` sigue cerrando | |
| 34 | Los dos botones se ven con el estilo del resto de la aplicación | |

### E. Regresión de la línea Cantilever completa

| # | Punto | Resultado |
|---|---|---|
| 35 | Crear una línea Cantilever de principio a fin dibuja lo mismo que antes | |
| 36 | Editar una línea existente por su GUID sigue funcionando | |
| 37 | El BOM de la línea sigue dando los mismos totales | |

## Veredicto

| Campo | Valor |
|---|---|
| Fecha | |
| Validador | |
| Resultado global | |
| Observaciones | |

**Token de aprobación**: `OWNER_APPROVED_I39C_MANUAL_VALIDATION` (solo si **todos** los puntos se cumplen).
