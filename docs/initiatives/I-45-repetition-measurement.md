# I-45 — Telemetría de repetición de validación

**Gate:** G3 · **Instrumento:** [`eng/validation/measure-repetition.ps1`](../../eng/validation/measure-repetition.ps1)
· **Vocabulario:** [I-45 — Método de medición](I-45-measurement-method.md) §8
· **Decisiones:** [ADR-0033](../adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md) (estado `propuesto`)

G3 **mide** repetición. No elimina obligaciones, no aplica LC-UI, no aplica reutilización por SHA
exacto, no cambia el CI y no toca el producto ni las pruebas.

La pregunta de G3 no es cuántas veces aparece `dotnet test` en la historia. Es: **para una iniciativa,
qué ejecuciones de evidencia ocurrieron, cuáles aportaron evidencia nueva, cuáles reconfirmaron el
mismo SHA, cuáles fueron cruces de canal y cuáles estaban obligadas por una norma citable.**

## 1. Corpus

| Iniciativa | Merge | Commits del rango | Contrato |
|---|---|---:|---|
| I-40 — cabeceras Push Back | `bf327b3` | 11 | sí |
| I-42 — Push Back compuesto | `e6bb6d7` | 93 | sí |
| I-43 — Selectivo por alcance | `bd40ef7` | 34 | sí |
| I-44 — peraltes intermedios | `1165240` | 5 | **no existe** |

Es el corpus comparativo de Discovery. **No se amplió**: la única laguna real del corpus —no contiene
ningún SHA con más de una ejecución de CI— se cubre con controles a nivel de repositorio (§7), que es
más honesto que añadir una iniciativa para fabricar el ejemplo.

I-44 **no tiene contrato** en `docs/initiatives/`. Su expediente vive solo en cuerpos de commit y en
HANDOFF. Es un hecho del corpus, no un fallo de la búsqueda.

## 2. Fuentes y orden de autoridad

1. **GitHub Actions real** (`MEASURED`).
2. Cuerpos de commit.
3. Contratos y evidencia de iniciativa.
4. Registros de validación del dueño.
5. HANDOFF, solo donde corresponde al cierre.

**Evidencia de máquina > afirmación en prosa.** Una autodeclaración de commit no equivale a evidencia
de CI. Donde una y otra discrepan, manda la API y la discrepancia se registra.

### Por qué el instrumento es un script aparte

`measure-validation.ps1` (G1) es un **cronómetro sobre comandos vivos**, y toda su semántica de
fallo-cerrado gira sobre el conteo de pruebas de una corrida que él mismo lanza. Este instrumento no
ejecuta ninguna suite: lee historia y clasifica. Meter el clasificador dentro del cronómetro le
impondría una semántica de fallo que aquí no significa nada y obligaría a convivir a dos esquemas de
salida en un mismo archivo. Comparten deliberadamente la forma de la procedencia y la disciplina de
escribir a `artifacts/`.

## 3. Dos hallazgos de método que decidieron el resultado

### 3.1 Un rerun de GitHub es invisible en `gh run list`

Un re-intento **conserva el mismo id de corrida e incrementa `run_attempt`**. `gh run list` devuelve
una fila por corrida, así que un rerun auténtico no aparece como segunda corrida y se contaría como
una sola ejecución. El instrumento **pagina la API** y expande cada corrida en tantas ejecuciones como
intentos tuvo.

Sin esta corrección, los dos únicos reruns de la historia del repositorio habrían sido invisibles.

### 3.2 Una suite local no atestigua el SHA del commit que la describe

`Directory.Build.targets` resuelve `SourceRevisionId` con `git rev-parse HEAD` y el SDK lo añade a
`InformationalVersion`. Una suite ejecutada **antes** de crear el commit —que es el orden normal:
implementar, probar, commitear— produce binarios estampados con el SHA del **padre**, no con el del
commit que después captura ese árbol.

