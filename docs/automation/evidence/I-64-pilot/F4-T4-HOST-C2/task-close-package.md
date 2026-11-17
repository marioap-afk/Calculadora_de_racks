# I-64 — F4-T4-HOST-C2: paquete de cierre

**Resultado: CLOSED / PASS WITH LIMITATIONS**, en el intento 1 de 3.

**Autoridades del cierre:**
- la decisión del Owner sobre F4-S2-03 v2, «(W), Cantilever fuera, basta, antes» (`F4-DRAFTS/coordinator/owner-f4-s2-03-v2-decision.md`, F4-E77);
- la orden del Owner «Sigue con la cadena hasta el cierre» (`F4-DRAFTS/coordinator/owner-f4-c2-continue.md`);
- la disposición del Owner/Coordinator sobre nc1, «A — SOLO C2» (`F4-DRAFTS/coordinator/owner-f4-c2-nc1-decision.md`), que responde a `F4-DRAFTS/c2/owner-decision-request-c2-nc1.md`;
- el paquete v2 (`docs/initiatives/I-64-f4-t4-host-c2-gate-contract-package.md`, §7: «el cierre, por la regla D2»);
- la decisión D2 del Owner (`owner-f4t4c-decisions-d1-d6.md`).

Se cumplen todas las condiciones de D2, con la disposición del Owner para nc1:
- las dos revisiones, con 0 bloqueantes y 0 importantes;
- la CI exacta y la verificación, en verde;
- nc1 por el sustituto LOCAL de C2 que autorizó el Owner, conservando su salida ordinaria; nc2 y nc3 por su oráculo ordinario;
- ningún otro modo de fallo nuevo.

## 1. Estado resultante

**El producto:** `9abc1320`. Corrige F4-S2-03: tras «Actualizar» del panel, o un redibujo clásico, `U` deshacía la base de datos pero la pantalla seguía con el redibujo hasta un `REGEN`.
- `LateralHeaderDrawer.RedefineSystemBlock` reasigna a cada referencia directa de la definición redefinida su misma posición (`reference.Position = reference.Position;`), dentro del bucle y tras su apertura para escritura, con el comentario fijado.
- El paso de Undo registra así el estado de esas referencias, y `U` y `REDO` las vuelven a dibujar. La sentencia es la medida en AutoCAD con un build desechable (F4-E76).
- **La guarda nueva** `RedefinedViewReferenceUndoTouchGuardTests` (4 casos) fija la sentencia sobre el código sin comentarios.

**Lo que no cambia:**
- el Freeze de I-64: ni el drenaje ni los eventos, ningún `Regen` nuevo y el mismo número de pasos de Undo;
- ninguna guarda existente;
- Cantilever (`Drawing/Cantilever/`), fuera por decisión del Owner.

**El resto:**
- **`attempts`:** 1 de 3;
- **main** sigue en `812baf87`.

## 2. Corridas

**La planificación** `R20261116T124830Z-cfb1`:
- fidelidad byte a byte PASS en las 8 colecciones, a la primera;
- A1-A8 8/8;
- **nc4** `R20261116T125238Z-f80f`: A3 en fail, con el oráculo cumplido.

**El Worker** `R20261116T125319Z-3ea9`:
- `claude-sonnet-5-5`/high, 0 denegaciones;
- prompt por archivo, con comprobación previa del SHA-256;
- RED `fe165a63` (solo la guarda) y GREEN `9abc1320` (solo las dos líneas), con 2 rutas, exactamente las del AllowedWriteScope.
- **Historia de ejecución:** el equipo entró en espera moderna a las 13:06:51Z y en suspensión a las 16:23Z, hasta las 22:31Z, y la cesión se cortó antes del commit RED. La sesión reanudó el MISMO Worker por mensaje, con su contexto y su WorkRunId. El Worker repitió después del commit RED las corridas previas al corte. Por la disposición del Owner, no es un defecto del producto ni un motivo para repetir el Worker.

**La CI exacta:**
- **GREEN** `43972738540`: 4/4 en success. Core completa 19067/19067; la clase nueva 4/4; Core Workspace 3482/3482; UI Workspace 694/694;
- **RED** `43971230759`:
  - Core en failure, con 3 fallos de 19067, todos de la clase nueva y por aserción;
  - UI en success;
  - Build UI en success;
  - Build Plugin omitido por su needs.

**La verificación** `R20261116T232445Z-5006`: **EXECUTION_VERIFIED, 14/14**, `VerifiedSha` `9abc1320`.

## 3. Revisiones y controles

**Revisiones** (`dcf3396f..9abc1320`):
- **la directa de la sesión** (`direct-review.md`): **ACEPTABLE CON MENORES, 0/0/3**;
- **la adversarial independiente** (`independent-review.md`): **ACEPTABLE CON MENORES, 0/0/3**, sin hallazgos OWNER.

**Controles** sobre las entradas de `5006`:

| Control | Corrida | Salida ordinaria | Resultado |
|---|---|---|---|
| nc1 | `R20261116T232824Z-a11e` | CONTROL NOT PASSED: BLOCKED/STOP, Handoff primero; difieren Handoff, Identity, Remote, Ci, Tests **y Trailer** | Firma **no exacta** de PD-F4-04, por Trailer. Disposición del Owner «A — SOLO C2». Sustituto local `F4C2-NC1-TRAILER-EXCEPTION-01`: **PASS**, las 10 condiciones; autoprueba 18/18, determinista. **nc1 = SATISFIED_UNDER_F4C2_NC1_TRAILER_EXCEPTION** |
| nc2 | `R20261116T233207Z-ffdd` | **CONTROL PASS:** Scope en fail con la ruta quitada | ordinario; PD-F4-01 no se reprodujo |
| nc3 | `R20261116T233755Z-5d84` | **CONTROL PASS:** FreeText en fail, con REWORK | ordinario |

**nc1, declarado:**
- **La copia mutada difiere de la entrega real solo en `CurrentSha`**, más el hash derivado del registro de relevo.
- **El Controller la detectó:** Identity en fallo, con el SHA falso y el HEAD real.
- **Pero dio Trailer en fallo:** buscó un campo `Trailer` en la raíz de la entrega. El esquema real (`worker-handoff.schema.json`) lo pone bajo `Worker` y la raíz no admite propiedades adicionales; el valor es idéntico en la copia y en la entrega real, y en la verificación real lo leyó bien.
- **El sustituto general ratificado no se modificó.** Clonado solo con su id, su alcance y su etiqueta, y ensayado en seco antes de la decisión, da FAIL solo por su propiedad 6 (Trailer) (`F4-T4-HOST-C2-nc1-substitute-dry/`).
- **El sustituto local** (`F4-T4-HOST-C2-nc1-trailer-exception/`) es un script propio de C2: no es global ni se reutiliza en otras tareas. Demuestra las diez condiciones de la disposición, C01 a C10.
- **El registro de relevo de nc1 anterior a la decisión** se conserva aparte (`relay-record-before-owner-decision.json`, SHA-256 `e66fad21238c7d3b81b2e5ba80f1e72ac08d3cec4d3a16a11cb6c30d00522093`).
- **Deuda nueva PD-F4-08:** el Controller puede buscar Trailer en la raíz de la entrega en lugar de bajo Worker durante una corrida de control negativo.
- nc1 no se repitió.

## 4. Limitaciones

**Los menores de las revisiones.** Ninguno cambia el producto ni pide un RED/GREEN nuevo.
- **M-1 (independiente):** la autoprueba 4 («moverla fuera del bucle») pone la sentencia antes del bucle y de la apertura. La rechaza la cláusula de orden, no la de contención (`[^}]*?`), así que ninguna autoprueba fija la contención.
- **M-2 (independiente):** una sentencia condicional dentro del bucle (`if (…) reference.Position = reference.Position;`) pasaría la guarda. D-C2-07 pide «dentro del bucle», no «incondicional».
- **M-3 (independiente):** la guarda falla cerrada ante refactorizaciones inocuas (otro molde, `var` explícito, renombrar `tr`, un bloque con llaves en medio). Es la familia de `InsertRedrawLayerAccessCharacterizationTests`.
- **m-1 (directa):** en el RED, la autoprueba «quitarla» falla por aserción, porque no hay nada que quitar. Las autopruebas «pueden» pasar en el RED; es conforme a INV-C2-05.
- **m-2 (directa):** la corrida local `h18` (Release, desde el DLL de una carpeta de `artifacts/`) dio 5 fallos de ruta ajenos al producto. Se repitió por el proyecto (`h18b`, 5/5) y la CI la pasa.
- **m-3 (directa):** el mínimo 3468 del filtro de Core Workspace salió de contar en el TRX por `testId`. La CI selecciona 3482, como ya en C1: el mínimo queda 14 por debajo y no detectaría la pérdida de hasta 14 casos. Aquí no se perdió ninguno.

**Los riesgos declarados del paquete (§8):**
- **R-1:** solo se midió Push Back. Selectivo, Dinámico, Cabecera, Cama, el ejecutor de variables y el Insertar de Selectivo cambian su `U`/`REDO` sin observarse. Por la decisión del Owner, basta para F4 y **se anota para la OV final de I-64**.
- **R-2:** el «Nothing to redo» de 1 de 4 del clásico, sin causa aislada. Lo cubre la regla post-cierre del §7 del paquete.
- **R-3:** la inferencia sobre el registro de Undo sale del comportamiento observado, no de documentación.
- **R-4:** Cantilever sigue con F4-S2-03.
- **R-5:** las vistas anidadas en bloques del usuario o en un `ARRAY` asociativo no están medidas.

**La validación física:** las pruebas no usan AutoCAD. La primera comprobación en el host del DLL de C2 es la reanudación de Smoke-2.

## 5. Siguiente

1. **La sesión:**
   - construye el DLL Debug del Plugin sobre `9abc1320`, con AutoCAD cerrado;
   - prepara copias nuevas de los dibujos de Smoke-2;
   - cierra el anexo de C2, que va junto al de C1. La guía v3 no se modifica.
2. **El Owner** opera la reanudación de Smoke-2 desde el bloque 1, con el DLL de C2.
   - **Regla post-cierre (D5):** un «Nothing to redo» con el DLL de C2, en una secuencia que con el de C1 rehace, vuelve al Owner.
3. **RACKPROYECTAR** queda para su propia iniciativa (F4-E73).
