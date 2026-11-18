# I-64 F4B PB-TC: revisión adversarial independiente del código (intento 1)

**Objeto:** `git diff 48fe199e3a024d98be28dc697c1e9ce0324e95a5 4440f67d4c76fb2ec019a566231353acdcca61f6`. El RED es `f665edac` y el GREEN, `4440f67d`, en la rama `architecture/workspace-persistente-rackcad`.

**Cómo se hizo:** en solo lectura, con `git show`, `git diff` y `git grep` sobre los SHA exactos, y leyendo los artefactos de la entrega. No se compiló, no se ejecutaron pruebas ni `gh`, y no se escribió en el worktree.

**Notación:**
- Las líneas de W, WC y WX son las del GREEN, salvo que se diga «base».
- `…/069b/` es `artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T021306Z-069b/`.

## Veredicto

**ACEPTABLE CON MENORES. B/I/M = 0/0/5.**

El código hace lo que el paquete v2.2 fija línea a línea en D-PBTC-01, 02, 03 y 11:
- el alcance es exacto;
- el RED cumple el criterio mecánico;
- la CI del GREEN da el recuento esperado;
- la medición de PBTC-C14 se sostiene con su TRX.

Los cinco menores no exigen rehacer el trabajo. Uno de ellos (M-01) es un hueco del diseño del paquete en «Ambos lados» con un campo mixto, y tiene que conocerlo el Owner (OWNER-1).

## Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **M-01** (OWNER-1) | Menor | **Con «Ambos lados» y un campo MIXTO** (A y B difieren en más de 1e-6), el número que se escribe se aplica a los dos lados solo si no «se ve igual» al valor de ese lado. Si se ve igual, ese lado conserva su exacto. **Ejemplo:** A=61,23456 y B=60. El campo está vacío porque es mixto; el usuario escribe `61.235`. Resultado: A sigue en 61,23456 y B pasa a 61,235. Los lados siguen distintos y, en cuanto el panel se recarga, el campo vuelve a vaciarse. **Por qué choca:** contradice el contrato del propio campo mixto, «en cuanto el usuario escribe un numero ese numero se aplica a los dos». Contradice también la letra de la opción del Owner, «si escribes algo distinto, manda lo tuyo»: el campo mostraba vacío. **Qué cubre el paquete:** solo el caso NO mixto («con una caja no mixta, cada lado conserva…», §3 D-PBTC-01). No lo fija ningún CT | `Keep` compara con la celda del lado que se escribe: W:818-822 (`ReadCellValues(side)`, W:802-808) y W:3685-3686. `OptionalVal`: W:3666-3668. Vaciado y aviso del campo mixto: WC:579-583. `Differs` con 1e-6: WC:506-508. Contrato del campo: WC:438-441. El mixto solo se repinta al recargar el panel: WC:634, desde W:489 | Ninguna por parte del Worker: el código sigue al pie de la letra el `Keep` del paquete (D-PBTC-01, pasos 1 y 2). **Lo decide el Owner:** (a) aceptarlo como consecuencia de R-03 (i) y declararlo; o (b) una variante en W en la que un número escrito en un campo `IsMixed` siempre manda. Sería un cambio de diseño, es decir, otra tarea o una enmienda del contrato |
| **M-02** | Menor | **CT-PBTC-08 es en parte tautológica.** <br>• `control=False` no puede fallar: `Aviso` ya exige el tipo exacto `TextBlock`, que no es un `Control`. El compilador lo detectó (CS0184 en `SideBNoticeTests.cs(118,74)`), y el aviso se silenció con un `(DependencyObject)` en vez de cambiar la comprobación. <br>• `enfocable` solo lee el valor por defecto. <br>• «No bloquea nada» solo mira `UpdateButton`, mientras INV-PBTC-05 dice «ningún botón» | RED `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs:191`; `PushBackDeclaredCorrectionSideBNoticeTests.cs:118-123`. `…/069b/tests/e1/run.log` y `…/069b/tests/m10/run.log`: CS0184 en (118,74) | Informativa. Las líneas son las del §4.2, así que no se toca en PB-TC. Para reforzarla más adelante: comparar `IsEnabled` de todos los botones de acción con el aviso visible y sin él |
| **M-03** | Menor | **Lo que ningún CT fija.** La extensión declarada de (a) se cumple al leer el código (tabla de diseño), pero no la fija ningún CT en: <br>• «Ambos lados»; <br>• el alcance; <br>• los datos del frente; <br>• «Restaurar»; <br>• escribir lo mismo en una caja del RACK (la ruta de los portadores); <br>• editar una caja del rack y volver al valor inicial. <br>D-PB-11 no tiene caso negativo: que un commit de módulos sin el flag, o un restore individual, NO reinicie los avanzados. Viene de la lista cerrada del §4, no de la entrega | W:1346, W:1510, W:3328, W:838-844. CT-PBTC-03 (`PushBackDeclaredCorrectionRoundingTests.cs:162-210`) solo edita `AnnotationScaleBox` y `CellPalletHeightBox` | Informar a la sesión. Candidatos para PB-T1, PB-T2 o Smoke-PB |
| **M-04** | Menor | **Evidencia de la entrega.** <br>• **Rutas:** las de los artefactos llevan el prefijo `System.Xml.XmlElement/` (Evidence 2, 31, 35, 37 y 44). En `TestsExecuted[*].Command` y `ResultsFile` hay tokens pegados, por ejemplo `"e1 tests/…csproj FullyQualifiedName~… RED /tests/e1/e1.trx"`. ExpectedEvidence pide rutas relativas resolubles. <br>• **`TreeSha`:** el campo lleva un SHA de COMMIT, y en red1 a red12 y en g1 a g3, gu1 a gu7 y gcore nombra commits que aún no existían cuando se corrió. red12 corrió a las 20:31:14 y el RED es de las 20:31:54; g1 corrió a las 20:35:30 y el GREEN es de las 20:44:26. Que el árbol sea el mismo no se puede probar con los artefactos. <br>• **El CS0184:** UnexpectedFindings lo sitúa en la build Release del RED, pero los logs lo muestran en las corridas Debug e1 y m10; r1 (Release) tiene 0 avisos CS | `…/069b/worker-handoff.json` (TestsExecuted y Evidence). Horas de los commits: `git log`. Horas de las corridas: los `run.log`. CS0184: `…/069b/tests/r1/build-ui.log` | La sesión corrige las rutas en su registro. La sustancia la cubre la CI sobre los SHA exactos (RED en failure solo por los 10 nuevos; GREEN 2418/0/21) |
| **M-05** | Menor (latente) | **Riesgo de bloqueo en CT-PBTC-14.** Entre `RestoreAllModulesButton` y `ConfirmModuleButton` la sesión de módulos tiene cambios pendientes. Si el segundo `Click` lanzara, el `finally` cerraría con trabajo pendiente: `OnClosing`, luego `EditorClosePolicy.MayClose` y un `MessageBox` real. Ninguna clase nueva sustituye el prompt, así que la suite se colgaría (PBTC-C09). Es el patrón heredado de CT-PB0-05. No ha ocurrido: la CI y las tres corridas locales pasan | `PushBackDeclaredCorrectionStandardRestoreTests.cs:54-55` y `:62`; W:3435-3441 (`OnClosing`, `EditorClosePolicy.MayClose` en W:3437) | Ninguna obligatoria. Opcional en otra tarea: confirmar o cancelar en un `finally` interno |

## Comparación de las listas de alcance

