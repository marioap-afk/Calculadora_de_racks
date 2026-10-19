# I-65 — Revisión independiente del Architect sobre el SHA propuesto del Candidato de hito de F5 (`bd1fcf8a`)

- Rol: Architect de I-65, SAME-SESSION ROLE (subagente opus de contexto independiente), solo lectura. 2026-10-19, 02:01Z-02:21Z.
- SHA revisado: `bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109`; HEAD al revisar: `daa6fb77127c9547b8056ac6f64913edf0a1841a`.
- Encargo: [architect-f5c.prompt.md](architect-f5c.prompt.md) (copia saneada; SHA-256 del original en la evidencia §93).
- Controles: K1/K2 sin cambios en ningún checkout; K5 limpio (71 usos de herramientas: 57 Bash, 12 Read, 2 Grep; 0 intérpretes, 0
  AutoCAD/RAM, 0 escrituras; [architect-f5c-k5.json](architect-f5c-k5.json)).
- Informe literal (sin cambios, salvo esta cabecera):

---

**Veredicto: CONFORMANT WITH OBSERVATIONS** sobre `bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109` como SHA propuesto del Candidato de hito de F5. Hay 0 hallazgos REQUIRED y 9 OPTIONAL (A65-F5C-01..09).

**pwd:** `/d/Documentos/Codex/Calculadora de racks`, que es el checkout principal. Lo informé y no lo usé para leer.

**Mis propias desviaciones de las reglas:**
- Hice un `ls -la` (solo metadatos, sin leer contenido) de `.git/FETCH_HEAD` del checkout principal. No uso el resultado en ninguna conclusión.
- Ejecuté `git status --porcelain` dos veces; esa orden no está en la lista permitida. Puede refrescar la caché de la índice del worktree, pero no cambia contenido.
- Usé otras órdenes git de solo lectura que tampoco están en la lista: `reflog show`, `merge-base`, `rev-list`, `check-attr`, `config` (lectura), `grep` y `diff --no-index`.
- Usé herramientas de texto: cat, grep, sed, awk (para formatear una salida), sha256sum, wc, head y cut.
- No ejecuté ningún intérprete Python, compilación ni prueba. No escribí nada. No entré en `D:/r65-*`.

En adelante, `<worktree>` es el worktree de I-65 (su ruta contiene el nombre de usuario).

## Respuestas

**1. Identidad.** Cumple.
- `bd1fcf8a` es hijo de `511f2043`.
- `git diff --name-only 51fc82f1 bd1fcf8a` fuera de `docs/` da vacío (310 archivos, todos en `docs/`). Lo mismo desde `1e7d8fc2`.
- `ls-tree` da los mismos árboles y blobs en `51fc82f1`, `1e7d8fc2`, `bd1fcf8a` y `daa6fb77`: src `6cadd526`, tests `47fbcaf5`, tools `29485f42`, assets `d1cc1811`, Directory.Build.props `69223076`, Directory.Build.targets `58d1c335`, global.json `eaa68c65`, RackCad.sln `6483b50d`, más .github, deploy y eng.
- Freeze `4e6e1feb…` y ADR-0050 `c18def42…` son iguales en los cuatro commits.
- No hay A-n ni Freeze delta de I-65.
- Entre `bd1fcf8a` y `daa6fb77` cambian 10 archivos, todos en `docs/`:
  - `I-65-evidence.md`: solo se añaden 42 líneas, la §92 (sin borrar nada).
  - 8 archivos nuevos en `candidate-bd1fcf8a/`.
  - `state/I-65.yml`: cambian `next_action` y `last_evidence_commit`, y se añade el registro `f5_milestone_candidate_proposed`. Es contabilidad, no una decisión.
- Las decisiones y la evidencia solo crecen (`--numstat` sin borrados); el único archivo editado es el yml de estado.

**2. Frontera documental (CD-I65-F5-09).**
- `bd1fcf8a` contiene:
  - Decisiones §84-§87 con los tres textos literales. Tamaños y SHA-256 del archivo y del cuerpo coinciden: 2245 / `46042188` / `DC223FC4`, 4648 / `A8EADA6C` / `31EF841D`, 3873 / `85E1F40C` / `0C0AD19B`.
  - La preparación (`2511170E`), los procedimientos W3/W4 (`88625E4E`) y la matriz AC-F5 (en el §5 de los procedimientos).
  - El producto es el árbol de `51fc82f1`.
- Lo posterior es solo evidencia del SHA exacto. Las conformidades no pueden ir dentro del SHA que revisan, así que esto no es una excepción documental. Es conforme con:
  - LIFECYCLE §8 («su propio Candidato completo»),
  - AGENTS «Orden para la evidencia local» y «Commit documental»,
  - la orden de no reutilizar CD-I65-F4-178/184.