Es el mismo argumento que en Round 3 retiró la igualdad de árbol como frontera de reutilización,
aplicado ahora al canal local. Consecuencia operativa: **rellenar `sha` con el commit contenedor sería
inventar**. El instrumento solo acepta un SHA si la propia frase lo nombra, o si el texto declara
explícitamente que la corrida fue posterior al commit.

## 4. Clasificación

Cada ejecución cae en **exactamente una** categoría, con esta prioridad, para que ninguna entre en dos
numeradores:

1. **commit de merge** → `NEW_EVIDENCE`, sin excepción. SHA nuevo, estampado en los ensamblados.
   Decisión cerrada en V4; no se reabre aunque `tree(merge) == tree(segundo padre)`.
2. **norma citable vigente entonces** → `POLICY_REQUIRED`, con
   `wouldOtherwiseBeExactShaReconfirmation` como campo secundario.
3. **mismo SHA, misma clase, sin invalidador conocido** → `EXACT_SHA_RECONFIRMATION`.
4. **mismo SHA, misma clase, invalidación indeterminable** → `UNKNOWN`.
5. **mismo SHA, evidencia previa en otro canal** → `CROSS_CHANNEL`.
6. resto → `NEW_EVIDENCE`.

### Solo se reconfirma lo que antes quedó confirmado

Una ejecución anterior que terminó en rojo o cancelada **no estableció nada**. La siguiente sobre el
mismo SHA no reconfirma: persigue un fallo, y es la primera evidencia verde de su clase. Contarla como
reconfirmación la presentaría como ahorro disponible, y eliminarla significaría «no reintentar nunca
un CI fallido», que no es lo que propone la regla. El instrumento solo cuenta como precedente una
ejecución previa **exitosa**.

### Invalidadores, y por qué casi todo queda indeterminable

Los de V4, y solo esos. Sin procedencia registrada, `UNKNOWN`: **nunca se asume estabilidad**.

- **Local** — el invalidador es «árbol sucio al producir la evidencia». La historia **no lo registra
  nunca**. Indeterminable por construcción.
- **CI** — el flujo fija `dotnet-version: "8.0.x"`, un pin **flotante**: dos corridas del mismo SHA en
  fechas distintas pueden resolver parches de SDK distintos. Sin el SDK resuelto de ambas, el
  invalidador «SDK resuelto cambió» queda indeterminable. Solo interviene cuando **existe** una
  segunda ejecución sobre el mismo SHA.

### El denominador

`eligible` = ejecuciones sobre las que la pregunta «¿fue reconfirmación por SHA exacto?» **se puede
contestar**, en un sentido o en otro: SHA atestiguado determinable **y** clasificación resuelta.

Una primera y única ejecución sobre un SHA **sí** es elegible: se puede afirmar definitivamente que no
fue reconfirmación. **El denominador no son commits.**

## 5. Resultados

### 5.1 Agregado del corpus

```
Ejecuciones de evidencia          440
  NEW_EVIDENCE                    376
  CROSS_CHANNEL                    64
  EXACT_SHA_RECONFIRMATION          0
  POLICY_REQUIRED                   0
  UNKNOWN                           0

Elegibles para la regla de SHA exacto   160
Excluidas del denominador               280   (SHA local indeterminable)
Tasa de reconfirmación por SHA exacto   0.00
```

| Iniciativa | Ejecuciones | Nuevas | Cruce de canal | Reconfirmación | Elegibles |
|---|---:|---:|---:|---:|---:|
| I-40 | 60 | 54 | 6 | 0 | 18 |
| I-42 | 201 | 169 | 32 | 0 | 90 |
| I-43 | 142 | 119 | 23 | 0 | 40 |
| I-44 | 37 | 34 | 3 | 0 | 12 |

Por clase: CI 121, Core Full local 84, UI Full local 76, Build UI 32, Build Plugin 31, validación del
dueño 49, indeterminada 47.

### 5.2 Pares mismo-commit local → CI

