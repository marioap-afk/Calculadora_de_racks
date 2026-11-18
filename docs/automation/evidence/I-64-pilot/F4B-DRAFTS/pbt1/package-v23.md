> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`, `<datos-locales-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T1 (el estado editable de Push Back sin pérdidas fuera de la ventana), v2.3

**Estado:** v2.3, el texto de emisión de PB-T1a. **PB-T1a se emite por F4B-E09 (la autorización condicional, pregunta 4) y F4B-E11 (D2 y «PB-T1 PLAN»), tras la remedición sobre el producto final de PB-TC2 y la comprobación acotada.** No declara GATE PASS.
- **La remedición** (§8.4; registro N) se hizo sobre `cd47523b`, la custodia del cierre de PB-TC2 (F4B-E12), con el producto de su GREEN `b5c09f0b`. **No cambia ninguna premisa material** (L-6): todas las diferencias son anclas, recuentos o textos, y la tabla del §8.4 clasifica cada una.
- **El generador del contrato de PB-T1a** es `make_pbt1a_contract.py`, junto al registro. Su `--dry` pasa: 9 rutas permitidas, 131 prohibidas, 14 invariantes, 30 paradas, 19 filtros y 38 autoridades (registro N-14).
- **La revisión independiente** IRT1-1 de la v1 (`F4B-DRAFTS/pbt1/independent-review-v1.md`) dio **EMITIBLE TRAS CORRECCIONES 0/8/15**, con **una decisión nueva del Owner, L-1**.
- **La comprobación acotada de la v2** (`F4B-DRAFTS/pbt1/independent-review-v2-check.md`) dio **EMITIBLE TRAS CORRECCIONES 0/1/10**. La v2.1 aplicó sus diez menores, y la sesión corrigió el importante.
- **L-1 está decidida** (F4B-E11, D2, `F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`): **(a), la excepción declarada**, con **diez condiciones de seguridad** y **un límite**. La v2.2 las incorporó (D-PBT1-15); la v2.3 cierra lo que quedaba «A MEDIR».
- **El Owner aceptó en principio la división en tres subtareas** (F4B-E11, «PB-T1 PLAN»), con una condición: «Re-measure it against the final PB-TC2 product before emission». Esta es esa remedición.

**Lo que pide el Owner** ([OD], fila de PB-T1 del §5 de [P]):

> ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.

**Lo que propone este paquete:**
- **Tres subtareas secuenciales**, cada una con su cadena y su presupuesto de 1/3 (OB-10.1):
  - **PB-T1a — la red** (`F4B-PBT1A-LOSSLESS-STATE-NET`): solo pruebas de caracterización, con RED por mutantes. El producto no cambia.
  - **PB-T1b — el tipo** (`F4B-PBT1B-LOSSLESS-STATE-TYPE`): el estado editable en Application. La ventana no cambia.
  - **PB-T1c — la ventana sobre el estado** (`F4B-PBT1C-LOSSLESS-STATE-WINDOW`): la ventana guarda su estado en ese tipo, en modo CLASSIC y sin cambio observable.
- **Ninguna corrección de producto.** RACKEDITAR no cambia (OB-RE.1). c-02 se reproduce como regla explícita (F4B-E09, pregunta 2).
- **L-1, decidida: (a), la excepción declarada** (F4B-E11, D2): la copia comparte por referencia **solo** el rack resuelto y el informe de módulos de cada lado, con las diez condiciones de seguridad del Owner probadas o arrastradas una a una (D-PBT1-15) y su límite como parada en cada contrato. El anexo L1-C queda **no elegido**.

**Condiciones para emitir** (§8.4):
- el cierre de PB-TC2 (D1 del Owner: seguir con la corrección): **hecho**, F4B-E12;
- la remedición sobre su GREEN: **hecha**, sin ningún cambio de una premisa material;
- la comprobación acotada de la remedición, que pide la sesión (F4B-E12, «Siguiente»);
- y la emisión de PB-T1a «normally» (F4B-E11).

**Base de la medición:**
- **La v1 y la v2** midieron la rama `architecture/workspace-persistente-rackcad` en `eedb069c`, cuyo producto es el GREEN de PB-TC, `4440f67d` (medición A-4).
- **La v2.3 lo vuelve a medir** en `cd47523b`, la custodia del cierre de PB-TC2, con el producto de su GREEN `b5c09f0b` (`git diff b5c09f0b cd47523b -- src tests` vacío; registro N). `main` sigue en `812baf87`.
- **La `BaseSha` de PB-T1a** es su commit de emisión, sobre `cd47523b`, con `src` y `tests` iguales a `b5c09f0b`.
- **Las zonas que tocó PB-TC2,** marcadas **[TC2]** (`Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, `ReadCellValues`, `OptionalVal`, la retirada de `Val` y los miembros nuevos), están medidas sobre `b5c09f0b` (§2.8; registro N-1 y N-2).
- Todo hecho va anclado por **nombre de miembro y línea**. Las anclas de W anteriores a W:3631 no se movieron. El detalle y las órdenes están en `pbt1-measurements.md`: apartados A a L de la v1, **M** de la v2 y **N** de la remedición.

**Precondiciones de emisión** (de cada subtarea, salvo que se diga otra cosa):
1. **PB-TC2 cerrada** con el diseño de su paquete v2 y la decisión D1 del Owner («CONTINUE WITH THE FIX», F4B-E11). **Hecha:** CLOSED / PASS WITH LIMITATIONS sobre `b5c09f0b` (F4B-E12).
2. **L-1, decidida** (F4B-E11, D2): (a), con sus diez condiciones (D-PBT1-15). **Hecha.**
3. **La remedición sobre el GREEN de PB-TC2** (§8.4, R-1 a R-13): **hecha**, sin ningún cambio de una premisa material (registro N). **Su comprobación acotada** (§6.0) la pide la sesión antes de emitir PB-T1a. La comprobación acotada de la v2 ya está hecha (0/1/10).
4. **Los anclajes de la subtarea**, medidos sobre su `BaseSha` (§6.0).
5. **Para PB-T1b: la autoridad de PB-T1a** (I-08). Ninguna línea fijada por PB-T1a muestra una pérdida (clase c, d o una nueva) ni un control que decida el estado, distinta de las declaradas (c-01, c-02 y M-07). Si alguna lo muestra, se vuelve al Owner antes de emitir PB-T1b (OB-06.5).
6. Las celdas del §6.0 no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.
7. **La custodia** (§6.0, «Custodia»).

## Registro de cambios v2.2 → v2.3 (la remedición sobre el producto final de PB-TC2)

Cada fila es una diferencia que encontró la remedición (registro N), con su clase según L-6 (§8.4). **Ninguna es material.**

| Id | Diferencia | Clase | Qué cambió | Dónde |
|---|---|---|---|---|
| V23-01 | Identidad: la rama está en `cd47523b`; el producto es `b5c09f0b`; W tiene 3700 líneas (+10); WC y WX, sin cambio | no material (anclas) | La base de la medición, la `BaseSha` de PB-T1a (su commit de emisión sobre `cd47523b`) y el §2.0 | Base; §2.0; §6.0 |
| V23-02 | W: sale `Val`; entran `KeepOptional`, `MixedBoth` y `KeepCell`; se mueven `OptionalVal`, `ShownFormat`, `SameShown`, `Keep` e `IntVal`. Ningún miembro anterior a W:3631 cambia de rango | no material (anclas) | El §2.8 pasa de previsión a medición | §2.8 |
| V23-03 | MU-T1A-07a, de W:3686 a **W:3684**; MU-T1A-10, de W:3668 a **W:3666**. Texto idéntico y único; el objetivo de los dos se conserva | no material (anclas) | La tabla de mutantes | §5.2 |
| V23-04 | Los otros trece mutantes, en su línea y con su texto. MU-T1A-13 tiene el mismo texto también en W:296, como en `eedb069c`; se ancla por línea | no material | Una nota en la tabla | §5.2 |
| V23-05 | `RackHeaderHeightBox` ya no redondea (W:1639, `KeepOptional`); el mapa de controles no cambia y WX tampoco | no material (confirma D1 y HT1-15) | HT1-15 y §8.5 lo dan por medido | §2.4; §8.5 |
| V23-06 | La CI de `cd47523b` (`44313012995`): UI 2 422 / 21 `Skipped` / 2 401 `Passed`; Core 19 070. Es la previsión exacta | no material (recuentos) | La línea base de la CI, medida | §6.1 |
| V23-07 | `PushBack*` de UI: 542 (538 + 4 de PB-TC2); `PushBackDeclaredCorrection`: 14; `PushBackRichPreviewTests`: 9. Lo demás, igual | no material (recuentos) | §5.1 y el `MinSelected` de `PushBack` en PB-T1c (542 + 14 + 1) | §5.1; §6.3 |
| V23-08 | Los alcances: `src/` con 59, 117 y 59, igual que antes. En PB-T1a, `tests/` = 55 entradas (con las dos clases de PB-TC2) y la raíz 17: **131** | no material (recuentos) | El total del `ForbiddenWriteScope` de PB-T1a | §6.1 |
| V23-09 | CT-PBT1A-11: `pb-05-topes` no tiene valores con más precisión de la que se muestra, y CT-11 no pasa por cajas avanzadas. D1 no cambia sus líneas del BOM | no material | R-8 cerrado | §8.4 |
| V23-10 | **G-L1, hoy: 0 coincidencias** en 47 archivos de Application y en W y WC. La premisa de D2 se sostiene | no material (confirma D2) | Se quita el «A MEDIR» | D-PBT1-15 |
| V23-11 | **El informe de módulos** expone cinco `IReadOnlyList<string>` (`Preserved`, `Adapted`, `Removed`, `Incompatible` y `Restored`) con `List<string>` detrás: una mutación compilable necesita un molde, que la regla 1 no ve. La autoprueba (c) de la v2.2 no compila | no material (texto de la guarda; R-11 lo dejó «A MEDIR») | G-L1 gana la **regla 1b** (un molde a una colección mutable de una expresión con ficha) y la regla 2 cuenta también la variable de un `foreach`. Las autopruebas: (c) en forma compilable con molde, y una nueva, (e), con el `foreach`. Medidas: cada una salta y las reglas dan 0 en el código de hoy | D-PBT1-15; CT-PBT1B-14; CT-PBT1C-04 |
| V23-12 | **La huella de CT-PBT1B-16:** `Persist(resolver.Snapshot(<rack>))` no ve lo derivado ni `Composite` (el lado B) | no material (datos del CT; R-11 lo previó) | La huella pasa a ser **el recorrido profundo de valores** (el `AllObjects` ampliado de CT-PBT1B-07, con cada campo y su valor), más `Name`. Ningún tipo de esas carpetas de Domain tiene caché perezosa | CT-PBT1B-16; CT-PBT1C-02; D-PBT1-15 |
| V23-13 | **El censo de `Name` del Plugin:** la regex de la v2.2 sobre todo el Plugin da 10 líneas (siete de otros sistemas), así que CT-PBT1B-15 habría fallado en el RED | no material (texto de la guarda; R-12 lo dejó «A MEDIR») | El censo pasa a la regex amplia sobre los archivos del Plugin que contienen `PushBack` (14): 7 líneas literales, tres de Push Back. **No hay cuarto escritor.** Las anclas del orden, medidas (`:48` → `:57` → `:98`; `:170` → `:190`; menú `:34` → `:135`) | D-PBT1-15; CT-PBT1B-15 |
| V23-14 | El informe tiene un consumidor más de lectura en la ventana, W:2559 (`Describe()`) | no material | Se añade a la condición 1 | D-PBT1-15 |
| V23-15 | **nc1 de PB-TC2:** el sustituto falló por su propiedad 5, y la aceptación del Owner vale solo para PB-TC2, intento 1 (F4B-E12) | no material (procedimiento; C11 ya lo manda al Owner) | C11 dice que tampoco se extiende; R-T1-08 prevé que nc1 de PB-T1a vuelva al Owner | §6.0 C11; §7 |
| V23-16 | La lección IRC-M02 de PB-TC2: las corridas temporales, también con el aviso de descarte sustituido | no material (procedimiento) | INV-PBT1x-10 | §6.0 |
| V23-17 | El arrastre de PB-TC2 a la red de PB-T1 («Ambos lados» en el alcance y en los datos del frente, sin prueba propia; M-07) | no material | Ya lo cubren CT-PBT1A-08 y 10; se anota | §5.2 |
| — | La emisión: estado, precondiciones, §8.4 con el resultado, y el generador `make_pbt1a_contract.py` en la custodia | — | La línea de estado y el §8.4 | Estado; §6.0; §8.4 |

## Registro de cambios v2.1 → v2.2

| Origen | Qué cambió | Dónde |
|---|---|---|
| **F4B-E11, D2: L-1 decidida, (a)** | L-1 pasa de abierta a «decidido: (a), excepción declarada», con la cita literal del Owner. La excepción ya no es una lectura del paquete: es la decisión D2 | Estado; §8.1; D-PBT1-05 |
| **D2, condiciones 1 a 10** | D-PBT1-15 nuevo: cada condición con su subtarea y su prueba, guarda o arrastre. Condición 1: los dos tipos y los cuatro miembros. Condición 9: CT-PBT1B-08 reescrito. Condición 10: la guarda G-L1, con sus archivos, sus fichas y sus reglas, y sus autopruebas en negativo (CT-PBT1B-14 y CT-PBT1C-04). Condición 8: la guarda de los escritores de `Name` del Plugin (CT-PBT1B-15). Condiciones 3 a 7: CT-PBT1B-16 nuevo y la parte 2 de CT-PBT1C-02, y arrastres a PB-T2 y PB-T4 | D-PBT1-15; §5.3; §5.4; §5.6; §6.2; §6.3 |
| **D2, el límite** | La parada PBT1x-C17 en los tres contratos, y el arrastre A-2 a PB-T2, PB-T3 y PB-T4, con el texto literal del Owner | §6.0; §10 |
| **F4B-E11, «PB-T1 PLAN»** | La división, aceptada en principio. Las condiciones de emisión pasan a ser el cierre de PB-TC2, la remedición sobre su GREEN y la emisión «normally». La lista de lo que comprueba la remedición, R-1 a R-13 | Precondiciones; §8.4 |
| **F4B-E11, D1: PB-TC2 sigue con la corrección** | La nota del §8.5 (la opción B de C14) ya no aplica: `RackHeaderHeightBox` queda corregida por PB-TC2. La consecuencia de D1 en el BOM (filas por valor exacto) se anota para la red | §8.5; R-8 |
| **Los alcances** | PB-T1b gana `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs` (12 rutas) y PB-T1c gana `tests/RackCad.Tests/PushBackWindowSharedReferenceGuardTests.cs` (5 rutas). Los `ForbiddenWriteScope` de `src/` no cambian | §6.2; §6.3 |
| **Los recuentos** | PB-T1b pasa de 13 a 16 CT; PB-T1c, de 3 a 4. Las previsiones de la CI se ajustan | §5.3; §5.4; §6.2; §6.3 |
| **El anexo L1-C** | «No elegido», con una línea. Su texto sigue en la v2.1 | Anexo L1-C |
| **La regla especial de esta redacción** | Sin lecturas del worktree: lo nuevo que necesita código va marcado «A MEDIR EN LA REMEDICIÓN SOBRE EL GREEN DE PB-TC2» | §8.4; D-PBT1-15; §9 |
| **Las lecturas declaradas al incorporar D2** | L-4 (cómo se prueba la condición 7), L-5 (el límite alcanza también a PB-T4 y, en PB-T1, a la vida de la edición clásica) y L-6 (qué es una premisa material). Ninguna es una decisión nueva | §8.3 |
| **Los riesgos y la custodia** | R-T1-12 reescrito (con el riesgo residual de G-L1) y R-T1-19 nuevo (la remedición cambia una premisa). La comprobación acotada de PB-T1b y PB-T1c revisa también lo de D2; la custodia lleva F4B-E11 y el apartado N de la remedición. INV-PBT1C-07c precisa qué `Name` no puede asignar la ventana | §7; §6.0; §6.3 |

## Registro de cambios v2 → v2.1

| Hallazgo de la comprobación acotada | Qué cambió | Dónde |
|---|---|---|
| **N-I-01** (IMPORTANTE; la pregunta de L-1 no era fiel ni sin sesgo) | **Lo corrigió la sesión** antes de esta v2.1: la pregunta de `owner-return-l1.md` (la línea base guarda las personalizaciones confirmadas; hoy solo se le cambia el nombre al dibujar; el riesgo residual de (a); las anulaciones en memoria de (b); la vuelta de (c) si fallan los A/B) y la línea de la opción (b) del §8.1, que ya trae la frase de `FrameExceptionOverride`. Esta v2.1 no la vuelve a tocar | `owner-return-l1.md`; §8.1 (b) |
| **N-M-01** | El tercer escritor de `Name`, el lote de vistas del menú (`RackMenuCommands.cs:135`), en HT1-14, en «Lo medido» del §8.1 y en A-2; y en la medición M-1 del registro | HT1-14; §8.1; §10 A-2; `pbt1-measurements.md` M-1 |
| **N-M-02** | La regla de la sesión del comparador: «sin cambios» es nula, o sin pendientes **y** con su lista confirmada equivalente a `Begin(<línea base>.Structure).Commit()` | D-PBT1-06; CT-PBT1B-10 |
| **N-M-03** | La rama de la fila (b) del paso 1 queda fijada: si da `SelectedId=P`, la regla compara sin mayúsculas y devuelve el id de la opción, como `PostBox`; con sus líneas esperadas en CT-PBT1B-03. El §8.3 se alinea | D-PBT1-09; INV-PBT1B-06; PBT1B-C14; CT-PBT1B-03; §8.3 |
| **N-M-04** | Cómo se producen en la ventana los cuatro bloqueantes de CT-PBT1B-01: el hueco negativo con la costura `CompositeState.SetGap(-1)` y un `LostFocus` en `AnnotationScaleBox`; la estructura de 1 y el separador sin hueco por documento (`EditInner`); la celda que no cabe con el diseño de `Blocking()` | CT-PBT1B-01 |
| **N-M-05** | La parte 2 de CT-PBT1B-07 recorre también las claves y los valores de todo `IDictionary` | CT-PBT1B-07 |
| **N-M-06** | El `RackFrameProjectStore` del estado y del commit es **local al método**, nunca un campo. De las dos que admite la comprobación («local o `static readonly`») se fija una, para no dejar latitud; INV-PBT1B-04 pasa a hablar de campos de instancia y prohíbe también los estáticos nuevos | D-PBT1-03; D-PBT1-07; INV-PBT1B-04 |
| **N-M-07** | PBT1A-C15 sale de `StopConditions` y pasa a ser el invariante INV-PBT1A-13 | §6.1 |
| **N-M-08** | La línea de F-03 lleva `pasadas-restaurar=<n>` y `frontal=<lado>`, con 2 y A esperados; otra cosa es PBT1A-C07 | §5.0; CT-PBT1A-14 |
| **N-M-09** | En el anexo L1-C, «INV-PBT1B-07 nuevo» pasa a INV-PBT1B-07b, y se dice que toda llamada a `Copy` lleva el resolver del mismo catálogo | Anexo L1-C |
| **N-M-10** (resto de M-13) | La definición de H-PB-44 se cita en [PKC] `:187` y `:1343` | §2.4 |

## Registro de cambios v1 → v2

| Hallazgo | Qué cambió | Dónde |
|---|---|---|
| **I-01** (L-1 con premisas inexactas) | L-1 se reescribe con las tres opciones del §4 de la revisión y con **todos** los consumidores del objeto compartido (M-1): solo el Plugin lo muta, tras cerrar la ventana. HT1-12 se corrige: la cola que aparca `ParkTailOf` apunta a la línea base y al commit (M-2). CT-PBT1B-07 comprueba **aparte, sin la exención**, que la cola, el commit y la sesión de la copia no comparten objetos con el original. CT-PBT1B-08 se extiende a los consumidores medidos. Arrastre A-2 con la obligación de no mutar | HT1-12, HT1-14; D-PBT1-05; CT-PBT1B-07 y 08; §8.1; §10 A-2; anexo L1-C |
| **I-02** (CT-07 inalcanzable) | Los objetos inmutables se excluyen con el criterio `IsImmutable` del censo del piloto, copiado; CT-07 nombra sus diseños | CT-PBT1B-07 |
| **I-03** (la paridad sin definir) | El ayudante de prueba `Pasada`, copia literal de W:659-704 adaptada al estado (no hay API de asiento), y el número de pasadas de cada camino. En el comparador, una sesión nula equivale a una sesión sin cambios pendientes | §5.0; D-PBT1-06; CT-PBT1B-01; CT-PBT1C-01 |
| **I-04** (el orden de la carga cambiaba) | Los portadores se adoptan donde hoy, dentro de `LoadFromModel` y bajo `suppressSync`: la carga del estado se parte en la de Application (`…Editor`) y `AdoptCarriers`, pública. CT-PBT1A-14 gana el guion F-03 | D-PBT1-03; D-PBT1-13 (3) y (4); CT-PBT1A-14; R-T1-13 |
| **I-05** (la copia de la cola no era literal) | La cola se copia campo a campo y literal, con `RackFrameProjectStore.DeepCopy`, como DA:557-566. Lo mismo en `Copy` de la sesión y del commit | D-PBT1-03; D-PBT1-05; D-PBT1-07 |
| **I-06** (faltaba C04) | PBT1A-C04, PBT1B-C04 y PBT1C-C04, cada una con el texto de su régimen | §6.1 a §6.3 |
| **I-07** (la API del mapa) | Las firmas exactas, el enum, la forma y la normalización de las rutas, y la fila `$.Design` del sobre | D-PBT1-08 |
| **I-08** (sin parada de autoridad entre subtareas) | La precondición 5 y la declaración obligatoria PBT1A-C15 | Precondiciones; §6.1 |
| **M-01** | El oráculo de los canónicos es CT-PBTC-06 más los del piloto; la garantía de los heredados se declara «salvo la canonicalización T2H-07» | CT-PBT1B-02; §8.2 |
| **M-02** | Las recetas de X-01 (copia literal de `BuildX01`), F-01, F-02 y F-03, en la ventana y en Core | §5.0 |
| **M-03** | CT-PBT1A-11 repite la preparación de `AbrirFueraDePantalla` con `Ventana()` y la carga en el `try`. M-01 de PB-T0 se declara **no corregido** (CT-PB0-17 sigue con `Abrir`) | CT-PBT1A-11; D-PBT1-12 |
| **M-04** | CT-PBT1A-07 gana la ruta de los portadores del rack | CT-PBT1A-07 |
| **M-05** | La regla de c-02 compara con los ids de `ToOptions` (recortados, sin duplicados sin mayúsculas). Las filas (a) a (e) del paso 1, literales | D-PBT1-09; INV-PBT1B-06 |
| **M-06** | INV-PBT1B-05 se limita a `src/`. La «Fuente» del mapa, con archivo y miembro, sin línea | INV-PBT1B-05; D-PBT1-08 |
| **M-07** | CT-PBT1B-12 con líneas literales; los cuatro casos bloqueantes también en la paridad con la ventana (CT-PBT1B-01) | CT-PBT1B-01 y 12 |
| **M-08** | La tabla del censo de CT-PBT1B-06 y la definición de «transitorio del armado» | CT-PBT1B-06 |
| **M-09** | `RackModuleCommit.Copy` y `SameAs`, literales, con ayudantes privados propios; las firmas de `Copy` e `Instance` | D-PBT1-05; D-PBT1-07 |
| **M-10** | `NamespaceFolderGuardTests` (7) y `HeaderBatchContractTests` (17) en los `RequiredTests` de PB-T1b; también `RackModuleEditSessionTests` (25) | §6.2 |
| **M-11** | El régimen de corrección del RED por mutantes (§16.8); la lectura de `Ci` con Core en `success` y sus precedentes; la regla «EXCEPCION» en PB-T1c | §6.0; §6.1; §6.3 |
| **M-12** | La comprobación acotada definida; la custodia de los contratos de PB-T1b y PB-T1c; el modelo paralelo interino (tres copias de la carga) y la salida si PB-T1c no cierra | §6.0; R-T1-17; R-T1-18 |
| **M-13** | Anclas y citas corregidas (M-10 del registro); `DDA` definido; la celda del Controller con `model-catalog.md:51` y `:97`; H-PB-44 citado con su fuente; `PushBackRichPreviewTests.cs:348-367` en el §4; el orden de los guiones de CT-14 | §2.0; §2.4; §4; §6.0; CT-PBT1A-14 |
| **M-14** | Los campos que la ventana solo transporta son `Editor` por D-PB-05, justificado | D-PBT1-08 |
| **M-15** | El comparador compara siempre el commit **y** la intención de la línea base | D-PBT1-06 |

**Ningún menor se deja para la emisión.**

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T1 (literal arriba). D-PB-03, D-PB-05 («FULL EDITABLE DESIGN wins for every field owned by the full Push Back editor»), D-PB-06 («Do not hide these corrections inside PB-T1 state extraction»), D-PB-10 (1/3; el intento 3 vuelve al Owner), §7.3 (T1B) y §7.4 (RACKEDITAR) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-01.1b, OB-01.5, OB-03.1, OB-05.1, OB-06.5, OB-10.1 («y sus subtareas si se dividen», `:470`), OB-T1B.1 a OB-T1B.3, OB-RE.1, H-PB-27, R-PB-01, R-PB-02, R-PB-05 y R-PB-09; la fila de PB-T1 del §5 (`:503`) y la cadena de cada tarea (`:515`) |
| [OPT1] (`F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`, F4B-E09) | Pregunta 2: c-02 «Como hoy, como regla explícita». Pregunta 3: `pb-21-cola` «Dejarlo fuera de la garantía». Los demás casos de T-2, tal cual. H-31 y OWNER-1 en PB-TC2. Pregunta 4: la autorización condicional de emisión |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Pregunta 1: los sustitutos de nc1 y nc2 solo con la firma EXACTA. Pregunta 2: una replanificación por la comparación literal |
| IRT1-1 (`F4B-DRAFTS/pbt1/independent-review-v1.md`) | Los 23 hallazgos de la v1 y las tres opciones de L-1 (su §4) |
| **[OD2] Las decisiones D1 y D2 del Owner** (`F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`, F4B-E11) | **D1:** PB-TC2 «CONTINUE WITH THE FIX» (la vuelta C14 se resuelve con su opción A). **D2:** L-1 «ACCEPT DECLARED EXCEPTION», con «L-1 SAFETY CONDITIONS» 1 a 10 y el «IMPORTANT LIMIT». **«PB-T1 PLAN»:** la división aceptada en principio, con la remedición sobre el producto final de PB-TC2 antes de emitir |
| La vuelta C14 de PB-TC2 (`F4B-DRAFTS/pbtc2/owner-return-c14.md`) | El contexto de D1 (§8.5) |
| El expediente (`F4B-DRAFTS/pbt1/ob065-dossier.md`) | §3: los huecos de red. §4: las guardas de T1B y de texto |
| PB-T0: su paquete, su cierre y su entrega (`F4B-PBT0-CHARACTERIZATION/…`) | Los 21 CT-PB0; T-1, T-2 y T-5; M-01, M-04, D-3, D-5 y CT-16; el RED por mutantes |
| PB-TC ([PKC], v2.2) y su cierre | El patrón; M-02 a M-05; el precedente del RED con Core en `success` ([PKC]:466) |
| PB-TC2 ([PKC2], v2) | El patrón más reciente; M-03 y M-07; las firmas literales de PBTC2-C11; la celda del Controller ([PKC2]:655) |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) | 55 casos en UI y 10 en Core |
| Los contratos de F4 con API nueva (`I-64-f4-t2-gate-contract-package.md:282`, `I-64-f4-t3-gate-contract-package.md` INV-F4T3A-13, `I-64-f4-t3c-gate-contract-package.md`) | El RED con esqueleto inerte que compila y no lanza |
| ADR-0029 D12 y D13; Proposal V5 D-10, D-15 (punto 3) y D-16 | Sin modelo paralelo; la caracterización no se edita para que pase; ida y vuelta sin la ventana; sin cambio observable |
| `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9, §16.10 y §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | A1-A8, el RED de la cadena, las 14 comprobaciones, los términos, las paradas, nc1 a nc4 y las celdas |

## 2. Hechos medidos en `eedb069c` y remedidos sobre `b5c09f0b`

Los hechos se midieron en `eedb069c` (registro A a M) y se volvieron a medir sobre el producto de PB-TC2, `b5c09f0b`, en `cd47523b` (registro N). Donde la remedición movió algo, el texto lleva ya la ancla nueva; lo demás se sostiene tal cual (§2.8 y §8.4).

### 2.0 Alias

- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3700 líneas en `b5c09f0b`; 3690 en `eedb069c`); **WC** = `….Composite.cs` (936); **WX** = `….xaml` (793).
- **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`; **ES** = `…/PushBackEditorState.cs`; **EL** = `…/PushBackEditorState.Load.cs`; **DA** = `…/PushBackEditorDesignAssembler.cs`; **CA** = `…/PushBackCompositeEditorAssembler.cs`; **PCS** = `…/PushBackCompositeStructure.cs`.
- **DDA** = `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs`.
- **PC** = `…/PushBack/Pilot/PushBackPilotStateComparer.cs`; **PM** = `…/PushBack/Pilot/PushBackPilotMaterializer.cs`.
- **RMES** = `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs`; **RFPS** = `src/RackCad.Application/Persistence/RackFrameProjectStore.cs`.
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`; **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.

