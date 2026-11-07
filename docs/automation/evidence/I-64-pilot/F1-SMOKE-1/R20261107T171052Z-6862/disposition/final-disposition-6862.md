# R20261107T171052Z-6862 — Disposición final (resumen de la sesión; el literal manda)

Literal del Coordinator: `disposition/coordinator-smoke1-final-review.md`.

```text
Smoke-1 = PASS (solo esta ocurrencia completa)
F1 = PASS; C1 = PASS / host validated; C2 = PASS / host validated
Candidate cc7503e3 = SMOKE1_VALIDATED_CANDIDATE
RackCad.Plugin.dll SHA-256 = 3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
SM-29: OPTION A ACCEPTED; FIRST ATTEMPT = OPERATOR / INPUT INCIDENT, HISTORICAL; CLEAN REPEAT = AUTHORIZED RECOVERY / AUTHORITATIVE; S1-02 = PASS
Recent File List = EXPECTED MRU / UI-STATE ACTIVITY; SEMANTIC RACKCAD DRIFT = NO; INFORMATIVE / NON-BLOCKING
HKCU Applications = WRITE_ACTIVITY YES; VALUE_CHANGE NO; SEMANTIC_DRIFT NO; SECURITY_DRIFT NO; LOAD_DRIFT NO; INFORMATIVE / NON-BLOCKING
.sv$ = EXPECTED AUTOCAD TEMPORARY HOST ARTIFACT; NON-BLOCKING
PaletteKeyAcceptance = NOT_RECORDED / NOT_NEEDED_FOR_DIFF_DISPOSITION (PROCEDURAL RECORDING OMISSION, NON-BLOCKING)
```

**Procedencia de la opción A.** La revisión final la acepta de forma expresa («OPTION A ACCEPTED»). Eso resuelve lo que `disposition/disposition-sm29-option-a.md` dejó pendiente.

**Condición de `PaletteKeyAcceptance`.** La revisión la anula si T3C o HOST-EXTRAS contienen una diferencia de la clave de la paleta que requiriera aceptación. La sesión lo comprobó con una lectura mecánica (`disposition/palette-key-check.txt`):
- T3C: `Disposition` CLEAN; las 3 entradas son ALLOWED por R-A-03 y ninguna nombra paleta, RackCad ni Workspace; REPORT = 0.
- BASELINE y POST: 70 líneas con «palette» en cada una, ninguna con RackCad; **0 líneas con «palette» difieren entre ellas.**
- HOST-EXTRAS: los 2 hallazgos son `Select File` y `Recent File List`; ninguno nombra la paleta.

Resultado: **ninguna diferencia de la clave de la paleta; la condición de anulación no se da.**

**Condiciones de `.sv$`, comprobadas en `logs/sm41.txt`:** ningún DWG guardado; A, B, C, D y biblioteca byte a byte iguales; ninguna aserción depende de un `.sv$`; no se usan como evidencia de producto.

**El registro firmado no se reescribe.** El registro conserva el primer gesto como se observó, con PASS propuesto en SM-29 y `PaletteKeyAcceptance` = NOT_RECORDED. Esta disposición y el literal son los que asignan el resultado. Las ocurrencias históricas no cambian: 184b = STOP, 0a77 = NOT PASS, a578 = STOP y 785a = NOT PASS / CLOSED.

**Arrastre durante la pausa (V1 → V2).** Lo hizo el Owner sin petición de la sesión. Se conserva en el registro y en el diario como parte del incidente de SM-29 y no se usa como prueba de S1-02: la prueba es la repetición limpia, V2 → V3 con `VIEWSIZE` igual.
