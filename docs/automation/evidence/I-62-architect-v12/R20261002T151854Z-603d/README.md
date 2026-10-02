# I-62 — Revisión formal limpia del Architect de la Proposal V12 (R20261002T151854Z-603d)

```text
LogicalReviewRequestId: L20261002T151854Z-603d   InvocationId: I20261002T151854Z-603d   AttemptSeq: 1   RunId: R20261002T151854Z-603d
Autorización:   OWNER / COORDINATOR AUTHORIZATION — FORMAL CLEAN ARCHITECT REVIEW OF I-62 PROPOSAL V12 (decisiones §25); UNA invocación de codex-cli
Objeto:         commit d0947c5d58280f8b0bfe2808dd8b7ff79e9f76bb (CI de publicación 37024119231, push, 4/4 success)
                docs/initiatives/I-62-proposal-v12.md          blob 320cecc9bd1b68112509e510b97c768b6d505ba2
                docs/initiatives/I-62-architect-package-v12.md blob f60873d15f4b62e1313bb03c9136076acbd08012
Veredicto:      CHANGES REQUIRED (registro: docs/initiatives/I-62-architect-review-v12.md)
Fidelidad:      preflight FAITHFUL_NORMALIZED por el mismo camino; tras la corrida DEGRADED_BOUNDED, sin degradación de caracteres; ningún hallazgo
                INVALID_PREMISE. La comprobación decisiva usa lo que VIO el modelo (log de sesión), no solo la captura de events.jsonl
Naturaleza:     evidencia de una invocación real. Los JSON son representaciones EXPERIMENTALES de los contratos de la Proposal V12 (§20.3.1, §20.3.2,
                §20.3.3, B.10.1); no son autoridad normativa. I-61 sigue vigente. IMPLEMENTATION AUTHORIZATION = NO
Medición:       la hizo la sesión autora (Claude), que lanzó el proceso; no hay observador independiente
```

## 1. Archivos custodiados

Todos son copia byte a byte de los del directorio de la corrida y usan saltos de línea LF. La identidad se comprueba con `git show <commit>:<ruta> | sha256sum`.

| Archivo | Bytes | SHA-256 | Papel |
|---|---|---|---|
| `prompt.md` | 15 741 | `7e31b136945edeb22e5524c1e38271eab52985963b9720f20f2ffc71d5d112ca` | prompt completo: cabecera con las formas de lectura obligatorias, la política de rangos, el cierre y los hallazgos abiertos; y, literal entre marcas, la orden del Owner y del Coordinator (6 177 bytes; SHA-256 `deba08bb25684275c89969936642820ac74ab8c0b1d31cb0b26c8595fb8e8796`) |
| `schema.json` | 10 060 | `8beabf5b6253fb1be45ef1335d732ba74e4cf4ac7b662a7ce1e16f930f9d44ea` | esquema de salida (`--output-schema`): representación experimental de B.10.1 de V12, con `PremiseRefs` de proposición completa (`Path`, `Section`, `LineStart`, `LineEnd`, `Quote`), más los campos que pide la orden |
| `closure.json` | 26 352 | `2f0010c9eb3b4a060f7a7d02d53c48c637c34f09383047a73eecda67d640a696` | `EffectiveInputClosure`: 48 insumos canónicos y 9 transitivos, cada uno con su blob, su SHA-256, su codificación y su fin de línea. También la política de lectura (rangos para los archivos grandes), la exención de `dotnet test` (aparte), `HealthSignals`, las obligaciones no activadas, el contexto de runtime y los insumos prohibidos |
| `input-fidelity-preflight.json` | 55 338 | `ea36ffe4efde568289660f3e42ae2b0b2f384d91544659ad9edbe704f27d1fe0` | preflight de fidelidad antes del lanzamiento (§3) |
| `input-fidelity-postrun.json` | 31 032 | `82231e42903c2b3019ca58cb187a870179d3a5a5720c44938358f2075ecec7e9` | fidelidad tras la corrida: transporte, lo visible por el modelo, tramos degradados y envoltorios de premisa (§5) |
| `output.json` | 45 120 | `fb39b8cdc2434702a9520504df3a70c883d41ef61221aa68dbe85edf63c9346d` | resultado literal del Architect (`-o`) |
| `read-audit.json` | 44 791 | `0c6e131cb864a2e5075b083399f419dcdcfeec920372f21ed10deb7c77cab552` | auditoría de las 122 lecturas frente al cierre, con su visibilidad para el modelo |
| `runtime-evidence.json` | 6 115 | `98413ee76a8f761cb2ee7a34bdd727715378c64fb23b33783e8bc645242bb5d5` | identidad observada por el invocador (`RuntimeEvidenceRef`) |
| `v11-model-visible-reaudit.json` | 8 206 | `5439dfbbcfd0c39869aac4efeaa34180eff63ac0266305f1b1ba2e7753981f3c` | corrección de método: re-auditoría de lo que vio el modelo en la revisión de V11 (§6) |

