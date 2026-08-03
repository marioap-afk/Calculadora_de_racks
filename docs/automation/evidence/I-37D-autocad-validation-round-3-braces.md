# I-37D — Validación manual · **RONDA 3, PUNTO 7: TENSORES Y ADAPTADORES**

Estado: **PENDIENTE DE EJECUTAR**. Los paquetes anteriores se conservan sin reescribir:
[ronda 1](I-37D-autocad-validation.md) · [ronda 2](I-37D-autocad-validation-round-2.md) ·
[columna y base](I-37D-autocad-validation-round-2-column-base-fix.md) ·
[ronda 3, puntos 1–6](I-37D-autocad-validation-round-3.md).

Este paquete cubre **sólo el punto 7**. **Los paneles quedan explícitamente fuera**: se sabe que están mal y
se difieren a la corrida siguiente.

## 1. Identificación

| Campo | Valor |
|---|---|
| Rama | `feature/cantilever-mvp-final` |
| **CODE_SHA / VALIDATED_BUILD_SHA** | `959927a8c3a938366126f69045bc2f007c124290` |
| **DLL SHA-256** | `431BD53195C0B09B93BD462E4C2AFA48236B8C95E0C5722A4C385850573BF485` |
| Tamaño / fecha | 135 680 bytes · `2026-07-31 08:31:08` |
| **Bundle** | ✅ con AutoCAD **cerrado**, fail-closed, 153 comprobaciones |
| Inventario | [`I-37D-round-3-braces-bundle-inventory.txt`](I-37D-round-3-braces-bundle-inventory.txt) |
| Suites | `RackCad.Tests` 2902/2902 · `RackCad.UI.Tests` 605/605 |
| CI | **success** — run [`30638904321`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30638904321) |
| Regresiones | 15/15 **en rojo** — [evidencia](I-37D-round-3-brace-regressions.md) |

> Si recompilas, el SHA-256 cambia: la versión incrusta la punta de git. Anota el nuevo antes de cargar.

## 2. La decisión que cambió

`OWNER_REVISED_CANTILEVER_BRACE_VISUAL_REPRESENTATION`, recogida en **ADR-0027 D7-bis**. El ADR sigue
**propuesto**: la revisión lo corrige, no lo acepta.

> El eje continúa siendo el **datum** geométrico del tensor, pero la geometría **visible** debe tener ancho
> físico.

| Pieza | Antes | Después |
|---|---|---|
| Cuerpo cold rolled | polilínea **abierta de 2 puntos** (el eje) | banda **cerrada de 4**, ancho = **diámetro** |
| Adaptador | **cuadrado** de 4 puntos | **L** de 6 puntos, dos alas de 2 in, espesor 3/16 |
| Cartabones | no se dibujaban | **2 triángulos** por adaptador, uno en cada extremo del corte |
| Agujero de varilla | no se dibujaba | **círculo** de 9/16 in |
| Orientación | la misma para los cuatro extremos | **derivada**, cuatro manos |

## 3. Preparar

```powershell
git status; git rev-parse HEAD
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

Abre AutoCAD 2025 con un dibujo **nuevo y descartable**, `NETLOAD`, y selecciona ese DLL.

---

## Bloque A — El cuerpo cold rolled

| # | Comprobación | ✅/❌ |
|---|---|---|
| A1 | El tensor se ve como una **banda**, no como una raya | ☐ |
| A2 | Su ancho mide **3/4 in** | ☐ |
| A3 | Tiene **dos bordes paralelos** y cierre en cada extremo | ☐ |
| A4 | El ancho es **constante**: no se afila hacia el adaptador | ☐ |
| A5 | Su longitud sigue siendo la de antes (no creció con el ancho) | ☐ |

## Bloque B — El adaptador

| # | Comprobación | ✅/❌ |
|---|---|---|
| B1 | Se ve como un **ángulo**, con su talón y sus dos alas | ☐ |
| B2 | Cada ala mide **2 in** y el espesor **3/16 in** | ☐ |
| B3 | Se ve el **agujero de la varilla** (9/16 in) | ☐ |
| B4 | El agujero contra el separador aparece **una sola vez**, no dos círculos encimados | ☐ |
| B5 | Se ven sus **dos cartabones** como triángulos | ☐ |
| B6 | El perfil **no atraviesa** el adaptador | ☐ |

## Bloque C — Las cuatro orientaciones

| # | Comprobación | ✅/❌ |
|---|---|---|
| C1 | Los **cuatro** adaptadores de un panel miran a sitios **distintos** | ☐ |
| C2 | Abajo-izquierda: el ala del tensor sube hacia la derecha | ☐ |
| C3 | Abajo-derecha: sube hacia la izquierda | ☐ |
| C4 | Arriba-izquierda y arriba-derecha: bajan, espejadas de las de abajo | ☐ |
| C5 | Ninguno gira 90° ni 180° sin razón | ☐ |

## Bloque D — Canal y ángulo

| # | Comprobación | ✅/❌ |
|---|---|---|
| D1 | Con tensor de **canal**, se ve su perfil y no su eje | ☐ |
| D2 | Con tensor de **ángulo**, se ven **sus dos alas** y no un rectángulo | ☐ |
| D3 | Los dos respetan su orientación sobre la diagonal | ☐ |

## Bloque E — El panel y las vistas

| # | Comprobación | ✅/❌ |
|---|---|---|
| E1 | En la **frontal**, las dos diagonales forman una **X** | ☐ |
| E2 | **No** hay pieza alguna en el cruce | ☐ |
| E3 | Se solapan sin unirse | ☐ |
| E4 | En **planta** y **lateral** no falta ninguna pieza visible | ☐ |

## Bloque F — Componente suelto, colores y BOM

| # | Comprobación | ✅/❌ |
|---|---|---|
| F1 | «Insertar sólo esta pieza» del **tensor** funciona | ☐ |
| F2 | Inserta la **misma** geometría que dentro de la línea, no una versión simple | ☐ |
| F3 | Cancelar **no deja** bloques ni definiciones fantasma | ☐ |
| F4 | Existen las capas `RACKCAD_CANT_TENSOR`, `_TENSOR_ADAPTADOR`, `_TENSOR_CARTABON`, `_TENSOR_TROQUEL` | ☐ |
| F5 | Cuerpo, adaptador y cartabón se distinguen por color; los agujeros en blanco | ☐ |
| F6 | Los colores de la previa y los del dibujo coinciden | ☐ |
| F7 | El **BOM no cambió**: misma longitud, diámetro, 2 adaptadores y 4 cartabones por tensor | ☐ |
| F8 | Guardar y reabrir un proyecto anterior sigue funcionando | ☐ |

---

## 4. Resultado

| Campo | Valor |
|---|---|
| Fecha | *(pendiente)* |
| DLL SHA-256 cargado | *(pendiente)* |
| Veredicto | ☐ APROBADA ☐ RECHAZADA |
| Observaciones | *(pendiente)* |

## 5. Lo que este paquete NO cubre

- **Los paneles.** Fuera por decisión tuya; la corrida siguiente se dedica a ellos.
- **Fabricación.** No hay preparación de bordes, destijeres, soldadura del talón, roscas ni tolerancias: es
  representación visual, y así está declarado en ADR-0027 D7-bis.
- ADR-0027 y ADR-0028 siguen **propuestos**; I-37 sigue abierta; no se integra nada; no se abre I-38.