- La lectura de CD-I65-F5-09 es **correcta pero no suficiente**. No define qué cuenta como una disposición «que deba constar en el Candidato». Con eso:
  - Una disposición previa del Owner sobre los OPTIONAL de esta revisión, o sobre las autorizaciones de W3/W4, podría leerse como motivo para un SHA nuevo. En F4 ese mismo tipo de disposición (§74-§75) fue el que obligó a la excepción.
  - Entonces cada disposición forzaría SHA nuevo, evidencia nueva y revisión nueva, en bucle.
  - Ver A65-F5C-01.

**3. Evidencia de §92.**
- La cronología es coherente: commit a las 01:36:48Z; evidencia local de 01:38:32Z a 01:54:08Z con `dirty=0` y HEAD = bd1fcf8a antes y después; push a las 01:55:48Z (reflog); CI de 01:55:49Z a 01:59:28Z.
- La suma de tiempos cuadra en secuencia: Core 253 s + UI 627 s + builds ≈ 15:36.
- Resultados registrados:
  - Core 14390/14390.
  - UI 1637 superadas y 17 omitidas.
  - Build de UI con 0/0; Plugin con 2 MSB3277; herramienta con 0/0.
  - Manifiesto `bd096bec` con 32 líneas. El fichero coincide con su hash. Frente a F4 solo cambian los 8 binarios `RackCad.*` (comprobado con diff).
  - Manifiesto de la herramienta `3e6ec61f`, con 11 archivos y la misma Application y Domain que el Plugin.
  - Tabla: comprobé que el blob `d088e2a9` da SHA-256 `fc3d9146…`, 3279 bytes, igual en `51fc82f1`.
  - Archivo de cargas: el blob `dcc1a039` da `d675d9f6…`, 1112 bytes, LF.
  - CI 39187567306: push, rama exacta, `head_sha` exacto, 4/4 con `UI Tests (WPF controls, net8.0-windows)` (según el resumen versionado).
- Los datos bastan para el bloque §7.1: SHA, árbol limpio, SDK 8.0.423, Core, UI, build de UI, build de Plugin y CI. Faltan cosas propias de la declaración:
  - el bloque literal,
  - el fetch fresco (A65-F5C-02),
  - el registro de la desviación de guía §2, porque la DLL está fuera de `bin\Debug` (A65-F5C-07),
  - la biblioteca de bloques y la versión de AutoCAD, que se miden en W3,
  - la conformidad del Coordinator.
- La custodia cuadra en 24 de 24 archivos (§89-§92): blob = disco.
- PRIVACY HOLD: se respeta. No hay usuario, rutas personales, `.trx` ni JSON crudo de la API. Mi barrido de las líneas añadidas solo encuentra el identificador del repositorio en textos de §85, que es anterior al hold e inherente al repositorio. Detalle menor en A65-F5C-07.

**4. OV-I65-02, recalculado a mano.**
- Bases:
  - L = 6.354218840504721 + 36 = 42.354218840504721 in.
  - r0 = 6.354218840504721 / L = 0.150025641233831; resto: 0.15·L = 6.353132826075708, sobran 0.00108601442901285, y eso entre L da 2.56412338e-5.
  - %(f) = 100·(r0 + f·(1−r0)).
  - 1/4.4482216152605 = 0.22480894309971. Además 0.45359237·9.80665 = 4.4482216152605 exacto, así que kgf/lbf = 1/0.45359237.

| Carga | Distancia desde J | Porcentaje | Fuerza | Entrada manual |
|---|---|---|---|---|
| Q1 | 6.354218840504721 in | 15.0025641233831 % | 2000 N = 449.617886199421 lbf | −0.449617886 kip |
| Q2 | 42.354218840504721 in | 100 % | 1500 N = 337.213414649566 lbf | −0.337213415 kip |
| Q3 | 15.354218840504721 in | 0.25 + 0.75·r0 → 36.2519230925373 % | 750 lbf | — |
| Q5 | 24.354218840504721 in | 0.5 + 0.5·r0 → 57.5012820616915 % | 300·9.80665 = 2941.995 N = 661.386786554633 lbf | −0.661386787 kip |

- Q4 (91.5002564123383 %, 1000 lbf) y Q6 (78.7506410308458 %, 1011.640243948697 lbf) también coinciden.
- Signo: D-04 dice Y_RAM = Z, así que −Z pasa a GLOBAL_Y con valor negativo. La plantilla (`RamBuildScriptTemplate.cs:198-199`) y el payload (Q2 = −337.2134146495658) lo confirman.
- Miembro: el brazo k va a `ARM.k.PositiveY`, que en la hoja son m5, m6 y m7 con I = nudo J.
- **La tabla es correcta.**
- Q2 sale en 99.99999999999997 %, que es 100 menos 2 ulp:
  - Frente a 100 son 2.8e-14 %, unos 1.2e-14 in. Frente al valor a mano en coma flotante, 1.4e-14 %.
  - El margen frente a 0.02 in (0.047 % de L) es mayor de 10^12. Cabe en la lectura de vuelta (1e-9) y no supera el 100 %.
  - El algoritmo y las tolerancias no cambian.
  - La diferencia es coherente con CD-I65-F5-08.
