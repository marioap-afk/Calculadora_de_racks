# I-65 — Conformidad del Coordinator sobre el SHA propuesto del Candidato de hito de F5 (`bd1fcf8a`)

Coordinator de I-65, 2026-10-19 (02:30Z aprox.). Orden: decisiones §87 (F5 CANDIDATE PREPARATION = GO, puntos 1-10). Este documento es
**evidencia del SHA exacto** `bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109`; no es una disposición del Owner. **El Candidato NO se declara**:
la declaración es un acto separado del Owner («Detente antes de declarar formalmente el Candidato si corresponde una autorización
separada del Owner»).

## 1. Veredicto

**CONFORMING** para `bd1fcf8a` como SHA propuesto del Candidato de hito de F5 (alcance OV-I65-02; no es `FINAL_CANDIDATE_SHA`).
Coincide con el Architect: **CONFORMANT WITH OBSERVATIONS, 0 REQUIRED, A65-F5C-01..09 OPTIONAL**
([informe](architect-f5c-conformance.md)). Ninguna observación revela un invariante violado ni un riesgo material no cubierto (LIFECYCLE
§9): todas se tratan como seguimiento o como anexos de las autorizaciones posteriores.

## 2. Comprobaciones propias del Coordinator

| # | Comprobación | Resultado |
|---|---|---|
| 1 | Identidad del producto: `git diff --name-only 51fc82f1 bd1fcf8a` fuera de `docs/` | 0 archivos; `src`, `tests`, `tools`, `Directory.Build.*` y `global.json` con los mismos árboles y blobs |
| 2 | Freeze `4e6e1feb…` y ADR-0050 `c18def42…` | intactos en `bd1fcf8a` y en HEAD; sin A-n |
| 3 | Frontera documental (CD-I65-F5-09) | `bd1fcf8a` contiene las disposiciones §84-§87, la preparación, los procedimientos W3/W4 y la matriz AC-F5; lo posterior (`daa6fb77` y este commit) es solo evidencia del SHA |
| 4 | Evidencia local del SHA (§92) | producida después del commit, árbol limpio antes y después; Core 14390/14390; UI 1637 superadas y 17 omitidas; builds Debug correctos |
| 5 | CI del SHA exacto | run `39187567306`, `push`, rama de I-65, `head_sha` exacto, 4/4 con `UI Tests (WPF controls, net8.0-windows)` |
| 6 | DLL y entradas | `RackCad.Plugin.dll` `81299b87…fef5` (1.0.0+bd1fcf8a); tabla `fc3d9146…` = blob `d088e2a9…`; cargas `d675d9f6…`; valores esperados `c61a6e12…` (copia redactada revisable `expected-inputs.redacted.json`) |
| 7 | Comprobación en seco | 6/6 IGUAL, paquete `Valid` en memoria, `PAYLOAD` idéntico a `5a5ca577`; DLL cargadas registradas en `f5-dryrun-candidate-identity.txt` |
| 8 | Comprobación previa a la declaración (02:22:50Z, `predeclaration-recheck.txt`) | **fetch fresco**: `origin/main` = `bb0d5522`, 0 detrás; HEAD = `origin` = `daa6fb77`; 0 archivos fuera de `docs/` desde el Candidato; árbol limpio; DLL, tabla, cargas, valores esperados y biblioteca de bloques (`b4ca2248…60e8`) iguales; `acad.exe` = 0, `RAMElements.exe` = 0 |
| 9 | Controles de la revisión | K1/K2 sin cambios (6 worktrees, 2151 rastreados del checkout principal); K5: 0 intérpretes, 0 AutoCAD/RAM, 0 escrituras. Desviaciones declaradas por el Architect (metadatos de `FETCH_HEAD`, `git status`, otras órdenes git de lectura): **NON-MATERIAL** |
| 10 | Privacidad | sin `.trx`, registros completos ni JSON crudo de la API; barrido de los archivos nuevos: 0 secretos, 0 datos personales; repositorio **público** (PRIVACY HOLD vigente) |

## 3. READY-01..09 en su lectura de hito (después de esta conformidad)

