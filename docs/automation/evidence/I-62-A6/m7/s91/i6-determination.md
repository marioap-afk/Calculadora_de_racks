# I-62 — Determinación normativa acotada: I6 y el cierre de VERIFY de FX-02 (R7-A) (decisiones §91, punto 2)

> Supervisión, plano a. Análisis estático sobre `46217ccc`; no decide: propone al Coordinator. Sin Codex. Datos de las entradas: el E congelado de la
> medición M (`kits/FX-02/a6m7-measure/frozen/E.json`, `137c7c3b…`) y el repositorio de la entrega del banco (`1746b404` = AuthorityRevision,
> `b417d36c` = `Target.commit` = G).

## 1. Pregunta

¿Cabe R7-A (evidencia §181; banco `m7/bench/README.md` §2) literalmente en las autoridades acordadas, o exige una enmienda material? R7-A fija las
autoridades por su blob en la AuthorityRevision; el objeto verificado y los registros de la ventana por su blob en `I.Target.Commit`; la delegación, la
entrega, el TRX y el contrato por el blob de sus bytes custodiados; y el instrumento por A6-2.

## 2. Literales aplicables

| Id | Fuente (blob) | Línea | Literal |
|---|---|---|---|
| L1 | V14 §20.3.1 (`34ad80ea`) | 1079 | `CanonicalInputs[]`: «los artefactos versionados que la acción necesita: el objeto exacto, sus registros de revisión y sus autoridades» |
| L2 | V14 §20.3.1 | 1098-1099 | «El cierre fija la `AuthorityRevision`, el blob de cada archivo y el blob de cada instrucción de la que se derivó una obligación. Si cambia uno de esos blobs, el cierre se recalcula antes de lanzar.» |
| L3 | V14 B.9 | 2231 | `Target`: «`{commit, path, blob}`; **obligatorio** en revisión y verificación» |
| L4 | V14 B.9 | 2233 | `CanonicalInputs[]`: «`{Path, Blob}` en la `AuthorityRevision`» |
| L5 | V14 B.9 | 2250-2251 | «El renderizado declara que el revisor puede leer `CanonicalInputs` y `AllowedTransitiveInputs` y ejecutar `AllowedActions`, y nada más.» |
| L6 | AP 16.24 (`f525cb1e`) | 1264 | `CanonicalInputs[]`: «los artefactos versionados que la acción necesita: el objeto exacto, sus registros de revisión y sus autoridades» |
| L7 | AP 16.24 | 1276 | «el cierre fija la `AuthorityRevision` y los blobs; si cambia uno, se recalcula antes de lanzar.» |
| L8 | AP 16.24 | 1279-1280 | «Tras la terminación, una lectura registrada fuera del cierre es INVALID_REVIEW_CONTEXT (P-22)» |
| L9 | AP 16.3 | 490 | «`AuthorityRevision` es el SHA del commit cuyo árbol contiene los documentos y los cambios normativos de la propia unidad que obligan a la delegación.» |
| L10 | RAE §15.2 I6 (`592dcfd4`) | 565 | «todo blob coincide con el de la `AuthorityRevision`; si cambia uno, el cierre se recalcula» |
| L11 | A-6 A6-2, regla 1 (`4bc919dd`) | 532-535 | la entrada del instrumento «Es la única excepción a la clase de §20.3.1, que admite el objeto exacto, sus registros de revisión y sus autoridades» |
| L12 | A-6 A6-2, regla 3 | 539-541 | para el instrumento, el blob de A-6 y no la `AuthorityRevision`; «Las demás entradas conservan sin cambio esa comprobación contra la `AuthorityRevision`» |
| L13 | A-6 A6-7, regla 7 | 789-794 | una disposición fija «cómo el cierre efectivo de las invocaciones VERIFY de FX-02, que fija el kit, cubre las lecturas legítimas del instrumento, en particular las de `CurrentSha`… o limita las órdenes del instrumento» |
| L14 | A-6 A6-9, regla 4, y tabla | 841-843; 861-871 | el cierre del aceptante se deriva del cierre efectivo «y de las entradas que ese cierre fija por blob (la delegación, la entrega, el contrato y la invocación del rol)»; C3 por identidad de blob en todos los commits de C1; C6 sobre `E.CanonicalInputs` con DELEG, H y TRX |
| L15 | A-6 §5, M-02 y M-05 | 1213, 1216 | que la regla I6 de una entrada se compruebe «contra el blob de A-6 y no» contra la `AuthorityRevision` cambia el significado persistido de `input-closure/v1` (M-02) y la semántica que consumen el invocador, el auditor y los validadores (M-05): **material** |

## 3. Análisis por categoría de insumo

Entradas del E de M, clasificadas por su blob en la AuthorityRevision `1746b404` y en G:

