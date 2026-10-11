# I-62 §74.4 / §74.7 — Estrategia de Controller: evidencia determinista + Controller (análisis en solo lectura)

> Respuesta a las disposiciones §74.4 y §74.7 (DEC L1510, L1513). Es un análisis con una recomendación. No es una enmienda. No se editó nada, no se
> escribió en Git, no se invocó ningún modelo ni CLI de modelo y no hubo red. Fecha: 2026-10-11.

## 0. Fuentes y convenciones

- **W** = `%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal`. Se leyó en `78dbb411`. Durante el análisis, otra sesión
  avanzó la rama desde `92510286` y añadió §142-§143 y `FX-02/s74/mv-static-analysis.md`, que este análisis incorpora. Ninguno de esos commits cambia
  un blob normativo.
- **Alias** (rutas relativas a W, blob en `78dbb411`):

| Alias | Ruta | Blob |
|---|---|---|
| AP | `docs/AUTOMATION_PLAN.md` | `f525cb1e` |
| RAE | `docs/automation/agent-execution/README.md` | `592dcfd4` |
| RT | `docs/automation/agent-execution/routing.md` | `bba08fc4` |
| CV2 / RI1 | `…/schemas/controller-verification.v2.schema.json` / `…/schemas/role-invocation.v1.schema.json` | `e1faa9ec` / `d54a7ae7` |
| V14 | `docs/initiatives/I-62-proposal-v14.md` | `34ad80ea` |
| A2, A4, A5 | `docs/initiatives/I-62-A-2.md`, `-A-4.md`, `-A-5.md` | `f1e1d6f0`, `5e2ba68e`, `ad94ceed` |
| DEC | `docs/automation/decisions/I-62.md` | `e74d2bba` |
| LC | `docs/INITIATIVE_LIFECYCLE.md` | `f19896a8` |
| ADR46 / ADR48 | `docs/adr/0046-…md` / `docs/adr/0048-…md` | `da275e1a` / `e1bd8d91` |
| P2 | `docs/automation/evidence/I-62-F6/OD-2/R20261011T102825Z-a2p2-v5-controller/` (README, comparison.json, failure-classes.json) | — |
| S70, S72, S74 | `…/I-62-F6/FX-02/s70/controller-failure-analysis.md`, `…/s72/controller-options.md`, `…/s74/mv-static-analysis.md` | — |
| CM / MV / RV / PT | `…/I-62-F4/compat/clause_map.py`, `…/I-62-F6/MAP-INVALID/mv_check.py`, `…/I-62-F6/FX-U1-chain/run_validator.ps1`, `…/kits/FX-02/a2p2-v5-run/probe_tools.py` | `ffe6ea57`, `9536d051`, `71750b0b`, `ec5eaa4c` |
| CC | `…/kits/FX-02/controller-contracts.md` | — |

- **Variantes de B** que se distinguen en todo el documento:
  - **B-min (instrumento del Controller).** Herramientas deterministas fijadas por blob, que emiten **hechos** (no veredictos). El propio Controller
    las ejecuta **dentro** de su invocación de solo lectura, decide y cita en `Checks.*.Evidence` la orden, el blob, el SHA-256 de la salida y los
    hechos que exige la norma. El aceptante las vuelve a ejecutar y compara.
  - **B-ext (paquete de la sesión).** El Principal ejecuta las herramientas **antes** de la invocación y entrega al Controller un paquete de evidencia.
    El Controller lo comprueba y decide.

## 1. Resumen

1. El contrato vigente **ya** respalda con hechos de la sesión 6 de las 14 comprobaciones de VERIFY: `Termination`, `Remote` («En el registro»),
   `Ci`, `Tests`, `Routing` y `Denials` (AP L630, L635, L638-L639, L641, L643). También **obliga** al aceptante a reproducir de forma mecánica
   `Scope`, `Evaluate` y la coherencia de la salida (RAE L172-L185, L517-L518; AP L805-L811, L1204-L1207).
2. Las otras 8 (`Handoff`, `Authority`, `Contract`, `Identity`, `Scope`, `CleanTree`, `Trailer`, `FreeText`) tienen al **Controller** como actor de
   la señal (AP L631-L640, L642). Que el Controller use una herramienta como instrumento cabe en su perfil, porque `tool-use` es capacidad obligatoria
   del Controller (RT L23, L116-L117). Pero la norma no prevé, en ninguna parte, una herramienta fijada como insumo canónico: los insumos se enumeran
   como «el objeto exacto, sus registros de revisión y sus autoridades» (AP L1264). A esa ambigüedad (S-03) se suman ADR-0046 #9, «sin … scripts
   operativos hasta que la evidencia lo justifique» (ADR46 L69), y la lista cerrada de contenidos del fixture B1 (A5 L178-L186).
