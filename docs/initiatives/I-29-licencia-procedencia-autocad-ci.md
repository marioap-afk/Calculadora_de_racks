---
schema: rackcad-initiative/v1
id: I-29
title: Licencia y procedencia de referencias AutoCAD para CI
type: docs
status: completed
branch: docs/licencia-procedencia-autocad-ci
base_branch: main
priority: 10
size: S
depends_on: []
conflicts_with: []
context_packs:
  - autocad-plugin
  - delivery-validation
automation_state_path:
decision_paths:
  - docs/adr/0003-referencias-autocad-para-ci.md
  - docs/initiatives/I-29-plantilla-decision.md
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-29 — Licencia y procedencia de referencias AutoCAD para CI

## 1. Identificacion y estado actual

| Campo | Valor |
|---|---|
| Iniciativa | I-29 |
| Rama canonica | `docs/licencia-procedencia-autocad-ci` |
| Worktree canonico | `D:\Documentos\Codex\docs-licencia-procedencia-autocad-ci` |
| Base | `1ffebcb07553661acba2eac4a0722c8781666bdf` |
| Commit de reclamo | `715d473721d216b55b21fc4aa80eea13da218371` |
| Claim-Id | `526b69aa-a56e-4da4-acd7-b96d0d8d1409` |
| Iniciativa relacionada | I-13 — Referencias de AutoCAD para CI |
| Gate | L2 — decision interna B registrada; gobernanza tecnica posterior bloqueada |
| ADR relacionado | ADR-0003, estado `propuesto` |

I-29 completo P1-P4. El 2026-07-20 Mario Pérez, Coordinador de Desarrollo de Proyectos de Industrias
Montilla y Owner de RackCad, selecciono **B. Aprobado con restricciones** como decision interna de
gestion del riesgo. La decision sustituye la recomendacion preliminar D de P3, sin borrarla del
historial. No es una conclusion juridica ni afirma autorizacion expresa de Autodesk. I-13 y el merge
de `architecture/referencias-autocad-ci` continúan bloqueados. Este documento no acepta el ADR ni
activa una excepcion a cero NuGet.

## 2. Problema

I-13 demostro tecnicamente que `RackCad.Plugin` puede compilarse en un runner Windows alojado por
GitHub sin AutoCAD instalado, usando tres paquetes Autodesk como referencias condicionales de
compilacion. La prueba tecnica no decide si la licencia y los acuerdos aplicables permiten la
descarga, las copias efimeras, el uso de infraestructura de terceros o cada modalidad de
almacenamiento evaluada.

La propuesta tambien contradice la regla vigente de cero paquetes NuGet en codigo de producto.
Por ello requiere una decision interna atribuida, un ADR aceptado cuando corresponda y controles
que separen compilacion, runtime y distribucion.

## 3. Objetivo

Registrar una decision interna fechada y verificable del Owner sobre el uso restringido de los
paquetes Autodesk evaluados, y registrar su efecto sobre I-13, ADR-0003 y la politica cero NuGet.

El [paquete de decision](I-29-paquete-decision-interna.md) presenta el caso para revision y la
[plantilla](I-29-plantilla-decision.md) registra la respuesta futura.

## 4. No objetivos

- interpretar definitivamente una licencia o emitir asesoramiento legal;
- elegir una salida en nombre del reviewer o del aprobador;
- modificar producto, CI, proyectos, paquetes, versiones o dependencias;
- aceptar o modificar ADR-0003;
- integrar o cerrar I-13;
- ejecutar AutoCAD o sustituir sus licencias; y
- autorizar redistribucion, caching persistente o uso de versiones no evaluadas.

## 5. Alcance

- licencia ObjectARX y composicion tecnica mixta de los assemblies observados;
- restore desde nuget.org en un runner GitHub-hosted efimero;
- cache local `NUGET_PACKAGES` durante un job;
- metadata, lock, hashes y firmas;
- caching persistente, artifacts y feeds privados;
- distribucion interna o externa de RackCad sin material Autodesk;
- avisos y atribucion; y
- posible necesidad de licencia AutoCAD por entorno de build.

Las versiones evaluadas son `AutoCAD.NET` 25.0.1, `AutoCAD.NET.Core` 25.0.0 y
`AutoCAD.NET.Model` 25.0.0. Otras versiones requieren una nueva evaluacion.

## 6. Salidas posibles

La decision seleccionada por el Owner es:

- A. Aprobado — no seleccionado.
- **B. Aprobado con restricciones — seleccionado.**
- C. Rechazado — no seleccionado.
- D. Requiere asesoria legal externa — recomendacion preliminar historica de P3, no seleccionada.

## 7. Roles requeridos

