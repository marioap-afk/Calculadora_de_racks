# I-64 F4B — Comprobación acotada de la v3 frente a IR2

## 1. Objeto, método y límites

**Objeto.** `I-64-push-back-completo-package-v3.md` (citado como `P3:n`) y la nota de corrección de MA H-061 en `map-a-classic-write-path.md` (citada como `MA:n`). Se comprueban frente a:
- los 25 hallazgos de `independent-review-v2.md` (IR2);
- la orden `owner-f4b-disposition.md` (`OD:n`);
- el código de `5ccaa770`.

**Método.** Las mismas reglas de solo lectura que IR2:
- `git -C` solo con órdenes de lectura;
- el árbol de trabajo sigue limpio;
- el script de solo lectura `check_literal.py` de esta carpeta. Solo se le añadió un argumento opcional con el nombre del paquete. Compara carácter a carácter los bloques literales de la v3 con la OD.

No se compiló nada, no se ejecutó ninguna prueba y no se abrió AutoCAD.

**Límite.** No se puede saber si algún rack real lleva un `DimensionStyle` guardado. Afecta a N-03.

## 2. Estado de los 25 hallazgos

Un PARCIAL tiene la gravedad de su **residuo**, que se indica en la columna. Ningún residuo es IMPORTANTE.

| Id | Estado | Evidencia en la v3 (y en el código) | Residuo |
|---|---|---|---|
| R-I-01 | RESUELTO | P3:671-676: no se hereda nada. Un rechazo de planificación o un control fallido vuelven al Owner, sin PD-F4-01 ni PD-F4-04 a 07. P3:653 remite al §7.2 | — |
| R-I-02 | RESUELTO | P3:546 y P3:623: opera el Owner presente, salvo orden expresa. P3:655-656: qué es un error del operador y un tope de dos repeticiones. P3:748: el bloque M espera esa orden | — |
| R-I-03 | RESUELTO | P3:96 corrige H-PB-17. MA:105 lleva la nota de corrección, y la frase falsa queda en MA:104 marcada como tal. P3:572: la fila «Borrado de vistas» cubre el compuesto. P3:609: M-4 ampliada. El código confirma `PushBackCompositeStructure.cs:397-400` | — |
| R-I-04 | PARCIAL | (b) queda resuelto: hay prueba Core del `DimensionStyleName` (P3:365, P3:577), recuento y M-7 (P3:366-367, P3:611) y vuelta al Owner antes de PB-TC (P3:384). **Residuo:** OB-06.4 obliga a presentar «el efecto medido de (a)» (P3:384) con un criterio de piezas, niveles, posiciones y lista de materiales (P3:383). Pero ninguna fila de PB-T0 mide ese efecto: la de redondeo (P3:578) solo dice qué campos redondean. El criterio además es ambiguo (P3:383): «cualquier diferencia en … niveles o posiciones» y, a la vez, «una coordenada que cambie más de 0,001» | MENOR: añadir una fila Core que compare el plan y el BOM con las entradas exactas y con las redondeadas a «0.###» (pb-08 y los campos de MB H-077), y quitar la contradicción del criterio. Puede hacerse en el paquete del contrato de PB-T0 |
| R-I-05 | RESUELTO | P3:389-392 (OB-06.6). Las 33 llamadas de Cantilever están comprobadas | — |
| R-I-06 | RESUELTO | P3:386-388 (OB-06.5), P3:576 y P3:503 | — |
| R-I-07 | PARCIAL | P3:158-161 (OB-01.1b), P3:163-167 (vector real, prueba UI-STA en PB-T2 y guarda en PB-T4), P3:483-485 y P3:498-500. Las citas `…SourceGuardTests.cs:302-319` y `…PluginGuardTests.cs:200` están comprobadas. **Residuo:** la guarda de PB-T4 (P3:167) prohíbe `RackPushBackCommands`, `RackMenuCommands` y `EditorModules`, pero no `RackViewBatchExecution` ni las entradas de escritura de las fachadas (`DrawAndPlace` y `RedrawInPlace`), que también son caminos clásicos (MA H-020, H-032, H-048). P3:487 conserva la frase que no protege nada («no recibe el comando clásico…»), ya inofensiva | MENOR: ampliar la lista de la guarda. Quitar P3:487 o enlazarla a OB-01.3 |
| R-I-08 | RESUELTO | P3:691-700 (OB-T1B.1 a 3), P3:697 (OB-01.6 fuera), P3:116 (H-PB-32) y P3:758 (vuelta al Owner). Nota: OB-T1B.3 llama a G-10 «guarda de T1B», cuando es de F4 (`WorkspacePilotUpdateIntegrationGuardTests`). Es inofensivo, porque es más estricto | — |
| R-I-09 | RESUELTO | P3:312-314 (PB-T3 prueba los pasos 4 a 7 y no conecta la composición), P3:323-330 (OB-05.4 y OB-05.5 en PB-T4), P3:499-500. Es coherente con `KeepPilotEditsOn` (`PushBackPilotMaterializer.cs:242-251`) | — |
| R-I-10 | RESUELTO | P3:304-310: el alcance real, la limitación declarada y la pregunta antes de PB-T3, también en P3:760. Las anclas `RackEmbedDocument.cs:74`, `RackProjectDocument.cs:51` y `PushBackDesignDocument.cs:75` están comprobadas | — |
| R-I-11 | RESUELTO | P3:630-636: el inventario ampliado con la longitud y el resumen del sobre, y con las entidades y la extensión de la definición | — |
| R-M-01 | RESUELTO | P3:396-409. La cita de la D-PB-07 de la v1 (P3:402) es idéntica a P1:73, comprobado carácter a carácter | — |
| R-M-02 | RESUELTO | P3:322: la letra exacta de `OD:129` | — |
| R-M-03 | RESUELTO | P3:47, P3:169 y P3:172 | — |
| R-M-04 | RESUELTO | P3:298-300 | — |
| R-M-05 | RESUELTO | P3:247-255 y P3:647. Su redacción nueva deja una contradicción: ver N-02 | — |
| R-M-06 | PARCIAL | P3:377 y P3:505-507 lo resuelven. **Residuo:** el texto viejo sigue en P3:114 (H-PB-30: «En PB-TC, la prueba "Workspace" … ya cierto, se fija») y en P3:496 (PB-TC: «y en la ruta del Workspace que exista»). Además, «pendiente de PB-TC» (P3:498) es ambiguo | MENOR: alinear P3:114, P3:496 y P3:498 con OB-06.3 |
| R-M-07 | RESUELTO | P3:497 (PB-T1 extrae) y P3:499 (PB-T3 usa). El ancla `RackPushBackSystemWindow.Composite.cs:778-782` está comprobada | — |
| R-M-08 | RESUELTO | P3:646 (por defecto se conserva el rechazo, y hay dos alternativas con la evidencia de M-4), P3:245-246 y P3:728 | — |
| R-M-09 | PARCIAL | P3:615-628: el marcador, la carga con NETLOAD y «Load Once» (es la de Smoke-2: `run-log-2.md:145`), el procedimiento, el último vínculo y las capturas. **Residuo:** (1) `dll-c2.md` solo registra los SHA-256 de los cuatro DLL (`dll-c2.md:11-14`), así que comprobar «sus dependencias y los catálogos frente a `dll-c2.md`» (P3:615) no tiene referencia; (2) falta comprobar que no hay `RACKCAD_DEBUG_*`, como hizo Smoke-2 (`run-log-2.md:145`; `f4-dispositions.md:2291`); (3) M-4 registra las vistas B que el rack ya tenía (P3:609), pero no exige insertarlas antes. Si el rack fabricado no tiene frontales B, H-PB-18 queda sin medir | MENOR: comparar `catalogs` de la carpeta del DLL con `assets/catalogs` en `5ccaa770`, y añadir el diff de `assets/`; comprobar las variables de depuración; exigir en M-4 que se inserten antes los frontales B |
| R-M-10 | RESUELTO | P3:544 | — |
| R-M-11 | RESUELTO | P3:582 | — |
| R-M-12 | RESUELTO | P3:379-380, P3:577 y P3:578 | — |
| R-M-13 | RESUELTO | P3:585 (el censo) y P3:109 (H-PB-25) | — |
| R-M-14 | RESUELTO | P3:726-728 (R-PB-09 a R-PB-11) y P3:733-735. El tratamiento de R-PB-09 trae un defecto nuevo: ver N-01 | — |