Archivos que **no** se versionan. Se registran su identidad y los campos extraídos:

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `events.jsonl` (salida `--json`) | 1 392 506 | `a72bb75e24fa4dd080ae6ac39208765e241f6cd4d328a4c9dab9f47665a6d6cf` |
| log de sesión (`rollout-2026-10-02T09-27-42-01a0fd3a-77cd-7f80-8300-86104919bf07.jsonl`, 368 líneas) | 5 585 973 | `80d96e28428061f1f6729766acada272c8c2262f998a9bbfa637661d6eb6c5ab` |
| `stderr.txt` | 39 | `1aa26269eb1cc57f86b235a03cda53c004edb5b1e9fc99d4da4f00843293d721` |

## 2. Exención del Owner (texto exacto)

Sección literal de la autorización del Owner y del Coordinator, 457 bytes en UTF-8, SHA-256
`0e2bd4d8445ca0b4d14352584905b5a805003ca9e317d4827e362ad9b9599a26`:

```text
DOTNET TEST EXEMPTION
For THIS Architect invocation only:
DO NOT execute `dotnet test`.
This is the same narrowly scoped Owner exemption used for the prior clean review.
The exact publication CI may be provided only as a separate publication-health signal.
It is NOT:

* equivalent to local Core;
* a substitute for any local test class;
* gate evidence;
* Candidate evidence;
* closure evidence;
* implementation evidence.

Record the exemption explicitly.
```

En el cierre figura como `ExemptedActions` (la acción se **omite**), separada de `HealthSignals` (la CI 37024119231, que no es evidencia equivalente). `dotnet test`
no se ejecutó (`read-audit.json`).

## 3. Preflight de fidelidad por el mismo camino (MEASURED antes de lanzar, terminado a las 15:26:25Z)

**Camino.** `codex sandbox` (el sandbox de Windows de `codex-cli` 0.159.2) → el `pwsh` del runtime de Codex con `-NoProfile -Command <script>`. Es el mismo
binario, la misma cuenta de sandbox y el mismo `PATH` que usa `codex exec`. No reproduce los límites de salida de la herramienta `exec` del modelo: la
comprobación tras la corrida sobre lo visible por el modelo es la decisiva (§5).

**Política de lectura** (parámetro operativo de esta invocación por GAP-10; no es un requisito congelado de la arquitectura): forma A solo para archivos de
hasta 150 000 bytes; los cuatro archivos mayores (Proposal V12, Proposal V11, `docs/HANDOFF.md` y `docs/ROADMAP.md`), solo por rangos de forma B de hasta
400 líneas.

| Forma | Script | Resultado |
|---|---|---|
| A, archivo completo (≤ 150 000 bytes) | `cmd /c type <ruta con barras invertidas>` | 53/53 insumos idénticos tras normalizar el fin de línea |
| B, archivo completo (≤ 150 000 bytes) | `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Get-Content -LiteralPath <ruta> -Encoding utf8"'` | 53/53 idénticos |
| B, rangos (archivos grandes) | igual, con `\| Select-Object -Skip N -First 400` | 4/4 archivos idénticos en todos sus tramos: 8, 8, 14 y 2 tramos |
| C, búsqueda | `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Select-String … \| ForEach-Object { [string]$_.LineNumber + [char]58 + $_.Line }"'` | 8/8 búsquedas (≤, ⊆, ✓, «, ∅, ↔ y dos ASCII), todas las líneas idénticas a las canónicas |
| Git | `git log --oneline -10` | igual a la salida de Git local |

