# I-64 — Paquete del contrato de F4-T4-HOST-C2 (corrección de Smoke-2: la vista se refresca tras `U` y `REDO`)

**Estado:** v2, borrador **sin emitir**.
- La v1 tuvo una revisión independiente: EMITIBLE TRAS CORRECCIONES, 0 B / 4 I / 12 m (`F4-DRAFTS/c2/independent-review-v1.md`). Todo se incorporó: §10.
- Necesita la decisión del Owner (§9): amplía el alcance de F4 a una primitiva de dibujo compartida, que todos los contratos de F4 prohibían, y cambia lo que el usuario ve tras `U` y `REDO` en otros sistemas.

**Producto base:** `b86c3cfe`, el GREEN de F4-T4-HOST-C1. La rama está en `e67fb511`, más la custodia documental de F4-E76; `src/` y `tests/` son idénticos a `b86c3cfe`.

**Qué corrige:** F4-S2-03. Tras «Actualizar» del panel, o un redibujo clásico, `U` deshace la base de datos pero la pantalla sigue mostrando el redibujo hasta un `REGEN`. Con la corrección, `U` y `REDO` refrescan la pantalla sin `REGEN`, así que ya no hace falta escribir `REGEN`. Lo que AutoCAD hace con un `REGEN` escrito entre `U` y `REDO` (deja sin Rehacer) no cambia, pero deja de ser necesario.

**Qué no corrige:**
- el redibujo de Cantilever, que tiene su propio bucle (H-07);
- el rendimiento del «Actualizar» (D-C1-05);
- RACKPROYECTAR (F4-E73).

---

## 1. Autoridades

**Órdenes del Owner** (literales, en `F4-DRAFTS/coordinator/`):
- `owner-f4-smoke2-fix-order.md`: «arregla todos los errores que encontramos y después continuamos con el SMOKE, arregla todos los errores posibles ya que no estaré en mi casa»;
- `owner-f4-advance-2026-11-16.md`: «Avanza lo más que puedas. No estoy disponible para ver los mensajes, pero necesito que avances.»;
- **para emitir:** la decisión del Owner sobre F4-S2-03 v2, que se custodiará como `owner-f4-s2-03-v2-decision.md`.

**Reglas del Owner que siguen en F4** (`owner-f4t4c-decisions-d1-d6.md`):
- D2: el cierre sin el Owner;
- D3: el intento 2 sin el Owner, y nunca el 3;
- D4: una replanificación reforzada por delegación;
- D5: volver ante una decisión material nueva (semántica de Actualizar, autoridad de escritura, Undo, conflicto/stale, T1B, o cambio del comportamiento ratificado de T4B/T4C1).

**Esta tarea sustituye** la decisión pendiente `F4-DRAFTS/c1/owner-decision-request-s2-03.md`: sus opciones a, b y c, y su enmienda del Freeze, ya no hacen falta (H-08).

**Evidencia** (`F4-DRAFTS/c2/`):
- `diagnosis-report-c2.md`: el diagnóstico en AutoCAD del 2026-11-16, con copias desechables cerradas sin guardar y un build desechable;
- `diag3.patch`: la línea medida;
- `map.md`: el mapa de código, solo lectura;
- `independent-review-v1.md`.

**Diseño vigente:**
- el Freeze de I-64 (`I-64-proposal-v5.md`; FRZ en adelante);
- el paquete de F4-T4C (D-T4C-09) y el de F4-T4B (el escritor);
- el Freeze I-55 V5, que describe este mismo bucle (`I-55-proposal-v5.md:569-575`). Su caracterización G3 lo fija en `InsertRedrawLayerAccessCharacterizationTests.cs:27`.

## 2. Hechos medidos

