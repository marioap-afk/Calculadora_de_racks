# Kit de la medición A6-7 (I-62): una invocación medida del EXECUTION_CONTROLLER de FX-02 con el instrumento B-min (NO LANZADO)

> **Preparado, congelado y autoprobado sin lanzar.** Para construirlo no se ejecutó `codex.exe` ni ningún `codex` (tampoco
> `--help` ni `--version`), ni `claude` ni ninguna CLI de modelo; no hubo red ni `gh`; no se leyó nada de `%USERPROFILE%\.codex\`
> salvo el worktree de I-62 (solo lectura: objetos Git y archivos), ni `auth.json`, ni valores o huella de `config.toml`, ni
> sesiones; el pwsh del runtime de Codex solo se hasheó (nunca se ejecutó). El instrumento (`9a829b0e…`) y el aceptante
> (`434ef1fc…`) se usan solo desde sus objetos Git. Se escribió únicamente en `D:\r62-a6m7\kit\` y en `D:\r62-a6m7\m7-run\`
> (creado por `prep_clone.py`; **sin** el objeto del instrumento: `prep_write.py` no se ejecutó sobre él). Este kit no autoriza
> ninguna invocación ni consumo, no declara la celda ELIGIBLE y no adelanta ningún resultado.

## 0. Autoridad, alcance y límites

- **Marco normativo** (worktree de I-62, `HEAD` `400a72d8`): A-6 AGREED (`docs/initiatives/I-62-A-6.md`, blob `4bc919dd`),
  decisiones §90 punto 3 (solo preparación). Reglas que el kit aplica: A6-1 (orden declarada con rutas absolutas y `-I`), A6-2
  (admisión en el cierre), A6-3 reglas 1, 3, 5 y 6, A6-4 regla 3, A6-5 reglas 2, 4 y 7, **A6-7 reglas 1-7**, A6-9 regla 4.
  Receta y revalidación: `controller-contracts.md` §4-§6.
- **Medición, no verificación** (A6-7, regla 2): no produce `controller-verification/v2`, ni VERIFIED, ni C-24 PASS. Tope **uno**
  para FX-02 en F6 (regla 3; P-07); **sin reintento** (regla 4); cualquier invalidador anula la medición (regla 5).
- **Base reutilizada:** el banco `D:\r62-a6m7\bench` (designación `M`, `make_eic_verify.py` R7-A, `designate_m7.py` con KPR-1..3,
  `table_m7.py`, `derive_r4.py`), el kit `a2p2-v5-run` (`cfg-fp.ps1`, `procs-snapshot.ps1`, línea base de nombres de claves) y
  `r3/r2static/pou_check.py`; todos copiados byte a byte (§13). El banco ya mostró que R7-B no puede demostrar tres operaciones de
  A6-7, regla 1; el kit propone **R7-A**.

## 1. Secuencia (en este orden; ninguna se da por cumplida antes)

1. **Coordinator:** disposición de las lecturas normativas del aceptante (A6-5, regla 6) y **disposición de medición** posterior a
   §90 con los marcadores literales de §2 (celda, trío, instrumento y orden, intérprete, Git, `-C`, cierre R7-A, regla de
   reconstrucción, referencia a la disposición de lecturas, autorización de la escritura de preparación con su DecisionRef, kit).
2. **Owner:** la línea OD-2 y la línea (b) `A6-CONTROLLER-MEDICION-CONSUMO` (archivos `line-od2.txt` y `line-b.txt`, byte a byte),
   como **líneas completas** del archivo de decisiones, **después** de esa disposición.
3. **Supervisión, antes de lanzar:** confirmación pasiva de la identidad de Codex; cerrar la app de escritorio de Codex (ningún
   `codex.exe` puede estar vivo); I-64 sin trabajo crítico.
4. `python -I prep_write.py` (sin argumentos): escritura de preparación de A6-3, regla 1 (§4, P-W).
5. `bash run_m7.sh launch` (sin otros argumentos): puertas → punto de uso → marcador P-07 → **un** lanzamiento → posterior
   (identidad después, Entrada, eventos, registro de sesión, `Runs`, solicitud, aceptante, tabla, comparación, custodia).
6. **Custodia** de `out/custody/` (y de los SHA-256 de `out/custody/manifest.json`) y **disposición de elegibilidad** del
   Coordinator sobre `out/comparison.json`. El veredicto del comparador es una propuesta, nunca una declaración de ELIGIBLE.

## 2. Marcadores literales que tiene que contener la disposición de medición

La puerta busca, después del encabezado `## 90.`, **una sola** sección `## N.` (N > 90) que contenga cada línea siguiente como
texto literal (`grep -F`; puede ir con viñeta). Los valores salen de los archivos congelados (`disposition-markers.json`):

