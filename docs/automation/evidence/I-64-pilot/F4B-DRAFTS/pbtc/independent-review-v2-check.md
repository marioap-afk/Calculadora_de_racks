# I-64 F4B PB-TC — Comprobación acotada de la v2 del paquete del contrato

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. Las rutas son relativas a la raíz del repositorio. «V2:n» es la línea *n* de `I-64-f4b-pbtc-gate-contract-package-v2.md`; «ME K-n» es la fila K-n de `pbtc-measurements.md`; «IRT1» es la revisión v1. Se usan los alias de la v2: W, WC, WX, NF, DA, DEA, CS, WLC y S0.

**Veredicto: EMITIBLE TRAS CORRECCIONES. Quedan abiertos 0 bloqueantes, 1 importante y 9 menores (0/1/9).**
- De los 15 hallazgos de IRT1, 14 quedan **RESUELTOS** y 1 **PARCIAL** (M-10). Ninguno queda sin resolver.
- El importante nuevo (N-01) es una contradicción entre D-PBTC-01 e INV-PBTC-03. Se corrige con una frase, sin otra revisión.
- No hace falta ninguna decisión nueva del Owner, además de las cinco del §8.

## 1. Método y límites

**Leído:**
- la v2 completa, con su registro de cambios, el §8.0 y los anexos A y B;
- el apartado K del registro de mediciones;
- `v2-instructions.md`;
- la v1 y IRT1, como referencia.

**Comprobado en la rama, en solo lectura** (HEAD `f9f2afbb`, árbol limpio):
- con lectura y `git grep`:
  - W, WC y WX;
  - `NumericFieldValidation.cs`;
  - DA y DEA;
  - `DynamicRackSystemResolver.cs` (`Resolve` y `Snapshot`);
  - `DynamicRackSystemBuilder.BuildDefault`;
  - `RackModuleEditSession.cs` y `RackModuleReconciliation.cs`;
  - `PushBackEditorState.cs` y `PushBackAdvancedRackParameters.cs`;
  - el shell (`RackEditorVisualShell.cs`, `Themes/Generic.xaml`);
  - `EditorWindowTestSupport.cs`, `PushBackShellAdoptionTests.cs`, T-KB y `PushBackFullEditorCharacterizationModuleTests.cs`;
  - el `StatusContent` del Dinámico;
- con `python -I` de solo lectura, sobre los 17 sobres del corpus: módulos personalizados, avanzados, datum y fondo;
- `git ls-tree` y `git cat-file -e` de las 131 entradas del `ForbiddenWriteScope`;
- el `<Message>` de CT-PB0-01 y CT-PB0-17 en el TRX de UI de la CI del RED de PB-T0. El artefacto local de la entrega existe, ignorado por Git.

**Límites:**
- nada se compiló ni se ejecutó;
- que `Assert.True(false, texto)` no trunque el texto es conducta de xUnit 2.9, no medida;
- el delta de la base de CT-PBTC-10 y la longitud 56 de CT-PBTC-12 los razono por el código, sin ejecutar.

## 2. El estado de los hallazgos de IRT1

