# I-65 F4 — Revisión de conformidad del Architect sobre el SHA propuesto `5a5ca577` (custodia literal, 2026-10-17)

- **Autoridad:** decisiones §75 (CD-I65-F4-176, ARCHITECT CONFORMANCE REVIEW = AUTHORIZED).
- **Revisor:** subagente `claude-opus-5-5`, SAME-SESSION ROLE, solo lectura ([prompt](architect-f4c.prompt.md)).
- **Controles:**
  - K1/K2 sin STOP (solo una señal de la sesión de I-62 en su propio worktree);
  - K5 ([barrido](architect-f4c-k5-transcript-scan.json)): 0 intérpretes, 0 RAM/AutoCAD, 0 escrituras y 0 menciones del checkout
    principal; la única lectura fuera del worktree es la salida desbordada de su **propio** comando que el entorno guardó en
    `tool-results/` (no es un archivo del proyecto).
- El informe se transcribe tal como lo entregó el revisor, salvo las entidades HTML del `git -C` de la sección de método, que se
  restauran.

---

**Veredicto sobre `5a5ca57731ebbbbc41ed4f702698709d5e128a86`: CONFORMANT WITH OBSERVATIONS.** No hay ningún hallazgo REQUIRED; registro 8 observaciones OPTIONAL nuevas (A65-F4C-01..08).
- Cierro mis observaciones A65-F4R-06 y A65-F4R-08.
- Confirmo que el NO CHANGE de A65-F1-06 no altera el contrato congelado.
- Para READY-06, esto equivale a conformidad del Architect; las observaciones quedan como seguimiento (LIFECYCLE §9). Falta la conformidad del Coordinator sobre el mismo SHA.

**pwd informado:** `/d/Documentos/Codex/Calculadora de racks` (checkout principal). No lo leí ni lo usé.

**Rol:** SAME-SESSION ROLE, subagente con contexto independiente. El autor es el Coordinator de la misma sesión: rol distinto, mismo modelo base.

**Cómo trabajé:**
- Solo leí con `git -C <worktree>`: `rev-parse`, `log`, `show`, `diff`, `grep`, `ls-tree`, `cat-file`, `merge-base`, `for-each-ref`.
- Calculé `sha256sum` sobre blobs del repositorio.
- Hice una consulta de solo lectura a GitHub (`gh run view 38977069487`).
- No escribí archivos, no compilé, no ejecuté pruebas ni intérpretes y no hice fetch. Tampoco leí D:/r65-smoke ni D:/r65-m05c.
- Transparencia: mi primer `git status` corrió sin `GIT_OPTIONAL_LOCKS=0`. Pudo refrescar la caché de stat del índice del worktree, sin cambiar contenido. Todas las llamadas siguientes usaron `GIT_OPTIONAL_LOCKS=0`.

## 1. Identidad

- **Solo documentación.**
  - `git diff 51fc82f1 5a5ca577 -- src tests tools assets '*.props' '*.targets' global.json RackCad.sln` está vacío.
  - Desde 51fc82f1 hasta HEAD (9a6f69d6) no cambia ningún archivo fuera de `docs/`. Son 156 archivos hasta 5a5ca577, más 7 en los hijos.
- **Árboles idénticos** en 51fc82f1, 5a5ca577 y 9a6f69d6: `src` 6cadd526, `tests` 47fbcaf5, `tools` 29485f42, `assets` d1cc1811. También coinciden `Directory.Build.targets` 58d1c335, `global.json` eaa68c65 y `RackCad.sln` 6483b50d.
  - `src` y `tools` son además los mismos de 93606ea0 (M-05c), ad595b42 (herramienta de P2) y 745653e1 (primer smoke).
- **Freeze y ADR intactos.**
  - El Freeze (4e6e1feb…) y ADR-0050 (c18def42…) tienen el mismo blob en a9d7080b, 51fc82f1, 5a5ca577 y HEAD, y ningún commit los toca desde a9d7080b.
  - El commit de Freeze solo cambia 3 líneas de cabecera respecto del blob acordado 11062a91 (3872fcc0).
  - ADR-0050 está «aceptado» desde ed8154c3.
- **Sin A-n ni freeze-delta.** No hay ningún archivo de ese tipo. El único cambio en un contrato es un aviso de estado de 4 líneas en `I-65-f4-d-mapping-verified-contract.md`, que dice que el texto de 9f6fff6e no cambia.