| Comprobación | Resultado |
|---|---|
| `git diff --name-only 48fe199e..4440f67d` frente a `AllowedWriteScope` | 13 rutas cambiadas = las 13 permitidas, exactas. No sobra ni falta ninguna |
| Frente a `ForbiddenWriteScope` (131 entradas, por prefijo y por ruta) | 0 coincidencias |
| `git diff 48fe199e..4440f67d -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin`, más `tests/RackCad.Tests` y `*.csproj` | Vacío |
| RED (`48fe199e..f665edac`) | 10 archivos: los 6 nuevos y los 4 de SUS. Ninguno de `src/` |
| GREEN (`f665edac..4440f67d`) | W, WC y WX. Ninguno de `tests/` |
| SUS-01 a SUS-04 | Una línea quitada y una añadida por archivo: el atributo, en WLC:231, `PushBackDimensionViewsWindowTests.cs:308`, `…RoundingLossTests.cs:129` y `…ModuleTests.cs:170`. Los cuatro literales coinciden byte a byte con INV-PBTC-06 y §5.2, y cada uno está en la prueba de su fila |
| INV-PBTC-15, hunks de `-U0` en la base | W en :102 (portadores), :389, :391, :392, :402 y :406 (LoadFromModel), :607 (aviso), :681 y :682 (D-PB-11), :797-801 (ReadCellValues), :813, :819, :837-839, :845 y :846 (ReadInputs), :1217 (`loadedDimensionStyle`) y :3626 (OptionalVal + comparador). WC: :72, es decir, la constante y el método entre miembros. WX: :684. **Ninguno** cae en la lista FUERA (Gap*, ModuleLength*, avanzados, Val ni IntVal) |
| INV-PBTC-03 | Ninguna línea `SetNumber(` añadida ni quitada. Hay 10 llamadas `Keep(` más la declaración (W:818-822, W:839 y W:841-844). `Val` sigue en W:3634 y su único uso, W:1905, no cambia |
| INV-PBTC-19 | Una línea antes de `ClearModuleCommit` (W:696) y el `if` con exactamente las dos sentencias (W:698-702), sin `RequestRecompute(`. W no contiene `forceRebuild:` ni `forceRebuild)`: solo el `<c>forceRebuild</c>` de la base, en W:2535. `RequestStandardRestore()` sigue en W:2539. Dos extras inocuos: un comentario al final de W:696 y una línea en blanco |
| H-31 y la lista FUERA | No se toca (ver los hunks) |

## Diseño frente al código

| Decisión | ¿Conforme? | Evidencia y lectura |
|---|---|---|
| **D-PBTC-01** (a) | **Sí** | **Paso 1:** `ShownFormat`, `SameShown` (invariante y ordinal) y `Keep`, tras OptionalVal y antes de IntVal (W:3670-3686). Es el cuerpo de Cantilever (`RackCantileverWindow.xaml.cs:373-384`). <br>**Paso 2:** W:818-822. <br>**Paso 3:** W:3666-3668. La semántica de nulo se conserva: vacío y no mixto da null, vacío y mixto da el lado; con valor, el lado si se ve igual. <br>**Paso 4:** los portadores están en W:106-109; cada asignación va justo tras su `SetNumber` sin cambiarlo (W:398, :401, :403 y :419). Al principio de `ReadInputs` (W:838-844): `generalPallet` se sustituye, no se muta, y los inputs leen los portadores (W:851, :869-871 y :877). <br>**Paso 5:** `Val` se conserva. <br>**Lectores:** el único lector de las 11 cajas es `ReadInputs`/`ReadCellValues` (grep de los nombres). <br>**Portadores:** solo se escriben en LoadFromModel, que sirve a todas las cargas y a «Restaurar» (W:3328), y en ReadInputs. No se desfasan: una pasada con error sale antes de leer (W:625-633) y la caja sigue siendo la fuente; una carga nueva reasigna los cinco. <br>**`Keep` en cada ruta:** <br>• carga y biblioteca: LoadFromModel; <br>• selección: el commit, luego la nueva siembra y después `Keep` (W:1094-1096); <br>• «Ambos lados»: `ReadCellValues(side)` por lado; <br>• `SetMixed`: caja vacía, así que devuelve el valor del lado; <br>• alcance y datos del frente: W:1346 y W:1510. <br>**Excepción:** M-01 |
| **D-PBTC-02** (b) | **Sí** | El portador está en W:1256, se asigna en W:414 y viaja en W:879. WX no tiene ningún control nuevo. Un rack nuevo sale con null (CT-04, `nuevo estilo=<null>`, en verde en la CI) |
| **D-PBTC-03** (D-PB-08) | **Sí** | **El elemento:** WX:685 es el último hijo del `StackPanel` de `StatusContent`, tras `StatusText`, con los atributos literales del paquete y sin `Text`. <br>**El texto:** la constante de WC:76-77 es igual byte a byte al literal de [OPD], pregunta 3, y al del paquete (comprobado por bytes). <br>**La regla:** WC:85-96 solo lee `SideBPresent` e `IsSlotPresent`; da texto y Visible, o vacío y Collapsed. <br>**Dónde se evalúa:** W:621, en toda pasada efectiva y antes del corte por error. Todos los gestos de presencia terminan en `RequestRecompute`. <br>**Lo que no hace:** no toca botones, estado, diseño, casilla ni `StatusText` |
| **D-PBTC-11** (D-PB-11) | **Sí** | W:696-702. El flag solo nace en `RequestStandardRestore` (W:2539; `RackModuleEditSession.cs:682`). Se borra en `Cancel` y en `Commit` (`RackModuleEditSession.cs:691`, `:727`) y en toda carga (`PushBackEditorState.cs:99-105`; `.Load.cs:43`, `:68`). **No hay reinicio sin pedirlo:** ni con un commit sin el flag, ni con un restore individual, ni tras una carga nueva. `LoadAdvancedRackParameters` repinta bajo `suppressSync`, así que no hay reentrada |

