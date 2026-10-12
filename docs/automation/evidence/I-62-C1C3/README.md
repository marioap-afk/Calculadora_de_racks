# I-62 — C1-C3: diseño y experimentos desechables (carril independiente de §81)

- **Autoridad:** decisiones de I-62 §81 (Owner, 2026-10-12): SECURITY DESIGN AUTHORIZATION = YES; IMPLEMENTATION AUTHORIZATION = NO.
- **Qué hay aquí:** el diseño de C1-C3 ([c1c3-design.md](c1c3-design.md)), la propuesta de una enmienda independiente sin redactar
  ([c1c3-amendment-proposal.md](c1c3-amendment-proposal.md)) y los experimentos con sus resultados ([experiments/](experiments/)).
- **Qué no hay:** ninguna implementación, ningún cambio en I-61, I-65, A-6, FX-02, lanzadores, permisos, `PATH` del sistema, alias o
  configuraciones `.claude`/`.codex`. C1-C3 **no** están implementados. I-65 **no** queda desbloqueada.

## 1. Conclusiones

| Tema | Resultado |
|---|---|
| C1 (fotografía de todos los checkouts) | detecta la clase de F4-T4 en la misma Entrada; necesita barrido de **directorios** para lo transitorio (F4-T5), `--no-optional-locks` para no escribir el índice, barrido de ignorados y un modo profundo opcional contra cambios con el mismo tamaño y la fecha restaurada |
| C2 (directorio de proceso fijado por el transporte) | arrancar el hijo en el worktree convierte la escritura de F4-T4 en una edición dentro del alcance; el canario y el primer hecho de `cwd` lo detectan antes de escribir; en `claude-subagent` no se puede fijar por llamada |
| C3 (intérprete bloqueado) | filtro de `PATH` del hijo + shims trampa: bloqueo de todas las formas por nombre (incluidas ofuscadas) con marcador y registro para una clasificación propia; límites: ruta absoluta, `perl` de Git Bash, alias colaterales |
| nc2 / A5 | son C5 y C6 del paquete: controles separados, no C1-C3; I-62 ya cubre parte de C5 (16.22 + repetición del Coordinator) |
| Materialidad | **MATERIAL**: M-02..M-06 sí; M-07 y M-08 UNKNOWN (cuentan como activados); M-01 no → propuesta de enmienda independiente de A-6 |

## 2. Experimentos

