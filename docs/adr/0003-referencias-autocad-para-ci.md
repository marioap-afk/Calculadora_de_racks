# ADR-0003: Referencias AutoCAD para compilacion en CI

- **Estado:** aceptado
- **Fecha:** 2026-07-20 (propuesta y decision)
- **Decisores:** Mario Perez, dueno del repositorio; redactado por Codex
- **Iniciativas relacionadas:** I-13 e I-29

Este ADR acepta una excepcion especifica y revocable a la politica cero NuGet. La decision queda
preparada en la rama de promocion; no forma parte de `main` hasta que esa rama se integre. El merge
de I-13 fue autorizado por el dueno el 2026-07-20 despues de revalidar la promocion; la excepcion
solo se vuelve politica vigente de `main` cuando entra ese merge.

## Contexto

`RackCad.Plugin` depende directamente de `AcCoreMgd`, `AcDbMgd` y `AcMgd`. El build local las
resuelve desde AutoCAD 2025 instalado, por lo que CI no podia compilar el Plugin en un runner sin
AutoCAD. I-13 demostro y promovio una ruta condicional de compilacion validada en CI.

La politica general prohibe paquetes NuGet en codigo de producto. I-29 evaluo la licencia,
procedencia, integridad, infraestructura, almacenamiento y distribucion del mecanismo propuesto.

## Decision I-29

| Campo | Decision registrada |
|---|---|
| Salida | B. Aprobado con restricciones |
| Fecha | 2026-07-20 |
| Commit final I-29 | `2fa1d5b9716a601eea3d6f0fd8d9e90658c29fbf` |
| Owner y aprobador | Mario Perez, Coordinador de Desarrollo de Proyectos |
| Organizacion | Industrias Montilla |
| Alcance organizacional | Uso interno de RackCad |
| Naturaleza | Aceptacion interna de riesgo |
| Vigencia | Mientras no cambien alcance ni restricciones; revision maxima 2027-07-20 |

La decision no es una conclusion juridica, una opinion legal externa ni una afirmacion de que
Autodesk haya autorizado expresamente GitHub-hosted runners. La concentracion de Owner, preparer,
reviewer y approver en Mario Perez forma parte del registro I-29.

## Decision

`RackCad.Plugin` puede declarar exclusivamente `AutoCAD.NET [25.0.1]` como referencia condicional de
compilacion cuando `UseAutoCADNuGetReferences=true`. Sus dependencias transitivas quedan fijadas en
`AutoCAD.NET.Core` 25.0.0 y `AutoCAD.NET.Model` 25.0.0. `PrivateAssets=all` y
`ExcludeAssets=runtime` son obligatorios. El comportamiento predeterminado continua resolviendo las
referencias desde una instalacion local de AutoCAD.

La politica general sigue siendo cero NuGet en producto. Esta excepcion no crea precedente para
otros proyectos o paquetes, no cubre runtime ni distribucion, y es revocable.

### Alcance material

La excepcion cubre unicamente:

- RackCad y `RackCad.Plugin`;
- AutoCAD 2025 y .NET 8;
- los tres paquetes y versiones exactas indicados;
- compilacion en CI; y
- uso interno de Industrias Montilla.

No cubre `NETLOAD` o ejecucion de AutoCAD en CI, AutoCAD 2026/2027, runners autohospedados,
distribucion externa, redistribucion de material Autodesk, otros paquetes NuGet ni otra finalidad.
AutoCAD instalado continua siendo obligatorio en runtime.

### Restricciones normativas

Las catorce restricciones son simultaneas:

1. Aplicar solo a RackCad.
2. Usar solo `AutoCAD.NET [25.0.1]`, `AutoCAD.NET.Core [25.0.0]` y
   `AutoCAD.NET.Model [25.0.0]`.
3. Usar los paquetes exclusivamente para compilacion.
4. No instalar ni ejecutar AutoCAD en CI.
5. Mantener `Private=false` efectivo en las referencias Autodesk resueltas.
6. Mantener `CopyLocal=false` efectivo.
7. Mantener cero assemblies Autodesk en el bundle final.
8. Mantener cero artifacts con material Autodesk.
9. No usar `actions/cache` para esos paquetes.
10. No usar feeds privados.
11. No redistribuir DLL Autodesk.
12. Mantener versiones, hashes y origen fijados y validados.
13. Mantener el rollback documentado y coordinado.
14. Ejecutar una nueva revision ante cualquier cambio material o documentacion incompatible de
    Autodesk.

La obtencion se limita a nuget.org y a almacenamiento aislado y efimero del job. Firmas, hashes,
owner y builds aportan evidencia tecnica; no constituyen autorizacion juridica.

### Cambios que obligan una nueva revision

Se debe revisar la decision antes de cambiar proyecto, versiones, source, runner, caching,
artifacts, audiencia, finalidad, TFM/SDK, guardas o ante documentacion incompatible de Autodesk.

