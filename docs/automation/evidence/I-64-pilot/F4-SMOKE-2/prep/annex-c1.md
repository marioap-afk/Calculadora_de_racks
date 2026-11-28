# Smoke-2: anexo de reanudación tras F4-T4-HOST-C1

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<carpeta-del-DLL-de-C1>`, `<carpeta-del-DLL-de-C2>`, `<dotnet-del-usuario>`). El resto no cambia.

La guía v3 no se modifica (orden del Owner). Este anexo dice solo **qué cambia** para la reanudación: dos rutas y unos esperados. Todo lo demás de la guía sigue igual.

## 1. Dos rutas que cambian

| Donde la guía v3 dice | Usa en su lugar |
|---|---|
| `<carpeta-de-copias-de-Smoke-2>\` (las copias de trabajo, p. ej. `<carpeta-de-copias-de-Smoke-2>\D1-b01.dwg`) | `<carpeta-de-copias-de-Smoke-2>\r2\` (p. ej. `<carpeta-de-copias-de-Smoke-2>\r2\D1-b01.dwg`): copias nuevas, con los mismos nombres |
| `<carpeta-del-DLL-de-Smoke-2>\RackCad.Plugin.dll` | `<carpeta-del-DLL-de-C1>\RackCad.Plugin.dll` |

**En el paso 0.3, el DLL nuevo:**
- InformationalVersion `1.0.0+e67fb511961ad75858eead81991bfb0f72c19feb`, el commit documental del cierre de C1, cuyo `src/` y `tests/` son idénticos al producto `b86c3cfe`;
- SHA-256 `7CD9E5D717532E483AB7B41896FE740742DB87F3D6E0E8B34FBF0BADC33E2BDF`.

Este SHA es el que la sesión comprueba en el paso 0.6 (la lista de módulos de `acad.exe`), en lugar del `8BB8E7E5…`. En el paso 0.5, la ruta que se escribe en la ventana de archivos es `<carpeta-del-DLL-de-C1>\RackCad.Plugin.dll`.

**Las copias de la corrida 1** (`<carpeta-de-copias-de-Smoke-2>\*.dwg`) y el DLL anterior (`<carpeta-del-DLL-de-Smoke-2>\`) se conservan sin tocar. **No los uses.**

En `<carpeta-de-copias-de-Smoke-2>\r2\` hay además `D1-b03-rep.dwg`, una copia de repuesto por si hace falta repetir el bloque 3.

## 2. Qué corrige el DLL nuevo

- **F4-E72:** «Actualizar» ya conserva el nombre del rack en las vistas redibujadas.
- **F4-S2-01:** las lecturas del propio panel («Editar», «Comprobar ahora» y la relectura tras «Actualizar») ya no marcan el borrador como `(posiblemente obsoleto)`. La causa eran los eventos de la red asociativa de AutoCAD, que llegan dentro de esas lecturas.
- **F4-S2-02:** era consecuencia de S2-01. El Retroceso en la caja debe borrar a la primera.

## 3. Qué NO cambia

- **Una orden de AutoCAD sigue marcando.** Después de `REDRAW`, `LINE`, `U`, `REDO`, RACKEDITAR, `DBMOD` u otra orden, los borradores siguen añadiendo ` (posiblemente obsoleto)`. La regla de la línea 27 de la guía sigue vigente.
- **F4-S2-03 sigue igual.** Tras `U`, la pantalla no se refresca hasta un `REGEN`, y un `REGEN` entre `U` y `REDO` deja sin Rehacer.
  - Es una limitación declarada que espera tu decisión.
  - En el paso 16 se anota «OBSERVADO (S2-03, limitación declarada)» y Smoke-2 continúa.
- **El rendimiento:** «Actualizar» tarda unos 14 s en D1, con la misma mecánica que el redibujo clásico.

## 4. Esperados que cambian

| Paso | Antes (v3) | Con el DLL nuevo |
|---|---|---|
| Regla R13 (la segunda, líneas 73-74) | El nombre perdido en el paso 14 = «FAIL (F4-E72)», conocido | **Retirada.** Si el nombre se pierde, es un FAIL real (R10). |
| 14 (líneas 416-419) | El nombre puede desaparecer | Cada vista conserva **el nombre del rack («PB G»)**, los niveles y las cotas. |
| 14 (línea 420) | `Limpio`, sin la marca | Igual. Ahora debe cumplirse: en la corrida 1 quedó `Limpio (posiblemente obsoleto)` por S2-01. |
| «Editar» en un borrador limpio, sin ninguna orden de por medio | En la corrida 1, volvía a `Limpio (posiblemente obsoleto)` en menos de un segundo | Queda `Listo para editar` y no vuelve a marcarse. |
| «Comprobar ahora» sin ninguna orden de por medio | En la corrida 1, `Limpio` un instante y luego `Limpio (posiblemente obsoleto)` | Queda `Limpio`. |
| Retroceso en la caja recién abierta | En la corrida 1, no borraba a la primera | Borra a la primera. |

## 5. Puntos a observar (nuevos)

- el rótulo del nombre en **todas** las vistas tras «Actualizar»;
- que «Listo para editar» y «Limpio» se mantienen estables tras «Editar» y «Comprobar ahora»;
- en el bloque de cierre (`CLOSE`, `CLOSEALL` y `QUIT` sobre D1), lo que la guía ya pide observar. Si algo sale distinto, anótalo como OBSERVADO.

## 6. Cómo se reanuda

- **Desde el bloque 1**, porque el producto cambió.
- Antes de empezar, la sesión repite las comprobaciones del día: el DLL y su SHA, los originales intactos, que no haya acad abierto y el aviso a I-52.
