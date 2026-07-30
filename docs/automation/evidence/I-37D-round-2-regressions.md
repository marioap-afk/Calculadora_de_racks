# I-37D ronda 2 — las doce regresiones, verificadas EN ROJO

Estado: **las doce muerden**. Cada una se rompió a propósito, se comprobó que la prueba fallaba **por la
causa esperada** y se revirtió. El árbol quedó byte a byte como estaba (`git diff -- src` vacío) y las dos
suites volvieron a verde.

Una regresión que no muerde no es una regresión, es una prueba que da confianza falsa. Ninguna de estas se
presenta como verificada sin haberla visto fallar.

## Método

Para cada una: aplicar el parche mínimo que reintroduce el defecto → ejecutar la prueba que debe cazarlo →
comprobar que falla → revertir. El parche se elige para que reproduzca el defecto REAL y no un error de
compilación cualquiera.

| # | Defecto reintroducido | Parche aplicado | Prueba que lo cazó | Resultado |
|---|---|---|---|---|
| 1 | Volver a poner parámetros de componente en la ventana principal | Se añade `ColumnPlateThicknessBox` al XAML de `RackCantileverWindow` | `CantileverRoundTwoSourceGuardTests.LaVentanaPrincipalNoVuelveATraerParametrosDeCOMPONENTE` | 🔴 ROJA |
| 2 | Sustituir el selector buscable por un `ComboBox` | El picker del separador se cambia por `<ComboBox x:Name="SectionBox" />` | `…LosConfiguradoresEligenSeccionConElSelectorBUSCABLE` | 🔴 ROJA |
| 3 | Resolver una sección mediante búsqueda parcial | El ensamblador de línea usa `StructuralSectionSearch.Choices` | `…LaBusquedaParcialNoSeUsaComoRESOLUCION` | 🔴 ROJA |
| 4 | No actualizar la base con `BaseFollowsColumn = true` | `SelectColumn` deja de escribir la base | `CantileverColumnBaseFollowTests.SeleccionarColumnaPoneLaBaseIgualCuandoEsElegible` | 🔴 ROJA |
| 5 | Sobrescribir la base con `BaseFollowsColumn = false` | Se elimina la guarda `if (!Template.BaseFollowsColumn) return;` | `…CambiarLaColumnaConSeguimientoApagadoConservaLaBase` | 🔴 ROJA |
| 6 | Omitir los troqueles regulares | El constructor de vistas deja de emitir `station.Punches` | `CantileverPunchRepresentationTests.LosTroquelesRegularesDeColumnaEstanTodos` | 🔴 ROJA |
| 7 | Omitir el agujero de las placas de separador | Se quita el `AddPunch` de `plate.Punch` | `…LasPlacasDeSeparadorSeDibujanConSuAgujeroCentral` | 🔴 ROJA |
| 8 | Preview del componente distinto del bloque insertado | La inserción construye `Planta` donde el preview muestra `Frontal` | `CantileverComponentEditorTests.ColumnaBase_ElPreviewYLoQueSeInsertaSonELMISMOPlan` | 🔴 ROJA |
| 9 | Cancelar una subventana y mutar el original | El estado del editor deja de copiar (`template` en vez de `template.DeepCopy()`) | `…ColumnaBase_CancelarTrasVariosCambiosNoMutaElOriginal` | 🔴 ROJA |
| 10 | Regenerar una vez POR CELDA al aplicar un alcance | `Restaurar` recorre las celdas del alcance llamando a `Recompute()` en cada una | `CantileverEditorWindowTests.UnaOperacionDeMatrizProduceUNASolaRegeneracion` | 🔴 ROJA |
| 11 | Insertar un componente usando el GUID de la línea | Se introduce un `RackId` en la petición de componente | `…ElComponenteSueltoNoUsaElIdDeLaLinea` | 🔴 ROJA |
| 12 | Ocultar el diagnóstico del componente hasta volver a la principal | El `DiagnosticsText` del tensor se mueve fuera de la ranura de diagnósticos y se colapsa | `…CadaConfiguradorMuestraSusPropiosDiagnosticos` | 🔴 ROJA |

## Por qué estas doce y no otras

Las seis primeras protegen los seis MOTIVOS del rechazo de la ronda 1, una por una. Las seis siguientes
protegen las propiedades que la reestructuración introdujo y que son fáciles de perder sin notarlo: que el
preview y el bloque sean el mismo plan, que cancelar no mute, que una operación de matriz sea una sola
regeneración, que un componente suelto tenga identidad propia y que sus diagnósticos se vean donde se
provocan.

La número 10 merece una nota: una regeneración por celda **no se ve**. El dibujo sale igual; lo único que
cambia es que una góndola doble de doce celdas reconstruye doce veces. Por eso se cuenta, en vez de
mirarse.

## Guardas que la ronda añadió

37 guardas de fuente en `CantileverRoundTwoSourceGuardTests`, sobre el código leído como TEXTO, porque una
arquitectura no la protege una prueba de comportamiento: nada falla en tiempo de ejecución si mañana
alguien vuelve a meter el `SectionId` de la columna en la ventana principal.

Cubren: los parámetros de componente fuera de la ventana principal (15 controles), las cuatro tarjetas, el
formulario del brazo fuera de la matriz, el selector buscable en los cuatro configuradores, que el selector
no lea CSV ni conozca Cantilever, que la búsqueda no se use como resolución, que la regla del *follow* no
vuelva al code-behind ni se deduzca comparando ids, que ninguna ventana reconstruya geometría de preview,
que el preview y el materializador consuman el mismo tipo de plan sin proyectar, que ningún proyecto fuera
del Plugin nombre `Autodesk.`, que el componente suelto no escriba payload ni kind ni use el id de la
línea, que el punto se pida antes de crear nada, que no haya un `Recompute` dentro de un bucle de celdas,
que las cuatro rutas de configurador comprueben el resultado nulo, que los cuatro editen una copia y que
los cuatro muestren sus propios diagnósticos.

**Ninguna guarda vigente se debilitó en esta ronda.** Las dos que la ronda 1 re-apuntó —el dueño único de
la carga del catálogo y la cardinalidad del registro de kinds— están documentadas en sus propios commits, y
las dos quedaron más estrechas, no más laxas.
