# I-37D ronda 3, punto 7 — las quince regresiones de tensor, verificadas EN ROJO

Estado: **las quince muerden**. Cada una se rompió a propósito, se comprobó que la prueba fallaba y se
revirtió. El árbol quedó como estaba y las dos suites volvieron a verde: 2902 y 605.

## Lo que el ejercicio encontró

Cinco salieron **verdes** en la primera pasada, y tres de ellas eran **debilidades reales de mis propias
pruebas**, no parches mal elegidos:

- **R03 y R04.** Las pruebas del canal y del ángulo medían sobre **los cuatro tensores de la línea juntos**.
  Cuatro ejes paralelos también dan varias curvas y mucha dispersión, así que una regresión que devolvía el
  canal a su eje pasaba desapercibida. Ahora se miden sobre **un** tensor.
- **R09.** El parche quitaba el agujero del modelo en vez de **dibujarlo dos veces**, que es el defecto que
  la prueba vigila. Reapuntado.
- **R15.** No hay forma de que la representación cambie el BOM: es pura y los planes son inmutables. Eso es
  una **garantía de arquitectura**, no algo que se pueda romper. La regresión se reapuntó a lo que sí importa
  comprobar: que el pin del BOM **muerde ante un cambio de producto de verdad** —bajar el diámetro de la
  varilla de 3/4 a 5/8—, de modo que su quietud durante esta ronda significa algo.
- **R13.** La inyección de una pieza suelta en el cruce no llegaba al plan por razones que no conseguí
  aislar, así que **no se declara cazada por ese parche**. Se reapuntó a la forma en que una unión central
  aparecería de verdad: **partir el cuerpo del tensor en el centro**. Así muerde, y por la causa correcta.

## Las quince

| # | Defecto reintroducido | Prueba que lo cazó | Resultado |
|---|---|---|---|
| R01 | El cold rolled vuelve a ser una sola línea | `EnLaFRONTALNingunTensorEsUnaLINEASIMPLE` | 🔴 |
| R02 | El ancho visible deja de ser el diámetro | `ElCuerpoEsUnaBANDADelAnchoDelDIAMETRO` | 🔴 |
| R03 | El canal vuelve a dibujarse sólo por su eje | `UnTensorDeCANALDibujaSuCONTORNOYNoSoloSuEje` | 🔴 |
| R04 | El ángulo se convierte en un rectángulo | `UnTensorDeANGULOConservaSuPerfilLYNoSeVuelveUnRECTANGULO` | 🔴 |
| R05 | El adaptador vuelve a ser un cuadrado plano | `ElAdaptadorTieneSEISPuntosYNoCUATRO` | 🔴 |
| R06 | El adaptador pierde su rotación | `ElAdaptadorNOSaleIGUALEnLosCuatroExtremos` | 🔴 |
| R07 | Los cuatro extremos reciben la misma mano | `LasCUATROManosSeDerivanYNoSeDeclaran` | 🔴 |
| R08 | Se omite un ala del ángulo | `LaLCONSERVASusDOSAlasYSuESPESOR` | 🔴 |
| R09 | Se dibuja DOS veces el agujero de la cara del separador | `ElAgujeroDeLaCARADelSeparadorNoSeDibujaDOSVeces` | 🔴 |
| R10 | Se omite el agujero del cold rolled | `ElAgujeroDeLaVARILLASeDibujaYEsUnCIRCULO` | 🔴 |
| R11 | Se omiten los cartabones | `CadaAdaptadorMuestraSusDOSCartabonesComoTRIANGULOS` | 🔴 |
| R12 | Los dos cartabones se apilan en el mismo sitio | `LosDosCartabonesVanEnLosDosEXTREMOSDelAdaptador` | 🔴 |
| R13 | Se parte el tensor en el centro: una unión central | `ElEJESigueSiendoElDATUMYLaBandaEstaCENTRADASobreEl` | 🔴 |
| R14 | La previa y la inserción suelta usan planes distintos | `ElTENSORSUELTODibujaElMISMOPlanQueDentroDeLaLinea` | 🔴 |
| R15 | Un cambio de PRODUCTO se cuela sin mover el BOM | `ElBomEsElMismo` | 🔴 |

## Lo que NO se comprobó

- **Que se vea bien en AutoCAD.** Ninguna prueba puede darlo: es el paquete de validación manual.
- **Los paneles**, fuera de alcance por decisión del dueño.
