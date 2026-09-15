# WORKFLOW — flujo Git, worktrees y trabajo multi-agente

> Actualizado: 2026-07-17 (I-00 ejecutada: `main` es el trunk único).
> Proceso repetible para que varias IAs (Claude, Codex, futuras) y el humano desarrollen RackCad en
> paralelo durante años sin pisarse. Las ramas se nombran por INICIATIVA técnica, nunca por
> herramienta ([ADR-0001](adr/0001-ramas-por-iniciativa.md)). El plan de iniciativas vive en
> [ROADMAP.md](ROADMAP.md); la migración inicial ya se ejecutó (nota en la sección 9).

> **Estado de transición.** Las secciones 1–10 describen Workflow V1, que sigue siendo el proceso
> efectivo. La sección 11 materializa Workflow V2, pero no entra en vigor hasta que exista
> `WORKFLOW_V2_EFFECTIVE_SHA` según §11.2. I-56 y los reclamos formales existentes de I-49, I-52 e
> I-55 siguen gobernados por V1. La presencia del texto V2 no inicia la pausa de activación.

## 1. Estrategia de ramas

**Esta tabla es LA fuente de la convención de prefijos** (los demás documentos remiten aquí, no la copian):

| Prefijo | Qué representa | Ejemplos |
|---|---|---|
| `main` | **Trunk único de integración.** Siempre verde, siempre funcional. Nunca se trabaja directo en ella | — |
| `architecture/<slug>` | Cambio estructural transversal (contratos, registros, shells) | `architecture/system-registry`, `architecture/editor-shell` |
| `feature/<slug>` | Funcionalidad visible para el usuario | `feature/push-back`, `feature/guardrail-unidades` |
| `refactor/<slug>` | Reestructuración que preserva comportamiento | `refactor/plugin-commands`, `refactor/fallos-silenciosos` |
| `fix/<slug>` | Corrección de bug | `fix/install-bundle-preserva-datos` |
| `docs/<slug>` | Iniciativa solo de documentación | `docs/reestructura`, `docs/adr-retroactivos` |
| `experiment/<slug>` | Spike/prototipo que puede descartarse; NUNCA se integra directo (su resultado se re-implementa limpio o se promueve conscientemente) | `experiment/refs-autocad-ci` |
| `release/vX.Y.Z` | Corte estable etiquetado. Se corta al entregar el bundle a un tercero; versión semver; el tag y el `AppVersion` suben en el mismo commit | `release/v1.1.0` |
| `hotfix/<slug>` | Corrección urgente sobre un release entregado; nace del tag, se entrega, y se re-aplica a `main` en el mismo cierre | `hotfix/v1.1.1-bom-crash` |

Reglas de nombre: kebab-case, español (como el resto del repo), y el slug describe la **iniciativa**,
no la sesión ni la herramienta. La procedencia de la IA vive en los **trailers de commit**
(`Co-Authored-By`), no en el nombre de la rama. Los prefijos `claude/*` y `codex/*` quedan retirados;
la última rama con esos nombres, `codex/dinamico-modular`, fue resuelta por
[ADR-0002](adr/0002-secuencia-dinamico-modular.md) (**aceptado con la opción A**) y renombrada e
integrada por I-02 como `feature/dinamico-modular` (sección 9).

## 2. Iniciativas: la unidad de trabajo

**1 iniciativa = 1 rama = 1 worktree = 1 entrada en ROADMAP.md.** La entrada es **obligatoria**, pero
no siempre **preexistente**: en el caso (d) de abajo nace en el bootstrap inmediatamente posterior al
reclamo. Lo que nunca ocurre es que una iniciativa viva sin fila.

- Una iniciativa cabe en **1-3 sesiones**. Si crece más, se parte (el ROADMAP muestra cómo).
- Se abre una rama solo si: (a) la iniciativa está en ROADMAP.md, o (b) es un `fix/` puntual, o
  (c) es un `experiment/` con pregunta concreta que responder, o (d) **el dueño la autoriza
  explícitamente** aunque todavía no tenga fila — con la obligación de crearla en el bootstrap
  inmediatamente posterior al reclamo (regla completa abajo).
- **El registro vivo del trabajo en curso son las ramas del REMOTO** (`git fetch && git branch -r`),
  no las locales. Por eso el reclamo de una iniciativa es su **commit de reclamo + primer push
  aceptado, sin force** (sección 4.1): crear la rama local no reclama nada — dos sesiones pueden
  partir del mismo commit y solo el remoto decide quién llegó primero.
- Dos iniciativas en paralelo no deben tocar el mismo archivo caliente (sección 7). La columna
  "se estorba con" del ROADMAP codifica esto: **"independiente" significa sin dependencias previas,
  pero los estorbos declarados siguen aplicando** — una iniciativa de relleno solo arranca si sus
  estorbos no están en curso.

### Qué es ROADMAP.md, y cuándo se edita — **autoridad única de esta regla**

`docs/ROADMAP.md` es **planificación, alcance y registro de cierre**. **No es el estado vivo de lo que
está en curso**: ese estado se deriva, siempre, de la existencia de la rama en el remoto
(`origin/<rama-de-la-iniciativa>`).

De ahí que sea legítimo editarlo en **tres momentos, y solo en tres**:

1. **Al planificar** una iniciativa, antes de su reclamo.
2. **En el bootstrap inmediatamente posterior al reclamo**, cuando el dueño autorizó explícitamente
   una iniciativa que aún no tenía fila (caso (d) de arriba).
3. **Al integrar o cerrar**, para dejar el registro de cierre.

Y **no** se edita para marcar «en curso», ni en cada sesión, ni en cada gate, ni en cada push.

