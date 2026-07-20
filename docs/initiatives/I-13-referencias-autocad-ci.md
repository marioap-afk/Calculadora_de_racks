---
schema: rackcad-initiative/v1
id: I-13
title: Referencias de AutoCAD para CI
type: experiment
status: waiting
branch: experiment/refs-autocad-ci
base_branch: main
priority: null
size: S
depends_on: []
conflicts_with: []
context_packs:
  - autocad-plugin
  - delivery-validation
  - documentation-governance
automation_state_path: null
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-13 — Referencias de AutoCAD para CI

Este documento conserva la conclusion sanitizada y reproducible del experimento E1. No contiene
assemblies, paquetes, caches, outputs ni logs completos. Tampoco adopta el mecanismo probado: la
politica vigente sigue siendo cero paquetes NuGet en codigo de producto hasta que el dueno tome una
decision explicita y, si corresponde, acepte un ADR.

## 1. Identificacion

| Campo | Valor |
|---|---|
| Iniciativa | I-13 — Referencias de AutoCAD para CI |
| Rama | `experiment/refs-autocad-ci` |
| Commit base original | `1e2a8b0cecdc92d8d5a6b96ea61912217ca3ba16` |
| Commit de reclamo | `cc7b9d297352accb2de6977bc5b3873c9f4063ff` |
| Claim-Id | `2e85a57f-de29-4c2c-a6b9-5dfa43190496` |
| Fecha de E1 | 2026-07-19, zona America/Mexico_City |
| Naturaleza | Experimento local aislado; no es una implementacion de producto |

Pregunta de E1:

> ¿Puede `RackCad.Plugin` restaurarse y compilarse en Windows utilizando exclusivamente
> `AutoCAD.NET` 25.0.1 y sus dependencias, sin resolver referencias desde la instalacion local de
> AutoCAD 2025 y sin copiar DLL Autodesk al output o al bundle?

La iniciativa procede del hallazgo G6 de la
[auditoria arquitectonica](../auditoria-arquitectura-2026-07.md) y de la entrada I-13 del
[ROADMAP](../ROADMAP.md). Su cierre e integracion se rigen por [WORKFLOW](../WORKFLOW.md).

## 2. Estado

```text
E1 aceptado.

Conclusion provisional:
B — tecnicamente plausible con restricciones y pruebas adicionales.

No adoptado.
No integrado.
No probado todavia en CI ni en una maquina limpia.
```

El resultado aceptado debe describirse como:

> Exito tecnico local provisional en una maquina que si tiene AutoCAD instalado, pero con las rutas
> de AutoCAD deliberadamente inutilizadas.

La rama sigue siendo `experiment/*`. El `PackageReference` de E1 existio solamente en una copia
temporal no Git y no esta presente en `RackCad.Plugin.csproj` de esta rama.

## 3. Baseline previa

Antes de E1 se comprobo la baseline vigente:

- restore correcto;
- build Release de la solucion correcto;
- build Release de `RackCad.Plugin` correcto;
- 635/635 pruebas correctas, conforme al estado vivo registrado en
  [HANDOFF](../HANDOFF.md);
- AutoCAD 2025 instalado fisicamente en la estacion;
- dos familias `MSB3277`, en `Microsoft.VisualBasic` y `System.Drawing`;
- el workflow de CI compilaba Domain/Application mediante las pruebas y construia UI en Windows,
  pero excluia el Plugin porque sus referencias dependian de `AutoCADInstallDir`.

Esta baseline demostraba que el repositorio compilaba con AutoCAD instalado. No demostraba que el
Plugin pudiera compilar sin AutoCAD ni que las referencias fueran reproducibles en CI.

## 4. Hipotesis de E1

> El Plugin puede compilarse usando `AutoCAD.NET` 25.0.1 y sus dependencias como compile assets, con
> assets runtime excluidos, sin utilizar la instalacion local y sin copiar assemblies Autodesk al
> output entregable.

La hipotesis exigia simultaneamente un control negativo, una variante positiva y una inspeccion del
grafo de NuGet, de `ResolveReferences` y de todos los outputs. Un build verde aislado no bastaba.

