# I-62 — Revisión formal final del Architect de la Proposal V13 (R20261002T165850Z-648a)

```text
LogicalReviewRequestId: L20261002T165850Z-648a   InvocationId: I20261002T165850Z-648a   AttemptSeq: 1   RunId: R20261002T165850Z-648a
Autorización:   OWNER / COORDINATOR AUTHORIZATION — FINAL FORMAL ARCHITECT REVIEW OF I-62 PROPOSAL V13 (decisiones §27); UNA invocación de codex-cli,
                más la autorización del Owner para relanzar tras un fallo previo a la sesión (§4)
Objeto:         commit b337a59fa6879893229096f6e423d441b3c97bc6 (CI de publicación 37036044921, push, 4/4 success)
                docs/initiatives/I-62-proposal-v13.md          blob 949c6403a04570dfa7b5dcd1a3509271819dc696
                docs/initiatives/I-62-architect-package-v13.md blob b1d3d6791e6addedaed8adccd171caefa3c7ed5b
Veredicto:      CHANGES REQUIRED (registro: docs/initiatives/I-62-architect-review-v13.md)
Fidelidad:      preflight FAITHFUL_NORMALIZED por el mismo camino; lo entregado al revisor, completo y sin truncamientos; DEGRADED_BOUNDED solo por una
                línea insertada por el runtime; ninguna clausura de dependencias solapa un tramo degradado
Clausuras:      con la regla literal de V13, las premisas de §20.3.3 (y dos de la disposición de V10) quedan en independencia UNKNOWN por referencias
                sin resolver lejanas (§6)
Naturaleza:     evidencia de una invocación real. Los JSON son representaciones EXPERIMENTALES de los contratos de la Proposal V13 (§20.3.1, §20.3.2,
                §20.3.3, B.10.1); no son autoridad normativa. I-61 sigue vigente. IMPLEMENTATION AUTHORIZATION = NO
Medición:       la hizo la sesión autora (Claude), que lanzó el proceso; no hay observador independiente
```

## 1. Archivos custodiados

Todos son copia byte a byte de los del directorio de la corrida y usan saltos de línea LF. La identidad se comprueba con `git show <commit>:<ruta> | sha256sum`.

| Archivo | Bytes | SHA-256 | Papel |
|---|---|---|---|
| `prompt.md` | 18 136 | `caccda1e88d8006c1917e265033f992ed2a55a0238103e93723e8f57c3f2cfe1` | prompt completo: formas de lectura, presupuesto de salida por llamada, cierre, hallazgos abiertos y, literal entre marcas, la orden (5 369 bytes; SHA-256 `4e6742b9f7b5884e9555f3e41c195c561a66328cae8be4eea129560c74116ec8`) |
| `schema.json` | 10 887 | `c29f267e5907a4ec4d52ed1a2a6c75557123c0e8d1a2b188c13b340ced3a7567` | esquema de salida: B.10.1 de V13, con `PremiseEnvelopeRefs` y `NormativeDependencyRefs` informativos en cada hallazgo |
| `closure.json` | 32 048 | `2b2538af19467c907a9762fc6cc34685b0e38984f6cadd38c1bbfa1d552ed891` | `EffectiveInputClosure`: 60 insumos canónicos y 9 transitivos con blob, SHA-256, codificación y fin de línea; política de lectura; exención aparte; `HealthSignals`; contexto de runtime; prohibidos |
| `input-fidelity-preflight.json` | 72 024 | `c91ccd48776c58558bd3b5b5464adbd333227b60060524563a39f0846ad35df3` | preflight de fidelidad antes del lanzamiento (§3) |
| `input-fidelity-postrun.json` | 3 334 | `b6494532ee23c8598f49510843b3fa4da2b86931455262aee0122f0bfde6665c` | fidelidad tras la corrida sobre lo entregado al revisor (§5) |
| `visible-output-audit.json` | 11 378 | `bb1e3094936035720143e0a85391fc27f3f90d55037737a17558333175adef3d` | auditoría de la salida visible para el revisor: llamadas, registros, cobertura por línea, compactación (§5) |
| `read-audit.json` | 29 224 | `999aaf61105e7a6dda06041a9b676586017888324a4d0f5b6a18f5603f29d1df` | auditoría de las 97 lecturas frente al cierre y a la política de lectura |
| `premise-envelopes.json` | 11 598 | `fa57b96bd42bf2cc78163d2cd43175c7213b213e03d63595fd54c9cac52396eb` | `PremiseEnvelope` de cada premisa, calculado por el invocador (§6) |
| `normative-dependency-closures.json` | 222 786 | `fb72e83cc13d4beecf7d503997cba9dd998dbbf5ad0cc94bd33c3821f17f26a2` | `NormativeDependencyClosure` de cada premisa, calculada por el invocador sobre el texto canónico; cada clausura distinta, una vez (§6) |
| `output.json` | 39 556 | `1efa96eef31cd5fa5099ac1e290919d8dfa38137dfd409e15ffca0cf8859c9d5` | resultado literal del Architect (`-o`) |
| `runtime-evidence.json` | 5 907 | `5e78caa04bc471237f78f87289d23609c64cf822e03e0272902b7056d39b5dbf` | identidad observada por el invocador (`RuntimeEvidenceRef`), con el fallo previo a la sesión y la compactación |

