# I-37D ronda de corrección — las veintiuna regresiones, verificadas EN ROJO

Estado: **las veintiuna muerden**. Cada defecto se reintrodujo a propósito, se comprobó que la prueba fallaba
y se revirtió. El árbol quedó como estaba y las dos suites volvieron a verde: **2929** y **609**.

## Lo que el ejercicio encontró

Cinco salieron verdes en la primera pasada, y **cuatro de ellas eran problemas míos**, no parches mal
elegidos. Vale la pena leerlas porque tres son la misma trampa.

### Dos pruebas mías eran AUTO-REFERENCIALES

- **R08.** `ElDATUMDeLaPlacaNoSeMovioYElSeparadorMideLoMismo` comprobaba que el corte del separador sale de
  la distancia entre los dos agujeros… calculando lo esperado **desde esos mismos agujeros**. Mover los dos
  a la vez mantiene la relación, así que el separador podía salir de otro largo sin que nadie protestara.
  Ahora la prueba ancla primero cada agujero contra una referencia que **no** es la placa: la cara del alma.
- **R12.** `LaColumnaSigueELEVADASobreSuPlacaInferior` comparaba `Column.Start.Z` con
  `CantileverColumnBaseDatum.ColumnStartZ(...)`, que es **la función bajo prueba**. Devolviera lo que
  devolviera —incluido el suelo— la prueba pasaba. Ahora se mide contra la **cara de apoyo de la placa**.

### Una tenía un hueco real, y encontró un error mío

- **R11.** Al cambiar el plano de espejo corregí también `NearOffset`, y **ninguna prueba lo miraba**. Al
  escribir la que faltaba salió que mi corrección tenía **el signo invertido**. Y al arreglarla apareció algo
  más grande: `CantileverPlatePlan.NearOffset` se documenta como «coordenada del mundo» pero las placas de
  montaje de brazo lo usan como «distancia a lo largo del normal», y la placa inferior de la columna sigue la
  otra. **Las dos lecturas conviven en el sistema desde antes de esta ronda.** Unificarlas toca cuatro
  familias de placa y es un cambio de contrato fuera de este encargo, así que lo que se hizo fue alinear la
  base espejada con las placas de **su mismo eje** y dejar el conflicto escrito en el código y aquí.

### Dos parches estaban mal apuntados

- **R18** no compilaba por cómo quité el bloque; se reescribió como una condición desactivada.
- **R21** rompía el BOM directamente, que no es lo que la prueba vigila. La propiedad real es «un ajuste de
  vista no toca el producto», así que ahora el parche **filtra un ajuste de planta hasta el modelo** — y con
  eso muerde.

## Las veintiuna

| # | Defecto reintroducido | Prueba que lo cazó | |
|---|---|---|---|
| R01 | El brazo vuelve al amarillo | `LaPaletaEsLaQueElDuenoPIDIO` | 🔴 |
| R02 | La ménsula del brazo vuelve al gris | `LaPaletaEsLaQueElDuenoPIDIO` | 🔴 |
| R03 | El tensor se reparte en tres colores otra vez | `LaPaletaEsLaQueElDuenoPIDIO` | 🔴 |
| R04 | La placa columna–separador vuelve a ser una placa de brazo | `TODAPlacaTieneNaturalezaDeclarada…` | 🔴 |
| R05 | Dos roles comparten capa | `CadaROLSigueTeniendoSuPROPIACapa` | 🔴 |
| R06 | La placa vuelve a centrarse en su agujero y cruza el alma | `LaPlacaNOATRAVIESAElAlmaDeLaColumna` | 🔴 |
| R07 | La placa se va al lado equivocado | `LaPlacaNOATRAVIESAElAlmaDeLaColumna` | 🔴 |
| R08 | Mover la placa arrastra su agujero y cambia el separador | `ElDATUMDeLaPlacaNoSeMovio…` | 🔴 |
| R09 | El espejo vuelve al plano `y = 0` | `LaBaseESPEJADANoSeMeteDENTRODeLaColumna` | 🔴 |
| R10 | La base espejada deja de coincidir con su brazo | `LaBaseYElBRAZODelMismoLadoApoyanEnLaMISMACara` | 🔴 |
| R11 | El offset de la placa espejada no sigue al plano | `LaCARADEREFERENCIADeCadaPlacaCoincideConSuCONTORNO` | 🔴 |
| R12 | La columna deja de estar elevada sobre su placa | `LaColumnaSigueELEVADASobreSuPlacaInferior` | 🔴 |
| R13 | Las puntas del ángulo vuelven a escuadra | `ElAnguloSeParecMASAlRealQueAntes…` | 🔴 |
| R14 | El radio de punta se dispara por encima del espesor | `ElRadioDePuntaNuncaSuperaMedioEspesor` | 🔴 |
| R15 | Un ángulo mejor aproximado reclama exactitud | `ElAnguloNOReclamaEXACTITUD…` | 🔴 |
| R16 | La planta vuelve a dibujar brazos y tensores por omisión | `LaPlantaNACEsinBrazosNiTensores` | 🔴 |
| R17 | Un interruptor manda sobre la familia del otro | `ENCENDERLOSLosDevuelve…` | 🔴 |
| R18 | Apagar los brazos deja sus ménsulas colgadas | `ApagarUnBrazoApagaTAMBIENSusPlacas…` | 🔴 |
| R19 | La regla de la planta se cuela en la frontal y la lateral | `LaFRONTALYLaLATERALNoSeEnteran…` | 🔴 |
| R20 | La visibilidad deja de persistirse | `LaVisibilidadSOBREVIVEAlGuardar…` | 🔴 |
| R21 | Un ajuste de VISTA se cuela en el producto | `APAGARLOSNoDescuentaNadaDelBOM…` | 🔴 |

## Lo que NO se comprobó

- **Que se vea bien en AutoCAD.** Ninguna prueba puede darlo: es el paquete de validación manual.
- **Los paneles**, fuera de alcance por decisión del dueño.