Entorno: Windows 11 (10.0.26300), PowerShell 7.6.6, Git 2.54.0.windows.1, unidad D: NTFS. Todo bajo `D:\r62-c1c3\` (repositorios Git
desechables: «principal» + worktree de la unidad + worktree hermano por caso). Git aislado por proceso (`GIT_CONFIG_NOSYSTEM=1`,
`GIT_CONFIG_GLOBAL` = archivo vacío, identidad sintética sin correo). Los entornos modificados solo existen en el bloque de entorno de cada
proceso hijo. Ejecución en serie (CPU moderada; ≈ 2,5 min la corrida completa). Orden canónica:

```text
pwsh -NoProfile -File D:\r62-c1c3\run-all.ps1
```

`run-all.ps1` lanza cada experimento como hijo, recoge su salida en `results/run-manifest.json` (con el SHA-256 de cada script y resultado) y
comprueba que ninguna salida contenga el nombre de usuario ni correos (0 hallazgos). Cada `results/exp-*.json` lleva en `Steps` cada orden,
su directorio, su código de salida y su resultado.

| Exp. | Pregunta | Resultado (corrida canónica) |
|---|---|---|
| **a** relativa | ¿Reproduce la clase de F4-T4? ¿La ven C1 y C2? ¿La impide `cwd` = worktree? | A1 (hijo en el principal, `Set-Location` al worktree, `[IO.File]` relativo): escribe el `RackCad.sln` del principal, BOM `true → false`, worktree intacto; C1 `STOP` (` M RackCad.sln`), C2 `STOP_C2_CWD_MISMATCH`. A2 (hijo en el worktree): escribe el del worktree; C1 `NONE`, C2 `PASS`. A3 (trampa vacía): la edición falla; la creación cae en la trampa. A4 (cmdlets): resuelven contra la ubicación de PowerShell. A5 (Bash): `cd` cambia el directorio del proceso |
| **b** hermano | ¿Detecta C1 una escritura en otro worktree? | B1: hermano `CONTENT_CHANGED` (` M src/A.cs`, `?? notes.txt`) → `STOP`; C2 `PASS` (no la impide). B1c: hermano declarado concurrente → `ATTRIBUTION_REQUIRED`. B2 (ruta desde el principal que vuelve al worktree propio, como F4-T1): `NONE` + señal `PATH_VIA_OTHER_CHECKOUT` |
| **c** intérprete | ¿Qué devuelve la sonda en un hijo sin Python ni alias? ¿Cómo se clasifica? | Python solo alcanzable por el directorio de alias (31 alias colaterales al quitarlo; `git`, `pwsh`, `dotnet`, `gh` siguen). Filtro: 15/15 `NOT_FOUND`. Shims + filtro: 14/15 `BLOCKED_BY_SHIM` con marcador y registro; `Process.Start` → `NOT_FOUND` sin marcador. Control positivo con intérprete falso: corre; con ruta absoluta también corre. Gancho de texto: 18 TP, 2 FN, 8 TN, 1 FP. Bash: `/usr/bin/perl` resuelve. Canario: PASS |
| **d** falsos positivos | ¿Qué ruido ve C1 y cómo se filtra? | `git status` sin `--no-optional-locks` reescribe `.git/index` (con la opción, no); toque de fecha → `TOUCH_ONLY` (`SIGNAL`); temporales: `.vs/`, `*.dwl`, `*.dwl2`, `~$*`, `*.swp` listados como ruido, pero `RackCad.sln~`, `plano.bak`, `.~lock…#`, `*.orig` siguen siendo `STOP`; `bin/`/`obj/` invisibles para `status` sin `--ignored`, vistos por el barrido (`IGNORED_CHANGED`); commit del Worker en su worktree → 14 archivos cambiados en el `.git` compartido, principal `UNCHANGED`; dos fotografías seguidas → `NONE`; edición del Owner → `STOP` (no atribuible) |
| **e** límites | ¿Dónde falla C1? | crear y borrar → solo `DIR_ENTRY_CHURN` (fecha del directorio); mismo tamaño + fecha restaurada → `git status` limpio; lo ven el hash de todos los rastreados y `ChangeTime`; granularidad: 72/200 pares con la misma `LastWriteTime` (94/200 en una corrida anterior); coste: 4 304 archivos / 499 directorios / 245 MB del worktree de I-62 barridos en 159 ms; SHA-256 384 MB/s |
| **f** canarios y negativos | ¿Discriminan los canarios y el validador? | K-C1 PASS (4,4 s con el fixture); NC-C1a (entrada mutada) PASS; NC-C1b (checkout ausente) PASS; NC-C1c (rol no derivado) PASS; K-C2: `cwd` worktree PASS, `cwd` principal `STOP_BEFORE_INVOCATION` |

Iteraciones de desarrollo (antes de la corrida canónica): se corrigieron fallos del propio arnés (aplanado de matrices al sanear, lista vacía
devuelta como nulo, propiedades ausentes con `StrictMode`, CR añadido por la tubería de PowerShell a `git check-ignore --stdin`, detección del
`NOT_FOUND` de Bash tras una tubería) y se amplió E4 de 20 a 200 pares. Los resultados canónicos son los de `results/`.

## 3. Lo que NO se probó

- Ningún gancho instalado en una sesión real: la semántica «entrada JSON por stdin, salida 2 = bloqueo, stderr al modelo» y el campo `cwd` de la
  entrada del gancho son INFERENCE (simulados con `c3-guard.ps1`).
- El campo `cwd` por entrada de las transcripciones de subagentes no se inspeccionó (no se leyeron transcripciones ajenas): INFERENCE.
- Ningún Worker, subagente, Codex, Claude CLI ni otro modelo; ninguna red.
- El checkout principal real y los worktrees reales no se fotografiaron; no se midió cuántos checkouts tiene el repositorio. Solo se enumeraron
  metadatos del worktree de I-62 (sin Git) para el coste.
- Ninguna compilación real (`bin/`/`obj/` simulados); ningún Python real (solo resolución y un intérprete falso `.cmd`); el alias real por ruta
  absoluta no se ejecutó.
