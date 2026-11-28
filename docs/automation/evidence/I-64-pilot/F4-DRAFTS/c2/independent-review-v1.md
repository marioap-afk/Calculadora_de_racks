# Revisión independiente del paquete F4-T4-HOST-C2, v1 (adversarial, solo lectura)

**Objeto:** `F4-DRAFTS/c2/I-64-f4-t4-host-c2-gate-contract-package.md` (PKG en adelante), con su evidencia `F4-DRAFTS/c2/diagnosis-report-c2.md` (DIAG) y `F4-DRAFTS/c2/map.md` (MAP, tratado como afirmaciones a verificar).
**Código:** worktree de la rama en `e67fb511` (producto `b86c3cfe`). Rutas relativas a su raíz. El diff del build de diagnóstico se comprobó: es exactamente una línea nueva en `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`, dentro del bucle de `:167-171`, con un comentario en español «DIAG-C3: …».
**Lo que no se hizo:** ni build, ni pruebas, ni escrituras de git, ni AutoCAD.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Además, como el propio PKG dice, no se puede emitir sin la decisión del Owner (§9).

**Recuento:** **B 0 / I 4 / m 12.**

La vía (W) es técnicamente sólida. La medición de H-06 la respalda, y la conclusión de H-09 («ningún cambio de guarda») se sostiene. Pero hay tres problemas:
- la base que se le da al Owner para decidir tiene un error de hecho: Cantilever no pasa por la primitiva (I-1);
- falta medir o regular dos efectos sobre lo ya ratificado (I-3);
- el contrato deja sin fijar un literal que sus propias invariantes y paradas necesitan (I-2).

---

## Hallazgos importantes

### I-1. H-07 y la opción (W) de §9 no son ciertas para Cantilever: su redibujo clásico se queda con F4-S2-03

**Lo que afirma el PKG:**
- PKG:51 dice que `RedefineSystemBlock` «la usan todas las redefiniciones»;
- PKG:89 y PKG:161 dicen que (W) «corrige también los redibujos clásicos».

**Lo que hace el código de Cantilever:**
- `RACKEDITAR` de Cantilever llega a `RackCantileverCommands.EditCantilever` (`src/RackCad.Plugin/RackCantileverCommands.cs:199`) a través de `KindHandlers/CantileverKindHandler.cs:23`;
- ese camino redefine con `CantileverViewMaterializer.RedefineBlock`, en una transacción propia por vista (`RackCantileverCommands.cs:363-370`), y termina con un único `document.Editor.Regen()` (`:399-402`);
- `CantileverViewMaterializer.RedefineBlock` (`src/RackCad.Plugin/Drawing/Cantilever/CantileverViewMaterializer.cs:61-88`) borra y vuelve a poblar la BTR, y en `:83-87` repite **el mismo bucle**: abre para escritura y llama a `RecordGraphicsModified(true)`, sin ninguna modificación que quede en el Undo;
- no pasa por `SystemBlockWriter` ni por `LateralHeaderDrawer`;
- el Freeze ya lo dice: Cantilever «confirma su propia transacción» (`docs/initiatives/I-64-proposal-v5.md:536-539`);
- y I-55 cita juntos los dos bucles (`docs/initiatives/I-55-proposal-v5.md:571-573`).

**Consecuencia:** según el modelo causal del propio PKG (H-04, H-05), después de (W) el `U` tras un `RACKEDITAR` de Cantilever **seguirá sin refrescar**. Los sistemas quedarían inconsistentes, y el Owner decidiría con una premisa falsa.

