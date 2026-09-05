---
schema: rackcad-initiative/v1
id: I-45
title: Engineering Productivity — arquitectura de pruebas y workflow de validacion
type: architecture
status: in-progress
branch: architecture/test-validation-workflow
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [delivery-validation, documentation-governance]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# Engineering Productivity — arquitectura de pruebas y workflow de validacion

> **Fase actual: DISCOVERY.** Esta iniciativa investiga y propone; **no implementa**.
>
> ```
> Phase:    DISCOVERY
> Baseline: parent of the atomic claim commit on origin/main
>
> NO IMPLEMENTATION BEFORE CONSENSUS
>
> Coordinator <-> Architect
> proposal <-> critical review <-> adjustments <-> reconciliation <-> CONSENSUS
>
> Implementation READY only when:
>   Coordinator = AGREED
>   Architect   = AGREED
>   same PLAN_VERSION
>   Open disagreements = NONE
> ```
>
> Mientras esas cuatro condiciones no se cumplan **sobre la misma version concreta del plan**, ningun
> cambio de la seccion 4 esta autorizado, aunque el Discovery ya haya medido que seria beneficioso.
> Un hallazgo no es una autorizacion.

## 1. Objetivo

Entender, con evidencia medida, **por que cuesta lo que cuesta validar un cambio en RackCad**, y
producir despues un plan acordado para reducir ese coste sin perder capacidad de deteccion.

El resultado verificable de la fase actual es un unico artefacto: una **Proposal** versionada que
enuncie el problema medido, las alternativas consideradas con su evidencia, la recomendada, sus
riesgos, su plan de despliegue y su plan de reversion — y el registro de la **reconciliacion** con el
Arquitecto hasta alcanzar consenso explicito. La implementacion es un alcance posterior y separado,
que esta iniciativa **no** ejecuta sin ese consenso.

## 2. Problema

Validar un cambio obliga hoy a pagar repetidamente la misma evidencia, y hasta este Discovery el
repositorio **no tenia ni un solo tiempo registrado**: lleva un registro cuidadoso de conteos de
pruebas y ninguno de duracion, de modo que el coste que motiva esta iniciativa nunca habia sido
medido. Las mediciones preliminares del Discovery (aun **no versionadas**; publicarlas es la fase 1)
apuntan a cuatro hechos:

1. **El reloj de pared del CI no lo gobierna el volumen de pruebas, sino un punto de serializacion.**
   La suite de UI cuesta del orden de doce veces mas por prueba que la del nucleo, y la causa medida
   no es construir ventanas WPF: es que mas del ochenta por ciento de sus pruebas se marshalan sobre
   **un unico hilo STA compartido** (`tests/RackCad.UI.Tests/StaTestRunner.cs`, un `Dispatcher`
   estatico invocado de forma sincrona). Las pruebas que no lo tocan son ~400 veces mas baratas, y
   forzar `xUnit.MaxParallelThreads=1` **acelera** la suite: el paralelismo actual solo aniade
   contencion sobre un embudo.
2. **La obligacion de correr todo esta escrita, no heredada.** Vive en cuatro capas independientes:
   [`AGENTS.md`](../../AGENTS.md) seccion «Pruebas — definicion de terminado»; la precedencia de
   [`WORKFLOW.md`](../WORKFLOW.md) seccion 10, que eleva `dotnet test` por encima de todo documento y
   por tanto obliga a pagarla solo para **saber donde estas**; el checklist de cierre de
   `WORKFLOW.md` seccion 5; y `ci.yml`, que dispara sus cuatro jobs en **todo** push. Ademas, la
   mayoria de los contratos de `docs/initiatives/` la reinscriben a mano, porque
   [`TEMPLATE.md`](TEMPLATE.md) no la prescribe.
3. **No existe taxonomia sobre la que construir una seleccion.** Las dos suites no declaran ningun
   `[Trait]` ni `IClassFixture`; las unicas dos `[CollectionDefinition]` del repositorio existen para
   **desactivar** paralelismo, no para seleccionar. La suite del nucleo es un directorio plano bajo un
   solo namespace.
