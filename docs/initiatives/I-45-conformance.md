# I-45 — Conformidad de V4

**Gate:** G7 · **Entregado para revisión independiente** · **NO integrado**

Este documento **no decide nada y no se aprueba a sí mismo**. Registra si lo que V4 acordó está de
verdad implementado, y deja por escrito lo que falta, lo que se difiere y lo que I-45 **no** demuestra.
Quien juzgue si el paquete está listo es la revisión Coordinador ↔ Arquitecto, no este texto.

## 0. Estado

```
PLAN VERSION:       V4
Coordinator:        AGREED
Architect:          AGREED
Open disagreements: NONE
CONSENSUS STATUS:   REACHED

ADR-0033:           propuesto        (ningún gate lo aceptó)
Gates ejecutados:   P0 · G0A · G2 · G0B · G1 · G3 · G4 · G5 · G6 · G6-C1 · G7
Integración:        NO
```

El consenso Coordinator ↔ Architect **no es** aceptación del ADR: son dos actos de dos autoridades
distintas, y aceptarlo corresponde solo al dueño.

## 1. Cómo se auditó, y qué vale esa auditoría

Ocho barridos sobre el árbol real —no sobre lo que los documentos afirman de sí mismos— repartidos por
área: lo que no debe existir, G2, G0B, G1, G3, G4, G5, G6, G6-C1, contradicciones normativas,
precedencia documental y ADR frente a implementación. Cada uno cubrió **128 enunciados de decisión**.

Cada decisión termina en **un** estado: `PASS`, `DEFERRED-INTEGRATION-CHECK`, `BLOCKER` o
`OUT-OF-SCOPE`. **No existe «casi PASS».** `PASS` exigió encontrar *dónde* la decisión es operativa y
verificarlo; una decisión que solo viva en ADR-0033 —que está `propuesto`— y no en un documento
operativo o en código **no es PASS**.

> **Límite de esta auditoría, y no es menor.** La produjo el mismo ejecutor que implementó los gates,
> dentro del mismo gate. Los barridos son separados, no independientes en el sentido que importa. Por
> eso el resultado se entrega **para** revisión externa y no **como** su conclusión.

Los barridos devolvieron **6 bloqueadores**, todos de conformidad documental. Se identifican y se
resuelven en §2 y §3; ninguno era una divergencia de comportamiento entre lo decidido y lo
implementado. Las tablas de §4 resumen por área los 128 enunciados; **el volcado completo de la
auditoría no se versiona**, igual que el resto de resultados de instrumento.

## 1.bis Revisión de conformidad: tres rondas, cinco hallazgos, consenso alcanzado

```
Architect, primera revisión (4d33871)   NOT AGREED   ARCH-01..04  → G7-C1
Architect, re-revisión      (19a555f)   NOT AGREED   ARCH-05      → G7-C2
Architect, revisión final   (e932c0c)   AGREED WITH NON-BLOCKING FINDINGS
Coordinator                             AGREED

IMPLEMENTATION CONFORMANCE CONSENSUS:   REACHED
```

La revisión final cerró con **0 BLOCKER y 0 HIGH**; lo que queda son `LOW` y `NOTE` que **no** se
convierten en backlog obligatorio. Dos de ellos, por dejarlos nombrados:

- **Redacción del borrado remoto** (`WORKFLOW.md` §3): enuncia solo la condición necesaria «solo
  procede tras confirmar que el merge existe». **`LOW`, no bloqueante**: no es una ruta ejecutable —la
  viñeta que la contiene encabeza con el gate completo, §3 no ejecuta ningún borrado, y `AGENTS.md`
  exige además el CI posterior verde y gana por precedencia.
- **Paquete de conformidad desactualizado**: no registraba ARCH-05 ni `G7-C2`. **Corregido aquí.**

**Tres de los cinco hallazgos los creó I-45 en sus propios gates** —dos en `G6-C1` y uno en `G7-C1`—,
al modificar algo de lo que dependía otra regla sin volver a mirarla. Es el patrón que estas revisiones
existen para atrapar.

