## Relectura del Architect (READY-06, ronda 2) sobre `4a03b65b375d468c220dd68400d5dfd955bf043e`

**pwd:** `/d/Documentos/Codex/Calculadora de racks`. Es el checkout principal del Owner; no lo leí ni lo usé. En adelante, `<worktree>` es el worktree de I-65. Al empezar y al terminar, HEAD y `origin/architecture/interop-analisis-estructural` eran `813d0cee` y el árbol estaba limpio (0 líneas en `status --porcelain`).

### Incidente de control: una escritura accidental

Para comparar árboles ejecuté `git ls-tree <sha> | grep -v docs | git mktree --missing`. `mktree` **escribe** un objeto árbol suelto, y pertenece a la misma clase que `merge-tree --write-tree`, que estaba prohibido.

- **Qué se escribió:** con toda probabilidad, el objeto `c81dde4a2d572284ef6a352afb44de8f503895fe`, en el almacén de objetos común (`D:/Documentos/Codex/Calculadora de racks/.git`, el common-dir del worktree).
- **Qué no se tocó:** refs, índice, archivos versionados ni el worktree.
- **Por qué sigue ahí:** no lo borré, porque borrarlo sería otra escritura. Es inalcanzable y `gc` lo podará; no altera ningún SHA ni ninguna evidencia.
- **Qué hice después:** cambié de método (SHA-256 del listado `ls-tree`).
- **Otra lectura fuera del worktree:** leí la salida guardada por el harness de mi propio `git diff 507806bf 4a03b65b -- docs/initiatives docs/guias README.md`, en la carpeta de la sesión. No es el scratchpad del Coordinator. K5 debe registrar estas dos cosas; la clasificación le toca al Coordinator.

No ejecuté python ni pwsh, no compilé, no abrí AutoCAD ni RAM, no hice fetch y no leí `D:/r65-smoke` ni `D:/r65-m05c`.

## Veredicto: **CONFORMING** sobre `4a03b65b`

- **Desviaciones REQUIRED:** ninguna.
- **Desviaciones frente al Freeze, el ADR o las decisiones:** ninguna.
- **Observaciones:** seis hallazgos nuevos, todos OPTIONAL (A65-FC-14..19).
- **Base del veredicto:** la revisión completa de la ronda 1 más este delta.
  - `4a03b65b` desciende linealmente de `507806bf` (`507806bf`→`ba4494b2`→`4a03b65b`).
  - Merge-base con `origin/main`: `bb0d5522`, sin cambios.
  - No hubo rebase, así que el último párrafo de LIFECYCLE §9 no se activa.
- **Lo que sigue valiendo de la ronda 1:** sus conclusiones sobre producto, Freeze, ADR-0050/0003, INV-01..16, matriz §4, OV §5, símbolos de FOUNDATIONS, el análisis de la huella de HB-04 y el plan de cierre. Ese delta no las afecta: todo lo que juzgan son blobs y árboles idénticos.
- **Lo que deja de valer:** solo READY-03 y READY-06 (NO CUMPLE) y las cifras ligadas a `507806bf` (DLL `31e2fd37`, CI 40811163677), que no se trasladan.

## Respuestas

**1. Identidad: CUMPLE.**
- **Delta de `507806bf` a `4a03b65b`:** 26 archivos, todos en `docs/`.
- **Delta de `4a03b65b` a `813d0cee`:** 16 archivos, todos en `docs/automation/`.
- **Nada fuera de `docs/` cambia desde `6a58c2b1`:** `diff --name-only 6a58c2b1 {4a03b65b,813d0cee} -- . ':(exclude)docs'` sale vacío.
  - El SHA-256 del listado de la raíz sin `docs` es `4823d8d5…` en los cinco SHA (`6a58c2b1`, `507806bf`, `ba4494b2`, `4a03b65b` y `813d0cee`).
  - Los subárboles no cambian: `src` `b6d89cd1`, `tests` `6937b65c`, `tools` `29485f42`, `deploy` `09159b1d`, `.github` `80ccff3a`. Son los mismos de la ronda 1.
