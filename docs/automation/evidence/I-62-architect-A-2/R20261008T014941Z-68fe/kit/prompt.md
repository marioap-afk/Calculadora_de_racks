I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA CANDIDATA A-2 (presupuestos de recuperación de F6), preparada según la disposición del Coordinator (decisiones §55, punto 8). ÚNICA invocación preparada, sin reintento automático.

Eres el ARCHITECT de esta revisión, en una sesión nueva y separada de la sesión autora de A-2. Revisas en solo lectura la enmienda exacta y devuelves un
único veredicto formal de LIFECYCLE §6. No implementas nada, no editas el repositorio, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261008T014941Z-68fe
InvocationId           = I20261008T014941Z-68fe
LogicalReviewRequestId = L20261008T014941Z-68fe   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a2   (su única rama, main, está en el commit exacto; sin remoto; no lo modifiques)
Commit                 = 4a059afa85dce5a82f74120cad88c5ebadb61c7a   (commit de publicación; CI 37714804772, push, 4/4 success: señal de salud, no prueba local)
Commit de A-2          = b280f7079e128113666e85a2f437e629d0f0d99e   (introduce A-2, sobre c9f9419dc4aed32242c59de489a8e78a8e369f9b; el commit de publicación solo añade a2-guards-result.json)
Objeto                 = docs/initiatives/I-62-A-2.md                              blob d47f71b66ba86c31f857f0d8c2d2437636947af0
Paquete                = docs/initiatives/I-62-architect-package-A-2.md            blob f45a2384eb1a3c9ec5654995c5b88f6c51b6742f
Guardas                = docs/automation/evidence/I-62-A2/a2-guards.py              blob 91c9c2a46a4a9327ecfff44fa52a4d48e0e33935
Resultado de guardas   = docs/automation/evidence/I-62-A2/a2-guards-result.json     blob 33ffbb9deb114972cfd11ade3ab448a2af1f50dd
Freeze                 = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; Proposal V14 commit 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Orden vigente          = D:\r62-arch-a2-run\order.txt   (disposición del Coordinator registrada en decisiones §55; 6 234 bytes, SHA-256 bfa265cd5b3bee47059dd8178153b19916826ad94d936715fd22893a3d6d5259)
```

El objeto y el número de invocaciones los fijan este prompt, la cabecera del paquete y la orden vigente (`order.txt` §4, §7 y §8). Los commits del
Freeze (`b64a3b64`) y de V14 (`4c617e82`) son anteriores a un rebase y no están en el clon: V14 se identifica por su blob en el commit.

## Paso 0 — identidad, antes de cualquier lectura sustantiva

Ejecuta los comandos de los puntos 1 a 3 con la herramienta **Bash** (no con PowerShell).

1. `git -C D:/r62-arch-a2 rev-parse HEAD` = el commit. Después, `git -C D:/r62-arch-a2 rev-parse HEAD:docs/initiatives/I-62-A-2.md`, y lo mismo para
   el paquete, las guardas, su resultado y `docs/initiatives/I-62-proposal-v14.md`, deben dar los blobs. Por último,
   `git -C D:/r62-arch-a2 status --porcelain` debe salir vacío.
2. En tu propio directorio de trabajo (el worktree de la tarea), `git rev-parse HEAD` debe ser el commit. Si no lo es, ejecuta una sola vez
   `git checkout --detach 4a059afa85dce5a82f74120cad88c5ebadb61c7a` en él y vuelve a comprobarlo.
3. `sha256sum D:/r62-arch-a2-run/order.txt D:/r62-arch-a2-run/prompt.md`: el primero debe dar el SHA-256 de arriba. Después lee **enteros**, con
   Read, `order.txt` (la autoridad vigente) y `prompt.md` (este mismo texto, para comprobar que te llegó fiel).

Si algo no coincide y el punto 2 no lo corrige, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto; rutas relativas al clon, que también puedes leer en tu worktree porque está en el mismo commit)

**Canónicos** (la tabla de insumos del paquete §2, en el commit):
1. `docs/initiatives/I-62-A-2.md`, el objeto, **entero** (25 567 bytes y 320 líneas). Secciones: cabecera 1-42; §1 44-66; §2 A2-P1 68-163 (2.1
   70-114; 2.2 116-128; 2.3 delta exacto 130-156; 2.4 158-163); §3 A2-P2 165-224 (3.1 167-181; 3.2 183-196; 3.3 delta exacto 198-224, con la «Lectura
   vigente» que no forma parte del delta en 221-224); §4 226-244; §5 246-257; §6 259-278; §7 280-288; §8 290-300; §9 302-309; §10 311-320.
2. **Los deltas explícitos** (acción MD-1), solo orientativos: las premisas se citan de líneas leídas, no del diff.
   - `git -C D:/r62-arch-a2 diff c9f9419d b280f707 -- <rutas del cierre>`: el commit que introduce A-2;
   - `git -C D:/r62-arch-a2 diff b280f707 4a059afa -- docs/automation/evidence/I-62-A2/a2-guards-result.json`: la publicación.
3. `docs/initiatives/I-62-architect-package-A-2.md`, entero (110 líneas): §1 24-37; §2 39-54; §3 56-77; §4 79-86; §5 88-103; §6 105-110.
4. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes; solo por rangos de ≤ 300 líneas). Lo que pide el paquete §2:
   - Anexo D 2384-2646: D.3 2431-2528 («Totales por ronda de F6» en 2519; la frase ancla de A-2 en 2528); D.4 2530-2554; D.5 2556-2565;
     D.6 2567-2587; D.8 2608-2646;
   - §17 931-950 (fila F6 en 943); §18 952-977 (fila OD-2 en 971);
   - «la fila de P-07 en §8» (paquete §2, línea 44). El cierre contiene dos filas que pueden corresponder, y este kit no elige entre ellas: V14 657,
     en la tabla de §9.3 (649-663), que nombra P-07; y `docs/AUTOMATION_PLAN.md` (transitivo del cierre) §16.11 (674-708), cuya fila 704 define
     P-07, junto con §16.8 (600-618), que en la línea 612 fija el STOP por el tope de invocaciones.
5. `docs/initiatives/I-62-A-1.md` (77 984 bytes y 437 líneas; por rangos de ≤ 300 líneas). Para lo que pide el paquete §2 (formato de una A-n,
   presupuestos de bucle y principio sin reinicios): cabecera 1-56; §2 FC-01 90-167; §4 229-254; §11 429-437.
6. `docs/initiatives/I-62-consensus-freeze.md` (113 líneas).
7. `docs/INITIATIVE_LIFECYCLE.md`: §3 66-94, §5 152-191 y §6 193-222.
8. `docs/automation/decisions/I-62.md`: §51-§55, líneas 972-1074 (§51 972-991; §52 993-1010; §53 1012-1032; §54 1034-1051; §55 1053-1074).
9. `docs/automation/evidence/I-62-evidence.md`, por rangos: §64 2415-2438; §67 2480-2490; §70-§75 2531-2630; §78-§80 2688-2743.
10. Los hechos de B2:
    - `docs/automation/evidence/I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/b2-report-for-coordinator.md` (24 líneas);
    - `docs/automation/evidence/I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/comparison-v2-analysis.json` (70 líneas).
11. `docs/automation/evidence/I-62-A2/a2-guards.py` (535 líneas) y `docs/automation/evidence/I-62-A2/a2-guards-result.json` (424 líneas): evidencia
    de apoyo, no autoridad.
12. Los archivos del run: `D:\r62-arch-a2-run\order.txt` y `D:\r62-arch-a2-run\prompt.md`.

**Transitivos permitidos.** Los obligan `CLAUDE.md`, `AGENTS.md` y el Context Pack `documentation-governance`, que declara el contrato de I-62; léelos
si tus instrucciones te lo piden:
- `CLAUDE.md`, `AGENTS.md`, `README.md`;
- `docs/HANDOFF.md` (448 804 bytes; por rangos);
- `docs/WORKFLOW.md` (§4.2: rebase al abrir sesión), `docs/ARCHITECTURE.md`, `docs/FOUNDATIONS.md`;
- `docs/ROADMAP.md` (168 867 bytes; por rangos);
- `docs/context-packs/README.md`, `docs/context-packs/documentation-governance.md`;
- `docs/initiatives/I-62-portabilidad-coordinador-principal.md` (contrato de I-62; su frontmatter `context_packs` declara el Context Pack);
- `docs/AUTOMATION_PLAN.md` (155 229 bytes; por rangos), `docs/initiatives/README.md` (por rangos), `docs/initiatives/PROMPT_TEMPLATES.md`.

## Contrato de acciones (cerrado y fijado antes del lanzamiento: nada fuera de esta lista)

El invocador audita después cada llamada contra esta misma tabla (auditor v4-a2.1, probado antes del lanzamiento). Cada ruta del sistema de archivos se
comprueba por su **efecto**: resuelta contra el directorio actual, con `.` y `..` colapsados y los enlaces resueltos. Se clasifica como archivo del
cierre, archivo del run, salida propia, raíz o subdirectorio del clon o del worktree, o fuera. No se compara la cadena literal del comando, ni un
prefijo de texto. El cierre y las acciones no se amplían después de ejecutar.

ID-1, ID-2, MD-1, RD-1, RD-2 y EX-1, y la redirección, `mkdir -p` y `sed -i` de OWN-2, se ejecutan **solo con la herramienta Bash**. PowerShell
queda para EX-2 (`dotnet test`), `Set-Location` (NAV-1) y `Get-Content` (OWN-1 y archivos del cierre o del run).

| Id | Acción permitida |
|---|---|
| ID-1 | `git`, con `-C D:/r62-arch-a2`, con `-C <tu worktree>` o desde el directorio del clon o del worktree (raíz o subdirectorio). Subcomandos: `rev-parse HEAD`; `rev-parse <commit>:<ruta del cierre>` y `cat-file -p\|-t\|-s <commit>:<ruta del cierre>`, con `<commit>` = `4a059afa` o `HEAD` (`cat-file -t\|-s` también con `c9f9419d` o `b280f707`); `status [--porcelain]`; `log --oneline [-N]`; `worktree list`; `config --get <clave>` |
| ID-2 | `git checkout --detach 4a059afa85dce5a82f74120cad88c5ebadb61c7a` en tu worktree, solo si su HEAD no es el commit |
| MD-1 | Metadatos y hashing: `git hash-object <archivo del cierre o salida propia>` (sin `-w` ni `--stdin`); `git diff [--stat\|--numstat\|--no-color\|--word-diff\|-U<N>] <base> <destino> -- <rutas del cierre>`, con exactamente dos revisiones y en este orden: `c9f9419d b280f707` (el delta que introduce A-2) o `b280f707 4a059afa` (la publicación; `HEAD` vale por `4a059afa`); `sha256sum` y `wc` sobre archivos del cierre, del run o salidas propias; `pwd`; `date` |
| NAV-1 | `cd` (o `Set-Location` en PowerShell) hacia la raíz o **cualquier subdirectorio** del clon o de tu worktree, o hacia un directorio de tus salidas propias. Cambiar de directorio **no** autoriza leer nada fuera del cierre: las rutas posteriores se resuelven desde ahí y se clasifican por su efecto |
| RD-1 | `git show 4a059afa:<ruta del cierre>` (o `HEAD:<ruta del cierre>`, con el HEAD ya verificado), sola o con tubería a `sed -n`, `head`, `tail`, `wc`, `grep`, `awk`, `sha256sum`, `sort`, `uniq`, `cut`, `tr` o `nl`. La ruta de un objeto Git es una ruta de **árbol** desde la raíz del repositorio: Git no normaliza `..` y rechaza esa ruta |
| RD-2 | `sed -n`, `wc`, `awk`, `grep` (sin `-r`, `-R` ni `--recursive`), `head`, `tail`, `cat`, `sha256sum`, `sort`, `uniq`, `cut`, `tr`, `nl`, `diff`, `cmp`, `echo`, `printf`, `true` y `read`. Solo sobre **archivos** del cierre (bajo el clon o tu worktree), sobre los dos archivos del run o sobre tus salidas propias. Combinables con `\|`, `;`, `&&`, `\|\|` y bucles `for`/`while`. Redirección solo hacia tus salidas propias o `/dev/null`. Ningún directorio como argumento |
| EX-1 | `python D:/r62-arch-a2/docs/automation/evidence/I-62-A2/a2-guards.py self-test --out <archivo en tus salidas propias>`: exactamente esos argumentos. El modo `run` no está disponible en el clon (G4 necesita `origin/main`, que el clon no tiene) y no está permitido. Además, `python` con heredoc, con `-c` o con un script propio, siempre que todas sus rutas literales sean archivos del cierre, archivos del run o salidas propias. Sin listar directorios (`os.listdir`, `os.walk`, `glob`, `iterdir`), sin procesos (`subprocess`, `os.system`), sin red y sin borrar ni mover archivos |
| EX-2 | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` dentro de `D:\r62-arch-a2`, desde Bash o PowerShell (SDK en `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`). Cualquier otra ruta que pases, como el log, `--results-directory` o `--logger`, debe estar en tus salidas propias. La orden no trae exención: si sigues el paso 4 de «Leer primero» de AGENTS.md, ejecútalo aquí. Es una comprobación local tuya, no evidencia de gate |
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
- **Bash**: las acciones de la tabla (todas salvo las de PowerShell de abajo). No hay listados de directorios.
- **PowerShell**: solo EX-2 (`dotnet test`), `Set-Location` hacia los directorios de NAV-1 y `Get-Content` de archivos del cierre, del run o de tus
  salidas propias (con tubería solo a `Select-Object`, `Measure-Object` u `Out-String`). `git`, `python`, `sha256sum` y cualquier otro comando
  ejecutados desde PowerShell quedan fuera del contrato, también en el Paso 0.

