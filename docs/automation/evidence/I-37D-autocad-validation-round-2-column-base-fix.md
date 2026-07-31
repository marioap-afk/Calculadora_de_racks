# I-37D — Paquete de validación manual en AutoCAD 2025 · **CORRECCIÓN DE COLUMNA Y BASE**

Estado: **PENDIENTE DE EJECUTAR**.

La ronda 1 quedó **RECHAZADA** (`OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_1`) y la ronda 2 quedó
**RECHAZADA en columna y base** (`OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_2_COLUMN_BASE`). **Los dos
paquetes anteriores se conservan sin reescribir**:
[ronda 1](I-37D-autocad-validation.md) · [ronda 2](I-37D-autocad-validation-round-2.md).

Esta ronda corrige **sólo columna y base**, más las costuras compartidas que su corrección tocaba
objetivamente. Brazo, separador y tensor no se modificaron. Un solo fallo rechaza la ronda.

## 1. Identificación

| Campo | Valor |
|---|---|
| Iniciativa | I-37D — Cantilever MVP final |
| Rama | `feature/cantilever-mvp-final` |
| **CODE_SHA funcional** | `00f4ca1` — última punta que tocó `src/**` o `tests/**` |
| **VALIDATED_BUILD_SHA** | `00f4ca1b373f9c210db6cbb78a2db869b81cc20c` — el DLL se compiló desde aquí |
| Punta de la rama | `55e6e92` — `00f4ca1` **más este paquete**, que es documentación y nada más |
| DLL Debug a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| **DLL SHA-256** | `FDB37A4F0DC26F13C01D87028AFBC72263F8C1AEF8603D8D6C6829EF7920E5AD` |
| `AssemblyInformationalVersion` | `1.0.0+00f4ca1b373f9c210db6cbb78a2db869b81cc20c` |
| Tamaño / fecha | 135 680 bytes · `2026-07-30 20:40:00` |
| **Bundle** | ✅ generado con AutoCAD **cerrado** y verificado fail-closed: 153 comprobaciones, DLL idénticos al publish, catálogos idénticos a `assets/catalogs`, cero DLL de Autodesk |
| Inventario del bundle | [`I-37D-column-base-fix-bundle-inventory.txt`](I-37D-column-base-fix-bundle-inventory.txt) |
| Suites | `RackCad.Tests` 2848/2848 · `RackCad.UI.Tests` 605/605 |
| CI | **success** sobre `00f4ca1` (run [`30599549470`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30599549470)) y sobre `55e6e92` (run [`30599724941`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30599724941)) — 4/4 jobs en ambos |
| Regresiones | 15/15 verificadas **en rojo** — [evidencia](I-37D-column-base-fix-regressions.md) |

El DLL de `bin\Debug` y el que viaja dentro del bundle son **el mismo binario** (`FDB37A4F…` los dos), así
que cargar por `NETLOAD` y cargar por bundle validan exactamente lo mismo.

**`git rev-parse HEAD` te dará `55e6e92`, no `00f4ca1`, y eso es sano.** El DLL se compiló desde `00f4ca1`
porque su `AssemblyInformationalVersion` incrusta la punta de git: recompilar tras un commit **de docs**
cambiaría el SHA-256 sin que una sola línea de código fuese distinta. El árbol funcional que se valida es
el de `00f4ca1`, y entre los dos commits `git diff 00f4ca1..55e6e92 -- src tests` está **vacío**.

> **Si recompilas, el SHA-256 vuelve a cambiar** por esa misma razón. Anota el nuevo antes de cargar: un DLL
> sin trazabilidad no valida la rama.

## 2. Preparar

**El DLL y el bundle de la tabla ya están construidos y verificados** sobre `00f4ca1`. Si el árbol no
cambió, no hace falta recompilar: basta con comprobar que el binario sigue siendo el mismo.

```powershell
git status; git rev-parse HEAD
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

Si prefieres reconstruirlo —o si el árbol cambió— **cierra AutoCAD** antes: bloquea el DLL y el script del
bundle aborta.

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
pwsh deploy\build-bundle.ps1 -Configuration Debug -InventoryOutPath docs\automation\evidence\I-37D-column-base-fix-bundle-inventory.txt
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -Algorithm SHA256
```

