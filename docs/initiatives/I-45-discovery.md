# I-45 — Discovery: evidencia

> **Esto es EVIDENCIA, no la Proposal.** No decide tiers, ni modelo de riesgo, ni taxonomia, ni Quick
> CI, ni politica de cobertura, ni Candidate SHA, ni que hacer con el hilo STA. Registra lo que se
> midio, lo que se reconstruyo, lo que quedo refutado y lo que sigue sin saberse.
>
> Contrato: [I-45-test-validation-workflow.md](I-45-test-validation-workflow.md). Fase DISCOVERY.
> **NO IMPLEMENTATION BEFORE CONSENSUS.**

## Marcadores

| Marcador | Significado |
|---|---|
| `MEASURED` | Medido directamente por un comando cuya salida se cita. |
| `RECONSTRUCTED` | Derivado de evidencia indirecta: cuerpos de commit, documentos, timestamps. Una autodeclaracion en un commit **no** es una medicion. |
| `REFUTED` | Contradice una afirmacion anterior de este mismo Discovery. Se conservan las dos. |
| `UNKNOWN` | La evidencia disponible no permite responder. Es una respuesta valida. |
| `HYPOTHESIS` | Explicacion plausible sin evidencia suficiente. |

---

## 1. Alcance y metodo

Discovery cubrio cuatro lineas: **D1** baseline del ciclo de validacion del Owner, **D2** precision y
falsos positivos de una seleccion por impacto, **D3** contraindicaciones de una futura estrategia
multi-STA, **D4** repeticion historica de suites completas. Cada linea la produjo un agente y la
verifico un **refutador independiente** que re-ejecuto los comandos y audito los marcadores; un
critico final reviso las cuatro.

Fuentes: historial de git, `docs/automation/{evidence,state,decisions}`, `docs/initiatives`, la API de
GitHub Actions (`gh run list`, `gh api .../runs/<id>/jobs`, logs de job) y mediciones locales
cronometradas. Prohibido durante la fase: escribir en el repositorio, y `dotnet build/test/restore`
salvo como **medicion declarada**.

**Limite metodologico que atraviesa todo el documento.** Los tiempos locales se midieron en una
sesion, en una sola maquina (8 nucleos, restore caliente), y **no estan versionados**: viven en un
directorio temporal de sesion. No son auditables por un tercero y no son reproducibles contra si
mismos (§2). Ese es el defecto mas serio de esta evidencia y se declara aqui, no en una nota al pie.

**Tension normativa declarada.** `WORKFLOW.md` §8 reserva conteos de pruebas y hashes de commit a
`HANDOFF.md` §12. Un documento de evidencia sin identificadores no es auditable, y los archivos de
`docs/automation/evidence/` ya los llevan. Aqui se usan **identificadores de corrida de CI** siempre
que sea posible y SHA solo cuando el SHA **es** el objeto de la evidencia. La regla no se cambia:
se declara el conflicto.

---

## 2. Baseline local

`MEASURED`, con la reserva del §1. Mediana de tres corridas donde hubo tres; `--no-build` salvo nota.

| Operacion | Reloj | Nota |
|---|---|---|
| Nucleo completo, sin cobertura | 77.9 / 85.0 / 80.7 s | mediana 80.7 s |
| Nucleo completo, **con** cobertura | 123.8 / 116.4 s | +49 % sobre la mediana |
| Nucleo sin las 5 clases mas caras | 80.2 / 84.2 s | ver §5 |
| Nucleo **solo** esas 5 clases | 40.8 / 47.5 / 42.6 s | 202 pruebas |
| UI completo | 175.6 / 268.0 s | descartada una tercera de 557 s por contaminacion |
| UI con `xUnit.MaxParallelThreads=1` | 151.5 s | n=1 |
| UI con `xUnit.MaxParallelThreads=4` | 199.6 s | n=1 |
| Focal `~Selective` (nucleo) | 3.3 s | 543 pruebas |
| Focal `~PushBack` (nucleo) | 48.9 s | 1 929 pruebas |
| Focal `~Selective` (UI) | 7.3 s | 173 pruebas |
| Guardas de codigo fuente | 8.5 s | 234 pruebas |
| Build incremental de la suite nucleo | 6.4 – 8.3 s | en caliente |
| Build `src/RackCad.UI` (= job del CI) | 5.1 s | en caliente |

**`REFUTED` — la reproducibilidad de estos numeros.** Una primera tanda de la MISMA operacion sobre
el MISMO arbol dio 296.0 / 416.1 / 217.9 s: dispersion interna de **1.9x**, y un desplazamiento de
**3.7x** entre esa tanda y la limpia, causado solo por el estado de la maquina. La causa esta
identificada —la primera tanda corrio a la vez que quince agentes barriendo el repositorio— y la
tanda contaminada se descarto entera. La leccion se conserva: **cualquier efecto por debajo del
factor 2 es indistinguible del ruido de esta instrumentacion.**

---

## 3. Baseline de CI

`MEASURED` via API de Actions, 40 corridas recientes: reloj de pared **mediana 220 s**, media 228 s,
min 194 s, max 498 s. Duracion media por job: `UI Tests` 212.9 s, `Tests (Domain + Application)`
111.8 s, `Build Plugin` 78.2 s, `Build UI` 48.3 s.

Por rama (`run_started_at` → `updated_at`): I-44 5 corridas / mediana 223 s; I-32 35 / 167 s;
I-37D 26 / 171 s; I-43 27 / 220 s.