**El caso (d), con precisión.** La autorización explícita del dueño sustituye **únicamente la
preexistencia de la fila**. No sustituye nada más: siguen siendo obligatorios el reclamo atómico
(sección 4.1), la rama y el worktree, el contrato de iniciativa y el bootstrap, y la fila durable en
ROADMAP. La secuencia es:

```
autorización explícita del dueño → reclamo atómico remoto → bootstrap inmediato
   → fila en ROADMAP → (solo entonces) trabajo sustantivo
```

**Ningún trabajo sustantivo antes de que el bootstrap esté versionado.** El bootstrap **no** es el
reclamo: el reclamo sigue siendo el primer push que el remoto acepta.

**«Ramas paralelas», con precisión** (sección 8 lo repite en su tabla). La prohibición de tocar
HANDOFF y ROADMAP desde una rama de iniciativa se refiere a **la rama de OTRA iniciativa**, y sigue
siendo absoluta para `HANDOFF.md`. Para `ROADMAP.md`, la propia rama de la iniciativa puede escribir
su fila en los momentos 1 y 2, y su marca de cierre en el 3 — nunca su estado «en curso», y nunca la
fila de una iniciativa ajena.

## 3. Worktrees

- **Crear** un worktree al arrancar la iniciativa, desde la punta ACTUALIZADA del trunk
  (`git fetch` primero). Nombre de carpeta = `<tipo>-<slug>` (ej. `architecture-system-registry`).
  Viven fuera del árbol versionado (las herramientas ya usan `.claude/worktrees/` y
  `%USERPROFILE%\.codex\worktrees`; ambas ubicaciones sirven).
- **El worktree vive lo que vive la iniciativa**: una iniciativa conserva EL MISMO worktree toda su
  vida y puede acumular varias sesiones **secuenciales** sobre él — Claude, Codex u otra herramienta
  pueden relevarse en ese mismo worktree. Lo prohibido es tener dos sesiones **activas a la vez**
  sobre el mismo worktree (o la misma rama). No se crea un worktree nuevo por sesión.
- **Relevo entre sesiones** (misma herramienta u otra): la sesión saliente deja commit + push +
  `git status` limpio + resumen del estado en el cuerpo del commit. La sesión entrante empieza
  verificando rama (`git branch --show-current`), upstream y divergencia (`git branch -vv`),
  `git status` y los últimos commits (`git log --oneline -5`) antes de escribir nada.
- El worktree principal (`D:\Documentos\Codex\Calculadora de racks`) es del humano: los agentes no
  dejan ahí cambios sin commitear. Un CSV editado "en vivo" en el principal se commitea o descarta
  el mismo día (es invisible para los demás worktrees mientras tanto).
- **Eliminar** el worktree en el mismo acto en que su rama muere, y su rama **no muere con el merge**:
  sobrevive hasta que el merge exista **y** pasen las comprobaciones posteriores al merge (sección 4.5
  pasos 6 y 7); solo entonces se limpia. Si esa verificación sale roja, la corrección se hace **en
  esta rama**, que por eso sigue viva. Una rama también muere por abandono formal, y una `experiment/*`
  por su cierre propio (sección 4.5). En todos los casos, con **borrado seguro por defecto**:
  `git worktree remove` + `git branch -d` — la `-d` minúscula
  falla si la rama no está contenida en el HEAD actual, y esa falla ES la protección (investigar,
  no forzar). `git branch -D` queda reservado para: (a) iniciativas abandonadas con autorización
  explícita del dueño, (b) ramas ya archivadas con tag verificado, y (c) reclamos locales
  rechazados que nunca se publicaron (sección 4.1). El borrado REMOTO (`git push origin --delete`)
  solo procede tras confirmar que el merge existe en `main` (`git branch -r --merged main` la
  lista) o que la rama fue archivada deliberadamente con tag.

## 4. Ciclo de vida de una iniciativa (el proceso repetible)

```
fila en ROADMAP            ┐
   O BIEN                  ├→ rama + commit de reclamo + push (reclamo) ┐
autorización del dueño     ┘                                            │
                                                                        ↓
   (solo caso (d)) bootstrap inmediato: contrato + fila en ROADMAP  ────┤
                                                                        ↓
   sesiones (rebase al abrir, push al cerrar)
       → sesión de integración (rebase final → CI → validación → HANDOFF/ROADMAP → merge
         → **CI post-merge** → cobertura del Candidato) → limpieza
```

El bootstrap **no reclama**: el reclamo sigue siendo el primer push aceptado. La condición de entrada
es «fila en ROADMAP **o** autorización explícita del dueño», y en el segundo caso la fila se crea
inmediatamente después (sección 2, que es la autoridad de esta regla).

1. **Abrir (reclamo atómico)**: elegir iniciativa del ROADMAP cuyos estorbos no estén en curso —o una
   que el dueño haya autorizado explícitamente, caso (d) de la sección 2— →
   `git fetch origin` → crear rama y worktree desde **`origin/main`** (la punta REMOTA del trunk,
   no la local) → **commit vacío de reclamo** (`git commit --allow-empty`) cuyo mensaje incluye:
   el ID de iniciativa (p. ej. `I-26`), el nombre de la rama, un trailer `Claim-Id:` único
   (UUID) y el trailer de procedencia del agente (`Co-Authored-By`) → `git push -u origin <rama>`
   **sin force**. **El primer push que el remoto ACEPTA es el reclamo**; crear la rama local no
   reclama nada.
   - Si el push del reclamo es rechazado porque otra sesión ya creó `origin/<rama>`: no usar
     force, no sobrescribir la rama remota; eliminar SOLO el worktree y la rama local del reclamo
     rechazado (`git worktree remove` + `git branch -D`, caso (c) del borrado — nunca se publicó)
     y elegir otra iniciativa.
   - El commit de reclamo forma parte del historial normal de la iniciativa (convención por
     defecto: se CONSERVA — es vacío, no estorba al bisect, y deja trazables fecha, agente y
     Claim-Id de la apertura). Eliminarlo en el rebase final antes de integrar es opcional.