- Ni ACL, ni desactivación de alias de ejecución de aplicaciones, ni auditoría de procesos (ajustes del sistema).
- Windows PowerShell 5.1 y `cmd.exe` como shell del Worker no se probaron (sí `pwsh` 7.6.6 y Git Bash).
- Concurrencia con una sesión hermana realmente activa (simulada declarando el rol).
- NC-C2b, NC-C3e y NC-C3f: solo diseño.
- Rendimiento bajo la carga de I-64 (se evitó cargar la CPU).

## 4. Lecturas y escrituras de esta tarea

- **Escrituras:** solo `D:\r62-c1c3\` y esta carpeta.
- **Lecturas:** worktree de I-62 (documentos, esquemas, decisiones §77-§81; `git --no-optional-locks rev-parse/log/status`, que leen el
  repositorio compartido); worktree de I-65 (paquete de escalamiento, propuesta previa, `incident-main-checkout/`, decisiones §27-§39;
  `git --no-optional-locks rev-parse`). El árbol de trabajo del checkout principal no se leyó ni se escribió.

## 5. Archivos entregados (SHA-256)

Este README no se incluye en su propia lista.

| Archivo | SHA-256 | Bytes |
|---|---|---|
| c1c3-design.md | `3aa1cbedec74b547a3baafb8c1e35cc1278fb5581d1ba7f514b543712c46c68c` | 47555 |
| c1c3-amendment-proposal.md | `7bbd1dbf3d4a5d600fad5a6ab4674c1549c10355919f50336dc62deaad3b0b7b` | 8918 |
| experiments/run-all.ps1 | `9cd2bdb917186a9499658ab58ad1741c301201f16c322f0384cf426783b9f1b9` | 2822 |
| experiments/lib/c1c3-lib.ps1 | `ee992655f6c4d6874978fb9e1809e6cc42d65d50da7b167b7c0087c1d8264ebe` | 20492 |
| experiments/worker-call-relative.ps1 | `82e4c0cf1f589c7ae172d6d2015ece5cc16f0a4503198137e04f433af46e9111` | 2793 |
| experiments/c3-guard.ps1 | `f8bcecbb97c819733191516db2dac5a09c56d6a4634e6503d0c2bcf083a51de5` | 5044 |
| experiments/exp-a-relative-write.ps1 | `c64437ba9d4786d6a28f470050b818812ed3458b4ef1a53fa2896a12c7822d79` | 7044 |
| experiments/exp-b-sibling-write.ps1 | `2739025252c9b7d198e9855308b3992c902ac664e6e554e531928c47b47743f7` | 4925 |
| experiments/exp-c-interpreter.ps1 | `899de1ca26c3b85dd399728c35753fc9d2e5850d4383c9871ced32caccc23f8d` | 18545 |
| experiments/exp-d-false-positives.ps1 | `807083534fc16979ed8956b456ae6dba2f8c0e31d55d84ccb97d78011f739e5b` | 8909 |
| experiments/exp-e-limitations.ps1 | `10393fad8984e4539550a9dee710e22ffde2a30a87c55a7bf47be7b32c2d8354` | 6843 |
| experiments/exp-f-canaries-and-negatives.ps1 | `45ce627cbca722b263dbdac1fd97c26ecde0fda5a72b8d8a2d74c969dbd4a20c` | 6616 |
| experiments/results/exp-a-relative-write.json | `41de8a103c5b924dea557c5fd5d243ae7e6c5cdc3c39ee39c53d9a9c9a96c2d1` | 39349 |
| experiments/results/exp-b-sibling-write.json | `87078e2c126336ac60c8ba866b207bae4f1667116aa86eb8d4324a50e6b989e7` | 4611 |
| experiments/results/exp-c-interpreter.json | `91413c7d2ab96551a77e7098e1a664202104d54b670e009bc4dadb291c0ed08b` | 36040 |
| experiments/results/exp-d-false-positives.json | `004033571672e1e1f99288020dfa1a7b367d988c10c8b36efdac7181b4cb8146` | 12760 |
| experiments/results/exp-e-limitations.json | `edd72ddeace70d7a8c2b7ea4b1edbab5475dc90199b73d1ca5b04777cf6bd314` | 4094 |
| experiments/results/exp-f-canaries-and-negatives.json | `a4d11a242f0358b5a03f06d7a2e6b3a5bd4073b15183955175432c046d8b4108` | 3432 |
| experiments/results/run-manifest.json | `3299c03e65f607cc2037e99a833059af8ec26507c25dc33bc1ed04998b4a110b` | 8097 |
