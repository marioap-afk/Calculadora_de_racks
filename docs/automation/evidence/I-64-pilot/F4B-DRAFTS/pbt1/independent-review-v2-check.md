> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. El worktree se cita como `<worktree>` y las rutas del repo son relativas.

# I-64 F4B — Comprobación acotada de la v2 del paquete de PB-T1 (tras IRT1-1)

**Veredicto:** **EMITIBLE TRAS CORRECCIONES — B/I/M abiertos = 0 / 1 / 10.**
- **Para el Owner:** una sola decisión, **L-1**, que sigue abierta. **Ninguna decisión material nueva.**
- **El importante abierto** no está en el paquete, sino en la pregunta de L-1 (`owner-return-l1.md`). Hay que corregirla **antes** de llevarla al Owner.
- **Los diez menores** se corrigen en el texto, sin otra revisión.
- **PB-T1a** se emite por la autorización condicional (F4B-E09, pregunta 4) cuando el Owner responda a L-1 y PB-TC2 cierre (precondición 1).

## 1. Método y límites

**Las entradas:**
- la v2 del paquete (citada `T2:n`), con su registro de cambios y el anexo L1-C;
- el apartado M de `pbt1-measurements.md`;
- `v2-instructions.md`;
- `owner-return-l1.md`;
- el informe de la v1 (IRT1-1).

**Contrastadas en `<worktree>`, HEAD `eedb069c`,** con lectura directa del código y de las pruebas (`git -C`, `sed` y `grep`). Volví a ejecutar `count_tests.py` con `python -I` para los tres recuentos nuevos (7, 17 y 25: coinciden).

**Reglas:** sin compilar, sin ejecutar pruebas y sin escribir en el worktree. No escribí ningún Python nuevo.

**Límites.** Como en la v1, el comportamiento de WPF y los efectos de borde son **lectura de código**. En particular, que F-03 alcance en la base la pasada reentrante: por lectura, el refuerzo de 100000 lanza en `ValidateReinforcementAgainstPost` (`PushBackAdvancedRackParameters.cs:85-99`), y la caja no tiene máximo (WX:510-511).

## 2. El estado de los hallazgos de la v1

