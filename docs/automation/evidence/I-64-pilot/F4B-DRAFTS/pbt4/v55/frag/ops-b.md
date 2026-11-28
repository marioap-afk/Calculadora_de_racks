%%%% OP replace s54
%%%% ANCHOR
`tests` en failure, `build-ui` en success y `build-plugin` omitido. `ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI, y en PB-T4d, que no añade ninguna CT de UI y cuyas bajas de UI no caen.
%%%% TEXT
`tests` en failure, `build-ui` en success y `build-plugin` omitido. `ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI, y en PB-T4d, que no añade ninguna CT de UI y cuyas bajas de UI no caen; **en failure en PB-T4e** (desde la v5.5), por CT-PBT4E-03.
%%%% END

%%%% OP after s55
%%%% ANCHOR
Los racks A/B fabricados se rotulan «CONSTRUCTED REPRESENTATIVE TEST CASES».
%%%% TEXT


**Desde la v5.5, la recarga de «Editar todo» (PB-T4e, D-PBT4-22; V55-09).** El Owner quiso PB-T4e antes de PB-T5 para que el Smoke-PB mire el comportamiento corregido (`owner-pbt4e-list-decisions.md`, pregunta 3). Sustituye a la observación del comportamiento que PB-T4c aceptó (OWNER-02). Sobre una copia de trabajo de un dibujo con un rack Push Back real (OB-09.2); los pasos son los de la propuesta de lista (`pbt4e/list-proposal.md`, §4), y la guía los fija:
1. **El borrador Clean:** fijar la pestaña del rack y pulsar «Editar»: «Listo para editar»; no cambiar nada.
2. **El cambio de fuera:** con el panel visible, cambiar el rack fuera del panel (por ejemplo, con RACKEDITAR, un valor que la ventana completa enseña) y aceptar: el campo dice «Limpio (posiblemente obsoleto)» y la caja queda en solo lectura. Captura.
3. **«Editar todo»:** la ventana abre con el valor nuevo del paso 2, sin aviso; al cerrarla sin cambios, «Editor cerrado sin cambios en el borrador.», el estado «Limpio» sin «(posiblemente obsoleto)» y la caja editable. Sin PB-T4e abría con el valor viejo.
4. **Con edición:** repetir 1 y 2; «Editar todo», cambiar algo, «Aplicar al borrador» y cerrar («Modificado»); «Actualizar» se aplica sin «No se aplicó: el dibujo cambió fuera del panel» ni «Conservar mi borrador», y el dibujo conserva el cambio del paso 2 y el del editor.
5. **El fallo:** con un borrador Clean, borrar fuera todas las vistas del rack, y «Editar todo»: no abre, y el campo muestra «No editable aquí: el origen ya no está en el dibujo», la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.». Deshacer para restaurar el dibujo.
6. **Dirty, sin cambio:** un borrador con un cambio propio, un cambio de fuera y «Editar todo»: abre sobre la base de antes, como hoy y como «Editar»; después, «Actualizar» da lo de hoy.
7. **Cero escrituras:** «Editar todo» y cerrar sin «Actualizar» no cambian el dibujo (por ejemplo, `DBMOD` y la lista de Deshacer; la guía fija cómo mirarlo).
8. **El modal y el coste:** con la ventana abierta, la paleta deshabilitada (K = 1); al cerrar, el panel responde; y el tiempo entre el clic y la ventana con la marca y sin ella, en un dibujo grande.
%%%% END

%%%% OP before s57
%%%% ANCHOR
## 6. Los contratos propuestos
%%%% TEXT
### 5.7 PB-T4e: la lista cerrada CT-PBT4E-01 a 04 (entera; V55-05)

**Las reglas de §5.0,** enteras: un `[Fact]` por CT, que acaba en un `Assert.Equal` de las líneas unidas por `\n`; ninguna `[Theory]` nueva; la regla «EXCEPCION» (cada paso va por `Paso`, así que una CT cae por aserción, nunca por una excepción escapada); las de UI; los ayudantes existentes sin editarlos (`FullReadRig`, `FullEditIntentFakeReader`, `FullEditIntentKit`, `FullPrepareKit`, `IntentKit`, `FullWriteKit` y `PilotEraseScopeDouble`, en Core). **Los dobles** (el de `IWorkspaceFullEditHost`, que pone el modal con `SetModalRackCadActive`, guarda el lanzamiento y cuenta las lecturas al abrir y durante el modal; y el del puerto de la UI, que registra las llamadas y responde lo que la receta le da) **van dentro de su clase,** como clases anidadas privadas, copiados de las clases de PB-T4c sin referenciarlas. **Las CT de Core van en `tests/RackCad.Tests/`, con `namespace RackCad.Tests`; la de UI, en `tests/RackCad.UI.Tests/`, con `namespace RackCad.UI.Tests` y en `EditorDiscardPromptCollection`.** Ninguna lleva `#nullable enable` ni un `?` de tipo de referencia (§5.0). **Los nombres de las clases** (`PushBackEditAllReloadTests` y `PushBackEditAllReloadFieldTests`) no empiezan por `Workspace` ni por el prefijo de un filtro heredado, y ningún filtro heredado de clase exacta los selecciona (D-S8; medido, §6.5.1).

