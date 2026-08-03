# I-37D — Paquete de validación manual en AutoCAD 2025 (Owner) · **RONDA 1**

Estado: ❌ **RECHAZADA**. Veredicto del dueño: `OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_1`.

> **Este documento se conserva tal cual y NO se reescribe como aprobado.** Es el registro de lo que se
> puso a validar y de lo que el dueño rechazó. La ronda 2 tiene su propio paquete:
> [`I-37D-autocad-validation-round-2.md`](I-37D-autocad-validation-round-2.md).

## 0. Motivos del rechazo

1. la ventana principal está **excesivamente saturada**;
2. **mezcla propiedades de línea con propiedades internas de componentes**;
3. los **perfiles son difíciles de seleccionar**;
4. la **base no sigue inicialmente la sección de columna**;
5. **faltan troqueles y placas visibles** en la representación de columnas;
6. la **arquitectura visual no refleja el flujo real de configuración** del producto.

Ninguno es un defecto de geometría resuelta: los seis son de **arquitectura visual y de flujo**. Por eso
la ronda 2 reestructura el producto —la ventana principal edita la LÍNEA y cada componente se edita en su
propia subventana— antes de revisar en detalle brazo, separador y tensor.

El checklist de abajo **no se ejecutó completo**: el rechazo llegó por la estructura de la ventana, que
es lo primero que se ve. Se conserva porque sus puntos siguen describiendo comportamiento que la ronda 2
debe preservar, y porque su distribución visual es justamente lo que **no** debe congelarse como golden.

---

Estado original del documento cuando se emitió: **PENDIENTE DE EJECUTAR**. Este documento era el gate
real de I-37D. La CI verde es necesaria y **no suficiente**: I-37D es la primera subiniciativa de la línea
I-37 que cambia interfaz y dibujo, así que sin el veredicto manual del dueño en AutoCAD 2025 **no se
integra** y **I-37 no se cierra**.

## 1. Identificación

| Campo | Valor |
|---|---|
| Iniciativa | I-37D — Cantilever MVP final: línea, arriostramiento, vistas y editor |
| Rama | `feature/cantilever-mvp-final` |
| Claim-Id | `c37684f6-4c3a-4243-82d6-e538dea2a8f6` |
| **CODE_SHA funcional** | `66ebe94` — la última punta que tocó `src/**` o `tests/**` |
| **VALIDATED_BUILD_SHA** | `4a2a6f576af433dcd5856fce7a2b6ef311ec0ea9` — la punta desde la que se compiló el DLL |
| DLL Debug a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| **DLL SHA-256** | `53CA2118BD8D4923C9D660B0FFE3A65D4FC38507DD3CD788D295C22D2C0FEABC` |
| `AssemblyInformationalVersion` | `1.0.0+4a2a6f576af433dcd5856fce7a2b6ef311ec0ea9` |
| Tamaño / fecha | 133 120 bytes · `2026-07-30 08:47:23` |
| **Bundle** | ✅ generado y **verificado fail-closed** (153 comprobaciones, 24 archivos, cero DLL de Autodesk) |
| Inventario del bundle | [`I-37D-bundle-inventory.txt`](I-37D-bundle-inventory.txt) |
| Worktree | `C:\Users\alejandra-mendoza\.claude\worktrees\feature-cantilever-mvp-final` |
| Pruebas | `RackCad.Tests` 2694/2694 · `RackCad.UI.Tests` 562/562 |
| CI | run [`30552560784`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30552560784) — **success**, 4/4 jobs sobre `4a2a6f5` |
| Requisitos | `requires_autocad: true`, `requires_owner_validation: true`, `bundle: REQUERIDO` |

**Los dos SHA no son el mismo, y la diferencia es sana.** `66ebe94` es el último commit que cambió
código o pruebas; `4a2a6f5` es `66ebe94` más dos commits **exclusivamente documentales**. El DLL se
compiló desde `4a2a6f5` porque su `AssemblyInformationalVersion` incrusta la punta de git, así que
**recompilar tras un commit de docs cambia el SHA-256 aunque el código sea byte a byte el mismo**. El
árbol funcional que se valida es el de `66ebe94`.

El DLL de `bin\Debug` y el que viaja dentro del bundle son **el mismo binario** (`53CA2118…` los dos), así
que cargar por `NETLOAD` y cargar por bundle validan exactamente lo mismo.

> **Si recompilas, el SHA-256 vuelve a cambiar.** Anota el nuevo antes de cargar: un DLL sin
> trazabilidad no valida la rama. El comando está en el paso 2.

## 2. Preparar el entorno

1. **Cierra AutoCAD por completo** y confirma que no quede un proceso `acad`. AutoCAD bloquea
   `RackCad.Plugin.dll`: con él abierto, el bundle **aborta** con un mensaje explícito y la copia del DLL
   puede fallar con `MSB3021`/`MSB3027`. Ese fallo de copia no es un error de código.

2. **El DLL y el bundle de la tabla ya están construidos y verificados** sobre `4a2a6f5`. Si el árbol no
   cambió, no hace falta recompilar: basta con comprobar que el binario sigue siendo el mismo.