## 5. Diseño del aislamiento

E1 uso estas rutas logicas:

```text
<WORKTREE_I13>       worktree canonico, solo lectura durante E1
<TEMP_E1>            raiz efimera del experimento
<TEMP_E1>\repo       copia positiva no Git
<TEMP_E1>\negative   copia original para el control negativo
<NUGET_CACHE_E1>     cache NuGet vacio y aislado al iniciar el restore del Plugin
<EMPTY_AUTOCAD_DIR>  directorio existente y vacio
```

Guardas aplicadas:

- la copia experimental no contenia `.git`;
- se excluyeron `.git`, `bin`, `obj`, `.vs`, artifacts y caches al crear las copias;
- `NUGET_PACKAGES`, el cache HTTP, scratch y `DOTNET_CLI_HOME` apuntaron bajo `<TEMP_E1>`;
- `NuGet.Config` limpio dejo como unica fuente `https://api.nuget.org/v3/index.json`;
- se fijaron `AutoCAD.NET` 25.0.1, `AutoCAD.NET.Core` 25.0.0 y
  `AutoCAD.NET.Model` 25.0.0;
- todos los comandos positivos recibieron `AutoCADInstallDir=<EMPTY_AUTOCAD_DIR>`;
- la copia positiva se comparo contra `<WORKTREE_I13>` excluyendo outputs: 366 archivos en cada
  arbol y una sola diferencia, el `.csproj` temporal;
- no se ejecuto AutoCAD, `NETLOAD`, instalacion, desinstalacion ni copia desde la instalacion local;
- los logs relevantes tuvieron cero coincidencias con una ruta de instalacion local declarada.

AutoCAD seguia fisicamente instalado. La ausencia de alteracion se establecio por disciplina de
rutas y auditoria de comandos; no se genero una instantanea criptografica antes y despues de toda
la instalacion.

## 6. Variante temporal

El unico cambio conceptual de fuente fue:

```xml
<ItemGroup Condition="'$(UseAutoCADNuGetReferences)' == 'true'">
  <PackageReference Include="AutoCAD.NET"
                    Version="25.0.1"
                    PrivateAssets="all"
                    ExcludeAssets="runtime" />
</ItemGroup>

<ItemGroup Condition="'$(UseAutoCADNuGetReferences)' != 'true'">
  <!-- referencias originales por AutoCADInstallDir -->
</ItemGroup>
```

La condicion preservo las tres referencias originales y permitio desactivarlas explicitamente en
la variante positiva. Todos los restores y builds positivos pasaron:

```text
UseAutoCADNuGetReferences=true
AutoCADInstallDir=<EMPTY_AUTOCAD_DIR>
RestorePackagesPath=<NUGET_CACHE_E1>
```

No se modifico codigo C#. `PrivateAssets=all` evita que la dependencia se propague a consumidores;
`ExcludeAssets=runtime` excluye sus runtime assets en el proyecto actual. Son funciones distintas.
La ausencia de copia al output se verifico recursivamente y no se dedujo solo de esos metadatos.

## 7. Paquetes inspeccionados

| Paquete | Version | Dependencia exacta | Assemblies relevantes | SHA-256 del nupkg |
|---|---:|---|---|---|
| AutoCAD.NET | 25.0.1 | `AutoCAD.NET.Core [25.0.0]` | `AcMgd` y nueve assemblies adicionales | `b629f09e10bb7f414460e1ad47e4efa6d24d2815d00def388a64b10570ccd4c1` |
| AutoCAD.NET.Core | 25.0.0 | `AutoCAD.NET.Model [25.0.0]` | `AcCoreMgd` | `167a3b003d30230197cc150911080815bd5299e8e28fa411c64498e8e830ea53` |
| AutoCAD.NET.Model | 25.0.0 | Ninguna | `AcDbMgd` y `acdbmgdbrep` | `06779a73f5da2eed6a98c063ea13cde4eb07056b088772fca91c93ecdc770283` |

Los SHA-256 se recalcularon directamente sobre los tres `.nupkg` temporales antes de redactar este
documento. Los SHA-512 registrados en el catalogo de NuGet se compararon con los generados por el
restore aislado y coincidieron para los tres paquetes. Sus valores codificados no se versionan.

