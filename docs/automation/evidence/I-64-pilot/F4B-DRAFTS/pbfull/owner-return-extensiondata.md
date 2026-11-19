# I-64 F4B: vuelta al Owner antes del contrato de PB-T3. La ExtensionData anidada y el paso 6 de D-PB-05

- **Origen de la pregunta:** paquete F4B v3.1, `docs/initiatives/I-64-f4b-push-back-completo-package.md:313-316` (OB-05.2, limitación declarada) y `:770` (vueltas previstas: «Antes del contrato de PB-T3», IR2 R-I-10).
- **Regla del Owner que hay que respetar:** [OD] `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md:110`, literal: «6. unknown/future ExtensionData must not be silently destroyed;». Y `:120`: «"Conservar mi borrador" must be an informed destructive choice.»
- **Revisión leída:** `2f5d3dec`, solo por objetos git. No se ejecutó dotnet ni AutoCAD. Las afirmaciones sobre el panel se apoyan en pruebas que ya existen (matriz de I-58). La pérdida en RACKEDITAR sale de leer el código: no hay ninguna prueba que la fije para Push Back (§2.4).

---

## 1. Qué es la ExtensionData, qué se conserva y qué se pierde

Cada rack guarda su diseño dentro del dibujo como un texto de campos y valores (JSON), en tres capas: el **sobre** (identidad, nombre y vista), el **proyecto** y el **diseño Push Back**. El diseño contiene piezas **anidadas**: la estructura con sus módulos y cabeceras, los frentes y niveles, la seguridad, el lado B y la interfaz A/B.

La **ExtensionData** es un cajón donde el plugin guarda, sin entenderlos, los campos que no conoce, para volver a escribirlos tal cual al guardar (I-11). Hoy ese cajón **solo existe en las tres capas de arriba**. Ninguna pieza anidada lo tiene. Un campo desconocido dentro de una pieza anidada se descarta al leer y desaparece al reescribir.

**Ejemplo:** una versión más nueva del plugin añade `"Anclaje": "quimico"` dentro del módulo 2, o `"AlturaGuarda": 6` dentro del lado B, o alguien edita ese texto a mano. Una versión anterior que reescriba el rack lo pierde.

No es un caso teórico. El repositorio añade así sus campos nuevos, dentro de las piezas y sin cambiar la versión del esquema:
- las tarimas por celda de I-41 (`DrawPallets`), dentro de cada frente (`PushBackDesignDocument.cs:220-238`; la versión sigue en `"1.0"`, `:22`);
- las ranuras sin almacenamiento del lado B, «Campo ADITIVO» (`:414-418`);
- la regla automática de la defensa de PB-010 (`ExitAuto`, `SelectivePalletDesignDocument.cs:770-774`).

Un plugin anterior a I-41 que reescribiera un rack de I-41 habría borrado sus tarimas por celda sin avisar.

## 2. El paso 6 de D-PB-05 y por qué esta limitación le afecta

### 2.1 El paso 6

D-PB-05 dice qué gana cuando el usuario elige «Conservar mi borrador» frente a un rack que alguien cambió por fuera:
- los campos del editor salen del borrador;
- el sobre y la infraestructura salen del rack actual.

El paso 6 (`package:281`) añade que la ExtensionData desconocida o futura **no se destruye en silencio**. PB-T3 construye la composición y la escritura completa, con los pasos 4 a 7 y sus pruebas (`package:307-318`). PB-T4 la conecta a «Conservar mi borrador».

### 2.2 Por qué le afecta

La escritura completa de PB-T3 reescribe el diseño entero desde el estado del editor: estructura, módulos, frentes, lado B e interfaz.

El escritor (`PushBackDesignDocument.FromDomain`) solo arrastra el cajón de arriba (`PushBackDesignDocument.cs:97`). Todas las piezas anidadas las rehace desde cero (`:105`, `:126`, `:143-144`, `:148`; `DynamicRackSystemDocument.cs:165-190`). Además, al leer, el sistema descarta sin error los campos de un tipo que no tiene cajón: las opciones de `RackProjectStore.cs:404-410` no cambian ese comportamiento por defecto.

Por eso, si ese escritor llegara a recibir un rack con un campo anidado desconocido, el campo se perdería sin aviso.

### 2.3 Lo que pasa hoy, medido en el código