**Prohibido:**
- cualquier archivo fuera del cierre, en particular:
  - `~/.claude/projects/*`, otras sesiones y transcripciones;
  - `~/.codex/worktrees/*` y el repositorio de trabajo;
  - los clones `D:\r62-arch-*` distintos de `D:\r62-arch-a2`;
  - el fixture de F6 (`D:\r62-fixture\*`);
  - los archivos de `D:\r62-arch-a2-run\` distintos de los dos del run;
- red y web;
- subagentes u otros agentes;
- editar, hacer commit o push en el repositorio;
- crear o aplicar una A-n;
- decidir materias del Owner.

Al terminar, el clon debe seguir limpio y en el commit; el auditor lo comprueba. Las salidas de `dotnet test` en `bin/` y `obj/` están ignoradas por Git.

## Forma de lectura (fidelidad)

- **Read:**
  - usa la ruta absoluta;
  - en los archivos de más de 60 000 bytes (V14, A-1, decisiones, evidencia, HANDOFF, ROADMAP, AUTOMATION_PLAN e `initiatives/README.md`), usa `offset`
    y `limit` de **300 líneas como máximo**;
  - haz una sola lectura grande por llamada, para que ninguna salida se trunque.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: V14 2352, 2380 y 2382; decisiones 657, 734, 750, 768, 787, 805 y 819; ROADMAP
  408-501. Ninguna cae en los rangos de arriba; el objeto A-2 y el paquete no tienen ninguna.
  - Si las necesitas, léelas con `git -C D:/r62-arch-a2 show 4a059afa:<ruta> | sed -n 'Np'`.
  - El auditor anota una línea larga conocida que llegue truncada por Read, pero no la cuenta como entregada.
- **Premisas.** Cita como premisa (`PremiseRefs`) solo líneas que hayas leído enteras con Read o con ese `git show | sed -n`, de archivos del cierre
  (`Path` relativo al clon) o de `order.txt` (`Path` = `D:\r62-arch-a2-run\order.txt`, leída con Read). `prompt.md` nunca es premisa. El invocador
  comprueba que esas líneas te llegaron fielmente.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ⇒ ⇔ ∈ ∉ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerlo fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Qué decidir

Se pide el veredicto del paquete §1 (líneas 24-37) sobre la A-2 exacta. No se te prohíbe señalar otro defecto material que encuentres.

Contesta en `Focus`, cada una una vez, las preguntas 1-4 del paquete §4 (líneas 81-85; el texto exacto está allí):
1. fidelidad de los deltas a las reglas candidatas del Coordinator (decisiones §55, puntos 5 y 6, líneas 1064-1065; el texto de la orden está en
   `order.txt` §5 y §6) y necesidad y minimalidad de cada precisión añadida;
2. ausencia o presencia de contradicciones con D.4, D.6, P-07, los totales de D.3, D.8, §18 (OD-2) y A-1;
3. la materialidad;
4. lo que prueban las guardas G1-G5 y cómo modelan cada regla numerada.

Dispón en `QuestionDispositions`, cada una una vez, las preguntas Q-A2-01..Q-A2-07 de A-2 §8 (líneas 294-300), como REQUIRED, OPTIONAL o NO_FINDING
(sin hallazgo), con los `FindingIds` que la sostienen (ninguno con NO_FINDING). El paquete §1 y §4 nombran Q-A2-01..Q-A2-05; A-2 §8 contiene siete, y
el resultado dispone las siete.

Además, según el paquete §1:
- si hace falta o no una decisión del Owner (`OwnerAuthorityCheck`);
- si A-2 cambia o no los contratos B.1-B.11, `state/v2`, §20, A-1, AUTOMATION_PLAN 16.x y F4 (`NoChangeConfirmation`: NO_CHANGE, CHANGE o
  NOT_DETERMINED, con la base de cada respuesta);
- tu modo de revisión, si revisor y autor son la misma persona y el contexto inyectado (`ReviewerMode`, `SamePersonAsAuthor`,
  `InjectedContextDeclaration`). Declara también el proveedor (paquete §5) en `SamePersonAsAuthor`;
- la identidad revisada: commit, ruta y blob.

Un REQUIRED existe solo si el defecto es material según LIFECYCLE §5, y debe traer:
- id estable `A62-A2-NN`;
- sección exacta de A-2 (`AffectedDelta`);
- premisa canónica completa en `PremiseRefs`;
- la autoridad o invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno de los dos;
- por qué importa;
- corrección exacta.

Una precisión sin cambio de significado es OPTIONAL (`A62-A2-ON`).

## Resultado

Tu último mensaje contiene **solo** un bloque ```json con este objeto (sin texto fuera del bloque). Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-architect-review-result/v1 (representación experimental; rigen I-61 y LIFECYCLE)",
  "RunId": "R20261008T014941Z-68fe", "InvocationId": "I20261008T014941Z-68fe", "LogicalReviewRequestId": "L20261008T014941Z-68fe", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN",
  "ReviewedUnit": "I-62", "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-2.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción"]},
  "IdentityCheck": {"CloneHeadMatches": true, "WorktreeHeadMatches": true, "ObjectBlobMatches": true, "PackageBlobMatches": true,
                    "GuardsBlobMatches": true, "GuardsResultBlobMatches": true, "V14BlobMatches": true, "CleanTree": true, "OrderSha256Matches": true},
  "InputsRead": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A2-01", "AffectedDelta": "...", "ViolatedInvariant": "...", "Counterexample": "...", "CorrectionRequired": "...",
                        "PremiseRefs": [{"Path": "...", "Section": "...", "LineStart": 1, "LineEnd": 1, "Quote": "proposición completa"}], "WhyItMatters": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A2-O1", "AffectedDelta": "...", "Note": "...", "PremiseRefs": []}],
  "QuestionDispositions": [{"Id": "Q-A2-01 | … | Q-A2-07", "Disposition": "REQUIRED | OPTIONAL | NO_FINDING", "FindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "Focus": [{"Item": 1, "Assessment": "CONFIRMED | CHALLENGED", "Note": "..."}],
  "NoChangeConfirmation": {"ContractsB1toB11": {"Assessment": "NO_CHANGE | CHANGE | NOT_DETERMINED", "Note": "..."}, "StateV2": {"Assessment": "...", "Note": "..."},
                           "Section20": {"Assessment": "...", "Note": "..."}, "A1": {"Assessment": "...", "Note": "..."},
                           "AutomationPlan16x": {"Assessment": "...", "Note": "..."}, "F4": {"Assessment": "...", "Note": "..."}},
  "GuardsVerification": {"SelfTestRun": false, "Vectors": null, "VectorsPassed": null, "Mutants": null, "MutantsKilled": null, "AllAsExpected": null,
                         "Notes": [{"Guard": "G1 | G2 | G3 | G4 | G5", "ClaimHolds": "HOLDS | DOES_NOT_HOLD | NOT_VERIFIED", "Note": "..."}]},
  "Materiality": {"Overall": {"M01": "YES | NO", "M02": "...", "M03": "...", "M04": "...", "M05": "...", "M06": "...", "M07": "...", "M08": "..."}, "Note": "tu evaluación"},
  "OwnerAuthorityCheck": {"Spending": "NONE | ...", "Credentials": "NONE | ...", "Destructive": "NONE | ...", "NewOVScenario": "NONE | ...",
                          "OVRemovalOrReassignment": "NONE | ...", "OwnerPolicyChoice": "NONE | ...", "NewOwnerAuthority": "NONE | ...", "OwnerDecisionRequired": false},
  "LegacyApplicability": {"AppliesOnlyToI62": true, "NoRetroactiveI61Change": true, "NoI64FixClaim": true, "Note": "..."},
  "IfAgreed": {"A2P1Agreed": null, "A2P2Agreed": null, "NoOwnerDecisionRequired": null, "NoChangeToProtectedSurfaces": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: `IdentityCheck`, `Materiality`, `OwnerAuthorityCheck`, `LegacyApplicability` y
  `GuardsVerification` llevan **tu** comprobación y **tu** evaluación.
- `GuardsVerification`: `a2-guards-result.json` registra G1-G5 del modo `run` sobre `b280f707`, que el clon no puede reproducir. Con las acciones
  permitidas se puede contrastar:
  - G1: los literales de A-2 §2.1 y §3.1 y el ancla, frente a V14 en el commit (mismo blob). La comprobación de secciones de G1 usa `clause_map.py`,
    que está fuera del cierre;
  - G3: solo los blobs de V14, del Freeze y de A-1;
  - G5: con EX-1 (`self-test`), que ejecuta los vectores, los mutantes y la cobertura de reglas, pero no el vínculo con el texto de A-2
    (`TextBinding`), que solo calcula el modo `run` (sus frases, `TEXT_BINDING`, están en `a2-guards.py`, dentro del cierre).

  No se pueden verificar con las acciones permitidas: las rutas de G2 fuera del cierre, los blobs de G3 de ADR-0048 y de `clause_map.py`, y G4
  entero. Lo que no puedas verificar va como `NOT_VERIFIED` o en `KnownLimitations`. Usa `null` en lo que no compruebes.
- Cobertura, una vez cada elemento:
  - `Focus`: los ítems 1-4;
  - `QuestionDispositions`: Q-A2-01..Q-A2-07; REQUIRED u OPTIONAL citan en `FindingIds` hallazgos de `RequiredFindings` u `OptionalFindings`.
- **Con AGREED:** cero REQUIRED (también en `QuestionDispositions`), `OwnerDecisionRequired` = false, los seis apartados de `NoChangeConfirmation` en
  NO_CHANGE y los cinco campos de `IfAgreed` en `true`. En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`. En ese caso `Focus` y `QuestionDispositions` pueden ir vacíos, y lo que no hayas evaluado va así: `Materiality.Overall` en
  `NOT_ASSESSED`; en `null` las booleanas de `IdentityCheck` que no comprobaste, las de `LegacyApplicability` y `OwnerDecisionRequired`;
  `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`; `NoChangeConfirmation` en `NOT_DETERMINED`. `NOT_ASSESSED` y `null` en esos
  campos solo valen con NOT_ACCREDITED.