### 2.1 Hoy: qué estado editable vive en la ventana y qué ya vive en Application

- **HT1-01. Ya en Application:** `composite` (W:56-57), un `PushBackCompositeEditorState` con los dos `PushBackEditorState`. Lleva los dos lados, la intención compuesta, la presencia de B y `sideBEverDeclared`, la cola aparcada (CS:58, CS:61) y su marca de consumo (CS:147), la selección (CS:167, CS:179 y la de cada matriz) y, por lado, la **autoridad de módulos**: `workingBaseline` (ES:65), `moduleSession`, `moduleCommit` y `moduleSignature` (ES:110-112) y `LastModuleReconciliation` (ES:143). Los módulos son del lado A (`moduleOwner`, W:82).
- **HT1-02. En campos de la ventana** (medición B.1): `generalPallet` (W:101), `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale` (W:106-109), `advanced` (W:114), `firstLevelDatum` (W:380), `loadedDimensionViews` y `dimensionViewsTouched` (W:1249, W:1260), `loadedDimensionStyle` (W:1256) y `safetySelections` (W:84).
- **HT1-03. Solo en un control, releído en cada pasada por `ReadInputs`** (medición B.3): `WeightUnitBox` (W:852), `PalletsDeepBox` (W:853), `PostBox` (W:868), las tres casillas de anotación (W:874-876), `DimensionsBox` (W:878) y las tres casillas de cotas (`SelectedDimensionViews`, W:1291-1297, desde W:880).
- **HT1-04. Contexto de la sesión clásica, no editable:** `sourceProject` (W:91), `isEditingExisting` (W:87), `frontalSide` (W:3261) y el origen de cabecera pendiente (W:2038-2039).
- **HT1-05. La identidad y el nombre** viven en `session.Identity` (`RackEditorIdentity`, UI): `Adopt` en W:279, :305, :322; `SetName` en `Name_Changed` (W:1234-1238) y en W:3352, :3405, :3504; `EnsureId` en W:3503.
- **HT1-06. Derivados o de interfaz** (medición B.2): `lastComputation`, las banderas de validez, `outputIsBlocked`, `lastCompositeSystem`, el estado mixto (WC:416, WC:429) y las cachés.

### 2.2 El flujo: quién siembra, quién lee y quién escribe

- **HT1-07.** Las tres cargas y «Restaurar» (W:270-289, W:294-307, W:311-324, W:3307-3330) hacen la parte de Application y después `LoadFromModel` (W:385-440), que **siembra a la vez los controles y los portadores, bajo `suppressSync`** (W:387-437), y recalcula (W:439). En `LoadNew` hay dos pasadas, con la seguridad por defecto entre ellas (W:280, W:286-288).
- **HT1-08. La parte de Application de la carga vive en la ventana:** `SideAOnly` (W:342-373) y `LoadCompositeFromDesign` (WC:813-814, WC:827-934), toda de Application salvo `UpdateCompositeEnabled()` (WC:933). El piloto tiene su propia copia (PM:150-172).
- **HT1-09. Cada pasada** (`Recompute`, W:616-723) escribe la celda primaria desde los controles (W:635 → `ReadCellValues` W:802-831 **[TC2]**), actualiza los portadores con `Keep` (W:838-844), construye unos inputs **nuevos** (W:846-889), arma y resuelve (W:659-660) y adopta la computación (W:674-704). La adopción abre la sesión de módulos (`RefreshModuleSelector`, W:704 → W:1688 → ES:124-133; medición M-6).
- **HT1-10. El ensamblador muta lo que recibe:** el estado del lado A (DA:155-159) y los inputs de la pasada (DA:302).
- **HT1-10b. La carga puede reentrar** (medición M-7): `UpdateCompositeEnabled()` (dentro de `LoadCompositeFromDesign`, WC:933) puede poner `FrontalSideBox` a 0 (WC:117-121), y eso lanza una pasada síncrona (W:3277-3289; `RecomputeGate.cs:35-44`) **antes** de `LoadFromModel`. Esa pasada lee controles todavía sin sembrar; hoy `LoadFromModel` reasigna después los portadores (W:391-427).

### 2.3 Los alias con el modelo cargado y dentro del estado (mediciones D, M-1 y M-2)

- **HT1-11.** `SideAOnly` y el diseño del lado B de `LoadCompositeFromDesign` comparten objetos con el diseño del llamador, solo de forma transitoria: la carga resuelve y el resolver copia (D-1, D-2).
- **HT1-12 (corregido). La cola aparcada guarda referencias, por dos caminos:**
  - **al cargar,** los mismos `DynamicRackModuleDesign` y `DynamicHeaderLineOverride` del diseño cargado (WC:863-867 → CS:85-95; D-3);
  - **al dormir,** `ParkTailOf` (CA:180-242) aparca intenciones cuyo `HeaderConfiguration` es el `RackFrameConfiguration` **del módulo de la línea base** (`ToTailDesign`, PCS:777 → PCS:800-811), las **mismas** líneas de la línea base (CA:188-190) y las **mismas** intenciones y líneas del commit (CA:201-239) (M-2).

  Por lectura nadie las muta: el despertar copia campo a campo (DA:557-566).
- **HT1-13.** `sourceProject` es el proyecto del llamador, por diseño (D-4).
- **HT1-14 (ampliado). La línea base resuelta es el mismo objeto que `lastComputation.System`, `session.System` y `lastCompositeSystem`** (W:674-711). La medición M-1 lista **todos** sus consumidores: el armado, la sesión, `ParkTailOf`, el panel compuesto, el criterio de bloqueo, los módulos del panel, la vista previa, el lote de vistas y las peticiones de inserción. **Todos leen salvo el Plugin,** que escribe `Name` **después de cerrar la ventana**, cuando su estado ya se descarta, en tres sitios: `RackPushBackCommands.cs:98` y `:190`, y el lote de vistas del menú, `RackMenuCommands.cs:135` (`pushBack.System.Name = pushBackName;`). `ProjectVariableMutationExecutor.cs:169` resuelve su propio sistema (Selectivo) y no cuenta.

### 2.4 Los controles fuente de verdad que quedan tras PB-TC y PB-TC2 (medición E)

**La definición** es la que usa el paquete de PB-TC para H-PB-44, formulada en su H-31 ([PKC] `:187`) y en su §10 ([PKC] `:1343`); la fila de H-PB-44 (`:355`) solo la nombra: un control que se relee sin que el usuario cambie su valor y, al releerse, cambia el estado o la salida. [OPD] pregunta 4 decidió el alcance de PB-TC (OD-PBTC-5), no esta definición.

- **HT1-15.** Con el diseño de PB-TC2 v2, **solo `PostBox` decide el estado con el catálogo entregado**: c-02 (un id fuera del catálogo → nulo → `Defaults.Post`; W:399, W:868, DA:169-171) y c-01 (en blanco → `Defaults.Post`; W:399). **D1 del Owner** (F4B-E11, «CONTINUE WITH THE FIX») resolvió la vuelta C14 con la corrección: `RackHeaderHeightBox` deja de redondear con PB-TC2 y **no se añade** aquí (§8.5). **Medido sobre `b5c09f0b`** (registro N-4): W:1639 es `KeepOptional(RackHeaderHeightBox, advanced.ManualHeaderHeightOverride)`, las lecturas crudas que quedan son enteros o acciones del usuario, y una caja mixta solo tiene valor si el usuario lo escribe. La conclusión se sostiene.
- **HT1-16.** Los demás controles de HT1-03 no pierden nada, pero hoy su valor no tiene portador fuera del control. PB-T1 se lo da.
- **HT1-17.** `RearPeralteBox` y los combos de peralte de la celda solo sustituyen con un catálogo distinto del entregado o con un documento que la carga ya normaliza (E-3, E-4). Sin caso alcanzable (OB-06.5).
- **HT1-18. M-07 de PB-TC2:** el indicador mixto obsoleto hace inerte una caja vacía. No cambia el estado (E-7).

### 2.5 El criterio de bloqueo de salida (medición F)

- **HT1-19.** La línea de la fase (`RackPushBackSystemWindow.Composite.cs:778-782` en `5ccaa770`) es `CompositeHasBlocking`; hoy **WC:803-807**, con `CompositeDiagnostics` (WC:783-786) y `CompositeStatusOr` (WC:789-801). Es «los diagnósticos de la intención (CS:629-631) más los del sistema resuelto (`PushBackCompositeDiagnostics.Evaluate`)». **No es `RackBomOutputGate.For`** (`RackBomOutputGate.cs:49-59`).

### 2.6 El copiador y el comparador del piloto (medición G)

- **HT1-20.** `PushBackPilotStateComparer.Copy` (PC:58-111) no copia la autoridad de módulos ni la cola, y el comparador no las lee (PC:53-56, PC:134-177). `KeepPilotEditsOn` (PM:242-251) nace sin ellas y `ComposeForWrite` (PM:196-234) repone desde la base (H-PB-27).

### 2.7 Guardas que atan el diseño y que el expediente no lista (mediciones H y M-10)

