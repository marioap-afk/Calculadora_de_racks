# I-65 — F4: filas finales, conformidad técnica (D-09) y solicitud de F4-D (D-10)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «Engineering Approval Package — D-01 to D-08»
([texto literal](../automation/decisions/I-65-owner-disposition-engineering-approval-d01-d08.original.txt); decisiones
[§61](../automation/decisions/I-65.md)). Sigue al [paquete de aprobación](I-65-f4-engineering-approval-package.md).

> **ENGINEERING BASIS = ACCEPTED FOR REF-CANT-01. FINAL ENGINEERING SIGN-OFF = PENDING. MAPPING UPDATE = NO.** La tabla del producto
> no cambia (SHA-256 `5229231e…4048`, dos filas `proposed`). `verifiedBy` y `verifiedOn` siguen vacíos. F4-D no se emite; sin Workers;
> Codex 41 de 46 y `attempts` 6 de 6, sin habilitar la reserva.

## 1. Estado de D-01 a D-08

| # | Estado | Alcance |
|---|---|---|
| D-01 | **APPROVED** | Geometría publicada de W12X26 y HSS 4X4X1/4 (AISC Shapes Database v16.0) = VERIFIED respecto a esa publicación; las derivaciones siguen DERIVED |
| D-02 | **APPROVED AS DERIVED** | Relación 0.93 y radio exterior 2·tdes: convenciones que reproducen las propiedades publicadas; no son citas normativas verificadas ni parámetros nuevos de las filas |
| D-03 | **APPROVED AS MODELING CONVENTION** | E = 29 000 ksi en los dos materiales, como hipótesis de la equivalencia de transferencia de REF-CANT-01; cita normativa sigue pendiente |
| D-04 | **APPROVED AS MODELING CONVENTION** | ν = 0.30, con el mismo alcance; cita pendiente |
| D-05 | **APPROVED FOR PILOT ONLY** | 490 pcf; DIF-01 (RAM ≈ 486.94 pcf, −0.62 %) fuera de la tolerancia original, aceptada solo como limitación informativa sin peso propio; tolerancias intactas |
| D-06 | **APPROVED AS REFERENCE IDENTITIES** | ASTM A992/A992M-22 y ASTM A500/A500M-23 Grado C; distintas de la identidad de material de RAM y del certificado del material real; Fy y Fu informativos |
| D-07 | **APPROVED: POISSON RATIO** | `poissonRatio` 0.30 y `shearModulus` null; sin segunda fuente de G |
| D-08 | **VARIANT B APPROVED IN PRINCIPLE → condición mecánica CUMPLIDA** | Comprobación de §1.1; texto sin cambios |

### 1.1 Comprobación mecánica de D-08

| Comprobación | Resultado | Evidencia |
|---|---|---|
| Coincidencia literal con el paquete | Los dos textos se extrajeron por programa de los bloques de la Variante B del paquete (`8d16eb52`) y son los que figuran en §2. SHA-256 UTF-8: W12X26 `E149A548C78B1496EC5285445BC99D95D772EA79891557F5260D12EF45DE0E25` (860 caracteres); HSS `830FFE891093C1258F1D4A750E17984851281570365ECF9FC9577D28BE108F93` (1001) | [script](../automation/evidence/I-65-f4/d08-variant-b-check.py.txt), [salida](../automation/evidence/I-65-f4/d08-variant-b-check.out.txt) |
| Esquema | Una línea, ASCII, reglas `IsPrintable` del lector, sin comillas ni barras invertidas; ida y vuelta JSON exacta; `sectionId`, nombres de RAM y materiales sin cambio | ídem |
| Lector del producto | `RamSectionMapReader.Read` del `RackCad.Application.dll` congelado del smoke (SHA-256 `f9a97451…7f0f`, igual al del manifiesto de `745653e1`; sin cambios de `src`, `tests`, `assets` ni `tools` desde ese commit): con los valores finales y `proposed` → **`Valid`**, E 29000 ksi, ν 0.3, G null, 490 pcf, `source` íntegro; con `verified` → **`Invalid` solo** por `RAM-MAP-VERIFIED-INCOMPLETE` en `verifiedBy` y `verifiedOn` de las dos filas | [script](../automation/evidence/I-65-f4/d08-reader-check.ps1.txt), [salida](../automation/evidence/I-65-f4/d08-reader-check.out.txt) |
| Limitaciones de las fuentes | Presentes: citas primarias no leídas; E y ν como convenciones; 490 pcf derivado; DIF-01 solo sin peso propio; valores de RAM no equivalentes; edición del catálogo de RAM sin verificar; RAM identificado aparte de la referencia ASTM. Ausentes: toda afirmación de cita normativa leída o de certificación | salida del script |

