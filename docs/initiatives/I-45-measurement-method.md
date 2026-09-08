# I-45 — Método de medición reproducible

**Gate:** G1 · **Implementación ejecutable:** [`eng/validation/measure-validation.ps1`](../../eng/validation/measure-validation.ps1)

Este documento describe **cómo se mide** lo que cuesta validar un cambio en RackCad. La salida de G1
es el **método**, no una plataforma: no hay panel, no hay serie histórica, no hay base de datos y no
hay recolección automática. Un tercero con este repositorio, PowerShell 7 y el SDK de .NET debe poder
reproducir cualquier número que este método produzca.

## 0. Qué es y qué no es

Este método **mide**. No optimiza, no elimina obligaciones, no cambia la población de pruebas y no
aplica ninguna de las reglas que
[ADR-0033](../adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md) deja **propuestas**. En
particular, y de forma explícita:

- **no** aplica la regla de UI de la iteración ordinaria (LC-UI);
- **no** aplica la reutilización de evidencia por SHA exacto;
- **no** introduce métricas de proceso, ni umbrales de aceptación, ni objetivos de rendimiento.

Medir el coste de una obligación no es proponer quitarla. El ciclo local que se mide en §1.5 es el
que **hoy** exige `AGENTS.md`, entero.

## 1. Las cinco medidas

Todas se toman en `Debug`, que es la configuración que el dueño carga por `NETLOAD`. Cada medida
lleva su propio contador; ninguna se deriva de otra.

### 1.1 `CoreFullSeconds`

Reloj de pared de `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug`, **suite completa,
sin filtro**. Incluye el build incremental que `dotnet test` dispara, porque es lo que cuesta de
verdad invocarlo; `-NoBuild` permite medir la variante sin build y queda registrado en el JSON como
`includesBuild`. Las dos variantes son **medidas distintas** y no se comparan entre sí.

### 1.2 `UiFullSeconds`

Igual, sobre `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`. Es `net8.0-windows` y solo corre en
Windows.

### 1.3 `BuildUiDebugSeconds`

Reloj de pared de `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug`.

### 1.4 `BuildPluginDebugSeconds`

Reloj de pared de `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug`.

**Dependencia del entorno del dueño.** Si AutoCAD tiene cargado por `NETLOAD` el ensamblado que este
build escribe, MSBuild falla con el archivo en uso (`MSB3021` / `MSB3027`). Eso **no es una medición
mala: es una imposibilidad del entorno**, y el script la distingue de un fallo de compilación: marca
la iteración como `BLOQUEADA`, anota `blockedBy` y **no** la mezcla con las buenas. El bloqueo es
condicional, no automático: AutoCAD abierto no basta si el DLL cargado pertenece a otro árbol de
trabajo.

### 1.5 `MandatoryLocalCycleSeconds`

El **ciclo local obligatorio tal como es hoy**. `AGENTS.md`, «Pruebas — definición de terminado»:
el punto 1 exige la validación automatizada completa —suite Core **y** suite UI— y el punto 3 exige
el build Debug de UI y de Plugin con 0 errores. G1 **precede** a LC-UI: aquí no se descuenta nada.

Se mide como **un solo reloj de pared de la secuencia completa**, ejecutada en este orden:

```
build UI → build Plugin → Core Full → UI Full
```

y **no** como suma de las medidas 1.1–1.4 tomadas por separado. Las partes medidas por separado no
comparten el estado de compilación incremental, y su suma es un número que nadie vive.

Si algún paso falla o queda bloqueado, la iteración **no** se cuenta como ciclo: se reporta aparte,
como `statsLowerBound`, y se lee como **cota inferior** —lo que se llevaba gastado cuando el ciclo se
rompió—, nunca como el coste del ciclo.

### 1.6 `OrdinaryIterationLocalSeconds` (añadida en G5)

Lo que la norma exige **en local** antes del push en una **iteración ordinaria**, una vez aplicada
LC-UI:

```
build UI → build Plugin → Core Full
```

La suite de UI no está porque su evidencia intermedia la aporta el CI sobre el SHA exacto empujado.

**No es la misma magnitud que §1.5, y no se compara como «antes y después» de una optimización.** La
definición de `MandatoryLocalCycleSeconds` **no cambia** —se conserva intacta para poder seguir
reproduciendo la línea base histórica—; lo que cambia es *qué secuencia describe la norma vigente*.
Restar una de otra daría un número sin significado: son dos flujos de trabajo distintos, no dos
medidas del mismo.

