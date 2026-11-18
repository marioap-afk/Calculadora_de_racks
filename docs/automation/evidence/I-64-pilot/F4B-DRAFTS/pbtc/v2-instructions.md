# Encargo: v2 del paquete del contrato de PB-TC, tras la revisión independiente v1

Lo da la sesión principal al redactor de la v1. La revisión independiente dio **NO EMITIBLE 1/4/10** (`pbtc/independent-review-v1.md`). La sesión comprobó en el código B-01 (`DA:165-168`, `DynamicLoadBeamGeometry.cs:52-71`, `secciones.csv:8`, `DynamicRackSystemResolver.cs:311`), I-01 (W:1865), I-02 (`DEA:44-60`, `DA:195-216`), I-03 (WC:168-174, W:1566-1626, W:1865-1871), I-04 (NF:138-142) y M-02 (WX:678-679), y los da por buenos.

## Reglas

Las mismas de la v1:
- solo lectura en el worktree (HEAD `f9f2afbb`);
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y no con un heredoc;
- sin rutas locales ni nombres de usuario o de máquina en lo que se custodiará.

**Salidas, en `pbtc/`:**
- `I-64-f4b-pbtc-gate-contract-package-v2.md`: la v2 completa. **No sobrescribas la v1.**
- `pbtc-measurements.md`: añade un apartado K con lo que midas para la v2. No reescribas los apartados anteriores; si alguno queda falso, anótalo en K.

La v2 empieza, tras la cabecera, con un **registro de cambios v1 → v2**: una tabla con hallazgo, qué cambió y dónde (`§`/CT/INV), y una fila por cada hallazgo de la revisión.

## Lo que decide la sesión para la v2

1. **B-01 (F4, `BeamDepth`):**
   - `Keep` se aplica también a `BeamDepthBox`, por uniformidad; es inocuo.
   - La fila F4 de CT-PBTC-01 y CT-PBTC-02 (i) fija la salida real: `salida=6`, con una clase nueva `CATALOGO` definida en el §4.1 (la salida es `ResolveBeamDepth(exacto)` del catálogo entregado).
   - Corrige el §2, con un hecho nuevo y H-02, además del §5.1, el §5.2 (S-03 falla en 10 filas sin su `Skip`, y en F4 sigue pasando), INV-PBTC-03, el registro (§5.4) y OD-PBTC-1: **diez** campos, y el peralte IN/OUT base es siempre el del catálogo.
2. **I-01:** `Val` se conserva para W:1865, que PB-TC no toca salvo por el anexo A. Borra el paso 5 de D-PBTC-01.
3. **I-02:** un CT nuevo, **CT-PBTC-10**, en el filtro con `ExpectRed`: la paridad ventana–piloto con `Cargar` en:
   - cada variante F1 a F11 de `pb-01-base` (+0,00044);
   - una variante de un diseño **con módulos personalizados** del corpus, con `Structure.PalletDepth` + 0,00044. Mide cuál de los diseños del corpus tiene módulos personalizados de verdad y elígelo con su motivo.

   **Lo esperado en el GREEN:** `admitida=True delta=[]`. Si para F4 el piloto y la ventana difieren solo por el catálogo, mídelo y fíjalo.

   **En la base:** razona qué delta saldría (la reconstrucción por `MustRebuild`) y si falla por aserción. Ese delta de la base es el efecto que hoy hace RACKEDITAR: OD-PBTC-1 lo presenta como **no medido**, y la cadena lo medirá en el RED. Añade una obligación en `ExpectedEvidence`: la entrega transcribe el delta de la base de CT-PBTC-10, leído del mensaje del TRX del RED. Añade también una parada: si ese delta de la base muestra un cambio de piezas, de niveles o de BOM en el diseño con módulos, la sesión lo lleva al Owner antes del GREEN (OB-06.4). Haz que el método sea **un** `Fact` con un solo `Assert.Equal` final, como los demás.
4. **I-03:** no entra en la v2 base. Se especifica completo como **anexo A** condicional: `GapBox`, `ModuleLengthBox` y las seis cajas avanzadas con la misma regla `Keep`, en su variante opcional para las anulables.

   El anexo A incluye:
   - el diseño por línea;
   - sus CT (desde CT-PBTC-11), con sus líneas esperadas y su fallo en la base;
   - el efecto del alcance;
   - las sustituciones, si alguna prueba existente choca. Mídelo con `git grep`;
   - las cifras del §6.9 con el anexo.

   Si el Owner lo elige, se incorpora tal cual en una v2.1. Va al Owner como **OD-PBTC-5**:
   - (A) incluirlo en PB-TC;
   - (B) llevarlo al Owner antes del contrato de PB-T1, por OB-06.5.

   Recomendación de la sesión: **(A)**, porque es la misma clase, la misma regla y los mismos tres archivos, y es la razón de [P]:509. Escribe su coste con honestidad.