### 3.1 El camino critico NO es el que se creia

**`REFUTED`.** Este Discovery afirmo antes que `UI Tests` es el job mas largo en **40 de 40** y por
tanto gobierna ~95 % del reloj de pared. El primer hecho sobrevive; **la inferencia no**. El reloj es
un `max()` sobre **dos caminos casi iguales**, y el segundo nunca se midio:

```
camino A:  UI Tests                          mediana 214 s
camino B:  Tests -> Build Plugin (needs)     mediana 194 s
margen mediano entre ambos: 16 s   (min 1 s, max 60 s)
```

Sobre las 25 corridas mas recientes, `UI Tests` termina el ultimo en **17 de 25**; la cadena
`tests → build-plugin` gobierna en **8 de 25**. Consecuencia directa: **eliminar entero el job de UI
moveria la mediana de 215 s a 194 s — una cota superior de ~10 % del reloj**, no del 95 %. El embudo
STA no puede venderse como una intervencion sobre el reloj de pared del CI.

Corolario `MEASURED`: `build-plugin` **no ejecuta ninguna prueba** y esta detras de
`needs: [tests, build-ui]`. Por eso cada segundo ahorrado en la suite del nucleo **si** cae en el
camino critico, y cada segundo ahorrado en la de UI solo cuenta cuando UI gobierna.

### 3.2 Terminos que nadie habia nombrado

- **`MEASURED` — restore + build dentro del paso de prueba: ~46-47 s por job.** En una corrida, el
  paso del nucleo duro 99 s con `Duration: 53 s` reportada → ~46 s son restore+build; el de UI, 204 s
  con 157 s reportados → ~47 s. Ese termino es **comparable o mayor que la ejecucion entera del
  nucleo** (37 s en otra corrida) y **ninguna estrategia de seleccion de pruebas puede tocarlo**.
- **`MEASURED` — el tiempo de ejecucion varia ~2x sobre poblaciones identicas.** Las mismas 4 643
  pruebas del nucleo: `Duration: 37 s` en una corrida y `1 m 15 s` en otra.
- **`MEASURED` — la cola del runner es una cola larga no modelada.** Una corrida de 498 s contiene
  **304 s de espera pura** de un runner de Windows entre el fin del nucleo y el arranque de
  `build-plugin`. En las otras 24 corridas de la muestra ese hueco es de 0-21 s.
- **`MEASURED` — el CI no corre en cada commit.** La rama de I-43 tiene 34 commits propios y 27
  corridas: los siete commits «en rojo» se empujaron agrupados con su commit verde y **ninguno tiene
  corrida propia**. «N corridas» no es proxy de «N commits verificados».
- **`MEASURED` — no toda corrida ejecuta los cuatro jobs.** En las dos corridas rojas de I-44,
  `build-plugin` quedo `skipped` por el `needs`. La formulacion correcta es: *toda corrida verde*
  ejecuta los cuatro.

---

## 4. UI / STA

`MEASURED` sobre `tests/RackCad.UI.Tests/StaTestRunner.cs`: un `Dispatcher` **estatico** sobre **un**
hilo STA creado una sola vez, con `Invoke` **sincrono**. Es el **unico campo static mutable** de toda
la suite de UI. Hay **957** llamadas a `StaTestRunner.Run` en **97 de 121** archivos, **400**
construcciones `new *Window`, y **una sola** referencia a `Application.Current` y **una sola**
construccion de `System.Windows.Application` en todo el repositorio, ambas en ese archivo.

### 4.1 El embudo existe; su magnitud NO es la que se dijo

**`REFUTED`.** Este Discovery afirmo que las pruebas que pasan por el STA cuestan ~400x mas que las
que no (1 268 ms frente a 3 ms de media). El cociente se reproduce —STA 1 053 pruebas, media
3 006.9 ms; no-STA 177 pruebas, media 6.3 ms, es decir 477x— **pero no es un cociente de trabajo
contra trabajo**: es de **duraciones infladas por espera** contra duraciones reales.

```
suma de duraciones por prueba : 3 167.3 s
reloj de pared de esa corrida :   403.7 s
factor de inflacion           :     7.8x   ->  ~87 % de la duracion reportada
                                             de cada prueba STA es BLOQUEO, no ejecucion
```

Una mediana de 2.570 s y un maximo de 10.5 s **por prueba** no son costes plausibles de construir una
ventana WPF. **La direccion del hallazgo sobrevive: un solo hilo es un embudo. La magnitud no.** Y la
cantidad que de verdad decide si multi-STA recuperaria algo —**el trabajo serializado real sobre ese
hilo**— sigue siendo `UNKNOWN`.

### 4.2 Construir la ventana no es lo caro

`MEASURED`: dentro del grupo STA, las pruebas que construyen una ventana promedian 1 246 ms y las que
**no** la construyen 1 319 ms. La construccion de ventanas WPF **no** discrimina el coste; pasar por
el embudo, si.

### 4.3 El experimento de paralelismo, con su reserva

`MEASURED`: 1 hilo → 151.5 s; 4 hilos → 199.6 s; por defecto → 175.6 / 268.0 s. Conteos identicos en
las tres. **Reserva `REFUTED` parcial:** es **n=1 contra n=1**, y el control (`ui-full`) abarca en la
misma sesion 175.6 – 557.4 s, una banda mas ancha que el efecto reclamado. La columna
`cpu_load_before` registro el factor de confusion (valores 7 a 66) y las conclusiones no se
estratificaron por el. **Es una senal, no una medicion concluyente.**