2. **Cada sesión — al abrir**: si el trunk avanzó, **rebase** sobre su punta antes de escribir una
   línea. Una rama que se queda atrás acumula conflictos semánticos (caso real: la rama del dinámico
   bifurcó justo antes de fixes de persistencia de su propia área).
3. **Cada sesión — al cerrar**: commit + push de la rama. Tras un rebase de una rama ya pusheada,
   publicar con `git push --force-with-lease` (seguro aquí: 1 iniciativa = 1 worktree = 1 sesión
   activa). **Push de rama = respaldo, siempre, sin esperar aprobación; integrar es otra cosa.**
   El resumen de la sesión va en el cuerpo de los commits, NO en HANDOFF (ese se toca al integrar).
4. **Pedir integración** cuando el checklist de cierre (sección 5) esté completo.
5. **Sesión de integración** (serializada: una iniciativa a la vez; la ejecuta el humano o una
   sesión dedicada **en la workstation del dueño** — el build del Plugin exige AutoCAD 2025
   instalado, y cerrado durante el build):
   1. Rebase final de la rama sobre `main` + `git push --force-with-lease`.
   2. Esperar **CI verde sobre el SHA empujado** —el tip rebasado; la evidencia de CI no se propaga a
      los commits anteriores del mismo push— + build Debug local de UI y Plugin. El build del Plugin
      no lo cubre ninguna suite; el de UI lo compila de paso la suite de UI, que bajo LC-UI puede no
      haberse corrido en local en las iteraciones previas — razón de más para hacerlo aquí explícito.
      **Ese tip rebasado es un Candidato**, así que exige además las DOS suites en local sobre él
      (AGENTS.md, «Pruebas — definicion de terminado», punto 1).

      > **El Candidato NO es el único SHA que entra a `main`**, y decirlo sería falso: el paso 4 crea
      > después el commit documental de cierre, que es el tip que de hecho se mergea, y el paso 5
      > produce además el commit de merge. Son tres SHAs distintos con papeles distintos:
      >
      > ```
      > SHA del Candidato      = el SHA validado que porta el producto
      > SHA de cierre documental = tip real de la rama que se mergea; SHA nuevo, evidencia propia (paso 4)
      > MERGE_SHA              = SHA nuevo del merge; exige su propio CI (paso 6)
      > ```
   2.bis. **Cobertura del Candidato**, si se quiere la señal de salud sobre ese SHA. Se pide de forma
      **explícita**, nunca se infiere:

      ```bash
      gh workflow run ci.yml --ref <rama> -f candidate_sha=<sha completo de 40 hex>
      ```

      El SHA viaja en el **input**, no en `--ref`. La corrida hace checkout de ese commit exacto y
      **aborta en rojo** si `git rev-parse HEAD` no coincide con lo pedido; sin input explícito
      también aborta, en vez de medir la punta de la rama. El artifact incluye
      `measured-sha.txt` con el commit que se midió de verdad.

      > **Condición para que el comando exista:** GitHub solo ofrece `workflow_dispatch` para
      > workflows presentes en la **rama por defecto**. Mientras `ci.yml` con ese disparador viva solo
      > en una rama de iniciativa, `gh workflow run` responde `HTTP 422: Workflow does not have
      > 'workflow_dispatch' trigger`. No es un fallo del proceso: se resuelve solo al integrar.

      **Esa corrida NO es la evidencia «CI verde sobre el SHA exacto» del Candidato.** El registro de
      un `workflow_dispatch` lleva como `head_sha` la punta del ref despachado, no el SHA medido; lo
      que prueba qué commit se midió es la comprobación dentro del log. La evidencia de CI del
      Candidato sigue siendo la corrida de **push** de ese SHA. Y la cobertura **no declara**
      Candidato: el proceso declara el Candidato, y solo después se le mide.

   3. **Validación manual en AutoCAD sobre el SHA YA rebasado** (sección 6) si cambió comportamiento
      de dibujo. Una validación anterior **solo** se reutiliza si recae sobre **ese mismo SHA exacto**
      —y con la misma versión de AutoCAD y la misma biblioteca de bloques—. Que el trunk no haya
      avanzado **ya no basta**: el rebase produce SHAs nuevos, y el SHA se estampa en el ensamblado
      (AGENTS.md, «Reutilización de evidencia»).
   4. Último commit de la rama: actualizar `docs/HANDOFF.md` §8-12 y marcar la iniciativa en
      `docs/ROADMAP.md` como `integrada (fecha)` — así el merge lleva los docs consigo y nadie
      commitea directo al trunk después.

      **Este commit es un SHA nuevo y no hereda NADA del Candidato.** Lo que se le exige, de forma
      explícita y proporcional a lo que contiene —esto es una regla de *qué evidencia se exige*, **no**
      una reutilización (AGENTS.md, «Reutilización de evidencia»)—:

      - **Comprobar mecánicamente que solo toca documentación**:
        `git diff --name-only <candidato>..HEAD` no debe listar nada fuera de `docs/`, `README.md` o
        `CLAUDE.md`. Si toca `src/`, `tests/`, `assets/`, `eng/`, `deploy/`, `.github/` o cualquier
        archivo de build, **no es un commit documental** y se le exige todo lo del Candidato.
      - **CI verde sobre ese SHA exacto.** Es evidencia real y propia de ese commit, no heredada: el
        CI ejecuta las dos suites y los dos builds sobre él.
      - **No se le exige validación del dueño**, y no porque se reutilice la del Candidato: la
        obligación de validar en AutoCAD se activa cuando **cambia el comportamiento de dibujo**, y un
        commit que demostrablemente no toca producto no lo cambia. Si tocara producto, la obligación
        se activa y el punto anterior ya lo manda al camino completo.
   5. `git checkout main && git merge --no-ff <rama>` y push de `main`. Cada iniciativa queda como
      una burbuja con su nombre; `git log --first-parent main` lee como el registro de iniciativas
      y los commits internos siguen siendo atómicos y bisecables.

   6. **Esperar y verificar el CI POSTERIOR AL MERGE. Es una compuerta real, no un trámite.** El
      commit de merge es un **SHA nuevo** que nadie ha construido antes —y RackCad estampa el SHA en
      el ensamblado—, así que ninguna evidencia de la rama lo cubre (AGENTS.md, «Reutilización de
      evidencia»). Con `MERGE_SHA = git rev-parse main`, exigir sobre **ese** SHA exacto:

      ```
      corrida de CI sobre MERGE_SHA          = success
        Tests (Domain + Application)         = success
        UI Tests (WPF...)                    = success
        Build UI                             = success
        Build Plugin without AutoCAD         = success
      artifact rackcad-coverage-cobertura    = PRESENTE      (main lleva cobertura: ADR-0033 §9)
      ```

      **Si esa corrida no está verde, o falta la cobertura, la integración no está VERIFICADA** —el
      merge ya ocurrió y no se deshace— y no se limpia nada. La corrección se hace **en la rama de
      iniciativa**, que por eso sigue viva: se arregla ahí, se ejecuta la validación que corresponda,
      se empuja la rama y se vuelve a entrar por el proceso de integración, con su propio merge y su
      propio CI posterior. **Nunca con un commit directo sobre `main`**: eso está prohibido y este
      caso no es una excepción.

   7. **Comprobación diferida de la cobertura del Candidato.** GitHub solo ofrece `workflow_dispatch`
      para workflows presentes en la rama por defecto, así que esta comprobación **solo es posible
      después** de que el merge publique `ci.yml` en `main`. Mientras la rama aún exista:

      ```bash
      gh workflow run ci.yml --ref main -f candidate_sha=<CANDIDATE_SHA>
      ```

      y verificar que `candidate_sha` pedido == `HEAD` del checkout == `measured-sha.txt`, y que el
      artifact `rackcad-coverage-cobertura` está presente. Es una **señal de salud**: no declara
      Candidato, y **no** es la evidencia «CI verde sobre el SHA exacto» del Candidato —esa sigue
      siendo su corrida de `push`—.

   - Protección de `main`: contra force-push y borrado, **sin** "required status checks" (el commit
     de merge local no tendría CI previo y GitHub lo rechazaría). Por eso las **dos** compuertas se
     verifican a mano en la sesión de integración, y **ambas son obligatorias**:

     ```
     CI de la rama sobre el SHA rebasado  = PRECONDICIÓN del merge   (paso 2)
     CI de main sobre el MERGE_SHA        = VERIFICACIÓN posterior   (paso 6)
     ```

     Ninguna sustituye a la otra. Que la primera esté verde **no** dice nada del SHA del merge.