### Los cinco hallazgos

La revisión sobre `4d33871` devolvió **cuatro hallazgos materiales** (0 BLOCKER, 4 HIGH), aceptados por
el **Coordinador** y aplicados en **`G7-C1`**:

| # | Hallazgo | Corrección en `G7-C1` |
|---|---|---|
| **ARCH-01** | El CI posterior al merge era obligatorio en `AGENTS.md` y en ADR §7, pero `WORKFLOW` §4.5 no lo exigía en ningún paso, ordenaba limpiar justo después del merge, y su nota llamaba al CI **pre**-merge «la compuerta real» | `WORKFLOW` §4.5 gana los pasos 6 y 7: esperar y verificar el CI sobre el `MERGE_SHA` **con su cobertura**, y la comprobación diferida de la cobertura del Candidato. **La limpieza queda bloqueada hasta que ambos pasen.** La nota se reformula: son **dos** compuertas, ambas obligatorias |
| **ARCH-02** | «Evidencia de UI del CI» se definía solo por `head_sha`. Desde `G6-C1` una corrida de `workflow_dispatch` tiene `head_sha` = punta del ref y ejecuta otro commit: podía acreditar UI a un SHA cuyo código nunca corrió | La definición exige ahora **las cuatro condiciones**, con `event = push` entre ellas. Un despacho **no acredita UI a ningún SHA** |
| **ARCH-03** | El instrumento agrupaba **todas** las corridas por `head_sha`, sin filtrar evento: un despacho contaminaría la evidencia de CI del SHA equivocado | Solo entran eventos `push`. Los no-`push` se **excluyen y se cuentan aparte**. **Control E** nuevo, ejercido con material sintético |
| **ARCH-04** | `requires_owner_validation` / `requires_autocad` podían leerse como exención de la validación del dueño —HANDOFF muestra casos propagados «por analogía»— y G7 cerró esa casilla buscando la nomenclatura `R0–R4`, no el concepto | La metadata es **monotónica**: `false` no exime; gana el requisito más estricto. La fila de §4.1 se reescribe diciendo lo que de verdad se verificó |

Y la re-revisión sobre `19a555f` devolvió un **quinto** hallazgo, esta vez creado por `G7-C1`:

| # | Hallazgo | Corrección en `G7-C2` |
|---|---|---|
| **ARCH-05** | `G7-C1` dejó **dos remedios incompatibles** ante un CI posterior al merge en rojo: uno ordenaba corregir **directamente sobre `main`** —que el repositorio prohíbe en dos sedes— y otro, corregir en la rama | Una sola semántica: el merge **sigue existiendo**, la integración queda **no verificada**, la limpieza se bloquea, **rama y worktree se conservan**, la corrección se hace **en la rama de iniciativa** y se reingresa por integración controlada con su propia verificación posterior. **Trabajo directo sobre `main`: prohibido, sin excepción** |

## 2. Los seis bloqueadores encontrados, y cómo quedaron

| # | Bloqueador | Resolución en G7 |
|---|---|---|
| 1 | La deuda de advertencias xUnit solo vivía en el cuerpo de un commit, no en un documento consultable | Registrada en `ideas-futuras.md` con método reproducible |
| 2 | Su cifra (60) no era reproducible desde ningún artefacto | **Re-medida: 30.** La de G0B contaba líneas de aviso duplicadas |
| 3 | `AGENTS.md` apoyaba esa deuda en `I-45-discovery.md` §8.4, que describe dos `CS0105` **ya corregidos** | Puntero rehecho; la norma se sostiene sola |
| 4 | El contrato afirmaba «cero cambios de producto, de pruebas y de CI» en §9, §11 y §14 | Corregido en las tres, con el alcance real por gate |
| 5 | El plan dejó sin anotar el punto 1 de su lista de no resueltos, que G0B cerró | Anotado, con su límite |
| 6 | El contrato citaba `WORKFLOW.md` §7 para una regla que vive en §2 | Corregido |