---

## 5. Nucleo

`MEASURED`: 4 643 pruebas, suma de duraciones 398.3 s, reloj 77.3 s, paralelismo efectivo 5.2x sobre
8 nucleos. Cinco clases —todas de Push Back: `PushBackDiverterLowAuthorityTests`,
`PushBackBootSidesTests`, `PushBackBootSemanticsTests`, `PushBackDiverterPlanTests`,
`PushBackBootIdentityTests`— concentran el 48.8 % de la suma.

**`REFUTED` en su formulacion, no en su fondo.** Se afirmo que «quitar las 5 clases mas caras no
cambia el reloj, luego no hay clases calientes». Dos objeciones validas:

1. Es **n=2 contra n=3 con bandas solapadas** (80.2 / 84.2 frente a 77.9 / 85.0 / 80.7).
2. La lectura correcta es la contraria y mas fuerte: esas 5 clases **solas** tardan 40.8 – 47.5 s,
   es decir ~54 % del reloj de ~81 s de la suite, **y aun asi quitarlas no libera nada**. Eso es la
   firma de un **poste largo enmascarado** en una planificacion paralela —un argumento de Amdahl—,
   no la prueba de que esas clases sean baratas.

Sintoma colateral `MEASURED` del artefacto de medicion: al quitar 202 pruebas, la **suma** de
duraciones **subio** de 398.3 s a 490.3 s, y `NamespaceFolderGuardTests` —las mismas 7 pruebas— paso
de 8.1 s a 34.0 s. Las duraciones del TRX incluyen espera de CPU.

---

## 6. Obligaciones normativas

`MEASURED`, con cita y `archivo:linea`:

- [`AGENTS.md`](../../AGENTS.md) §«Pruebas — definicion de terminado»: «`dotnet test` verde (todas las
  pruebas, no solo las nuevas)» y build de UI + Plugin en Debug.
- [`WORKFLOW.md`](../WORKFLOW.md) §10: «Y por encima de todos: el estado real del repo (`git log`,
  `dotnet test`)» — la suite no es una compuerta, es **autoridad epistemica**.
- `WORKFLOW.md` §5: checklist de cierre con suite completa y CI verde.
- `ci.yml`: `on: push` sin filtro de rama, cuatro jobs, sin `--filter` en ninguna linea.
- **34 de 49** contratos de iniciativa reinscriben `dotnet test` a mano, porque
  [`TEMPLATE.md`](TEMPLATE.md) no lo prescribe.

### 6.1 Hallazgo que reduce el tamano del problema

**`MEASURED`.** `AGENTS.md` acota la regla a **«Un cambio de COMPORTAMIENTO esta terminado
cuando…»**. El disparador es un cambio de comportamiento, **no cualquier commit**. D4 midio commits
de I-43 que pagaron el nucleo completo sobre diffs confinados a UI o a pruebas. Parte del coste puede
ser **sobre-cumplimiento de una regla que no lo exige**, y esa parte **no necesita consenso para
corregirse**: solo aplicar la regla como esta escrita. Separar los dos terminos es tarea de la
Proposal.

### 6.2 Contradiccion normativa registrada, NO resuelta

`WORKFLOW.md` §2 exige que una iniciativa **tenga fila en ROADMAP antes** de abrir su rama.
`WORKFLOW.md` §8 dice que ROADMAP **se edita solo al integrar**. Tomadas juntas al pie de la letra,
registrar una iniciativa nueva es imposible. La practica del repositorio resuelve el circulo de
hecho: el dueno anade la fila. **Cambiar `WORKFLOW.md` es diseno de proceso y pertenece a la
Proposal, que exige consenso.** Aqui solo se registra.

### 6.3 Dos derivas documentales

- **`MEASURED` — la regla fantasma del filtro a cero.** Cuatro sitios atribuyen a `AGENTS.md` la
  regla de que «un filtro que coincide con cero pruebas no avisa»: `I-23-namespaces-sistemas.md:177`,
  `I-37D-seam-audit.md:574`, `.editorconfig:58` y el XML-doc de
  `tests/RackCad.Tests/NamespaceFolderGuardTests.cs:47`. `AGENTS.md` tiene 134 lineas y la palabra
  «filtro» **no aparece ninguna vez**. Uno de los cuatro sitios es una prueba que el CI ejecuta.
- **`MEASURED` — el «&lt;2 s».** [`AGENTS.md:31`](../../AGENTS.md) anuncia pruebas «rapidas, &lt;2 s».
  El dato viene de `ADR-0002`, que medía 627 pruebas y distinguia «ejecucion 570 ms» de «total
  13.6 s»: se copio el primero y se descarto el segundo. La linea nunca se ha editado.

---

## 7. Seleccion por impacto

### 7.1 No hay eje sobre el que operar

`MEASURED`: **0** `[Trait]` sobre 3 708 atributos del nucleo y 1 182 de UI; **0** `IClassFixture`;
**2** `[CollectionDefinition]`, ambas con `DisableParallelization = true` y ambas existentes para
**proteger un estatico de proceso**, no para seleccionar. Ni `xunit.runner.json` ni `.runsettings` en
todo el repositorio. Las dos suites son planas: 304 archivos en `namespace RackCad.Tests` y 121 en
`namespace RackCad.UI.Tests`. **Hoy `fqn-system` y `path-only` son literalmente la misma heuristica
de subcadena.**

