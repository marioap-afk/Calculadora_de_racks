I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA A-1 CORREGIDA PARA A62-A1R-01..03 (orden nocturna, decisiones §41, punto E). Primera de las dos invocaciones como máximo que permite la orden.

Eres el ARCHITECT de esta revisión, en una sesión nueva y separada de la sesión autora de A-1. Revisas en solo lectura la enmienda exacta y devuelves un
único veredicto formal de LIFECYCLE §6. No implementas nada, no editas el repositorio, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261005T063359Z-86e3
InvocationId           = I20261005T063359Z-86e3
LogicalReviewRequestId = L20261005T063359Z-86e3   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a1r3   (su única rama, main, está en el commit exacto; sin remoto; no lo modifiques)
Commit                 = 9dcfc08d5171b01df97b3a492a60218c5ab2453e   (CI de publicación 37272599376, push, 4/4 success: señal de salud, no prueba local)
Objeto                 = docs/initiatives/I-62-A-1.md                                blob 39c2f8317ec381fa60c3564a834278df8898101c
Paquete                = docs/initiatives/I-62-architect-package-A-1.md              blob 82bea6e61cd0712dfe3d235420d5d7cabb0507be
Disposición            = docs/initiatives/I-62-architect-review-A-1-disposition.md   blob 2c7a8e7e030c3d76516bbc9889237aca0a35bfc6
Freeze                 = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; Proposal V14 commit 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Orden vigente          = D:\r62-arch-a1r3-run\order.txt                 (orden nocturna; 20 652 bytes, SHA-256 4fc5883f3adba35f23e5b3af22602e8229064b51bc64303d4399abcc78dd8d09)
Autorización anterior  = D:\r62-arch-a1r3-run\prior-authorization.txt   (decisiones §40; 9 954 bytes, SHA-256 046089940e9f11dbc94d01818dac63e745ca5be8890a949c37dfb5fe1b47c837)
```

La autorización anterior designaba otro objeto (`0ad410f8`, blob `9c621fce`) y una sola invocación, que ya se consumió en R20261005T044948Z-ac67. Aquí
solo sirve como fuente de los focos 1-12 y de las condiciones formales de revisión, que siguen aplicando a la misma enmienda. El objeto, el commit y el
número de invocaciones los fijan este prompt y la orden vigente (`order.txt` §3.E y §6).

## Paso 0 — identidad, antes de cualquier lectura sustantiva

1. `git -C D:/r62-arch-a1r3 rev-parse HEAD` = el commit. Después, `git -C D:/r62-arch-a1r3 rev-parse HEAD:docs/initiatives/I-62-A-1.md`, y lo mismo para
   el paquete y la disposición, deben dar los blobs. Por último, `git -C D:/r62-arch-a1r3 status --porcelain` debe salir vacío.
2. En tu propio directorio de trabajo (el worktree de la tarea), `git rev-parse HEAD` debe ser el commit. Si no lo es, ejecuta una sola vez
   `git checkout --detach 9dcfc08d5171b01df97b3a492a60218c5ab2453e` en él y vuelve a comprobarlo.
3. `sha256sum D:/r62-arch-a1r3-run/order.txt D:/r62-arch-a1r3-run/prior-authorization.txt` debe dar los dos SHA-256 de arriba. Después lee **enteros**, con
   Read, `order.txt` (la autoridad vigente) y `prior-authorization.txt` (focos 1-12).

Si algo no coincide y el punto 2 no lo corrige, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto; rutas relativas al clon, que también puedes leer en tu worktree porque está en el mismo commit)

**Canónicos:**
1. `docs/initiatives/I-62-A-1.md`, el objeto, entero (62 828 bytes y 363 líneas: dos lecturas de ≤ 300 líneas). Secciones: §1 46-72; §2 FC-01 73-150;
   §3 FC-02 151-209; §4 210-236; §5 237-257; §6 258-264; §7 265-276; §8 277-286; §9 287-312; §10 313-354; §11 355-363.
   También `docs/initiatives/I-62-architect-package-A-1.md` (168 líneas).
2. `docs/initiatives/I-62-architect-review-A-1-r2.md` y `docs/automation/evidence/I-62-architect-A-1/R20261005T044948Z-ac67/output.json`: el registro y el
   resultado literal de la revisión formal anterior, A62-A1R-01..03 y A62-A1R-O1..O5. Su acreditación la decide el Coordinator: no la des por hecha ni
   la niegues.
3. `docs/initiatives/I-62-architect-review-A-1-disposition.md`, con las disposiciones del Coordinator y la matriz de §4 para A62-A1R-01..03 y O1..O5, y
   `docs/initiatives/I-62-architect-review-A-1.md`, el registro de la corrida no acreditada R20261003T023945Z-cab7. Esa corrida es solo insumo técnico
   histórico, nunca el acuerdo del Architect.
4. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes; solo por rangos de ≤ 300 líneas). Secciones (líneas): §3.1 178-203; §8.8 497-556;
   §8.9 557-592; §9 593-664; §11.3 704-720; §13 769-794; §14 795-882; §18 952-978; §20.5 1403-1521; §20.5.2 1522-1544; §20.6 1545-1611;
   §20.7 1612-1641; §20.8 1642-1655; B.2 1815-1836; B.7 1901-1909; B.8.1 1915-1975; B.8.4 2006-2053; B.8.6 2092-2109; B.8.7 2110-2127;
   B.8.8 2128-2222; B.9 2223-2253; B.10 2254-2307; Anexo C 2330-2383; F.6-F.8 3153-3246; G.1 3247-3292.
5. `docs/initiatives/I-62-consensus-freeze.md`.
6. `docs/automation/evidence/I-62-A1/a1-counterexamples.py` (84 462 bytes; por rangos) y `a1-counterexamples-result.json` (94 trazas): evidencia de
   apoyo, no autoridad.
7. `docs/automation/evidence/I-62-prep/freeze-issues.md` (incluye SM-05), `docs/automation/evidence/I-62-prep/f4-dossier.md` (§4) y
   `docs/automation/evidence/I-62-prep/night-2026-10-05/rebase-map.json`, el mapa del rebase del 2026-10-05: los registros citan SHAs de antes del
   rebase como hechos históricos.
8. `docs/INITIATIVE_LIFECYCLE.md` (§3 66-95, §5 152-192, §6 193-223, §7 224-246, §8 247-275, §9 276-293).
9. `docs/automation/decisions/I-62.md`: §34-§41, líneas 650-795, por rangos. Y `docs/automation/evidence/I-62-evidence.md`: §40-§44, líneas 1779-2022,
   por rangos.
10. `docs/initiatives/I-62-portabilidad-coordinador-principal.md` (alcance y no-objetivos).
11. Contratos F3 materializados (inactivos):
    - `docs/AUTOMATION_PLAN.md`: §16 de I-61, líneas 425-845; 16.20-16.24, líneas 846-1105; por rangos;
    - `docs/automation/agent-execution/README.md`: §13-§16, líneas 287-629;
    - los esquemas `docs/automation/agent-execution/schemas/{role-invocation.v1,binding.v1,input-closure.v1,input-fidelity.v1,relay-record.v2,gate-contract.v2,reviewer-result.v1,architect-review-result.v1}.schema.json`.
12. `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` y `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md`.
13. Los archivos del run: `D:\r62-arch-a1r3-run\order.txt`, `D:\r62-arch-a1r3-run\prior-authorization.txt` y `D:\r62-arch-a1r3-run\prompt.md`.

**Transitivos permitidos.** Los obligan `CLAUDE.md`, `AGENTS.md` y el Context Pack `documentation-governance`; léelos si tus instrucciones te lo piden:
- `CLAUDE.md`, `AGENTS.md`, `README.md`;
- `docs/HANDOFF.md` (448 804 bytes; por rangos);
- `docs/WORKFLOW.md`, `docs/ARCHITECTURE.md`, `docs/FOUNDATIONS.md`;
- `docs/ROADMAP.md` (168 867 bytes; por rangos);
- `docs/context-packs/README.md`, `docs/context-packs/documentation-governance.md`;
- `docs/initiatives/README.md` (por rangos), `docs/initiatives/PROMPT_TEMPLATES.md`.

## Contrato de acciones (cerrado: nada fuera de esta lista)

El invocador audita después cada llamada contra esta misma tabla. La lista blanca del auditor es idéntica a ella, y cada ruta se comprueba por su
efecto: resuelta contra el directorio actual y clasificada como archivo del cierre, archivo del run, salida propia, raíz del clon o del worktree, o
fuera. No se compara la cadena literal del comando.

| Id | Acción permitida |
|---|---|
| ID-1 | `git`, con `-C D:/r62-arch-a1r3`, con `-C <tu worktree>` o desde el directorio del clon o del worktree. Subcomandos: `rev-parse HEAD`; `rev-parse <commit>:<ruta del cierre>`; `cat-file -p\|-t\|-s <commit>:<ruta del cierre>`; `status [--porcelain]`; `log --oneline [-N]`; `worktree list`; `config --get <clave>` |
| ID-2 | `git checkout --detach 9dcfc08d5171b01df97b3a492a60218c5ab2453e` en tu worktree, solo si su HEAD no es el commit |
| NAV-1 | `cd` hacia el clon, tu worktree o un directorio de tus salidas propias. Las rutas relativas posteriores se resuelven desde ahí |
| RD-1 | `git -C D:/r62-arch-a1r3 show 9dcfc08d:<ruta del cierre>` (o `HEAD:<ruta del cierre>`, con el HEAD ya verificado en el paso 0), sola o con tubería a `sed -n`, `head`, `tail`, `wc`, `grep`, `awk` o `sha256sum` |
| RD-2 | `sed -n`, `wc`, `awk`, `grep` (sin `-r`, `-R` ni `--recursive`), `head`, `tail`, `cat`, `sha256sum`, `echo`, `printf` y `true`. Solo sobre **archivos** del cierre (bajo el clon o tu worktree), sobre los tres archivos del run o sobre tus salidas propias. Combinables con `\|`, `;`, `&&` y `\|\|`. Redirección solo hacia tus salidas propias o `/dev/null`. Ningún directorio como argumento |
| EX-1 | `python D:/r62-arch-a1r3/docs/automation/evidence/I-62-A1/a1-counterexamples.py <archivo en tus salidas propias>`: exactamente un argumento, y debe ser una salida propia. Además, `python` con heredoc, con `-c` o con un script propio que hayas escrito en esta sesión (con Write o con `cat > <salida propia> <<EOF`), siempre que todas sus rutas literales sean archivos del cierre, archivos del run o salidas propias. Sin listar directorios (`os.listdir`, `os.walk`, `glob`, `iterdir`), sin procesos (`subprocess`, `os.system`), sin red y sin borrar ni mover archivos |
| EX-2 | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` dentro de `D:\r62-arch-a1r3`, desde Bash o PowerShell (SDK en `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`). Cualquier otra ruta que pases, como el log, `--results-directory` o `--logger`, debe estar en tus salidas propias. La orden no trae exención: si sigues el paso 4 de «Leer primero» de AGENTS.md, ejecútalo aquí. Es una comprobación local tuya, no evidencia de gate |
| OWN-1 | Leer (Read, `cat`, `tail`, `grep`, o `Get-Content` en PowerShell) o escribir (Write o redirección) tus **salidas propias**: archivos que tú mismo creas en tu scratchpad o en tu directorio de tareas |

