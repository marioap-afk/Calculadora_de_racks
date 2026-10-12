# I-62 — Diseño de los controles generales C1-C3 del protocolo de ejecución delegada

```text
Estado:        DISEÑO (propuesta técnica). No implementa nada. No es una enmienda.
Autoridad:     decisiones de I-62 §81 (Owner, 2026-10-12): SECURITY DESIGN AUTHORIZATION = YES;
               IMPLEMENTATION AUTHORIZATION = NO. Carril documental y experimental independiente (§81.5).
Unidad:        I-62. Rama de I-62 en 2c531b533181a9efb3bba351c09e657e0696bbe1.
Fuentes (blob en esa revisión):
  docs/AUTOMATION_PLAN.md                                   f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5
  docs/automation/agent-execution/README.md                 592dcfd45e8743fd83b3423249fbd3c73e3d3055
  schemas/relay-record.v2.schema.json                       dca5b29c1a260b679efc0fb249c73850925302f0
  schemas/delegation.v2.schema.json                         dfdbe461270e8fd92200c36bab78b9fdc3385014
  schemas/gate-contract.v2.schema.json                      f644bb1941656b65c62ee474bf37752d328fd091
  schemas/controller-verification.v2.schema.json            e1faa9eca5c50b2b3a9f91575575614366962720
  schemas/input-closure.v1.schema.json                      6bdf71843e2c70362a5e2d0ea19c8891e22bcff6
  docs/initiatives/I-62-proposal-v14.md (Freeze)            34ad80ea1bfff144bfc5169f62920a4c904c1bfa
  docs/initiatives/I-62-consensus-freeze.md                 0f6b860837e2478db8525e1fb30a485bc4646816
  docs/adr/0046-…                                           da275e1a141a750fee2a8f501b36aa280d7aa534
  docs/INITIATIVE_LIFECYCLE.md                              f19896a8f1a7c82f74bac7231636f68462a87271
Fuentes de I-65 (solo lectura, rama de I-65 en 2c9b4ca602b31151c9de0f02503cf4adc2a039fd):
  docs/automation/evidence/I-65-f4/i62-escalation-package.md          118673e9180c11e5525d38d97b67e9bf06a7bbf0
  docs/automation/evidence/I-65-f4/i62-worker-environment-controls-proposal.md 7c151090e65b60a87e490336a4501f6b1610fc2f
  docs/automation/evidence/I-65-f4/incident-main-checkout/README.md   7ff9a340b3d3dba0274f7ecd5f9fcb65e5e9aecb
  docs/automation/decisions/I-65.md (§27-§39)                         cc45ce7d12e7883ca0ed23992f01f374b4196fc7
Experimentos:  D:\r62-c1c3\ (repositorios desechables); copia de scripts y resultados en experiments/.
Fuera:         A-6, FX-02 y la revisión de A-6 no se tocan ni se citan como dependencia normativa.
               C1-C3 NO quedan implementados. I-65 NO queda desbloqueada.
```

Etiquetas de afirmaciones (LIFECYCLE §4): **MEASURED** (experimento de este carril o evidencia custodiada), **RECONSTRUCTED**
(derivado de evidencia), **INFERENCE** (documentación o razonamiento, no medido aquí), **UNKNOWN**.

## 0. Resumen