## 2. Conformidad acumulada con Proposal V3.1 y ADR-0050

- **Fronteras.** Comparado con main (bb0d5522), el producto cambia en:
  - `Application/StructuralAnalysis` (33 archivos nuevos), `Systems/Cantilever` (2), `Units/Force` y `Units/Mass`;
  - `StructuralSectionUnits`, que pasa a usar el propietario único de la libra (D-03 / A65-V2-07);
  - un archivo nuevo de comando en el Plugin, más el `Content` de la tabla en el `.csproj`;
  - la herramienta RamTool, las pruebas y dos censos.

  Domain, UI y Workspace no cambian: no hay UI nueva, Domain queda intacto e I-62 e ID30/I-64 no se tocan. Fuera de `Providers/RamElements`, los identificadores de RAM solo aparecen en `RackRamElementsCommands.cs`, el archivo único de la lista de permiso de G1 (D-13). El modelo neutral no tiene tipos de RAM.
- **Alcance.**
  - Solo Cantilever de una cara: códigos `STATION-FACE/SIDE-UNSUPPORTED` y `ARM-SLOPE-UNSUPPORTED` (`StructuralDiagnostic.cs:107-110`), y el comando rechaza cualquier otro tipo de rack.
  - RAM 26.00.01.015 queda «declared, not verified» (`RamBuildManifest.cs:34`).
  - Sin análisis, resultados ni diseño: la plantilla no tiene `analyze`, `save` ni `os.`; no hay `Analyze`/`ReadResults` en `src`; los tokens de INV-11 no aparecen en `src` ni en `tools`.
- **Tabla verified.** Blob d088e2a9, idéntico a `f4d-table-target.json`. Los bytes del blob dan SHA-256 fc3d9146…, 3279 bytes, sin CR.
- **Guardas.** G1-G4, INV-10/11/14/16 y los censos no cambian desde 51fc82f1, que ya revisé en F4-D. Están en verde en Core Full local (14390/14390) y en el CI de 5a5ca577. Las tolerancias (0.02 in, 0.1°, 0.1 %) siguen intactas.
- **REQUIRED:** ninguno.

## 3. Disposiciones del Owner (§75)

| Ítem | Dictamen |
|---|---|
| A65-F4R-07 | **Coherente.** En `RackRamElementsCommands.cs:116-118`, si la tabla devuelve `Valid` con `Table == null`, el comando imprime «…: Valid. La tabla … no es utilizable.» y sale sin escribir nada (falla cerrado). Solo pasa si el lector incumple su contrato. |
| A65-F4R-02 | **Coherente.** El volcado se hizo en la misma sesión en P2 (generado y manual) y la preparación lo mantiene; ninguna tolerancia cambia. |
| A65-F4D-02/03/04/05/09 | **Coherente.** Son observaciones de pruebas y de proceso; ninguna toca un INV ni un elemento congelado. |
| A65-F1-06 NO CHANGE | **CONFIRMO.** El Freeze §3 dice: «sin `member_design`, ratios ni veredictos en RackCad; una llamada o tipo de ratio → falla». La guarda (`StructuralAnalysisSourceGuardTests.cs:229-256, 413-414, 499-503`) busca 8 tokens sin límites de palabra en el código de todo `src/**/*.cs` (quita los comentarios y conserva las cadenas, así que la plantilla Python también se revisa). Buscar sin límites encuentra un superconjunto de coincidencias: solo puede dar falsos positivos, nunca menos detecciones. El Freeze no fija límites de palabra. Los tokens no cambian desde F1. No altera el contrato. |
| A65-F4R-06 | **CIERRO.** M-05c verificó que se leen las combinaciones (MP: C1 = A·1.2 + B·1.6, exacto) para RAM 7dcb4855, `ramelements` 0.1.0 y el volcado 1.0.0 (CD-I65-F4-67/71). Con CMB-3 y P2 sin `--*-unread` (COMBINATIONS PASS, 0 en ambos), comparar 0 con 0 es una comparación real. Queda un detalle residual: el informe no lista los casos sin cargas que ignora (`CM`), aunque los volcados sí. Conviene que el registro de W2 los cite. La validez depende de esas tres identidades (ver A65-F4C-05). |
| A65-F4R-08 | **CIERRO.** El comando ya se ejecutó en AutoCAD por los dos caminos: `Unsupported` en el primer smoke y `Valid` con publicación en el segundo (16/16 tras V-11), ambos con el mismo `src` 6cadd526. No sustituyen W1 (otra DLL). Lo que quedaba de «copia de la tabla para OV-03» pasa a A65-F4C-01. |
| DIF-04 | **Coherente** con ASM-PROP-01 y Q-O2 (b). El `source` ya dice «RAM catalog formal edition not verified». |

