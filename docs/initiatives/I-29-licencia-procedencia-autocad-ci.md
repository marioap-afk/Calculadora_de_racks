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

Estado: **cerrada documentalmente**. El 2026-07-20 Mario Perez, Coordinador de Desarrollo de
Proyectos de Industrias Montilla y Owner de RackCad, selecciono **B. Aprobado con restricciones**.
Es una aceptacion interna de riesgo para uso interno; no es una conclusion juridica ni una
afirmacion de autorizacion expresa de Autodesk. I-13 y el merge continúan bloqueados.

## 1. Objetivo

Obtener una decision interna fechada sobre el uso de paquetes Autodesk en runners GitHub-hosted y
su efecto sobre I-13 y la aplicacion de ADR-0003.

## 2. Problema

I-13 demostro tecnicamente una compilacion del Plugin sin AutoCAD instalado, pero el gate L2 conserva
ambiguedad material sobre licencia, procedencia, copias efimeras y terceros que alojan el build. La
promocion en CI #54 no convierte esa evidencia tecnica en autorizacion legal.

I-29 acepta esa incertidumbre residual solo bajo las restricciones registradas en ADR-0003. La
vigencia inicia el 2026-07-20 y exige revision ante un cambio material o, como maximo, el 2027-07-20.

## 3. Alcance

- licencia ObjectARX y composicion mixta de los assemblies;
- restore efimero en runners GitHub-hosted;
- nuget.org, firmas, propietario verificado y metadata de lock;
- caching, artifacts y feeds privados;
- distribucion interna/externa, avisos y atribucion; y
- necesidad de licencia AutoCAD por entorno de build.

### Caracterizacion tecnica corregida

Los trece assemblies tienen composicion mixta. `AcCui`, `AcDx`, `AcMr`, `AcSeamless`, `AcWindows`,
`AdUIMgd` y `AdUiPalettes` contienen `ReferenceAssemblyAttribute`. `AcMgd`, `AcTcMgd`, `AdWindows`,
`AcCoreMgd`, `AcDbMgd` y `acdbmgdbrep` no lo contienen. Las tres referencias principales del Plugin
—`AcMgd`, `AcCoreMgd` y `AcDbMgd`— carecen del atributo y contienen cuerpos de metodos.

La composicion heterogenea no invalida E1/E2 ni las guardas de compilacion y no constituye una
conclusion legal.

## 4. Fuera de alcance

- implementacion tecnica o cambios de codigo;
- compra de licencias;
- AutoCAD 2026/2027;
- certificacion de runtime; y
- interpretacion legal por Codex.

## 5. Contexto requerido

Leer el ADR-0003 aceptado en la rama de promocion; la evidencia I-13 en la rama congelada
`experiment/refs-autocad-ci`;
los Context Packs `autocad-plugin` y `delivery-validation`; la guia de despliegue; y la evidencia de
E2/CI #50 y de la promocion/CI #54.

## 6. Dependencias y responsables

- **Owner, reviewer y aprobador:** Mario Perez, Coordinador de Desarrollo de Proyectos, Industrias
  Montilla. La concentracion de roles fue aceptada para esta decision interna.
- **Entradas requeridas:** I-13, ADR-0003 aceptado, evidencia E2 y promocion CI #54. I-13 aporta
  evidencia, pero no es una dependencia que deba integrarse: su merge espera precisamente a I-29.

La rama canonica `docs/licencia-procedencia-autocad-ci` queda congelada como evidencia P1-P4.
Su commit final es `2fa1d5b9716a601eea3d6f0fd8d9e90658c29fbf`.

## 7. Archivos esperados

La decision interna quedo versionada en la rama canonica y ADR-0003 registra su aplicacion. No se
esperan cambios de producto, CI ni paquetes.

## 8. Fases

1. Identificar reviewer competente y las fuentes vigentes — completada.
2. Responder las quince preguntas de cierre con fuente y responsable — completada.
3. Registrar usos permitidos/prohibidos, obligaciones y fecha de revision — completada.
4. Emitir una salida suficiente para aprobar, restringir, rechazar o escalar — B seleccionada.

### Preguntas de cierre

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

## 9. Pruebas y builds

No requiere build del Plugin ni AutoCAD. Validar Markdown, enlaces, `git diff --check` y el CI
documental de la rama. La evidencia tecnica existente no se repite.

## 10. Validacion manual

El Owner registro responsable, fuentes, respuestas, restricciones, obligaciones, vigencia y fecha de
revision. La validacion en AutoCAD no aplica a esta iniciativa documental.

## 11. Criterios de aceptacion

La decision interna fechada identifica responsable y fuentes, define usos permitidos y prohibidos,
caching, artifacts, obligaciones y fecha de revision, y decide el efecto sobre I-13/ADR-0003. La
salida registrada es aprobar la excepcion con restricciones. Las salidas posibles eran:

- aprobar la excepcion;
- aprobarla con restricciones;
- rechazarla y ejecutar rollback; o
- requerir asesoria externa.

## 12. Condiciones para detenerse

La decision se revoca o vuelve a revision ante un cambio de proyecto, versiones, source, runner,
caching, artifacts, audiencia, finalidad, guardas o documentacion incompatible de Autodesk. I-29
no autoriza por si sola el merge de `architecture/referencias-autocad-ci`.

## 13. Estado versionado y entrega del Pull Request

I-29 esta completada en su rama canonica, con automatizacion deshabilitada y sin merge automatico.
La rama se conserva en solo lectura. `completed` no significa integrada; cualquier integracion sera
una decision manual conforme a WORKFLOW.

## 14. Evidencia final

La evidencia final registra decision B, Owner, fuentes, quince respuestas, usos permitidos y
prohibidos, politica de caching/artifacts, obligaciones, vigencia y revision. ADR-0003 aplica ahora
la decision; I-13 permanece abierta y el merge bloqueado hasta la revalidacion y autorizacion final.

### Fuentes minimas para la revision

- licencia incluida en los paquetes;
- documentacion Autodesk APS/ObjectARX;
- paginas de los paquetes NuGet y sus firmas;
- documentacion de GitHub Actions; y
- politicas internas aplicables.
