# Encargo: la v2 del paquete de PB-TC2 (el texto que se emite), su generador del contrato y la sección del registro

Lo da la sesión principal al redactor.

**El veredicto de la revisión independiente de la v1** (`pbtc2/independent-review-v1.md`) es **EMITIBLE TRAS CORRECCIONES, 0/3/7**, sin ninguna decisión nueva para el Owner.

**Las reglas son las de siempre:**
- solo lectura en el worktree (HEAD `5597c05b`);
- sin compilar ni ejecutar;
- sin escribir en el worktree ni en `artifacts/`;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c`. En la v1 hubo una desviación por eso; no la repitas.

## Lo que decide la sesión

1. **I-01:** aplica la corrección de la revisión.
   - La regla mixta se aplica solo con «Ambos lados» activo y con el lado B presente, la misma condición con la que hoy se pinta el estado mixto.
   - La rama de la caja vacía queda exactamente como hoy.
   - CT-PBTC2-04 gana una ventana negativa con el camino del hallazgo: «Ambos» con A ≠ B, se desmarca B y se hace clic en una celda. La caja sin editar conserva el exacto.
   - Mide en el código la condición exacta y escríbela por línea.
2. **I-02:** INV-PBTC2-04 pasa a ser una expresión regular de palabra, con la orden exacta (`git grep -nP '\bVal\(' -- <W>` vacío) y sin el texto `cref="Val"`.
3. **I-03:** aplica la corrección de la revisión.
   - ` descartes=0` va en la última línea de cada ventana de CT-04.
   - En las ventanas que no cierra «Actualizar», el contador se lee **después** de cerrar la ventana. Fija el orden exacto (cerrar y después leer) dentro del `try` o del `finally`, de forma que el invariante y las líneas coincidan letra por letra.
4. **M-01 a M-07:** aplica las correcciones de la revisión. Además:
   - **El umbral de la medición en la cadena:** se queda en la definición de «material» de OB-06.4, una coordenada que se mueve más de 0,001. Di de forma explícita que el «menos de 0,0005"» que se dio al Owner era una estimación, no un umbral.
   - **El riesgo de nc1:** añádelo como riesgo. Si nc1 vuelve a dar la variación de PB-TC (Remote, Scope, Tests y Trailer en fail además de Handoff, Identity y Ci), el Owner solo la aceptó una vez (F4B-E08, `owner-pbtc-nc1-decision.md`). Vuelve al Owner por PBTC2-C11, sin sustituto.
   - **M-07 (defecto previo):** anótalo como arrastre al paquete de PB-T1, no a PB-TC2.
   - **La ruta local del archivo de decisiones del Owner:** ya la ha corregido la sesión. No la toques.
5. **El registro de cambios v1 → v2,** al principio: una fila por hallazgo.

## Además, escribe

- **`pbtc2/I-64-f4b-pbtc2-gate-contract-package-v2.md`,** con la línea de estado: «v2, el texto que se emite; revisión independiente IRT2-1 (EMITIBLE TRAS CORRECCIONES 0/3/7); decisiones del Owner F4B-E09, con autorización condicional de emisión (pregunta 4); se emite si la comprobación acotada de la v2 da 0 bloqueantes y 0 importantes sin resolver y ninguna decisión nueva».
- **`pbtc2/make_pbtc2_contract.py`,** que sigue a `pbtc/make_pbtc_contract.py`:
  - el mismo uso y las mismas comprobaciones;
  - `TaskId` `F4B-PBTC2-DECLARED-CORRECTIONS`;
  - las autoridades:
    - `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md` y `docs/initiatives/I-64-f4b-pbtc2-measurements.md`;
    - `F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md` (F4B-E09);
    - `F4B-DRAFTS/pbt1/ob065-dossier.md`;
    - el cierre de PB-TC;
    - el registro;
    - las demás, como en PB-TC;
  - en `IssuedBy`: «las decisiones del Owner F4B-E09 con su autorización condicional de emisión (pregunta 4)».
  - Ejecuta solo `python make_pbtc2_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`, que no escribe nada.
- **`pbtc2/superseding-section.md`:** la sección nueva de PB-TC2 para `superseding-characterization.md`, en estado DECLARADA. Incluye:
  - el cierre del «Pendiente: decisión del Owner antes de PB-T1» para H-31;
  - el de la «Limitación conocida (OWNER-1)», con la decisión F4B-E09 y la tarea PB-TC2;
  - lo que sigue pendiente de PB-T2.

  Es texto para el repositorio público: sin rutas locales ni nombres.
- **`pbtc2-measurements.md`:** un apartado nuevo con lo que midas.

**Tu respuesta final** es corta:
- las rutas;
- la salida del `--dry`;
- los recuentos finales: rutas, CT, `MinSelected`, invariantes y paradas;
- cualquier decisión nueva.