- **Blobs protegidos intactos** en los cinco SHA:
  - Freeze `I-65-proposal-v3.1.md` = `4e6e1feb`;
  - ADR-0050 = `c18def42`;
  - tabla `assets/…/section-material-map.json` = `d088e2a9`.
  - `log a9d7080b..813d0cee` no toca ni el Freeze ni el ADR. La tabla cambió por última vez en `51fc82f1`.
- **Historia sin reescribir:**
  - El padre de `4a03b65b` es `ba4494b23fa5…`.
  - El reflog de `origin/architecture/…` muestra avances rápidos de un commit cada uno: `d53367e5` → `507806bf` → `ba4494b2` → `4a03b65b` → `813d0cee`.
  - Es decir, `4a03b65b` se publicó solo, como pide WORKFLOW §11.5.

**2. A65-FC-01: CERRADO.**
- **Qué dice ahora el oráculo** (procedimientos:193): «`OK: bundle verificado (160 comprobaciones)`: 158 del bundle más 2 de … `Compare-Inventory`» y además la línea «Reproducibilidad: el inventario y los hashes coinciden con el base.».
- **El código lo respalda:**
  - `verify-bundle.ps1:92-102`: `assertionCount++` en cada `Assert-True`;
  - `:320` y `:328`: las dos aserciones de `Compare-Inventory`;
  - `:366-368`: la llamada y la línea de reproducibilidad, que coincide con el oráculo carácter por carácter;
  - `:372`: el mensaje final. El oráculo cita su prefijo exacto.
- **Las mediciones en seco lo respaldan:**
  - `hb02-dryrun-verify-507806bf.txt`: 160, reproducibilidad conforme;
  - `candidate-4a03b65b/hb02-dryrun-verify.txt`: 160, reproducibilidad conforme;
  - sin el inventario base: 158 (`bundle-evidence.txt`).
  - Las 25 líneas de inventario de la medición en seco son iguales a `candidate-4a03b65b/bundle-inventory.txt`. Este archivo, en CRLF, da `33e660b2…` = `inventory_sha256` de `identity.txt`.
- **El paso (3) también da 160:** la cuenta depende del contenido y `install-bundle.ps1` no añade archivos (sin instalación previa no hay biblioteca que conservar; `:178-200`).
- **No toca un oráculo aprobado:** el oráculo aprobado de HB-02 (contrato R-D11 §6, línea 117) no fija ninguna cuenta. El «160» es un oráculo añadido por el Coordinator, y CD-I65-FC-20 lo clasifica correctamente.

**3. Aclaraciones A65-FC-02..13: correctas.**
- Ninguna cambia un oráculo aprobado, las nueve condiciones de §111, R-D2..R-D6, la matriz OV ni el Freeze. Comprobaciones concretas:
  - **02:** la solicitud de §10 lleva ahora la línea de HB-04, y `state:22` dice «confirmación». Están armonizados.
  - **03:** la lectura «exacta = clase protegida» es la misma que el oráculo aprobado de HB-05 («DIFF sin PROHIBIDO», R-D11 §6:120). Que un residuo del Autoloader sea STOP endurece, no relaja. B-04 ya protegía `Applications`, `InstalledApplications` y `Loaded`.
  - **04:** la comprobación de `.dwl`/`.dwl2`, el SHA-256 de `hb-ref.dwg` también en W-R1b y NOT_EVALUATED si se detiene antes de O8-07 son correctos.
  - **05:** B-09 de `wr-hostsnap.ps1.txt:35-39` cubre exactamente el DLL, el manifiesto, la tabla, las cargas y `expected-inputs.json`, como se afirma. La copia de `secrets_scan.py` existe.
  - **06:** el blob `6dc0d8da` (`ov-i65-02-manual-entry-sheet.json`) tiene SHA-256 de contenido `b15b9f9e…`, y §4.1 existe. El cambio de §1.2 de «antes de comparar» a «antes de abrir RAM» se alinea con WR2-PR-04.
  - **07:** `a9d7080b` cambia exactamente tres líneas: Estado, Frozen y Coordinator/Architect.
  - **08:** `docs/adr/README.md` y `docs/ideas-futuras.md` existen.
  - **09:** lo comprobé con refs locales. `main` tiene 0045, 0046 y 0049; I-64 añade 0047, I-62 añade 0048 e I-65 añade 0050.
  - **10:** coincide con CD-I65-RG-01.
  - **11:** `analysisInputFingerprint` está en el manifiesto (`RamBuildManifest.cs:105`) y en el script (`RamBuildPayload.cs:340`), así que consola = manifiesto = script es satisfacible.
  - **12:** `identity.txt` mide `i/lf w/lf`.
  - **13:** coincide con las cuentas S-07 históricas (inyección 3→4).
