# I-64 F4-T1B-PILOT-CHARACTERIZATION, intento 1: revisión directa de la sesión

**Objeto:** `0a6f84c2..1d36d006`, con el RED `92f5f5db` (siete clases nuevas más los mutantes) y el GREEN `1d36d006` (el inverso de los mutantes). Se revisa contra el contrato `gate-contract.json` (16 INV, 27 paradas) y contra el paquete v5 (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`, D-T1B-03 a D-T1B-07).

**Veredicto:** la entrega cumple el contrato. No hay bloqueantes ni importantes. Hay 4 menores, más un hallazgo previo a T1B que se comunica al Owner.

## Comprobado por la sesión

- **Producto sin cambio (INV-01).**
  - `git diff 0a6f84c2 1d36d006 -- src` sale vacío.
  - `--name-only` lista exactamente los siete archivos nuevos de prueba.
  - En los 14 archivos de MutantScope, el blob de la base es igual al del GREEN y distinto del RED. Lo comprueba `relay_t1b_work.py` con `rev-parse`.
  - `git diff RedSha..CurrentSha` no toca ninguna ruta de `tests/`.
- **La conjunción del RED (INV-02, INV-12).**
  - La CI del RED es `43649940464`: `tests` y `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido.
  - La corrida termina con normalidad: Core 11 fallos de 18471 en 1 m 17 s, UI 57 de 2310 en 3 m 37 s. No aparece ningún aborto, volcado de cuelgue ni host caído.
  - El criterio mecánico, medido por la sesión sobre los TRX de esa CI (`_trx_check.py`): UI 55/55 en Failed (mínimo 22) y Core 10/10 (mínimo 4). Ningún Message lleva el prefijo de tipo, la primera línea de la traza está siempre bajo `tests/` y no hay ningún resultado NotExecuted ni Skipped.
- **La unión de pruebas existentes rotas** coincide con la entrega:
  - `PushBackRoundTripSourceGuardTests.EditPushBack_RedrawsWithTheThreeDrawServices_EveryRedrawRegenFalse` ← MU-25;
  - `RichEditorCharacterizationTests.TheDefaultActionOfEachRichEditorIsWhatItIsToday` ← MU-01;
  - `PushBackTopeSectionInSafetyTests.MovingControl_DoesNotChangeTopeGeometryOrBom` ← MU-17.
  - La CI del GREEN `43650040527` tiene los 4 jobs en `success`: Core 18471/18471 y UI 2293 superadas, con las 17 omitidas de la línea base. Los filtros dan 10/10 y 55/55.
- **Los mutantes contra la tabla cerrada (INV-14, C04).** Se leyó el diff completo de `src/` del RED (83+/49−, 14 archivos), y cada cambio instancia su fila:
  - de comportamiento solo en el XAML y el code-behind de la ventana y en `RackViewBatchDialog.xaml`;
  - textual en los otros 11 archivos: cualificación redundante, conversión redundante, valor entre paréntesis o un método privado sin uso.
  - No se ve ningún mutante fuera de tabla.
  - Regla (b):
    - ningún mutante de comportamiento toca una asignación de `Owner`, un `ShowDialog`, un `MessageBox`, `EditorDiscardPrompt` ni una comprobación de nulo de una costura;
    - ninguno edita el constructor;
    - ninguno desvía un gesto hacia un botón que abre un diálogo o escribe: MU-14 D y C los pasan a «Agregar» y «Quitar», que solo mueven listas en un diálogo que no se muestra.
  - MU-10 C (`rackName + "*"` en `LoadExisting`) solo cambia el texto que recibe `NameBox` (`LoadFromModel:382`).
- **Contención (INV-06, INV-07, C09).**
  - Ninguna clase nueva instancia `RackFrameConfiguratorWindow`.
  - Ninguna llama a `PromptSaveToLibrary`, a `RackProjectStore`, a un `MessageBox`, a `Bom_Click`, a `SaveLibrary_Click` ni a `ConfigureModuleHeader`. Esos nombres solo aparecen como texto en las guardas de fuente o como nombres de botón para leer `IsEnabled`.
  - `InsertBatch_Click` solo se ejerce con un campo inválido (ventana sin mostrar) o con la salida bloqueada.
  - Toda ventana mostrada va fuera de pantalla, con `ShowActivated = false`, y se cierra en `finally`.
  - El único `ShowDialog` (CT-04) lleva el respaldo a `ApplicationIdle` con captura de excepciones, la bandera local y `Abort` de las operaciones pendientes, sin `Assert` dentro de los delegados.
  - No hay `Thread.Sleep`, `Task.Delay`, `Skip`, `#nullable`, P/Invoke ni rutas de máquina.
- **Colecciones (INV-05):** las cuatro clases que muestran ventanas o sustituyen el aviso van en `EditorDiscardPromptCollection`; las de recursos y de fuente, sin colección.
- **CT-12** lee `Owner`, la ubicación, `ShowInTaskbar` y los tamaños en una instancia nueva, sin mostrar y antes de reescribir nada (INV-07).
- **La medición del Paso 1** decide sin ramas: CT-01 y CT-02 por el gesto, de modo que MU-04 no se aplica, y CT-04 promovido tras tres corridas estables. CT-18 no se escribe, y MU-18 tampoco se aplica.

## Menores

| Id | Contenido | Disposición propuesta |
|---|---|---|
| D-1 | Durante la medición, una sonda temporal disparó `InsertBatch_Click` **con entradas válidas** sobre una ventana sin mostrar: lanzó en `Owner = owner` antes de abrir el diálogo y no escribió nada. Contradice la letra de INV-F4T1B-06, que en su texto se refiere a las pruebas. Es código temporal ya retirado, y el Worker lo declara como desviación | Se acepta como desviación declarada y contenida por el mismo mecanismo de CT-23. Se comunica al Owner |
| D-2 | El Worker lanzó en segundo plano el tercer lote de mutantes, contra el prompt, y el arnés pasó a segundo plano órdenes de más de 180 s. Lo declara | Se acepta: los relevos de salida y entrada dan 0 procesos participantes y 0 no atribuibles persistentes |
| D-3 | Varias filas se instancian en más de un sitio a la vez: MU-08 T en las guardas 1, 2 y 3, y MU-09 T en dos tooltips. Las filas dicen «el texto de una guarda» y «el texto de un tooltip». El paquete admite «las alternativas que la propia fila enumera»; aquí son varias instancias de la misma alternativa en la misma corrida aislada | Se acepta: cada cambio es una instancia literal de su fila, y la discriminación se mide igual |
| D-4 | MU-01 y MU-15 V/D se volvieron a correr aislados sobre el GREEN (`m01b`, `m15vb`, `m15db`) después de retocar dos pruebas por los avisos xUnit2013 y xUnit2029, antes del commit RED. Las corridas aisladas originales (`m01`, `m15v`, `m15d`) son de la versión anterior de esas pruebas | Se acepta: la versión commiteada tiene su corrida aislada, y ninguna prueba cambia después del RED |

## Hallazgo previo a T1B, para el Owner (no es de esta tarea)

- **H-1. La suite de UI existente escribe en el perfil real del usuario.**
  - Varias pruebas existentes instancian el configurador de cabecera real:
    - con `new RackFrameConfiguratorWindow(` directo, en 9 archivos de `tests/RackCad.UI.Tests`, por ejemplo `EditorShellAdoptionTests.cs:100` y `FirstViewFreedomTests.cs:102` y `:119`;
    - o a través de la costura `HeaderConfiguratorPresenter`, en `PushBackHeaderAuthorityWindowTests.cs:100`, `:125` y `:151`, donde el producto crea la ventana real y la costura solo sustituye el `ShowDialog`.
  - Al cerrarse, `OnClosed` → `SaveCurrentLayout` reescribe el archivo de disposición: `RackFrameConfiguratorLayoutStore.cs:26-29` lo guarda en `LocalApplicationData`, en una ruta `static readonly`.
  - La prueba de limpieza del Worker lo mide: el mismo SHA-256 y otra fecha, tras cada suite completa en local.
  - Las pruebas de T1B no lo hacen, porque su prueba de limpieza se mide alrededor de sus propias corridas y nada cambia. Así que no es la parada F4T1B-C09, que habla de «a characterization test».
  - Pero toda corrida local de la suite completa, que la cadena pide (§16.8, INV-12), toca el perfil real. Y en la CI, el perfil del runner.
  - Es la misma ruta que la contención del Owner prohibió a T1B. Se propone declararlo como deuda (candidato a una costura de prueba del configurador o a la contención de T4B) y que lo decida el Owner. No se corrige en T1B: D13 e INV-03.
