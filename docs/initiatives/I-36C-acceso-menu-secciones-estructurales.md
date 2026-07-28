---
schema: rackcad-initiative/v1
id: I-36C
title: Acceso desde el menu principal al generador de perfiles estructurales
type: fix
status: review-ready
branch: fix/acceso-menu-secciones-estructurales
base_branch: main
priority:
size: S
depends_on: [I-36A, I-36B, I-15]
conflicts_with: []
context_packs: [ui-editors, autocad-plugin, delivery-validation]
automation_state_path: docs/automation/state/I-36C.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Acceso desde el menu principal al generador de perfiles estructurales

> **Fix pequeno, no iniciativa de producto.** El generador **ya existe, esta integrado y esta cerrado**.
> Lo unico que falta es un boton para llegar a el.

## 1. Objetivo

Que el generador de perfiles estructurales sea **alcanzable** desde `RACKCAD`.

Resultado verificable: el menu principal muestra un boton **«Generar perfil estructural»** que ejecuta
**el mismo caso de uso** que `RACKSECCION`, y no existe una segunda copia de ese flujo.

## 2. Problema

I-36A e I-36B entregaron el catalogo, la geometria, el inspector, el preview y la materializacion. La
funcionalidad esta completa y validada por el dueno. Pero la **unica** forma de invocarla es escribir
`RACKSECCION` en la linea de comandos, y el menu principal —que es por donde entra un usuario— no la
menciona.

Es un defecto de **descubribilidad**: la capacidad existe y nadie la encuentra.

## 3. Alcance

1. Un **boton** en `RackMainMenuWindow`, con el estilo `MenuButton` vigente, entre «Disenar larguero» y
   «Abrir de la biblioteca de disenos».
2. Una **accion tipada** `MainMenuAction.GenerateStructuralSection` que la ventana reporta al cerrarse.
3. Una **autoridad compartida** `StructuralSectionCommandFlow` que consumen `RACKSECCION` y el boton.
4. **Pruebas** de UI sobre el arbol visual real y **guardas de fuente** del Plugin.
5. **Documentacion**: resumen de lo ya implementado y registro de pendientes.

## 4. Fuera de alcance

Estricto. Cualquiera de estos exige detenerse:

perfiles IPS/S; familias nuevas del catalogo; **cualquier cambio geometrico**; mejora visual de los
canales C; I-37 Cantilever; solidos 3D; persistencia; round-trip de la representacion insertada;
modificacion de sistemas existentes; `blocks.csv`; `blocks-library.dwg`; los CSV de secciones;
dimensiones AISC nuevas; y el rediseno general del menu.

## 5. Diseno

### La accion no es una insercion de rack

El menu ya lleva **un** `RackInsertionRequest` tipado para los seis sistemas que sabe disenar (I-15).
Meter ahi una seccion habria sido **incorrecto**, no solo inelegante: una seccion **no es un rack**. No
tiene `RackSystemKind` sobre el que despachar, no tiene payload de diseno que embeber y no tiene
round-trip —lo que produce es geometria plana que el usuario mide y copia—. Un request cuyo `Kind`
hubiera que inventar empuja esa mentira hasta el `switch` del host, que acaba con una rama especial
para un rack que no existe.

Por eso el menu informa una **accion**: `MainMenuAction.GenerateStructuralSection`.

### El host la lee despues de cerrar el modal

Misma regla que la insercion, y por la misma razon: el flujo pide un **punto de insercion** y el editor
de AutoCAD tiene que estar libre. `RackMenuCommands` la despacha justo despues de que
`ShowModalWindow` retorne, **antes** del `switch` de racks y con un `return` que impide caer en el.

### Una sola autoridad

`StructuralSectionCommandFlow.Run(document)` contiene el caso de uso completo: carga fail-closed del
catalogo, inspector, aviso de unidades, insercion transaccional, regen y mensaje de fidelidad. Con el
viven el resultado de insercion y el servicio que la ejecuta.