- **HT1-21. El censo del piloto** (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:204-537`, `:955-982`): **un campo nuevo** en `PushBackCompositeEditorState`, `PushBackEditorState`, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions` o los tipos de seguridad lo rompe. Los métodos nuevos no.
- **HT1-22.** `NamespaceFolderGuardTests` exige un solo `namespace` de bloque por archivo de `src/`, igual a su carpeta (medición H-11).
- **HT1-23.** `PushBackRichPreviewTests.cs:348-367` recorre por reflexión los métodos y los tipos de campo no públicos de la ventana (M-10).
- **HT1-24.** Ninguna prueba nombra los campos que PB-T1c convierte en reenvíos; solo los literales de T-SG nombran `lastComputation`, `sourceProject`, `moduleOwner`, `session.Identity` y `NameBox.Text` (medición H).

### 2.8 Lo que PB-TC2 cambió [TC2] (medido sobre `b5c09f0b`; registro N-0 a N-2)

- **Los cambios, en las líneas previstas y con el diseño de PB-TC2 v2:**
  - WC:198 `composite.SetGap(Keep(GapBox, composite.Gap));`;
  - W:1905 `var length = Keep(ModuleLengthBox, module.Length);`;
  - W:1639, :1641, :1642 y :1646, con `KeepOptional(<caja>, advanced.<campo>)`;
  - W:818-822, con `KeepCell(<caja>, <valor>)`;
  - en `OptionalVal` (W:3663-3666), la rama con valor pasa a `MixedBoth(field) ? field.Value : KeepOptional(field, sideValue)`, y la rama vacía (W:3666) no cambia;
  - `Val` sale;
  - `KeepOptional` (W:3687-3688), `MixedBoth` (W:3691-3692) y `KeepCell` (W:3695-3696) entran tras `Keep` (W:3683-3684).
- **Las líneas de W anteriores a W:3631 no se movieron.** Las posteriores se movieron −2 hasta `Keep`, e `IntVal` pasó a W:3698. WC y WX conservan sus líneas.
- **PB-T1 usa de esas zonas** los mutantes MU-T1A-03 (W:817) y 04 (W:827), que no se movieron, MU-T1A-07a (`Keep`, ahora W:3684) y MU-T1A-10 (la rama vacía de `OptionalVal`, ahora W:3666).
- **El objetivo de cada mutante se conserva** (registro N-2). `Keep` lo usan ahora también `KeepCell` (fuera de «Ambos lados» con B), el hueco y la longitud del módulo. Por eso, en el RED, las pruebas de PB-TC2 que fallen se atribuyen a MU-T1A-07a en la tabla T-8.

## 3. El diseño

### D-PBT1-01. Tres subtareas secuenciales (R-PB-01, R-PB-09, OB-10.1)

**Por qué dividir:** tres regímenes de RED distintos (mutantes, esqueleto inerte y costura inerte); la red tiene que estar en la CI y con su discriminación probada **antes** de mover el estado; el tipo de Application se prueba entero contra la ventana de hoy **antes** de que la ventana dependa de él (D-15, punto 3); y el tamaño: PB-T0 hizo 21 CT y 20 mutantes en 437 turnos, y PB-T1 entera serían unos 30 CT, 15 mutantes, cuatro archivos de Application y la refactorización de W y WC.

**La secuencia:** PB-T1a → PB-T1b → PB-T1c, cada una desde la punta que deja cerrada la anterior, sin rebase entre tareas, con delegaciones sucesivas (`AUTOMATION_PLAN.md:557-558`). PB-T1a toca W y WC solo con mutantes, que revierte.

**Lo que no cambia en la fase:** PB-T2 y PB-T3 dependen de PB-T1, que cierra con PB-T1c. El contenido es el del §5 de [P].

**Propiedad de la división:** PB-T1a solo cambia `tests/` en su commit final y PB-T1b solo cambia Application. Por eso los anclajes de W y WC de PB-T1c son los del cierre de PB-TC2.

### D-PBT1-02. El tipo de estado: `PushBackEditableState` (R-PB-05)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`, `namespace RackCad.Application.Systems.PushBack` de bloque. Ni en `Pilot/` ni en `Workspace/`.

**Qué es:** un **agrupador** del estado que ya existe, sin modelo paralelo (D-10, ADR-0029 D12). `public sealed class PushBackEditableState`, con un constructor público sin argumentos y estos miembros:

| Miembro | Tipo | Valor inicial (el de hoy) | Es hoy |
|---|---|---|---|
| `Editor` | `PushBackCompositeEditorState` `{ get; }` | `new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState())` | `composite` (W:56-57) |
| `Pallet` | `PalletSpecification` `{ get; set; }` | `PushBackEditorInputs.NewDesign().Pallet` | `generalPallet` (W:101); **también lleva la unidad** |
| `PalletsDeep` | `int` `{ get; set; }` | `DynamicRackDefaults.DefaultPalletsDeep` | `PalletsDeepBox` |
| `PostCatalogId` | `string` `{ get; set; }` | `null` | `PostBox` |
| `PostPeralte`, `PalletTolerance`, `BeamDepth`, `AnnotationScale` | `double` `{ get; set; }` | `PushBackEditorInputs.NewDesign().PostPeralte`, `DynamicRackDefaults.DefaultPalletTolerance`, `DynamicRackDefaults.DefaultBeamDepth`, `1.0` | W:106-109 |
| `FirstLevelDatum` | `int?` `{ get; set; }` | `(int)RackFirstLevelDatumMode.LowestUsablePunch` | W:380 |
| `NumberFronts`, `NumberLevels`, `DrawRackName` | `bool` `{ get; set; }` | `false` | las tres casillas |
| `Dimensions` | `DimensionDetail` `{ get; set; }` | `DimensionDetail.None` | `DimensionsBox` |
| `DimensionStyle` | `string` `{ get; set; }` | `null` | `loadedDimensionStyle` (W:1256) |
| `LoadedDimensionViews` | `DimensionViewVisibility?` `{ get; set; }` | `null` | W:1249 |
| `DimensionViewsTouched` | `bool` `{ get; set; }` | `false` | W:1260 |
| `DimensionsFrontal`, `DimensionsLateral`, `DimensionsPlanta` | `bool` `{ get; set; }` | `false` | las tres casillas de cotas |
| `Advanced` | `PushBackEditorInputs` `{ get; }` (solo sus seis avanzados) | `PushBackEditorInputs.NewDesign()` | `advanced` (W:114) |
| `SafetySelections` | `List<SelectiveSafetySelection>` `{ get; }` | vacía | `safetySelections` (W:84) |
| `Name` | `string` `{ get; set; }` | `null` | el nombre de la identidad (HT1-05) |

**No contiene** (D-PBT1-08): el GUID, `sourceProject`, `isEditingExisting`, `frontalSide`, el origen de cabecera pendiente, el estado mixto, `lastComputation` ni lo demás de HT1-06. **Ningún campo nuevo en los tipos del censo del piloto** (HT1-21).

### D-PBT1-03. Las cargas, con la semántica y el orden de hoy (I-04)

Son **métodos de instancia que mutan en sitio** el mismo `Editor`, como hoy la ventana muta su `composite`.

**La carga se parte en dos,** para que la ventana adopte los portadores donde hoy (dentro de `LoadFromModel`, bajo `suppressSync`, **después** de la posible pasada reentrante de HT1-10b):

| Método (público) | Reproduce | Pasos, en este orden |
|---|---|---|
| `PushBackEditorInputs LoadNewEditor()` | W:273-276 | `Editor.SetActiveSide(PushBackSide.A)`; `Editor.SetSideBPresent(false)`; `Editor.LoadComposite(null)`; devuelve `Editor.Active.LoadNew()` |
| `PushBackEditorInputs LoadDesignEditor(PushBackDesign design, PushBackResolver resolver)` | W:316-319 (= W:299-302) y WC:827-932 con `preserveDormantIntent: false` | `SetActiveSide(A)`; `SetSideBPresent(false)`; `var inputs = Editor.Active.LoadFromDesign(SideAOnly(design), resolver)`; `LoadComposite(design, false, resolver)`; devuelve `inputs` |
| `PushBackEditorInputs RestoreEditor(PushBackDesign lastValidDesign, PushBackResolver resolver)` | W:3320-3327 | igual, con `preserveDormantIntent: true` |
| `void AdoptCarriers(PushBackEditorInputs inputs, RackCatalog catalog)` | la parte de portadores de `LoadFromModel` (W:391-427) | abajo |
| `void SeedDefaultSafety(RackCatalog catalog)` | W:286-287 | `SafetySelections.Clear()`; `SafetySelections.AddRange(new PushBackSafetyAuthority(catalog).Defaults())` |
| `PushBackEditorInputs LoadNew(RackCatalog catalog)` | lo anterior, para quien no es la ventana | `LoadNewEditor()`; `AdoptCarriers(inputs, catalog)`; `Name = null` |
| `PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver)` | ídem | `LoadDesignEditor(…)`; `AdoptCarriers(inputs, resolver.Catalog)`; `Name = rackName` |
| `PushBackEditorInputs Restore(PushBackDesign lastValidDesign, PushBackResolver resolver)` | ídem | `RestoreEditor(…)`; `AdoptCarriers(inputs, resolver.Catalog)`; `Name` sin tocar |
| `static PushBackDesign SideAOnly(PushBackDesign design)` | W:342-373, movido literal | — |

- **`LoadComposite(design, preserveDormantIntent, resolver)`** es privado y es WC:829-931 movido literal, con dos cambios sin efecto observable:
  1. no llama a `UpdateCompositeEnabled()`, que es de la interfaz;
  2. **la cola que aparca se copia campo a campo y literal (I-05):** cada `DynamicRackModuleDesign` como `new DynamicRackModuleDesign { ModuleId, Kind, Length, IsCalculated, IsManualOverride, UseCalculatedHeaderConfiguration, HeaderConfiguration = <store>.DeepCopy(HeaderConfiguration), Notes }`, con los mismos ocho campos que copia el despertar (DA:557-566), y cada `DynamicHeaderLineOverride` como `new DynamicHeaderLineOverride { PostIndex, ModuleId, Header = <store>.DeepCopy(Header) }`. `<store>` es un `RackFrameProjectStore` (RFPS:87) **local al método, nunca un campo** (N-M-06): un campo de instancia entraría en el censo de CT-PBT1B-06 y rompería INV-PBT1B-03. **No se usa `CopyIntent` de RMES,** que es privado y normaliza la procedencia (M-3).
- **`AdoptCarriers(inputs, catalog)`** reproduce W:391-427 con sus normalizaciones literales:
  - `Pallet` = `inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")`, con la unidad en blanco → «kg» (W:392, W:395), como objeto nuevo;
  - `PalletsDeep = Math.Max(2, inputs.PalletsDeep)` (W:396);
  - `PalletTolerance`, `BeamDepth` y `AnnotationScale` con su «> 0 ? : defecto» (W:397-398, W:402-403, W:418-419);
  - `PostPeralte = inputs.PostPeralte` (W:401);
  - **`PostCatalogId = ResolvePostCatalogId(catalog, inputs.PostCatalogId)`** (D-PBT1-09);
  - los seis avanzados copiados a `Advanced` (W:406-411);
  - de `inputs.Annotations ?? new DynamicAnnotationOptions()`: `DimensionStyle`, las tres casillas y `Dimensions` recortado a `[0, Detailed]` (W:413-420);
  - `LoadedDimensionViews = options.DimensionViews`, `DimensionViewsTouched = false` y cada casilla de cotas = `DimensionViewPolicy.EffectiveDetail(DimensionDetail.Minimal, views, kind) != DimensionDetail.None` (W:1268-1287);
  - `FirstLevelDatum = inputs.FirstLevelDatum` (W:391);
  - `SafetySelections` vaciada y llenada con `DeepCopy` de las no nulas (W:423-427).
- **Una carga que lanza** (c-13, D-3) lanza el mismo tipo y el mismo mensaje que hoy, desde el resolver, antes de tocar los portadores.

### D-PBT1-04. La salida, con la semántica de hoy

- **`PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)`** devuelve **un objeto nuevo en cada llamada**, igual campo a campo al de `ReadInputs` (W:846-889) con los controles sin editar: `Pallet` nueva con los cinco valores; los escalares y los seis avanzados copiados; `Annotations` nuevas con `DimensionViews = DimensionViewPolicy.FromEditor(LoadedDimensionViews, DimensionViewsTouched, DimensionsFrontal, DimensionsLateral, DimensionsPlanta)`; `SafetySelections` = `assembler.AuthorizedSafety(SafetySelections)`.
- **Por qué nuevo cada vez:** el ensamblador muta los inputs (DA:302).
- **El armado no cambia:** sigue siendo `assembler.BuildFrom(compositeAssembler.BuildDesign(state.Editor, state.BuildInputs(assembler)), frontalSide)` y la adopción de W:674-704. **No hay API de asiento** (I-03): PB-T1 no la crea, y las pruebas usan el ayudante `Pasada` (§5.0).

### D-PBT1-05. El copiador (H-PB-27)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs`. **Firmas:** `public sealed class PushBackEditableStateComparer : IEqualityComparer<PushBackEditableState>`, con `public static PushBackEditableStateComparer Instance { get; }` y `public static PushBackEditableState Copy(PushBackEditableState state)`. **El copiador del piloto no se toca.**

**Qué hace,** en este orden:
1. **El contenido editable y la selección:** el `Snapshot()`/`Restore()` de CS sobre un `Editor` nuevo, con la reposición de `ActiveSelection`, de la selección de cada lado y de `ActiveSide` que hace el piloto (PC:64-83), copiada.
2. **La cola:** `copy.Editor.ParkDormantTail(<copias>, <copias>)` con la copia literal de D-PBT1-03; después, si `source.Editor.DormantTailConsumptionPending`, `copy.Editor.MarkDormantTailConsumed()` (CS:116-117).
3. **La autoridad de módulos de cada lado:** `copy.Editor.SideX.CopyAuthorityFrom(source.Editor.SideX)` (D-PBT1-07), sobre el lado ya restaurado:
   - la sesión con `RackModuleEditSession.Copy()` y el commit con `RackModuleCommit.Copy()`, o null si eran null;
   - `moduleSignature`, copiada;
   - **`workingBaseline` y `LastModuleReconciliation`, compartidos por referencia: es la excepción decidida por el Owner** (F4B-E11, D2: «the resolved rack» y «the module report»), con las condiciones de D-PBT1-15.
4. **Los portadores:** campo a campo; `Pallet` nueva; los seis avanzados de `Advanced`; `SafetySelections` con `DeepCopy`; `Name`.

**El resultado** es igual para el comparador; conserva los módulos (sesión con lo escenificado, commit y línea base), la cola (también la que `ParkTailOf` aparcó desde la línea base, ahora copiada) y el lado B con `sideBEverDeclared`; y **no comparte con el original ningún objeto mutable fuera de las dos referencias declaradas de D2 y de lo que ellas alcanzan**. Como dice el Owner, «This is NOT authorization for general shallow copying». En particular, la cola, el commit y la sesión de la copia no comparten **nada** mutable con el original (CT-PBT1B-07, parte 2).

### D-PBT1-06. El comparador

Compara con igualdad exacta (un `double` por valor, una cadena ordinal):
- **el contenido editable del `Editor`**, como el piloto (PC:134-249, copiado);
- **la cola**, elemento a elemento, con la forma persistida de cada configuración de cabecera (`RackFrameProjectStore`, como RMES:818-846);
- **la autoridad de módulos de cada lado:**
  - **siempre** la intención de módulos de la línea base (`RackModuleEditSession.Begin(<baseline>.Structure).Commit()` y las listas `HeaderLineOverrides` y `DerivedPostLineOverrides`, como DA:57-81), con igualdad estructural (M-15);
  - **siempre** el commit, con `RackModuleCommit.SameAs`, o los dos null;
  - **la sesión, con esta regla (I-03, N-M-02):** una sesión es «sin cambios» si es null, o si `HasPendingChanges` es false **y** su lista confirmada es equivalente, con `SameEditAs`, a `RackModuleEditSession.Begin(<línea base de su lado>.Structure)` recién abierta (la que abriría el getter, ES:124-133). Así una sesión que conserva la lista confirmada de una línea base anterior con la misma firma (`ReseedModuleSession`, ES:176-187) no cuenta como «sin cambios». Dos sesiones sin cambios son iguales. Si alguna tiene cambios, las dos tienen que existir y ser iguales con `SameEditAs`;
- **los portadores** (todos los de D-PBT1-02), con la igualdad estructural de la seguridad del piloto (PC:309-350, copiada).

**No compara:** la selección, la línea base como objeto, el informe de reconciliación ni la marca de consumo. `GetHashCode` mira campos baratos (como PC:37-49). `NaN` es igual a sí mismo.

### D-PBT1-07. Los métodos nuevos en ES y en RMES (sin campos nuevos)

- **ES:** `public void CopyAuthorityFrom(PushBackEditorState source)`, con el paso 3 de D-PBT1-05. **CS no cambia.**
- **RMES, en `RackModuleEditSession`:**
  - `public RackModuleEditSession Copy()`: una sesión nueva con `restored`, `committed`, `working`, `standardRestoreRequested`, las dos tablas de líneas y las dos de postes derivados, copiados **literales**: cada intención con un ayudante privado nuevo, `CopyLiteral`, que copia sus ocho campos con `clone.DeepCopy` para la configuración (como DA:557-566, **sin** `ResolveProvenance`), y las líneas con `CopyLines` (RMES:732-742);
  - `public bool SameEditAs(RackModuleEditSession other)`: `committed` y `working` con `AreEquivalent` (RMES:818-846), las líneas con `AreEquivalentLines` (RMES:750-769), los postes con `AreEquivalentDerivedPosts` (RMES:744-746), `restored` en orden y `standardRestoreRequested`.
- **RMES, en `RackModuleCommit`** (RMES:16-50), que no alcanza los privados de la sesión (M-09):
  - `public RackModuleCommit Copy()`: un commit nuevo por su constructor `internal` (RMES:18), con `Modules` copiados por un ayudante privado estático propio, `CopyModule`, idéntico a `CopyLiteral`; `RestoredModuleIds` en una lista nueva; `StandardRestoreRequested`; `LineOverrides` como `new DynamicHeaderLineOverride { PostIndex, ModuleId, Header = <store>.DeepCopy(Header) }`; y `DerivedPostOverrides` como `new DynamicDerivedPostLineOverride { PostIndex, Height }`;
  - `public bool SameAs(RackModuleCommit other)`: las mismas listas, en orden, comparando cada configuración por su forma persistida con un `RackFrameProjectStore` (como RMES:818-846).
  - **El `RackFrameProjectStore` de `RackModuleCommit`** (`<store>` en `Copy` y el de `SameAs`) es **local al método, nunca un campo** (N-M-06; INV-PBT1B-04). La sesión usa su campo `clone` de siempre (RMES:74).
- **Neutrales:** no nombran `RackSystemKind` ni Push Back; sus nombres no empiezan por un verbo de motor (`HeaderBatchContractTests` C-10, `:524-537`).

### D-PBT1-08. El mapa de propiedad (OB-05.1)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs`. Una **tabla de datos**, sin lógica de composición (es de PB-T3).

**La API exacta (I-07):**

```csharp
public enum PushBackFieldOwner { Unclassified = 0, Editor = 1, Envelope = 2, Container = 3 }

public enum PushBackDocumentLevel { Envelope = 0, Project = 1, Payload = 2 }

public sealed class PushBackOwnershipRow
{
    public PushBackOwnershipRow(PushBackDocumentLevel level, string path, PushBackFieldOwner owner, string source);
    public PushBackDocumentLevel Level { get; }
    public string Path { get; }
    public PushBackFieldOwner Owner { get; }
    public string Source { get; }
}

public static class PushBackEditableStateOwnership
{
    public static IReadOnlyList<PushBackOwnershipRow> Table { get; }
    public static string Normalize(string jsonPath);
    public static PushBackFieldOwner Of(PushBackDocumentLevel level, string jsonPath);
}
```

- **Los niveles:** `Envelope` es la raíz del JSON del sobre (`RackEmbedDocument`); `Project` es la raíz del JSON de su propiedad `Design` (`RackProjectDocument`); `Payload` es el objeto `PushBack` del proyecto (`PushBackDesignDocument`).
- **Las rutas:** de la forma `$.A.B[3].C`, con los nombres de propiedad del JSON. `Normalize` cambia todo índice `[n]` por `[*]`.
- **`Of`:** normaliza la ruta y busca, en las filas de su nivel, la de **prefijo más largo**. Una fila de ruta `Q` cubre una ruta `P` si `P == Q`, o si `P` empieza por `Q + "."` o por `Q + "["`. Si ninguna fila cubre la ruta, la primera propiedad de la ruta no es una propiedad declarada del documento de su nivel, y el nivel tiene `[JsonExtensionData]` (los tres la tienen), el resultado es `Envelope` («ExtensionData»). Si no, `Unclassified`.
- **La tabla:**

| Nivel | Ruta | Clase | Fuente (archivo y miembro) |
|---|---|---|---|
| Envelope | `$.SchemaVersion`, `$.Kind`, `$.View`, `$.Section`, `$.Id`, `$.CustomProperties` | `Envelope` | `RackEmbedDocument.cs`, `RackEmbedDocument` |
| Envelope | `$.Name` | `Editor` | `RackPushBackSystemWindow.xaml.cs`, `Name_Changed` |
| Envelope | `$.Design` | `Container` (su contenido se clasifica en el nivel `Project`) | `RackEmbedDocument.cs`, `RackEmbedDocument.Design` |
| Project | `$.SchemaVersion`, `$.Kind`, `$.Header`, `$.DynamicSystem`, `$.SelectiveRack`, `$.FlowBed`, `$.Larguero`, `$.Cantilever` | `Envelope` | `RackProjectDocument.cs`, `RackProjectDocument`; `RackPushBackSystemWindow.xaml.cs`, `SaveLibrary_Click` (`WithSourceMetadataFrom`) |
| Project | `$.PushBack` | `Container` (nivel `Payload`) | `RackProjectDocument.cs`, `RackProjectDocument.PushBack` |
| Payload | `$.SchemaVersion` | `Envelope` | `PushBackDesignDocument.cs`, `PushBackDesignDocument` |
| Payload | `$.Structure`, `$.Fronts`, `$.LegacyHighEndBeamPeralte`, `$.RearTopeSaque`, `$.RearTopePieceId`, `$.DefensePieceId`, `$.RearTopeOffCells`, `$.SideB`, `$.Composite` | `Editor` | `PushBackEditorDesignAssembler.cs`, `BuildDesign`; `PushBackCompositeEditorAssembler.cs`, `BuildDesign`; `DynamicEditorDesignAssembler.cs`, `BuildDesign` |
| (sesión del borrador) | `RackId` y los metadatos | `Envelope` | `DraftSession.cs`, `DraftSession` (no es una ruta: se documenta, sin fila) |

- **Por qué lo que la ventana solo transporta es `Editor` (M-14):** `FirstLevelDatum`, `DimensionStyle`, `LegacyHighEndBeamPeralte` y los demás campos sin control **forman parte del diseño que el editor escribe entero en cada salida** (D-PBT1-04; MB §1: «lo que la ventana carga y escribe»). [OD] D-PB-05 manda que «FULL EDITABLE DESIGN wins for every field owned by the full Push Back editor»: con «Conservar mi borrador», el diseño del borrador sale entero, también esos campos. «Lo que no edita la ventana» de OB-05.1 es lo que queda **fuera** de ese diseño: el sobre, el proyecto y la sesión.
- **`Name` es del editor; la identidad es el GUID** (OB-05.1).
- **Límite declarado** (OB-05.2, IR2 R-I-10): los campos desconocidos de los documentos **anidados** se pierden en cada reescritura; el mapa los cuenta como `Editor` por su contenedor. La pregunta al Owner sigue siendo de antes de PB-T3.

### D-PBT1-09. La regla de c-02 (F4B-E09, pregunta 2; M-05)

- **`public static string ResolvePostCatalogId(RackCatalog catalog, string requested)`:**
  - en blanco o solo espacios → `catalog?.Defaults?.Post` (c-01, W:399);
  - si no, el id de la opción de `PostBox` que coincide con `requested` según la **comparación fijada por el paso 1**; si ninguna coincide, `catalog?.Defaults?.Post` (c-02);
  - **los ids que ofrece `PostBox`** son los de `UiSupport.ToOptions(catalog.PostProfiles)` (`UiSupport.cs:124-133`), reproducidos en Application: las entradas no nulas con `Id` no en blanco, agrupadas por `Id.Trim()` sin mayúsculas, la primera de cada grupo, con su `Id.Trim()`.
- **La comparación, con sus dos ramas fijadas (N-M-03):** la decide la fila (b) del paso 1 de PB-T1b (INV-PBT1B-06), que mide lo que hace hoy `PostBox.SelectedValue` con el id del catálogo en minúsculas:
  - **rama ordinal** (la lectura; fila (b) con `PostBox.SelectedId=<null>`): coincide la opción cuyo id es ordinalmente igual a `requested`, y la regla devuelve `requested`;
  - **rama sin mayúsculas** (fila (b) con `PostBox.SelectedId=P`): coincide la opción cuyo id es igual a `requested` con `StringComparison.OrdinalIgnoreCase`, y la regla devuelve **el id de esa opción**, como `PostBox`.

  El Worker aplica la rama que mida, la declara en la entrega y usa las líneas esperadas de esa rama en CT-PBT1B-03. No hay otra rama.
- **Por qué así:** el Owner pidió «como hoy» y «RACKEDITAR no cambia», y hoy decide `PostBox.SelectedValue`. Con el catálogo entregado (un solo poste, que es el de por defecto; medición M-4), las filas (a) a (e) dan la misma salida con cualquier rama; la rama solo importa con un catálogo editado con varios postes, y así el Workspace hereda la regla de hoy.
- **Dónde vive:** en `AdoptCarriers`. La ventana sigue sembrando `PostBox` con la expresión de hoy (W:399): el desplegable se ve igual.
- **CT-PB0-17 sigue en verde.**

### D-PBT1-10. La exclusión de `pb-21-cola` (F4B-E09, pregunta 3)

- **Declaración:** «Un rack de un solo sentido (`!design.IsComposite`) cuya `Structure` guarda módulos o líneas con ids de cola (`PushBackCompositeStructure.IsCompositeTailId`) **no es un supported classic rack** para la garantía sin pérdidas de PB-T1.»
- **`public static PushBackEditableSupport SupportOf(PushBackDesign design)`**, con `public enum PushBackEditableSupport { Supported = 0, DormantCompositeTail = 1 }`, en `PushBackEditableState.cs`. Es la condición del piloto (PM:160-170).
- **No cambia:** RACKEDITAR (pierde las 667 hojas al Actualizar, CT-PB0-01); `DormantTailOutOfPilotReason` en todo F4B (OB-03.1); la deuda H14.

### D-PBT1-11. El criterio de bloqueo de salida, extraído

`src/RackCad.Application/Systems/PushBack/PushBackEditorOutputGate.cs`, `public static class PushBackEditorOutputGate`:
- `public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(PushBackCompositeEditorState composite, PushBackSystem system)` = `composite.IntentDiagnostics().Concat(PushBackCompositeDiagnostics.Evaluate(system)).ToList()` (WC:783-786);
- `public static bool IsBlocked(PushBackCompositeEditorState composite, PushBackSystem system)` (WC:806);
- `public static string StatusOr(PushBackCompositeEditorState composite, PushBackSystem system, string fallback)` (WC:789-801).

`RackBomOutputGate` y `PushBackCompositeDiagnostics` no cambian. Lo usan la ventana desde PB-T1c y el PREPARE de PB-T3.

### D-PBT1-12. M-07 de PB-TC2 y los demás arrastres

- **M-07 se reproduce tal cual, y no aplica OB-06.5:** OB-06.5 habla de pérdidas, y aquí el estado no cambia (HT1-18): la caja no consigue editarlo. Por OB-RE.1, RACKEDITAR no cambia; corregirlo sería una decisión nueva, que no se propone. CT-PBT1A-10 lo fija como «INCUMPLIMIENTO CARACTERIZADO». Con esto se cierra «con una posible vuelta por OB-06.5» de [PKC2] §10.
- **M-03 de PB-TC y de PB-TC2, D-5, M-04, CT-16 y M-02 de PB-TC** entran en PB-T1a (§5.2).
- **M-01 de PB-T0 queda sin corregir:** S0 no cambia y `Abrir` sigue en CT-PB0-17. Las pruebas nuevas no usan `Abrir` ni `AbrirFueraDePantalla`.
- **M-05 de PB-TC:** las pruebas nuevas confirman o cancelan los módulos en un `finally` interno.
- **D-3:** CT-PBT1A-13 en la ventana y CT-PBT1B-05 en el estado.

### D-PBT1-13. La ventana sobre el estado (PB-T1c): un cambio mínimo, sin MVVM ni estética

**Regla general:** cada campo que pasa al estado se queda con su **mismo nombre** como reenvío privado.

1. **El estado:** en lugar de `composite` (W:56-57), `private readonly PushBackEditableState editable = new PushBackEditableState();` y `private PushBackCompositeEditorState composite => editable.Editor;`.
2. **Los reenvíos**, en el sitio de cada campo y con su comentario: `safetySelections` (`=> editable.SafetySelections`, W:84); `generalPallet` (`get`/`set` sobre `editable.Pallet`, W:101); `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale` (sobre `PalletTolerance`, `PostPeralte`, `BeamDepth` y `AnnotationScale`, W:106-109); `advanced` (`=> editable.Advanced`, W:114); `firstLevelDatum`, `loadedDimensionViews`, `loadedDimensionStyle` y `dimensionViewsTouched` (sobre `FirstLevelDatum`, `LoadedDimensionViews`, `DimensionStyle` y `DimensionViewsTouched`; W:380, W:1249, W:1256, W:1260).
3. **Las cargas,** con el orden de hoy:
   - `LoadNew`: W:273-276 → `var inputs = editable.LoadNewEditor();`; W:286-287 → `editable.SeedDefaultSafety(catalog);`;
   - `LoadDesignForNew` y `LoadExisting`: W:299-302 y W:316-319 → `var inputs = editable.LoadDesignEditor(design, assembler.Resolver);` y, en la línea siguiente, `UpdateCompositeEnabled();` (el sitio donde hoy lo llama `LoadCompositeFromDesign`, WC:933);
   - `Restore_Click`: W:3320-3327 → `var inputs = editable.RestoreEditor(design, assembler.Resolver);` y `UpdateCompositeEnabled();`.

   El proyecto, el modo, la identidad y `LoadFromModel` siguen en su orden.
4. **`LoadFromModel` adopta los portadores donde hoy (I-04):** dentro del `try`, bajo `suppressSync`, **la primera sentencia** pasa a ser `editable.AdoptCarriers(inputs, catalog);`, y se quitan las asignaciones a portadores (W:391, :393, :398, :401, :403, :406-411, :414, :419 y :423-427). Las siembras de los controles **no cambian ni un carácter** (W:390, :392, :394-397, :399-400, :402, :413, :415-418, :420-421). Es equivalente: ninguna siembra lee un portador, y `LoadAdvancedRackParameters` (W:432) y `ShowDimensionViews` (W:421) corren después y ven los mismos valores.
5. **`ReadInputs`** escribe en el estado lo que lee, con la expresión de hoy, y devuelve `editable.BuildInputs(assembler)`:
   - la caja de fondo y la unidad, en `generalPallet` (la unidad, quinto argumento de la `PalletSpecification` de W:838-840, con la expresión de W:852);
   - las cuatro de `Keep`, como W:841-844;
   - `editable.PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep);` (W:853);
   - `editable.PostCatalogId = PostBox.SelectedId ?? editable.PostCatalogId;` (W:868, con el respaldo de las vigas de W:823 y W:826). Con la regla de D-PBT1-09, el valor del estado es el que `PostBox` ofrecería o el que hoy pone DA:169-171, así que la salida es la de hoy también con catálogos editados (espacios, duplicados por mayúsculas);
   - las tres casillas, `Dimensions` y las tres de cotas, con las expresiones de W:874-876, W:878 y W:1295-1297.

   `SelectedDimensionViews` (W:1291-1297), cuyo único uso es W:880, se retira.
6. **`SideAOnly`** (W:342-373) y **`LoadCompositeFromDesign`** (WC:813-814, WC:827-934) se retiran.
7. **El criterio de bloqueo:** `CompositeDiagnostics` y `CompositeStatusOr` delegan en `PushBackEditorOutputGate` con `composite` y `lastCompositeSystem`; `CompositeHasBlocking` guarda `lastCompositeSystem = system` y devuelve `PushBackEditorOutputGate.IsBlocked(composite, system)`.
8. **El nombre:** `private void SyncName() => editable.Name = session.Identity.Name;`, llamado inmediatamente después de cada `Adopt` y de cada `SetName` (W:279, :305, :322, :1237, :3352, :3405 y :3504).
9. **La costura:** `internal PushBackEditableState EditableStateForTest => editable;`, junto a las demás.

**Fuera, por la letra:** `ReadCellValues`, `CommitCurrentCell`, `ForEachEditedSide`, `Keep`, `KeepCell`, `KeepOptional`, `MixedBoth`, `OptionalVal`, `IntVal`, el estado mixto, los manejadores, los subdiálogos, `RequestDraw` (salvo `SyncName`), `SaveLibrary_Click` e `InsertBatch_Click` (salvo `SyncName`), WX y lo demás.

### D-PBT1-14. Lo que no cambia en todo PB-T1

- **Los constructores y las llamadas de los cuatro sitios** (OB-01.1b): W:131-161, el Plugin y `EditorModules`.
- **Las firmas públicas** de `LoadExisting`, `LoadDesignForNew` y `LoadNew` (W:270, :294, :311): el Plugin sigue llamando a `window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)` (T-PG CT-25).
- **El modo WORKSPACE no se añade** (PB-T2).
- **El piloto no se toca** (OB-03.1): `Pilot/` entero.
- **`NumericField`, Cantilever, Domain, el Plugin, WX, `Workspace/` y `Drafts/`.**
- **Ninguna corrección de producto** (OB-RE.1, D-16).

### D-PBT1-15. La decisión D2 del Owner: las dos referencias compartidas, sus diez condiciones y su límite

**El texto del Owner** (F4B-E11, literal):

> The PB-T1 copied state may retain shared references ONLY for:
>
> 1. resolved rack;
> 2. module report.
>
> Everything else governed by the extracted editable state must satisfy the
> normal no-aliasing requirement.

**Cómo se reparte la prueba.** PB-T1 no tiene `DraftSession`, `InitialDraftState`, `CurrentDraftState` ni la operación de texto pendiente del Workspace: son de PB-T2 y PB-T4. Por eso cada condición se prueba **en todo lo que el API de PB-T1 ya permite** (editar el estado, validar, descartar o recargar, y comparar) y lo demás se arrastra como obligación de contrato, con la parada del límite.

**La tabla de las condiciones:**

| # | Condición (literal) | Subtarea | Prueba, guarda o arrastre |
|---|---|---|---|
| 1 | «the exact two shared-reference types are enumerated» | PB-T1b | **Los dos tipos y los cuatro miembros** (abajo). CT-PBT1B-06: la clase **D** del censo contiene exactamente `PushBackEditorState.workingBaseline` y `PushBackEditorState.LastModuleReconciliation`, de tipo `PushBackSystem` y `RackModuleReconciliationResult`. CT-PBT1B-07, parte 1: las raíces de lo compartido son exactamente esos cuatro objetos (dos por lado) |
| 2 | «no other mutable component of editable state aliases the source» | PB-T1b | CT-PBT1B-07, partes 1 a 3 (con `IsImmutable` y el recorrido ampliado a los diccionarios): fuera de las dos referencias, nada mutable compartido con el original ni con el diseño cargado |
| 3 | «DraftSession editing does not mutate either shared object» | PB-T1b, PB-T1c; **arrastre a PB-T4** | **PB-T1b,** CT-PBT1B-16, bloque «editar»: todas las operaciones de edición del API (abajo) en el original y en la copia. **PB-T1c,** CT-PBT1C-02, parte 2: cada gesto del censo en la ventana. **PB-T4** (que conecta el `DraftSession` al estado): su contrato prueba que editar, aplicar al borrador y fijar el estado actual no mutan las dos referencias del estado inicial ni del actual, con la parada C17 |
| 4 | «validation does not mutate either shared object» | PB-T1b, PB-T1c; **arrastre a PB-T2 y PB-T4** | **PB-T1b,** CT-PBT1B-16, bloque «validar»: `PushBackEditorOutputGate` (las tres), `IntentDiagnostics`, una `Pasada` con una entrada inválida (el refuerzo de F-03, que lanza en `ValidateReinforcementAgainstPost`), `SupportOf` y `ResolvePostCatalogId`. **PB-T1c,** CT-PBT1C-02, parte 2: un campo en error y un cálculo inválido en la ventana. **PB-T2** (la validación única de campos, [OD]) y **PB-T4** (la validación de la frontera del borrador): su contrato lo prueba sobre su validación, con C17 |
| 5 | «pending-text/edit operations do not mutate either shared object» | PB-T1b, PB-T1c; **arrastre a PB-T4** | **Edición:** como la condición 3. **Texto pendiente:** PB-T1 no tiene ese concepto; en la ventana, PB-T1c cubre una caja escrita sin salir de ella y una ventana cerrada sin pedir (CT-PBT1C-02, parte 2). **PB-T4** (`IDraftPendingTextAdapter` y la frontera de F4): su contrato prueba que confirmar o rechazar el texto pendiente no muta las dos referencias, con C17 |
| 6 | «discard/reload does not mutate either shared object» | PB-T1b; **arrastre a PB-T4** | **PB-T1b,** CT-PBT1B-16, bloque «descartar y recargar»: `LoadDesign`, `Restore` y `LoadNew` sobre la copia y sobre el original, y una copia que se descarta, sin cambio en las dos referencias del otro estado. **PB-T4** (descartar, recargar, «Conservar mi borrador» y la nueva base del `DraftSession`): su contrato lo prueba, con C17 |
| 7 | «comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects» | PB-T1b; **arrastre a PB-T4** | **PB-T1b,** CT-PBT1B-16, bloque «comparar»: (i) `Equals` y `GetHashCode`, en los dos sentidos, no mutan nada; (ii) el comparador solo lee de la línea base la **proyección declarada** (la intención de módulos, D-PBT1-06) y nada del informe; (iii) **con dos estados que comparten la misma línea base**, el resultado no cambia si se muta el objeto compartido, ni fuera de la proyección (`Name`) ni dentro (la `Length` de un módulo). **PB-T4** (su `IsDirty` usa este comparador sobre el estado inicial y el actual del borrador): su contrato lo prueba con su `DraftSession`, con C17. Lectura L-4 del §8.3 |
| 8 | «the current permitted post-window name adjustment remains outside the draft editing lifetime or is otherwise explicitly controlled» | PB-T1b (guarda), PB-T1c (declaración); **arrastre a PB-T3 y PB-T4** | **Abajo, «La condición 8».** CT-PBT1B-15, la guarda de los tres escritores de `Name` del Plugin. CT-PBT1B-16 y CT-PBT1C-02: `Name` de las dos referencias no cambia en ningún gesto del editor |
| 9 | «a test proves that mutating all ordinary editable state in the copy does not change the source» | PB-T1b | **CT-PBT1B-08, reescrito** (abajo, «La condición 9») |
| 10 | «source guards/tests identify any future added mutation of resolved rack or module report inside the draft/editing path» | PB-T1b, PB-T1c; **arrastre a PB-T2, PB-T3 y PB-T4** | **La guarda G-L1** (abajo, «La condición 10»): CT-PBT1B-14 sobre Application y CT-PBT1C-04 sobre W y WC, cada una con sus autopruebas en negativo. Cada tarea posterior **añade a la lista de archivos de G-L1** los suyos del camino de edición y del borrador, con una prueba propia y C17 |

**La condición 1: los dos tipos y los cuatro miembros.**
- **Tipo 1, «resolved rack»:** `RackCad.Domain.Systems.PushBack.PushBackSystem`. Lo contiene `PushBackEditorState.workingBaseline` (ES:65; expuesto por `WorkingBaseline`, ES:71).
- **Tipo 2, «module report»:** `RackCad.Application.Systems.Shared.RackModuleReconciliationResult` (`RackModuleReconciliation.cs:16`). Lo contiene la propiedad automática `PushBackEditorState.LastModuleReconciliation` (ES:143; su campo de respaldo `<LastModuleReconciliation>k__BackingField`).
- **Los cuatro miembros de un `PushBackEditableState`:** `Editor.SideA.workingBaseline`, `Editor.SideA.LastModuleReconciliation`, `Editor.SideB.workingBaseline` y `Editor.SideB.LastModuleReconciliation`.
- **Nada más se comparte:** ni la sesión, ni el commit, ni la cola (aunque `ParkTailOf` la aparque desde la línea base, HT1-12), ni ningún portador.
- **El informe, medido** (registro N-9): cinco listas `IReadOnlyList<string>` de solo lectura (`Preserved`, `Adapted`, `Removed`, `Incompatible` y `Restored`) con `List<string>` detrás y un constructor `internal`. Se crea solo en `Reconcile` (`RackModuleReconciliation.cs:212`) y se asigna solo en DA:325; ES:105 lo pone a nulo. En la ventana lo lee solo W:2559 (`Describe()`).

**La condición 8: el ajuste de nombre del Plugin.**
- **Lo medido** (M-1): el Plugin escribe `Name` del sistema resuelto en tres sitios, **después** de que la ventana se cierra: `RackPushBackCommands.cs:98` (`system.Name = rackName;`), `RackPushBackCommands.cs:190` (`system.Name = name;`) y `RackMenuCommands.cs:135` (`if (pushBack.System != null) pushBack.System.Name = pushBackName;`).
- **PB-T1a:** no crea estado ni toca el Plugin; lo declara en su contrato.
- **PB-T1b, CT-PBT1B-15** (`PushBackSharedReferenceGuardTests`, Core, texto sin comentarios), **medido sobre `b5c09f0b`** (registro N-11):
  - **el censo:** en los archivos `.cs` de `src/RackCad.Plugin/` que contienen `PushBack` (14 hoy), las líneas que casan con `(?<![\w.])\w+(?:\s*\.\s*\w+)*\s*\.\s*Name\s*=(?![=>])` son **exactamente estas siete, literales**:
    - `RackMenuCommands.cs`: `header.Configuration.Name = …` (`:64`), `dynamic.System.Name = dynamicName;` (`:80`), `selective.System.Name = selectiveName;` (`:102`), `cantilever.Design.Name = cantileverName;` (`:121`) y `pushBack.System.Name = pushBackName;` (`:135`);
    - `RackPushBackCommands.cs`: `system.Name = rackName;` (`:98`) y `system.Name = name;` (`:190`).

    **Tres son de Push Back.** Las otras cuatro son de otros sistemas en el mismo lote del menú. La guarda las fija para que un escritor nuevo en esos archivos no pase sin verse. La regex estrecha de la v2.2 sobre todo el Plugin daba 10 líneas (siete de otros sistemas) y habría fallado en el RED;
  - **el orden** (las anclas textuales, medidas):
    - en `RackPushBackCommands.cs`, `AcApplication.ShowModalWindow(window);` (`:48`) precede a `DrawPushBackView(` (`:57`), que es su **única** llamada en `src/` y contiene `:98`;
    - en `EditPushBack`, `AcApplication.ShowModalWindow(window);` (`:170`) precede a `:190`;
    - en `RackMenuCommands.cs`, `RackCad()`, `AcApplication.ShowModalWindow(menu);` (`:34`) precede a `:135`.

    La guarda comprueba, por posición en el texto de cada método, que cada escritor va después de su `ShowModalWindow`, y que `DrawPushBackView(` aparece una sola vez fuera de su declaración;
  - **las autopruebas en negativo:** una octava asignación inyectada (`state.Editor.SideA.WorkingBaseline.Name = "x";`) en `RackPushBackCommands.cs`, y `:190` movida antes de `:170`, hacen fallar la guarda.
- **El resultado de la medición:** no hay un cuarto escritor de Push Back, y los tres van después de que la ventana vuelve. **La premisa de D2 se sostiene.**
- **PB-T1c:** no toca el Plugin (INV-PBT1C-01). CT-PBT1C-02, parte 2: `Name` de las dos referencias no cambia en ningún gesto de la ventana.
- **Arrastre a PB-T3 y PB-T4** (A-2): el escritor del panel (PREPARE, MUTATE y POST) no puede ajustar `Name` sobre un sistema que comparte un estado del borrador mientras el borrador vive; lo hace sobre una copia o fuera de esa vida, y su contrato lo prueba. Si no puede: C17.

**La condición 9: CT-PBT1B-08, reescrito.**
- **Sobre:** el estado rico de CT-PBT1B-06 (F-01 con una edición escenificada y un commit; `pb-20-ab` con B retirado y la cola aparcada) y `pb-06-seguridad`.
- **Qué se muta en la copia, todo el estado editable ordinario:**
  - **cada ranura de clase E** del censo, con el `Mutator` del censo del piloto (`WorkspaceDraftPilotStateTests.cs`, `Mutator.TryMutate`, copiado);
  - **la autoridad copiada (clase A), por su API:** escenificar una longitud y una cabecera en la sesión, confirmar (`CommitModuleEdits`), cancelar, `ParkDormantTail` con una cola nueva y `ClearDormantTail`;
  - **la intención compuesta y la selección,** por el API de CS: `SetGap`, `SetCentralSeparator`, `SetStructureOverride`, `ApplyTopology`, `SetSideBPresent` (ida y vuelta), `SetSlotCount`, `SetSlotPresent` y `SetActiveSelection`;
  - **los portadores:** cada propiedad de D-PBT1-02, y `SafetySelections` con un elemento añadido y otro cambiado.
- **Qué se comprueba en el original,** tras cada mutación: igual, con el comparador, a una copia tomada antes; el diseño de una `Pasada` sobre una copia del original, igual al de antes; y el conjunto de objetos del original (el recorrido de CT-07) intacto. **Y al revés:** mutar el original deja la copia igual.

**La condición 10: la guarda G-L1.**
- **Qué detecta:** una mutación nueva del rack resuelto o del informe de módulos dentro del camino de edición.
- **Las fichas** (las expresiones que nombran una de las dos referencias): `WorkingBaseline`, `workingBaseline`, `LastModuleReconciliation`, `computation.System`, `lastComputation.System`, `session.System`, `lastCompositeSystem` y `SystemToInsert`.
- **Los archivos:**
  - **CT-PBT1B-14** (`PushBackSharedReferenceGuardTests`): los cuatro archivos nuevos de PB-T1b; ES; EL; CS; CA; DA; `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs`; `src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs`; y todo `src/RackCad.Application/Workspace/` (recursivo, por si PB-T2 o PB-T4 añaden ahí el camino del borrador);
  - **CT-PBT1C-04** (`PushBackWindowSharedReferenceGuardTests`): W y WC.
- **Las reglas,** sobre el texto sin comentarios y con los espacios colapsados (como T-SG):
  1. **ninguna mutación a través de una ficha:** ninguna coincidencia de `<ficha>(\s*\??\.\s*\w+(\s*\([^()]*\))?)*\s*\??\.\s*\w+\s*(=(?!=)|\+=|-=|\*=|/=|\+\+|--)` ni de `<ficha>(\s*\??\.\s*\w+(\s*\([^()]*\))?)*\s*\??\.\s*(Add|AddRange|Clear|Remove|RemoveAt|RemoveAll|RemoveRange|Insert|InsertRange|Sort|Reverse)\s*\(`;
  1b. **ningún molde de una ficha a una colección mutable** (V23-11): ninguna coincidencia de `\(\s*\(\s*(List|IList|ICollection|Collection|HashSet|Dictionary|IDictionary)\s*<[^>]*>\s*\)[^;]*?<ficha>`. Las listas del informe son `IReadOnlyList<string>`, así que una mutación que compile necesita un molde;
  2. **un nivel de alias:** si una variable local se asigna desde una expresión con una ficha (`var <id> = …<ficha>…;` o `<id> = …<ficha>…;`), **o es la variable de un `foreach` sobre una expresión con una ficha** (`foreach (var <id> in …<ficha>…)`, V23-11), ese `<id>` cuenta como ficha para la regla 1 en el resto del miembro;
  3. **lo que sí se permite:** volver a apuntar el campo (`workingBaseline = …`, `LastModuleReconciliation = …`, `lastComputation = …`, `lastCompositeSystem = …`), leer, y pasar la referencia como argumento.
- **Las autopruebas en negativo,** que hacen fallar la guarda sobre el texto real con una línea inyectada:
  - (a) `state.WorkingBaseline.Structure.Name = "x";` (regla 1);
  - (b) `var b = state.WorkingBaseline; b.Structure.Modules.Clear();` (regla 2);
  - (c) `((List<string>)side.LastModuleReconciliation.Preserved).Add("x");` (regla 1b; es la forma compilable, con el miembro `Preserved` medido);
  - (e) `foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }` (regla 2 con el `foreach`);
  - y, en CT-PBT1C-04, (d) `lastComputation.System.Name = "x";` (regla 1).

  Medido sobre un texto neutro (registro N-9): cada una salta exactamente por la regla indicada.
- **Lo que no ve:** la mutación dentro de un método llamado en otro archivo. La cubren, para los caminos ejercidos, las huellas profundas de CT-PBT1B-16 y CT-PBT1C-02.
- **Pasan en el RED** (no hay mutación hoy) y discriminan por sus autopruebas, como las guardas de F4-T3A (INV-F4T3A-13: «los arrastres que solo tocan guardas pueden pasar en el RED, y demuestran que discriminan con autopruebas de mutación»). Por eso sus filtros van con `ExpectRed` false.
- **Medido sobre `b5c09f0b`** (registro N-9), en los 47 archivos que ya existen de la lista de CT-PBT1B-14 (997 miembros) y en W y WC (320): las reglas 1, 1b y 2 (con el `foreach`) dan **0 coincidencias**. Los alias que encuentra la regla 2 (`structure`, `signature`, `baseline`, `rebuildShape`, `outputIsBlocked` y `reconciliation`) solo se leen. **Hoy no hay ninguna mutación del rack resuelto ni del informe en el camino de edición; la premisa de D2 se sostiene.**

**El límite** (F4B-E11, literal):

> This is NOT authorization for general shallow copying.
>
> It is exactly:
>
> two declared shared references.
>
> If later PB-T1/PB-T2/PB-T3 needs to mutate either:
>
> * resolved rack;
> * module report;
>
> inside the Workspace draft lifetime:
>
> STOP and reopen L-1.
>
> At that point the object must either:
>
> * be cloned;
> * be made immutable;
> * or be replaced by a value snapshot appropriate to the draft model.

Va como la parada **PBT1x-C17** en los tres contratos (§6.0) y como el arrastre A-2 (§10).

## 4. Las guardas de T1B y de texto (OB-T1B.3)

**Conclusión:** el diseño mantiene en verde todas las guardas sin sustituir ninguna. OB-T1B.3 no obliga a volver al Owner.

| Guarda | Qué fija | ¿Depende del diseño? | Cómo queda en verde |
|---|---|---|---|
| **T-SG S-9** (`…SourceGuardTests.cs:293-298`) | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` | Sí | `SaveLibrary_Click` solo gana `SyncName();` tras `SetName` (INV-PBT1C-04) |
| **T-SG CT-22** (`:323-341`, por índices) | El orden de las cinco sentencias de la biblioteca | Sí | `session`, `lastComputation` y `sourceProject` siguen en W; las cinco sentencias no cambian (INV-PBT1C-04) |
| **T-PG CT-25** (`PushBackClassicCharacterizationPluginGuardTests.cs:235-272`) | La carga del Plugin con `window.LoadExisting(...)` | Sí | El Plugin no se toca y la firma no cambia |
| T-SG S-7 (`:280-284`) | `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();` | No | `Bom_Click` no cambia |
| T-SG CT-23 (`:356-379`) | El orden de `InsertBatch_Click` | Débil | Solo gana `SyncName();` tras su `SetName` (W:3352) |
| T-SG CT-21 (`:302-319`) | El host del menú | No | `EditorModules` no se toca |
| T-SG CT-16, S-5, S-6, S-8 y el configurador | Los subdiálogos | No | No se tocan |
| T-SG CT-19 e `InsertRequestedRaised` (`:235-264`) | Ningún token de teclado, foco ni `DialogResult` bajo `src/RackCad.UI/Systems/PushBack` | Débil | El diff de PB-T1c no los añade (INV-PBT1C-05) |
| **G-10** (`WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`) | Una sola `.RequestUpdate(` en W | Sí | Ningún archivo nuevo de `src` la contiene (INV-PBT1B-05, INV-PBT1C-05) |
| I-55 (`I55DeferredProductFixCharacterizationTests.cs:8-15`) | `PhysicalPostIndex(LateralSectionBox.SelectedItem)` y `corte.PostIndex == section` | No | No se tocan |
| Fact3 (`PushBackModuleEditorCharacterizationTests.cs:381-401`) | W sin `forceRebuild:` ni `forceRebuild)`, en crudo; DA con `StandardRestoreRequested` | Débil | Ninguna línea nueva de W contiene `forceRebuild`; DA no se toca |
| Desviador (`BlankFrontDesviadorHandoffTests.cs:311-318`) | Dos literales de W | No | La vista previa no se toca |
| **`PushBackRichPreviewTests.cs:348-367`** (M-13) | Por reflexión: el método `DrawSharedPreview`, un campo `EditorPreviewSurface`, y sin `DrawModel`, `RoleBrush` ni un campo `PreviewCanvasPainter` | Débil | Los reenvíos son propiedades, y el campo nuevo es de tipo `PushBackEditableState` (M-10 del registro) |
| T-KB, T-OW, T-RS, foco y `AnnotationScaleBox` | WX | No | WX no se toca |
| Censo de ventanas (`WindowCensusGuardTests.cs:86-120`) | Ningún `Window` nuevo | No | Ninguno |
| Corpus de WLC (`WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:323-333`) | Ningún `pb-*.json` nuevo en `Fixtures/I64` | Restricción | Los diseños nuevos se fabrican en código |
| RAM Elements (`StructuralAnalysisRamElementsHardeningGuardTests.cs:84-92`) | Los literales de `tests/RackCad.Tests` | Restricción | Las pruebas nuevas no parten ninguna palabra de ese dominio |
| Núcleo de borradores (`WorkspaceDraftPilotIntentGuardTests.cs:232-244`) | `Drafts/` sin `PushBack` | Restricción | `Drafts/` no se toca |
| Espacio de nombres = carpeta (`NamespaceFolderGuardTests.cs`) | Un `namespace` de bloque por archivo de `src/` | Restricción | INV-PBT1B-04b |
| **El censo del piloto** (HT1-21) | Todo campo de instancia de su grafo | **Sí** | Solo métodos nuevos en ES y RMES (INV-PBT1B-04) |
| Las costuras internas (W:165-193, WC:43 y las de T-7 de PB-T0) | Las usan T1B, PB-T0, PB-TC y PB-TC2 | Sí | Se conservan las 40; se añade `EditableStateForTest` (INV-PBT1C-06) |

**Para PB-T2, informativo:** G-10, T-SG CT-23 y T-PG CT-25 son las que probablemente salten al separar aplicar, cerrar y escribir.

## 5. La red de pruebas

### 5.0 Recetas y ayudantes comunes (I-03, M-02)

**Las recetas de los diseños fabricados** (todas en código; ninguna en `Fixtures/I64`):
- **X-01 (ventana):** copia literal de `BuildX01` (`PushBackFullEditorCharacterizationStateTests.cs:90-113`): `pb-21-cola`; `SideBPresentCheck` a true; `SideSelectorBox` a 1; `EditorWindowTestSupport.SetFrontBlank(w, "PushBackMatrixGrid", 0, false)`; `LastComputation.Design`, `RackId` y `RackName`; reabierto con `LoadExisting` en otra ventana. Con `Ventana()` y `try`/`finally`.
- **X-01 (Core):** `pb-21-cola` con `LoadDesign` y `Pasada`; `Editor.SetSideBPresent(true)`; `Editor.SetActiveSelection(PushBackSideSelection.B)`; `Editor.SetSlotPresent(PushBackSide.B, 0, true)`; `Pasada`; el diseño de esa computación.
- **F-01, compuesto con módulos personalizados en A y en B:** `pb-20-ab`; en la sesión del lado A (en la ventana, con `SelectModule`, `SetNumberR(w, "ModuleLengthBox", …)` y `Click(w, "ConfirmModuleButton")`; en Core, con `Editor.SideA.ModuleSession.SetLength` y `Editor.SideA.CommitModuleEdits()` seguidos de `Pasada`), el primer módulo de `ModuleSession.Modules` de clase `Separator` sin prefijo `B:` pasa a su longitud + 2,0, y después el primero con prefijo `B:` de clase `Separator`, también + 2,0; el diseño resultante se reabre. CT-PBT1B-01 comprueba que las dos recetas dan el mismo diseño.
- **F-02:** `pb-20-ab` con `FrontalSideBox` a 1 («Frontal de B»). Solo en la ventana.
- **F-03 (I-04):** `pb-01-base` con `Structure.PalletTolerance = 3.00044` (`EditInner`); `SideBPresentCheck` a true (B declarado y sin frentes: el diseño sigue de un sentido); `FrontalSideBox` a 1; `AdvancedModulesToggle` a true, `DerivedPostReinforcedCheck` a true y `SetNumberR(w, "DerivedPostReinforcementHeightBox", 100000)` (un cálculo inválido: el refuerzo supera la cabecera, `PushBackAdvancedRackParameters.ValidateReinforcementAgainstPost`); `SetNumberR(w, "ToleranceBox", 4)`; y «Restaurar» (`Click(w, "RestoreButton")`). Solo en la ventana. **Se comprueba a sí misma (N-M-08):** `RecomputePassesForTest` se lee justo antes y justo después del clic de «Restaurar», y `FrontalSideForTest` después; lo esperado es `pasadas-restaurar=2` (la reentrante de `UpdateCompositeEnabled` y la de `LoadFromModel`) y `frontal=A`. Otro valor quiere decir que el guion no ejerce el camino de I-04, y es PBT1A-C07.

**El ayudante `Pasada`** (privado de cada clase que lo use; copia literal de W:659-704, adaptada al estado):

```csharp
private static PushBackEditorComputation Pasada(
    PushBackEditableState s, PushBackEditorDesignAssembler a, PushBackCompositeEditorAssembler ca, PushBackSide frontalSide)
{
    PushBackEditorComputation computation;
    try { computation = a.BuildFrom(ca.BuildDesign(s.Editor, s.BuildInputs(a)), frontalSide); }      // W:659-660
    catch (Exception) { return null; }                                                               // W:662-670
    if (!computation.IsValid) return computation;                                                    // W:714-719
    a.AcceptComputation(s.Editor.SideA, computation);                                                // W:675
    s.Editor.CommitDormantTailConsumption();                                                         // W:679
    var standardRestoreLanded = s.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;      // W:696
    s.Editor.SideA.ClearModuleCommit();                                                              // W:697
    if (standardRestoreLanded) s.Editor.Active.RestoreAdvancedRackParameters(s.Advanced);           // W:698-702
    _ = s.Editor.SideA.ModuleSession;                                                                // W:704 -> W:1688
    return computation;
}
```

`a` y `ca` son dos instancias nuevas con el mismo catálogo, como W:145-146. En la UI, el catálogo es `w.Session.Catalog`; en Core, `JsonRackCatalogProvider.FromBaseDirectory().Load()`.

**El número de pasadas de cada camino** (las de la ventana, que el estado reproduce en este orden):

| Camino | Secuencia |
|---|---|
| Ventana nueva | `LoadNew(catalog)`; `Pasada`; `SeedDefaultSafety(catalog)`; `Pasada` (W:280-288) |
| `LoadExisting` o `LoadDesignForNew` | la ventana nueva y después `LoadDesign(…)`; `Pasada` (W:439) |
| «Restaurar» | lo anterior; `Restore(<diseño de la última computación válida>, resolver)`; `Pasada` |
| «Actualizar» | lo anterior; `Pasada` (W:3391) |

**La regla «EXCEPCION»** (PB-T1b y PB-T1c): cada paso que llama al API nuevo pasa por un ayudante privado que convierte una excepción en la línea `EXCEPCION <Tipo>: <Mensaje>`. Las líneas esperadas no contienen ninguna.

### 5.1 Lo que tiene que seguir en verde sin tocarlo (las tres subtareas)

T1B (55 y 10), PB-T0 (16 y 3, con SUS-03 y SUS-04 en `Skip`), PB-TC (10), PB-TC2 (4), todas las clases `PushBack*` de UI (542 sobre `b5c09f0b`, con las 4 de PB-TC2; registro N-6) y las de Core que leen la ventana, `RackCad.Tests.Workspace` entero (con el censo del piloto), Cantilever (67) y, una vez cerradas, PB-T1a y PB-T1b.

### 5.2 PB-T1a: la red de caracterización (lista cerrada CT-PBT1A-01 a 14)

**Régimen** (el de PB-T0 y T1B, copiado en el contrato):
- se fija lo que hay; las líneas esperadas **las mide el Worker en la `BaseSha`** con código temporal (paso 1) y las transcribe; si algo contradice el diseño, la línea lleva «INCUMPLIMIENTO CARACTERIZADO»;
- el RED son las pruebas nuevas y los quince mutantes, juntos, en W y WC; el GREEN los revierte con `git restore --source=<BaseSha>`. Cada mutante, aislado, hace fallar por aserción el CT de su fila;
- ninguna prueba existente se edita, se reactiva, se salta ni se borra; las existentes que fallen en el RED se atribuyen a un mutante (tabla T-8). Tras PB-TC2, `Keep` lo usan también el hueco, la longitud del módulo y `KeepCell`, así que MU-T1A-07a puede hacer fallar también pruebas de PB-TC2 (§2.8);
- las corridas temporales del paso 1 también van con el aviso de descarte sustituido (lección IRC-M02 del cierre de PB-TC2);
- ventanas con `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`; ninguna mostrada salvo la de CT-11 (M-03);
- el aviso de descarte, sustituido, con el sufijo ` descartes=<n>` en la última línea de cada ventana, leído tras cerrarla (D-PBTC2-08);
- sin teclado ni foco real; sin `MessageBox` real, BOM, «Guardar en biblioteca», configurador real ni «Insertar»;
- «R», cultura invariante, GUID fijo, orden ordinal; los JSON con la navegación de SC1.

**Las clases** (`tests/RackCad.UI.Tests/`, `namespace RackCad.UI.Tests`, `public sealed`, `[Collection(EditorDiscardPromptCollection.Name)]`, `[Fact]` que acaban en **un** `Assert.Equal`): `PushBackStateNetRackControlsTests` (01, 02), `PushBackStateNetCellAndFrontTests` (03, 04, 07, 08), `PushBackStateNetCompositeTests` (05, 09, 10), `PushBackStateNetModuleAndSafetyTests` (06, 11), `PushBackStateNetActionsTests` (12, 13) y `PushBackStateNetFingerprintTests` (14). El ayudante `PushBackStateNetSupport` (`internal static`, sin pruebas) solo tiene las recetas del §5.0 y las huellas.

| CT | Qué fija (sobre la salida persistida, nunca sobre el estado privado) | Origen | Mutante |
|---|---|---|---|
| **01** | Las tres casillas de anotación, `DimensionsBox` y las tres de cotas, cada una sola, en la salida; un `DimensionViews` nulo sigue nulo sin tocarlas, y tocar una escribe el valor de I-50 | D-5 | MU-T1A-01 |
| **02** | `WeightUnitBox` a «lb»; la carga con «ton» (c-09) y su Actualizar; `PalletsDeepBox` editado | D-5 | MU-T1A-02 |
| **03** | `PositionsBox`, `LevelsBox`, `FondosBox` y `DepthStartBox` en un frente; lo mismo con «Ambos lados» en un compuesto (`Structure.Fronts` y `SideB.Fronts`) | D-5 | MU-T1A-03 [TC2] |
| **04** | `CellIntermediatePeralteBox`, `CellFondoOverrideBox` y `CellDrawPalletCheck` en la celda de la salida | D-5 | MU-T1A-04 [TC2] |
| **05** | `CentralSeparatorCheck`, `StructureOverrideBox` con «Aplicar» y «Restaurar estructura», la topología, el sentido y el alcance, y `FrontCountBox` | D-5 | MU-T1A-05 |
| **06** | `DerivedPostLineHeightBox` en una línea: nada hasta «Confirmar»; después, `Structure.DerivedPostLineOverrides`; cancelar no deja nada | D-5 | MU-T1A-06 |
| **07** | **M-03 de PB-TC:** un valor exacto sin editar de la celda (61,23456) llega exacto por el alcance «Todas», por «Aplicar a todos los frentes» y tras «Restaurar»; **la ruta de los portadores del rack (M-04):** con `PalletTolerance = 3.00044` y `AnnotationScale = 1.23456`, esos valores siguen exactos tras el alcance, los datos del frente, un cambio de celda y «Restaurar»; y **el caso negativo de D-PB-11:** «Restaurar módulo» individual con «Confirmar» no reinicia los avanzados | M-03 de PB-TC | MU-T1A-07a [TC2], MU-T1A-07b |
| **08** | **M-03 de PB-TC2:** con B presente y «Ambos», una caja mixta escrita llega a los dos lados por el alcance (W:1346) y por los datos del frente (W:1510); sin escribirla, cada lado conserva el suyo. Es el arrastre del cierre de PB-TC2 a esta red («Ambos lados» en el alcance y en los datos del frente, sin prueba propia; V23-17) | M-03 de PB-TC2 | MU-T1A-08 |
| **09** | **M-04:** «+ frente», «- frente» y «En blanco», con y sin B: `Structure.Fronts.Count`, `SideB.Fronts.Count`, `Composite.AbsentSlotsA` y `Composite.AbsentSlotsB` | M-04 de PB-T0 | MU-T1A-09 |
| **10** | **M-07, INCUMPLIMIENTO CARACTERIZADO:** el camino de H2-11 y después vaciar «Largo manual»: la salida conserva el override; una caja obligatoria vacía no marca error. La rama vacía de `OptionalVal` (W:3666) no cambió con PB-TC2; es el M-07 que su cierre arrastra a esta red | M-07 de PB-TC2 | MU-T1A-10 [TC2] |
| **11** | **La observación de CT-16:** aceptar Seguridad sin cambios sobre `pb-05-topes`: `Structure.SafetySelections` y las líneas del BOM antes y después. **La ventana (M-03):** `var w = Ventana(); try { w.WindowStartupLocation = WindowStartupLocation.Manual; w.ShowInTaskbar = false; w.ShowActivated = false; w.Left = -10000; w.Top = -10000; Cargar(w, t); w.Show(); w.UpdateLayout(); … } finally { Cerrar(w); }`, la preparación de `AbrirFueraDePantalla` (S0:139-154) con la carga dentro del `try` | CT-16 de PB-T0 | MU-T1A-11 |
| **12** | **M-02 de PB-TC:** `IsEnabled` de los once botones de acción con el aviso de D-PB-08 visible y sin él | M-02 de PB-TC | MU-T1A-12 |
| **13** | **D-3:** `LoadExisting` con `PalletTolerance = 0`: el tipo y el mensaje de la excepción, y que la ventana se cierra en el `finally` | D-3 | MU-T1A-13 |
| **14** | **Las huellas de la ventana.** El corpus extendido: los 17 de `Fixtures/I64`, X-01, F-01, F-02 y **F-03**. Por diseño y guion, la línea `<diseño> <guion> valido=<b> bloqueado=<b> sha256=<huella de Persist(LastComputation.Design)>`. **Los guiones, cada uno sobre una ventana nueva y en este orden (M-13):** G-1 cargar; G-2 cargar y «Actualizar» (la huella es la de `DesignToInsert`; la ventana se cierra); G-3 cargar y «Restaurar»; G-4 cargar, retirar B y volver a declararlo (solo compuestos); G-5 cargar, «Restaurar estándar» y «Confirmar»; G-6 cargar, escribir 61.5 en `CellPalletHeightBox` y aplicar a «Todas». **F-03 tiene un solo guion,** el suyo (§5.0), con su línea `F-03 G-F03 valido=<b> bloqueado=<b> sha256=<huella> PalletTolerance=<R> pasadas-restaurar=<n> frontal=<lado>`, con `pasadas-restaurar=2` y `frontal=A` esperados (N-M-08) | La equivalencia de PB-T1c | MU-T1A-14 |

**La tabla cerrada de mutantes** (W y WC; sustitución exacta de una línea en la `BaseSha`). **Remedida sobre `b5c09f0b`** (registro N-2): las quince líneas tienen el texto de abajo en la línea de abajo; MU-T1A-07a y MU-T1A-10 se movieron −2. El generador del contrato lo vuelve a comprobar sobre HEAD antes de escribir. Si en la `BaseSha` el texto no es el de abajo, es PBT1A-C07.

| Id | Miembro | Línea de hoy | Sustitución |
|---|---|---|---|
| MU-T1A-01 | `ReadInputs` | W:874 `NumberFronts = NumberFrontsCheck.IsChecked == true,` | `NumberFronts = NumberLevelsCheck.IsChecked == true,` |
| MU-T1A-02 | `ReadInputs` | W:852 `generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),` | `generalPallet.Weight, "kg"),` |
| MU-T1A-03 [TC2] | `ReadCellValues` | W:817 `DepthStartPosition = IntVal(DepthStartBox, front.DepthStartPosition),` | `DepthStartPosition = front.DepthStartPosition,` |
| MU-T1A-04 [TC2] | `ReadCellValues` | W:827 `IntermediateBeamDepth = SelectedPeralte(CellIntermediatePeralteBox, cell.IntermediateBeamDepth)` | `IntermediateBeamDepth = cell.IntermediateBeamDepth` |
| MU-T1A-05 | `CentralSeparator_Changed` | WC:209 `composite.SetCentralSeparator(CentralSeparatorCheck.IsChecked == true);` | `composite.SetCentralSeparator(false);` |
| MU-T1A-06 | `ApplyDerivedPostLines_Click` | W:2133 `var height = DerivedPostLineHeightBox.Value;` | `var height = (double?)null;` |
| MU-T1A-07a [TC2] | `Keep` | W:3684 (era W:3686) `=> !field.Value.HasValue \|\| SameShown(field.Value.Value, current) ? current : field.Value.Value;` | `=> !field.Value.HasValue ? current : field.Value.Value;` |
| MU-T1A-07b | `Recompute` | W:696 `var standardRestoreLanded = moduleOwner.ModuleCommit?.StandardRestoreRequested == true; // I-64 F4B PB-TC (D-PB-11)` | `var standardRestoreLanded = moduleOwner.ModuleCommit != null;` |
| MU-T1A-08 | `ApplyFrontData` | W:1510 `ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues(side).Dynamic, frontTargets));` | `ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues().Dynamic, frontTargets));` |
| MU-T1A-09 | `AddFront_Click` | W:1171 `=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 1));` | `=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 2));` |
| MU-T1A-10 [TC2] | `OptionalVal` | W:3666 (era W:3668) `: (IsMixed(field) ? sideValue : null);` | `: null;` |
| MU-T1A-11 | `Safety_Click` | W:2682 `ApplyBootSections(bootSectionA, bootSectionB);` | `;` |
| MU-T1A-12 | `UpdateButtons` | W:3568 `BomButton.IsEnabled = canOutput;` | `BomButton.IsEnabled = canOutput && SideBEmptyNoticeText.Visibility != Visibility.Visible;` |
| MU-T1A-13 | `LoadExisting` | W:313 `if (design == null) return;` | `if (design == null \|\| design.Structure?.PalletTolerance <= 0.0) return;` |
| MU-T1A-14 | `LoadFromModel` | W:419 `rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;` | `rackAnnotationScale = 1.0;` |