3. **B-ext es material y debilita la independencia.** Cambia el actor de 8 comprobaciones (M-01, M-03, M-04, M-05). Choca con «no le pasa el
   resultado esperado ni su propio registro» (AP L806-L808). Y en la topología A el Worker es subagente de la sesión del Principal (V14 L2444), así
   que la evidencia del verificador saldría de la sesión de referencia (AP L1153, L1169).
4. **Sonda 2 (P2).** Respalda B: se ejecutaron todos los pasos, OP_A/OP_B/OP_D quedaron demostradas, los 14 `Checks` y `AuthorityResolution`
   coincidieron, y los fallos residuales son de copia, formato y vínculo. Matiza B: (a) ninguna sonda midió todavía un Controller con herramientas
   fijadas; (b) las herramientas existentes no sirven tal cual (salida no ASCII, un modo de escritura, MV-4 no literal en `clause_map.py`); (c) MV-4
   tiene una ambigüedad normativa que una herramienta fija, pero no resuelve (S74 L116-L117).
5. **Ninguna opción evita una medición nueva.** «Una celda sin medición previa no se vincula» (AP L1093), y el bloque A2-P2 de `d13914ce…` está
   agotado (DEC L1507). La medición solo se abre con una actualización observada del runtime (A2 L222-L226) o con otra A-n (A4 L205-L207; DEC L1351).
6. **Recomendación:** adoptar **B-min** (no B-ext), con la otra celda de `codex-cli` como respaldo en el mismo bloque futuro. Ruta mínima: una
   disposición ordinaria para preparar B-min en estático; una revisión independiente que dictamine su materialidad; y después la medición, en el
   próximo bloque A2-P2 o, si el dictamen es «material» o el Owner no quiere esperar, por una A-6 estrecha (§8).

| | Autoridad | Coste de recuperación (además del común*) | Ruta demostrable a C-24 |
|---|---|---|---|
| **A** Controller actual | actualización observada + disposición (par) + OD-2 + `BLOQUE-CODEX-CONSUMO`; o A-n material + consumo | 0-1 A-n; 2 líneas del Owner; 1-2 sondas; kit v6 | sí en teoría; baja confianza: depende de una transcripción perfecta del modelo |
| **B-min** | lo de A + disposición ordinaria de preparación + dictamen de materialidad; si es material, A-6 (Architect + Coordinator) + consumo | lo de A + herramientas endurecidas + 1 revisión `claude-cli` (+1 reejecución); 0-1 A-n | **la más demostrable**: conserva D.3 y lo ya demostrado; los fallos residuales pasan a hechos deterministas reproducibles |
| **B-ext** | A-6 amplia (M-01..M-05, probable M-08); posible decisión del Owner sobre el ADR | 1 A-n amplia, ≥ 2 revisiones, posible cambio de esquema, 1-2 sondas | dudosa: debilita la independencia de Contexto y de Sesión |
| **C1** otra celda `codex-cli` (`gpt-6.1-sol/high`) | como A (la elección de celda es ordinaria) | como A | como A; sin evidencia de esa celda como Controller |
| **C2** `claude-cli` como Controller | A-n material (D.3 y A4-2 regla 6) + línea nueva del Owner + operaciones 2-9 del descriptor por medir | 1 A-n + caracterización y medición (≥ 2-4 invocaciones) + 1-2 líneas del Owner | larga; sin evidencia |
| **C3/C4** sesión de escritorio o subagente | no elegibles con los descriptores y la independencia vigentes | — | no |

\* Común a todas las rutas: crear el repositorio B1 (Owner), la sesión de FX-U2 (A5-2 y consumo del Owner), OD-7 y la ronda de FX-02 dentro de D.3
(Codex 11 + pool 4 ≤ 15, hoy 0 consumidos; Worker ≤ 2; Reviewer ≤ 2).

## 2. Qué permite ya el contrato vigente

### 2.1 Por comprobación: quién produce qué hoy