**Sin discrepancia material**: ningún enunciado es falso ni contradice D-01..D-07. Observaciones **no materiales** (el texto no cambia sin
nueva disposición):

- O-1: dice «current ASTM edition»; es cierto a la fecha de verificación, y D-06 la llama edición de referencia.
- O-2: no dice expresamente que las referencias ASTM y las convenciones no son un certificado del material real; tampoco afirma lo
  contrario, y la declaración de §3 lo deja explícito.
- O-3: las filas de la tabla del producto se aplican a toda exportación Cantilever con esas dos secciones, no solo a REF-CANT-01. En V1 el
  peso propio está excluido por el código (`StructuralModel.IncludesSelfWeight` = `false`; el script de construcción exige peso propio
  cero en cada caso de carga), así que DIF-01 no puede alcanzar un modelo con peso propio mientras rija V1.

## 2. Texto literal final de las dos filas

Archivo: [d08-rows-final.json](../automation/evidence/I-65-f4/d08-rows-final.json) (SHA-256
`B4CEC50140C434001E09DD8A5812C83CCB2A86759F1B576CB5AEA275311BF993`). Es la tabla completa que escribiría F4-D; **los cuatro `null` de
`verifiedBy` y `verifiedOn` son los únicos campos pendientes (D-09)** y, mientras sigan vacíos, el lector rechaza las filas `verified`.

```json
{
  "schema": "rackcad-ram-elements-section-map/v1",
  "rows": [
    {
      "sectionId": "AISC-W-W12X26",
      "ramSection": {
        "name": "W 12X26",
        "table": "W",
        "country": "United States"
      },
      "materialId": "ASTM-A992-GR50",
      "ramMaterial": "A992 Gr50",
      "expectedMaterial": {
        "elasticModulus": {
          "value": 29000,
          "unit": "ksi"
        },
        "poissonRatio": 0.3,
        "shearModulus": null,
        "unitWeight": {
          "value": 490,
          "unit": "pcf"
        },
        "source": "Geometry: AISC Shapes Database v16.0 (Aug 2023; file SHA-256 82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496), row W12X26 as published: A 7.65 in2, Ix 204 in4, Iy 17.3 in4, J 0.300 in4; independent derivation within 0.24 %. Material: ASTM A992/A992M Grade 50 (current ASTM edition A992/A992M-22). E 29000 ksi and nu 0.30: modeling conventions for structural steel accepted at verification; primary citation not read in I-65. Unit weight 490 pcf: steel density implied by the AISC v16.0 HSS nominal weights (median 490.00 pcf). RAM: United States/W/W 12X26, material A992 Gr50 (E 29000 ksi, nu 0.3, 486.94 pcf). DIF-01: RAM unit weight -0.62 %, informative only for the pilot transfer test without self weight; RAM values are not equivalent for later analyses. RAM catalog formal edition not verified. Ref: I-65 F4 engineering approval package."
      },
      "status": "verified",
      "verifiedBy": null,
      "verifiedOn": null
    },
    {
      "sectionId": "AISC-HSS-RECT-HSS4X4X_250",
      "ramSection": {
        "name": "HSS_SQR 4X4X1_4",
        "table": "HSS_SQR",
        "country": "United States"
      },
      "materialId": "ASTM-A500-GRC",
      "ramMaterial": "A500 GrC rectangular",
      "expectedMaterial": {
        "elasticModulus": {
          "value": 29000,
          "unit": "ksi"
        },
        "poissonRatio": 0.3,
        "shearModulus": null,
        "unitWeight": {
          "value": 490,
          "unit": "pcf"
        },
        "source": "Geometry: AISC Shapes Database v16.0 (Aug 2023; file SHA-256 82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496), row HSS4X4X1/4 as published: tnom 0.25 in, tdes 0.233 in (0.93 tnom), A 3.37 in2, Ix = Iy 7.80 in4, J 12.8 in4; independent derivation with outside corner radius 2 tdes within 0.15 %. Material: ASTM A500/A500M Grade C, square and rectangular (current ASTM edition A500/A500M-23). E 29000 ksi and nu 0.30: modeling conventions for structural steel accepted at verification; primary citation not read in I-65. Unit weight 490 pcf: steel density implied by the AISC v16.0 HSS nominal weights (median 490.00 pcf). RAM: United States/HSS_SQR/HSS_SQR 4X4X1_4, material A500 GrC rectangular, not A500 GrC rounded (E 29000 ksi, nu 0.3, 486.94 pcf). DIF-01: RAM unit weight -0.62 %, informative only for the pilot transfer test without self weight; RAM values are not equivalent for later analyses. RAM catalog formal edition not verified. Ref: I-65 F4 engineering approval package."
      },
      "status": "verified",
      "verifiedBy": null,
      "verifiedOn": null
    }
  ]
}
```