Archivos que **no** se versionan. Se registran su identidad y los campos extraídos:

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `events.jsonl` (salida `--json`) | 1 016 058 | `ad80947e6d8b00ee138665865f8180b6e908b4117f417438079345fbc5d19ae8` |
| log de sesión (`rollout-2026-10-02T11-19-28-01a0fda0-c870-7723-a2a9-c1cde4c223ef.jsonl`, 567 líneas) | 4 446 476 | `db9aef49a80d8fd036ed3992777582e9e4b594d4af203ac8e14f456064b46955` |
| `stderr.txt` | 39 | `1aa26269eb1cc57f86b235a03cda53c004edb5b1e9fc99d4da4f00843293d721` |
| primer preflight (`preflight-run1-transient.json`, §3) | — | `33006ac9a7385ac4b5f33281acc42f7d5e44930de2448399dbae9f2c42b72161` |

## 2. Exención del Owner (texto exacto)

Sección literal de la autorización, 302 bytes en UTF-8, SHA-256 `2d0448eb2c5ec0b3d8781fb9016a64abe13eedd801b27540d602e1423072a6f0`:

```text
DOTNET TEST EXEMPTION
For THIS Architect invocation only:
DO NOT execute `dotnet test`.
This is an explicit scoped Owner exemption for the read-only Architect action.
Publication CI is only a separate health signal.
It is NOT equivalent to local Core and does not satisfy any local-test evidence class.
```

En el cierre figura como `ExemptedActions` (la acción se **omite**), separada de `HealthSignals` (la CI 37036044921, que no es evidencia equivalente). `dotnet test`
no se ejecutó (`read-audit.json`).

## 3. Preflight de fidelidad por el mismo camino (MEASURED antes de lanzar)

Camino: `codex sandbox` → el `pwsh` del runtime de Codex con `-NoProfile -Command <script>` (mismo binario, cuenta de sandbox y `PATH` que `codex exec`).

| Forma | Resultado |
|---|---|
| A (`cmd /c type`) y B completa, archivos de hasta 150 000 bytes | 65/65 insumos idénticos tras normalizar el fin de línea |
| B por rangos de 150 líneas, archivos grandes | 4/4 idénticos en todos sus tramos: Proposal V13 (22), Proposal V12 (21), HANDOFF (36) y ROADMAP (5) |
| C, búsqueda | 8/8 (≤, ⊆, ✓, «, →, `NormativeDependencyClosure`, ∅ y ↔), todas las líneas idénticas |
| Git | igual a Git local |
| control negativo (`Get-Content` directo) | degrada la Proposal V13: 3 839 U+FFFD y los 20 «≤» perdidos |

Se conservaron los 61 caracteres no ASCII del corpus. **Estado: FAITHFUL_NORMALIZED.**

**Primera corrida del preflight (registrada):** una lectura A de `docs/automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/input-fidelity-preflight.json`
difirió solo en ASCII, con los mismos recuentos de los 61 caracteres no ASCII. No se reprodujo ni en el diagnóstico (bytes idénticos) ni en la segunda corrida
completa. Causa UNKNOWN. La comprobación tras la corrida audita cada lectura real.

**Política de lectura de esta invocación** (parámetro operativo, no requisito de la arquitectura): forma A solo hasta 24 000 bytes; B por rangos de hasta 150
líneas; una lectura grande por llamada, con menos de ~8 000 tokens de salida; releer todo tramo con marca de truncamiento.

## 4. Lanzamiento, ejecución e identidad observada (MEASURED; `runtime-evidence.json`)

- **Fallo previo a la sesión:** el primer lanzamiento (17:17:28Z) terminó con salida 1 porque la ruta de Windows de la carpeta de la corrida quedó vacía en el
  comando (`Failed to read output schema file \schema.json`). No se creó sesión, no hubo tokens ni lecturas, y el clon y la configuración no cambiaron. La
  sesión no reintentó por su cuenta: preguntó, y el Owner autorizó el relanzamiento con los mismos insumos y la ruta corregida (`cygpath -w`, comprobada antes).
- **Antes del relanzamiento** (17:19:19Z): clon limpio en `b337a59f`, rama remota en el mismo SHA, `config.toml` `40c27b57…`, ningún proceso del clon.
- **Ejecución:** de 17:19:27Z a 17:37:15Z (17 min 48 s), salida 0, con `turn.completed`. Consumo: entrada 11 089 843 tokens (10 778 368 en caché) y salida
  23 001 (4 805 de razonamiento).
- **Identidad observada:** hilo `01a0fda0-c870-7723-a2a9-c1cde4c223ef`, `codex_exec`, `gpt-6.1-sol`, `high`, `never`, `read-only` en los dos
  `turn_context`.