6. **Limpiar** — **solo después de que los pasos 5.6 y 5.7 hayan pasado**: borrar rama local
   (`git branch -d` — el merge la contiene), remota (procede: el merge ya existe en `main`) y el
   worktree, según las reglas de borrado seguro de la sección 3.

   La razón del orden **no** es que los SHAs se pierdan: tras el merge son ancestros de `main` y el
   CI hace checkout por SHA, sin depender de ningún nombre de rama. La razón es que **limpiar es
   declarar terminada la integración**, y no lo está mientras falte una compuerta: si el paso 5.6
   sale rojo, la corrección se hace **sobre la rama**, que para entonces ya no existiría.

`experiment/*` tiene un final distinto: se cierra con una **conclusión escrita** (en el ADR o
iniciativa a la que alimenta, o en ideas-futuras.md) y la rama se borra. Su código no se mergea;
si el resultado se adopta, se re-implementa limpio en una rama `architecture/`/`feature/`.

## 5. Checklist de cierre de iniciativa

- [ ] **Las DOS suites** verdes **en local** —Core y UI, no un solo `dotnet test`— y **CI verde sobre
      el SHA exacto que ese punto del proceso exija**, no «en la rama»: una rama no tiene evidencia,
      la tienen sus commits, y el verde de un SHA no dice nada de otro (AGENTS.md, «Reutilización de
      evidencia»). El cierre exige ambas suites en local: LC-UI retira la corrida de UI de la
      **iteración ordinaria**, nunca del cierre ni del Candidato (AGENTS.md, «Pruebas — definicion de
      terminado», punto 1).
- [ ] Build Debug de UI y Plugin con 0 errores (los MSB3277 conocidos no cuentan).
- [ ] Bugfix ⇒ test de regresión **verificado fallando** sin el fix (AGENTS.md).
- [ ] Cambio de dibujo ⇒ validación manual del usuario en AutoCAD (sección 6).
- [ ] Documentación actualizada según la tabla de la sección 8 (en la MISMA rama).
- [ ] Decisiones tomadas durante la iniciativa registradas como ADR (criterios en
      [adr/README.md](adr/README.md)).
- [ ] Hallazgos fuera de alcance anotados en ideas-futuras.md (no arreglados "de paso").
- [ ] En la sesión de integración: HANDOFF §8-12 + estado en ROADMAP como último commit de la rama;
      tras el merge, **CI del `MERGE_SHA` verde con su cobertura** y comprobación de la cobertura del
      Candidato (§4.5 pasos 6 y 7); **solo entonces** rama + worktree borrados.
      > Ese commit de cierre marca la iniciativa como `integrada` **antes** de que exista el CI del
      > merge, y eso es correcto: `integrada` significa que **el merge existe en `main`**, no que la
      > integración esté verificada. Si esa corrida sale roja, la iniciativa **sigue mergeada** pero
      > su integración **aún no está verificada ni completa**; se corrige en la rama, que por eso no
      > se ha borrado todavía.

