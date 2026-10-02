# I-62 — Revisión formal final del Architect de la Proposal V14 (R20261002T184526Z-b331)

```text
LogicalReviewRequestId: L20261002T184526Z-b331   InvocationId: I20261002T184526Z-b331   AttemptSeq: 1   RunId: R20261002T184526Z-b331
Autorización:   OWNER / COORDINATOR AUTHORIZATION — FINAL FORMAL ARCHITECT REVIEW OF I-62 PROPOSAL V14 (decisiones §29); UNA invocación de codex-cli
Objeto:         commit 4c617e82b32b6c810b68d75fc19472efed22b393 (CI de publicación 37047587864, push, 4/4 success)
                docs/initiatives/I-62-proposal-v14.md          blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                docs/initiatives/I-62-architect-package-v14.md blob 3c3b3446b66ae8d0ccbb1f1beb347f461a37442d
Veredicto:      BLOCKED — OWNER DECISION, solo por OD-6; cero REQUIRED; A62-V11-01 CLOSED (registro: docs/initiatives/I-62-architect-review-v14.md)
Fidelidad:      preflight FAITHFUL_NORMALIZED por el mismo camino; lo entregado al revisor, FAITHFUL_NORMALIZED, sin truncamientos ni omisiones, con tres
                diagnósticos del runtime separados como metadato de transporte (GAP-12)
Naturaleza:     evidencia de una invocación real. Los JSON son representaciones EXPERIMENTALES de los contratos de la Proposal V14 (§20.3.1, §20.3.2,
                §20.3.3, B.10.1); no son autoridad normativa. La revisión se rige por las autoridades vigentes (I-61 y LIFECYCLE). I-61 sigue vigente.
                IMPLEMENTATION AUTHORIZATION = NO
Medición:       la hizo la sesión autora (Claude), que lanzó el proceso; no hay observador independiente
```

## 1. Archivos custodiados

Todos son copia byte a byte de los del directorio de la corrida y usan saltos de línea LF. La identidad se comprueba con `git show <commit>:<ruta> | sha256sum`.

| Archivo | Bytes | SHA-256 | Papel |
|---|---|---|---|
| `prompt.md` | 22 339 | `1ffedeffbefdb6e5942e6b40a59a3a1dce6f6b751b25add59221f8a5c1a6d564` | prompt completo: formas de lectura, presupuesto de salida por llamada, cierre, hallazgos abiertos y, literal entre marcas, la orden (7 728 bytes; SHA-256 `6c1ded51cbbba33889f11524ee504beb8ac204d940f4dd527b6ff91c6f6ee2be`) |
| `schema.json` | 10 887 | `02981183e305fe8592ebef21657b95abddeb07196f299b45feec319cef6e8f5e` | esquema de salida: B.10.1, con `PremiseEnvelopeRefs` y `NormativeDependencyRefs` informativos en cada hallazgo |
| `closure.json` | 37 885 | `56c6e5153835efb62441cd93b35f06eb8c62669360fb8338e270e77c46829f78` | `EffectiveInputClosure`: 74 insumos canónicos y 9 transitivos con blob, SHA-256, codificación y fin de línea; política de lectura; exención aparte; `HealthSignals`; contexto de runtime; prohibidos |
| `input-fidelity-preflight.json` | 86 900 | `976090c7a330b662cf55c52a8097f26075689bdde491fad4e510aa6dae9452a9` | preflight de fidelidad antes del lanzamiento (§3) |
| `input-fidelity-postrun.json` | 3 860 | `a6dd44e1164c4567299330909c095e1415bb1e96a020f0d5f729c2b096d7327a` | fidelidad tras la corrida sobre lo entregado al revisor, con los diagnósticos separados (§5) |
| `visible-output-audit.json` | 22 818 | `0f9b32df2622a365f85f22d919d6eb252b6b50455fd5a611a4ee94de9ad4d7bb` | auditoría de la salida visible para el revisor: llamadas, registros, cobertura por línea, compactación (§5) |
| `read-audit.json` | 35 755 | `9ffdd018b1e956de3da7ba33bae63239d4095576bab812b779687fe45afa2128` | auditoría de las 117 lecturas frente al cierre y a la política de lectura |
| `premise-envelopes.json` | 6 334 | `f9aa34b2680768f4049a5b264e4872c6bfbf3b9e54dd62ac596a532880a7b450` | `PremiseEnvelope` de cada premisa, calculado por el invocador (§6) |
| `output.json` | 43 751 | `e3c7f44e85670e9c3a3a9d9c57dbcacd0f79c95e01c6cb9d0c12f411f8fba19a` | resultado literal del Architect (`-o`) |
| `runtime-evidence.json` | 5 724 | `40aeb1ac25411b8987e3c72c528cf1bbc1b7608bbe933d1db0b9f762361ca6cb` | identidad observada por el invocador, con la compactación y los diagnósticos del runtime |