Y tres más, encontrados por la revisión adversaria **sobre este mismo documento** y corregidos antes
de confirmarlo: una compuerta de riesgo inventada sobre la validación del dueño (§6), los criterios de
reapertura sin sede operativa (§9), y la promesa de un mecanismo automático a un gate que ya no existe
(§8).

## 3. Lo que se corrigió de mis propias afirmaciones

- **La cifra de advertencias.** G0B registró «60 (28/18/12/2)». La medición reproducible de G7 da
  **30 (14/9/6/1)**: exactamente la mitad en **cada** categoría, porque aquello contaba líneas de
  aviso y MSBuild las emite dos veces.
- **«Frontera de riesgo en ejecución».** Llegué a escribirlo como condición de la validación del
  dueño. No existe en el repositorio, no está definido, y ADR-0033 §10 dice que esa validación **no se
  reduce por política general**. Retirado: el disparador vigente es objetivo y ya existía —«cambió el
  comportamiento de dibujo»—.
- **«Cero bloqueadores».** No se proclama. Se listan los seis, con su resolución, y se deja que la
  revisión externa juzgue.

## 4. Matriz de decisiones

### 4.1 Lo que NO debe existir — y no existe

| Decisión | Verificación | Estado |
|---|---|---|
| Tiers T0–T4 operativos | Sin coincidencias operativas en `src/`, `tests/`, `eng/`, `.github/`; CI con 4 jobs planos | `PASS` |
| Modelo de riesgo R0–R4 **introducido por I-45** | Ninguno. Los `R1..R4` del repo son etiquetas de ronda e ids de catálogo, y ninguno decide ejecución | `PASS` |
| ⚠ Metadata **preexistente** de requisito de validación (`requires_autocad`, `requires_owner_validation`) | **Existe**, es anterior a I-45, y gobierna si se exige la validación del dueño. **Esta casilla se cerró en G7 buscando la nomenclatura `R0–R4`, no el concepto** — lo señaló la revisión del Arquitecto (ARCH-04). Desde `G7-C1` esa metadata es **monotónica**: `false` no exime de una obligación que venga de otra sede, y gana el requisito más estricto | `PASS` tras `G7-C1` |
| Selector por impacto | `on: push` sin `paths:`; cero `--filter` ejecutable; los cuatro comandos apuntan al csproj completo | `PASS` |
| Taxonomía manual de pruebas | `[Trait]` = 0, `[Category]` = 0, `*.runsettings` = 0, `xunit.runner.json` = 0 | `PASS` |
| Quick CI / carril rápido | Un solo workflow; ningún `if:` reduce la población | `PASS` |
| Multi-STA | Sin cambios en el modelo de hilos | `PASS` |
| Golden DWG implementado | No existe | `PASS` |
| `ValidationStateKey` / equivalencia por contenido | Sin implementación ni residuo | `PASS` |
| Reutilización por igualdad de árbol | Retirada de sus sedes normativas en G6 | `PASS` |

### 4.2 G2 · G0B

| Decisión | Estado |
|---|---|
| Timeouts por job (20/15/25/30), TRX de Core y UI, `--blame-hang` en ambas | `PASS` |
| Artefactos de diagnóstico con `if: always()` | `PASS` |
| Normalización de cobertura fail-closed donde se recolecta | `PASS` |
| Sin cambio de población de pruebas | `PASS` |
| Aislamiento del prompt de descarte, 2 de 121 clases | `PASS` |
| Paralelismo del ensamblado **no** deshabilitado | `PASS` |
| `SelectiveCabeceraHeightPrompt` no serializado sin necesidad | `PASS` |
| Deuda de advertencias registrada | `PASS` (era bloqueador) |

### 4.3 G1 · G3