Metadatos y contenido:

- NuGet mostraba como propietario a `Autodesk`;
- el `.nuspec` declaraba `<owners>Autodesk, Inc.</owners>` y
  `<authors>AutoCAD Team</authors>`;
- el servicio de busqueda devolvia `verified=false`;
- los tres paquetes declaraban TFM `net8.0` y colocaban sus assemblies bajo `lib/net8.0`;
- no contenian carpetas `ref`, `runtimes`, `build` ni `buildTransitive`;
- no contenian `.props` ni `.targets`;
- cada paquete incluia `tools/install.ps1`, que no fue ejecutado;
- no declaraban `developmentDependency` ni incluian README;
- incluian `LICENSE.txt` con la licencia ObjectARX;
- el texto de licencia era identico en los tres paquetes, con SHA-256 recalculado
  `df90a4dd078e9674a5f2b7be32664c85ec4bc516a415cfc06080e8cb69df1709`;
- no se encontraron EXE, ARX, DBX o bibliotecas nativas dentro de esos tres paquetes;
- no habia nombres de DLL repetidos entre paquetes con versiones distintas.

`AutoCAD.NET` aportaba como compile assets `AcCui`, `AcDx`, `AcMgd`, `AcMr`, `AcSeamless`,
`AcTcMgd`, `AcWindows`, `AdUIMgd`, `AdUiPalettes` y `AdWindows`. Core aportaba `AcCoreMgd` y Model
aportaba `AcDbMgd` y `acdbmgdbrep`.

## 8. Naturaleza de los assemblies

Las 13 DLL se inspeccionaron como PE administrados sin cargarlas para ejecutar codigo. En las tres
referencias principales se observo:

| Assembly | Tamaño | Assembly version | File/Product version | Metodos con cuerpo |
|---|---:|---|---|---:|
| `AcMgd.dll` | 1,045,280 bytes | 25.0.0.0 | 25.0.58.0.0 | 8,657 de 9,106 |
| `AcCoreMgd.dll` | 403,744 bytes | 25.0.0.0 | 25.0.58.0.0 | 3,544 de 4,001 |
| `AcDbMgd.dll` | 2,271,008 bytes | 25.0.0.0 | 25.0.58.0.0 | 20,106 de 20,807 |

No contenian `ReferenceAssemblyAttribute` y conservaban cuerpos de metodos en proporcion
significativa. Por ello no son reference assemblies formales. NuGet los selecciono como compile
assets porque estan bajo `lib/net8.0`; prudentemente deben tratarse como assemblies de
implementacion o stubs con implementacion significativa. Este hallazgo no determina por si solo
la licencia aplicable ni autoriza su uso en CI.

## 9. Prueba negativa

El control uso otra copia no Git con el `RackCad.Plugin.csproj` original, sin `PackageReference`:

```powershell
dotnet restore src/RackCad.Plugin/RackCad.Plugin.csproj `
  --configfile .\NuGet.Config `
  --no-cache --force `
  -p:Configuration=Release `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>"

dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj `
  -c Release --no-restore `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>" `
  -v:minimal
```

Resultado:

- codigo de salida 1;
- cero warnings y 291 errores;
- primeras causas `CS0246` por namespaces `Autodesk` ausentes;
- tambien faltaron atributos y tipos como `CommandMethod`;
- las tres referencias originales apuntaban a `<EMPTY_AUTOCAD_DIR>`;
- ninguna referencia Autodesk se resolvio desde NuGet o desde una instalacion local.

`ResolveReferences` aislado termino sin error de target aunque con los HintPath dirigidos a la
carpeta vacia; el build del compilador fue la prueba causal. El contraste demuestra que el build
positivo no encontro las referencias por un mecanismo oculto de la estacion.

## 10. Restore y resolucion

Variables logicas usadas:

```powershell
$env:NUGET_PACKAGES = "<NUGET_CACHE_E1>"
$env:NUGET_HTTP_CACHE_PATH = "<TEMP_E1>\nuget-http-cache"
$env:NUGET_SCRATCH = "<TEMP_E1>\nuget-scratch"
$env:DOTNET_CLI_HOME = "<TEMP_E1>\dotnet-home"
```

Comandos positivos sanitizados:

```powershell
dotnet restore src/RackCad.Plugin/RackCad.Plugin.csproj `
  --configfile .\NuGet.Config `
  --no-cache --force `
  -p:Configuration=Release `
  -p:UseAutoCADNuGetReferences=true `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>" `
  -v:diag