| Rol | Responsabilidad | Identificacion actual |
|---|---|---|
| Owner | Patrocinar la solicitud y confirmar su alcance organizacional | Mario Pérez, Coordinador de Desarrollo de Proyectos, Industrias Montilla |
| Technical preparer | Reunir evidencia tecnica y declarar sus limites | Mario Pérez, Coordinador de Desarrollo de Proyectos, Industrias Montilla |
| Internal licensing reviewer | Realizar la revision interna de riesgo, licencia y gobernanza | Mario Pérez, Coordinador de Desarrollo de Proyectos, Industrias Montilla |
| Final approver | Seleccionar A/B/C/D y gobernar sus efectos | Mario Pérez, Coordinador de Desarrollo de Proyectos, Industrias Montilla; decision B registrada |

Mario Pérez ocupa los cuatro roles. El Owner acepta esta concentracion para I-29, pero no existe
independencia entre preparer, reviewer y approver. La revision interna no es asesoria legal
profesional; la salida D permanece disponible cuando se requiera criterio juridico externo.
La autoridad registrada se limita a la gestion interna del proyecto RackCad. La fecha es
2026-07-20 y el mecanismo verificable es la instruccion escrita del Owner incorporada al registro y
commit P4. No se declara autoridad juridica profesional ni autoridad corporativa adicional. Los
conflictos de interes no fueron declarados en la instruccion.

## 8. Evidencia tecnica disponible

- E1: build local aislado, control negativo, versiones exactas y bundle sin DLL Autodesk.
- E2: build declarado exitoso en GitHub Actions sobre runner Windows sin AutoCAD, con cache aislado,
  restore bloqueado, referencias `Private=false` y `CopyLocal=false`, y cero material Autodesk fuera
  del cache del job.
- Promocion limpia: commit tecnico
  `fae0b150fa3bb4f3c9ea6d5473c027c021e9a3c2` y commit documental
  `31e146ded403fefc45f1b7e7302c98957773fec8`.
- CI #54 y #55: declarados verdes en el encargo y la evidencia relacionada, pero no verificados de
  forma independiente durante P1.

La fuente detallada de E1/E2 es `docs/initiatives/I-13-referencias-autocad-ci.md` en
`experiment/refs-autocad-ci`. La promocion y sus archivos se leen desde
`architecture/referencias-autocad-ci`; ninguna de esas ramas fue modificada por P1.

## 9. Revalidacion independiente

P3 revalido de forma independiente los paquetes exactos el 2026-07-20. Coincidieron los SHA-256 de
E1, los SHA-512 del catalogo NuGet, el contenido y hash de `LICENSE.txt`, las firmas CMS de autor y
repositorio, el owner mostrado y `verified=false`. La inspeccion conto trece DLL y corrigio su
caracterizacion:

> Los paquetes auditados contienen una composicion mixta: siete assemblies estan marcados mediante
> `ReferenceAssemblyAttribute` y seis no contienen esa marca. Las tres referencias principales
> utilizadas por `RackCad.Plugin` —`AcMgd`, `AcCoreMgd` y `AcDbMgd`— no contienen
> `ReferenceAssemblyAttribute` y contienen cuerpos de metodos.

La presencia o ausencia del atributo es una propiedad tecnica, no una licencia. La revalidacion no
conservo paquetes, DLL ni temporales. La evidencia, sus limites y la evaluacion de las quince
preguntas se registran en la [matriz maestra](I-29-matriz-evidencia-evaluacion.md).

## 10. Capas que no deben confundirse

| Capa | Pregunta que responde | Limite |
|---|---|---|
| Funcionamiento tecnico | ¿El mecanismo compila y mantiene limpio el output? | No concede permisos de uso |
| Integridad | ¿Los bytes coinciden con hashes o firmas auditados? | No define el alcance contractual |
| Procedencia | ¿Que canal, owner y firmas se observaron? | No equivale por si sola a autorizacion legal |
| Politica interna | ¿RackCad admite hoy esa dependencia? | La excepcion aun no esta vigente |
| Autorizacion legal/contractual | ¿La organizacion puede usar el material de esta manera? | Solo la decide un responsable competente |

## 11. Quince preguntas de cierre

Estas preguntas se copian sin reformular del contrato preparado en la rama de promocion:

1. ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted?
2. ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida?
3. ¿Puede GitHub actuar como proveedor de infraestructura para ese uso?
4. ¿Puede usarse `actions/cache`?
5. ¿Puede conservarse `packages.lock.json`?
6. ¿Pueden conservarse `contentHash` y hashes?
7. ¿Es admisible que el paquete contenga assemblies de implementacion?
8. ¿NuGet es un canal autorizado por Autodesk?
9. ¿`verified=false` requiere validacion adicional?
10. ¿Puede usarse un feed privado?
11. ¿Puede usarse un runner autohospedado?
12. ¿Hay obligaciones de avisos o atribucion?
13. ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado?
14. ¿Difiere uso interno de distribucion externa?
15. ¿Se requiere una licencia AutoCAD por cada entorno de build?

