# I-56 G1 — Evidence Audit del flujo de iniciativas

> ```text
> EVIDENCE AUDIT = COMPLETE        (este documento)
> PROPOSAL       = NOT STARTED
> WORKFLOW V2    = NOT EFFECTIVE    (contrato I-56, seccion 0.2)
> I-56           = GRANDFATHERED, gobernada por el workflow vigente (contrato I-56, seccion 0.1)
> NINGUNA INICIATIVA ACTIVA FUE MODIFICADA
> ```
>
> Documento de **evidencia**, no de politica. No propone, no recomienda y no pone en vigor nada. Donde la
> evidencia favorece una idea, se registra como **hipotesis** (seccion G), nunca como decision. Ninguna
> cifra de este documento cambia la politica de I-49, I-52, I-55 ni de I-56.
>
> **Anclas de evidencia.** Este documento cita SHAs cortos, IDs de corridas de GitHub Actions y conteos
> porque son su materia prima verificable, como lo hacen los documentos de evidencia de I-45. No es estado
> vivo ni contrato. La tension con la regla de AGENTS.md / WORKFLOW seccion 8 («hashes y conteos solo en
> HANDOFF seccion 12») se registra como hallazgo (Q10), no se resuelve aqui.

## 0. Alcance, metodo y reglas de lectura

### 0.1 Muestra

| Unidad | Rama | Estado al auditar | Rol en la muestra |
|---|---|---|---|
| I-45 | `architecture/test-validation-workflow` | integrada (merge `e85c588`) | obligatoria; ademas fuente de lo ya establecido (seccion 1) |
| I-48 | `architecture/generic-linked-property-editing` | integrada (`a4d88f1`) | obligatoria |
| I-50 | `feature/cotas-independientes-por-vista` | integrada (`f8deb67`) | obligatoria |
| I-51 | `feature/rackduplicar-multiples-origenes` | integrada (`46fcac2`) | obligatoria |
| I-53 {C} | (conceptual; ramas de E1) | cerrada | obligatoria, separada en cuatro unidades |
| I-53 E1 | `feature/cabeceras-configurables-multidestino` | integrada (`1b091be`) | |
| I-53S = E2 | `feature/cabeceras-multidestino-selectivo` | integrada (`104ef3a`) | |
| I-53D = E3 | `feature/cabeceras-multidestino-dinamico` | integrada (`dad4e77`) | |
| I-54 | `architecture/propiedades-personalizadas` | integrada (`ba497f1`) | obligatoria |
| I-52 | `feature/rackmirror-espejo-semantico` | **ACTIVA, incompleta** (punta auditada `2275f21`) | control; nunca se suma a totales de iniciativas cerradas |
| I-47 | (integrada, `507921f`) | — | solo para preguntas puntuales: origen de Project Variables y cierre directo sobre `main` |
| I-55 | `feature/creacion-de-vistas` (punta auditada `1e12414`) | ACTIVA | solo para contar re-descripciones de fundaciones |

Base de la auditoria: `origin/main` = `dad4e77` (sin avance durante la sesion). I-49, I-52 e I-55 se leyeron solo por
`git show` sobre refs remotas y por transcripcion de sesion, sin tocar sus worktrees.

**Instantanea de refs activas.** Las cifras de I-52 e I-55 se congelaron hacia 2026-09-14T16:30Z (I-52 `2275f21`, I-55
`1e12414`, I-49 `75f1862`). Esas ramas avanzaron despues (al cerrar la redaccion: I-52 `dd45b0f` con Proposal V13, tras
`915a520` con V12; I-55 `d091eeb`; I-49 `f6f0991`); **ninguna cifra de este documento se actualiza con ese avance**.

### 0.2 Clases de evidencia (aplican a cada cifra)

- **MEASURED**: verificado directamente en Git, registros de GitHub Actions, archivos, marcas de tiempo, logs de jobs o
  texto de transcripcion leido y medido.
- **RECONSTRUCTED**: derivado de varios artefactos durables sin medicion directa. **Toda afirmacion de ejecucion escrita en
  un commit, contrato, HANDOFF o reporte del executor (p. ej. «Core Full 6451/6451») es RECONSTRUCTED**, no MEASURED; una
  comparacion entre esa afirmacion y un conteo del CI es MEASURED solo en su mitad CI.
- **INFERENCE**: interpretacion apoyada en evidencia, no observable.
- **UNKNOWN**: evidencia insuficiente.

Reglas aplicadas: no se infiere una ejecucion porque un contrato la exigia; **las marcas de tiempo miden reloj entre
eventos, nunca duracion activa** (la duracion activa es UNKNOWN en toda la muestra); se distingue fecha del evento y fecha
de publicacion de su evidencia (0.5).

### 0.3 Fuentes inspeccionadas

| Fuente | Cobertura | Clase |
|---|---|---|
| Git: rangos `merge^1..merge^2` de cada unidad; `merge-base..punta` de I-52; cuerpos completos de commits; numstat; igualdad de arboles; `patch-id`; `range-diff`; tags `archive/*` | completa | MEASURED |
| GitHub Actions: 843 registros de corridas (volcado del 2026-09-14); jobs y logs de corridas seleccionadas (fallos, dispatch, merges) | completa para registros; logs muestreados | MEASURED |
| Docs en `main`: contratos, Discovery, Proposals, decisiones, ADR-0033/0034/0035/0037/0039, HANDOFF, ROADMAP, WORKFLOW, AGENTS, AUTOMATION_PLAN, guias, context packs | lectura dirigida; tamanos (blob) y solapamientos medidos | MEASURED / RECONSTRUCTED |
| Docs de I-52 e I-55 via `git show` de la ref remota | lectura dirigida | MEASURED |
| Transcripciones de sesiones executor (solo lectura): I-45 `local_9d027cb5…` (2277 mensajes), I-48 `local_e4b9a34c…` (2603), I-50 `local_22ab6f1d…` (3361), I-51 `local_c4924edb…` (1059), I-53 `local_d8cafadf…` (6035; una sesion para las cuatro unidades), I-54 `local_b1d23b41…` (6278), I-52 `local_7a6025bd…` (~4608, en ejecucion) | I-51, I-48, I-45, I-50: completas; I-53 ~73%; I-54 ~72%; I-52 ultimos ~1300 mensajes | MEASURED (texto) / RECONSTRUCTED (afirmaciones dentro del texto) |
| Mediciones mecanicas: containment de 8-shingles y lineas verbatim entre documentos; simulacion de merges en modo trivial (`git merge-tree` a stdout, sin escribir objetos) | completas para los documentos listados | MEASURED |

Metodo: la lectura documental y de transcripciones se repartio en diez auditorias de solo lectura (una por unidad, una de
fundaciones y dos de ordenes). Sus cifras se cruzaron contra Git y Actions; donde discreparon se re-midio (0.6). Un
verificador adversarial independiente reviso el borrador contra las fuentes antes del commit (seccion 9).

### 0.4 Hecho estructural que condiciona toda lectura de «revision de Arquitecto»

**MEASURED en I-45, I-48, I-50, I-51, I-53, I-54 e I-52**: cada revision de Arquitecto se ejecuto **dentro de la misma
sesion executor**, disparada por una orden de cambio de rol («Actua exclusivamente como Arquitecto par de I-48», «Actua como
ARQUITECTO de I-53», «Vuelve al rol de ARQUITECTO independiente»). Los registros de decisiones de I-51, I-52 e I-53 lo
declaran («la revision la ejecuto el mismo agente que redacto el Discovery»). En ordenes de I-45, I-48, I-50, I-51, I-52,
I-53 e I-54 el executor es nombrado «Codex» mientras el modelo de la sesion es Claude. En este documento «Architect round»
significa **revision adversarial de rol dentro de la sesion**, pedida por el Coordinador. **Su independencia real es
UNKNOWN**; tampoco se encontro evidencia de revisiones externas adicionales para estas unidades (UNKNOWN, no ausencia
probada).

### 0.5 Fecha del evento vs fecha de publicacion de la evidencia

| Caso | Evento | Publicacion de la evidencia | Clase |
|---|---|---|---|
| Revisiones de Arquitecto (todas) | texto en transcripcion | resumen en decisiones/Proposal siguiente; texto original **no versionado** en I-48, I-50, I-53, I-54 | MEASURED |
| Validaciones del Owner | fuera de sesion | veredicto transcrito en el commit de cierre (p. ej. I-51: solo via texto del Coordinador en la orden G8) | RECONSTRUCTED |
| I-45 mediciones del Discovery | antes del commit de reclamo (sesion creada 2026-09-04T21:56Z; reclamo 19:38 −06) | Discovery versionado despues | RECONSTRUCTED |
| Merge y CI post-merge de I-51 | 2026-09-12 | el MERGE_SHA no queda registrado como tal en los documentos de I-51 (contrato y HANDOFF quedaron en «MERGE_SHA = PENDING»; `46fcac2` solo aparece como base en documentos de I-50, I-53 e I-54) y la corrida post-merge `34724848178` no aparece en ningun documento | MEASURED |
| CI post-merge de I-50 | 2026-09-13 | registrado en documentos de I-53, no de I-50 | MEASURED |
| Guardia `patch-id` del merge sin rebase de I-50 | integracion | no registrada en el repo; re-medida en esta auditoria | MEASURED (re-medicion) |

### 0.6 Correcciones hechas durante la auditoria

1. **I-45 corridas de rama**: 20 sobre SHAs supervivientes + 1 huerfana (`0864662`, cierre reescrito), no 21 sobre la
   historia final. (MEASURED)
2. **I-45 clasificacion de commits**: G0A y G4 tocan `AGENTS.md` pero ninguna ruta ejecutada por CI; solo 4 de 21 commits
   tocan rutas construidas o probadas por CI. (MEASURED)
3. **Blob esperado de la Proposal en el cierre de I-48**: el reporte final del executor atribuyo el error a la orden; la
   orden daba `17a9068` sin nombre de archivo y ese blob **es** `I-48-proposal-v8.md`; el executor habia consultado `-v1`
   («Era mi consulta la que apuntaba al fichero equivocado»). Error del executor, mal atribuido. (MEASURED)
4. **Conflictos del merge de I-50 con I-51**: una auditoria parcial leyo `git show --cc` sin conflictos; la simulacion en
   modo trivial reproduce conflictos **solo documentales**: HANDOFF 5 hunks, ROADMAP 1, ideas-futuras 1, guia de validacion
   manual 1; codigo 0. (MEASURED; el modo trivial puede diferir de la estrategia `ort` en casos limite)
5. **«18 defectos» de I-45 Discovery**: las filas suman 17 (8+6+3+0+0+0); ADR-0033 repite «dieciocho». (MEASURED)
6. **I-48 «tres gates de diagnostico»**: enumerados, fueron 5 gates sin commit (G4G, G4G.1, G4G.2, DIAG, DIAG2) antes del
   fix G4G.3; «tres» aparece en ideas-futuras y en el reporte final del executor sin enumerar. (MEASURED / RECONSTRUCTED)
7. **Primer commit verificable por usuario de I-50**: posicion 17 de 20 (`ed50cbd`), no 18. (MEASURED)
8. **Commits de producto/pruebas de I-48**: 10, no 9. (MEASURED)
9. **Filas de ROADMAP con hashes**: las contienen I-13, I-23, I-32..I-35, I-37D, I-40..I-44 e I-47; las filas de I-45 e
   I-48..I-54 no. (MEASURED)
10. **Cuelgue de UI de I-48**: ocurrio en el job de UI del runner Windows, no en Linux. (MEASURED)

---

## 1. Lo que I-45 ya establecio vs lo que I-56 mide nuevo

| Tema | Ya establecido por I-45 (se reutiliza, no se rehace) | Nuevo en I-56 |
|---|---|---|
| Linea base local/CI | Core Full mediana 92.15 s y UI Full 273.30 s (G1, n=3, `233d52e`); ciclo obligatorio 333.44 s; CI mediana 220 s (40 corridas); ruta critica `max(UI, Tests→Plugin)`; ruido ~2x (RECONSTRUCTED: JSON no versionados) | no se re-mide; I-56 solo cuenta **cuantas veces** se ejecuto cada clase por unidad |
| Repeticion | corpus I-40/I-42/I-43/I-44: 440 ejecuciones, 64 CROSS_CHANNEL, **0 EXACT_SHA_RECONFIRMATION**; 160 afirmaciones Full sin SHA → «LOCAL FULL PER CANDIDATE = UNKNOWN» (RECONSTRUCTED) | repeticion **posterior** a la regla exact-SHA (I-48..I-54): duplicados antes/despues del commit, re-validacion por rebase, CI sobre SHAs docs-only |
| Exact-SHA | `Directory.Build.targets` estampa el SHA; reutilizacion solo por SHA exacto; tres invalidadores; docs-only no hereda (MEASURED en AGENTS.md) | cuanto retrabajo real produjo la regla y cuanto el avance de `main` (Q9) |
| LC-UI | la suite UI local deja de ser obligatoria en la iteracion ordinaria; la evidencia intermedia de UI la aporta el CI push; el Core queda fuera («no se deduce ninguna por analogia con LC-UI») (MEASURED en AGENTS.md) | cuantas unidades lo aplicaron (I-51, E1) y cuantas corrieron UI Full local igualmente (Q7) |
| Fuentes de defectos | corpus I-40/I-43/I-44: Owner manual 8, revision arquitectonica 6, prueba de regresion nueva 3, Full/CI/prueba preexistente 0 (RECONSTRUCTED; filas suman 17, rotulo 18) | etapas que encontraron defectos en I-45..I-54 (TABLE B) |
| Rechazados | T0–T4 y R0–R4 (definiciones solo en transcripcion), seleccion por impacto y por FQN/ruta/grafo de proyectos, taxonomia de pruebas, clave de estado de validacion por contenido, igualdad de arbol, retirar cobertura, reducir validacion del Owner, multi-STA inmediato (ADR-0033 L197-226, MEASURED; ADR en estado `propuesto`) | **no se reintroducen** bajo otro nombre (secciones G y H) |
| Duracion activa del Owner | experimento no bloqueante (guia §8); linea base UNKNOWN | ningun valor capturado encontrado en `main`; las plantillas de cierre de I-51/I-53 dejan «Duracion activa aproximada: ___ min» en blanco (MEASURED) |
| Sobrecarga de proceso por iniciativa | **no medida por I-45** | objeto de este documento: rondas, versiones, ordenes, documentacion, ceremonia por unidad Git |