## Los CT

| CT | Clase.método (mapa 4.4) | ¿Líneas del §4.2? | Valoración |
|---|---|---|---|
| 01 | `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_…` (`:46-90`) | Sí, 11 | **Bien:** la misma construcción que CT-PB0-19 y la clase `CATALOGO` solo en F4 |
| 02 | `…RoundingTests.CT_PBTC_02_…` (`:92-160`) | Sí, 13 | **Bien:** la petición real de «Actualizar», con el sistema y la biblioteca sin pulsar |
| 03 | `…RoundingTests.CT_PBTC_03_…` (`:162-210`) | Sí, 6 | **Bien** en lo que promete, que incluye R-03 (i) con un texto distinto que dispara `TextChanged`. **Parcial** en los portadores del rack (M-03) |
| 04 | `PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_…` (`:25-80`) | Sí, 6 | **Bien:** la construcción y el gesto de `T19_H1`, las cuatro cargas y el rack nuevo |
| 05 | `…DimensionStyleTests.CT_PBTC_05_…` (`:94-130`) | Sí, 2 | **Bien:** el diseño, el sistema y cada cota, con el orden ordinal |
| 06 | `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_…` (`:19-56`) | Sí, 14 | **Bien:** más fuerte que SUS-01 |
| 07 | `PushBackDeclaredCorrectionSideBNoticeTests.CT_PBTC_07_…` (`:39-103`) | Sí, 13 | **Bien:** los gestos de CT-PB0-09 y CT-PB0-07, copiados fielmente, y el literal |
| 08 | `…SideBNoticeTests.CT_PBTC_08_…` (`:105-136`) | Sí, 3 | **Débil en parte (M-02):** `control` es tautológico y «no bloquea» solo mira «Actualizar». El lugar (banda de estado y no barra lateral) sí lo fija |
| 10 | `…PilotParityTests.CT_PBTC_10_…` (`:58-91`) | Sí, 12 | **Bien.** F4 es trivial por diseño, y M-02 cubre los módulos personalizados |
| 14 | `PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_…` (`:39-81`) | Sí, 10 | **Bien:** fija a la vez el portador (A2) y el repintado (A1 controles), y seis líneas iguales a CT-PB0-05. No tiene caso negativo (M-03) |