```text
M7-CELDA: codex-cli / gpt-6-luna / high; EXECUTION_CONTROLLER de FX-02 en F6
M7-TRIO: BinaryHash 9ecb9df2ecafcc00542ef0966c6f1737a81397dc9531b0a4b178238e2bf4a46d, AppVersion 26.1013.3656.0, huella 6EFA5B0E1E971F55CC925DB17550325A090FB9155398087C4089444E33DDBE1F
M7-INSTRUMENTO: blob 9a829b0eeb1210238052b1feac06cfa8f7964aed; orden declarada <git> --no-replace-objects cat-file blob 9a829b0eeb1210238052b1feac06cfa8f7964aed | <interprete> -I - <orden> ...
M7-INTERPRETE: %LOCALAPPDATA%\Microsoft\WindowsApps\python.exe; Python 3.13.14; SHA-256 ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec
M7-GIT: %USERPROFILE%\AppData\Local\Programs\Git\cmd\git.exe; git version 2.54.0.windows.1; SHA-256 81ef35ae005ca9318018d18e3327578ce939fb99feaad6b2d7c8ab15f3de8db5
M7-C: D:\r62-a6m7\m7-run; instantanea SHA-256 43a8caf6304edb034c3be3fd5677b35a9b3de238570ce1b71582badbcfbbc9f1
M7-CIERRE: R7-A; E SHA-256 137c7c3bba6591c84fdef4c5f6ef443f72da1cfd871478968c64a5c7cb5a1152; cierre derivado SHA-256 3d1a065f47c80c61f43d6b887a5fbe428dd628591b038a625e210c74629d09aa; registro de derivacion SHA-256 b2f0e609bf2ea14e29a13b4a8154e380fc253901e334f437ae56f8a3f3b08c1b
M7-RECONSTRUCCION: reconstruct_runs_m7.py SHA-256 1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab
M7-ESCRITURA: autorizada la escritura de preparacion de A6-3, regla 1, en D:\r62-a6m7\m7-run; DecisionRef I62-A6-7-M7
M7-KIT: FREEZE.json SHA-256 <SHA-256 de FREEZE.json, que se da fuera del kit>
M7-LECTURAS: disposicion de A6-5, regla 6: §<N>
```

- `M7-KIT` lo calcula la puerta desde el `FREEZE.json` presente (no puede estar en un archivo congelado sin circularidad).
- `M7-LECTURAS` no es literal: la puerta exige exactamente una línea así en la disposición, con `§N` (N > 90), y que la sección
  `## N.` exista y contenga el texto `A6-5, regla 6`.
- La DecisionRef `I62-A6-7-M7` es la que `E` ya lleva en la acción del instrumento (A6-2, regla 2): la disposición tiene que
  adoptarla como su identificador. El intérprete es el **alias genérico** `WindowsApps\python.exe` (el que resuelve `python` en el
  `PATH`, el de r3/r2static); el banco usó el alias del paquete (misma imagen `python3.13.exe`, mismo SHA-256).

## 3. Orden de lanzamiento exacta

`bash run_m7.sh launch`. El lanzamiento que hace (controller-contracts §4, sin ningún elemento añadido a la receta):