| # | Comprobación | Actor de la señal | Quién puede producir la evidencia hoy | ¿Herramienta determinista? |
|---|---|---|---|---|
| 1 | `Termination` | sesión → Controller (AP L630; AP L1197) | la sesión: `Outcome`, PID y árbol muertos, operación 7 del adapter (`relay-record/v2`) | **sí, por la sesión** (procesos, PID, `CreationDate`) |
| 2 | `Handoff` | Controller (AP L631) | el Controller lee la entrega y la compara con `Outcome.OutputSha256`, que registra la sesión | instrumento del Controller (hash, lectura JSON) |
| 3 | `Authority` | Controller (AP L632) | el Controller ejecuta `Evaluate` de forma independiente; el Coordinator ejecuta el suyo y compara (AP L805, L809-L811); la sesión **no** le pasa su resultado (AP L806-L808) | instrumento del Controller; reproducción del Coordinator ya obligatoria |
| 4 | `Contract` | Controller (AP L633) | el Controller (A3-A5 entre delegación y contrato); el Coordinator ya evaluó A1-A8 al aceptar (AP L556-L567) | instrumento del Controller |
| 5 | `Identity` | Controller, Git local (AP L634) | el Controller | instrumento del Controller |
| 6 | `Remote` | sesión → Controller, **«En el registro»** (AP L635) | la sesión registra `ls-remote` y `origin/main` (RAE L167); el Controller compara. El kit v5 ya no pidió `ls-remote` al Controller (PT L5-L7), que el sandbox bloquea (S70 L23-L28) | **sí, por la sesión** |
| 7 | `Scope` | Controller (AP L636) | el Controller, con la salida reproducible de `git diff` y la entrada por ruta (AP L1205-L1206); el Coordinator repite la comparación (RAE L517-L518; AP L1207) | instrumento del Controller; reproducción del Coordinator ya obligatoria |
| 8 | `CleanTree` | Controller (AP L637) | el Controller | instrumento |
| 9 | `Ci` | sesión → Controller (AP L638) | la sesión: `gh run list/view/download` y `RemoteFacts` (RAE L158-L168) | **sí, por la sesión** |
| 10 | `Tests` | sesión → Controller (AP L639) | la sesión descarga los TRX y registra conteos y fallidas (RAE L163-L167); el Controller comprueba la coherencia | **sí, por la sesión**; el Controller cruza |
| 11 | `Trailer` | Controller (AP L640) | el Controller | instrumento |
| 12 | `Routing` | sesión → Controller (AP L641) | la sesión, desde el registro de sesión (RAE L149-L150) | **sí, por la sesión** |
| 13 | `FreeText` | Controller (AP L642) | el Controller en la entrega; el Coordinator, en el texto libre de la verificación (AP L672) | instrumento |
| 14 | `Denials` | sesión → Controller (AP L643) | la sesión (eventos y transcripción) | **sí, por la sesión** |

### 2.2 Reproducción mecánica por el aceptante: ya es norma

- **Coherencia de la salida (OBL-11).** El relevo aplica un guion mecánico: pares válidos, VERIFIED ⇔ 14 pass, `FailureClass` y `RedPart`
  (RAE L172-L185). Si falla, INVALID_OUTPUT.
- **Esquemas.** `Test-Json` de la delegación (RAE L119-L121) y de los hechos del adapter (AP L1031-L1034).
- **`Scope`.** El Coordinator repite `git diff` y la pertenencia por ruta, y rechaza un VERIFIED que no coincide (RAE L517-L518; AP L1207; caso S en
  RAE L531).
- **`Authority`.** `Evaluate` del Coordinator y comparación posterior con el registro del Controller (AP L805, L810-L811; también AP L772-L773).
- **Trabajo terminado.** `git diff --name-only` de los commits de la sesión (AP L450-L451); el Coordinator puede rechazar un VERIFIED (AP L451-L452).
- **Hechos del adapter.** Contraste independiente con Exit/Entry; una discrepancia es S-04 (AP L1036-L1037).
- **Bindings.** Quien acepta o valida **reproduce** la comprobación (AP L1123-L1124).
- **FX-02.** El titular evalúa la aceptación y la custodia, y el Q7 y S29 la reproducen (A4 L326-L329).

Conclusión: el patrón «el aceptante reproduce de forma mecánica» ya existe y es obligatorio para `Scope`, `Authority` y la coherencia. B no lo crea:
lo extiende.

### 2.3 ¿Puede el Controller ejecutar herramientas del repositorio con la receta 16.4?

- **Técnicamente, sí.** La receta es read-only con el `pwsh` del runtime (AP L541-L543). En P2 la celda ejecutó 24 órdenes con intérprete
  (P2 comparison.json L236), sin escrituras de Git, de archivos ni de red (L239, L242, L244). §69.4 admitió en las sondas «los intérpretes
  disponibles en la receta nativa, solo para operaciones read-only autorizadas» (DEC L1414).
- **Por norma, con condiciones no escritas:**
  - el archivo de la herramienta tiene que estar en el cierre (`CanonicalInputs` {Path, Blob}, RI1 L110; si no, P-22, AP L1279-L1280), y la orden
    en `AllowedActions` como ACTION_COMPATIBLE (RI1 L176-L193; AP L1271);
  - la lista de insumos canónicos de AP L1264 / V14 L1079 no menciona herramientas → ambigüedad, a resolver por el Coordinator o el Architect;
  - el Controller no puede leer el worktree real de I-62 (prohibido: V14 L2573; CC L50), y el fixture no contiene ningún `.py` (S74 L94-L95). La
    herramienta tendría que entrar en B1, cuyo contenido enumera A5-1 (A5 L178-L186), o ir como copia transitoria enumerada y comprobada por blob.