| ID | Estado | Evidencia |
|---|---|---|
| READY-01 | CUMPLE | alcance OV-I65-02 aprobado (§86-§87); Freeze sin cambios |
| READY-02 | CUMPLE | F1-F4 cerradas; decisiones y documentos de F5 versionados en el SHA |
| READY-03 | CUMPLE | 0 REQUIRED; las decisiones abiertas (C-1, C-2, C-5, declaración, W3/W4) son actos posteriores, no contenido del Candidato |
| READY-04 | CUMPLE | fetch fresco a las 02:22:50Z, 0 detrás (A65-F5C-02 atendida); se repite al declarar |
| READY-05 | CUMPLE | CI `39187567306` 4/4 del SHA exacto |
| READY-06 | CUMPLE | Architect CONFORMANT WITH OBSERVATIONS + Coordinator CONFORMING sobre `bd1fcf8a` |
| READY-07 | CUMPLE | árbol limpio; producto sin cambios pendientes |
| READY-08 | CUMPLE | OV-I65-02 asignada a W3/W4 con oráculos; OV-I65-01..07 siguen con su resultado de F4 y se repiten sobre el Candidato final |
| READY-09 | CUMPLE | Freeze, ADR-0050 y secuencia append-only comprobados |

## 4. Propuesta de disposición de A65-F5C-01..09 (decide el Owner, idealmente en el mismo acto de declaración)

| ID | Propuesta del Coordinator | Estado tras este commit |
|---|---|---|
| A65-F5C-01 | Criterio operativo para CD-I65-F5-09: **SHA nuevo solo si cambian** producto, Freeze, archivo de cargas, tabla, tolerancias, oráculos o la matriz AC-F5/OV. Son **actos posteriores** que no exigen SHA nuevo: la declaración, la disposición de estos OPTIONAL, las autorizaciones de W3/W4 con sus anexos operativos (§5) y C-1/C-2/C-5 | requiere decisión del Owner |
| A65-F5C-02 | Comprobación previa con fetch fresco | HECHA (02:22:50Z); se repite al declarar |
| A65-F5C-03 | Anexo de W3: identidades de herramientas (§5.1) y medición de la ACL con el mismo camino y página de códigos fija (A65-F4F-08) | anexo propuesto |
| A65-F5C-04 | Anexo de W4: dirección `GLOBAL_Y` con valor **negativo**, sin doble signo (como `I-65-f4-ref-cant-01-manual-procedure.md` §8, punto 3) | anexo propuesto |
| A65-F5C-05 | Anexo de W4: ventana ≥ 2.5 h con parada por tiempo, integridad del paquete antes y después, árboles de las bases y ruta fija del registro de custodia | anexo propuesto |
| A65-F5C-06 | Anexo de W3/W4: recortes por región, segunda revisión de cada recorte por el Architect antes de versionar, el operador no abre las rejillas del modelo generado | anexo propuesto |
| A65-F5C-07 | (a) DLL de la comprobación en seco registradas; (b) aclarado qué se escribió; (c) herramientas versionadas en `candidate-bd1fcf8a/tools/` (la lista local de nombres del escáner no se versiona) y barrido ampliado a §92, §93 y al estado; (d) copia redactada revisable de los valores esperados; (e) desviación de la guía §2 registrada en el bloque §7.1; (f) sin más detalle de la sesión del usuario (el texto ya publicado no se reescribe: C-3) | ATENDIDA en este commit, salvo (f), que queda como está |
| A65-F5C-08 | No cambiar la matriz del Candidato (cambiarla exigiría SHA nuevo): evaluar en el cierre la aceptación del Owner de W3/W4 dentro de AC-F5-04/05 y la integridad del host (BASELINE/POST sin PROHIBIDO, K1/K2) dentro de AC-F5-08; AC-F5-03 se lee con la desviación de la guía §2 registrada | requiere conformidad del Owner |
| A65-F5C-09 | Errata editorial de la preparación §4: con 6 decimales el error de posición es ≤ 2.1e-7 in (no 2e-7); sin efecto (tolerancia 0.02 in). `next_role` del estado corregido | errata registrada; estado corregido |

## 5. Anexos operativos propuestos para las autorizaciones de W3/W4 (no cambian el Candidato)

### 5.1 Identidades de herramientas (A65-F5C-03)