**Lo que sí pasa por la primitiva:** el resto de redefiniciones, comprobado. H-07 debería nombrarlas todas:
- Selectivo, Dinámico, Cabecera (lateral y planta), Cama y Push Back clásicos (`*Commands*.cs` → `ViewBlockDraw.RedrawInPlace`/`LateralHeaderDrawService.RedrawInPlace` → `SystemBlockWriter.RedrawInPlace`, `SystemBlockWriter.cs:67-102`);
- el ejecutor de variables (`ProjectVariableMutationExecutor.cs:235-257`);
- el piloto (`RackPilotUpdateWriter.cs:275-286`);
- **y el redibujo de hermanas de Selectivo al Insertar** (I-55 OV-RED: `RackSelectivoInsertIntegration.cs:241-250` → fachadas `RedrawInTransaction` → `ViewBlockDraw.cs:97-109` → `SystemBlockWriter.cs:126-138`, aplicado por `SiblingRedrawTransaction.cs:34-39`). MAP §2 dice que `SiblingRedrawTransaction` «no está en ninguna de las dos rutas» de Push Back. Es cierto, pero **sí pasa por la primitiva**, y H-07 no lo nombra.

**Corrección:**
1. Reescribir H-07 con la lista completa anterior y con la excepción de Cantilever.
2. En D-C2-02 y §9, (W) pasa a «corrige el panel y todos los redibujos que usan `SystemBlockWriter`; **no** Cantilever».
3. Añadir al §9 una pregunta del Owner: ¿Cantilever entra, con la misma sentencia en `CantileverViewMaterializer.cs:83-87`? Si entra:
   - es una segunda sentencia NO medida (choca con la parada C2-S01 tal como está escrita);
   - abre una ruta más en AllowedWriteScope, hoy prohibida por «Drawing/ archivo a archivo»;
   - la guarda nueva necesita un segundo sitio.

   Si no entra, se declara como residual R-4 y en Smoke-2 se espera para Cantilever el comportamiento de hoy.
4. Anotar que la variante caller-owned de Cantilever prevista en F5 (`I-64-proposal-v5.md:980`, `:1043`) tendría que llevar el mismo toque.

### I-2. El comentario no está fijado literalmente, pero INV-C2-01, C2-S05 y la guarda nueva dependen de él

**Lo que fija el PKG:**
- D-C2-01 fija la sentencia «exactamente», pero el comentario solo por su intención (PKG:83);
- INV-C2-01 exige que el diff de `src/` sea «exactamente la sentencia … y su comentario» (PKG:129), lo que no se puede comprobar sin un literal;
- C2-S05 habla de «fidelidad literal» (PKG:142).

**Riesgos concretos:**
- **El idioma.** `AGENTS.md:80` exige comentarios en inglés que expliquen RESTRICCIONES, imitando el archivo. `LateralHeaderDrawer.cs` está comentado en inglés (`:163-166`), y el comentario del build de diagnóstico estaba en español.
- **La cuenta de la guarda nueva.** La regla «la sentencia aparece UNA vez en el archivo» (PKG:117) contaría también un comentario que cite la sentencia (algo natural al explicar F4-S2-03), salvo que se cuente sobre el código sin comentarios.
- **Las barridas sobre el texto crudo de todo el Plugin.** Cuentan los comentarios. No saltan con un comentario normal, pero sí con ciertas palabras:
  - `RackUnitsGuardSourceTests.cs:88-101` (cualquier `Insunits`);
  - `SelectiveEditorOpenTests.cs:565-572` (el censo bruto de `[CommandMethod(`, 40);
  - `CustomPropertiesCommandGuardTests.cs:178-189`;
  - `PushBackRoundTripSourceGuardTests.cs:66-84`;
  - `I55G15Auth15ConsumerTests.cs:44-55`.

**Corrección:**
1. Fijar en D-C2-01 el literal exacto del comentario, en inglés, sin `//` incrustado ni tokens de esas barridas. Por ejemplo: `// Opening for write records no undo; this no-op assignment does, so U and REDO repaint every reference of the redefined block (F4-S2-03).`
2. La guarda nueva cuenta y ordena sobre el código **sin comentarios** (`I55ProductCharacterizationTestSupport.Code` o `WorkspaceHostSource.CodeOnly`), con una regex tolerante al espacio en blanco.

### I-3. Hay dos efectos sobre lo ratificado sin medir ni regular: el estado del panel tras «Actualizar» y Rehacer

