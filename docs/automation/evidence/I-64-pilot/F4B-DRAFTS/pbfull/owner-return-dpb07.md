# I-64 F4B: vuelta al Owner por D-PB-07 (BOM, «Guardar en biblioteca», lote de vistas e Insertar en el modo WORKSPACE)

**Fecha:** 2026-11-18. La prepara la sesión principal, antes del contrato de PB-T2 (OB-07.1).

**Revisión medida:** `2f5d3dec`, en solo lectura y por objetos git. Las anclas de código son de esa revisión. El paquete de la fase cita «Guardar en biblioteca» en `W:3463-3464` porque se midió en `5ccaa770`; en `2f5d3dec` esas dos líneas están en `W:3503-3504`.

**Abreviaturas:**
- `W` = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`;
- `SES` = `src/RackCad.UI/Editor/RackEditorSession.cs`;
- `RPC` = `src/RackCad.Plugin/RackPushBackCommands.cs`;
- `T-SG` = `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`;
- [OD] = la disposición del Owner (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`);
- «el paquete» = `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1.

## 1. Qué es el modo WORKSPACE y por qué importa D-PB-07

- **Hoy el panel no abre el editor.** Solo edita la escala de anotación. Cuando un rack no entra en el piloto, muestra «Usa RACKEDITAR para editarlo con el editor clásico.» y no lo lanza (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:56`; `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:281`).
- **PB-T2** separa en la ventana «aplicar», «cerrar» y «escribir», y le da dos modos explícitos. En el modo WORKSPACE, el botón principal pasa a ser «Aplicar al borrador»: devuelve lo editado al borrador y no escribe nada en el dibujo (D-PB-01, OB-01.6, §4).
- **PB-T4** añade «Editar todo» al panel. Abre esa misma ventana, a tamaño completo y modal, cargada desde el borrador. El dibujo solo cambia con el «Actualizar» del panel (PREPARE → MUTATE → POST de T4: todo o nada, un paso de Deshacer, conflicto antes de escribir).
- **Por qué importa D-PB-07.** El contrato de PB-T2 tiene que decir qué botones existen en el modo WORKSPACE:
  - cada botón visible necesita sus pruebas y sus guardas;
  - cada botón oculto necesita la prueba de que no se ve;
  - la prueba UI-STA de OB-01.3 («ningún gesto anota una petición clásica») tiene que pulsar todos los visibles.

## 2. Los cuatro gestos, medidos

**Lo que ya está decidido.** Insertar (los cuatro botones por vista e «Insertar vista actual») e «Insertar varias…» (el lote de vistas) **escriben en el dibujo**. En el modo WORKSPACE los excluyen dos frases literales de [OD]: «it performs ZERO authored DWG writes» (`OD:31`) y «Do not let a Workspace invocation accidentally execute the classic drawing-write path» (`OD:45-46`). OB-01.6 ya los oculta en ese modo, y la revisión independiente IR2 lo confirmó (`independent-review-v2.md` §2.2). Van en la tabla para que la comparación sea completa, pero **la pregunta es solo sobre BOM y biblioteca**.