**Esta métrica no incluye reloj de CI.** La preparación local y la realimentación del CI se reportan
**por separado** y no se suman: sumarlas presentaría como trabajo serial local algo que ocurre en otra
máquina y en paralelo con lo que la persona haga después.

## 2. Cómo se ejecuta

```powershell
pwsh -File eng/validation/measure-validation.ps1 -Measure all -N 3
```

| Parámetro | Para qué |
|---|---|
| `-Measure` | `all`, `none`, o cualquier combinación de `core`, `ui`, `build-ui`, `build-plugin`, `cycle`, `ordinary` |
| `-Repeat` (alias `-N`) | repeticiones solicitadas |
| `-NoBuild` | mide las suites sin el build incremental |
| `-Sessions <merge>` | reconstrucción de calendario (§6) |
| `-Ci <id\|latest>` | medición de una corrida de CI (§7) |
| `-SelfTest` | verifica las guardas de fallo-cerrado (§5) |
| `-OutputPath` | destino del JSON; por omisión, `artifacts/validation/` |
| `-DotnetPath` | `dotnet` explícito |

**Dependencias.** PowerShell 7 sobre Windows, `git`, y el SDK de .NET. Nada más: sin Python, sin
`jq`, sin módulos de PowerShell adicionales y sin paquetes nuevos en el repositorio. `gh` es
**opcional** y solo lo usa §7.

En este equipo el SDK vive a nivel de usuario (`%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`);
`Program Files` solo trae runtimes. El script lo resuelve solo, y registra en la procedencia **qué
ejecutable** acabó usando.

## 3. Repetición: mínimos, y por qué no se pueden bajar

| Medida | Mínimo |
|---|---:|
| `CoreFullSeconds`, `UiFullSeconds` | 3 |
| builds, `MandatoryLocalCycleSeconds` y `OrdinaryIterationLocalSeconds` | 2 |

Discovery midió una banda de **175.6 – 557.4 s** para la misma suite en la misma sesión: un ruido de
más de 2×. Una sola corrida no dice nada sobre una diferencia menor que eso, y **una línea base de
una sola corrida es peor que ninguna**, porque parece un número.

Si se pide menos que el mínimo, el script **sube** hasta el mínimo y lo declara en el JSON
(`repeatRequested`, `repeatMinimum`, `repeatEffective`, y un aviso en pantalla). No se puede producir
una línea base por debajo del mínimo ni por accidente ni en silencio.

Se reporta siempre `n / min / mediana / max`, más la media y `spreadRatio` (max ÷ min). **La mediana
es el resumen; el `spreadRatio` es el que dice si la mediana significa algo.** Una diferencia menor
que la dispersión observada no es una diferencia.

**La primera iteración de una tanda con build suele ser un valor atípico** —compila de verdad; las
siguientes encuentran el build al día—. Es coste real, no un artefacto: por eso se conservan todas
las muestras individuales en `samples` y no se descarta ninguna.

## 4. Procedencia: qué se registra, y qué invalida una medida

Cada ejecución registra, **automáticamente y sin intervención**: SHA de `HEAD` (completo y corto),
rama, asunto del último commit, si el árbol de trabajo estaba limpio y qué rutas estaban sucias,
descripción del sistema operativo y arquitectura, nombre de máquina, número de procesadores, versión
de PowerShell, ruta y versión de `dotnet`, lista de SDK instalados, marca de tiempo UTC y local, y si
AutoCAD estaba en ejecución.

**Un árbol sucio no invalida la medida, pero la desatribuye.** El número deja de pertenecer al SHA:
pertenece al SHA *más* unos cambios sin registrar. El script lo avisa en pantalla y lo deja en el
JSON (`workingTreeClean`, `dirtyPaths`). Una línea base que se quiera citar se toma con el árbol
limpio.

**Los SHA aparecen en la salida de ejecución, nunca pegados a mano en un documento normativo.** Este
documento, el ADR y el contrato no llevan hashes: quien quiera saber sobre qué SHA se midió, lee el
JSON.

## 5. Guardas de fallo-cerrado

El método falla **cerrado**: ante la duda, no produce un número.

1. **Cero pruebas observadas ⇒ FALLO.** `AGENTS.md` ya lo dice para las invocaciones filtradas
   («0 pruebas seleccionadas = FALLO»); aquí se aplica también a la corrida completa. Un cronómetro
   sobre cero pruebas mide el arranque del runner, no la validación.
2. **No se encuentra el resumen del runner ⇒ FALLO.** Sin resumen no se puede afirmar que corrió
   nada, y no afirmar es distinto de afirmar que sí.