dotnet msbuild src/RackCad.Plugin/RackCad.Plugin.csproj `
  -t:ResolveReferences `
  -p:Configuration=Release `
  -p:UseAutoCADNuGetReferences=true `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>" `
  -getProperty:TargetFramework,AutoCADInstallDir,UseAutoCADNuGetReferences `
  -getItem:ReferencePath,ReferenceCopyLocalPaths

dotnet msbuild src/RackCad.Plugin/RackCad.Plugin.csproj `
  -t:ResolveReferences `
  -p:Configuration=Release `
  -p:UseAutoCADNuGetReferences=true `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>" `
  -v:diag

dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj `
  -c Release --no-restore `
  -p:UseAutoCADNuGetReferences=true `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>" `
  -v:minimal

dotnet restore RackCad.sln `
  --configfile .\NuGet.Config `
  --no-cache --force `
  -p:Configuration=Release `
  -p:UseAutoCADNuGetReferences=true `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>"

dotnet build RackCad.sln `
  -c Release --no-restore `
  -p:UseAutoCADNuGetReferences=true `
  -p:AutoCADInstallDir="<EMPTY_AUTOCAD_DIR>" `
  -p:RestorePackagesPath="<NUGET_CACHE_E1>" `
  -v:minimal
```

`project.assets.json` confirmo:

- unica fuente `https://api.nuget.org/v3/index.json`;
- versiones exactas 25.0.1, 25.0.0 y 25.0.0;
- package folder unico bajo `<NUGET_CACHE_E1>`;
- TFM efectivo `net8.0-windows7.0`;
- compile assets bajo `lib/net8.0`;
- runtime assets representados por `lib/net8.0/_._` para los tres paquetes;
- archivos `.nuget.g.props` y `.nuget.g.targets` generados normalmente, sin imports de build
  aportados por los paquetes.

Resolucion principal:

| Referencia | Ruta sanitizada | Origen | Private | CopyLocal | Conflictos |
|---|---|---|---|---|---|
| `AcCoreMgd` | `<NUGET_CACHE_E1>\autocad.net.core\25.0.0\lib\net8.0\AcCoreMgd.dll` | `AutoCAD.NET.Core`; `{HintPathFromItem}`; `ExternallyResolved=true` | false | false | ninguno |
| `AcDbMgd` | `<NUGET_CACHE_E1>\autocad.net.model\25.0.0\lib\net8.0\AcDbMgd.dll` | `AutoCAD.NET.Model`; `{HintPathFromItem}`; `ExternallyResolved=true` | false | false | ninguno |
| `AcMgd` | `<NUGET_CACHE_E1>\autocad.net\25.0.1\lib\net8.0\AcMgd.dll` | `AutoCAD.NET`; `{HintPathFromItem}`; `ExternallyResolved=true` | false | false | ninguno |

Las 13 DLL Autodesk expuestas como compile assets tuvieron `Private=false` y `CopyLocal=false`.
`ReferenceCopyLocalPaths` no incluyo ninguna DLL Autodesk. Los logs relevantes tuvieron cero
coincidencias con rutas declaradas de una instalacion AutoCAD.

## 11. Resultados

| Operacion | Resultado | Warnings | Errores |
|---|---|---:|---:|
| Restore Plugin | Correcto | 0 | 0 |
| ResolveReferences estructurado | Correcto | 0 | 0 |
| ResolveReferences diagnostico | Correcto | 0 | 0 |
| Build Plugin Release | Correcto | 0 | 0 |
| Restore solucion | Correcto | 0 | 0 |
| Build solucion Release | Correcto | 0 | 0 |