| Herramienta | SHA-256 del archivo ejecutado |
|---|---|
| Validador v2 `validate_smoke2_package_v2.py` | `1f56b8cba1a64f908c46eb5691930891c3dc0ef1d9bf8ed9c45f8e5f8bdd297a` (custodiado en F4) |
| BASELINE/POST de AutoCAD `w1_hostsnap.ps1` | `5c33a842fa1a39d4dd9ce9ec1dd4b9143b3271fceeca0d59d570f3b551b35ba4` (custodiado en F4); ejecutar BASELINE y POST por el mismo camino y con la misma página de códigos (A65-F4F-08) |
| PRE/POST de RAM `w2_hostsnap.ps1` | `6904a43965de69b33b78c304cd57d1958b31b2018121906cf22d034724ff5485` (custodiado en F4) |
| Contraste de la carga útil (W3-07) | `f5_dryrun_compare.py` `39ac443a5a20368afbab217309c9e8afa27af77e7e7dcf97cdfd0aa950f85265` sobre la línea `PAYLOAD` extraída del script de `pkg-OV02` (copia en `candidate-bd1fcf8a/tools/`) |
| Custodia de sesión y contraste de W4 | `session_custody.ps1` `5487544aec2685c8554145b46d68bd4fe8fe9545e0b1628deee84d7fc080491b`; `readback_dump_contrast.py` `0790930297d221ddefeadaef8e04611858b2e81446717155e754d34155114bb1` (preparación de F5) |
| AutoCAD | versión y `acad.exe` medidos en la BASELINE de W3 (bloque de la guía §7) |

### 5.2 Reglas adicionales propuestas

- **W4, signo (A65-F5C-04):** cada carga se teclea en dirección `GLOBAL_Y` con el valor **negativo** de la hoja (por ejemplo,
  −0.449617886 kip); no se invierte además la dirección.
- **W4, ventana (A65-F5C-05):** pedir a I-64 una ventana ≥ 2.5 h (o dividir: generado + volcado en una sesión; modelo manual en otra);
  STOP si se superan 2 h 15 min sin terminar el modelo manual; SHA-256 de los tres archivos de `pkg-OV02` antes y después (solo se añade
  `ram-readback.json`); árboles de `W.sec`, `HSS_SQR.sec` y `Steel.mat` antes y después; registro de custodia en
  `D:\r65-smoke\bd1fcf8a\f5-<RunId>\custody.jsonl`.
- **Capturas (A65-F5C-06):** solo recortes por región producidos por script (nunca la pantalla completa); cada recorte lo revisa el
  Architect antes de versionar; el operador no abre las rejillas del modelo generado antes de terminar el modelo manual.

## 6. Bloque de declaración propuesto (guía §7.1) — NO DECLARADO

```text
Candidate SHA:        bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109   (Candidato de HITO de F5, OV-I65-02; PROPUESTO, pendiente del acto del Owner; no es FINAL_CANDIDATE_SHA)
Arbol limpio:         SI            <- al producir la evidencia local (§92; HEAD antes y despues = el SHA, status vacio)
SDK resuelto:         8.0.423
Core Full local:      PASS          (14390/14390)
UI Full local:        PASS          (1637 superadas, 0 con error, 17 omitidas declaradas)
Debug UI build:       PASS
Debug Plugin build:   PASS          (DLL 81299b87eb14f41b61721e79c266d76a15345cdcaac23b193496d2d57f31fef5, 1.0.0+bd1fcf8a; -o D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows, desviacion registrada de la guia §2, como en F4)
CI exact SHA:         GREEN (run 39187567306, event=push, ref=refs/heads/architecture/interop-analisis-estructural, head_sha exacto, job ui-tests success; 4/4)
Owner validation:     required (OV-I65-02 en W3 AutoCAD + W4 RAM; no autorizadas)
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg  SHA-256 b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8   <- medida a las 02:22:50Z; se vuelve a medir en W3
```

Custodia adicional: tabla `fc3d9146…` (blob `d088e2a9…`); herramienta `D:\r65-smoke\bd1fcf8a\ramtool\` (`RackCad.StructuralAnalysis.RamTool.exe`
`21290785…`); cargas `d675d9f6…`; valores esperados `c61a6e12…`; Architect CONFORMANT WITH OBSERVATIONS (A65-F5C-01..09 OPTIONAL);
Coordinator CONFORMING. La cobertura no es requisito previo (WORKFLOW §2.bis/§7).

## 7. Condiciones de W3/W4 (sin cambios; procedimientos §3-§4)

Ejecución condicionada, en este orden: (1) repositorio **privado** comprobado por lectura (C-1); (2) declaración del Candidato por el
Owner; (3) autorización específica de W3 y, tras aceptar W3, de W4; (4) confirmación expresa del Coordinator de I-64 en su chat y aviso a
I-52; (5) nadie usando el equipo, `acad.exe` = 0 y `RAMElements.exe` = 0. W3/W4 EXECUTION = NO; F5 GATE PASS = NO; READY FINAL = NO.