### 7.2 Comparacion de las seis estrategias

Amplitud sobre la suite del nucleo en el momento de cada cambio (`MEASURED` en **metodos
declarados**; ver la reserva de unidad abajo):

| Estrategia | I-44 | I-43 | I-40 | Observacion |
|---|---:|---:|---:|---|
| `explicit-metadata` (hipotetica) | 0.6 – 2.4 % | — | — | el esquema **no existe**: todo es `HYPOTHESIS` |
| `dependency-graph` | 3.7 – 29.0 % | 6.7 % | **44.3 %** | ver inversion (1) |
| `fqn-system` | **41.0 %** | 13.0 % | 14.6 % | Push Back es el 41 % de la suite |
| `path-only` | 41.0 % | 13.0 % | 20.0 % | identica a la anterior hoy |
| `project-reference-graph` | 100 % | 100 % | 100 % | ver inversion (3) |
| `full` | 100 % | 100 % | 100 % | — |

**Tres inversiones contraintuitivas, todas `MEASURED`:**

1. `dependency-graph`, teoricamente la mas precisa, produce en I-40 la seleccion **mas amplia** de
   todas (44.3 % del nucleo), porque toca tipos arquitectonicamente profundos. Su precision la fija
   la profundidad del tipo tocado, no la disciplina del selector.
2. En I-43 `dependency-graph` es a la vez la **mejor** para el nucleo (6.7 %) y la **peor** para la UI
   (31.2 %) — y la UI es el camino mas caro, asi que empeora el reloj justo donde parece ganar.
3. `project-reference-graph` es **indistinguible de `full` en el 79 %** de los commits con `src`
   (108 de 137 tocan Application o Domain), y donde si recorta —29 commits solo-UI— **recorta mal**:
   se saltaria los 14 archivos del nucleo que verifican `src/RackCad.UI` y `src/RackCad.Plugin`
   **leyendo su texto** (204 metodos declarados), invisibles para todo grafo de compilacion porque
   **ningun `csproj` de pruebas referencia el Plugin**.

**La forma real de los cambios rompe la premisa.** `MEASURED` sobre los ultimos 200 commits: **40
(20 %)** tienen diff de `src` que abarca **mas de una** carpeta `Systems/<X>`; **39 (19.5 %)** tocan
`src` **fuera** de toda carpeta `Systems/<X>`.

### 7.3 El artefacto de cobertura: lo que puede y lo que no

`MEASURED`, descargado e inspeccionado. Es **un** XML de 8.4 MB con **un solo** `<coverage>`, 3
`<package>` (Application, Domain, Import), 641 `<class>`, 75 286 `<line hits>`.

**Puede** decir que lineas y ramas ejecuto la suite del nucleo **en conjunto**, y senalar zonas
muertas. **No puede**, y no debe afirmarse:

1. **Ningun mapa prueba → codigo.** `grep -c "RackCad.Tests|testId|test-name|TestCase"` sobre el XML
   devuelve **0**. El formato Cobertura no tiene ese eje y el agregado por corrida lo eliminaria
   igualmente.
2. **Cero cobertura de `src/RackCad.UI` y `src/RackCad.Plugin`.** El job `ui-tests` corre **sin
   `--collect`**. La suite mas cara no tiene informacion de cobertura de ninguna granularidad.
3. **No demuestra deteccion.** Prueba decisiva `MEASURED`: en el artefacto **previo** al arreglo de
   I-44, la clase defectuosa tenia `line-rate=0.9381` y **las lineas exactas del defecto estaban
   ejecutadas decenas de miles de veces** (una de ellas 108 981 hits) con la suite entera en verde.
   **La cobertura mide ejecucion, no asercion.**

> El contrato de I-45 §3 prometia «el mapa empirico prueba → codigo derivado de la cobertura que el
> CI **ya publica**». Ese entregable **no es derivable** de esa fuente. Queda registrado como
> `REFUTED` contra el propio contrato.

Ademas, `MEASURED`: el artefacto **caduca** (`retention-days: 14`; el inspeccionado expira el
2026-09-16), y proviene de `main`, no de ninguna de las iniciativas analizadas.

### 7.4 El sesgo del retrospectivo, ahora medido

`MEASURED`, y es lo que descoloca toda la comparacion: **el recall de la cobertura preexistente es
CERO en los tres cambios**, `full` incluida.

- I-44: la corrida roja da 7 fallos, **todos** en un archivo creado por el mismo commit. El commit de
  arreglo **no modifica ni un archivo de prueba preexistente**.
- I-43: el commit rojo anade un archivo de prueba y **cero** de `src`; su padre tenia CI `success`
  con los tres defectos dentro.
- I-40: 11 commits, 9 con corrida, las **9 `success`**; ningun rojo en toda la iniciativa, pese a
  cinco rechazos del Owner.

Caso limite `MEASURED`: un defecto de vocabulario entro el 2026-08-30 en un commit rotulado de Push
Back que modifico ademas cuatro archivos de Selectivo, con CI `success`, y **sobrevivio 5.05 dias con
CI verde en cada push** hasta que lo encontro una revision humana.

---

## 8. Owner Validation

### 8.1 Las tres cosas que se confundian