| Iniciativa | Pares | Con Core local | Con UI local |
|---|---:|---:|---:|
| I-40 | 6 | 6 | 6 |
| I-42 | 32 | 32 | 30 |
| I-43 | 23 | 23 | 23 |
| I-44 | 3 | 3 | 1 |
| **Total** | **64** | **64** | **60** |

Se dicen **mismo commit**, no «mismo SHA»: por §3.2, el lado local de cada par no atestigua con
certeza el SHA de ese commit. Todos son `CROSS_CHANNEL` y **ninguno cuenta como ahorro disponible**.
Esto es entrada de G5, no de G6.

### 5.3 Push agrupado, y por qué no se propaga

26 commits del corpus **no tienen corrida propia**: llegaron en un push junto a un commit posterior,
y solo el tip recibe corrida. Su evidencia de CI es `NONE`, sin propagación retrospectiva.

La verificación independiente muestra que la regla no es formalismo. Los siete commits sin corrida de
I-43 son, **todos**, fases **rojas** de TDD seguidas inmediatamente de su verde y empujadas con él:

```
[17] 98b0407  ... guard de construccion y factory de la ventana, en rojo
[18] 493e604  ... la suite de UI deja de tocar los settings reales
[19] 0179a41  ... frontera pendiente/comprometido, en rojo
[20] 9884529  ... las cuatro cajas pasan a ser editores pendientes
```

Propagar hacia atrás el verde del tip habría afirmado que un commit rojo por diseño estaba verde.

### 5.4 El merge no siempre tiene CI

El merge de **I-40 (`bf327b3`) no tiene ninguna corrida** — verificado directamente contra la API
(`total_count = 0` para ese `head_sha`). El CI posterior al merge se clasifica `NEW_EVIDENCE` cuando
existe, pero **no es universal**.

I-43 tiene además un **merge interno de rama** (`d582dee`, `origin/main` dentro de la rama) que **sí**
recibió corrida. El instrumento no usa `--no-merges`: excluirlo perdería una ejecución real. Su cuerpo
de merge lo explica: la rama incorporó `main` «por merge, no por rebase».

### 5.5 Coste

**Canal local**, con la línea base de G1 **como proxy de orden de magnitud, no como cronometraje
histórico**:

| Clase | n | proxy | Total |
|---|---:|---:|---:|
| Core Full local | 84 | 92.15 s | 7 741 s |
| UI Full local | 76 | 273.30 s | 20 771 s |
| Build UI Debug | 32 | 2.97 s | 95 s |
| Build Plugin Debug | 31 | 4.83 s | 150 s |
| **Total documentado** | | | **28 756 s ≈ 7.99 h** |

**Eliminable por la regla de reutilización por SHA exacto, con lo que se puede determinar: 0
ejecuciones, 0 segundos.**

**Canal de CI**, con tiempos **reales** del propio workflow: 120 corridas del corpus, **23 252 s ≈
6.46 h**, mediana 198 s. Se excluye una corrida **cancelada** (`32984903755`) cuyo `updated_at` es
tres días posterior a su creación: `updated_at − created_at` no es su duración. Excluirla es la
diferencia entre 6.46 h y un total absurdo de 71.6 h.

Corroboración cruzada: Discovery reportó para I-43 «27 corridas, 1 h 40 m»; este método mide 28
corridas y 1.73 h. Concuerdan.

## 6. Norma vigente: qué obligaba de verdad

Reconstruida por commits de `AGENTS.md` y `docs/WORKFLOW.md`, y verificada **por hash de blob**, no
por fecha: en los cuatro merges del corpus el contenido de ambos archivos es **byte-idéntico** al de
`363608f` (2026-07-27). Los bloques de pruebas e integración no se tocan desde **2026-07-17**. El
siguiente cambio normativo es de 2026-09-07, **posterior a todo el corpus**.