```text
<GNU timeout por ruta absoluta> --kill-after=10 600 "%LOCALAPPDATA%\OpenAI\Codex\bin\e78bad9887fa50cf\codex.exe" exec
  -C D:\r62-a6m7\m7-run -s read-only -m gpt-6-luna -c model_reasoning_effort="high"
  --output-schema D:\r62-a6m7\kit\schema.m7.strict-projection.json
  -o D:\r62-a6m7\m7-run\artifacts\orchestration\FX-U1\T1-m7\0\R20261016T000000Z-m7m0\output.json --json "<texto armado>"
  < /dev/null > out/raw/events.jsonl 2> out/raw/stderr.txt
```

- **Entorno del hijo:** `PATH` = directorio del pwsh del runtime + `PATH` de Windows (Machine;User, leído del registro), nunca el
  `PATH` del Git Bash; sin ninguna variable `GIT_*`; `MSYS2_ARG_CONV_EXCL='*'` para que el texto llegue sin conversión de rutas. El
  entorno tal como lo recibe un hijo nativo se captura (`out/raw/launch-env.json`) y lo usan la puerta (vi) y el aceptante.
- **Texto armado:** `prompt.m7.template.txt` + órdenes de M; 16 440 caracteres, ASCII, sin comillas dobles, SHA-256
  `a3bb4f9e2d9b9bdcaff6e9ebf7567357c382515143df91ddb7335c1dea9ba35c` (copia saneada en `frozen/`). Lleva rutas con el nombre de
  usuario: se arma en `out/raw/prompt.txt` y se custodia por SHA-256.
- **Salida `-o`:** en `IgnorePath(I)` dentro del `-C` (KPR-1; receta: `-o` al directorio del `RunId`). La Entrada admite
  exactamente ese archivo nuevo, ignorado, escrito por la CLI (no por el Controller).
- **Tope y árbol:** un vigilante mata el árbol (`taskkill /T /F` de los `codex.exe` cuya línea de órdenes nombra el `-C`) a los
  595 s; `timeout` es el respaldo; después se confirma la muerte (ningún `codex.exe` ni proceso que nombre el `-C`, hasta 60 s).

## 4. Puertas y matriz de fallo cerrado

Ninguna se omite; cualquier fallo es `REFUSED` (salida 2) **antes** del marcador P-07 y del lanzamiento.

| Puerta | Qué comprueba | Rechaza si |
|---|---|---|
| P-W `prep_write.py` | disposición con todos los marcadores; (i) sin `refs/replace/`; (ii) sin `grafts`/`alternates`, no superficial; (vii) configuración = la congelada, sin `include`/`includeIf`; estado = instantánea congelada; objeto ausente | cualquiera falla (no escribe); después: id ≠ blob o instantáneas que difieren en algo más que un objeto suelto → `Held` false |
| Kit | cada archivo = `FREEZE.json`; ningún archivo nuevo | archivo cambiado, añadido o ausente |
| Decisiones | sección única tras §90 con los marcadores de §2; `M7-LECTURAS` válida; `line-od2.txt` y `line-b.txt` líneas completas después de la disposición; la línea (b) nombra el trío en orden, el blob y la celda | falta o cambia un marcador; dos disposiciones; sin §90; línea del Owner ausente, alterada, no completa o anterior |
| Identidad antes | huella (`cfg-fp.ps1`) = `6EFA5B0E…`, nombres de claves = línea base, un único `codex.exe`, SHA-256 = `9ecb9df2…`, AppVersion = `26.1013.3656.0`, acompañantes = `2aaa7f5a6e240a4c`, pwsh del runtime = `362a356c…` | cualquier diferencia |
| P-07 | marcador ausente; ninguna salida de un lanzamiento previo | marcador presente (tope 1, sin reintento) |
| Estado del `-C` | instantánea actual = congelada + exactamente el objeto suelto `objects/9a/829b0e…`; registro de P-W `Held` con DecisionRef y la instantánea posterior | deriva de refs, índice, estado, archivos, git-dir o configuración; sin objeto; sin registro |
| Pwsh del runtime | presente con su SHA-256 | ausente o distinto |
| Git estático | sin `git.*` en el directorio del alias y de la imagen del intérprete, `System32`, `System`, `Windows`, la raíz del `-C` y el directorio del pwsh del runtime | alguno, o no se puede listar |
| Entorno | sin `GIT_*`; pwsh del runtime primero en el `PATH` del hijo | lo contrario |
| P-02 | ningún `codex.exe` vivo; ningún proceso con el `-C` en su línea de órdenes; ningún proyecto de Claude del `-C` | alguno |
| Texto | texto rearmado = SHA-256 congelado | distinto |
| Punto de uso (`pou_m7.py`) | (i)-(viii) de A6-3, regla 3, con el shell de la invocación (pwsh del runtime), modo de lenguaje registrado | `Launch` false o salida ≠ 0 |
| P-02 tras el punto de uso | los procesos de la comprobación terminaron | queda alguno |