## 3. Declaración de conformidad técnica (para aprobación expresa del verificador)

> **Declaración de conformidad técnica — I-65, modelo analítico del piloto REF-CANT-01**
>
> Como verificador, declaro que he revisado las dos filas de la tabla de mapeo de secciones y materiales del adaptador de RAM Elements
> —`AISC-W-W12X26` → `W 12X26` / `A992 Gr50` y `AISC-HSS-RECT-HSS4X4X_250` → `HSS_SQR 4X4X1_4` / `A500 GrC rectangular`—, con el texto
> literal de `d08-rows-final.json` (SHA-256 `B4CEC501…F993`), y que las acepto **únicamente para el modelo analítico del piloto
> REF-CANT-01** y su prueba de equivalencia de transferencia, con estas bases y limitaciones:
>
> 1. La geometría es la publicada en la AISC Shapes Database v16.0 (SHA-256 `82D0CEB9…3496`); las derivaciones matemáticas son
>    complementarias.
> 2. E = 29 000 ksi y ν = 0.30 son hipótesis de material adoptadas como convención de modelado; las citas normativas no leídas siguen
>    pendientes.
> 3. El peso unitario de 490 pcf es la referencia del piloto. DIF-01 (RAM ≈ 486.94 pcf, −0.62 %) se acepta solo como limitación
>    informativa de la prueba sin peso propio; no cambia ninguna tolerancia ni se extiende a modelos con peso propio.
> 4. ASTM A992/A992M-22 y ASTM A500/A500M-23 Grado C son identidades de referencia; no equivalen a la identidad del material en RAM
>    ni al certificado del material real. Fy y Fu son informativos.
> 5. La edición formal del catálogo de RAM no está verificada.
>
> Esta declaración no certifica propiedades reales de fabricación, no es un diseño ni una revisión de resistencia, y no se extiende a otras
> secciones, materiales, modelos o análisis.
>
> Verificador (`verifiedBy`): ________________  Fecha (`verifiedOn`, aaaa-mm-dd): ____________

## 4. Campos exactos pendientes de D-09

| Campo | Fila | Regla del lector |
|---|---|---|
| `rows[0].verifiedBy` | `AISC-W-W12X26` | Texto JSON no vacío, de una línea; sin caracteres de control ni de formato; el espacio es el único blanco admitido |
| `rows[0].verifiedOn` | `AISC-W-W12X26` | Exactamente `yyyy-MM-dd` (10 caracteres), fecha de calendario válida |
| `rows[1].verifiedBy` | `AISC-HSS-RECT-HSS4X4X_250` | Igual que `rows[0].verifiedBy` |
| `rows[1].verifiedOn` | `AISC-HSS-RECT-HSS4X4X_250` | Igual que `rows[0].verifiedOn` |