**El defecto**
- **H-01. Se reproduce con el DLL de C1:** panel, escala 2 → 4 → `U`. `EXECUTEFUNCTION` queda deshecho y el rótulo sigue grande. `REDO` funciona.
- **H-02. Es PREEXISTENTE:** `RACKEDITAR` clásico de Push Back, escala 4 → 2 → `U`. El rótulo sigue con el tamaño de la escala 2, y tras `REGEN` vuelve al de la 4. `REDO` funciona (1 de 1 con el DLL de C1).
- **H-03. Por qué:**
  - el redibujo reemplaza el CONTENIDO de la definición y conserva sus referencias (`LateralHeaderDrawer.cs:88-182`);
  - termina con un `Editor.Regen()` dentro de su orden (`RackPilotUpdateWriter.cs:194-202`; `RackPushBackCommands.cs:362-367`);
  - `U` revierte la definición, pero ni vuelve a ejecutar el `Regen` ni rehace las referencias.
- **H-04. Lo que ya hay no basta:** `RedefineSystemBlock` ya abre cada referencia directa para escritura y llama a `RecordGraphicsModified(true)` (`:167-171`), y S2-03 ocurre igual. Inferencia por el comportamiento observado, no por documentación: abrir para escritura sin modificar nada no deja registro de Undo de la referencia.

**La corrección, medida en AutoCAD**
- **H-05. Experimento con LISP:**
  - `UNDO BE` → `RACKEDITAR` → `entmod(entget(...))`, con los mismos datos, sobre las dos inserciones de primer nivel → `UNDO E` → `U`: la pantalla se refresca sola, y `REDO` también;
  - **control** (lo mismo sin el `entmod`): la pantalla no se refresca.
- **H-06. Build desechable** (`<worktree-diag3>`, detached `e67fb511`, una línea en el bucle `:167-171`; DLL `E2DD7BB8…2F0B`):
  - **panel**, escala 1 → 4, una secuencia:
    - justo después de «Actualizar»: `Limpio` + «Actualizado…», sin la marca, como con C1 (una sola captura, sin seguimiento a los 5 s);
    - `U` refresca sola, sin `REGEN`;
    - `REDO` («Everything has been redone») refresca;
    - después, «Comprobar ahora» da `Limpio`, y los rótulos «PB G» siguen (planta=1, lateral=1);
  - **`RACKEDITAR` clásico**, cuatro secuencias:
    - `U` deshace en las 4, y la pantalla se refresca en las 3 que se capturaron;
    - `REDO` da «Everything has been redone» en 3 de 4;
    - en la 1.ª, «Nothing to redo», que no se reprodujo en las tres siguientes. Lo que hubo entre `U` y `REDO` en cada una está en el diagnóstico, §4;
  - ningún error: no hay registro de RackCad del día ni eventos de error de `acad.exe`.
- **H-07. Qué redibujos pasan por la primitiva.** `SystemBlockWriter.RedefineInTransaction` es el único llamador de `drawer.RedefineSystemBlock` (`SystemBlockWriter.cs:126-138`).
  - **Pasan:**
    - los redibujos clásicos de Selectivo, Dinámico, Cabecera (lateral y planta), Cama y Push Back (`*Commands*.cs` → `ViewBlockDraw.RedrawInPlace` o `LateralHeaderDrawService.RedrawInPlace` → `SystemBlockWriter.RedrawInPlace`, `:67-102`);
    - el ejecutor de variables (`ProjectVariableMutationExecutor.cs:235-257`);
    - el «Actualizar» del piloto (`RackPilotUpdateWriter.cs:275-286`);
    - el redibujo de hermanas del Insertar de Selectivo (I-55 OV-RED: `RackSelectivoInsertIntegration.cs:241-250` → fachadas → `ViewBlockDraw.cs:97-109` → `SystemBlockWriter.cs:126-138`, aplicado por `SiblingRedrawTransaction.cs:34-39`).
  - **No pasa Cantilever.** `CantileverViewMaterializer.RedefineBlock` (`Drawing/Cantilever/CantileverViewMaterializer.cs:61-88`) tiene su propio bucle (`:83-87`), con la misma apertura y sin modificación. Con esta corrección, el `U` tras un `RACKEDITAR` de Cantilever seguirá sin refrescar.
  - **Solo se midió Push Back.**