Las sangrías son las de la línea original. **MU-T1A-13 se ancla por línea y miembro:** el mismo texto está también en W:296 (`LoadDesignForNew`), igual que en `eedb069c`, y esa línea no se toca. Ningún mutante toca un literal de I-55, Fact3, el desviador, G-10 ni T-SG: Core sigue en `success` en el RED. MU-T1A-14 discrimina por `pb-08-redondeo` (`AnnotationScale` 1,23456), que está en el corpus.

### 5.3 PB-T1b: el tipo (lista cerrada CT-PBT1B-01 a 16)

**Régimen** (el de F4-T2A): **el RED** es el **esqueleto inerte** y las pruebas nuevas, que fallan **por aserción**, salvo las dos guardas de D2 (CT-PBT1B-14 y 15), que pasan en el RED y discriminan por sus autopruebas en negativo (INV-F4T3A-13); **el GREEN** solo toca `src/`; **la ventana no cambia**; la regla «EXCEPCION» del §5.0.

**El esqueleto exacto:**
- `PushBackEditableState`: todos los miembros con sus valores iniciales; `LoadNewEditor`, `LoadDesignEditor`, `RestoreEditor`, `LoadNew`, `LoadDesign` y `Restore` devuelven `PushBackEditorInputs.NewDesign()` sin hacer nada más; `AdoptCarriers` y `SeedDefaultSafety` no hacen nada; `BuildInputs` devuelve `PushBackEditorInputs.NewDesign()`; `ResolvePostCatalogId` devuelve `requested`; `SupportOf` devuelve `Supported`; `SideAOnly` devuelve `design`;
- `PushBackEditableStateComparer`: `Instance`; `Equals` = `ReferenceEquals(x, y)`; `GetHashCode` = 0; `Copy` = `new PushBackEditableState()`;
- `PushBackEditableStateOwnership`: `Table` vacía; `Normalize` devuelve su argumento; `Of` devuelve `Unclassified`;
- `PushBackEditorOutputGate`: `Diagnostics` vacía; `IsBlocked` = false; `StatusOr` = `fallback`;
- ES `CopyAuthorityFrom` no hace nada; RMES `Copy()` = `RackModuleEditSession.Begin(Array.Empty<DynamicRackModuleDesign>())`; `SameEditAs` = false; `RackModuleCommit.Copy()` = `this`; `SameAs` = false.