| Id | Estado | Evidencia |
|---|---|---|
| I-01 | **RESUELTO** | L-1 reescrita con las tres opciones (T2:834-845). HT1-12 corregido (T2:116-120); comprobado: `ToDesign` comparte `AssociatedFrameConfiguration` (`PushBackCompositeStructure.cs:800-811`) y `ParkTailOf` aparca líneas e intenciones del commit por referencia (CA:180-242). M-1 amplía los consumidores (T2:122), aunque se deja uno (N-M-01). CT-07 parte 2 va sin exención (T2:540), con un punto ciego (N-M-05) |
| I-02 | **RESUELTO** | `IsImmutable` copiado (`WorkspaceDraftPilotStateTests.cs:531-536`) y diseños nombrados (T2:540). `SafetyFacePiece` es inmutable: campos de solo lectura, `None` estático |
| I-03 | **RESUELTO** | El ayudante `Pasada` (T2:430-450) reproduce W:659-704 en lo que toca al estado: `AcceptComputation` sobre el lado A (W:675), `CommitDormantTailConsumption` (W:679), D-PB-11 (W:696-702) y la apertura de la sesión (W:704 → W:1688). Lo que omite es solo interfaz. Las secuencias de pasadas (T2:452-459) cuadran con W:280-288, W:439 y W:3391. El comparador lleva la regla de la sesión (T2:255), algo laxa (N-M-02) |
| I-04 | **RESUELTO** | `AdoptCarriers` es la primera sentencia del `try` de `LoadFromModel`, bajo `suppressSync` (T2:364). Así la pasada reentrante de `UpdateCompositeEnabled` (WC:117-121 → W:3277-3289) queda **antes** de la adopción, como hoy. Comprobado: ninguna siembra de W:390-421 lee un portador. F-03 (T2:428) ejerce el camino, pero falta que se compruebe a sí mismo (N-M-08) |
| I-05 | **RESUELTO** | La copia es literal de los ocho campos (T2:212). `DynamicRackModuleDesign` tiene exactamente esos ocho (`DynamicRackDesign.cs:113-128`), y `DynamicHeaderLineOverride` tres (`DynamicHeaderLineOverride.cs:32-41`). `DeepCopy` clona las anulaciones de tiempo de ejecución (`RackFrameProjectStore.cs:87-114`) |
| I-06 | **RESUELTO** | PBT1A-C04, PBT1B-C04 y PBT1C-C04, cada una con el texto de su régimen (T2:720, :761, :793). PBT1A-C15 está mal colocada (N-M-07) |
| I-07 | **RESUELTO** | La API exacta (T2:277-297) y la fila `$.Design`. Comprobado: las rutas de la tabla son las propiedades declaradas de `RackEmbedDocument` (`:35-75`), `RackProjectDocument` (`:18-52`, `Cantilever` incluida) y `PushBackDesignDocument` (`:24-76`), y los tres tienen `[JsonExtensionData]` |
| I-08 | **RESUELTO** | La precondición 5 (T2:32) y PBT1A-C15 (T2:721). `UnexpectedFindings` existe en `worker-handoff.schema.json:55` |
| M-01 | RESUELTO | T2:535 y T2:856 |
| M-02 | RESUELTO | Recetas en T2:424-428. X-01 de la ventana = `BuildX01` (`PushBackFullEditorCharacterizationStateTests.cs:90-113`). La receta de Core usa la misma API que la casilla «En blanco» (`composite.SetSlotPresent`, W:1146-1148). En `pb-20-ab` hay separadores `M2…` y `B:M7…`, así que F-01 es construible |
| M-03 | RESUELTO | T2:492 y T2:348 |
| M-04 | RESUELTO | T2:488 |
| M-05 | RESUELTO, con una incoherencia nueva (N-M-03) | T2:321-325 y T2:749-759 |
| M-06 | RESUELTO | T2:748 y T2:304-313 |
| M-07 | RESUELTO, con un hueco nuevo (N-M-04) | T2:545. Los textos coinciden con `PushBackCompositeDiagnostics.cs:94-140`. Por lectura, `Valid()` no da aviso, así que «RESERVA» se sostiene |
| M-08 | RESUELTO, con un detalle nuevo (N-M-06) | T2:539 |
| M-09 | RESUELTO | T2:264-268. El constructor `internal` está en RMES:18 |
| M-10 | RESUELTO | T2:739; recuentos 25, 7 y 17 |
| M-11 | RESUELTO | T2:667-669 y T2:461. F4-E56 está en `f4-dispositions.md:1917-1928` |
| M-12 | RESUELTO | T2:610-621 y T2:819-820 |
| M-13 | **PARCIAL** | Corregidas `:232-244`, `DDA` y las celdas `:51`/`:97`. Queda una: la definición de H-PB-44 se cita en [PKC] `:355` (T2:126), pero esa fila no la contiene. La formulación está en H-31 ([PKC] `:187`) y en `:1343` (N-M-10) |
| M-14 | RESUELTO | T2:315 |
| M-15 | RESUELTO | T2:253-254 |

## 3. Los hallazgos nuevos

### 3.1 Lo nuevo de la v2, atacado

- **La medición M-1.** Es correcta en lo esencial: todos los consumidores de la ventana leen, y el `Name` se escribe después de cerrarla. Falta un tercer escritor del Plugin del mismo tipo, también después de cerrar la ventana: el lote de vistas del menú, `RackMenuCommands.cs:135` (`pushBack.System.Name = pushBackName;`). `ProjectVariableMutationExecutor.cs:169` resuelve su propio sistema (Selectivo) y no cuenta (N-M-01).
- **La nueva CT-07.**
  - Las partes 1 y 3 son alcanzables con `IsImmutable`.
  - La parte 2 recorre con el `AllObjects` del piloto (`WorkspaceDraftPilotStateTests.cs:501-529`). Ese recorrido no entra en los valores de un `Dictionary`: `KeyValuePair` es un tipo valor y el diccionario no es un tipo de producto. Por eso no ve las configuraciones de línea de la sesión (`committedLines` y `workingLines`, RMES:82-83), que son `RackFrameConfiguration` mutables (N-M-05).
