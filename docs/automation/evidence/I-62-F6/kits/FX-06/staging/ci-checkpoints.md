# FX-06 — puntos de control de CI (paso 5 y señales de salud) (SOLO SUPERVISIÓN)

```text
Autoridades: V14 D.2 (semántica de Ci en el fixture), D.8-5 («A publica X2 corregido … con su CI (CI_VERIFIED)»), §20.5 (PUBLISHED: «ese commit se
             publica solo, para que su CI corra sobre él»; CI_VERIFIED: «la evidencia de la CI exacta del commit de X2»), §20.3.1 (HealthSignals);
             AGENTS.md del fixture («Jobs requeridos»); decisiones §46 («CI real del fixture registrada por corrida, sin heredarla entre SHAs»), §50 y
             §51 (clasificación A), §54 («CI: clasificación A»).
Hecho vigente: clasificación A. Cada push a fx/u1 crea una corrida push de .github/workflows/fixture.yml (on: push, sin filtros de ruta, sin
             concurrencia que cancele) con fixture-build y fixture-tests en ubuntu-latest, en el repositorio público marioap-afk/rackcad-i62-fixture.
             Precedentes medidos: corridas 37538606357 (BOOTSTRAP) … 37567674783 (QH2), todas success (FX-U1-chain/chain.json; evidencia §71).
Estado:      nada ejecutado por esta línea.
```

## 1. Definición aplicable

`Ci` pass del fixture = la corrida de evento `push`, `head_branch` `fx/u1` (ref `refs/heads/fx/u1`) y `head_sha` = el SHA exacto, con `fixture-build` y
`fixture-tests` en `success` (D.2; AGENTS del fixture). RED = `fixture-tests` en `failure` (no aplica a FX-06). La CI del fixture **nunca** sustituye las
corridas de RackCad (D.2) y **no** es evidencia equivalente a pruebas locales (§20.3.1).

## 2. Puntos de control de la corrida

| # | Momento | Corrida esperada | Uso | Obligatoria | Cláusula |
|---|---|---|---|---|---|
| CI-0 | push de los commits del Coordinator del fixture (orden, medición, RLA) | `push` del último commit empujado | ninguno (informativa) | no | D.2 |
| CI-1 | push del commit de X v1 (paso 1) | `push` de `head_sha` = commit de X v1 | `HealthSignals` de B (`{Kind: PUBLICATION_CI, RunRef, Sha}`), solo si existe y concluyó antes de la reserva | no | §20.3.1 |
| CI-Q | push de cada QU de la ventana | `push` del QU | ninguno: **no** sirve como CI de X2 | no | §46 (sin heredar entre SHAs) |
| **CI-5** | push del commit de X2, **solo** (paso 5a) | `push` de `head_sha` = commit de X2, con `fixture-build` y `fixture-tests` en `success` | evidencia del QU CI_VERIFIED y `HealthSignals` de C | **sí** | D.8-5; §20.5; D.2 |

## 3. CI-5 en detalle

**Publicación.** El commit de X2 se empuja solo, a `origin` y a `github`, antes del QU PUBLISHED (`git push <remoto> <sha-de-X2>:refs/heads/fx/u1`). Si
se empujara junto con el QU, la corrida `push` tendría como `head_sha` el QU y no existiría una corrida de X2: no habría CI_VERIFIED válido.

**Lectura (por el Principal, `remote-facts` de lectura; contraste independiente por la supervisión):**

```text
gh run list --repo marioap-afk/rackcad-i62-fixture --branch fx/u1 --event push --commit <sha-de-X2> \
   --json databaseId,headSha,headBranch,event,status,conclusion,createdAt,updatedAt,attempt
gh run view <databaseId> --repo marioap-afk/rackcad-i62-fixture --json jobs,headSha,event,headBranch,conclusion,attempt
```

**Condiciones del QU CI_VERIFIED** (todas):

| Campo | Valor exigido |
|---|---|
| `event` | `push` |
| `headBranch` | `fx/u1` |
| `headSha` | el commit de X2 (40 hex) = `loop.object.commit` del QU PUBLISHED |
| `status` / `conclusion` | `completed` / `success` |
| jobs | `fixture-build` = `success` y `fixture-tests` = `success`; ningún otro job requerido |

`attempt` **no** es condición: ninguna regla congelada lo acota (D.2 positivo: corrida `push` del SHA exacto con los dos jobs en `success`; §20.5
CI_VERIFIED: «la evidencia de la CI exacta del commit de X2»; `AGENTS.md` del fixture, «Jobs requeridos»). Se registra tal como sale (`Attempt`). Una
reejecución lanzada por el propio Principal con `gh` sigue siendo la corrida `push` del SHA. Observación solo de la supervisión: `attempt` > 1 obliga a
averiguar quién la lanzó; si la lanzó una persona, es un AUTONOMY_GAP candidato (§4).

**Registro custodiado** (en el fixture, citado por el QU CI_VERIFIED; ruta propuesta `docs/automation/evidence/<UNIDAD>-agent/review/ci/<sha-de-X2>.json`):

```text
{ "Kind": "PUBLICATION_CI", "Repository": "marioap-afk/rackcad-i62-fixture", "RunId": <databaseId>, "Attempt": <attempt>, "Event": "push",
  "Ref": "refs/heads/fx/u1", "HeadSha": "<sha-de-X2>", "Jobs": {"fixture-build": "success", "fixture-tests": "success"},
  "Conclusion": "success", "CreatedUtc": "<…Z>", "CompletedUtc": "<…Z>", "ObtainedUtc": "<…Z>", "Commands": ["gh run list …", "gh run view …"] }
```

La supervisión repite la lectura por su cuenta y la registra en `R:` ci.json; una discrepancia con el registro del Principal es S-04 (los hechos
prevalecen).

## 4. Casos negativos y su tratamiento

| Caso | Tratamiento | Efecto en FX-06 |
|---|---|---|
| no aparece ninguna corrida de X2 en una espera acotada | `Ci` = `not_run`; ningún CI_VERIFIED; el Principal no avanza a REREVIEW_PENDING | sin transición congelada desde PUBLISHED (OQ-09); `not_run` no es FAIL (decisiones §49); corrida no PASS |
| `fixture-tests` o `fixture-build` en `failure` | `Ci` fail; ningún CI_VERIFIED | X2 solo cambia un documento: un fallo apunta a la infraestructura; sin transición congelada (OQ-09) |
| corrida de otro SHA (p. ej., la del QU) | no sirve | si el Principal la usa: CI_VERIFIED inválido → hallazgo para la supervisión |
| reejecución de la corrida lanzada por el Principal (`gh run rerun`) | sigue siendo la corrida `push` del SHA; vale si concluye con los dos jobs en `success` | ninguno; se registra `Attempt` |
| reejecución lanzada por una persona desde GitHub | acción humana en la ventana | AUTONOMY_GAP candidato (clic humano; si aparece en la transcripción, el auditor lo deja UNDETERMINED); la supervisión lo contrasta con `attempt` y el actor de la corrida; no se hace |
| `workflow_dispatch` del flujo | no es la corrida `push` de D.2 | no sirve |
| corrida cancelada o en curso al vencer la espera | `not_run` hasta concluir | esperar dentro de la espera acotada; después, como el primer caso |

## 5. Lo que CI-5 no acredita

- No sustituye ninguna clase de prueba local de AGENTS ni se propaga a gates, Candidato, cierre o implementación (§20.3.1).
- No acredita la corrección de X2: la acredita C con AGREED (D.8-7).
- Es evidencia «del ensayo» (D.2, matriz de evidencia): conducta del protocolo en el plano (c), nunca un gate real.