| Gesto | Qué hace hoy en RACKEDITAR | Dónde escribe | Efectos sobre el estado | Riesgo si queda activo en WORKSPACE | Coste de ocultarlo en WORKSPACE |
|---|---|---|---|---|---|
| **BOM** (y su exportación) | Recalcula, comprueba que los datos son válidos y que no hay un diagnóstico bloqueante, y abre la lista de materiales en una ventana modal (`W:3463-3479`). Desde ahí el usuario puede exportar a Excel o CSV con un diálogo «Guardar como» (`src/RackCad.UI/RackBomWindow.xaml.cs:37-87`) | **Dibujo: no.** **Disco: solo si el usuario exporta**, en el archivo que él elige. Perfil: no | Ninguno. El recálculo confirma la celda en edición, como cualquier edición. No toca la identidad, el borrador ni la petición clásica | **Bajo.** No dibuja ni pide ningún dibujo. Dos matices: (1) la lista es la de los valores que hay en la ventana, que pueden no estar aplicados al borrador ni dibujados; (2) la exportación escribe un archivo fuera del borrador. **Para probarlo:** hoy no hay costura; el botón abre un modal real, fijado por texto en `T-SG:281-284` (S-7) | Para ver o exportar el BOM de un borrador habría que escribirlo antes en el dibujo y usar `RACKBOMTOTAL`, que da el total del dibujo por rack (`src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs:34`). La otra vía es RACKEDITAR, fuera del borrador. Mientras «Editar todo» está abierto, el dibujo no se puede usar (D-PB-01) |
| **«Guardar en biblioteca»** | Recalcula y pasa la misma compuerta. Pide el archivo (`W:3498`, `UiSupport.PromptSaveToLibrary`, que crea la carpeta de biblioteca si falta: `src/RackCad.UI/UiSupport.cs:64-82`). Después hace `EnsureId` y `SetName` (`W:3503-3504`) y guarda el `.rackcad.json` (`W:3507-3508`) | **Dibujo: no.** **Disco: sí**, un `.rackcad.json` en la carpeta de biblioteca configurada. **Perfil:** solo crea esa carpeta si no existe | `EnsureId` acuña un GUID **solo si la ventana no tiene ninguno** (`src/RackCad.UI/Editor/RackEditorIdentity.cs:45-52`). `SetName` pone el texto de la caja «Nombre», el mismo que ya pone cada cambio de esa caja (`W:1234-1238`). No marca la petición clásica: el comentario de `W:3505-3506` dice «Saving never flags an insert». **El archivo no lleva el GUID ni el nombre del rack**, solo el diseño (`src/RackCad.Application/Persistence/RackProject.cs:130-133`) | **Bajo, con una condición que PB-T2 tiene que probar** (ver §2.1). No dibuja ni pide ningún dibujo. Escribe un archivo fuera del borrador, y puede guardar valores no aplicados ni dibujados. **Para probarlo:** hoy no hay costura. La prueba existente solo llega al diálogo (`tests/RackCad.UI.Tests/PushBackLibrarySaveGateTests.cs:95`), y `EnsureId` va después. El orden y el texto están fijados en `T-SG:294-298` (S-9) y `T-SG:323-341` (CT-22) | Guardar como plantilla una variante del borrador exigiría escribirla antes en el dibujo y abrir RACKEDITAR → «Guardar en biblioteca» |
| **«Insertar varias…»** (lote de vistas) | Confirma los cambios de módulo, recalcula, abre el diálogo del lote (`W:3334-3351`), pone el nombre, anota la petición clásica (`SES:145-170`: `EnsureId` y `InsertRequested = true`) y cierra (`W:3360`). RACKEDITAR la consume: redibuja las vistas del rack y luego inserta las elegidas (`RPC:193-198`, `RPC:369-374`) | **Dibujo: sí.** Redibuja y renombra las vistas existentes y crea definiciones y referencias nuevas | Marca la petición clásica y cierra. Además, el botón no mira si la ventana se abrió desde AutoCAD, y nunca se deshabilita solo (mapa B, H-097) | **Rompe D-PB-01 y OB-01.3.** Anota la petición que solo sabe ejecutar el camino clásico. Si algo la consumiera, dibujaría fuera de T4: sin conflicto previo y sin todo o nada | Ya decidido. Crear vistas sigue en RACKEDITAR, RPB y el menú (no-objetivo de la Proposal, `:75`; §6.4 del paquete, fila 1, resuelta por el bloque M). Para insertar vistas, se cierra «Editar todo», se actualiza desde el panel y se usa RACKEDITAR |
| **Insertar** (4 botones por vista e «Insertar vista actual») | `RequestDraw`: confirma módulos, recalcula, comprueba, pone el nombre, anota la petición (`SES:186-193`) y cierra (`W:3363-3419`). RACKEDITAR la consume por la misma rama que el lote | **Dibujo: sí.** Medido en el bloque M de PB-T0 (M-6a y M-6b) | Igual que el lote | Igual que el lote. Además, **M-6b midió** que, con una edición sin «Actualizar», las vistas existentes se redibujan y renombran, pero **la vista nueva sale con el sobre de ANTES de la edición** (`block-m-analysis.md` §1) | Ya decidido, igual que el lote |

### 2.1 El dato de H-PB-33 (identidad), leído en el código

OB-07.1 pide llevar este dato: «Guardar en biblioteca» ejecuta `EnsureId` y `SetName`, y en el modo WORKSPACE eso tocaría la identidad del borrador. Lo que dice el código, **por lectura y sin ejecutarlo**:

1. `EnsureId` solo acuña un GUID si la ventana no tiene ninguno (`RackEditorIdentity.cs:45-52`).
2. RACKEDITAR abre la ventana con el GUID del rack (`LoadExisting` → `Adopt`, `W:311-324`). Un borrador del panel es siempre de un rack que ya está en el dibujo. **Si el modo WORKSPACE adopta el GUID igual que `LoadExisting`, `EnsureId` no hace nada.**
3. `SetName` repite lo que ya hace cada cambio de la caja «Nombre» (`W:1234-1238`).
4. El archivo de biblioteca no lleva GUID ni nombre. Al abrirlo se crea un rack nuevo, con un GUID nuevo al insertar (`LoadDesignForNew`, `W:294-306`).
5. El GUID de la ventana no llega al dibujo cuando el rack ya tiene uno. El clásico lo ignora («NEVER mint a new GUID on edit», `RPC:186`), y en el Workspace el paso 7 de D-PB-05 prohíbe que la identidad venga del borrador.