Después: abre AutoCAD 2025 con un dibujo **nuevo y descartable**, `NETLOAD`, y selecciona ese DLL exacto.
Alternativa equivalente: instalar
`src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle` con `deploy\install-bundle.ps1`.

## 3. Qué cambió respecto de la ronda 2

Los cinco motivos del rechazo, y lo que se hizo con cada uno. **Los tres defectos geométricos se midieron
antes de tocar nada**, sobre la línea de referencia, y las medidas están aquí para que puedas comprobarlas
en el dibujo:

| Motivo | Qué se hizo | Medida antes → después |
|---|---|---|
| 1 · Dos márgenes de troquel sin utilidad de producto | Retirados. El troquel se acota por su **radio**: un agujero entero cabe o no cabe | Cabe un agujero más por fila: 204 → 210 troqueles en la línea sencilla |
| 2 · Geometría de la planta incorrecta | Se preguntaba a la **cámara** si miraba a lo largo del eje Z *del mundo*; ahora se pregunta por el eje del **miembro** | Base en planta: `6.49 × 0.0000` → `6.49 × 48.00` in. Brazos: idem, `4.00 × 36.00` |
| 3 · La lateral omite placas y cartabón | No las omitía: las dibujaba con **espesor cero**, por proyectar el contorno de una sola cara. Ahora se dibuja la silueta del sólido | Placa inferior: `9.73 × 0.0000` → `9.73 × 0.2500`. Placas de base: `0.0000` → `0.2500` de ancho |
| 4 · La columna arranca en el piso | Arranca en la **cara superior de su placa inferior**. La base **se queda en el piso** (decisión normativa del dueño) | Envolvente: `−0.25 … 96` → `0 … 96.25` |
| 5 · Naturalezas físicas indistinguibles | Todo salía BYBLOCK en la capa 0. Ahora cada entidad va en la **capa de su naturaleza**, BYLAYER, con color por rol | 9 capas `RACKCAD_CANT_*`, creadas todas y sin tocar las que ya existan |

**Lo que NO cambió, y es deliberado:** la longitud **nominal** de corte de la columna, el BOM comercial, el
patrón de conexión compartido y las piezas de la base —placa posterior, frontal y cartabón— que **no suben**
con la columna. Base y columna comparten el datum **lógico** de conexión, no el mismo origen físico en Z:
está precisado en la nota **N1** de [ADR-0024](../../adr/0024-fundacion-cantilever-base-columna.md).

---

## Bloque A — El datum vertical

| # | Comprobación | ✅/❌ |
|---|---|---|
| A1 | `RACKCANTILEVER` dibuja la línea sin error | ☐ |
| A2 | En la **lateral**, la placa inferior de la columna **se apoya en el piso** (su cara inferior en `z = 0`) | ☐ |
| A3 | La columna **arranca en la cara superior** de esa placa, no en el piso: no la atraviesa | ☐ |
| A4 | La **base** sigue apoyada en el piso, **no** levantada un espesor | ☐ |
| A5 | La cota de la columna de extremo a extremo es la **longitud nominal** pedida, no esa longitud más el espesor | ☐ |
| A6 | Cambia el espesor de la placa inferior a `1.0 in` y vuelve a insertar: la columna **sube** entera y su longitud **no cambia** | ☐ |
| A7 | Con ese espesor, la **placa posterior y el cartabón NO se movieron** | ☐ |
| A8 | Nada del conjunto queda por debajo de `z = 0` | ☐ |

## Bloque B — La vista en planta

| # | Comprobación | ✅/❌ |
|---|---|---|
| B1 | La **base** se ve como una huella rectangular con su longitud real (≈ `6.49 × 48` in), no como una línea | ☐ |
| B2 | Cada **brazo** se ve con su longitud real (≈ `4 × 36` in) | ☐ |
| B3 | La **columna** se ve como su **sección** (≈ `7.96 × 9.73` in), no tumbada | ☐ |
| B4 | Cambia la altura de la columna y vuelve a insertar: **la planta no cambia** | ☐ |
| B5 | Los troqueles de la **placa inferior** —taladrados hacia abajo— se ven **redondos** | ☐ |
| B6 | Los troqueles de **conexión** —taladrados hacia la base— se ven **de canto**, como un trazo | ☐ |
| B7 | Ninguna primitiva se sale de la pieza que la aloja | ☐ |

