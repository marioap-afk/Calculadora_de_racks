# WORKFLOW — flujo Git, worktrees y trabajo multi-agente

> Actualizado: 2026-07-17 (I-00 ejecutada: `main` es el trunk único).
> Proceso repetible para que varias IAs (Claude, Codex, futuras) y el humano desarrollen RackCad en
> paralelo durante años sin pisarse. Las ramas se nombran por INICIATIVA técnica, nunca por
> herramienta ([ADR-0001](adr/0001-ramas-por-iniciativa.md)). El plan de iniciativas vive en
> [ROADMAP.md](ROADMAP.md); la migración inicial ya se ejecutó (nota en la sección 9).

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
- **Eliminar** el worktree en el mismo acto en que su rama muere (integración o abandono formal),
  con **borrado seguro por defecto**: `git worktree remove` + `git branch -d` — la `-d` minúscula
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
       → sesión de integración (rebase final → CI → validación → HANDOFF/ROADMAP → merge) → limpieza
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
      (AGENTS.md, «Pruebas — definicion de terminado», punto 1). Es el único SHA que entra a `main`.
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
   - Protección de `main`: contra force-push y borrado, **sin** "required status checks" (el commit
     de merge local no tendría CI previo y GitHub lo rechazaría). El requisito "CI verde en la rama"
     del paso 2 es la compuerta real y la verifica la sesión de integración.
6. **Limpiar**: borrar rama local (`git branch -d` — el merge la contiene), remota (procede: el
   merge ya existe en `main`) y el worktree, según las reglas de borrado seguro de la sección 3.

`experiment/*` tiene un final distinto: se cierra con una **conclusión escrita** (en el ADR o
iniciativa a la que alimenta, o en ideas-futuras.md) y la rama se borra. Su código no se mergea;
si el resultado se adopta, se re-implementa limpio en una rama `architecture/`/`feature/`.

## 5. Checklist de cierre de iniciativa

- [ ] **Las DOS suites** verdes **en local** —Core y UI, no un solo `dotnet test`— y CI verde en la
      rama. El cierre exige ambas en local: LC-UI retira la corrida de UI de la **iteración
      ordinaria**, nunca del cierre ni del Candidato (AGENTS.md, «Pruebas — definicion de terminado»,
      punto 1).
- [ ] Build Debug de UI y Plugin con 0 errores (los MSB3277 conocidos no cuentan).
- [ ] Bugfix ⇒ test de regresión **verificado fallando** sin el fix (AGENTS.md).
- [ ] Cambio de dibujo ⇒ validación manual del usuario en AutoCAD (sección 6).
- [ ] Documentación actualizada según la tabla de la sección 8 (en la MISMA rama).
- [ ] Decisiones tomadas durante la iniciativa registradas como ADR (criterios en
      [adr/README.md](adr/README.md)).
- [ ] Hallazgos fuera de alcance anotados en ideas-futuras.md (no arreglados "de paso").
- [ ] En la sesión de integración: HANDOFF §8-12 + estado en ROADMAP como último commit de la rama;
      tras el merge, rama + worktree borrados.

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
| Conteos de tests / hashes de commit | SOLO `docs/HANDOFF.md` §12 | Nunca copiarlos a otros docs; en ROADMAP la marca de cierre es `integrada (fecha)`, sin hash |

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

## 10. Precedencia de documentos

Normas: `AGENTS.md` (convenciones de código/arquitectura) y este `WORKFLOW.md` (proceso) — si se
contradicen entre sí, gana AGENTS y se corrige aquí. Después: `docs/HANDOFF.md` (estado) >
`docs/ROADMAP.md` (plan) > `README.md` > guías temáticas > documentos históricos/archivo.
Y por encima de todos: el estado real del repo (`git log`, `dotnet test`).