**Las clases:** en `tests/RackCad.Tests/` (`namespace RackCad.Tests`): `PushBackEditableStateLoadTests` (02 a 05), `PushBackEditableStateCopyTests` (06 a 10, 13 y 16), `PushBackEditableStateOwnershipTests` (11), `PushBackEditorOutputGateTests` (12) y `PushBackSharedReferenceGuardTests` (14 y 15); en `tests/RackCad.UI.Tests/` (`namespace RackCad.UI.Tests`): `PushBackEditableStateParityTests` (01), en `EditorDiscardPromptCollection`.

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** (UI) | **La paridad de las cargas** con la ventana de la base, con el ayudante `Pasada` y las secuencias del §5.0, sobre los 17 de `Fixtures/I64`, X-01 y F-01: ventana nueva; `LoadExisting`; `LoadDesignForNew`; «Restaurar» tras cargar, tras retirar B y tras `SetGap(60)`; «Actualizar». Por camino, `<diseño> <camino> iguales=<b> bloqueado-ventana=<b> bloqueado-estado=<b>`, comparando `Persist` de la ventana y del estado, y `OutputIsBlockedForTest` con `PushBackEditorOutputGate.IsBlocked`. Incluye los casos c-01 a c-16 de CT-PB0-17, **los cuatro bloqueantes de CT-12** (M-07) con su `StatusText` frente a `StatusOr`, y las dos recetas de X-01 y de F-01 (ventana y Core). **Cómo se producen los cuatro bloqueantes (N-M-04),** porque dos no se alcanzan con los controles (`GapBox` tiene `Minimum="0"`, WX:148, y `Gap_Changed` no escribe con error, WC:192-195; `StructureOverrideBox` tiene `Minimum="2"`, WX:235) y el hueco negativo tampoco por documento (`PushBackDesignDocument.cs:469` lo normaliza a 0): **hueco negativo:** `pb-20-ab`; en la ventana, la costura `w.CompositeState.SetGap(-1)` y un `LostFocus` elevado en `AnnotationScaleBox`, sin editarla, que dispara el recálculo (`Input_Changed`, WX:68); en el estado, `Editor.SetGap(-1)` y `Pasada`. **Separador sin hueco:** `pb-20-ab` con `Composite.CentralSeparator = true` y `Composite.Gap = 0` (`EditInner`). **Estructura manual de 1:** `pb-20-ab` con `Composite.StructureOverrideA = 1` (`EditInner`). **Celda que no cabe:** el diseño de `PushBackOutputGateTests.Blocking()` (`:41-48`, sus llamadas copiadas), cargado con `LoadExisting` en la ventana y con `LoadDesign` en el estado | la ventana de hoy | el esqueleto no carga |
| **02** | **La ida y vuelta sin pérdidas** de los racks soportados: diseño → estado → diseño, y estado → diseño → estado (igual para el comparador) | **(M-01)** los 8 canónicos: la salida del piloto (CT-PBTC-06, `PushBackDeclaredCorrectionPilotParityTests.cs:20-26`, iguala ventana y piloto, y `WorkspaceDraftPilotCanonicalLineEndingTests` iguala piloto y sobre): delta vacío; los 6 heredados: exactamente las líneas de `LegacyDeltas` (`WorkspaceDraftPilotMaterializationTests.cs:706-735`), copiadas; `pb-20-ab`, `pb-31-v2-modulos` y X-01: delta vacío (CT-PB0-01); F-01: delta vacío | el esqueleto da el diseño por defecto |
| **03** | **c-02:** `ResolvePostCatalogId` con el id del catálogo, el mismo en minúsculas, uno inexistente, en blanco, solo espacios y nulo; con un catálogo fabricado en Core con un id con espacios (`" PB1_X "`) y dos ids que solo difieren en mayúsculas (`PB1_Y`, primero, y `pb1_y`), con `Defaults.Post` = P; y el `HeaderPostCatalogId` de la salida del estado para c-01 y c-02. **Las líneas del catálogo fabricado, por rama (N-M-03):** `PB1_X` da `PB1_X` en las dos ramas; `" PB1_X "` da P en las dos; `PB1_Y` da `PB1_Y` en las dos; `pb1_y` da P en la rama ordinal y `PB1_Y` en la rama sin mayúsculas. El Worker usa las de la rama que fijó el paso 1 | el paso 1 de PB-T1b (INV-PBT1B-06); CT-PB0-17 | el esqueleto devuelve `requested` |
| **04** | **`pb-21-cola`:** `SupportOf` da `DormantCompositeTail` para `pb-21-cola` y `Supported` para los demás; su ida y vuelta pierde exactamente las 667 hojas de CT-PB0-01 (`delta-filtrado=667 sha256=0b54e8ff5c8ad08d262bdfd8e30db37eb0dd40b19797940f8a5fd85b04eed875`); el piloto sigue dando `OutOfPilot` con `DormantTailOutOfPilotReason` | CT-PB0-01; PM:108 | el esqueleto da `Supported` |
| **05** | **Los demás de T-2, tal cual:** c-01, c-03, c-04/c-06, c-05/c-07, c-08/c-09, c-10/c-11/c-16, c-14 y c-15 dan las líneas de CT-PB0-17; c-12, rechazado por el almacén; **c-13 lanza al cargar el estado el mismo tipo y el mismo mensaje** | CT-PB0-17 | el esqueleto no carga ni lanza |
| **06** | **El censo** (M-08): todo campo de instancia del grafo de `PushBackEditableState` está en la tabla, y toda entrada existe. **La tabla** es `PilotCensus.Table` (`WorkspaceDraftPilotStateTests.cs:211-369`), copiada **sin** las tres filas de `PushBackPilotDraftState`, con estas reclasificaciones: `workingBaseline` y `LastModuleReconciliation` → **D** (derivado compartido, no se recorre); `moduleSession`, `moduleCommit`, `moduleSignature`, `DormantCompositeTail` y `DormantTailLineOverrides` → **A** (autoridad copiada, no se recorre); `dormantTailConsumed` → **T** (transitorio del armado: la marca entre `BeginDormantTailConsumptionAttempt` y la adopción, CS:116-147; se copia y no se compara); y con las filas nuevas de `PushBackEditableState` (`Editor`, `Pallet`, `PalletsDeep`, `PostCatalogId`, `PostPeralte`, `PalletTolerance`, `BeamDepth`, `AnnotationScale`, `FirstLevelDatum`, `NumberFronts`, `NumberLevels`, `DrawRackName`, `Dimensions`, `DimensionStyle`, `LoadedDimensionViews`, `DimensionViewsTouched`, `DimensionsFrontal`, `DimensionsLateral`, `DimensionsPlanta`, `Advanced`, `SafetySelections`, `Name`) → **E**. Con un estado rico (F-01 con una edición escenificada, un commit y `pb-20-ab` con B retirado), cada campo **E** sobrevive a `Copy` | el censo del piloto | `Copy` da un estado vacío |
| **07** | **Sin alias (I-01, I-02),** sobre `pb-06-seguridad`, `pb-07-combinado`, F-01 con una edición escenificada y un commit, `pb-20-ab` con B retirado tras una `Pasada` (cola aparcada por `ParkTailOf`) y X-01. **Parte 1:** la copia es igual; los objetos compartidos por todo el grafo, quitados los inmutables (`IsImmutable` de `PilotCensus`, `:531-536`, copiado), son exactamente la línea base y el informe de cada lado y lo que ellos alcanzan. **Parte 2, sin exención:** los objetos que alcanzan la cola, el commit y la sesión de cada lado de la copia no comparten **ningún** objeto mutable con **todo** el grafo del original (línea base incluida). **El recorrido (N-M-05)** es el `AllObjects` del piloto (`WorkspaceDraftPilotStateTests.cs:501-529`), copiado y ampliado para entrar también en **las claves y los valores de todo `IDictionary`**, porque los `KeyValuePair` son tipos valor y el diccionario no es un tipo de producto: así ve las configuraciones de línea de la sesión (`committedLines` y `workingLines`, RMES:82-83). Las partes 1 y 3 usan el mismo recorrido ampliado. **Parte 3:** el contenido editable, la cola, la sesión y el commit de un estado recién cargado no comparten ningún objeto mutable con el diseño cargado | — | `Equals` es `ReferenceEquals` |
| **08** | **D2, condición 9: mutar todo el estado editable ordinario de la copia no cambia el original.** Lo que se muta, sobre qué y qué se comprueba es lo de D-PBT1-15, «La condición 9»: cada ranura **E** con el `Mutator` del piloto; la sesión, el commit y la cola por su API; la intención compuesta y la selección por el API de CS; y los portadores. En el original, tras cada mutación: igual con el comparador a su copia previa, el mismo diseño de una `Pasada` sobre una copia suya, y su conjunto de objetos intacto. Y al revés | el original | `Copy` da un estado vacío |
| **09** | **Módulos, cola y A/B:** sobre F-01, `pb-20-ab` con B retirado y X-01: la copia calcula el mismo diseño; volver a declarar B en la copia devuelve la cola intacta, como en el original; confirmar en la copia la edición escenificada da lo mismo que en el original | el original | `Copy` da un estado vacío |
| **10** | **El comparador:** dos cargas del mismo diseño, iguales; una con la sesión abierta y sin cambios y otra con la sesión nula, iguales (I-03); un estado cuya sesión, abierta y sin pendientes, conserva la lista confirmada de una línea base anterior de la misma firma (cargar `pb-31-v2-modulos`; `Pasada`; abrir la sesión; cambiar `Pallet` a un fondo de 50, que obliga a reconstruir con los mismos módulos; `Pasada`) frente al diseño de esa última computación cargado en un estado nuevo con su `Pasada`, distintos (N-M-02); cada cambio de contenido, cola, sesión con cambios, commit, línea base con el mismo commit (M-15) o portador, distintos; solo la selección, iguales; `NaN` reflexivo; `null` como el piloto | — | `Equals` es `ReferenceEquals` |
| **11** | **El mapa:** `Normalize` de `$.Structure.Fronts[3].Cells[0]` es `$.Structure.Fronts[*].Cells[*]`; toda ruta de hoja de los 17 sobres de `Fixtures/I64`, en sus tres niveles, sale distinta de `Unclassified`; en `Payload`, toda ruta salvo `$.SchemaVersion` es `Editor`; en `Project`, toda ruta es `Envelope` salvo `$.PushBack`, que es `Container`; en `Envelope`, `$.Name` es `Editor`, `$.Design` es `Container` y las demás son `Envelope`; una propiedad desconocida en la raíz de cada nivel es `Envelope`; toda fila tiene `Source` no vacía y sin número de línea | D-PBT1-08 | la tabla está vacía |
| **12** | **El criterio de bloqueo, con líneas literales (M-07):** con el sistema de `PushBackOutputGateTests.Valid()` (`PushBackOutputGateTests.cs:27-34`, sus llamadas copiadas) y un `PushBackCompositeEditorState` nuevo con `LoadComposite(new PushBackCompositeDesign { … })`: `valido bloqueado=False estado=RESERVA`; `hueco-negativo` (`Gap = -1`) `bloqueado=True estado=El hueco (-1") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor.`; `separador-sin-hueco` (`CentralSeparator = true`, `Gap = 0`) `bloqueado=True estado=El separador central necesita un hueco mayor que cero: sin el no hay donde colocarlo.`; `override-no-construible` (`StructureOverrideA = 1`) `bloqueado=True estado=Lado A: la estructura manual (1) no es construible; el minimo fisico es 2 fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.`; con el sistema de `Blocking()` (`:41-48`) y un estado sin intención inválida, `celda-no-cabe bloqueado=True estado-igual-al-primer-bloqueante-de-Evaluate=True`; y `RackBomOutputGate.For(Valid()).Allowed=True`, `RackBomOutputGate.For(Blocking()).Allowed=False`. Cultura invariante | M-8 del registro | `IsBlocked` es false |
| **13** | **`BuildInputs` nuevo en cada llamada:** dos llamadas no comparten objetos; un «Restaurar estándar» confirmado (DA:302) no toca `Advanced`; los seis avanzados, la unidad y las casillas llegan a los inputs | D-PBT1-04 | `BuildInputs` da `NewDesign()` |
| **14** | **D2, condición 10: la guarda G-L1 sobre Application** (`PushBackSharedReferenceGuardTests`). Los archivos, las fichas, las reglas (1, 1b, 2 con el `foreach`, y 3) y las autopruebas (a), (b), (c) y (e) de D-PBT1-15, «La condición 10». Las líneas: `reglas-sin-coincidencias=True`, y `autoprueba-a=falla`, `autoprueba-b=falla`, `autoprueba-c=falla`, `autoprueba-e=falla` | D-PBT1-15; registro N-9 | **no falla en el RED** (es una guarda; `ExpectRed` false) |
| **15** | **D2, condición 8: los escritores de `Name` del Plugin** (`PushBackSharedReferenceGuardTests`). El censo de las siete asignaciones de los archivos del Plugin que contienen `PushBack` (tres de Push Back), el orden de las tres de Push Back tras su `ShowModalWindow` y las dos autopruebas en negativo de D-PBT1-15, «La condición 8». Las líneas: `archivos=14`, `asignaciones=7`, los siete textos literales con su archivo, `escritores-pushback=3`, `orden=True` por cada uno de los tres, `DrawPushBackView-llamadas=1`, y `autoprueba-octava=falla`, `autoprueba-orden=falla` | M-1; D-PBT1-15; registro N-11 | **no falla en el RED** (`ExpectRed` false) |
| **16** | **D2, condiciones 3 a 7: las operaciones del API no mutan las dos referencias compartidas.** Sobre el estado rico de CT-06, su copia y X-01. Antes y después de cada bloque, para las cuatro referencias del original y las cuatro de la copia, **la huella profunda** (V23-12; registro N-10): el recorrido `AllObjects` ampliado de CT-07 desde el rack o el informe, que escribe por cada objeto su tipo y cada campo de instancia (públicos, privados y de respaldo) con su valor (números con «R» y cultura invariante; cadenas tal cual; referencias por su índice de recorrido; listas y diccionarios en orden), con su `sha256`, más `<rack>.Name` en claro. `Persist(resolver.Snapshot(<rack>))` no basta: no ve lo derivado ni `Composite`. **Bloque «editar»** (3 y 5): `Pasada`; retirar B y `Pasada` (`ParkTailOf`); abrir la sesión, escenificar, confirmar y cancelar; `ParkDormantTail` y `ClearDormantTail`; las operaciones de CS de la condición 9; y cada portador. **Bloque «validar»** (4): `PushBackEditorOutputGate.Diagnostics`, `IsBlocked` y `StatusOr`; `Editor.IntentDiagnostics()`; una `Pasada` con el refuerzo de 100000 (que lanza); `SupportOf`; `ResolvePostCatalogId`; `BuildInputs`; y los dos consumidores medidos de la ventana que solo leen, `RackModuleDescriptor.Describe(<rack>.Structure)` y `new PushBackSystemLateralBuilder().Cortes(<rack>, catalog)` (M-1). **Bloque «descartar y recargar»** (6): `LoadDesign`, `Restore` y `LoadNew` sobre la copia, y una copia descartada sin tocar. **Bloque «comparar»** (7): `Equals` en los dos sentidos y `GetHashCode`; y, con dos estados que comparten la misma línea base, el resultado de `Equals` antes y después de mutar en el objeto compartido su `Name` y la `Length` de un módulo, que **no cambia**. Cada bloque da `<bloque> huellas-iguales=True`, y el último, además, `comparar-invariante=True` | M-1; D-PBT1-15 | `Copy` da un estado vacío |