- **Compactación:** una, a las 17:30:16Z, antes de la llamada `exec` 72 de 89. El historial de reemplazo conserva el prompt completo; el resumen va cifrado
  (25 612 caracteres) y no se cita como evidencia.
- **Prompt:** igual a `prompt.md` salvo el salto final, con 102 caracteres no ASCII y ningún U+FFFD.
- **Después:** clon en `b337a59f`, limpio; configuración sin cambio; ningún proceso de la corrida vivo. Se excluye un `codex exec` de otra sesión sobre el
  worktree de I-63, arrancado a las 17:39:40Z, después del fin de esta corrida.
- **Lecturas:** 97 comandos: 25 de forma A, 54 de forma B, 13 de forma C y 5 de Git. 36 rutas, todas del cierre. **La política de lectura se cumplió**: ninguna
  lectura A de más de 24 000 bytes ni B de más de 150 líneas. Una búsqueda falló sin devolver contenido (el patrón llevaba `|`).

## 5. Fidelidad tras la corrida (MEASURED; `input-fidelity-postrun.json`, `visible-output-audit.json`)

| Comprobación | Resultado |
|---|---|
| llamadas con salida truncada (lo entregado al modelo) | **0 de 89** |
| registros (comandos) | **97 completos**; ninguno truncado ni omitido |
| líneas pedidas que el modelo nunca vio | ninguna |
| cobertura del objeto | Proposal V13 3 216/3 216 líneas y paquete 136/136 |
| control independiente (alineación) | mismos resultados |
| transporte (captura) | A 24/25 idénticas (la otra, con el diagnóstico antepuesto); B 54/54; C 12/12; ninguna degradación de caracteres |
| `FidelityStatus` | **DEGRADED_BOUNDED**: la única desviación es una línea insertada por el runtime antes de la línea 1 del paquete; por conservadurismo, esa línea 1 cuenta como degradada |

## 6. Envoltorios y clausuras de dependencias normativas (calculados por el invocador)

**Envoltorios** (`premise-envelopes.json`): todos fieles y visibles completos.

**Clausuras** (`normative-dependency-closures.json`). Regla mecánica aplicada, conservadora y literal respecto de V13 §20.3.3:
- dependencias: §n (con documento calificador por enlace o nombre adyacente), anexos, identificadores de cláusula, enlaces a documentos, identificadores y
  estados con una unidad que los define;
- destino de un § o de un anexo: la sección entera;
- un enlace a un documento entero es fuente terminal, como dice V13;
- un § sin calificador se busca en el documento, en su contexto de versión y, si no, debe ser único en el cierre;
- lista de trabajo con visitados hasta el punto fijo.

Resultado:
- **ninguna clausura solapa un tramo degradado**: no hay INVALID_PREMISE;
- las premisas sin dependencias de largo alcance cierran sobre 2 a 9 líneas, sin referencias pendientes, y son independientes: Proposal V13 l. 78 (los
  cuatro OPTIONAL), disposición de V12 l. 33 y disposición de V10 l. 43-45 y 47;
- **las premisas de §20.3.3 de la Proposal (l. 1260-1272) y las de la disposición de V10 l. 46 y 48 cierran sobre unas 8 660 líneas de 19 archivos**,
  hasta la profundidad 12, y alcanzan **32 referencias sin resolver distintas** desde la profundidad 3: 15 ambiguas, 12 sin destino y 5 sin unidad que las
  defina. Ejemplos:
  - «§16.13», una sección que la Proposal propone para AUTOMATION_PLAN y que aún no existe;
  - «§16.3» sin calificador, que existe en AUTOMATION_PLAN y en el Freeze de I-61;
  - § de los registros de decisiones y de evidencia que pueden apuntar a V12 o a V13;
- con la regla literal, por tanto, la independencia de esas premisas es **UNKNOWN**: afecta al REQUIRED A62-V11-01, a su disposición y a R62-FIDELITY-02,
  -03, -04 y -06. Son independientes las disposiciones de los OPTIONAL y R62-FIDELITY-01 y -05.

**Lectura de este resultado (INFERENCE de la sesión):**
- la clausura literal se vuelve casi todo el corpus, porque una sección entera trae todas sus referencias;
- un resolutor mecánico no puede decidir el «control material» ni desambiguar referencias a secciones propuestas o a otra versión sin juicio semántico;
- la corrección que pide el Architect, quitar la terminalidad automática, agranda aún más las clausuras;
- en la práctica, A62-V11-01 sigue abierto en cualquier caso: una disposición no acreditada no cierra un linaje.

## 7. Límites

1. La medición la hizo la sesión autora, que lanzó el proceso.
2. El modelo servido detrás del proveedor no es observable.
3. La compactación deja las lecturas anteriores solo en un resumen cifrado; tras ella se volvieron a ver 359 de las 3 216 líneas de V13. El efecto sobre el
   razonamiento no es observable.
4. El resolutor de dependencias es heurístico. Una parte de las referencias sin resolver puede deberse a sus límites y no al texto; la lista completa está en
   `normative-dependency-closures.json`.
5. La causa de la diferencia transitoria del primer preflight es desconocida.
