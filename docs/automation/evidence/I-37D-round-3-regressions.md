# I-37D ronda 3 — las quince regresiones, verificadas EN ROJO

Estado: **las quince muerden**. Cada una se rompió a propósito, se comprobó que la prueba fallaba y se
revirtió. El árbol quedó como estaba y las dos suites volvieron a verde.

Una regresión que no muerde no es una regresión, es una prueba que da confianza falsa.

## Método

Aplicar el parche mínimo que reintroduce el defecto → ejecutar la prueba que debe cazarlo → comprobar que
falla → revertir. El parche reproduce el defecto REAL y no un error de compilación cualquiera.

## Lo que el ejercicio encontró

Dos salieron **verdes** en la primera pasada, y una de ellas era un **hueco de cobertura real**:

- **R10.** Romper el aplanado de la frontal en la ruta del **componente suelto** dejaba todo en verde,
  porque las demás pruebas miran la ruta de la **línea**. Son dos constructores distintos y la previa del
  configurador de brazo usa el primero: sin cubrirlo, la ventana podía mostrar el brazo inclinado mientras
  la línea lo mostraba plano — exactamente la clase de discrepancia entre previa y dibujo que esta
  iniciativa lleva tres rondas cerrando. Se añadió
  `LaFRONTALDelCOMPONENTESUELTOTampocoLaDibuja`.
- **R15.** Apuntaba a la guarda del tercio de anchura, que para un tubo es inalcanzable: lo rechaza antes
  la comprobación de que el tramo de material contenga el centro. Se reapuntó a esa primera, que es la que
  hace el trabajo.

## Las quince

| # | Defecto reintroducido | Prueba que lo cazó | Resultado |
|---|---|---|---|
| R01 | Crear las capas sólo en una de las tres puertas que dibujan curvas | `…LaCapaLaGarantizaQuienAPPENDEALasCurvasYNoCadaPuerta` | 🔴 ROJA |
| R02 | Sacar del rojo una pieza del conjunto columna–base | `…ElCONJUNTOColumnaBaseSeLeeEnteroENROJO` | 🔴 ROJA |
| R03 | Devolver el troquel al color del acero | `…ElTROQUELSeLeeEnBLANCOYNoComoElAceroQuePerfora` | 🔴 ROJA |
| R04 | Teñir de rojo también la placa de montaje de un brazo | `…UnaPlacaDeBRAZONoSeTinneDelRojoDelConjunto` | 🔴 ROJA |
| R05 | Volver a la orilla de 1.5 in | `…TheDefaultsAreTheOnesTheOwnerApproved` | 🔴 ROJA |
| R06 | Acotar la fila desde la COLUMNA y no desde la placa | `…TheRowsAreDerivedFromTheREARPLATEAndNotFromTheColumn` | 🔴 ROJA |
| R07 | Devolver la pendiente del brazo a cero | `…LaPENDIENTEPorOmisionEsSieteDieciseisavosPorDoce` | 🔴 ROJA |
| R08 | Quitar el default aprobado del margen vertical | `…ElMARGENVerticalPorOmisionEsDOSPulgadas` | 🔴 ROJA |
| R09 | Volver a una cuenta de troqueles FIJA | `…LaCuentaSaleDeLosQueCABENEnLaAlturaDelPerfil` | 🔴 ROJA |
| R10 | Dibujar la inclinación también en la frontal del componente | `…LaFRONTALDelCOMPONENTESUELTOTampocoLaDibuja` | 🔴 ROJA |
| R11 | Aplanar también la lateral, que es donde se mide la pendiente | `…LaLATERALSIDibujaLaInclinacionYEsDondeSeMide` | 🔴 ROJA |
| R12 | Volver a la descomposición del espejo que voltea la Y local | `…ElEspejoConservaLaVERTICALDeLaSeccion` | 🔴 ROJA |
| R13 | Devolver el arriostramiento a la cara del patín | `…EnPLANTAElSeparadorPasaPORDENTRODeLaColumna` | 🔴 ROJA |
| R14 | Medir el claro del separador de patín a patín | `…ElCLARODelSeparadorSeMideDeALMAAALMAYNoDePatinAPatin` | 🔴 ROJA |
| R15 | Dejar que un tubo reciba el arriostramiento sin tener alma | `…UNTUBONoTieneDondeRecibirElArriostramientoYSeDICE` | 🔴 ROJA |

## Lo que NO se comprobó, y por qué

- **Que se vea bien en AutoCAD.** Ninguna prueba puede darlo; es el paquete de validación manual.
- **El punto 7 del encargo** —tensores con espesor y adaptadores de ángulo con forma de L— **no se hizo en
  esta ronda**, así que no hay regresión suya. Ver el informe de cierre.
