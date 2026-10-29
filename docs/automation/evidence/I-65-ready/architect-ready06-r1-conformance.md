## Revisión de conformidad del Architect (READY-06) sobre `507806bfcc37cf956836e9cb96bba114da4cec60`

**pwd:** `/d/Documentos/Codex/Calculadora de racks`, que es el checkout principal del Owner. No lo leí ni lo usé. En adelante `<worktree>` es el worktree de I-65. Todo se leyó con `git -C <worktree>` o con rutas absolutas de ese worktree. Rol: SAME-SESSION ROLE, solo lectura.

**Lo que hice y lo que no:**
- No escribí, no hice commit, push ni fetch, y no usé `merge-tree`.
- No ejecuté python ni pwsh, ni compilé, ni abrí AutoCAD o RAM.
- No leí `D:/r65-smoke`, `D:/r65-m05c` ni el scratchpad del Coordinator.
- Para el barrido K5: usé `awk`, `sed`, `sha256sum`, `comm`, `grep` y `wc` sobre objetos git y rutas del worktree, solo para filtrar texto y calcular hashes.
- Para I-64 y las otras ramas usé las refs remotas locales, sin hacer fetch.
- Al empezar y al terminar, HEAD era `ba4494b2`, igual a `origin/architecture/interop-analisis-estructural`, con el árbol limpio.

## Veredicto: **NON-CONFORMING** por una sola desviación

| ID | Desviación | Clase (LIFECYCLE §9) |
|---|---|---|
| A65-FC-01 | El oráculo de HB-02 en los procedimientos (línea 180) es «(1) y (3) `OK (158 comprobaciones)`», pero el mismo paso ordena ejecutar `verify-bundle.ps1` con `-BaselineInventoryPath`. En ese caso `Compare-Inventory` (`verify-bundle.ps1:315-330`, llamado en `:366-369`) suma **dos** comprobaciones antes del mensaje final (`:372`), así que la salida será **160**. Las 158 son la cuenta sin inventario base: así se midió en `build-bundle.ps1 -InventoryOutPath` (`tools/bundle-evidence.ps1.txt:16`) y en la CI (`eng/ci/verify-autocad-references.ps1:647`). Hay precedente en el repositorio: «107 … (105 en la primera, las 2 extra son la comparación de reproducibilidad)» (`I-18-autocad-validation.md:54-55`). Tal como está escrito, el oráculo no se puede cumplir. | NON-MATERIAL. No toca el Freeze, el producto ni las tolerancias. Severidad **REQUIRED** |

Todo lo demás es conforme: producto, Freeze, ADR-0050, invariantes, matriz §4, OV §5, fundaciones y no-objetivos. La corrección es solo documental. Según LIFECYCLE §9 y final-candidate §7, crea un SHA nuevo y reinicia READY-02; basta una relectura del Architect limitada al delta.

## Respuestas

**1. Identidad y frontera.**
- **De `6a58c2b1` a `507806bf`:** 62 archivos, todos en `docs/`.
  - El árbol raíz sin `docs/` es idéntico en `6a58c2b1`, `507806bf` y `ba4494b2` (`3ab0eb4b…`).
  - También son idénticos `src`, `tests`, `tools`, `deploy` y `.github` (`b6d89cd1`, `6937b65c`, `29485f42`, `09159b1d`, `80ccff3a`).
- **De `507806bf` a `ba4494b2`:** 19 archivos, todos en `docs/` (decisiones §113, evidencia §115, `state` y `candidate-507806bf/*`).
- **Padres y base:** el padre de `507806bf` es `d53367e5` y el de `ba4494b2` es `507806bf`. El merge-base es `bb0d5522`.
- **Secuencia temporal:**

  | Momento (UTC) | Hecho |
  |---|---|
  | 03:15:54Z | commit de `507806bf` |
  | 03:16:00Z | se crea la CI `push` (se publicó solo, como pide WORKFLOW §11.5) |
  | 03:17:40Z-03:27:37Z | evidencia local, con `dirty=0` antes y después |
  | 03:29:05Z | commit de `ba4494b2` |