**La norma es la misma en las cuatro fechas.** Ninguna diferencia de trato entre iniciativas puede
explicarse por evolución normativa.

Búsqueda literal de verbos de repetición (`repetir|de nuevo|volver a|otra vez|re-?ejecut|nuevamente`)
sobre ambos documentos vigentes: **cero coincidencias**.

- «Suite completa verde» y «`dotnet test` verde (todas las pruebas)» describen un **estado exigido**,
  sin decir cuántas veces ni en qué momentos. **No** es obligación de repetir.
- La **única** obligación citable que fuerza una corrida nueva es `WORKFLOW.md` §4.5 paso 2: «Esperar
  **CI verde sobre esos commits**» tras el rebase final. Ancla la corrida a los commits **rebasados**,
  que son **SHAs nuevos**: por tanto produce `NEW_EVIDENCE`, no reconfirmación.

Por eso `POLICY_REQUIRED = 0`. **No es que la norma no exista: es que la que existe no produce
repetición sobre el mismo SHA.** No se etiqueta nada como obligado por política solo porque solía
hacerse.

Queda una obligación citable cuyas ejecuciones **no son identificables**: el mismo paso exige «build
Debug local de UI y Plugin» en la sesión de integración. Ninguno de los cuatro cuerpos de merge
documenta esos builds, así que no se puede señalar qué ejecución concreta los cumple. Se declara aquí
en vez de imputarla a ciegas.

## 7. Controles falsadores

| Control | Veredicto | Qué se comprobó |
|---|---|---|
| **A** — mismo SHA con dos corridas | `PASS` | 5 casos en el repositorio, **ninguno en el corpus** |
| **B** — cambio de SHA entre dos Full | `PASS` | invariante + ejemplo real |
| **C** — local → CI del mismo commit | `PASS` | 64 casos, todos `CROSS_CHANNEL` |
| **D** — CI posterior al merge | `PASS` | todos `NEW_EVIDENCE` |

**Control A no existe en el corpus.** Los cinco casos del repositorio son de julio y de dos naturalezas
que el instrumento distingue:

- **3 re-pushes de la misma revisión bajo otra referencia** (`e47e81e`, `4e084d2`, `a6febd2`):
  `success → success`. Repiten sobre un verde previo, y quedan `UNKNOWN` por el pin flotante del SDK.
  Son los únicos candidatos históricos a reconfirmación por SHA exacto en toda la historia.
- **2 reruns auténticos** (`run_attempt > 1`): `29623ab` `cancelled → success` y `e36dcf0`
  `failure → failure`. Ninguno repite sobre un verde: son `NEW_EVIDENCE`.

> **En toda la historia de este repositorio, el CI nunca se ha vuelto a ejecutar sobre un SHA que ya
> estaba verde en la misma referencia.**

Control B se comprueba de dos maneras y **puede fallar de verdad**: (B1) toda reconfirmación debe
tener una ejecución estrictamente anterior con el mismo SHA y la misma clase; (B2) debe existir un par
real de la misma clase sobre SHAs distintos y ninguno de sus miembros puede estar etiquetado como
reconfirmación.

## 8. Comparación con Discovery

Discovery §9, sobre I-44 + I-40 + I-43: **146 ejecuciones de suite completa**, clasificadas
`NEW EVIDENCE 60 (41 %)`, `RECONFIRMATION 66 (45 %)`, `REQUIRED BY POLICY 18 (12 %)`, `UNKNOWN 2`.

**Los denominadores concuerdan.** Discovery contó Core Full 74 = 33 autodeclaradas + 41 en CI, y UI
Full 72 = 31 + 41. Este método, sobre las mismas tres iniciativas, encuentra 35 Core locales
documentadas, 33 UI locales y 43 corridas de CI. Las diferencias son de una o dos unidades, y en el
sentido esperado: la auditoría independiente detectó que la extracción **omite** afirmaciones.

**Los numeradores no concuerdan, y la razón es la taxonomía.**