**Conclusión:** el riesgo de identidad no es un efecto medido, sino una **condición que PB-T2 tiene que probar**: tras «Guardar en biblioteca» y «Aplicar al borrador», el GUID del borrador no cambia.

### 2.2 Lo que midió PB-T0

- **Nada de BOM ni de biblioteca.** El contrato de PB-T0 prohibía ejecutarlos («ninguna prueba ejecuta «Guardar en biblioteca», el BOM, el configurador real…», paquete §6.2).
- **Insertar sí:**
  - nombre, sufijo `_1`, jig, rotación 0, capa actual y mismo GUID (M-6a y M-6b);
  - la vista nueva sale con el sobre previo a la edición (M-6b);
  - el Deshacer de la propia inserción **no se midió** (`block-m-analysis.md` §4).
- **«Actualizar» clásico:**
  - nunca crea vistas (M-1 y M-4);
  - cada «Actualizar» es un solo paso de Deshacer (M-5).
- **No hay datos medidos de cuánto se usan** el BOM y la biblioteca desde el editor.

## 3. Las opciones

**Común a las tres:**
- en el modo WORKSPACE, Insertar y el lote se ocultan (OB-01.6);
- RACKEDITAR no cambia (OB-01.5);
- ocultar un botón solo en el modo WORKSPACE no toca las pruebas de T1B, que fijan el modo CLASSIC (OB-T1B.2: la ocultación de ese modo no necesita registro de sustitución).

### (A) BOM y biblioteca, sí. Lote e Insertar, no. **(Recomendado)**

Es la propuesta de la v1, ahora con los hechos.

- **PB-T2, pruebas y guardas:**
  - dos costuras de prueba nuevas, una para la ventana del BOM y otra para el diálogo de guardado, porque hoy ninguna de las dos se puede ejecutar en una prueba;
  - las costuras tienen que dejar en verde los textos literales de S-7, S-9 y CT-22. Si no pueden, OB-T1B.3 obliga a volver al Owner antes del contrato;
  - la prueba UI-STA de OB-01.3 pulsa BOM y biblioteca en el modo WORKSPACE y comprueba cuatro cosas:
    - no hay petición clásica (`InsertRequested` en false e `InsertionRequest` en null);
    - el borrador no cambia hasta «Aplicar al borrador»;
    - el GUID del borrador no cambia (§2.1);
    - el archivo de biblioteca lleva los campos desconocidos del rack (I-11);
  - PB-T2 decide el dueño de esas dos ventanas hijas en el modo WORKSPACE (D-15, punto 4).
- **Uso diario:**
  - el editor del panel tiene los mismos botones secundarios que RACKEDITAR; solo faltan los que dibujan;
  - se puede cotizar o guardar una variante sin tocar el dibujo.
- **Contras:**
  - dos salidas a disco fuera del borrador (el archivo de biblioteca y la exportación del BOM);
  - los dos reflejan valores que pueden no estar aplicados ni dibujados;
  - es la opción con más trabajo de prueba en PB-T2.

### (B) Solo el BOM. Biblioteca, lote e Insertar, no

- **PB-T2:**
  - una costura nueva, la del BOM, con S-7 en verde;
  - la prueba UI-STA pulsa el BOM;
  - una prueba de que «Guardar en biblioteca» no se ve en el modo WORKSPACE y sigue en CLASSIC.
- **Uso diario:**
  - se cotiza el borrador sin tocar el dibujo;
  - para guardar una variante en la biblioteca hay que escribirla en el dibujo y usar RACKEDITAR.
- **Contras:** la línea es fina. La exportación del BOM también escribe un archivo en disco, y el efecto de identidad que se quita es nulo en un rack con GUID (§2.1). Es «solo lectura» en la pantalla, no en el disco.

### (C) Ninguno de los dos. En «Editar todo» solo quedan «Aplicar al borrador» y «Cerrar»

- **PB-T2:**
  - sin costuras nuevas;
  - dos botones más ocultos en el modo WORKSPACE, con su prueba;
  - la prueba UI-STA de OB-01.3 queda más corta;
  - es la opción más simple de probar y la garantía más fuerte: de la ventana solo sale el borrador.