3. **Código de salida distinto de cero ⇒ FALLO**, y se propaga al código de salida del script.
4. **SHA indeterminable ⇒ FALLO**, lanzado antes de medir nada.

El lector del resumen entiende las **dos localizaciones** en que este repositorio ve la salida de
VSTest, española (`Correctas! - Con error: 0, Superado: 4643, … Total: 4643`) e inglesa
(`Passed! - Failed: 0, Passed: 4643, … Total: 4643`). Un lector que solo entendiera inglés
degradaría a «no encontré resumen» en la máquina del dueño: fallaría cerrado, que es correcto, pero
sería inservible.

### Cómo se demuestran, sin romper nada

`-SelfTest` ejerce las guardas **sin filtrar una suite real, sin modificar ninguna prueba y sin
escribir en el árbol de trabajo**:

- la guarda de cero pruebas se ejerce contra las funciones **puras** `Get-TestOutcome` y
  `Test-OutcomeAdmissible` con salida sintética —el mismo código que juzga las corridas reales—,
  cubriendo diez casos: cero pruebas en ambos idiomas, los dos banners de «no hay pruebas», salida
  sin resumen, salida vacía, verde con código de salida distinto de cero, rojo declarado, y los dos
  verdes legítimos que **deben** admitirse;
- la propagación del código de salida se ejerce con un comando temporal controlado
  (`cmd /c exit 7`), fuera del repositorio;
- la guarda de procedencia se ejerce apuntando la resolución de repositorio a un directorio temporal
  recién creado.

> **`GIT_CEILING_DIRECTORIES` no es decoración.** En el equipo del dueño **el propio directorio de
> usuario es un repositorio git**, de modo que `%TEMP%` cae *dentro* de un repositorio y `git
> rev-parse --show-toplevel` resuelve un toplevel desde cualquier carpeta temporal. Sin ese techo, la
> comprobación pasaría por la razón equivocada. Cualquier herramienta futura que camine hacia arriba
> buscando la raíz del repositorio hereda este riesgo.

## 6. Reconstrucción de calendario — y lo que **no** significa

```powershell
pwsh -File eng/validation/measure-validation.ps1 -Measure none -Sessions <merge>
```

Rango `<merge>^1..<merge>^2` con `--no-merges`: exactamente los commits que ese merge incorporó, sin
el ruido de otras ramas. Se ordena y se agrupa por la marca de tiempo de **AUTOR**.

**Autor, no committer.** El committer lo reescribe cualquier rebase. El script detecta el desfase y,
si afecta a la mitad o más de los commits, marca `rebaseDetected` para que nadie use por error unas
marcas que son la hora del rebase y no la del trabajo.

Se agrupa con **seis umbrales** —0.5, 1, 2, 3, 6 y 12 h— y se reporta la sensibilidad entre el
resultado mínimo y el máximo. El umbral no se elige: se muestran todos, porque **el número depende
del umbral mucho más que del trabajo**.

### Lo que este resultado es

Un **resultado descriptivo de calendario**: cuántos bloques de commits próximos hubo, cuánto suman
sus tramos, cuál fue el mayor, y cuántos bloques son de un solo commit (span cero).

### Lo que este resultado **no** es

- **No** es tiempo de espera.
- **No** es trabajo humano activo.
- **No** es duración de sesión.
- **No** es coste, ni de agente ni de persona.

Un bloque de un solo commit tiene span cero y no significa que costara cero. Dos commits separados
por diez horas no significan diez horas de nada. La frase que este método autoriza es «los commits
de esta rama se reparten en N bloques de proximidad», y ninguna otra. El JSON lleva esta advertencia
incrustada en el campo `interpretation` para que viaje pegada al número.

## 7. Medición del CI

```powershell
pwsh -File eng/validation/measure-validation.ps1 -Measure none -Ci latest
```

Se reportan cuatro cosas, todas derivadas de marcas de tiempo que da la propia API:

| Magnitud | Definición |
|---|---|
| **reloj de pared de la corrida** | `updatedAt − createdAt` |
| **cola del runner** | `startedAt(corrida) − createdAt` |
| **duración por trabajo** | `completedAt − startedAt` de cada job |
| **desplazamiento de inicio por trabajo** | `startedAt(job) − createdAt(corrida)` |
| **trabajo crítico de cierre** | el job cuyo `completedAt` es el mayor |

El **trabajo crítico de cierre** es el que hay que mirar, y es por lo que este método lo calcula en
vez de dejarlo a la vista: el reloj de pared del CI es un `max()` sobre caminos que corren en
paralelo, no una suma. Acelerar un job que no es el crítico no mueve el reloj.