- **Herramientas existentes: ninguna sirve tal cual.**

| Herramienta | Utilidad | Impedimentos para usarla como instrumento del Controller |
|---|---|---|
| CM `clause_map.py` | derivación del mapa y C-20b | modo `write` que escribe el mapa y hace `git add` (CM L151-L156); stdout con `ensure_ascii=False` (CM L168); **su «MV-4» no es la regla MV-4**: es MV-3 rotulado fila a fila (CM L133-L137; S74 L79-L82, L110); no calcula MV-1 ni MV-7 (S74 L77) |
| MV `mv_check.py` | Derive, E1-E3, PRE, MV-1..MV-7 **literal** (MV L375-L393), solo lectura y solo biblioteca estándar (MV L13) | resumen legible con acentos por stdout (MV L536-L560): con el `pwsh` del sandbox se degrada (P2 README L36-L44); escribe archivos solo con `--out` (MV L532-L534) |
| RV `run_validator.ps1` | validador de producción F4 sobre puntos reales | `Add-Type` (RV L5) y `Set-Content` (RV L36): previsiblemente incompatible con ConstrainedLanguage (no medido) y escribe; exige el DLL compilado. Es herramienta de la sesión o del aceptante, no del Controller |
| PT `probe_tools.py` (`reference`, `compute_checks`) | oráculo completo de las 14 comprobaciones y de `AuthorityResolution` (PT L927, L1087-L1187) | es el **oráculo** de la supervisión, que se ejecuta después de la sonda (PT L22-L27). Usarlo como instrumento del Controller fundiría el oráculo con el procedimiento bajo prueba (V14 L2333-L2334) |

### 2.4 ¿Una salida de herramienta puede ser `Checks.Evidence`?

- **Forma:** `Evidence` es una cadena libre (CV2 L199-L205); el esquema es estricto (CV2 L6) y no tiene campo de procedencia de herramienta.
- **Contenido:** sí, si trae los hechos que pide la norma. Para `Scope`, la salida reproducible de `git diff` y la entrada por ruta (AP L1205-L1206).
  **Nunca** un veredicto suelto: «Ni stdout, ni un código de salida, ni un JSON conforme bastan» (ADR46 L61; AP L646).
- **Momento:** AP L646 añade «se ignora toda salida anterior al inicio de la invocación». La frase está en el párrafo de la señal de pruebas, pero
  leída a la letra excluye un paquete precalculado para las comprobaciones del Controller. Favorece B-min y es, como mínimo, S-03 para B-ext.

### 2.5 Independencia aplicable

- La verificación es exclusiva del Controller, y la sesión nunca declara `EXECUTION_*` (AP L444, L446, L462).
- La autonomía cambia el transporte, nunca la autoridad (AUT-I1, V14 L1039-L1047). Un acto de autoridad del Principal es P-20 (AP L1139).
- Controller frente a WORKER: Actor, Sesión y Contexto REQUIRED. Con Coordinator y Worker en la misma sesión, la referencia es esa sesión (AP
  L1168-L1169). Contexto = «solo el cierre … sin transcripción, memoria ni razonamiento de la referencia» (AP L1153). Un subagente comparte la sesión
  de su padre (AP L1152).

## 3. Qué exige una modificación material

