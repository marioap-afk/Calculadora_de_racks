# I-37D — Paquete de validación manual en AutoCAD 2025 · **RONDA 2**

Estado: **PENDIENTE DE EJECUTAR**. La ronda 1 quedó **RECHAZADA**
(`OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_1`) y su paquete se conserva sin reescribir en
[`I-37D-autocad-validation.md`](I-37D-autocad-validation.md).

Esta ronda **reestructuró el producto** antes de volver a mirar cada componente en detalle. Un solo fallo
rechaza la ronda.

## 1. Identificación

| Campo | Valor |
|---|---|
| Iniciativa | I-37D — Cantilever MVP final |
| Rama | `feature/cantilever-mvp-final` |
| **CODE_SHA funcional** | `5142e1b` — última punta que tocó `src/**` o `tests/**` |
| **VALIDATED_BUILD_SHA** | `5142e1ba1b73a332f3df9960df05a40df59b3f2d` |
| DLL Debug a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| **DLL SHA-256** | `BFF6135E988A510D314FC8EBD65FE2692D257714F7465C57E41A5BFE24AD4CCD` |
| `AssemblyInformationalVersion` | `1.0.0+5142e1ba1b73a332f3df9960df05a40df59b3f2d` |
| Tamaño / fecha | 135 168 bytes · `2026-07-30 17:45:23` |
| **Bundle** | ⏸ **NO regenerado en esta corrida**: `deploy\build-bundle.ps1` aborta con AutoCAD abierto y la sesión de AutoCAD del dueño lo estaba. El comando exacto va en el paso 2; hay que cerrarlo para validar de todos modos. El inventario de la ronda anterior queda OBSOLETO al regenerarlo. |
| Suites | `RackCad.Tests` 2788/2788 · `RackCad.UI.Tests` 605/605 |
| Regresiones | 12/12 verificadas **en rojo** — [evidencia](I-37D-round-2-regressions.md) |
| Guardas de fuente nuevas | 37 |

> **Recompilar cambia el SHA-256** aunque el código no cambie: la `AssemblyInformationalVersion` incrusta
> la punta de git. Anota el nuevo antes de cargar.

## 2. Preparar

1. **Cierra AutoCAD** por completo (bloquea `RackCad.Plugin.dll`).
2. Desde la raíz del worktree:

```powershell
git status; git rev-parse HEAD
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
pwsh deploy\build-bundle.ps1 -Configuration Debug -InventoryOutPath docs\automation\evidence\I-37D-bundle-inventory.txt
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

3. Abre AutoCAD 2025 con un dibujo **nuevo y descartable**, `NETLOAD`, y selecciona ese DLL exacto.

## 3. Qué cambió respecto de la ronda 1

Los seis motivos del rechazo, y lo que se hizo con cada uno:

| # | Motivo | Qué cambió |
|---|---|---|
| 1 | Ventana saturada | La principal edita **sólo la línea**: nombre, GUID, estaciones, separación, topología compartida y distribución del arriostramiento. |
| 2 | Mezcla línea/componentes | Los componentes salieron a **cuatro subventanas**. Aquí sólo quedan cuatro **tarjetas** con resumen y botón. |
| 3 | Perfiles difíciles de elegir | **Selector buscable** con filtro de familia y lista virtualizada, en los cuatro configuradores. |
| 4 | La base no seguía a la columna | `BaseFollowsColumn`, con sus siete reglas, **persistido**. |
| 5 | Faltaban troqueles y placas | Se dibujan **todos** los agujeros resueltos: círculo real de frente, traza de canto. |
| 6 | La arquitectura no reflejaba el flujo | Línea → tarjeta → configurador → aceptar. Un shell común para los cuatro. |

**Sigue fuera de alcance** (no es defecto): cálculo resistente, cargas, capacidad, peso, costo,
soldaduras, tornillos, anclas, roscas, tolerancias, CNC, planos de taller y la interferencia física en el
cruce de tensores.

**Decisiones que no son defecto:** la varilla cold rolled se dibuja como su **eje** y su adaptador como el
cuadrado de su corte; la lateral de una estación **no** muestra el arriostramiento; el juego inicial son
tres vistas y no N laterales; una línea nueva **no se resuelve** hasta que se eligen las secciones.

---

## Bloque A — Arquitectura de la ventana principal

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| A1 | `RACKCANTILEVER` (o el botón del menú) | Abre el editor de línea | ☐ |
| A2 | Recorre la barra lateral de arriba abajo | **Sólo** hay línea, topología compartida y arriostramiento. **No** hay secciones, espesores, troqueles ni parámetros de brazo | ☐ |
| A3 | Busca las tarjetas de componente | Cuatro: **Columna y base · Brazo · Separador · Tensor**, cada una con un resumen corto y un botón «Configurar» | ☐ |
| A4 | Lee los resúmenes | Compactos, tipo `W10X33 · base W10X33 · sigue a la columna`. El id largo sólo en el tooltip | ☐ |
| A5 | Mira la vista previa | Ocupa un área amplia; la barra lateral no la ahoga | ☐ |
| A6 | Mira la matriz | Está separada, y **debajo no hay un formulario de brazo**: sólo el resumen de la celda y «Editar brazo de la celda» | ☐ |
| A7 | Mira la barra de acciones | Lo que **no** dibuja (Actualizar · BOM · Guardar) separado de lo que **sí** (Insertar frontal/lateral/planta) y de Cerrar | ☐ |
| A8 | Abre una línea nueva | El estado dice qué falta; los botones de insertar están deshabilitados | ☐ |

## Bloque B — Selector estructural

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| B1 | Abre «Columna y base» y escribe `W10X33` | La lista se reduce y aparece esa sección | ☐ |
| B2 | Escribe `AISC-W-W10X33` | La encuentra por su id completo | ☐ |
| B3 | Escribe `10X33` | La encuentra por un fragmento | ☐ |
| B4 | Escribe `w 10x33` con espacios y minúsculas | La encuentra igual | ☐ |
| B5 | Usa el filtro de familia | Sólo ofrece las familias que ESE selector admite (columna: W) | ☐ |
| B6 | Desde la caja, flechas ↓ y ↑ | Recorren la lista sin soltar el teclado | ☐ |
| B7 | Escribe algo que no existe | Dice «Ninguna sección coincide», no se queda en blanco | ☐ |
| B8 | Elige una fila y vuelve a filtrar de modo que desaparezca | La sección **sigue elegida**; sólo deja de estar marcada | ☐ |
| B9 | Mira el tooltip de una fila | Muestra el id exacto | ☐ |

## Bloque C — Columna y base

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| C1 | Abre el configurador con una línea nueva | La casilla «La base sigue a la columna» está **marcada** | ☐ |
| C2 | Elige una columna W | La base toma **la misma** sección | ☐ |
| C3 | Cambia la columna a otra W | La base la sigue | ☐ |
| C4 | Elige una base distinta a mano | La casilla se **desmarca** | ☐ |
| C5 | Cambia la columna otra vez | La base **no** cambia | ☐ |
| C6 | Pulsa «Usar misma sección» | Vuelve a marcarse y la base se iguala **en el acto** | ☐ |
| C7 | Elige como columna una sección no elegible como base (un canal) | La base **no** se toca y aparece el aviso «no es elegible como base» **en esta ventana** | ☐ |
| C8 | Revisa los parámetros | Están todos: placa inferior, base y sus tres placas, y los nueve de troquel | ☐ |
| C9 | Mira el preview frontal | Columna, base, placas, cartabón **y los troqueles** | ☐ |
| C10 | Cuenta los troqueles regulares de la columna | Se ven todos, como círculos | ☐ |
| C11 | Cambia a lateral y a planta | Las tres vistas dibujan la pieza | ☐ |
| C12 | Cambia varios valores y pulsa «Restaurar» | Vuelve a como se abrió | ☐ |
| C13 | Cambia valores y pulsa **Cancelar** | La tarjeta de la ventana principal **no** cambió | ☐ |
| C14 | Repite y pulsa **Aceptar** | La tarjeta y el preview de la línea reflejan el cambio | ☐ |
| C15 | Guarda la línea, ciérrala y reábrela | La casilla del *follow* conserva su estado | ☐ |
| C16 | Con la línea en **doble**, mira la lateral | Dos bases espejadas, cada una con sus placas y troqueles | ☐ |

## Bloque D — Componente independiente

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| D1 | En «Columna y base», pulsa «Insertar sólo esta pieza» | Pide un punto y dibuja las **tres** vistas, separadas | ☐ |
| D2 | Compara con el preview | La misma figura | ☐ |
| D3 | Mira el nombre de los bloques | `RACKCAD_CANTILEVER_COMPONENTE_COLUMNA_BASE_…` con un id propio | ☐ |
| D4 | `RACKLISTA` | La pieza suelta **no** aparece como línea Cantilever | ☐ |
| D5 | `RACKEDITAR` sobre ella | **No** la abre: no es editable en esta ronda, y así se anuncia | ☐ |
| D6 | Inserta una segunda igual | Recibe un id **distinto** | ☐ |
| D7 | Repite y pulsa **ESC** en el punto | No queda bloque, ni definición suelta en el listado de bloques | ☐ |
| D8 | Repite desde «Brazo» | Inserta su lateral (y las otras sólo si aportan algo distinto) | ☐ |
| D9 | Repite desde «Separador» con la línea resuelta | Inserta frontal y planta | ☐ |
| D10 | Repite desde «Tensor» con la línea resuelta | Inserta **una** vista: el plano del tensor | ☐ |
| D11 | Comprueba que la línea abierta no cambió | Sus vistas siguen como estaban | ☐ |

## Bloque E — Brazo, separador y tensor

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| E1 | Abre «Brazo» | Trae **todos** sus parámetros: arreglo, sección, corte, pendiente, filas, margen, espesor de placa, tapa/tope, espesor y altura extra | ☐ |
| E2 | Cambia el arreglo a canal doble | El selector sigue funcionando y el preview responde | ☐ |
| E3 | Deja vacío el margen vertical | Lo dice por su nombre y no inventa un valor | ☐ |
| E4 | Cancela | La tarjeta no cambió | ☐ |
| E5 | Abre «Separador» | La sección por omisión es **C4X4.5** y sólo ofrece canales | ☐ |
| E6 | Con la línea resuelta, lee el derivado | Corte, cuatro agujeros y elevación | ☐ |
| E7 | Abre «Tensor» | Cold rolled con **Ø0.75 in** por omisión | ☐ |
| E8 | Cambia a estructural sin elegir sección | Lo **rechaza** y lo dice | ☐ |
| E9 | Elige un ángulo o un canal | Lo acepta; la receta nombra el perfil | ☐ |
| E10 | Vuelve a cold rolled | La receta nombra varilla, adaptadores y cartabones | ☐ |

## Bloque F — Matriz y línea

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| F1 | Selecciona una celda | Muestra un **resumen** compacto, no un formulario | ☐ |
| F2 | Alcance **Celda** → «Editar brazo» → cambia el corte → Aceptar | Sólo esa celda queda en negritas | ☐ |
| F3 | Alcance **Estación** | Cambian las celdas de esa estación y sólo ésas | ☐ |
| F4 | Alcance **Nivel** | Cambia ese nivel en todas las estaciones | ☐ |
| F5 | Alcance **Toda la línea** | Cambian todas | ☐ |
| F6 | En cada una, mira el estado | **Un** mensaje por operación, no uno por celda | ☐ |
| F7 | «Restaurar» con alcance de línea | Todas vuelven al brazo por omisión | ☐ |
| F8 | Aplica un brazo idéntico al de omisión | Dice que **ninguna** celda cambió | ☐ |
| F9 | Preview frontal, lateral y planta de la línea | Las tres dibujan, con sus troqueles | ☐ |
| F10 | Inserta frontal, lateral y planta | Tres bloques ligados al **mismo** GUID | ☐ |
| F11 | «Lista de materiales» | BOM por componentes | ☐ |
| F12 | `RACKEDITAR` sobre una vista → cambia niveles → «Actualizar» | Redibuja las tres en sitio, mismo GUID | ☐ |
| F13 | Guarda en biblioteca y reábrela | Vuelve con todo, y al insertar acuña un GUID nuevo | ☐ |

## 4. Resultado

| Campo | Valor |
|---|---|
| Fecha | *(pendiente)* |
| DLL SHA-256 realmente cargado | *(pendiente)* |
| Veredicto | ☐ APROBADA ☐ RECHAZADA |
| Observaciones | *(pendiente)* |

Si es **RECHAZADA**, anota el número de cada punto y qué se vio. El historial de rondas vive en
`docs/automation/state/I-37D.yml` y **no se reescribe**.

## 5. Lo que este paquete no decide

- **ADR-0027 y ADR-0028 siguen PROPUESTOS.**
- La **edición independiente** de un componente suelto no existe en esta ronda, y es una decisión, no un
  olvido: exigiría un `RackSystemKind` nuevo o un handler de edición, y ninguno estaba autorizado. El bloque
  se inserta identificado y **no editable**, siguiendo el precedente de `RACKSECCION`.
- `DefaultSeparatorSectionId = AISC-C-C4X4_5` y el cold rolled de `0.75 in` son decisiones **cerradas**.