| Magnitud | Estado |
|---|---|
| **CI / test latency** | `MEASURED` — 146 – 272 s por corrida en las cuatro ramas muestreadas (con una cola de 498 s en `main`, §3.2). |
| **validation-cycle elapsed latency** | `MEASURED` de punta a punta — ver tabla. |
| **Owner hands-on time** | **`UNKNOWN` en las cuatro iniciativas, sin excepcion.** |

| Iniciativa | Rondas | Latencia de ciclo | CI de la rama | CI / latencia |
|---|---:|---|---|---:|
| I-44 (pequena) | 1 | ≤ 1 h 03 m | 5 corridas | — |
| I-32 (mediana) | 5 | **26 h 14 m** | 35 corridas, 1 h 39 m | 6.3 % |
| I-43 (grande) | 2 en AutoCAD | **2 d 02 h 46 m** | 27 corridas, 1 h 40 m | 3.3 % |
| I-37D (multirronda) | 7 paquetes | **4 d 01 h 21 m** | 26 corridas, 1 h 15 m | 1.3 % |
| **I-42** (`REFUTED`: es el extremo) | **8 rechazadas + 1** | **8 d 22 h 42 m** | 77 corridas | — |

**El CI no es el cuello del ciclo de validacion:** vale entre el 1.3 % y el 6.3 % de la latencia.

**`REFUTED` — la muestra excluyo su propio maximo.** D1 presento I-37D como el caso extremo. **I-42
lo dobla**: 8 candidatos rechazados, ciclo de 8 d 22 h 42 m, contrato de 3 383 lineas con cinco
secciones de veredicto del dueno y un checklist de 73 puntos. Es el registro de validacion mas rico
del repositorio y **no se examino**.

**`REFUTED` — mas rondas no implica mas latencia.** I-40 encadeno **seis** rondas en **3 h 40 m** de
ciclo completo, frente a las cinco de I-32 en 26 h. La variable dominante **no** es el numero de
rondas.

### 8.2 Por que `Owner hands-on time` es estructuralmente inmedible

`MEASURED`: `grep -niE 'duracion|tiempo|minutos|hora'` sobre `docs/guias/validacion-manual-autocad.md`
devuelve **cero lineas**, y el bloque de evidencia obligatorio de su §7 tiene campos para fecha,
validador, commit, ruta del DLL, SHA-256, version de AutoCAD y resultados — **y ninguno para tiempo
transcurrido**. El proceso, por diseno, nunca capturo el dato. Ningun timestamp de git o de Actions
puede sustituirlo: un candidato entregado el viernes y aprobado el lunes son **2 d 16 h de latencia** y
**cero minutos demostrados de trabajo**.

### 8.3 La carga que si es medible

- `MEASURED` — I-37D redacto **327 filas** de checklist repartidas en 7 paquetes (58 + 67 + 36 + 26 +
  31 + 35 + 74), **ninguna reutilizada** entre rondas.
- `MEASURED` — **cero de esas 327 filas** tiene marca, en los siete paquetes, incluidos aquel cuyo
  veredicto global es RECHAZADA y aquel cuyo veredicto es APROBADA. Lo unico resuelto es la casilla
  de veredicto **global**, en 2 de 7.
- **`MEASURED` y correctivo:** el formato de la guia **ya obliga** a un campo `Resultado por punto:`,
  y `grep -rn 'Resultado por punto' docs/automation/evidence/` lo encuentra en **cero de los 46**
  archivos de evidencia. **La carencia es de cumplimiento de un campo existente, no de formato.** Una
  propuesta que ofrezca «anadir registro por fila» estaria reinventando algo que el repositorio ya
  exige e ignora.
- `MEASURED` — I-11 es la **unica** iniciativa con una matriz resuelta fila a fila (16 filas, todas
  PASS con evidencia por fila). El formato por fila **existio y se abandono**.
- `MEASURED` — la guia §2 obliga a repetir **tres comandos locales antes de CADA carga por NETLOAD**
  (`dotnet test` del nucleo, build de UI, build del Plugin) y a cerrar AutoCAD antes de cada rebuild.
  La suite de UI no la pide la guia: la piden los contratos.
- `MEASURED` — factor de repeticion de CI por carga manual: I-32 **35 corridas para 5 cargas** (7:1);
  I-37D **26 para 7** (3.7:1).

### 8.4 La autodeclaracion falla, y esta demostrado

**`REFUTED`.** `HANDOFF.md:2318` declara para la baseline de I-44 «Debug de UI (**0 advertencias**, 0
errores)». El log del job `Build UI` de la corrida de ese mismo candidato dice **`Build succeeded.` +
`2 Warning(s)`**: dos `CS0105` por `using` duplicados en
`src/RackCad.Application/Systems/PushBack/PushBackPlanComposer.cs`, lineas 8 y 10, verificables con
`git show`. `MEASURED`: el defecto **no lo introdujo I-44** —entro durante I-42— y **sigue vivo en la
punta actual**. La baseline de I-42 decia solo «0 errores»; fue la de I-44 la que **escalo** a «0
advertencias». No es un error heredado: es una afirmacion nueva y falsa.

> Este defecto **no se corrige aqui**: tocar `src/` esta fuera del gate. Queda registrado.

---

## 9. Repeticion historica de Full

`RECONSTRUCTED` para las corridas locales (autodeclaracion en cuerpos de commit), `MEASURED` para el
CI. Sobre I-44 + I-40 + I-43, **49 commits sin merge**:

```
Ejecuciones de suite COMPLETA contabilizadas : 146     (~3 por commit)
  Full de nucleo : 74   = 33 autodeclaradas + 41 en CI
  Full de UI     : 72   = 31 autodeclaradas + 41 en CI
  Build UI       : 68     Build Plugin : 66
```

Clasificacion: **NEW EVIDENCE 60 (41 %)**, RECONFIRMATION 66 (45 %), REQUIRED BY POLICY 18 (12 %),
UNKNOWN 2. El 41 % mide **cobertura de riesgo** (el diff podia romperlas), **no rendimiento**.

### 9.1 El dato duro

> `MEASURED` — **numero de suites completas que encontraron un defecto no previsto en estas tres
> iniciativas: CERO.** Los unicos cuatro rojos de suite completa observados fueron los declarados a
> proposito por el propio autor en los gates de reproduccion de I-44; el cuerpo del commit anunciaba
> el rojo antes de empujarlo.

### 9.2 Quien encontro los 18 defectos catalogados

| Nivel detector | Defectos |
|---|---:|
| Owner manual | **8** |
| Revision arquitectonica | **6** |
| Prueba de regresion nueva | 3 |
| **Full** | **0** |
| **CI** | **0** |
| **Prueba preexistente** | **0** |

El unico caso documentado de una prueba automatica atrapando una regresion (I-40) fue una regresion
**escrita a proposito tres commits antes**.

### 9.3 El contrafactual mas duro

`MEASURED`: el defecto que I-44 arreglo lo dejo I-42. I-42 acumulo **77 corridas de CI** sobre 93
commits, del orden de **120 ejecuciones de nucleo y 116 de UI** autodeclaradas, **mas una validacion
manual del Owner APROBADA 8/8**. El defecto sobrevivio a todo ello y aparecio cuando el dueno **abrio
un DWG real**.

### 9.4 El coste no escala con la deteccion

I-44 pago 14 Full; I-43 pago 100 —siete veces mas— y su nivel detector siguio siendo el humano: las
tres regresiones estructurales que casi la bloquean las encontro una **revision arquitectonica**, no
las 50 corridas de nucleo ni las 50 de UI que las precedieron **en verde**. Y en I-40 **cada uno de
los cinco candidatos rechazados por el Owner tenia las dos suites y el CI en verde**.

---

## 10. Riesgos de una futura estrategia multi-STA

Auditoria **estatica** de contraindicaciones. **No es autorizacion para implementar multi-STA.**

| Recurso global | Riesgo | Evidencia |
|---|---|---|
| `EditorDiscardPrompt.confirm` | **UNSAFE**(1) | delegado **estatico de proceso**, guardar-restaurar **no reentrante**, sin `[ThreadStatic]`, sin `AsyncLocal`, sin lock; su defecto es un **`MessageBox` modal real**; `StaTestRunner` lo sustituye y **descarta el `IDisposable`**, luego la sustitucion es permanente y de proceso |
| `SelectiveCabeceraHeightPrompt` | **UNSAFE**(1) | misma forma con **dos** estaticos y **sin** sustitucion por defecto: fuera de sus 4 ambitos de prueba, el defecto ya es un `MessageBox` real |
| 22 `SolidColorBrush` estaticas **sin congelar** | **UNSAFE**(1) | en dos ventanas que las pruebas construyen y cierran; en todo `src/` solo hay **2** llamadas a `.Freeze()` |
| `ShellResources` (una `ResourceDictionary` de proceso) | CONDITIONAL | mergeada en el arbol de las ventanas desde 3 sitios; 13 `SolidColorBrush` sin `po:Freeze`; **pruebas que fijan su IDENTIDAD** con `Assert.Same` |
| `%APPDATA%` / `%LOCALAPPDATA%` sin costura | CONDITIONAL | el menu principal lee `settings.json` en un **inicializador de campo** (14 construcciones); el configurador **escribe** su layout con `File.WriteAllLines` **no atomico** en cada `OnClosed` |
| Caches estaticas de catalogo | CONDITIONAL | `ConcurrentDictionary` con lock por entrada; entregan instancias **compartidas** declaradas de solo lectura **por contrato**, no verificado |
| Clipboard, cultura, estado de hilo | SAFE | **0** usos de `Clipboard`; **0** asignaciones de cultura en todo el repositorio |
| `RackCad.Plugin` y sus estaticos | SAFE | la suite de UI **no referencia** ese proyecto |

(1) **Reserva epistemologica aceptada:** con la ejecucion prohibida, la etiqueta maxima defendible
sobre esta evidencia es **CONDITIONAL**, con la rotura como `HYPOTHESIS` derivada del modelo de WPF.
Se conservan como UNSAFE **anotadas**, no como medicion.

**`MEASURED` — ya existe una carrera, hoy, sin multi-STA:** seis `[Fact]` mutan
`EditorDiscardPrompt.confirm` desde hilos **MTA** del pool de xUnit mientras otros tres lo mutan desde
el hilo **STA**. Y `MEASURED`: el job `ui-tests` **no declara `timeout-minutes`** (`build-plugin` si,
30), asi que un cuelgue en un `MessageBox` modal real no tiene red.

**`RECONSTRUCTED` — precedente interno:** las dos unicas `[CollectionDefinition]` del repositorio
existen para exactamente este problema —«un estatico de proceso sustituido por una prueba, con otra
corriendo en paralelo»— y en ambos casos la solucion elegida fue **serializar**, no aislar por hilo.
Los dos equivalentes de la capa de UI **no tienen ningun mecanismo**.