4. **El grafo de compilacion no describe el impacto real.** Varios archivos de `tests/RackCad.Tests`
   verifican `src/RackCad.Plugin` y `src/RackCad.UI` **leyendo su texto**, y ningun `csproj` de
   pruebas referencia esos proyectos; tres guardas aseveran ademas sobre la prosa de documentos de
   `docs/`. Un selector construido sobre referencias de proyecto concluiria justo lo contrario de la
   verdad.

Existe ademas un **sospechoso mayor todavia sin medir**: el ciclo de validacion manual del Owner.
`docs/guias/validacion-manual-autocad.md` exige la suite completa y dos builds **antes de cada
carga**, y el historial de `docs/automation/` registra iniciativas con varias rondas y checklists de
decenas de filas. Si ese ciclo domina el coste total, una estrategia de seleccion de pruebas
atacaria el termino menor de la suma. Discovery **debe** medirlo antes de proponer.

**Absorcion declarada.** El item «CI por capas» de [`ideas-futuras.md`](../ideas-futuras.md) queda
absorbido por esta iniciativa **como problema y como alternativa a estudiar**. Absorberlo no decide
nada: no significa que la solucion final sea CI por capas.

## 3. Alcance

Fase de **DISCOVERY**, y solo eso:

- **Medir** el coste real de validar: CI (reloj de pared, por job, y el reparto restore/build/
  ejecucion), ciclo local obligatorio, perfil por prueba y por clase de las dos suites, y el ciclo de
  validacion manual del Owner.
- **Inventariar** la arquitectura de pruebas vigente: taxonomia (o su ausencia), guardas de codigo
  fuente, goldens, dependencias por texto y por datos, y el mapa empirico prueba -> codigo derivado
  de la cobertura que el CI **ya publica**.
- **Versionar** esa evidencia en `docs/automation/evidence/` para que sea auditable y reutilizable.
- **Redactar** la Proposal con alternativas, recomendacion, riesgos, despliegue y reversion.
- **Reconciliar** con el Arquitecto hasta consenso explicito, registrando acuerdos y desacuerdos.
- **Corregir** las derivas documentales que el propio Discovery detecte **solo cuando sean errores de
  hecho comprobables** y no impliquen decision de disenio (por ejemplo, una afirmacion de tiempo
  obsoleta en `AGENTS.md`, o una regla atribuida a un documento que no la contiene). Cualquier cambio
  de este tipo se propone al Owner antes de escribirlo.

Todo cambio de esta fase es **documental**. No se toca codigo de producto, ni codigo de pruebas, ni
configuracion de CI.

## 4. Fuera de alcance

Fuera de **este gate y de toda la fase DISCOVERY**, aunque el Discovery mida que serian beneficiosos:

- cambios de producto;
- cambios funcionales de cualquier tipo;
- cambios de CI (`.github/workflows/**`, `eng/**`);
- refactors de pruebas;
- borrado o consolidacion de pruebas;
- cambios de paralelismo (incluido `xUnit.MaxParallelThreads`, `CollectionDefinition` y el disenio de
  `StaTestRunner`);
- introduccion de `[Trait]` o de cualquier taxonomia de pruebas;
- implementacion de tiers, Quick CI / Full CI, seleccion por impacto o reutilizacion de evidencia;
- Golden DWG;
- **cualquier optimizacion basada en los hallazgos del Discovery**.

Fuera del alcance de la iniciativa completa, tambien tras el consenso: cambiar el criterio de
**cobertura funcional** del producto, relajar la validacion manual del Owner en AutoCAD, y sustituir
la decision del dueno sobre que se considera «terminado».

## 5. Contexto requerido

- [`AGENTS.md`](../../AGENTS.md) — convenciones obligatorias y «Pruebas — definicion de terminado».
- [`docs/WORKFLOW.md`](../WORKFLOW.md) — ciclo de iniciativa, checklist de cierre, archivos calientes
  y precedencia de documentos.