- **Uso diario:** para ver el BOM de un borrador hay que escribirlo en el dibujo y usar `RACKBOMTOTAL`, o usar RACKEDITAR fuera del borrador. Va contra la idea del borrador: probar antes de escribir.
- **Contras:** el usuario pierde en el panel dos botones que tiene en RACKEDITAR, sin que se haya medido ningún riesgo que justifique quitarlos.

**Descartada como opción: «BOM para ver, sin exportar».**
- Obliga a cambiar la ventana de BOM compartida por todos los sistemas, fijada por `tests/RackCad.UI.Tests/DialogWindowCharacterizationTests.cs:91-157`.
- Además, la exportación escribe lo mismo que la biblioteca: un archivo que elige el usuario.

**Por qué (A), en una frase:** ni el BOM ni la biblioteca escriben en el dibujo ni anotan la petición clásica, y el riesgo de identidad de H-PB-33 queda en una condición que PB-T2 puede probar, así que ocultarlos cuesta uso diario sin quitar ningún riesgo medido. A cambio, PB-T2 carga con dos costuras y sus pruebas, y se aceptan dos salidas a disco fuera del borrador.

## 4. La pregunta, lista para AskUserQuestion

**header:** `D-PB-07`

**question** (347 caracteres):

> «Editar todo» abrirá el editor Push Back desde el panel, sobre el borrador y sin escribir en el dibujo. Insertar e «Insertar varias…» ya quedan fuera porque dibujan (D-PB-01). Faltan dos botones que no tocan el dibujo: «BOM» (ver y exportar a Excel o CSV) y «Guardar en biblioteca» (un archivo .rackcad.json). ¿Cuáles ofrece el editor en ese modo?

**options** (`multiSelect: false`):

| label | description |
|---|---|
| BOM y biblioteca (Recomendado) | El editor del panel tiene los mismos botones que RACKEDITAR, menos los que dibujan. PB-T2 prueba que ninguno cambia el borrador ni su GUID ni pide un dibujo. |
| Solo el BOM | Se oculta «Guardar en biblioteca»: para guardar una variante habrá que actualizar el dibujo y usar RACKEDITAR. Quita el único botón que toca la identidad, aunque hoy no tiene efecto en un rack con GUID. |
| Ninguno de los dos | Solo queda «Aplicar al borrador». Es lo más simple de probar, pero el BOM de un borrador solo se verá después de escribirlo en el dibujo. |

## 5. Qué NO decide esta pregunta

- **No reabre Insertar ni el lote de vistas.** Los deciden D-PB-01 y OB-01.6. Crear vistas sigue siendo del clásico (§6.4 del paquete, fila 1).
- **No cambia RACKEDITAR.** En el modo CLASSIC los cuatro botones siguen como hoy (OB-01.5 y §7.4).
- **No decide los rótulos ni la forma de ocultar.** «Aplicar al borrador» y la ocultación son de OB-01.6.
- **No decide los subdiálogos de edición** (configurador de cabecera, seguridad, botas, defensas y topes). Son parte de editar y siguen en los dos modos.
  - El configurador guarda el ancho de sus columnas en el perfil del usuario al cerrarse (H-PB-33). Es una preferencia de pantalla, no un dato del rack.
  - No se pregunta aquí. Si quieres tratarlo, es otra pregunta.
- **No decide quién es el dueño de las ventanas hijas en el modo WORKSPACE** (D-15, punto 4). Lo diseña PB-T2.
- **No toca qué escribe el «Actualizar» del panel** (PB-T3), ni el conflicto y «Conservar mi borrador» (PB-T4 y D-PB-05).
- **No cambia el contenido ni la precisión del BOM** (D1 de PB-TC2), ni el formato de la biblioteca.
- **No emite PB-T2.** El contrato sigue su cadena. Si PB-T2 no puede mantener en verde alguna guarda de T1B, sigue vigente la vuelta de OB-T1B.3.

## Fuentes (revisión `2f5d3dec`)

- `docs/initiatives/I-64-f4b-push-back-completo-package.md`: H-PB-30, H-PB-33, D-PB-01, OB-01.3, OB-01.6, D-PB-07, OB-07.1, §4, §5 (filas de PB-T2 y PB-T4), §6.4, §7.3 y §10.
- `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`: `OD:22-49`.
- `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/package/independent-review-v2.md` §2.2, y `map-b-editor-state-testability.md` (H-097 a H-100).
- `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/task-close-package.md` y `block-m/block-m-analysis.md`.
- El código y las pruebas citados en cada fila.