| Decisión | Estado |
|---|---|
| Medición local reproducible con procedencia automática | `PASS` |
| Fallo-cerrado ante cero pruebas, ejercido sin filtrar suites reales | `PASS` |
| Reconstrucción de calendario y medición de CI | `PASS` |
| `OrdinaryIterationLocalSeconds` añadida sin renombrar la métrica histórica | `PASS` |
| Sin plataforma, sin base de datos, sin serie histórica versionada | `PASS` |
| Exact-SHA / cross-channel / policy-required separados | `PASS` |
| Candidato **nunca** inferido | `PASS` |
| CI posterior al merge = evidencia nueva | `PASS` |
| El instrumento ya no trata co-ubicación como identidad de SHA | `PASS` |

### 4.4 G4 · G5 · G6

| Decisión | Estado |
|---|---|
| Duración del dueño: experimental, solo él la declara, nunca inferida, su ausencia no bloquea nada, criterio 10/3/80 %, sin relleno retroactivo | `PASS` |
| UI Full local no obligatoria en la iteración ordinaria | `PASS` |
| Evidencia intermedia de UI del CI = **las cuatro condiciones a la vez**: `event = push` · job `ui-tests` · `conclusion = success` · `head_sha` = el SHA empujado exacto | `PASS` |
| Una corrida de **`workflow_dispatch` NO es evidencia LC-UI** de ningún SHA —ni del tip ni del medido—: su `head_sha` es la punta del ref y ejecuta el commit del input | `PASS` (cerrado en `G7-C1`, ARCH-02) |
| Candidato exige UI Full local; Full final conserva UI; sin sustitución del Core | `PASS` |
| Push agrupado no propaga evidencia | `PASS` |
| Frontera de reutilización = SHA exacto, misma clase | `PASS` |
| Invalidadores: árbol sucio, SDK resuelto, AutoCAD y biblioteca de bloques | `PASS` |
| Pre-rebase ≠ post-rebase; CI post-merge obligatorio; commit documental no hereda | `PASS` |
| Contradicción sobre el mismo SHA → no determinismo | `PASS` |

### 4.5 G6-C1

| Decisión | Estado |
|---|---|
| Cobertura OFF en push ordinario | `PASS` — **verificado en corrida real**, sin artifact de cobertura |
| Cobertura de Candidato solo por `workflow_dispatch` explícito sobre SHA exacto | estructura `PASS` · ejecución `DEFERRED-INTEGRATION-CHECK` |
| Cobertura ON en el trunk | estructura `PASS` · ejecución `DEFERRED-INTEGRATION-CHECK` |
| Sin umbral, sin selector | `PASS` |

## 5. Comprobaciones que no pueden ejecutarse antes de integrar

**Son bloqueantes de la integración final, no criterios de reapertura.** No se pierden en
`ideas-futuras.md`.

GitHub solo habilita `workflow_dispatch` para workflows presentes en la **rama por defecto**.
Comprobado, no supuesto: hoy `gh workflow run` responde
`HTTP 422: Workflow does not have 'workflow_dispatch' trigger`.

**Cobertura de Candidato**, una vez `ci.yml` esté en `main`:

```
workflow_dispatch con candidate_sha = <SHA exacto del Candidato>
requested SHA == checkout HEAD           (el job aborta en rojo si no)
cobertura recolectada · artifact rackcad-coverage-cobertura presente
measured-sha.txt coincide con el Candidato
```

**Cobertura del trunk**, tras el merge `--no-ff`: CI de `main` verde, cobertura recolectada, artifact
presente, sobre el SHA post-merge exacto.

## 6. La validación del dueño no se toca

El disparador sigue siendo el vigente y objetivo: **cambió el comportamiento de dibujo**
(`AGENTS.md` punto 5, `WORKFLOW.md` §4.5.3). I-45 no lo ha cambiado, pero **eso lo decide el
Coordinador**, no este documento. No se introduce ninguna compuerta de riesgo: ADR-0033 §12 rechaza un
modelo operativo de riesgo, §10 dice que esta validación **no se reduce por política general**, y §4.2
del contrato la fija como límite permanente que ningún gate levanta.

