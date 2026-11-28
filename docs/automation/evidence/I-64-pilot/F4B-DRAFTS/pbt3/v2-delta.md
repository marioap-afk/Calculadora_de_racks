> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v2-delta: los hallazgos de la revisión de la v1, uno a uno

**Qué es:** para cada hallazgo de `independent-review-v1.md` (NECESITA v2, B/I/M 1/11/16), y para las cuatro lecciones de PB-T1b que pidió la sesión, el cambio de la v2 (con su id del registro de cambios, V2-xx) o por qué no se aplica tal cual.

**Resultado:**
- **los 28 hallazgos se aplican.** Tres, con una precisión respecto de la revisión, que se explica:
  - I-01: la variante (A′) exige S-4, además de S-1 y S-2;
  - B-01: se elige el reanclaje y no el rechazo;
  - M-14: el número F4B-E de D-1 queda como marcador hasta que la sesión lo registre.
- **Tres menores se resuelven declarándolos,** porque su arreglo es de otra tarea: M-10, M-15 y la parte de coste de M-09. La revisión lo admite así.
- **Las cuatro lecciones, aplicadas:** V2-29 a V2-32.
- **Decisiones para el Owner:** **D-1**, reformulada, con su pregunta. D-2 no es una decisión: es un registro declarado. El bloqueante se resuelve **sin** decisión nueva.

