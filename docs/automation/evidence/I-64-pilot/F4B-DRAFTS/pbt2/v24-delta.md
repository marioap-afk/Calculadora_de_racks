# I-64 F4B — PB-T2: de la v2.3 a la v2.4 del paquete (para emitir PB-T2b)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **La comprobación acotada de PB-T2b** (`independent-review-pbt2b-v23-check.md`, citada **CA-T2b**): EMITIBLE TRAS CORRECCIONES, B 0 / I 3 / M 7, sin decisiones nuevas para el Owner.
- **El mensaje de la sesión:**
  - aplicar sus tres importantes y sus siete menores;
  - repasar todas las CT de PB-T2b y PB-T2c buscando el mismo riesgo, el que costó el intento 1 de PB-T1c y el de PB-T2a (F4B-E30, F4B-E33):
    - una lectura antes o después de un paso que cambia el estado o la computación;
    - un gesto inerte con los datos de prueba;
    - una línea esperada que depende de datos.
- **La revisión:** HEAD `17872be8`, con PB-T2a en su intento 2 (F4B-E33).

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v24.md`: la v2.3 con el registro v2.3 → v2.4 (V24-01 a V24-16) delante de los anteriores;
- este archivo;
- `make_pbt2b_contract.py` alineado con la v2.4, con su seco y sus pruebas en negativo;
- `measure_pbt2b_t1c.py` con el modo `--final`, y `pbt2b-anchors-out.txt` regenerado (V24-07).

**Resultado:**
- **El diseño no cambia, ni lo que se informa al Owner. No hay decisiones nuevas.** Cambian recetas, el momento de las lecturas, un mínimo y la custodia.
- **Las tres importantes quedan corregidas.** La revisión encontró, además, un defecto de la misma clase que CA-T2b no vio, y que hacía fallar cuatro CT en el GREEN:
  - **La tolerancia de los 17 fixtures de `Fixtures/I64` es 4.** Por eso `ToleranceBox=4` no cambia el estado aunque eleve `LostFocus`: `Keep` (W:3569-3570) conserva un valor que se ve igual.
  - **Con 4, estas líneas no se cumplían:**
    - la pasada 1 de CT-03: «Aplicar» daba «sin cambios», `llamadas=0`;
    - el `aplicar` de CT-04;
    - `cerrar-con-cambios-no` de CT-06, aun con `SetNumberR`;
    - y la cuarta ventana de CT-05 no llegaba al puerto, aunque su línea sí se cumplía.
  - **Las ediciones escriben ahora 4.5** (V24-11).

## 1. Lo que cambia en cada CT

| CT | Qué cambia | Id |
|---|---|---|
| **CT-PBT2A-02** (§5.2) | **`launchfor`:** `estado-es-el-actual` e `id` se leen antes de aplicar por `desdeBorrador.Port`, y `puerto-es-del-borrador`, después. Es C-1 de F4B-E33, y la línea esperada no cambia | V24-10 (M-07) |
| **CT-PBT2B-01** | **La sonda va por ventana.** En (ii) es el primer campo de la línea, y (iii) va en la misma ventana que (ii). Las dos ventanas de los constructores públicos registran su `Closed`, para no cerrarlas dos veces | V24-09 (M-06), V24-15 |
| **CT-PBT2B-02** | **`seleccion=`:** lo esperado sale del estado de entrada, con `Sel(entrada)`. El lado es literal (`Both`, `B` y `A`; WC:154-156) y los índices son los del lado activo de la entrada.<br>**`nombre=`:** lo esperado es `Nombre(draft.CurrentDraftState.Name)`, del estado de entrada.<br>**`estado-invalido`:** el orden de sus lecturas; `tras-restaurar` se lee justo tras «Restaurar» y antes de «Aplicar», que pone su propio estado | V24-03 (I-03), V24-12 |
| **CT-PBT2B-03** | **Pasada 1:** la edición es `SetNumberR(ToleranceBox, 4.5)`, y su línea, `ToleranceBox=4.5`. `ArchivoTemporal` se borra antes de cada guardado, así que el `archivo=True` del segundo lo da ese clic | V24-08 (M-05), V24-11, V24-13 |
| **CT-PBT2B-04** | **La primera edición es 4.5;** la segunda sigue en 5. Cada línea se lee justo tras su «Aplicar».<br>**`sin-cambio`** = `ReferenceEquals(draft.CurrentDraftState, actual) && Igual(draft.CurrentDraftState, c)`, con `actual` y `c` tomados justo antes de ese «Aplicar».<br>**`archivo`,** por guardado (como en CT-03) | V24-11, V24-13 |
| **CT-PBT2B-05** | **La cuarta ventana:** `SetNumberR(ToleranceBox, 4.5)`, y su línea es `cuarta-ventana llamadas=1 huellas-iguales=True descartes=0`.<br>**Las huellas:** las toma un solo `Vigia` de CT-PBT1C-02, que captura tras la carga y tras cada paso las referencias de D2 de la ventana, de `draft.CurrentDraftState` y de `draft.InitialDraftState` | V24-06 (M-03), V24-11 |
| **CT-PBT2B-06** | **La edición es `SetNumberR(ToleranceBox, 4.5)`,** en `cerrar-con-cambios-no`, en `cerrar-tras-aplicar` y en `clasico-con-cambios`.<br>**La receta:** cuatro ventanas de WORKSPACE sobre `pb-01-base` y una CLASSIC, con el orden de sus líneas.<br>**El `finally`** pone «Sí» y llama a `Cerrar(w)` | V24-01 (I-01), V24-04 (M-01), V24-11 |
| **CT-PBT2B-07** | **`bom-es-el-calculado`:** la costura lo compara en el acto, y la línea se arma justo tras «BOM» y antes de «Guardar en biblioteca», que recalcula (W:3368) y sustituye la computación (W:607).<br>**El fixture** es `pb-01-base`, `nombre=PB-01` es literal y `archivo` es el del §5.0 | V24-02 (I-02) |
| **CT-PBT2B-08 a 10** | Nada: son guardas de texto y validaciones puras, sin momento de lectura | — |
| **CT-PBT2C-01** | **`aplicado`** es el del §5.0: el borrador cambió de objeto.<br>**El gesto neutro** (`DrawRackNameCheck.IsChecked`) no recalcula solo, porque la casilla escucha `Click`. Lo lee el recálculo de «Aplicar» (`ReadInputs`, W:760-782), así que no es inerte | V24-14 |
| **CT-PBT2C-02** | Nada: copia los gestos y las líneas de CT-PBTC-07, que no dependen del modo | — |
| **CT-PBT2C-03** | **Las filas** son los 49 gestos de `Gestos()` con `Compara`, sin los cuatro de D2, que dejan el último diseño válido detrás o cierran la ventana.<br>**Lo esperado de cada fila** sigue tres casos, que fija la remedición:<br>- «Aplicar» aplica;<br>- no hay nada que aplicar (la selección, el lado frontal o `ToleranceBox=4`);<br>- «Aplicar» se niega con el estado cambiado, por un bloqueo. En ese caso, `igual-a-la-ventana=False`, `diseno-igual=False` y `descartes=1`.<br>La v2.1 ponía `descartes=0` en todas | V24-14 |

**Lo común, en el §5.0 y fuera de las CT:**
- **dos reglas normativas nuevas:**
  - **el momento de cada lectura:** cada línea, justo tras su último gesto; `draft.CurrentDraftState` y `w.LastComputation`, en el momento que dice su CT; y `aplicado`;
  - **los gestos:** `SetNumberR`, un valor que se ve igual no cambia nada y la tolerancia de los fixtures;
- **dos ayudantes:** `Nombre(x)` y `Sel(e)`;
- **`ArchivoTemporal`:** `archivo` = `File.Exists`, y se borra antes de cada guardado;
- **R-PBT2-22** (§7);
- **D-PBT2-07 y L-PBT2-10**, que dicen el gesto de la red;
- **`DialogWindowCharacterizationTests`:** `MinSelected` 20 (§5.1 y §6.2; V24-05, M-02);
- **la custodia** (§6.0; V24-07, M-04).

## 2. Lo que se comprobó y no cambia

Todo se leyó en `17872be8` por objetos git, y el generador lo comprueba:
- **W, WC y WX** son los mismos blobs que en `161a0dad` y `0dc623ca` (`f8756a88`, `1a3300b4` y `fb094bef`). Las 92 líneas de la v2.3 siguen; la v2.4 añade 8 líneas ancla y 6 citas nuevas: W:607, W:1135, W:3350, W:3368, W:3569-3570 y WC:154-156.
- **El código en que se apoyan las recetas nuevas:**
  - `ToleranceBox` recalcula en `LostFocus` (WX:103);
  - `Bom_Click` y `SaveLibrary_Click` empiezan por `RequestRecompute()`;
  - `lastComputation = computation;` está en el ramal válido de `Recompute`;
  - `SideSelector_Changed` hace 2 → `Both` y 1 → `B`;
  - `SetNumberR` eleva `LostFocus`;
  - `CloseQuietly` solo cierra con `IsVisible`;
  - `DraftSession.InitialDraftState` devuelve una copia;
  - `SetCurrentDraftState` guarda una copia nueva.
- **Los datos:**
  - los 17 fixtures tienen tolerancia 4;
  - `pb-01-base` se llama `PB-01`;
  - `Gestos()` de CT-PBT1C-02 tiene 49 gestos con `Compara` y 4 de D2;
  - F4B-E33 y su análisis (C-1) están en la revisión.
- **Lo que no tenía el riesgo:**
  - CT-PBT2B-03, pasadas 2 y 3: sus gestos son los de sus originales, y cada línea se lee tras su gesto;
  - CT-PBT2C-02: copia CT-PBTC-07;
  - el paso 1 y la regla del sufijo `descartes`.
- **La validez de 4.5 y de 5 en `pb-20-ab`:** se razona, no se midió. La tolerancia solo entra en la geometría transversal y en la comprobación de que es positiva (`DynamicRackSystemResolver`), y ningún diagnóstico bloqueante la mira (`PushBackCompositeDiagnostics`). La v2.3 ya suponía lo mismo de 5.

## 3. El texto que compara el generador y cambia (CA-T2b §6, punto 6)

Solo cambian dos textos:
- la fila de la tabla de RequiredTests del §6.2 con `DialogWindowCharacterizationTests`, de 23 a 20 (V24-05);
- la frase de custodia del §6.0, que gana la v2.4 y `v24-delta.md` (V24-07).

Los invariantes, las paradas, el Objective, la costura y las clases del §5.3, la ExpectedEvidence y la línea base no cambian. El generador comprueba además las huellas de las recetas nuevas, y el registro V24 con una fila por hallazgo de CA-T2b.

## 4. Lo que queda para la sesión

- **El encargo de CA-T2b** (M-04): hay que custodiarlo con su texto literal en `independent-review-pbt2b-v23-check-prompt.md`. No lo tengo, así que no lo he escrito: lo escribe la sesión.
- **`BOUNDED_RESULT`:** está puesto con el literal que propone CA-T2b para cuando se corrijan I-01 a I-03: `EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete)`. El generador lo contrasta con el veredicto del informe custodiado y con el registro V24 (`bounded_check`).
- **La remedición del §6 de CA-T2b,** al escribir, sobre el cierre de PB-T2a. El generador ya imprime sus siete pasos y comprueba que el cierre es posterior a F4B-E33.

## 5. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal (`17872be8` y sus antecesores), sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura (`_work/c/make_v24.py`, `_work/c/fxdump.py`, `_work/c/negcheck.py`).
- **Ni dotnet ni git de escritura.**
- **Incidencias:** ninguna en esta tanda (ver las notas del generador, §6).
