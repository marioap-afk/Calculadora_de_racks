# I-37D — Validación manual · **RONDA DE CORRECCIÓN: paleta, placa, base, ángulo y planta**

Estado: **PENDIENTE DE EJECUTAR**. Los paquetes anteriores se conservan sin reescribir:
[ronda 1](I-37D-autocad-validation.md) · [ronda 2](I-37D-autocad-validation-round-2.md) ·
[columna y base](I-37D-autocad-validation-round-2-column-base-fix.md) ·
[ronda 3, puntos 1–6](I-37D-autocad-validation-round-3.md) ·
[ronda 3, punto 7](I-37D-autocad-validation-round-3-braces.md).

**Los paneles quedan fuera**, como en la ronda anterior.

## 1. Identificación

| Campo | Valor |
|---|---|
| Rama | `feature/cantilever-mvp-final` |
| **CODE_SHA / VALIDATED_BUILD_SHA** | `06ceb33a26a83499232e6178102c029482813f2b` |
| **DLL SHA-256** | `854EABAEC580F5C13D14DBCA29FF92D27A7C86C5F90F8A7EBD275A1108CD7055` |
| Tamaño / fecha | 135 680 bytes · `2026-07-31 11:50:15` |
| **Bundle** | ✅ con AutoCAD **cerrado**, fail-closed, 153 comprobaciones |
| Inventario | [`I-37D-round-3-correction-bundle-inventory.txt`](I-37D-round-3-correction-bundle-inventory.txt) |
| Suites | `RackCad.Tests` 2929/2929 · `RackCad.UI.Tests` 609/609 |
| Regresiones | 21/21 **en rojo** — [evidencia](I-37D-round-3-correction-regressions.md) |

> Si recompilas, el SHA-256 cambia: la versión incrusta la punta de git. Anota el nuevo antes de cargar.

## 2. La paleta que se aplicó

| Pieza | Color | ACI |
|---|---|---|
| Columna, base, placa de base, cartabón de base **y placa columna–separador** | rojo | 1 |
| Troqueles | blanco | 7 |
| Separador | naranja | 30 |
| **Perfil del brazo** | **azul** | 5 |
| **Placa/ménsula del brazo** | **morado** | 6 |
| **Tensor entero** — cuerpo, adaptadores y cartabones | **cian** | 4 |

Cada rol conserva **su propia capa**: compartir color no impide apagar una familia sola.

---

## Bloque A — Los colores

| # | Comprobación | ✅/❌ |
|---|---|---|
| A1 | Columna, base, placas de base y cartabones se ven **rojos** | ☐ |
| A2 | La placa que une columna con separador se ve **roja**, como la columna | ☐ |
| A3 | Los troqueles se ven **blancos** | ☐ |
| A4 | El separador se ve **naranja** | ☐ |
| A5 | El **perfil** del brazo se ve **azul** | ☐ |
| A6 | La **ménsula** del brazo se ve **morada**, distinta del perfil | ☐ |
| A7 | El tensor, sus adaptadores y sus cartabones se ven **cian**, los tres | ☐ |
| A8 | Los colores de la **previa** y los del **dibujo** coinciden | ☐ |
| A9 | Se puede apagar una capa sola (p. ej. `RACKCAD_CANT_TENSOR_CARTABON`) sin perder las demás | ☐ |

## Bloque B — La placa columna–separador

| # | Comprobación | ✅/❌ |
|---|---|---|
| B1 | La placa **no atraviesa** el alma de la columna | ☐ |
| B2 | Apoya en la **cara** del alma y crece hacia su propio tramo | ☐ |
| B3 | Queda del **lado correcto**, coincidiendo con el separador | ☐ |
| B4 | Su agujero sigue **dentro** de la placa | ☐ |
| B5 | El separador mide **lo mismo que antes** de esta ronda | ☐ |
| B6 | Igual en la previa y en el dibujo | ☐ |

## Bloque C — La base de columna

Es el defecto de las imágenes 4 y 5: la columna se dibujaba **dentro** de la base.

| # | Comprobación | ✅/❌ |
|---|---|---|
| C1 | En una estación **doble**, la columna queda **entre** las dos bases y no dentro de ninguna | ☐ |
| C2 | Cada base arranca en **su** cara de la columna | ☐ |
| C3 | La base y el **brazo** del mismo lado apoyan en la **misma** cara | ☐ |
| C4 | La columna sigue **elevada** sobre su placa inferior | ☐ |
| C5 | Nada quedó **invertido** ni **desfasado**: los cartabones acompañan a su base | ☐ |
| C6 | Igual en **frontal**, **lateral** y **planta**, y en la previa | ☐ |
| C7 | Una estación **sencilla** sigue igual que antes | ☐ |

## Bloque D — El ángulo

| # | Comprobación | ✅/❌ |
|---|---|---|
| D1 | Un ángulo se ve con las **puntas redondeadas**, no a escuadra | ☐ |
| D2 | Conserva su **filete de raíz** en el talón | ☐ |
| D3 | Se parece razonablemente al perfil real de la comparación | ☐ |
| D4 | Un ángulo **delgado** termina en nariz redonda sin deformarse | ☐ |
| D5 | Los **adaptadores** de tensor —que son ángulos— siguen bien | ☐ |

## Bloque E — La planta

| # | Comprobación | ✅/❌ |
|---|---|---|
| E1 | La planta abre **sin brazos ni tensores** | ☐ |
| E2 | Aparecen los dos **checkbox**, los dos apagados | ☐ |
| E3 | Encender «Brazos» devuelve los brazos **y sus ménsulas** | ☐ |
| E4 | Encender «Tensores» devuelve los tensores; cada uno manda **sólo sobre lo suyo** | ☐ |
| E5 | La planta sigue mostrando **columnas, bases y separadores** | ☐ |
| E6 | La **frontal** y la **lateral** no cambian al tocarlos | ☐ |
| E7 | El **BOM no cambia** al tocarlos | ☐ |
| E8 | Guardar y reabrir conserva el estado de los dos | ☐ |

---

## 3. Resultado

| Campo | Valor |
|---|---|
| Fecha | *(pendiente)* |
| DLL SHA-256 cargado | *(pendiente)* |
| Veredicto | ☐ APROBADA ☐ RECHAZADA |
| Observaciones | *(pendiente)* |

## 4. Lo que este paquete NO cubre

- **Los paneles.** Fuera por decisión tuya.
- **La ambigüedad de `NearOffset`.** Se encontró en esta ronda y se dejó **declarada, no resuelta**: el
  resumen dice «coordenada del mundo» y las placas de brazo lo usan como «distancia a lo largo del normal».
  Convive desde antes; unificarlo toca cuatro familias de placa y es un cambio de contrato.
- **La firma de la línea no capta la posición en Y de la base.** La corrección del punto 3 movió la base
  espejada 9.73 in y el pin `linea-doble` **no se movió**. No es un fallo introducido aquí: es un límite de
  esa firma, y conviene saberlo antes de confiar en ella para detectar una colocación mal puesta.
- ADR-0027 y ADR-0028 siguen **propuestos**; I-37 sigue abierta; no se integra nada; no se abre I-38.
