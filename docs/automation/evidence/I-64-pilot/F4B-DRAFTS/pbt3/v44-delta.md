> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v44-delta: la comprobación acotada de PB-T3a (CA-T3a), hallazgo a hallazgo

**Qué es:** el cambio de la v4.4 para cada hallazgo de CA-T3a (`independent-review-pbt3a-v43-check.md`: EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 4, sin decisiones del Owner), con sus ids V44-xx. Todo se leyó por objetos git desde el checkout principal, en `05beabfd` (P-08). El worktree no se tocó.

**Resultado:**
- **Se corrigen en el paquete I-01, M-01, M-02 y M-03.** M-04 es del generador.
- **La v4.4 es solo de texto:** no cambian el diseño ni D-1.
- **Lo que cambia:**
  - dos recetas, CT-PBT3A-09 y CT-PBT3B-01;
  - una regla del §5.0, con el repaso de todos los gestos;
  - la ayuda de RR-3 y su tabla;
  - la `ExpectedEvidence`.
- **Ningún recuento cambia, ni el GREEN previsto.**

| Hallazgo | Grav. | Cambio | Id | Dónde |
|---|---|---|---|---|
| **CA-T3a I-01** `BeamDepth`, gesto inerte | I | **El problema:** `LARGUERO_IN_OUT_C6` solo ofrece el peralte 6, y el ensamblador lo resuelve contra el catálogo. El ancla `ChangedByUser` de CT-PBT3A-09 era un C07 seguro.<br>**El cambio:**<br>• CT-PBT3A-09 (d) y CT-PBT3B-01 (i) editan **`PalletDepth`**, de 48 a **48.5**;<br>• el ancla pasa a `ancla Payload $.Structure.PalletDepth grupo=Rack clase=ChangedByUser estructural=False`;<br>• CT-PBT3B-01 (i) lee los dos valores de vuelta.<br><br>**Comprobado en el código que se conserva:**<br>• `ClonePallet` (`PushBackEditorDesignAssembler.cs:176`, `:573-576`);<br>• la reconstrucción con esa tarima (`:213-218`; `DynamicRackSystemBuilder.cs:92`);<br>• `Snapshot` (`DynamicRackSystemResolver.cs:306`);<br>• el documento (`DynamicRackSystemDocument.cs:146`);<br>• y F1 ya mide el exacto.<br><br>Y que no mueve los cortes laterales: salen de los postes del frente (`DynamicFrontGeometry.Compute`). **§5.0 gana la lección M-13 de CA-T2c,** con la tabla del repaso de todos los gestos de PB-T3: ninguno más es inerte | V44-01 | §5.0; §5.2; §5.3; §6.0 |
| CA-T3a M-02 | M | **Los valores de los gestos se comparan como valor:** se cuentan sus apariciones como token en la clase, en `RedSha` y en `RedRepairSha`. Cambiar la línea del gesto sin cambiar el valor ya no es C07. Las líneas y los fragmentos siguen comparándose como subcadenas exactas | V44-02 | §6.0; §6.1 |
| CA-T3a M-03 | M | **Cada fila de la tabla de RR-3 se enumera sin interpretar.** Las cuatro filas de PB-T3a que pedía:<br>• **CT-02:** sus tres fragmentos y los siete nombres de constantes de `PushBackPilotMaterializer.cs:100-120`;<br>• **CT-08:** las ocho etiquetas de `PushBackFullEditorCharacterizationLateralCutTests.cs:99-119`, con sus obsoletas en `:127-133`;<br>• **CT-12:** los cinco nombres y los dos destinos;<br>• **CT-14:** sus quince términos.<br><br>**Además:**<br>• las filas de PB-T3b y la de CT-PBT3C-01 a 04 también se enumeran; la de CT-PBT3C-05 queda para la emisión de PB-T3c, con su referencia;<br>• **los valores de `PostPeralte` pasan a 3.25 y 4.25,** porque 3 y 4 no se pueden contar como token. `ApplyPostPeralte` no los ajusta | V44-03 | §5.0; §5.2; §6.0 |
| CA-T3a M-01 | M | **La lectura de la verificación con una reparación va entera en la `ExpectedEvidence`**, como en PB-T2c:<br>• las comprobaciones n.º 10 y n.º 11;<br>• lo que el Controller comprueba de RR, con sus órdenes git;<br>• «En PB-T3a, además: … ningún archivo de `src/`»;<br>• la ayuda de RR-3 y los indicios.<br><br>PB-T3b y PB-T3c la heredan con su frase propia | V44-04 | §6.1; §6.2; §6.3 |
| CA-T3a M-04 | M | **Es del generador:** sus notas y las 80 pruebas en negativo, sobre el generador final. El paquete no cambia | V44-05 | — |
| — | — | La versión: `Objective` «v4.4»; la custodia gana la v4.4, `v44-delta.md` y CA-T3a con su encargo | V44-06 | Cabecera; precondición 9 |

## Lo que el generador de PB-T3a tiene que cambiar

1. **La versión:**
   - `PKG_VERSION = 'v4.4'`;
   - `CUSTODY_SHA256` del paquete v4.4: `a96063acfe857625c85c3b23f37d17026e0bc2f6b28105ccc2c00898bfc36c14`, 256 921 bytes, LF;
   - `OBJECTIVE`: «…PB-T3 v4.4…».
2. **La `ExpectedEvidence` de §6.1** pasa de 8 a **9** viñetas, más la línea base. La 8 queda en «la verificación con las tres comprobaciones de F4B-E15, y nc1 a nc3», y la 9 es la lectura entera de la verificación con una reparación, como la `ExpectedEvidence/8` de PB-T2c. Cambian `EVID_PKG`, sus `assert` y la comparación viñeta a viñeta.
3. **La ayuda de RR-3 en el relevo** (M-05 de la §12 de CA-T2b): la tabla del §6.0 distingue ahora «Líneas» (subcadena exacta, por número de apariciones) de «Valores» (token, por número de apariciones). La lista que escribe la sesión lleva las dos clases con sus cuentas en `RedSha`. La comprobación de que la tabla nombra las seis clases de PB-T3a y ninguna otra no cambia.
4. **`bounded_check` y `BOUNDED_RESULT`:**
   - la fila `**CA-T3a I-01**` ya está en el registro (V44-01), así que el error esperado de CA-T3a §9 desaparece;
   - el literal es el que propone CA-T3a: `EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el gesto inerte de BeamDepth)`.
5. **La custodia:**
   - `CUSTODY_EXTRA` gana `v44-delta.md`, `independent-review-pbt3a-v43-check.md` y su encargo, si no estaban;
   - el texto de la custodia del paquete nombra el registro hasta V44-06;
   - los guiones `cat3a_*.py` de la revisión llevan la ruta del checkout principal: se redactan o no se custodian (DU-11).
6. **Las notas:** el §6.0 sigue con 6 y el §6.1 con 3.
7. **M-04:** volver a pasar las 80 pruebas en negativo sobre el generador final, con la v4.4, y poner las notas al día, con los bytes y el SHA-256 del seco nuevo.

## Lo que no se pudo medir

- **Nada se ejecutó con dotnet.** Que `PalletDepth` = 48.5 se conserva y que su ancla sale `presente=True` sale de leer la cadena del código y de la medida de F1, no de una corrida. Si no saliera, la línea lo diría en el GREEN, y sería C07.
- **CT-PBT3B-01 (i): que `Erasures` siga vacía con la tarima cambiada** sale de leer que los cortes laterales salen de los postes del frente (`PushBackSystemLateralBuilder.Cortes` y `DynamicFrontGeometry.Compute`), no del fondo de la tarima.
