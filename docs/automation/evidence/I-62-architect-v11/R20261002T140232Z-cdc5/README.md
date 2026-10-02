# I-62 — Revisión formal limpia del Architect de la Proposal V11 (R20261002T140232Z-cdc5)

```text
LogicalReviewRequestId: L20261002T140232Z-cdc5   InvocationId: I20261002T140232Z-cdc5   AttemptSeq: 1   RunId: R20261002T140232Z-cdc5
Autorización:   OWNER DECISION — FORMAL CLEAN ARCHITECT REVIEW OF I-62 PROPOSAL V11 (decisiones §23); UNA invocación de codex-cli, solo revisión
Objeto:         commit 26127a69a7dbc1324566b081381cd11db6b20f35 (CI de publicación 36973392633, push, 4/4 success)
                docs/initiatives/I-62-proposal-v11.md          blob 3e8fa9d8eb8a875069224e8ed7fd5850145e4d0e
                docs/initiatives/I-62-architect-package-v11.md blob 10fe74796ac94cbeaf1945ee3e47a60cb8f39ee4
Veredicto:      CHANGES REQUIRED (registro: docs/initiatives/I-62-architect-review-v11.md)
Fidelidad:      preflight FAITHFUL_NORMALIZED por el mismo camino; tras la corrida DEGRADED_BOUNDED sin degradación de caracteres; ningún hallazgo
                INVALID_PREMISE
Naturaleza:     evidencia de una invocación real. Los JSON son representaciones EXPERIMENTALES de los contratos de la Proposal V11 (§20.3.1, §20.3.2,
                §20.3.3, B.10.1); no son autoridad normativa. I-61 sigue vigente. IMPLEMENTATION AUTHORIZATION = NO
Medición:       la hizo la sesión autora (Claude), que lanzó el proceso; no hay observador independiente
```

## 1. Archivos custodiados

Todos son copia byte a byte de los del directorio de la corrida y usan saltos de línea LF. La identidad se comprueba con `git show <commit>:<ruta> | sha256sum`.

| Archivo | Bytes | SHA-256 | Papel |
|---|---|---|---|
| `prompt.md` | 15 781 | `aaa6bae438099537bdbfc68924fb0ee4fdd57c9c80e0afc56bff955a6f810de8` | prompt completo: cabecera con las formas de lectura obligatorias, el cierre y los hallazgos abiertos; y, literal entre marcas, la orden del Owner (6 415 bytes; SHA-256 `f412b8ec87ea7f56a615a328fd4061933164ccce962dd0f204f498c15b5091c4`) |
| `schema.json` | 6 985 | `ae43421b282d6d83e1a4390153d99c49d0c30ce591b70f29838152a2dc012915` | esquema de salida (`--output-schema`): representación experimental de B.10.1 de V11, con `PremiseRefs`, más los campos que pide esta orden (`RequirementAssessments`, `PreservedDispositionChecks`, `FreezeAssessment`, `FocusAreas`, `AutonomyAssessment`) |
| `closure.json` | 23 966 | `bd598fd5a1af0b45c44aea922098844e50141433e93fa10c30f793b1a6bbc72b` | `EffectiveInputClosure`: 48 insumos canónicos y 9 transitivos, cada uno con su blob, su SHA-256, su codificación y su fin de línea. También las formas de lectura permitidas, la exención de `dotnet test` (aparte), `HealthSignals`, las obligaciones no activadas, el contexto de runtime y los insumos prohibidos |
| `input-fidelity-preflight.json` | 75 839 | `c4ee320f2ac5e6940a4d6013ca31014208bb3d36c9b7e804c55e3373fb0bccf7` | preflight de fidelidad antes del lanzamiento (§3) |
| `input-fidelity-postrun.json` | 5 779 | `02b9dded2ae6860e4a0bc4ed4ccb0928632d41d261c328036e21299d8c2c7550` | comprobación de fidelidad tras la corrida y de las premisas (§5) |
| `output.json` | 40 630 | `cf731d6ca6271121b8a2c9fdf50b8d5b60e19a59dfb5c59cc945a33b35e83cc6` | resultado literal del Architect (`-o`) |
| `read-audit.json` | 25 665 | `80fd5339a03eb9377683c474f6c3fd600c3d677923ad230fc60a1d734f736c60` | auditoría de lecturas frente al cierre |
| `runtime-evidence.json` | 4 310 | `91e6aa80ca772ee31a971542a2fc64a80c06930bf0b84b1df1a102e1ccbdf51e` | identidad observada por el invocador (`RuntimeEvidenceRef`) |

