> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v3-delta: los hallazgos de la recomprobación de la v2, uno a uno

**Qué es:** para cada hallazgo de la recomprobación de la v2 (`independent-review-v1.md`, §10: NECESITA v3, B/I/M 0/3/4), el cambio de la v3, con su id del registro de cambios (V3-xx). Todo se leyó por objetos git desde el checkout principal, en `833ea6bf`, la HEAD de la rama.

**Resultado:**
- **los siete hallazgos se aplican,** N-01 a N-07, cada uno con su cambio (V3-01 a V3-07);
- **dos con una precisión** respecto de la recomprobación, que se explica abajo:
  - N-02: los miembros privados de `Execution` que cambian de firma son tres más de los que nombraba la recomprobación;
  - N-04: el censo de S-6 se define también, no solo el de S-2;
- **una corrección del redactor** al aplicar N-03 (V3-08): la medición sobre el GREEN y el invariante común 07 de PB-T3b llevaban el mismo número;
- **ninguna decisión material nueva.** D-1 sigue siendo la única, con la pregunta corregida (abajo).

| Hallazgo | Grav. | ¿Se aplica? | Cambio | Id | Dónde |
|---|---|---|---|---|---|
| **N-01** La pregunta de D-1 no es simétrica sobre PB-T4 | I | Sí | Las dos opciones dicen lo que dejan a PB-T4:<br>• con (A), **dos**: PB-T4 sustituye con registro E-3 y E-5 al quitar el piloto;<br>• con (A′), **cinco**: S-3, S-5, S-6, E-1 y E-4.<br><br>Las dos suman ocho en F4B. «Ajustar E-5» pasa a «sustituir con registro», porque en F4B no se edita ninguna prueba existente. La tabla de §8.2 gana las filas «Después, en PB-T4» y «En total». También cambian D-PBT3-09b, la cabecera y A-T3-1 | V3-01 | §8.2; D-PBT3-09b; cabecera; §10 |
| **N-02** PBT3B-C13 prohíbe lo que exige (A); dos reglas para los privados | I | **Sí**, con una precisión | **Una regla por contrato** (INV-13):<br>• PB-T3a, solo con archivos nuevos: la lista de PBT3A-C13 más los ayudantes privados, como en la v2;<br>• PB-T3b y PB-T3c, sobre archivos existentes: solo la lista de su C13, **por nombre, acotada a los archivos que tocan y con las sobrecargas**, en la forma de PBT1C-C13 de la v2.8 custodiada (`:1338` en `833ea6bf`). Ningún ayudante privado fuera de ella.<br><br>**PBT3B-C13** enumera las 34 declaraciones que prescribe (A), sobrecargas incluidas, y su firma solo para ante un miembro **público o interno**. Declara los dos cambios privados: `ComposeMemberEnvelope` gana `string name`; `Execution` pasa a `Execution<TState>`, pierde `_input`, y su constructor gana los tres delegados. **Precisión:** además de los que nombraba la recomprobación, `ChooseOrigin`, `SurvivorsAndLayers` y el campo `_draft` llevan `PushBackPilotDraftState` en su firma (UR:229, :521, :537; R-18).<br><br>La variante (A′) tiene su propia lista normativa. **PBT3C-C13** pasa a la misma forma, con cinco declaraciones, y declara el parámetro nuevo del constructor interno de `RackPilotMutationScope`. Las dos entregas traen el censo de declaraciones | V3-02 | §6.0; §6.2; §6.3; D-PBT3-09 a 11; R-18 |
| **N-03** La medición sobre el GREEN choca con INV-07 y C02 | I | Sí | El invariante común 07 y C02 exceptúan **solo** la reactivación **temporal, local y sin commit** de los métodos de D-1 para la medición; el Worker la deshace antes del commit GREEN. La `ExpectedEvidence` de PB-T3b pide que el `git diff -U0 BaseSha..CurrentSha` de los archivos de guardas solo lleve las líneas de atributo con `Skip` | V3-03 | §6.0; §5.3; §6.2 |
| N-04 Con (A′), «Quedan vacías: ninguna» | M | **Sí**, con una precisión | La celda dice: las filas 2 a 5 de S-2. La medición esperada trae, en las dos variantes, una línea por fila de S-2 (`from-presente`, que lee la `Assert.Contains(from, plan)` de PrepareGuard:136). **Precisión:** el censo de S-6, que la v2 no definía, pasa a ser `vacia`, cierto si S-5 falla sobre el mismo texto | V3-04 | §8.2; §5.3; D-PBT3-09b; §4 |
| N-05 Con (A′), el `ForbiddenWriteScope` deja fuera dos archivos, no tres | M | Sí | «Menos los archivos de guardas de la variante elegida»: 76 de 79 con (A), y 77 con (A′), con `WorkspacePilotUpdateIntegrationGuardTests.cs` prohibido. El `AllowedWriteScope` dice 11 con (A) y 10 con (A′) | V3-05 | §6.0; §6.2; R-15 |
| N-06 CT-PBT3B-07 (ii) pide `Ready` sobre un borrador limpio | M | Sí | Antes del segundo `Prepare`, el estado actual se cambia (la escala a 3,0 con `SetCurrentDraftState`). Sin el cambio daría `NothingToApply` | V3-06 | §5.3 |
| N-07 El estado de PB-T1 cambió | M | Sí | Medido en `833ea6bf`:<br>• PB-T1b, CERRADA (F4B-E26; CI `44438060317`: Core 19 086, UI 2 437);<br>• PB-T1c, EMITIDA (F4B-E27; `AuthorityRevision` `af46fcd5`);<br>• F4B-E24 y F4B-E25, en git desde `95373af6` (`f4b-dispositions.md:684` y `:693`);<br>• `git diff 2f5d3dec 833ea6bf -- src`, vacío.<br><br>Se ponen al día las precondiciones 1 y 8, §1, §9, R-0 y R-17. No cambia ningún CT | V3-07 | Precondiciones; §1; §9; R-0; R-17 |
| *(del redactor)* La medición y el común 07 con el mismo número | — | Sí | La medición sobre el GREEN pasa de INV-PBT3B-07 a **INV-PBT3B-15**, detrás de los comunes 07 a 14. El registro v1 → v2 conserva el nombre antiguo | V3-08 | §5.3; §6.2; §8.2; §9; R-11 |

