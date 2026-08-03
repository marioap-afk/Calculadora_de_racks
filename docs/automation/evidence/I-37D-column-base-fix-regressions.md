# I-37D corrección de columna y base — las quince regresiones, verificadas EN ROJO

Estado: **las quince muerden**. Cada una se rompió a propósito, se comprobó que la prueba fallaba y se
revirtió. El árbol quedó byte a byte como estaba y las dos suites volvieron a verde: 2848 en
`RackCad.Tests` y 605 en `RackCad.UI.Tests`.

Una regresión que no muerde no es una regresión, es una prueba que da confianza falsa. Ninguna de estas se
presenta como verificada sin haberla visto fallar.

## Método

Para cada una: aplicar el parche mínimo que reintroduce el defecto → ejecutar la prueba que debe cazarlo →
comprobar que falla → revertir. El parche se elige para que reproduzca el defecto REAL y no un error de
compilación cualquiera: una ruptura que no compila no demuestra que la prueba mire lo correcto.

## Lo que el ejercicio encontró, y que no era una regresión sino un defecto vivo

Cinco de las quince salieron **verdes** en la primera pasada. Tres eran parches mal elegidos por mí y se
reapuntaron; **dos eran defectos reales del código**, y esa es la razón de hacer esto en vez de declararlo:

1. **El arranque de la columna se calculaba en DOS sitios.** La precomputación lo pasaba al marco y el
   cuerpo lo recalculaba para acotar los troqueles. Coincidían por aritmética, no por construcción: R01
   devolvió la columna al piso y **movió los troqueles sin mover la columna**. Corregido — la
   precomputación publica `ColumnStartZ` y el cuerpo lo LEE. Es exactamente la corrección arquitectónica
   que el dueño pidió, aplicada a un sitio que no se veía hasta romperlo.
2. **`ColumnTopZ` se había quedado sin consumidor.** El cuerpo escribía `columnStartZ + column.Height` a
   mano, así que la fórmula declarada podía decir una cosa y la columna dibujada otra. Corregido: el
   resolutor usa la fórmula, y la prueba ata las dos —`ColumnTopZ(t, corte) == Column.End.Z`— para que no
   puedan volver a separarse.

Además, R04 pasaba comparando sólo el `Datum`. Se reforzó para comparar también el **centro** que llega al
dibujo: un datum correcto con un centro desplazado es precisamente el fallo silencioso que esta corrección
existía para no reintroducir.

## Las quince

| # | Defecto reintroducido | Parche aplicado | Prueba que lo cazó | Resultado |
|---|---|---|---|---|
| R01 | Devolver la columna al piso | `ColumnStartZ(...)` → `FloorZ` en la precomputación | `CantileverVerticalDatumTests.LaColumnaARRANCAEnElEspesor` | 🔴 ROJA |
| R02 | Levantar la base junto con la columna | El marco de la base suma `+ 0.25` | `…LaBaseNOSeLevantaConLaColumna` | 🔴 ROJA |
| R03 | Sumar el espesor DOS veces al tope | `ColumnTopZ` suma `+ bottomPlateThickness` | `…ElEspesorNoSeSumaDOSVeces` | 🔴 ROJA |
| R04 | Trasladar los troqueles de la columna con ella | El centro de conexión suma `lowerEdgeZ` | `…LosDatumsDeLaPlacaPosteriorYDeLaColumnaCOINCIDEN` | 🔴 ROJA |
| R05 | Colgar la placa inferior bajo el piso | `ColumnBottomPlateBottomZ = FloorZ - 0.25` | `…LaPlacaInferiorOcupaDeCeroAlEspesor` | 🔴 ROJA |
| R06 | Acotar el troquel por un margen y no por su radio | `PunchRadius => 0.0` | `CantileverColumnBaseTests` (2 pruebas) | 🔴 ROJA |
| R07 | Dibujar una placa por el contorno de UNA cara | La extrusión de la silueta se anula | `CantileverPlantaViewTests.CadaPLACAMuestraSuEspesorEnLaPlanta` | 🔴 ROJA |
| R08 | Preguntar a la cámara por el eje Z DEL MUNDO | Vuelve `viewpoint.PreservesSectionShape` | `…LaBaseTieneSuHUELLAYNoUnaLineaSinLongitud` | 🔴 ROJA |
| R09 | Repartir una naturaleza por defecto en vez de fallar | El `default` de `Of(kind)` devuelve `Plate` | `CantileverVisualRoleTests.UnaPiezaSinClasificarFALLAEnCerrado` | 🔴 ROJA |
| R10 | Volver a una sola capa para todo | El materializador escribe un nombre de capa fijo | `…ElPluginNoNOMBRACapasNiColoresPorSuCuenta` | 🔴 ROJA |
| R11 | Que la previa se invente su color de pieza | El pincel se construye con un `Color.FromRgb` literal | `…LaPreviaTampocoSeInventaLaPaleta` | 🔴 ROJA |
| R12 | Colocar un troquel fuera de la placa que perfora | El centro se desplaza 40 in en X | `…TodaPrimitivaCaeDENTRODeLaHuellaDeSuPiezaAnfitriona` | 🔴 ROJA |
| R13 | Dejar de dibujar el cartabón, en silencio | El bucle de cartabones recorre una secuencia vacía | `CantileverLateralViewTests.LaLateralDibujaCadaCARTABONDeSuEstacion` | 🔴 ROJA |
| R14 | Dibujar redondo un troquel visto de canto | El límite de escorzo se hace incondicional | `…UnTroquelPerforadoHACIAABAJOSeVeRedondoEnLaPlanta` | 🔴 ROJA |
| R15 | Que la previa del componente y la línea discrepen | El componente desplaza su columna 1 in en Z | `…LaLateralDelComponenteYLaDeLaLineaCoincidenPiezaAPIEZA` | 🔴 ROJA |

## Lo que NO se comprobó, y por qué

- **Que la corrección se vea bien en AutoCAD.** Ninguna prueba puede darlo. Es lo que el paquete de
  validación manual pide al dueño, y por eso el paquete existe.
- **Que los colores elegidos sean los acertados.** Las pruebas fijan que las seis naturalezas críticas se
  leen APARTE, no que el verde de la columna sea el mejor verde. Eso es criterio del dueño.