Archivos que **no** se versionan. Se registran su identidad y los campos extraídos:

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `events.jsonl` (salida `--json`) | 1 199 431 | `bf9184e9e3ef970d911842d156e6a6f1e39e5e55e586eebfcaeb579d88b85e78` |
| log de sesión (`rollout-2026-10-02T13-21-17-01a0fe10-4d02-7750-a7c0-98eb1a58c97f.jsonl`, 701 líneas) | 5 234 244 | `3d4a7996468fc72733f9d1ed3b5ad802288410c4519c5de8c671c15447557a77` |
| `stderr.txt` | 39 | `1aa26269eb1cc57f86b235a03cda53c004edb5b1e9fc99d4da4f00843293d721` |
| primer preflight (`preflight-run1-pattern-absent.json`, §3) | — | `745bbde3bf2b8ddcb7d061bbf0f872e4f7d1d28a8ddf24c0ec97900f707a6561` |

## 2. Exención del Owner y regla de GAP-12 (textos exactos)

Exención, 306 bytes en UTF-8, SHA-256 `c1923b8740b1bb39ee31776c8e9cbff375751466ed97d9e739ca1dc0ab34d756`:

```text
DOTNET TEST EXEMPTION
For THIS Architect invocation only:
DO NOT execute `dotnet test`.
This remains a narrowly scoped Owner exemption for the read-only Architect action.
Publication CI is a separate health signal only.
It is not equivalent to local Core and does not satisfy any local-test evidence class.
```

En el cierre figura como `ExemptedActions` (la acción se **omite**), separada de `HealthSignals` (la CI 37047587864). `dotnet test` no se ejecutó.

La sección «RUNTIME WRAPPER DIAGNOSTICS — GAP-12» de la orden (738 bytes, SHA-256 `9b5e16fa2d04b6aaa655176a9f8cf8077136b65606bfaffaedf8c7d58612a472`) va
literal dentro de `prompt.md`. La auditoría la aplica así (§5): solo se separa la línea **exacta y conocida** del diagnóstico del `pwsh` del runtime, al
principio o al final de una salida. El resto se compara igual que siempre.

## 3. Preflight de fidelidad por el mismo camino (MEASURED antes de lanzar)

Camino: `codex sandbox` → el `pwsh` del runtime de Codex con `-NoProfile -Command <script>`.

| Forma | Resultado |
|---|---|
| A (`cmd /c type`) y B completa, archivos de hasta 150 000 bytes | 78/78 insumos idénticos tras normalizar el fin de línea |
| B por rangos de 150 líneas, archivos grandes | 5/5 idénticos en todos sus tramos: Proposal V14 (23), Proposal V13 (22), `normative-dependency-closures.json` de V13 (20), HANDOFF (36) y ROADMAP (5) |
| C, búsqueda | 8/8 (≤, ⊆, ✓, «, →, `NormativeUnitRef`, ∅ y ↔), todas las líneas idénticas |
| Git | igual a Git local |
| control negativo (`Get-Content` directo) | degrada la Proposal V14: 3 996 U+FFFD y los 20 «≤» perdidos |

Se conservaron los 62 caracteres no ASCII del corpus. **Estado: FAITHFUL_NORMALIZED.**

**Primera corrida del preflight (registrada):** dio DEGRADED solo por el diseño de la prueba: una búsqueda de prueba con «≈», que no aparece en la Proposal
V14, no tenía coincidencias y la prueba exige al menos una. Los 83 insumos fueron fieles. Se repitió con «→».

## 4. Ejecución e identidad observada (MEASURED; `runtime-evidence.json`)