## 5. Cobertura de A6-7, regla 1 (operación → paso del texto → comprobación de `tools_m7.compare`)

| Operación | Paso | DEMONSTRATED solo si |
|---|---|---|
| OP1 entrega exacta de los bytes por la orden declarada en el shell de la invocación | C2 (`cat-file -t` y `cat-file blob … \| hash-object --no-filters --stdin` con el Git declarado y el nombre **abreviado** del objeto) | registro: `blob` y el blob fijado como salida, código 0; salida estructurada igual; el shell de C2 es el único shell de las ejecuciones del instrumento (se registra si es el pwsh del runtime) |
| OP2 ejecución desde el objeto y aislada | C3 (comprobación primaria previa, A6-1 regla 7), C4-C15 | registro reconstruible con la regla nombrada; las 12 órdenes; Git e intérprete declarados por ruta, `-I`, salida 0; cada sobre `Isolated` true, Python 3.13.14, `Error` null; C3 antes de la primera ejecución; `Runs` de la salida enlazadas al registro; aceptante R1 R2 R3 `MATCH` |
| OP3 `git diff --name-only <base>..<head>` y pertenencia por ruta de `Scope` | C5, C6, P09 | sobres y salida = oráculo; P09 registrado e igual; R5 y R7 `MATCH` |
| OP4 lecturas Git de `Identity`, `Remote`, `CleanTree`, `Trailer` | C4, C9, C5; P01-P08, P10-P14 | sobres y salida = oráculo (tráileres por SHA-256); primarias registradas e iguales |
| OP5 blobs y hashes | C8; P15, P16 | sobre y salida = oráculo; R5 `MATCH` |
| OP6 lectura fiel de JSON y texto | C7, C10, C11, C13, C14, C15; P17-P21, P24 | sobres = oráculo; copias de la salida en texto JSON **ASCII** iguales al oráculo |
| OP7 resolución por el mapa en la forma de `AuthorityResolution` | C12; P22, P23 | `Derive` y blob del mapa = oráculo; filas en la forma, una por cita; Path/Section/Class/EffectiveSha/ClauseMapBlob/UnitProtocol = oráculo; Mode/RevisionSha/Parts = una de dos lecturas documentadas (V5 o R3), que se nombra; R6 `MATCH` |
| OP8 contraste de cada hecho con la orden Git primaria | C16, C17 (marcadores `M-P01`…`M-P24`) | 24 filas; cada orden registrada literal, su salida (entre marcadores) = oráculo y = la citada; `Equal` true |
| OP9 salidas estructuradas completas | esquema `schema.m7.json` | JSON estricto válido (ningún campo obligatorio omitido); `ShellInvocations` = ejecuciones registradas; cada `C<n>` citado existe |