**Freeze y guardas**
- **H-08. El Freeze de I-64 no cambia:**
  - la mecánica del Actualizar no está congelada (FRZ:506);
  - el resultado «todo o nada» (FRZ:502-503) se conserva, porque el toque va dentro de la transacción del piloto;
  - el paso de UNDO único (FRZ:542-543) se conserva, porque lo da la orden (`EXECUTEFUNCTION` o `RACKEDITAR`), no la transacción: el clásico abre una por vista;
  - ningún evento escribe nada: FRZ:227-232, FRZ:237-240, FRZ:339 (D-06) y FRZ:363-364 (D-07) quedan intactos;
  - no hay ningún `Regen` nuevo.
- **H-09. Lo que lee el archivo** (mapa §3 y revisión m-5). Ninguno se rompe con la sentencia y su comentario:
  - `InsertRedrawLayerAccessCharacterizationTests.cs:22-28` (`tr.GetObject(referenceId, OpenMode.ForWrite)`, que se conserva; mira el código sin comentarios);
  - `SharedViewFoundationF1CharacterizationTests.cs:237-239`;
  - `StructuralSectionPluginSourceGuardTests.cs:123-124` y `:367`;
  - `RackDefinitionCreatorGuardTests.cs:216-254`, que no incluye `RedefineSystemBlock`;
  - `CallerOwnedTransactionGuardTests` y `CallerOwnedFacadeGuardTests`, que leen los envoltorios, no el drawer;
  - las barridas de texto de todo el Plugin, que cuentan también los comentarios: `RackUnitsGuardSourceTests.cs:88-101`, `SelectiveEditorOpenTests.cs:565-572`, `CustomPropertiesCommandGuardTests.cs:178-189`, `PushBackRoundTripSourceGuardTests.cs:66-84` e `I55G15Auth15ConsumerTests.cs:44-55`. El comentario fijado no lleva ninguno de sus tokens;
  - la barrida G9 (`WorkspacePilotUpdateIntegrationGuardTests.cs:689-699`), que quita los comentarios. El código no nombra el piloto.

  **Ningún cambio de guarda.**
- **H-10. Alcance histórico:**
  - todos los contratos de F4 prohíben `src/RackCad.Plugin/Drawing/`, de forma explícita o con el prefijo `src/RackCad.Plugin/`. Esta sería la primera tarea de F4 que lo toca;
  - el archivo no está en la clausura de T1B: es un archivo compartido del Plugin (paquete de T1B §3, `:212`), aunque la clausura incluye G4 y G5;
  - tampoco es una fachada de la parada F4T4C-C03.

## 3. Alcance

**Rutas permitidas (AllowedWriteScope, 2):**
- `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`: dos líneas nuevas en el bucle de `:167-171` (D-C2-01);
- `tests/RackCad.Tests/RedefinedViewReferenceUndoTouchGuardTests.cs`: nuevo.

**ForbiddenWriteScope** (lo genera `make_c2_contract.py`; el generador comprueba que ninguna permitida queda bajo una prohibida):
- las raíces de la solución;
- `src/RackCad.UI/`, `tests/RackCad.UI.Tests/`, `src/RackCad.Domain/` y `src/RackCad.Application/`, completos;
- en el Plugin:
  - todos los subdirectorios salvo `Drawing/`, y todos los archivos de su raíz;
  - en `Drawing/`, sus subcarpetas (`Cantilever/` y `StructuralSections/`) y sus archivos uno a uno, salvo `LateralHeaderDrawer.cs`;
- en las pruebas:
  - `tests/RackCad.Tests/Workspace/` completo, también las que C1 tenía permitidas;
  - las caracterizaciones y guardas de H-09 y su soporte `I55ProductCharacterizationTestSupport.cs`.

## 4. Decisiones

**D-C2-01. Las dos líneas y su sitio.** En `RedefineSystemBlock`, dentro del bucle que recorre `systemDef.GetBlockReferenceIds(directOnly: true, forceValidity: true)`, entre `var reference = (BlockReference)tr.GetObject(referenceId, OpenMode.ForWrite);` y `reference.RecordGraphicsModified(true);`, con la indentación del bucle, exactamente:

```csharp
// Opening for write records no undo; this no-op assignment does, so U and REDO repaint every reference of the redefined block (F4-S2-03).
reference.Position = reference.Position;
```

- **La sentencia** es la medida en H-06; cualquier otra es la parada C2-S01.
- **El comentario** va en inglés, como exige `AGENTS.md`, y no lleva tokens de las barridas de H-09.
- **No cambia la geometría:** es la misma posición WCS, también con simetrías, atributos, bloques dinámicos o anotativos. Los handles no cambian, así que la navegación y el eco no se ven afectados.
- **Ningún modo de fallo nuevo con capas bloqueadas:** la apertura para escritura ya existía, y el piloto ya comprueba antes las capas de esas mismas referencias (`RackPilotUpdateWriter.cs:57-62`).

**D-C2-02. El sitio compartido, frente al solo-piloto** (lo decide el Owner, §9):
- **(W) compartido [recomendado]:** la primitiva. Corrige el panel y cambia, **sin observarlo**, el `U`/`REDO` de todos los redibujos de H-07 salvo Cantilever. Ningún cambio de guarda.
- **(W-p) solo el piloto:** el toque en `RackPilotUpdateWriter.cs`.
  - Rompe guardas ratificadas del escritor:
    - `WorkspacePilotUpdateGuardTests.cs:185` y `:195`, su teoría negativa `:312-326` y las aserciones del texto real en `:260`, `:274`, `:330` y `:349`;
    - los conteos G9 de `WorkspacePilotUpdateIntegrationGuardTests.cs:683`.
  - Es el disparador «cambio del comportamiento ratificado de T4B/T4C1» de D5, y deja el panel y el clásico distintos. **No se recomienda.**
- **Cantilever:**
  - **fuera [recomendado]:** se declara como residual R-4, con el comportamiento de hoy;
  - **dentro:** la misma línea en `CantileverViewMaterializer.cs:83-87`, que está **sin medir**, abre una ruta más y necesita un segundo sitio en la guarda. Si el Owner la quiere, la tarea se amplía y se vuelve a revisar.

**D-C2-03. Efecto sobre los eventos del panel:**
- como mucho, un `ObjectModified` por referencia directa, quizá ya emitido hoy por `RecordGraphicsModified(true)`. No está medido;
- en el piloto ocurre dentro del ámbito de mutación propia: el commit de MUTATE va dentro del ámbito (`EventBridgeCore.cs:333-353`), que lo coalesce (IC-09). Con el build W, el estado justo después de «Actualizar» fue `Limpio` sin la marca (H-06, una captura);
- en una orden clásica es ajeno y marca, como ya marcan los demás objetos que modifica el redibujo;
- ninguna pista nueva.

**D-C2-04. Rendimiento:** una asignación por referencia directa. `GetBlockReferenceIds(directOnly: true)` devuelve **todas** las copias enlazadas de la definición, y RACKRELLENAR y RACKLAYOUT pueden colocar muchas (`RackLayoutCommands.Fill.cs:25-29`, `:454-459`; `RackLayoutCommands.cs:250-255`). El coste marginal es pequeño, porque la apertura y `RecordGraphicsModified` ya existen, pero no está medido a escala.

**D-C2-05. Undo:**
- el número de pasos no cambia: un `EXECUTEFUNCTION` por Actualizar del panel y un `RACKEDITAR` por orden clásica;
- el paso registra además el estado de las referencias directas;
- `U` y `REDO` las rehacen y la pantalla se refresca.

**D-C2-06. D5 y el alcance.** La vía (W) no toca el Freeze de I-64 ni el comportamiento ratificado del escritor. Pero:
- **(i)** cambia lo que el usuario ve tras `U` y `REDO` (la categoría «Undo» de D5), en el panel y en los redibujos de H-07, y solo está observado en Push Back;
- **(ii)** amplía el alcance de F4 a una primitiva de dibujo compartida, prohibida en todos los contratos de F4 (H-10) y descrita por el Freeze I-55 V5.

**Por eso no se emite sin el Owner (§9).**