| Ruta | ¿Lee un rack con un campo anidado desconocido? | Resultado | Ancla |
|---|---|---|---|
| **Panel (Workspace)**: abrir el borrador, Actualizar y «Conservar mi borrador» | **No.** La admisión (AUTH-13, de I-58) rechaza cualquier campo desconocido en cualquier nivel: el sobre, el proyecto, el diseño y cada pieza anidada. También rechaza una versión de esquema que no sea exactamente la actual y un nombre de topología o de sentido desconocido | **No escribe nada y lo dice:** «No editable aquí: ilegible o de versión futura» y «Usa RACKEDITAR para editarlo con el editor clásico.» | Admisión: `PushBackPilotIntentReader.cs:217-222`, `:35`, `:56`. Relectura en Actualizar: `PushBackPilotUpdatePlan.cs:325-326`. «Conservar mi borrador»: `WorkspaceHostCore.cs:694-705`. Lector estricto: `AuthoredRawReader.cs:44`, `:95`, `:133-145`; `AuthoredRawReader.Shapes.cs:379-492`. Enumerados: `AuthoredCanonical.cs:192-196`. Pruebas: `I58F1Matrix.cs:59-64` y `:97-102` (CT58-15 y CT58-16, esperado Unreadable) |
| **RACKEDITAR → Actualizar** (clásico) | **Sí.** Lee con el lector tolerante y no pasa por AUTH-13 | **Borra el campo anidado sin avisar.** Conserva los campos desconocidos de las tres capas de arriba | Lectura: `RackPushBackCommands.cs:154`. AUTH-13 solo cuando no es Actualizar: `:193-195`. Escritura: `:456`. Arrastre de arriba: `RackProjectStore.cs:71-83` y `SystemRegistry.Default.cs:168` |
| **RACKEDITAR → Insertar** (clásico) | No: pasa por AUTH-13 | Rechaza con un mensaje y no escribe nada | `RackPushBackCommands.cs:193-195`; `RackUnsupportedSiblingInsert.cs` |
| **Guardar en biblioteca** (ventana clásica) | Sí | El archivo de biblioteca pierde el campo anidado sin avisar | `RackPushBackSystemWindow.xaml.cs:3507` |

`DeepCopy` (`RackFrameProjectStore.cs:87`) y los copiadores del estado de PB-T1 no añaden pérdida: copian objetos en los que el campo ya no estaba desde la lectura. Pasarían a importar solo con la opción C.

### 2.4 Lo que esto cambia en el marco del paquete (nota para la sesión)

- **Panel.** El `package:314` dice que los campos anidados se pierden «en cada reescritura, clásica o del panel». Es cierto del **escritor**. Pero hoy el panel **nunca llega a escribir** un rack así, porque su admisión lo rechaza antes. La pérdida silenciosa solo ocurre en las rutas clásicas, y el panel remite a RACKEDITAR.
- **OB-05.2.** Promete conservar la ExtensionData del nivel superior en el panel (`package:310`). Hoy esa conservación tampoco se ejercita: AUTH-13 rechaza también los campos desconocidos de arriba (CT58-15).
- **Lo que prevé el paquete.** Solo quita el rechazo «fuera del piloto» (OB-03.1 y OB-03.2, `package:220-221`). No prevé tocar AUTH-13.
- **Pruebas.** No hay prueba que fije la pérdida anidada de Push Back en el clásico. Las pruebas de campos desconocidos de Push Back solo cubren el nivel superior: `PushBackCoreTests.cs:324`, `PushBackCompositeModelTests.cs:241`, `PushBackCompositeRoundTripTests.cs:118` y `PushBackAdvancedRackParametersTests.cs:510`.
- **Contrato de PB-T1.** El mapa de propiedad cuenta los campos anidados desconocidos como `Editor` por su contenedor (`I-64-f4b-pbt1-gate-contract-package.md:492`).
- **I-11.** Su limitación está escrita: `I-11-persistencia-uniforme.md:241-247`. La cabecera (`RackFrameProjectDocument`) quedó fuera por decisión del Owner y como deuda (`docs/automation/decisions/I-11.md:13-25` y `:43-44`). No hay ninguna deuda registrada para el resto de lo anidado.

### 2.5 Tabla de tipos anidados del diseño Push Back

En ninguno de estos tipos hay ExtensionData, y en todos se pierde un campo desconocido al reescribir. Hoy eso ocurre en las rutas clásicas de §2.3; en el panel, solo si se abriera su barrera.