## 7. Métricas, con su procedencia

| Magnitud | n | mediana | dispersión | Procedencia |
|---|---:|---:|---:|---|
| `MandatoryLocalCycleSeconds`, línea base G1 | 3 | 333.44 s | — | sesión de G1, árbol limpio |
| `MandatoryLocalCycleSeconds`, comparable G5 | 3 | 401.98 s | 1.10 | misma sesión que la fila siguiente |
| `OrdinaryIterationLocalSeconds` | 3 | 95.65 s | 1.24 | misma sesión, árbol limpio |
| Ejecuciones de evidencia (G3) | — | 440 | — | corpus I-40/42/43/44 |
| Cruce de canal (G3) | — | 64 | — | el término que Discovery contaba como repetición |
| Reconfirmaciones por SHA exacto determinables | — | 0 | — | — |
| Advertencias propias de las suites | — | 30 | — | re-medidas en G7 |

Los JSON viven en `artifacts/validation/` y **no se versionan**: se reproducen ejecutando el método
(`I-45-measurement-method.md`). Las dos primeras filas de tiempo **no se restan**: la suite de UI sigue
ejecutándose, en el CI. Son **semánticas de flujo distintas, no aceleración de la suite**, y solo las
dos filas de la misma sesión se comparan entre sí.

**CI:** no se reclama ninguna mejora de reloj de pared. El plan esperaba aproximadamente cero.

**Duración activa del dueño:** línea base `UNKNOWN`, sin ningún valor todavía.

## 8. Qué cambió en materia de seguridad

Con la precisión que cada uno admite:

- Hay **fusible y diagnóstico** para los cuelgues: `--blame-hang` en ambas suites y `timeout-minutes`
  por job. Un fusible corta y deja rastro; no garantiza que no haya cuelgues.
- Existe **identidad de prueba** en un artefacto: TRX de Core y de UI.
- La carrera concreta del prompt de descarte está **cerrada estructuralmente**: sus dos clases ya no
  pueden ejecutarse concurrentemente. No cierra el determinismo global de la suite de UI.
- Un filtro que no selecciona nada **es un fallo por norma** — pero **no hay mecanismo automático**
  que lo compruebe: la obligación es de quien ejecuta, y eso queda registrado como deuda (§10).
- El Candidato tiene **forma escrita** y se declara por SHA exacto, no por convención oral.
- La evidencia de CI **no se propaga** entre commits de un push agrupado.
- La reutilización entre SHAs distintos quedó retirada, en sus **dos** fronteras: «el trunk no
  avanzó» y «el commit documental no cambia el binario».
- El CI posterior al merge **sigue siendo obligatorio**.
- El Full del Candidato, el Full final y la validación del dueño **se conservan** donde se exigían, y
  el checklist de cierre pasó de un ambiguo «suite completa» a **las dos suites, en local**.

## 9. Criterios de reapertura

Se **reproducen** aquí los de [ADR-0033](../adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md)
§13, sin inventar otros y sin convertirlos en backlog. Se reproducen en vez de solo citarlos porque el
ADR está `propuesto`: si el dueño lo rechazara, estos criterios seguirían siendo el registro de qué
evidencia haría falta.

- **Selección por impacto** — cuando exista un instrumento defendible para medir capacidad de
  detección: controles positivos, defectos sembrados o un corpus de mutación representativo. **La
  observación pasiva no sirve**: produciría un registro vacío, y un registro vacío no es evidencia.
- **Taxonomía** — cuando exista un consumidor operacional real que la justifique y que pueda
  verificarse suficientemente.
- **Múltiples hilos STA** — tras controlar el estado global mutable relevante, disponer de un
  *benchmark* reproducible y demostrar retorno.
- **CI rápido separado del completo** — cuando el tiempo hasta la primera evidencia relevante deje de
  cumplir sus objetivos, o cuando exista una reducción demostrablemente segura y material.