Ninguna disposición amplía el Freeze ni autoriza código.

## 4. Evidencia del SHA (§79)

- **Producida después del commit, con árbol limpio.**

  | Paso | Hora (UTC) |
  |---|---|
  | Commit 5a5ca577 | 22:39:33Z |
  | Arranque de la corrida de CI | 22:39:44Z |
  | Core (308 s) y UI (598 s), con `HEAD=5a5ca577 dirty=0` antes y después | entre ambos |
  | Build del Plugin | 22:56:03Z - 22:56:47Z |
  | `expected-inputs` | 22:57:24Z |
  | Commit e8c7f73a | 22:57:57Z |

- **Suites.** Core Full 14390/14390. UI Full 1637 con 17 omitidas: cuento exactamente 17 líneas `Skip =` en `tests/RackCad.UI.Tests` en 5a5ca577 (8+3+4+2). Builds Debug de UI, Plugin y RamTool con 0 errores.
- **CI verificado por mí (solo lectura).** Corrida 38977069487: `event=push`, rama `architecture/interop-analisis-estructural`, `headSha` 5a5ca577…, los 4 jobs requeridos en `success` (UI Tests terminó a las 22:44:19Z).
- **DLL y tabla.** La DLL c72b759d… (1.0.0+5a5ca577) solo consta en la custodia. La tabla fc3d9146 coincide con el blob d088e2a9, que sí verifiqué.
- **Suficiente para el bloque §7.1.** Lo que falta es de registro, no de identidad (A65-F4C-06):
  - la `ref` de la corrida de CI no está escrita y su JSON no está custodiado;
  - la DLL se compila con `-o` fuera del worktree, lo que la guía §2 no prevé, aunque la trazabilidad es completa;
  - no consta la cobertura del Candidato.

  La versión de AutoCAD y el hash de la biblioteca de bloques se vuelven a medir en W1.

## 5. READY y decisiones en commits hijos

- **Hechos.** READY-04 (~22:40Z) y READY-05 (hasta 22:57Z) se hicieron sobre 5a5ca577. READY-03 solo se cumplió con §75 (9a6f69d6, 23:48Z).
- **Lo que dice el texto.** LIFECYCLE §8 exige satisfacer READY «en este orden», y §6 dice literalmente: «Tras READY-04, cualquier A-n o decisión nueva crea un SHA nuevo y reinicia READY-02».
- **Mi lectura:**
  - **La frase se refiere a cambios del contrato congelado.** Está en la sección de enmiendas, justo después de definir quién decide cada cambio de un elemento congelado; §9 lo repite para correcciones y aceptaciones. Leer «cualquier» sin límite no funciona: READY-06 y sus seguimientos, la autorización de declarar y la OV son posteriores al SHA por construcción. Además, §7 manda registrar los hechos posteriores sin atribuirlos al commit anterior, y WORKFLOW §11.5 fija el Candidato después de READY-06.
  - **Las disposiciones de §75 no cambian producto, Freeze, tolerancias ni tabla**, y A65-F1-06 queda confirmado. La estrategia A usa una cláusula del propio Freeze (§5, «reducirlo exige al Owner») sobre la ejecución de la OV, que necesariamente ocurre después.
  - **Un SHA posterior no reduciría ningún riesgo de producto.** Solo produciría otra DLL con el mismo árbol y obligaría a repetir Core/UI Full, builds, CI y esta revisión.
  - Hay una tensión que conviene reconocer: la preparación prefirió 5a5ca577 a 51fc82f1 para tener los documentos dentro del SHA.