**`RECONSTRUCTED` — precedente de contaminacion real:** el Selectivo esta blindado por una factory y
un guard porque la suite **ya sufrio** una dependencia de orden via `%APPDATA%`, que hacia pasar una
prueba «solo por contaminacion». Se cerro **por inyeccion**, no por serializacion.

**Alternativa conservada, no elegida:** un solo STA con `MaxParallelThreads=1` elimina las cuatro
filas de riesgo alto sin tocar produccion, cierra la carrera que ya existe y es **configuracion, no
rediseno**. A cambio pierde el techo de escalado, pierde el paralelismo real de las 24 clases que no
usan el STA, y **oculta** los globales mutables en vez de corregirlos. `UNKNOWN`: cuanto reloj queda
sobre la mesa, dado el §3.1.

---

## 11. Hipotesis refutadas

| Antes | Ahora | Fuente |
|---|---|---|
| «El CI esta serializado» | Los cuatro jobs arrancan en paralelo; solo `build-plugin` declara `needs` | informe previo |
| «El catalogo es el cuello» | Una clase con 84 accesos corre sus 49 pruebas en **0.5 s** | este Discovery |
| «5 clases = 49 % del tiempo del nucleo, luego son el blanco» | Quitarlas no libera reloj **porque el poste largo esta enmascarado** (Amdahl), no porque sean baratas; y la comparacion es n=2 vs n=3 con bandas solapadas | §5 |
| **«El job de UI gobierna ~95 % del reloj de pared»** | **`max()` sobre dos caminos casi iguales; UI gobierna en 17 de 25. Cota superior de ahorro por eliminarlo entero: ~10 %** | §3.1 |
| **«Las pruebas STA cuestan ~400x mas»** | **~87 % de esa duracion es BLOQUEO, no trabajo. El embudo existe; la magnitud no esta medida** | §4.1 |
| «`MaxParallelThreads=1` es mas rapido» | Senal real, pero **n=1 vs n=1** dentro de una banda de ruido mas ancha que el efecto | §4.3 |
| «Saltar el job de UI ahorra ~95 % por commit» | Solo **14 de 138** commits son elegibles; y el ahorro por commit elegible es el ~10 % del §3.1 | §3.1 |
| «`verify-autocad-references.ps1` es lo mas caro del CI» | 78.2 s frente a 212.9 s del job de UI | inventario CI |
| «Falta cache de NuGet: es el driver principal» | El restore es ~16 s de un paso de 216 s: techo del 7 % | inventario CI |
| «I-37D es el ciclo de validacion extremo» | **I-42 lo dobla**: 8 d 22 h 42 m y 8 rondas rechazadas | §8.1 |
| «El repo no registra resultado por fila de checklist» | El formato **ya obliga** al campo `Resultado por punto:`; el fallo es de **cumplimiento** | §8.3 |
| «HANDOFF: build de UI con 0 advertencias» | El log del CI del mismo candidato reporta **2 `CS0105`**, aun vivos | §8.4 |

---

## 12. Hechos firmes

1. `MEASURED` — El reloj de pared del CI es un `max()` sobre dos caminos casi iguales (mediana 214 s
   y 194 s, margen mediano 16 s). Eliminar el job de UI entero rinde ~10 %.
2. `MEASURED` — `restore + build` cuesta ~46-47 s **dentro de cada job de prueba**, comparable a la
   ejecucion entera del nucleo. Ninguna seleccion de pruebas lo toca.
3. `MEASURED` — El tiempo de ejecucion varia **~2x** sobre poblaciones identicas. Cualquier ahorro
   por debajo de ese factor es indistinguible del ruido.
4. `MEASURED` — La suite de UI embotella en **un** `Dispatcher` estatico; ~87 % de la duracion
   reportada por prueba es bloqueo.
5. `MEASURED` — No existe taxonomia de pruebas: 0 `[Trait]`, 0 `IClassFixture`, 2
   `[CollectionDefinition]` que **desactivan** paralelismo, sin `runsettings` ni `runner.json`.
6. `MEASURED` — El grafo de compilacion **miente**: 14 archivos del nucleo verifican `Plugin` y `UI`
   leyendo su texto, y ningun `csproj` de pruebas referencia el Plugin.
7. `MEASURED` — El artefacto de cobertura **no contiene identidad de prueba** y no cubre `UI` ni
   `Plugin`. Cobertura mide ejecucion, no asercion.
8. `MEASURED` — En las tres iniciativas analizadas, **ninguna suite completa encontro un defecto no
   previsto**. Owner manual 8, revision arquitectonica 6, regresion nueva 3; Full 0, CI 0,
   preexistente 0.
9. `MEASURED` — El CI vale entre el **1.3 % y el 6.3 %** de la latencia del ciclo de validacion.
10. `MEASURED` — El 20 % de los commits recientes cruzan mas de un sistema y el 19.5 % tocan `src`
    fuera de toda carpeta de sistema.
11. `MEASURED` — `AGENTS.md` acota la obligacion de suite completa a un **cambio de comportamiento**.

---

## 13. Incognitas

1. `UNKNOWN` — **Owner hands-on time**, en las cinco iniciativas. Estructuralmente inmedible con el
   formato actual (§8.2).
2. `UNKNOWN` — **El trabajo serializado real** sobre el hilo STA. Es la cantidad que decide si
   cualquier cambio de paralelismo recuperaria algo.