- **Golden DWG** — cuando exista infraestructura suficiente para comparar salidas reales.
  **Invariante: el baseline de un Golden NUNCA se actualiza solo; el dueño aprueba cada cambio de
  baseline.**

## 10. Deuda diferida

| Deuda | Dónde | Estado |
|---|---|---|
| 30 advertencias de analizadores xUnit | `ideas-futuras.md` 16.bis | **Deuda con registro, NO excepción**. La meta de 0 advertencias propias no se debilita |
| Nadie comprueba automáticamente «0 pruebas seleccionadas = FALLO» | `ideas-futuras.md` 16.ter | La regla sigue normativa; falta su comprobación. **Ningún gate la tiene asignada** |
| Golden DWG · selección por impacto · multi-STA · CI rápido · taxonomía | §9 de este documento | Futuro, con los criterios de arriba |

El **experimento de duración del dueño no es deuda**: está **activo** hasta cumplir su criterio de
10 rondas elegibles en 3 iniciativas.

## 11. Lo que I-45 NO demuestra

- **No** demuestra que el Full sea innecesario.
- **No** demuestra que el CI sea lento: su reloj es un `max()` sobre caminos paralelos, con techo
  medido de ~10 %.
- **No** demuestra que seleccionar pruebas sea seguro. El *recall* de cualquier selección **no es
  acotable** desde este repositorio.
- **No** demuestra ningún ahorro histórico por SHA exacto: la medida fue **0 determinable**, y el
  canal local es `UNKNOWN` porque nunca registró contra qué SHA corrió.
- **No** mide todavía el esfuerzo activo del dueño.
- **No** implementa Golden DWG, ni elimina la deuda de advertencias.
- **No** garantiza el determinismo global de la suite de UI.
- **No** impide que un `workflow_dispatch` de cobertura mida un SHA alcanzable solo desde un fork: se
  comprueba identidad, no ascendencia.
- **No** ha ejecutado los dos caminos de cobertura de §5.

## 12. Incógnitas conocidas

- Cuántas veces se ejecutó una Full local sobre el **mismo** SHA: `UNKNOWN`, y estructuralmente —de
  160 afirmaciones de clase Full del corpus, ninguna nombra un SHA.
- Si los 3 candidatos históricos de reconfirmación de CI lo eran: haría falta el SDK resuelto de cada
  corrida, y el pin `8.0.x` es flotante.
- Cuánto costaron de verdad las corridas locales históricas: la línea base de G1 es un proxy.
- El determinismo global de la suite de UI.

## 13. Alcance tocado

`AGENTS.md` · `docs/WORKFLOW.md` · `docs/guias/validacion-manual-autocad.md` ·
`docs/adr/0033-*.md` (solo §11, y sigue `propuesto`) · `docs/initiatives/I-45-*.md` ·
`docs/ideas-futuras.md` · `.github/workflows/ci.yml` · `eng/validation/*.ps1` ·
`tests/RackCad.UI.Tests` (G0B) · `src/RackCad.Application/…/PushBackPlanComposer.cs` (dos `using`).

`main` no ha sido modificada. `docs/ROADMAP.md` y `docs/HANDOFF.md` **no** se tocan: corresponden a la
sesión de integración.

## 14. Qué queda

1. ~~Revisión de conformidad del **Coordinador**~~ — **AGREED**.
2. ~~Revisión de conformidad del **Arquitecto**~~ — **AGREED WITH NON-BLOCKING FINDINGS**, tras dos
   rondas en `NOT AGREED` y sus dos correctivos.
3. Candidato final y su evidencia completa sobre un SHA exacto.
4. Validación del dueño en AutoCAD **si aplica el disparador vigente** (§6).
5. Integración, y con ella las dos comprobaciones diferidas de §5.
6. Limpieza.

**I-45 no está cerrada, no está integrada, y ADR-0033 no está aceptado.**