**(a) El resultado ratificado de C1 tras «Actualizar».** C1 deja «Limpio» + «Actualizado», estable a los 5 s, sin la marca (DIAG §1, `F4-DRAFTS/c2/diagnosis-report-c2.md:23`; IC-09).
- La tabla de H-06 con el DLL nuevo solo informa del estado tras `U` y `REDO` («Comprobar ahora», DIAG:63). No dice qué mostraba el panel **inmediatamente después del «Actualizar»** con el toque.
- D-C2-03 afirma que el `ObjectModified` del toque cae dentro del ámbito propio y se coalesce (PKG:97). Es una inferencia razonable: el commit ocurre dentro del ámbito (`RackPilotUpdateWriter.cs:301-307`; `EventBridgeCore.cs:346-353`). Pero no está medida.

**(b) Rehacer.**
- Con el arreglo, el `REDO` del clásico falló 1 vez («Nothing to redo»). Sin el arreglo hay 1 de 1 correcto (DIAG:64-66).
- El PKG lo deja como R-2 y en Smoke-2 solo «se anota como OBSERVADO» (PKG:149).
- Un Rehacer que se pierde por el arreglo sería un cambio de la categoría «Undo» de D5 (`owner-f4t4c-decisions-d1-d6.md`, D5). El contrato no tiene ninguna regla de decisión para ese caso.

**Corrección:**
1. Declarar (a) como «no medido» en H-06 y D-C2-03.
2. Añadir a Smoke-2 la comprobación explícita del paso 14 (Limpio estable tras «Actualizar») con el DLL de C2.
3. Para (b), añadir un punto de Smoke-2 con muestra mínima (por ejemplo, 3 secuencias `U` → `REDO` inmediatas en el panel y 3 en el clásico, sin lecturas del panel en medio).
4. Añadir una parada post-cierre: si «Nothing to redo» se reproduce con el DLL de C2 en una secuencia que con el DLL de C1 rehace, el resultado vuelve al Owner (D5), no se anota como OBSERVADO.

### I-4. La pregunta al Owner no le presenta como decisión el cambio no observado en los demás sistemas

**Lo que dice el PKG:**
- D-C2-06 (ii) y R-1 (PKG:110, PKG:154) declaran que solo se midió Push Back;
- §9 presenta (W) solo como un beneficio («corrige también los redibujos clásicos», PKG:161).

**Lo que de verdad cambia (W):** lo que el usuario ve tras `U` y `REDO`, la categoría «Undo» de D5, en al menos siete caminos fuera del piloto, ninguno observado (I-1). Entre ellos está el redibujo de hermanas de Insertar, que tiene su propia prueba física ratificada en I-55 (OV-RED, `I-64-proposal-v5.md:507-508`).

**Primitiva compartida:** el Freeze de I-55 describe exactamente este bucle (`I-55-proposal-v5.md:569-575`), y su caracterización G3 lo fija (`tests/RackCad.Tests/InsertRedrawLayerAccessCharacterizationTests.cs:5`, `:27`). El PKG lo cita como autoridad (PKG:30), pero no analiza qué exige.

**Corrección:** que la decisión 1 de §9 diga «(W): corrige el panel y cambia, **sin observarlo**, el `U`/`REDO` de Selectivo, Dinámico, Cabecera, Cama, el ejecutor de variables y el Insertar de Selectivo; Cantilever, según I-1». Y añadir una subpregunta: ¿basta con el riesgo declarado, o se pide un punto de OV por sistema (por ejemplo, en la OV final de I-64)?

---

## Hallazgos menores

### m-1. H-10: la clausura de T1B no es «solo de `src/RackCad.UI/`»