- **Conclusión: aceptable para el Candidato de hito, con tres condiciones** (A65-F4C-02):
  1. La autorización del Owner para declarar registra expresamente esta lectura (LIFECYCLE §10) y cita los hijos documentales como registro posterior sin cambio de producto.
  2. READY-04 se repite justo antes de declarar, con un fetch fresco. Si main avanzó, hay rebase y se repite todo.
  3. Cualquier decisión posterior que cambie producto, Freeze, tabla o tolerancias, por ejemplo corregir A65-F4R-07, sí exige un SHA nuevo.

  No debe tomarse como patrón para el FINAL_CANDIDATE_SHA de READY: allí las decisiones deben estar versionadas antes de READY-04. Si el Owner prefiere la lectura literal, el Candidato sería un SHA documental posterior, con toda la evidencia de §79 repetida y una revisión nueva.

## 6. Matriz final

- **Cobertura frente al Freeze §5 y al paquete de ingeniería §3:**
  - OV-I65-01 completa.
  - OV-I65-03 cubre los cinco casos del Freeze, incluido «mapeo ausente». La alternativa «sin verificar» está mal apuntada (A65-F4C-01).
  - OV-I65-04, 05 y 06 están cubiertas. OV-06 sí discrimina: el giro sale de `rotation` y solo se compara `l2` (`RamBuildScriptTemplate.cs:189-191, 265`).
  - OV-I65-07 prueba las dos variantes, más de lo que exige el «o» del Freeze.
- **Independencia.**
  - Todos los paquetes son nuevos: no se reutiliza `pkg-01` ni su lectura de vuelta.
  - `inject` vuelve a embeber la carpeta de salida (`embedded = --embed-folder ?? target`) y no toca el origen.
  - CMB-3, DIF-01 informativa y tolerancias quedan intactas.
