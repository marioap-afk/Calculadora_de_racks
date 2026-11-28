# Smoke-2: anexo de reanudación tras F4-T4-HOST-C2

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<carpeta-del-DLL-de-C1>`, `<carpeta-del-DLL-de-C2>`, `<dotnet-del-usuario>`). El resto no cambia.

La guía v3 no se modifica (orden del Owner). Este anexo se usa **junto con el anexo de C1** (`ANEXO-REANUDACION-C1.md`) y dice solo **qué cambia respecto de él**. Todo lo demás de la guía y del anexo de C1 sigue igual.

## 1. Lo que cambia del anexo de C1

| Donde el anexo de C1 dice | Usa en su lugar |
|---|---|
| `<carpeta-del-DLL-de-C1>\RackCad.Plugin.dll` (pasos 0.3 y 0.5) | `<carpeta-del-DLL-de-C2>\RackCad.Plugin.dll` |
| El SHA-256 `7CD9E5D7…` (paso 0.6) | `798C936698277F418CE47D31CAF2028F9253002DBF9DFEE16774EA923E4D4E48` |
| §3: «F4-S2-03 sigue igual» | **Retirado.** Con el DLL de C2, `U` y `REDO` refrescan la pantalla solos (ver el §3 de este anexo) |

**El DLL nuevo:** InformationalVersion `1.0.0+9f894587f4f600b34abd03931ddced2c4306f1b8`. Es el commit documental del cierre de C2, cuyo `src/` y `tests/` son idénticos al producto `9abc1320`.

**Las copias:** las mismas de `<carpeta-de-copias-de-Smoke-2>\r2\`. La sesión comprobó que siguen sin usar: las 37 coinciden con su SHA-256 de `copias.json`, y los 7 originales siguen intactos.

**El DLL de C1** (`<carpeta-del-DLL-de-C1>\`) se conserva sin tocar. **No lo uses.**

## 2. Qué corrige el DLL de C2 (además de lo que corrige el de C1)

**F4-S2-03.** Tras «Actualizar» del panel, o tras el «Actualizar» de la ventana clásica de RACKEDITAR, `U` deshacía el dibujo pero la pantalla seguía mostrando el redibujo hasta un `REGEN`. Ahora `U` y `REDO` vuelven a dibujar las vistas solos.

**Qué NO corrige:**
- **Cantilever**, que tiene su propio redibujo: tras `U`, sigue sin refrescarse hasta `REGEN` (decisión del Owner);
- **el rendimiento** del «Actualizar», que no cambia.

## 3. Esperados que cambian

| Paso | Con el DLL de C1 | Con el DLL de C2 |
|---|---|---|
| 14 | `Limpio`, sin la marca | Igual, y **estable**: cuenta hasta 5 sin tocar nada y vuelve a mirar el estado |
| 16 (`U` tras «Actualizar») | La pantalla no se refresca hasta `REGEN` («OBSERVADO, S2-03») | **La pantalla vuelve sola** al dibujo anterior, sin `REGEN`. Si no vuelve, es **FAIL** |
| 17 (`REDO`) | «Everything has been redone» | Igual, y **la pantalla vuelve sola** al dibujo actualizado |
| 26 (`U` sin `REDO`) | Las vistas vuelven al valor anterior, tras `REGEN` | Igual, y **sin `REGEN`** |
| 58 (RACKEDITAR clásico) | Todas las vistas se redibujan como siempre | Igual. Haz además el punto 1b del §4 |

## 4. Puntos nuevos para observar

**1. Rehacer, con una muestra pequeña.** Cada `U` va seguido de su `REDO`, así que la copia queda como estaba.
- **1a. En el panel**, al terminar el paso 19:
  - haz 3 veces seguidas `U` y después `REDO`, sin tocar el panel entre `U` y `REDO`;
  - anota cada respuesta de `REDO` y si la pantalla vuelve sola cada vez.
- **1b. En el clásico**, al terminar el paso 58:
  - lo mismo, 3 veces `U` y después `REDO`;
  - anota lo mismo.
- **Si sale «Nothing to redo»:**
  - anota qué hiciste entre ese `U` y ese `REDO`;
  - sigue con Smoke-2;
  - la sesión repite esa misma secuencia con el DLL de C1. Si con el de C1 rehace y con el de C2 no, vuelve a ti como decisión: no se anota solo como OBSERVADO.

**2. Selección tras `U`.** En el paso 26, después de su `U` (que va sin `REDO`, a propósito):
- haz una ventana de selección que cubra todo el rack A;
- anota si queda seleccionado entero;
- pulsa Esc para quitar la selección.

No la hagas entre el `U` y el `REDO` de los pasos 16 y 17: seleccionar puede hacer que el panel lea, y una lectura del panel vacía el Rehacer (O-18).

**3. Si el dibujo lo tiene** (si no, NO DISPONIBLE):
- **un rack con copias enlazadas:** actualízalo y comprueba que las copias cambian y que `U` las devuelve, también en pantalla;
- **un rack de otro sistema** (Selectivo, Dinámico…): RACKEDITAR, «Actualizar» de la ventana clásica y `U`. Anota si la pantalla vuelve sola, como OBSERVADO.

**4. Cantilever**, si el dibujo lo tiene: RACKEDITAR, «Actualizar» y `U`. Lo esperado es que la pantalla **NO** se refresque, como hoy (es la limitación declarada). Si no hay Cantilever: NO DISPONIBLE.

## 5. Cómo se reanuda

- **Desde el bloque 1**, porque el producto cambió.
- Antes de empezar, la sesión repite las comprobaciones del día:
  - el DLL de C2 y su SHA;
  - las copias de `<carpeta-de-copias-de-Smoke-2>\r2\` y los originales intactos;
  - que no haya ningún AutoCAD abierto;
  - el aviso de inicio a I-52.