- **Antes** (19:20:51Z): clon limpio en `4c617e82`, rama remota en el mismo SHA, `config.toml` `40c27b57…`, ningún proceso del clon. La ruta del
  lanzamiento se calculó con `cygpath -w` y se comprobó.
- **Ejecución:** una sola, de 19:21:14Z a 19:47:25Z (26 min 11 s), salida 0, con `turn.completed`. Consumo: entrada 13 279 704 tokens (12 915 200 en caché)
  y salida 24 668 (3 714 de razonamiento).
- **Identidad observada:** hilo `01a0fe10-4d02-7750-a7c0-98eb1a58c97f`, `codex_exec`, `gpt-6.1-sol`, `high`, `never`, `read-only` en los dos
  `turn_context`.
- **Compactación:** una, a las 19:34:32Z, antes de la llamada `exec` 72 de 113; el prompt se conservó y el resumen cifrado (31 864 caracteres) no se cita.
- **Prompt:** igual a `prompt.md` salvo el salto final, con 114 caracteres no ASCII y ningún U+FFFD.
- **Diagnósticos del runtime:** la línea de perfil de `pwsh` apareció al principio de las salidas de los comandos Git 0, 1 y 2 (verificación de identidad).
  Ninguna reemplazó, interrumpió ni ocultó contenido canónico.
- **Después:** clon en `4c617e82`, limpio; configuración sin cambio; ningún proceso de la corrida vivo.
- **Lecturas:** 117 comandos: 38 de forma A, 59 de forma B, 15 de forma C y 5 de Git. 49 rutas, todas del cierre. La política de lectura se cumplió. Dos
  búsquedas fallaron sin devolver contenido: el patrón `^#{1,4}` y uno con `|` rompieron el análisis de argumentos de `pwsh`.

## 5. Fidelidad tras la corrida (MEASURED; `input-fidelity-postrun.json`, `visible-output-audit.json`)

| Comprobación | Resultado |
|---|---|
| llamadas con salida truncada (lo entregado al modelo) | **0 de 113** |
| registros | 114 completos. Los otros 3 son los comandos Git de identidad 0-2, cuya salida llegó en la llamada siguiente (sondeo); no contienen líneas canónicas |
| líneas pedidas que el modelo nunca vio | ninguna |
| cobertura del objeto | Proposal V14 3 323/3 323 líneas y paquete 137/137 |
| control independiente (alineación) | 117 registros completos, sin truncamientos |
| transporte (captura) | A 38/38, B 59/59 y C 13/13 idénticas; ninguna degradación de caracteres |
| diagnósticos del runtime | 3, separados como metadato de transporte (GAP-12) |
| `FidelityStatus` | **FAITHFUL_NORMALIZED** |

**Inexactitud de una cita** (no es degradación). En la primera premisa de la disposición de A62-V11-01 (Proposal V14, líneas 1298-1301), la cita omite el
artículo «un» al final de la línea 1298: «un párrafo; un elemento…» pasa a «un párrafo; elemento…». La línea llegó íntegra al revisor, así que es un error de
transcripción del revisor. No cambia el significado normativo, y las líneas y el envoltorio son correctos.

## 6. Envoltorios de premisa

Los envoltorios (`premise-envelopes.json`) de las 11 entradas son fieles y fueron vistos completos: la disposición de A62-V11-01, las cuatro de los OPTIONAL y
las seis evaluaciones R62-FIDELITY.

**La clausura de dependencias no se calcula:** la orden establece que esta revisión F0 se rige por las autoridades vigentes y que la regla propuesta de B.11,
cuyo manifiesto es entregable de F3, no se aplica de forma retroactiva. Con lo entregado al revisor en FAITHFUL_NORMALIZED, ninguna premisa puede apoyarse en
un tramo degradado.

## 7. Límites

1. La medición la hizo la sesión autora, que lanzó el proceso.
2. El modelo servido detrás del proveedor no es observable.
3. La compactación deja las lecturas anteriores solo en un resumen cifrado; tras ella se volvieron a ver 529 de las 3 323 líneas de V14. El efecto sobre el
   razonamiento no es observable.
4. La separación de GAP-12 cubre solo la línea exacta y conocida del diagnóstico del runtime; cualquier otra diferencia se trataría como degradación.
