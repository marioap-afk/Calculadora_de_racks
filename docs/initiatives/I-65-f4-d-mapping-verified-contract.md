# I-65 — F4-D: contrato propuesto para actualizar las filas a `verified` (borrador; no emitido)

> **Estado (2026-10-16): EMITIDO, EJECUTADO Y CERRADO.** Autorizado en las decisiones §62 (D-09 y D-10) y cerrado en §66: la tabla del
> producto tiene las dos filas `verified` en `51fc82f1` (verificación del Controller `R20261016T161137Z-e795`, `EXECUTION_VERIFIED`). El texto
> de abajo es la revisión `9f6fff6e` que el Owner autorizó; no cambia.

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation and Host Readiness» §2
([texto literal](../automation/decisions/I-65-owner-disposition-f4-validation-host-readiness.original.txt); decisiones
[§49](../automation/decisions/I-65.md)).

> **OD-F4-12 = CONDITIONAL APPROVAL. MAPPING UPDATE = PENDING ENGINEERING APPROVAL.** Este es el contrato que se **presentaría** al
> Owner; no se emite ni se ejecuta hasta que el Owner valide expresamente las propiedades y las fuentes de la
> [hoja](I-65-f4-engineering-validation-sheet.md) (tarjeta §6). No reinicia contadores: el presupuesto de §6 es una **petición**.

## 1. Condición de entrada (todas)

1. Tarjeta §6 de la hoja confirmada **expresamente** por el Owner para las dos filas, con su texto literal custodiado: V-01..V-09
   resueltas, incluidos el oráculo de material (V-02), la edición del catálogo de RAM (V-01), el texto de `source` (V-08) y `verifiedBy` /
   `verifiedOn` (V-09).
2. Smoke de AutoCAD **ya hecho con las filas `proposed`** (la disposición exige comprobar el fallo cerrado antes del cambio).
3. Orden del Owner que autorice este contrato, sus rutas y su presupuesto.

## 2. Tarea

`F4-D-SECTION-MAP-VERIFIED`: escribir en la tabla del producto las dos filas `verified` con los valores validados por el Owner, con una
cadena RED → GREEN y las pruebas que fijan la tabla distribuida (T4-18). Solo datos y pruebas; ningún cambio de código de producción.

## 3. Rutas

**AllowedWriteScope (exactas):**

| Ruta | Cambio permitido |
|---|---|
| `assets/structural-analysis/ram-elements/section-material-map.json` | en las **dos filas existentes**: `status` → `verified`; `ramSection.table` y `.country`; `expectedMaterial` (`elasticModulus`, `poissonRatio` **o** `shearModulus`, `unitWeight`, `source`); `verifiedBy`; `verifiedOn`. Sin filas nuevas, sin cambio de esquema, de `sectionId`, de nombres de RAM ni de materiales |
| `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs` (nuevo) | T4-18 sobre la tabla distribuida |
| `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsMapReaderTests.cs` | edición **solo** de `ElArchivoDeDatosEsValidoYTieneExactamenteLasDosFilasProposedDeRefCant01`, `ElArchivoDeDatosNoTrajoValoresDeIngenieriaNiVerificacion` y `ConLaTablaTalComoSeEntregaTodaExportacionEsUnsupportedYNoSeAsignaNingunMaterial` (fijan el estado `proposed` de la tabla distribuida) |
| `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageValidationTests.cs` | edición **solo** de `ConLaTablaDelProductoTalComoSeEntregaElResultadoEsUnsupportedYSinContenido` y `LaPrecedenciaEsLaDeCdI65F104InvalidGanaAUnsupported`; la segunda conserva su cobertura de precedencia con una tabla `proposed` de fixture en lugar de la distribuida |
| `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsTranslationTests.cs` | edición **solo** de `UnResultadoQueNoEsValidoSePropagaConSuResultadoYSinPlan`, con una tabla `proposed` de fixture |
| `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandProductTableTests.cs` | la clase pasa a afirmar la tabla del producto `verified`: modelo `Valid` y paquete `Valid` en memoria (sin publicar); la prueba del `Kind` distinto de Cantilever se conserva |