**Y el crítico no es necesariamente el más largo.** Un job con `needs:` no arranca hasta que
terminan sus dependencias, así que puede cerrar la corrida siendo mucho más corto que otro que
corrió en paralelo desde el principio. Por eso el crítico se calcula por `completedAt` y no por
duración.

> **«Desplazamiento de inicio» no es «cola».** Para un job con `needs:`, el hueco entre la creación
> de la corrida y su arranque mezcla la cola del runner con la espera por sus dependencias.
> Llamarlo cola sería falso y produciría el diagnóstico contrario al correcto: aparecería un
> problema de capacidad de runners donde solo hay una dependencia declarada. **La cola del runner
> solo es aislable a nivel de corrida.** Discovery dejó abierto (`UNKNOWN`) qué fracción del reloj
> se lleva la cola a lo largo del tiempo; este método la expone por corrida, pero **una corrida no
> es una serie** y no cierra ese `UNKNOWN`.

### CI sin `gh`

`gh` es opcional. Si no está —o está sin autenticar—, el script lo reporta (`available: false`) y
**no** inventa nada. El procedimiento equivalente, a mano:

1. Abrir la corrida en la interfaz de GitHub Actions.
2. `createdAt`, `startedAt` y `updatedAt` de la corrida, y `startedAt` / `completedAt` de cada job,
   están en `GET /repos/{owner}/{repo}/actions/runs/{id}` y en `.../runs/{id}/jobs`.
3. Aplicar las cuatro definiciones de la tabla.

Es el mismo cálculo; solo cambia de dónde salen las marcas.

## 8. Vocabulario de repetición (para G3)

G3 tendrá que contar repeticiones. Contarlas mal es la forma más fácil de reclamar un ahorro que no
existe, así que el vocabulario se fija **aquí**, antes de que haya un número que defender. Toda
ejecución se clasifica en **exactamente una** de estas cuatro categorías:

### 8.1 Ejecución de evidencia

Una corrida que produce evidencia **de una clase requerida** sobre un SHA que aún no la tenía. Es lo
que un Candidato necesita tener, una por clase. **No es repetición y no es eliminable.**

### 8.2 Reconfirmación por SHA exacto

Volver a ejecutar, sobre **el mismo SHA**, algo que ya se ejecutó sobre ese mismo SHA, sin que haya
mediado ningún invalidador de los de ADR-0033 §6. Cuesta tiempo y **no produce evidencia nueva**.

**Es la única categoría cuya eliminación puede reclamarse como ahorro**, y es exactamente lo que la
regla de reutilización por SHA exacto ataca. Su valor cuantitativo sigue sin medir: ADR-0033 §5 ya
advierte que en el CI la métrica está esencialmente satisfecha, y que el valor se concentra en el
canal local.

### 8.3 Cruce de canal

Ejecutar la misma suite en el **otro canal** —local ↔ CI—. **No es repetición.** Produce una clase de
evidencia distinta, y las clases no son intercambiables (ADR-0033 §4). Contar un cruce de canal como
repetición eliminable es la falacia central que V1 cometió y que V4 cerró: **nunca se cuenta como
ahorro.**

### 8.4 Candidato

Un SHA exacto con evidencia asociada, en el sentido de ADR-0033 §3. **Nunca se infiere.** Un
Candidato existe cuando el registro documental vivo de la iniciativa responde a las nueve preguntas
de esa sección; si falta una respuesta, no hay Candidato, y no se supone. Un SHA verde en el CI pero
sin evidencia de UI **no es un Candidato**, es un SHA sin evidencia de UI.

### Regla de conteo

Un conteo de repeticiones que no clasifique cada ejecución en una de estas cuatro categorías **no es
un conteo**: es una suma. Y una suma de ejecuciones de clases distintas no autoriza ninguna
conclusión sobre ahorro.

## 9. `OwnerActiveDurationMinutes` — definida, no operacionalizada

**Definición.** Minutos de atención activa que el dueño dedicó a **una ronda** de validación manual
en AutoCAD, **declarados explícitamente por él**.

**No** es la latencia entre el candidato y el veredicto. **No** se infiere de marcas de tiempo, ni de
commits, ni de mensajes, ni de nada. Discovery ya declaró el tiempo del dueño `UNKNOWN` en las cuatro
iniciativas examinadas, y lo declaró **estructuralmente inmedible** por reconstrucción: esa
conclusión sigue en pie y esta métrica no la contradice, precisamente porque **solo** admite un valor
dado a mano.