- La clausura de T1B incluye G4 (`src/RackCad.Application/Systems/PushBack/**`, sin `Pilot/`) y G5 (`src/RackCad.Plugin/Systems/PushBack/*.cs`, `RackPushBackCommands.cs` y `KindHandlers/PushBackKindHandler.cs`) (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md:214-221`).
- `LateralHeaderDrawer.cs` queda fuera porque es un archivo compartido del Plugin (`:212`), no porque la clausura sea solo de UI.
- La conclusión vale. Conviene también decir que no es una «fachada» de la parada F4T4C-C03.

**Corrección:** cambiar el motivo en PKG:65.

### m-2. D-C2-03: el `ObjectModified` por referencia probablemente no es nuevo

- `RecordGraphicsModified(true)` ya marca la referencia como modificada sin dejar registro de Undo. Es la propia inferencia de H-04 (PKG:41).
- Lo más probable es que ese evento ya se emita hoy.

**Corrección:** decir «como mucho uno por referencia directa, posiblemente ya emitido hoy por `RecordGraphicsModified(true)`; no medido» (PKG:96).

### m-3. D-C2-04 se queda corto con el número de referencias

- `GetBlockReferenceIds(directOnly: true)` devuelve **todas** las copias enlazadas de la definición, no «de una a unas pocas por vista» (PKG:101).
- RACKRELLENAR y RACKLAYOUT colocan muchas copias de una misma definición (`RackLayoutCommands.Fill.cs:25-29`, `:454-459`; `RackLayoutCommands.cs:250-255`).
- El coste marginal es pequeño, porque la apertura para escritura y `RecordGraphicsModified` ya existen, pero no está medido a escala.

**Corrección:** reformular, y añadir un punto de Smoke-2 con un rack de copias enlazadas, si el dibujo lo tiene.

### m-4. Faltan residuales de las referencias anidadas y de la red asociativa

`directOnly: true` incluye las referencias que están dentro de otras definiciones (`RackSiblingScan.cs:141-147`; `I-55-proposal-v5.md:571-572`).
- **Las anidadas:** se tocan, pero las referencias de primer nivel del contenedor no. Para una vista colocada dentro de un bloque del usuario, o de un `ARRAY` asociativo, S2-03 seguiría. Es coherente con FRZ:341 («referencias anidadas … no se mapean»).
- **La red asociativa:** tocar una referencia que es origen de un objeto asociativo podría reevaluar la red asociativa dentro de la transacción. No está medido. D1 tiene red asociativa (C1, H-06), aunque H-06 de C2 no dio errores.

**Corrección:** declararlos como R-5 en §8. No es un modo de fallo nuevo de capas: la apertura ya existía. Eso sí es correcto.

### m-5. H-09 enumera menos lectores de los que hay

H-09 cita cuatro clases. Faltan:
- **otros dos lectores directos de `LateralHeaderDrawer.cs`:**
  - `SharedViewFoundationF1CharacterizationTests.cs:237-239` (`blockTable.Has(instance.BlockName)` y `BlockNaming.SanitizeBlockName(baseName)`);
  - `StructuralSectionPluginSourceGuardTests.cs:123-124` y `:367` (`"RACKCAD_ANOTACIONES"`);
- **las barridas crudas de I-2;**
- **la barrida de G9** (`WorkspacePilotUpdateIntegrationGuardTests.cs:689-699` vía `WorkspaceHostSourceSupport.cs:48-58`, `:64`):
  - quita los comentarios, así que un comentario que nombre `RackPilot…` es seguro;
  - código que nombrase el piloto en el drawer metería el archivo en G9 con `ForWrite`/`AppendEntity`/`Erase` y saltaría.

Ninguna se rompe con la sentencia. «Ningún cambio de guarda» se sostiene.

**Corrección:** completar H-09, y añadir esas clases a RequiredTests con ExpectRed false (m-6).

### m-6. RequiredTests se reduce respecto del precedente sin explicarlo, y falta el mínimo del filtro nuevo

- C1 llevaba «los 23 filtros de Core de T4C2», con ExpectRed false (`I-64-f4-t4-host-c1-gate-contract-package.md:196`; contrato de C1: `RackUnitsGuardSourceTests`, `I55G15Auth15ConsumerTests`, `PushBackRoundTripSourceGuardTests`, `NamespaceFolderGuardTests`, …).
- D-C2-07 (PKG:120-123) los sustituye por siete, sin decir por qué.
- El `MinSelected` del filtro nuevo (ExpectRed true) no se puede medir sobre la CI de C1 (PKG:123), porque la clase no existe allí.

**Corrección:** conservar la lista de C1 y añadir `SharedViewFoundationF1CharacterizationTests`, `StructuralSectionPluginSourceGuardTests` y `SelectiveEditorOpenTests`. Declarar el mínimo del filtro nuevo desde el diseño (el número de pruebas de la clase).

### m-7. La guarda nueva fija más de lo que posee

- **(a) `RecordGraphicsModified(true)`.** El orden «apertura → sentencia → `RecordGraphicsModified(true)`» (PKG:116) fija una línea que esta tarea no crea: es el arreglo de extensiones de `:163-166`. Una tarea futura que la retire rompería esta guarda.
- **(b) La cuenta.** «UNA vez en el archivo» (PKG:117) es una cuenta de archivo, cuando debería serlo del cuerpo de `RedefineSystemBlock`.
- **(c) Las autopruebas en el RED.** No se dice cómo se comportan en el RED:
  - con el texto real sin la sentencia, la mutación «quitarla» no cambia nada;
  - la mutación «fuera del bucle» tiene que insertar la sentencia.

**Corrección:**
1. Fijar solo «dentro del bucle de `GetBlockReferenceIds(directOnly: true`, después de la apertura `ForWrite` de la misma variable `reference`», y «exactamente una vez en el cuerpo de `RedefineSystemBlock`», sobre código sin comentarios. Si se quiere conservar el orden respecto de `RecordGraphicsModified`, declararlo como fijación de lo medido.
2. Cada mutación de la autoprueba asegura que cambió el texto.

### m-8. La cabecera promete más de lo que corrige con `REGEN`

- PKG:7 incluye en lo que se corrige que «un `REGEN` entre `U` y `REDO` deja sin Rehacer».
- El arreglo quita la **necesidad** del `REGEN`, pero no cambia lo que hace un `REGEN` entre `U` y `REDO`, que es comportamiento de AutoCAD.

**Corrección:** reformular.

### m-9. Los recuentos de H-06 no cuadran con DIAG

- PKG:49 dice «`U` refresca 3 de 3. `REDO` funciona 3 de 4».
- La tabla de DIAG dice «(3 veces)», con `REDO` correcto en 2 de 3 (`F4-DRAFTS/c2/diagnosis-report-c2.md:64`), y luego habla de «1 de 4» (`:66`).

**Corrección:** reconciliar el número de pruebas de `U` y de `REDO`, y qué se hizo entre ellas.

### m-10. H-08 justifica el paso de UNDO único con la transacción equivocada

- En el clásico hay una transacción por vista (`SystemBlockWriter.cs:83-88`; `RackPushBackCommands.cs:279`, `:297`, `:336`).
- El paso único de UNDO lo da la orden (`EXECUTEFUNCTION` o `RACKEDITAR`), no la transacción.
- «Todo o nada» sí se apoya en la transacción del piloto.

**Corrección:** separar las dos razones (PKG:56).

### m-11. La lista de guardas que rompe W-p está incompleta

Además de `:185`, `:195`, la teoría `:312-326` y G9 `:683`, saltarían también las aserciones del texto real del escritor en `WorkspacePilotUpdateGuardTests.cs:260`, `:274`, `:330` y `:349`. La conclusión no cambia.

### m-12. ForbiddenWriteScope y el anexo de Smoke-2, poco precisos

**El alcance:**
- «Drawing/ archivo a archivo» (PKG:73) debe incluir las subcarpetas `Drawing/Cantilever/` y `Drawing/StructuralSections/`, como prefijos o archivo a archivo.
- Hay que decir si se conservan como prohibidas las pruebas que C1 tenía permitidas (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`, etc.).

**Smoke-2 (§7):**
- decir si se repite desde el bloque 1, como recomendaba C1 §8;
- añadir la selección por ventana del rack revertido tras `U`: es el defecto de extensiones que motivó `:163-166`;
- añadir, con I-1, la expectativa para Cantilever.

---

## Afirmaciones verificadas como correctas

- **H-03:** `RedefineSystemBlock` borra el contenido y conserva la BTR y sus referencias (`LateralHeaderDrawer.cs:88-182`). Los `Regen` finales están donde se dice: `RackPilotUpdateWriter.cs:194-202` y `RackPushBackCommands.cs:362-367`.
- **H-04:** el bucle existente abre para escritura y llama a `RecordGraphicsModified(true)` sin cambiar ninguna propiedad (`LateralHeaderDrawer.cs:167-171`). Ninguna prueba nombra `RecordGraphicsModified` ni `Position =` sobre este archivo (grep de `tests/` sin `bin`/`obj`).
- **H-06:** el diff del diagnóstico es exactamente una línea en ese bucle, entre la apertura y `RecordGraphicsModified`.
- **H-07, en parte:** toda redefinición basada en `LateralHeaderDrawer` pasa por `SystemBlockWriter.RedefineInTransaction` (`SystemBlockWriter.cs:126-138`), que es el único llamador de `drawer.RedefineSystemBlock` en `src/`. La excepción es Cantilever (I-1).
- **H-08:**
  - FRZ:502-503 (todo o nada), FRZ:506 (la mecánica no se congela) y FRZ:542-543 (un solo paso de UNDO) dicen lo que se cita;
  - FRZ:227-232, FRZ:237-240, FRZ:339 y FRZ:363-364 quedan intactos, porque el toque no nace de un evento y no hay ningún `Regen` nuevo.
- **H-09, las guardas citadas:**
  - `InsertRedrawLayerAccessCharacterizationTests.cs:22-28` sigue verde, y su `DoesNotContain("IsLocked")` mira el código sin comentarios (`I55ProductCharacterizationTestSupport.cs:25-30`);
  - `RackDefinitionCreatorGuardTests.cs:216-254` no incluye `RedefineSystemBlock`;
  - `CallerOwnedTransactionGuardTests` lee `SystemBlockWriter.cs` y `LateralHeaderDrawService.cs`, no el drawer (`:48`, `:50`).
- **H-10, la parte del alcance:** todos los contratos de F4 prohíben `src/RackCad.Plugin/Drawing/`, de forma explícita o por el prefijo `src/RackCad.Plugin/`. Revisados todos los `F4-*/coordinator/gate-contract*.json`.
- **D-C2-01, las capas:** la apertura para escritura ya existía, así que no hay modo de fallo nuevo por capas bloqueadas. El piloto comprueba las capas de las referencias directas (`RackPilotUpdateWriter.cs:57-62`; `RackSiblingScan.cs:141-147`).
- **D-C2-03, el piloto:** el commit de MUTATE ocurre dentro del ámbito de mutación propia, y los eventos de objeto de ese ámbito son propios (`EventBridgeCore.cs:333-353`).
- **D-C2-02, W-p:** colocar el toque en el escritor rompe G-n ratificadas del escritor (`WorkspacePilotUpdateGuardTests.cs:185`, `:195`) y G9 (`WorkspacePilotUpdateIntegrationGuardTests.cs:683`). Es el disparador «comportamiento ratificado de T4B/T4C1» de D5.
- **La sentencia, en lo técnico:**
  - el setter de `Position` es una modificación que AutoCAD registra en el Undo dentro de la misma transacción y del mismo paso de orden. El build de diagnóstico lo confirma;
  - no cambia la geometría de referencias simétricas, con atributos, dinámicas ni anotativas, porque escribe el mismo valor WCS;
  - los handles no cambian, así que la navegación por handle y el eco no se ven afectados.
- **La gobernanza:** es correcto que el PKG no se emita sin el Owner. Cambia el `U`/`REDO` que ve el usuario (la categoría «Undo» de D5) y amplía el alcance de F4 a una primitiva compartida que todos sus contratos prohibían. La orden del Owner manda anotar las decisiones materiales «sin parar el resto».
