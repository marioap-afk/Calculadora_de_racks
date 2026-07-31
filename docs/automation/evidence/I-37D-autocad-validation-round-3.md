# I-37D — Paquete de validación manual en AutoCAD 2025 · **RONDA 3**

Estado: **PENDIENTE DE EJECUTAR**. Los paquetes de las rondas anteriores se conservan sin reescribir:
[ronda 1](I-37D-autocad-validation.md) · [ronda 2](I-37D-autocad-validation-round-2.md) ·
[corrección de columna y base](I-37D-autocad-validation-round-2-column-base-fix.md).

Esta ronda atiende **seis de los siete puntos** que pediste. El séptimo —tensores y adaptadores de ángulo—
**no se hizo**, y está declarado como pendiente al final. Los paneles quedan fuera por decisión tuya.

## 1. Identificación

| Campo | Valor |
|---|---|
| Rama | `feature/cantilever-mvp-final` |
| **CODE_SHA / VALIDATED_BUILD_SHA** | `051c7c23aa2d6cffd2316101dab36c00959690a7` |
| DLL Debug | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| **DLL SHA-256** | `8C6D6F92A4CC8423CF514C26BD47BC9EBF8630AF6834E59E8137B8705005E81A` |
| `AssemblyInformationalVersion` | `1.0.0+051c7c23aa2d6cffd2316101dab36c00959690a7` |
| Tamaño / fecha | 135 680 bytes · `2026-07-30 23:50:23` |
| **Bundle** | ✅ generado con AutoCAD **cerrado**, verificado fail-closed: 153 comprobaciones |
| Inventario | [`I-37D-round-3-bundle-inventory.txt`](I-37D-round-3-bundle-inventory.txt) |
| Suites | `RackCad.Tests` 2880/2880 · `RackCad.UI.Tests` 605/605 |
| CI | **success** — run [`30607880920`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30607880920) |
| Regresiones | 15/15 **en rojo** — [evidencia](I-37D-round-3-regressions.md) |

> Si recompilas, el SHA-256 cambia: la versión incrusta la punta de git. Anota el nuevo antes de cargar.

## 2. Preparar

```powershell
git status; git rev-parse HEAD
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

Abre AutoCAD 2025 con un dibujo **nuevo y descartable**, `NETLOAD`, y selecciona ese DLL.

## 3. Qué cambió, con las medidas

| Punto | Qué se hizo | Antes → después |
|---|---|---|
| 1 · La columna suelta no se insertaba | De las **tres** puertas que dibujan curvas sólo una creaba las capas por rol, y la inserción de un componente era otra: AutoCAD tiraba la transacción entera. La creación de capas cuelga ahora del único sitio por el que pasan todas las curvas | Falla siempre → inserta |
| 2 · Rol y color de la columna | Conjunto columna–base **entero en rojo**, troqueles en **blanco**. Los roles siguen con capa propia; comparten el color | 4 colores distintos → 1 rojo + blanco |
| 3 · Orilla al troquel | 1.5 → **1.0 in**, medida desde el **exterior de la placa** hacia el centro de la columna | filas x = ±2.48 → **±2.245** |
| 4 · Brazo | Pendiente **7/16 por 12**, margen vertical **2 in**, filas **derivadas** de la altura del perfil (mín. 2, paso 4 in), y la **frontal sin inclinación** | pendiente 0 → 7/16; frontal idéntica con y sin pendiente |
| 5 · Bases del doble | El espejo volteaba la **Y local** de la sección y compensaba en la **X**: un giro de 180° | patín inferior en 11.82–12.2 → **0–0.38** |
| 6 · Separador | Se ata al **alma** y no al patín: pasa entre los patines y topa contra ella | claro **88.04 → 95.71 in** |

## Bloque A — Inserción del componente suelto

| # | Comprobación | ✅/❌ |
|---|---|---|
| A1 | «Insertar sólo esta pieza» en columna/base **funciona** y deja el bloque en el dibujo | ☐ |
| A2 | Deja las tres vistas, separadas y con nombre propio | ☐ |
| A3 | Los otros tres componentes —brazo, separador, tensor— siguen insertándose | ☐ |
| A4 | La línea completa se sigue insertando igual que antes | ☐ |

## Bloque B — Color y capas

| # | Comprobación | ✅/❌ |
|---|---|---|
| B1 | Columna, base, sus tres placas y el cartabón se ven **rojos** | ☐ |
| B2 | Los troqueles se ven **blancos** | ☐ |
| B3 | La placa de montaje de un **brazo NO** es roja | ☐ |
| B4 | Apagar `RACKCAD_CANT_TROQUEL` deja ver el acero sin agujeros | ☐ |
| B5 | Cada pieza sigue en su capa: se puede apagar la base sin apagar la columna | ☐ |
| B6 | Los colores de la previa y los del dibujo son los mismos | ☐ |

## Bloque C — La pulgada a la orilla

| # | Comprobación | ✅/❌ |
|---|---|---|
| C1 | La distancia del **exterior de la placa posterior** al centro de la fila mide **1 in** | ☐ |
| C2 | Los agujeros caben enteros en la placa y en la columna | ☐ |
| C3 | La pieza de referencia **no se bloquea** | ☐ |

## Bloque D — El brazo

| # | Comprobación | ✅/❌ |
|---|---|---|
| D1 | Un brazo nuevo trae pendiente **7/16 por 12** | ☐ |
| D2 | Trae margen vertical **2 in** | ☐ |
| D3 | Al elegir sección, la cantidad de filas **cambia sola** según la altura del perfil | ☐ |
| D4 | Esa cantidad se puede **subir y bajar a mano** y no se vuelve a pisar | ☐ |
| D5 | En la **frontal** el brazo se ve como su perfil, **sin inclinar** | ☐ |
| D6 | En la **lateral** sí se ve la pendiente | ☐ |
| D7 | La previa del configurador de brazo coincide con el dibujo | ☐ |

## Bloque E — El doble y el separador

| # | Comprobación | ✅/❌ |
|---|---|---|
| E1 | En una estación doble hay **dos bases enteras**, una a cada lado | ☐ |
| E2 | Las dos tienen sus troqueles | ☐ |
| E3 | Ninguna sale **boca abajo** | ☐ |
| E4 | El separador queda **centrado** sobre el eje de las columnas en planta | ☐ |
| E5 | Pasa **entre los patines** y no por fuera | ☐ |
| E6 | Su longitud es la de alma a alma (≈ 95.7 in con 96 de separación) | ☐ |

## 4. Resultado

| Campo | Valor |
|---|---|
| Fecha | *(pendiente)* |
| DLL SHA-256 cargado | *(pendiente)* |
| Veredicto | ☐ APROBADA ☐ RECHAZADA |
| Observaciones | *(pendiente)* |

## 5. Lo que este paquete NO cubre

- **Punto 7 del encargo: tensores y adaptadores de ángulo.** No se hizo. Los tensores siguen dibujándose
  como líneas y los adaptadores sin forma de L. **No lo valides**: no ha cambiado.
- **Los paneles**, fuera de alcance por decisión tuya.
- ADR-0027 y ADR-0028 siguen **propuestos**; I-37 sigue abierta; no se integra nada.