- Errores nuevos: ninguno de clase REQUIRED. Hay imprecisiones menores (A65-FC-16, 17 y 19).

**4. Delta documental: sin cambios no declarados.**
- `README.md` y `docs/guias` no cambian.
- Los tres archivos de `docs/initiatives` cambian solo en lo que declaran el cuerpo del commit, §114 (CD-I65-FC-20..23) y §116.
- La custodia de §116 (7 blobs) y la de §117 (13 blobs) coinciden en SHA-256 (las recalculé).
- **FOUNDATIONS sigue siendo factual:**
  - de `bd1fcf8a` a `4a03b65b` no cambia ningún `.cs` de `src/`; solo `RackCad.Plugin.csproj`, `deploy/`, una prueba y `README`;
  - «aceptada por el Owner con A65-RD11-01 = A» corresponde a CD-I65-FC-02.
- **Privacidad:** las líneas añadidas en `507806bf..813d0cee` no llevan nombres, cuentas ni secretos. Las 8 rutas aparecen como `<usuario>`.

**5. READY-05 sobre `4a03b65b`: CUMPLE** (fila `FINAL_CANDIDATE_SHA`).
- **Ventana local:** `HEAD=4a03b65b`, `dirty=0` antes y después, de 03:54:41Z a 04:05:37Z. Empieza después del commit (03:54:28Z). SDK 8.0.423.
- **Suites y builds:**
  - Core 14392/14392;
  - UI 1637 superadas y 17 omitidas;
  - builds Debug de UI, Plugin y herramienta con 0 errores.
- **CI 40814761082** (según `ci-run.txt`): `push`, rama exacta, `head_sha` exacto, 4/4 `success`, incluido «UI Tests (WPF controls, net8.0-windows)».
- **Coherente con §1.1:**
  - DLL `0f5a508e…` con `1.0.0+4a03b65b…`;
  - manifiestos `9939f6da`/`4fbe3ea5`, iguales a los blobs versionados;
  - tabla `fc3d9146…` = blob `d088e2a9`;
  - bundle de 25 archivos, DLL `60e1dd44`, verificador 158, harness 14;
  - las siete cargas recalculadas desde los blobs de `4a03b65b`: `25f48e7f`, `8e05faad`, `87e64230` y `d675d9f6`;
  - `expected-inputs*.json` con `13e8678f`/`f1a7c036`, y comprobaciones en seco conformes (6/0; f5 con salida 0);
  - frente a `507806bf`, solo cambian los cuatro DLL (versión embebida).

**6. Conclusiones de la ronda 1.** Además de READY-03 y 06 (resueltos), queda superada la reserva «160 no ejecutado», porque ahora lo midió el Coordinator dos veces. El resto se mantiene.

## Estado de READY-01..09 sobre `4a03b65b`