**Partes del registro** (A6-7, regla 1, segunda frase, y el resto de condiciones): RA `git` por nombre (`where.exe git` en C1) =
Git declarado, `--exec-path` y versión de esa instalación, y exploración estática limpia; RB `Runs` reconstruidas con la regla
nombrada (SHA-256), ningún byte alterado; RC aceptante fijado (bytes por blob), intérprete declarado, mismo `-C`, tras la Entrada,
cero escrituras, árbol de procesos terminado, `ACCEPT_FACTS` con R1 R2 R3 R9 `MATCH`, R4 R10 `NOT_APPLICABLE`, R8 `MANUAL`, R5-R7
según la tabla, y tabla de M aceptada (construcción, filas, cotejo del cierre, designación); RD identidad antes = después = trío; RE
modelo y effort de `turn_context` = `gpt-6-luna`/`high`; RF sin lectura P-22; RG sin escritura en el clon (salvo el `-o`); RH salida 0,
sin tope, muerte confirmada, eventos completos, sin señal de límite de uso (P-06), intérprete y Git declarados en la Entrada.
`ALL_OPERATIONS_DEMONSTRATED` solo si todo se cumple; si no, `NOT_DEMONSTRATED` con sus causas.

## 6. Cierre de VERIFY de M (A6-7, regla 7; A6-2, regla 5)

- `-C`: `D:\r62-a6m7\m7-run`, copia determinista de `D:\r62-a6r2\r3\deliv` con sus refs (HEAD = `origin/fx/u1` = G `b417d36c`,
  `origin/main` = `fbe25347`), índice refrescado una vez; copias de las entradas en rutas ignoradas: DELEG y H en sus rutas KPR-2 (=
  rutas de `E`), el TRX en `<RunPath>ci-current/` (la ruta de `E`, `artifacts/ci/…`, no está ignorada; sigue siendo el identificador).
  Instantánea `43a8caf6…` (9 refs, 81 archivos, 263 archivos del git-dir, `!! artifacts/`).
- Designación: la `M` del banco con `Repo` = el `-C`, `Custody` en `inputs/` y `DecisionRef`; `ExpectedWorktree`/`Worktree` de las
  copias nombran el `-C` (el mismo método reproduce byte a byte las del banco).
- `E` (R7-A, 16 entradas canónicas, con el instrumento) `137c7c3b…`; cierre derivado `3d1a065f…`: 7 commits, 89 `RevPaths`, 3 refs,
  49 rutas, archivos CONTRACT, DELEG, H, TRX; omisiones: solo `C1.D.ChainRedSha FIELD_NULL`; registro `b2f0e609…`. Frente al D-M-R7-A
  del banco solo cambian los SHA-256 de DELEG y H (la ruta del `-C`). R7-A exige la lectura de I6 que describe el banco (§2): la
  disposición la adopta al nombrar R7-A.

## 7. Regla de reconstrucción de `Runs` (A6-3, regla 6 (a))

`reconstruct_runs_m7.py` (v2, `1eceecee…`) = la de referencia (`5dc9f78e…`, que se conserva en el kit) más **solo**:
(1) dos representaciones exteriores documentadas en r3/r2static: `"<pwsh>" -NoProfile -Command <inner>` y `"<cmd.exe>" /c <inner>`
con una gramática de cmd cerrada (dos rutas absolutas entre comillas dobles, argumentos desnudos, sin `& | < > ^ % ! ( ) ; $ '`
salvo la tubería única); (2) en la posición del **shell**, un componente puede empezar por punto. **Hallazgo:** con la referencia, una
orden registrada con el pwsh del runtime (`%USERPROFILE%\.cache\codex-runtimes\…\pwsh.exe`) no se reconstruye (`.cache`), así que la
referencia rechazaría todas las ejecuciones reales. Git y el intérprete conservan la comprobación de la referencia. Pruebas (§9, B):
64 registros r3/r4/banco/custodiados con el mismo resultado salvo F-outer-flags (la forma añadida) y F-cmd-shell (ambas rechazan; otro
mensaje); las formas nuevas reconstruyen con los mismos argv y sobres; 29 variantes mal formadas fallan cerradas. La disposición nombra
v2; la referencia queda disponible.

## 8. Texto, esquema y oráculo

- **Texto** (medición, no verificación; sin el aceptante, la SPEC ni las lecturas): C1-C17 literales (17 ejecuciones de la shell);
  el identificador completo del blob solo en las 12 órdenes (A6-3, regla 6 (a): toda orden que lo contiene es una ejecución).