**D-C2-07. Las pruebas automatizadas.** Ninguna necesita AutoCAD. El comportamiento (que la pantalla se refresque) solo se observa en AutoCAD: H-06 y Smoke-2.

**`RedefinedViewReferenceUndoTouchGuardTests`** tiene **exactamente 4 casos**, sobre el código **sin comentarios** de `LateralHeaderDrawer.cs` (con `I55ProductCharacterizationTestSupport.Code` u otro de los cargadores existentes) y con regex tolerantes al espacio en blanco:
1. la sentencia está dentro del bucle de `GetBlockReferenceIds(directOnly: true`, después de la apertura `ForWrite` de la misma variable `reference`;
2. la sentencia aparece exactamente una vez en el cuerpo de `RedefineSystemBlock`;
3. y 4. autopruebas en negativo, como mutaciones del texto real: quitarla, y moverla fuera del bucle. Cada una comprueba primero que su mutación cambió el texto, y en el RED, donde la sentencia no existe, la de «moverla» la inserta fuera.

**Lo que no fija:** `RecordGraphicsModified(true)` ni el orden respecto de él. Pertenecen al arreglo de extensiones de `:163-166`, que esta tarea no crea.

**Filtros de RequiredTests** (en `make_c2_contract.py`):
- la clase nueva, con **ExpectRed true** y mínimo 4;
- Core `~RackCad.Tests.Workspace`, con ExpectRed false y mínimo 3468 (CI del GREEN de C1, `43801607010`);
- los 23 filtros de clase de C1, sin cambios y con ExpectRed false;
- `InsertRedrawLayerAccessCharacterizationTests` (5), `RackDefinitionCreatorGuardTests` (26), `SharedViewFoundationF1CharacterizationTests` (26) y `StructuralSectionPluginSourceGuardTests` (45), con ExpectRed false;
- UI `~RackCad.UI.Tests.Workspace` (694) y `~PushBackClassicCharacterization` (55), con ExpectRed false, como en C1.

**El RED** es solo la prueba nueva, con el producto sin cambios: falla por aserción. **El GREEN** añade las dos líneas.

## 5. Invariantes

- **INV-C2-01:** el diff de `src/` son exactamente las dos líneas de D-C2-01, en el bucle de `RedefineSystemBlock`. Ningún otro archivo de `src/`; Cantilever no cambia.
- **INV-C2-02:** ninguna regeneración nueva, ningún tratamiento de `U`, `UNDO`, `REDO` ni `MREDO`, ninguna suscripción, pista ni operación de drenaje. El Freeze de I-64 no cambia.
- **INV-C2-03:** las guardas, las caracterizaciones y las barridas de H-09 siguen en verde y sin cambios, también todo `tests/RackCad.Tests/Workspace/`.
- **INV-C2-04:** la geometría, las capas, los nombres, los sobres, la purga y el número de pasos de UNDO no cambian.
- **INV-C2-05:** la guarda nueva es la de D-C2-07. El RED es solo ella y falla por aserción.
- **INV-C2-06 (contención, F4-E59):** ninguna prueba escribe fuera de una ubicación temporal aislada; sin la suite completa de UI en local.

## 6. Paradas propias

- **C2-S01:** la corrección exige otra sentencia, otro comentario u otro sitio que los de D-C2-01.
- **C2-S02:** cualquier cambio de guarda, caracterización o barrida existente.
- **C2-S03:** cualquier `Regen`, evento, suscripción o cambio del drenaje, o cualquier tratamiento de `U`/`UNDO`/`REDO`/`MREDO`.
- **C2-S04:** una decisión material nueva de D5 distinta de las que el Owner haya decidido para esta tarea. **STOP al Owner.**
- **C2-S05:** un fallo de fidelidad literal de la planificación permite una replanificación reforzada (D4); un segundo, STOP al Owner.

## 7. Después