### 5.4 PB-T1c: la ventana sobre el estado (lista cerrada CT-PBT1C-01 a 04)

**Régimen:** **el RED** es la costura `internal PushBackEditableState EditableStateForTest => new PushBackEditableState();` en W y las pruebas nuevas, que fallan por aserción salvo la guarda CT-PBT1C-04, que pasa en el RED y discrimina por sus autopruebas (INV-F4T3A-13), con la regla «EXCEPCION» (M-11); **el GREEN** es D-PBT1-13. **La equivalencia la prueban las redes existentes sin tocarlas**, en particular CT-PBT1A-14 con F-03.

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** (UI) | La ventana edita el estado extraído: `ReferenceEquals(w.EditableStateForTest.Editor, w.CompositeState)`; y, con las secuencias de pasadas del §5.0, el estado de la ventana tras cada camino (ventana nueva, `LoadExisting`, `LoadDesignForNew`, «Restaurar») es igual, con el comparador de PB-T1b, al del estado solo que siguió la misma secuencia, sobre los 17 de `Fixtures/I64`, X-01 y F-01 | la costura da otro estado |
| **02** (UI) | **Ningún control es la fuente de verdad:** para cada control del censo (§5.5) y su gesto, el diseño calculado desde una **copia** del estado de la ventana (`Copy`, `BuildInputs`, `BuildDesign` y `BuildFrom` con `FrontalSideForTest`) es igual, persistido, a `LastComputation.Design`; y `EditableStateForTest.Name` es igual a `RackName`. **Parte 2, D2, condiciones 3, 4, 5 y 8:** la prueba guarda la referencia de cada rack resuelto y de cada informe que el estado de la ventana adopta (los dos lados, tras cada pasada) y su huella profunda (la de CT-PBT1B-16, V23-12). Después de cada gesto del censo, de un campo en error, de un cálculo inválido (el refuerzo de 100000), de una caja escrita sin salir de ella, de «Restaurar» y de cerrar la ventana sin pedir, **ninguna huella guardada cambia**: la línea `<gesto> huellas-iguales=True` | la costura da otro estado |
| **04** (Core) | **D2, condición 10: la guarda G-L1 sobre W y WC** (`PushBackWindowSharedReferenceGuardTests`, una clase propia porque pasa en el RED): las fichas, las reglas (1, 1b, 2 con el `foreach`, y 3) y las autopruebas (a), (b), (d) y (e) de D-PBT1-15, sobre W y WC. Las líneas: `reglas-sin-coincidencias=True` y `autoprueba-<x>=falla` por cada una | **no falla en el RED** (`ExpectRed` false) |
| **03** (Core) | En W y WC, sin comentarios: ninguna declaración de campo con almacenamiento para los nombres de D-PBT1-13 (2); sin declaración de `SideAOnly`, sin `LoadCompositeFromDesign` y sin `ParkDormantTail(`; `CompositeHasBlocking` contiene `PushBackEditorOutputGate.IsBlocked(`; `LoadFromModel` contiene `editable.AdoptCarriers(inputs, catalog);` como primera sentencia de su `try`; W contiene `editable.BuildInputs(assembler)` y `EditableStateForTest => editable;` | la base declara los campos y no delega |

### 5.5 El censo control → prueba actualizado

| Control | Prueba que fija su efecto en la salida | Prueba de que el estado lo lleva (PB-T1c) |
|---|---|---|
| `DepthBox`, `ToleranceBox`, `PostPeralteBox`, `BeamDepthBox`, `AnnotationScaleBox` | CT-PB0-19 (SUS-03), CT-PBTC-01 a 03 y CT-PBT1A-07 | CT-PBT1C-02 |
| `WeightUnitBox`, `PalletsDeepBox` | CT-PB0-17, CT-PB0-04 y **CT-PBT1A-02** | CT-PBT1C-02 |
| `PostBox` | CT-PB0-17 (c-01, c-02) y **CT-PBT1B-03** | CT-PBT1C-02 |
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck`, `DimensionsBox`, las tres de cotas | CT-PB0-17, `PushBackDimensionViewsWindowTests` y **CT-PBT1A-01** | CT-PBT1C-02 |
| `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` | **CT-PBT1A-03** | CT-PBT1C-02 |
| `FirstLevelHeightBox` y las cuatro de la celda | CT-PB0-19, CT-PBTC-01 a 03 y CT-PBT1A-07 | CT-PBT1C-02 |
| `CellInOutBeamBox`, `CellInOutPeralteBox`, `CellIntermediateBeamBox` | CT-PB0-17 | CT-PBT1C-02 |
| `CellIntermediatePeralteBox`, `CellFondoOverrideBox`, `CellDrawPalletCheck` | **CT-PBT1A-04** | CT-PBT1C-02 |
| `CellBeamLengthOverrideBox` | CT-PB0-19, CT-PBTC2-04 y **CT-PBT1A-10** | CT-PBT1C-02 |
| `RearPeralteBox` | CT-PB0-17 (c-03) | CT-PBT1C-02 |
| `GapBox` | CT-PB0-20 y CT-PBTC2-01 | CT-PBT1C-02 |
| `CentralSeparatorCheck`, `StructureOverrideBox`, `CellTopologyBox`, `RunDirectionBox`, `TopologyScopeBox`, `FrontCountBox` | **CT-PBT1A-05** | CT-PBT1C-02 |
| `NameBox` | CT-PB0-11 | CT-PBT1C-02 (`Name` = `RackName`) |
| Las cajas avanzadas | CT-PB0-05 (SUS-04), CT-PBTC-14 y CT-PBTC2-03 | CT-PBT1C-02 |
| `ModuleLengthBox`, `DerivedPostLineHeightBox` | CT-PB0-03, CT-PB0-04, CT-PBTC2-02 y **CT-PBT1A-06** | CT-PBT1C-02 |
| `SideBPresentCheck`, `SideSelectorBox`, `FrontalSideBox` | CT-PB0-06 a 09, **CT-PBT1A-09** y CT-PBT1A-14 (F-02, F-03) | CT-PBT1C-02 |

**Huecos que quedan:** ninguno de los 20 de D-5.

### 5.6 Trazabilidad

| Obligación | CT |
|---|---|
| «module state preserved» | CT-PBT1B-06, 08 y 09; CT-PBT1A-06; CT-PBT1C-02 |
| «parked queue preserved» | CT-PBT1B-07 (parte 2), 09 y 02 (X-01); CT-PBT1A-14 (G-4) |
| «A/B preserved» | CT-PBT1B-02 y 09; CT-PBT1A-03, 08 y 09 |
| «no aliasing», con la excepción D2 | CT-PBT1B-07 (partes 1 a 3), 08 y 13; las diez condiciones de D-PBT1-15 |
| D2, condición 1 | CT-PBT1B-06 y 07 (parte 1) |
| D2, condición 2 | CT-PBT1B-07 (partes 1 a 3) |
| D2, condiciones 3, 4 y 5 | CT-PBT1B-16 («editar» y «validar»); CT-PBT1C-02 (parte 2); arrastre a PB-T2 y PB-T4 (A-2) |
| D2, condición 6 | CT-PBT1B-16 («descartar y recargar»); arrastre a PB-T4 (A-2) |
| D2, condición 7 | CT-PBT1B-16 («comparar»); arrastre a PB-T4 (A-2); lectura L-4 |
| D2, condición 8 | CT-PBT1B-15; CT-PBT1B-16 y CT-PBT1C-02 (el `Name`); arrastre a PB-T3 y PB-T4 (A-2) |
| D2, condición 9 | CT-PBT1B-08 |
| D2, condición 10 | CT-PBT1B-14; CT-PBT1C-04; arrastre a PB-T2, PB-T3 y PB-T4 (A-2) |
| D2, el límite | PBT1A-C17, PBT1B-C17 y PBT1C-C17; A-2 |
| «no WPF control becomes source of truth» | CT-PBT1C-02; CT-PBT1B-03; §5.5 |
| «state round-trip lossless for supported classic racks» | CT-PBT1B-02 y 04 |
| «classic RACKEDITAR still works through the extracted state» | CT-PBT1C-01; §5.1 y CT-PBT1A-14 sobre PB-T1c |
| OB-05.1 | CT-PBT1B-11 |
| El criterio de bloqueo para PB-T3 | CT-PBT1B-12 y 01; CT-PBT1C-03 |
| c-02; `pb-21-cola`; T-2; D-3 | CT-PBT1B-03, 04 y 05; CT-PBT1A-13 |
| D-5; M-03 de PB-TC y de PB-TC2; M-04; M-07; CT-16; M-02 de PB-TC | CT-PBT1A-01 a 12 |
| OB-03.1 | CT-PBT1B-04 y el filtro `Workspace` |
| OB-T1B.3 | §4; INV-PBT1x-07 |

## 6. Los contratos propuestos

### 6.0 Común a las tres subtareas

**Identidad:** `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true. Evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Anclajes:** antes de emitir cada subtarea, la sesión vuelve a medir sobre su `BaseSha`, con las órdenes del registro, y lo deja escrito en el commit de emisión:
- PB-T1a: los quince textos de la tabla de mutantes y las zonas [TC2]. Su `BaseSha` es el commit de emisión, sobre `cd47523b`, con `git diff b5c09f0b <BaseSha> -- src tests` vacío. Los mide además el generador del contrato con sus `assert` (registro N-14);
- PB-T1b: `git diff <cierre de PB-T1a> -- src` vacío;
- PB-T1c: `git diff <cierre de PB-T1b> -- src/RackCad.UI` vacío y los anclajes de D-PBT1-13.

**La comprobación acotada (M-12):**
- **Quién:** un revisor independiente, en solo lectura, con el método de IRT1-1.
- **Sobre qué:** este paquete (v2.3), la sección de la subtarea, la salida de sus anclajes y, para PB-T1a, la remedición del §8.4 (registro N), que la sesión pidió revisar (F4B-E12, «Siguiente»). La de la v2 ya está hecha (`independent-review-v2-check.md`).
- **Qué comprueba:** que los anclajes se sostienen; que ningún hecho nuevo cambia un CT o el diseño; que no aparece ninguna decisión material nueva; para la v2, que los 23 hallazgos de IRT1-1 están resueltos; y, en PB-T1b y PB-T1c, que lo de D2 (D-PBT1-15) cubre sus condiciones como se declara.
- **Resultado:** EMITIBLE o NO EMITIBLE, con hallazgos, custodiado en `F4B-DRAFTS/pbt1/` (`bounded-check-v2.md`, `bounded-check-pbt1b.md` y `bounded-check-pbt1c.md`).

Si un anclaje no se sostiene de un modo que cambia un CT o el diseño, la parte afectada se rehace y vuelve a una comprobación acotada.

**Custodia (M-12):**
- en el commit de emisión de **PB-T1a**: este paquete (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`), el registro (con el apartado N de la remedición), los cinco scripts de medición (`members.py`, `count_tests.py`, `forbidden_scope.py`, `remeasure_anchors.py` y `remeasure_d2.py`), IRT1-1 y su comprobación acotada, la comprobación acotada de la remedición, y el contrato y su generador `make_pbt1a_contract.py` en `docs/automation/evidence/I-64-pilot/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/`. Las decisiones D1 y D2 del Owner (F4B-E11) ya están custodiadas en `cd47523b`;
- en los de **PB-T1b** y **PB-T1c**: su contrato y su generador en su `coordinator/`, la salida de sus anclajes y su comprobación acotada;
- en `f4b-dispositions.md`: la división (D-PBT1-01), D2 (L-1) con sus condiciones y su límite, el resultado de la remedición, cada emisión y los arrastres del §10.

**`ForbiddenWriteScope`:** con el método de [PKC-C], calculado por el generador de cada contrato con `assert` (medición J):
- `src/`: 59 en PB-T1a y PB-T1c (la forma de PB-TC2, con WX), 117 en PB-T1b;
- `tests/`: en PB-T1a, `tests/RackCad.Tests/` entero; en PB-T1b y PB-T1c, la lista curada de PB-T0 (18 entradas; medición H-12) más las clases de Core de PB-T0 y, en PB-T1c, las de PB-T1b. En `tests/RackCad.UI.Tests/`, la lista curada de [PKC2] (51 archivos y `Workspace/`) más las clases de PB-TC2 y, en PB-T1b y PB-T1c, las de las subtareas anteriores;
- la raíz, las 17 entradas de [PKC2].

**`StopConditions` estándar** (17), copiadas de [PKC2] §6.7: S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = «config.toml no longer matches the accepted Snapshot in force (today Snapshot I, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)».

**Las de [DP], literales, en cada contrato** (x = A, B o C):
- **PBT1x-C08** ([DP], pregunta 2): una sola replanificación, y solo si el rechazo de la planificación es **solo** por la comparación literal de las colecciones, con el prompt reforzado y sin gastar intento; un segundo rechazo de fidelidad, o cualquier otro rechazo (A1-A8 u otra causa), es STOP y vuelve al Owner.
- **PBT1x-C11** ([DP], pregunta 1). Un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    **Qué cuenta como la cláusula de exactitud:** forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`). No forma parte que Contract cite cualquier otra discrepancia.

    → El sustituto **`PBT1x-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBT1x_EXACT_WHITELIST_SUBSTITUTE`.
  - **nc1 con la firma exacta de PD-F4-04.** El texto literal de su fuente (`owner-f4t3-authorization.md:255-265`):

    > «* false CurrentSha correctly detected;
    > * Identity = fail;
    > * Handoff also fails first only because the handoff itself contains the false
    > SHA;
    > * outcome BLOCKED / STOP;
    > * all other intended checks match;»

    Su aplicación en F4, transcrita literal (`f4-dispositions.md:1657-1660`):

    > «- el CurrentSha falso se detecta, e Identity está en fail con una evidencia que nombra el SHA falso y el HEAD;
    > - Handoff falla primero solo porque la entrega contiene el SHA falso;
    > - `EXECUTION_BLOCKED` / STOP con S-04;
    > - solo difieren consumidores del CurrentSha: Handoff, Identity y Ci, con Remote en pass, un subconjunto de la cascada de F4-T2C.»

    Por tanto, Ci en fail solo como consumidor del `CurrentSha` falso forma parte de la firma. Ci en fail por cualquier otra causa no.

    → El sustituto **`PBT1x-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBT1x_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto:** PD-F4-05 y PD-F4-06; nc2 con Ci o Trailer en fail además; cualquier nc1 que no reproduzca la firma literal, **como el de PB-TC** (R-T1-08); un sustituto que no dé PASS, **como el de PB-TC2**, que falló por su propiedad 5 (la evidencia de Identity no escribe el SHA falso); cualquier fallo de nc3. **Ninguna de las dos aceptaciones puntuales del Owner se extiende a PB-T1:** ni «Aceptarlo solo esta vez» de nc1 en PB-TC (F4B-E08) ni `nc1 = SATISFIED_BY_OWNER_DISPOSITION` de PB-TC2, que valió solo para su intento 1 (F4B-E12, `owner-pbtc2-nc1-decisions.md`).
  - **Cómo se decide la coincidencia:** nunca por el nombre; comprobación por comprobación (el conjunto en fail, la primera, el `Outcome`, la `StopClass` y el texto de la evidencia). Las únicas diferencias admitidas: en PD-F4-01, Scope (en fail sobre la ruta quitada) y Contract (solo por el alcance); en PD-F4-04, Identity, Handoff (la primera) y Ci (solo como consumidor del `CurrentSha` falso); en las dos, `EXECUTION_BLOCKED` / STOP. La sesión registra la tabla en `f4b-dispositions.md`.
- **PBT1x-C12:** una corrección sería el intento 3. Vuelve al Owner antes de emitirse (D-PB-10).

**La parada del límite de D2, en cada contrato** (x = A, B o C):
- **PBT1x-C17 (F4B-E11, D2, «IMPORTANT LIMIT»):** si para hacer la tarea hiciera falta **mutar el rack resuelto (`PushBackSystem`) o el informe de módulos (`RackModuleReconciliationResult`) dentro del tiempo de vida de la edición o del borrador**, o compartir por referencia cualquier otro objeto mutable entre una copia del estado y su original, o si una medición encuentra ya una mutación así en el camino de edición: **STOP y se reabre L-1**. El texto del Owner, literal: «This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model.» El Worker no elige ninguna de las tres salidas: la decide el Owner.

**El RED de la cadena en una corrección (M-11, §16.8):** una corrección que toque `RT` ∪ `ChainRedFiles` exige RED otra vez. En PB-T1a, su commit RED vuelve a aplicar **los quince mutantes** sobre la punta y su GREEN los revierte; en PB-T1b, el RED vuelve a poner el esqueleto; en PB-T1c, la costura inerte.

**La lectura de `Ci` `RedPart` con Core en `success` (M-11):** el RED de PB-T1a deja Core en `success` y UI en `failure`. Se acepta con `RedPart = pass`, como F4-T3B1 ([PKC]:466) y F4-T4A2 en su intento 2 (`F4-DRAFTS/f4-dispositions.md:1917-1928`, F4-E56). En PB-T1b y PB-T1c, Core falla en el RED.

**`EligibleCells`:** las de [PKC2]: Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high`; Controller `openai/process/gpt-6-luna`, effort `high`. `model-catalog.md:51` y `:97` las verifican el 2026-09-30, y con `:14` vencen el **2026-12-29** ([PKC2]:655). nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT1x-07 a 12 y 14; el 13 de PB-T1a es su declaración de autoridad):
- **07. Las redes:** T1B, PB-T0, PB-TC, PB-TC2, Cantilever, las guardas del §4, el censo del piloto y las subtareas anteriores, en verde; ninguna prueba existente se edita, se salta, se reactiva ni se borra.
- **08. Las pruebas de UI:** las clases nuevas en `EditorDiscardPromptCollection`; `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`; ninguna ventana mostrada salvo CT-PBT1A-11, fuera de pantalla; el aviso de descarte sustituido con su sufijo; sin teclado ni foco real; prohibidos un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar»; ninguna aserción dentro de una costura; ningún fixture nuevo en `Fixtures/I64`.
- **09. Determinismo:** el GUID fijo, la cultura invariante, «R», el orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:** solo los filtros del contrato en UI, **nunca la suite completa de UI en local**; la de Core, sí; sin copias de árboles (MAX_PATH); las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, corren también dentro de `EditorDiscardPrompt.Substitute` (lección IRC-M02 del cierre de PB-TC2), no entran en ningún commit y guardan TRX y log como artefactos; el perfil no cambia (INV-PBT0-06).
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real (`git write-tree`) o null; el Worker no escribe en `docs/`; una sustitución se llamaría `SUS3A-0n`, `SUS3B-0n` o `SUS3C-0n` y es C02.
- **12. Las reglas de F4B y sin AutoCAD.**
- **14. Las dos referencias de D2:** fuera de `PushBackEditorState.workingBaseline` y `PushBackEditorState.LastModuleReconciliation` de cada lado, ninguna copia del estado comparte un objeto mutable con su original; ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado (las reglas de G-L1, D-PBT1-15). En PB-T1a, que no cambia producto, se cumple por INV-PBT1A-01.