Control negativo: `Get-Content` directo en el `pwsh` del sandbox degrada la Proposal V12 (3 687 U+FFFD y los 20 «≤» perdidos), lo que prueba que la prueba
detecta un transporte con pérdida. Se conservaron los 61 caracteres no ASCII del corpus:
`§ª«°±·»¿ÁÉÍÑÓ×ÚÜáéíñóúüΔα–—“”…′←→↓↔⇒⇔∅∈∉−∖∧∩∪≠≡≤≥⊆⊇─│┐┘├┤✅✋✓` y U+FFFD literal, presente en la evidencia canónica de V10.

**Estado del preflight: FAITHFUL_NORMALIZED.** Las pruebas no modificaron la configuración ni el clon.

## 4. Condiciones, ejecución e identidad observada (MEASURED; detalle en `runtime-evidence.json`)

- **Antes de lanzar:**
  - `codex-cli 0.159.2`, SHA-256 `fcd5eafe…`, con la autenticación existente;
  - `config.toml` con el mismo hash antes y después (`40c27b57…`);
  - clon limpio `D:\r62-arch-v12` en `d0947c5d`, con los blobs verificados y la rama remota en el mismo SHA;
  - ningún proceso sobre rutas de I-62 ni del clon, y `memories` desactivado;
  - el prompt se pasó como argumento con `MSYS_NO_PATHCONV=1`.
- **Ejecución:** de 15:27:42Z a 15:45:22Z (17 min 40 s), salida 0, con `turn.completed`. Consumo: entrada 3 975 060 tokens (3 633 024 en caché) y salida
  26 793 (6 539 de razonamiento).
- **Identidad observada** (RUNTIME_OBSERVED): hilo `01a0fd3a-77cd-7f80-8300-86104919bf07`, `codex_exec`. Los dos `turn_context` dicen `gpt-6.1-sol`,
  `high`, `never` y `read-only`; la red y la escritura quedaron restringidas.
- **Compactación:** **una**, a las 15:34:27Z, antes de la llamada `exec` 21 de 35. El historial de reemplazo conserva el prompt **completo** (igual a
  `prompt.md`), la inyección de `AGENTS.md` y los mensajes de desarrollador, más un resumen cifrado de 27 788 caracteres cuyo contenido no es observable.
- **Fidelidad del prompt:** el texto que recibió el revisor es igual a `prompt.md` salvo el salto final, con 85 caracteres no ASCII y ningún U+FFFD.
- **Herramienta del modelo:** el revisor leyó con la herramienta `exec` (scripts que invocan `tools.exec_command`): 35 llamadas, 122 comandos.
- **Después:** clon en `d0947c5d`, limpio y sin archivos ignorados; configuración sin cambio; ningún proceso de la corrida vivo (se excluye un
  `codex.exe exec-server` preexistente del 2026-10-01, ajeno a la corrida).
- **Lecturas:** 122 comandos, todos de lectura: 39 de forma A, 57 de forma B, 20 de forma C y 6 de Git u otros. Leyó 47 rutas, todas del cierre. Ninguna
  lectura directa en el `pwsh` exterior, ningún archivo grande leído entero, ningún comando de escritura y ningún `dotnet test`. Dos búsquedas fallaron
  con ParserError del propio script, sin devolver contenido.

## 5. Fidelidad tras la corrida y premisas (MEASURED; `input-fidelity-postrun.json`)

**Transporte** (`aggregated_output` de `events.jsonl`):

| Comprobación | Resultado |
|---|---|
| lecturas A aisladas frente al blob | 38 de 39 idénticas; en la del comando 1 (paquete V12) el runtime antepuso el diagnóstico de `profile.ps1`, con el contenido completo |
| lecturas B frente al tramo canónico | 57/57 idénticas |
| búsquedas C frente a las líneas canónicas | 16/16 idénticas; otras dos se comprobaron línea a línea y dos fallaron sin contenido |
| truncamiento de la captura | **ninguno**: las lecturas por rangos evitaron el truncamiento central que en V11 afectó al HANDOFF |
| degradación de caracteres | **ninguna** |

**Lo visible por el modelo** (salidas de las 35 llamadas `exec` en el log de sesión). Es la fuente decisiva: la herramienta `exec` limita su salida y puede
truncar lo que recibe el modelo aunque la captura esté completa.