| Categoría | Entradas de R7-A | Blob en AR | Cabe en el literal | Fundamento |
|---|---|---|---|---|
| **Autoridades** (autoridades del contrato, `AGENTS.md`, transitivos del punto fijo) | `AGENTS.md`, `AUTOMATION_PLAN.md`, `WORKFLOW.md`, `INITIATIVE_LIFECYCLE.md`, README y `routing.md` de agent-execution, mapa y su esquema, esquema de la entrega; las autoridades del contrato en AR | iguales | **sí** | L1, L2, L4, L10, L12 |
| **Objeto verificado** (`Target` y rutas de `git diff BaseSha..G`) | `Calculator.cs` y `CalculatorTests.cs` en G; `worker-notes.md` en G | distinto; distinto; ausente | **no** como `CanonicalInputs`: L4 y L10 exigen el blob de AR, L12 conserva esa comprobación; L3 obliga a nombrar el objeto en `Target`, pero L5 solo permite leer `CanonicalInputs` y transitivos | conflicto L1/L3/L6 frente a L4/L5/L10/L12 |
| **Delegación** D | `output.json` de la planificación, fuera de Git | no existe en ningún árbol | **no** por L1/L4/L6 (no es un artefacto versionado) ni por L10 | solo L14 la presupone «fijada por blob» |
| **Contrato** K | `FX-U1-T1.gate-contract.json`, blob `628d89af` | ausente en AR (entra en `d30fb6a9`) | **no** por L4/L10/L12 | L14 lo presupone fijado por blob |
| **Entrega** H | `worker-handoff.json`, fuera de Git | no existe en ningún árbol | **no** (igual que D) | L14 |
| **Registros y artefactos custodiados** | `FX-U1.md` y `FX-U1.yml` en G; TRX fuera de Git | distinto; distinto; no existe | **no** | L14 (C6 nombra el TRX) |
| **Instrumento y sus lecturas legítimas** | `{Path, Blob 9a829b0e}` y su acción | — | **sí**: excepción expresa | L11, L12; lecturas: A6-4, regla 3, contra el cierre derivado, que depende de las entradas anteriores |

## 4. Determinación

1. **R7-A no cabe literalmente.** Seis entradas Git (contrato, `worker-notes.md`, los dos archivos entregados en G y los dos registros en G) no tienen el
   blob de la AuthorityRevision que exigen V14 B.9 L2233 (L4), RAE I6 (L10) y A6-2, regla 3 (L12), y tres más (D, H, TRX) no son artefactos
   versionados (L1, L6). La AuthorityRevision no puede ser G: L9 la define como el commit de los documentos de la unidad que obligan a la delegación, y el
   contrato fija `1746b404`.
2. **El hueco es anterior a A-6 y no depende del instrumento.** Con la lectura literal, ningún `EffectiveInputClosure` de una VERIFY de FX-02 admite el
   objeto que L3 obliga a verificar, ni la delegación y la entrega que 16.9 lee (`Handoff`, `Scope`, `Contract`): el Controller, con o sin
   instrumento, haría lecturas fuera del cierre (P-22, L8). El kit de FX-02 ya lo dejó abierto (OQ-18, OQ-31). R7-B no lo resuelve: también conserva
   D, H y el TRX fuera del literal y, además, deja fuera el contrato y `clausemap`, sin los que no se forma `AuthorityResolution` (banco, §2 y §8).
3. **A6-7, regla 7, no basta como autoridad.** Delega en el Coordinator cómo el cierre «que fija el kit» cubre las lecturas legítimas del instrumento
   (L13); no le da poder para cambiar la comprobación de blobs, que A6-2, regla 3, conserva expresamente para toda entrada distinta del instrumento
   (L12). A6-9, regla 4 (L14), presupone que E fija D, H y K por blob, pero no establece la regla de admisión ni excepciona I6.
4. **Es material.** Por el mismo criterio que la propia A-6 aplicó a una sola entrada (L15: M-02 y M-05), cambiar contra qué se comprueba el blob del
   objeto, de los registros, del contrato y de los artefactos fuera de Git cambia el significado persistido de `input-closure/v1` y lo que juzgan el
   invocador, el auditor y P-22.
5. **Veredicto: R7-A exige una enmienda material acotada.** No se adopta por conveniencia de la medición, y la medición de A6-7 no puede usar un cierre
   que las autoridades no admiten.

## 5. Cambio mínimo y autoridad (sin redactar; solo si el Coordinator lo dispone)

- **Contenido:** solo para las invocaciones VERIFY de FX-02 en F6 (la verificación, su reverificación y los controles con invocación), y para la medición de
  A6-7, que deriva su cierre igual: (a) autoridades fijadas en la AuthorityRevision, con I6 y A6-2, regla 3, sin cambio; (b) el objeto exacto del `Target`
  y las rutas de `git diff --name-only BaseSha..Target.commit`, por su blob en `Target.commit`; (c) los registros de la unidad que la verificación lee
  (decisiones y estado), por su blob en `Target.commit`; (d) el contrato, la delegación, la entrega y el TRX que nombre el kit, por el blob Git de sus
  bytes custodiados, identificados por las rutas de la designación (O3); (e) ningún otro insumo, y recálculo ante cualquier cambio de blob.
- **Pruebas ya hechas** (reutilizables por identidad): el banco con R7-A (77 de 77) y el kit (113 de 113); el cambio no toca el instrumento, el aceptante,
  la tabla de A6-9 ni la receta.
- **Autoridad:** Architect independiente y Coordinator (LIFECYCLE §6), como A-n posterior a A-6. No toca ADR-0046 #9 ni los blobs fijados: la línea (a) del
  Owner sigue cubriendo el instrumento y el aceptante, y no se identifica otra consecuencia reservada al Owner, salvo que el Architect la encuentre.
- **Alternativa más amplia** (no mínima): la misma regla para toda VERIFY de una unidad I62, que corregiría el hueco de V14 B.9 fuera de FX-02.
