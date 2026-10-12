# Evidencia con datos reales para R1 y R4 (I-62, A-6 ronda 2)

Banco de pruebas de la supervisión para dos procedimientos candidatos que corrigen los hallazgos A62-A6-R1 y
A62-A6-R4 de la revisión formal de A-6 (`R20261011T215735Z-c434`). Aquí solo hay pruebas y resultados: este
paquete no redacta ni edita el texto de la enmienda. No se invocó ningún modelo ni CLI de modelo, no hubo red,
y no se leyó `auth.json`, credenciales ni `config.toml`.

## 1. Entorno exacto

| Elemento | Valor |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300 (`Windows-11-10.0.26300-SP0`) |
| Git | `git version 2.54.0.windows.1` |
| Python | 3.13.14 (CPython, siempre con `-I`) |
| PowerShell 7 (el shell del Controller) | 7.6.6 (`pwsh` del PATH) |
| Windows PowerShell (negativo) | 5.1.26100.9549 |
| cmd.exe | 10.0.26300.9550 |
| Instrumento fijado | `bmin_facts.py`, blob `9a829b0eeb1210238052b1feac06cfa8f7964aed`, SHA-256 `b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200`, 64 764 bytes |
| Aceptante fijado | `bmin_accept.py`, blob `434ef1fccb0d15762b30244b4a60928120698411`, SHA-256 `173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0`, 111 873 bytes |