| Comprobación | Resultado |
|---|---|
| llamadas con salida truncada | 4 de 35. En las llamadas 1 y 4 se truncó la salida conjunta del script (41 396 y 10 327 tokens originales); en las 18 y 28, la de un comando (7 617 y 6 975). Cada truncamiento llegó con su marca explícita «…N tokens truncated…», y el revisor lo advirtió y releyó por tramos menores |
| registros (comandos) | 111 completos, 2 solo con el principio, 2 solo con el final, 2 con el centro omitido y 5 omitidos |
| líneas pedidas que el modelo **nunca** vio | `docs/ROADMAP.md` 91-140, `docs/FOUNDATIONS.md` 121-169 y `docs/initiatives/README.md` 76-695: insumos transitivos que ninguna premisa cita |
| cobertura del objeto | Proposal V12 3 129/3 129 líneas y paquete 124/124, vistas completas al menos una vez |
| líneas de frontera de un empalme | 8, ninguna en un envoltorio de premisa ni en un destino de referencia cruzada |
| `FidelityStatus` | **DEGRADED_BOUNDED**: tramos estructurales localizados mecánicamente y ninguna degradación de caracteres |

**Premisas.** Por cada premisa se construyó el envoltorio con: la unidad estructural completa; la cadena de títulos; y los destinos de **todas** las referencias
cruzadas de esa unidad (§n con el cuerpo completo de la sección), aunque el hallazgo no los cite. Es más estricto que el punto 2 de §20.3.3 de V12, para
cumplir la condición de la orden sobre los destinos de referencia cruzada. Resultado:
- ninguna premisa, envoltorio ni destino cae en una línea nunca vista ni en una frontera de empalme;
- las 13 entradas (1 REQUIRED, 6 disposiciones y 6 evaluaciones) son independientes;
- **ningún hallazgo ni disposición queda INVALID_PREMISE**, y la revisión no se invalida.

Un segundo método independiente (alineación del texto visible con cada comando) da los mismos rangos nunca vistos y la misma cobertura.

## 6. Corrección de método sobre la revisión de V11 (`v11-model-visible-reaudit.json`)

La auditoría custodiada de V11 comparó `aggregated_output`, no lo visible por el modelo. Re-auditado el log de sesión de V11, sin invocación nueva:
- hubo truncamiento visible en 11 de 34 llamadas, y 7 232 líneas pedidas nunca llegaron al modelo;
- todas esas líneas son de insumos transitivos o auxiliares (sobre todo `docs/HANDOFF.md`, `docs/initiatives/README.md`, `docs/ROADMAP.md` y
  `docs/ARCHITECTURE.md`). `AGENTS.md` llegó completo por la inyección del runtime;
- la Proposal V11 (3 071 líneas) y su paquete fueron visibles enteros, y todas las premisas localizables tienen el envoltorio visible al menos una vez;
- la evidencia de V11 informó un solo truncamiento (HANDOFF 4639-4672); su afirmación de lecturas «íntegras y fieles» describía el transporte.

No cambia la independencia de las premisas de V11. Es un registro de corrección de método: no dispone, no reabre ni rebaja ningún hallazgo, y la
acreditación de V11 es del Owner. Las revisiones de V9 y V10 usaron la misma fuente y no se re-auditan: su visibilidad por el modelo es UNKNOWN.

## 7. Límites

1. La medición la hizo la sesión autora, que lanzó el proceso.
2. El modelo servido detrás del proveedor no es observable.
3. La compactación deja las lecturas anteriores solo en un resumen cifrado. De las 3 129 líneas de V12, 420 se volvieron a ver después, y la mayoría de los
   envoltorios no se releyó entera. Todas las citas coinciden con el texto canónico, pero el efecto de la compactación sobre el razonamiento no es observable.
4. Las lecturas transitivas obligatorias de `docs/initiatives/README.md`, `docs/ROADMAP.md` y `docs/FOUNDATIONS.md` quedaron incompletas para el modelo.
5. El preflight no reproduce los límites de salida de la herramienta `exec`. Leer por rangos protegió la captura, pero no lo visible por el modelo cuando el
   revisor agrupó varias lecturas en un solo script.
