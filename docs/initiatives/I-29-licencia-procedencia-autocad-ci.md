---
schema: rackcad-initiative/v1
id: I-29
title: Licencia y procedencia de referencias AutoCAD para CI
type: docs
status: pending
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

Estado: **pendiente y bloqueante para el merge de I-13**. Este contrato formula las preguntas para
una revision competente; no contiene interpretacion ni conclusion legal de Codex.

## 1. Objetivo

Obtener una decision interna fechada sobre el uso de paquetes Autodesk en runners GitHub-hosted y
su efecto sobre I-13 y el ADR-0003 propuesto.

## 2. Problema

I-13 demostro tecnicamente una compilacion del Plugin sin AutoCAD instalado, pero el gate L2 conserva
ambiguedad material sobre licencia, procedencia, copias efimeras y terceros que alojan el build. La
promocion en CI #54 no convierte esa evidencia tecnica en autorizacion legal.

## 3. Alcance

- licencia ObjectARX y uso de assemblies de implementacion;
- restore efimero en runners GitHub-hosted;
- nuget.org, firmas, propietario verificado y metadata de lock;
- caching, artifacts y feeds privados;
- distribucion interna/externa, avisos y atribucion; y
- necesidad de licencia AutoCAD por entorno de build.

## 4. Fuera de alcance

- implementacion tecnica o cambios de codigo;
- compra de licencias;
- AutoCAD 2026/2027;
- certificacion de runtime; y
- interpretacion legal por Codex.

## 5. Contexto requerido

Leer el ADR-0003 propuesto; la evidencia I-13 en la rama congelada `experiment/refs-autocad-ci`;
los Context Packs `autocad-plugin` y `delivery-validation`; la guia de despliegue; y la evidencia de
E2/CI #50 y de la promocion/CI #54.

## 6. Dependencias y responsables

- **Owner:** dueno del repositorio.
- **Reviewer:** por designar, persona interna competente en licenciamiento/legal.
- **Entradas requeridas:** I-13, ADR-0003 propuesto, evidencia E2 y promocion CI #54. I-13 aporta
  evidencia, pero no es una dependencia que deba integrarse: su merge espera precisamente a I-29.

La rama futura propuesta es `docs/licencia-procedencia-autocad-ci`; este contrato no la crea.

## 7. Archivos esperados

Una decision interna versionada en la ruta que se defina al reclamar I-29 y, segun su resultado, la
actualizacion o rechazo del ADR-0003. No se esperan cambios de producto, CI ni paquetes.

## 8. Fases

1. Identificar reviewer competente y las fuentes vigentes.
2. Responder las quince preguntas de cierre con fuente y responsable.
3. Registrar usos permitidos/prohibidos, obligaciones y fecha de revision.
4. Emitir una salida suficiente para aprobar, restringir, rechazar o escalar la propuesta.

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

El dueno valida que el responsable, las fuentes, todas las respuestas, restricciones, obligaciones y
fecha de revision sean suficientes. La validacion en AutoCAD no aplica a esta iniciativa documental.

## 11. Criterios de aceptacion

La decision interna fechada identifica responsable y fuentes, define usos permitidos y prohibidos,
caching, artifacts, obligaciones y fecha de revision, y decide el efecto sobre I-13/ADR-0003. Debe
permitir una de estas salidas:

- aprobar la excepcion;
- aprobarla con restricciones;
- rechazarla y ejecutar rollback; o
- requerir asesoria externa.

## 12. Condiciones para detenerse

Detenerse ante falta de reviewer competente, fuentes insuficientes, una respuesta ambigua que no
permita gobernar I-13, expansion a implementacion tecnica o necesidad de asesoria externa. Mientras
I-29 no cierre con decision suficiente, no aceptar ADR-0003, no integrar
`architecture/referencias-autocad-ci` y no declarar vigente la excepcion.

## 13. Estado versionado y entrega del Pull Request

I-29 permanece pendiente, con automatizacion deshabilitada y sin rama, archivo de estado ni Pull
Request creados. Al reclamarse debe seguir WORKFLOW, registrar su estado canonico y mantener
`auto_merge: false`; la integracion sera una decision manual.

## 14. Evidencia final

Entregar la decision interna fechada, su responsable, fuentes, matriz de respuestas, usos
permitidos/prohibidos, politica de caching/artifacts, obligaciones, fecha de revision y decision
sobre I-13/ADR-0003. Confirmar que no se modificaron producto, CI ni `main`.

### Fuentes minimas para la revision

- licencia incluida en los paquetes;
- documentacion Autodesk APS/ObjectARX;
- paginas de los paquetes NuGet y sus firmas;
- documentacion de GitHub Actions; y
- politicas internas aplicables.