- **La frontera es correcta** frente a WORKFLOW §11.4-§11.5 y LIFECYCLE §8: READY-05..09 se evalúan sobre el SHA que deja READY-04, y la comprobación `<Candidato>..HEAD ⊆ docs/` del cierre cubre los commits de evidencia. Tampoco reutiliza `5a5ca577`, `bd1fcf8a`, CD-I65-F4-178/184 ni A65-F5C-01.
- **Tres reservas:**
  - Necesita la confirmación expresa del Owner, y ya se le pide.
  - CD-I65-FC-16..18 (§113) son evaluaciones o hechos, no decisiones de contenido. Solo así no activan la regla literal de LIFECYCLE §6 («tras READY-04, cualquier decisión nueva crea un SHA nuevo»).
  - READY-07 debe leerse como «Candidato `507806bf`; la punta de la rama solo añade `docs/`».

**2. READY-09: CUMPLE.**
- **Acuerdo:** `3872fcc0:<Freeze>` = `11062a91` = `a9d7080b^:<Freeze>`, y `a9d7080b:<Freeze>` = `4e6e1feb`.
- **Commit de Freeze:** cambia **tres** líneas de cabecera (Estado, Frozen, Coordinator/Architect). Las tres están enumeradas en el acuerdo (`architect-r4b-agreed.md:36`), así que el diff está permitido. Los trailers Agreed-Blob, Agreed-Commit y Claim-Id coinciden con `state` (`0b4aad79…`).
- **Inmutabilidad:** ningún commit toca el Freeze entre `a9d7080b` y `507806bf`.
- **Unicidad y A-n:** no hay Freeze delta ni A-n; el contrato tiene `freeze_delta_ref: null` y `amendment_refs: []`.
- **ADR-0050** (`c18def42`) no cambia desde `ed8154c3`.
- **Compatibilidad de fuente V1:** la unidad es V2 T4 (evidencia §1), y el diff desde `bb0d5522` no toca contratos, Freeze ni ADR de otras unidades.
- **Errata:** final-candidate §2.1 dice «solo la línea `Frozen`» (A65-FC-07).

**3. Conformidad completa.**
- **Delta de producto respecto del objeto de F7 (`51fc82f1`):** los cinco archivos de R-D11 (target, verificador, harness, una prueba y README).
  - Ningún `.cs` de `src/` cambia.
  - La tabla sigue siendo `d088e2a9` en `51fc82f1`, `bd1fcf8a`, `6a58c2b1` y `507806bf`.
- **La matriz de F7 sigue siendo cierta:**
  - Las seis puertas de cierre (F1, F3, F4, F5, F7 y R-D11) son ancestras de `507806bf`.
  - D-01..D-15 no cambian.
  - Que el comando encuentre la tabla con el Autoloader es la decisión R-D11 B del Owner; el Freeze no regula la distribución.
  - INV-01..16 siguen protegidas: Core 14392 = 14390 − 1 + 3, y la prueba sustituida codificaba CD-I65-F4-10, no una INV.
  - §4, §5, §7 y §9 siguen conformes.
- **ADR-0050 (decisiones 1-9 y cláusula de compatibilidad):** ADR-0005, 0017, 0021 y 0040 no cambian.
- **ADR-0003:** el inventario tiene 4 DLL de RackCad, 0 de Autodesk, y además catálogos y tabla.
- **Desviaciones:** solo A65-FC-01.

**4. FOUNDATIONS: factual.**
- Los símbolos existen en `507806bf`.
- La línea del bundle es correcta: el `Content` está en `RackCad.Plugin.csproj:27`; el target incluye `BundleSectionMapFile` y su `Copy`; el verificador tiene la rama `-ceq` (`:174-177`) y `Assert-SectionMapMatchesSource` (`:285-301`, llamado en `:352`).
- RD11-OBS-01 es factual (sin atributo; `i/lf w/lf`; un checkout nuevo en Windows da CRLF).
- Falta mencionar «aceptada (A65-RD11-01 = A)». La cabecera «verificadas en `bd1fcf8a`» está desactualizada, y `Status` debe publicarse exactamente como `STABLE` (A65-FC-08).

**5. READY-01..04 y 08.**
- READY-03 no se cumple mientras A65-FC-01 siga abierto.
- No queda ninguna decisión del Owner pendiente sobre el contenido. Las confirmaciones de la frontera y de HB-04 van con la declaración (A65-FC-02).
- **El preflight de READY-04 es suficiente, salvo un punto:** no registra la comprobación del número de ADR que F7 §7 prometía. La verifiqué: `main` llega a 0049, I-64 usa 0047 e I-62 usa 0048, sin colisión (A65-FC-09).
- **Antes de declarar hay que repetir:**
  - un fetch fresco;
  - `origin/main` = merge-base;
  - la rama remota igual a la punta, con `git diff --name-only <SHA>..<punta>` ⊆ `docs/`;
  - I-64 sin integrar;
  - el ADR 0050 libre;
  - el SDK resuelto igual.