E1 no ejecuto las pruebas xUnit. La cifra de la seccion 3 corresponde a la baseline previa, no a
una ejecucion dentro del experimento.

## 12. Outputs

El inventario recursivo de 272 archivos genero un bundle con:

- `RackCad.Plugin.dll`;
- `RackCad.Application.dll`;
- `RackCad.Domain.dll`;
- `RackCad.UI.dll`;
- los catalogos y `PackageContents.xml` que ya genera el proyecto.

Resultados de la inspeccion:

- cero DLL Autodesk en `bin`;
- cero DLL Autodesk en `obj`;
- cero DLL Autodesk en el bundle;
- cero ZIP o `.nupkg` generado como entregable;
- cero artifacts publicados;
- DLL Autodesk presentes solamente en el cache temporal de NuGet y en la zona temporal de
  inspeccion, nunca en el repositorio ni en el entregable.

No se conservan hashes de los cuatro DLL RackCad: son outputs reconstruibles y no forman parte de
la procedencia del candidato.

## 13. MSB3277

La baseline con referencias desde AutoCAD instalado presentaba:

| Familia | Versiones observadas | Seleccion baseline | Estado en E1 |
|---|---|---|---|
| `Microsoft.VisualBasic` | 10.0.0.0 del reference pack frente a 10.1.0.0 solicitada por assemblies Autodesk | 10.0.0.0 por ser primaria | warning ausente |
| `System.Drawing` | 4.0.0.0 del reference pack frente a 8.0.0.0 solicitada por assemblies Autodesk | 4.0.0.0 por ser primaria | warning ausente |

E1 no introdujo nuevas familias. Los assets NuGet quedaron marcados como
`ExternallyResolved=true`, lo cual limita el analisis transitivo de `ResolveAssemblyReference`.
La desaparicion del warning no equivale a demostrar ausencia de conflictos runtime.

## 14. Que demuestra E1

E1 demuestra que, en la estacion usada:

- el Plugin compila localmente con los tres paquetes y versiones fijados;
- las tres referencias principales proceden del cache NuGet aislado;
- no se usan las rutas declaradas de la instalacion local;
- no se copian DLL Autodesk a `bin`, `obj` o bundle;
- no se requieren cambios C#;
- el proyecto original falla con la ruta AutoCAD vacia;
- la configuracion es tecnicamente plausible para un runner Windows.

## 15. Que no demuestra E1

E1 no demuestra:

- ejecucion en una maquina fisicamente limpia;
- funcionamiento en GitHub Actions;
- CI reproducible;
- funcionamiento en Linux;
- carga o ejecucion en AutoCAD;
- compatibilidad funcional o runtime;
- autorizacion de licencia para el uso concreto en CI;
- procedencia verificada criptograficamente por Autodesk;
- estabilidad para AutoCAD 2026 o 2027;
- disponibilidad futura de los paquetes;
- aceptacion de una excepcion a la politica cero NuGet;
- que la ausencia de `MSB3277` implique ausencia de conflictos runtime.

## 16. Evaluacion de licencia y procedencia

### Hechos

- Los paquetes y versiones estaban publicados en NuGet.
- NuGet mostraba propietario `Autodesk`.
- El `.nuspec` declaraba propietario `Autodesk, Inc.` y autor `AutoCAD Team`.
- Los paquetes incluian una licencia ObjectARX con texto identico.
- El propietario no aparecia marcado como verificado en el servicio consultado.
- Los paquetes contienen assemblies de implementacion o stubs con implementacion significativa, no
  reference assemblies formales.
- Los SHA-256 locales quedaron fijados y la coincidencia de los SHA-512 de NuGet quedo registrada
  sin versionar sus valores codificados.

### Interpretaciones

- Restaurar los paquetes en CI parece tecnicamente posible.
- El texto de licencia parece contemplar copias relacionadas con desarrollo.
- Los hashes permiten detectar sustituciones respecto de los paquetes inspeccionados.
- Ninguna de estas observaciones constituye una conclusion legal ni una afirmacion de autenticidad
  criptografica emitida por Autodesk.

### Decisiones pendientes