| Control | Qué es | Preventivo | Detectivo | Dónde se apoya |
|---|---|---|---|---|
| **C1** | Fotografía de todos los checkouts del repositorio en la Salida y la Entrada de cada cesión | indirecto (C2; lecturas sin bloqueos opcionales) | diferencia salida/entrada por estado + barrido de metadatos de archivos y directorios; canario K-C1 por cesión | hechos de Salida/Entrada de 16.4; patrón de P-01/P-11 (STOP aplicado por la sesión); `Scope` y `CleanTree` cubren el worktree propio |
| **C2** | Directorio de proceso del Worker fijado por el transporte (worktree o directorio trampa vacío) | `WorkingDirectory` del proceso hijo; para el subagente, la sesión abierta en el worktree | canario K-C2 antes de ceder; hecho de `cwd` por orden (gancho o transcripción); C1 ve el efecto | receta de 16.4 (`-C <worktree>`), frontera de adapters 16.19, binding 16.20 |
| **C3** | Intérprete prohibido no invocable en el entorno del Worker | filtro de `PATH` del hijo (sin el directorio de alias) + shims trampa; gancho previo opcional | canario K-C3 por cesión; registro del guardián → `GuardEvents` con clase propia (no `Denials`); barrido de la transcripción | condiciones STOP del contrato (16.5), `Denials` (16.9 #14) leído con 16.22, receta de Codex (`PATH` del hijo) |

**Materialidad (§9):** M-02, M-03, M-04, M-05 y M-06 activados; M-07 y M-08 UNKNOWN (cuentan como activados, LIFECYCLE §3); M-01 no.
**Conclusión: C1-C3 son MATERIALES** frente al Freeze de I-62 y a los contratos de ejecución. Se prepara la propuesta de una enmienda
independiente de A-6 ([c1c3-amendment-proposal.md](c1c3-amendment-proposal.md)), sin redactarla.

**Hallazgos de diseño que salen de los experimentos (MEASURED salvo indicación):**

1. La reproducción de la clase de F4-T4 en un par desechable da exactamente el mismo efecto: `RackCad.sln` del «checkout principal» reescrito y
   sin BOM, worktree intacto; C1 lo marca STOP en la misma Entrada y el primer hecho de C2 lo marca antes de escribir (exp-a, A1).
2. Arrancar el hijo con `cwd` = worktree hace que la misma orden escriba en el worktree (exp-a, A2): en F4-T4 la entrada en `RackCad.sln`
   estaba dentro del alcance (I-65 CD-I65-F4-09), así que C2 la habría dejado donde debía.
3. Una escritura transitoria (crear y borrar, como el `placeholder.txt` de F4-T5) no deja rastro en `git status` ni en los hashes; sí en la fecha
   del directorio (exp-e, E1). C1 necesita el barrido de **directorios**, no solo de archivos.
4. `git status` sin `--no-optional-locks` reescribe `.git/index` del checkout inspeccionado (exp-d, D1): la propia fotografía sería una escritura.
5. Las rutas ignoradas (`bin/`, `obj/`) no aparecen en `git status --untracked-files=all` (exp-d, D3); solo el barrido las ve.
6. Un cambio de contenido con el mismo tamaño y la fecha restaurada es invisible para `git status` (comparación por stat) y para el barrido de
   fechas; lo ven el hash de todos los rastreados y `ChangeTime` de NTFS (exp-e, E2/E3). Dos escrituras seguidas comparten `LastWriteTime` en
   72-94 de 200 pares (exp-e, E4): C1 compara **estado**, no ventanas de tiempo.
7. En este host Python solo es alcanzable por el directorio de alias de ejecución de aplicaciones; quitarlo del `PATH` del hijo bloquea las 15
   sondas probadas (las 7 de I-65, en 6 formas distintas, y 9 más, incluidas las ofuscadas), pero se lleva 31 alias ajenos (entre ellos `winget` y `codex-core-command-runner`); `git`, `pwsh`,
   `dotnet` y `gh` siguen resolviendo (exp-c).
8. Los shims trampa delante del `PATH` bloquean y registran toda resolución por nombre (14 de 15), pero `[Diagnostics.Process]::Start('python')`
   solo busca `.exe` y los ignora: los shims solo son seguros **junto con** el filtro de `PATH` (exp-c).
9. El clasificador de texto de un gancho previo acierta 18 de 20 órdenes prohibidas y 8 de 9 permitidas; falla con concatenación de cadenas y con
   un script en archivo, y bloquea un mensaje de commit (exp-c). Es complemento, no barrera.
10. Para el transporte `claude-subagent` el directorio de proceso y el entorno son los de la sesión padre (I-65 §2.3; descriptor del adapter):
    C2 y C3 no se pueden fijar por llamada. Esto convierte C2-C3 en un requisito de elegibilidad del WORKER por transporte (M-05, M-06).

## 1. Alcance, autoridad y no-objetivos

- **Objetivo (§81.1):** controles mecánicos que prevengan o detecten (a) escrituras del Worker en checkouts ajenos, (b) directorios de proceso
  incorrectos y (c) la ejecución de intérpretes prohibidos.
- **Alcance:** unidades I62 desde su vigencia (AUTOMATION_PLAN 16.14). Las unidades I61 siguen con I-61 sin cambios (criterio 13 de V14 §1);
  I-65, que es I61, solo puede adoptar contención local más estricta con su propia autoridad (§11).
- **No-objetivos:** cambiar I-61, I-65, lanzadores compartidos, permisos o ajustes del sistema, alias de ejecución de aplicaciones, `PATH` del
  sistema o configuraciones `.claude`/`.codex`; ampliar A-6; tocar FX-02; resolver C4, C7, C8 o C9 del paquete de I-65 (salvo la pieza mínima de
  clasificación que C3 necesita, §6.4); un servicio, plataforma o motor (no-objetivos de V14 §1).
- **Modelo de amenaza:** errores y hábitos del modelo (sondas `--version`, rutas relativas, suposiciones sobre el directorio actual), no un
  adversario que oculta sus huellas. La ocultación deliberada (restaurar fechas o `ChangeTime`) queda como riesgo residual (§12).

## 2. Casos del incidente de I-65 y lo que haría cada control

| Caso (fuente) | Hecho | C1 | C2 | C3 | Otro |
|---|---|---|---|---|---|
| **F4-T4 intento 2, llamada 25** (`R20261011T202738Z-6356`; DEV-I65-F4-16) | `cd <worktree>` y `[IO.File]::ReadAllText/WriteAllText("RackCad.sln")` reescriben el `RackCad.sln` del checkout principal (BOM eliminado, proyecto añadido); no declarado; visto una hora después | **detecta** en la Entrada: `MAIN` = `CONTENT_CHANGED` (` M RackCad.sln`) → STOP con la ruta (exp-a A1, MEASURED) | **previene**: con `cwd` = worktree la misma orden escribe el archivo del worktree (A2); **detecta**: primer hecho `ProcessCwd` = checkout principal → STOP antes de escribir (A1) | — | `Scope`/`CleanTree` no lo ven por construcción (16.9 #7-#8) |
| **F4-T5, llamada 37** (DEV-I65-F4-15) | `placeholder.txt` creado en el checkout principal y borrado 3 s después; declarado | **detecta solo por el barrido de directorios**: `DIR_ENTRY_CHURN` en la raíz → STOP (exp-e E1); `git status` y hashes no ven nada | no previene una ruta absoluta (la herramienta de escritura usa rutas absolutas, INFERENCE) | — | un guardián de rutas de las herramientas de archivo lo impediría (C2-P4, opcional) |
| **F4-T5, llamada 34** (DEV-I65-F4-18) | borrador en el área temporal de la sesión | **fuera de alcance**: no es un checkout | — | — | riesgo residual R3 (§12) |
| **F4-T1 corrección** (DEV-I65-F4-18) | edición por `D:\…\..\..\..\Users\…\worktree\…`: destino dentro del worktree | **no marca** (correcto: el destino es el worktree propio; exp-b B2) | señal de higiene `PATH_VIA_OTHER_CHECKOUT` (clase A, no STOP); revela que el Worker creía estar en el checkout principal | — | el barrido de la transcripción (C4) la registraría |
| **7 sondas de intérprete** (tabla 2.1 del paquete; DEV-I65-F4-01, -05, -06, -14, -17; CD-I65-F3-12) | `python3 --version`, `python3 -c "print(1)"`, `python3 -`, `python --version 2>$null \| Out-Null`, `python3 --version 2>/dev/null`; dos de F1 sin registrar | — | — | **previene**: las formas reproducidas dan `NOT_FOUND` en el entorno filtrado (15/15) o `BLOCKED_BY_SHIM` con marcador (14/15) (exp-c); **detecta**: cada bloqueo queda en el registro del guardián con clase `C3_INTERPRETER_BLOCKED`, sin depender de lo que declare el Worker; las dos de F1 habrían quedado registradas en la misma Entrada | la parada incoherente de F4-T3 (14 `pass` + STOP) no se produce: un intento bloqueado es clase A por construcción (§6.4) |
| **nc2 de F4-T3** (DEV-I65-F4-08) y **I-63 DEV-G2-01** | el Controller afirmó que un prefijo retirado cubría el archivo y dio VERIFIED | no aplica | no aplica | no aplica | **C5**, control separado (§7.1) |
| **A5 de la corrección de F4-T4** (DEV-I65-F4-11) y CD-I65-F4-19 | C-07 recortado aceptado por id; U+0016 en `Authorities[0].Section` | no aplica | no aplica | no aplica | **C6**, control separado (§7.2) |

## 3. Superficies existentes que se reutilizan

| Superficie (AUTOMATION_PLAN salvo indicación) | Qué aporta | Uso en C1-C3 |
|---|---|---|
| 16.4 Salida / Cesión / Entrada (L507-L523) y README §3.1-§3.4 | hechos de Git y de huella antes y después de cada invocación; la sesión no opera sobre el worktree durante la cesión | C1 añade la fotografía de todos los checkouts a la Salida y la Entrada; C2 y C3 añaden sus canarios a la Salida y sus hechos a la Entrada |
| P-01 / P-11 (16.4 L522-L523; 16.19 L1058-L1061) | comparación salida/entrada de una huella; el cambio es STOP aplicado por la sesión, sin el Controller | **patrón de C1**: «fotografía cambiada durante una cesión» se trata igual que «huella cambiada» |
| Procesos (16.4 L525-L533) y README §3.2 | lista cerrada de procesos, `session-descendant`, `orphan` | C3 no puede usarla para procesos efímeros (un `python.exe` que ya terminó no aparece); se declara el límite |
| Receta de Codex (16.4 L541-L543) | `-C <worktree>`; «el `pwsh` del runtime de Codex delante en el `PATH` del proceso hijo» | precedente de C2-P1 y de C3-P1/P2: el transporte ya fija directorio y `PATH` del hijo |
| `relay-record/v2` `Exit`/`Entry` (objetos cerrados, `additionalProperties: false`) y `Notes` | registro del relevo de una unidad I62 | destino de los hechos de C1-C3 (exige delta de esquema: M-02) |
| 16.9 #1 `Termination`, #6 `Remote`, #12 `Routing`, #14 `Denials` (señales sesión → Controller) | señales mecánicas que el Controller no recalcula | `GuardEvents` de C3 entran por la misma vía; `Denials` excluye solo los bloqueos emparejados 1:1 con el registro del guardián |
| 16.9 #7 `Scope`, #8 `CleanTree` | alcance de los commits y limpieza del worktree propio | C1 cubre lo que ellos no ven por construcción: otros checkouts y rutas ignoradas fuera del worktree |
| 16.22 (L1204-L1207) y README §14.7 | `Scope` = pass exige pertenencia por ruta; el Coordinator repite la comparación | precedente de «comparación mecánica que el Coordinator repite»; base de C5 (§7.1) |
| 16.24 cierre de insumos (`ForbiddenInputs`: «worktrees reales de otras unidades»; `AllowedActions` con exención acotada) | qué puede leer y hacer una invocación de rol | C1 aporta la **evidencia de escritura** que el cierre no tiene (el cierre trata lecturas); C3 se expresa como acción prohibida del contrato, no como insumo |
| 16.19 frontera de adapters (nueve operaciones, descriptor, `Facts`) y 16.20 elegibilidad | qué acredita un transporte para un rol | C2/C3 piden que el descriptor declare si el transporte **fija** directorio y entorno del Worker |
| 16.26 T10 (c) «escritura ajena → STOP P-02/S-07» | trato de un desajuste físico | C1 da a T10 (c) una observación mecánica más allá de `HEAD`/remoto |
| README §10 (nc1-nc4, oráculo relativo) y §8 (coherencia mecánica) | forma de los controles negativos | NC-C1, K-C1, K-C2, K-C3 siguen el oráculo relativo (§8) |
| 16.5 «condiciones STOP adicionales» del contrato | I-65 C-05 (escritura fuera de alcance) y C-08 (intérprete) | C3 hace mecánica la condición; I-65 puede añadir condiciones locales sin I-62 (§11) |
| V14 D.6 (cobertura declarada de la auditoría) | la auditoría no garantiza lo interno del runtime | mismo criterio para el gancho y la transcripción de C2/C3 |

## 4. C1 — Fotografía de todos los checkouts

### 4.1 Qué cubre y qué no

| Señal | Ve | No ve |
|---|---|---|
| `Scope` (16.9 #7) | commits del Worker fuera del alcance | escrituras sin commit; otros checkouts; ignorados |
| `CleanTree` (16.9 #8) | residuos sin commit en el worktree propio | otros checkouts |
| **C1** | cualquier cambio de estado en otro checkout del repositorio durante la cesión, incluidos ignorados y entradas transitorias de directorio | escrituras fuera de los checkouts (área temporal, perfil, memoria, otros repositorios); quién escribió |

### 4.2 Mecanismos detectivos

- **C1-D1 Enumeración.** Desde el worktree de la unidad, `git --no-optional-locks worktree list --porcelain`: el primero es el árbol de trabajo
  principal; incluye los marcados `locked` o `prunable`. **Roles derivados, nunca declarados por el registro** (NC-C1c): `UNIT` =
  `ExpectedWorktree` de la delegación; `MAIN` = primer árbol; `CONCURRENT_SIBLING` = hermano que la Salida declara activo por decisión
  registrada del Coordinator; `QUIESCENT_SIBLING` = el resto.
- **C1-D2 Fotografía por checkout** (MEASURED en exp-a..f; instrumento en `experiments/lib/c1c3-lib.ps1`):
  1. `HEAD` y rama (`rev-parse`, `symbolic-ref`);
  2. `git --no-optional-locks -c core.fsmonitor=false -c core.untrackedCache=false status --porcelain=v1 -z --untracked-files=all`
     (`--no-optional-locks` es obligatorio: sin él Git reescribe `.git/index`, D1; `fsmonitor` desactivado para no arrancar un demonio que la
     comprobación de procesos vería como participante);
  3. SHA-256 de cada ruta del estado;
  4. barrido de metadatos de **archivos y directorios** (tamaño y `LastWriteTimeUtc`), incluidos los ignorados, excluido `.git` (la actividad
     compartida del repositorio, D4, es legítima y no es del árbol de trabajo);
  5. modo profundo opcional: SHA-256 de todos los rastreados (`git ls-files`) o `ChangeTime` de NTFS (E2/E3).
- **C1-D3 Comparación con estado**, no por ventana temporal (E4: dos escrituras pueden compartir fecha). Clases:

  | Clase | Condición | Ejemplo medido |
  |---|---|---|
  | `CONTENT_CHANGED` | cambia `HEAD`, rama, una línea del estado o el hash de una ruta del estado (o de un rastreado en modo profundo) | A1, B1, D7, E3 |
  | `IGNORED_CHANGED` | archivo ignorado nuevo, borrado o modificado (barrido + `check-ignore --no-index`) | D3 |
  | `DIR_ENTRY_CHURN` | fecha de directorio cambiada sin entradas netas nuevas ni borradas en él | E1 |
  | `CHANGETIME_ONLY` | `ChangeTime` distinto con tamaño y fecha iguales (modo profundo) | E2 |
  | `TOUCH_ONLY` | fecha cambiada, mismo tamaño, `git status` limpio | D1 |
  | `NOISE_ALLOWLISTED` | ruta de la lista de ruido declarada (se lista, no se descarta) | D2 |
  | `CHECKOUT_ADDED` / `CHECKOUT_REMOVED` | la enumeración cambió | NC-C1b |

- **C1-D4 Disposición (rol × clase):**

  | Rol | `CONTENT_CHANGED`, `IGNORED_CHANGED`, `DIR_ENTRY_CHURN`, `CHANGETIME_ONLY`, `CHECKOUT_*` | `TOUCH_ONLY` | `NOISE_ALLOWLISTED` |
  |---|---|---|---|
  | `MAIN`, `QUIESCENT_SIBLING` | **STOP** con la lista de rutas | señal registrada | registrado |
  | `CONCURRENT_SIBLING` | **ATTRIBUTION_REQUIRED**: el Coordinator atribuye con evidencia (commits del dueño, cruce con las rutas de las órdenes del Worker) antes de aceptar cualquier VERIFIED; una coincidencia con una ruta del Worker es STOP | señal | registrado |
  | `UNIT` | no es materia de C1 (`Scope`, `CleanTree`) | — | — |

- **C1-D5 Canario K-C1 por cesión**, en un fixture desechable, **nunca** en un checkout real: fotografía, escritura canario en el «principal» y
  en un hermano, fotografía, oráculo exacto; y una segunda pareja sin escritura que debe dar `NONE` (exp-f: PASS, 4,4 s). Prueba que el
  instrumento discrimina antes de confiar en él.

### 4.3 Mecanismos preventivos

C1 es detectivo. Su prevención es indirecta: C2 (el origen más probable de una escritura ajena es una ruta relativa) y dos reglas de la sesión:
toda lectura de Git sobre un checkout durante la ventana usa `--no-optional-locks`, y la fotografía de Salida se toma después de la última
escritura de la sesión e inmediatamente antes del lanzamiento. Una ACL de solo lectura del checkout principal durante la cesión sería preventiva,
pero es un ajuste del sistema: queda fuera (y sería OWNER-RESERVED).

### 4.4 Dónde vive

- **Hechos:** `Exit.Checkouts[]` y `Entry.Checkouts[]` + `Entry.CheckoutsDiff` (forma ilustrativa en el Anexo A), validados mecánicamente por el
  relevo como la coherencia de README §8.
- **Aplicación:** STOP de Entrada aplicado por la sesión, como P-01/P-11 (un id P nuevo, sin número aquí). Con STOP no se invoca al Controller.
- **Alternativa descartada:** una comprobación 15 en 16.9. Cambiaría `controller-verification/v2` (`Checks` cerrado con las 14), el orden de
  README §8, `FailureClass` y los oráculos de nc1-nc3, y volvería a poner en el Controller una comparación que es mecánica (lección de C5).
- **Atribución de hermanos concurrentes:** comprobación del Coordinator entre artefactos antes de aceptar un VERIFIED, como 16.22.
- **Lista de ruido:** la declara el contrato (`NoisePatterns`), nunca el Worker; cada coincidencia se lista en el registro.

### 4.5 Coste (MEASURED)

| Medida | Valor |
|---|---|
| Barrido de metadatos del worktree de I-62 (solo enumeración, sin Git ni contenidos) | 4 304 archivos, 499 directorios, 245 MB, 159-222 ms |
| SHA-256 | 241-418 MB/s; hash completo del mismo árbol ≈ 0,6-1 s |
| Fotografía de un checkout del fixture (proceso Git + barrido) | 0,36-0,52 s |

El número de checkouts del repositorio real no se midió (exigiría leer el `.git` del checkout principal); el coste crece de forma lineal con él.

### 4.6 Límites propios

Atribución (D7: una edición legítima del Owner es indistinguible); ruido de editores no listado (D2: `RackCad.sln~`, `plano.bak`,
`.~lock…#`, `*.orig` siguen siendo STOP); escrituras fuera de los checkouts; ocultación deliberada; frontera temporal de las fotografías.

## 5. C2 — Directorio de proceso del Worker fijado por el transporte

### 5.1 Hecho técnico (MEASURED, exp-a)

En `pwsh`, `Set-Location` cambia la ubicación de PowerShell pero no `Environment.CurrentDirectory`. Tras `Set-Location <worktree>` en un hijo
arrancado en el checkout principal: `PSLocation` = worktree, `ProcessCwd` = checkout principal; `[IO.Path]::GetFullPath("RackCad.sln")` resuelve
en el principal y allí escribe `WriteAllText` (A1). Los cmdlets (`Get-Content`/`Set-Content`) resuelven contra la ubicación de PowerShell y
escriben en el worktree (A4). En Bash, `cd` cambia el directorio del proceso y la escritura relativa cae en el worktree (A5). La clase de fallo
es «API .NET con ruta relativa en PowerShell, con un directorio de proceso distinto del worktree».

### 5.2 Preventivos

- **C2-P1 Transporte de proceso externo** (`codex-cli` como Worker, `claude-cli`): el transporte arranca el hijo con `WorkingDirectory` = worktree
  (A2). Extiende la receta de 16.4, que ya fija `-C <worktree>`.
- **C2-P2 Transporte interno `claude-subagent`:** el subagente hereda el directorio de proceso de la sesión padre y no se puede fijar por llamada
  (I-65 §2.3, RECONSTRUCTED; descriptor `claude-subagent.md`, «comparte la sesión de su padre»). Opciones: (a) la sesión Principal se abre con el
  worktree de la unidad como directorio (acción del Owner al abrirla; no cambia ningún lanzador); (b) el WORKER no es elegible en ese transporte
  mientras el descriptor no declare la capacidad. **Tensión con P-08:** con la sesión en el worktree, cualquier orden de la sesión durante una
  cesión a un proceso externo (Codex) corre con `cwd` = worktree; la regla de cesión de 16.4 obliga entonces a que la sesión no opere, o solo con
  rutas absolutas fuera del worktree (pregunta abierta).
- **C2-P3 Directorio trampa vacío:** para roles de solo lectura o cuando no se quiere el worktree. Una edición relativa falla (A3a) y una creación
  relativa cae en la trampa (A3b); trampa no vacía en la Entrada = señal.
- **C2-P4 (opcional, fuera de C2 estricto):** guardián de rutas para las herramientas de archivo (ruta normalizada fuera del worktree y del
  directorio del `RunId` → rechazo), con la misma infraestructura que C3-P3. Habría impedido F4-T5 (llamada 37). Pregunta abierta.
- **Alternativa descartada:** el aislamiento por worktree temporal que ofrecen algunos runtimes de subagentes crea un worktree nuevo, no el de la
  unidad; choca con `Identity` (rama y worktree) y con «una rama en un solo worktree» (INFERENCE).

### 5.3 Detectivos

- **C2-D1 Canario de transporte (K-C2) antes de ceder:** un hijo lanzado exactamente como se lanzará al Worker informa
  `Environment.CurrentDirectory`; distinto del esperado → **no se invoca** (STOP antes de la invocación, sin consumir `attempts`). exp-f: `cwd` =
  worktree → PASS; `cwd` = principal → `STOP_BEFORE_INVOCATION`.
- **C2-D2 Hecho por orden:** el primer hecho de cada orden del Worker es el directorio de proceso. Fuentes mecánicas posibles: (a) el `cwd` que un
  gancho previo recibe en su entrada (INFERENCE: campo documentado, no medido); (b) el `cwd` que la transcripción registra por entrada (INFERENCE:
  a medir en el paso 1 del plan, §13). Cualquier valor distinto del esperado → STOP en la Entrada. Sin fuente → `UNVERIFIED`, y C2-D2 no cuenta
  como acreditado (mismo criterio que V14 D.6).
- **C2-D3** C1 ve el efecto aunque falle todo lo anterior.
- **C2-D4** señal de higiene de rutas (exp-b B2): ruta cruda que pasa por otro checkout o sube con `..` → clase A, registro.

### 5.4 Dónde vive

`Exit.WorkerProcessDirectory {Expected, Policy: WORKTREE|EMPTY_DIR, TransportFixed, Canary}` y `Entry.ProcessDirectoryFacts {Source:
HOOK|TRANSCRIPT|NONE, Mismatches[]}`; el descriptor de adapter declara la capacidad «fijar el directorio de proceso» (DISPONIBLE/UNVERIFIED/NO
APLICA); el binding de WORKER la exige (16.20). Política en el contrato (`ExecutionEnvironment.ProcessDirectory`).

### 5.5 Límites propios

No impide rutas absolutas ni un `Set-Location` deliberado a otro checkout seguido de cmdlets; en el subagente depende de cómo se abra la sesión;
las fuentes de C2-D2 están sin medir.

## 6. C3 — Intérprete prohibido no invocable en el entorno del Worker

### 6.1 Política

El contrato declara `ForbiddenExecutables` (por defecto la familia Python: `python`, `python3`, `python3.N`, `pythonw`, `py`, `pyw`, `pip`,
`pip3`, `idle`). No se prohíben «los intérpretes» en abstracto: `pwsh`, `bash`, `cscript` y `perl` también lo son y el Worker necesita al menos el
primero. Residuales: `perl` de Git Bash (`/usr/bin/perl`, MEASURED) y Windows Script Host en `System32` (INFERENCE).

### 6.2 Preventivos (MEASURED, exp-c)

- **C3-P1 Filtro de `PATH` del hijo:** se quitan los directorios que contienen ejecutables prohibidos y el de alias de ejecución de aplicaciones.
  En este host el único es el de alias (Python 3.13 de la tienda). Resultado: 15/15 sondas en `NOT_FOUND` (las 7 de I-65, que son 6 formas distintas, y 9 más,
  incluidas `py`, `pip`, concatenación, variable, `cmd /c`, `Invoke-Expression`, `-EncodedCommand`, `Process.Start` y `Start-Process`). Coste: 31 alias
  colaterales (`winget`, `wt`, `codex-core-command-runner`, etc.); `git`, `pwsh`, `dotnet` y `gh` siguen resolviendo. No impide una ruta absoluta
  (control con un intérprete falso: corre) y el alias real sigue alcanzable por ruta absoluta.
- **C3-P2 Shims trampa** delante del `PATH` (`.cmd` para PowerShell/cmd, `sh` para Bash): bloquean con un marcador en stderr y una línea en el
  registro del guardián. 14/15 `BLOCKED_BY_SHIM`; `Process.Start('python')` da `NOT_FOUND` sin marcador porque .NET solo busca `.exe`. Con el
  directorio de alias presente, esa llamada llegaría al alias real: **P2 solo junto con P1** (entorno «S» del experimento = shims + `PATH`
  filtrado). P1 previene; P2 aporta el marcador y el registro que permiten clasificar.
- **C3-P3 Gancho previo a cada orden (opcional):** clasificador de texto con conjunto de reglas fijado por SHA-256; simulado con el protocolo
  documentado (entrada JSON por stdin, salida 2 = bloqueo, stderr al modelo; INFERENCE). Corpus: 18/20 bloqueos correctos, 2 falsos negativos
  (concatenación de cadenas; script en archivo), 8/9 permisos correctos, 1 falso positivo (mensaje de commit). Ve rutas absolutas por nombre; no
  ve lo que ocurre dentro de un mismo proceso de PowerShell ni dentro de un script.
- **Por transporte:** proceso externo → P1+P2 los fija el transporte por hijo (precedente de la receta de Codex). Subagente → comparte el entorno
  de la sesión: P1/P2 exigen lanzar la sesión Principal con ese entorno (lanzador compartido: OWNER-RESERVED) y P3 exige ajustes de sesión
  (configuración persistente: OWNER-RESERVED). Un gancho en los ajustes locales del propio worktree solo afectaría a sesiones abiertas en él
  (INFERENCE).

### 6.3 Detectivos

- **C3-D1 Canario K-C3 por cesión,** en el entorno exacto del Worker: resolución vacía (`Get-Command` y `where.exe`) y una sonda que debe dar
  `NOT_FOUND` o `BLOCKED_BY_SHIM`; registrado en `Exit.InterpreterGuard.Canary`. Fallo → no se invoca. exp-c: PASS en ambos entornos.
- **C3-D2 Registro del guardián → `Entry.GuardEvents[]`** con clase `C3_INTERPRETER_BLOCKED`, `InterpreterExecuted = false`,
  `CountsAsDenials = false`.
- **C3-D3 Barrido de la transcripción** (mecanismo de C4, precedente `scan_ram_execution.py.txt` de I-65): toda orden de la familia prohibida sin
  evento del guardián emparejado y con salida 0 es candidata a clase B → STOP. «Cero invocaciones» = cero ejecuciones no emparejadas.
- **Límite:** la comprobación de procesos de 16.4 solo ve procesos vivos en la Entrada; un `python.exe` efímero no aparece. La auditoría de
  creación de procesos es un ajuste del sistema: fuera.

### 6.4 Clasificación propia (no `Denials`)

| Hecho | Clase | Disposición | `Denials` |
|---|---|---|---|
| bloqueo del guardián emparejado 1:1 con su evento (orden, ventana) | `C3_INTERPRETER_BLOCKED`, clase A | registro obligatorio, sin STOP | **excluido** |
| orden prohibida fallida sin evento (p. ej. `NOT_FOUND` del filtro sin shim, o `Process.Start`) | orden fallida | como hoy | incluida (BLOCKED) |
| orden prohibida ejecutada (salida 0, sin evento) | clase B | STOP (condición del contrato) | — |
| canario fallido o guardián no establecido | — | no se invoca (fallo cerrado) | — |

Es la pieza mínima de C7 que C3 necesita: sin ella, un intento bloqueado activa la condición del contrato (C-08 en I-65) y reproduce la salida
incoherente de F4-T3. El resto de C7 queda fuera.

### 6.5 Límites propios

Rutas absolutas; `perl` de Git Bash; Windows Script Host; alias colaterales (un Worker Codex podría necesitar `codex-core-command-runner`,
UNKNOWN); falsos negativos y positivos del texto; sin aplicación por llamada en el subagente.

## 7. Insuficiencia de nc2 y de A5 (casos de §81.3)

### 7.1 nc2 (DEV-I65-F4-08; I-63 DEV-G2-01) — es **C5**, control separado

- **Modo de fallo:** el Controller (un modelo) evalúa la pertenencia de cada ruta a `AllowedWriteScope` y afirmó una cobertura inexistente; nc2
  (README §10) no discriminó. En I-63 y en I-65 la comprobación mecánica de la sesión sí lo detectó.
- **Lo que I-62 ya tiene:** 16.22 exige en `Scope.Evidence` la ruta y la entrada que la contiene, `not_run` si falta, y el Coordinator repite la
  comparación antes de aceptar un VERIFIED (README §14.7, caso S). Con ese texto, la afirmación falsa de F4-T3 se rechazaría en la comprobación
  entre artefactos.
- **Lo que cambia con C5:** la sesión calcula `Scope` (diff ⊆ `AllowedWriteScope`, intersección vacía con `ForbiddenWriteScope`) y lo entrega
  como señal sesión → Controller, como `Termination` o `Remote`; el Controller no la reinterpreta; el oráculo de nc2 pasa a medir la fidelidad
  con la que el Controller consume la señal.
- **Pertenencia:** no es C1-C3. C1 y C5 comparten el principio (alcance mecánico en dos ejes: commits y checkouts). Se propone como delta
  separable de la misma enmienda o de otra, según disponga el Coordinator; §81 solo autorizó el diseño de C1-C3.

### 7.2 A5 (DEV-I65-F4-11; CD-I65-F4-19) — es **C6**, control separado

- **Modo de fallo:** A5 (16.5 L564) compara `StopConditions` por id; un texto recortado con el mismo id pasó. El esquema ya lleva `{Id, Text}`
  (`delegation/v2`, `gate-contract/v2`).
- **Cambio propuesto:** igualdad de bytes de `(Id, Text)` con la única normalización fiel de 16.24 (fin de línea), rechazo de caracteres de control
  C0/C1 en los campos copiados del contrato, y la misma igualdad para `Authorities` (`Path`, `Section`, `Class`).
- **Pertenencia:** no es C1-C3; reutiliza los conceptos de fidelidad de 16.24. Delta separable, igual que C5.

## 8. Pruebas negativas

Oráculo relativo como README §10: la condición mutada da la disposición esperada y todo lo demás queda igual que en la ejecución real.

| Id | Control | Condición | Oráculo | Ejecutada |
|---|---|---|---|---|
| K-C1 | C1 | escritura canario en un fixture entre dos fotografías; segunda pareja sin escritura | exactamente `?? C1-CANARY-…` (principal) y ` M src/A.cs` (hermano), STOP; sin escritura `NONE` | **sí** (exp-f, PASS) |
| NC-C1a | C1 | registro de Entrada mutado con ` M RackCad.sln` en el principal | STOP con esa ruta; demás checkouts igual que la real | **sí** (PASS) |
| NC-C1b | C1 | un checkout de la Salida falta en la Entrada | `CHECKOUT_REMOVED` → STOP | **sí** (PASS) |
| NC-C1c | C1 | el registro llama `UNIT` al principal | roles derivados ≠ registrados → registro inválido | **sí** (PASS) |
| NC-C1d | C1 | crear y borrar en el principal | `DIR_ENTRY_CHURN` → STOP | **sí** (exp-e E1) |
| NC-C1e | C1 | mismo tamaño, fecha restaurada (rastreado e ignorado) | modo profundo: `CONTENT_CHANGED` + `CHANGETIME_ONLY` | **sí** (E2/E3) |
| NC-C1f | C1 | ruido: toque de fecha; commit del Worker en su worktree; compilación en su worktree | `SIGNAL`; `NONE`; `NONE` | **sí** (D1, D4, D6) |
| NC-C1g | C1 | hermano declarado concurrente con cambios | `ATTRIBUTION_REQUIRED` | **sí** (exp-b B1c) |
| K-C2 | C2 | hijo lanzado con `cwd` = principal | `STOP_BEFORE_INVOCATION` | **sí** (exp-f) |
| NC-C2a | C2 | orden con `Set-Location` + `[IO.File]` relativo, hijo en el principal | primer hecho `STOP_C2_CWD_MISMATCH` | **sí** (A1, A4) |
| NC-C2b | C2 | `cwd` por orden distinto en el gancho o la transcripción | STOP en la Entrada | **no** (necesita sesión real) |
| K-C3 | C3 | canario en el entorno del Worker | resolución vacía y sonda bloqueada | **sí** (exp-c, PASS) |
| NC-C3a | C3 | 15 formas de sonda, `PATH` filtrado | `NOT_FOUND` en todas | **sí** |
| NC-C3b | C3 | las mismas con shims | `BLOCKED_BY_SHIM` con marcador; `Process.Start` → `NOT_FOUND` | **sí** |
| NC-C3c | C3 | intérprete por ruta absoluta | corre (límite declarado) | **sí** (con intérprete falso) |
| NC-C3d | C3 | corpus del gancho (29 órdenes) | matriz de confusión | **sí** (18/2/8/1) |
| NC-C3e | C3 | bloqueo en la transcripción sin evento emparejado | cuenta en `Denials` | **no** (diseño) |
| NC-C3f | C3 | guardián no establecible | no se lanza | **no** (diseño) |

Como C1-C3 se aplican en la sesión, no hace falta un control negativo nuevo del Controller. Si la enmienda optara por una comprobación del
Controller, su control negativo seguiría el oráculo relativo de README §10.

## 9. Matriz de materialidad (M-01..M-08)

Frente al Freeze de I-62 (Proposal V14 + A-1..A-5 acordadas; A-6 no cuenta: está en revisión) y a los contratos de ejecución (16.4, 16.9,
16.18-16.24, esquemas `/v2`, descriptores).

| M | Activado | Motivo |
|---|---|---|
| M-01 | no | el contrato sigue siendo dueño de las prohibiciones, la sesión de los hechos del relevo y el descriptor de las capacidades del transporte; el registro del guardián es evidencia, no una segunda autoridad. La atribución de hermanos concurrentes es una decisión nueva del Coordinator, no un dueño nuevo de un dato |
| M-02 | **sí** | `relay-record/v2` `Exit`/`Entry` son objetos cerrados: nuevos campos (`Checkouts`, `CheckoutsDiff`, `WorkerProcessDirectory`, `ProcessDirectoryFacts`, `InterpreterGuard`, `GuardEvents`); `gate-contract/v2` y `delegation/v2` ganan `ExecutionEnvironment`; esquemas de hechos de adapter y `preflight/v1` |
| M-03 | **sí** | cambia el comportamiento observable del relevo: canarios antes de cada cesión, otro directorio y otro entorno del Worker, nuevos STOP de Entrada |
| M-04 | **sí** | cambia qué falla y cómo: un cambio en otro checkout pasa de silencioso a STOP; `cwd` distinto → STOP antes de invocar; un intento de intérprete pasa a bloqueo clasificado A en lugar de condición del contrato/`Denials`; fallo cerrado sin guardián o canario |
| M-05 | **sí** | el registro del relevo, los descriptores y la elegibilidad del binding los consumen todas las unidades I62; el WORKER en `claude-subagent` deja de ser elegible sin la capacidad de C2/C3, lo que alcanza a los pilotos de F6 |
| M-06 | **sí** | la frontera de adapters (lista cerrada de nueve operaciones, V14 §7) gana una capacidad («fijar directorio y entorno del hijo») |
| M-07 | **UNKNOWN** (cuenta como activado) | el instrumento de fotografías y, sobre todo, un gancho de sesión son mecanismos transversales; V14 excluye motor, framework y plataforma (§1) y fija Level A. Un filtro de `PATH` en la receta del transporte no lo es; el gancho podría serlo (EXP-09) |
| M-08 | **UNKNOWN** (cuenta como activado) | ADR-0046 #9 («Nivel A: … sin plataforma ni scripts operativos hasta que la evidencia lo justifique»), conservado por ADR-0048: C1 y C3 necesitan instrumentos operativos en cada cesión. El precedente `A6-ADR0046-9-FX02` (§78) fue una determinación OWNER-RESERVED limitada que «no modifica ADR-0046 ni crea precedente»; un control general es más amplio. Además 16.22 declara `Denials` sin cambio para I62, y C3 cambia su lectura |

**Conclusión: MATERIAL.** Exige una enmienda material posterior al Freeze (Architect + Coordinator, LIFECYCLE §6) con partes OWNER-RESERVED
(§11). No modifica unidades I61 ni I-61.

**Recomendación de forma:** las reglas nuevas en una subsección I62 propia de §16 (añadida) en lugar de editar 16.4, que es texto de I-61. Así
16.4 conserva su texto para las unidades I61 y el mapa de cláusulas recibe una entrada ADDED, no una MODIFIED sobre una sección de I-61 (16.13,
MV-3..MV-6).

## 10. Superficies afectadas (una implementación posterior; nada se toca ahora)

| Superficie | Cambio previsto |
|---|---|
| `docs/AUTOMATION_PLAN.md` §16 | subsección I62 nueva (aislamiento del Worker: C1-C3, ids P nuevos); 16.18 (lista de comprobaciones de relevo), 16.19 (capacidad del adapter), 16.20 (elegibilidad del WORKER), 16.22 (filas `Denials` y relevo), 16.26 T10 (c) (referencia a C1) |
| `docs/automation/agent-execution/README.md` | §3.1 y §3.4 (procedimiento de fotografía, canarios y hechos), §10 (K-C1/K-C2/K-C3 y NC-C1/NC-C3), §13 (preflight con las capacidades), §14.7 (A1'-A8'), §17 (diario con los hechos nuevos) |
| `schemas/relay-record.v2.schema.json` | `Exit`/`Entry`: campos de C1-C3 |
| `schemas/gate-contract.v2.schema.json`, `schemas/delegation.v2.schema.json` | `ExecutionEnvironment` (`ProcessDirectory`, `ForbiddenExecutables`, `GuardMechanisms`, `NoisePatterns`, hermanos concurrentes declarados) y su inclusión en A3'-A5' |
| `schemas/role-invocation.v1.schema.json` | solo si la invocación de rol transporta el entorno (pregunta abierta) |
| `schemas/adapters/*.facts.v1.schema.json`, `schemas/preflight.v1.schema.json` | hechos de las capacidades de directorio y entorno |
| `schemas/controller-verification.v2.schema.json` | sin cambio de forma (C1-C3 se aplican en la sesión); `Denials.Evidence` cita los `GuardEvents` excluidos |
| `adapters/claude-subagent.md`, `codex-cli.md`, `claude-cli.md` | estado de la capacidad nueva por transporte |
| `docs/initiatives/PROMPT_TEMPLATES.md` §G | sin cambio normativo (la prohibición en el prompt queda como C9, secundaria) |
| `compatibility/I62-clause-map.json` | regenerado si se tocan superficies del mapa (MV-1..MV-7) |
| instrumentos operativos (fotografía, shims, gancho, canarios, validador) | ubicación por decidir; dependen de la determinación sobre ADR-0046 #9 |
| `docs/automation/decisions/I-62.md`, evidencia | solo por añadido |

## 11. Límites de autoridad

| Materia | Quién | Por qué |
|---|---|---|
| Acuerdo de la enmienda material | Architect + Coordinator | LIFECYCLE §6 (M material) |
| Determinación sobre ADR-0046 #9 para instrumentos operativos generales | **OWNER-RESERVED** | precedente §77-§78 |
| Lanzar la sesión Principal con un entorno restringido o exigir abrirla en el worktree; instalar ganchos en ajustes de sesión; cualquier cambio de alias o `PATH` del sistema | **OWNER-RESERVED** (y fuera de este carril) | configuración persistente o del host; afecta a otras unidades (I-64) |
| Precondición de que nadie escriba el checkout principal durante una cesión, o vía de atestación del Owner ante un STOP de C1 | **OWNER-RESERVED** | limita el uso de la copia de trabajo del Owner |
| Aplicar C1-C3 a pilotos de F6 o a FX-02 | Coordinator, con disposición del Owner si cambia OD-5/OD-7 o la matriz OV | §81.4: no interferir con A-6 ni con la recuperación de FX-02 |
| Contenido de `NoisePatterns`, declaración de hermanos concurrentes y atribución caso a caso | Coordinator de la unidad | decisión registrada, no del Worker |
| Cambios en I-61 | nadie en este carril | criterio 13 de V14; no se proponen |
| **I-65 sin esperar a I-62** (con su Coordinator y, donde se indica, su Owner) | — | — |
| · condición STOP local «cambio en cualquier otro checkout durante la cesión» | Coordinator de I-65 | 16.5 admite condiciones STOP adicionales; solo endurece |
| · fotografía C1 de la sesión en cada cesión (`--no-optional-locks`, sin escribir en ningún checkout) registrada en su evidencia | sesión de I-65 | hechos adicionales, no rebaja ninguna regla |
| · barrido de transcripciones tras cada Worker (C3-D3) | sesión de I-65 | ya lo hizo de forma puntual (DEV-I65-F4-17) |
| · abrir la sesión de I-65 en su worktree (C2-P2) | Owner | acción al abrir la sesión, no cambia lanzadores; con la tensión P-08 de §5.2 |
| · gancho o shims para los Workers de I-65 | Owner | configuración persistente |

Nada de esto declara C1-C3 implementados ni I-65 desbloqueada.

## 12. Riesgos residuales

| # | Riesgo | Mitigación parcial |
|---|---|---|
| R1 | hermanos concurrentes (otras sesiones, I-64): C1 no atribuye | rol `CONCURRENT_SIBLING` + atribución del Coordinator; coincidencia con rutas del Worker = STOP |
| R2 | actividad del Owner en su checkout durante la ventana (D7) | precondición o atestación (OWNER-RESERVED) |
| R3 | escrituras fuera de los checkouts (área temporal, perfil, memoria de sesiones, otros repositorios) | fuera de C1; C4 (transcripción) |
| R4 | ocultación deliberada (fecha o `ChangeTime` restaurados) | fuera del modelo de amenaza; el modo profundo eleva el coste |
| R5 | intérprete por ruta absoluta (el alias sigue alcanzable) | gancho por nombre (C3-P3) y C3-D3 |
| R6 | `perl` de Git Bash, Windows Script Host | lista explícita del contrato; residual declarado |
| R7 | falsos negativos y positivos del clasificador de texto | P1+P2 como barrera; texto solo complemento |
| R8 | el subagente no admite directorio ni entorno por llamada | elegibilidad por capacidad; sesión abierta en el worktree |
| R9 | sesión en el worktree frente a P-08 durante cesiones a Codex | no operar durante la cesión, o rutas absolutas fuera del worktree |
| R10 | coste con muchos worktrees | lineal; modo profundo solo para el principal |
| R11 | fronteras temporales de las fotografías | Salida justo antes del lanzamiento; Entrada tras la terminación acreditada |
| R12 | afirmaciones INFERENCE (semántica del gancho, `cwd` de la transcripción, alcance de ajustes locales) | paso 1 del plan: medir antes de usar |
| R13 | alias colaterales necesarios para un transporte (p. ej. Codex) | lista de directorios añadidos explícitamente por el descriptor |
| R14 | `TOUCH_ONLY` oculta una reescritura de contenido idéntico | inocua por definición; se registra |
| R15 | puntos ciegos de la lista de ruido | se lista cada coincidencia; la declara el contrato |

## 13. Plan mínimo de implementación posterior

Cada paso exige la disposición que lo autorice; ninguno está autorizado hoy.

| # | Paso | Prueba de aceptación |
|---|---|---|
| 0 | Disposición nueva; acuerdo de la enmienda; determinación del Owner sobre ADR-0046 #9 | decisiones registradas con su texto literal |
| 1 | Medir lo INFERENCE: `cwd` por entrada en la transcripción de un subagente; entrada y salida 2 de un gancho previo; alcance de ajustes locales (en una sesión desechable autorizada por el Owner) | hechos MEASURED custodiados; si falta alguno, el mecanismo dependiente queda UNVERIFIED y no se usa |
| 2 | Instrumento C1 de solo lectura + K-C1 + NC-C1a..g como pruebas sobre fixtures | reproduce los oráculos de exp-a, b, d, e y f; `.git/index` de los checkouts inspeccionados sin cambios |
| 3 | Integración en el relevo I62 (hechos de Salida/Entrada, STOP de Entrada, validador) | repetición de F4-T4 sobre fixture → STOP en la Entrada con ` M RackCad.sln`; registro válido contra el esquema |
| 4 | C2 para transportes de proceso externo (directorio del hijo + K-C2) | A1/A2 sobre fixture: `cwd` principal → no se invoca; `cwd` worktree → la escritura queda en el worktree |
| 5 | C2 para `claude-subagent` (capacidad del descriptor, regla del binding, directorio de la sesión) | K-C2 en la sesión real autorizada y barrido de `cwd` = worktree en todas las entradas del Worker |
| 6 | C3 para proceso externo (filtro + shims + canario + emparejamiento en `Denials`) | resultados de exp-c reproducidos; solo los bloqueos emparejados quedan fuera de `Denials` |
| 7 | C3 para subagente (entorno de sesión o gancho), solo con autorización del Owner | canario bloqueado con clase propia en la sesión real; cero ejecuciones no emparejadas en el barrido |
| 8 | Controles negativos y piloto en una unidad de fixture, fuera del carril de A-6/FX-02 | oráculos NC-C1..C3; blobs de las superficies de A-6 sin cambios |
| 9 | Documentación y mapa de cláusulas | MV-1..MV-7 PASS; lectura I61 de 16.4 sin cambios |

## 14. Experimentos (resumen; detalle en README.md)

| Exp. | Qué | Resultado |
|---|---|---|
| a | escritura relativa (F4-T4) en un par desechable | A1: principal reescrito, BOM perdido, C1 STOP, C2 STOP; A2: escrito en el worktree, C1 NONE; A3: trampa; A4: cmdlets → worktree; A5: Bash → worktree |
| b | escritura en un hermano | B1: STOP (` M src/A.cs`, `?? notes.txt`); B1c: ATTRIBUTION_REQUIRED; B2: NONE + señal `PATH_VIA_OTHER_CHECKOUT` |
| c | intérprete bloqueado | F: 15/15 `NOT_FOUND`; S: 14/15 con marcador; ruta absoluta corre; gancho 18/2/8/1; `perl` en Bash |
| d | falsos positivos | índice reescrito sin `--no-optional-locks`; `TOUCH_ONLY`; temporales; ignorados; `.git` excluido; estabilidad; Owner |
| e | límites | transitorio por fecha de directorio; mismo tamaño + fecha restaurada; granularidad 72-94/200; coste |
| f | canarios y negativos | K-C1, NC-C1a-c, K-C2: PASS |

## Anexo A — Forma ilustrativa de los hechos (no es esquema; valores de exp-a A1, abreviados)

```json
{
  "Exit": {
    "Checkouts": [
      { "Role": "MAIN", "Path": "D:\\r62-c1c3\\work\\A1-hazard-cwd-main\\main", "Head": "c021787c8589", "Branch": "main",
        "Status": [], "StatusHashes": {}, "FileCount": 4, "Sweep": "<metadatos de archivos y directorios, sin .git>" },
      { "Role": "QUIESCENT_SIBLING", "Path": "…\\wt-sibling", "Status": [] },
      { "Role": "UNIT", "Path": "…\\wt-unit", "Status": [] }
    ],
    "C1Canary": { "Result": "PASS" },
    "WorkerProcessDirectory": { "Expected": "…\\wt-unit", "Policy": "WORKTREE", "TransportFixed": true, "Canary": "PASS" },
    "InterpreterGuard": { "Mechanisms": ["PATH_FILTER", "PATH_SHIM"], "RulesetSha256": "<sha256>", "Canary": "PASS" }
  },
  "Entry": {
    "CheckoutsDiff": {
      "Overall": "STOP",
      "Checkouts": [
        { "Role": "MAIN", "Classes": ["CONTENT_CHANGED"], "Disposition": "STOP", "StatusAdded": [" M RackCad.sln"] },
        { "Role": "QUIESCENT_SIBLING", "Classes": ["UNCHANGED"], "Disposition": "NONE" },
        { "Role": "UNIT", "Classes": ["UNCHANGED"], "Disposition": "NOT_C1" }
      ]
    },
    "ProcessDirectoryFacts": { "Source": "HOOK|TRANSCRIPT|NONE", "Mismatches": ["…\\main"] },
    "GuardEvents": [ { "Mechanism": "PATH_SHIM", "Name": "python3", "Args": "--version", "Class": "C3_INTERPRETER_BLOCKED",
                       "InterpreterExecuted": false, "CountsAsDenials": false } ]
  }
}
```