| Id | Estado | Evidencia en la v2 y en el código |
|---|---|---|
| B-01 | **RESUELTO** | H-29 (V2:124-130) cita DA:165-168, `DynamicLoadBeamGeometry.cs:52-71`, `DynamicRackDefaults.cs:30` («LARGUERO_IN_OUT_C6») y `secciones.csv:8` («6»); todo comprobado. F4 se fija como `salida=6 CATALOGO` en CT-PBTC-01 (V2:472) y como `diseno=6` en CT-PBTC-02 (i) (V2:480). Se corrigen: el §5.2 (S-03 falla en diez filas y F4 pasa, V2:623), el §5.4 (V2:651) y OD-PBTC-1 («diez», V2:1052-1059). Residuo menor en la definición de la clase (N-02) |
| I-01 | **RESUELTO** | `Val` se conserva para W:1865 (V2:102, V2:296), e INV-PBTC-03 (3) lo fija (V2:760). Solo el anexo A la retira (V2:1224) |
| I-02 | **RESUELTO** | Se añaden:<br>• H-30 (V2:131-135; DEA:44-61 y DA:211-218, comprobados);<br>• CT-PBTC-10 con `pb-02-modulos` (V2:555-569);<br>• la medición en la cadena (D-PBTC-10, V2:407-413);<br>• la transcripción en `ExpectedEvidence` (V2:833-838);<br>• la parada PBTC-C14 (V2:937-940);<br>• el «no medido» en OD-PBTC-1 (V2:1086).<br>La elección de `pb-02-modulos` está bien medida (ME K-3, reproducido): M1 `HeaderStart` 54 manual con cabecera propia y M2 `Separator` 30 manual; `pb-31` tiene solo M3 a 48, y `pb-07` solo M2 a 36. K-1 se confirma en el TRX: cada cadena se corta a 50 caracteres con «···», y solo se ven cinco posiciones alrededor de la flecha (CT-PB0-01: 903 caracteres). Residuos menores: N-04 y N-05 |
| I-03 | **RESUELTO** | H-31 (V2:136-142), el registro (V2:656-658), el anexo A completo (V2:1217-1299) y OD-PBTC-5 (V2:1171-1191). Son cuatro cajas anulables y no seis: `SeparatorCountOverride` es entero y `DerivedPostReinforced`, una casilla (W:1599-1606; comprobado). Residuos menores: N-07 y N-08 |
| I-04 | **RESUELTO** | El paso 3 asigna `"61.2350"` (V2:490). `NumericFieldValidation.Validate` acepta el texto (H-32, V2:143). `CellPalletHeightBox` tiene mínimo 0 exclusivo (WX:351-352). Asignar un `Text` distinto dispara `TextChanged` (NF:138-142). En la base sale `c00=61.235`; con (i), `61.23456`; con (ii), `61.235`. El paso discrimina las dos opciones |
| M-01 | **RESUELTO** | Están las frases «no medido» de R11 y de R7 (V2:1084-1085), la extensión explícita de OB-06.6 (V2:1063) y la sub-decisión R-03 (i)/(ii), con su implementación, su coste y su CT (V2:1090-1097). Las siembras bajo `suppressSync` que cita, comprobadas: W:379, W:441-443, WC:588-590, WC:715-716, W:1562-1563 y W:1766-1767. OD-PBTC-2 añade el borrado del sobre y la biblioteca (V2:1106-1108). La recomendación ya no dice «< 0,02» (V2:1100) |
| M-02 | **RESUELTO** | L2 está en `StatusContent`, detrás de `StatusText` (V2:338-342), con los atributos de `StatusText` (WX:684). Comprobado:<br>• `SetStatus` solo escribe `StatusText` (`UiSupport.SetStatus(StatusText, …)`, W:3574);<br>• `PushBackShellAdoptionTests` comprueba tres nombres sin contar hijos (`:126-147`);<br>• T-KB exige la banda sin paradas (`:232`), y un `TextBlock` no es parada;<br>• `PART_StatusHost` no tiene altura fija (`Themes/Generic.xaml:161-163`);<br>• el precedente `RebuildReportText` existe (`RackDynamicSystemWindow.xaml:368`; `DynamicShellMigrationTests.cs:121`).<br>CT-PBTC-08 mide el lugar con `EditorWindowTestSupport.Descendants(DependencyObject)` (`EditorWindowTestSupport.cs:192-230`), que es público y ya se usa así (`PushBackDimensionViewsWindowTests.cs:137`). L1 queda como alternativa (V2:359), y «los dos modos» está matizado (V2:360, V2:1145) |
| M-03 | **RESUELTO** | 131 entradas (V2:694-718). Reproducido:<br>• 3 proyectos;<br>• 9 carpetas y 34 archivos de la raíz de `src/RackCad.UI`, con la lista idéntica a la de `git ls-tree`;<br>• 5 sistemas y los 7 hermanos de la ventana;<br>• `tests/RackCad.Tests/`, 32 archivos de UI, todos existentes;<br>• los 23 hermanos de WLC: `Workspace/` tiene 24 archivos;<br>• 17 de la raíz.<br>Ninguna ruta permitida queda bajo un prefijo, y el método es el de PB-T0. Con el anexo B quedan 130 |
| M-04 | **RESUELTO** | CT-PBTC-09 se retira, con su archivo y su filtro (V2:553, V2:734), y se deja el hueco del número |
| M-05 | **RESUELTO** | H-08 con `MigrateFirstLevelDatum` (V2:119-122), H-PB-47 (V2:278) y la excepción en OD-PBTC-1 (V2:1062) |
| M-06 | **RESUELTO** | H-12 separa las lecturas de la asignación (V2:149-153). H-18 retira la guarda saltada y añade las cuatro (V2:183-190) |
| M-07 | **RESUELTO** | §4.3, con los pasos 3 y 6 (V2:576-580); la precondición 3 (V2:29-31); el punto informativo (V2:1193-1198) |
| M-08 | **RESUELTO** | Formatos definidos en el §4.1 (V2:450-458), y el literal en un bloque de código (V2:345-347). Queda un ancla imprecisa en D-PBTC-09 (N-09) |
| M-09 | **RESUELTO** | Artefactos de las corridas temporales: V2:625, V2:804, V2:860 |
| M-10 | **PARCIAL** | INV-PBTC-15 es mecánico y razonable (V2:810-816). Pero INV-PBTC-03 (1) contradice el paso 4 de D-PBTC-01 (N-01), y el punto (2) cuenta mal las cajas (N-03) |