**Resumen:**

| Estado | Cuántos | Cuáles |
|---|---|---|
| RESUELTO | 21 | El resto |
| PARCIAL | 4 | R-I-04, R-I-07, R-M-06 y R-M-09, todos con residuo MENOR |
| NO RESUELTO | 0 | — |

## 3. Defectos nuevos de la v3

**Textos del Owner.** Los 12 bloques «Texto del Owner (literal)» de la v3 coinciden carácter a carácter con la OD:

| Bloque | Paquete | OD |
|---|---|---|
| D-PB-01 | P3:127-154 | OD:22-49 |
| D-PB-02 | P3:181-189 | OD:13-21 |
| D-PB-03 | P3:200-211 | OD:50-61 |
| D-PB-04 | P3:221-241 | OD:75-95 |
| D-PB-05 | P3:261-294 | OD:96-129 |
| D-PB-06 | P3:336-361 | OD:130-155 |
| D-PB-08 | P3:416-428 | OD:62-74 |
| D-PB-09 | P3:440-448 | OD:224-232 |
| D-PB-10 | P3:458-461 | OD:233-236 |
| PB-T0 | P3:515-536 | OD:156-177 |
| T1B | P3:682-688 | OD:237-243 |
| RACKEDITAR | P3:706-710 | OD:244-248 |