**De dónde sale cada línea:** de la sonda sobre el GREEN literal (`/d/r71`; `pbt4ecc/compile/r71-tr/green-core-lineas.txt` y `green-ui-lineas.txt`), una copia temporal de cada clase con el prefijo `Sonda` cuyo `Finish` acaba en `Assert.Fail` con sus líneas (INV-PBT4E-10): **las 24, tal cual.** Las mismas clases corren tal cual en la precondición 6 y pasan en el GREEN.

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| **01** | **La recarga antes de abrir** (D-PBT4-22, punto 1; pregunta 1): el borrador empezado con la lectura de siempre; el dibujo cambia fuera (el lector responde la misma lectura con otro nombre, `Otro nombre`); con la marca, `TryEditAll` recarga con una lectura, antes de abrir y ninguna dentro, abre sobre el borrador recargado (el mismo objeto) y lo deja Clean y sin la marca. **Los contrastes,** como hoy: sin la marca (no lee); Dirty marcado y Conflict marcado (no leen y abren); sin el anfitrión del editor (no lee: va antes de la recarga); otra sesión (la compuerta, antes de leer) | {{CT01_LINES}} | `PushBackEditAllReloadTests` | true |
| **02** | **Si la recarga no sale, no abre** (D-PBT4-22; pregunta 2), y lleva lo que da la recarga de «Editar»: el origen que ya no está (`FullEditIntentKit.OriginMissing`), el lector `Busy` y el lector que lanza (contenido como `Failed`, con su informe a nombre de `TryReloadPilotEdit`); y, para cada uno, el resultado de `TryEditAll` igual al de `TryReload` sobre un borrador gemelo | {{CT02_LINES}} | `PushBackEditAllReloadTests` | true |
| **03** | **El campo, como «Editar»** (D-PBT4-22, punto 2): la FVM real sobre un doble del puerto, con una vista Clean marcada: para la recarga no admitida, en espera y con fallo, «Editar todo» (que responde `NotBegun` con ese resultado) y «Editar» (cuya recarga responde lo mismo) muestran lo mismo; el campo solo pide `TryEditAll`; tras una recarga que sale y el editor cerrado, la caja vuelve a ser editable; y sin borrador, un comienzo no admitido sale como hoy, sin el aviso de la recarga | {{CT03_LINES}} | `PushBackEditAllReloadFieldTests` (UI) | true |
| **04** | **De punta a punta sin AutoCAD,** el beneficio (A2-11; F4B-E30): el dibujo cambia fuera y la pista del puente marca el borrador en el drenaje real; «Editar todo» con un gesto compuesto (un frente menos y la escala 2.5), una vez; y «Actualizar» con `PilotEraseScopeDouble` sobre el dibujo de ahora (como CT-PBT4C-10): se aplica, borra el lateral obsoleto y escribe las vistas con el nombre del dibujo | {{CT04_LINES}} | `PushBackEditAllReloadTests` | true |

**Las clases:** Core, `ExpectRed` true: **`PushBackEditAllReloadTests`** (CT-01, 02 y 04: 3); UI, `ExpectRed` true: **`PushBackEditAllReloadFieldTests`** (CT-03: 1). **3 CT de Core y 1 de UI: 4.** No hay equivalentes, `Skip` ni clases nuevas que no sean CT.

**La CT-05 opcional de la propuesta** (una guarda de texto sobre `EditAllOfPilot`) **no se escribe:** lo que fijaría (una sola recarga, tras el anfitrión del editor y antes de `ShowModal`, sin `ReadIntent` nuevo) lo fija ya el comportamiento de CT-01 (`lecturas=1 lecturas-antes-de-abrir=1 lecturas-dentro=0` y `marcado-sin-anfitrion … lecturas=0`), y una guarda de fuente no sustituye una prueba de comportamiento (`AGENTS.md`, «Guardia de fuente y prueba de comportamiento»). CT-PBT4C-07 sigue leyendo el cuerpo de `EditAllOfPilot`.

**CT -> la aserción que falla en el RED** (medido en `/d/r71`, con `src/` de la base; cada CT cae por su `Assert.Equal`, en su `Finish`, y ninguna por una excepción):