- aceptar o rechazar NuGet como canal de procedencia para estas referencias;
- permitir cache de los paquetes en runners;
- permitir su restauracion en infraestructura de GitHub;
- determinar si el mecanismo constituye una excepcion a cero NuGet;
- solicitar o no una revision legal adicional;
- decidir si se exige otro canal o firma de Autodesk antes de adoptar.

Solo el dueno puede tomar estas decisiones y aceptar o rechazar el ADR que corresponda.

## 17. Riesgos

- Supply chain: la compilacion depende de paquetes externos y de la cuenta que los publica.
- La cuenta observada no aparecia como verificada.
- NuGet no permite mutar normalmente el contenido de una version publicada, pero la disponibilidad
  del paquete sigue dependiendo de infraestructura externa y puede retirarse o quedar inaccesible.
- Los paquetes no contienen reference assemblies formales.
- Pueden existir diferencias runtime ocultas por `ExternallyResolved=true`.
- AutoCAD cambia anualmente y el conjunto 25.x no valida 2026/2027.
- Un cambio posterior de metadatos puede copiar DLL Autodesk accidentalmente al bundle.
- Un cache tibio puede ocultar una dependencia o una retirada del paquete.
- Una compilacion verde puede generar falsa confianza sobre licencia, runtime y comportamiento real.
- Publicar el bundle como artifact sin inspeccion puede redistribuir material no autorizado.

## 18. Guardas necesarias si se adopta

Sin implementar aun, una adopcion deberia exigir:

- versiones exactas, sin rangos flotantes;
- `packages.lock.json` o mecanismo equivalente revisado;
- restore en modo bloqueado;
- hashes esperados de los paquetes y revision explicita de cada actualizacion;
- `PrivateAssets=all`;
- `ExcludeAssets=runtime`;
- una condicion clara y mutuamente excluyente entre referencias NuGet y referencias locales;
- build de CI con `AutoCADInstallDir` dirigido a una carpeta vacia;
- una verificacion que falle si cualquier DLL Autodesk entra en `ReferenceCopyLocalPaths`;
- inspeccion recursiva de `bin`, `obj`, bundle, staging, ZIP y cualquier output posterior;
- prohibicion de publicar artifacts que contengan DLL Autodesk;
- cache de CI gobernado por el lock file y las versiones, nunca versionado en Git;
- build de Plugin, build de solucion y suite completa;
- documentacion explicita de que compilar no equivale a ejecutar en AutoCAD;
- ADR propuesto y decision del dueno antes de modificar el proyecto real o el workflow.

## 19. E2 propuesto

Definicion:

> Build y pruebas en Windows limpio, sin AutoCAD instalado, con cache NuGet inicialmente vacio y
> las mismas versiones exactas.

Estado obligatorio:

```text
BLOQUEADO HASTA:
- integracion de I-26;
- rebase de I-13 sobre la nueva punta de main;
- revision de los cambios de ci.yml;
- autorizacion expresa del dueno.
```

No se ejecuta E2 ni se diseña YAML definitivo en esta fase. El protocolo propuesto es:

1. Confirmar que I-26 esta contenida en `origin/main`, que I-13 fue rebasada y publicada de acuerdo
   con WORKFLOW, y que el dueno autorizo E2 y el uso de los paquetes en infraestructura de prueba.
2. Usar un runner o VM Windows efimero sin AutoCAD, con checkout del commit exacto de I-13 y SDK
   compatible con `global.json`.
3. Registrar commit, imagen del runner, version y build del sistema operativo, arquitectura,
   `dotnet --info`, variables relevantes y evidencia de ausencia de AutoCAD. Sanitizar nombres y
   rutas personales.
4. Crear bajo `<TEMP_E2>` dos copias no Git del checkout: `<NEGATIVE_E2>` original y
   `<POSITIVE_E2>` con exactamente el diff conceptual de la seccion 6. No modificar el checkout que
   conserva la evidencia Git.
5. Crear `<NUGET_CACHE_E2>`, cache HTTP, scratch, `DOTNET_CLI_HOME` y `<EMPTY_AUTOCAD_DIR>` vacios.
   Registrar que el cache de paquetes tiene cero entradas antes del primer restore.