| Tipo (dónde va en el JSON) | ¿ExtensionData? | ¿Se pierde al reescribir? | Ancla |
|---|---|---|---|
| **Sobre** `RackEmbedDocument` | **Sí** | **No** (`RackEmbedComposer.cs:58`) | `RackEmbedDocument.cs:74` |
| **Proyecto** `RackProjectDocument` | **Sí** | **No** (`RackProjectStore.cs:82`) | `RackProjectDocument.cs:51` |
| **Diseño** `PushBackDesignDocument` | **Sí** | **No** (`PushBackDesignDocument.cs:97`) | `PushBackDesignDocument.cs:75` |
| `PushBack.Structure`: `DynamicRackSystemDocument` | No | Sí | `DynamicRackSystemDocument.cs:14` |
| `Structure.Fronts[*]`: `DynamicRackFrontDocument` | No | Sí | `DynamicRackSystemDocument.cs:435` |
| `Structure.Fronts[*].Levels[*]`: `DynamicRackLevelDocument` | No | Sí | `DynamicRackSystemDocument.cs:624` |
| `Structure.Modules[*]`: `DynamicRackModuleDocument` (**módulos**) | No | Sí | `DynamicRackSystemDocument.cs:793` |
| `Modules[*].Header` y `HeaderLineOverrides[*].Header`: `RackFrameProjectDocument` (cabecera) | No (excluida por el Owner en I-11) | Sí | `RackFrameProjectDocument.cs:12` |
| Postes, placas, horizontales y paneles de la cabecera: `PostDocument`, `PlateDocument`, `HorizontalDocument`, `PanelDocument` | No | Sí | `RackFrameProjectDocument.cs:123`, `:157`, `:190`, `:235` |
| `Structure.HeaderLineOverrides[*]`: `DynamicHeaderLineOverrideDocument` | No | Sí | `DynamicRackSystemDocument.cs:756` |
| `Structure.DerivedPostLineOverrides[*]`: `DynamicDerivedPostLineOverrideDocument` | No | Sí | `DynamicRackSystemDocument.cs:727` |
| `Structure.SafetySelections[*]`: `SafetySelectionDocument` (**seguridad**) | No | Sí | `SelectivePalletDesignDocument.cs:522` |
| Dentro de la seguridad: `PostSideDocument`, `GridCellDocument`, `PostDefenseDocument`, `BootPostDocument` | No | Sí | `SelectivePalletDesignDocument.cs:756`, `:749`, `:763`, `:741` |
| `PushBack.Fronts[*]` y `SideB.FrontConfigs[*]`: `PushBackFrontDocument` (peraltes, fondo y **tarimas** por celda) | No | Sí | `PushBackDesignDocument.cs:227` |
| `PushBack.SideB`: `PushBackSideDocument` (**lado B**) | No | Sí | `PushBackDesignDocument.cs:247` |
| `PushBack.Composite`: `PushBackCompositeDocument` (interfaz A/B) | No | Sí | `PushBackDesignDocument.cs:400` |
| `Composite.Topologies[*]`: `PushBackTopologyCellDocument` | No | Sí | `PushBackDesignDocument.cs:528` |
| `RearTopeOffCells[*]` (también en el lado B): `PushBackCellDocument` | No | Sí | `PushBackDesignDocument.cs:543` |

En total son **21 tipos anidados sin cajón**. Las tarimas no tienen un tipo propio: sus medidas van en `Structure` y la tarima por celda va en `PushBackFrontDocument.DrawPallets`. Varios de estos tipos son **compartidos**: la estructura con Dinámico, la seguridad con Selectivo y la cabecera con todos.

## 3. Opciones

### (A) Basta con la limitación declarada (Recomendado)

PB-T3 sigue, y el panel mantiene su barrera actual.

**Trabajo:**
- El contrato de PB-T3 declara que su escritura completa sigue pasando por la admisión (AUTH-13).
- PB-T3 lo fija con una prueba: un rack con un campo desconocido dentro de un módulo o del lado B da «no admitido» y no se escribe nada.
- La pérdida del clásico queda escrita como limitación conocida y anterior a F4B, como hizo I-11 con la cabecera.
- Son unas pocas filas de prueba, sin código de producto nuevo.

**Riesgo:**
- Ninguno nuevo en el producto.
- El que queda es antiguo: RACKEDITAR → Actualizar y Guardar en biblioteca siguen borrando esos campos sin avisar.

**Uso diario:**
- Con una sola versión del plugin y sin ediciones a mano, no cambia nada y nunca aparece.
- Con versiones mezcladas en la oficina, el panel dice «No editable aquí… usa RACKEDITAR». Si el usuario va a RACKEDITAR y pulsa Actualizar, el campo nuevo desaparece sin aviso, como hoy.

**Por qué se recomienda:** cumple el paso 6 donde aplica (el panel) con una barrera que ya existe y está probada, sin trabajo grande ni cambios en RACKEDITAR. **Su contra:** la pérdida silenciosa del clásico, la ruta a la que remite el panel, sigue como hasta hoy.

### (B) Basta para PB-T3, y además RACKEDITAR avisa antes de perder

PB-T3 queda igual que en (A). Aparte, una tarea con su propio contrato hace que RACKEDITAR → Actualizar y Guardar en biblioteca detecten los campos anidados desconocidos. Antes de reescribir, avisan y piden confirmación, o bloquean; se decide en ese contrato. No los conservan.

**Trabajo:**
- Un detector que reutilice la forma estricta que ya tiene AUTH-13, limitado a los campos desconocidos.
- El mensaje y sus pruebas.