- **El cierre:** por la regla D2.
- **Smoke-2:** se reanuda desde el bloque 1 con el DLL nuevo y un anexo, que añade:
  - en el paso 14: `Limpio` estable 5 s tras «Actualizar»;
  - en el paso 16: `U` refresca sin `REGEN`, y `REDO` funciona y refresca;
  - Rehacer, con una muestra mínima: 3 secuencias `U` → `REDO` inmediatas en el panel y 3 tras un `RACKEDITAR` clásico, sin lecturas del panel entre medias;
  - tras `U`, la selección por ventana del rack revertido (el defecto de extensiones que motivó `:163-166`);
  - si el dibujo lo tiene, un rack con copias enlazadas, y un redibujo clásico de otro sistema seguido de `U`, como OBSERVADO;
  - en Cantilever, si está fuera: el comportamiento de hoy (`U` sin refresco).
- **Regla post-cierre (vuelve al Owner, D5):** si «Nothing to redo» se reproduce con el DLL de C2 en una secuencia que con el DLL de C1 rehace, el resultado vuelve al Owner. No se anota como OBSERVADO.

## 8. Riesgos declarados

- **R-1:** solo se midió Push Back. Los demás caminos de H-07 comparten la primitiva, pero no están observados.
- **R-2:** el «Nothing to redo» de H-06 (1 de 4 en el clásico), sin causa aislada. Lo cubre la regla post-cierre del §7.
- **R-3:** la inferencia de H-04 sobre el registro de Undo sale del comportamiento observado, no de documentación.
- **R-4:** Cantilever, si queda fuera, sigue con S2-03. La variante caller-owned de Cantilever prevista en F5 (FRZ:980, FRZ:1043) tendría que llevar el mismo toque.
- **R-5:** `directOnly: true` incluye referencias anidadas en otras definiciones. Para una vista colocada dentro de un bloque del usuario o de un `ARRAY` asociativo, el contenedor no se toca y S2-03 podría seguir (coherente con FRZ:341). Tocar una referencia que sea origen de un objeto asociativo podría reevaluar la red asociativa dentro de la transacción. No está medido; D1 tiene red asociativa y H-06 no dio errores.

## 9. Lo que decide el Owner

1. **¿Corregir F4-S2-03 en I-64?**
   - **(W) [recomendada]:** sí, con este paquete, en la primitiva compartida. Corrige el panel y cambia, **sin observarlo**, el `U`/`REDO` de Selectivo, Dinámico, Cabecera, Cama, el ejecutor de variables y el Insertar de Selectivo.
   - **(b):** no. Smoke-2 lo declara como limitación preexistente (H-02), y se corrige aparte, en otra iniciativa o en un hotfix.
   - **(W-p):** solo el piloto. No se recomienda (D-C2-02).
2. **Si es (W): ¿Cantilever entra?** Fuera, como R-4 [recomendado], o dentro, sin medir (D-C2-02).
3. **Si es (W): ¿basta el riesgo declarado de los demás sistemas,** o se pide un punto de OV por sistema, por ejemplo en la OV final de I-64? [recomendado: basta para F4 y se anota para la OV final].
4. **Si es (W): ¿antes o después de Smoke-2?**
   - **antes [recomendado]:** Smoke-2 lo comprueba;
   - **después:** Smoke-2 sigue con el DLL de C1 y S2-03 declarado.

## 10. Disposición de la revisión independiente de la v1 (0 B / 4 I / 12 m)

| Hallazgos | Se incorporan en |
|---|---|
| I-1 | Cabecera, H-07, D-C2-02, R-4, §7 y §9.2 |
| I-2 | D-C2-01 (el comentario literal en inglés) y D-C2-07 (código sin comentarios) |
| I-3 | H-06 (el estado tras «Actualizar», medido en una captura) y el §7 (paso 14, muestra de Rehacer y regla post-cierre) |
| I-4 | D-C2-02, D-C2-06 y §9.1 y §9.3 |
| m-1 | H-10 |
| m-2 | D-C2-03 |
| m-3 | D-C2-04 y §7 |
| m-4 | R-5 |
| m-5 y m-6 | H-09 y D-C2-07 (RequiredTests) |
| m-7 | D-C2-07 |
| m-8 | Cabecera |
| m-9 | H-06 y el diagnóstico, §4 |
| m-10 | H-08 |
| m-11 | D-C2-02 |
| m-12 | §3 y §7 |