3. `UNKNOWN` — **Duracion del ciclo local obligatorio** (`dotnet test` + 2 builds antes de cada
   NETLOAD). Ni un solo registro en todo el repositorio.
4. `UNKNOWN` — **Recall real de un selector por grafo**: acotado solo entre 29 % y 100 %.
5. `UNKNOWN` — **Coste por prueba de cada estrategia en casos ejecutados**, no en metodos declarados.
6. `UNKNOWN` — Cuantas de las 327 filas de checklist de I-37D se recorrieron de verdad.
7. `UNKNOWN` — Si las advertencias `CS0105` aparecian tambien en el build local del Owner
   (`HYPOTHESIS`: un build incremental no las reemite).
8. `UNKNOWN` — Reparto de la latencia de ciclo entre espera, correccion, nueva validacion y CI. Solo
   el CI es aislable.
9. `UNKNOWN` — Que fraccion del reloj se lleva la cola del runner a lo largo del tiempo.

---

## 14. Evidencia insuficiente

Cosas que **no existen en el repositorio** y sobre las que, por tanto, **ninguna Proposal puede
apoyarse**:

1. La duracion de cualquier sesion de validacion del Owner, o de cualquier fila de checklist.
2. Tiempo por prueba o por clase de cualquiera de las dos suites. El CI sube **un** artefacto
   (cobertura) y **ningun** `.trx`.
3. Cualquier dato de cobertura de `src/RackCad.UI` y `src/RackCad.Plugin`.
4. Un mapa prueba → codigo con granularidad de prueba.
5. Cualquier duracion registrada del ciclo local obligatorio.
6. Verificacion independiente de las ~146 corridas locales autodeclaradas. Solo el CI es
   comprobable, y se contrasto **una** corrida contra su log.
7. Estado legible por maquina de las **cinco** iniciativas mas recientes: `docs/automation/state/`
   termina en I-39D y no hay evidencia de I-40 a I-44.
8. Un resultado por punto de cualquier validacion manual: **0 de 46** archivos de evidencia.
9. **Un solo caso historico** de una prueba preexistente detectando un defecto introducido por un
   cambio de otro sistema. Sin el, el coste de falso negativo de cualquier selector es **no acotable
   desde la evidencia**: solo la **exposicion** (20 %) es medible, nunca la **materializacion**.
10. Cualquier taxonomia a la que enganchar un selector.
11. La cola del runner como magnitud nombrada y seguida.
12. **Cualquier baseline versionada** de lo que costaba el CI o el ciclo local antes de esta
    iniciativa. Las metricas de exito no tienen contra que medirse.
13. Los propios tiempos de este Discovery: viven en un directorio temporal de sesion, **no
    versionado y no auditable** por el Arquitecto (§1).

---

## 15. Preguntas que la Proposal V1 debera resolver

1. **Cual es la metrica objetivo, y permite el DAG moverla.** Si es el reloj del CI, el techo esta
   medido en ~10 %. Si es la latencia del ciclo del Owner, el termino dominante se mide en **dias** y
   no es atribuible entre trabajo y espera. La Proposal debe **nombrar una** y declarar su techo
   medido.
2. **Donde queda el ciclo del Owner y como se medira alguna vez.** El contrato lo declara
   precondicion y esta sin cumplir. O se propone anadir un campo de duracion al formato de evidencia
   y se difiere la recomendacion hasta acumular datos, o se declara **fuera de alcance** aceptando
   que puede dominar.
3. **El coste STA es trabajo o cola, y cual es el suelo serializado real.** Sin ese numero, nada
   puede recomendarse sobre paralelismo.
4. **Cual es el suelo de ruido y esta el ahorro propuesto por encima.** ~2x en CI, 1.9x–3.7x local.
   Hace falta un efecto minimo detectable y un protocolo de repeticion, o las metricas de exito seran
   infalsables.
5. **Que cambio normativo hace falta realmente.** Separar el coste causado por **las reglas como
   estan escritas** del causado por **sobre-cumplimiento**: lo segundo no necesita consenso.
6. **Cual es el presupuesto de deteccion que el dueno acepta.** No hay caso historico del que estimar
   recall; solo exposicion. Debe convertirse en una **declaracion de riesgo** que el dueno acepte o
   rechace, no en una cifra calculada.
7. **Donde vivira la evidencia y cuanto sobrevivira.** La cobertura caduca en 14 dias, no se sube
   ningun `.trx`, y los tiempos actuales estan fuera del repositorio.
8. **Por que se excluyo I-42** —el registro de validacion mas rico y el extremo en rondas y en
   latencia— y si incluirlo cambia la recomendacion.

---

## 16. Correcciones pendientes sobre el propio contrato de I-45

Detectadas por el critico de cierre y **corregidas en el mismo gate que publica este documento**:

1. El contrato declaraba una fase «HECHA» cuyo unico entregable —este archivo— **no existia en
   ninguna referencia de git**. Es exactamente la clase de error que §8.4 documenta en `HANDOFF`.
2. El contrato nombraba **dos rutas obligatorias y mutuamente excluyentes** para este mismo
   artefacto (`docs/automation/evidence/` en §3 y `docs/initiatives/I-45-discovery.md` en §7), y §7
   cierra con «una desviacion material obliga a detenerse». Se unifica en la ruta de §7.
3. El contrato prometia derivar el mapa prueba → codigo de la cobertura publicada. §7.3 demuestra que
   **no es derivable de esa fuente**. La promesa se retira y el hueco pasa a §14.