{{RED_TABLE}}

**Lo que fija cada `ExpectRed`:** en el RED, `src/` es el de la base, así que las cuatro caen por aserción; en el GREEN, pasan las cuatro. **El control** (§6.5.1, el paso 8): con solo la parte del anfitrión del GREEN, pasan las tres de Core y cae CT-03; con solo la del campo, al revés: cada CT mira su parte, y ningún oráculo es ciego.

**Los totales de PB-T4e** (F4B-E46), **en cifras,** con la base del cierre de PB-T4d (F4B-E59, `160e019d`; la CI de su GREEN, `45057868915` sobre `7db6fd0c`: Core **19 418 = 19 015 + 0 + 403** y UI **2 464 = 2 416 + 0 + 48**):

| Rama | Core | UI |
|---|---|---|
| **GREEN** | **{{CORE_GREEN}}** (Passed + Failed + Skipped) | **{{UI_GREEN}}** |
| **RED** | **{{CORE_RED}}** | **{{UI_RED}}** |

- Core: +3 CT (las tres en `Failed` en el RED); UI: +1 CT (en `Failed` en el RED). Nada se quita ni se salta: los `Skipped` no cambian.
- **Core** se mide entero en la precondición 6 (la suite completa es la de la CI). **UI** es la línea base de la CI más la diferencia, por desenlace, entre la unión de los 19 filtros de la rama y la de la base (cada filtro en su proceso), porque en local nunca corre la suite completa de UI.
- **K = 0, D = 0 y E = 0.**

%%%% END

%%%% OP after s60-taskid
%%%% ANCHOR
así que se usan sin editarlos, con `--task PBT4` en las tres subtareas.
%%%% TEXT
 Lo mismo en PB-T4d, `F4B-PBT4-RETIRE-PILOT` (21), y en PB-T4e (desde la v5.5), `F4B-PBT4-EDIT-ALL-RELOAD` (24, provisional).
%%%% END

%%%% OP after s60-unit
%%%% ANCHOR
y `PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`, que se borran.
%%%% TEXT

- **PB-T4e:** `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` (el cuerpo de `EditAllOfPilot`), `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` (la constante, la línea de `BeginEdit` que la usa, el resumen de `EditAll` y su aviso), y `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` y `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs` (solo comentarios). Ningún archivo nuevo ni borrado en `src/`.
%%%% END

%%%% OP after s60-rr2
%%%% ANCHOR
nunca los 26 existentes ni las tres clases nuevas que no son CT, cuyo texto es el del paquete.
%%%% TEXT
 **En PB-T4e,** el RED solo añade los dos archivos de las CT (`PushBackEditAllReloadTests.cs` y `PushBackEditAllReloadFieldTests.cs`) y no toca ningún archivo existente: una reparación solo toca esos dos.
%%%% END

%%%% OP after s60-rr7
%%%% ANCHOR
(`PushBackRetirePilotRunTests`, `PushBackRetirePilotFullCopyGuardTests` y `PushBackRetirePilotPlanGuardTests`).
%%%% TEXT
 **En PB-T4e,** ningún archivo de `src/` ni ningún archivo existente de `tests/`.
%%%% END

%%%% OP after s60-rr3
%%%% ANCHOR
| PB-T4d | `PushBackRetirePilotCensusGuardTests` (CT-02) | **Líneas:** las 11 de CT-02 en §5.6, literales, y las expresiones e inyecciones de su tabla `RetiredUses` |
%%%% TEXT

| PB-T4e | `PushBackEditAllReloadTests` (CT-01, 02 y 04) | **Líneas:** las 15 de esas CT en §5.7, literales. **Valores:** `Otro nombre`, `2.5`, `boom` y `Hay un comando en curso.` |
| PB-T4e | `PushBackEditAllReloadFieldTests` (UI; CT-03) | **Líneas:** las 9 de CT-03 en §5.7, literales. **Valores:** `No editable aquí: cambió el rack.`, `Usa RACKEDITAR.`, `Hay un comando en curso.` y `boom` |
%%%% END

%%%% OP after s60-filas
%%%% ANCHOR
Las tres clases nuevas que no son CT no tienen fila: no admiten reparación (RR-2).
%%%% TEXT


**Las filas de PB-T4e** (V55-05) son una por clase de las CT, como las de PB-T4c y PB-T4d.
%%%% END

%%%% OP replace s60-ci
%%%% ANCHOR
y en `success` en PB-T4d (sin CT de UI; sus bajas de UI no caen), sin cuelgue.
%%%% TEXT
en `success` en PB-T4d (sin CT de UI; sus bajas de UI no caen) y en `failure` en PB-T4e (por CT-PBT4E-03), sin cuelgue.
%%%% END