```powershell
git status; git branch --show-current; git rev-parse HEAD
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

   Si prefieres reconstruirlo desde cero —o si el árbol cambió— este es el ciclo completo. **Ojo:**
   recompilar tras cualquier commit nuevo cambia el SHA-256, así que anótalo y sustitúyelo en la tabla.

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
pwsh deploy\build-bundle.ps1 -Configuration Debug -InventoryOutPath docs\automation\evidence\I-37D-bundle-inventory.txt
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

   `build-bundle.ps1` publica y **verifica fail-closed**: DLL idénticos al publish, catálogos idénticos a
   `assets/catalogs`, sólo archivos de RackCad y **cero DLL de Autodesk** (ADR-0003). También deja el
   `bin\Debug\…\RackCad.Plugin.dll` recompilado, que es el que se carga en el paso 3.

3. Abre AutoCAD 2025 con un dibujo **nuevo y descartable**, ejecuta `NETLOAD` y selecciona el DLL exacto
   del worktree. No cargues el DLL del worktree principal: valida otra rama.

   Alternativa equivalente: instalar el bundle
   (`src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle`) con `deploy\install-bundle.ps1`.
   Lleva **el mismo binario**, así que el veredicto vale igual por cualquiera de las dos vías.

4. Comprueba las unidades del dibujo. Todo el modelo está en **pulgadas** y nada lo convierte: si el
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
| E1 | Lee el resumen de la banda de estado | **3 intervalos · 18 separadores · 24 tensores** (y, en el BOM, **36 placas de columna de separador**: 6 elevaciones × una cara en cada estación extrema y dos en cada interior) | ☐ |
| E2 | Cuenta los paneles arriostrados de un intervalo en la frontal | **4 paneles**, con **1 espacio vacío al centro** | ☐ |
| E3 | Mide la secuencia vertical de un intervalo | 32 · 40 · 40 · **40 vacío** · 40 · 40 · 32 (in) | ☐ |
| E4 | Cuenta los separadores de un intervalo | **6** (paneles + vacíos + 1) | ☐ |
| E5 | Cuenta los tensores de un intervalo | **8** (dos por panel arriostrado) | ☐ |
| E6 | Dentro de UN mismo intervalo, mira la frontera **horizontal** compartida entre dos paneles o segmentos verticales consecutivos | Hay **un solo** separador a esa elevación; no aparecen dos superpuestos (ADR-0027 D5: dos paneles adyacentes comparten su separador y se cuenta **una vez**) | ☐ |
| E7 | Ahora mira una estación **interior**, donde se tocan dos intervalos, a una misma elevación | Hay **dos** separadores físicos distintos —uno hacia cada intervalo— y **dos** placas de columna, una por cara. **No** es un defecto: son piezas distintas que sólo coinciden en altura | ☐ |
| E8 | Cambia la altura a otras de la tabla de producto (96, 144, 216, 336) | Los paneles siguen la regla: 1, 2, 3, 5 | ☐ |

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
| Fecha | 2026-07-30 |
| DLL cargado | `53CA2118BD8D4923C9D660B0FFE3A65D4FC38507DD3CD788D295C22D2C0FEABC` (build `4a2a6f5`) |
| Veredicto | ❌ **RECHAZADA** — `OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_1` |
| Observaciones | Los seis motivos de la sección 0. Arquitectura visual y flujo, no geometría resuelta. |
| Ronda siguiente | 2 — ver [`I-37D-autocad-validation-round-2.md`](I-37D-autocad-validation-round-2.md) |

Si el veredicto es **RECHAZADA**, anota el número de cada punto que falló y qué se vio. Cada ronda de
corrección se registra en `docs/automation/state/I-37D.yml` y esta tabla conserva su historial: las
validaciones previas **no se reescriben**.

## 6. Lo que este paquete no decide

- **ADR-0027 y ADR-0028 siguen PROPUESTOS.** Aceptarlos es del dueño y es un acto aparte del veredicto
  de esta validación.
- El **perfil por omisión del brazo** no existe: la UI ofrece el catálogo y el sistema bloquea hasta que
  se elija un id exacto.

## 7. Lo que ya está decidido y no se revalida

- **`DefaultSeparatorSectionId = AISC-C-C4X4_5`** — decisión **CERRADA** y vinculante
  (`OWNER_DECIDED_SEPARATOR_DEFAULT_AISC_C_C4X4_5`, decisión 12.51). Es el canal C4 **más ligero**
  disponible y es el valor por omisión aprobado del MVP. El usuario puede elegir otra sección
  explícitamente en el editor, pero **el default ya no está pendiente**: no se busca por designación
  durante la resolución y ningún punto de este checklist lo cuestiona.
- **Un separador no se comparte entre intervalos distintos.** Lo que se comparte —y se cuenta una sola
  vez— es el separador **horizontal** entre dos segmentos verticales consecutivos del **mismo** intervalo
  (ADR-0027 D5, decisión 12.14). Dos intervalos adyacentes tienen separadores **físicamente distintos**
  aunque coincidan en elevación: uno une la estación *i* con la *i+1* y otro la *i+1* con la *i+2*. Por
  eso una estación interior lleva **dos** placas de columna por elevación y una extrema **una**.