**Riesgo:**
- Es un **cambio observable del editor clásico**. El [OD] lo prohíbe salvo correcciones autorizadas expresamente (`package:61`, D-16 e INV-19). Esta respuesta sería esa autorización.
- Si el detector se copia entero de AUTH-13, también avisaría por una versión de esquema menor más nueva sin campos nuevos. Hay que acotarlo.
- No retrasa PB-T3, pero suma una tarea al presupuesto de F4B. Cuándo hacerla lo planifica la sesión.

**Uso diario:** igual que (A), pero quien vaya a RACKEDITAR con un rack de una versión más nueva verá un aviso antes de perder datos.

### (C) No basta: conservar lo anidado antes de PB-T3

Una iniciativa nueva añade el cajón a los 21 tipos anidados y lo lleva por toda la cadena:
- la lectura y la escritura;
- el modelo de dominio, que hoy no admite datos de persistencia por la regla de capas (`RackProjectStore.cs:419`);
- el estado editable de PB-T1, con su copiador y su comparador, y `DeepCopy`.

Además:
- Para que el panel pudiera editar esos racks, habría que **abrir la barrera AUTH-13**, una base de I-58 que se diseñó para fallar cerrada (`I-58-architect-review-package.md:53`, AR58-15).
- Habría que reabrir la exclusión de la cabecera que el Owner aprobó en I-11.

**Trabajo:** grande, del tamaño de una iniciativa propia. Toca también Dinámico y Selectivo, porque los tipos son compartidos. PB-T3 espera.

**Riesgo:**
- Alto en código.
- **También de producto.** Conservar a ciegas un campo futuro puede dejar un dato incoherente. Si el usuario cambia la longitud del módulo 2, la versión anterior no puede saber si el `"Anclaje"` guardado sigue valiendo. Y si borra o reordena frentes, no está claro a qué elemento pertenece cada campo guardado.

**Uso diario:** los datos de una versión más nueva sobrevivirían a una edición con la versión anterior, en el panel y en el clásico.

## 4. La pregunta, lista para AskUserQuestion

- **header** (12 caracteres como máximo): `Paso 6 PB-T3`
- **question** (335 caracteres):

> Paso 6 de «Conservar mi borrador»: un campo nuevo dentro de un módulo o de un lado (de una versión más nueva del plugin o editado a mano) no se conserva al reescribir. Hoy el panel no escribe esos racks: los rechaza con aviso y remite a RACKEDITAR, que sí borra ese campo sin avisar (pérdida antigua). ¿Basta así para seguir con PB-T3?

- **options** (la recomendada primero):

| label | description |
|---|---|
| `Basta así (Recomendado)` | PB-T3 sigue. El panel mantiene su rechazo con aviso y PB-T3 lo fija con una prueba; la pérdida de RACKEDITAR queda declarada como limitación conocida, sin tocarlo. |
| `Basta, y que RACKEDITAR avise` | PB-T3 sigue igual y, aparte, una tarea con su propio contrato hace que RACKEDITAR avise antes de borrar esos campos. Es un cambio del editor clásico que esta respuesta autorizaría. |
| `No basta: conservarlos` | Antes de PB-T3, una iniciativa nueva hace que esos campos se conserven (21 tipos de datos, compartidos con Dinámico y Selectivo). PB-T3 espera; es la opción más cara y con más riesgo. |

- **multiSelect:** no.

## 5. Qué NO decide esta pregunta

- **Si se abre la barrera del panel (AUTH-13)** para admitir racks con campos desconocidos o con una versión menor más nueva. Hoy el panel los rechaza todos, también los de arriba. Abrirla sería otra decisión, con su revisión (I-58).
- **El resto de D-PB-05:**
  - los pasos 1 a 5 y 7;
  - la presentación clasificada de conflictos (OB-05.3);
  - la confirmación de «Conservar mi borrador» (OB-05.4);
  - el trabajo de PB-T4.
- **El contrato, el alcance ni el presupuesto de PB-T3**, salvo la fila de prueba de la opción (A).
- **La cabecera de biblioteca** (`RackFrameProjectDocument` por `RackFrameProjectStore`). El Owner ya la excluyó en I-11 y sigue como deuda.
- **Los valores desconocidos dentro de un campo conocido**, por ejemplo un nombre de topología nuevo. El clásico los cambia sin avisar por el valor por defecto (`PushBackDesignDocument.cs`, `ParseTopology`) y el panel los rechaza (`AuthoredCanonical.cs:192-196`). Es un hecho vecino que esta pregunta no cubre.
- **Los otros sistemas** (Dinámico, Selectivo, Cantilever, cama). Tienen la misma limitación de I-11 y quedan como están.
- **Cuándo se hace la tarea de (B)**, si se elige. Solo se autoriza su existencia y su contrato pasa por el procedimiento normal.