- El comparador empareja por la posición más cercana (`StructuralReadbackComparison.cs:1214ss`). Con cargas separadas unos 21-36 in el emparejamiento no es ambiguo.

**5. Comprobación en seco.** El resultado registrado es correcto:
- Application `1.0.0+bd1fcf8a`, cargas `d675d9f6`.
- Modelo `Valid` con 0 diagnósticos y paquete `Valid` con 3 archivos.
- 6/6 IGUAL; `PAYLOAD` `939dc7e6` igual al de `5a5ca577`; `exists_D_r65_dryrun=False`.
- Matiz: la línea PAYLOAD sí se escribió a un archivo del scratchpad, y la salida no registra la ruta ni el SHA-256 de las DLL cargadas (A65-F5C-07).
- Los valores esperados del validador v2 salen de la Application de la salida (`1.0.0+bd1fcf8a`), de la tabla `fc3d9146` y de las cargas `d675d9f6`. Las filas `54ca9f6c` son iguales a las de F4. El v2 comprueba las cargas por bytes (V-06/V-07), así que 16/16 aplica a seis cargas. El contenido del JSON solo se custodia por hash.

**6. Hoja manual.**
- Se deriva de la hoja de geometría de P2 (que viene de la especificación congelada) y del archivo de cargas, con las constantes de la especificación (`I-65-f4-ref-cant-01-manual-procedure.md:48-71,130`) y las de D-10b/D-03. No lee el paquete, el payload ni la lectura de vuelta.
- Un `diff` contra la hoja de P2 solo cambia `source` y `loads`. La geometría es idéntica y los nudos de la especificación coinciden.
- Conforme con CD-I65-F5-04/10 y D-11.
- Ambigüedad de signo: la hoja pone «global Y negativo» y además valores negativos (A65-F5C-04).

**7. Procedimientos W3/W4.**
- Son coherentes con W1/W2 y con el Freeze:
  - W4-08 compara sin `--*-unread` (CMB-3), con propiedades NOT_EVALUATED (D-09) y categoría manual y peso propio INFORMATIVE.
  - La opción 0 se declara evidencia operativa y no criptográfica (W4-10, preparación §7, CD-I65-F5-11).
  - Q2 tiene STOP (§4.3).
  - C-4 es aplicable: W3-PR-01 y W4-PR-01 exigen el repositorio privado comprobado.
- Huecos:
  - Faltan identidades de herramientas en W3 (validador, contraste de la carga útil, script de host, AutoCAD), y queda pendiente A65-F4F-08 (A65-F5C-03).
  - W4 presupuesta unas 2 h frente al precedente de I-64 de 90 min y no tiene parada por tiempo; tampoco comprueba la integridad del paquete antes y después (A65-F5C-05).
  - Faltan reglas para Computer Use y para la verificabilidad de C-4 (A65-F5C-06).
- Riesgos:
  - La sesión del usuario en AutoCAD tiene cargada la DLL de F4 y puede ensuciar la BASELINE (W3-PR-05 la detectaría y pararía).
  - Las cargas de cada miembro van en orden ascendente; la lectura de vuelta por índice no distingue el orden de inserción del orden por posición (P1 L1 también era ascendente). No afecta a OV-I65-02.

**8. READY-01..09 (lectura de hito).**

| ID | Estado | Motivo |
|---|---|---|
| READY-01 | CUMPLE | |
| READY-02 | CUMPLE | F1-F4 cerradas; documentos de F5 en el SHA; F5 cierra después de W3/W4 por construcción |
| READY-03 | CUMPLE | Sin REQUIRED; C-1, C-2, C-5 y W3/W4 quedan fuera del contenido del Candidato (A65-F5C-01) |
| READY-04 | PENDIENTE | Hace falta un fetch fresco antes de declarar |
| READY-05 | CUMPLE | Según el resumen del CI |
| READY-06 | PENDIENTE | Falta la conformidad del Coordinator |
| READY-07 | CUMPLE | Árbol limpio; `status` vacío en HEAD |
| READY-08 | CUMPLE | |
| READY-09 | CUMPLE | |