## Caracterizacion tecnica de los assemblies

Los trece assemblies forman un conjunto heterogeneo.

Con `ReferenceAssemblyAttribute`:

- `AcCui`, `AcDx`, `AcMr`, `AcSeamless`, `AcWindows`, `AdUIMgd` y `AdUiPalettes`.

Sin `ReferenceAssemblyAttribute`:

- `AcMgd`, `AcTcMgd`, `AdWindows`, `AcCoreMgd`, `AcDbMgd` y `acdbmgdbrep`.

Las tres referencias usadas directamente por `RackCad.Plugin` —`AcMgd`, `AcCoreMgd` y `AcDbMgd`—
carecen del atributo y contienen cuerpos de metodos. Esta correccion no invalida E1, E2, la
promocion, `Private=false`, `CopyLocal=false` ni el bundle limpio. El atributo es una propiedad
tecnica y no determina por si solo licencia, procedencia o permiso de uso.

## Matriz de cumplimiento

| Restriccion | Implementacion | Verificacion |
|---|---|---|
| Solo RackCad | Alcance del ADR y `RackCad.Plugin.csproj` | Revision documental |
| Versiones exactas | `PackageReference` y grafo esperado del script | Restore y lock en CI |
| Compile-only | `ExcludeAssets=runtime` | Assets y build del Plugin |
| No AutoCAD en CI | Runner limpio y `AutoCADInstallDir` vacio | Guarda de entorno |
| `Private=false` | Metadata efectiva de las referencias Autodesk | `ResolveReferences` |
| `CopyLocal=false` | Cero Autodesk en `ReferenceCopyLocalPaths` | `ResolveReferences` |
| Bundle limpio | Contrato de cuatro DLL RackCad | Guarda recursiva de outputs |
| Cero artifacts Autodesk | Solo se publica cobertura de tests | Revision del workflow |
| Sin `actions/cache` | No existe esa accion en el workflow | Revision del workflow |
| Sin feeds privados | `NuGet.Config` temporal con solo nuget.org | Guarda de sources |
| No redistribucion | Cero Autodesk en output, bundle y artifacts | Guardas de outputs |
| Versiones, hashes y origen | Grafo exacto, lock, `contentHash` y metadata nuget.org | Script fail-closed |
| Rollback | Este ADR exige retiro coordinado | Revision documental |
| Nueva revision | Cambios materiales enumerados en este ADR | Gate de gobernanza |

## Alternativas consideradas

- **Mantener el build local-only o excluir el Plugin de CI** — deja CI ciego a errores del Plugin.
- **Aprovisionar SDK/ObjectARX** — requiere gobernar canal, licencia y mantenimiento propios.
- **Runner autohospedado** — traslada operacion, aislamiento y licencias a infraestructura propia;
  queda fuera del alcance aceptado.
- **Versionar DLL o paquetes** — se rechaza porque introduce persistencia y redistribucion.
- **Feed privado** — se rechaza porque conserva y sirve copias no autorizadas por esta decision.
- **Paquetes NuGet condicionales** — se aceptan con restricciones porque permiten compilar en un
  runner alojado con guardas fail-closed y sin alterar el flujo local predeterminado.

## Consecuencias

- Positivas: el Plugin compila en CI, los errores se detectan antes de integrar y el bundle no
  distribuye assemblies Autodesk.
- Negativas / costos aceptados: excepcion estrecha a cero NuGet, dependencia de nuget.org,
  mantenimiento de guardas y riesgo interno residual sobre licencia/procedencia.
- Gobernanza: cualquier incumplimiento o cambio material revoca el alcance y exige nueva revision.

## Rollback

Retirar coordinadamente:

1. el bloque condicional `PackageReference` del proyecto;
2. `eng/ci/verify-autocad-references.ps1`;
3. el job `build-plugin` del workflow; y
4. las referencias normativas y operativas a la excepcion.

Luego se recupera el build local-only, se validan los jobs existentes y se conserva la evidencia
historica.

## Estado del merge

I-29 esta cerrada documentalmente. El dueno autorizo la integracion de I-13 el 2026-07-20 despues de
actualizar la documentacion y revalidar la rama. Esa autorizacion cubre exclusivamente la promocion
auditada; no amplía el alcance material del ADR ni autoriza cambios futuros sin nueva revision.

## Referencias

- I-13, evidencia tecnica congelada en `experiment/refs-autocad-ci`.
- [I-29 — Licencia y procedencia de referencias AutoCAD para CI](../initiatives/I-29-licencia-procedencia-autocad-ci.md), con decision P4 en la rama congelada `docs/licencia-procedencia-autocad-ci`.
- CI #50 (experimento E2) y CI #54/#55 (promocion limpia y gate documental).
- `.github/workflows/ci.yml`, `eng/ci/verify-autocad-references.ps1` y
  `src/RackCad.Plugin/RackCad.Plugin.csproj`.