- **OV-05.** P1 S1 midió `ValueError: Section not found … must include a space and match the database exactly`, con miembro huérfano, y la inyección produce `W INEXISTENTE-99`. Si RAM aceptara ese nombre, el script solo comprueba el nombre leído (`:261`), daría `READBACK OK` y OV-05 fallaría. Eso contradice P1: STOP, decisión del Owner y autorización de implementación (`attempts` 7 de 9). El REJECTED del comparador no discrimina (A65-F4C-03).
- **OV-04.** La puerta escribe `ram-readback.json` FAILED con `gate: the model has 1 nodes` sin tocar el modelo. Solo se ejercita la rama de nudos. Teclear el nudo con Computer Use tiene el riesgo de pegado por portapapeles que ya ocurrió en P2.
- **Segunda generación (pkg-OV04).** La carpeta debe teclearse con `\`. Conviene validarla con v2 (16/16) para que el rechazo se deba solo a la puerta (A65-F4C-04).
- **Inyecciones.** Se hacen tras W1 y antes de W2, cuando pkg-OV01 todavía tiene 3 archivos. No usar `--embed-folder`, y comprobar que los hashes de pkg-OV01 no cambian.
- **Paradas y presupuesto.** El presupuesto alcanza sin reintentos. Faltan paradas operativas (A65-F4C-07). También conviene generar primero los dos paquetes válidos en W1 y después los casos de OV-03.

## 7. Estrategia A

**Compatible**, por estas razones:
- **D-11.** El volcado c4a98fb0 viene de un modelo construido solo desde la especificación (golden derivado a mano en F1, hoja de entrada beefa054 sin leer nada del generado). Se volcó antes de cualquier análisis: `initialUnits` KIP_FT, `readUnits` LB_IN, `unitsRestored` true. El operador fue Claude con Computer Use y la interfaz estaba en ft/kip, ambas cosas aceptadas por el Owner (CD-I65-F4-154/162).
- **Freeze §5.** Todo lo que depende del producto se repite sobre el Candidato. Lo que no se repite (el modelo manual) es una reducción decidida por el Owner, como exige §5.
- **AGENTS «SHA exacto».** La regla gobierna la evidencia sobre un SHA. El volcado manual es una referencia externa que no depende de ningún SHA de RackCad y tiene sus propios invalidadores. La validación del Candidato no se reutiliza: se produce de nuevo en W2.

He comprobado el volcado en el repositorio: SHA-256 c4a98fb0. Los blobs de la especificación (f39918ad y 11853341) son iguales en 5a5ca577 y HEAD.

**Las condiciones 1-5 son necesarias pero no suficientes.** Faltan (A65-F4C-05):
- **(a) Misma plantilla de volcado.** El script de volcado del Candidato debe diferir de `manual-rackcad-ram-dump.py.txt` (af262489…) solo en la línea de la carpeta. De eso depende también la validez de CMB-3.
- **(b) Volcado manual registrado.** El informe del comparador de W2 debe registrar `manualDumpSha256` = c4a98fb0….
- **(c) Condición 3 sobre la carga útil, no sobre la huella.** Comparar la carga útil de pkg-OV01 con la especificación (117/117), con `rowsSha256` 54ca9f6c y cargas 25f48e7f. No exigir la huella de P2 (505941e2): incluye `rackId` y la huella de la fuente (`StructuralModelCanonicalizer.cs:52`, `StructuralFingerprints.cs:110`), así que cambia con un DWG nuevo.
- **(d) Alcance.** Aclarar si «para el Candidato final» del texto del Owner cubre también la OV del FINAL_CANDIDATE_SHA de READY.

**Qué comprobar en cada ventana:**
- **W1:** carga útil frente a la especificación, v2 16/16, DLL c72b759d y tabla fc3d9146 releídas, y biblioteca de bloques.
- **W2:** antes y después, RAM 7dcb4855, `ramelements` f71a017f y bases US dffd9e00/992ddac9/2a8b4239; además, los puntos (a) y (b), y que no se use la lectura de vuelta de P2.

## 8. Riesgos residuales para el Owner

1. **Gobernanza.**
   - El orden READY-03/04 está invertido: hace falta reconocerlo al autorizar la declaración y repetir el fetch (A65-F4C-02).
   - El conflicto conocido con I-64 en los censos (37→38) sigue ahí: quien llegue segundo a main reconcilia a 39.
2. **DLL equivocada.** El diálogo de NETLOAD recuerda la carpeta `51fc82f1` (POST/DIFF del segundo smoke), donde están `fa334b14` y `2e04a4bb`. Hay que comprobar la ruta del módulo cargado en `acad.exe` y el SHA-256.
3. **Script equivocado.** En RAM, la carpeta `PythonScripts` apunta a `dump-man` de P2. El modo exclusivo protege los paquetes ya usados, pero conviene comprobar ruta y hash antes de cada ejecución.
4. **Riesgos de Computer Use (P2 §8):**
   - pegado del portapapeles (en P2 pegó el texto de la autorización del Owner y creó 189 elementos);
   - celdas que solo conservan el último carácter;
   - renombrado accidental por doble clic lento en el diálogo «Abrir»: en W1/W2 podría romper rutas embebidas;
   - órbita de la vista y botones de fuente.
5. **OV-05 depende de que RAM rechace el nombre;** el REJECTED del comparador no discrimina.
6. **OV-03:** no está demostrado que los casos (c) y (d) se puedan dibujar, y un `Invalid` de las cargas podría tapar el diagnóstico de la estación (A65-F4C-04).
7. **Licencia y host:**
   - licencia «Practicante» con validación lenta (Freeze §12);
   - una sola `acad.exe` (ya apareció una ajena en P2) y avisos a I-64 e I-52;
   - `VIEWBACKSTATUS` se borrará otra vez al salir de AutoCAD.
8. **Margen corto ante un defecto:** quedan 2 intentos (`attempts` 7 de 9) y Codex está en 50 de 52.
9. **Alcance del resultado.**
   - Equivalencia de transferencia ≠ validación de ingeniería (Q-O3).
   - DIF-01 (−0.62 %) queda fuera de ±0.5 %: solo es informativa porque no hay peso propio.
   - Las propiedades se evalúan con la hoja D-09.
   - La edición del catálogo de RAM es UNKNOWN.

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-F4C-01 | preparación §6 OV-I65-03; paquete de ingeniería §3 G (`:343`) | El caso «fila sin verificar» se atribuye a T4-18, que desde F4-D (D-INV-04) es la prueba **positiva** de la tabla verificada. También se atribuye a «la herramienta de pruebas sobre una copia de la tabla», y RamTool no tiene esa capacidad (solo `dump-script`, `compare` e `inject`). La cobertura real está en pruebas de fixture: `UnaFilaSinVerificadorOSinFechaEsUnsupported`, `UnaFilaProposedNuncaSeTrataComoVerified` y `UnResultadoQueNoEsValidoSePropagaConSuResultadoYSinPlan`. Además, eso sustituye una clase de evidencia por otra. | OPTIONAL | Corregir el oráculo en el procedimiento de W1 y que la autorización de W1 nombre la sustitución (guía §7.2, LIFECYCLE §8) |
| A65-F4C-02 | LIFECYCLE §6 y §8; §75 en 9a6f69d6 | READY-03 se cumplió después de READY-04/05 | OPTIONAL | Las tres condiciones de la pregunta 5; en READY, decisiones antes de READY-04 |
| A65-F4C-03 | §6 OV-05; `RamToolCompare.cs:94-126` | El REJECTED del comparador no discrimina (toda inyección se rechaza por la marca). Además `compare` exige `--generated` y `--manual` existentes y la matriz no dice cuáles (A65-F4R-04) | OPTIONAL | Tomar como evidencia la consola y la lectura de vuelta con `ValueError`; fijar los archivos de entrada |
| A65-F4C-04 | §6 OV-03 y OV-04 | Oráculos poco específicos: con la precedencia Invalid > Unsupported, un `Invalid` de las cargas o de la carpeta pasaría el caso | OPTIONAL | Código esperado por caso; cargas válidas para cada geometría; en OV-04, que la única diferencia sea la de la puerta; validar pkg-OV04 con v2 |
| A65-F4C-05 | CD-I65-F4-175 | Condiciones de la estrategia A incompletas | OPTIONAL | Añadir (a)-(d) de la pregunta 7 como paradas de W2 |
| A65-F4C-06 | §79; guía §2; AGENTS | Faltan la `ref` del CI y la custodia de su JSON; la desviación de la ubicación de la DLL no está registrada; no consta la cobertura del Candidato | OPTIONAL | Registrarlo en la declaración |
| A65-F4C-07 | preparación §10 | Faltan paradas: ruta del módulo o del script equivocada; licencia; diálogo modal; condición de Q-O1 (paquete de ingeniería §3 H); caso de OV-03 que no se puede dibujar | OPTIONAL | Incluirlas en los procedimientos de W1 y W2 |
| A65-F4C-08 | `state/I-65.yml` | `last_evidence_commit` sigue en 51fc82f1 | OPTIONAL | Corrección editorial en la próxima custodia |

## Estado de READY-01..09 para 5a5ca577

| ID | Estado |
|---|---|
| READY-01 | Cumple: Freeze sin A-n; Q-O1 y Q-O7 aprobados; OV-02 en F5 y OV-08 en el READY final |
| READY-02 | Cumple para un hito, con A65-F4C-02: F1 y F3 cerrados; las unidades de F4 cerradas; §75 está en un hijo |
| READY-03 | Cumple desde 9a6f69d6 (0 REQUIRED). Solo queda la autorización de declarar, que ya era necesaria |
| READY-04 | Cumplida a las ~22:40Z. En mi copia local, `origin/main` = bb0d5522 = merge-base, 0 commits detrás, pero sin fetch. **Repetir al declarar** |
| READY-05 | Cumple (§79 y CI verificado) |
| READY-06 | Architect: conforme con observaciones. **Coordinator: pendiente** |
| READY-07 | Cumple: árbol limpio, HEAD identificado; los hijos son solo documentación |
| READY-08 | Cumple con observaciones (A65-F4C-01, 03, 04, 05, 07, que se corrigen antes de W1) |
| READY-09 | Cumple: Freeze 4e6e1feb, diff de cabecera permitido, ADR c18def42, sin A-n |

## No comprobado sin ejecutar

- Los resultados de Core y UI locales: me basé en `local-evidence.txt`. Las 17 omitidas solo las conté por las líneas `Skip =`.
- La DLL c72b759d, su manifiesto de 32 archivos, la herramienta (0b56e198 y d421c82b) y la tabla junto a la DLL en disco.
- La ruta y el hash de la biblioteca de bloques, y la versión de AutoCAD.
- Las identidades de RAM y de las bases de datos, que la copia del volcado en disco sea igual a la del repositorio, y las capturas.
- Cómo responde RAM a OV-04..07: me baso en P1 y en la lectura de la plantilla.
- Que los casos de OV-03 se puedan dibujar en el editor.
- Si main avanzó después del último fetch.
- La cobertura del Candidato.
- K1/K2 en el checkout principal.
- La conformidad del Coordinator.

**Comprobación final.** Volví a leer las 8 preguntas y el formato pedido. Cada afirmación de identidad la verifiqué con git, la de la tabla y el volcado con `sha256sum` sobre los blobs, y la del CI con GitHub. No escribí ningún archivo.