| ID | Estado | Base |
|---|---|---|
| 01 | CUMPLE | Sin cambios |
| 02 | CUMPLE | Reiniciado; decisiones hasta §114 versionadas |
| 03 | CUMPLE | A65-FC-01 cerrado; 0 REQUIRED abiertos |
| 04 | CUMPLE (03:53:58Z, §116) | Las refs locales siguen en `bb0d5522` = merge-base. Hay que repetirlo antes de declarar |
| 05 | CUMPLE | No consulté GitHub |
| 06 | Parte del Architect: CONFORMING | Falta la conformidad del Coordinator |
| 07 | CUMPLE | Árbol limpio; la punta `813d0cee` solo añade `docs/` |
| 08 | CUMPLE | El oráculo de HB ahora es satisfacible |
| 09 | CUMPLE | Freeze, acuerdo, `amendment_refs: []` y `freeze_delta_ref: null`; lo registra el Coordinator |

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-FC-14 | `candidate-*/expected-inputs-ov02.redacted.json` `_redaction.originalPath`; `redact-expected-inputs.py.txt:38` | Dice `expected-inputs.json` aunque el SHA y los bytes (`f1a7c036`, 7210) son los de `-ov02`. Ya estaba en la ronda 1 | OPTIONAL | Tomar el valor de referencia de `identity.txt`; anotarlo |
| A65-FC-15 | Evidencia §117 / `candidate-4a03b65b` | No registra el SHA-256 de las herramientas (`fc_evidence.sh`, `fc_identity.ps1`, `bundle_evidence.ps1`) que se ejecutaron para `4a03b65b`. Las únicas copias están en `candidate-507806bf/tools`, y §1.1 (A65-FC-05) remite a «la copia de la evidencia del Candidato» | OPTIONAL | Declarar en la conformidad del Coordinator que son las del README de `candidate-507806bf/tools` |
| A65-FC-16 | HB-05c (procedimientos:198) | Dice que las listas de recientes «se registran», pero `wr_hostsnap` solo exporta Profiles, FixedProfile, Applications, InstalledApplications, Loaded y HKLM Applications (`wr-hostsnap.ps1.txt:23-24`). Además, «cualquier referencia en el registro» se limita en la práctica a esas exportaciones | OPTIONAL | Escribir «no se mide» o «en las exportaciones B-04» |
| A65-FC-17 | HB-04 (a) / WR1-12 | «La misma vista que en O8-06 y O8-09 (la vista principal)» supone algo que WR1-12 no fija ni anota. El caso con `.dwl` presente no se enlaza de forma explícita con NOT_EVALUATED | OPTIONAL | Anotar la vista elegida en O8-06/09 y decir que con `.dwl` la parte (a) queda NOT_EVALUATED |
| A65-FC-18 | final-candidate:5, :62; evidencia §116 | La cabecera sigue diciendo «§111-§112». READY-04 cita el preflight de 03:00:47Z y no el repetido. §116 dice «rackmirror ninguno nuevo», pero esa rama añade ADR-0036 (no está en `main`; no choca con 0050) | OPTIONAL (EDITORIAL) | Corregir en el cierre documental |
| A65-FC-19 | WR2-AC-05 | No menciona el archivo original de `dump-OV01-gen`, que S-07 sí mide (1→2) | OPTIONAL | Añadir «y el script de `dump-OV01-gen`» |

## Estado de A65-FC-01..13

Los 13 están **CERRADOS**. Quedan residuos OPTIONAL: el 03 en A65-FC-16, el 04 en A65-FC-17, el 05 en A65-FC-14/15, el 09 en A65-FC-18 y el 13 en A65-FC-19.

## No comprobado sin ejecutar

- La CI 40814761082 en GitHub (tomé `ci-run.txt`).
- Las suites, los builds, el bundle y las mediciones en seco: no los reejecuté. Tampoco medí en disco los SHA-256 de `D:/r65-smoke`.
- El 160 sobre la copia instalada en el paso (3).
- Si el Autoloader deja algo en el registro y dónde guarda AutoCAD 2025 los recientes en este equipo.
- Si la huella depende de la vista elegida.
- Un fetch fresco y K1/K2/K5.

No declaro READY, ni el Candidato final, ni la integración.