Las propuestas de P3 se conservan como historial. P4 registra la decision B del Owner y las
respuestas operativas en la [plantilla de decision](I-29-plantilla-decision.md). Las incertidumbres
contractuales se aceptan como riesgo residual interno; no se presentan como resueltas juridicamente.

## 12. Estado de ADR-0003

ADR-0003 existe solo en `architecture/referencias-autocad-ci` y su estado textual es `propuesto`.
No es politica vigente de `main`, no autoriza el mecanismo y no fue modificado por P1-P4. Una decision
suficiente debe indicar si puede aceptarse, debe restringirse, debe rechazarse o requiere trabajo
adicional; el cambio de estado corresponde a una fase posterior autorizada.

## 13. Estado de la politica cero NuGet

`AGENTS.md`, `docs/ARCHITECTURE.md` y `docs/HANDOFF.md` mantienen la regla: cero paquetes NuGet en
codigo de producto y toda excepcion requiere decision explicita. El `PackageReference` condicional
en `RackCad.Plugin` sigue siendo una excepcion propuesta, aunque no sea dependencia runtime ni se
copie al bundle. P1-P4 no cambian esa politica.

## 14. Criterio de suficiencia

Una decision suficiente debe incluir:

- fecha, persona identificada, cargo, organizacion y autoridad;
- mecanismo verificable de aprobacion y fuentes revisadas;
- respuestas expresas sobre hosted runners, nuget.org, copia efimera, caching, lock y artifacts;
- alcance de distribucion/redistribucion, uso interno/externo, avisos y atribuciones;
- paquetes, versiones y materiales cubiertos;
- vigencia, revision anual, responsable de cumplimiento y conflictos de interes;
- efecto explicito sobre ADR-0003, politica cero NuGet, I-13 y su merge; y
- condiciones de rollback, revocacion o cambio de version.

No bastan una conversacion informal, una aprobacion verbal, una respuesta anonima o sin autoridad,
una respuesta sin fecha, una respuesta que omita caching/artifacts/distribucion, ni una conclusion
tecnica presentada como autorizacion legal.

P4 satisface este criterio como registro interno de gestion del riesgo mediante la instruccion
escrita del Owner. No satisface ni pretende sustituir una conclusion juridica externa.

## 15. Fases

| Fase | Estado | Resultado |
|---|---|---|
| P1 — Crear paquete documental | Completada | Contrato, paquete, plantilla e indice publicados |
| P2 — Entregar al reviewer interno | Completada | Entrega interna versionada, receptor y concentracion de roles registrados |
| P3 — Evaluar evidencia y preparar recomendacion | Completada preliminarmente | Revalidacion, matriz, quince propuestas y recomendacion no vinculante |
| P4 — Registrar decision final de I-29 | Completada | Decision B, restricciones, vigencia y revision registradas; ADR/I-13/merge sin modificar |

## 16. Bloqueos posteriores a I-29

- ADR-0003 permanece propuesto;
- la politica cero NuGet permanece vigente y la excepcion tecnica no esta activa;
- I-13 permanece abierta y bloqueada;
- la promocion no tiene autorizacion de merge;
- cualquier incumplimiento de las catorce restricciones revoca el alcance aprobado; y
- cualquier version, fuente, audiencia, runner, finalidad o documentacion incompatible exige nueva
  revision antes de trabajo tecnico o integracion.

## 17. Trazabilidad minima

| Hito | Referencia | Estado |
|---|---|---|
| Evidencia experimental I-13 | `experiment/refs-autocad-ci` @ `4e084d250c5385f04ec452f5a17499ff68a42367` | Solo lectura; no integrada |
| Promocion tecnica | `fae0b150fa3bb4f3c9ea6d5473c027c021e9a3c2` | Merge bloqueado |
| Promocion documental | `31e146ded403fefc45f1b7e7302c98957773fec8` | ADR propuesto; merge bloqueado |
| Reclamo I-29 | `715d473721d216b55b21fc4aa80eea13da218371` | Publicado |
| P1 | `195cc8b26e58e191eeb4c3f5af8fa325ad43a77d` | Completada y publicada |
| P2 | [Registro de entrega](I-29-registro-entrega-revision.md) | Entrega registrada |
| P3 | [Matriz maestra](I-29-matriz-evidencia-evaluacion.md) y [hoja de revision](I-29-hoja-revision-interna.md) | Evaluacion preliminar y recomendacion D conservadas como historial |
| P4 | [Plantilla de decision](I-29-plantilla-decision.md) y [registro](I-29-registro-entrega-revision.md) | Decision B registrada; iniciativa documental cerrada |

## 18. Pruebas y entrega

P4 requiere validacion documental, enlaces relativos, `git diff --check` y confirmacion de que solo
se tocaron los siete documentos I-29 autorizados. No requiere build, restore, AutoCAD ni CI. Su push
no equivale a integracion: ADR-0003 permanece propuesto, cero NuGet sigue vigente, I-13 sigue abierta
y el merge continua bloqueado.