## 3. Hallazgos nuevos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| N-01 | **IMPORTANTE** | **INV-PBTC-03 (1) contradice el diseño.**<br>El paso 4 de D-PBTC-01 dice que, en `LoadFromModel`, «cada portador toma el mismo valor que hoy recibe su `SetNumber`, **y la llamada pinta ese portador**». Así, `ToleranceBox.SetNumber(inputs.PalletTolerance > 0.0 ? … )` pasa a `ToleranceBox.SetNumber(<portador>)`, y lo mismo en BeamDepth, PostPeralte y AnnotationScale.<br>INV-PBTC-03 (1) exige que ninguna línea añadida o quitada del diff de W y WC contenga `SetNumber(`.<br>Si el Worker sigue el paso 4, el invariante falla mecánicamente. Si lo esquiva, se aparta de la letra del paso 4. Es la latitud contractual que PB-T0 enseñó a no dejar | V2:294; V2:758; W:386-406 | Elegir una de dos:<br>• (a) el paso 4 dice «las llamadas `SetNumber` de W:386-406 no cambian; cada portador se asigna en una línea propia con la misma expresión» (para el fondo basta `generalPallet.Depth`, que ya es portador, W:385);<br>• (b) INV-PBTC-03 (1) pasa a «las 29 llamadas a `SetNumber` siguen sin segundo argumento, y en W:386-406 solo cambia su argumento por el portador con la misma expresión».<br>Recomiendo (a), que conserva el invariante tal cual |
| N-02 | MENOR | **La clase `CATALOGO` se aplica a todas las filas y clasifica mal F10 en la base.** `ResolveBeamDepth(6.00044)` = 6, y en la base `ClearHeight` sale 6, así que F10 se imprime `CATALOGO` y no `REDONDEA`. No afecta al RED, que falla en F1, ni al GREEN, donde F10 da `CONSERVA`. Pero el §5.1 («diez filas dan salida = R3») no describe la línea que se imprime. Además, el nombre del método, «…TheElevenFieldsKeepTheirExactValue…», ya no es cierto | V2:443-449; V2:592; `DynamicLoadBeamGeometry.cs:52-71` | Definir `CATALOGO` solo para la fila cuya ruta es `Structure.BeamDepth` (F4). O evaluar `REDONDEA` antes de `CATALOGO` en todas las filas salvo F4 |
| N-03 | MENOR | **INV-PBTC-03 (2) cuenta mal las cajas:** «aparecen las llamadas `Keep(` sobre las **mismas once** cajas». Las líneas `Val(` citadas son diez, y la undécima caja (`CellBeamLengthOverrideBox`) cambia dentro de `OptionalVal`, no con `Keep(`. Una comprobación literal encontraría diez | V2:759; W:797-801, :804, :819, :837-839, :845 | «diez llamadas `Keep(` sobre esas diez cajas, y el cambio de `OptionalVal` (W:3625-3626)» |
| N-04 | MENOR | **D-PBTC-10 y PBTC-C14.**<br>• PBTC-C14 mira K1, K2 y K3, pero no K4. El criterio de materialidad de OB-06.4 y de CT-PB0-21 incluye las coordenadas de más de 0,001. En la práctica lo cubre «alguna línea bajo `Modules`», pero no por la letra.<br>• D-PBTC-10 dice que el «único cambio» de la corrida temporal es la aserción final, y la misma viñeta añade el código de K1-K3.<br>• `Assert.True(false, …)` es la forma que xUnit2020 desaconseja; en xUnit 2.9 existe `Assert.Fail(texto)`. Es código temporal | V2:410-413; V2:937; [P]:389 | Añadir K4 a PBTC-C14 (o justificar su exclusión), cambiar «único cambio» por «los cambios temporales» y usar `Assert.Fail` |
| N-05 | MENOR | **La predicción de la base de CT-PBTC-10 es inexacta.** «una línea cada una» no vale para F6: el `Snapshot` escribe `Structure.FirstLevelHeight` desde `Fronts[0].FirstLevelHeight` (DA:161-164; `DynamicRackSystemResolver.cs:306`), así que salen dos líneas. Es inocuo, porque la base se mide en el paso 1 y no se fija, pero la transcripción del Worker no coincidirá con el texto | V2:565; V2:600 | Escribir «al menos una línea» |
| N-06 | MENOR | **Anclas del anexo B equivocadas.** «A0 y A1 salida, iguales a las de CT-PB0-05 (`:198-199`, `:201`, `:204-205`, `:207`)». Las líneas que no cambian son `:199-201` y `:204-206` (A0 salida, A0 controles y A1 salida); `:207` es «pb-07 A1 controles», que **sí** cambia. «A0 y A1 salida» tampoco nombra a A0 controles | V2:1329; `PushBackFullEditorCharacterizationModuleTests.cs:199-208` | Corregir a `:199-201` y `:204-206`, y escribir «A0 salida, A0 controles y A1 salida» |
| N-07 | MENOR | **K-6 no es exhaustivo.** Faltan:<br>• los usos de `GapBox` con `LostFocus` en `PushBackCompositeLifecycleWiringTests.cs:47` y `:55-56`, `PushBackCompositeLoadExistingModulesTests.cs:58-59`, `:114`, `:130`, `:137`, `:153` y `:159`, y `PushBackCompositeWindowTests.cs:119-120` y `:233-234`;<br>• las ediciones de cajas avanzadas en `PushBackAdvancedRackParametersWindowTests.cs:99-217`.<br>La conclusión (ninguna sustitución) se sostiene: los huecos son enteros (54, 14, 12), y las ediciones avanzadas escriben valores distintos, o vacíos, frente a un portador nulo | V2:1287-1292; ME K-6 | Completar K-6 y la lista del A.4 |
| N-08 | MENOR | **CT-PBTC-12, ventana 2: M3 = 56 no está demostrado** (lo admite ME K.2). Por el código es probable que valga:<br>• `SetLength` acepta cualquier longitud mayor que cero (`RackModuleEditSession.cs:185-203`);<br>• la reconciliación conserva la longitud manual y adapta la cabecera propia (`RackModuleReconciliation.cs:186-201`, `:232-237`).<br>Pero ninguna prueba ha editado nunca la longitud de una cabecera. Sí está probado M2 `Separator` a 56 en `pb-31` (CT-PB0-03, `ModuleTests.cs:91` y `:137`) | V2:1270-1271 | Usar M2 de `pb-31`, de 48 a 56, en la ventana 2. O declarar el riesgo en OD-PBTC-5 (A) |
| N-09 | MENOR | **El ancla de D-PBTC-09 no muestra lo que dice.** `PushBackClassicCharacterizationIntentTests.cs:98-102` es `SetNumber` más `LoseFocus`, no `Text` asignado por código. El precedente real de asignar `Text` es `EditorWindowTestSupport.SetText` (`:89-95`), que ya usa CT-PB0-11 | V2:401 | Citar `EditorWindowTestSupport.cs:89-95` |
| N-10 | MENOR | **El §8.0 usa identificadores internos** (PB-TC, PB-T1, PB-T2, «anexo A/B»), aunque el encargo pide evitarlos salvo los nombres de las decisiones. Además, «la escala de las cotas» simplifica la escala de anotación, que también escala los textos | V2:997-1048 | Sustituirlos por descripciones llanas, por ejemplo «antes de la tarea que separa el estado», y escribir «la escala de textos y cotas» |

