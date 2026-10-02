# I-62 — Disposición del Owner y del Coordinator sobre la revisión formal del Architect de la Proposal V10 (registro)

```text
Emisores:         Owner y Coordinator exclusivo de I-62 (disposición de la revisión formal, aceptación de hallazgos, requisito R62-FIDELITY y orden de V11)
Naturaleza:       disposición de una revisión, requisito arquitectónico nuevo y orden de corrección; no es dictamen del Architect, Freeze, decisión de OD-6
                  ni autorización de implementación
Fecha:            2026-10-02 (UTC)
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — PROPOSAL V11 / V10 ARCHITECT CORRECTIONS + INPUT FIDELITY»);
                  sin archivo de origen. Procedencia: transcripción literal recibida (10 559 bytes en UTF-8, SHA-256
                  e97c6a5bdc7d26056f518528ed046e087c099985af9977f964873340790d3a44), custodiada fuera del repositorio con la transcripción de la sesión
Objeto:           revisión formal del Architect de la Proposal V10 (commit 41ded86e6494e3d43e07ce97de9eedf4710a631f, blob 58f88fc602d0d4eaf3900a3514259da6b94faba2);
                  registro I-62-architect-review-v10.md; resultado literal en docs/automation/evidence/I-62-architect-v10/R20261002T010757Z-ef59/
Disposición:      revisión formal VÁLIDA con una excepción por hallazgo; A62-V10-01, -03 y -04 ACCEPTED REQUIRED; A62-V10-02 NOT ACCEPTED (premisa inválida
                  por degradación de la codificación); R62-FIDELITY-01..06 REQUIRED; A62-V9-04 pendiente de nueva disposición; Proposal V11 autorizada
Estado:           Frozen: NO · OD-6 PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 vigente hasta que I-62 se integre
```

Registro redactado por la sesión responsable por orden del Owner y del Coordinator. La respuesta está en la [Proposal V11](I-62-proposal-v11.md) y en el
[paquete V11](I-62-architect-package-v11.md) §§3-4.

## 1. Disposición de la revisión (resumen fiel)

- La revisión formal limpia de V10 se acepta como revisión formal, con una excepción limitada a un hallazgo.
- **Aceptados como REQUIRED:** A62-V10-01, A62-V10-03 y A62-V10-04.
- **No aceptado:** A62-V10-02, por **premisa inválida causada por la degradación de la codificación**. El texto canónico de V10 dice
  «`review_rounds` ≤ `logical_requests`»; el Architect recibió «`review_rounds` = `logical_requests`» porque el transporte de la invocación degradó los
  caracteres no ASCII. No se modifica la semántica del presupuesto de revisión para satisfacer A62-V10-02.
- **A62-V9-04:** queda PENDIENTE DE UNA NUEVA DISPOSICIÓN EXPLÍCITA. La siguiente revisión válida debe evaluarlo contra el texto canónico fiel.
- La Proposal V10 queda histórica; se produce la V11.

## 2. Hallazgos aceptados y dirección exigida (resumen fiel)

| Id | Dirección exigida |
|---|---|
| A62-V10-01 | No decir que la CI exacta es «evidencia equivalente» de `dotnet test`. Definir que, en una invocación de rol cuyas restricciones de aislamiento son incompatibles con una acción inicial del repositorio, esa acción solo se omite por una exención explícita y acotada de una autoridad aplicable a esa invocación y esa acción. Para el Architect de solo lectura: `dotnet test` puede eximirse de forma explícita; la CI exacta de publicación puede darse como señal canónica separada de salud; no es equivalente a Core local, no satisface ninguna clase de prueba local y no se propaga a la evidencia de gates, Candidato, cierre o implementación; sin exención, la invocación no se lanza. Retirar el precedente del Controller si implica sustitución de evidencia. Alinear §20.3.1, la semántica del cierre de insumos, el paquete, C-41 y las tablas de autoridad o delta aplicables |
| A62-V10-03 | Separar la **vigencia de acción** (si la `ReviewLoopAuthorization` permite acciones nuevas: materializar, reservar, lanzar; termina por ARCHITECT_SATISFIED, agotamiento, caducidad, revocación o sustitución o enmienda del Coordinator) de la **acreditación histórica** (un binding o resultado materializado y usado válidamente mientras la autorización estaba vigente sigue acreditado tras su caducidad o revocación). Custodiar `AuthorizationRef`, el blob y el commit exactos de la autorización, `MaterializationCheck`, la vigencia, el instante y la `record_version` de la materialización, la identidad del binding y la evidencia de capacidad e independencia. El fin posterior impide el uso nuevo, sin invalidar de forma retroactiva la materialización, la invocación completada, el resultado, una disposición o un ARCHITECT_SATISFIED válido. Elegir y verificar una regla determinista para los intentos en curso. Actualizar `Validity`, `AuthorizationRef`, I-S18, la validación del binding, F.8 y la reconstrucción por un sucesor. Casos: caducidad tras AGREED; revocación antes de un lanzamiento nuevo; sucesor que reconstruye tras la caducidad; reutilización de una autorización caducada rechazada |
| A62-V10-04 | Correspondencia cerrada: EXECUTION_CONTROLLER / PLAN → `delegation/v2`; EXECUTION_CONTROLLER / VERIFY → `controller-verification/v2`; `rackcad-role-invocation/v1` admite ambos contratos exactos; no son intercambiables. Positivos para cada uno; negativos: PLAN con `controller-verification` y VERIFY con `delegation` → INVALID. Conservar la semántica de I-61 |

## 3. Requisito nuevo R62-FIDELITY-01..06 (resumen fiel)