---

## 2. TABLE A — Initiative Process Inventory

Clases por columna: Discovery rounds, Proposal versions e Implementation gates **MEASURED** (commits) salvo nota; Architect
rounds **MEASURED** en transcripcion para I-45/I-48/I-51/I-52 y **RECONSTRUCTED** (resumenes versionados) para I-50/I-53/I-54;
Core Full y UI Full = **ejecuciones locales RECONSTRUCTED** (afirmaciones registradas); Candidate attempts y Owner rounds
**RECONSTRUCTED** salvo SHAs y CI; CI runs **MEASURED** (rama / main).

| Initiative | Change shape | Discovery rounds | Proposal versions | Architect rounds | Implementation gates | Core Full | UI Full | Candidate attempts | CI runs | Owner rounds | Process deviations | Evidence confidence |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| I-45 | Proceso y normas + CI + herramientas `eng/validation`; producto: 4 lineas borradas; AGENTS +143%, WORKFLOW +82% | 2 (DR0 antes del reclamo; DR1 cierre D1–D4) | Plan V1–V4 (solo V4 versionado; no archivos por version) | 7 (plan R1–R4; conformidad CR1–CR3) | 17 commits de gate; 4 tocan rutas de CI; 5 correcciones (G2, G1, G6-C1, G7-C1, G7-C2) | 1 validacion (Candidato) + corridas de medicion (Discovery, G1, G5) + 1 corroboracion G0B | 1 validacion + 10 corroboracion G0B + medicion | 1 | rama 21 (20 + 1 huerfana; 1 fallo) / main 2 | 0 | Discovery con mediciones antes del reclamo; bootstrap corregido (ubicacion ROADMAP + hash en contrato); WORKFLOW autorado con remedio «corregir sobre `main`» (G7-C1) y corregido en G7-C2; cierre reescrito con force-push | ALTA Git/CI; MEDIA rondas y ejecuciones locales |
| I-48 | Arquitectura: kernel generico de propiedades vinculables + editor UI; extiende I-47 sin ADR; src +3,891/−427, tests +6,627/−153 | 2 (G1 + adenda G1.1 pedida por el Coordinador) | 8 archivos (V1..V8) + 2 revisiones de V1 = 10 estados; V8 no autocontenida | 8 (7 NOT AGREED, 1 AGREED): 2 BLOCKER / 18 MATERIAL / 15 MINOR | 10 commits producto/pruebas (4 verificables por usuario) + 5 gates de diagnostico sin commit | ≥11 registradas (9 en cuerpos de commit G4A..G4F; G4G y G4G.3 en transcripcion); 34 invocaciones de suite sin filtro | ≥10 registradas (6 en commits; 4 en transcripcion); 44 invocaciones | 2 SHAs (`acecde6` falla CI; `b054799` PASS) + 2 intentos sin commit | rama 27 (2 fallos) / main 2 | 1 PASS (transmitido por el Coordinador) | Estado ROADMAP incorrecto en bootstrap; CI rojo desde G4E sin consultar hasta G4G; orden G3 enviada dos veces; al Owner se le indico un DLL con marca de tiempo anterior al Candidato (mismo arbol de `src`; DLL cargado UNKNOWN); cierre con «MERGE_SHA = PENDING» sin actualizar | ALTA |
| I-50 | Feature ID1: politica de cotas por tipo de vista (ADR-0035) en 3 sistemas; produccion +465/−35, pruebas +4,377; Plugin 0 | 1 | 3 (V1, V1.1, V1.2) | 2 (V1.1 NOT AGREED; V1.2 AGREED) + 1 NOT AGREED del Coordinador sobre V1 | 11 commits (8 internos; 3 verificables; el primero verificable es el commit 17 de 20) | 7 registradas en commits/docs; 13 sobre 12 SHAs segun transcripcion | 4 | 1 | rama 21 (20 push + 1 dispatch; 1 fallo) / main 2 | 1 PASS COMPLETO | Punta roja publicada (G5-01); merge sin rebase por orden (excepcion a WORKFLOW 4.5.1/4.5.3); guardia patch-id no registrada; cobertura del Candidato medida dos veces; «tier I-45» pedido en ordenes (no existe) | ALTA |
| I-51 | Feature ID15: planificador puro + comando multi-seleccion; extiende ADR-0009 sin ADR; +3,814/−107 | 1 | 0 archivos (contrato vinculante en G2) | 1 (AGREED WITH CHANGES, 8 cambios) | 3 (1 verificable) | 4 (G3, G4, G5, Candidato); conteos afirmados iguales a los del CI (comparacion MEASURED) | 1 (Candidato) | 1 | rama 9 (8 push + 1 dispatch) / main 1 | 1 PASS TOTAL | Dispatch de cobertura en rama antes del merge (autorizado por la orden; distinto de WORKFLOW 4.5.7); MERGE_SHA y CI post-merge no registrados en docs; granularidad de UNDO (M9) sin registro | ALTA (transcripcion completa) |
| I-53 {C} | Solo docs: decidir ID6+ID7 para Selectivo y Dinamico (Push Back ya lo tenia por I-40) | 1 | 2 + 1 revision en sitio de V2 al congelar (+541/−520 lineas) | 2 (V1 AGREED WITH CHANGES; V2 AGREED) | 0 | 0 | 0 | 0 | incluidas en E1 (5 huerfanas + congelacion) | 0 validaciones; decisiones OD-2.b, OD-6, OD-8, ADR-0037 | La particion tomo 5 formas (4 cambios); texto congelado no identico byte a byte al revisado | MEDIA-ALTA |
| I-53 E1 | Nucleo compartido + Application de Selectivo y Dinamico; sin cambio visible | 0 (compartido) | 0 | 0 | 3 (0 verificables) | 4 | 1 | 1 | rama 10 (5 huerfanas) / main 3 (push + 2 dispatch) | 1 smoke sobre codigo sin llamadores de produccion | Limpieza antes de la cobertura del Candidato (la orden contradijo WORKFLOW) → E1-I.1; dispatch redundante sobre MERGE_SHA; ROADMAP «integrada» antes de ID6/ID7 visibles | ALTA |
| I-53S = E2 | UI Selectivo | 0 | 0 | 0 | 1 (verificable) | 2 | 2 | 1 | rama 4 / main 2 | 1 PASS (14 escenarios) | Suites pre-commit estampadas con el SHA del padre → repeticion; fetch antes de la primera edicion del archivo caliente fuera de secuencia | ALTA |
| I-53D = E3 | UI Dinamico + L-1 + retiro de presets (OD-8) | 0 | 0 | 0 | 1 (verificable) | 3 | 3 | 2 (`d280197` BLOCKED por avance de `main`; `a57bd50` PASS) | rama 5 (3 huerfanas) / main 2 | 1 PASS | Rebase E3-C.1 + re-validacion completa; reformulacion erronea «aviso y confirmacion» en contrato y fila ROADMAP (persiste en `main`) | ALTA |
| I-54 | Arquitectura: fundacion Custom Properties (ADR-0039) + extension del sobre; src +4,624, tests +8,752 | 1 + 3 adendas de re-medicion de paralelas | 5 | 5 (4 AGREED WITH CHANGES, 1 AGREED): 7 MATERIAL / 20 MINOR | 6 commits producto/pruebas (1 verificable: G7) + 6 commits docs `-CLOSE`/G7A | ~14 registradas (incluye 4 duplicados antes/despues del commit) | 4 | 1 (`02987bd`, commit docs-only) | rama 25 (21 sobre SHAs reescritos; 4 en la historia final) / main 2 | 1 PASS | 3 commits sobre base superada (solo G7A etiquetado); 3 rebases; rebase sin tag de archivo (G2G); orden de cierre esperaba 4 archivos y WORKFLOW exigia 6 | ALTA Git/CI; MEDIA rondas (textos no versionados) |
| I-52 (control, a la punta `2275f21`) | Feature ID16 RACKMIRROR; sin codigo aun | 1 (+1 registro) | 11 | 11 completas (G1 + V1..V10); V11 en revision en la instantanea (la rama avanzo luego a V12 y V13) | 0 (G3 NOT OPEN) | 0 | 0 | 0 | rama 15 (14 docs-only + reclamo vacio) | 0 (O-1 PENDING) | Contrato y fila ROADMAP de la rama sin actualizar desde el bootstrap | ALTA para lo medido; **provisional** |

---

## 3. TABLE B — Defect / Finding Capture

«Architect-role» = revision de rol dentro de la sesion executor (0.4). La columna de etapa previa es siempre INFERENCE y
expresa detectabilidad, no un remedio.