## Lo que no cambia

- **El diseño:** B-01 sigue resuelto con el reanclaje, y la barrera del piloto sigue declarada y probada. La recomprobación no encontró regresiones (su §10.6).
- **D-1, exacta:** seis con (A) y tres con (A′) en PB-T3b, como midió la recomprobación (su §10.4).
- **D-2:** sigue siendo un registro declarado, sin `Skip`. La recomprobación lo da por resuelto (I-10).
- **Ningún CT nuevo ni retirado.** Cambian el texto de CT-PBT3B-07 (ii) y la medición sobre el GREEN.

## Lo que no se pudo medir

- **Nada se compiló ni se ejecutó,** como en la v1 y la v2.
- **El censo de declaraciones** de PB-T3b y PB-T3c: lo que se añade sale del diseño (34 con (A) y 5 en PB-T3c). Lo mide el Worker en su entrega.
- **Lo demás,** como en la v2 (R-11): la TRX de una `Theory` con `Skip`, el Deshacer real con borrado y el estado de bloqueo de los diseños de las clases de F4.

## D-1, la pregunta lista para AskUserQuestion

- **header** (10 caracteres): `Guardas F4`
- **question** (373 caracteres, contados con `count_question.py` sobre el paquete): «PB-T3b: para que el «Actualizar» del panel renombre y borre vistas, hay que sustituir pruebas de F4 que fijan el texto del escritor del piloto, cada una con registro y una prueba equivalente. Con un solo código para el piloto y el borrador completo son seis ahora y dos en PB-T4; con el borrador completo en código duplicado hasta PB-T4, tres ahora y cinco en PB-T4. ¿Cuál?»
- **options** (la recomendada primero):

  | label | description |
  |---|---|
  | `Un solo código (Recomendado)` | Un único algoritmo de escritura para el piloto y el borrador completo. PB-T3b sustituye seis pruebas de F4 (cinco fallan y una quedaría vacía), cada una con su equivalente; al quitar el piloto, PB-T4 sustituirá dos de esos equivalentes. Ocho en total. |
  | `Código duplicado hasta PB-T4` | El texto del piloto no cambia, salvo la barrera, y el borrador completo va en código propio, copiado, hasta PB-T4. PB-T3b sustituye tres; al quitar el piloto, PB-T4 sustituirá cinco (tres pruebas de F4 y dos equivalentes). Ocho en total. |
  | `Sin renombrar ni borrar` | No se sustituye ninguna. El panel no renombra ni borra vistas en PB-T3: esas dos cosas, que pide la fila de PB-T3, quedan pendientes. |
- **multiSelect:** no.