Variables de shell: solo por asignación simple (`X=valor`), sin `export`.

**Herramientas:**
- **Read**: solo archivos del cierre, los tres archivos del run y tus salidas propias.
- **Grep**: solo con `path` = un **archivo** del cierre, sin `glob`. Nunca uses un directorio, ni siquiera para contar: una búsqueda sobre un directorio
  es una lectura fuera del cierre aunque no devuelva contenido.
- **Write**: solo hacia tus salidas propias.
- **TodoWrite, TaskCreate, TaskUpdate, TaskList y ToolSearch**: permitidas como contabilidad, sin efecto sobre archivos. Ninguna otra herramienta.
- **Bash o PowerShell**: solo para las acciones de la tabla. No hay listados de directorios.

**Prohibido:**
- cualquier archivo fuera del cierre, en particular:
  - `~/.claude/projects/*`, otras sesiones y transcripciones;
  - `~/.codex/worktrees/*` y el repositorio de trabajo;
  - los clones `D:\r62-arch-*` distintos de `D:\r62-arch-a1r3`;
  - los archivos de `D:\r62-arch-a1r3-run\` distintos de los tres del run;
- red y web;
- subagentes u otros agentes;
- editar, hacer commit o push;
- crear una A-n;
- decidir materias del Owner.

Al terminar, el clon debe seguir limpio y en el commit; el auditor lo comprueba. Las salidas de `dotnet test` en `bin/` y `obj/` están ignoradas por Git.

## Forma de lectura (fidelidad)

- **Read:**
  - usa la ruta absoluta;
  - en los archivos de más de 60 000 bytes, usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura grande por llamada, para que ninguna salida se trunque.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: V14 2352, 2380 y 2382; decisiones 657, 734, 750, 768 y 787; ROADMAP 408-501.
  - Si las necesitas, léelas con `git -C D:/r62-arch-a1r3 show 9dcfc08d:<ruta> | sed -n 'Np'`.
  - El auditor anota una línea larga conocida que llegue truncada por Read, pero no la cuenta como entregada.
- **Premisas.** Cita como premisa (`PremiseRefs`) solo líneas que hayas leído enteras con Read o con ese `git show | sed -n`. El invocador comprueba que
  esas líneas te llegaron fielmente.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ⇒ ⇔ ∈ ∉ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerlo fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Qué decidir

Contesta los focos 1-16, cada uno una vez:
- **1-12:** los FOCUS 1-12 de `prior-authorization.txt`, aplicados a la A-1 corregida (blob `39c2f831`).
- **13 — A62-A1R-01** (`order.txt` §5). El cierre del REVIEWER tras EXHAUSTED, junto a EXPIRED y REVOKED:
  - con decisión del Coordinator y sin reescribir el motivo;
  - sin satisfacer el requisito operativo;
  - con los BLOCKING abiertos y la operación dependiente bloqueada;
  - sin trabajo vivo incompatible;
  - alineado en texto, estados, historial y C-38.
- **14 — A62-A1R-02.** Una regla coherente de rebase por tipo:
  - REVIEWER: cuándo cambia su `loop.object` por cierre y cuándo solo su commit por reconciliación;
  - EXECUTION: una regla única, compatible con la autoridad congelada, y el delta observable declarado con su materialidad.
- **15 — A62-A1R-03.** No resucitar autorizaciones del REVIEWER:
  - apertura con autoridad vigente y comprobación de `reviewer_closures`;
  - continuación solo con la decisión custodiada;
  - historial y presupuesto conservados;
  - deltas de vigencia de 16.20 y README §14.3 como propuesta (D1-21);
  - M-05 de OBS-A1-01 = SÍ.
- **16 — Opcionales de la revisión anterior.**
  - O1, O3, O4 y O5 incorporados.
  - O2 preparado con la autoridad existente. Si exigiera campos, permisos o política nuevos, debería figurar como delta propuesto explícito.

Dispón de forma explícita, como CLOSED o STILL_OPEN sobre la versión exacta, estos diez hallazgos: A62-A1-01..06, OBS-A1-01 y A62-A1R-01..03.
- No reabras A62-A1-01..06 sin un contraejemplo nuevo.
- Evalúa A62-A1-O1..O5 (de la primera revisión; O6 sigue rechazado por el Coordinator) y A62-A1R-O1..O5 (de la revisión formal anterior), cada uno una
  vez.

Un REQUIRED nuevo existe solo si el defecto es material según LIFECYCLE §5, y debe traer:
- id estable `A62-A1S-NN`;
- delta o sección exacta de A-1;
- premisa canónica completa en `PremiseRefs`;
- contraejemplo concreto;
- corrección exacta.

Una precisión sin cambio de significado es OPTIONAL (`A62-A1S-ON`). Si un contrato F3 necesita un cambio real de forma de esquema, es REQUIRED, con el
contrato exacto y el contraejemplo.

## Resultado

Tu último mensaje contiene **solo** un bloque ```json con este objeto (sin texto fuera del bloque). Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-architect-review-result/v1 (representación experimental; rigen I-61 y LIFECYCLE)",
  "RunId": "R20261005T063359Z-86e3", "InvocationId": "I20261005T063359Z-86e3", "LogicalReviewRequestId": "L20261005T063359Z-86e3", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN",
  "ReviewedUnit": "I-62", "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-1.md", "ReviewedBlob": "...",
  "ReviewerMode": "SEPARATE SESSION",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora y el operador humano",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción"]},
  "IdentityCheck": {"CloneHeadMatches": true, "WorktreeHeadMatches": true, "ObjectBlobMatches": true, "PackageBlobMatches": true, "CleanTree": true,
                    "OrderSha256Matches": true, "PriorAuthorizationSha256Matches": true},
  "InputsRead": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A1S-01", "AffectedDelta": "...", "ViolatedInvariant": "...", "Counterexample": "...", "CorrectionRequired": "...",
                        "PremiseRefs": [{"Path": "...", "Section": "...", "LineStart": 1, "LineEnd": 1, "Quote": "proposición completa"}], "WhyItMatters": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A1S-O1", "AffectedDelta": "...", "Note": "...", "PremiseRefs": []}],
  "FindingDispositions": [{"FindingId": "A62-A1-01 | … | A62-A1-06 | OBS-A1-01 | A62-A1R-01 | A62-A1R-02 | A62-A1R-03", "State": "CLOSED | STILL_OPEN", "Rationale": "...", "PremiseRefs": []}],
  "OptionalCorrections": [{"Id": "A62-A1-O1 | … | A62-A1-O5 | A62-A1R-O1 | … | A62-A1R-O5", "Assessment": "CONFIRMED | CHALLENGED | NOT_APPLICABLE", "Note": "..."}],
  "Focus": [{"Item": 1, "Assessment": "CONFIRMED | CHALLENGED", "Note": "..."}],
  "F3ContractCrossCheck": [{"Contract": "...", "Assessment": "COMPATIBLE | TEXT_AMENDMENT_DECLARED | REQUIRES_SCHEMA_CHANGE", "Note": "..."}],
  "CounterexampleVerification": {"Traces": 94, "Valid": 32, "Invalid": 62, "AllAsExpected": true, "Notes": [{"Trace": "...", "ClaimedReasonHolds": true, "Note": "..."}]},
  "Materiality": {"Overall": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"},
                  "ObsA101": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"}, "Note": "tu evaluación"},
  "OwnerAuthorityCheck": {"Spending": "NONE | ...", "Credentials": "NONE | ...", "Destructive": "NONE | ...", "NewOVScenario": "NONE | ...",
                          "OVRemovalOrReassignment": "NONE | ...", "OwnerPolicyChoice": "NONE | ...", "NewOwnerAuthority": "NONE | ...", "OwnerDecisionRequired": false},
  "LegacyApplicability": {"AppliesOnlyToI62": true, "NoRetroactiveI61Change": true, "NoI64FixClaim": true, "Note": "..."},
  "IfAgreed": {"A62_A1_01_CLOSED": null, "A62_A1_02_CLOSED": null, "A62_A1_03_CLOSED": null, "A62_A1_04_CLOSED": null, "A62_A1_05_CLOSED": null,
               "A62_A1_06_CLOSED": null, "OBS_A1_01_CLOSED": null, "A62_A1R_01_CLOSED": null, "A62_A1R_02_CLOSED": null, "A62_A1R_03_CLOSED": null,
               "FC01Resolved": null, "FC02Resolved": null, "NoOwnerDecisionRequired": null, "SuitableForCoordinatorAgreement": null,
               "F4MayProceedAfterCoordinatorAgreed": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- `Materiality` lleva **tu** evaluación. Los valores de arriba son los de A-1 y del Coordinator, como referencia: M-05 de OBS-A1-01 = YES desde
  A62-A1R-03.
- Cobertura, una vez cada elemento:
  - `Focus`: los ítems 1-16;
  - `FindingDispositions`: los diez hallazgos;
  - `OptionalCorrections`: A62-A1-O1..O5 y A62-A1R-O1..O5.
- **Con AGREED:** cero REQUIRED, los diez hallazgos en CLOSED y los quince campos de `IfAgreed` en `true`. En cualquier otro caso, `IfAgreed` va todo en
  `null`.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