Archivos que **no** se versionan. Se registran su identidad y los campos extraídos:

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `events.jsonl` (salida `--json`) | 2 040 392 | `658fc0b177b9c195739017d510763974be80db633db76bd3320bb6b519fcc0df` |
| log de sesión (`rollout-2026-10-02T08-11-33-01a0fcf4-bff5-77d3-8db4-a104dc451ee5.jsonl`, 324 líneas) | — | `663f86dd849f897890dd597cd8130fb888a3ffb296c7b73a7d73b4ba18d824b8` |
| `stderr.txt` | 39 | `1aa26269eb1cc57f86b235a03cda53c004edb5b1e9fc99d4da4f00843293d721` |

## 2. Excepción del Owner (texto exacto)

Sección literal de la decisión del Owner, 662 bytes en UTF-8, SHA-256 `9f110074c7dead8d67bba021edcd9763e282edaf5b49431ce158463a688573e4`:

```text
DOTNET TEST EXCEPTION
For THIS Architect invocation only:
DO NOT execute `dotnet test`.
This is an explicit Owner exemption of that initial repository action for this exact read-only Architect invocation.
The publication CI may be supplied as a separate canonical publication-health signal.
It is NOT equivalent to local Core evidence and does not satisfy or replace any local-test evidence class.
This exemption:

* applies only to this invocation;
* applies only to `dotnet test`;
* does not modify AGENTS;
* does not establish precedent for Candidate, gate, implementation or closure evidence.

Without this explicit exemption the invocation would not launch.
```

En el cierre figura como `ExemptedActions` (la acción se **omite**), separada de `HealthSignals` (la CI 36973392633, que no es evidencia equivalente). `dotnet test`
no se ejecutó (`read-audit.json`).

## 3. Preflight de fidelidad por el mismo camino (MEASURED antes de lanzar, 14:00Z-14:10Z)

**Camino.** `codex sandbox` (el sandbox de Windows de `codex-cli` 0.159.2) → el `pwsh` del runtime de Codex con `-NoProfile -Command <script>`. Es el mismo
binario, la misma cuenta de sandbox y el mismo `PATH` que usa `codex exec`. No reproduce la captura interna de `codex exec`: la comprobación tras la corrida es
la decisiva.

**Hechos del runtime (medidos):**
- `pwsh` corre en **ConstrainedLanguage**, con la salida de consola en la **página 850**;
- leer con `Get-Content` en ese `pwsh` degrada los caracteres no ASCII (control negativo: 3 571 U+FFFD, los 21 «≤» y los 357 «→» perdidos);
- `[Console]::OutputEncoding` no se puede asignar en ese modo, y `chcp` no cambia la codificación ya fijada;
- la consola del sandbox no hereda la página 65001 del proceso padre.

**Formas de lectura acreditadas** (las únicas que la cabecera permitió):

| Forma | Script | Resultado |
|---|---|---|
| A, archivo completo | `cmd /c type <ruta con barras invertidas>`: los bytes del comando nativo atraviesan el `pwsh` sin conversión | 57/57 insumos idénticos tras normalizar el fin de línea; los 61 caracteres no ASCII del corpus, conservados |
| B, rango | `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Get-Content -LiteralPath <ruta> -Encoding utf8 \| Select-Object -Skip N -First M"'`: un `pwsh` anidado que arranca con la página 65001 | 57/57 insumos completos idénticos; 3/3 rangos idénticos |
| C, búsqueda | `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Select-String … \| ForEach-Object { [string]$_.LineNumber + [char]58 + $_.Line }"'` | 7/7 búsquedas con patrones no ASCII (≤, ⊆, ✓, «, ↔, ∅ y uno ASCII), todas las líneas idénticas a las canónicas |
| Git | `git log --oneline -10`, `rev-parse`, `status` | iguales a la salida de Git local |

Se rechazó `Select-String` con su formato por defecto: los caracteres llegan bien, pero parte las líneas largas al ancho de consola y añade escapes ANSI, lo
que es degradación de estructura. El corpus de 61 caracteres no ASCII es:
`§ª«°±·»¿ÁÉÍÑÓ×ÚÜáéíñóúüΔα–—“”…′←→↓↔⇒⇔∅∈∉−∖∧∩∪≠≡≤≥⊆⊇─│┐┘├┤✅✋✓` y U+FFFD literal, este último presente en la evidencia canónica de V10.