**6. Correcciones documentales: las cuatro son exactas y tienen el efecto declarado.**
- `despliegue.md`:56: la tabla es `Content`, así que está en la salida.
- `despliegue.md`:247-251: coincide con el verificador.
- Guía, línea 181: `install-bundle.ps1 -Build` pasa por `build-bundle.ps1`, que verifica (`install-bundle.ps1:18-20`).
- Guía, líneas 28-31.
- Los blobs del Freeze, ADR-0050 y la tabla no cambian.

Otras afirmaciones desactualizadas:
- el contrato: `status: f4-implementation` y §8 y §13 («Ninguno todavía»);
- la cabecera de FOUNDATIONS;
- procedimientos §1.2: la referencia «(§3.5)» debería ser §4.1;
- WR2-PR-04: «hoja 6dc0d8da…» es un blob git, no un SHA-256 (el SHA-256 es `b15b9f9e…`);
- O8-PR-02 («igual al blob») frente a §1.1 («fuente del checkout»): coinciden con el `w/lf` medido.

**7. Procedimientos W-R1 y W-R2: fieles en lo esencial.**
- **Reglas del Owner:** R-D2 (solo volcados manuales, con condiciones de identidad), R-D3 (WR2-13), R-D4 (WR2-PR-01), R-D5 (a) (método único en PRE y POST), R-D5 (b) (WR2-09 después del cierre), R-D5 (c) y R-D6 (`wr_hostsnap` con `Get-Acl .Sddl`) están recogidas. Ninguna lectura de vuelta se reutiliza.
- **Condiciones de HB del Owner:** las nueve están cubiertas: HB-01 cubre la 1 y la 2, HB-02 la 3, HB-03 la 4, HB-04 la 5, HB-05a/b/c la 7, la 8 y la 9.
- **Oráculos añadidos:** «158» es un oráculo añadido y erróneo (A65-FC-01). Los demás añadidos (16/16, 117/117, SHA del DLL del bundle, raíz sin `.stage` ni `.backup`) refuerzan y tienen base.
- **Puntos a precisar:**
  - La «restauración exacta» de la condición 9 se aplica solo a la clase protegida. El Autoloader podría dejar una clave del perfil (sin verificar), y abrir o guardar DWG deja rutas en la lista de recientes, que no se exporta (A65-FC-03).
  - Dependencias de HB-04 (a) (A65-FC-04).
  - Identidades medidas por un mecanismo no fijado (A65-FC-05).
  - WR1-07 no repite «huella consola = manifiesto» de W1-07 (A65-FC-11).
  - WR2-AC-05 dice «paquetes iguales», pero S-07 cambia por diseño (A65-FC-13).
- **`w2_hostsnap.ps1` sin modificar con la carpeta de W-R1: regla correcta.**
  - Solo recibe `-Run` y escribe en `<Run>\w2-pre|post`, carpetas nuevas.
  - Sus carpetas fijas coinciden con las que crea W-R1, y S-08 es el volcado manual de OV-01.
  - Lo de OV-02 se mide aparte (WR2-PR-06).

**8. Huella de HB-04: verificada en el código.**
- **Qué entra en la huella:** `AnalysisInput` (`StructuralFingerprints.cs:101-121`) incluye:
  - el modelo canónico, con `rackId`, `kind`, `stationIndex` y `sourceFingerprint` (`StructuralModelCanonicalizer.cs:50-61`); a su vez, `sourceFingerprint` cubre los bytes del diseño, `rackId`, `kind` y la estación (`:34-58`);
  - `generatorVersion`, constante (`StructuralModelBuilder.cs:132`);
  - las huellas de cargas, filas y supuestos;
  - `adapterVersion`, constante `"1.0.0"` (`RackRamElementsCommands.cs:38`).
- **Consecuencia:** la huella no depende del binario (Debug o Release, `InformationalVersion`). Para el mismo rack da H igual; un rack nuevo tiene otro GUID y da otra H. El oráculo literal (rack nuevo con la misma huella) era imposible.
- **Clasificación:** es una errata o interpretación dentro del dominio del Coordinator (LIFECYCLE §10):
  - el oráculo lo redactó él;
  - las nueve condiciones del Owner quedan intactas;
  - nada se relaja: (a) evalúa la igualdad de huella donde tiene sentido, y (b) conserva el paso literal con 16/16 y 117/117.