## 6. Validación manual en AutoCAD (a mitad o al cierre de una iniciativa)

- El procedimiento detallado, los escenarios y el formato de evidencia viven en
  [guias/validacion-manual-autocad.md](guias/validacion-manual-autocad.md). Esta sección conserva
  únicamente los gates del proceso.
- El usuario carga con NETLOAD **el DLL compilado DENTRO del worktree de la iniciativa**:
  `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` (la ruta de CLAUDE.md
  apunta al worktree principal y NO sirve para validar una rama; la misma rama no puede hacerse
  checkout en dos worktrees).
- Cerrar AutoCAD antes de cada rebuild del worktree (el DLL cargado queda bloqueado — trampa
  conocida de AGENTS.md).
- La validación que cuenta para integrar es la que se hace **sobre el árbol ya rebasado sobre
  `main`** (sección 4.5.3): una validación hecha **antes** del rebase final recae sobre un SHA que ya
  no existe en la rama, así que **no vale** para el SHA rebasado, aunque el árbol sea idéntico y
  aunque el trunk no se haya movido. La reutilización se decide por **SHA exacto**, nunca por árbol
  (AGENTS.md, «Reutilización de evidencia»).

## 7. Archivos calientes (alto riesgo de conflicto)

Un solo agente a la vez por archivo caliente; la columna "se estorba con" del ROADMAP debe reflejar
cualquier iniciativa que los toque.

| Archivo | Por qué es caliente |
|---|---|
| `docs/HANDOFF.md` | Único doc de estado; se actualiza SOLO en la sesión de integración (último commit de la rama), nunca en sesiones paralelas |
| `docs/ROADMAP.md` | Planificación, alcance y registro de cierre. Se edita en los **tres momentos** que fija la sección 2, que es la autoridad de esa regla. El estado "en curso" NO se anota ahí: se deriva de la existencia de la rama en origin |
| `assets/catalogs/*.csv` y `*.json` | Datos compartidos por todos los sistemas: 16 CSV y 3 JSON que **208 archivos de prueba** alcanzan. **Append-only** (filas nuevas al final); nunca reordenar ni re-guardar con otro encoding |
| Los tres editores grandes de `src/RackCad.UI/Systems/` | `PushBack/RackPushBackSystemWindow.xaml.cs` (3.511 líneas), `Dynamic/RackDynamicSystemWindow.xaml.cs` (2.937) y `Selective/RackSelectiveWindow.xaml.cs` (2.867). Toda feature de su sistema pasa por el suyo |
| `src/RackCad.UI/RackFrames/RackFrameConfiguratorViewModel.cs` (2.305 líneas) | God-ViewModel del configurador de cabecera |
| `src/RackCad.Plugin/*Commands*.cs` | 14 archivos de comandos con helpers estáticos cruzados; dos de ellos son parciales de otro (`RackLayoutCommands.Fill.cs`, `RackInventarioCommands.BomTotal.cs`). Tocar uno rara vez basta |
| `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` (776 líneas) | DeepCopy + DTO de seguridad: cada familia nueva lo toca |

Las rutas de esta tabla se verificaron contra el árbol real en el gate G0A de I-45: tres de ellas
apuntaban a archivos que ya no existían. `RackFrameCommands.cs` desapareció como archivo único cuando
I-09 partió los comandos del Plugin por área, y los dos editores citados se movieron a
`src/RackCad.UI/Systems/<sistema>/` con I-23. Una tabla que nombra archivos inexistentes no protege
nada, así que **cualquier iniciativa que mueva o parta uno de estos archivos actualiza esta fila en la
misma rama**.

## 8. Cuándo actualizar cada documento

| Evento | Documento | Cuándo |
|---|---|---|
| Cambia comportamiento visible, catálogos o elementos | `docs/guias/catalogos-y-plantillas.md` / guía correspondiente | En la misma rama, antes de integrar |
| Cambian comandos de AutoCAD, build o superficie de uso | `README.md` | En la misma rama |
| Se toma una decisión de arquitectura (criterios en adr/README.md) | `docs/adr/NNNN-*.md` | ANTES de implementarla |
| Se planifica una iniciativa | `docs/ROADMAP.md` | Antes de su reclamo (momento 1 de la sección 2) |
| Bootstrap de una iniciativa que el dueño autorizó explícitamente **sin** fila previa | `docs/ROADMAP.md` + contrato en `docs/initiatives/` | Inmediatamente después del reclamo y **antes de todo trabajo sustantivo** (momento 2 de la sección 2) |
| Cierre de iniciativa | `docs/HANDOFF.md` §8-12 y estado en `docs/ROADMAP.md` | En la sesión de integración, como último commit de la rama (sección 4.5.4) — momento 3 de la sección 2 |
| Cierre de sesión intermedia (sin integrar) | Cuerpo del commit + push de la rama | Nunca `HANDOFF.md`, y nunca la fila de OTRA iniciativa en `ROADMAP.md`. La propia fila sí, pero solo en los tres momentos de la sección 2 — jamás para marcar «en curso» |
| Cambia el proceso mismo | Este documento + ADR si es decisión de fondo | Antes de aplicar el proceso nuevo |
| Hallazgo fuera de alcance de la iniciativa | `docs/ideas-futuras.md` | Al detectarlo |
| Conteos de tests / hashes de commit | Mientras V1 sea efectivo: SOLO `docs/HANDOFF.md` §12. Para una unidad clasificada V2 después de la activación: archivo de evidencia y tag de integración (§11.4) | Nunca se copian a contratos, ROADMAP, índices o documentos normativos |