## Bloque C — La vista lateral

| # | Comprobación | ✅/❌ |
|---|---|---|
| C1 | Se ven **las seis piezas**: columna, base, placa inferior, placa frontal, placa posterior y cartabón | ☐ |
| C2 | Cada **placa se ve con su espesor** (`0.25 in`), no como una línea sin grosor | ☐ |
| C3 | El **cartabón** se ve en verdadera magnitud, apoyado sobre la base y contra la placa posterior | ☐ |
| C4 | Los troqueles de la columna están, y **ninguno queda por debajo** de su cara de apoyo | ☐ |
| C5 | En una estación **doble**, la lateral muestra los brazos de **ambos lados** | ☐ |
| C6 | La lateral de la estación 2 es distinta de la de la estación 1 cuando sus brazos difieren | ☐ |

## Bloque D — Las naturalezas visuales

| # | Comprobación | ✅/❌ |
|---|---|---|
| D1 | Tras insertar, existen las capas `RACKCAD_CANT_COLUMNA`, `_BASE`, `_BRAZO`, `_PLACA`, `_CARTABON`, `_SEPARADOR`, `_TENSOR`, `_TROQUEL` y `_ANOTACION` | ☐ |
| D2 | Columna, base, brazo, placa, cartabón y troquel se distinguen **a simple vista** por su color | ☐ |
| D3 | Apagar `RACKCAD_CANT_TROQUEL` deja ver el acero sin los agujeros, y volver a encenderla los devuelve | ☐ |
| D4 | Los colores del dibujo son **los mismos** que muestra la previa del editor | ☐ |
| D5 | Si ya tenías una capa propia con alguno de esos nombres, **no se le cambió el color** | ☐ |

## Bloque E — El configurador de columna y base

| # | Comprobación | ✅/❌ |
|---|---|---|
| E1 | La ventana de columna/base **ya no pide** los dos márgenes de troquel retirados | ☐ |
| E2 | Su previa muestra la columna con sus troqueles, las tres placas y el cartabón | ☐ |
| E3 | La previa del componente y lo que se inserta son **el mismo dibujo** | ☐ |
| E4 | Un proyecto guardado **antes** de esta corrección se abre sin error y sin avisos sobre los márgenes | ☐ |
| E5 | El BOM de la línea **no cambió** respecto de la ronda 2: un troquel no es una línea de BOM | ☐ |

## Bloque F — Que no se rompió lo que no se tocó

| # | Comprobación | ✅/❌ |
|---|---|---|
| F1 | El **brazo** se dibuja e inserta como en la ronda 2 | ☐ |
| F2 | El **separador** y su placa, igual | ☐ |
| F3 | El **tensor** —estructural y cold rolled con sus adaptadores—, igual | ☐ |
| F4 | `RACKSECCION` sigue dibujando una sección suelta correctamente | ☐ |
| F5 | Los demás sistemas (selectivo, dinámico, push back, cama) abren y dibujan sin cambio | ☐ |

---

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

- **ADR-0027 y ADR-0028 siguen PROPUESTOS.** La nota **N1** de ADR-0024 precisa D7 sin contradecirlo, y se
  presenta al dueño con este paquete.
- **I-37 sigue abierta.** No se integra nada, no se abre I-38.
- **Los colores concretos.** Las pruebas fijan que las seis naturalezas críticas se leen **aparte**, no que
  el verde de la columna sea el mejor verde. Eso es criterio tuyo, y cambiarlo es una línea.
- **La edición independiente** de un componente suelto sigue sin existir, por la misma razón que en la
  ronda 2: exigiría un `RackSystemKind` nuevo o un handler de edición, y ninguno está autorizado.
