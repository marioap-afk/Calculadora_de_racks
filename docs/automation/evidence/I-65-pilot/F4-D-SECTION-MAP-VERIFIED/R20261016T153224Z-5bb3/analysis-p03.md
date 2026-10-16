# I-65 — F4-D-SECTION-MAP-VERIFIED: analisis del STOP P-03 de la planificacion de la correccion (Coordinator)

Planificacion R20261016T153224Z-5bb3 (intento 7, Codex 44, CorrectionOf R20261016T152242Z-2e6f / Ci), sobre el contrato de gate de la
primera delegacion sin reemitir (gate-contract.json identico, SHA-256 1d30597a8c19...).

## Hechos

- Salida valida contra rackcad-delegation/v1 (A1). A2, A3, A6, A7 y A8 en pass (A8 con ChainBaseSha e886b557, ChainRedSha fe9b6720 y los
  cinco ChainRedFiles en el orden de git). CorrectionOf exacto (RunId, Ci y el SHA-256 del analisis en minusculas).
- A4 fail: ForbiddenWriteScope[63] = " .github/" (con un espacio inicial) en lugar de ".github/"; el prohibido del contrato no queda
  cubierto.
- A5 fail: Authorities[1].Path = "docs/automation/decisions/I-65-owner-authorization-d09-d10-f4-d.original.txt" (con un guion insertado
  entre f4 y d) en lugar de "docs/automation/decisions/I-65-owner-authorization-d09-d10-f4d.original.txt".
- Invariants, StopConditions, RequiredTests y AllowedWriteScope exactos.

## Causa raiz

Corrupcion de transcripcion del Controller al proyectar dos colecciones (el mismo modo de fallo de CD-I65-F3-11 y CD-I65-F4-19): el
preflight que el prompt exigia no detecto ni el espacio inicial ni el guion insertado. No hay ambiguedad en el contrato ni cambio del
trabajo.

## Resolucion (precedentes CD-I65-F3-11 y CD-I65-F4-19)

STOP P-03 en la aceptacion; ningun Worker lanzado; la planificacion se REEMITE con el mismo contrato de gate y un aviso expreso de las dos
cadenas exactas (la ruta con "f4d" sin guion y ".github/" sin espacios) y de comparar por igualdad de cadenas sin recortar. No consume
attempts (sigue en 7). Codex: la reemision es la invocacion 45; quedan la verificacion (46), nc1 (47) y nc2 (48); nc3 seria la 49 y
excede el tope de 48: se pedira autorizacion al Owner antes de lanzarla (P-07).