- Como el Owner aprobó esos oráculos (§111, condición 6), recomiendo que reconozca la lectura de forma explícita al declarar. No es una condición previa de READY-03 (A65-FC-02).
- **La solución es correcta y suficiente.** Riesgos que quedan: pide `.dwl` ausente antes de `Copy-Item`, la vista elegida, el bloqueo de OV-08 antes de O8-07 y la ventana partida (A65-FC-04). O8-10 vuelve a guardar `ov08-ref.dwg`, pero la copia es la instantánea de O8-07; el SHA de `hb-ref.dwg` se comprueba.

**9. READY-05: cumple la fila `FINAL_CANDIDATE_SHA`.**
- Core 14392/14392 y UI 1637 superadas con 17 omitidas, con `dirty=0` y SDK 8.0.423, el de `global.json`.
- Builds Debug con 0 errores.
- CI 40811163677: `push`, rama exacta, `head_sha` exacto y 4/4 `success`, incluido `ui-tests`, según `ci-run.txt`.
- **Coherente con §1.1:**
  - DLL `31e2fd37…`; manifiestos `4a7770ef…` y `c0ad711f…` (coinciden con los blobs versionados);
  - tabla `fc3d9146…` = blob `d088e2a9`, con `i/lf w/lf`;
  - bundle de 25 archivos con 4 DLL;
  - verificador con 158 comprobaciones y harness con 14 casos.
- **Las siete cargas coinciden con sus blobs** (las recalculé: `25f48e7f`, `8e05faad`, `87e64230`, `d675d9f6`).
- Valores esperados y comprobaciones en seco calculados con la DLL de esa salida.
- Los 16 archivos de custodia de §115 y los 7 de §114 coinciden en SHA-256.

**10. Plan de cierre e integración: conforme con WORKFLOW §4.5 y §11.4-§11.6.**
- El merge por plomería es una variante con precedente que da la misma estructura de commits.
- Faltan en el paso 4: el índice de ADR, `ideas-futuras.md` (A65-RD11-07, R-D8, la guarda de `BuildStructuralModel`, A65-RD11-04 y la opción `-text`), el contrato desactualizado y la normalización de FOUNDATIONS (A65-FC-08).
- **Archivos en común con I-64:** exactamente los tres declarados (`ROADMAP.md`, `CustomPropertiesCommandGuardTests.cs` y `SelectiveEditorOpenTests.cs`). La rama de I-62 y `feature/rackmirror-espejo-semantico` comparten solo `ROADMAP.md`.

**11. Hallazgos residuales.**
- A65-RD11-01..14 están dispuestos (01 = A; 02, 06, 08 y 09 corregidos; 03 por sustitución; 07 para una unidad futura).
- A65-F7-01..11 están cerrados.
- A65-F5F-03/04/05 quedan dispuestos por R-D5 y A65-F5F-08 por R-D4; A65-F4F-04 por R-D3 y A65-F4F-08 por R-D6.
- No hay promoción indebida. Solo A65-FC-01 impide READY.
- Final-candidate §9 no lista R-D3 ni R-D4 entre las limitaciones (A65-FC-10).

**12. Privacidad: correcta.**
- Barrí las líneas añadidas de `d53367e5` a `ba4494b2`: ningún nombre, cuenta, ruta de usuario ni secreto.
- Las copias de herramientas usan `<usuario>`.
- `ci-run.txt` no incluye el actor.
- `verifiedBy` aparece solo como número de caracteres.

## Estado de READY-01..09 tal como lo veo