| Discovery | V4 | Por qué |
|---|---|---|
| `RECONFIRMATION` 66 (45 %) | `CROSS_CHANNEL`, no reconfirmación | Discovery trataba «Full local y luego Full en CI sobre el mismo commit» como repetición. En V4 son **clases de evidencia no intercambiables**: local Debug ≠ CI Release, máquina del dueño ≠ runner. |
| `REQUIRED BY POLICY` 18 (12 %) | 0 | La norma vigente no contiene ningún verbo de repetición, y la única obligación citable ancla a SHAs **nuevos** (§6). |
| — | 280 excluidas del denominador | Discovery atribuía la evidencia local al commit que la describe. §3.2 muestra que ese SHA es indeterminable. |

**La aritmética cierra.** Sobre las tres iniciativas de Discovery este método encuentra **32 pares**
mismo-commit local→CI. En la descomposición de Discovery cada par vale dos ejecuciones (núcleo y UI):
32 × 2 = **64**, frente a sus **66** reconfirmaciones. Es decir: **el 45 % de reconfirmación de
Discovery es, bajo V4, prácticamente todo cruce de canal**, y el cruce de canal **nunca se cuenta como
ahorro disponible**.

Esto no refuta a Discovery en los hechos: reclasifica los mismos hechos con una taxonomía más estricta.
Discovery ya lo advertía al marcar sus corridas locales como `RECONSTRUCTED` y al dejar su
verificación independiente como pendiente (§ «lo que no se midió», punto 6).

## 9. Las preguntas de G3

**Q1 — ¿Cuánto de la repetición histórica es realmente Exact-SHA misma clase?**
**Cero, en el corpus.** 440 ejecuciones, 160 elegibles, 0 reconfirmaciones, tasa 0.00. En todo el
repositorio hay 3 candidatos —re-pushes de la misma revisión bajo otra referencia, todos de julio y
todos fuera del corpus— y quedan `UNKNOWN` porque el pin flotante del SDK impide descartar el
invalidador. El canal local no aporta ninguno **y no puede aportarlo**: su SHA es indeterminable.

**Q2 — ¿Cuánto es en realidad cruce de canal?**
**64 ejecuciones, el 14.5 % del total y el 40 % de las elegibles.** 64 pares con Core local, 60 con UI
local. Es el término que Discovery contaba como repetición.

**Q3 — ¿Cuánto estaba obligado por política?**
**Cero ejecuciones identificables.** La norma estuvo congelada durante todo el corpus y no contiene
ningún verbo de repetición. La única obligación citable —CI tras el rebase final— ancla a SHAs nuevos.
Queda declarada una obligación cuyas ejecuciones no son identificables: los dos builds Debug locales
de la sesión de integración.

**Q4 — ¿Hay evidencia histórica de más de una Full local sobre el mismo Candidate SHA?**
**`UNKNOWN`, y estructuralmente.** De las **160** afirmaciones documentadas de clase Full —84 de núcleo
y 76 de UI, 154 de canal local y 6 de canal indeterminado—, **ninguna nombra un SHA**. Ni una. Las
cuatro extracciones responden `false` a «¿algún texto dice que una suite completa se volvió a correr
sobre el mismo SHA?». La pregunta **no es contestable** desde el registro histórico, y ese es un
resultado válido: `LOCAL FULL PER CANDIDATE = UNKNOWN`.

No es una laguna de la extracción: es la forma del registro. Un cuerpo de commit dice «1016/1016» o
«suite completa verde», nunca «suite completa verde **sobre `abc1234`**». Y por §3.2, aunque lo
dijera, el SHA correcto no sería el del commit contenedor.

**Q5 — ¿El término de repetición suficientemente grande está en G5, en G6, en ambos o en ninguno?**
G3 entrega la evidencia y **no decide**:

- **El término medible es el cruce de canal (64), y pertenece a G5**, no a G6: no es repetición
  eliminable, es evidencia de clases distintas sobre el mismo commit.
