# I-37D ronda 4 — las catorce regresiones, verificadas EN ROJO

Estado: **las catorce muerden**. Cada defecto se reintrodujo a propósito en el código de producción, se
comprobó que la suite fallaba, y se revirtió. El árbol quedó limpio y las dos suites volvieron a verde:
**2966** y **619**.

La lista la fijó el dueño al encargar la ronda. No es una selección mía de lo que resultó fácil de probar.

---

## Frente A — el adaptador

| # | Defecto reintroducido | Parche | Resultado |
|---|---|---|---|
| 01 | Volver a forzar `ΔY = 0` | el término del ala del tensor multiplicado por cero en `TryResolve` | **ROJO** · 3 fallos |
| 02 | Usar `CutLength / 2` | `HoleOffsetPerAxis` devuelve `1.0` | **ROJO** · 5 fallos |
| 03 | Dibujar una L plana manual | `AngleSectionGeometryBuilder` fuerza `BuildSharp`, o sea la L a escuadra de seis vértices | **ROJO** · 7 fallos |
| 04 | Agujeros en la misma ala | el agujero de varilla se queda en el plano medio del ala **apoyada** | **ROJO** · 3 fallos |
| 05 | Misma orientación en los cuatro extremos | `alongSeatedLeg` fijado a `(1, 0, 0)` en vez de derivarse de la diagonal | **ROJO** · 7 fallos |
| 06 | Tensor conservando la longitud anterior | los extremos vuelven a `bolt ± dirección × 1.0` en vez de salir de los agujeros | **ROJO** · 3 fallos |
| 07 | BOM que no cambia ante un cambio real de longitud | el mismo parche que 06, mirando **sólo** el pin del BOM | **ROJO** · el pin `bom` muerde |

**Por qué 06 y 07 comparten parche.** Son dos preguntas distintas sobre el mismo cambio: 06 pregunta si
alguien nota que el tensor volvió a medir lo de antes, y 07 si el BOM **sigue** a la longitud o se quedó
anclado a un número propio. Verificar 07 con el parche de 06 y comprobar que el pin del BOM se pone rojo es
exactamente la prueba de que el BOM **no** tiene una idea paralela de cuánto mide una varilla.

**01 y 04 también se solapan**, y conviene decirlo en vez de disimularlo: con este marco el ala del tensor
corre según ±Y, así que «poner los dos agujeros en la misma ala» y «forzar `ΔY = 0`» son geométricamente el
mismo defecto visto desde dos sitios. Se verificaron los dos parches por separado y los dos muerden.

---

## Frente B — los paneles

| # | Defecto reintroducido | Parche | Resultado |
|---|---|---|---|
| 08 | Paneles persistidos sólo como cantidad | `[JsonIgnore]` sobre `AdvancedPanelSegments` | **ROJO** · 3 fallos |
| 09 | Hueco implícito entre segmentos | la validación deja pasar los huecos y sólo mira solapes | **ROJO** · 1 fallo |
| 10 | Separador duplicado en frontera | `SeparatorElevationsOf` deja de deduplicar | **ROJO** · 7 fallos |
| 11 | Tensor generado en segmento `None` | el derivador trata **todos** los tramos como arriostrados | **ROJO** · 6 fallos |
| 12 | Omitir segmentos avanzados del JSON | `DeepCopy` devuelve siempre la lista vacía | **ROJO** · 5 fallos |
| 13 | Volver a automático sin reemplazar la lista | `RestoreAutomatic` devuelve `Ok()` en vez de `Warned(...)` | **ROJO** · 1 en core **y** 1 en UI |
| 14 | Preview y materialización divergentes | `MaterializeAutomatic` se salta el primer tramo | **ROJO** · 2 fallos |

**Nota sobre la 10.** Siete fallos, y no uno, porque quitar la deduplicación rompe además la comprobación
interna del derivador: con tramos contiguos hay exactamente **una frontera más que tramos**, y esa guarda
salta antes de que ningún dibujo llegue a salir mal. Es la señal de que la regla de fronteras únicas está
comprobada en dos sitios y no sólo en una prueba.

**Nota sobre la 13.** Es la única que se verificó en **las dos suites**: el aviso vive en Application —lo
marca `ReplacesManualWork`— y la ventana lo consume por su costura. Que las dos se pongan rojas confirma que
la decisión de avisar no está duplicada en la UI.

---

## Lo que el ejercicio encontró

**Un error mío, en la 13.** El primer parche que escribí para ella dejaba código inalcanzable y **no
compilaba**, así que el «rojo» que reportó el arnés era un fallo de compilación y no una prueba mordiendo.
Un defecto que no compila no demuestra nada: la pregunta era si alguna prueba se da cuenta de que el aviso
desapareció, y un proyecto que no compila no responde a eso. Se rehízo el parche sustituyendo el `return`
entero, y ahí sí: rojo en core y en UI, por la razón correcta.

**Ninguna otra salió verde en la primera pasada.** A diferencia de la ronda de corrección —donde cinco
pasaron y cuatro eran problemas de las pruebas— aquí las trece restantes mordieron directamente.

---

## Cómo se ejecutó

Cada defecto se aplicó sobre el archivo real, se corrió la suite filtrada, y el archivo se restauró desde su
copia **antes** de pasar al siguiente. Al terminar:

- `git status` limpio;
- `RackCad.Tests` **2966/2966**;
- `RackCad.UI.Tests` **619/619**.

Dos archivos quedaron marcados como modificados al final por el reescrito de finales de línea —`git diff`
vacío, o sea sin diferencia de contenido— y se restauraron con `git checkout` antes de continuar.