**Reglas de las ventanas** (D-PBTC-08 y 09, INV-PBTC-10 a 12), que se cumplen en todas:
- `var w = Ventana(); try { carga; … } finally { Cerrar(w); }`, también en `Paridad` (`Support.cs:141-154`);
- sin `Abrir`, `Show`, `MessageBox`, BOM, biblioteca, configurador ni «Insertar» (grep);
- solo los mecanismos de D-PBTC-09;
- las costuras, de H-28 o públicas (`InsertionRequest`, `InsertRequested`, `UpdateOnly` y `DesignToInsert`, W:255-264);
- `Sta` con la cultura invariante, el GUID fijo, «R», el orden ordinal y sin reloj;
- las cinco clases van en `EditorDiscardPromptCollection` y son `public sealed`, con un único `Assert.Equal` final.

## El RED (CI, `…/069b/red-ci-ui/ui.trx`)

- **El filtro con ExpectRed:** 10 de 10 en Failed.
  - Ningún mensaje lleva el prefijo de tipo de excepción.
  - La primera línea del StackTrace está siempre bajo `tests/`: en el propio método, o en `Support.Aviso` para CT-07 y CT-08, que es un fallo de `Assert.IsType` y por tanto de aserción.
  - Ninguno queda en NotExecuted. Failed = Selected = 10.
- **Las primeras diferencias** coinciden con el §5.1: CT-01 y CT-02 en F1, CT-03 en la línea 1, CT-06 en la posición 5, CT-10 en F1 y CT-14 en la posición 3.
- **El resto:**
  - UI: 2418 resultados, 2387 Passed, 10 Failed y 21 NotExecuted. Son los 17 de la base más SUS-01 a SUS-04, y no hay ningún otro Failed.
  - Core (`red-ci-core`): 19 070 de 19 070.

## La medición de PBTC-C14 (`…/069b/tests/m10/`)

- **El código temporal** (`temp-measure-k.cs.txt`) copia `Compute`, `Signature`, `MaxDelta`, K1/K1o, K2, K3 y K4/K4'/K5 de `RoundingEffectTests.cs:107-245`, con los mismos umbrales: 0,001 y 1e-9 (`:29-30`). Compara la salida de la ventana con la del piloto mediante `BuildFrom`. Es lo que pide D-PBTC-10.
- **El TRX** (`m10.trx`) contiene el mensaje completo.
  - F1 y M-02: solo `$.PushBack.Structure.PalletDepth: 48 -> 48.00044`, sin líneas bajo `Modules` ni `HeaderLineOverrides`.
  - K de F1 y de M-02: `K1=[] K2=False K3=False K4=[]`, `material=False` y `maxDelta=0`.
  - **La conclusión «no material» se sostiene.**
- **La corrida es anterior al RED:** 20:26, frente al commit de las 20:31. La compilación llevaba la versión previa de `SideBNoticeTests.cs:118` (M-04), lo que no afecta a CT-10.
- **Informativo:** en la base, F2 añade cuatro líneas `Modules[n].Header.PostPeralte`. Vienen del valor exacto de un campo corregido: no es PBTC-C03, y PBTC-C14 solo mira F1 y M-02.

## Las sustituciones (`…/069b/tests/s1` a `s4`)

| Fila | Qué muestra | ¿Cobertura completa? |
|---|---|---|
| SUS-01 (s1) | 14 resultados: fallan pb-06, pb-07 y pb-08 y pasan las otras 11 | Sí. CT-06 tiene las mismas 14 filas y es más estricta: un delta vacío también excluye las líneas «OUTSIDE» |
| SUS-02 (s2) | `Assert.Null` falla con `"ESTILO_PB"` | Sí. `EstiloPb` es la copia exacta de `SingleSided(null, "ESTILO_PB")`, porque `Policy(null)` es null (`PushBackDimensionViewsWindowTests.cs:51-52`, `:64-90`) |
| SUS-03 (s3) | Diez líneas en CONSERVA; F4 = `salida=6 REDONDEA`, igual que la esperada vieja | Sí: las mismas 11 filas y la misma construcción |
| SUS-04 (s4) | Difieren cuatro líneas (A1 controles y A2 de pb-06 y pb-07) y coinciden seis | Sí: los mismos pasos y diseños |

Las cuatro corridas se hicieron sin commit, antes del GREEN (M-04).

## GREEN, estabilidad y contención