**Prohibido:** todo lo demás, en particular `src/`, `tools/`, los `.csproj`, `RackCad.sln`, `assets/catalogs/`, las demás pruebas, el
Freeze, ADR-0050, la plantilla de construcción y el volcado, el comparador y la guarda del comando.

## 4. Invariantes

| ID | Invariante |
|---|---|
| D-INV-01 | Solo cambian las dos filas y solo en los campos de §3; el lector devuelve `Valid` con las dos filas `IsVerified` |
| D-INV-02 | Los valores escritos son **exactamente** los de la tarjeta confirmada por el Owner (prueba con los literales) |
| D-INV-03 | DIF-01 **no** se codifica como cambio de valor esperado: `unitWeight` es el valor esperado con su fuente (490 pcf si el Owner lo confirma), no el de RAM; ninguna tolerancia cambia |
| D-INV-04 | T4-18: con la tabla distribuida, `CantileverStructuralExport.Build` de `REF-CANT-01` da `Valid` y la fábrica da un paquete `Valid` en memoria; cambiar un campo de una fila verificada cambia `AnalysisInputFingerprint` (INV-04) |
| D-INV-05 | El comportamiento con filas `proposed` sigue cubierto por las pruebas de fixture existentes (proyección, traducción, paquete); ninguna se borra |
| D-INV-06 | Ninguna prueba escribe fuera de su carpeta temporal ni publica un paquete |
| D-INV-07 | Controles del Owner K1-K6 y condición C-12 |

## 5. Cadena y verificación

- **RED:** un commit con la prueba nueva y las ediciones nombradas, que esperan la tabla `verified`. Falla contra la tabla `proposed`
  por aserción y no por compilación.
- **GREEN:** un commit con **solo** el cambio de la tabla. `git diff RED..GREEN -- tests` vacío.
- **Verificación** (protocolo I-61 §16): planificación y verificación del Controller Codex `gpt-6-luna`/`high` en solo lectura;
  aceptación A1-A8; 14 comprobaciones; Worker `claude-sonnet-5-5`; controles nc1-nc3 (Codex) y nc4 (sin invocación); alcance mecánico
  `scope_check`; filtro `FullyQualifiedName~RackCad.Tests.StructuralAnalysis`; suite Core completa; CI del SHA exacto.
- **Después:** el DLL que incluya la tabla `verified` sale del Candidato de hito (la tabla se copia a la salida de compilación); hasta
  entonces ningún DLL en uso cambia.

## 6. Presupuesto que se pediría (sin reinicio de contadores)

| Recurso | Consumo actual | Necesario | Petición |
|---|---|---|---|
| Codex | 41 de 46 | planificación 1 + verificación 1 + nc1-nc3 3 = 5; reserva 2 (una planificación rechazada o una reverificación) | tope **48** (+2 sobre 46) |
| `attempts` | 6 de 6 (sin margen) | 1 intento + hasta 2 correcciones | `max_attempts` **9** (+3); el contador sigue en 7, 8 y 9, sin reinicio |
| Workers | — | 1 por intento | hasta 3 |
| Architect | — | revisión breve del diff (subagente, sin Codex) | 1 |

## 7. Forma de las filas (los valores entre `<…>` salen de la tarjeta confirmada; ninguno se atribuye antes)

```json
{
  "sectionId": "AISC-W-W12X26",
  "ramSection": { "name": "W 12X26", "table": "<V-07, hoy W>", "country": "<V-07, hoy United States>" },
  "materialId": "ASTM-A992-GR50",
  "ramMaterial": "A992 Gr50",
  "expectedMaterial": {
    "elasticModulus": { "value": "<V-03, candidato 29000>", "unit": "ksi" },
    "poissonRatio": "<V-02, candidato 0.3; null si el oráculo es G>",
    "shearModulus": "<V-02: null si el oráculo es ν>",
    "unitWeight": { "value": "<V-04, candidato 490>", "unit": "pcf" },
    "source": "<V-08>"
  },
  "status": "verified",
  "verifiedBy": "<V-09>",
  "verifiedOn": "<V-09, yyyy-MM-dd>"
}
```

La fila `AISC-HSS-RECT-HSS4X4X_250` tiene la misma forma: `HSS_SQR 4X4X1_4`, tabla `<V-07, hoy HSS_SQR>`, `ASTM-A500-GRC` →
`A500 GrC rectangular`.
