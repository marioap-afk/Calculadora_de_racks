# ADR-0003: Referencias AutoCAD para compilación en CI

- **Estado:** propuesto
- **Fecha:** 2026-07-20
- **Decisores:** dueno del repositorio (decision pendiente); Codex (redaccion)
- **Iniciativas relacionadas:** I-13 e I-29

Mientras este ADR permanezca propuesto, documenta una alternativa condicionada y revocable: no es
una politica vigente de `main`, una aprobacion legal ni una autorizacion de merge.

El merge de I-13 permanece bloqueado hasta que I-29 entregue una decision suficiente, el dueno
apruebe la excepcion, este ADR sea aceptado, `main` este actualizado y el CI de promocion siga verde.

## Contexto

`RackCad.Plugin` depende de `AcCoreMgd`, `AcDbMgd` y `AcMgd`. El build local resuelve estas
referencias desde AutoCAD 2025 instalado, por lo que CI no podia compilar el Plugin en un runner sin
AutoCAD. I-13 demostro una ruta tecnica en E1 y E2; la promocion reducida fue validada en CI #54.

La politica vigente prohibe paquetes NuGet en codigo de producto. La ruta propuesta es por ello una
excepcion de build que requiere decision del dueno y cierre del gate L2 de licencia y procedencia.

## Decision provisional

Proponer que `RackCad.Plugin` pueda declarar exclusivamente `AutoCAD.NET` 25.0.1 como referencia
condicional de compilacion cuando `UseAutoCADNuGetReferences=true`. Las dependencias transitivas
Autodesk quedan fijadas a las versiones validadas; `PrivateAssets=all` y `ExcludeAssets=runtime` son
obligatorios. El comportamiento predeterminado continua usando una instalacion local de AutoCAD.
Los paquetes solo pueden restaurarse desde nuget.org en almacenamiento aislado y efimero de CI, sin
`actions/cache`, artifacts, feeds privados, versionado de assemblies ni inclusion en outputs o
bundle. AutoCAD continua siendo obligatorio en runtime. La excepcion es revocable y su integracion
depende de I-29, aceptacion del ADR y autorizacion del dueno.

### Alcance

La propuesta cubre unicamente `RackCad.Plugin`, compilacion en CI, la rama y mecanismo autorizados,
y AutoCAD 2025 sobre .NET 8. No cubre runtime, `NETLOAD`, AutoCAD 2026/2027, redistribucion de
assemblies Autodesk, otros paquetes NuGet de producto ni una politica general de dependencias.

### Guardas obligatorias

- versiones exactas y dependencias Core/Model fijadas y validadas;
- nuget.org como unica procedencia permitida para los paquetes Autodesk;
- cache de paquetes aislado y efimero, sin `actions/cache`;
- runner Windows sin AutoCAD y `AutoCADInstallDir` vacio;
- `library-packs` validado estrictamente;
- `CopyLocal=false` y `Private=false` efectivos;
- cero assemblies Autodesk en `ReferenceCopyLocalPaths`, outputs y bundle;
- cero artifacts del Plugin; y
- rollback coordinado de la ruta condicional y del job.

## Evidencia

La evidencia detallada permanece en el documento I-13 de la rama congelada
`experiment/refs-autocad-ci`: E1, E2 en CI #50 y la decision tecnica B. La rama limpia
`architecture/referencias-autocad-ci` conserva el mecanismo reducido, validado por CI #54 con sus
tres jobs verdes, solo el artifact de cobertura y ningun artifact del Plugin. No se copian aqui logs,
conteos ni hashes; Git y los runs citados son las fuentes verificables.

Las advertencias externas sobre Node.js 20 observadas en GitHub Actions no cambian la decision de
I-13 y deben tratarse fuera de esta iniciativa.

## Alternativas consideradas

- **Mantener el build local-only o excluir el Plugin de CI** — conserva la politica actual, pero CI
  seguiria sin detectar errores del Plugin antes de integrar.
- **Aprovisionar SDK/ObjectARX** — puede ofrecer una fuente mas explicita, pero exige confirmar canal,
  licencia, versionado y mantenimiento del aprovisionamiento.
- **Runner autohospedado** — aumenta el control, pero traslada operacion, aislamiento y licencias a
  infraestructura propia.
- **Versionar DLL o paquetes** — se rechaza porque introduce redistribucion, persistencia y
  actualizacion inseguras.
- **Feed privado** — se rechaza mientras no exista autorizacion para conservar y servir esas copias.
- **Paquetes NuGet condicionales** — se eligen provisionalmente porque resolvieron el build alojado
  con un diff acotado y guardas fail-closed, sin alterar el flujo local predeterminado.

## Consecuencias

- Positivas: el Plugin compila en CI, los errores se detectan antes de integrar, I-09/I-10/I-16
  obtienen cobertura de compilacion y no se distribuyen assemblies Autodesk.
- Negativas / costos aceptados: excepcion a cero NuGet, dependencia de nuget.org, mantenimiento del
  script y del runner, revision anual y ambiguedad material de licencia/procedencia hasta cerrar I-29.

## Licencia y procedencia

El gate permanece **L2 — ambiguedad material**. Este ADR no contiene una conclusion legal. I-29 debe
resolver las preguntas de cierre y dejar una decision interna fechada. `verified=false` no invalida
por si solo los paquetes, pero exige gobernanza adicional. Firmas y hashes aportan evidencia tecnica;
no constituyen autorizacion legal.

## Caching y artifacts

Quedan prohibidos para material Autodesk `actions/cache`, artifacts, feeds privados, DLL o `.nupkg`
en Git y su inclusion en el bundle. Solo se propone almacenamiento efimero dentro del job.

## Rollback

Si la propuesta se rechaza o una guarda deja de sostenerse, retirar coordinadamente:

1. el bloque condicional `PackageReference`;
2. el job `build-plugin`;
3. `eng/ci/verify-autocad-references.ps1`; y
4. las referencias documentales de la excepcion.

Luego se recupera el build local-only, se validan los jobs existentes y se conserva la evidencia
historica.

## Revision anual

Antes de soportar AutoCAD 2026/2027 se revisan versiones, .NET, paquetes, licencia, firmas,
procedencia, outputs, `NETLOAD` y validacion funcional.

## Criterios de aceptacion y rechazo

El dueno puede aceptar este ADR solo cuando I-29 este resuelta, autorice la excepcion, el CI limpio
siga verde, el rollback este documentado y ninguna DLL Autodesk se distribuya.

Debe rechazarse ante una prohibicion legal, un canal no aceptado, un cambio de licencia, material
Autodesk en outputs o imposibilidad de mantener las guardas. Un rechazo activa el rollback.

## Referencias

- I-13, evidencia tecnica congelada en `experiment/refs-autocad-ci`.
- [I-29 — Licencia y procedencia de referencias AutoCAD para CI](../initiatives/I-29-licencia-procedencia-autocad-ci.md).
- CI #50 (experimento E2) y CI #54 (promocion limpia).
- `.github/workflows/ci.yml`, `eng/ci/verify-autocad-references.ps1` y
  `src/RackCad.Plugin/RackCad.Plugin.csproj` en la rama de promocion.
- Iniciativas relacionadas I-09, I-10 e I-16.
