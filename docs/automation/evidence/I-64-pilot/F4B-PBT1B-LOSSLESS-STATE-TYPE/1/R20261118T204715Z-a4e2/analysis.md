# I-64 F4B-PBT1B-LOSSLESS-STATE-TYPE: análisis de la corrección del intento 2

Lo redacta la sesión principal, en el papel del Coordinator (AUTOMATION_PLAN §16.6), el 2026-11-18. La delegación de corrección lo cita en `CorrectionOf`.

## 1. Qué se corrige y por qué

**El intento 1:** delegación `R20261118T204434Z-6063` y Worker `R20261118T204715Z-a4e2`, con el contrato de la v2.7 y BaseSha `4b0498e3`. Entregó:
- RED `2d9febc6087280e0ba9f7a6c72e77b283506891d`: el esqueleto del §5.3 y las seis clases de prueba;
- GREEN `a0a5f699b138c1b5251fb0c88d28e3ca9e1610c3`: solo `src/` de Application;
- CI del GREEN `44405531606`, 4/4 con las cifras previstas (Core 19 086; UI 2 437).

**La revisión independiente** (`F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/independent-review.md`) dio **REWORK 0/2/15**. El producto está bien y D2 se respeta; los dos importantes son defectos de **pruebas** que venían del borrador heredado de `fb01`.

**El Owner** (F4B-E22, `F4B-DRAFTS/coordinator/owner-pbt1b-rework-decisions.md`) decidió:
- «Corregir en el intento 2»;
- ratificar, solo en PB-T1b, M10 (los ayudantes privados fuera de la lista de INV-PBT1B-03) y M14 (`build-plugin` omitido en la CI del RED).

**FailureClass:** `Tests`. Las pruebas no prueban todo lo que el contrato les pide; el producto no se toca.

## 2. Las correcciones (vinculantes para el Worker del intento 2)

Todas van dentro de las seis clases de prueba del AllowedWriteScope.

| Id | Archivo | Qué cambia | Por qué |
|---|---|---|---|
| **I01** | `PushBackSharedReferenceGuardTests.cs` (CT-14) | `Violations` aplica a los alias de la regla 2 de D-PBT1-15 también la **regla 1b** (`Rule1b(member, aliases.Keys)`), no solo la 1. Se añade una autoprueba en negativo «alias + molde» que tiene que fallar, por ejemplo `var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add("x");`, y su línea `autoprueba-<x>=falla` | El paquete (PKG:692-696) dice que un alias cuenta como ficha para las reglas 1 y 1b. Sin la 1b, una mutación futura con un molde sobre un alias pasaría sin verse |
| **I02** | `PushBackEditableStateCopyTests.cs` (CT-10, caso N-M-02) | `fresh` se carga con el **mismo nombre** que `anterior` (el del sobre), para que la única diferencia sea la sesión | Con nombres distintos, `CarriersEqual` compara `Name` y el caso sale «distintos» siempre: no puede fallar |
| **M01** | `PushBackEditableStateCopyTests.cs` (CT-16) | Un caso más en «comparar»: dos líneas base **distintas** e iguales en la proyección; se muta `Name` en una y se comprueba que `Equals` no cambia | Hoy «comparar-invariante» no puede fallar, y la condición 7 (ii) solo se sostiene por lectura |
| **M02** | `PushBackEditableStateCopyTests.cs` (CT-16) | El bloque «editar» muta **todos** los portadores de D-PBT1-02 (los que faltan: `PostPeralte`, `PalletTolerance`, `BeamDepth`, `FirstLevelDatum`, las casillas, `Dimensions`, `DimensionStyle`, `LoadedDimensionViews`, `DimensionViewsTouched`, las tres casillas de cotas y los avanzados) | PKG:918 dice «y cada portador» |
| **M04** | `PushBackEditableStateCopyTests.cs` (CT-06) | Un segundo estado rico, `pb-20-ab` con B retirado, y la comprobación de que `undo()` deshace cada mutación, como el piloto (`WorkspaceDraftPilotStateTests.cs:1001-1002`) | Es lo que pide el texto de CT-06 (2) |
| **M06** | `PushBackEditableStateCopyTests.cs` (CT-07, parte 3) | Medir desde el contenido, la cola, la sesión y el commit, sin restar «todo lo que alcanza la línea base», y ejercer sesión y commit no nulos | Hoy, un alias que alcanzara la línea base quedaría oculto |
| **M07** | `PushBackSharedReferenceGuardTests.cs` (CT-15) | El censo registra la **sentencia completa** de cada asignación, como el paquete (PKG:646-647), y el orden exige que el escritor esté **dentro** del método `DrawPushBackView` | Hoy un cambio del lado derecho de la asignación no se vería |
| **M08** | `PushBackEditableStateParityTests.cs` (CT-01) | «Restaurar tras `SetGap(60)`» en los 19 diseños del paquete (PKG:903). «Retirar B» puede seguir solo donde hay B, **declarado** en una línea | Es una reducción no declarada |
| **M13** | `PushBackEditableStateCopyTests.cs`, `PushBackEditableStateLoadTests.cs` y `PushBackEditorOutputGateTests.cs` | Toda llamada al API nuevo pasa por `Paso` (la regla «EXCEPCION», PKG:829). CT-05 distingue el rechazo del almacén del rechazo de la carga del estado | La regla se aplica hoy a medias |

**Las líneas esperadas que cambian por estas correcciones** se miden en la base de la corrección (con la implementación del intento 1) en una corrida temporal, antes del RED, y se transcriben. No se ajustan después.

## 3. Lo que NO se corrige (declarado, o arrastres)

- **Producto** (el Owner no pide tocarlo):
  - M09: `Of` decide «declarada» por la tabla y no por el tipo, aunque hoy es equivalente;
  - M11: `LoadDesignEditor` y `RestoreEditor` con un diseño nulo mutan antes de lanzar, y pasa como arrastre a PB-T2 y PB-T4;
  - M12: `SameAs` con `NaN` mantiene la semántica que ya existía;
  - M15: el coste de `Equals` pasa como arrastre al `IsDirty` de PB-T4.
- **M10 y M14:** ratificados por el Owner (F4B-E22), solo en PB-T1b.
- **M03:** el límite de «desde su adopción» en CT-16 se declara como límite.
- **M05:** no es urgente.

## 4. Cómo se hace (§6.0, M-11: una corrección que toca pruebas exige RED otra vez)

- **El RED:** sobre la punta del intento 1 (con la implementación), el commit RED **vuelve a poner el esqueleto exacto** del §5.3 en los seis archivos de `src/` (`git restore --source=2d9febc6 -- <los seis>`) y lleva las pruebas corregidas. Los filtros con `ExpectRed` fallan por aserción, y las guardas de D2 pasan.
- **El GREEN:** restaura la implementación del intento 1 (`git restore --source=a0a5f699 -- <los seis de src/>`) **sin ningún cambio de producto**, y no toca `tests/`.
- **Las cifras previstas** del GREEN: las del intento 1, más las líneas nuevas que no cambian el número de métodos. La autoprueba de I01 es una línea de CT-14, no un método nuevo.