Además: la aprobación expresa de la declaración de §3. Salvo indicación del Owner, las dos filas llevan los mismos valores. Ambos campos
entran en la huella `AnalysisInputFingerprint`. El Coordinator no propone ni atribuye la identidad del verificador.

## 5. Confirmación: F4-D sin nuevas decisiones arquitectónicas

| # | Comprobación | Resultado |
|---|---|---|
| A-1 | Base del contrato | `src`, `tests`, `assets` y `tools` sin cambios desde `745653e1`: el [contrato](I-65-f4-d-mapping-verified-contract.md) aplica tal cual al HEAD |
| A-2 | Esquema | `rackcad-ram-elements-section-map/v1` admite todos los campos; sin cambio de esquema ni parámetros nuevos (D-02); el lector del producto acepta los valores (§1.1) |
| A-3 | Alcance de escritura (§3 del contrato) | Cubre todas las pruebas que leen la tabla distribuida y fijan su estado `proposed`: 3 de `StructuralAnalysisRamElementsMapReaderTests`, 2 de `...PackageValidationTests`, 1 de `...TranslationTests` y la clase `...PluginCommandProductTableTests`, con los nombres exactos del contrato. La cuarta prueba que lee la tabla (`LasDosFilasDelArchivoLlevanLosNombresQueRamAceptoEnP1`) solo comprueba nombres, que no cambian. Las pruebas de guarda solo miran el elemento `Content`, la ruta y `deploy/`. Ningún hash de la tabla está fijado en `src`, `tests` ni `tools` |
| A-4 | DIF-01 y tolerancias | Sin cambio: V1 excluye el peso propio por código (O-3); `unitWeight` lleva 490, no el valor de RAM (D-INV-03) |
| A-5 | Condición de entrada §1 del contrato | §1.1 (tarjeta V-01..V-09): la cumplen D-01..D-08 y D-09 cuando se apruebe. §1.2 (smoke con filas `proposed`): cumplida (CD-I65-F4-92). §1.3: la orden D-10 |
| A-6 | Decisiones restantes | Solo D-09 (firma) y D-10 (orden y presupuesto). **Ninguna arquitectónica** |
| A-7 | Riesgo técnico residual, no arquitectónico | D-INV-04 (REF-CANT-01 → `Valid` con la tabla distribuida) aún no se ha ejecutado; el smoke solo mostró los dos `MATERIAL-ROW-UNVERIFIED`. Si RED/GREEN revela otro diagnóstico: STOP y vuelta al Owner, sin improvisar |
| A-8 | Consecuencia conocida | F4-D cambia el DLL (la tabla se copia a la salida); el segundo smoke con filas `verified` y la primera corrida en RAM son decisiones aparte |

## 6. Solicitud concreta de autorización

**D-09 (Owner):** aprobar expresamente la declaración de §3 y dar, literalmente, `verifiedBy` = «…» y `verifiedOn` = «aaaa-mm-dd» (las
dos filas).

**D-10 (Owner):** autorizar `F4-D-SECTION-MAP-VERIFIED` con estas condiciones:

1. Contrato: [`I-65-f4-d-mapping-verified-contract.md`](I-65-f4-d-mapping-verified-contract.md) tal como está en el commit de este
   registro (rutas §3, invariantes D-INV-01..07, cadena RED → GREEN y verificación §5).
2. Valores: los de [`d08-rows-final.json`](../automation/evidence/I-65-f4/d08-rows-final.json) (SHA-256 `B4CEC501…F993`) con los cuatro
   `null` de D-09 sustituidos por los valores aprobados; ningún otro cambio en la tabla.
3. Condición de entrada §1.1 cumplida por D-01..D-09.
4. Presupuesto sin reinicio de contadores: Codex con tope **48** (consumo actual 41), `max_attempts` **9** (siguiente intento: 7), hasta
   **3** Workers, **1** revisión del Architect; controles K1-K6 y C-12.
5. Fuera de alcance: `src/`, tolerancias, AutoCAD, RAM, segundo smoke, Candidato de hito y GATE PASS de F4.
6. Final: tras GREEN, verificación, suite Core y CI del SHA exacto, el Coordinator registra y se detiene.