5. **H-PB-28 (OD-PBTC-4):** especifícalo igual, como **anexo B** condicional. Incluye el arreglo por línea (lo que hace «Restaurar parámetros», W:1628-1634, tras la pasada válida que consume «Restaurar estándar»), su CT, la sustitución S-04 de CT-PB0-05 con su literal y sus aserciones A1/A2 viejas y nuevas, las rutas añadidas al `AllowedWriteScope`, sus `RequiredTests` y las cifras. Las opciones de OD-PBTC-4 se quedan como están, con (A) = incorporar el anexo B en la v2.1.
6. **I-04:** el paso 3 de CT-PBTC-03 escribe un texto distinto que se ve igual, `"61.2350"`, asignado a `Text` (dispara `TextChanged`), y eleva `LostFocus`. Lo esperado es `c00=61.23456`. Comprueba en NF que `"61.2350"` valida y da 61,235.
7. **M-01:**
   - en OD-PBTC-1 y OD-PBTC-2, las frases sin medir se escriben como no medidas: R11 «100,00044», «< 0,02» para R7 en la planta;
   - OD-PBTC-2 añade que RACKEDITAR deja de borrar el estilo del sobre, y el efecto en la biblioteca y en «Insertar»;
   - OD-PBTC-1 declara de forma explícita la extensión más allá de la «ruta de carga» de OB-06.6 (selección, «Ambos lados», alcance, datos del frente y biblioteca);
   - OD-PBTC-1 añade, como **sub-decisión R-03**:
     - (i) la regla de Cantilever: escribir algo que se ve igual conserva el exacto. Recomendada;
     - (ii) cualquier edición del texto manda. Indica cómo se implementaría (marca por caja puesta por `TextChanged` fuera de la siembra; revisa en W bajo qué `suppressSync` se siembra), su coste y su CT.
8. **M-02 (el lugar del aviso), en OD-PBTC-3, dos lugares:**
   - **L1:** debajo de la casilla del lado B. Es el de la v1 y contradice la regla del shell «no second status inside the sidebar» (WX:678-679; `PushBackShellAdoptionTests.cs:15-20`).
   - **L2:** un `TextBlock` nuevo dentro de `StatusContent`, después de `StatusText` (WX:682-685). Respeta la regla, siempre está visible fuera del desplazamiento y `SetStatus` no lo sobrescribe. Comprueba que ninguna guarda fija el número de hijos de esa banda. Usa el `FontSize` y el pincel de `StatusText` para no introducir un literal nuevo.

   La recomendación de la sesión es **L2**. Diseña la v2 con L2, y deja L1 descrito como la alternativa con sus diferencias para los CT. Matiza «vale en los dos modos»: en WORKSPACE no está demostrado y se comprueba en PB-T2.
9. **M-03:** `ForbiddenWriteScope` con el método del contrato de PB-T0, hermano por hermano: los hermanos de `src/RackCad.UI/Systems/PushBack/`, los demás sistemas y carpetas de `src/RackCad.UI/`, las guardas de UI que leen la ventana y los hermanos de `tests/RackCad.UI.Tests/Workspace/`. El script generador comprobará que existen. Da el número final.
10. **M-04:** **retira CT-PBTC-09** y su archivo. La prueba de OB-06.6 la dan INV-PBTC-01 (el diff vacío en `Controls/` y `Cantilever/`) y el filtro de Cantilever en `RequiredTests`. Deja el hueco del número (no renumeres) con la nota «retirado en la v2 (M-04)».
11. **M-05:** corrige H-08 con `MigrateFirstLevelDatum` (`PushBackEditorState.Load.cs:91-124`) y la frase del §8 sobre los racks con tres decimales o menos. Un rack antiguo sin marcador de datum puede llegar a la caja con más de tres decimales tras la migración: dilo con su efecto (T-3 R6: el troquel lo absorbe).
12. **M-06:** corrige las anclas (H-12, que son lecturas; H-18, la guarda de foco saltada) y completa H-18 con las cuatro guardas que revisó el revisor.
13. **M-07:**
    - en el §4.3, filas para los pasos 3 y 6;
    - un punto informativo en el §8: el paso 4 en el Workspace y el aviso en el modo WORKSPACE se prueban en PB-T2 ([P] lo asigna; [OD]:144-147 pide los dos);
    - la sesión lo anotará en `f4b-dispositions.md` como obligación de PB-T2 en la emisión. Añádelo a las precondiciones.
14. **M-08:** define en el §4.1 cada formato: `vistas` = `InsertionRequest.Views.Count`, como en `StateTests.cs:191`; `AnnotationScaleBox=` = el **texto** de la caja; el separador de `estilos`; y el literal del aviso, en un bloque de código sin comillas exteriores.
15. **M-09:** las corridas temporales del §5.2 guardan su TRX y su log como artefactos de la entrega, y se citan en `worker-handoff.json`.
16. **M-10:** reformula INV-PBTC-03 e INV-PBTC-15 como comprobaciones mecánicas del diff, o como CT que los fije.

## Además

- **Recalcula** el §6.4 y el §6.9 con CT-PBTC-09 retirado y CT-PBTC-10 añadido (y aparte, con los anexos A y B). Revisa los `MinSelected`.
- **Un §8.0 «Resumen para el Owner»** en español llano, sin rutas ni identificadores internos salvo los nombres de las decisiones. Para cada decisión OD-PBTC-1 a OD-PBTC-5 y el punto informativo: qué cambia para quien usa el programa, lo que está medido y lo que no, las opciones y la recomendada. Máximo unas 60 líneas.
- **Mantén** todo lo de la v1 que la revisión dio por bueno (CT-PBTC-04 a 08, el recuento de Cantilever, las transcripciones de PD-F4-01 y PD-F4-04, las paradas, las celdas y la línea base).

**Tu respuesta final** es corta: las rutas de los archivos, qué no pudiste medir y cualquier hecho que contradiga una de estas decisiones de la sesión. Si una decisión de este encargo resulta inviable por el código, no la fuerces: dilo y propón la alternativa.