- **Esquema:** `schema.m7.json` (validación del comparador) y su proyección estricta sin `minLength` (`--output-schema`); mismas
  palabras clave que v5; cada objeto cerrado y con todas sus propiedades obligatorias.
- **Oráculo** `frozen/oracle.json` (`2e0c1a0d…`): solo primitivas Git con el Git declarado y los bytes custodiados; nunca el
  instrumento, el aceptante ni la SPEC; sellado en `FREEZE.json`; fuera del `-C`; nunca se entrega al Controller. EFF = `fbe25347`,
  blob del mapa `4d49d3e1…`. Para `AuthorityResolution` registra dos lecturas de 16.13 ya usadas (V5: `RevisionSha` = AR para
  UNIT_DOC/UNIT_CHANGE; R3: `--main` para todas): el comparador acepta una y la nombra; **la confirma el Coordinator**.

## 9. Autoensayo (sin lanzar): 113 de 113

`bash selftest_m7.sh` (≈ 4-5 min); informe `selftest-report.json`. Codex y el pwsh del runtime no se ejecutaron (el pwsh 7 del `PATH`
hace de shell); config.toml y sesiones son fixtures; `m7-run` solo se leyó (igual a su instantánea al final); todo se escribió en
copias bajo `.selftest/`, borradas.

| Grupo | Casos | Qué |
|---|---|---|
| A estático | 6 | copias byte a byte; sintaxis; esquema; texto (y **hallazgo**: la referencia rechaza el shell del runtime); oráculo independiente |
| B reconstrucción | 8 | v2 = referencia en 62/64 registros (las 2 diferencias previstas); `-NoProfile`, cmd y pwsh del runtime reconstruyen; `-EncodedCommand` no se identifica (límite igual que la referencia); 29 mal formadas fallan |
| P preparación | 9 | clon determinista; `prep_clone` no toca `m7-run`; P-W rechaza `refs/replace`, `alternates`, `include`, sin disposición, deriva; escribe una vez exactamente un objeto suelto |
| U punto de uso | 7 | pasa; falla en (i), (iii), (v), (vi), (vii), (viii) |
| S sesión sintética | 3 | las 17 ejecuciones reales por la forma declarada (pwsh 7); 12 sobres reconstruidos; aceptante real `ACCEPT_FACTS`, tabla aceptada; comparador `ALL_OPERATIONS_DEMONSTRATED` (Controller perfecto leído de los sobres frente al oráculo) |
| N comparador | 25 | una alteración por causa (OP1-OP9, RA-RH) → `NOT_DEMONSTRATED` con esa causa |
| G puertas | 48 | cada marcador ausente o alterado, líneas del Owner, identidad (7), P-07, deriva del `-C`, entorno, `git.*`, procesos, proyecto de Claude, kit, texto, guardas del modo fixture |
| D ejecutor | 7 | formas de uso; `--dry-run` pasa y rechaza; lanzamiento con **stub** compilado: argv exacto de la receta, texto byte a byte, `PATH` y `git` por nombre, posterior completo `ALL_OPERATIONS_DEMONSTRATED`; segundo lanzamiento rechazado; vigilante mata el árbol y confirma la muerte |

## 10. Condiciones que solo se verifican en la sesión real (nunca se dan por cumplidas aquí)

Identidad real (huella, binario, versión, acompañantes) antes y después; estado de autenticación (no observable); que el Owner
cerró la app de Codex; el punto de uso con el pwsh del runtime y su modo de lenguaje; la forma exacta con que 26.1013 registra las
órdenes y si el shell es el pwsh del runtime, `-NoProfile` o cmd; que el sobre llega sin alterar, separable de stderr y sin truncar
(clausemap ≈ 44 KB; el mayor `aggregated_output` observado mide 59 805 bytes); que el intérprete alias arranca bajo el sandbox de
Codex; `git` por nombre en el entorno real del sandbox; el tiempo (tope 600 s; v5 usó 42 órdenes en ≤ 415 s; aquí son 17); que el
modelo copia literalmente las órdenes y rellena el esquema completo; el modelo y el effort efectivos; la escritura de `-o`.