| Hallazgo | Grav. | ¿Se aplica? | Cambio | Id | Dónde |
|---|---|---|---|---|---|
| **B-01** Borrar la vista de origen rompe POST | B | **Sí**, con el reanclaje | El plan gana `PostOriginKey`: el origen, o la primera clave `Redraw` en ordinal si el origen se borra. POST relee con ella y el borrador se rebasa con esa lectura. Casos nuevos en CT-PBT3B-02 (iii) y 07 (ii), una fila del Smoke-PB, R-T3-18 y el arrastre A-T3-8. **Por qué reanclar y no rechazar** (la revisión admitía las dos): es la igualdad con el clásico, que borra la vista elegida si queda alguna superviviente (RPC:198, :348-352). Además, tras el «Actualizar» todos los miembros llevan el mismo diseño, así que cualquier redibujado es un origen equivalente. Rechazar haría al panel más estricto que el clásico, con un texto nuevo y sin necesidad | V2-01 | D-PBT3-09 (h); D-PBT3-10; §5.3; §5.5; §7; §10; R-13 |
| **I-01** D-1 mal enunciada | I | **Sí**, con una precisión | (a) S-6 y tres filas de S-2 «quedan vacías»; D-1 dice «cinco fallan y una queda vacía». (b) La exactitud se mide **sobre el texto del GREEN**, con una corrida temporal del Worker (INV-PBT3B-07; C16). (c) Se añade (A′) (D-PBT3-09b), con sus costes. **La precisión:** la revisión decía que (A′) solo exige S-1 y S-2. Medido por lectura, exige también **S-4**, que cuenta un solo `mutation.Dispose(` y un solo `scope.Dispose(` en todo UR (UpdateGuard:134-135): una segunda corrida en el mismo archivo la rompe, y cambiar los nombres para esquivarla sería debilitarla. La pregunta, lista (§8.2) | V2-02 | §8.2; D-PBT3-09b; §5.3; §6.2; R-6; §9 |
| **I-02** E-3 no cubre S-3 | I | Sí | E-3 lee `BuildCore` y el cuerpo de `Of`, con todas las agujas de S-3 | V2-03 | §5.3 |
| **I-03** E-2 y E-6 pasan en el RED | I | Sí | Las dos empiezan con `Assert.Contains` de cada literal de la forma nueva y `Assert.Empty(<reglas>(real))`; E-6 exige además que cada mutación cambie el texto | V2-04 | §5.3 |
| **I-04** CT-PBT3C-05 lee una fachada que el RED no crea | I | Sí | El RED de PB-T3c crea el esqueleto inerte de la fachada (cuerpo `throw new NotSupportedException();`) | V2-05 | §5.4; §6.3 |
| **I-05** Forbidden corta Allowed en PB-T3b | I | Sí | El prefijo de `Workspace/` se sustituye por la lista explícita de 76 archivos (79 menos los tres de guardas), medida por el generador; el doble sale de `Workspace/` | V2-06 | §5.0; §6.0; §6.2; R-15 |
| **I-06** Sin constructores en la API cerrada | I | Sí | Todos los constructores, con su visibilidad y firma, en D-PBT3-02 a 07; PBT3A-C13 por nombre | V2-07 | §3; §6.1 |
| **I-07** CT-PBT3B-10 no llega a AUTH-13 | I | Sí | La receta con la lectura admitida fabricada (`BuildStaleBase` del mismo escaneo), sus líneas literales y su autocomprobación. Se declara que en uso real AUTH-13 actúa al abrir y al escribir solo aparece el Conflict | V2-08 | §5.3; D-PBT3-13 |
| **I-08** La barrera del piloto sin prueba | I | Sí | CT-PBT3B-06 (ii): un borrador del piloto sobre `pb-01-base` con `StructureOverrideA = 1` → `Invalid(BlockedPrefix + StatusOr)`, que se comprueba a sí mismo. `Blocking()` es compuesto y no sirve. El cambio visible queda declarado en el resumen, D-PBT3-09 (d2), INV-PBT3B-04 y §8.3. El censo de los diseños de F4, a la remedición | V2-09 | Cabecera; §3; §5.3; §6.2; §8.3; R-14 |
| **I-09** Snapshot I anticuada | I | Sí | Snapshot J (F4B-E19) en la precondición 6, P-01, §1 y la remedición | V2-10 | Precondiciones; §1; §6.0 |
| **I-10** D-2 sin registro | I | Sí | Entrada en `superseding-characterization.md` sin `Skip`, en la emisión de PB-T3c, con la autoridad, la guarda que sigue, las nuevas y los dos comentarios de WR | V2-11 | D-PBT3-11; §6.3; §8.2 |
| **I-11** El clasificador sin líneas ni hojas derivadas | I | Sí | Oráculo mecánico independiente (`PilotKit.JsonDiff` y las reglas de la tabla, reescritas en la prueba), comparado por (nivel, ruta, grupo, clase, estructural), y entradas ancla literales. Las hojas derivadas se informan con el grupo de su ruta (declarado); su presentación, a PB-T4 | V2-12 | D-PBT3-07; §5.2; §10 |
| M-01 CT-PBT3B-04 (iv) tautológico | M | Sí | Oráculo: el hecho medido de M-i, generalizado | V2-13 | §5.3 |
| M-02 CT-PBT3B-12 pasa sobre el esqueleto | M | Sí | `prepare=Ready corrida=Applied` en la línea | V2-14 | §5.3 |
| M-03 Sobrecarga y coincidencias del doble | M | Sí | Los constructores nuevos de la expectativa y del plan, su `Equals`; el doble da coincidencias para las claves a borrar | V2-15 | D-PBT3-09; §5.0 |
| M-04 Sin CT del `Precheck` ni del orden | M | Sí | CT-PBT3B-15, nuevo | V2-16 | §5.3; §6.2 |
| M-05 Ancla de [PKC2] | M | Sí | §6.7 `:579-581`; §6.8 `:655` | V2-17 | §1; §6.0 |
| M-06 Numeración de [P] atribuida al [OD] | M | Sí | «[P] §7.3 y §7.4, con el texto del [OD]» | V2-18 | §1 |
| M-07 La ExtensionData fuera del repo y de las precondiciones | M | Sí | F4B-E24 y F4B-E25 en la precondición 8; A-T3-7 sale | V2-19 | Precondiciones; §1; §10 |
| M-08 El texto del frontal B | M | Sí | Texto nuevo y la limitación declarada | V2-20 | D-PBT3-09; D-PBT3-14 |
| M-09 El coste | M | Sí, declarado | R-T3-07 cuenta las pasadas; a R-PB-06, al Smoke-PB y a PB-T4 (A-T3-4) | V2-21 | §7; §10 |
| M-10 `Pass` duplica la ventana | M | **Declarado, no arreglado** | La ventana es de PB-T2 y PB-T4; se declara interino y va como A-T3-9 | V2-22 | D-PBT3-03; §10 |
| M-11 AUTH-13 por muestra | M | Sí | Las 28 rutas de `Nested("pushback")`, copiadas, sobre `Rich("pushback")` | V2-23 | §5.2; R-16 |
| M-12 El heredoc, modo repetido | M | Sí | Se devuelve al Coordinator como modo repetido (en esta respuesta y en R-12). La v2 no usó heredoc | V2-24 | §9; §7; R-12 |
| M-13 La fachada puede nombrar el piloto | M | Sí | CT-PBT3C-01 prohíbe `NamesThePilot` | V2-25 | §5.4 |
| M-14 El `Skip` sin F4B-E; el `MinSelected` | M | Sí, con un marcador | El motivo cita «F4B-E<n> (D-1)»: el número lo pone la sesión al registrar D-1. El `MinSelected` de `Workspace` se fija desde la TRX del RED | V2-26 | §5.3; §6.2 |
| M-15 Ciclo `Pilot` ↔ `Draft` | M | **Declarado, no arreglado** | Interino hasta PB-T4 (R-T3-19; A-T3-10) | V2-27 | D-PBT3-04; §7; §10 |
| M-16 Erratas | M | Sí | R-6 sin «`Create`»; INV-PBT3A-06 como CT-12; el comentario de WR:225 | V2-28 | R-6; §6.1; D-PBT3-11 |

