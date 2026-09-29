# I-52 AUTH-15 — Registro de decisiones de la unidad de integracion

Unit: `I-52-AUTH15` (iniciativa I-52). Rama: `feature/i52-auth15-definition-creator`. Workflow: V2.

Contrato de la unidad: [I-52-auth15-definition-creator.md](../../initiatives/I-52-auth15-definition-creator.md).

## 1. Autorizacion del Coordinador

```text
Coordinator = AUTH-15 IMPLEMENTATION AUTHORIZED
Integration unit = feature/i52-auth15-definition-creator
Base = 016bf46715cec45e644f88a22ef091b311a2bef1
§148/§156 production exclusion = LIFTED FOR AUTH-15 ONLY
AUTH15-DEV-01 = BlockNameUnavailable omitted from implementation because the existing
                family naming loops expose no reachable/distinguishable exhaustion result.
                ARCHITECT REVIEW REQUIRED.
```

- **Segunda unidad de I-52.** El Coordinador autoriza expresamente esta rama como segunda unidad de integracion
  de I-52, de proposito unico. No contradice la regla «1 iniciativa = 1 rama»:
  - su funcion es aislar AUTH-15 de la rama de investigacion CT-DA de I-52, que esta bloqueada;
  - nace de `origin/main`;
  - solo implementa e integra AUTH-15 y no absorbe otro trabajo de I-52.
- **Exclusion levantada solo para AUTH-15.** La exclusion de codigo de produccion de §148 y §156 del registro
  de I-52 queda levantada unicamente para AUTH-15. El comando RACKMIRROR y el resto del producto de I-52 siguen
  BLOQUEADOS en su propia rama.

## 2. Reclamo y clasificacion Workflow

- **Reclamo.** Commit vacio `4b20bda3`, aceptado en el primer push sin force, con
  `Claim-Id: 1f035b6f-6891-48fd-8965-f7ff1f522767`.
- **Clasificacion: V2, caso T4.** La base contiene el `WORKFLOW_V2_EFFECTIVE_SHA` y no hay pausa activa.
  - La pausa de activacion termino segun el registro durable del tag `integration/I-56`
    (`Claim pause: end=2026-09-17T22:30:00Z`), que es el registro que exige WORKFLOW §11.7.
  - La linea de `docs/HANDOFF.md` en `main` que aun dice «pausa ACTIVE» esta desfasada respecto de ese tag. No
    se corrige aqui (ver §3).
- **Orden.** El codigo de la unidad se redacto en local antes del reclamo, por orden previa del Coordinador. En
  el historial versionado el orden es el de WORKFLOW §2: reclamo, bootstrap (contrato, este registro y la fila
  de ROADMAP) y despues la implementacion.

## 3. HANDOFF

La orden pedia una entrada en decisiones y HANDOFF. **`docs/HANDOFF.md` no se toca en esta sesion**: WORKFLOW
lo reserva a la sesion de integracion, como ultimo commit de la rama (tabla de archivos calientes y de
registro). El registro durable de la unidad es este archivo, junto con el contrato y la fila de ROADMAP del
bootstrap (momento 2 de WORKFLOW §2). HANDOFF se actualizara en el commit de cierre de la integracion.

## 4. Contrato implementado y desviacion AUTH15-DEV-01

- **Contrato.** Se implemento el de la revision de Arquitecto «AUTH-15 DESIGN / FREEZE REVIEW», resumido en el
  contrato de la unidad.
- **Superficie.** Dos altas en el Plugin (`RackDefinitionCreator`, `RackDefinitionCreationResult`) y guardas de
  fuente. No se modifica ningun archivo de produccion existente.
- **AUTH15-DEV-01.** La revision listaba el fallo tipado `BlockNameUnavailable`, y la implementacion lo omite.
  - **Por que.** Ninguno de los dos creadores de familia expone un agotamiento de nombres alcanzable y
    distinguible:
    - el bucle de `LateralHeaderDrawer.UniqueBlockName` no tiene cota;
    - el de `CantileverViewMaterializer.UniqueBlockName` solo lanza `InvalidOperationException` tras
      `int.MaxValue` candidatos, indistinguible de cualquier otra `InvalidOperationException`.
  - **Que pasaria si ocurriera.** Saldria como `WriteFailed` con su diagnostico.
  - **Estado.** Por orden del Coordinador, no se restaura ni se cambia el comportamiento de produccion para
    resolverla. Resuelta en §5.