`RackSeccionCommands` queda como lo que es —el punto de entrada del comando— y `RackMenuCommands`
**invoca**, no reimplementa. Copiar el flujo habria dado dos caminos que hoy coinciden y que divergirian
a la primera correccion, con nada que avisara de que solo se arreglo uno.

## 6. Archivos

**Nuevos**: `src/RackCad.UI/MainMenuAction.cs`;
`src/RackCad.Plugin/StructuralSectionCommandFlow.cs`;
`tests/RackCad.UI.Tests/RackMainMenuStructuralSectionTests.cs`;
`tests/RackCad.Tests/MainMenuStructuralSectionAccessGuardTests.cs`;
este contrato, `docs/automation/state/I-36C.yml` y su evidencia.

**Modificados**: `src/RackCad.UI/RackMainMenuWindow.xaml` y `.xaml.cs`;
`src/RackCad.Plugin/RackMenuCommands.cs`; `src/RackCad.Plugin/RackSeccionCommands.cs`;
`tests/RackCad.Tests/StructuralSectionPluginSourceGuardTests.cs` (reapuntada al archivo del flujo);
`README.md`; `docs/guias/geometria-secciones-estructurales.md`; `docs/ideas-futuras.md`;
`docs/initiatives/README.md`.

**Prohibido tocar**: `assets/**`, `blocks-library.dwg`, `src/RackCad.Domain`, los sistemas vigentes de
UI y Plugin, `deploy/`, `.github/`, y **la geometria de I-36B**.

`docs/ROADMAP.md` y `docs/HANDOFF.md` **tampoco**: [`WORKFLOW.md`](../WORKFLOW.md) seccion 8 lo prohibe
desde una rama paralela y tiene precedencia sobre este documento. I-36C **no tiene fila en ROADMAP**;
se escribe en la sesion de integracion.

## 7. Pruebas y builds

```powershell
dotnet test  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --filter "FullyQualifiedName~RackMainMenu"
dotnet test  tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.Application/RackCad.Application.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
pwsh deploy/build-bundle.ps1
```

## 8. Validacion manual

**AutoCAD: SI aplica** (`requires_autocad: true`, `requires_owner_validation: true`). El cambio es
**visible en el menu**, y eso solo se comprueba mirandolo. El checklist de ocho puntos vive en la
evidencia.

## 9. Criterios de aceptacion

1. El boton existe, esta habilitado y lleva el texto y la descripcion acordados.
2. Esta **despues** de «Disenar larguero» y **antes** de «Abrir de la biblioteca de disenos», con el
   mismo estilo que el resto.
3. Al pulsarlo fija la accion tipada, cierra el menu y **no** genera `InsertionRequest`.
4. `RACKCAD` -> boton y `RACKSECCION` producen **el mismo** resultado.
5. **Cero duplicacion**: cada pieza del flujo la menciona exactamente un archivo del Plugin.
6. `RackCad.UI` sigue sin referenciar Autodesk.
7. Los seis sistemas y la biblioteca conservan titulo, orden y handler.
8. `git diff` sin una linea en `assets/`, `blocks-library.dwg`, Domain, `deploy/`, `.github/` ni en la
   geometria de I-36B.

## 10. Condiciones para detenerse

El flujo de `RACKSECCION` no se puede extraer sin cambiar su comportamiento; el menu necesitaria conocer
tipos de Autodesk; la accion no se puede representar sin inventar un `RackSystemKind`; hace falta tocar
geometria; o el alcance intenta expandirse hacia IPS/S o I-37.

## 11. Estado versionado y entrega

Estado canonico: [`docs/automation/state/I-36C.yml`](../automation/state/I-36C.yml). `state` recorre
`claimed` -> `implementing` -> `review-ready`; el gate final es `owner-validation`. No se abre Pull
Request: el repositorio integra por `git merge --no-ff` desde una sesion de integracion.