**9. Matriz AC-F5.** Los estados son correctos y AC-F5-02 lo verifiqué. Le faltan un criterio explícito de aceptación de W3/W4 por el Owner y otro de integridad del host. Además, AC-F5-03 dice «DLL según la guía», que choca con el `-o` mientras la desviación no se registre (A65-F5C-08).

**10. Hallazgos previos.**

| Hallazgo | Estado |
|---|---|
| F4F-01 | Atendido (bytes LF; CRLF declarado) |
| F4F-02, 03, 10 | No aplican |
| F4F-04 | Opción 0 aprobada para F5; se ejecuta en W4; sigue abierto para READY |
| F4F-05 | Cerrado |
| F4F-06 | Pendiente para W3-07 |
| F4F-07 | Abierto: hold activo; C-1 pendiente (repositorio público a las 01:57Z); C-2 aprobado sin ejecutar; C-5 pendiente |
| F4F-08 | Vence en W3 y no está explícito |
| F4F-09 | Parcial (sin árboles de las bases) |
| F4C-02 | Superado por el Candidato propio |
| F4C-06 | La desviación de la ubicación de la DLL no se vuelve a registrar |
| F4R-07 y CD-I65-F4-220 | Siguen como limitaciones |
| Conflicto de censos con I-64 | Pendiente para la fusión |

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-F5C-01 | CD-I65-F5-09 | No hay un criterio operativo para «disposición que deba constar»; riesgo de bucle | OPTIONAL (prioritario) | Disponer de estos OPTIONAL **dentro del mismo acto de declaración** (como CD-I65-F4-187). Clasificar como actos posteriores las autorizaciones y anexos de W3/W4 y C-1/C-2/C-5. Pedir SHA nuevo solo si cambian producto, Freeze, cargas, tabla, tolerancias, oráculos o la matriz |
| A65-F5C-02 | Evidencia §92 | «0 detrás» sin fetch fresco registrado | OPTIONAL | Repetir la comprobación previa a la declaración con fetch (como CD-I65-F4-188) |
| A65-F5C-03 | Procedimientos §1 y §3 | Sin nombre y SHA-256 del validador v2, del contraste 6/6 (el script de comparación no está versionado), del script BASELINE/POST y de AutoCAD; A65-F4F-08 vence | OPTIONAL | Fijarlo en la autorización de W3; usar la página de códigos 65001 o el SDDL |
| A65-F5C-04 | Hoja y preparación §5 | «Global Y negativo» más valor negativo puede leerse como doble signo (frente a `ref-cant-01-manual-procedure.md:118`) | OPTIONAL | Instrucción explícita en la autorización de W4 |
| A65-F5C-05 | Procedimientos §4 | W4 de unas 2 h frente a ventanas de I-64 de 90 min; sin parada por tiempo; sin integridad del paquete antes y después ni árboles de las bases; ruta del registro sin fijar | OPTIONAL | Pedir una ventana ≥ 2.5 h o dividir; añadir lo que falta |
| A65-F5C-06 | Procedimientos §2 | Las capturas de Computer Use son de pantalla completa y transitorias; no se define cómo producir los recortes; revisa la misma persona; el operador puede ver las rejillas del generado | OPTIONAL | Recorte por script de región; segundo revisor (Architect); no abrir las rejillas del generado |
| A65-F5C-07 | Evidencia §92 y archivos del Candidato | Seis puntos, detallados abajo | OPTIONAL | Corregirlo en el bloque §7.1 y en adelante |
| A65-F5C-08 | Procedimientos §5 | AC sin aceptación del Owner de W3/W4 ni integridad del host; AC-F5-03 sin la desviación | OPTIONAL | Evaluarlos en el cierre |
| A65-F5C-09 | Preparación §4 y yml | «≤ 2e-7 in» (real ≈ 2.1e-7); `next_role` desactualizado | OPTIONAL | Editorial |

Detalle de A65-F5C-07:
- (a) La comprobación en seco no registra las DLL que cargó.
- (b) «Nada escrito» frente a la línea PAYLOAD escrita en el scratchpad.
- (c) El escáner de secretos no está versionado y no cubrió §92 ni el yml.
- (d) `expected-inputs.json` no se puede revisar.
- (e) La desviación de guía §2 no está registrada.
- (f) Hay detalle innecesario de la sesión de AutoCAD del usuario.

## No comprobado sin ejecutar

- La corrida de CI 39187567306 en la API.
- Las DLL, salidas y manifiestos en disco, y el ProductVersion.
- Las cuentas de pruebas y builds (sin TRX) y los nombres de las 17 omitidas.
- La ejecución de la comprobación en seco y su script de comparación.
- El contenido de `expected-inputs.json`.
- El escáner de secretos.
- El fetch fresco y la visibilidad del repositorio.
- El estado del host.
- El comportamiento de RAM con el 100 % y con dos cargas por miembro.
