# I-65 — F7: revisión de conformidad del Coordinator (Freeze y ADR-0050)

- **Objeto:** producto del Candidato de hito F5 `bd1fcf8a` (árbol de `51fc82f1`), sin cambios desde entonces; evidencia de F1, F3, F4 y F5.
- **Autoridades:** Freeze [Proposal V3.1](I-65-proposal-v3.1.md) (blob `4e6e1feb…`) y
  [ADR-0050](../adr/0050-interoperabilidad-analisis-estructural.md) (blob `c18def42…`, aceptado), sin A-n.
- **Alcance:** AC-F7-04 y AC-F7-05 de [I-65-f7-preparation.md](I-65-f7-preparation.md). La revisión independiente del Architect se versiona
  aparte. Esta revisión no rediseña: una mejora queda como seguimiento (LIFECYCLE §9).
- **Estados:** CONFORME (con evidencia), LIMITACIÓN (declarada en el Freeze, el ADR o una decisión del Owner y conservada) y PENDIENTE
  READY (se repite o se decide en READY por regla, no por defecto de F7).

## 1. Gates del Freeze (§6)

| Gate | Estado | Evidencia |
|---|---|---|
| F0 | CONFORME | P1, Proposal V3.1, ADR-0050 aceptado, AGREED, Freeze (decisiones §8-§9) |
| F1 (con F2 del mandato) | CONFORME | GATE PASS sobre `4514a99a` (CD-I65-F1-21) |
| F3 | CONFORME | GATE PASS sobre `1d8c713e` (CD-I65-F3-17) |
| F4 | CONFORME | GATE PASS sobre `1e7d8fc2` (CD-I65-F4-221); OV-I65-01, 03..07 sin FAIL ni INCOMPLETE |
| F5 | CONFORME | GATE PASS sobre `45aaad6e` (CD-I65-F5-61); OV-I65-02 PASS |
| F6 | NO APLICA | fuera de V1 (D-14, Q-O5) |
| F7 | en curso | este documento y AC-F7-01..12 |
| READY | PENDIENTE READY | READY-01..09 y OV completa sobre el Candidato final |

## 2. Decisiones D-01..D-15

| Decisión | Estado | Verificación |
|---|---|---|
| D-01 capas y frontera de proveedor | CONFORME | proveedor en `Application/StructuralAnalysis/Providers/RamElements/`; guardas G1-G3 (`StructuralAnalysisSourceGuardTests`, `StructuralAnalysisRamElementsGuardTests`) |
| D-02 `StructuralModel` V1 | CONFORME | `StructuralModel.cs`; F1 |
| D-03 unidades | CONFORME | `StructuralLength`/`StructuralForce`/`StructuralMomentOfInertia`; `ForceUnits` y `MassUnits` propietarios únicos; `StructuralSectionUnits` consume `MassUnits` (INV-16); OV-I65-02 recalculada a mano por el Architect en F5 |
| D-04 ejes y orientación | CONFORME | `RamLocalAxes`; `StructuralAnalysisRamElementsAxesTests`; filas LOCAL-AXES PASS en OV-I65-01 y 02; OV-I65-06 |
| D-05 idealización | CONFORME | golden de `REF-CANT-01` (`StructuralAnalysisCantileverGoldenTests`); coordenadas PASS en OV-01 y 02 |
| D-06 secciones | CONFORME | nombres de RAM por la tabla verificada; SECTION-NAME PASS |
| D-07 materiales | CONFORME / LIMITACIÓN | MATERIAL-NAME PASS; propiedades NOT_EVALUATED por la API (hoja D-09 firmada); DIF-01 −0.62 % informativa |
| D-08 errores, versión, escritura atómica | CONFORME | `StructuralExportOutcome`; `StructuralPackagePublisher`; OV-I65-03 (sin carpeta) y OV-I65-07 (puerta de versión) |
| D-09 paquete de RAM | CONFORME | `RamBuildPackageFactory`, plantilla fija; OV-01..07; paquete `Valid` 16/16 en W1 y W3 |
| D-10 supuestos (Q-O3) | CONFORME / LIMITACIÓN | supuestos con identificador y advertencia literal en el manifiesto; equivalencia ≠ validación |
| D-10b canal de cargas | CONFORME | `StructuralLoadFileReader` (`rackcad-structural-loads/v1`); OV-I65-02 PASS (6/6 en valor, posición y dirección) |
| D-11 equivalencia | CONFORME | comparador con la matriz §4; PASS en OV-01 (W2) y OV-02 (W4) contra modelos manuales independientes |
| D-12 provenance | CONFORME / LIMITACIÓN | huellas de fuente y de entrada; `STALE` en el comparador; volcado ligado a la sesión solo por evidencia operativa (A65-F4F-04, abierta para READY) |
| D-13 capacidad para ID30 y comando mínimo | CONFORME | `BuildStructuralModel`, `CompareReadbacks`, `CreateRamBuildPackage`, `RamDumpReader`; un solo archivo de comando (`RackRamElementsCommands.cs`) con su guarda; sin `Analyze` ni `ReadResults` |
| D-14 análisis y resultados | CONFORME | no existen; guarda de ADR-0017 (INV-11) |
| D-15 ADR-0050 | CONFORME | aceptado antes de F1 (Q-O6); texto congelado sin cambios |