Las otras citas del Owner también coinciden: las de la tabla del §5 (P3:495-500) y las insertadas en P3:172, P3:322, P3:397 y P3:405. La regla de los sustitutos (P3:675) es literal de `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-decisions-d1-d6.md:171`.

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| N-01 | MENOR | PB-T2 y PB-T3 «en paralelo», con «se integran una tras otra, y la segunda se rebasa sobre la primera» (R-PB-09). Eso supone dos delegaciones abiertas a la vez en la unidad I-64. AUTOMATION_PLAN lo prohíbe: «Delegación abierta … Como máximo una por unidad», con un único puesto de escritor | P3:499, P3:726; `docs/AUTOMATION_PLAN.md:557-558` | Aclarar que «en paralelo» solo significa que PB-T3 no depende de PB-T2. Las delegaciones son sucesivas, y no hay rebase entre tareas. No afecta a PB-T0 |
| N-02 | MENOR | OB-04.2 dice que «F4B no las crea», sin condición. Pero la fila 2 del §6.4, de acuerdo con `OD:91-93`, admite meter la creación en el contrato de PB-T3 si M contradice H-PB-16 y encaja en T4. OB-04.2 dice además «solo actúa si…», refiriéndose a un rechazo que ya no está en su texto. La consecuencia de H-PB-22 («patrón para OB-04.2 mientras no exista el borrado») quedó desfasada | P3:247-249, P3:645, P3:647, P3:101 | Redactar OB-04.2 como «F4B no crea vistas salvo por la fila 2 del §6.4», con el rechazo de la fila 4. Actualizar P3:101 |
| N-03 | MENOR | M-7 puede no ser medible. Ningún camino de la interfaz escribe un `DimensionStyle` en un rack Push Back: `PushBackEditorModule` no llama a `SetDimensionStyles` y la ventana pierde el estilo (pérdida (b)). Solo tendría estilo un rack de datos antiguos. Fabricarlo exigiría escribir el Xrecord, es decir, un arnés, que P3:615 prohíbe | P3:611; `src/RackCad.UI/Editor/EditorModules.cs:112-127` (MB H-082); `RackPushBackSystemWindow.xaml.cs:840-848` | Prever el resultado NO DISPONIBLE. Entonces decide la prueba Core de OB-06.1 (P3:365), y el recuento de racks con estilo (seguramente cero) es el dato para el Owner antes de PB-TC |

**Otras comprobaciones:**
- **Obligaciones sin dueño:** ninguna nueva. OB-05.4 y los pasos 1 a 3 tienen dueño (PB-T4), y el criterio de bloqueo también (PB-T1).
- **Decisiones materiales de producto sin autoridad:** ninguna.
  - La regla del frontal B (P3:646) conserva por defecto el comportamiento actual del panel, y sus alternativas pasan por la revisión del paquete de PB-T3, con la evidencia de M-4.
  - «Gana el nombre del borrador» (P3:300) es la lectura literal de `OD:100-101`.
  - La vuelta incondicional al Owner antes de PB-TC (P3:384) es más estricta que `OD:153-155`, y eso está permitido.

## 4. ¿Algo que deba volver al Owner antes de PB-T0 y que no esté en el §10?

**No.** Las únicas intervenciones del Owner que afectan a PB-T0 ya están en el §10:
- operar el bloque M, u ordenárselo a la sesión (P3:748, P3:755). No bloquea la emisión del bloque C, pero sí el cierre de PB-T0;
- la información sobre D-PB-04, PB-TC y D-PB-07 (P3:754).

**Lo que se revisó y no hace falta llevar al Owner:**
- **El RED por mutantes de PB-T0** (P3:598). Ya estaba en la v2 que revisó IR2. Es un método del contrato, que tendrá su propia revisión independiente (P3:746), y no una excepción de proceso de F4 como la replanificación o los sustitutos.
- **El recuento «ALL ten» frente a nueve textos.** Es informativo y ya está en el §10.

## 5. Veredicto

**APTO.**

| Recuento | B | I | M |
|---|---|---|---|
| Nuevos | 0 | 0 | 3 (N-01 a N-03) |
| No resueltos | 0 | 0 | 0 |
| Parciales, con residuo | 0 | 0 | 4 (R-I-04, R-I-07, R-M-06 y R-M-09) |

**Por qué APTO:**
- no quedan bloqueantes ni importantes sin resolver;
- la revisión no hace aparecer ninguna decisión material de producto nueva;
- se cumple la condición de `OD:261-266`, y se puede preparar el paquete del contrato de PB-T0.

**Los siete menores** (cuatro residuos y tres nuevos) deben corregirse al redactar ese paquete, o en una v3.1, antes de su propia revisión independiente:
- la fila Core del efecto de (a) y el criterio sin contradicción (R-I-04);
- la lista de la guarda de PB-T4 (R-I-07);
- el texto desfasado de PB-TC (R-M-06);
- la referencia de los catálogos, las variables de depuración y los frontales B previos de M-4 (R-M-09);
- las delegaciones sucesivas (N-01);
- OB-04.2 (N-02);
- la alternativa NO DISPONIBLE de M-7 (N-03).