**Las propias comunes** (con el prefijo de cada contrato):
- **C01:** hacerlo bien exigiría escribir fuera del `AllowedWriteScope`.
- **C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente, o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`. Una guarda del §4, el censo del piloto o una prueba de PB-T0, PB-TC o PB-TC2 que no queda en verde **vuelve al Owner** (OB-T1B.3).
- **C03:** aparece un cambio observable de RACKEDITAR frente a la base. STOP y vuelta al Owner (D-16, OB-RE.1).
- **C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o escribir en el perfil o en el disco.
- **C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **C10:** hacerlo bien exigiría corregir algo fuera del alcance: una pérdida de T-2, M-07, `pb-21-cola`, el modo WORKSPACE, el piloto, D-PB-07 o una corrección de PB-TC o PB-TC2.

### 6.1 Contrato de PB-T1a — `F4B-PBT1A-LOSSLESS-STATE-NET`

- **Objective** (ASCII): «Fijar con pruebas de caracterizacion nuevas, sin cambiar el producto, la red de la ventana clasica de Push Back que PB-T1 necesita antes de mover su estado: los huecos de D-5, M-03 de PB-TC y de PB-TC2, M-04, M-07, la observacion de CT-16, M-02 de PB-TC y D-3, y las huellas de salida del corpus extendido (con F-03) por los guiones G-1 a G-6, segun el paquete del contrato de PB-T1 v2.3, remedido sobre el producto de PB-TC2 (b5c09f0b), y la lista cerrada CT-PBT1A-01..14. El RED son las pruebas nuevas y los mutantes de la tabla cerrada MU-T1A-01..14 en W y WC; el GREEN revierte los mutantes. Ninguna linea de src cambia entre BaseSha y CurrentSha; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).»
- **`AllowedWriteScope`** (9): W y WC (solo los mutantes del RED); los siete archivos nuevos `tests/RackCad.UI.Tests/PushBackStateNet{Support,RackControlsTests,CellAndFrontTests,CompositeTests,ModuleAndSafetyTests,ActionsTests,FingerprintTests}.cs`.
- **`ForbiddenWriteScope`** (131, medido por el generador; registro N-7 y N-14): 59 de `src/` (los de PB-TC2, con WX); 55 de `tests/` (`tests/RackCad.Tests/` entero, los 51 archivos curados de [PKC2], las dos clases de PB-TC2 y `tests/RackCad.UI.Tests/Workspace/`); y las 17 de la raíz.
- **`RequiredTests`:**

  | Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackStateNet` | **14** | true |
  | UI | `PushBackClassicCharacterization` | 55 | false |
  | UI | `PushBackFullEditorCharacterization` | 16 | false |
  | UI | `PushBackDeclaredCorrectionRoundingTests`, `…DimensionStyleTests`, `…PilotParityTests`, `…SideBNoticeTests`, `…StandardRestoreTests` (uno por clase) | 3, 2, 2, 2, 1 | false |
  | UI | `PushBackDeclaredCorrection2` | 4 | false |
  | UI | `Cantilever` | 67 | false |
  | UI | `PushBackCompositeWindowTests`, `PushBackCompositeLifecycleWiringTests`, `PushBackCompositeLoadExistingModulesTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackModuleEditorWindowTests`, `PushBackAdvancedRackParametersWindowTests` | 52, 7, 3, 10, 13, 9 | false |
  | Core | `PushBackClassicCharacterization` | 10 | false |
  | Core | `PushBackFullEditorCharacterization` | 3 | false |
  | Core | `PushBackModuleEditorCharacterizationTests` | 15 | false |

  En el RED, los filtros sin `ExpectRed` pueden fallar **solo** por los mutantes, con la atribución T-8; en el GREEN, todos en verde.
- **Invariantes propios:**
  - **INV-PBT1A-01. Alcance:** `git diff --name-only BaseSha..CurrentSha` = los siete archivos nuevos; `git diff BaseSha..CurrentSha -- src` vacío.
  - **INV-PBT1A-02. RED y GREEN:** el RED son los siete archivos y los quince mutantes, juntos; el GREEN es `git restore --source=BaseSha` de W y WC y no toca `tests/`. Criterio del RED sobre el filtro con `ExpectRed`: todos `Failed`; `Message` sin el prefijo de tipo de [PKC-C]; la primera línea del `StackTrace` bajo `tests/`; ninguno `NotExecuted` ni `Skipped`; `Failed == Selected >= 14`.
  - **INV-PBT1A-03. Cada mutante solo:** aplicado aislado sobre la `BaseSha`, hace fallar por aserción el CT de su fila; la entrega lo transcribe con el TRX de cada corrida temporal.
  - **INV-PBT1A-04. Las líneas esperadas** salen del paso 1, transcrito en la entrega antes del RED.
  - **INV-PBT1A-05. CT-14:** el corpus, las recetas y los guiones del §5.0 y del §5.2, en ese orden.
  - **INV-PBT1A-06. Las mediciones guardadas** (M-06 de PB-T0), como artefactos con ruta relativa.
  - **INV-PBT1A-13. La declaración de autoridad (I-08; N-M-07: era PBT1A-C15 y no es una parada):** el Worker lista en `UnexpectedFindings` cada línea fijada que muestre un valor del diseño que cambia sin edición del usuario (una pérdida de clase c, d o nueva) o un control releído sin edición que cambia el estado o la salida, **distinta** de c-01, c-02 y M-07. La sesión la lleva al Owner antes de emitir PB-T1b (precondición 5). Con la numeración común (07 a 12), este es el 13.
- **`StopConditions`:** las 17 estándar, C08, C11, C12, las comunes, y:
  - **PBT1A-C04 (I-06):** un mutante de la tabla, aislado, no hace fallar por aserción el CT de su fila; o un CT nuevo falla en la `BaseSha` sin mutantes; o falla por excepción. El Worker no cambia el CT ni el mutante.
  - **PBT1A-C17:** la parada del límite de D2 (§6.0). PB-T1a no crea estado ni toca el Plugin: la declara para que valga en toda la cadena (por ejemplo, si un mutante o una receta necesitara mutar el rack resuelto o el informe de una ventana viva).
- **`ExpectedEvidence`:** antes del Worker, el contrato, la delegación, la comparación byte a byte, A1-A8 y nc4; el paso 1 con su TRX y su log; el RED y su CI (ui-tests en `failure`; tests, build-ui y build-plugin en `success`; sin cuelgue); la tabla T-8; los quince mutantes aislados; el GREEN y su CI 4/4; en local, el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN y tres corridas en verde del filtro con `ExpectRed` sobre el GREEN; la entrega con el mapa CT → método, las rutas relativas comprobadas y la lista de INV-PBT1A-13; la verificación y nc1 a nc3.
- **Línea base de la CI:** la del cierre de PB-TC2, **medida** (registro N-5): el run `44313012995` sobre `cd47523b`, 4 de 4 en `success`, con UI 2 422 resultados, 21 `Skipped` y 2 401 `Passed`, y Core 19 070. Es la previsión de su §6.9. **GREEN de PB-T1a:** UI 2 436, 21 `Skipped`, 2 415 `Passed`; Core 19 070.

### 6.2 Contrato de PB-T1b — `F4B-PBT1B-LOSSLESS-STATE-TYPE`

- **Objective** (ASCII): «Crear en Application el estado editable de Push Back sin perdidas fuera de la ventana (PushBackEditableState), con las cargas de LoadNew, LoadExisting, LoadDesignForNew y Restaurar partidas en la de Application y AdoptCarriers, la salida BuildInputs, el copiador y el comparador completos con modulos, cola aparcada y A/B (con la excepcion de dos referencias compartidas decidida por el Owner en F4B-E11, D2, y sus diez condiciones de seguridad), el mapa de propiedad, la regla explicita de c-02, la exclusion de pb-21-cola y el criterio de bloqueo de salida extraido, segun el paquete del contrato de PB-T1 v2.2 y la lista cerrada CT-PBT1B-01..16. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo las dos guardas de D2; el GREEN implementa solo Application. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion: STOP y se reabre L-1. La ventana, el piloto, el Plugin y el XAML no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (12): ES y RMES; los cuatro nuevos de `src/RackCad.Application/Systems/PushBack/` (`PushBackEditableState.cs`, `PushBackEditableStateComparer.cs`, `PushBackEditableStateOwnership.cs`, `PushBackEditorOutputGate.cs`); los cinco nuevos de `tests/RackCad.Tests/` (`PushBackEditableStateLoadTests.cs`, `PushBackEditableStateCopyTests.cs`, `PushBackEditableStateOwnershipTests.cs`, `PushBackEditorOutputGateTests.cs` y **`PushBackSharedReferenceGuardTests.cs`**) y `tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs`.
- **`ForbiddenWriteScope`:** 117 de `src/` (CS incluido) y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackEditableState` | **12** | true |
  | Core | `PushBackEditorOutputGate` | **1** | true |
  | UI | `PushBackEditableStateParityTests` | **1** | true |
  | Core | `PushBackSharedReferenceGuardTests` (CT-14 y 15, las guardas de D2) | **2** | false |
  | Core | `Workspace` | 3 271 | false |
  | Core | `PushBackOutputGateTests`, `PushBackBlankFrontAuthorityTests`, `PushBackModuleEditorCharacterizationTests` | 8, 9, 15 | false |
  | Core | `RackModuleEditSessionTests`, `NamespaceFolderGuardTests`, `HeaderBatchContractTests` (M-10) | 25, 7, 17 | false |
  | Core y UI | `PushBackStateNet` (14), T1B, PB-T0, PB-TC, PB-TC2 y Cantilever | como en §6.1 | false |

- **Invariantes propios:**
  - **INV-PBT1B-01. Alcance:** el diff dentro de las 12 rutas; en `src/`, solo las seis de producto. `git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Plugin src/RackCad.Domain src/RackCad.Application/Systems/PushBack/Pilot src/RackCad.Application/Workspace src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs` vacío.
  - **INV-PBT1B-02. RED y GREEN:** el RED es el esqueleto exacto del §5.3 y las seis clases de prueba; el GREEN solo toca `src/`. Criterio del RED sobre cada filtro con `ExpectRed`: todos `Failed` por aserción (como INV-PBT1A-02), con `Failed == Selected`. El filtro de las guardas de D2 pasa en el RED y en el GREEN, y sus autopruebas en negativo fallan sobre el texto inyectado.
  - **INV-PBT1B-07c. D2** (D-PBT1-15): CT-PBT1B-06, 07, 08, 14, 15 y 16 en verde en el GREEN; la clase **D** del censo son exactamente los dos miembros de la condición 1.
  - **INV-PBT1B-03. La API:** los miembros, las firmas y los valores iniciales son exactamente los de D-PBT1-02 a D-PBT1-11; ningún otro miembro público nuevo. Los ayudantes privados nuevos son solo `LoadComposite` en `PushBackEditableState`, `CopyLiteral` en la sesión y `CopyModule` en el commit, más los privados del comparador que copian literal los del piloto (PC:64-83, PC:134-350).
  - **INV-PBT1B-04. Ningún campo de instancia nuevo** en CS, ES, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions`, los tipos de seguridad ni RMES (el censo del piloto en verde y `git diff -U0` sin declaraciones de campo de instancia añadidas en ES ni en RMES). Ningún campo estático nuevo tampoco: el `RackFrameProjectStore` de `RackModuleCommit` y el de `PushBackEditableState` son locales al método (N-M-06).
  - **INV-PBT1B-04b.** Cada archivo nuevo de `src/` declara un solo `namespace RackCad.Application.Systems.PushBack` de bloque; las pruebas, `namespace RackCad.Tests` o `RackCad.UI.Tests`.
  - **INV-PBT1B-05 (M-06), solo sobre `src/`:** ningún archivo nuevo de `src/` contiene `.RequestUpdate(` ni nombra `RackCad.UI`, `System.Windows`, `Autodesk`, `Workspace`, `Pilot` ni `RackSystemKind`.
  - **INV-PBT1B-06. Paso 1, sobre `BaseSha` y antes del RED** (una corrida temporal de UI, fuera de todo commit, con `Assert.Fail`): con `pb-01-base` y su `HeaderPostCatalogId` cambiado, la línea `<caso> PostBox.SelectedId=<v> salida=<HeaderPostCatalogId de LastComputation.Design>`. **Las filas esperadas** (M-05), con P = `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA`:

    | Caso | `HeaderPostCatalogId` | Línea esperada |
    |---|---|---|
    | (a) | P | `a PostBox.SelectedId=P salida=P` |
    | (b) | P en minúsculas | `b PostBox.SelectedId=<null> salida=P` (rama ordinal) o `b PostBox.SelectedId=P salida=P` (rama sin mayúsculas) |
    | (c) | `PB0_POSTE_INEXISTENTE` | `c PostBox.SelectedId=<null> salida=P` |
    | (d) | en blanco | `d PostBox.SelectedId=P salida=P` |
    | (e) | tres espacios | `e PostBox.SelectedId=P salida=P` |

    Se transcribe en la entrega. **La fila (b) fija la rama de la regla de c-02** (D-PBT1-09, N-M-03): con `<null>`, la rama ordinal; con `P`, la rama sin mayúsculas. El Worker declara cuál midió y la aplica; cualquier otro valor de `PostBox.SelectedId` en (b) es PBT1B-C14.
- **`StopConditions` propias:**
  - **PBT1B-C04 (I-06):** un método de un filtro con `ExpectRed` pasa sobre el esqueleto, o falla por excepción y no por aserción. Es un defecto del contrato.
  - **PBT1B-C13:** la implementación necesitaría un campo nuevo en un tipo del censo del piloto, tocar CS, o cambiar el comportamiento de un método existente de ES, CS, DA, CA o RMES.
  - **PBT1B-C14:** en el paso 1, alguna `salida` de (a) a (e) no es P, o una fila no es ninguna de sus líneas esperadas (en (b), ninguna de las dos ramas). STOP antes del RED: la regla de c-02 vuelve a la sesión.
  - **PBT1B-C15:** CT-PBT1B-01 o 02 da una diferencia entre el estado y la ventana de hoy, o una pérdida en un rack soportado, que no sale del esqueleto. STOP: es un hecho nuevo y vuelve al Owner por OB-06.5.
  - **PBT1B-C17:** la parada del límite de D2 (§6.0).
- **`ExpectedEvidence`:** como §6.1, con el paso 1 y, en vez de los mutantes, el diff del esqueleto, la tabla «CT → la aserción que falla en el RED» y la salida de las autopruebas en negativo de las dos guardas.
- **Línea base de la CI:** la del cierre de PB-T1a. **GREEN:** Core 19 070 + 15 (12 + 1 + 2); UI la de PB-T1a + 1.

### 6.3 Contrato de PB-T1c — `F4B-PBT1C-LOSSLESS-STATE-WINDOW`

