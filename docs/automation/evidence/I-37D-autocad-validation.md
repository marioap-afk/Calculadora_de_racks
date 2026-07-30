# I-37D — Paquete de validación manual en AutoCAD 2025 (Owner)

Estado: **PENDIENTE DE EJECUTAR**. Este documento es el gate real de I-37D. La CI verde es necesaria y
**no suficiente**: I-37D es la primera subiniciativa de la línea I-37 que cambia interfaz y dibujo, así
que sin el veredicto manual del dueño en AutoCAD 2025 **no se integra** y **I-37 no se cierra**.

## 1. Identificación

| Campo | Valor |
|---|---|
| Iniciativa | I-37D — Cantilever MVP final: línea, arriostramiento, vistas y editor |
| Rama | `feature/cantilever-mvp-final` |
| Claim-Id | `c37684f6-4c3a-4243-82d6-e538dea2a8f6` |
| **CODE_SHA candidato** | `66ebe94` (fase 14: pruebas del editor y del Plugin) |
| DLL Debug a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| **DLL SHA-256** | `6769A7824EB2648017D05801F00617FD92BE375F53CB300A17F144D9C2B77A00` |
| Tamaño / fecha | 133 120 bytes · `2026-07-30 08:09:11` |
| Worktree | `C:\Users\alejandra-mendoza\.claude\worktrees\feature-cantilever-mvp-final` |
| Pruebas | `RackCad.Tests` 2694/2694 · `RackCad.UI.Tests` 562/562 |
| Requisitos | `requires_autocad: true`, `requires_owner_validation: true`, `bundle: REQUERIDO` |

> **Si recompilas, el SHA-256 cambia.** Anota el nuevo antes de cargar: un DLL sin trazabilidad no valida
> la rama. El comando está en el paso 2.

## 2. Preparar el entorno

1. **Cierra AutoCAD por completo** y confirma que no quede un proceso `acad`. AutoCAD bloquea
   `RackCad.Plugin.dll`: con él abierto, el bundle **aborta** con un mensaje explícito y la copia del DLL
   puede fallar con `MSB3021`/`MSB3027`. Ese fallo de copia no es un error de código.

2. Desde la raíz del worktree:

```powershell
git status; git branch --show-current; git rev-parse HEAD
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

3. El bundle (requerido porque el diff toca UI, Plugin y materialización):

```powershell
pwsh deploy\build-bundle.ps1 -Configuration Debug -InventoryOutPath docs\automation\evidence\I-37D-bundle-inventory.txt
```

4. Abre AutoCAD 2025 con un dibujo **nuevo y descartable**, ejecuta `NETLOAD` y selecciona el DLL exacto
   del worktree. No cargues el DLL del worktree principal: valida otra rama.

5. Comprueba las unidades del dibujo. Todo el modelo está en **pulgadas** y nada lo convierte: si el
   dibujo no está en pulgadas, RackCad avisa una vez antes del primer bloque (I-05, ADR-0005). El aviso
   es informativo y **no bloquea**.

## 3. Qué es nuevo y qué no

**Nuevo en I-37D:** el comando `RACKCANTILEVER` (alias `RCT`), el botón «Diseñar línea Cantilever» del
menú `RACKCAD`, el editor de línea sobre el shell visual común, las tres vistas materializadas, el BOM
por componentes de la línea, la persistencia en biblioteca y el round-trip por `RACKEDITAR`.

**Deliberadamente FUERA de alcance** (no lo reportes como defecto): cálculo resistente, cargas,
capacidad, peso, costo, optimización, soldaduras, tornillos y tuercas, anclas, roscas, tolerancias,
preparación de extremos, CNC, planos de taller y **la interferencia física en el cruce de tensores**.

**Convenciones que ya estaban decididas y no son defectos:**

- Una varilla *cold rolled* se dibuja como su **eje**, no como un cilindro: ninguna fila del catálogo
  respalda un contorno circular para ella (ADR-0027, D7).
- Su **adaptador** se dibuja como el cuadrado de su corte, declarado como representación.
- La **lateral** es de UNA estación y **no muestra el arriostramiento**: el arriostramiento vive ENTRE
  estaciones, y de perfil se vería como una línea del ancho de un patín.
- El juego inicial es **una frontal, una planta y una lateral**, no N laterales. La lateral de otra
  estación se inserta cambiando el número en la vista previa.
- Una línea **recién abierta no se resuelve**: las tres secciones y los dos márgenes de troquel
  obligatorios no tienen valor aprobado, así que el editor los pide en vez de inventarlos.

## 4. Checklist

Marca cada punto **OK** o **FALLA** y, si falla, describe qué viste. Un solo FALLA rechaza el gate.

### A. Descubrimiento y arranque

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| A1 | `RACKCAD` → busca «Diseñar línea Cantilever» | El botón existe, entre «Push Back» y «Diseñar cabecera» | ☐ |
| A2 | Pulsa el botón | Abre el editor con la barra lateral, la matriz, la vista previa y la banda de estado del **mismo aspecto** que Push Back y el selectivo | ☐ |
| A3 | Cierra y ejecuta `RACKCANTILEVER` (y luego `RCT`) | Abre el mismo editor | ☐ |
| A4 | `RACKAYUDA` | Lista `RACKCANTILEVER` / `RCT` | ☐ |
| A5 | Con el editor recién abierto, lee la banda de estado | Dice qué campo falta (p. ej. «Margen de extremo de placa inferior»), **no** un error genérico. Los botones de insertar están deshabilitados | ☐ |

### B. Góndola sencilla, 2 estaciones — las tres vistas

Configura: Estaciones **2**, separación **96**, Niveles **2**, Claro **24**, Sección de columna
**W10X33**, Sección de base **W12X26**, Longitud de base **48**, Margen de extremo de placa inferior
**1.5**, Margen del troquel superior **4**, Sección de brazo **HSS4X4X1/4**, Longitud de corte **36**,
Margen vertical de placa **1.5**. Nombre: **Cantilever B**.

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| B1 | Al completar el último campo | La banda de estado resume: 2 estaciones · 1 intervalo · altura común · paneles · separadores · tensores | ☐ |
| B2 | Vista previa **Frontal** | Dos columnas verticales, sus bases, los brazos por nivel y, ENTRE las columnas, los separadores y los tensores en X | ☐ |
| B3 | Vista previa **Lateral** | Una columna con su base y sus brazos, **sin** arriostramiento | ☐ |
| B4 | Vista previa **Planta** | La línea a lo largo y los brazos hacia arriba de la imagen | ☐ |
| B5 | «Insertar frontal» → clic en el dibujo | Se inserta un bloque; el prompt dice «Punto de insercion de la vista Cantilever» | ☐ |
| B6 | Repite con «Insertar planta» y «Insertar lateral» | Tres bloques independientes, cada uno con su nombre (`… - frontal`, `… - planta`, `… - lateral 1`) | ☐ |
| B7 | Compara cada bloque con lo que mostraba la vista previa | **La misma figura.** Es el mismo plan: si difieren, es un defecto grave | ☐ |
| B8 | Mide la separación entre ejes de columna en la frontal | 96 in | ☐ |
| B9 | Cancela una inserción con ESC | No queda ningún bloque suelto ni definición fantasma en el dibujo | ☐ |

### C. Góndola doble

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| C1 | En el editor, Caras → **Doble** | La matriz duplica sus filas: cada nivel aparece con lado **+Y** y lado **−Y** | ☐ |
| C2 | Vista previa lateral | Brazos a AMBOS lados de la columna, y **dos bases espejadas** | ☐ |
| C3 | Inserta la lateral | El dibujo coincide con la previa | ☐ |

### D. Matriz estación × nivel × lado y alcances

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| D1 | Con 3 estaciones y 2 niveles, cuenta las celdas | 6 en sencilla, 12 en doble | ☐ |
| D2 | Clic en una celda | El panel inferior dice qué celda es y carga su brazo | ☐ |
| D3 | Cambia «Longitud de corte» a 60, alcance **Celda**, «Aplicar» | Solo esa celda queda en negritas (excepción); el estado dice «1 de 1 celdas actualizadas» | ☐ |
| D4 | Alcance **Estación**, «Aplicar» | Cambian todas las celdas de esa estación y solo ésas | ☐ |
| D5 | Alcance **Nivel (toda la línea)**, «Aplicar» | Cambia ese nivel en todas las estaciones | ☐ |
| D6 | «Aplicar» un brazo idéntico al de omisión | El estado dice que **ninguna celda cambió**; no aparece una excepción nueva | ☐ |
| D7 | Alcance **Toda la línea**, «Restaurar» | Todas vuelven al brazo por omisión; el contador dice «Sin excepciones» | ☐ |
| D8 | Con una excepción puesta, mira la frontal | El brazo distinto se ve distinto **solo** donde corresponde | ☐ |

### E. Arriostramiento — el caso normativo de 264 in

Configura: Estaciones **4**, Altura de columna **Manual = 264**, resto como en el bloque B.

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| E1 | Lee el resumen de la banda de estado | **3 intervalos · 18 separadores · 24 tensores** | ☐ |
| E2 | Cuenta los paneles arriostrados de un intervalo en la frontal | **4 paneles**, con **1 espacio vacío al centro** | ☐ |
| E3 | Mide la secuencia vertical de un intervalo | 32 · 40 · 40 · **40 vacío** · 40 · 40 · 32 (in) | ☐ |
| E4 | Cuenta los separadores de un intervalo | **6** (paneles + vacíos + 1) | ☐ |
| E5 | Cuenta los tensores de un intervalo | **8** (dos por panel arriostrado) | ☐ |
| E6 | Mira una frontera entre dos intervalos | El separador de esa frontera aparece **una sola vez**; no hay dos superpuestos | ☐ |
| E7 | Cambia la altura a otras de la tabla de producto (96, 144, 216, 336) | Los paneles siguen la regla: 1, 2, 3, 5 | ☐ |

### F. Tensores: cold rolled y estructural

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| F1 | Tensor = **Cold rolled** (por omisión), diámetro 0.75 | Cada tensor se dibuja como su **eje**, con un cuadrado (el adaptador) en cada extremo | ☐ |
| F2 | Cambia a **Estructural** sin elegir sección | La línea se **rechaza** con un mensaje claro; no se inventa una sección | ☐ |
| F3 | Elige un ángulo o canal como sección de tensor | La línea se resuelve y el tensor se dibuja como **perfil**, no como eje | ☐ |
| F4 | Vuelve a cold rolled | Vuelve el eje con adaptadores | ☐ |

### G. Lista de materiales

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| G1 | «Lista de materiales» | Abre el BOM **por componentes**: columna-base, brazo, separador y tensor | ☐ |
| G2 | Con 4 estaciones y 3 intervalos | Los separadores suman 18 y los tensores 24 | ☐ |
| G3 | Componente «Tensor» cold rolled | Lleva su varilla, **2 adaptadores** por tensor y los **cartabones calibre 10** | ☐ |
| G4 | Estaciones idénticas | Se agrupan por RECETA: una línea de cantidad N, no N líneas iguales | ☐ |
| G5 | Pon una excepción de brazo en una celda | Aparece un componente de brazo **distinto**, y el otro baja de cantidad | ☐ |
| G6 | Con la línea sin resolver (borra una sección) | El BOM **no** se abre; el estado explica por qué | ☐ |

### H. Guardar, cerrar y reabrir

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| H1 | «Guardar en biblioteca» | Guarda el archivo sin marcar inserción | ☐ |
| H2 | `RACKCAD` → «Abrir de la biblioteca de diseños» | La línea aparece listada como **Cantilever** | ☐ |
| H3 | Ábrela | Vuelve con **todos** sus datos: estaciones, topología, brazo, excepciones y arriostramiento | ☐ |
| H4 | Insértala | Se acuña un **GUID nuevo**: es un rack nuevo, no el guardado | ☐ |
| H5 | Guarda el DWG, ciérralo y vuelve a abrirlo | Los bloques siguen ahí y sus datos también | ☐ |

### I. Editar y redibujar (round-trip)

| # | Paso | Resultado esperado | Veredicto |
|---|---|---|---|
| I1 | Con las tres vistas insertadas, `RACKEDITAR` → selecciona una | Reabre el editor con los datos de esa línea y el botón «Actualizar» **habilitado** | ☐ |
| I2 | Cambia el número de niveles y pulsa «Actualizar» | Se redibujan **las tres vistas en sitio**; el mensaje dice cuántas de cada una | ☐ |
| I3 | Comprueba el GUID | Es el **mismo** de antes: una edición nunca acuña identidad nueva | ☐ |
| I4 | Haz crecer la línea (más estaciones) y actualiza | La parte nueva se dibuja **y se puede seleccionar con ventana** | ☐ |
| I5 | Con una lateral de la estación 4 insertada, baja a 2 estaciones y actualiza | La lateral obsoleta se **elimina** y el mensaje lo dice | ☐ |
| I6 | Repite dejando SOLO la lateral obsoleta | **No** se borra el último vínculo de la línea; lo avisa | ☐ |
| I7 | En `RACKEDITAR`, pulsa «Insertar planta» en vez de «Actualizar» | Redibuja las existentes **y** además inserta la nueva vista, ligada al mismo GUID | ☐ |
| I8 | `RACKLISTA` | La línea aparece con su nombre, tipo **Cantilever** y sus vistas | ☐ |
| I9 | `RACKBOMTOTAL` | Incluye la línea, etiquetada **Cantilever**, con sus componentes | ☐ |
| I10 | `RACKDUPLICAR` sobre una vista | La copia es **independiente**: al editarla no cambia la original | ☐ |

## 5. Resultado

| Campo | Valor |
|---|---|
| Fecha | *(pendiente)* |
| DLL SHA-256 realmente cargado | *(pendiente)* |
| Veredicto | ☐ APROBADA ☐ RECHAZADA |
| Observaciones | *(pendiente)* |

Si el veredicto es **RECHAZADA**, anota el número de cada punto que falló y qué se vio. Cada ronda de
corrección se registra en `docs/automation/state/I-37D.yml` y esta tabla conserva su historial: las
validaciones previas **no se reescriben**.

## 6. Lo que este paquete no decide

- **ADR-0027 y ADR-0028 siguen PROPUESTOS.** Aceptarlos es del dueño y es un acto aparte del veredicto
  de esta validación.
- La sección **C4 más ligera** (`AISC-C-C4X4_5`) es el valor por omisión del separador porque el
  catálogo la determina sin ambigüedad; **cuál C4 usa el producto** sigue siendo una elección del dueño.
- El **perfil por omisión del brazo** no existe: la UI ofrece el catálogo y el sistema bloquea hasta que
  se elija un id exacto.