## 3. Invariantes (§3)

INV-01..16 se materializaron con RED→GREEN en F1, F3 y F4 (revisiones del Architect de cada gate, sin REQUIRED abiertos) y siguen
protegidos por la suite Core del producto sin cambios. La regresión Full de F7 sobre el SHA de cierre vuelve a ejecutarlas. **CONFORME.**

## 4. Matriz de equivalencia (§4)

Cada fila evaluable dio PASS en OV-I65-01 (W2) y OV-I65-02 (W4). Las propiedades de sección y material son NOT_EVALUATED (la API no las
lee; D-09) y la categoría manual y el peso propio, INFORMATIVE. No hay filas de comportamiento (F6 fuera). **CONFORME, con la limitación
declarada.**

## 5. Validación del Owner (§5)

| OV | Estado |
|---|---|
| OV-I65-01, 03..07 | hito F4 PASS (W1 y W2); **se repiten sobre el Candidato final** (PENDIENTE READY) |
| OV-I65-02 | hito F5 PASS (W3 y W4); **se repite sobre el Candidato final** (PENDIENTE READY) |
| OV-I65-08 | PENDIENTE READY (lista de la guía manual sobre el Candidato final) |

## 6. Fundaciones (§7) y no-objetivos (§9)

- **Fundaciones:** las tres entradas redactadas en [I-65-foundations-entry.md](I-65-foundations-entry.md) cubren el «Introduces» y el
  «Extends» del Freeze. Están verificadas contra el código y se publican al integrar. **CONFORME.**
- **No-objetivos:** ninguno se implementó. Selectivo, Dinámico, Push Back y mezzanine, doble cara, `NegativeY`, C/L, base modelada,
  offsets, momentos, cargas distribuidas, peso propio, combinaciones, resultados y guardado de modelos responden `Unsupported` o no
  existen. **CONFORME.**

## 7. ADR-0050

| Decisión del ADR | Estado | Verificación |
|---|---|---|
| 1. Modelo neutral derivado, no persistido | CONFORME | el modelo no se guarda en el DWG; se regenera desde el sobre (D-02, D-13) |
| 2. Autoridad de producto intacta | CONFORME | ningún archivo de RAM se lee como autoridad de RackCad; el comparador solo informa |
| 3. Unidades (cláusula de compatibilidad) | CONFORME | pulgada canónica intacta; `LengthUnits`; `StructuralSectionUnits` bit a bit; propietarios únicos de lb y gₙ |
| 4. Frontera de proveedor | CONFORME | carpeta de proveedor y lista de permiso del comando, con guardas |
| 5. Supuestos explícitos | CONFORME | supuestos con identificador y advertencia en el manifiesto |
| 6. Sin exportación parcial | CONFORME | cuatro resultados; publicación atómica en carpeta nueva; OV-I65-03 sin carpeta |
| 7. RAM Elements por su API Python oficial | CONFORME | script de plantilla fija sobre modelo vacío con lectura de vuelta; sin automatizar la interfaz |
| 8. Provenance | CONFORME / LIMITACIÓN | huella de entrada verificada en la lectura de vuelta y en el comparador (`STALE`); el volcado no lleva huella (A65-F4F-04) |
| 9. ADR-0017 | CONFORME | sin utilización, ratios ni veredictos |
| Cláusula de compatibilidad con ADR-0005, ADR-0017, ADR-0021 §10 y ADR-0040 D3 | CONFORME | sin cambios de API ni de texto de esos ADR |

**EXP-01:** ninguna discrepancia de clase A entre código, ADR-0050 y Freeze. La ausencia de huella en el volcado (Decisión 8) es una
limitación conocida y dispuesta (opción 0 en F5; decisión del Owner pendiente para READY), no una contradicción con algo consumido.

**Estado del ADR:** «aceptado» desde 2026-10-11, sin edición. El índice de ADR se actualiza en el cierre de I-65 (nota del propio ADR y
CD-I65-F0-05); el número 0050 se comprueba en READY-04 contra `main`.

## 8. Observaciones de F7

| ID | Observación | Clase |
|---|---|---|
| F7-OBS-01 | La ayuda integrada (`RACKAYUDA`, `RackCommandReference`) no lista `RACKRAMPAQUETE`. El Freeze no lo exige (D-13: comando de línea mínimo, sin WPF ni menús). Añadirlo es código fuera de F7: decisión del Owner (READY o seguimiento). | NON-MATERIAL |
| F7-OBS-02 | El README no listaba el comando (WORKFLOW §8): se añade su fila y el enlace a la guía en F7. | EDITORIAL |

**Resultado del Coordinator:** sin desviaciones materiales; conforme con el Freeze y ADR-0050 dentro del alcance de F7.