## 9. Migración inicial — ejecutada el 2026-07-17

La migración Git inicial (I-00) se ejecutó el **2026-07-17**: `main` es el **trunk único** de
integración, es la rama por defecto en GitHub y está protegida contra force-push y borrado (sin
required checks — sección 4.5); `release/claude-review` y las ramas heredadas por-herramienta
quedaron retiradas (sus puntas se preservaron con tags `archive/*`). El detalle del estado
(hashes, tags de recuperación, ramas restantes y el prompt de reanudación) vive en
[HANDOFF.md](HANDOFF.md) §8 y §14, no aquí.

**I-02 resolvió la última rama heredada por herramienta** (2026-07-17): `codex/dinamico-modular`
fue renombrada a `feature/dinamico-modular`, rebasada sobre `main` e integrada conforme a
[ADR-0002](adr/0002-secuencia-dinamico-modular.md) (opción A). Su punta pre-rebase permanece
preservada en el tag `archive/dinamico-modular-pre-rebase-9f19a8c` (no se elimina). Tras el merge
y la limpieza de I-02, ninguna rama activa por-herramienta forma parte del flujo. `main` continúa
siendo el trunk único. Toda rama nueva sigue la convención de la sección 1.

## 10. Autoridad por dominio

Los hechos de Git, código, CI, pruebas y builds describen lo ocurrido y vencen una afirmación factual
falsa. Cada regla tiene un solo dueño:

| Dominio | Autoridad | Referencias subordinadas |
|---|---|---|
| Git, reclamo, worktrees, integración, cierre, cadencia documental y transición | Este `WORKFLOW.md` y ADR de proceso aceptado dentro de su alcance | Contratos y lifecycle enlazan |
| Composición Full, clases de evidencia, SHA exacto e invalidadores | `AGENTS.md` | WORKFLOW y guías enlazan |
| Ciclo de diseño, Discovery, Architect, Freeze/A-n, gates funcionales, READY y conformidad | [INITIATIVE_LIFECYCLE.md](INITIATIVE_LIFECYCLE.md) cuando V2 sea efectivo | WORKFLOW y plantillas enlazan |
| Procedimiento de Owner Validation | [validacion-manual-autocad.md](guias/validacion-manual-autocad.md) | Freeze asigna escenarios; lifecycle verifica asignación |
| Arquitectura | `AGENTS.md`, ADR aceptado y Freeze+A-n dentro de su alcance | [FOUNDATIONS.md](FOUNDATIONS.md) solo resume hechos verificados |
| Prompts | [PROMPT_TEMPLATES.md](initiatives/PROMPT_TEMPLATES.md) | Siempre subordinadas a los dueños anteriores |
| Decisiones del Owner | `docs/automation/decisions/<I>.md`, dentro del alcance registrado | Contrato y evidencia enlazan |
| Estado y plan | `HANDOFF.md` y `ROADMAP.md` | No crean política normativa |

Un conflicto dentro de un dominio detiene el gate y cita ambas fuentes. Un ADR aceptado solo desplaza
una convención general cuando declara la excepción en su alcance; cualquier otro conflicto llega al
Owner. Los documentos históricos y ADR propuestos no son precedente normativo.

## 11. Workflow V2 materializado — todavía no efectivo

Esta sección materializa la política aprobada por ADR-0045. Hasta la activación definida en §11.2,
solo sirve para preparar y verificar el único merge normativo; no gobierna I-56 ni reclamos V1.
Arquetipos, Discovery, revisión, Freeze/A-n, gates funcionales, READY y conformidad pertenecen a
[INITIATIVE_LIFECYCLE.md](INITIATIVE_LIFECYCLE.md). La composición Full y las clases de evidencia
pertenecen a `AGENTS.md`. La ejecución de Owner Validation pertenece a la guía manual.

### 11.1 Identidad del reclamo y clasificación estable

El primer push aceptado del commit de reclamo crea un `Claim-Id` UUID estable. Clasificación y
Claim-Id se conservan aunque cambien el tip o el nombre de rama y aunque un rebase incorpore el SHA
efectivo. Una clasificación durable nunca se vuelve a derivar por ascendencia actual.

### 11.2 Merge normativo y `WORKFLOW_V2_EFFECTIVE_SHA`

Exactamente un commit de I-56 lleva el trailer `Workflow-V2-Normative: I-56`. El merge normativo es
el primer merge, en orden first-parent de `origin/main`, cuyo segundo padre alcanza ese commit y cuyo
primer padre no lo alcanza. El trailer debe ser único en esa historia; ausencia, duplicidad o forma
estructural distinta detiene la identificación.

`WORKFLOW_V2_EFFECTIVE_SHA` es el SHA de ese merge `--no-ff`. Workflow V2 entra en vigor cuando el
remoto acepta el push de `main` que lo publica. El merge local, la pausa, CI posterior, un tag, una
limpieza o una corrección no crean otro SHA efectivo. **`WORKFLOW_V2_EFFECTIVE_SHA` no está aún
establecido hasta que el merge normativo sea publicado remotamente.** No se escribe el SHA dentro de
su propio commit.

No hay activación parcial: todas las normas aprobadas entran en ese único merge o la activación se
detiene. I-56 conserva Workflow V1 durante toda su integración.

No existe rollback automático. Una desactivación futura exige Proposal aprobada explícitamente por
el Owner y un evento durable que establezca su alcance. El SHA efectivo histórico no se borra ni se
redefine, y los reclamos no se reclasifican retroactivamente por cambios posteriores de ascendencia.

### 11.3 Snapshots PRE/POST y tabla de transición

La integración V1 de I-56 conserva dos snapshots completos de `git ls-remote --heads origin`:

- **PRE:** inmediatamente antes de publicar el merge normativo. Comando, salida, fecha UTC, main
  observado y tabla `ref / tip / commit de reclamo / Claim-Id` van en el cuerpo del merge. Si la
  preparación se retrasa o cambia, se repite PRE y se regenera el merge.
- **POST:** inmediatamente después de que el remoto acepte el push. La misma tabla y el registro
  ordenado del push aceptado se conservan en el informe post-merge y en `integration/I-56`.

Claim-Id es la clave de unión. Una rama presente solo en POST tiene orden ambiguo y cae en T6. PRE
incompleto, Claim-Id ausente/duplicado o identidad contradictoria nunca recibe un valor inventado.

| ID | Condición comprobada | Clasificación y acción |
|---|---|---|
| T1 | PRE o prueba temporal válida de anterioridad; pausa respetada | V1 definitivo por Claim-Id |
| T2 | Anterior probado; reclamo durante la pausa antes de vigencia | V1; desviación sujeta a suspensión o excepción, sin reclasificar |
| T3 | Posterior probado; pausa activa | V2; STOP hasta fin durable o excepción explícita, nunca V1 por infringir la pausa |
| T4 | Posterior probado; base original contiene el efectivo; sin pausa | V2 normal |
| T5 | Posterior confirmado desde T6 con base original obsoleta | V2; desviación y STOP hasta rebase y tratamiento de pausa aplicable |
| T6 | Orden o identidad desconocidos/contradictorios, incluido solo POST | Sin clasificar; STOP y Owner con evidencia, sin default V1 |
| T7 | Unidad nueva posterior que consume diseño V1 fuera de I-49/I-52/I-55 | V2; contrato delta propio, sin reescribir la fuente V1 |
| T8 | Unidad nueva posterior conceptualmente perteneciente a I-49/I-52/I-55 | **T8-A:** V2 con reclamo, contrato, Freeze delta/A-n y evidencia propios; no hereda V1 |

Para probar anterioridad fuera de PRE se requiere un registro ordenado del primer push aceptado y un
fetch posterior que aún no contenga el SHA efectivo, contrastado con las corridas `push` de la rama y
de main. Base obsoleta, registro incompleto, contraste ausente o incoherencia caen en T6. Los reclamos
formales existentes de I-49, I-52 e I-55 y el de I-56 son V1.

Tras activación, una unidad V1 lee `main` actual. Solo las cláusulas modificadas por el merge normativo
se consultan en `WORKFLOW_V2_EFFECTIVE_SHA^1`; el mapa de esas cláusulas se conserva en la integración.
No se congelan archivos completos y los cambios V1 compatibles posteriores se leen desde main actual.

### 11.4 Superficies documentales y evidencia V2

Workflow V2 separa tres superficies:

1. **Contrato mutable** `docs/initiatives/<unidad>-<slug>.md`: estado, alcance resumido y enlaces;
   workflow, agrupación, arquetipo/materialidad, coordinación y `Consumes/Extends/Introduces`. No
   contiene tips, corridas, conteos ni hashes cambiantes.
2. **Freeze inmutable**: Proposal congelada, `<I>-freeze.md` o `<unidad>-freeze-delta.md`; contiene
   alcance, no-objetivos, invariantes y matriz OV. Los cambios posteriores son A-n append-only.
3. **Evidencia por unidad** `docs/automation/evidence/<unit>-evidence.md`: hechos verificables de la
   unidad. Los hechos posteriores al merge pertenecen al tag anotado de §11.6.

Esqueleto mínimo del archivo de evidencia:

```text
Unit / Initiative / Workflow / Claim-Id
Claim base and transition classification evidence
Discovery and Freeze identity; Freeze delta and A-n references
Candidate rounds: base, FINAL_CANDIDATE_SHA, clean tree, resolved SDK
Required local Full/build evidence and exact push-CI references
Conformance result and reviewed SHA
Owner Validation: assignment, scenarios, DLL identity and verdict
Metrics row with UNKNOWN explicit where unavailable
Integration tag reference
```

No hay commit ceremonial `-CLOSE` obligatorio por gate. El cierre puede quedar acreditado por el SHA,
informe y archivo de evidencia del gate. Las guías de comportamiento visible se actualizan en el
último gate de implementación antes del Candidato; el ADR nace antes de implementar su decisión. El
commit de cierre concentra HANDOFF, ROADMAP, índices, hallazgos fuera de alcance, entradas FOUNDATIONS
ya conformadas y evidencia final. ROADMAP conserva únicamente sus tres momentos de §2 y HANDOFF solo
se edita al integrar/cerrar. No se reescriben registros históricos.

Hashes, corridas y conteos de una unidad V2 viven en cuerpos de commit, su archivo de evidencia y su
tag; HANDOFF enlaza. Contrato, ROADMAP, índices y documentos normativos no los copian.

### 11.5 Integración V2 y ruta R