- **El ayudante `Pasada`.** Es fiel (§2, I-03). Comprobado que la ventana nueva también hace una pasada en el constructor (`InitializeCompositeSection` → `UpdateFrontalButtons`, WC:57 → WC:110-112). No deja rastro, porque `LoadNew` llama a `AdoptLoadedBaseline(null)` (EL:43). La secuencia «ventana nueva» vale.
- **La equivalencia de la sesión nula y la recién abierta.** Basta para CT-PBT1C-01, pero es laxa: una sesión sin cambios pendientes puede guardar la lista confirmada de una línea base anterior con la misma firma (`ReseedModuleSession`, ES:176-187), y esa lista condiciona lo que se escenifique después (N-M-02).
- **`AdoptCarriers` en `LoadFromModel` y F-03.** Correcto (I-04). F-03 fija una huella, pero no prueba que la pasada reentrante ocurriera: si en la base no ocurriera, el guion quedaría vacío sin que nadie lo note (N-M-08).
- **La copia literal de la cola.** Correcta (I-05). Para el almacén de `DeepCopy` del estado, T2:212 no dice dónde vive (N-M-06).
- **Las C04 y la C15 con la precondición 5.** Las C04 son correctas. La C15 está bien como regla, pero figura entre las `StopConditions` aunque «no es una parada» (N-M-07).
- **Las firmas del mapa.** Correctas y coherentes con los documentos (§2, I-07).
- **c-02 y el paso 1.** La regla imita `ToOptions` (`UiSupport.cs:124-133`), así que con espacios o duplicados por mayúsculas da la salida de hoy. Pero la v2 deja de parar si (b) da `SelectedId=P` (T2:759), mientras el §8.3 sigue diciendo que PBT1B-C14 «la para» (T2:860). Si WPF comparara sin mayúsculas, la regla del estado (ordinal) se apartaría de RACKEDITAR con un catálogo editado de varios postes. La ventana de PB-T1c no lo notaría, porque `PostBox` gana, pero el Workspace heredaría una regla distinta de «como hoy» (N-M-03).
- **Los cuatro bloqueantes en la paridad.** CT-PBT1B-01 (T2:534) los compara con la ventana, pero dos no se alcanzan con los controles:
  - el hueco negativo: `GapBox` tiene `Minimum="0"` (WX:148) y `Gap_Changed` no escribe con error (WC:192-195); además el documento lo normaliza a 0 al cargar (`PushBackDesignDocument.cs:469`);
  - la estructura manual de 1: `StructureOverrideBox` tiene `Minimum="2"` (WX:235).

  Falta decir cómo se producen en la ventana (N-M-04).
- **El anexo L1-C.** Es aplicable. Con el resolver, `Snapshot(PushBackSystem)` y `Resolve(PushBackDesign)` existen (`PushBackResolver.cs:387`, `:41`), y el paso 1b con PBT1B-C16 cubre el riesgo de los compuestos. Dos detalles:
  - «INV-PBT1B-07 nuevo» (T2:915) choca con el común INV-PBT1x-07 («Las redes»);
  - el anexo no dice que toda llamada a `Copy` (CT-PBT1B-06 a 10 y CT-PBT1C-02) pasa el resolver del mismo catálogo (N-M-09).

### 3.2 La pregunta de L-1 (`owner-return-l1.md`): no es del todo fiel ni sin sesgo

Está en lenguaje llano y con las tres opciones, pero tiene cuatro defectos. Los cuatro empujan hacia (a):

1. **«resultados calculados, no datos que el usuario edita».** La línea base es calculada, pero **es donde viven las personalizaciones de módulos confirmadas** entre recálculos:
   - «whose MODULAR structure — custom cabeceras and manual fondos — the next recompute preserves» (ES:66-71);
   - «The intents themselves survive inside the new baseline» (ES:163-164).

   Sin decirlo, el Owner subestima qué se comparte.
2. **«nadie las modifica»** contradice la medición M-1: el Plugin escribe su `Name` al dibujar, después de cerrar la ventana (`RackPushBackCommands.cs:98`, `:190`; `RackMenuCommands.cs:135`). Hoy es inocuo, pero no es «nadie».
3. **(a) se presenta con «coste cero»** y sin su riesgo residual: si algún día un código modifica ese objeto, la copia y el original cambian a la vez, y las pruebas solo vigilan a los consumidores de hoy. Por eso existe la obligación A-2.
4. **(b) dice que las configuraciones de cabecera «tienen un historial de errores en tiempo de ejecución».** Es inexacto. Lo de I-17 son las **anulaciones manuales de la cabecera que solo viven en memoria** (`FrameExceptionOverride`, que `DeepCopy` vuelve a pegar; `RackFrameProjectStore.cs:78-114`), no errores. La frase de IRT1-1 («excepciones de tiempo de ejecución») era ambigua y la heredó §8.1 (b) (T2:843): hay que corregirla también ahí.

Además, a (c) le falta decir que, si la medición de los racks A/B falla, vuelve al Owner (PBT1B-C16).

**Corrección propuesta** (N-I-01), con el mismo formato:
- «La línea base es calculada, pero guarda las personalizaciones de módulos ya confirmadas.»
- «Hoy solo se le cambia el nombre al dibujar, cuando la ventana ya se cerró.»
- En (a): «el riesgo es que un cambio futuro en ese objeto afecte a las dos copias; las tareas siguientes quedan obligadas a no hacerlo».
- En (b): «toca las configuraciones de cabecera, cuyas anulaciones manuales solo viven en memoria y hay que copiar a mano».
- En (c): «si la prueba con racks A/B falla, vuelvo a ti».