**G1 la define y ahí se detiene.** Cómo se pregunta, cuándo, dónde se registra y con qué criterio se
conserva o se retira el experimento lo operacionalizó **G4**, y su sede operativa es
[`docs/guias/validacion-manual-autocad.md`](../guias/validacion-manual-autocad.md) §8, que se sostiene
por sí sola. ADR-0033 §11 enuncia la misma métrica pero sigue en estado `propuesto`: vale como origen,
no como autoridad, y convendría alinearlo con la guía antes de aceptarlo. Este script **no** la mide,
**no** la pregunta y **no** la almacena.

## 10. Salida legible por máquina

Un JSON por ejecución en `artifacts/validation/`, más una copia en `measure-latest.json`.

```
schema        : "rackcad.validation-measurement"
schemaVersion : 2      (v2 en G5: cada métrica de secuencia publica `steps`,
                        y existe la clave OrdinaryIterationLocalSeconds)
gate, status ("OK" | "FAIL"), failures[]
invocation    : qué se pidió
provenance    : §4
measurements  : una entrada por medida, con stats y todas las muestras
sessions      : §6, con su campo `interpretation`
ci            : §7
selfTest      : §5
```

`schemaVersion` sube cuando cambia la forma; un consumidor que no reconozca la versión debe
detenerse, no adivinar.

**Los resultados no se versionan.** `artifacts/` está en `.gitignore`. Lo que se versiona es el
**método**; los números se vuelven a producir ejecutándolo. Un número pegado a mano en un documento
no se puede auditar y envejece sin avisar — que es exactamente lo que Discovery reprochó a sus
propias mediciones, que vivían en un directorio temporal de sesión.

## 11. Comparación con Discovery

Discovery midió con guiones de sesión no versionados y lo dijo. Este método **no lo contradice**: lo
reproduce y lo extiende.

| Magnitud | Discovery | Este método |
|---|---|---|
| Núcleo completo | 3 corridas, mediana 80.7 s | `CoreFullSeconds`, mismo procedimiento, ahora reproducible |
| Suite de UI | banda 175.6 – 557.4 s | `UiFullSeconds`, con `spreadRatio` explícito |
| Ciclo local obligatorio | **`UNKNOWN`** (§ «lo que no se midió», punto 3) | `MandatoryLocalCycleSeconds` |
| Latencia de CI | 146 – 272 s por corrida | `-Ci`, además con cola y trabajo crítico |
| Latencia de ciclo por iniciativa | tabla de calendario | `-Sessions`, con seis umbrales y sensibilidad |
| Tiempo activo del dueño | `UNKNOWN`, estructuralmente inmedible | §9: definida, **no** medida |

**Dónde difieren los números de calendario, y por qué.** No miden lo mismo, y la diferencia tiene un
signo predecible:

- La **latencia de ciclo** de Discovery se leyó del registro documental de la iniciativa: va del
  primer candidato presentado al veredicto final. Es un **sub-intervalo** de la vida de la rama,
  porque una rama empieza a acumular commits antes de que exista candidato alguno.
- El **calendario** de este método va del primer al último commit de **autor** del rango del merge,
  es decir, la rama entera.

De ahí que lo esperable sea **calendario ≥ latencia de ciclo** sobre la misma iniciativa, y que la
diferencia crezca con lo que la rama trabajó antes de su primer candidato. Una diferencia con ese
signo no es una contradicción; una con el signo contrario sí lo sería, y habría que investigarla.

**Regla de precedencia.** Donde este método y Discovery discrepen sobre la **misma** magnitud, gana
este método —es reproducible por un tercero y Discovery no lo era—, pero **solo si explica la
diferencia**. Una discrepancia sin explicación no la gana nadie: se anota como abierta.

## 12. Limitaciones conocidas

1. **Una máquina, un sistema operativo.** Todo lo local se midió en el equipo del dueño (Windows, 8
   procesadores). Ninguno de estos números vale como predicción para otra máquina, y el CI corre en
   hardware distinto: sus números y los locales **no se comparan**.
2. **El ruido puede ser mayor que cualquier efecto que se quiera medir.** Con `spreadRatio` por
   encima de 2, ninguna diferencia menor que eso es observable con estas repeticiones.
3. **`BuildPluginDebugSeconds` depende del entorno del dueño** (§1.4).
4. **La reconstrucción de calendario no observa el trabajo**, solo commits (§6).
5. **No hay serie histórica.** Cada ejecución es independiente; comparar dos exige conservar los dos
   JSON a mano. Construir la serie sería una plataforma, y G1 no es eso.
6. **El método no se auto-verifica en el CI.** `-SelfTest` es una comprobación que se pide a mano.
   Automatizarla tocaría `.github/**`, que G1 tiene prohibido.