- [`docs/AUTOMATION_PLAN.md`](../AUTOMATION_PLAN.md) — reclamo atomico, estado versionado y limites.
- [`docs/ROADMAP.md`](../ROADMAP.md) — fila de I-45 y dependencias.
- [`docs/initiatives/README.md`](README.md) y [`TEMPLATE.md`](TEMPLATE.md) — contrato de iniciativas.
- [`docs/ideas-futuras.md`](../ideas-futuras.md) — item «CI por capas», absorbido por esta iniciativa.
- Context packs `delivery-validation` y `documentation-governance`.
- `.github/workflows/ci.yml` y `eng/ci/` — **solo lectura** durante DISCOVERY.
- `tests/RackCad.Tests/`, `tests/RackCad.UI.Tests/` y `tests/RackCad.UI.Tests/StaTestRunner.cs` —
  **solo lectura** durante DISCOVERY.

## 6. Dependencias

Ninguna iniciativa debe estar integrada previamente: I-45 no depende de trabajo de producto y su fase
DISCOVERY no toca ningun archivo de codigo, asi que no compite por archivos calientes con ninguna
iniciativa en curso.

Entradas del dueno que **deben existir** antes de pasar de fase:

1. La decision, ya tomada, de que I-45 es iniciativa formal de ROADMAP y de que «CI por capas» queda
   absorbido como problema y no como solucion.
2. El **consenso Coordinator/Architect** sobre una `PLAN_VERSION` concreta. Sin el, la fase de
   implementacion no existe.

## 7. Archivos esperados

Del gate de reclamo y bootstrap, exactamente tres:

- `docs/initiatives/I-45-test-validation-workflow.md` (nuevo) — este contrato.
- `docs/ROADMAP.md` (modificado) — fila de I-45.
- `docs/ideas-futuras.md` (modificado) — «CI por capas» marcado como absorbido.

Del gate de cierre de Discovery:

- `docs/initiatives/I-45-discovery.md` (nuevo) — **la evidencia**, no la Proposal. Es el unico
  artefacto donde viven las mediciones, las reconstrucciones, las hipotesis refutadas y las
  incognitas, cada una con su marcador epistemologico.
- `docs/ROADMAP.md` (modificado) — reubicacion de la fila a la seccion transversal.
- Este contrato (modificado) — retirada del SHA versionado y ajuste de fases.

De las fases posteriores:

- `docs/automation/decisions/I-45.md` — decisiones del dueno, si las hubiera.
- La Proposal y el registro de reconciliacion, en la ruta que el Owner apruebe.

Una desviacion material respecto de estas listas obliga a detenerse.

## 8. Fases

0. **Reclamo y bootstrap documental** — HECHA. Rama, worktree, fila de ROADMAP, contrato y absorcion
   de «CI por capas». Sin evidencia versionada todavia.
1. **Cierre de Discovery y versionado de la evidencia** — HECHA. Publica
   [`I-45-discovery.md`](I-45-discovery.md) con el comando que produjo cada dato y su marcador
   epistemologico, y cierra las cuatro lineas que quedaban abiertas: baseline del ciclo de
   validacion del Owner (D1), precision y falsos positivos de una seleccion por impacto (D2),
   contraindicaciones de una futura estrategia multi-STA (D3) y repeticion historica de Full (D4).
   Corrige ademas la ubicacion de la fila en ROADMAP y retira el SHA de este contrato.
2. **Proposal V1** — problema medido, alternativas con evidencia, recomendacion, riesgos, metricas de
   exito, despliegue y reversion. **No escrita todavia.**
3. **Revision independiente del Arquitecto** — analisis propio, no aprobacion jerarquica; salida con
   acuerdos, desacuerdos, cambios propuestos, riesgos, alternativas y preguntas.
4. **Reconciliacion** — ajustes y nueva version del plan hasta que ambos declaren AGREED sobre la
   misma `PLAN_VERSION`, sin desacuerdos abiertos.
5. **CONSENSUS** — se registra el acuerdo. Solo aqui deja de aplicar la seccion 4, y la
   implementacion se planifica como alcance separado.

Cada fase termina con evidencia revisable. Ninguna fase posterior a la 5 forma parte de esta
iniciativa sin una decision explicita del dueno.

## 9. Pruebas y builds

