# I-62 / FX-02 — medición de claude-cli: ARCHITECT, REVIEW_DESIGN (RunId R20261010T012414Z-d6d8)

Actúas como ARCHITECT en una **medición del transporte**. No es una revisión que vaya a aceptarse: su resultado no se acepta, no crea autoridad
ni decide nada. Trabajas en solo lectura sobre el directorio de trabajo (un clon temporal del fixture) con las herramientas Read, Grep y Glob, y
devuelves tu respuesta final llamando **una sola vez** a la herramienta StructuredOutput con un objeto válido contra el esquema que el runtime te
impone (`rackcad-architect-review-result/v1`).

## Objeto revisado

- Ruta (relativa al directorio de trabajo): `docs/automation/decisions/FX-U1-T1.gate-contract.json`
- Commit: `d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99`
- Blob: `628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0`
- Unidad y tarea: FX-U1, T1 (contrato de gate de la tarea T1)

## Pasos obligatorios, en este orden

1. Glob con el patrón exacto `docs/automation/decisions/FX-U1-T1.gate-contract.json`, sin `path`.
2. Grep del patrón `"Role"` con `path` = `docs/automation/decisions/FX-U1-T1.gate-contract.json` y `output_mode` = `content`.
3. Read de `docs/automation/decisions/FX-U1-T1.gate-contract.json` completo (un solo Read, sin `offset` ni `limit`).
4. Una revisión breve del diseño del contrato: coherencia interna entre AllowedWriteScope y ForbiddenWriteScope, entre RequiredTests y
   ExpectedEvidence, y entre los RoleRequirements del ARCHITECT y su independencia. Como mucho tres hallazgos; ninguno es obligatorio.
5. Una única llamada a StructuredOutput. No leas, busques ni listes ningún otro archivo o directorio.

## Valores fijos: cópialos exactamente en el objeto de salida

```json
{
 "Schema": "rackcad-architect-review-result/v1",
 "ResultId": "R20261010T012414Z-d6d8/architect-review-result",
 "LogicalReviewRequestId": "L20261010T012414Z-d6d8",
 "InvocationId": "I20261010T012414Z-d6d8",
 "AttemptSeq": 1,
 "RequestedRole": "ARCHITECT",
 "Action": "REVIEW_DESIGN",
 "ReviewedUnit": "FX-U1",
 "ReviewedCommit": "d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99",
 "ReviewedPath": "docs/automation/decisions/FX-U1-T1.gate-contract.json",
 "ReviewedBlob": "628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0",
 "ReviewerBinding": {
  "UnitId": "FX-U1",
  "Scope": "TASK",
  "TaskId": "T1",
  "Role": "ARCHITECT",
  "BindingId": "B20261010T012414Z-d6d8",
  "Sha256": "025a971a4b2b590baf0d85fb305706d7424af20e12f1382282c2e6a16fa69053",
  "Location": {"Kind": "TRANSIENT", "Path": "ccli-meas/binding-stub.json", "Commit": null, "Blob": null}
 },
 "ReviewerDeclaredIdentity": {
  "Runtime": "claude-cli 2.1.293",
  "Model": "claude-opus-5-5",
  "Effort": "xhigh",
  "SessionOrThread": "ee381259-e0ca-4806-b264-09aa32783f39"
 },
 "ReviewerMode": "SEPARATE SESSION",
 "InputFidelityEvidenceRef": {"path": "docs/automation/decisions/FX-U1-T1.gate-contract.json", "blob": "628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0"},
 "IndependenceEvidence": {
  "Dimensions": [
   {"ReferenceRole": "AUTHOR", "Dimension": "Actor", "Result": "UNKNOWN", "Evidence": "medición del transporte: el paquete no aporta evidencia custodiada del autor; no se declara SATISFIED"},
   {"ReferenceRole": "AUTHOR", "Dimension": "Session", "Result": "UNKNOWN", "Evidence": "medición del transporte: el paquete no aporta evidencia custodiada del autor; no se declara SATISFIED"},
   {"ReferenceRole": "AUTHOR", "Dimension": "Context", "Result": "UNKNOWN", "Evidence": "medición del transporte: el paquete no aporta evidencia custodiada del autor; no se declara SATISFIED"},
   {"ReferenceRole": "AUTHOR", "Dimension": "Provider", "Result": "UNKNOWN", "Evidence": "medición del transporte: el paquete no aporta evidencia custodiada del autor; no se declara SATISFIED"}
  ],
  "ReviewSubject": {
   "Kind": "DESIGN",
   "Commit": "d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99",
   "Paths": ["docs/automation/decisions/FX-U1-T1.gate-contract.json"],
   "Blobs": ["628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0"],
   "RangeBase": null,
   "Authors": [
    {"Kind": "UNKNOWN", "Actor": null, "Session": null, "HumanId": null, "Operator": null,
     "Commits": ["d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99"],
     "Evidence": "identidad Git sintética; sin evidencia custodiada del autor en este paquete de medición"}
   ]
  }
 },
 "Downgrades": [],
 "OwnerDecisions": []
}
```

## Campos que completas tú

- `InjectedContextDeclaration`: `{"State": "NONE_DECLARED", "Items": []}` si no percibes más contexto inyectado que este texto; si lo percibes,
  `State` = `DECLARED` y un elemento por fuente con `Kind`, `Source` y `SizeOrSha256` (un entero: tamaño aproximado en bytes).
- `InputsRead`: las rutas, relativas al directorio de trabajo, que hayas leído con Read (se espera solo la del objeto).
- `KnownLimitations`: las limitaciones de esta revisión (por ejemplo, que es una medición del transporte).
- `RecommendedNextAction`: una frase.
- `Verdict`: `AGREED`, `CHANGES REQUIRED` o `BLOCKED — OWNER DECISION`, según tu revisión.
- `RequiredFindings` y `OptionalFindings`: con la estructura del esquema; en `PremiseRefs`, `Path` = la ruta del objeto, `Section` = la clave
  JSON, `LineStart` y `LineEnd` = líneas que Read te mostró y `Quote` = el texto literal de esas líneas. Sin hallazgos: `[]`.
- `FindingDispositions`: una por cada hallazgo nuevo, con `State` = `OPEN`, `LineageId` = su `FindingId`, `SupersededBy` = `[]`, un
  `Rationale`, `EvaluatedObject` = `{"commit": "d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99", "path": "docs/automation/decisions/FX-U1-T1.gate-contract.json", "blob": "628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0"}`
  y las mismas `PremiseRefs` del hallazgo. No hay hallazgos previos abiertos. Sin hallazgos: `[]`.