**Comprobado sin hallazgo:**
- **CT-PBTC-10 en el GREEN da `delta=[]`.** La ventana con el fondo exacto y el piloto copian el mismo baseline. El resolver conserva las longitudes guardadas de los módulos (`DynamicRackSystemResolver.cs:135-200`), y la altura de cabecera se redondea al pie comercial (`DynamicHeaderHeightCalculator.cs:77-84`). F4 da delta vacío en la base y en el GREEN, porque los dos normalizan al catálogo. En la base falla por aserción, en F1.
- **El anexo B:**
  - `ModuleCommit` es público (`PushBackEditorState.cs:139`) y `StandardRestoreRequested` existe (`RackModuleEditSession.cs:40`);
  - `Reset` deja los seis en nulo, con `DerivedPostReinforced = true` (`PushBackAdvancedRackParameters.cs:148-161`), que son las líneas «A1 controles» y «A2» previstas;
  - `LoadAdvancedRackParameters` repinta bajo `suppressSync` (W:1555-1579);
  - CT-PB0-04 y `PushBackModuleEditorWindowTests.cs:295` no chocan, porque `pb-31` tiene los avanzados por defecto.
- **Los recuentos del §6.4 y del §6.9**, en las cuatro variantes:
  - la v2 base: 2 417, con 9 Failed en el RED, 20 Skipped, 2 388 Passed en el RED y 2 397 en el GREEN;
  - con el anexo A: 2 420 / 12 / 20 / 2 388 / 2 400;
  - con el anexo B: 2 418 / 10 / 21 / 2 387 / 2 397;
  - con los dos: 2 421 / 13 / 21 / 2 387 / 2 400;
  - los `MinSelected` 9, 12, 10 y 13, y 16 para PB-T0 con el anexo B.