**Estado del preflight: FAITHFUL_NORMALIZED.** Las pruebas no modificaron la configuración ni el clon.

## 4. Condiciones, ejecución e identidad observada (MEASURED; detalle en `runtime-evidence.json`)

- **Antes de lanzar:**
  - `codex-cli 0.159.2`, SHA-256 `fcd5eafe…`, con la autenticación existente;
  - `config.toml` con el mismo hash antes y después (`40c27b57…`);
  - clon limpio `D:\r62-arch-v11` en `26127a69`, con los blobs verificados y la rama remota en el mismo SHA;
  - ningún proceso sobre rutas de I-62 ni del clon, y `memories` desactivado;
  - el prompt se pasó como argumento con `MSYS_NO_PATHCONV=1`, para que Git Bash no reescribiera los «/c» como rutas.
- **Ejecución:** de 14:11:32Z a 14:31:04Z (19 min 32 s), salida 0, con `turn.completed`. Consumo: entrada 3 807 092 tokens (3 479 552 en caché) y salida
  23 956 (6 827 de razonamiento).
- **Identidad observada** (RUNTIME_OBSERVED): hilo `01a0fcf4-bff5-77d3-8db4-a104dc451ee5`. Los dos `turn_context` dicen `gpt-6.1-sol`, `high`, `never` y
  `read-only`. Hubo **una compactación** de contexto a las 14:20:00Z, dentro del mismo hilo.
- **Fidelidad del prompt:** el texto que recibió el revisor es igual a `prompt.md` salvo el salto final, con 108 caracteres no ASCII y ningún U+FFFD.
- **Después:** clon en `26127a69`, limpio y sin archivos ignorados; configuración sin cambio; ningún proceso vivo.
- **Lecturas:** 96 comandos, todos de lectura: 52 de forma A, 27 de forma B, 13 de forma C y 4 de Git. Leyó 51 rutas, todas del cierre. Ninguna lectura
  directa en el `pwsh` exterior, ningún comando de escritura y ningún `dotnet test`.

## 5. Fidelidad tras la corrida y premisas (MEASURED; `input-fidelity-postrun.json`)

| Comprobación | Resultado |
|---|---|
| lecturas A aisladas frente al blob | 50 de 52 idénticas. En la del comando 1 (paquete V11), el runtime antepuso un diagnóstico de `profile.ps1`, con el contenido completo; se releyó íntegro y fiel en el comando 68. En la del comando 2 (HANDOFF, 438 341 caracteres), la captura de `codex exec` **truncó el centro**: faltan las líneas canónicas 4639-4672 (3 310 caracteres) |
| lecturas B frente al tramo canónico | 27/27 idénticas |
| búsquedas C frente a las líneas canónicas | 13/13 idénticas |
| degradación de caracteres | **ninguna** |
| `FidelityStatus` | **DEGRADED_BOUNDED**: dos tramos estructurales acotados y ninguna degradación de caracteres |
| premisas (`PremiseRefs`) | 42: 35 citas exactas y 7 de sección completa. Ninguna sin coincidencia y ninguna en un tramo degradado. Los archivos citados (Proposal y paquete V11, disposición de V10, registros de V6 y V7) se leyeron íntegros y fieles |
| hallazgos sin acreditar (`unaccredited`) | **ninguno**: todos los hallazgos, disposiciones y evaluaciones son independientes según §20.3.3 |

Bajo V11, un resultado DEGRADED_BOUNDED nunca produce ARCHITECT_SATISFIED; el veredicto es, en todo caso, CHANGES REQUIRED. La independencia aquí no descansa
solo en coincidencias de subcadena (cf. A62-V11-01): los archivos citados no tienen ningún tramo degradado.

## 6. Límites

1. La medición la hizo la sesión autora, que lanzó el proceso.
2. El modelo servido detrás del proveedor no es observable.
3. El registro no captura las lecturas internas del runtime.
4. El preflight usa `codex sandbox`, no la captura interna de `codex exec`. El truncamiento central de salidas muy grandes solo se vio tras la corrida
   (GAP-10, evidencia §25).
5. La compactación ocurrió dentro del hilo, y su efecto sobre el razonamiento no es observable.