**Este gate es documental.** No modifica codigo de produccion ni de pruebas, ni la configuracion de
CI, de modo que no existe regla vigente que exija ejecutar las suites completas ni los builds de UI y
Plugin para cerrarlo: el checklist de `WORKFLOW.md` seccion 5 gobierna el **cierre de la iniciativa**,
no cada commit intermedio, y `AGENTS.md` fija la definicion de terminado de un cambio de codigo.

La validacion proporcional de un cambio documental es:

- comprobar que ninguna prueba lee como texto los documentos modificados — las guardas de fuente
  vigentes aseveran sobre `docs/adr/`, `docs/initiatives/I-37B-*` y `docs/automation/decisions/I-37.md`,
  ninguno de los cuales entra en este gate;
- comprobar que el arbol queda limpio y que `main` no fue modificada;
- dejar que el CI del push corra, que es la compuerta real segun `WORKFLOW.md` seccion 4.5.

Durante DISCOVERY, cualquier ejecucion de suites es **medicion**, no gate: se ejecuta para producir
evidencia y sus resultados se versionan como tales.

## 10. Validacion manual

**No aplica.** I-45 no cambia dibujo, BOM, GUID, persistencia, catalogos ni ninguna superficie
observable en AutoCAD. `requires_autocad: false` y `requires_owner_validation: false`.

Lo que **si** requiere el dueno es una **decision**: el consenso sobre la `PLAN_VERSION`
(`requires_owner_decision: true`).

## 11. Criterios de aceptacion

De este gate:

1. `origin/architecture/test-validation-workflow` existe y contiene el commit de reclamo con su
   `Claim-Id`.
2. I-45 tiene fila en `docs/ROADMAP.md`.
3. Este contrato existe y declara fase, baseline **sin hash versionado** (`WORKFLOW.md` seccion 8),
   el bloqueo por consenso y la lista de la seccion 4.
4. «CI por capas» aparece en `ideas-futuras.md` marcado como absorbido por I-45, sin presentarse como
   decision arquitectonica.
5. Cero cambios en codigo de producto, codigo de pruebas y configuracion de CI.
6. `main` no fue modificada.

De la fase DISCOVERY completa: evidencia versionada, huecos de medicion cerrados, Proposal escrita,
revision del Arquitecto realizada y consenso registrado — o, si no se alcanza, el desacuerdo
documentado con su motivo.

## 12. Condiciones para detenerse

- Cualquier tentacion de implementar antes del consenso, incluida una «mejora obvia» que el Discovery
  acabe de medir.
- Una medicion que contradiga un hallazgo ya publicado: se corrige el hallazgo antes de seguir.
- Un cambio de alcance que empuje hacia codigo de producto, de pruebas o de CI.
- Un conflicto entre estas instrucciones y `AGENTS.md` o `WORKFLOW.md`: se reporta la regla concreta
  con su evidencia y se espera decision del dueno.
- La aparicion de otra iniciativa que reclame los mismos archivos documentales.

## 13. Estado versionado y entrega del Pull Request

`automation.enabled: false` y `automation_state_path` vacio: I-45 se conduce manualmente, igual que
las iniciativas recientes del mismo tipo, de modo que **no** se crea
`docs/automation/state/I-45.yml` y **no** se abre Pull Request. El estado en curso se deriva de la
existencia de `origin/architecture/test-validation-workflow`, conforme a `WORKFLOW.md` seccion 7.

Si el dueno decidiera pasar la iniciativa al ejecutor automatico, ese cambio exige actualizar el
frontmatter y crear el archivo de estado con el esquema de `TEMPLATE.md` seccion 13; hasta entonces,
Git es la unica fuente de estado.

El merge automatico esta prohibido, como en toda iniciativa del repositorio.

## 14. Evidencia final

Se completa al cerrar la iniciativa. De momento consta unicamente el gate de reclamo y bootstrap:
rama y worktree creados desde la punta remota de `main`, commit vacio de reclamo con `Claim-Id`,
primer push aceptado sin force, y los tres archivos documentales de la seccion 7.

**Cero cambios de producto, de pruebas y de CI. `main` no fue modificada.**