- **CI del GREEN** (`ci-ui`, `ci-core`):
  - UI: 2418 resultados, 2397 Passed, 0 Failed y 21 NotExecuted. Es la cifra del §6.9.
  - Core: 19 070 Passed.
  - Por filtro: Cantilever 67, T1B UI 55, PB-T0 UI 16 + 2, WLC 3 + 1, DimensionViews 12 + 1, avanzados 9, módulos UI 13 y nuevos 10. En Core: T1B 10, PB-T0 3, BlankFront 9 y módulos 15.
  - Las horas de los TRX (02:45-02:48Z) siguen al commit (02:44Z). Los runners son de GitHub.
- **Estabilidad:** no se ve dependencia de orden, de reloj, de cultura, de rutas de máquina (la raíz del repo se busca por `RackCad.sln`) ni de foco. Ninguna ventana se muestra y todas se cierran. Riesgo latente: M-05.
- **Contención:** solo hay lecturas (los fixtures del repo y el catálogo por `FromBaseDirectory`). Ninguna prueba escribe en disco ni en el perfil, y no se carga el Plugin ni AutoCAD.

## Hallazgos OWNER

1. **OWNER-1 (= M-01).** En «Ambos lados», con un campo que muestra estado mixto, escribir un número que se ve igual al valor exacto de uno de los lados no lo unifica.
   - Ese lado conserva su exacto, y el campo vuelve a vaciarse como mixto.
   - No está declarado: el paquete y el registro solo describen el caso no mixto, y choca con la letra de la opción elegida («si escribes algo distinto, manda lo tuyo»).
   - El efecto numérico es menor que la precisión mostrada, 0,0005.
   - Se alcanza con datos de más de tres decimales, que el panel del Workspace ya escribe hoy (H-PB-37).
   - El Owner decide si lo acepta y se declara, o si pide la variante en la que «un número escrito en un campo mixto siempre manda».
2. **Recordatorio, ya decidido (R-03 (i)):** no se puede fijar directamente un valor que se vea igual al exacto guardado, por ejemplo redondear 61,23456 a 61,235. Hace falta pasar antes por un valor distinto. No es nuevo.

## Lo no verificado

- **Lo que dependía de ejecutar:** no se compiló ni se ejecutó nada. Las conclusiones sobre los TRX y los logs son lecturas de artefactos.
- **Los trabajos de la CI y sus avisos:** sin `gh` no se pudo ver el estado de cada trabajo de la CI (RED: `ui-tests` en failure y los demás en success; GREEN 4/4), ni los recuentos de avisos de la CI, ni el vínculo de cada TRX con su run y su SHA más allá de las horas y los runners. Localmente, `r1` y `rg1` (Release) igualan la línea base: Core 132 líneas xUnit, UI 28 y Plugin 104 MSB3277, sin avisos CS.
- **Las corridas locales:** que se hicieran sobre un árbol idéntico a los commits (M-04).
- **La geometría de D-PB-11 en los dos flujos** («Confirmar» y luego «Actualizar», frente a «Actualizar» con el restore pendiente).
  - En la pasada que consume el restore, el ensamblador calcula `headerHeight` con el manual aún sin reiniciar y reinicia después (`PushBackEditorDesignAssembler.cs:191-193` frente a `:300-305`).
  - El resolver vuelve a derivar la altura de `ManualHeaderHeightOverride` (`DynamicRackSystemResolver.cs:117`, `:131`), lo que sugiere que las dos salidas coinciden. Sin medir.
  - CT-14 solo fija los seis parámetros. Queda para Smoke-PB.
- **Fuera del alcance de esta revisión:** el dibujo real en AutoCAD, el modo WORKSPACE (PB-T2) y la presentación del aviso con teclado y foco reales.
- **Lo razonado:** que `SameShown` en cultura invariante equivale a «se ve igual» en cualquier cultura. Es una lectura de `NumericField.SetNumber` (formato `"0.###"`, sin agrupación), no una medición.
- **Lo no releído:** la materialidad del efecto de F2 en las cabeceras de módulo; el paquete remite a T-3.