## 5. Revision exacta del Arquitecto sobre `fed44e56` y correccion C-1..C-5

```text
ARCHITECT_VERDICT = CHANGES REQUIRED (1 blocker, 2 major, 4 minor)
EXACT_SHA_REVIEWED = fed44e564a3efc0544fb07c1a1529ca6167300aa
AUTH15_DEV_01 = ACCEPTED / CONTRACT NORMALIZATION
```

- **B-1 (bloqueante).** `Precheck` comparaba `ReferenceEquals(database.TransactionManager.TopTransaction,
  transaction)`.
  - En AutoCAD 2025, el getter de `TopTransaction` construye un wrapper gestionado **nuevo** en cada lectura
    (`newobj Transaction(IntPtr, false)` en `AcDbMgd.dll`). La comparacion por referencia era siempre falsa y toda
    llamada devolvia `TransactionMismatch`.
  - Las 16 guardas y el CI pasaron, porque la guarda solo buscaba el token `TopTransaction`: las guardas de fuente
    (ADR-0003) prueban la forma, no el comportamiento. Por eso la validacion en host es obligatoria antes de
    Candidate.
  - **Correccion C-1.** Si la base de datos o la transaccion son nulas o estan dispuestas: `TransactionMismatch`.
    Despues, `top = database.TransactionManager.TopTransaction`; si `top` es nulo o `top.UnmanagedObject !=
    transaction.UnmanagedObject`: `TransactionMismatch`. Mismo diagnostico. La identidad es la nativa (la que
    comparan `DisposableWrapper.Equals` y `==`). La `OpenCloseTransaction` sigue sin admitirse. Ningun otro cambio
    de comportamiento de produccion.
- **M-1 → C-3.** Nueva guarda sobre los cuerpos exactos de los escritores delegados:
  - `LateralHeaderDrawer`: `CreateSystemBlock`, `NewBlock`, `AppendInstance`, `AppendDimension`,
    `ResolveDimStyle`, `EnsureAnnotationLayer`, `ApplyDynamicParameters` y `UniqueBlockName`;
  - `CantileverViewMaterializer`: `CreateBlockDefinitionNamed`, `AppendCurves`, `EnsureRoleLayers`,
    `UniqueBlockName` y `Sanitize`;
  - `LayerHelper.EnsureLayer`;
  - `RackBlockData.Write` y `Read`.

  La guarda localiza cada cuerpo por su firma exacta (bloque o expresion) y falla si no la encuentra o si esta
  repetida. Nunca escanea el archivo entero: `LateralHeaderDrawer.PurgeUnreferenced` abre y commitea legitimamente.
- **M-2 → C-4.** La fuente de AUTH-15 no puede contener `Dispose(`, `using (` ni `using var`: disponer la
  transaccion sin commit la aborta, y su vida es del llamador.
- **C-2.** La guarda de la transaccion exige `database.IsDisposed`, `UnmanagedObject` y `TopTransaction`, y prohibe
  `ReferenceEquals(`.
- **AUTH15-DEV-01 = ACCEPTED / CONTRACT NORMALIZATION.** Ningun creador de familia expone un agotamiento de nombres
  alcanzable y distinguible. `BlockNameUnavailable` sale de la lista normativa de fallos, sin codigo muerto; si
  alguna vez ocurriera, saldria como `WriteFailed`.
- **Menores m-1..m-4 → C-5 (contrato).** Clases de fallo PRE-WRITE / POST-WRITE, alcance de `InvalidBlockName` y de
  `InvalidPlan`, y semantica representativa de `MissingInstances`. m-4 (`database.IsDisposed`) queda cubierta por
  C-1.
- **Diseno de la validacion en host.** Queda registrado en la revision del Arquitecto. El arnes **no** se crea en esta
  compuerta: requiere orden propia del Coordinador.

## 6. Estado

```text
IMPLEMENTATION = CORRECTED (C-1..C-5) on this branch
AUTH15_DEV_01 = ACCEPTED / CONTRACT NORMALIZATION
HOST_VALIDATION = NOT RUN (no harness in this gate)
NEXT_GATE = ARCHITECT DELTA RE-REVIEW OF CORRECTED SHA
```

Los conteos de pruebas, los SHA y el CI exacto viven en el cuerpo de los commits y, al cierre, en el archivo de
evidencia de la unidad (WORKFLOW: tabla de registro, unidades V2). No se copian aqui.