Los bytes del instrumento y del aceptante se obtienen solo con `git --no-replace-objects -C <worktree de I-62> cat-file blob <blob>`.
El banco recalcula su SHA-1 de blob y se detiene si no coincide. Las copias quedan en `D:\r62-a6r2\r1r4\pin\` y no
se modifican. El origen del fixture (`D:/r62-fixture/fixture-origin.git`) solo se usa como fuente de
`git clone -q --no-hardlinks -c core.autocrlf=false -b <rama>` y de `git ls-remote` (lectura) para los hechos
del relevo. Todo lo demás se escribe en `D:\r62-a6r2\r1r4\` y en esta carpeta.

Escenario FX-U1, el mismo de `bmin/e2e/e2e.py`: BASE `1a4fc9c6`, RED `d30fb6a9`, HEADX `ae25b596`
(CurrentSha de la entrega), MAIN = EFF `fbe25347`, TIP `95bdc29d` (HEAD de `fx/u1`). Se ejecutan las mismas
13 órdenes con las mismas revisiones. Diferencias respecto a e2e.py:

- `--repo` = el T de cada caso.
- DELEG = una `rackcad-delegation/v2` derivada del contrato K. e2e.py usaba K como DELEG.
- La entrega cumple `worker-handoff` (`TestsExecuted` y `TestResults` como arrays, `WorkerStatus`
  `IMPLEMENTATION_COMPLETE`, `Worker` completo). El texto libre es el mismo, con «GATE PASS» a propósito.
  `Worktree` = T.
- `Tool.RunFrom = object` con `GitObject.GitDir = T`, sin `Tool.Path`.
- `NotApplicable` va vacío en las pruebas R4.

## 2. Cómo reproducir

```powershell
python -I run_r1.py         # P1, P2, P3 y ataques (a)-(h)      -> r1-p1-p2-p3.json, r1-attacks.json
python -I run_r1_extra.py   # ataque extra (i): commit-graph    -> r1-attacks-extra.json
python -I run_r4.py         # pruebas R4 1-8                     -> r4-t1.json, r4-t3.json, r4-t6.json, r4-t2.json
python -I summarize.py      # vista compacta; --hashes lista el SHA-256 de cada archivo
```

Las órdenes se ejecutan en secuencia, nunca en paralelo. Los resultados son JSON canónico ASCII (claves
ordenadas, separadores `,` `:`, terminado en LF) y pasan por un saneado: el prefijo de USERPROFILE se escribe
`%USERPROFILE%`, el nombre de la cuenta y su forma corta se escriben `<usuario>`, y las direcciones de correo
se escriben `<email>`. Los sobres del instrumento no se copian aquí porque llevan direcciones de correo en
`CoAuthoredBy`. Se citan por SHA-256 y `FactsSha256`, y quedan en `D:\r62-a6r2\r1r4\`. `inputs-t1/` contiene
copias byte a byte de la solicitud, la salida del Controller, la delegación, la entrega y los hechos del relevo
de la prueba R4 sobre T1.

## 3. R1: identidad del objeto Git efectivo en el momento de la ejecución

Evidencia: `r1-p1-p2-p3.json` (P1, P2 y P3 sobre T0 = `D:/r62-a6r2/r1r4/r1/t0`), `r1-attacks.json` (a-h) y
`r1-attacks-extra.json` (i). Cada ataque usa un clon nuevo con su propia P1 y una línea base de sobres P3
tomada antes de plantar el ataque.

### 3.1 P1, P2 y P3

| Prueba | Orden | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| P1 | `git --no-replace-objects -C <wt> cat-file blob B` y después `git -C T hash-object -w --no-filters --stdin` | id = B. La instantánea (refs con `refs/replace`, HEAD, SHA-256 del índice, `status --porcelain=v1 -z --ignored`, archivos del árbol, archivos del git dir) es idéntica salvo el objeto suelto nuevo | id `9a829b0e…`. Única diferencia: se añade `objects/9a/829b0eeb1210238052b1feac06cfa8f7964aed`. Se registran repo, hora UTC e id | sí |
| P2 con pwsh 7 | pasos (1)-(4), el paso (3) en `pwsh -Command` con cwd = T | lanzamiento | `Launch: true`. (3): `blob` y `9a829b0e…`. (4): fsck sale con 0 y no escribe nada | sí |
| P2 con cmd.exe | lo mismo con `cmd /d /s /c` | lanzamiento | `Launch: true` | sí |
| P2 con PS 5.1 (negativo) | lo mismo con `powershell.exe -Command` | sin lanzamiento en (3) | `FailedSteps: ["3"]`. El tubo entrega 66 378 bytes (se añaden 1 614 CR), blob `7ea57452…` ≠ B | sí |
| P3 | `git --no-replace-objects cat-file blob B \| python -I - <orden> --repo T …` con pwsh y con cmd, frente a `python -I <copia>` | sobres idénticos byte a byte | 13/13 idénticos con pwsh y 13/13 con cmd. Ninguna escritura en T durante todas las corridas | sí |
| P3 con PS 5.1 (nota) | `identity` y `diff` | — | los sobres también son idénticos byte a byte aunque los bytes del instrumento cambiaron, porque Python acepta el fuente con CRLF. Comparar sobres no prueba que los bytes sean los fijados; lo prueba el paso (3) | — |

### 3.2 Ataques (cada uno sobre una copia nueva de T)

| Ataque | Git normal | Instrumento | P2 | Aceptante (`RunFrom=object`, `GitDir`=T) | ¿Se cumple? |
|---|---|---|---|---|---|
| (a) `refs/replace/B` → blob sustituto (herramienta falsa, inofensiva, `0594de72…`) | `cat-file blob B` entrega el sustituto. Con `--no-replace-objects` entrega el original. `cat-file -e B` sale con 0 y `hash-object -w` vuelve a dar B, así que la comprobación de la ronda 1 (en la escritura) pasa | por el camino sin `--no-replace-objects` se ejecuta el sustituto (hechos fabricados, `GitCalls` vacío). Por P3 se ejecuta el fijado (idéntico a la línea base) | falla solo en (1) | sobres fabricados: REJECT_FACTS, con R1 MATCH (lee el original con `GIT_NO_REPLACE_OBJECTS=1`), R3 MISMATCH (la reejecución difiere) y R5 MISMATCH. Sobres P3: ACCEPT_FACTS | sí |
| (b1) `refs/replace/HEADX` → commit fabricado (árbol de RED) | `diff --name-only BASE..HEADX`: 2 rutas; con `--no-replace-objects`: 5 | cambian `diff.Paths` y `scope.Paths/PerPath`. Con `GIT_NO_REPLACE_OBJECTS=1` en el entorno del shell cambian igual, porque la herramienta descarta `GIT_*` | falla solo en (1) | REJECT_FACTS: R5 y R7 MISMATCH, R3 **MATCH** (la reejecución ve la misma historia sustituida) | sí |
| (b2) `refs/replace/EFF` → merge fabricado con un tráiler `Agent-Protocol-Normative: I-62` de más | — | `clausemap` falla cerrado: `AMBIGUOUS` («trailer line found in 2 commits») | falla solo en (1) | REJECT_FACTS: R3, R6 y R7 MISMATCH | sí |
| (c) `info/grafts` (HEADX colgado de BASE) | `rev-list BASE..HEADX` = [HEADX] tanto normal como con `--no-replace-objects` y con `GIT_NO_REPLACE_OBJECTS=1`: los grafts **no** se desactivan | cambian `identity.Ancestry` y `diff.Commits/CoAuthoredBy` | falla solo en (2) | REJECT_FACTS: R5 MISMATCH (recorrido propio sobre objetos crudos), R3 MATCH | sí |
| (d1) `objects/info/alternates` hacia otro clon (contenido benigno) | — | hechos sin cambio | falla solo en (2) | — | sí |
| (d2, extra) alternate con un pack hecho a mano cuyo índice asigna B a otros bytes | si P1 se hizo antes, gana el objeto suelto local y git entrega el fijado. Si el alternate ya estaba antes de P1, git entrega los bytes falsos (1 349 bytes, `0594de72…`) con el id B | si el alternate ya estaba antes de P1, P3 ejecuta el falso | después de P1: falla en (2) y (4) (fsck: «packed 9a829b… is corrupt»). Antes de P1: falla en (2), (3) y (4) | — | sí. Además, si el alternate ya estaba antes de P1, **P1 devuelve B pero no escribe nada**: solo lo delata la comprobación de que aparece exactamente un objeto suelto nuevo |
| (e1) archivo suelto de B recomprimido (zlib nivel 9) | entrega el fijado (mismo objeto, otros bytes de archivo) | — | pasa (benigno) | — | sí |
| (e2) archivo suelto de B sustituido por zlib(cabecera + bytes falsos) | `cat-file blob B` entrega los bytes falsos: git no verifica el hash al leer | P3 ejecuta el falso | falla en (3) y (4) | REJECT_FACTS: R1 MISMATCH (`ObjectBlob 0594de72…` ≠ fijado) y la herramienta no se ejecuta | sí |
| (f) archivo `shallow` con RED | `is-shallow-repository` = true y `merge-base --is-ancestor BASE RED` sale con 1 | cambia `identity.Ancestry` | falla solo en (2) | REJECT_FACTS: R5 MISMATCH | sí |
| (g) secuencia PLAN → cesión del Worker → VERIFY | el Worker (simulado) escribe `refs/replace/B` | — | P2 antes de PLAN: pasa. Después de la Entrada del Worker, la comprobación en la escritura todavía pasa, pero P2 antes de VERIFY falla en (1): sin lanzamiento | — | sí |
| (h, extra) `git.bat` y `fake.py` plantados en la raíz del árbol de trabajo | con `NoDefaultCurrentDirectoryInExePath=1` (heredada en este host), cmd usa el git real. Sin la variable, cmd ejecuta `.\git.bat` desde el cwd = T | sin la variable, P3 con cmd ejecuta el falso. Con pwsh, P3 es idéntico a la línea base en las dos variantes | sin la variable, P2 con cmd **da Launch true** mientras el tubo entrega los bytes falsos; solo el digest del consumidor (extra) lo ve. Con pwsh, P2 es correcto | sin la variable, sobres de cmd: REJECT_FACTS | sí |
| (i, extra) `objects/info/commit-graph` reescrito (primer padre de HEADX → BASE) | `log` y `merge-base` siguen al grafo (`merge-base --is-ancestor RED HEADX` sale con 1). Con `core.commitGraph=false`, la historia original. `--no-replace-objects` no ayuda. En este caso `rev-list BASE..HEADX` no cambió | cambia `identity.Ancestry` (`red->current` false) | (1)-(3) pasan; **solo (4) lo detecta** (fsck: «commit-graph parent for ae25… is 1a4fc… != d30fb…») | REJECT_FACTS: R5 MISMATCH | sí |

En los ataques, el aceptante recibe solo las corridas del ataque, sin salida del Controller. Por eso se
declaran R4 (y R6/R7 cuando no hay corridas que los alimenten) en `NotApplicable`. Es andamiaje de la prueba de
R1, no del procedimiento R4.

## 4. R4: dónde y cuándo se ejecuta el aceptante

La solicitud usa `Repo` = T y `Tool = {PinnedBlob: B, RunFrom: object, GitObject: {GitDir: T, Blob: B}}`.
`Runs` se reconstruye desde el registro de sesión del Controller (simulado: JSONL con la línea de órdenes, el
código de salida y el stdout en base64). Solo se aceptan entradas que usaron el camino declarado P3. El
aceptante se ejecuta después de la Entrada de la invocación VERIFY y antes de cualquier escritura posterior en
T, con la sesión retenida. La supervisión lo ejecuta en solo lectura y confirma que su árbol de procesos murió
antes de liberar la retención.

| # | Prueba | Esperado | Observado | ¿Se cumple? | Evidencia |
|---|---|---|---|---|---|
| 1 | T1 = clon `--no-hardlinks` nuevo. T2 = `git worktree add T2 fx/u1` sobre otro clon C2 (con `main` en su checkout) | HEAD = TIP | T1 y T2 en `95bdc29d`. Git dir de T2 = `C2/.git/worktrees/t2wt`, common dir = `C2/.git` | sí | `r4-t1.json` Test1, `r4-t2.json` Test1 |
| 2 | P1 sobre T y las 13 órdenes por P3 (pwsh, cwd = T), registradas | 13 sobres sin error. Cero escrituras | 13 corridas recuperadas, todas por el camino declarado. Instantánea igual antes y después. Procesos sobre T en la Salida y la Entrada: ninguno | sí | `r4-t1.json` Pipeline |
| 3 | salida controller-verification/v2 coherente con los hechos (más delegación v2, entrega, hechos del relevo relay-record/v2 y cierre), validadas contra los esquemas | ACCEPT_FACTS con R4 MATCH | **ACCEPT_FACTS** (rc 0). R1, R2, R3, R4, R5, R6, R7, R9 y R10 MATCH; R8 MANUAL; **ningún NOT_APPLICABLE**. Filas de R4: Identity fail/«not pass», Scope fail/«not pass», CleanTree pass/pass, Trailer fail/«not pass», FreeText fail/«not pass», Authority fail/«not pass» (14 defectos del mapa). R6 también compara las 6 filas de `AuthorityResolution` (EFF `fbe25347`, mapa `4d49d3e1`) | sí | `r4-t1.json` Test3, `inputs-t1/` |
| 4 | se invierte un resultado: CleanTree pass→fail. Variante: Identity fail→pass | R4 MISMATCH y REJECT_FACTS | en las dos, REJECT_FACTS con **solo** R4 MISMATCH | sí | `r4-t1.json` Test4 |
| 5a | la misma solicitud con `Repo` = `GitDir` = T3 (otro clon, mismos SHA). Las corridas siguen con `--repo T1` | — (documentar) | REJECT_FACTS: R5, R6 y R7 MISMATCH («USAGE run --repo differs from the request Repo»). R3 MATCH porque la reejecución sigue el `--repo T1` del argv y vuelve a leer T1, no T3. R4 MATCH | se documenta el fallo de «un clon del fixture» | `r4-t3.json` Test5a |
| 5b | lo mismo, con el argv reescrito a `--repo T3` y los sobres de T1 | — (documentar) | REJECT_FACTS: R3 MISMATCH (`Args`, y en `identity` los `Facts`) y R5 MISMATCH (`Toplevel`). R6 y R7 MATCH: esos hechos no dependen de la ruta | se documenta | `r4-t3.json` Test5b |
| 6a | commit de sesión en T entre las corridas y el aceptante | — (documentar) | REJECT_FACTS: R3 (`identity.Head`) y R5 (`Head`) MISMATCH. R4 MATCH, porque Identity ya era fail y el árbol sigue limpio | se documenta la ventana | `r4-t6.json` t6a |
| 6b | archivo versionado modificado en T entre las corridas y el aceptante | — (documentar) | REJECT_FACTS: R3 (reejecución de `cleantree` y `hashes`), **R4** (CleanTree: el Controller dice pass y el aceptante «not pass») y R5 (`hashes.PerPath`) MISMATCH | se documenta la ventana | `r4-t6.json` t6b |
| 7 | cero escrituras y procesos | instantánea de T igual antes y después del aceptante, y ningún git/python con T en su línea de órdenes al volver | instantáneas iguales (incluido todo el git dir). 0 procesos con T. 0 hijos vivos del PID del aceptante. Liberación registrada | sí | `r4-t1.json` Test7 |
| 8 | 2-4 y 7 sobre el worktree enlazado T2 | lo mismo que en T1 | P1 escribe en `C2/.git/objects`. P2 (2) usa el common dir `C2/.git`. ACCEPT_FACTS sin NOT_APPLICABLE. La inversión de CleanTree da solo R4 MISMATCH. Cero escrituras y ningún proceso | sí | `r4-t2.json` |

**NOT_APPLICABLE:** ninguno en las pruebas R4. Con la salida del Controller, la delegación, la entrega, el cierre,
los hechos del relevo y las 13 corridas, el aceptante tiene datos para todas sus comprobaciones. R8 es MANUAL
por diseño (texto libre, AP 16.10).

## 5. Observaciones medidas que importan para la redacción

1. **`--no-replace-objects` en la orden de ejecución solo protege los bytes del instrumento, no sus hechos.**
   El instrumento elimina `GIT_*` en sus propias llamadas. Por eso los reemplazos de commits (b1, b2), los
   grafts (c), el shallow (f) y el commit-graph (i) cambian sus hechos aunque P3 sea correcto.
   `GIT_NO_REPLACE_OBJECTS=1` en el entorno no llega a esas llamadas (b1). Las comprobaciones de (1)/(2) de P2
   no son una alternativa a `--no-replace-objects`: cubren otra cosa.
2. En git 2.54, `--no-replace-objects` y `GIT_NO_REPLACE_OBJECTS` **no desactivan los grafts** (c).
3. **La comprobación en la escritura** (la regla 1 de la ronda 1) sigue pasando después de una escritura del
   Worker (a, g). También pasa cuando un alternate ya aporta B: P1 devuelve B sin escribir nada (d2). Solo la
   comprobación de que aparece exactamente un objeto suelto nuevo lo delata.
4. **Comparar sobres no prueba la identidad de los bytes.** Con PS 5.1, los sobres son idénticos a pesar de los
   1 614 CR añadidos. El paso (3) tiene que hashear lo que entrega el camino.
5. **El shell importa.** cmd.exe resuelve órdenes desde el directorio actual si no está
   `NoDefaultCurrentDirectoryInExePath`. Con cwd = worktree del sistema bajo prueba, un `git.bat` del Worker
   engaña a P2 (3) y a P3 a la vez (h). pwsh no resuelve desde el cwd.
6. **Git no verifica el hash al leer** un objeto suelto ni un pack de un alternate (e2, d2). Solo fsck (paso 4)
   lo ve. Un commit-graph manipulado (i) solo lo detecta fsck; los pasos (1)-(3) no están pensados para él.
7. **Aceptante.** Gracias a `GIT_NO_REPLACE_OBJECTS=1`, al recorrido propio sobre objetos crudos y al rehash,
   rechazó todos los conjuntos atacados. Pero su reejecución (R3) no detecta los ataques a la historia, porque
   la herramienta reejecutada ve la misma historia; los detecta R5.
8. **R4.** `Repo` tiene que ser el `--repo` de cada corrida. La reejecución sigue el `--repo` del argv, no el
   `Repo` de la solicitud (5a). Sobre otro clon, los hechos que dependen de la ruta (`Toplevel`, `Args`) fallan
   siempre (5b). Un cambio de estado entre las corridas y el aceptante se detecta (6a, 6b). Solo un cambio del
   árbol de trabajo afecta a R4 en sí (6b); un commit de sesión lo delatan R3 y R5 (6a).
9. **Procesos.** En casi todas las consultas apareció un `git.exe` de vida corta cuya línea de órdenes no se pudo
   leer. Cada vez tenía otro PID y era hijo de un `python3.13.exe` cuya línea de órdenes no contiene T. Se
   registran en `UnreadableClosedList`, con padre, abuelo y hora, sin atribuirlos ni clasificarlos. En el host
   corría a la vez otra sesión de banco que lanza procesos python → git. En una sonda previa, fuera de los
   archivos de evidencia, se vio lo mismo con un `git.exe` hijo del propio banco, capturado al crearse. Una
   prueba que solo mira la línea de órdenes puede no ver a un participante en el instante en que se crea.

## 6. Lo que no queda demostrado

- **No hay Controller ni Worker reales.** El registro de sesión es un JSONL del banco. No se ejercitan el
  formato real de eventos `--json` de Codex, su captura de stdout (truncado, codificación) ni la recuperación
  del argv desde ese registro. La escritura «del Worker» de (g) también es simulada.
- **El pwsh del runtime de Codex** (delante en el PATH del proceso hijo, receta 16.4) no se probó. Se usó pwsh
  7.6.6 del PATH. Tampoco se probaron otras versiones de pwsh: que el tubo nativo conserve los bytes depende de
  la versión.
- **La búsqueda del ejecutable `git` por los procesos hijos del instrumento y del aceptante.** Los dos corren con
  cwd = T, y CreateProcess busca en el directorio actual del padre. No se probó un `git.exe` plantado en el
  árbol, porque exigiría ejecutar un binario plantado. Tampoco se midió el efecto de
  `NoDefaultCurrentDirectoryInExePath` sobre CreateProcess.
- **Las variables de entorno del Controller** (`GIT_ALTERNATE_OBJECT_DIRECTORIES`, `GIT_OBJECT_DIRECTORY`,
  `GIT_DIR`, `GIT_REPLACE_REF_BASE`, `GIT_GRAFT_FILE`, `GIT_CONFIG_*`) no se probaron. Los pasos (1)/(2) de P2
  solo miran archivos. El `git cat-file` de P3 corre en ese entorno; el instrumento elimina `GIT_*`.
- **La configuración de T** (`.git/config`: `include.path`, `core.useReplaceRefs`, `extensions.*`, …) y el
  multi-pack-index no están cubiertos por P2 (2) ni se probaron. Solo se probó el commit-graph (i).
- **La clasificación completa de procesos de AP 16.4** (huérfanos, descendientes de la sesión, reutilización
  de PID, ilegibles de la lista cerrada) no se aplica. La consulta del banco es más estrecha: líneas de órdenes
  legibles que contienen T e hijos del PID del aceptante.
- **La huella de `config.toml` no se lee** (límite de esta tarea). Por eso `Fingerprint.Kind` = `NONE` en los
  hechos del relevo. La `DecisionRef` de P1 es un marcador del banco, y la relación con W-2 no se ejercita.
- **La ruta VERIFIED.** El mapa del fixture tiene defectos (MV-4), así que Authority no puede pasar. No se
  ejercitó una verificación con las 14 comprobaciones en pass. Las 8 comprobaciones que no son de R4 van en
  `not_run`. `AuthorityResolution.Mode/RevisionSha` son valores del banco, no derivados de E.3.1.
- **La retención de la sesión** se simula con un banco secuencial. No hubo ningún escritor concurrente real
  sobre T.
- **Un solo host, NTFS y una versión de git.** No se comprobó si un clon local `--no-hardlinks` copia
  `objects/info/alternates` u otros archivos de control del origen.

## 7. Archivos (SHA-256)

El SHA-256 de este README se da fuera de él.

| SHA-256 | Archivo |
|---|---|
| `c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5` | `lib_r1r4.py` |
| `8f87b7e2bde32c613c2c74a797f12da85c50974fe0930c41f6b85b822157687f` | `r1-attacks-extra.json` |
| `c530085afcc9709802186ac9977a8cfd2f3eb96cd86a34e04592e906470083f5` | `r1-attacks.json` |
| `5d4628d3e794b19173dee23efe4803af03a824cd2253500a8813486cde6a451a` | `r1-p1-p2-p3.json` |
| `6c550c15c5802c3019ef9502b57a020d9d72eb223991ac6e975906fdd607817e` | `r4-t1.json` |
| `837de3fa383360d9b3455aa7e9e7bb7dc2fbb629b1b359124976b97931a4e799` | `r4-t2.json` |
| `71333dd569ea630dd769bf45337ba90c754221c92592aaa8d65b9f7f9748456e` | `r4-t3.json` |
| `19a9d5bbb898001f2fbe491cf63397a213f6c9ce6906da6ac29538df914570f7` | `r4-t6.json` |
| `d8f85fc6256e009eb925a85a1ba87f98ad70afa2b08eb84f2379cdc394a4b784` | `run_r1.py` |
| `fccddab2d7be33f8d9f7555b4c8a0919c1cac39e454ff79fa1d88cba9f0fc0a8` | `run_r1_extra.py` |
| `987147556189ee7cd1287cbe67b30670a1e8932bd1691a0c9d5922d481c76bff` | `run_r4.py` |
| `e116b5be86619c4efc0b340bb13791755ec24ddedc09031601b7b072a838aaf1` | `summarize.py` |
| `5c14bd5cc46c27e3ccebef1beb2b851d256a5bddadcd67fba9d24c585ab436f5` | `inputs-t1/controller-output.json` |
| `5336bfa120b978915a5dcf7193a914b71f8db23897a22448357a2b0ce7d14192` | `inputs-t1/deleg.json` |
| `f1d05e5087fd805df5ab4a3ec3d1aa4e93ea10aadcd9c1fc13c6ffc52cbaafc8` | `inputs-t1/handoff.json` |
| `eba9cfb8f55f5a4301b6c32d01e434dc877566a95239ddb2afa3a03872ad3fdb` | `inputs-t1/relay-facts.json` |
| `8435dbff7b83301863acde93182aab759b52c171e52620de6879c94a4ccd8423` | `inputs-t1/request.json` |