- **Objective** (ASCII): «Hacer que la ventana clasica de Push Back guarde su estado editable en el PushBackEditableState de PB-T1b, en modo CLASSIC y sin cambio observable, con el cambio minimo de D-PBT1-13 (reenvios con el mismo nombre, la carga de Application delegada, AdoptCarriers dentro de LoadFromModel, ReadInputs que escribe en el estado, el criterio de bloqueo extraido y la costura EditableStateForTest), segun el paquete del contrato de PB-T1 v2.2 y la lista cerrada CT-PBT1C-01..04, con las condiciones de seguridad de la decision D2 del Owner (F4B-E11) que toca la ventana. El RED es la costura inerte y las pruebas nuevas, salvo la guarda de D2; el GREEN toca solo W y WC. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion: STOP y se reabre L-1. Los constructores, los cuatro sitios, el XAML, el Plugin, el piloto y Application no cambian; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (5): W, WC, `tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs`, `tests/RackCad.Tests/PushBackWindowOnStateSourceGuardTests.cs` y **`tests/RackCad.Tests/PushBackWindowSharedReferenceGuardTests.cs`**.
- **`ForbiddenWriteScope`:** 59 de `src/` y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackWindowOnStateTests` | **2** | true |
  | Core | `PushBackWindowOnStateSourceGuardTests` | **1** | true |
  | Core | `PushBackWindowSharedReferenceGuardTests` (CT-04, la guarda de D2) | **1** | false |
  | Core | `PushBackSharedReferenceGuardTests` (las de PB-T1b) | 2 | false |
  | UI | `PushBack` (todas las clases `PushBack*`) | el medido en la emisión (542 sobre `b5c09f0b`, con las 4 de PB-TC2, + 14 de PB-T1a + 1 de PB-T1b = 557) | false |
  | UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace`, `PushBackRichPreviewTests` | 33, 8, 25, 547, el medido | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests`, `I55DeferredProductFixCharacterizationTests`, `NamespaceFolderGuardTests`, `Workspace` | 12, 1, 10, 3, 15, 3, 7, 3 271 | false |
  | UI | `Cantilever` | 67 | false |

- **Invariantes propios:**
  - **INV-PBT1C-01. Alcance:** dentro de las 5 rutas, y en `src/` solo W y WC; `git diff BaseSha..CurrentSha -- src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls` vacío.
  - **INV-PBT1C-02. RED y GREEN:** el RED son las tres clases de prueba y, en W, solo la línea de la costura inerte; el GREEN solo W y WC; criterio del RED como INV-PBT1A-02, con `Failed == Selected` en cada filtro con `ExpectRed`. La guarda de D2 pasa en el RED y en el GREEN, y sus autopruebas fallan sobre el texto inyectado.
  - **INV-PBT1C-03. Dónde cambia, sobre el diff:** cada hunk de `git diff -U0 BaseSha..CurrentSha -- src` cae en un miembro de esta lista, por su rango en `BaseSha` (la definición de «tocar un miembro» de [PKC2] INV-PBTC2-03): los campos de D-PBT1-13 (1) y (2); `LoadNew`, `LoadDesignForNew`, `LoadExisting`, `SideAOnly` (retirado), `LoadFromModel`, `ReadInputs`, `Name_Changed`, `SelectedDimensionViews` (retirado), `Restore_Click`, `InsertBatch_Click`, `RequestDraw`, `SaveLibrary_Click`; en WC, los dos `LoadCompositeFromDesign` (retirados), `CompositeDiagnostics`, `CompositeStatusOr` y `CompositeHasBlocking`; y las declaraciones nuevas `SyncName` y `EditableStateForTest`.
  - **INV-PBT1C-04. Los literales de T-SG:** las cinco sentencias de CT-22, la de S-9, la de S-7 y las cuatro de CT-23 siguen en W, byte a byte y en su orden; `SyncName();` va inmediatamente después de cada `SetName` y de cada `Adopt`.
  - **INV-PBT1C-05. Las cuentas, sobre `CurrentSha`:** W con una sola `.RequestUpdate(`; ninguna línea añadida de W o WC contiene `forceRebuild`, `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult`; W sigue conteniendo los literales de I-55, Fact3 y el desviador; ninguna línea con `SetNumber(` se añade ni se quita; las siembras de `LoadFromModel` de D-PBT1-13 (4) no cambian; `editable.AdoptCarriers(inputs, catalog);` es la primera sentencia del `try` de `LoadFromModel`; `UpdateCompositeEnabled();` va en la línea siguiente a `LoadDesignEditor` y a `RestoreEditor`.
  - **INV-PBT1C-06. Las costuras:** las 40 de la medición H, con el mismo nombre y tipo; solo se añade `EditableStateForTest`.
  - **INV-PBT1C-07b. Las huellas:** CT-PBT1A-14, con F-03, en verde sin tocarla, en local y en la CI.
  - **INV-PBT1C-07c. D2:** CT-PBT1C-02 (parte 2) y CT-PBT1C-04 en verde; las guardas de PB-T1b (CT-PBT1B-14 y 15) siguen en verde sin tocarlas; ninguna línea añadida de W o WC asigna el `Name` de `lastComputation.System`, `session.System`, `lastCompositeSystem` ni de la línea base de un lado, ni muta de otro modo esos objetos o el informe de un lado (el `Name` del estado, que escriben `Name_Changed` y `SyncName`, no es ninguno de ellos).
- **`StopConditions` propias:**
  - **PBT1C-C04 (I-06):** un método de un filtro con `ExpectRed` pasa sobre la costura inerte, o falla por excepción.
  - **PBT1C-C13:** mantener sin cambio observable un gesto exigiría tocar un miembro fuera de INV-PBT1C-03 o cambiar el orden de una carga.
  - **PBT1C-C14:** una huella de CT-PBT1A-14 cambia. STOP (C03); el Worker no ajusta ni la huella ni el producto.
  - **PBT1C-C17:** la parada del límite de D2 (§6.0).
- **`ExpectedEvidence`:** como §6.1, con la salida de INV-PBT1C-03 a 05 sobre el diff, la de las autopruebas de CT-PBT1C-04 y, en local, la clase de huellas tres veces en verde sobre el GREEN.
- **Línea base de la CI:** la del cierre de PB-T1b. **GREEN:** Core + 2 (CT-03 y CT-04); UI + 2.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-T1-01 | **Sin latitud para el Worker** | Los contratos no dan latitud; las recetas, el ayudante `Pasada`, la tabla del censo y la API del mapa están en el paquete |
| R-T1-02 | **Las líneas esperadas salen de leer el código** en PB-T1b y PB-T1c | Oráculos existentes (ventana de hoy, CT-PB0-01, CT-PB0-17, CT-PBTC-06, `LegacyDeltas`) y líneas literales donde el código no basta (CT-12); PBT1B-C15 |
| R-T1-03 | **Evidencia con rutas relativas y `TreeSha` real** | INV-PBT1x-11 |
| R-T1-04 | **Corridas temporales con `Assert.Fail`** | INV-PBT1x-10 |
| R-T1-05 | **Las ventanas con el aviso de descarte sustituido** | INV-PBT1x-08 |
| R-T1-06 | **Nunca la suite completa de UI en local** | INV-PBT1x-10 |
| R-T1-07 | **MAX_PATH** | Sin copias de árboles; `TaskId` de 28 a 31 caracteres |
| R-T1-08 | **La variación de nc1 de PB-TC, y la de PB-TC2** (el sustituto falló por su propiedad 5, y el nc1 repetido falló además en Remote; F4B-E12) | Es previsible que nc1 de PB-T1a no satisfaga el sustituto: vuelve al Owner por C11 en cada cadena. Ninguna aceptación puntual anterior se extiende (V23-15) |
| R-T1-09 | **Los nombres de las sustituciones** | `SUS3A-0n`, `SUS3B-0n` y `SUS3C-0n`; se esperan 0 |
| R-T1-10 | **Tres cadenas antes del 2026-12-29** | Vigencia de las celdas en cada emisión y delegación (A7) |
| R-T1-11 | **El censo del piloto** | INV-PBT1B-04; PBT1B-C13 |
| R-T1-12 | **El rack resuelto o el informe de módulos, compartidos por D2,** podrían mutarse dentro del camino de edición en una tarea posterior. **Hoy no** (registro N-9 y N-11: G-L1 da 0 coincidencias y no hay cuarto escritor de `Name`) | D2 con sus diez condiciones (D-PBT1-15): CT-PBT1B-08, 14, 15 y 16; CT-PBT1C-02 (parte 2) y 04; la parada PBT1x-C17; el arrastre A-2. **Riesgo residual declarado:** G-L1 es textual y no ve una mutación dentro de un método de otro archivo (Domain incluido); la cubren solo las huellas de los caminos ejercidos |
| R-T1-19 | **La remedición sobre el GREEN de PB-TC2 cambia una premisa material** (§8.4) | **No ocurrió** (registro N; §8.4). Si la comprobación acotada de la remedición encuentra una, el paquete se corrige antes de emitir; si toca D2, C17 |
| R-T1-13 | **La pasada reentrante de `UpdateCompositeEnabled()`** (HT1-10b) | Los portadores se adoptan donde hoy (D-PBT1-13 (4)); F-03 en CT-PBT1A-14 lo ejerce; INV-PBT1C-05; PBT1C-C13 |
| R-T1-14 | **PB-TC2 no ha cerrado** | **Cerrado:** PB-TC2 está CLOSED / PASS WITH LIMITATIONS sobre `b5c09f0b` (F4B-E12), y las zonas [TC2] están remedidas (§2.8) |
| R-T1-15 | **Cómo cuenta el Controller un `Skipped`** | `MinSelected` sobre los que pasan |
| R-T1-16 | **El RED de PB-T1a rompe pruebas existentes** | La tabla T-8; Core en `success` (§6.0) |
| R-T1-17 | **El modelo paralelo interino (M-12):** entre el cierre de PB-T1b y el de PB-T1c hay **tres** copias de la carga compuesta (WC, el estado y el piloto), contra ADR-0029 D12 | Se declara como interino. PB-T1c retira la de la ventana; PB-T4 retira la del piloto (A-5) |
| R-T1-18 | **PB-T1c no cierra** (M-12) | El tipo de PB-T1b queda sin uso de producto y sin cambio observable; ante un intento 3, el Owner decide (C12); nada se integra en `main` (A64-PV1-21) |

## 8. Las decisiones del Owner

### 8.1 L-1, decidido: (a), excepción declarada (F4B-E11, D2)

**La decisión del Owner** (F4B-E11, D2, literal):

> D2 — PB-T1 L-1 / NO ALIASING
>
> Decision:
>
> ACCEPT DECLARED EXCEPTION.
>
> The PB-T1 copied state may retain shared references ONLY for:
>
> 1. resolved rack;
> 2. module report.
>
> Everything else governed by the extracted editable state must satisfy the
> normal no-aliasing requirement.

**Su razón** («WHY THE EXCEPTION IS ACCEPTED», literal):

> These two objects already behave as shared/reference objects in the current
> product.
>
> Their known post-editor use is limited to the existing drawing path, where
> the resolved rack may receive the established name adjustment after the
> window has closed.
>
> PB-T1 is a state-extraction task, not a redesign of those authorities.
>
> Therefore cloning them merely to satisfy a blanket alias rule would add a
> new semantic boundary without evidence that it is needed.

**Cómo la aplica este paquete:**
- **Las dos referencias:** `PushBackSystem` en `PushBackEditorState.workingBaseline` y `RackModuleReconciliationResult` en `PushBackEditorState.LastModuleReconciliation`, de cada lado; cuatro por estado (D-PBT1-15, condición 1). Todo lo demás se copia en profundidad (D-PBT1-05).
- **Las diez condiciones de seguridad** («L-1 SAFETY CONDITIONS»), una a una, con su subtarea y su prueba, su guarda o su arrastre: la tabla de D-PBT1-15.
- **El límite** («IMPORTANT LIMIT», literal en D-PBT1-15): la parada PBT1x-C17 en los tres contratos (§6.0) y el arrastre A-2 (§10).
- **La vuelta** (`F4B-DRAFTS/pbt1/owner-return-l1.md`) queda respondida: la opción elegida es «Excepción declarada (Recomendado)».

**El caso, tal como se planteó.** El copiador comparte por referencia dos objetos de cada lado: la **línea base resuelta** (`workingBaseline`, un `PushBackSystem` mutable) y el **informe de la última reconciliación** (`LastModuleReconciliation`). La revisión IRT1-1 la consideró **material**, por ser una excepción a la letra de un requisito duro; por eso la decidió el Owner.

**Lo medido (M-1, M-2), la base de la decisión:**
- El mismo objeto es a la vez la línea base, `lastComputation.System`, `session.System` y `lastCompositeSystem`.
- Sus consumidores son el armado siguiente, la sesión de módulos, `ParkTailOf`, el panel compuesto, el criterio de bloqueo, los módulos del panel, la vista previa, el lote de vistas y las peticiones de inserción. **Todos leen.**
- **Solo el Plugin muta:** escribe `Name` **después** de cerrar la ventana, cuando su estado ya se descarta, en `RackPushBackCommands.cs:98` y `:190` y en el lote de vistas del menú (`RackMenuCommands.cs:135`). `CopyStructureSystem` no lleva `Name` al diseño.
- La cola que aparca `ParkTailOf` apunta a configuraciones y líneas de la línea base; el copiador la copia en profundidad.

**Las opciones que se presentaron:**
- **(a) La excepción declarada: ELEGIDA** (F4B-E11, D2). Es el diseño de D-PBT1-05, ahora con las diez condiciones y el límite de D-PBT1-15. **Coste:** cero, sin cambio de comportamiento; las pruebas y las guardas de D2 (CT-PBT1B-14, 15 y 16, CT-PBT1C-04, CT-PBT1B-08 reescrito y la parte 2 de CT-PBT1C-02).
- **(b) Una tarea previa de Domain** (la copia profunda de `PushBackSystem`, con las anulaciones `FrameExceptionOverride` copiadas a mano como hace `RackFrameProjectStore.DeepCopy`): **no elegida.**
- **(c) Volver a resolver la línea base en la copia** (`resolver.Resolve(resolver.Snapshot(<línea base>))`): **no elegida.** El anexo L1-C queda como «no elegido».

**Si una tarea necesitara después mutar uno de los dos objetos dentro del tiempo de vida del borrador o de la edición,** el límite de D2 obliga a parar y reabrir L-1; las salidas que nombra el Owner («be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model») las decide él, no el Worker (PBT1x-C17).

### 8.2 Ya tomadas, que este paquete aplica

- **F4B-E11, D2 (L-1):** «ACCEPT DECLARED EXCEPTION» → §8.1, D-PBT1-05 y D-PBT1-15; el límite → PBT1x-C17 y A-2.
- **F4B-E11, D1 (C14 de PB-TC2):** «CONTINUE WITH THE FIX» → la precondición 1, HT1-15 y §8.5.
- **F4B-E11, «PB-T1 PLAN»:** la división en tres, aceptada en principio → D-PBT1-01; la remedición y la emisión «normally» → la precondición 3 y §8.4.
- **F4B-E09, pregunta 2 (c-02):** «Como hoy, como regla explícita (Recomendado)» → D-PBT1-09 y CT-PBT1B-03.
- **F4B-E09, pregunta 3 (`pb-21-cola`):** «Dejarlo fuera de la garantía (Recomendado)» → D-PBT1-10 y CT-PBT1B-04.
- **F4B-E09, alcance:** los demás casos de T-2, tal cual → CT-PBT1B-05.
- **F4B-E09, pregunta 1:** H-31 y OWNER-1 en PB-TC2, antes de PB-T1 → la precondición 1.
- **F4B-E09, pregunta 4:** la emisión condicional → §8.4.
- **[OD]:** sin MVVM ni estética → D-PBT1-13. **[P] OB-10.1:** cada subtarea en 1/3 → D-PBT1-01.
- **La lectura de la garantía para los heredados** (M-01): «sin pérdidas» quiere decir **salvo la canonicalización T2H-07** de `LegacyDeltas`, que la ventana y el piloto ya aplican igual (CT-PBTC-06). Se declara, no se decide.

### 8.3 Lo que no es decisión (según IRT1-1, y al incorporar D2)

- **L-2. La comparación de c-02.** Es «como hoy»: la regla reproduce lo que ofrece `PostBox` (D-PBT1-09). **El paso 1 elige entre dos ramas ya fijadas** según la fila (b): ordinal, o sin mayúsculas devolviendo el id de la opción (N-M-03). **PBT1B-C14 para** si alguna salida de (a) a (e) no es P o si una fila no coincide con ninguna de sus líneas esperadas. **No es material.**
- **L-3. La división y su procedimiento.** OB-10.1 y D-PB-10 la prevén, y el contenido, el orden y las dependencias de la fase no cambian. El procedimiento (una revisión para las tres y comprobaciones acotadas) es compatible con [P]:515, porque el paquete revisado contiene los tres contratos, y la comprobación acotada queda definida (§6.0). **No es material;** se informa en la emisión con D-PBT1-01.
- **M-07 de PB-TC2 reproducido** (D-PBT1-12): no es una pérdida y OB-RE.1 manda no cambiar RACKEDITAR.
- **`Name` en el estado y el piloto sin tocar:** son OB-05.1 y OB-03.1.
- **L-4. Cómo se lee la condición 7 de D2** («comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects»). El comparador lee de la línea base **solo la proyección declarada**, la intención de módulos (`RackModuleEditSession.Begin(<línea base>.Structure).Commit()` y sus listas de anulaciones; D-PBT1-06, M-15), y nada del informe. Este paquete la lee así:
  - **con dos estados que comparten la misma línea base** (una copia y su original), mutar el objeto compartido no cambia el resultado, dentro o fuera de la proyección: lo prueba CT-PBT1B-16, bloque «comparar»;
  - **con líneas base distintas** (tras recargar), una mutación sí podría cambiarlo; ese caso no se prueba como invariancia, sino que **se prohíbe**: ninguna mutación de los dos objetos dentro del tiempo de vida de la edición (G-L1 y C17), y PB-T4 lo prueba sobre su `DraftSession` (A-2).
  Es una lectura de cómo se prueba la condición, no una decisión nueva. Si el Owner o la revisión la leen de otro modo (por ejemplo, que el comparador no lea nada de la línea base), cambia el diseño de D-PBT1-06 y el paquete vuelve al Owner antes de emitir PB-T1b.
- **L-5. A qué tareas alcanza el límite.** El texto del Owner nombra «PB-T1/PB-T2/PB-T3» y «the Workspace draft lifetime». Este paquete lo aplica **también a PB-T4**, que es la tarea que conecta el `DraftSession` del Workspace al estado (A-2), y, **dentro de PB-T1**, donde aún no hay borrador del Workspace, al tiempo de vida de la edición de la ventana clásica (de la carga al cierre). Es más estricto que la letra (así lo pidió la sesión para esta v2.2). No es una decisión nueva; el Owner puede estrecharlo.
- **L-6. Qué es una «premisa material»** para la emisión «normally» (§8.4). Este paquete la define para que la remedición no tenga latitud. No es una decisión nueva; la revisión puede ajustarla.

**Decisiones materiales nuevas para el Owner, al incorporar D2: ninguna.** Las diez condiciones se prueban en PB-T1 en todo lo que su API permite, o se arrastran a PB-T2, PB-T3 o PB-T4 como obligación de contrato con la parada C17, sin cambiar el plan, el orden ni las dependencias de la fase. Lo que la v2.2 dejó abierto era **medición**, no decisión, y la remedición lo cerró: no hay hoy ninguna mutación de los dos objetos en el camino de edición (R-11) ni un cuarto escritor de `Name` (R-12) (registro N-9 y N-11). **La premisa de D2 se sostiene. Tampoco hay decisiones nuevas en la v2.3.**

### 8.4 Las condiciones de emisión (F4B-E11, «PB-T1 PLAN»)

**El texto del Owner** (F4B-E11, literal): «Re-measure it against the final PB-TC2 product before emission. If that remeasurement changes no material premise: issue the first PB-T1 task normally.» Y en su estado: «v2.1 MAY PROCEED AFTER PB-TC2 CLOSE + REMEASUREMENT». Esta v2.3 es esa v2.1 con D2 incorporada (v2.2) y remedida (v2.3): la división, el orden y las dependencias no cambian.

1. **El cierre de PB-TC2** con D1 («CONTINUE WITH THE FIX»): **hecho.** CLOSED / PASS WITH LIMITATIONS sobre su GREEN `b5c09f0b`, con la custodia del cierre en `cd47523b` (F4B-E12). La parada C14 y D1 están en su evidencia.
2. **La remedición sobre ese producto final:** **hecha** en `cd47523b`, en solo lectura, sin ningún Worker en el worktree. Usó las órdenes del registro y cinco scripts de lectura; la custodia va con la emisión (§6.0). El detalle está en `pbt1-measurements.md`, apartado N, y el resultado en la tabla de abajo.
3. **Ninguna premisa material cambió** (L-6). Por eso, PB-T1a se emite «normally» (F4B-E11) por F4B-E09 y F4B-E11, **tras la comprobación acotada de la remedición** que pide la sesión (F4B-E12, «Siguiente»; §6.0). Las anclas, las líneas, los textos de los mutantes y los recuentos ya están actualizados como datos en esta v2.3 y en el generador del contrato.
4. **Si la comprobación acotada encontrara un cambio material,** el paquete se corrige y vuelve a ella antes de emitir PB-T1a. Si el cambio tocara una premisa de D2 (R-11 o R-12), sería PBT1x-C17 y se reabriría L-1.

**PB-T1b y PB-T1c,** además, con sus precondiciones 4 y 5 y su propia comprobación acotada (§6.0), que revisa también lo de D2: CT-PBT1B-14, 15 y 16, y CT-PBT1C-04, con lo que la remedición fijó en V23-11 a V23-13.

**Qué es una premisa material** (L-6 del §8.3). Lo es si su cambio:
- (i) cambia el alcance de una subtarea (sus rutas, o que una ruta caiga en un `ForbiddenWriteScope`);
- (ii) hace que un mutante ya no pueda hacer fallar el CT de su fila, o que un CT cambie de sustancia;
- (iii) añade una pérdida, o un control fuente de verdad (HT1-15, §5.5);
- (iv) toca una premisa de D2 (R-11, R-12);
- (v) cambia el orden de la carga, el número de pasadas o las dependencias de las subtareas;
- (vi) o pide una decisión del Owner.

**No es material:** un número de línea, un recuento, la línea base de la CI, o el texto literal de un mutante que conserva su objetivo. Tampoco **completar lo que la v2.2 dejó «A MEDIR»** sin cambiar el objetivo del CT: el miembro de lista del informe y la forma de su autoprueba, la huella de CT-PBT1B-16 (R-11 lo previó) y la regex del censo de `Name` (R-12 lo previó).

**Lo que comprobó la remedición, y su resultado** (registro N; las filas V23 del registro de cambios):

| Id | Qué | Resultado sobre `b5c09f0b` | Clase |
|---|---|---|---|
| R-1 | **Las anclas de las zonas que tocó PB-TC2** (§2.8) | Todas en la línea prevista, con el diseño de PB-TC2 v2. `Val` sale; `KeepOptional`, `MixedBoth` y `KeepCell` entran; nada anterior a W:3631 se mueve; W pasa a 3700 líneas (N-0, N-1, N-2) | no material (V23-01, V23-02) |
| R-2 | **Los textos de los quince mutantes** | Trece en su línea. MU-T1A-07a pasa a W:3684 y MU-T1A-10 a W:3666, con texto idéntico y único y el mismo objetivo. MU-T1A-13 se ancla por línea (N-2) | no material (V23-03, V23-04) |
| R-3 | **Los anclajes de D-PBT1-13 y D-PBT1-03** | Todos en su línea y con su texto. `KeepOptional` y `MixedBoth` leen los portadores y `composite`, que PB-T1c reenvía con su mismo nombre (N-3) | no material |
| R-4 | **La pasada reentrante** (HT1-10b, F-03) | El camino no cambia (WC:117-121, W:3277-3289); lo esperado en CT-PBT1A-14, tampoco (N-3) | no material |
| R-5 | **D1 en el producto** | `RackHeaderHeightBox` ya no redondea (W:1639, `KeepOptional`) (N-4) | no material (V23-05) |
| R-6 | **El mapa de controles** | Las lecturas crudas que quedan son enteros o acciones del usuario. Una caja mixta solo tiene valor si el usuario lo escribe. WX no cambia. Solo `PostBox` decide el estado (N-4) | no material (V23-05) |
| R-7 | **La línea base de la CI** | `44313012995` sobre `cd47523b`: UI 2 422 / 21 / 2 401; Core 19 070. Es la previsión exacta (N-5) | no material (V23-06) |
| R-8 | **El BOM por valor exacto (D1) frente a CT-PBT1A-11** | `pb-05-topes` no tiene valores con más precisión de la que se muestra, y CT-11 no pasa por cajas avanzadas: sus líneas no cambian por D1 (N-8) | no material (V23-09) |
| R-9 | **Los recuentos estáticos** | `PushBack*` de UI pasa a 542 (+4 de PB-TC2), `PushBackDeclaredCorrection` a 14 y `PushBackRichPreviewTests` da 9. Lo demás, igual; los prefijos nuevos, libres (N-6) | no material (V23-07) |
| R-10 | **Los alcances y las guardas** | `src/` con 59, 117 y 59, igual. PB-T1a: 131 (55 de `tests/` con las dos clases de PB-TC2). Las guardas de HT1-21 a HT1-24, sin cambio (N-7) | no material (V23-08) |
| R-11 | **D2, condición 10, hoy** | G-L1 da **0 coincidencias** (47 archivos de Application; W y WC). El informe solo admite mutación con un molde: regla 1b, regla 2 con el `foreach`, y autopruebas (c) compilable y (e). La huella pasa a ser profunda, porque `Snapshot` no ve lo derivado ni `Composite` (N-9, N-10) | no material (V23-10 a V23-12): la premisa de D2 se sostiene |
| R-12 | **D2, condición 8, hoy** | No hay cuarto escritor de Push Back. Los tres van tras su `ShowModalWindow`. La regex del censo se corrige: la de la v2.2 daba 10 líneas en todo el Plugin. El informe tiene un consumidor más de lectura, W:2559 (N-9, N-11) | no material (V23-13, V23-14): la premisa de D2 se sostiene |
| R-13 | **Lo demás de la `BaseSha`** | c-02 (W:399, W:868; DA sin cambio), HT1-19 (WC:783-807) y M-6 (W:704, :1688), en su sitio. `docs/automation/agent-execution` sin cambio, así que las celdas siguen y vencen el 2026-12-29. El `config.toml` y el binario los comprueba la sesión al emitir (precondición 6; no medido aquí) | no material |
| — | **Lo que añade el cierre de PB-TC2** | nc1 de PB-TC2 (el sustituto falló por la propiedad 5; aceptación solo para su intento 1); la lección IRC-M02; el arrastre de «Ambos lados» y M-07 a esta red (N-12) | no material (V23-15 a V23-17) |

**Diferencias materiales: ninguna.** La emisión no se para.

### 8.5 D1 del Owner y la vuelta C14 de PB-TC2

- **D1** (F4B-E11, «CONTINUE WITH THE FIX»; «Ratify exact-value preservation»): PB-TC2 sigue con su corrección. La nota de la v2.1 sobre la opción (B) de C14 **ya no aplica**: `RackHeaderHeightBox` no queda como control fuente de verdad, y HT1-15, el mapa de controles (§5.5) y CT-PBT1C-02 no ganan ninguna excepción. **Medido** (R-5; registro N-4): W:1639 usa `KeepOptional` y la caja ya no redondea.
- **La consecuencia que toca a PB-T1** es el BOM: «grouping/equality must not pretend two distinct exact values are equal unless the BOM contract explicitly defines a tolerance». PB-T1 no cambia el BOM ni define ninguna tolerancia; su red lo caracteriza tal como lo deja PB-TC2 (R-8). Con `pb-05-topes`, las líneas de CT-11 no cambian por D1 (registro N-8).
- **Lo que la parada C14 no consumió:** según F4B-E11, «NO ATTEMPT CONSUMED BY THE MEASUREMENT STOP». Eso es de PB-TC2 y no cambia los presupuestos de PB-T1.

## 9. Límites

En `pbt1-measurements.md`, apartados L, M-11 y N-16. En resumen:
- **Nada se compiló ni se ejecutó.** Las líneas esperadas de PB-T1a las mide el Worker; las de PB-T1b y PB-T1c salen de oráculos existentes o son literales. Del generador del contrato solo se ejecutó el `--dry`, que no escribe nada.
- **La v2.2 no leyó el worktree** (un Worker de PB-TC2 trabajaba en él). **La v2.3 lo remidió** en solo lectura, sin Worker, y cerró todo lo que quedaba «A MEDIR» (registro N).
- **No medido:**
  - cómo compara WPF un `SelectedValue` en minúsculas (lo mide el paso 1 de PB-T1b; con el catálogo entregado no cambia la salida);
  - que la sesión y el commit se puedan copiar y comparar como se describe (por lectura de sus campos);
  - si F-03 alcanza en la base exactamente la pasada reentrante descrita (por lectura sí; lo fija CT-PBT1A-14 tal como sea);
  - el `config.toml` frente a la Snapshot I y el binario acreditado (precondición 6; los comprueba la sesión al emitir);
  - los TRX de la CI de `cd47523b`: solo se leyó la línea de resumen de cada trabajo.
- **Lo que G-L1 no ve:** una mutación dentro de un método de otro archivo, Domain incluido. Solo la cubren las huellas profundas de los caminos ejercidos (R-T1-12).
- **Los recuentos son estáticos;** las `Theory` con `MemberData` no se cuentan.
- **Una incidencia de procedimiento en la remedición** (registro N-15): una orden de solo lectura usó `python -c` para imprimir las celdas del contrato de PB-TC2. Incumple la regla. No se repitió, y el generador lo vuelve a comprobar con sus `assert`.

## 10. Arrastres que salen de PB-T1

| Id | Qué | Va a |
|---|---|---|
| A-1 | El modo WORKSPACE, la prueba UI-STA de OB-01.3, el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE, y el dueño de los subdiálogos | PB-T2 (sin cambio) |
| A-2 | El borrador sobre `PushBackEditableState`: el copiador y el comparador de PB-T1 sustituyen a los del piloto; la composición y el clasificador de D-PB-05 con el mapa de D-PBT1-08; la barrera de PREPARE con `PushBackEditorOutputGate`; una API de asiento, si PB-T3 o PB-T4 la necesitan (I-03). **Y D2 (F4B-E11), como obligación de contrato** (abajo, «A-2, D2») | PB-T2, PB-T3 y PB-T4 |
| A-3 | Quitar el rechazo A/B del piloto (OB-03.2) con PB-T1 cerrada; `DormantTailOutOfPilotReason` se queda en todo F4B | PB-T3 o PB-T4 |
| A-4 | La pregunta al Owner sobre la `ExtensionData` anidada (OB-05.2) | Antes del contrato de PB-T3 |
| A-5 | La copia de la carga del piloto (PM:150-172) | PB-T4 la retira al consumir el API |
| A-6 | M-07 (reproducido), la caja de c-02 que se ve vacía y M-01 de PB-T0 (no corregido) | `docs/ideas-futuras.md`, por la sesión |

### A-2, D2: lo que heredan PB-T2, PB-T3 y PB-T4

**El límite, literal** (F4B-E11, D2, «IMPORTANT LIMIT»), que cada uno de esos contratos copia como su parada C17 (con su prefijo):

> This is NOT authorization for general shallow copying.
>
> It is exactly:
>
> two declared shared references.
>
> If later PB-T1/PB-T2/PB-T3 needs to mutate either:
>
> * resolved rack;
> * module report;
>
> inside the Workspace draft lifetime:
>
> STOP and reopen L-1.
>
> At that point the object must either:
>
> * be cloned;
> * be made immutable;
> * or be replaced by a value snapshot appropriate to the draft model.

El texto nombra PB-T1, PB-T2 y PB-T3; este paquete lo extiende a PB-T4, que conecta el `DraftSession` al estado (L-5 del §8.3).

**Las obligaciones, por condición:**

| Condición de D2 | Tarea | Qué prueba su contrato |
|---|---|---|
| 3. editar en el `DraftSession` | PB-T4 | Editar, aplicar al borrador y fijar el estado actual no mutan las dos referencias del estado inicial ni del actual (huellas como CT-PBT1B-16) |
| 4. validar | PB-T2 y PB-T4 | Su validación única de campos ([OD]) y la de la frontera del borrador no mutan las dos referencias |
| 5. texto pendiente y edición | PB-T4 | Confirmar o rechazar el texto pendiente (`IDraftPendingTextAdapter` y la frontera de F4) no muta las dos referencias |
| 6. descartar y recargar | PB-T4 | Descartar, recargar, «Conservar mi borrador» y la nueva base del `DraftSession` no mutan las dos referencias |
| 7. comparar el estado inicial y el actual | PB-T4 | Su `IsDirty` usa el comparador de PB-T1; con su `DraftSession`, el resultado no depende de cambios dentro de los dos objetos (L-4) |
| 8. el ajuste de `Name` | PB-T3 y PB-T4 | El escritor del panel (PREPARE, MUTATE y POST) no ajusta `Name` sobre un sistema que comparte un estado del borrador mientras el borrador vive: lo hace sobre una copia o fuera de esa vida. CT-PBT1B-15 sigue en verde; si el panel añade un escritor de `Name`, amplía su censo con una prueba propia |
| 10. las guardas | PB-T2, PB-T3 y PB-T4 | Cada una **añade a la lista de archivos de G-L1** los suyos del camino de edición y del borrador (fuera de `src/RackCad.Application/Workspace/`, que ya cubre CT-PBT1B-14), con una prueba propia y sus autopruebas en negativo; ninguna quita un archivo ni una ficha |

**La regla para todas:** las guardas de D2 de PB-T1 (CT-PBT1B-14 y 15, CT-PBT1C-04) siguen en verde sin tocarlas; ningún copiador nuevo comparte por referencia nada fuera de las dos referencias; y si cumplir la tarea exigiera mutar uno de los dos objetos dentro del tiempo de vida del borrador: STOP y se reabre L-1.

## Anexo L1-C. No elegido

No elegido (F4B-E11, D2 eligió (a)); su texto sigue en la v2.1 (`I-64-f4b-pbt1-gate-contract-package-v21.md`).