**Las lecciones nuevas de PB-T1b:**

| Lección | Cambio | Id |
|---|---|---|
| **F4B-E25** (las rutas `UNIT_CHANGE` en la base de una corrección) | Sustituye al texto de la v1 sobre la `AuthorityRevision` de una corrección; cada contrato declara sus rutas `UNIT_CHANGE` (PP y UR; WR) y cita la regla con sus tres condiciones | V2-29 |
| **Las paradas por declaración nueva, por nombre** (PBT1C-C13, v2.8) | PBT3A-C13, PBT3B-C13 y PBT3C-C13, en inglés, con su lista cerrada de nombres | V2-30 |
| **Lo normativo separado de las notas** (V28-19) | «Nota (no va al contrato)» en las invariantes y paradas que lo necesitan; el generador copia solo lo normativo | V2-31 |
| **El `Trailer` real** (V28-17) | En la `ExpectedEvidence` de los tres contratos y en INV-11 | V2-32 |

**Otros cambios de la v2,** de la revisión pero no de su tabla de hallazgos, o de hechos nuevos:
- **V2-33:** el «cualquier otro → `NoAddress`» de la disposición es inalcanzable hoy (`RackViewCodec.cs:263-286`); se declara.
- **V2-34:** la corrección de PB-T1b ya está en git (`126fd65f`, `a18ba345`), sin cambio de producto ni de recuentos.
- **No se funden PB-T3b y PB-T3c:** la revisión no lo recomienda.
- **El frontal B:** se queda el valor por defecto, que la revisión mantiene.

## D-1, la pregunta lista para AskUserQuestion

- **header** (10 caracteres): `Guardas F4`
- **question** (347 caracteres): «PB-T3b: para que el «Actualizar» del panel renombre y borre vistas, hay que sustituir pruebas de F4 que fijan el texto del escritor del piloto, cada una con registro y una prueba equivalente. Con un solo código para el piloto y el borrador completo son seis; si el borrador completo va en código duplicado hasta PB-T4, ahora son tres. ¿Cuál?»
- **options:**
  1. `Un solo código, seis (Recomendado)`: el plan y la corrida del piloto se generalizan, con un único algoritmo de escritura. Se sustituyen seis pruebas de F4 (cinco fallan y una quedaría vacía), cada una con su equivalente.
  2. `Código duplicado, tres`: el piloto no cambia de texto y el borrador completo va en código propio, copiado, hasta PB-T4. Se sustituyen tres ahora; PB-T4 tendrá que sustituir las otras tres al quitar el piloto.
  3. `Sin renombrar ni borrar`: no se sustituye ninguna. El panel no renombra ni borra vistas en PB-T3: esas dos cosas, que pide la fila de PB-T3, quedan pendientes.
- **multiSelect:** no.