La integración sigue siendo manual y serializada, sin commits directos a main ni merges automáticos.
Primero se ejecuta READY-01..09 conforme a
[INITIATIVE_LIFECYCLE §8](INITIATIVE_LIFECYCLE.md#8-ready-y-frontera-del-candidato): el fetch, preflight
y rebase finales son **READY-04**, no una operación posterior a READY; READY-05 verifica focales,
relevantes y CI exacto de rama sobre el SHA rebasado; READY-06 realiza la conformidad completa de
Architect + Coordinator; y READY-07..09 completan la preparación restante. Solo después de satisfacer
las nueve condiciones se fija `FINAL_CANDIDATE_SHA` y se completa sobre ese SHA la evidencia final que
define `AGENTS.md`: Core y UI Full locales, builds Debug de UI y Plugin, CI exacto requerido, cobertura
según la política vigente y Owner Validation aplicable según la guía manual. La punta de producto se
publica sola; nunca se agrupa con el cierre documental.

Después siguen: cierre documental con CI propio; `fetch` inmediatamente antes del merge y ruta R si
main avanzó; merge manual `--no-ff`; CI de `event=push`, `ref=refs/heads/main`,
`head_sha=MERGE_SHA` con cobertura; comprobación diferida de cobertura del Candidato; limpieza segura;
y tag §11.6. Las suites que componen Full y los invalidadores se consultan en `AGENTS.md`.

Si main se mueve después del cierre se aplica **ruta R**:

1. conservar la ronda anterior en evidencia;
2. retirar el cierre anterior del tip activo de producto sin borrar la evidencia conservada;
3. rebasar el producto sobre main;
4. publicar el Candidato rebasado **solo**, antes de recrear cierre;
5. repetir READY, conformidad, Full, CI y Owner Validation que correspondan sobre los nuevos SHAs;
6. crear y publicar un cierre nuevo, con evidencia propia;
7. volver a verificar main e integrar.

La CI del cierre nunca sustituye la CI del Candidato. Igualdad de árbol no permite reutilizar evidencia.
Una corrección posterior al merge vuelve por la rama y por una ronda completa; nunca se arregla main
directamente. Cobertura, exact-SHA, post-merge CI y limpieza posterior a ambas verificaciones se conservan.
La población y la política de cobertura no cambian.

### 11.6 Tag de integración

Cada unidad V2 termina con un tag **anotado** `integration/<unit>`. Una ronda correctiva usa
`integration/<unit>-corr<N>`, donde N es entero positivo sin ceros iniciales y aumenta por orden
numérico. Los tags publicados nunca se mueven, sobrescriben ni fuerzan. Una corrección contiene el
bloque completo, `Corrects` con el tag/objeto anterior y `Correction reason`.

`MERGE_SHA` significa únicamente el merge de la ronda verificada y es el commit destino del tag.
Merges anteriores no verificados se enumeran aparte con candidate, closure y motivo. El mensaje exige,
una vez cada una:

```text
Initiative: <I>
Unit: <unit>
Workflow: V2
Claim-Id: <uuid>
FINAL_CANDIDATE_SHA: <40 hex>
CLOSURE_SHA: <40 hex>
MERGE_SHA: <40 hex de la ronda verificada>
Unverified merges: none | <merge / candidate / closure / motivo>
Post-merge CI: run=<id> event=push ref=refs/heads/main head_sha=<MERGE_SHA> jobs=<resultados> coverage-artifact=<identidad>
Candidate coverage: run=<id> event=workflow_dispatch candidate_sha=<FINAL_CANDIDATE_SHA> measured-sha=<mismo SHA> artifact=<identidad>
Cleanup: local-branch=<resultado> remote-branch=<resultado> worktree=<resultado> date=<fecha>
Evidence: docs/automation/evidence/<unit>-evidence.md
Corrects: none | <tag anterior y objeto>
Correction reason: none | <razon verificable>
```

La validación comprueba tipo `tag`, regex anclada del nombre, destino en first-parent de main, padres
del merge, ascendencia del Candidato, claves únicas, evidencia y cadena de correcciones. Tag ausente,
ligero, malformado, con destino incorrecto o cadena inválida es desviación de proceso; datos no
recuperables son `UNKNOWN` y se escalan. Una corrida disparada por `refs/tags/*` no acredita evidencia
de Candidate, rama ni post-merge.

La protección activa contra actualización y borrado de `integration/*`, incluido patrón, restricciones
y bypass, es **obligatoria antes de la activación**. Su creación/verificación es una acción
administrativa separada; esta sección no crea el ruleset.

`integration/I-56` es el registro excepcional de activación autorizado bajo Workflow V1. Conserva el
bloque completo anterior con `Workflow: V1` y añade, una vez cada uno:

```text
WORKFLOW_V2_EFFECTIVE_SHA: <merge normativo derivado según §11.2>
PRE: <referencia al snapshot completo en el cuerpo del merge normativo>
POST: <snapshot y tabla completos>
Claim pause:
  start=<registro durable y fecha de inicio>
  end=<registro durable y fecha de fin>
  decision=docs/automation/decisions/I-56.md
```

El tag se crea después de las compuertas V1 4.5.6/4.5.7 y de la limpieza. Su destino `MERGE_SHA`
puede ser el merge de una ronda correctiva, pero `WORKFLOW_V2_EFFECTIVE_SHA` sigue siendo el primer
merge normativo derivado por §11.2; el tag lo registra y nunca lo redefine. Para I-56, `POST` completo
y los tres valores de `Claim pause` son contenido obligatorio del mensaje durable. `start` debe
acreditar un inicio anterior al paso V1 4.5.1; `end` solo puede registrarse tras completar y verificar
V1 4.5.6/4.5.7. Ausencia de inicio o fin es `UNKNOWN` y registro no conforme, nunca `none`. Mientras la
pausa esté activa solo proceden excepciones de emergencia/fix aprobadas explícitamente por el Owner;
un bloqueo prolongado vuelve al Owner conforme a §11.7.

### 11.7 Pausa obligatoria de activación

La existencia de esta sección **no inicia la pausa**. Su inicio se registra de forma durable en
`docs/automation/decisions/I-56.md` antes del paso V1 4.5.1 de la integración normativa. Desde ese
momento se suspenden los reclamos nuevos ordinarios. Solo puede continuar una excepción de
emergencia/fix aprobada explícitamente por el Owner.

La pausa termina únicamente después de completar y verificar los pasos V1 4.5.6 y 4.5.7 y registrar
durablemente su fin en el registro de activación `integration/I-56`. Un bloqueo prolongado vuelve al
Owner; no autoriza relajar la pausa. Hasta el registro durable de fin, la pausa sigue activa.