| # | Initiative | Finding | Materiality | Detected by | Stage | Changed design/code/process? | Could a cheaper earlier stage plausibly detect it? | Evidence class |
|---|---|---|---|---|---|---|---|---|
| B-01 | I-51 | GUID generado dentro del restamp compartido: una copia multivista recibiria N GUIDs | HIGH | Executor (lectura de codigo) | Discovery G1 | Diseno + codigo (G4) | Ya era la etapa mas temprana | RECONSTRUCTED |
| B-02 | I-51 | RACKLISTA/RACKBOMTOTAL cuentan el MAX de referencias: clonar por referencia cambiaria el conteo | HIGH | Executor | Discovery | Diseno (INV-06, T4, M4) | Idem | RECONSTRUCTED |
| B-03 | I-51 | Divergencia Id/Name entre vistas hermanas aborta en tres autoridades | HIGH | Executor | Discovery | Diseno (INV-05) | Idem | RECONSTRUCTED |
| B-04 | I-54 | `Compose` descarta un campo nuevo del sobre (corrige la hipotesis H-3 del Coordinador) | MATERIAL | Executor | Discovery | Diseno (D-21, T-GRD-02) | Idem | RECONSTRUCTED |
| B-05 | I-53 | L-1: el configurador del Dinamico devuelve una instancia obsoleta (perdida de receta) | HIGH, defecto real | Executor | Discovery | Codigo en E3 (RED observado) | Idem | RECONSTRUCTED |
| B-06 | I-48 | `UnlinkAllAndDelete`: 1 variable → N propiedades fabrica un vinculo roto y reporta Success | HIGH (dato silencioso) | Coordinador | Revision de G1 → adenda G1.1 | Diseno (D-07) | Plausible en G1, que ya habia encontrado el hardcode pero no su consecuencia | MEASURED (orden) / RECONSTRUCTED |
| B-07 | I-48 | Criterio de aceptacion por token ciego a hardcodes de campo | BLOCKER | Architect-role AR1 | Proposal | Oraculo de prueba | Parcial: el Discovery midio token vs campo | MEASURED |
| B-08 | I-48 | `RepairBroken` queda bloqueado para siempre con 2 vinculos rotos | BLOCKER | Architect-role AR2 | Proposal | Diseno (`RepairBrokenRack`) | Plausible: el Discovery lo registro como fuera de alcance | MEASURED / INFERENCE |
| B-09 | I-48 | La re-lectura en el commit del executor evita la acreditacion (`RemoveAll` → perdida de datos) | MATERIAL | Architect-role AR7 (7a ronda) | Proposal | `RegistryCommit` | Derivado de una afirmacion de V7 | MEASURED |
| B-10 | I-48 | `Enum.TryParse` amplio la gramatica persistida de `Type` | MATERIAL (contrato de persistencia) | Coordinador tras el gate | G4A → G4A.1 | Codigo | Plausible: el Discovery registro el uso de `Enum.TryParse` | MEASURED / INFERENCE |
| B-11 | I-48 | Insertar una vista durante RACKEDITAR pierde `PropertyValues` (rack inoperable) | MATERIAL | Coordinador tras el gate | G4C → G4C.1 | Codigo | Poco plausible: ninguna Proposal menciona el camino de insercion | MEASURED |
| B-12 | I-48 | Cuelgue del job de UI en CI (runner Windows): fixtures con ids fuera de catalogo + `MessageBox` de produccion sin costura | Bloquea el Candidato | CI push en G4E y G4F; advertido en G4G | Implementacion | Solo pruebas; 5 gates de diagnostico | Plausible desde G4E: su CI fallo 12 min antes de la orden G4F; ninguna consulta de CI entre G4A y G4F; el Full local afirmado verde en G4G no lo detecto | MEASURED (CI) / RECONSTRUCTED (Full local) |
| B-13 | I-48 | Al Owner se le indico un DLL con marca de tiempo anterior al Candidato (mismo arbol de `src`, otro SHA); el DLL realmente cargado es desconocido | Proceso (evidencia) | Esta auditoria (transcripcion) | Validacion | Ninguno | Plausible: un registro del SHA del DLL entregado lo habria expuesto | RECONSTRUCTED (transcripcion) / UNKNOWN (DLL cargado) |
| B-14 | I-50 | V1 leia enteros negativos como legacy (un valor presente se volvia ausente) | MATERIAL | Coordinador | Proposal V1 | Diseno (P-07) | Plausible: V1 lo tenia abierto como QA-2 | RECONSTRUCTED |
| B-15 | I-50 | MAT-A1: gates no ejecutables (T-06 depende de C-05; T-08 de C-10/C-14/C-15) | MATERIAL | Architect-role | Proposal V1.1 | Plan de gates | Plausible en la revision del Coordinador de V1 | RECONSTRUCTED |
| B-16 | I-50 | MIN-4: la carga marca «touched» y materializa `7` en legacy | MINOR registrado; confirmado real | Architect-role → pruebas RED2 («Expected null / Actual 7» ×3) | Proposal → G3 | Codigo en 3 ventanas | — | RECONSTRUCTED |
| B-17 | I-50 | Pines JSON dependientes de CRLF | Portabilidad de pruebas | CI push (job Core, Ubuntu) | G5 A | Pruebas (A.1); desviacion G5-01 | Poco plausible con Full local en Windows (el commit A.1 afirma que paso con pines CRLF) | MEASURED (CI) / RECONSTRUCTED (causa y Full local) |
| B-18 | I-50 | Vistas compuestas A/B quedan legacy sin C-15 | MATERIAL (riesgo #3 de G1 confirmado) | Prueba RED T-08 | G6 | 1 linea de codigo | Predicho en G1 | RECONSTRUCTED |
| B-19 | I-53 | H-01..H-07: destruccion de recetas; rechazo de fondos inexistentes contra I-43; mezcla Plan/Outcome; `IsManualOverride` sobrecargado; presets como segundo mecanismo (→ OD-8); captura oculta del origen; `ModuleId` posicional | HIGH ×7 | Architect-role | Proposal V1 | Diseno V2 + decision OD-8 | Plausible para H-02, H-04, H-05 desde material del Discovery | RECONSTRUCTED |
| B-20 | I-53 | RR-01: frescura del sistema resuelto que lee PREPARE | MEDIUM | Architect-role 2a ronda | Proposal V2 | Contrato (S-32) | Aparecio en la 2a auto-revision, no en la 1a | RECONSTRUCTED |
| B-21 | I-54 | AR-54-01..05: descarte destructivo, filtro «solo colocados», ausente≡vacio, aislamiento sobre-reclamado, sin invariante de preservacion | MATERIAL ×5 | Architect-role | Proposal V1 | Reescritura V2 (22 cambios) | UNKNOWN | RECONSTRUCTED |
| B-22 | I-54 | AR-54-V2-01 `Kind` desconocido escribible; AR-54-V2-02 limite de profundidad fuera del ADR (introducido por V2) | MATERIAL ×2 | Architect-role | Proposal V2 | Diseno + codigo G5/G6 | UNKNOWN | RECONSTRUCTED |
| B-23 | I-54 | AR-54-V3-01: `Normalize(FormC)` lanza con no-caracteres | MINOR registrado; defecto de correccion | Architect-role con sonda ejecutada | Proposal V3 | Validacion en G3 | Plausible: una prueba unitaria en G3 lo habria detectado | RECONSTRUCTED |
| B-24 | I-54 | AR-54-V4-01: redaccion «fallan sin escribir» incorrecta (introducida por el texto del cambio C-F2) | Editorial | Architect-role | Proposal V4 | Solo redaccion | Plausible en una revision editorial de C-F2 | RECONSTRUCTED |
| B-25 | I-45 | Igualdad de arbol ≠ identidad de binario (el SHA se estampa); corrige la regla de igualdad de arbol que la ronda R2 habia introducido | MATERIAL («se descubrio tarde») | Punto R3-01 del Coordinador, confirmado por Architect-role R3 | Plan | ADR-0033 §5/§7; AGENTS | Plausible leyendo `Directory.Build.*` | MEASURED / RECONSTRUCTED |
| B-26 | I-45 | ARCH-01 CI post-merge sin paso operativo; ARCH-02/03 `head_sha` del dispatch contamina evidencia (creados por G6-C1); ARCH-04 la metadata podia eximir la validacion del Owner | HIGH ×4 | Architect-role CR1, tras «NONE» del Coordinador | Conformidad | WORKFLOW, AGENTS, AUTOMATION_PLAN | Plausible en P0/G6 cruzando AGENTS↔WORKFLOW | MEASURED / RECONSTRUCTED |
| B-27 | I-45 | ARCH-05: dos remedios incompatibles en `19a555f` (WORKFLOW:238 «se corrige sobre `main`» vs :296 «sobre la rama»), creado por G7-C1 | HIGH | Architect-role CR2 | Conformidad | WORKFLOW (`e932c0c`: «Nunca con un commit directo sobre `main`») | Plausible con una busqueda de «sobre main» | MEASURED |
| B-28 | I-45 | La cadencia de cobertura (ADR-0033 §9) no tenia gate en el plan V4 | Brecha de conformidad | Coordinador | Tras G6 | `ci.yml` (G6-C1) | Plausible cruzando ADR↔plan en P0 | MEASURED / RECONSTRUCTED |
| B-29 | I-51 | `newId` como string → Cantilever cae a GUID aleatorio | MEDIUM | Architect-role (unica ronda) | Revision tras G1 | API tipada `Guid` | Plausible: G1 ya citaba la linea | RECONSTRUCTED |
| B-30 | I-51 | INV-09 (preservacion View/Section/SchemaVersion/ExtensionData) sin prueba de comportamiento | MEDIUM (criterio parcialmente no probado) | Discovery de I-54, **despues del merge** | Post-merge | Ninguno en I-51 | Plausible cruzando lista de pruebas ↔ invariantes en G2 | RECONSTRUCTED |
| B-31 | I-53D | Contrato y fila ROADMAP dicen «aviso y confirmacion»; V2 §7.8 no tiene aviso y G7-4 registra «sin confirmacion» | LOW doc; persiste en `main` | Executor (G7-4) / esta auditoria | G7 | Codigo sin confirmacion; fila ROADMAP sin corregir | Plausible leyendo V2 en G0 | MEASURED |
| B-32 | I-53 E1 | La orden E1-I apunto la cobertura al MERGE_SHA («No Candidate») contra WORKFLOW 4.5.7; limpieza antes de la cobertura del Candidato | Proceso | UNKNOWN (el executor siguio la orden y anoto la discrepancia); remediado por la orden E1-I.1 | Integracion | Remediacion + registro de desviacion | Plausible: la regla estaba literal en WORKFLOW | MEASURED |
| B-33 | I-53D | `main` avanzo (merge de I-54) durante la evidencia de E3-C | Coordinacion | Fetch final del executor | Candidato | BLOCKED → rebase + re-validacion | No: no evitable por una etapa previa | MEASURED |
| B-34 | I-54 | Tres commits sobre una base ya superada (V3, G4A/G4B, G7A); solo G7A etiquetado como desviacion | Proceso, no material | Executor / Coordinador | G2E, G4, G7 | Rebase + tag de archivo | Plausible con un re-fetch inmediatamente anterior al commit | MEASURED |
| B-35 | Transversal | Comentario `RackBlockData.cs:7-11` («extension dictionary de la referencia», cuando todos los llamadores usan la definicion) registrado 5 veces de forma independiente (I-47, I-50, I-51, I-54, I-55); sin corregir | LOW (doc de codigo) | Discovery ×5 | Discovery | Ninguno | Plausible con un registro previo consultable | MEASURED |
| B-36 | Transversal | WORKFLOW (lineas 205, 300, 357) y AGENTS.md:194 citan «HANDOFF §8-12» y WORKFLOW:361 cita «§12»; HANDOFF solo tiene §1–§7; advertido en 3 cierres (I-45, I-48, I-51) | Proceso doc | Executors en cierre | Cierre | Ninguno | — | MEASURED |
| B-37 | Transversal | Marcador de conflicto de ancestro comun (siete barras verticales seguidas de `085ca2f`) en `docs/ideas-futuras.md:816`; visto por el Discovery de I-47 y por I-50 | Doc | Discovery I-47; executor I-50 | Discovery / cierre | Ninguno | — | MEASURED |
| B-38 | Transversal | Owner Validation en AutoCAD: 0 hallazgos registrados en I-48, I-50, I-51, E1, E2, E3 e I-54 (7 rondas, PASS a la primera) | — | Owner | Validacion | — | — | RECONSTRUCTED (ausencia registrada; **no prueba falta de valor**) |
| B-39 | I-52 (control, activa) | G1 + V1..V10: la revision de G1 dio 18 hallazgos (6H/7M/5L); V1..V10 dieron 127 (8 HIGH / 32 MED / 87 LOW), HIGH solo hasta V4; clasificacion de los 127: 12 aspecto nuevo, 41 corrige un mecanismo previo, 43 detalle de contrato/prueba, 13 deriva paralela, 17 editorial, 1 proceso, 0 decision tardia del Owner | — | Architect-role | Proposal | Solo texto (sin codigo) | Plausible para AR-01..03 desde el Discovery | MEASURED (conteos) / INFERENCE (clasificacion) |
| B-40 | I-48 | `FindBroken`: gana el primer hermano en lugar de la autoridad authored; el texto de consentimiento dice «esta propiedad» pero repara el lote | MATERIAL ×2 | Coordinador tras el gate | G4B → G4B.1 | Codigo y UI | Plausible: la especificacion existia (V3-R05, V3-R01/V4-R08); la implementacion la omitio | MEASURED (orden) / RECONSTRUCTED |
| B-41 | I-51 | Guardas literales historicas quedaban en verde con la condicion rota (restamp dentro de la transaccion; rama literal `"pushback"`) | MEDIUM (calidad de prueba) | Demostraciones RED de G4 | G4 | Pruebas (G-R1, G-R3b) | Parcial: G1 marco las guardas literales, pero el punto ciego aparecio solo con RED | RECONSTRUCTED |

---

## 4. TABLE C — Repeated / No-New-Evidence Operations

Clasificacion INFERENCE con un unico valor por fila; los matices van en las otras columnas. «Repetido» no equivale a
desperdicio: se registra que evidencia existia y que anadio la repeticion.

| # | Initiative | Operation | Previous valid evidence | Why repeated | New evidence produced | Classification |
|---|---|---|---|---|---|---|
| C-01 | I-48 | 17 de 27 corridas CI sobre SHAs docs-only o vacios (66.9 de 116.8 min de reloj CI) (MEASURED) | `src`/`tests` identicos a la base, ya verde | `ci.yml` corre en todo push | Verde por SHA; ninguna evidencia de producto | ceremonial/redundant |
| C-02 | I-54 | 18 de 25 corridas sobre SHAs docs-only o vacios (~70 min); 21 de 25 sobre SHAs reescritos despues (MEASURED) | — | Push + 3 rebases | Nada que sobreviva en la historia final para las huerfanas | ceremonial/redundant |
| C-03 | I-53 (E1+E2+E3; incluye las corridas documentales de {C} en la rama de E1) | 13 de 19 corridas de rama sobre SHAs docs-only o vacios (~54 min) (MEASURED) | Arboles ya verdes | `on: push` | Sello de SHA | ceremonial/redundant |
| C-04 | I-45, I-51, I-53 | CI sobre el commit vacio de reclamo con arbol igual al de `main` ya verde (MEASURED) | Corrida de `main` | `on: push` | Nuevo SHA estampado | ceremonial/redundant |
| C-05 | I-45, I-48, I-51, I-54, E1, E2, E3 | CI post-merge con arbol(merge) == arbol(cierre); en I-50 el arbol del merge es distinto (merge sin rebase) (MEASURED) | CI del cierre sobre un SHA distinto con el mismo arbol | Regla exact-SHA (AGENTS; WORKFLOW 4.5.6) | Evidencia del MERGE_SHA y artefacto de cobertura en `main`; sin evidencia de producto nueva mas alla del sello de SHA; 0 fallos en la muestra; la regla vigente no admite equivalencia por arbol | necessary |
| C-06 | I-53 E1 | Dispatch de cobertura sobre MERGE_SHA ya cubierto por el push con `cobertura=true` (MEASURED) | Push 34770347066 mismo SHA | La orden E1-I apunto al MERGE | Ninguna | ceremonial/redundant |
| C-07 | I-50 | Dispatch en `main` midiendo el mismo Candidato ya medido por el dispatch de rama (MEASURED) | Dispatch 34733326111, mismo SHA y mismo `ci.yml` | Texto de WORKFLOW 4.5.7 | Ninguna | ceremonial/redundant |
| C-08 | I-54 | Core Full «antes del commit y otra vez sobre el SHA» ×4 (G3, G5, G6, G7) | Corrida pre-commit sobre el mismo arbol | Orden de evidencia de AGENTS (la corrida previa estampa el padre) | Sello de SHA | useful but reducible |
| C-09 | I-53S | E2-C repitio Core+UI Full porque G5 corrio antes del commit («llevaban el SHA del padre, asi que no cuenta») | Mismo contenido | Estampado del SHA | Sello de SHA | useful but reducible |
| C-10 | I-54 | G8: Core+UI Full y builds sobre el Candidato docs-only `02987bd` (sin diff de `src`/`tests` respecto de G7 post-commit, MEASURED) | G7 post-commit | Candidato = SHA exacto; DLL estampado | DLL con el SHA del Candidato para el Owner; sin evidencia de producto nueva mas alla del sello | necessary |
| C-11 | I-50 | Core Full local 6249 repetido ×4 con entradas de Core sin cambio (`8ceb3a7`→`6cd2970`, diff vacio en Domain/Application/tests, MEASURED) | `8ceb3a7` local + CI | Habito por commit y ordenes | Ninguna para Core | useful but reducible |
| C-12 | I-53D | E3-C sobre `d280197` + E3-C.1 sobre `a57bd50` (mismo patch-id; arbol +57 archivos de I-54, MEASURED) | Full + builds + CI sobre `d280197` | `main` avanzo; nuevo SHA; arbol distinto | G7 combinado con I-54 (Core 6451→7240, UI 1455→1585); valor de deteccion bajo (INFERENCE) | necessary |
| C-13 | I-53 E1 | Smoke del Owner en AutoCAD sobre codigo sin llamadores de produccion | E1-INV: 0 diff en UI/Plugin | Requerido por la metadata/regla registrada | Confirma que nada visible cambio | ceremonial/redundant |
| C-14 | I-48 | Rondas AR4–AR7 | Rondas previas | Cada una con ≥1 MATERIAL aceptado | Hallazgos convertidos en codigo, derivados de construcciones introducidas en la reconciliacion anterior | useful but reducible |
| C-15 | I-54 | G2J (0 hallazgos) y G2H (1 editorial + integridad de rebase) | V4 y V5 ya revisadas | Protocolo exact-SHA | Confirmacion | useful but reducible |
| C-16 | I-45 | R4 (0 materiales; 2 aclaraciones) y CR3 (0 bloqueantes; detecto paquete desactualizado) | R3 / CR2 | Protocolo tras PENDING/NOT AGREED | Aclaraciones; deteccion de desactualizacion | useful but reducible |
| C-17 | I-52 (activa, no agregada) | Rondas V5–V10 con hallazgos mayormente de clases «corrige mecanismo previo», «detalle» y «editorial»; V7 y V8 derivadas de MED latentes con costo medido 0 | Versiones previas | Cierre estricto del contrato antes de G3 | Texto de contrato; ninguna evidencia de producto | unknown |
| C-18 | I-52 (activa, no agregada) | Bloque de estado ×11; tabla del Owner reemitida ×10; preflight de paralelas ×11 (~4,630 palabras); §3 modelo formal copiado V4..V11 sin cambio (2,617 palabras) | Registros previos | Un archivo nuevo por version | Cambios minimos (5 avances de `main`, todos NON-MATERIAL) | useful but reducible |
| C-19 | I-51 | Orden G2 re-transcribe la revision de Arquitecto a su propio autor (~45–70% de la orden segun auditoria, RECONSTRUCTED) | Revision en la misma sesion | Estructura de la orden | Anadio el cierre de G1 de I-50 (AM-4) | useful but reducible |
| C-20 | I-50 | Orden de reconciliacion con ~73% de contenido re-pegado de la revision R1 (RECONSTRUCTED) | Revision en sesion | Estructura de la orden | — | useful but reducible |
| C-21 | I-45 | «HANDOFF AL ORQUESTADOR» (21,261 caracteres) pegado despues de integrar; sus SHAs ya estaban 3 veces en HANDOFF (MEASURED) | HANDOFF integrado | Relevo al orquestador | Arranco una auditoria de 9 agentes; resultado UNKNOWN | unknown |
| C-22 | I-48, I-45 | Salidas del executor pegadas de vuelta como mensajes de usuario (3; 21,373 caracteres) y orden G3 de I-48 duplicada (9,114 caracteres) (MEASURED) | Salida propia | Error de pegado | Ninguna (un turno de verificacion cada una) | ceremonial/redundant |
| C-23 | I-48 | Integridad del blob de V8 verificada casi en cada gate (16 menciones en 2 paginas) | Blob sin commits que lo toquen | Ordenes lo pedian | Confirmacion | useful but reducible |
| C-24 | I-51, I-54, I-52 (activa) | Re-medicion de ramas paralelas en cada gate (I-51 ~16; I-54 bloque «Paralelas» ×8; I-52 ×11) | Medicion previa | Las refs se mueven | Mayoria «sin impacto»; capturas reales: archivo caliente de I-53S, E3-C BLOCKED; deriva de I-54 G7A no detenida | useful but reducible |
| C-25 | Todas las cerradas | Bloques de cierre repetidos: evidencia del Candidato 3–7 copias por iniciativa (I-48: 5; relato del cuelgue: 7; I-50: 6 lugares; I-53: «resultado verificable» ≥6 por unidad) (MEASURED ubicaciones) | Primer registro | Varios documentos llevan estado | Ninguna | ceremonial/redundant |
| C-26 | I-45 | UI Full ×10 antes del commit en G0B | Fix estructural | Corroborar el cierre de la carrera | «0 cuelgues en 10» (el propio cuerpo dice que no prueba) | useful but reducible |
| C-27 | I-48 | Gates G4G.1 y G4G.2 guiadas por hipotesis antes de leer volcados | Logs de CI con la prueba colgada + hangdumps en artefactos | Hipotesis del executor adoptadas como fix | Refutacion de dos hipotesis; volcados locales | useful but reducible |
| C-28 | I-47, I-50, I-51, I-54, I-52, I-55 | Re-descripcion o redescubrimiento de identidad de rack e identidad de vista (lineas aproximadas: I-54 ~200, I-47 ~98, I-50 ~86, I-51 ~72) | ADR-0009/0010 + extensiones | El Discovery exige evidencia de codigo; los detalles no estan en el ADR | Parcial: descubrimientos reales (F-03, L-5, reglas de pertenencia divergentes) | useful but reducible |
| C-29 | I-53S, I-53D | Contratos «minimos» (272 + 287 lineas al bootstrap) que re-enuncian el contrato congelado (18–22% de 8-shingles verbatim; §3, §4, §10–12 mayormente re-enunciado) | Contrato congelado | «Contrato minimo que remite» | Especificos de ejecucion (auditoria de archivo caliente); introdujo el error B-31 | useful but reducible |

---

## 5. Sobrecarga documental medida

### 5.1 Versiones de Proposal (8-shingles de V(n+1) contenidos en V(n); MEASURED)

| Familia | Tamanos (blob) | Contencion consecutiva | Lectura |
|---|---|---|---|
| I-48 | 41.9 → 11.4 KB (8 archivos, 157 KB) | 0.1% – 5.3% | Cada version es un documento **delta** distinto; V8 declara «Todo lo demas de V2..V7 se conserva sin cambio»: el acuerdo congelado abarca 8 archivos |
| I-50 | 32 → 39 → 51 KB | 65.2%, 52.8% | Revision parcial |
| I-53 | 67 → 95 KB | 2.5% | Reescritura |
| I-54 | 47 → 113 → 148 → 162 → 169 KB (639 KB) | 4.1%, 56.6%, 79.3%, 90.4% | Reescritura y luego acumulacion (matrices de trazabilidad, reconciliacion y hallazgos) |
| I-52 (activa, no agregada) | 83 → 521 KB; 11 archivos 3.32 MB / 477,145 palabras | 25% → 83.5% | Acumulacion: ~69% de las palabras de los 11 archivos es texto arrastrado de la version anterior (RECONSTRUCTED por proxy) |

### 5.2 Repeticion entre documentos durables de una misma iniciativa (MEASURED)

- 8-shingles de cada documento contenidos en la union de los demas documentos de su iniciativa: **0–8%** (contrato,
  Discovery, Proposal final, decisiones, ADR). Excepcion: contratos I-53S **21.6%** e I-53D **17.5%**.
- Contenidos en HANDOFF ≤4.4%; en ROADMAP ≤3.3%; en WORKFLOW+AGENTS+AUTOMATION_PLAN ≤0.2%.
- Consecuencia de metodo: **la repeticion durable es parafrasis, no copia**; las metricas verbatim la subestiman. Los racimos
  de parafrasis se enumeraron a mano (C-25; I-45: 11 racimos, p. ej. el argumento del SHA estampado en 8 lugares; I-54:
  contrato de 1,497 lineas con §14 de 43 KB que re-enuncia decisiones y V5 ronda por ronda).

### 5.3 Documentos compartidos (MEASURED)

- HANDOFF (blob): 288,280 bytes antes del merge de I-45 → 412,839 despues de I-53D (**+43%**) a traves de 11 commits de primer
  padre que lo tocaron entre el 2026-09-08 y el 2026-09-13 (10 merges — I-45, I-46, I-47, I-48, I-51, I-50, E1, E2, I-54, E3 —
  y el cierre directo `e8ed2bc` de I-47). Cada merge anade 74–192 lineas y borra 1–7; `e8ed2bc` anadio 96 y borro 18. En la
  practica, **crece por anexion**.
- ROADMAP: +1 fila por iniciativa (filas de 2.5–5.4 KB); ideas-futuras +17..+166 lineas por integracion.
- Ratio documentacion/codigo por merge (insertions): I-53 E1 4,324 vs 7,333; E2 729 vs 1,997; E3 817 vs 2,755; I-50 +3,309
  docs vs +465 produccion y +4,377 pruebas.

### 5.4 Coordinator → Executor y Coordinator ↔ Architect

**Volumen de ordenes (caracteres; MEASURED salvo «≈», estimacion por lineas)**

| Unidad | Cobertura | Mensajes de usuario u ordenes | Caracteres | Solicitudes de Architect | Salidas de Architect |
|---|---|---|---|---|---|
| I-51 | 100% | 9 ordenes + 1 pregunta + 2 notificaciones automaticas | ≈59.4k (57,426 medidos; otra auditoria midio 56,650) | 12,644 (21%) | 32,749 |
| I-48 | 100% | 44 mensajes de usuario (incluye 3 ecos, 1 duplicado y 1 pregunta) | ≈385k (296,776 medidos) | 100,314 | 93,255 |
| I-45 | 100% | 27 ordenes | 276,060 | 57,782 | 75,140 |
| I-50 | 100% | 15 ordenes | ≈98k (58.3k medidos) | 11,520 | 21,422 |
| I-53 (4 unidades) | ~73% | 21 ordenes | ≈227k (191.4k medidos) | 26,270 | 39,648 |
| I-54 | ~72% | 23 ordenes (+2 re-pegadas) | 262,801 (284,403 con re-pegados) | 55,457 (ordenes G2B–G2J) | 85,204 |
| I-52 (activa, no agregada) | ultimos ~1300 mensajes | 4 ordenes | 55,448 | 26,355 | 33,986 |

**Composicion (RECONSTRUCTED por lectura o clasificador de palabras clave).** Ordenes de implementacion: 60–85% contenido
especifico de la iniciativa, 9–20% politica general re-enunciada. Ordenes de Candidato: ~55% politica general. Ordenes de
integracion: 40–55% politica de Owner/integracion (copian WORKFLOW 4.5.4–4.5.7 casi literal, incluida la referencia obsoleta a
«§8–12»).

**Referencias vs copias (MEASURED).** 5-gramas de las ordenes de I-53 presentes en WORKFLOW+AGENTS: 0–2%; en Proposal V2,
contrato, ADR-0037 o decisiones: 0–5%. Las reglas se re-enuncian **en parafrasis**. La copia literal se concentra en el
andamiaje: guardia docs-only, lista de jobs post-merge, dispatch de cobertura, limpieza, bloques de estado de consenso y
listas «AGREE … NO reabrir». I-52 usa plantillas (bloques identicos de 226 y 348 caracteres; lineas separadoras 18–24% de
cada orden). I-45 recibio el cuerpo de la skill `/workflow-authoring` (16,886 caracteres) 3 veces como mensaje de usuario.

**Bloques repetidos entre ordenes.** Conteos del corpus de 65 ordenes de I-45, I-48 e I-51 (MEASURED); los ejemplos de I-50,
I-53 e I-54 citados en la tabla provienen de la auditoria de ordenes de esas sesiones.

| Bloque | Ordenes | ¿En documentos vinculantes? | Clasificacion al retirarlo (INFERENCE sobre evidencia) |
|---|---|---|---|
| Preflight (fetch, arbol limpio, stash, operaciones Git, AutoCAD cerrado) | 30 | fetch y AutoCAD si; **stash y operaciones incompletas no** (grep 0) | Aclaratorio; sin incidente por item ausente |
| Re-fetch en momentos definidos (antes de la primera edicion de archivo caliente, antes de declarar Candidato, antes de commit/merge) | varios en I-53/I-54 | parcial (rebase al abrir y rebase final) | **Critico para seguridad**: capturo E3-C BLOCKED; su ausencia coincidio con I-54 G7A («main se movio y no me detuve») |
| «Si `origin/main` avanzo: DETENTE» | 17 | **no**; WORKFLOW prescribe rebase | Aclaratorio; en I-50 G8 una variante («NO rebasear») **contradijo** WORKFLOW:160 y AGENTS:244-248 |
| Regimen de Candidato (Core/UI Full, builds, `event=push`, `head_sha`) | 22–26 | si (AGENTS desde I-45) | Redundante para I-48/I-51; E2-C aplico la regla antes de que una orden la re-enunciara |
| Guardia docs-only del cierre | 20 | si (WORKFLOW 4.5.4, mismas rutas) | Redundante; la cifra esperada de una orden de I-54 (4 archivos) fue menos exacta que el doc (6) |
| Jobs post-merge + artefacto de cobertura | 18 | si (WORKFLOW 4.5.6, casi literal) | Redundante por texto |
| Dispatch de cobertura del Candidato + `measured-sha` | 9 | si (WORKFLOW 4.5.7) | Redundante por texto, **critico por historia**: la orden E1-I lo re-enuncio mal (B-32); el daño vino de la orden que contradijo el doc |
| «Nunca commit directo sobre `main`; corregir en la rama» | presente en I-51 G8, I-45 integracion, G7-C2 | si (CLAUDE.md:25, AUTOMATION_PLAN:102, WORKFLOW 4.5.6) | **Critico para seguridad**: con la orden G7-C1 silenciosa, el executor autoro texto normativo que autorizaba commits sobre `main` (B-27) |
| Celda de estado ROADMAP «integrada (fecha)», sin «en curso» | 5 | si (WORKFLOW §2, §8) | Aclaratorio: el executor se desvio 2 veces cuando la orden no lo re-enuncio (I-48 G0; I-45 bootstrap) |
| Plantillas de salida («Devuelve:», «Reportar:») | 48 de 65 | no | Aclaratorio (uniformidad de reportes) |
| Cercas de alcance por gate («NO implementes codigo», «DETENTE») | 16 / 35 | no | Aclaratorio; los executors siempre se detuvieron en el limite |
| «HANDOFF §8–12» | ≥6 | presente pero **incorrecto** en WORKFLOW y AGENTS | Redundante y erroneo en ambos lugares |

**Incidentes de ordenes (MEASURED; clasificacion INFERENCE).**

| # | Unidad / gate | Hecho | Retrabajo |
|---|---|---|---|
| O-1 | I-53 E1-I | Orden de cobertura contra WORKFLOW 4.5.7; limpieza anticipada | E1-I.1 (2,120 caracteres) + dispatch; clausula «No repetir la desviacion de E1» en E2-I |
| O-2 | I-50 G7/G8 | «NO rebasear» contra WORKFLOW:160/AGENTS:244-248 | Excepcion registrada en decisions/I-50; citada luego como precedente en E3-C |
| O-3 | I-53D E3-C | La orden decia a la vez «rebasar antes de declarar» y «NO tocar docs»; el rebase conflictuo solo en ROADMAP | E3-C BLOCKED + E3-C.1 con re-validacion completa |
| O-4 | I-54 G7A | Commit sobre base superada 4.5 min despues de un merge en `main` | Tag de archivo, rebase de 20 commits, CI nuevo, 1,257 caracteres de clasificacion |
| O-5 | I-50 (4 ordenes) | Se pidio un «tier I-45» que I-45 no define | El executor invento un tier (+2,314 pruebas) |
| O-6 | I-48 G4E/G4F | Ordenes «NO hagas candidate/CI final», sin chequeo de CI por gate | CI rojo sin advertir; 6 ordenes de retrabajo (44,401 caracteres) |
| O-7 | I-48 cuelgue | Dos hipotesis del executor convertidas en gates de fix antes de leer volcados | G4G.1 y G4G.2 refutadas por DIAG |
| O-8 | I-45 G7-C1 | Orden sin indicar donde corregir un CI post-merge rojo | Texto normativo contradictorio (B-27), G7-C2 |
| O-9 | I-48 U0 | Discovery pedido antes de cualquier reclamo | El executor se detuvo (WORKFLOW caso d); contraste: en I-45 hubo mediciones antes del reclamo (RECONSTRUCTED) |
| O-10 | Varios | Ejecutores no leyeron `measured-sha.txt` exigido por WORKFLOW:256 (la descarga requiere permiso); lo verificaron por log | Ninguno; hueco entre texto normativo y permisos operativos |

---

## 6. Fundaciones y contratos candidatos a registro

Conteos sobre 9 lineas auditadas (I-45, I-47, I-48, I-50, I-51, I-53, I-54, I-52, I-55); **cotas inferiores** (versiones
intermedias de Proposals no leidas completas); I-52 e I-55 activas, contadas solo como re-descripcion. Clases: fuentes
MEASURED; conteos RECONSTRUCTED; juicio de registro INFERENCE.

| Fundacion | Fuente durable actual | Re-descrita / redescubierta en | ¿Extendida o consumida? | ¿Un registro conciso eliminaria plausiblemente el redescubrimiento? | Riesgo de quedar obsoleto |
|---|---|---|---|---|---|
| Rack Identity | ADR-0009; ARCHITECTURE §4.1; persistence pack (1 viñeta); glosario | 6: I-47, I-50, I-51, I-54, I-52, I-55 | Extendida por I-47 (ADR-0034 §9/§12), I-51 (NI-1..6, sin ADR), I-54 (ADR-0039 §4/§8); I-52 e I-55 proponen extensiones | Parcial: el ADR era correcto en lo esencial; lo re-derivado (filtros de pertenencia por consumidor, herencia en Compose, restamp por kind) no esta en el ADR; el Discovery exige evidencia de codigo de todos modos | ALTO: 5 extensiones en ~7 semanas; ARCHITECTURE y glosario ya omiten `CustomProperties`/`ExtensionData`; el texto de ADR-0009/0010 dice «propuesto» dentro de un registro aceptado |
| View Identity (Actualizar/Insertar, View/Section) | ADR-0010; ARCHITECTURE; guia §5.3 (solo 3 sistemas) | 6: I-47, I-50, I-51, I-54, I-52, I-55; la tabla View×Section se re-derivo 3 veces | Extendida por I-50 (ADR-0035), I-54 (ADR-0039); I-55 propone enmendarla (ADR-0042 propuesto) | Plausible para la tabla de codificacion, que ningun documento durable contiene | ALTO: en enmienda activa; el proceso de ADR no tiene estado de «enmienda parcial» |
| Authored vs Effective | ADR-0034 §5–6 (sentido de vinculo); «efectivo» con otros sentidos en ADR-0030/0031/0035/0037 | 4 (+1 parcial): I-48, I-51, I-52, I-55 (+I-50) | Extendida por I-48 (sin ADR); I-50 introdujo otro concepto (`EffectiveDetail`) | Plausible para desambiguar el termino | MEDIO-ALTO |
| Project Variables | ADR-0034; contrato y decisiones de I-47 | 5: I-48 (mapa de cableado, tarea asignada), I-51, I-54, I-52, I-55 | Extendida por I-48 (sin ADR); I-49 (activa) propone reemplazos de ADR en su rama (ADR-0038→0040→0041) | Baja: lo re-descrito es cableado de codigo y forma del store; ya hay desacuerdo doc/codigo (F-03) | MEDIO |
| Linked Properties | Solo contrato/V8/decisiones de I-48; sin ADR, sin ARCHITECTURE ni packs | 1 (ligera): I-55 | Consumida por simbolos de codigo (I-50, I-51, I-54, I-52) | Casi no hubo redescubrimiento que eliminar | BAJO-MEDIO (I-49, activa, propone extenderla) |
| Custom Properties | ADR-0039; contrato/decisiones de I-54 | 2: I-52, I-55 | I-55 propone cambiar la semantica de hermanas | No: tenia 1 dia y ya se propone cambiarla; ADR-0039 cita ADR-0038, ya reemplazado dos veces en la rama de I-49 | ALTO |
| RACKDUPLICAR / Restamp identity | 5 fragmentos (ADR-0009, ARCHITECTURE, ADR-0034 §12, contrato I-51, decisions/I-51); la extension de I-51 no tiene ADR | 5: I-47, I-50, I-51, I-54, I-55 | Extendida por I-47 e I-51; consumida por I-52 e I-54 | Plausible: fuente fragmentada; una decision vivia solo en ideas-futuras y la guia de despliegue | MEDIO |
| DimensionViews | ADR-0035 (acotado por ADR-0039); contrato I-50 | 2: I-52 (ligera), I-55 | Acotada por I-54; consumida por I-54 e I-52 | Baja necesidad | MEDIO (leerla bien exige dos ADR) |
| Header Mutation / Reconciliation | Contratos I-35/I-40/I-17 (sin ADR); ADR-0037 §10; prosa en HANDOFF | 1 (+1 ligera): I-53 (reconstruyo el contrato de I-40 en ~134 lineas y hallo 5 discrepancias doc↔codigo) | Extendida por I-53 (ADR-0037, `DynamicRackRebuild`); segundo mecanismo paralelo (Push Back conserva el de I-40 por diseño) | Plausible, pero la prosa existente ya era erronea: un registro construido de ella la habria propagado | ALTO: 4 cambios en ~7 semanas |
| *Adicional*: inventario de sistemas/kinds y codificacion View/Section por sistema | ARCHITECTURE §3 (4 sistemas, no menciona Push Back); guia §5.3 (3 sistemas) | 5: I-50, I-53, I-54, I-52, I-55 | Crece con cada kind nuevo | Plausible: la fuente durable esta incompleta | MEDIO |
| *Adicional*: preservacion de campos desconocidos del sobre (I-11) | Solo contrato de I-11 | 4: I-50, I-54, I-52, I-55 | Extendida por ADR-0039 §6 (D-21) | Plausible | MEDIO |
| *Adicional*: vocabulario de clases de evidencia de los Discovery | — | re-inventado 6 veces (I-47, I-50, I-53, I-54, I-52, I-55) | — | n/a | — |

**Context packs (MEASURED).** No funcionan como registro: 8 de 10 cambiaron por ultima vez el 2026-07-17 y 2 el 2026-07-27 (antes
de I-43..I-55); su contenido de fundaciones son viñetas sueltas; `ui-editors.md:27-28` todavia llama «arquitectura objetivo» al
Editor Shell integrado por I-30 el 2026-07-24; se declaran en todos los contratos pero su contenido se cita en solo 2 lugares.

---

## 7. Respuestas a Q1–Q14

### Q1. ¿Donde es mayor la sobrecarga fija por iniciativa?

- **Bucle de diseño cuando hay varias rondas** (MEASURED): I-54 639 KB de Proposals + contrato de 108 KB + decisiones de 83 KB;
  I-48 157 KB de Proposals y ~193k caracteres de solicitudes y salidas de Architect para 8 rondas. Por contraste, I-51
  convergio con 1 Discovery, 1 revision y 0 archivos de Proposal (≈59k caracteres de ordenes en total). Control activo (no
  agregado): I-52 lleva 3.32 MB de Proposals, ADR de 80 KB y decisiones de 197 KB sin una linea de producto tras 11 revisiones.
- **Ceremonia por unidad Git** (MEASURED): reclamo + bootstrap + cierre + merge + CI post-merge + dispatch. I-53 la pago tres
  veces (3 reclamos, 3 bootstraps, 3 cierres, 3 merges, 7 corridas en `main`, 6 ediciones de ROADMAP, ~490 lineas de HANDOFF).
- **CI sobre SHAs sin producto** (MEASURED, tiempo de maquina, no humano): I-48 57% de minutos CI de rama; I-54 ~71%; I-53
  13/19 corridas; I-51 20.1 de 36.3 min.
- **Documentacion de cierre** (MEASURED): HANDOFF +43% en 5 dias, por anexion; evidencia del Candidato copiada 3–7 veces por
  iniciativa; estados «PENDING» que no se actualizan tras el merge (I-48, I-51).
- **Ordenes** (MEASURED/≈): I-48 ≈385k caracteres; I-45 276k; I-54 263k (72% cubierto); I-53 ≈227k (73%); I-50 ≈98k; I-51 ≈59k.
- **UNKNOWN**: el tiempo activo humano y de agente de cada una de estas partidas.
- INFERENCE: la mayor sobrecarga variable esta en el numero de rondas de diseño; la mayor sobrecarga fija esta en la ceremonia
  por unidad Git y en la documentacion de cierre.

### Q2. ¿Que etapas encontraron defectos materiales?

| Etapa | Capturas materiales en la muestra (TABLE B) |
|---|---|
| Discovery (lectura de codigo) | I-51 3 HIGH (B-01..03); I-54 `Compose` (B-04); I-53 L-1 y L-7 (B-05); I-45: «0 advertencias» falso en HANDOFF, regla fantasma de filtro vacio, afirmacion «<2 s» y 3 rutas erroneas de archivos calientes |
| Revision del Coordinador | I-48 G1.1 (B-06) y 4 defectos materiales en 3 gates de correccion entre gates internos (B-10, B-40 ×2, B-11); I-50 2 MATERIAL en V1 (B-14); I-45 cadencia de cobertura (B-28) |
| Architect-role, 1a ronda | todas las unidades cerradas con ronda: I-48, I-50, I-51, I-53, I-54, I-45 |
| Architect-role, rondas posteriores | I-48 AR2–AR7; I-54 G2D (material) y G2F (correccion menor); I-45 CR1/CR2 (HIGH, 3 de 5 creados por gates de la propia I-45). Control activo (no agregado): I-52 con HIGH solo hasta V4 |
| RED focal durante implementacion | I-50 T-08 compuesto, MIN-4 confirmado (B-16, B-18); I-51 guardas literales debiles (B-41); I-54 defectos de prueba |
| CI push (runner) | I-50 CRLF en el job Core de Ubuntu (B-17); I-48 cuelgue en el job de UI del runner Windows (B-12); I-45 copia de blame |
| Owner Validation en AutoCAD | **0 hallazgos registrados** en 7 rondas; en el corpus de I-45 (I-40/I-43/I-44) el Owner encontro 8 de 17 |
| Full local antes del Candidato | 0 defectos inesperados registrados en la muestra |
| Iniciativas posteriores | I-51 INV-09 sin prueba (hallado por I-54, B-30); comentario de `RackBlockData` (5 registros, B-35) |

La ausencia de hallazgos en una etapa **no prueba** que carezca de valor: la validacion del Owner es el unico ejercicio sobre
bloques DWG reales (AGENTS punto 5) y su valor contrafactual no es medible con esta muestra.

### Q3. ¿Que etapas mayormente re-confirman estado conocido?

- CI sobre SHAs docs-only/vacios: 0 fallos en la muestra (los 4 fallos de CI de rama ocurrieron en SHAs que tocan rutas
  ejecutadas por CI) (MEASURED).
- CI post-merge con arbol igual al del cierre: 0 fallos (MEASURED).
- Ultima ronda de Architect de cada iniciativa cerrada con 2+ rondas: AGREED sin materiales abiertos (I-45 R4/CR3, I-48 AR8,
  I-50 R2, I-54 G2J; I-53 R2 con un MEDIUM, RR-01) (RECONSTRUCTED).
- Dispatches de cobertura duplicados (C-06, C-07) y Full locales repetidos sobre el mismo arbol (C-08..C-11) (MEASURED/
  RECONSTRUCTED).
- Owner Validation en esta muestra: 7 PASS a la primera; E1-V sobre codigo no invocable (C-13).
- Re-medicion de ramas paralelas: mayormente «sin impacto», con capturas reales puntuales (C-24).

### Q4. ¿Con que frecuencia la revision de Arquitecto cambia materialmente un diseño?

- Rondas con ≥1 hallazgo material aceptado, iniciativas cerradas (RECONSTRUCTED): I-45 5 de 7; I-48 7 de 8; I-50 1 de 2;
  I-51 1 de 1; I-53 1 de 2 (2 si se cuenta RR-01, MEDIUM, de la 2a); I-54 2 de 5 (3 si se cuenta G2F). **Total 17–19 de 25
  rondas** segun el umbral de materialidad.
- Toda primera ronda cambio el diseño. En las iniciativas con 2 o mas rondas, la ultima termino AGREED sin hallazgos
  materiales abiertos (I-53 R2 añadio RR-01, MEDIUM, incorporado al congelar).
- I-52 (activa, no agregada): 11 revisiones completas; la de G1 dio 18 hallazgos y V1..V10 (10 revisiones) dieron 127, de los
  que solo 12 se clasifican como aspecto nuevo del problema (INFERENCE).
- Condicion de lectura: todas son revisiones de rol dentro de la sesion executor (0.4); si una revision independiente habria
  cambiado lo mismo es UNKNOWN.

### Q5. Cuando hay varias rondas de Arquitecto, ¿que las causa?

| Causa | Evidencia |
|---|---|
| Hallazgos genuinamente nuevos | Primeras rondas de todas; I-48 AR2 (sobre una superficie que el Discovery marco fuera de alcance); I-52 (activa) 12 de 127 |
| Proposal previa incompleta | I-48 AR1; I-54 G2B (5 MATERIAL; V2 casi reescrita: 4.1% de contencion, metrica 5.1); I-50 V1.1 (MAT-A1) |
| Consecuencias de la reconciliacion anterior o de un contrato demasiado detallado | I-48 AR4–AR7 (precondiciones y composicion de construcciones recien agregadas); I-54 G2D (V2 introdujo V2-02) y G2H (redaccion introducida por C-F2); I-45 CR1 y CR2 (3 HIGH creados por G6-C1 y G7-C1); I-52 (activa) cadenas de simetria (reabierta en 8 revisiones) y de huella en Model Space |
| Deriva de `main` en paralelo | I-52 (activa) 13 hallazgos; I-54 anadio integridad de rebase a G2H sin hallazgos; **no** en I-48 (`main` no se movio) |
| Decision tardia del Owner | **0 casos** observados (I-48, I-54, I-52) |
| Revision de detalles mecanicos | I-54 G2H/G2J; I-50 R2 editorial; I-52 (activa) 43 de detalle + 17 editoriales |
| Otras | Autor y revisor en la misma sesion releen lo recien escrito (I-53 22+17 llamadas; I-50 ~43+25; I-52 33+35); en I-52 el Coordinador escalo un LOW a regla vinculante y siguieron 3 rondas MED (AR7-07); en I-52 MED latentes de costo 0 generaron versiones completas (V7, V8); en 8 de 8 casos de I-52 el «AGREED WITH Vn» del Coordinador fue seguido de CHANGES REQUIRED |

### Q6. ¿Los gates de implementacion son unidades funcionales o micro-gates de estructura?

| Unidad | Commits de implementacion | Verificables por usuario | Primer verificable |
|---|---|---|---|
| I-48 | 10 (+5 de diagnostico sin commit) | 4 | G4B.1 |
| I-50 | 11 | 3 | commit 17 de 20 |
| I-51 | 3 | 1 | G5 (ultimo) |
| I-53 E1 | 3 | 0 | — |
| I-53S / I-53D | 1 / 1 | 1 / 1 | unico gate |
| I-54 | 6 (+6 docs `-CLOSE`) | 1 | G7 (ultimo) |

(MEASURED conteos; clasificacion INFERENCE.) Patron dominante: gates internos por capa o por sistema y un unico gate
verificable al final, validado una sola vez por el Owner. Excepcion: I-53S e I-53D, un gate funcional cada una, sobre un
contrato ya congelado y sin rondas de diseño propias.

### Q7. ¿Con que frecuencia se corren Full Core/UI antes del Candidato final, y por que?

- Core Full local registrado antes del Candidato final (RECONSTRUCTED): I-54 ~13 de ~14; I-48 ≥10 (34 invocaciones en
  transcripcion); I-50 6 de 7 (13 sobre 12 SHAs en transcripcion); I-51 3 de 4; E1 3 de 4; E2 1 de 2; E3 2 de 3. I-45 queda
  aparte: sus corridas previas al Candidato fueron de medicion (Discovery, G1, G5) y 1 de corroboracion en G0B, no de validacion.
- UI Full local antes del Candidato: I-51 0 y E1 0 (LC-UI aplicado); I-50 3; I-54 3; E2 1; E3 2; I-48 ~9 (incluye el
  diagnostico del cuelgue).
- Motivos registrados: el estampado de SHA invalida corridas previas al commit (E2-C); habito por commit; ordenes que exigian
  Full por gate o «tiers» inexistentes (I-50); diagnostico (I-48); invalidacion por rebase (E3-C.1); verificar antes de push.
- Capturas: estas corridas no registraron defectos inesperados; los problemas de plataforma y de runner los encontro el CI
  push, no el Full local (B-12, B-17). Esto no mide su valor como red de regresion (UNKNOWN contrafactual).

### Q8. ¿Con que frecuencia la evidencia del Candidato queda invalidada por cambios posteriores?

- SHAs de Candidato comprometidos en iniciativas cerradas: 10 en 8 unidades; invalidados: 2 (I-48 `acecde6` por CI rojo;
  I-53D `d280197` por avance de `main`) (MEASURED/RECONSTRUCTED).
- Evidencia **intermedia** invalidada por rebase: I-54 (3 rebases, 21 corridas huerfanas, re-validacion en G4); I-53 E1
  (reescritura docs-only); I-45 (cierre reescrito) (MEASURED).
- I-50 conservo la evidencia del Candidato mediante excepcion (merge sin rebase) + guardia patch-id no registrada en el repo.

### Q9. ¿Que retrabajo viene del requisito exact-SHA y cual de declarar el Candidato demasiado pronto?

| Origen | Casos |
|---|---|
| Regla exact-SHA sin cambio de producto | E2-C repeticion por sello del padre; I-54 G8 sobre un Candidato docs-only; duplicados antes/despues del commit (I-54 ×4); CI post-merge sobre merges con arbol igual al del cierre (I-45, I-48, I-51, I-54, E1, E2, E3); CI del cierre docs-only; corrida huerfana del cierre reescrito de I-45; dispatch que repite ambas suites para obtener cobertura |
| Avance de `main` (deriva) | E3-C.1 (arbol distinto: +57 archivos); I-54 G4 y G7A |
| Candidato declarado sin comprobar la salud del CI | I-48 `acecde6`: CI rojo desde G4E → 5 gates de diagnostico |
| Candidato declarado antes de completar la funcionalidad | **No observado** en la muestra (resultado nulo registrado) |

### Q10. ¿Cuanta documentacion se repite entre contrato, Discovery, Proposal, paquetes de Architect, decisiones, HANDOFF, ROADMAP y ordenes?

- Verbatim entre documentos durables: bajo (0–8%, 5.2); entre versiones de Proposal: alto cuando se acumula (I-54 90.4%;
  I-52, activa, 83.5%) y casi nulo cuando cada version es delta (I-48, cuyo acuerdo final abarca 8 archivos) (MEASURED).
- Parafrasis: racimos de 3–11 re-enunciados por iniciativa (C-25; I-45 11 racimos; I-54 §14 del contrato 43 KB) (RECONSTRUCTED).
- Ordenes: re-enuncian reglas en parafrasis (0–2% de 5-gramas de WORKFLOW+AGENTS) y re-pegan revisiones a su propio autor
  (I-51 ~45–70% segun auditoria; I-50 ~73%) (MEASURED/RECONSTRUCTED).
- Copias obsoletas generadas por la repeticion: «HANDOFF §8–12» (B-36); estados PENDING (I-48, I-51); «confirmacion» en I-53D
  (B-31).
- Tension normativa (MEASURED): AGENTS/WORKFLOW §8 reservan hashes y conteos a HANDOFF §12. Contienen hashes: contratos
  recientes (I-51, I-53D, I-55) y las filas de ROADMAP de I-13, I-23, I-32..I-35, I-37D, I-40..I-44 e I-47; las filas de I-45 e
  I-48..I-54 no. Este documento tambien los contiene (encabezado).

### Q11. ¿Que fundaciones o contratos se redescubrieron repetidamente despues de aceptados?

Rack Identity y View Identity (6 de 9 lineas cada una), RACKDUPLICAR/restamp (5), Project Variables (5), inventario de sistemas
y codificacion View/Section (5), Authored vs Effective (4 + 1), preservacion del sobre (4). El mismo defecto de comentario se
registro 5 veces (B-35) y el vocabulario de evidencia se re-invento 6 veces. Detalle en la seccion 6 (RECONSTRUCTED).

### Q12. ¿Que conflictos causaron las iniciativas paralelas al modificar documentacion compartida?

- Ventanas concurrentes que tocaron los mismos documentos compartidos (MEASURED): I-51×I-50 (HANDOFF, ROADMAP, guia de
  validacion, ideas-futuras); I-50×I-53 E1, I-50×I-54, I-53 E1×I-54 (HANDOFF, ROADMAP, indice ADR, ideas-futuras); I-53S×I-54 e
  I-54×I-53D (HANDOFF, ROADMAP, ideas-futuras); I-52 con I-50, I-53 E1, I-53S, I-54 e I-53D (ROADMAP; con I-50, I-53 E1 e I-54
  tambien el indice ADR).
- Conflictos registrados o reproducidos, **todos documentales** (0 de producto):
  - I-50 merge con I-51: HANDOFF 5 hunks, ROADMAP 1, ideas-futuras 1, guia 1 (MEASURED por simulacion).
  - I-53 E1 rebase G2-F sobre I-50: `adr/README.md` 1 (RECONSTRUCTED).
  - I-54 rebase G4: ROADMAP, `adr/README.md` ×2, ideas-futuras; rebase G7: ROADMAP, ideas-futuras (RECONSTRUCTED; range-diff
    MEASURED).
  - I-53D rebase E3-C.1: ROADMAP 1 (RECONSTRUCTED).
- Los merges de E1, E2, E3, I-54, I-51 e I-48 no pudieron conflictuar: su base era el primer padre (MEASURED).
- HANDOFF no conflictuo en la linea I-53 porque solo se edita en el cierre, despues del rebase (MEASURED).
- Residuo: marcador de conflicto commiteado en ideas-futuras y aun presente (B-37).

### Q13. ¿Agrupar varios IDs funcionales en una iniciativa conceptual ayudo o perjudico?

- **Ayudo (I-53)**: un Discovery, dos Proposals, un ADR y un registro para 2 IDs en 2 sistemas; E2 y E3 sin rondas de diseño y
  sin desviaciones de diseño hacia atras; nucleo compartido sin cambios desde G3 hasta el final; unidades de UI rapidas en reloj
  (E2 5h16, E3 4h25 entre reclamo y merge; E1 17h17); validacion por unidad visible; el rebase de E3 no re-valido E2
  (MEASURED/RECONSTRUCTED).
- **Perjudico (I-53)**: ceremonia triplicada (Q1); E1 sin salida verificable pero con Candidato completo y smoke en AutoCAD; fila
  ROADMAP «integrada» 11 h antes de ID6/ID7 visibles; codigo dormido 6h11 (Selectivo) y 11h00 (Dinamico); la particion tomo 5
  formas (~100 lineas de V2); desviaciones de proceso concentradas en E1; error de reformulacion en contratos minimos
  (MEASURED/RECONSTRUCTED).
- Otros ejemplos: I-50 (1 ID en 3 sistemas, gates por sistema, 1 validacion final); I-55 agrupa ID17+ID18+ID19 (activa,
  resultado UNKNOWN).

### Q14. Evidencia a favor y en contra de cada idea

| Idea | A favor (evidencia) | En contra o limites (evidencia) |
|---|---|---|
| Discovery focalizado | I-51 convergio con 1 Discovery + 1 revision; E2/E3 reutilizaron el contrato congelado sin incidentes; 20% del Discovery de I-54 fue re-medicion de paralelas; fundaciones re-descritas 4–6 veces | Los Discovery amplios encontraron defectos HIGH (I-51 ×3, I-53 L-1, I-54 `Compose`) y discrepancias doc↔codigo (F-03, L-5); I-48 G1 no vio la consecuencia de cardinalidad (la hallo el Coordinador) |
| Participacion proporcional del Arquitecto | Las ultimas rondas terminaron AGREED sin materiales abiertos; causas «derivadas» y «mecanicas» frecuentes (Q5); I-51 (1 ronda) y E2/E3 (0) sin defectos de producto registrados despues (I-51: un hueco de prueba, B-30) | I-48 AR4–AR7 aun produjeron MATERIAL convertidos en codigo (p. ej. perdida de datos por `RemoveAll`); I-45 CR1 hallo 4 HIGH despues de «NONE» del Coordinador; 2 BLOCKER en I-48 |
| Gates dimensionados por funcion | Gates internos no verificables por usuario (I-50 primero verificable en el commit 17 de 20; I-54 solo G7); el Owner valido una sola vez al final; E2/E3 funcionaron con un gate | RED por gate capturo defectos (I-50 T-08, I-51 guardas); la revision del Coordinador entre gates internos de I-48 encontro 4 materiales; gates que terminaban en push sin mirar CI dejaron pasar el cuelgue |
| Full suites principalmente en el Candidato final | Full local previo no registro defectos inesperados; duplicados por estampado; LC-UI aplicado sin incidentes (I-51, E1); I-45 ya midio 0 capturas del Full en su corpus | El CI push encontro problemas reales en gates intermedios (CRLF, cuelgue); el valor de regresion del Full no es medible (I-45 advierte que el oraculo ciego «no prueba que la suite completa carezca de valor»); AGENTS excluye expresamente el Core de la logica de LC-UI |
| Documentacion de cierre concentrada | Bloques de cierre copiados 3–7 veces; HANDOFF crece por anexion; estados PENDING obsoletos; commits `-CLOSE` de I-54 mayormente re-enuncian tablas | Los `-CLOSE` tambien registraron precisiones del Coordinador y una decision del Owner; el registro de decisiones de I-53 sirvio al rebase de E3; HANDOFF no conflictuo gracias a editarse solo al cierre |
| Ordenes por referencia en lugar de repeticion | Bloques con texto casi literal en WORKFLOW sin incidente al omitirse; E2-C aplico exact-SHA sin re-enunciado; I-51/I-50 re-pegaron revisiones a su autor; una orden que re-enuncio mal una regla causo O-1 | Ciertos bloques no estan en los documentos (stash, operaciones incompletas, re-fetch en momentos definidos, cercas por gate); cuando no se re-enuncio, hubo desviaciones (celda ROADMAP ×2, I-54 G7A, I-48 sin chequeo de CI); con la orden silenciosa, I-45 autoro texto que autorizaba commits sobre `main` |
| Registro de fundaciones | Re-descripciones 4–6 veces; defecto de comentario registrado 5 veces; context packs obsoletos y sin uso como registro; decisiones de I-51 solo en contrato e ideas | Contratos que cambian rapido (Rack Identity 5 extensiones en ~7 semanas; Custom Properties con cambio propuesto al dia siguiente); la re-verificacion contra codigo encontro errores en la prosa existente, que un registro habria propagado; el metodo de Discovery exige evidencia de codigo igualmente |

---

## 8. TABLE D — Workflow Stage Value Map

> **ANALISIS, no politica de Workflow V2.** Las columnas «¿siempre?», «¿condicional?» y «¿una vez?» describen lo que la
> evidencia de esta muestra sostiene o no sostiene; no autorizan ni retiran ninguna etapa.

| Stage | Risk/defect it protects against | Historical captures (muestra) | Must always run? (lectura de evidencia) | Candidate for conditional execution? | Can run once? | Human decision or automatable check? | Confidence |
|---|---|---|---|---|---|---|---|
| Reclamo atomico + bootstrap | Colision de ramas; iniciativa sin registro | 0 colisiones; bootstraps corregidos 2 veces | Evidencia de necesidad por unidad Git | No observado | Una vez por unidad Git (I-53 pago 3) | Automatizable salvo decision de autorizacion | MEDIA |
| Discovery con evidencia de codigo | Diseño sobre supuestos falsos; defectos preexistentes | I-51 3 HIGH; I-54 `Compose`; I-53 L-1; I-45 afirmaciones falsas de HANDOFF y rutas | Valor alto en fundaciones nuevas | Evidencia a favor: E2/E3 sin Discovery sin incidentes | Parcial: fundaciones re-descritas repetidamente | Agente + humano (alcance) | MEDIA |
| Revision del Coordinador (Discovery/Proposal) | Desalineacion de producto y alcance | I-48 G1.1; I-50 V1 2 MATERIAL; I-45 cadencia | Evidencia de valor | — | — | Humano | MEDIA |
| Proposal versionada | Acuerdo explicito antes de codigo | Base de todas las capturas de diseño | — | Evidencia a favor: I-51 sin archivo de Proposal | Evidencia de sobrecarga en versiones acumulativas | Humano/agente | MEDIA |
| Architect-role, 1a ronda | Defectos de diseño | Todas las primeras rondas | Evidencia de valor | — | — | Agente (rol en sesion); independencia UNKNOWN | MEDIA |
| Architect-role, rondas ≥2 | Seguimiento de reconciliaciones | Mixtas (Q4/Q5) | Evidencia mixta | Evidencia mixta | — | Agente | BAJA (contrafactual) |
| Congelacion + decision de politica del Owner (ADR) | Autoridad y alcance | OD-6/OD-8 cambiaron alcance; ADR-0035/0037/0039 aceptados | Cuando hay ADR o decision (regla vigente) | — | Una vez por diseño | Humano (Owner) | ALTA |
| Gates con RED→GREEN focal | Pruebas que no fallan; regresion local | I-50 T-08 y MIN-4; I-51 guardas; I-54 defectos de prueba | Evidencia de valor | — | — | Automatizable (ejecucion) + humano (diseño de prueba) | MEDIA |
| Revision del Coordinador entre gates | Defectos antes del siguiente gate | I-48: 4 MATERIAL en 3 gates de correccion | Evidencia de valor en I-48 | — | — | Humano | MEDIA |
| Commits `-CLOSE` por gate | Trazabilidad | Nuevas precisiones y decision del Owner (I-54); resto re-enunciado | — | Evidencia mixta | Evidencia a favor de concentrar | Parcialmente automatizable | MEDIA |
| Core Full local por gate | Regresion | 0 inesperados registrados | No sostenido por capturas; AGENTS lo mantiene fuera de LC-UI | Evidencia no concluyente | — | Automatizable | MEDIA (ausencia ≠ sin valor) |
| UI Full local por gate | Regresion UI | 0; Full local afirmado verde mientras el CI colgaba | No sostenido; LC-UI ya lo retira de la iteracion ordinaria (vigente) | Ya condicional por LC-UI | — | Automatizable | MEDIA |
| CI push sobre SHAs que tocan rutas de CI | Diferencias de plataforma (Linux: CRLF) y de entorno del runner (cuelgue de UI en Windows), determinismo | CRLF (I-50), cuelgue (I-48), blame (I-45) | Evidencia de valor | — | — | Automatizable (leer el resultado exige accion) | ALTA |
| CI push sobre SHAs docs-only/vacios | Documentos que rompen pruebas (medido en G0 de I-56: la unica prueba que lee `docs/` es `CantileverSourceGuardTests`) | 0 fallos | No sostenido por capturas | **Colisiona** con enfoques rechazados por ADR-0033 (clase R0 «docs/nonfunctional» y clave de estado por contenido) | — | Automatizable | ALTA (conteos) |
| Re-fetch/preflight de ramas paralelas | Base superada; archivo caliente | E3-C BLOCKED (captura); I-54 G7A (fallo al no detener) | En momentos puntuales | Evidencia a favor (el resto «sin impacto») | — | Automatizable (chequeo) | ALTA |
| Candidato: Full local + builds + CI exacto | Integrar un binario sin evidencia | E3-C.1; `acecde6` CI rojo | Regla vigente | — | Una vez por Candidato | Automatizable | ALTA |
| Dispatch de cobertura del Candidato | Salud de cobertura sobre SHA medido | 0 defectos; 2 duplicados | Regla vigente | — | Una vez | Automatizable | ALTA |
| Validacion del Owner en AutoCAD | Comportamiento de dibujo real (bloques DWG) | 0 hallazgos en la muestra; 8 de 17 en el corpus de I-45 | Cuando cambia dibujo (regla vigente, monotonica) | Metadata solo puede anadir (vinculante); «no se reduce por politica general» es conclusion de ADR-0033 §10 (propuesto) | Una vez por Candidato | Humano | MEDIA |
| Commit documental de cierre + CI | Registro durable | Estados obsoletos; copias 3–7x | — | — | Evidencia a favor de concentrar | Humano/automatizable | ALTA |
| Merge `--no-ff` + CI post-merge | SHA de merge nunca construido | 0 fallos; arbol igual al del cierre salvo I-50 | Regla exact-SHA vigente | — | Una vez | Automatizable | ALTA |
| Limpieza tras ambas coberturas | Declarar integrada una integracion no verificada | E1-I (limpieza anticipada) | Regla vigente | — | Una vez | Automatizable (chequeo) | ALTA |
| Ordenes del Coordinador | Alcance y seguridad de cada gate | Capturas por cercas y re-fetch; incidentes O-1..O-10 | — | — | — | Humano | MEDIA |

---

## 9. Control adversarial aplicado antes del commit

Se hizo una autorrevision y luego una verificacion independiente de solo lectura del borrador contra las notas y las fuentes
primarias. Sus hallazgos se verificaron (git, Actions) y se corrigieron; uno resulto infundado (I-48 si registra conteos de
suite en sus cuerpos de commit; la busqueda heuristica los habia omitido).

| Riesgo revisado | Como se trato |
|---|---|
| Conteos inferidos de prosa | Enumerados item por item o marcados RECONSTRUCTED; corregidos «18 defectos» (17), «tres gates de diagnostico» (5), «candidatos de G4E» (G4E no fue Candidato), posicion del primer commit verificable de I-50 (17), commits de producto de I-48 (10) |
| Commits vs corridas CI | Columnas separadas; pushes agrupados sin corrida propia; corridas huerfanas contadas aparte |
| Versiones de Proposal vs rondas de Architect | Columnas separadas (I-48 10 estados vs 8 rondas; I-50 3 vs 2; I-51 0 vs 1; I-45 4 vs 7; I-52 127 hallazgos en 10 revisiones, no 11) |
| CI docs-only tratado como Full local | Clases separadas; CI docs-only nunca se cuenta como validacion local |
| Ausencia de defecto registrado = sin valor | Advertido en B-38, Q2, Q7, E y TABLE D |
| Mismo arbol tratado como mismo SHA | Retirada la expresion «redundante por arbol» (C-05, C-10); señalado donde ocurrio (I-48 DLL; I-54 G8; merges con arbol igual; I-50 con arbol distinto) |
| I-53 conceptual mezclado con E1/E2/E3 | Cuatro filas separadas en TABLE A; corridas de {C} señaladas dentro de E1 (C-03) |
| I-52 incompleta usada como cerrada | Marcada «activa, no agregada» en cada uso; excluida de agregados de Q2, Q4, Q8 y B-38; instantanea de refs declarada (0.1) |
| Hipotesis convertidas en recomendaciones | Seccion G formulada como hipotesis con prueba de refutacion; retirada de TABLE B la redaccion imperativa |
| Politica V2 accidental | TABLE D rotulada como analisis; seccion H cita fuentes y su estado vinculante |
| Reintroduccion de T0–T4 / R0–R4 | Retirada una hipotesis sobre exentar CI por clase «solo documentacion» (coincide con R0 y con la clave por contenido rechazadas en ADR-0033); colision anotada en TABLE D y en F |
| Etiquetas de plataforma | El cuelgue de I-48 ocurrio en el runner Windows (job de UI); solo CRLF fue Linux |
| Premisas del propio encargo | La premisa del blob de I-48 resulto error del executor (0.6.3); discrepancia de conflictos I-50/I-51 resuelta por simulacion (0.6.4) |

---

## A. MEASURED FACTS

1. En todas las unidades con revision de Arquitecto, la revision se ejecuto dentro de la sesion executor mediante orden de cambio
   de rol.
2. Corridas CI de rama sobre SHAs sin cambio de producto: I-48 17/27, I-54 18/25, I-53 13/19, I-51 5/9; I-52 (activa) 15/15. 0
   fallos sobre esos SHAs (los 4 fallos de CI de rama de la muestra ocurrieron en SHAs que tocan rutas ejecutadas por CI: I-45
   `d8d9919`, I-48 `234f975` y `acecde6`, I-50 `94220fb`).
3. I-54: 21 de 25 corridas de rama sobre SHAs reescritos por 3 rebases; solo 4 SHAs finales tuvieron corrida propia.
4. Versiones de Proposal: I-54 V4→V5 90.4% contenido; I-48 versiones delta (≤5.3%); I-52 (activa) 83 KB → 521 KB (V10→V11 83.5%).
5. Documentos durables de una iniciativa comparten 0–8% de 8-shingles (salvo contratos I-53S/I-53D 18–22%).
6. HANDOFF crecio 288,280 → 412,839 bytes (+43%) en 11 commits de primer padre (10 merges y un cierre directo) en 5 dias; cada
   merge borro 1–7 lineas y el cierre directo `e8ed2bc` borro 18.
7. Primer gate verificable por usuario: I-50 commit 17 de 20; I-54 G7 (ultimo gate de producto); I-51 G5 (ultimo); E1 ninguno.
8. I-48: CI de G4E y G4F en rojo (job de UI abortado por inactividad en el runner Windows); 0 consultas de CI entre G4A y G4F;
   Candidato `acecde6` fallo por CI.
9. I-50: CI de `94220fb` fallo (2 de 6088 pruebas T12 legacy byte a byte, job Core en Ubuntu); la causa CRLF y el pase previo del
   Full local en Windows son afirmaciones del commit A.1 (RECONSTRUCTED).
10. Merges de I-45, I-48, I-51, I-54, E1, E2 y E3: arbol(merge) == arbol(cierre); I-50 no (merge sin rebase). Todos los CI
    post-merge en verde.
11. Dispatch de cobertura duplicado: E1 sobre MERGE_SHA ya cubierto; I-50 dos dispatches midiendo el mismo Candidato.
12. WORKFLOW (205, 300, 357) y AGENTS.md:194 citan «HANDOFF §8-12»; WORKFLOW:361 cita «§12»; HANDOFF tiene §1–§7.
13. `19a555f` contenia en WORKFLOW:238 «se corrige sobre `main`» y en :296 «sobre la rama»; `e932c0c` lo corrigio.
14. Simulacion del merge de I-50: conflictos solo en HANDOFF (5), ROADMAP (1), ideas-futuras (1) y guia de validacion (1).
15. Marcador de conflicto `||||||| 085ca2f` presente en `docs/ideas-futuras.md:816` en `main`.
16. Fila ROADMAP de I-53D contiene «preparacion con aviso y confirmacion».
17. Volumen de ordenes medido: I-45 276,060 caracteres; I-48 296,776 (mas ≈88k estimados por lineas); I-51 57,426 (mas ≈2k
    estimados); solicitudes de Architect de I-48 100,314.
18. Context packs: ultimo cambio 2026-07-17 (8) y 2026-07-27 (2); `ui-editors.md` sigue llamando «objetivo» al shell integrado.
19. `RackBlockData.cs:7-11` conserva el comentario erroneo registrado 5 veces.
20. El MERGE_SHA de I-51 no queda registrado como tal en sus documentos (estados «PENDING») y su corrida post-merge
    `34724848178` no aparece en ningun documento del repo.
21. Filas de ROADMAP con hashes: I-13, I-23, I-32..I-35, I-37D, I-40..I-44 e I-47; ninguna en I-45 ni I-48..I-54.

## B. RECONSTRUCTED FACTS

1. Rondas de Architect: I-45 7, I-48 8, I-50 2, I-51 1, I-53 2, I-54 5; I-52 (activa) 11 completas.
2. Rondas con cambio material de diseño: 17–19 de 25 en iniciativas cerradas; primera ronda siempre material; con 2+ rondas, la
   ultima termino AGREED sin materiales abiertos.
3. Owner Validation: 7 rondas en la muestra, todas PASS a la primera, 0 hallazgos registrados.
4. Core Full local registrado antes del Candidato final: entre 1 (E2) y ~13 (I-54) por unidad de validacion (I-45 aparte, con
   corridas de medicion); UI Full 0 en I-51 y E1.
5. Candidatos invalidados: 2 de 10 (I-48 por CI; I-53D por avance de `main`).
6. Defectos materiales encontrados por Discovery (I-51, I-53, I-54), Coordinador (I-48, I-50), Architect-role (todas) y CI push
   (I-48, I-50); ninguno por Full local ni por el Owner en esta muestra.
7. I-48: 4 defectos materiales detectados por el Coordinador en 3 gates de correccion entre gates internos, no reportados por el
   executor.
8. Fundaciones re-descritas o redescubiertas: Rack Identity 6, View Identity 6, restamp 5, Project Variables 5, inventario de
   sistemas 5, Authored vs Effective 4(+1), preservacion del sobre 4.
9. Conflictos de documentacion compartida en rebases: I-53 E1 (1), I-54 G4 (4) y G7 (2), I-53D (1); 0 de producto.
10. Incidentes donde la orden contradijo el documento vinculante: I-53 E1-I (cobertura), I-50 G8 (sin rebase), I-53D E3-C
    (instrucciones incompatibles).

## C. SUPPORTED INFERENCES

1. La sobrecarga variable mas grande es el numero de rondas de diseño; la fija, la ceremonia por unidad Git y la documentacion
   de cierre.
2. Las rondas posteriores a la primera se explican mas por consecuencias de la reconciliacion previa y por detalle mecanico que
   por hallazgos nuevos, y no por decisiones tardias del Owner.
3. La repeticion documental es sobre todo parafrasis, lo que la hace dificil de detectar mecanicamente y facilita copias
   obsoletas (§8–12, PENDING, «confirmacion»).
4. Los problemas de plataforma y de entorno del runner los detecta el CI push, no el Full local; un gate que no mira el CI puede
   dejar pasar un rojo durante varios gates.
5. El requisito exact-SHA, correcto por el estampado del binario, produce re-ejecuciones sin cambio de producto cuyo costo
   depende del orden commit→evidencia.
6. Omitir en una orden reglas documentadas no produjo incidentes en los bloques de guardia docs-only, jobs post-merge, dispatch
   y limpieza, pero si en la celda de estado ROADMAP (2 desviaciones); re-enunciarlas mal produjo O-1 y O-2; omitir
   restricciones no documentadas (re-fetch en momentos precisos, chequeo de CI por gate) coincidio con desviaciones.
7. Separar un contrato congelado en unidades de entrega acorto las unidades de UI y aislo la re-validacion, a costa de triplicar
   la ceremonia y de una unidad fundacional sin salida verificable.
8. La re-descripcion de Rack/View Identity y restamp se concentra en detalles ausentes de los ADR; si un registro la reduciria es
   la hipotesis H7; los contratos implicados cambiaron 4–5 veces en ~7 semanas.
9. La revision «de Arquitecto» de esta muestra es auto-revision adversarial guiada; su tasa de hallazgos no puede atribuirse a
   revision independiente.

## D. UNKNOWN / NOT RECOVERABLE

1. Tiempo activo humano y de agente por etapa, gate, ronda o validacion.
2. Independencia real de las revisiones de Arquitecto y existencia de revisiones externas no registradas.
3. Textos originales de revisiones de Arquitecto y del Coordinador en I-48, I-50, I-53 e I-54 (solo resumenes versionados).
4. Detalle por escenario de las validaciones del Owner (I-48, I-53D) y DLL realmente cargado en I-48.
5. Ejecuciones locales: toda cifra es afirmacion; no hay TRX ni logs versionados.
6. Autoria de las ordenes (modelo Coordinador, Owner u orquestador).
7. Primer 27% de la sesion de I-53 y de I-54; ordenes V1–V9 de I-52.
8. Valor contrafactual de Full local, Owner Validation y rondas tardias (que habria escapado sin ellas).
9. Si cada re-descripcion de fundaciones fue pedida por la orden (confirmado solo en I-48).
10. Resultado final de I-52 e I-55 (activas) y cifras posteriores a la instantanea de refs.
11. Resultado de la auditoria de 9 agentes iniciada al cierre de I-45.

## E. PROCESS STEPS WITH DEMONSTRATED VALUE

1. **Discovery con evidencia de codigo** — capturo defectos HIGH antes del diseño (B-01..B-03, B-04, B-05).
2. **Primera ronda de revision de diseño** — cambio el diseño en todas las unidades cerradas con ronda (B-07, B-15, B-19, B-21,
   B-29; I-45 R1 con 8 hallazgos materiales).
3. **Revision del Coordinador entre gates internos** — 4 defectos materiales en I-48 (B-10, B-40, B-11).
4. **RED→GREEN focal por gate** — capturas en I-50 e I-51 (B-16, B-18, B-41).
5. **CI push sobre SHAs que tocan rutas de CI** — problemas que el Full local afirmado no detecto (B-12, B-17).
6. **Re-fetch inmediatamente antes de declarar Candidato** — E3-C BLOCKED evito declarar sobre base superada (B-33).
7. **Decision de politica del Owner** — cambio alcance (OD-6, OD-8) y acepto ADR.

Sin capturas propias en esta muestra, pero con valor por mecanismo o por evidencia previa (no se cuentan como «demostrado»
aqui): CI post-merge por SHA exacto (el binario estampa el SHA; B-25), Owner Validation en AutoCAD (8 de 17 defectos en el
corpus de I-45) y cercas de alcance por gate (cumplidas en toda la muestra; valor contrafactual UNKNOWN).

## F. PROCESS STEPS WITH EVIDENCE OF REPEAT OVERHEAD

1. CI sobre SHAs docs-only, vacios o luego reescritos (C-01..C-04). Nota: una exencion por clase «solo documentacion» coincide
   con enfoques rechazados por ADR-0033 (R0; clave de estado por contenido); este documento no la formula como hipotesis.
2. Rondas de Architect que confirman o corrigen su propia reconciliacion previa (C-14..C-16; I-52, activa, C-17).
3. Versiones de Proposal acumulativas y documentos que re-enuncian el estado ronda por ronda (5.1; I-52, activa, C-18).
4. Full locales repetidos sobre el mismo arbol por orden de commit y estampado (C-08..C-11).
5. Dispatches de cobertura duplicados (C-06, C-07).
6. Bloques de cierre copiados en varios documentos y HANDOFF que crece por anexion (C-25; 5.3).
7. Ceremonia completa por cada unidad Git de una iniciativa conceptual (Q1, Q13).
8. Re-medicion de ramas paralelas en cada gate cuando las refs no se movieron (C-24).
9. Re-descripcion de fundaciones ya aceptadas en cada Discovery (C-28; seccion 6).
10. Ordenes que re-pegan revisiones a su propio autor o re-enuncian reglas ya vinculantes (C-19, C-20; 5.4).
11. Contratos minimos que re-enuncian el contrato congelado (C-29).

## G. HYPOTHESES TO TEST IN PROPOSAL V1

Hipotesis, no decisiones. Cada una indica que evidencia la refutaria. Ninguna reintroduce T0–T4 ni R0–R4 (seccion H, punto 7).

1. **H1 — Rondas de diseño proporcionales.** Una segunda ronda solo cuando la primera deja un hallazgo material abierto no
   aumenta los defectos de producto posteriores. *Refutaria*: defectos materiales hallados despues del congelamiento en
   iniciativas con una sola ronda.
2. **H2 — Chequeo de CI por gate.** Leer el resultado del CI push de cada gate que toca rutas de CI antes de abrir el siguiente
   adelanta la deteccion de fallos de plataforma y de runner. *Refutaria*: fallos de CI que igual se detectan solo al declarar
   Candidato.
3. **H3 — Full local concentrado en el Candidato.** Mantener Full local solo en el Candidato, con focal por gate y CI push, no
   aumenta regresiones integradas. Nota: no es un tier T0–T4; para el Core choca con AGENTS («no se deduce ninguna por analogia
   con LC-UI»), por lo que exigiria cambiar esa regla dentro de la compuerta de vigencia. *Refutaria*: un defecto que solo un
   Full local intermedio habria capturado.
4. **H4 — Re-fetch en momentos definidos.** Chequeos en puntos fijos (antes de la primera edicion de archivo caliente, antes del
   commit, antes de declarar Candidato, antes del merge) sustituyen la re-medicion por gate. *Refutaria*: trabajo sobre base
   superada no detectado por esos puntos.
5. **H5 — Cierre concentrado.** Un unico registro de cierre por iniciativa referenciado (no copiado) reduce copias obsoletas sin
   perder trazabilidad. *Refutaria*: informacion de cierre que solo existia en una copia secundaria.
6. **H6 — Ordenes por referencia.** Referenciar reglas ya vinculantes y conservar solo restricciones no documentadas y
   especificas del gate no aumenta desviaciones. *Refutaria*: desviaciones en reglas vinculantes omitidas en la orden (la celda
   de estado ROADMAP ya muestra 2).
7. **H7 — Registro de fundaciones.** Un registro breve, enlazado a ADR y a simbolos, reduce la re-descripcion de Rack/View
   Identity y restamp sin propagar errores. *Refutaria*: entradas del registro contradichas por el codigo o por un ADR posterior
   en menos de dos iniciativas.
8. **H8 — Unidades de entrega funcionales.** Separar un contrato congelado en unidades verificables por usuario (E2/E3) sin una
   unidad fundacional invisible reduce ceremonia sin aumentar re-validacion. *Refutaria*: re-validaciones cruzadas entre
   unidades o fundaciones integradas sin prueba de uso.
9. **H9 — Revision independiente vs de rol.** La tasa de hallazgos materiales de rondas posteriores cambia cuando la revision
   no se ejecuta en la sesion del autor. *Refutaria*: tasas equivalentes en una comparacion controlada.

## H. CONSTRAINTS THE PROPOSAL MUST NOT VIOLATE

1. **Invariante de transicion** (contrato I-56 §0.1, vinculante): I-49, I-52 e I-55 grandfathered; toda iniciativa reclamada antes de
   `WORKFLOW_V2_EFFECTIVE_SHA` termina bajo el workflow con el que se reclamo; I-56 incluida; ningun contrato activo se reescribe para
   aplicar la V2.
2. **Invariante de vigencia** (contrato I-56 §0.2, vinculante): ninguna politica normativa de la V2 entra en vigor sin
   `Coordinator = AGREED` y `Architect = AGREED` sobre la misma version y `Owner = APPROVED`; `WORKFLOW_V2_EFFECTIVE_SHA` no
   existe y no se inventa.
3. **Identidad por SHA exacto** (AGENTS.md, «Reutilizacion de evidencia», vinculante): el binario estampa el SHA; igualdad de arbol
   no es identidad de binario; las clases de evidencia no se sustituyen entre si.
4. **Validacion del Owner** (AGENTS punto 5; AUTOMATION_PLAN §11, vinculantes): se exige cuando cambia el comportamiento de dibujo y
   la metadata solo puede anadir, nunca quitar. Que ademas «no se reduce por politica general» es conclusion de ADR-0033 §10, en
   estado `propuesto`, citada por AUTOMATION_PLAN §11.
5. **Nunca un commit directo sobre `main`**; la correccion de un CI post-merge rojo se hace en la rama (CLAUDE.md; WORKFLOW 4.5.6;
   AUTOMATION_PLAN §3; vinculantes).
6. **Sin merge automatico** (AUTOMATION_PLAN §13, vinculante).
7. **Enfoques rechazados por I-45 no se reintroducen bajo otra terminologia**: T0–T4, R0–R4, seleccion por impacto o por
   FQN/ruta/grafo, taxonomia de pruebas, clave de estado de validacion por contenido, igualdad de arbol, retirar cobertura, reducir
   la validacion del Owner, multi-STA inmediato. Fuente: ADR-0033 (estado `propuesto`; criterios de reapertura en su §13) y la orden
   de G1 de I-56; su fuerza deriva de esa orden mientras el ADR no sea aceptado.
8. **Aceptar o rechazar ADR corresponde solo al Owner** (AUTOMATION_PLAN §11, vinculante).
9. **Una seleccion de pruebas que no selecciona nada es un FALLO** (AGENTS.md, vinculante).
10. **Alcance de I-56** (contrato I-56 §1, §4): solo documentacion y proceso; sin cambios de producto ni de pruebas en toda su vida;
    fuera de `docs/**` prohibido en G0/G0.1 y sujeto a declaracion en la Proposal despues. G0, G0.1 y G1 no crearon
    `docs/ORCHESTRATION.md`; si la V2 lo necesita lo decide la Proposal, sujeta a la compuerta de vigencia (punto 2). Los registros
    cerrados no se corrigen hacia atras (AGENTS, «Registro historico, no precedente»).