| Id | Contenido |
|---|---|
| R62-FIDELITY-01 | Contrato de **fidelidad de los insumos canónicos**: `CanonicalInputFidelity {InputRef, GitBlob, CanonicalSha256, Encoding, TransportRepresentation, TransportSha256 o verificación equivalente, FidelityStatus}`. Codificación canónica del texto del repositorio: UTF-8 según los bytes reales. Antes de lanzar: bytes y blob canónicos, transporte en UTF-8 donde aplique, verificación de que el camino de lectura conserva caracteres no ASCII representativos y registro de la evidencia. La verificación cubre las clases presentes en el corpus (letras latinas acentuadas, ≤, ≥, ≠, →, ⇒, ⇔, ∈ cuando aparecen), no clases ausentes |
| R62-FIDELITY-02 | **Fallo cerrado:** si la representación visible para el revisor difiere materialmente del insumo canónico, INPUT_FIDELITY_INVALID; el resultado no se ingiere como disposición válida para lo afectado. Distinguir (A) diferencia de bytes con representación normalizada semánticamente fiel, si el contrato la permite de forma explícita, y (B) degradación semántica (≤ → «=», ≠ → «?», corrupción de identificadores o rutas, negación eliminada, caracteres de reemplazo en palabras normativas, estructura de tablas u operadores perdida): STOP o resultado inválido. No depender de que el Architect lo note |
| R62-FIDELITY-03 | **Invalidación acotada por hallazgo:** si la degradación se puede acotar mecánicamente a pasajes o hallazgos, los no afectados pueden seguir siendo utilizables solo si se prueba que su camino de evidencia es independiente del material corrompido; los afectados son INVALID_PREMISE / UNACCREDITED, y el resultado registra sus `FindingId`. Caso medido de V10: A62-V10-02 depende del «≤» corrompido; A62-V10-01, -03 y -04 no. Si no se puede establecer la independencia, se invalida toda la revisión |
| R62-FIDELITY-04 | **Observada por el invocador, no autodeclarada:** el Architect no la certifica; el invocador o el adapter la registra, como el `RuntimeEvidenceRef`. El resultado puede citar `InputFidelityEvidenceRef`, pero no crea esa evidencia |
| R62-FIDELITY-05 | **Preflight de transporte:** para `codex-cli` / PowerShell en Windows, el adapter establece y verifica de forma explícita un transporte seguro en UTF-8, sin asumir los valores por defecto de la terminal. No se congela un comando concreto de PowerShell; se congela el requisito observable de que el texto canónico visible para el revisor sea fiel |
| R62-FIDELITY-06 | **Cobertura de pruebas:** (1) ≤ se mantiene; (2) ≠ se mantiene; (3) texto acentuado conservado; (4) transporte con pérdida deliberada rechazado; (5) un hallazgo apoyado solo en texto degradado no abre, cierra ni sustituye un linaje; (6) un hallazgo no afectado sigue válido solo con la independencia demostrada mecánicamente; (7) un Principal sucesor reconstruye la evidencia de fidelidad desde la custodia; (8) cambiar de proveedor o runtime no evita la validación. Añadir a contratos, estado y referencias de evidencia de la orquestación, semántica de fallos, matriz de invariantes y obligaciones, y FX-06 cuando aplique |

## 4. Disposiciones previas que se conservan (resumen fiel)

- **CLOSED:** A62-V9-02, A62-V9-03, A62-V9-05, A62-V9-06, A62-V7-01..03 y A62-V6-01..03.
- **SUPERSEDED:** A62-V9-01 → A62-V10-03.
- **No se registra** A62-V9-04 → A62-V10-02: A62-V9-04 queda pendiente de nueva disposición.
- Se conserva la distinción de V10: `review_rounds` = número de versiones distintas revisadas; `logical_requests` = solicitudes lógicas; puede haber varias
  solicitudes por versión; por tanto, `review_rounds` ≤ `logical_requests`. Los presupuestos cerrados se conservan salvo que aparezca otro defecto
  independiente.
- Los OPTIONAL siguen sin bloquear.

## 5. Entrega ordenada y siguiente revisión formal

**Entrega**, producida de forma autónoma:
1. Proposal V11 completa, `Frozen: NO`;
2. paquete del Architect V11;
3. este registro;
4. decisiones, evidencia, estado y contrato de I-62;
5. delta V10 → V11 exacto;
6. commit y blobs exactos;
7. CI de publicación exacta.

Límites: sin implementación, ediciones normativas compartidas, pilotos, OD-6 ni Freeze. Tras la CI en verde: STOP e informe único.

**Siguiente revisión formal:** V11 tendrá **una** revisión formal limpia nueva. Antes de esa invocación, el invocador DEBE probar la fidelidad de los
insumos. La revisión debe disponer de forma explícita de:
- A62-V10-01, A62-V10-03 y A62-V10-04;
- A62-V9-04;
- la arquitectura de R62-FIDELITY-01..06.

No debe evaluar A62-V10-02 como REQUIRED abierto. Con cero REQUIRED, el Owner decide OD-6, y la misma V11 exacta puede avanzar hacia el Consensus Freeze si
el Coordinator también está de acuerdo.

## 6. Estado declarado por la disposición

```text
V10 formal review:         VALID WITH FINDING-SCOPED EXCEPTION
A62-V10-02:                INVALID / NOT ACCEPTED
Open REQUIRED:             A62-V10-01, A62-V10-03, A62-V10-04, R62-FIDELITY architecture requirement
Pending re-disposition:    A62-V9-04
OD-6 = PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 REMAINS ACTIVE UNTIL I-62 IS INTEGRATED
```
