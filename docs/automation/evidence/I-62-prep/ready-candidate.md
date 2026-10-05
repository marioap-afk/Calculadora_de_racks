# I-62 — READY-01..09, conformidad de READY-06 y paquete del Candidato (preparación)

> **Preparación.** Fuente: LIFECYCLE §6 y §8-§9, WORKFLOW §4 y §11, AGENTS (clases de evidencia) y Proposal V14 §17-§18. Nada de esto designa un Candidato ni
> declara READY. Estado al 2026-10-04: F0-F3 COMPLETE; F4 no abierto (bloqueado en A-1 para la parte afectada); F6/F7 no abiertos.

## 1. Lista ejecutable (en este orden; un SHA nuevo reinicia desde READY-02)

| READY | Decide | SHA de entrada | Evidencia exigida | Invalidadores | ¿Satisfacible hoy? | Comprobación |
|---|---|---|---|---|---|---|
| 01 alcance congelado completo | Coordinator (+ Owner si OWNER-RESERVED) | Freeze `b64a3b64` + A-n versionadas | todas las A-n acordadas y visibles (hoy A-1 PROPUESTA); diferimientos decididos | una A-n nueva o pendiente | **no**: A-1 pendiente | `git log --oneline -- docs/initiatives/I-62-A-*.md`; leer cada A-n y sus veredictos |
| 02 gates cerrados | Coordinator | punta de la rama | GATE PASS de F1-F4, F6 y F7 en decisiones; documentos versionados | cualquier commit posterior que cambie el SHA | **no**: F4, F6 y F7 abiertos | decisiones §§ de cada GATE PASS; `git status` limpio |
| 03 sin pendientes | Owner + Coordinator | ídem | OD-1 aceptada; OD-2..OD-7 resueltas o con su limitación decidida; cero REQUIRED abiertos | una decisión nueva | **no**: OD-1..OD-7 abiertas | tabla de `owner-decision-packets.md` con respuesta registrada |
| 04 rebase final | sesión, bajo WORKFLOW | `origin/main` recién obtenido | `git fetch`; rebase sin conflictos; si hubo cierre previo, su ruta de retorno | avance de `main` | sí, en su momento | `git fetch origin && git rebase origin/main`; `git range-diff` registrado |
| 05 CI y focales del SHA resultante | AGENTS | SHA tras READY-04 | Core Full y UI Full locales sobre el SHA exacto; CI `push` con los cuatro jobs en `success` | cualquier commit | sí, en su momento | `dotnet test tests/RackCad.Tests/...` (SDK de usuario) y UI; `gh run view <id> --json headSha,jobs` |
| 06 conformidad | Architect + Coordinator | el mismo SHA | `CONFORMING` de ambos contra Freeze + todas las A-n (plantilla de §2) | un rebase posterior (LIFECYCLE §9: repetir completa) | no hasta READY-05 | registro de conformidad con el SHA exacto |
| 07 árbol limpio | sesión | ídem | `git status --porcelain` vacío; `HEAD` = remoto | cualquier cambio local | sí | `git status --porcelain`; `git ls-remote origin <rama>` |
| 08 matriz OV completa | Coordinator + Owner | ídem | OV-I62-01..06 asignados a I-62 y preparados para el SHA final; ninguno retirado sin el Owner | retirar o reasignar un escenario | **no**: asignación OV pendiente | `ov_assignment_ref` del contrato; `ov-scripts.md` |
| 09 identidad del Freeze y de las A-n | Coordinator | ídem | acuerdo exacto; diff permitido; Freeze inmutable desde su commit; A-n append-only y visibles desde la rama | una edición del Freeze o de una A-n | sí, en su momento | `git log --format=%H -- docs/initiatives/I-62-consensus-freeze.md docs/initiatives/I-62-proposal-v14.md` sin cambios tras `b64a3b64`; secuencia A-1.. sin huecos |

**Solo entonces** se declara `FINAL_CANDIDATE_SHA`. Las evidencias que solo existen después (PRE/POST, blob del mapa en EFF, C-20b sobre el merge local, CI
posterior al merge, tag) pertenecen a la integración.

## 2. Plantilla del paquete de conformidad de READY-06

```text
I-62 — CONFORMIDAD FINAL (READY-06)
SHA evaluado          = <40 hex, tras READY-04/05>   CI = <run id> 4/4   Core Full = <n/n>   UI Full = <n/n>
Freeze                = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43  (Proposal V14 4c617e82… / 34ad80ea…; OD-6 = alternativa 1)
A-n aplicables        = A-1 (<estado y veredictos>), …                      Decisiones aprobadas = decisiones §§…
Modo de cada revisión = <SEPARATE SESSION | EXTERNAL HUMAN>; mismo autor y revisor: <sí/no>   (OD-6 alt. 1 no es retroactiva a I-62)

Por arquetipo y obligación (Anexo C):
| Obligación | Evidencia (ruta, SHA) | Resultado | Desviación (EDITORIAL | NON-MATERIAL | BEHAVIORAL-WITHIN-FREEZE | MATERIAL | OWNER-RESERVED) |
| C-01 … C-42 |  |  |  |

Desviaciones abiertas: <ninguna | lista con autoridad del dominio>
Resultado del Architect:   CONFORMING | NON-CONFORMING
Resultado del Coordinator: CONFORMING | NON-CONFORMING
Un rebase posterior obliga a repetir esta conformidad completa sobre el SHA nuevo (range-diff, patch-id o igualdad de árbol no la trasladan).
```

## 3. Plantilla del paquete del Candidato final

```text
I-62 — FINAL_CANDIDATE_SHA = <40 hex>
READY-01..09            = <decisión/evidencia de cada uno, en orden>
Freeze + A-n            = b64a3b64 + <A-1…>
Owner                   = OD-1 <…>; OD-2 <…>; OD-3 <…>; OD-4 <…>; OD-5 <…>; OD-7 <…>; limitaciones decididas <…>
Evidencia exact-SHA     = Core Full <n/n> (TRX SHA-256 …); UI Full <n/n>; CI <run id> push, head_sha exacto, 4/4
Builds                  = UI (WPF), Plugin sin AutoCAD (CI)
Matriz OV               = OV-I62-01..06 sobre este SHA, fixture resembrado desde el Candidato (D.5); resultados: <PASS | UNVERIFIED/UNSUPPORTED con causa y decisión>
Conformidad (READY-06)  = Architect CONFORMING (<registro>); Coordinator CONFORMING (<registro>)
Pendiente de integración = cierre documental (closure-plan.md), merge efectivo con PRE, mapa regenerado (C-20b), POST y tag integration/I-62
```