## 11. Hallazgos (no se reetiquetan como superados)

1. La referencia de reconstrucción (`5dc9f78e…`) **no puede** reconstruir ninguna ejecución con el pwsh del runtime (`.cache`); v2 lo corrige (§7).
2. Desde el Git Bash, `/mingw64/bin` va antes en el `PATH`: `git` por nombre sería `mingw64\bin\git.exe` (otro SHA-256). El ejecutor construye el `PATH` del hijo desde el registro.
3. Con ese `PATH`, `timeout` desnudo es `C:\Windows\System32\timeout.exe`; el ejecutor usa el GNU `timeout` por ruta absoluta (lo detectó el stub).
4. Hoy hay `codex.exe` vivos de la app de escritorio: la puerta P-02 rechazará hasta que se cierre.
5. La prueba de entrega exacta dentro de la invocación usa el nombre abreviado del objeto (`9a829b0eeb12`) porque A6-3, regla 6 (a), cuenta como ejecución toda orden con el identificador completo; la disposición tiene que aceptar este paso.
6. Lectura de `AuthorityResolution` (Mode/RevisionSha) sin fijar: V5 y R3 difieren en las filas UNIT_*.
7. Una ejecución que no nombre el blob (`-EncodedCommand`, variable, archivo) no se identifica (límite de la referencia, sin cambio).
8. La ruta de `E` del TRX (`artifacts/ci/…`) no está ignorada en el fixture; la copia va a una ruta ignorada y `--file` la nombra.
9. `pou_check.py` usaba `[Console]::Out.Write` (bloqueado en ConstrainedLanguage): `pou_m7.py` lo sustituye solo en la resolución por nombre.
10. Hallazgos del banco que siguen vigentes: contrato ausente de la AR, lectura de I6 que exige R7-A, ruta del instrumento en C4.

## 12. Riesgos residuales

Versión 26.1013 sin medir (representación de órdenes, shell, `-NoProfile`, ConstrainedLanguage, prefijos de codificación); alias de
Python bajo el sandbox; tope de 600 s; tamaños de sobre (clausemap ≈ 44 KB); el modelo puede desviarse del texto literal (una
desviación es `NOT_DEMONSTRATED`, sin reintento); lectura de `AuthorityResolution` pendiente del Coordinator; el SHA-256 del Git
declarado cubre el lanzador `cmd\git.exe`, no `mingw64\bin\git.exe` ni sus DLL; TOCTOU entre el punto de uso y el lanzamiento; la
configuración global de Git no se comprueba; el `PATH` del hijo depende del registro de Windows en ese momento.

## 13. Archivos (SHA-256)

Reutilizados byte a byte: `derive_r4.py` `3de12c54…`, `designate_m7.py` `491dcbd6…`, `make_eic_verify.py` `58f13c6a…`,
`table_m7.py` `a0765733…`, `table_r3.py` `ec4d9a8e…`, `table_r4.py` `e1efb54a…`, `reconstruct_runs.py` `5dc9f78e…` (banco);
`cfg-fp.ps1` `b1d833c0…`, `procs-snapshot.ps1` `f879db30…`, `baseline-keynames-F909F480.json` `076637c5…` (kit v5, bytes custodiados);
`pou_check.py` `c3490925…` (r3/r2static); `line-od2.txt` `3b33dd56…`, `line-b.txt` `dda4b3f9…` (líneas del Owner).
`FREEZE.json` lista el SHA-256 y el tamaño de **todos** los archivos del kit (incluidos `frozen/`, `inputs/`, este README y el informe
del autoensayo), el texto armado, la instantánea del `-C`, el oráculo, `E`, el cierre, el registro y las dos reglas. El SHA-256 de
`FREEZE.json` se da fuera del kit (marcador `M7-KIT`). `out/` no existe antes de la medición.
