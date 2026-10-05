I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA A-1 CORREGIDA PARA A62-A1T-01 Y A62-A1T-O1..O3 (orden nueva del Coordinator, decisiones §42, punto E). ÚNICA invocación que permite la orden, sin reintento automático.

Eres el ARCHITECT de esta revisión, en una sesión nueva y separada de la sesión autora de A-1. Revisas en solo lectura la enmienda exacta y devuelves un
único veredicto formal de LIFECYCLE §6. No implementas nada, no editas el repositorio, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261005T151303Z-c64c
InvocationId           = I20261005T151303Z-c64c
LogicalReviewRequestId = L20261005T151303Z-c64c   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a1t01   (su única rama, main, está en el commit exacto; sin remoto; no lo modifiques)
Commit                 = ca09ade8bb31b1ecb57b2b0d6220628c8434e78d   (CI de publicación 37329298556, push, 4/4 success: señal de salud, no prueba local)
Objeto                 = docs/initiatives/I-62-A-1.md                                blob c01899a72b940503bb85a0fab42bc085c603fd0f
Paquete                = docs/initiatives/I-62-architect-package-A-1.md              blob 5d6a107cb2bdafc0a98a87f5f66aeb115ed47b4b
Disposición            = docs/initiatives/I-62-architect-review-A-1-disposition.md   blob 81a84996a7cec1398785ae3df31618e9abd616ab
Blob revisado antes    = 03dd822d1310a0ce298eba6da2321383aa5e4887 (commit 411e01ce4176e4fe6648ea3e9ff2febc219ff438, presente en el clon)
Freeze                 = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; Proposal V14 commit 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Orden vigente          = D:\r62-arch-a1t01-run\order.txt   (orden nueva del Coordinator; 13 157 bytes, SHA-256 588d62896bfc4a96b5451474cc991cb66c12c8342a33041df4fbb34442741c4d)
```

El objeto, el commit y el número de invocaciones los fijan este prompt y la orden vigente (`order.txt` §1, §7, §8 y §9).

## Paso 0 — identidad, antes de cualquier lectura sustantiva

1. `git -C D:/r62-arch-a1t01 rev-parse HEAD` = el commit. Después, `git -C D:/r62-arch-a1t01 rev-parse HEAD:docs/initiatives/I-62-A-1.md`, y lo mismo
   para el paquete y la disposición, deben dar los blobs. Por último, `git -C D:/r62-arch-a1t01 status --porcelain` debe salir vacío.
2. En tu propio directorio de trabajo (el worktree de la tarea), `git rev-parse HEAD` debe ser el commit. Si no lo es, ejecuta una sola vez
   `git checkout --detach ca09ade8bb31b1ecb57b2b0d6220628c8434e78d` en él y vuelve a comprobarlo.
3. `sha256sum D:/r62-arch-a1t01-run/order.txt D:/r62-arch-a1t01-run/prompt.md`: el primero debe dar el SHA-256 de arriba. Después lee **entera**, con
   Read, `order.txt` (la autoridad vigente).

Si algo no coincide y el punto 2 no lo corrige, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto; rutas relativas al clon, que también puedes leer en tu worktree porque está en el mismo commit)

**Canónicos:**
1. `docs/initiatives/I-62-A-1.md`, el objeto, **entero** (77 984 bytes y 437 líneas: dos lecturas de ≤ 300 líneas). Secciones: §1 58-89; §2 FC-01 90-168
   (delta exacto 139-168; D1-17 y D1-17 (cont.) en 159-160); §3 FC-02 169-228 (delta exacto 205-228; D2-2 y D2-2 (cont.) en 210-211); §4 229-255;
   §5 256-280; §6 281-287; §7 288-303; §8 304-319; §9 320-353; §10 354-428 («Frente a `03dd822d`» al principio); §11 429-437.
2. **El delta explícito:** `git -C D:/r62-arch-a1t01 diff 411e01ce ca09ade8 -- docs/initiatives/I-62-A-1.md` (acción MD-1). Es solo orientativo: las
   premisas se citan de líneas leídas del objeto, no del diff.
3. `docs/initiatives/I-62-architect-package-A-1.md` (217 líneas) y `docs/initiatives/I-62-architect-review-A-1-disposition.md` (164 líneas; §5 = la
   disposición del Coordinator sobre A62-A1T-01 y O1..O3).
4. Las revisiones formales anteriores, registro y resultado literal; su acreditación la decide el Coordinator, no la des por hecha ni la niegues:
   - `docs/initiatives/I-62-architect-review-A-1-r4.md` y `docs/automation/evidence/I-62-architect-A-1/R20261005T073911Z-2dfe/output.json`
     (A62-A1T-01, A62-A1T-O1..O3 y los doce cierres), con `.../R20261005T073911Z-2dfe/audit-classification.json` (su NOT_ACCREDITED, conservado);
   - `docs/initiatives/I-62-architect-review-A-1-r3.md` y `.../R20261005T063359Z-86e3/output.json` (A62-A1S-01..02);
   - `docs/initiatives/I-62-architect-review-A-1-r2.md` y `.../R20261005T044948Z-ac67/output.json` (A62-A1R-01..03);
   - `docs/initiatives/I-62-architect-review-A-1.md`, la corrida no acreditada R20261003T023945Z-cab7: solo insumo técnico histórico.
5. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes; solo por rangos de ≤ 300 líneas). Secciones (líneas): §3.1 178-203; §8.8 497-556;
   §8.9 557-592; §9 593-664; §11.3 704-720; §13 769-794; §14 795-882; §18 952-978; §20.5 1403-1521; §20.5.2 1522-1544; §20.6 1545-1611;
   §20.7 1612-1641; §20.8 1642-1655; B.2 1815-1836; B.7 1901-1909; B.8.1 1915-1975; B.8.4 2006-2053; B.8.6 2092-2109; B.8.7 2110-2127;
   B.8.8 2128-2222; B.9 2223-2253; B.10 2254-2307; Anexo C 2330-2383; F.6-F.8 3153-3246; G.1 3247-3292. Y `docs/initiatives/I-62-consensus-freeze.md`.
6. Evidencia de apoyo, no autoridad:
   - el arnés `docs/automation/evidence/I-62-A1/a1-counterexamples.py` (101 785 bytes; por rangos; `chain` y `resolve` en 115-160; A1-P08 en 400-440;
     trazas `a62-a1-04-*` desde la 1002 y de A62-A1T-01 desde la 1024) y `a1-counterexamples-result.json` (118 trazas);
   - `docs/automation/evidence/I-62-A1/a1t01/`: `red-result.json`, `green-result.json`, `green.diff`, `t8-clean-clone.py` y `t8-result.json`;
   - F4 experimental, en `docs/automation/evidence/I-62-prep/night-2026-10-05/f4-exp/`: `README.md`, `combo_sequences.py`, `combo-result-before.json`,
     `combo-result.json` (históricos) y `combo-result-a1t01.json` (nuevo);
   - `docs/automation/evidence/I-62-prep/night-2026-10-05/portability/README.md` y `reconstruct.py`.
7. `docs/automation/evidence/I-62-prep/freeze-issues.md`, `docs/automation/evidence/I-62-prep/f4-dossier.md` (§4) y
   `docs/automation/evidence/I-62-prep/night-2026-10-05/rebase-map.json` (los registros citan SHAs de antes del rebase del 2026-10-05).
8. `docs/INITIATIVE_LIFECYCLE.md` (§3 66-95, §5 152-192, §6 193-223, §7 224-246, §8 247-275, §9 276-293).
9. `docs/automation/decisions/I-62.md`: §34-§42, líneas 650-810, por rangos. Y `docs/automation/evidence/I-62-evidence.md`: §40-§55, líneas
   1779-2277, por rangos.
10. `docs/initiatives/I-62-portabilidad-coordinador-principal.md` (alcance y no-objetivos).
11. Contratos F3 materializados (inactivos):
    - `docs/AUTOMATION_PLAN.md`: §16 de I-61, líneas 425-845; 16.20-16.24, líneas 846-1105; por rangos;
    - `docs/automation/agent-execution/README.md`: §13-§16, líneas 287-629;
    - los esquemas `docs/automation/agent-execution/schemas/{role-invocation.v1,binding.v1,input-closure.v1,input-fidelity.v1,relay-record.v2,gate-contract.v2,reviewer-result.v1,architect-review-result.v1}.schema.json`.
12. `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` y `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md`.
13. Los archivos del run: `D:\r62-arch-a1t01-run\order.txt` y `D:\r62-arch-a1t01-run\prompt.md`.

**Transitivos permitidos.** Los obligan `CLAUDE.md`, `AGENTS.md` y el Context Pack `documentation-governance`; léelos si tus instrucciones te lo piden:
- `CLAUDE.md`, `AGENTS.md`, `README.md`;
- `docs/HANDOFF.md` (448 804 bytes; por rangos);
- `docs/WORKFLOW.md` (§4.2: rebase al abrir sesión), `docs/ARCHITECTURE.md`, `docs/FOUNDATIONS.md`;
- `docs/ROADMAP.md` (168 867 bytes; por rangos);
- `docs/context-packs/README.md`, `docs/context-packs/documentation-governance.md`;
- `docs/initiatives/README.md` (por rangos), `docs/initiatives/PROMPT_TEMPLATES.md`.

## Contrato de acciones (cerrado y fijado antes del lanzamiento: nada fuera de esta lista)

El invocador audita después cada llamada contra esta misma tabla (auditor v4, probado antes del lanzamiento). Cada ruta del sistema de archivos se
comprueba por su **efecto**: resuelta contra el directorio actual, con `.` y `..` colapsados y los enlaces resueltos. Se clasifica como archivo del
cierre, archivo del run, salida propia, raíz o subdirectorio del clon o del worktree, o fuera. No se compara la cadena literal del comando, ni un
prefijo de texto. El cierre y las acciones no se amplían después de ejecutar.

| Id | Acción permitida |
|---|---|
| ID-1 | `git`, con `-C D:/r62-arch-a1t01`, con `-C <tu worktree>` o desde el directorio del clon o del worktree (raíz o subdirectorio). Subcomandos: `rev-parse HEAD`; `rev-parse <commit>:<ruta del cierre>`; `cat-file -p\|-t\|-s <commit>:<ruta del cierre>`; `status [--porcelain]`; `log --oneline [-N]`; `worktree list`; `config --get <clave>` |
| ID-2 | `git checkout --detach ca09ade8bb31b1ecb57b2b0d6220628c8434e78d` en tu worktree, solo si su HEAD no es el commit |
| MD-1 | Metadatos y hashing: `git hash-object <archivo del cierre o salida propia>` (sin `-w` ni `--stdin`); `git diff [--stat\|--numstat\|--no-color\|--word-diff\|-U<N>] 411e01ce ca09ade8 -- <rutas del cierre>`; `sha256sum` y `wc` sobre archivos del cierre, del run o salidas propias; `pwd`; `date` |
| NAV-1 | `cd` (o `Set-Location` en PowerShell) hacia la raíz o **cualquier subdirectorio** del clon o de tu worktree, o hacia un directorio de tus salidas propias. Cambiar de directorio **no** autoriza leer nada fuera del cierre: las rutas posteriores se resuelven desde ahí y se clasifican por su efecto |
| RD-1 | `git show ca09ade8:<ruta del cierre>` (o `HEAD:<ruta del cierre>`, con el HEAD ya verificado), sola o con tubería a `sed -n`, `head`, `tail`, `wc`, `grep`, `awk`, `sha256sum`, `sort`, `uniq`, `cut`, `tr` o `nl`. La ruta de un objeto Git es una ruta de **árbol** desde la raíz del repositorio: Git no normaliza `..` y rechaza esa ruta |
| RD-2 | `sed -n`, `wc`, `awk`, `grep` (sin `-r`, `-R` ni `--recursive`), `head`, `tail`, `cat`, `sha256sum`, `sort`, `uniq`, `cut`, `tr`, `nl`, `diff`, `cmp`, `echo`, `printf`, `true` y `read`. Solo sobre **archivos** del cierre (bajo el clon o tu worktree), sobre los dos archivos del run o sobre tus salidas propias. Combinables con `\|`, `;`, `&&`, `\|\|` y bucles `for`/`while`. Redirección solo hacia tus salidas propias o `/dev/null`. Ningún directorio como argumento |
| EX-1 | `python D:/r62-arch-a1t01/docs/automation/evidence/I-62-A1/a1-counterexamples.py <archivo en tus salidas propias>`: exactamente un argumento, y debe ser una salida propia. Además, `python` con heredoc, con `-c` o con un script propio, siempre que todas sus rutas literales sean archivos del cierre, archivos del run o salidas propias. Sin listar directorios (`os.listdir`, `os.walk`, `glob`, `iterdir`), sin procesos (`subprocess`, `os.system`), sin red y sin borrar ni mover archivos |
| EX-2 | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` dentro de `D:\r62-arch-a1t01`, desde Bash o PowerShell (SDK en `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`). Cualquier otra ruta que pases, como el log, `--results-directory` o `--logger`, debe estar en tus salidas propias. La orden no trae exención: si sigues el paso 4 de «Leer primero» de AGENTS.md, ejecútalo aquí. Es una comprobación local tuya, no evidencia de gate |
| OWN-1 | Leer (Read, `cat`, `tail`, `grep`, `Get-Content`) tus **salidas propias**: archivos que tú mismo creas en tu scratchpad o en tu directorio de tareas |
| OWN-2 | Crear y editar scripts y archivos temporales propios **solo** en tus salidas propias: Write, Edit, redirección, `mkdir -p` y `sed -i` (declarado aquí). **Ningún** permiso sobre salidas propias autoriza editar los insumos canónicos, los scripts custodiados, las fuentes del repositorio ni archivos de otra sesión |

