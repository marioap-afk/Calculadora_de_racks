# Encargo: la v2.2 (texto de emisión), el registro declarado y el generador del contrato de PB-TC

Lo da la sesión principal al redactor. La comprobación acotada de la v2.1 (`pbtc/independent-review-v21-check.md`) dio **EMITIBLE 0/0/5** y recomienda aplicar sus menores en la emisión. Las reglas son las de siempre: solo lectura en el worktree (HEAD `f9f2afbb`), sin compilar, sin ejecutar pruebas y Python sin heredoc. **No escribas nada en el worktree ni en `artifacts/`.**

## 1. `pbtc/I-64-f4b-pbtc-gate-contract-package-v22.md`

- **Partida:** la v2.1, sin sobrescribirla.
- **Registro de cambios v2.1 → v2.2:** una fila por cada N2-xx. Conserva los registros anteriores.
- **Línea de estado:** «v2.2, el texto que se emite; revisiones: IRT1 (NO EMITIBLE 1/4/10), ICT2 (EMITIBLE TRAS CORRECCIONES 0/1/9) e ICT21 (EMITIBLE 0/0/5); decisiones del Owner F4B-E06; **la emisión espera la orden expresa del Owner** (F4B-E05, Siguiente, punto 4)».

**Correcciones:**
- **N2-01:** en el §5.2, para la sustitución de CT-PB0-19 (S-03) y la de CT-PB0-05 (S-04), el cambio temporal sobre el GREEN quita el `Skip` **y** sustituye el `Assert.Equal` final por `Assert.Fail(string.Join("\n", lineas))`, como en D-PBTC-10, con su TRX y su log como artefactos citados en la entrega. Para la de WLC, la Theory por filas basta.
- **N2-02:** añade `tests/RackCad.UI.Tests/PushBackAdvancedRackParametersWindowTests.cs` al `ForbiddenWriteScope`, que pasa a 131 entradas. Ajusta el §6.3 y L-7.
- **N2-03:** el comparador (`ShownFormat`, `SameShown`, `Keep` y su variante) se declara **inmediatamente después de `OptionalVal`** (W:3625-3626), no junto a `Val`. Define en INV-PBTC-15 «tocar un miembro» como cambiar o quitar una línea de su rango, e indica que una inserción entre miembros es una declaración nueva.
- **N2-04:** renombra las sustituciones a **SUS-01 a SUS-04** en todo el paquete, en el literal del `Skip`, en el registro, en los invariantes, en las paradas y en `ExpectedEvidence`. El literal queda así:

  `SUSTITUIDA por PB-TC (D-PB-06/D-PB-11), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-0<n>: la ejecuta <Clase.Metodo>`

  Ajusta a cada fila qué decisión la autoriza: D-PB-06 o D-PB-11.
- **N2-05:** INV-PBTC-03 (2) enumera las diez cajas con `Keep(` (Depth, Tolerance, PostPeralte, BeamDepth, AnnotationScale, FirstLevelHeight y las cuatro de la celda) y aparte el cambio de `OptionalVal` para el largo manual.

## 2. `pbtc/superseding-characterization.md`

El registro declarado del §5.4, ya en su forma final:
- formato de I-39;
- estado **DECLARADA**;
- cabecera con la autoridad: [OD] D-PB-06 y D-PB-08, D-PB-11 por F4B-E06, y OB-T1B.2;
- las filas SUS-01 a SUS-04 completas;
- lo no sustituido, con su motivo;
- los pendientes de PB-T2;
- el caso de H-31, pendiente de la decisión del Owner antes de PB-T1;
- el cambio de comportamiento declarado de RACKEDITAR.

Debe valer para custodiarse en un repositorio **público**: sin rutas locales ni nombres de usuario, de máquina o de clientes.

## 3. `pbtc/make_pbtc_contract.py`

- **Patrón:** `pbt0/make_pbt0_contract.py`, que se lee entero. Mismo uso: `python make_pbtc_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry]`.
- **Salida:** JSON en ASCII, con el `TaskId` `F4B-PBTC-DECLARED-CORRECTIONS`, que escribe en `artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json` y valida con `Test-Json` contra `docs/automation/agent-execution/schemas/gate-contract.schema.json`. Eso **solo** sin `--dry`.
- **Contenido:** el §6 de la v2.2, fiel:
  - `Objective`;
  - `Authorities`:
    - el paquete `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` y `docs/initiatives/I-64-f4b-pbtc-measurements.md`;
    - el paquete de la fase;
    - [OD] y [DP];
    - `F4B-DRAFTS/coordinator/owner-pbtc-decisions.md` (F4B-E06);
    - `F4B-DRAFTS/superseding-characterization.md`;
    - `f4b-dispositions.md`;
    - el cierre de PB-T0 y su contrato;
    - T1B;
    - las fuentes de las firmas;
    - el estado y la evidencia;
    - el propio contrato;
    - las rutas de producto como `UNIT_CHANGE`;
    - ADR-0029, `AUTOMATION_PLAN`, README, `routing` y el catálogo como `EXTERNAL`;
  - `AllowedWriteScope`, con 13 rutas exactas;
  - `ForbiddenWriteScope`, con 131 entradas por el algoritmo del §6.3 y con `assert` sobre sus recuentos y su forma, como en PB-T0;
  - `Invariants` en ASCII, sin tildes, con el texto del §6.5 en sustancia completa;
  - `RequiredTests`;
  - `ExpectedEvidence`;
  - `StopConditions`: las estándar desde el contrato de T1B, con P-01 de la Snapshot I como en PB-T0, más PBTC-C01 a PBTC-C14, con C11 en el texto literal de las firmas como en PB-T0;
  - `EligibleCells`, copiadas como en PB-T0;
  - `RoutingEnforcement` `required` y `CorrectionsAuthorized` true;
  - `IssuedBy`, que nombra las decisiones del Owner F4B-E06 y «la orden expresa del Owner en el chat de emitir PB-TC (F4B-E07)».
- **Comprobaciones, las mismas de PB-T0:**
  - ninguna ruta permitida bajo un prefijo prohibido;
  - toda prohibida existe;
  - todas las rutas nuevas permitidas **no** existen en HEAD;
  - términos de cierre prohibidos;
  - sin rutas locales.
- **Lo que puedes ejecutar:** **solo** `python make_pbtc_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`, que no debe escribir nada. Comprueba que lo que imprime cuadra con el §6.

**Tu respuesta final** es corta: las rutas de los tres archivos, la salida del `--dry` (los recuentos y los bytes) y cualquier contradicción.