### 3.3 Tabla de hallazgos nuevos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| N-I-01 | **IMPORTANTE** | La pregunta de L-1 no es fiel ni sin sesgo (§3.2) | `owner-return-l1.md:15-31`; ES:66-71, :163-164; `RackPushBackCommands.cs:98`, `:190`; `RackMenuCommands.cs:135`; `RackFrameProjectStore.cs:78-114`; T2:843 | La redacción de §3.2, también en T2:843 |
| N-M-01 | MENOR | M-1 y §8.1 omiten el tercer escritor de `Name` (el lote del menú) | `RackMenuCommands.cs:135`; T2:831 | Añadirlo |
| N-M-02 | MENOR | Regla de la sesión laxa: dos sesiones sin cambios pendientes, iguales aunque una guarde la lista confirmada de una línea base anterior | T2:255; ES:176-187 | Sin cambios = nula o sin pendientes **y** con su lista confirmada equivalente a `Begin(<línea base>).Commit()` |
| N-M-03 | MENOR | La fila (b) del paso 1 ya no para, y el §8.3 dice que sí. Si WPF compara sin mayúsculas, la regla del estado se aparta de RACKEDITAR con catálogos editados | T2:759, :763, :860 | Fijar la rama: si (b) da `P`, la regla compara sin mayúsculas y devuelve el id de la opción, como `PostBox`. Alinear el §8.3 |
| N-M-04 | MENOR | Dos de los cuatro bloqueantes de CT-PBT1B-01 no se alcanzan con controles, y el hueco negativo tampoco por documento | WX:148, :235; WC:192-195; `PushBackDesignDocument.cs:469`; T2:534 | El hueco con la costura `CompositeState.SetGap(-1)` y un disparo de recálculo; la estructura de 1 por documento (`EditInner`) |
| N-M-05 | MENOR | La parte 2 de CT-07 no ve los valores de los diccionarios de la sesión | `WorkspaceDraftPilotStateTests.cs:501-529`; RMES:82-83 | Recorrer también los valores de cualquier `IDictionary` (o los campos de referencia de los `KeyValuePair`) |
| N-M-06 | MENOR | El `RackFrameProjectStore` del estado, si fuera un campo de instancia, rompería el censo de CT-PBT1B-06 e INV-PBT1B-03 | T2:212, :539, :745 | Local o `static readonly` |
| N-M-07 | MENOR | PBT1A-C15 es una declaración, pero va en `StopConditions` con id (A5 las compara por id) | T2:721 | Moverla a un invariante (ya está en `ExpectedEvidence`) |
| N-M-08 | MENOR | F-03 no comprueba que la pasada reentrante ocurrió | T2:428, :495, :890 | Añadir a su línea `pasadas-restaurar=<RecomputePassesForTest después − antes>` y `frontal=<FrontalSideForTest>`. Esperado: 2 y A |
| N-M-09 | MENOR | L1-C: «INV-PBT1B-07 nuevo» choca con el común 07; no dice que `Copy` lleve el resolver en todos sus usos | T2:910, :915 | Renumerarlo (INV-PBT1B-07b) y añadir la frase |
| N-M-10 (resto de M-13) | MENOR | La definición de H-PB-44 se cita en una fila que no la contiene | T2:126; [PKC] `:187`, `:355`, `:1343` | Citar `:187` y `:1343` |

## 4. Veredicto

**EMITIBLE TRAS CORRECCIONES — B/I/M abiertos = 0 / 1 / 10.**

- **Los 23 hallazgos de IRT1-1:** 22 RESUELTOS y M-13 PARCIAL.
- **El importante abierto (N-I-01)** es la pregunta de L-1. Se corrige antes de llevarla al Owner; no exige otra v3 del paquete, salvo T2:843.
- **Los diez menores** se aplican en el texto de emisión de cada subtarea, sin otra comprobación.
- **Las decisiones para el Owner:** **L-1, abierta** (con (a), recomendada; (b); y (c), con su anexo). **Ninguna decisión material nueva.**
- **Las condiciones para emitir PB-T1a** (T2:867-873):
  1. la respuesta a L-1 con la pregunta corregida;
  2. el cierre de PB-TC2 con su diseño v2. La vuelta C14 sigue abierta, y si el Owner elige (B), se aplica la nota T2:875-878 y esa parte se vuelve a redactar.