- **El §8 y el §8.0:**
  - son fieles a T-3 y a OWNER-4, con lo no medido dicho como tal;
  - la recomendación de OD-PBTC-5 (A) declara su coste y su efecto;
  - el §8.0 tiene unas 52 líneas.

  No falta ninguna decisión del Owner antes de emitir. PBTC-C14 ya prevé la vuelta si el delta de la base resulta material.
- **Una muestra de anclas nuevas, correcta:**
  - en W: :385, :1529, :1793, :2093, :3346-3349 y :3417;
  - en WC: :159-175, :715-716 y :721;
  - en WX: :351-352, :492-531 y :677-686;
  - DA:315-317 y `PushBackCompositeEditorAssembler.cs:59`;
  - `StateTests.cs:189-191`.

  La reconciliación por `ModuleId` y `Kind` está en DA:325, `Reconcile`; DA:315-317 elige las intenciones. La cita de la v2 vale para «las intenciones».

## 4. Veredicto

**EMITIBLE TRAS CORRECCIONES. Abiertos: 0 bloqueantes, 1 importante y 9 menores (0/1/9).**

- **N-01 debe corregirse antes de emitir.** Con la opción (a) es un cambio de una frase en el paso 4 de D-PBTC-01, que deja intacto INV-PBTC-03 (1). Aplicado así, no hace falta otra comprobación.
- **Los menores** (N-02 a N-10) son de texto, anclas y definiciones. Conviene aplicarlos en el mismo cambio, sobre todo N-02, N-03 y N-06, que tocan líneas que el Worker o el Controller comprobarán.

**Lo que va al Owner:** las cinco decisiones del §8, tal como están. OD-PBTC-1 incluye la sub-decisión R-03. Además, el punto informativo sobre PB-T2. No hay ninguna decisión material nueva.

Si el Owner elige el anexo A, conviene aplicar antes N-08: en CT-PBTC-12, cambiar la longitud de M2 en vez de la de M3, o declarar el riesgo. Si elige el anexo B, N-06. La v2.1 resultante necesita la comprobación acotada que ya prevé la precondición 2.