Variables de shell: solo por asignación simple (`X=valor`), sin `export`.

**Lecturas fallidas.** Un comando que falla o no devuelve contenido es una lectura **fallida**: no acredita ningún contenido y no puede sostener una
premisa. Eso incluye un `git show` que Git rechaza: que el auditor normalice la ruta para saber a qué apuntaba no la convierte en una lectura exitosa.
Un intento de leer fuera del cierre sigue fuera del contrato aunque falle.

**Herramientas:**
- **Read**: solo archivos del cierre, los dos archivos del run y tus salidas propias.
- **Grep**: solo con `path` = un **archivo** del cierre, sin `glob`. Nunca uses un directorio, ni siquiera para contar.
- **Write y Edit**: solo hacia tus salidas propias.
- **TodoWrite, TaskCreate, TaskUpdate, TaskList y ToolSearch**: permitidas como contabilidad, sin efecto sobre archivos. Ninguna otra herramienta.
- **Bash o PowerShell**: solo para las acciones de la tabla. No hay listados de directorios.

**Prohibido:**
- cualquier archivo fuera del cierre, en particular:
  - `~/.claude/projects/*`, otras sesiones y transcripciones;
  - `~/.codex/worktrees/*` y el repositorio de trabajo;
  - los clones `D:\r62-arch-*` distintos de `D:\r62-arch-a1t01`;
  - los archivos de `D:\r62-arch-a1t01-run\` distintos de los dos del run;
- red y web;
- subagentes u otros agentes;
- editar, hacer commit o push en el repositorio;
- crear una A-n;
- decidir materias del Owner.

Al terminar, el clon debe seguir limpio y en el commit; el auditor lo comprueba. Las salidas de `dotnet test` en `bin/` y `obj/` están ignoradas por Git.

## Forma de lectura (fidelidad)

- **Read:**
  - usa la ruta absoluta;
  - en los archivos de más de 60 000 bytes, usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura grande por llamada, para que ninguna salida se trunque.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: V14 2352, 2380 y 2382; decisiones 657, 734, 750, 768, 787 y 805; ROADMAP 408-501;
  `output.json` de 86e3, línea 79; `output.json` de 2dfe, línea 85. El objeto A-1 no tiene ninguna.
  - Si las necesitas, léelas con `git -C D:/r62-arch-a1t01 show ca09ade8:<ruta> | sed -n 'Np'`.
  - El auditor anota una línea larga conocida que llegue truncada por Read, pero no la cuenta como entregada.
- **Premisas.** Cita como premisa (`PremiseRefs`) solo líneas que hayas leído enteras con Read o con ese `git show | sed -n`. El invocador comprueba que
  esas líneas te llegaron fielmente.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ⇒ ⇔ ∈ ∉ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerlo fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Qué decidir

Tu foco mínimo es **A62-A1T-01 y las regresiones del cambio**. No se te prohíbe señalar otro defecto material que encuentres, y tampoco se te pide
reabrir hallazgos solo porque aparecen en el historial (`order.txt` §8).

Contesta los focos 1-12, cada uno una vez:
1. **A62-A1T-01.** ¿Cierra la fila «D2-2 (cont.)» el bloqueo con un segundo rebase y un intento todavía en LAUNCHING, también en una toma (F.7) y en una
   sesión nueva que rebasa al abrir (WORKFLOW §4.2; V14 §8.8 y §8.9)? (paquete, pregunta 17)
2. **Fallo cerrado y sin atajos.** Sin M1 o sin otro eslabón, o con otro blob, UNRESOLVED y STOP. No basta el último mapa, ni la igualdad de path, el
   parecido de parches o una sustitución de SHA sin prueba. Ningún mapa recibe una entrada compuesta X → X''.
3. **El intento no cambia.** La resolución acredita la referencia histórica y no decide si el proceso arrancó. La invocación, el `Target` original, el
   estado, `RunId`, `reserved_at`, `BudgetSnapshot`, contadores, fase y linajes quedan iguales (D2-7, D2-8).
4. **D2-6 sin cambio.** Sus tres ramas siguen decidiendo, coherentes con la nueva fila: no arrancó con vigencia abierta → replanificar sobre X'' con la
   misma reserva; desconocido → LAUNCH_UNCERTAIN, sin prueba fabricada ni relanzamiento; autorización terminada → reglas de vigencia.
5. **Sin autorreferencia ni punto extra.** La comprobación usa la historia candidata de n y la punta rebasada, nunca el commit todavía inexistente de n, y
   no exige publicar antes otro punto (orden §4, condiciones 9 y 10).
6. **Alineación.** D2-3, D2-9 y D2-11, y las obligaciones C-15 (d)(e)(d2)(e2)(i), C-29 (i) y C-38 (cuarta revisión).
7. **Evidencia.** RED/GREEN de A1-P08 (en RED fallan solo las 12 trazas del segundo rebase, y solo por A1-P08; GREEN 118/118), negativos con su regla
   exacta, T8 con Git real y las secuencias c3-obs. Verifica lo que necesites con EX-1.
8. **O1.** D1-17: «abiertas en el par de apertura o después» y la reconstrucción por `last_request`, sin cambio de significado.
9. **O2.** Coherencia documental de A-1, del paquete, de la descripción de A1-P02 y de la etiqueta «Asserted», sin reescribir resultados históricos.
10. **O3.** El negativo de una autoridad SUPERSEDED (C-38 y A1-R05) y el A2' de EXECUTION a través de uno y de dos rebases (C-15 (i)).
11. **Doce cierres.** ¿Se conservan A62-A1-01..06, OBS-A1-01, A62-A1R-01..03 y A62-A1S-01..02, también tras partir en dos las filas D1-17 y D2-2?
12. **Owner, F3 y materialidad.** ¿Hace falta una decisión del Owner? ¿Siguen sin cambio de esquema los contratos F3? ¿Se mantiene la materialidad?

Dispón de forma explícita, como CLOSED o STILL_OPEN sobre la versión exacta, estos **trece** hallazgos: A62-A1T-01 y los doce cierres anteriores
(A62-A1-01..06, OBS-A1-01, A62-A1R-01..03 y A62-A1S-01..02). No reabras un cierre sin un contraejemplo nuevo. Evalúa A62-A1T-O1..O3, cada uno una vez.

Un REQUIRED nuevo existe solo si el defecto es material según LIFECYCLE §5, y debe traer:
- id estable `A62-A1U-NN`;
- delta o sección exacta de A-1;
- premisa canónica completa en `PremiseRefs`;
- contraejemplo concreto;
- corrección exacta.

Una precisión sin cambio de significado es OPTIONAL (`A62-A1U-ON`). Si un contrato F3 necesita un cambio real de forma de esquema, es REQUIRED, con el
contrato exacto y el contraejemplo.

## Resultado

Tu último mensaje contiene **solo** un bloque ```json con este objeto (sin texto fuera del bloque). Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-architect-review-result/v1 (representación experimental; rigen I-61 y LIFECYCLE)",
  "RunId": "R20261005T151303Z-c64c", "InvocationId": "I20261005T151303Z-c64c", "LogicalReviewRequestId": "L20261005T151303Z-c64c", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN",
  "ReviewedUnit": "I-62", "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-1.md", "ReviewedBlob": "...",
  "ReviewerMode": "SEPARATE SESSION",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora y el operador humano",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción"]},
  "IdentityCheck": {"CloneHeadMatches": true, "WorktreeHeadMatches": true, "ObjectBlobMatches": true, "PackageBlobMatches": true,
                    "DispositionBlobMatches": true, "CleanTree": true, "OrderSha256Matches": true},
  "InputsRead": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A1U-01", "AffectedDelta": "...", "ViolatedInvariant": "...", "Counterexample": "...", "CorrectionRequired": "...",
                        "PremiseRefs": [{"Path": "...", "Section": "...", "LineStart": 1, "LineEnd": 1, "Quote": "proposición completa"}], "WhyItMatters": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A1U-O1", "AffectedDelta": "...", "Note": "...", "PremiseRefs": []}],
  "FindingDispositions": [{"FindingId": "A62-A1T-01 | A62-A1-01 | … | A62-A1-06 | OBS-A1-01 | A62-A1R-01 | A62-A1R-02 | A62-A1R-03 | A62-A1S-01 | A62-A1S-02", "State": "CLOSED | STILL_OPEN", "Rationale": "...", "PremiseRefs": []}],
  "OptionalCorrections": [{"Id": "A62-A1T-O1 | A62-A1T-O2 | A62-A1T-O3", "Assessment": "CONFIRMED | CHALLENGED | NOT_APPLICABLE", "Note": "..."}],
  "Focus": [{"Item": 1, "Assessment": "CONFIRMED | CHALLENGED", "Note": "..."}],
  "F3ContractCrossCheck": [{"Contract": "...", "Assessment": "COMPATIBLE | TEXT_AMENDMENT_DECLARED | REQUIRES_SCHEMA_CHANGE", "Note": "..."}],
  "CounterexampleVerification": {"Traces": 118, "Valid": 39, "Invalid": 79, "AllAsExpected": true, "Notes": [{"Trace": "...", "ClaimedReasonHolds": true, "Note": "..."}]},
  "Materiality": {"Overall": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"},
                  "ObsA101": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"}, "Note": "tu evaluación"},
  "OwnerAuthorityCheck": {"Spending": "NONE | ...", "Credentials": "NONE | ...", "Destructive": "NONE | ...", "NewOVScenario": "NONE | ...",
                          "OVRemovalOrReassignment": "NONE | ...", "OwnerPolicyChoice": "NONE | ...", "NewOwnerAuthority": "NONE | ...", "OwnerDecisionRequired": false},
  "LegacyApplicability": {"AppliesOnlyToI62": true, "NoRetroactiveI61Change": true, "NoI64FixClaim": true, "Note": "..."},
  "IfAgreed": {"A62_A1_01_CLOSED": null, "A62_A1_02_CLOSED": null, "A62_A1_03_CLOSED": null, "A62_A1_04_CLOSED": null, "A62_A1_05_CLOSED": null,
               "A62_A1_06_CLOSED": null, "OBS_A1_01_CLOSED": null, "A62_A1R_01_CLOSED": null, "A62_A1R_02_CLOSED": null, "A62_A1R_03_CLOSED": null,
               "A62_A1S_01_CLOSED": null, "A62_A1S_02_CLOSED": null, "A62_A1T_01_CLOSED": null,
               "FC01Resolved": null, "FC02Resolved": null, "NoOwnerDecisionRequired": null, "SuitableForCoordinatorAgreement": null,
               "F4MayProceedAfterCoordinatorAgreed": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- `Materiality` lleva **tu** evaluación. Los valores de arriba son los de A-1 y del Coordinator, como referencia.
- Cobertura, una vez cada elemento:
  - `Focus`: los ítems 1-12;
  - `FindingDispositions`: los trece hallazgos;
  - `OptionalCorrections`: A62-A1T-O1..O3.
- **Con AGREED:** cero REQUIRED, los trece hallazgos en CLOSED y los dieciocho campos de `IfAgreed` en `true`. En cualquier otro caso, `IfAgreed` va todo
  en `null`.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