| Operación | Cláusula afectada | M (LC L83-L90) | Por qué |
|---|---|---|---|
| Paquete de la sesión que sustituye la señal del Controller en las comprobaciones 2-5, 7, 8, 11 y 13 (B-ext) | AP L631-L642 (actor); AP L806-L808; AP L646; AP L1153, L1169 | M-01, M-03, M-04, M-05 | segundo productor del mismo dato; cambia lo que consume la verificación y sus modos de fallo (paquete alterado u obsoleto); evidencia nacida en la sesión de referencia |
| Campo estructurado de evidencia de herramienta en `controller-verification/v2` o `relay-record/v2` | CV2 L6 (`additionalProperties: false`); C-05 (V14 L2342) | M-02, M-05 | cambia el esquema y el contrato que consumen el relevo y el aceptante |
| Herramientas como regla permanente del protocolo (§16 o RAE) | ADR46 L69 (#9, sin scripts operativos); ADR48 L91, L111; V14 L1670, L994 | M-08 (+ M-03) | reinterpreta el Level A de un ADR aceptado. Cambiar el ADR es materia del Owner (ADR48 L5) |
| Medir otra vez la identidad vigente sin actualización | A2 L222-L226 (un bloque por actualización); A4 L205-L207 | M-03, M-04 (precedentes: A2 L271-L272; A4 L393-L394) | abre un presupuesto congelado (DEC L1066) |
| Un control negativo nuevo con invocación («salida de herramienta alterada») en la ronda | V14 L2441-L2442 (8 negativos; 11 base; tope 15) | M-03, M-04 | sube un tope congelado de D.3 |
| Cambiar el adapter del Controller de FX-02 (C2, C3) | V14 L2438-L2441; A4 L232-L233 (regla 6: el Controller conserva su adapter); RAE L220 («el Controller Codex real») | M-03, M-04 (+ M-05 por descriptor) | como A4-2 para el Architect |
| Cambiar la receta 16.4 (`-NoProfile`, codificación, `cmd.exe` en la vía normal) | AP L541-L543; A4 L192-L194, L210-L211, L337 | M-03, M-04 | S72 L41 |
| Meter las herramientas en F_seed o F_norm de B1 | A5 L178-L186 (contenido enumerado) | M-03 | se aparta de A5-1 acordada. Alternativa sin enmienda: copia transitoria enumerada, por lectura del Coordinator |
| **Probablemente no material (disposición ordinaria):** diseño de la medición; elección de celda dentro de `codex-cli`; redacción del kit | S72 L45; DEC L1494; RT L50-L55; S74 L206-L207 | — | no cambian ningún contrato consumido |
| **B-min** (instrumento del Controller, sin cambio de esquema ni de actor) | AP L1264 (enumeración); ADR46 L69; A5 L178-L186 | **dudoso**: M-03 y M-04 probables; M-08 si se lee como «scripts operativos» | requiere el **dictamen del Architect**. Si es material: A-6 estrecha. Owner solo para el consumo, salvo que se toque el texto del ADR |

Regla de autoridad: «Una M material exige Architect + Coordinator; una materia OWNER-RESERVED exige Owner»; una A-n solo del Coordinator puede añadir
pruebas sin cambiar resultados (LC L219-L221).

## 4. Qué debe reproducir el aceptante (sin delegar la autoridad en las herramientas)

| # | Verificación | Cómo | Ancla | ¿Nueva con B? |
|---|---|---|---|---|
| R1 | identidad de la herramienta | blob en el commit custodiado = blob que nombra la disposición o el contrato; el Controller registra el blob que ejecutó; ejecución desde el objeto Git, nunca desde el árbol de trabajo en `CurrentSha` | patrón de AP L712-L714 (identidad por blob) | sí |
| R2 | intérprete | ruta, versión y hash del `python` que usó el Controller (eventos) = los declarados; `python -I` | DEC L1414; V14 L2572 (dependencias técnicas enumeradas con su SHA-256) | sí |
| R3 | reejecución | mismas órdenes y argumentos (`BaseSha`, `CurrentSha`, `MainSha`, `AuthorityRevision`, EFF) en un clon limpio con los mismos objetos; SHA-256 de la salida igual al citado en `Evidence` | AP L1204-L1207 (reproducible) | sí |
| R4 | de los hechos a la decisión | recalcular cada `Check` desde los hechos con una implementación del aceptante, y comparar `Classification`, `Disposition` y `FailureClass` | RAE L172-L188; AP L451-L452 | amplía |
| R5 | `Scope` | repetir `git diff` y la pertenencia por ruta | RAE L517-L518 | ya existe |
| R6 | `Authority` | `Evaluate` propio y comparación con `AuthorityResolution` | AP L805-L811 | ya existe |
| R7 | diversidad en las reglas con lectura normativa (MV-4, R61 b, cobertura) | implementación distinta o la orden Git primaria; una discrepancia es S-04 | AP L687; S74 L105-L110 (tres implementaciones, tres criterios) | sí |
| R8 | texto libre | control manual de la verificación | AP L672 | ya existe |
| R9 | lecturas dentro del cierre | lo que lee la herramienta (objetos Git en SHAs nombrados), declarado; cobertura declarada | AP L1279-L1280; V14 L1126-L1130 | amplía |
| R10 | señales de la sesión | contraste del registro de relevo con su fuente | AP L1036-L1037 | ya existe |

Lo que el aceptante **no** hace: no convierte una salida de herramienta en `EXECUTION_*` (AP L444) y no usa la herramienta del Controller como
oráculo de C-24 (V14 L2333-L2334).

## 5. Lo que dicen las sondas reales

**A favor de B**

| Hecho | Fuente |
|---|---|
| Ejecución completa: 42 órdenes, salida 0, ≤ 415 s de 600, sin F6-OBS-03, identidad estable | P2 README L9-L15 |
| OP_A, OP_B (Git y hashes) y OP_D (TRX y CI) demostradas | P2 README L21-L29; comparison.json L8, L10 |
| Los 14 `Checks`, `Classification`, `Disposition` y `FailureClass` coinciden; `AuthorityResolution` exacta | P2 README L33-L34; comparison.json L200-L216, L223 |
| Fidelidad exacta con `python -c` y JSON ASCII (`AllFaithful`), registrada como capacidad observada | P2 README L42-L43; comparison.json L95; DEC L1508 |
| Los fallos residuales son de copia, formato y vínculo: lectura bloqueada y claves mal nombradas, `Schema` sin comillas, C43, `IssuedBy`, 14 de 15 rutas en `Scope` | failure-classes.json L4-L42 |
| Paso 21: la celda reimplementó MV-1..MV-7 a mano; C34 falló y C35 añadió una condición; el método de C38 era tautológico y tiene un defecto latente | S74 L84-L96, L157-L162, L189-L191 |
| El diseño de la medición cuenta: las omisiones dominantes de las sondas anteriores (≈ 88 %) desaparecieron en la v5 | S70 L10-L17; P2 README L62-L65 |
| `Remote` en el registro evita la restricción del sandbox sobre `ls-remote` | S70 L23-L28; AP L635 |

**En contra o con matices**

| Hecho | Consecuencia para B | Fuente |
|---|---|---|
| Consola `pwsh` no fiel; `[Console]::OutputEncoding` no se puede fijar en ConstrainedLanguage | toda herramienta tiene que emitir JSON solo ASCII; CM y MV no lo hacen hoy | P2 README L36-L44; CM L168; MV L536-L560 |
| MV-4: oráculo 7 frente a celda 31; ambigüedad normativa entre L905 y L916/L835; `clause_map.py` cuenta 31 por otro motivo | una herramienta fija **una** lectura y no resuelve la norma. En B1 (A5-1: ningún ADDED/ENTRY en F_seed) las lecturas A y B coinciden, así que la ambigüedad no decide un mapa válido | S74 L105-L123; A5 L178-L180 |
| `IssuedBy` y `Scope` 14 de 15 son errores de transcripción | una herramienta no los elimina si el Controller sigue copiando; R3 y R4 los **detectan** (rechazo y reejecución dentro del pool). Hacen falta salidas con conteos y hashes para el autocontrol | failure-classes.json L29-L42; RAE L517-L518 |
| Ninguna sonda midió un Controller con herramientas fijadas | B-min no está demostrado: es una extrapolación hasta medirlo | P2; S72 |
| PLAN copia campos del contrato | el valor de B en PLAN es menor; generar la delegación con una herramienta sería delegar la planificación (autoridad) | comparison.json L179; AP L1236 |

## 6. Riesgos de seguridad y de independencia de B

| Riesgo | Mecanismo | Mitigación (B-min) | Residuo |
|---|---|---|---|
| Manipulación de la herramienta | el Worker modifica la herramienta en sus commits y el Controller la ejecuta desde el árbol en `CurrentSha` | blob fijado en `AuthorityRevision` o en RackCad revisado; ejecución desde el objeto (`git cat-file blob <b>` → `python -I -`); ruta en `ForbiddenWriteScope`; R1 | bajo |
| Intérprete no fijado | `python` de WindowsApps (C34 de P2), fuera de la receta; módulos plantados en el cwd | `python -I`, solo biblioteca estándar (como MV L13), R2, invalidador declarado | medio: no forma parte de la receta ni del descriptor |
| El Controller firma sin mirar | copia el veredicto de la herramienta | la herramienta emite hechos, no veredictos; el Controller decide cada `Check` y cruza al menos la orden Git primaria que exige AP L1205-L1206; en la medición, una salida sellada y alterada que el Controller debe detectar (fuera de la ronda; dentro sería material, §3) | medio |
| Error de modo común entre herramienta y aceptante | misma herramienta, mismo error | R7 diversidad: probe_tools 7, `mv_check` 7 y `clause_map` 31 muestran que el riesgo es real (S74 L105-L110) | medio |
| Oráculo no independiente de C-24 | la misma sesión de supervisión escribe la herramienta y el oráculo | revisión independiente de la herramienta frente al texto (no frente al oráculo) y oráculo de D.3 propio; V14 L2333-L2334 | medio |
| Cambio de sandbox o de receta | se ajusta la receta para que la herramienta funcione | B-min no cambia la receta (AP L541-L543); herramientas sin escritura (sin el modo `write` de CM L151-L156) y sin red | bajo |
| Inyección de prompt por archivos del repositorio | títulos, mensajes de commit y texto libre de la entrega entran en el contexto | salidas estructuradas y escapadas, con hashes en lugar de texto donde baste; `FreeText` (AP L669-L672). Ya existe hoy | igual que hoy |
| Autoría y fijación | quién escribe y quién fija la herramienta | escrita fuera de la unidad (RackCad), revisada por un revisor independiente, fijada por blob en una disposición; un blob distinto es invalidador de la medición (como A4 L208-L209) | bajo |
| Procedencia | sin rastro reproducible | `Evidence` con orden, blob, versión del intérprete, SHAs de entrada y SHA-256 de la salida; custodia (AP L712-L714) | bajo |
| Deriva de autoridad (M-01) | la herramienta pasa a ser la autoridad de facto | AUT-I1 (V14 L1039-L1047); el Controller sigue siendo el único que declara | bajo con B-min; **alto con B-ext** |
| Independencia de Contexto y de Sesión (B-ext) | la evidencia del verificador sale de la sesión Coordinator/Worker | B-min la calcula dentro de la invocación del Controller | B-ext: no mitigable sin cambiar AP L1153, L1169 |

## 7. Comparación A / B / C

| Opción | Qué cambia | Autoridad necesaria | Coste estimado | Ruta a C-24 |
|---|---|---|---|---|
| **A** Controller actual (`gpt-6-luna/high`, órdenes improvisadas) | nada, salvo el kit v6 (lecturas fijadas: «MV-4 literal (L905)», claves yes/no; S74 L206-L207) | (i) actualización observada → disposición que nombra el par (A2 L224-L225) + OD-2 + `BLOQUE-CODEX-CONSUMO`; o (ii) A-n material de medición (precedente A4 L200-L204) + consumo | 0 o 1 A-n; 2 líneas del Owner (o 1 con A-n); ≤ 2 sondas; kit ≈ 1 sesión; fecha incierta en (i) | posible, poco probable: los 5 errores inequívocos de P2 dependen de transcripción, no de capacidad, y el criterio es «sin PASS parcial» (DEC L1492) |
| **B-min** | herramientas de hechos fijadas, ejecutadas por el Controller; reproducción del aceptante (R1-R10) | disposición ordinaria de preparación + dictamen de materialidad; medición por A2-P2 en (i) o **A-6 estrecha** (B-min + una medición tipo A4-1, tope 1) | herramientas: base MV y lecturas de Git, stdout ASCII, sin escritura, autopruebas y mutaciones (≈ 1-2 sesiones); 1 revisión `claude-cli` + 1 reejecución (CLAUDE-CLI-I62, DEC L1085); 0-1 A-n; 1-2 sondas; 1-2 líneas del Owner | **la más demostrable**: conserva el adapter de D.3, OP_A/B/D y los 14 `Checks` ya vistos; las clases de fallo residuales pasan a hechos que se reproducen y se detectan |
| **B-ext** | paquete de la sesión | A-6 amplia (M-01..M-05, M-08); posible cambio de esquema (M-02); Owner si se toca el ADR | 1 A-n amplia, ≥ 2 revisiones, cambios de esquema y de validadores, 1-2 sondas | debilita la independencia; no recomendable |
| **C1** `codex-cli` `gpt-6.1-sol/high` | celda Equilibrado (CC L154-L155) | elección de celda ordinaria, con `ModelEscalationReason` (RT L53); medición por el mismo camino que A (el bloque no está atado a una celda: S70 L35-L41) | como A; puede compartir bloque con B-min (sonda 1 luna, sonda 2 sol) | como A; la medición 0/6 anterior está STALE y no informa (DEC L1320) |
| **C2** `claude-cli` como Controller | otro adapter | A-n material (V14 L2438-L2441; A4 L232-L233); línea nueva del Owner (CLAUDE-CLI-I62 cubre solo ARCHITECT/REVIEWER, DEC L1085; `A4-CLAUDE-FX02-CONSUMO` excluye EXECUTION_CONTROLLER, DEC L1294); operaciones 2-9 UNVERIFIED (`adapters/claude-cli.md` L23-L30 → no elegible, AP L1050-L1051); ejecución de Git sin medir (S72 L39); Proveedor PREFERRED sin cumplir frente al Worker `claude-subagent` | 1 A-n + 1-2 revisiones + caracterización y medición (presupuesto nuevo) + 1-2 líneas del Owner | larga; sin evidencia |
| **C3** `claude-desktop-session` o `codex-desktop-session` | sesión abierta por el Owner | operación «invocar» NO APLICA (`adapters/claude-desktop-session.md` L24-L27; `codex-desktop-session.md` L24-L27) → no invocable como rol; el Owner sería el transporte | — | no |
| **C4** `claude-subagent` | subagente del Principal | Sesión REQUIRED incumplida (AP L1152, L1168-L1169) | — | no |

**Demostración que cierra C-24** (V14 L2363; PASS = «ejecutado y conforme al oráculo», V14 L2534; necesita OD-5, OD-7, OD-2 y la medición del
binario, V14 L2544). En B1 y FX-U2 (A-5):

1. un Controller elegible por una medición ALL_OPERATIONS_DEMONSTRATED (DEC L1492) y un binding aceptado;
2. contrato T1 válido y revisión del Architect;
3. PLAN `delegation/v2` aceptado por A1'-A8', con nc4 rechazado;
4. Worker RED y GREEN con CI;
5. VERIFY = `EXECUTION_VERIFIED`, con las 14 comprobaciones en pass sobre G, aceptado después de R1-R10;
6. 8 negativos con invocación conformes al oráculo relativo (RAE L224-L226), y nc4 y N8-N10 de sesión;
7. Q7 conforme a B.8.4, dentro de los topes de D.3 con A-4 y A-5.

B-min no cambia esta lista: cambia cómo produce y cómo demuestra el Controller sus hechos.

## 8. Recomendación

**Adoptar B-min como estrategia de Controller de FX-02, y no B-ext.** Usar C1 como respaldo dentro del mismo bloque de medición. A queda como caso
particular: B-min sin herramientas.

**Ruta mínima de autoridad**

1. **Disposición ordinaria del Coordinator.** Adopta B-min como candidata y autoriza solo la **preparación estática**, sin invocaciones:
   - especificación de herramientas de **hechos**: Git (`Identity`, `Scope` con cobertura por ruta, `CleanTree`, `Trailer`, hash de la entrada),
     `Evaluate`/`Resolve` y MV-1..MV-7 sobre la base de MV (literal, más el informe de R61 b), subconjunto A3-A5/G4/G5 y lectores fieles;
   - stdout JSON solo ASCII, sin modo de escritura, solo biblioteca estándar, `python -I`, ejecución desde el blob;
   - autopruebas y mutaciones (incluida una salida alterada que el Controller debe detectar en la medición);
   - especificación de R1-R10;
   - colocación en B1: copia transitoria enumerada, sin tocar A5-1, o propuesta de A-n.
2. **Revisión independiente** con `claude-cli`, dentro de CLAUDE-CLI-I62 y con su disposición de lanzamiento. Dos preguntas: (a) ¿B-min es material?
   Ver AP L1264, ADR46 L69 y A5 L178-L186. (b) ¿Las herramientas son conformes al texto normativo, y no al oráculo?
3. **Medición:**
   - (a) con «no material»: en el próximo bloque A2-P2 tras una actualización observada y estable. Disposición que nombra el par, OD-2 y
     `BLOQUE-CODEX-CONSUMO`. Sonda 1 = `gpt-6-luna/high` con B-min; sonda 2 = `gpt-6.1-sol/high` con B-min solo si la primera falla;
   - (b) con «material», o si el Owner prefiere no esperar: **A-6 estrecha** (Architect + Coordinator) que fije B-min solo para FX-02 en F6 y abra
     **una** medición con la identidad vigente, tope 1 y sin reintento (molde de A4-1 regla 3, A4 L200-L204). Más una línea de consumo del Owner.
4. **Con ALL_OPERATIONS_DEMONSTRATED:** binding del Controller → B1 (Owner) → FX-U2 (A5-2 y consumo del Owner) → FX-02 con B-min y R1-R10 → C-24.
   Sin ese resultado, C-24 sigue UNVERIFIED (DEC L1507).

**Siguiente paso concreto:** el Coordinator emite la disposición del punto 1, sin invocaciones ni revisión todavía. Mientras tanto, la supervisión
sigue con la vigilancia pasiva de la identidad de Codex (`BinaryHash`, `AppVersion`, huella), que decide entre 3(a) y 3(b).

## 9. Supuestos y límites

- No se midió que `Add-Type` falle en ConstrainedLanguage: se infiere de que ese modo bloqueó `[System.IO.File]::ReadAllLines` en P2
  (failure-classes.json L7).
- Las probabilidades de éxito son cualitativas. Ninguna sonda midió B-min.
- B-min no rebaja ningún criterio: las 14 comprobaciones, la fidelidad, la independencia y ALL_OPERATIONS_DEMONSTRATED siguen iguales (DEC L1442,
  L1455). Solo cambia el instrumento con el que el Controller obtiene los hechos y añade reproducciones del aceptante.
- No se recomienda ningún modo de permisos. Cualquier ampliación de herramientas de `claude-cli` queda fuera de este análisis.