6. Ejecutar el control negativo en `<NEGATIVE_E2>` con `AutoCADInstallDir` vacio y sin
   `PackageReference`. Debe fallar por referencias Autodesk ausentes; el numero exacto de errores
   puede cambiar con el codigo, pero la causa debe conservarse.
7. En `<POSITIVE_E2>`, aplicar solamente la condicion y el `PackageReference` de la seccion 6. Fijar
   la unica fuente a `https://api.nuget.org/v3/index.json` y usar las versiones exactas.
8. Restaurar primero el Plugin con cache vacio, sin fallback local. Recalcular SHA-256 de los tres
   `.nupkg` y comparar SHA-512 con NuGet y con la seccion 7.
9. Ejecutar `ResolveReferences` estructurado y diagnostico. Las tres referencias principales deben
   quedar bajo `<NUGET_CACHE_E2>`, con `Private=false`, `CopyLocal=false`, cero entradas Autodesk en
   `ReferenceCopyLocalPaths` y cero rutas de AutoCAD instalado.
10. Compilar el Plugin Release; si falla, no continuar con la solucion.
11. Restaurar y compilar `RackCad.sln` Release con las mismas propiedades. No modificar el proyecto
    de pruebas.
12. Ejecutar la suite completa en Windows. Deben pasar 635 o mas pruebas, conforme al estado
    actualizado despues de I-26, sin fallos ni omitidas inesperadas.
13. Inspeccionar recursivamente `bin`, `obj`, bundle, staging, ZIP y cualquier output. Fallar E2 si
    aparece una DLL Autodesk fuera del cache temporal.
14. No ejecutar AutoCAD, `NETLOAD`, Linux ni validacion funcional. No publicar el bundle o sus DLL
    como artifact.
15. Conservar solo un resumen sanitizado: comandos, codigos, tiempos, warnings, errores, hashes,
    rutas logicas, inventario textual y entorno. No conservar DLL, paquetes, caches o logs completos
    en Git.

Criterios de aceptacion de E2:

1. el control negativo falla por referencias Autodesk ausentes;
2. el restore positivo usa solamente NuGet y un cache inicialmente vacio;
3. el Plugin compila;
4. la solucion compila;
5. pasan 635 o mas pruebas, conforme al estado actualizado;
6. ninguna DLL Autodesk aparece en outputs;
7. no se publica artifact propietario;
8. los logs no contienen rutas de AutoCAD instalado;
9. versiones y hashes coinciden con E1;
10. el entorno queda identificado de forma reproducible.

Si un criterio falla, E2 no se clasifica como exito parcial silencioso: se documenta la primera
causa y se detiene antes de adopcion.

## 20. Impacto sobre iniciativas posteriores

Si el mecanismo se adopta con sus guardas:

- I-09, I-10 e I-16 podran obtener compilacion real del Plugin en CI;
- se reducira el riesgo de introducir errores de compilacion en iniciativas con mucho trabajo de
  Plugin;
- el bundle podra auditarse automaticamente contra copias accidentales;
- la validacion manual en AutoCAD seguira siendo necesaria para comportamiento, bloques y runtime.

Si el candidato se descarta, esas iniciativas deberan mantener build local del Plugin y CI parcial.
La conclusion de I-13 debe conocerse antes de abrirlas, conforme al ROADMAP.

## 21. Condicion de cierre de I-13

I-13 no se considera concluida hasta que exista:

- E2 ejecutado o descartado con motivo explicito;
- decision expresa del dueno sobre procedencia, licencia y excepcion NuGet;
- clasificacion final A/B/C/D;
- conclusion escrita y sanitizada;
- ADR propuesto y aceptado o rechazado cuando corresponda;
- promocion limpia del mecanismo a una rama no experimental o descarte formal;
- integracion autorizada conforme a WORKFLOW;
- actualizacion de HANDOFF y ROADMAP solo en la sesion de integracion;
- limpieza final de la rama remota, rama local y worktree.

Hasta entonces se mantiene la clasificacion provisional B y la rama experimental no se integra
directamente como implementacion.