| ID | Estado | Por qué |
|---|---|---|
| 01 | CUMPLE | Alcance congelado completo; los diferimientos los decidió el Owner |
| 02 | CUMPLE en `507806bf` | Se reinicia con el SHA corregido |
| 03 | **NO CUMPLE** | A65-FC-01 REQUIRED abierto |
| 04 | CUMPLE (03:15Z) | Pendiente de repetir antes de declarar, con el número de ADR |
| 05 | CUMPLE | No consulté GitHub |
| 06 | **NO CUMPLE** | NON-CONFORMING por A65-FC-01; falta además la conformidad del Coordinator |
| 07 | CUMPLE | Árbol limpio; la punta solo añade `docs/` |
| 08 | CUMPLE (OV-01..08) | HB, el complemento, está afectado por A65-FC-01 |
| 09 | CUMPLE | Errata en A65-FC-07 |

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-FC-01 | procedimientos:180 (HB-02); `verify-bundle.ps1:315-330, 366-372` | Con `-BaselineInventoryPath` salen 160 comprobaciones, no 158 | **REQUIRED** (LIFECYCLE §5: oráculo no satisfacible; §111 cond. 6; READY-08; CD-I65-FC-11 impide corregirlo después de declarar) | Antes de declarar: «OK (160 = 158 + 2 de reproducibilidad) y "Reproducibilidad: … coinciden"», o quitar la cuenta. SHA nuevo |
| A65-FC-02 | final-candidate §10 frente a `state` l.22 | La lectura de HB-04 aparece como «puede revisar» en un sitio y como «confirmación» en el otro; la solicitud no tiene línea para ella | OPTIONAL | Añadir una línea de reconocimiento de CD-I65-FC-13 y armonizar |
| A65-FC-03 | §3.4, HB-05c frente a la cond. 9 | La «exacta» se aplica solo a la clase protegida. Posible residuo del Autoloader en `Loaded`/`Applications` (sin verificar); lista de recientes no medida | OPTIONAL | Hacer explícita la lectura y preacordar con el Owner qué hacer con un residuo |
| A65-FC-04 | WR1-12, HB-04 (a) | No se fijan la vista elegida, la ausencia de `.dwl` antes de copiar, qué pasa si OV-08 para antes de O8-07, ni la carpeta y el SHA de `hb-ref.dwg` en W-R1b | OPTIONAL | Precisar cada punto |
| A65-FC-05 | §1.1, §1.3, `wr_hostsnap` B-09 | La herramienta, el bundle y `expected-inputs-ov02.json` se identifican por un mecanismo sin fijar; `secrets_scan.py` no está en §1.3 | OPTIONAL | Fijar el mecanismo y compararlos en PRE y POST |
| A65-FC-06 | WR2-PR-04; §1.2 | `6dc0d8da` es un blob git, no un SHA-256 (riesgo de un falso «R-D2 no aplica»); la referencia «§3.5» está mal | OPTIONAL | Corregir la etiqueta y la referencia |
| A65-FC-07 | final-candidate:60 | «solo la línea Frozen» es inexacto: son tres líneas, todas permitidas | OPTIONAL | Errata |
| A65-FC-08 | final-candidate §8 paso 4; `foundations-entry` | Faltan el índice de ADR, `ideas-futuras.md`, el contrato desactualizado, el `Status` exacto y la base de verificación | OPTIONAL | Añadirlo al cierre |
| A65-FC-09 | READY-04 (§114) | No se registra la comprobación del número de ADR | OPTIONAL | Incluirla en el preflight repetido |
| A65-FC-10 | final-candidate §9 | Faltan R-D3 y R-D4 entre las limitaciones | OPTIONAL | Añadirlas |
| A65-FC-11 | WR1-06/07 | No se dice el origen de H0, H1, H2 ni de la huella de HB, ni se exige consola = manifiesto | OPTIONAL | Precisar |
| A65-FC-12 | O8-PR-02 frente a §1.1 | «Igual al blob» frente a «fuente del checkout» | OPTIONAL | Anotar que el `w/lf` medido cumple ambas |
| A65-FC-13 | WR2-AC-05 | «Paquetes iguales», cuando S-07 cambia por diseño | OPTIONAL | «Los tres archivos originales de cada paquete» |

## No comprobado (habría que ejecutar o consultar fuentes externas)

- La corrida de CI 40811163677 en GitHub: tomé `ci-run.txt`.
- La regresión local, los builds, el bundle, el harness y las comprobaciones en seco: no los reejecuté.
- Los SHA-256 en disco de las salidas, las herramientas y los volcados de R-D2: no leí `D:/r65-smoke`.
- La cuenta de 160 de A65-FC-01: es deducida del código y del precedente de I-18, no ejecutada.
- Cómo se comportan en el registro el Autoloader y la lista de recientes, si `TRUSTEDPATHS` cubre `ApplicationPlugins`, y HB-01..05 dentro de AutoCAD.
- Que todas las vistas de un rack lleven el mismo `Design`.
- K1/K2 y K5 de esta revisión: los mide el Coordinator.

No declaro READY, ni el Candidato final, ni la integración.
