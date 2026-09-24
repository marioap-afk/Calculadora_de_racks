# Borrador de entrada FOUNDATIONS — I-59 AUTH-08/AUTH-12

Esta es una redaccion candidata para publicacion posterior. No modifica `docs/FOUNDATIONS.md`, no declara
READY ni convierte por si sola la entrada en publicada. `Status: STABLE` expresa el estado propuesto una
vez completadas conformidad, Candidate, Owner Validation aplicable, integracion y cierre documental.

Name: Shared View Foundation — AUTH-08 Placement Facts and AUTH-12 Block Requirements
Status: STABLE
Authority: `RackSourcePlacementCaptureAdapter` copia los valores AutoCAD a `RackSourcePlacementInput`; `RackSourceTransformClassifier` publica hechos neutrales AUTH-08. `RackViewPreparationAdapter.PrepareV2`, `RackHeaderPieceRequirementExtractorV2` y `RackHeaderBlockRequirementRoleClassifier` conservan la address tipada y producen requirements AUTH-12 por instancia. `AutoCadExternalLibraryBlockQuery` observa la biblioteca externa; `AutoCadLibraryBlockQuery` conserva la presencia V1 del dibujo activo.
Persistence: NONE. No hay schema, DTO persistido, serializer, store, Xrecord ni migracion nuevos; los carriers y facts viven solo durante preparacion, consulta y consumo.
Mutation contract: captura, clasificacion, extraccion, proyeccion y consulta son neutrales; no aceptan, rechazan, remedian ni colocan. Query e import permanecen ports separados; el importador V1 puede mutar el dibujo solo cuando su caller lo solicita, y la query V1 posterior sigue siendo la autoridad de presencia final en el dibujo.
Extension point: consumidores clasifican `RackSourcePlacementInput` con tolerancias explicitas, usan `PrepareV2`/`IRackPieceRequirementExtractor<TPayload>` para identidad por pieza/vista, y consumen `ILibraryPieceAvailabilityQuery` mas `LibraryPieceAvailabilityFlowV2` sin reconstruir desde strings, payloads o presencia del dibujo.
Decision source: Consensus Freeze V3 de I-59 (`docs/initiatives/I-59-proposal-v3.md`), ADR-0044, y las autoridades Shared View Foundation previas de I-57 para `RackViewAddress`, `RackViewPreparationAdapter`, `Transform2D`, `Point3D`, `Vector3D`, query/import V1 y `LibraryBlockRequirement`.
Protecting tests: `I59F2ContractTests`, `I59F2BoundaryGuardTests`, `I59F3ConsumerIntegrationTests`, `I59F3CorrectionTests`, `I59F4ConformanceTests`, `SharedViewFoundationF1CharacterizationTests`, `SharedViewFoundationF5Tests` y `SharedViewFoundationF6Tests`.
Known limitations: consumer policy queda fuera, incluidos Rigid, Orthographic, Relative Frame Window, accept/reject, remedy, anchor/placement, mensajes y transacciones. I-55 G14 no queda desbloqueado hasta integrar I-59 y verificar el receipt `integration/I-59`. AUTH-15 queda fuera. No existe persistencia I-59. V1 sigue coexistiendo. Presencia en el dibujo activo y presencia en la biblioteca externa son hechos distintos. La redaccion requiere re-verificacion sobre el futuro `FINAL_CANDIDATE_SHA` antes de publicarse.
Last changed by: I-59

```text
F4 verification basis = F4 product/closure SHA
Final Candidate re-verification = REQUIRED before publication
```