- **G6 no tiene, en este corpus, ningún término que reducir.** Su mecanismo ataca la reconfirmación por
  SHA exacto, y la medida es 0 en el corpus y 3 `UNKNOWN` en toda la historia. Coincide con lo que
  ADR-0033 §5 ya advertía para el CI, y **lo extiende al canal local**, donde Round 3 esperaba que se
  concentrase el valor.
- Con lo medible, **el valor de G6 no está demostrado**. Lo que impide medirlo es la falta de
  procedencia local, exactamente lo que G1 empezó a capturar.

## 10. Incertidumbre declarada

1. **Las cuentas locales son cota INFERIOR.** La auditoría independiente encontró omisiones en tres de
   las cuatro extracciones (11 afirmaciones), y **ninguna alucinación** en las cuatro. Hay más
   ejecuciones locales de las contadas; no menos.
2. **Las rondas del dueño son cota SUPERIOR.** Los recuentos brutos (I-40 10, I-42 17, I-43 13, I-44 2)
   incluyen duplicados y recuentos que la auditoría señaló; I-40 son unas 6 rondas reales. No se
   deduplicaron a ojo.
3. **47 ejecuciones quedan con clase indeterminada.** El texto afirma que algo se ejecutó sin permitir
   decidir qué clase.
4. **Ningún Candidato se infirió.** Las cuatro auditorías devuelven `inferredCandidates` vacío. Un
   Candidato solo existe donde el texto lo declara con palabras.
5. **Una sola captura de CI.** La reconstrucción vale para la historia recuperada el día de la captura
   (600 corridas, `total_count` de la API = 600). El instrumento aborta si lo recuperado no cubre el
   total declarado: una conclusión sobre «SHAs con una sola corrida» derivada de una paginación
   incompleta sería un artefacto.
6. **La extracción documental la produjeron agentes.** Cada una fue auditada por un agente
   independiente que **eligió sus propias muestras** (7 a 14 commits por iniciativa) sin que el
   instrumento las seleccionara, y además se comprobaron a mano contra la API el merge sin CI de I-40 y
   los siete commits sin corrida de I-43.

## 11. Qué pueden y qué no pueden contestar G5 y G6

**Ya pueden contestarse:**

- El tamaño del cruce de canal local→CI: 64 ejecuciones, 32 pares en el subcorpus de Discovery.
- Que el CI nunca reconfirmó un verde sobre el mismo SHA en la misma referencia.
- Que el push agrupado deja 26 commits sin evidencia de CI, y que propagarla sería falso.
- Que el 45 % de reconfirmación de Discovery es, bajo V4, cruce de canal.

**No pueden contestarse con este registro histórico:**

- Cuántas veces se corrió una Full local sobre el **mismo** SHA (Q4).
- Si los 3 candidatos históricos de reconfirmación de CI lo eran de verdad: haría falta el SDK resuelto
  de cada corrida, y el pin es flotante.
- Cuánto costaron **de verdad** las corridas locales históricas: la línea base de G1 es un proxy de
  orden de magnitud, no un cronometraje de entonces.
- Qué ejecuciones concretas cumplieron la obligación de los dos builds Debug de la sesión de
  integración.

## 12. Reproducción

```powershell
pwsh -File eng/validation/measure-repetition.ps1 -ClaimsPath artifacts/validation/g3-claims.json -Controls
```

Sin `-ClaimsPath`, solo se reconstruye el canal de CI y los canales local y del dueño quedan **no
observados** — nunca en cero. Sin `-PolicyPath`, ninguna ejecución se clasifica `POLICY_REQUIRED`: la
costumbre no es norma.

Salida legible por máquina: `rackcad-validation-repetition/v1`, en `artifacts/validation/` (ignorado).
**Los resultados no se versionan**; se versiona el método. El inventario completo de las 440
ejecuciones vive en el JSON, no aquí.
