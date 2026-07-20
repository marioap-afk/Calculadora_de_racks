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

Este documento conserva la conclusion sanitizada y reproducible del experimento E1 y el gate tecnico
posterior a la integracion de I-26. No contiene assemblies, paquetes, caches, outputs ni logs
completos. Tampoco adopta el mecanismo probado: la politica vigente sigue siendo cero paquetes NuGet
en codigo de producto hasta que el dueno tome una decision explicita y, si corresponde, acepte un
ADR.

## 1. Identificacion

| Campo | Valor |
|---|---|
| Iniciativa | I-13 — Referencias de AutoCAD para CI |
| Rama | `experiment/refs-autocad-ci` |
| Commit base original de E1 | `1e2a8b0cecdc92d8d5a6b96ea61912217ca3ba16` |
| Base actual tras el rebase local post-I-26 | `1ffebcb07553661acba2eac4a0722c8781666bdf` |
| Commit de reclamo original | `cc7b9d297352accb2de6977bc5b3873c9f4063ff` |
| Commit de reclamo reescrito | `9403f06602a4efc9e22eeceedbc87f38c455fb00` |
| Evidencia E1 original / reescrita | `a6febd2bbc63e6392bdd88efdbbfaf659fbfa1e5` / `3f604dd53a0954821069e1c228c20cf44b52de77` |
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

I-26 integrada en main y auditada.
Rebase local de I-13 sobre la nueva main completado sin conflictos.

Conclusion provisional:
B — tecnicamente plausible con restricciones y pruebas adicionales.

No adoptado.
No integrado.
No probado todavia en CI ni en una maquina limpia.
E2 no ejecutado: espera decisiones expresas del dueno.
```

El resultado aceptado debe describirse como:

> Exito tecnico local provisional en una maquina que si tiene AutoCAD instalado, pero con las rutas
> de AutoCAD deliberadamente inutilizadas.

La rama sigue siendo `experiment/*`. El `PackageReference` de E1 existio solamente en una copia
temporal no Git y no esta presente en `RackCad.Plugin.csproj` de esta rama. El rebase no cambia la
naturaleza, los limites ni la clasificacion provisional de E1.

## 3. Baselines

### 3.1 Baseline previa a E1

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

### 3.2 Baseline post-I-26 y post-rebase

Despues de rebasar sobre `main` se comprobo en el worktree canonico de I-13:

- restore de `RackCad.sln` correcto con el SDK 8.0.423 fijado por `global.json`;
- build Release de la solucion correcto, con 0 errores y solo las dos familias `MSB3277` conocidas;
- 636/636 pruebas Release correctas, sin fallos ni omitidas;
- guardian `ShippedCatalogs_MatchCanonicalTestExpectations`: 1/1 correcto;
- ejecucion Debug con la configuracion exacta de cobertura de CI: 636/636 correctas;
- exactamente un `coverage.cobertura.xml` normalizado, version 1.9, con
  `RackCad.Application` y `RackCad.Domain`;
- cobertura observada de 91.77 % de lineas y 75.26 % de ramas, sin umbral contractual.

El diff entre la base original y la nueva `main` es vacio para `RackCad.Plugin.csproj`,
`Directory.Build.*`, `NuGet.Config`, `global.json`, `RackCad.sln` y `deploy`. El blob de
`RackCad.Plugin.csproj` es identico en ambas bases. Por tanto E1 sigue vigente sin repetirlo como
E1b: I-26 cambio tests, cobertura y documentacion, no la resolucion de referencias ni el bundle.

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

- autorizar o rechazar la ejecucion de E2;
- autorizar o rechazar cambios experimentales acotados en `ci.yml` y
  `RackCad.Plugin.csproj` para habilitar el job aislado;
- permitir o rechazar la restauracion de estos paquetes desde NuGet en infraestructura de GitHub;
- aceptar o rechazar una primera ejecucion sin cache y gobernar cualquier cache posterior por lock;
- aceptar el umbral provisional de procedencia de la seccion 19.5, exigir otro canal/firma de
  Autodesk o solicitar revision legal adicional;
- resolver la publicacion de la historia rebasada de I-13 conforme a WORKFLOW;
- solo despues de E2, aceptar o rechazar NuGet como mecanismo permanente y la excepcion a cero
  NuGet mediante el ADR correspondiente.

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

## 19. Diseño definitivo de E2 post-I-26

Definicion:

> Build y pruebas en un runner Windows efimero sin AutoCAD instalado, con cache NuGet inicialmente
> vacio, las mismas versiones exactas de E1 y cero publicacion de assemblies o bundles.

### 19.1 Estado del gate

```text
GATES TECNICOS CUMPLIDOS LOCALMENTE:
- I-26 esta contenida en main;
- I-13 fue rebasada sobre la nueva punta de main;
- el CI post-I-26 fue auditado;
- la baseline post-rebase esta verde;
- E1 sigue vigente y no requiere E1b.

GATES ACTIVOS:
- autorizacion expresa del dueno para ejecutar E2;
- autorizacion para restaurar los paquetes en infraestructura de GitHub;
- aceptacion provisional del umbral de procedencia descrito abajo;
- resolucion de la publicacion de la rama rebasada conforme a WORKFLOW.
```

E2 no fue ejecutado. Cumplir sus gates tampoco adopta el mecanismo ni acepta una excepcion
permanente a cero NuGet.

### 19.2 CI vigente que debe preservarse

| Job | SO | Contrato actual | Relacion con E2 |
|---|---|---|---|
| `tests` | Ubuntu | Ejecuta una vez la suite Debug, incluido el guardian; genera, normaliza y publica `rackcad-coverage-cobertura` por 14 dias | Se conserva sin cambios; no compila Plugin |
| `build-ui` | Windows | Restaura y compila UI Debug, validando transitivamente Domain/Application | Se conserva sin cambios; no recibe el experimento |
| Plugin | — | No existe job; `ci.yml` lo excluye expresamente | E2 cubre esta brecha en aislamiento |

I-26 no agrego cache de paquetes, lock files, propiedades MSBuild para Windows ni cambios de TFM.
`EnableWindowsTargeting` no es necesario para E2 porque su runner es Windows; no se propone ejecutar
el Plugin en Linux.

### 19.3 Topologia recomendada

| Opcion | Ventaja | Riesgo | Decision tecnica |
|---|---|---|---|
| Job Windows experimental separado | Aisla los controles negativo/positivo, no altera cobertura ni UI y permite retirar el experimento limpiamente | Consume otro runner y repite la suite en Windows con un proposito distinto | **Recomendada para E2** |
| Extender `build-ui` | Reutiliza el runner Windows existente | Mezcla un job estable con una prueba de supply chain que espera un fallo negativo y complica su diagnostico | No recomendada |
| Mover `tests` a Windows o insertar E2 en ese job | Evita un job adicional | Pierde la señal Linux vigente, mezcla cobertura con referencias propietarias y amplia el radio de fallo | Descartada |

El job separado debe limitarse a la rama experimental durante E2. La suite se repite en Windows
porque valida el arbol positivo y el entorno objetivo; Cobertura y su artifact siguen perteneciendo
unicamente al job Ubuntu existente.

### 19.4 Protocolo del job experimental

1. Confirmar que el commit exacto pertenece a `experiment/refs-autocad-ci` y registrar imagen del
   runner, version/build de Windows, arquitectura, `dotnet --info` y SHA de Git.
2. Comprobar y registrar ausencia de `acad.exe`, rutas conocidas y claves de registro de AutoCAD. Si
   existe cualquier señal de instalacion, detener E2: no reinterpretar el runner como limpio.
3. Crear bajo `RUNNER_TEMP` las copias no Git `<NEGATIVE_E2>` y `<POSITIVE_E2>`, un
   `<NUGET_CACHE_E2>` inicialmente vacio, caches HTTP/scratch/CLI aislados y
   `<EMPTY_AUTOCAD_DIR>`. El checkout versionado permanece sin cambios.
4. Usar un `NuGet.Config` temporal con `clear` y solo `https://api.nuget.org/v3/index.json`. No usar
   `actions/cache` ni ningun cache precalentado en la primera ejecucion aceptable de E2.
5. En `<NEGATIVE_E2>`, conservar el proyecto original y compilar con
   `AutoCADInstallDir=<EMPTY_AUTOCAD_DIR>`. Exigir codigo no cero y causas `CS0246`/tipos Autodesk
   ausentes; un fallo por red, SDK o sintaxis no satisface el control.
6. En `<POSITIVE_E2>`, aplicar solamente el diff conceptual de la seccion 6. Las propiedades
   experimentales son `UseAutoCADNuGetReferences=true`,
   `AutoCADInstallDir=<EMPTY_AUTOCAD_DIR>` y
   `RestorePackagesPath=<NUGET_CACHE_E2>`; las referencias locales y NuGet deben ser mutuamente
   excluyentes.
7. Restaurar primero `RackCad.Plugin.csproj` desde cache vacio. Fijar `AutoCAD.NET` 25.0.1,
   `AutoCAD.NET.Core` 25.0.0 y `AutoCAD.NET.Model` 25.0.0, sin rangos ni flotantes.
8. Recalcular los SHA-256 de los tres `.nupkg` y compararlos con la seccion 7. Recalcular SHA-512 y
   compararlo con el sidecar generado por NuGet. Verificar ademas IDs/versiones, propietarios del
   `.nuspec`, licencia y ausencia de carpetas o targets nuevos respecto de E1.
9. Generar un `packages.lock.json` solo en la copia temporal, revisar que el grafo sea exactamente el
   esperado y repetir el restore con `RestoreLockedMode=true`. Si E2 se adopta, el lock revisado y el
   restore bloqueado pasan a ser requisitos de la implementacion real; no se versionan como efecto
   lateral del experimento.
10. Ejecutar `ResolveReferences` estructurado y diagnostico. Las referencias Autodesk deben proceder
    exclusivamente de `<NUGET_CACHE_E2>`, con `Private=false`, `CopyLocal=false`, cero entradas en
    `ReferenceCopyLocalPaths` y cero rutas de una instalacion AutoCAD.
11. Compilar `RackCad.Plugin` Release. Si falla, detenerse antes de restaurar la solucion.
12. Restaurar y compilar `RackCad.sln` Release con las mismas propiedades y cache. No modificar el
    proyecto de pruebas ni agregar `EnableWindowsTargeting`.
13. Ejecutar la suite completa Release en Windows y el guardian dirigido. La referencia actual es
    636 pruebas sin fallos ni omitidas y 1/1 para el guardian; un conteo futuro distinto exige
    contrastarlo con HANDOFF, no relajar el gate.
14. Inspeccionar recursivamente `bin`, `obj`, bundle, staging, ZIP y cualquier output fuera del cache.
    Fallar si aparece una DLL Autodesk, un `.nupkg` o una copia inesperada. Verificar tambien que el
    bundle conserva solo los cuatro DLL RackCad, catalogos y manifiesto esperados.
15. No ejecutar AutoCAD, `NETLOAD`, Linux ni validacion funcional. No publicar artifacts desde el job
    E2; en particular no subir bundle, DLL, paquete, cache, lock temporal o log completo.
16. Conservar solo un resumen sanitizado con comandos, codigos, tiempos, warnings/errores, hashes,
    rutas logicas, inventario textual y entorno. GitHub Actions retiene su log operativo, pero el
    repositorio no conserva assemblies, paquetes, caches ni outputs.

### 19.5 Umbral provisional de procedencia

Para autorizar **solo E2**, la aceptacion minima propuesta es: versiones exactas; SHA-256 fijados en
la seccion 7; SHA-512 recalculado consistente con NuGet; metadatos/licencia sin cambios; restore desde
la unica fuente declarada; y reconocimiento expreso de que el propietario observado no esta
verificado criptograficamente por Autodesk. Esto detecta sustituciones respecto de E1, pero no prueba
autenticidad oficial ni resuelve la licencia. Si el dueno no acepta ese riesgo provisional, E2 se
descarta o se exige primero un canal/firma oficial de Autodesk.

### 19.6 Criterios de aceptacion

1. el runner no tiene AutoCAD y el control negativo falla por referencias Autodesk ausentes;
2. el restore positivo usa solo NuGet y un cache inicialmente vacio;
3. versiones, grafo, hashes y metadatos coinciden con E1;
4. el Plugin y la solucion compilan en Release;
5. la suite Windows y el guardian quedan verdes contra la baseline vigente;
6. ninguna DLL Autodesk o paquete aparece fuera del cache temporal;
7. no se publica artifact desde E2 y el artifact Cobertura existente no cambia;
8. los logs no contienen rutas de AutoCAD instalado y el entorno queda identificado;
9. el checkout versionado solo contiene los cambios de CI/proyecto que el dueno haya autorizado para
   habilitar el experimento, no sus copias, caches u outputs.

Si un criterio falla, E2 no se clasifica como exito parcial silencioso: se documenta la primera
causa y se detiene antes de adopcion. Un E2 verde sigue siendo evidencia experimental B; la adopcion,
el ADR y la excepcion permanente a cero NuGet requieren una decision posterior separada.

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

## 22. Preparacion de E2 en GitHub Actions

Estado al preparar esta seccion: **E2 pendiente de ejecucion**. Nada de lo descrito aqui constituye
adopcion del mecanismo, aceptacion de licencia/procedencia ni excepcion permanente a la politica de
cero paquetes NuGet en producto.

### 22.1 Gate P completado

La rama remota seguia exactamente en
`a6febd2bbc63e6392bdd88efdbbfaf659fbfa1e5` despues de `git fetch --prune origin`; `origin/main`
seguia en `1ffebcb07553661acba2eac4a0722c8781666bdf`, el worktree estaba limpio y el backup local
`backup/i-13-pre-rebase-a6febd2` conservaba el commit anterior. Se publico el rebase mediante un
unico push con lease explicito para ese hash. Despues del push, HEAD local, tracking y
`git ls-remote` coincidieron en `c7d5841e49778e619845e743df083bd74819b907`, sin divergencia.
No se uso `--force` indiscriminado y los pushes posteriores deben ser normales.

### 22.2 Diseño implementado

| Elemento | Implementacion experimental |
|---|---|
| Job | `plugin-autocad-references-experiment`, `windows-latest`, limitado a `refs/heads/experiment/refs-autocad-ci` |
| Jobs de I-26 | `tests`, Cobertura, su artifact y `build-ui` permanecen sin cambios |
| Modo local | `UseAutoCADNuGetReferences=false` por defecto; conserva los tres `HintPath` originales |
| Modo E2 | `AutoCAD.NET` 25.0.1, `PrivateAssets=all`, `ExcludeAssets=runtime`; Core y Model quedan transitivos |
| Runner | Comprueba rutas, comando y claves acotadas de AutoCAD; no instala ni ejecuta AutoCAD |
| Control negativo | Compila con referencias NuGet desactivadas y una carpeta AutoCAD vacia; exige fallo causal `CS0234`/`CS0246` y descarta fallos de infraestructura |
| Restore | `NuGet.Config` efimero con solo nuget.org y caches de paquetes, HTTP, scratch y CLI aislados bajo `RUNNER_TEMP`, inicialmente vacios |
| Procedencia provisional | Revisa grafo y versiones, SHA-256 fijados por E1, SHA-512 contra sidecar y lock, owners/authors, licencia y layout de los tres paquetes |
| Resolucion | Audita las 13 compile references y las tres DLL principales; exige cache aislado, version 25.0.0.0, `Private=false`, `CopyLocal=false` y cero Autodesk en `ReferenceCopyLocalPaths` |
| Build y pruebas | Plugin y solucion Release con `--no-restore`; suite Windows de al menos 636/636 y guardian dirigido 1/1 |
| Outputs | Falla ante cualquier DLL de los paquetes Autodesk fuera del cache; valida las cuatro DLL RackCad del bundle y ausencia de nupkg, lock o ZIP en el checkout |
| Cache/artifacts | No usa `actions/cache` ni publica artifacts desde E2 |

El lock se genera temporalmente fuera del checkout, se inspecciona y se usa en un segundo restore
con `--locked-mode`. No se versiona en E2: el mismo proyecto representa dos grafos mutuamente
excluyentes y su modo predeterminado no tiene el PackageReference, por lo que un lock en la raiz
seria ruido para el flujo local y podria sugerir que el restore local esta gobernado por un grafo
que no aplica. Esta decision usa los mecanismos oficiales `--use-lock-file`, `--lock-file-path` y
`--locked-mode` documentados por NuGet en
[PackageReference y bloqueo de dependencias](https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#locking-dependencies).
La limitacion es expresa: si el mecanismo se adopta, la estrategia de lock permanente debe
rediseñarse y aprobarse por separado.

### 22.3 Alcance del commit experimental

El unico commit de preparacion se identifica por el asunto
`CI: prepara experimento E2 de referencias AutoCAD`, por `Experiment: E2` y
`Status: pending-ci`; su SHA es necesariamente el `github.sha` que se verificara en la ejecucion,
porque un commit no puede contener su propio hash. Incluye exclusivamente:

- `.github/workflows/ci.yml`;
- `src/RackCad.Plugin/RackCad.Plugin.csproj`;
- `docs/initiatives/I-13-referencias-autocad-ci.md`.

No incluye lock, paquete, cache, binario, log ni artifact.

### 22.4 Validacion previa local

Con AutoCAD 2025 instalado y el modo predeterminado se ejecutaron restore de la solucion, build
Release sin restore y tests Release sin build/restore. El restore fue correcto, el build termino
con cero errores y solo las dos familias `MSB3277` conocidas de las referencias locales, y la suite
termino 636/636, sin fallos ni omitidas. Los ocho bloques PowerShell multilínea del workflow se
analizaron como `ScriptBlock` sin errores; `git diff --check` quedo limpio. Esta validacion confirma
que el flujo local no cambio, pero no sustituye E2 en un runner limpio.

### 22.5 Exito, rollback y siguiente decision

E2 solo sera exitoso si el run del commit exacto cumple todos los criterios de la seccion 19.6,
mantiene verdes `tests`/Cobertura y `build-ui`, publica unicamente
`rackcad-coverage-cobertura` y deja cero DLL Autodesk fuera del cache. Ante el primer fallo no se
reintenta automaticamente ni se relaja una guarda: se conserva la causa en evidencia sanitizada y
se detiene para decidir correccion o descarte.

El rollback experimental consiste en revertir conjuntamente el job y la ruta condicional del
Plugin, sin tocar los jobs de I-26 ni promover el lock temporal. Aun con E2 verde, I-13 permanece
en clasificacion B hasta que el dueño decida adopcion o descarte, licencia, procedencia, excepcion
NuGet, lock/guardas permanentes y necesidad de ADR.

## 23. Resultado de E2

Estado: **E2 tecnicamente exitoso en GitHub Actions**.

| Campo | Evidencia aceptada |
|---|---|
| Run | [CI #50, run 29753642014](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/29753642014) |
| Commit | `1c49aab483d522e32c1f69f6c3d5ad7a6732ba79` |
| Runner | `windows-latest`, imagen Windows 2025 vigente |
| SDK | 8.0.423 |
| AutoCAD | No instalado |
| Cache | Cache NuGet aislado; primera ejecucion sin `actions/cache` |

La ejecucion aceptada produjo estos resultados:

- el control negativo fallo por la causa esperada: el Plugin no compila sin referencias Autodesk;
- el restore inicial y el restore locked terminaron correctamente;
- se resolvieron `AutoCAD.NET` 25.0.1, `AutoCAD.NET.Core` 25.0.0 y
  `AutoCAD.NET.Model` 25.0.0;
- la procedencia observada de los tres paquetes fue nuget.org;
- `library-packs` se acepto unicamente como fuente adicional propia del SDK, se valido en su ruta
  canonica y no contenia material Autodesk;
- `ResolveReferences` obtuvo las referencias Autodesk desde el cache aislado, con
  `Private=false`, `CopyLocal=false` y cero entradas Autodesk en `ReferenceCopyLocalPaths`;
- el Plugin y la solucion compilaron correctamente en Release;
- la suite Windows termino 636/636, sin fallos ni omitidas, y el guardian dirigido termino 1/1;
- no aparecio ninguna DLL Autodesk fuera del cache aislado;
- el bundle generado contuvo solamente las cuatro DLL RackCad esperadas;
- E2 publico cero artifacts; el unico artifact del workflow fue
  `rackcad-coverage-cobertura`, perteneciente al job estable y sin cambios.

La evidencia versionada que condujo al run aceptado comprende la preparacion en
`1f0d59edc13b62f97a0dc5fde33bcf1353814f52`, la correccion del control negativo en
`fb6035e55441882803529574784ce87ec2054636`, las guardas de lock y outputs en
`990e3a57a942f65da758c3a358c4d74bc77e19b0`, la instrumentacion sanitizada de fuentes en
`e36dcf071fa9b45cdf35fc3656ee824f94f4cb9c` y la validacion estricta de `library-packs` y
procedencia en `1c49aab483d522e32c1f69f6c3d5ad7a6732ba79`.

## 24. Respuesta experimental

**Si, `RackCad.Plugin` puede compilarse de forma reproducible en GitHub Actions sin una instalacion
completa de AutoCAD, utilizando los paquetes Autodesk fijados como referencias de compilacion y
excluyendo sus runtime assets.**

Este resultado se limita a compilacion. AutoCAD real sigue siendo necesario en runtime; E2 no
ejecuto `NETLOAD`, no realizo validacion funcional en AutoCAD y no demostro compatibilidad con
AutoCAD 2026 o 2027. Un build verde no valida bloques reales, comandos, transacciones, WPF dentro
del host ni comportamiento runtime.

## 25. Clasificacion

La clasificacion permanece:

> **B — viable con restricciones y decision adicional.**

E2 resuelve la pregunta tecnica de reproducibilidad del build en el entorno probado. No satisface
los gates de licencia, procedencia autorizada, excepcion a cero NuGet, diseño mantenible ni
integracion. Por esas razones no corresponde clasificar el resultado como A.

## 26. Decision del dueño

El dueño selecciono la **opcion B — adopcion provisional**.

El alcance autorizado es:

- mantener temporalmente un build experimental o reducido del Plugin en CI;
- no reconocer todavia el mecanismo como politica arquitectonica permanente;
- revisar licencia y procedencia antes de una adopcion definitiva;
- preparar una promocion limpia desde `main`, con un diff minimo y auditable;
- mantener un rollback sencillo que retire conjuntamente el job y la ruta condicional.

La decision no implica una excepcion permanente a cero NuGet, integracion directa del YAML
experimental completo, aprobacion legal, ADR definitivo ni cierre de I-13. Tampoco autoriza por si
sola una promocion, merge, limpieza de la rama experimental o cambio de las versiones probadas.

## 27. Inventario de componentes experimentales

| Componente | Estado provisional | Accion futura propuesta |
|---|---|---|
| `PackageReference` condicional | Candidato a conservar | Revisar y promover limpiamente |
| Versiones exactas | Conservar | Añadir politica de actualizacion |
| `PrivateAssets=all` | Conservar | Convertir en guarda permanente |
| `ExcludeAssets=runtime` | Conservar | Convertir en guarda permanente |
| Job Windows separado | Conservar provisionalmente | Simplificar |
| Control negativo | Conservar | Reducir manteniendo causalidad |
| Restore locked temporal | Evaluar | Decidir estrategia estable |
| Hashes y metadata | Conservar parcialmente | Reducir a controles mantenibles |
| Instrumentacion de sources | Reducir | Mantener solo diagnostico util |
| `library-packs` | Permitir de forma estricta | Documentar la dependencia del SDK |
| Matrices 16/16, 11/11 y 10/10 | No ejecutar en cada CI permanente | Conservar como evidencia experimental |
| Auditoria de outputs | Conservar | Simplificar sin debilitar |
| Ausencia de artifacts E2 | Conservar | Convertir en politica permanente |
| Documento experimental | Conservar | Usar como base verificable del ADR futuro |

## 28. Reduccion del YAML experimental

El YAML de E2 fue diseñado para resolver incertidumbres y producir evidencia, no como forma final
del job. Ninguna reduccion debe ejecutarse antes de conservar el resultado aceptado, las causas de
los fallos corregidos y las guardas que hicieron valido el run #50.

| Categoria | Contenido | Tratamiento futuro |
|---|---|---|
| Guardas de seguridad permanentes | Versiones exactas; origen autorizado; cache aislado; exclusiones runtime; `Private=false`; `CopyLocal=false`; ausencia de DLL Autodesk y artifacts del Plugin; referencias locales como default | Conservar en una forma corta, explicita y fail-closed |
| Diagnosticos solo de investigacion | Inventario extenso del entorno; volcados sanitizados de propiedades y sources; mensajes detallados del SDK; ramas de diagnostico para fallos ya comprendidos | Retirar del camino normal despues de preservar evidencia; mantener solo salida accionable ante fallo |
| Regresiones ocasionales | Control negativo completo; matrices 16/16, 11/11 y 10/10; inspeccion exhaustiva de paquetes, licencia, metadata, `library-packs`, lock y bundle | Ejecutar manualmente, al cambiar versiones/SDK o en un workflow programado que el dueño autorice |
| Evidencia historica | Secuencia de commits, fallos de infraestructura corregidos, run #50, conteos e inventarios exactos del experimento | Conservar en este documento; no reejecutar ni copiar literalmente al job permanente |

No deben promoverse literalmente la instrumentacion extensa del entorno, las matrices
autocontenidas en cada push, los mensajes de investigacion, las verificaciones duplicadas, la
logica especifica de fallos ya comprendidos ni pasos cuyo costo de mantenimiento exceda el riesgo
que controlan. Reducir no significa debilitar: toda eliminacion futura debe mapearse a una guarda
equivalente, una prueba ocasional o evidencia documental.

## 29. Propuesta de promocion limpia y reversible

La promocion futura debe implementarse desde `main` en una rama distinta, reaplicando solo los
cambios autorizados. No debe integrar ni recortar en sitio la historia experimental.

### 29.1 Proyecto Plugin

- conservar `UseAutoCADNuGetReferences` como propiedad explicita;
- agregar el `PackageReference` condicional con versiones exactas;
- conservar `PrivateAssets=all` y `ExcludeAssets=runtime`;
- mantener las referencias locales como comportamiento predeterminado para desarrollo;
- dejar un comentario minimo que separe referencias de compilacion de dependencias runtime.

### 29.2 CI

El job Windows permanente o provisional reducido deberia limitarse a:

1. checkout;
2. setup de .NET;
3. verificacion de ausencia de AutoCAD;
4. cache aislado;
5. restore exacto;
6. validacion mantenible de procedencia de los paquetes Autodesk;
7. build Release del Plugin;
8. build Release de la solucion;
9. pruebas y guardian, o dependencia explicita de los jobs existentes si evita duplicacion sin
   perder cobertura;
10. comprobacion de ausencia de DLL Autodesk fuera del cache;
11. prohibicion de publicar artifacts del Plugin.

### 29.3 Documentacion

La promocion debe documentar la excepcion provisional, versiones soportadas, instrucciones de
actualizacion anual, riesgo y tratamiento de `library-packs`, diferencia entre compilar y ejecutar,
y procedimiento de rollback. El rollback minimo debe retirar juntos el job y la seleccion NuGet,
dejando intacto el flujo local basado en una instalacion de AutoCAD.

## 30. Estrategia de rama

| Opcion | Ventajas | Riesgos |
|---|---|---|
| A. Reducir `experiment/refs-autocad-ci` | Conserva continuidad inmediata y requiere menos reaplicacion | Mezcla evidencia de investigacion con implementacion permanente, dificulta revisar el diff final y favorece integrar deuda experimental |
| B. Crear una rama limpia desde `main` | Produce un diff minimo, revisable y reversible; separa experimento de adopcion | Exige reaplicar y volver a validar las guardas autorizadas |
| C. Crear una iniciativa posterior a la revision legal | Hace explicitos los gates de gobierno y evita confundir exito tecnico con adopcion | Puede demorar la promocion si se trata como bloqueo total de trabajo que solo necesita compilacion provisional |

La recomendacion es conservar I-13 y su rama experimental como evidencia, no integrarla
directamente y usar este documento como especificacion verificable para la opcion B cuando el dueño
autorice la promocion. La opcion C puede gobernar la adopcion definitiva y el ADR sin invalidar un
build provisional separado. Esta secuencia conserva un rollback claro: la rama de promocion puede
revertirse sin reescribir ni perder la evidencia experimental.

## 31. Gate de licencia y procedencia

Este gate formula preguntas; no emite una conclusion legal.

| # | Pregunta pendiente | Clasificacion primaria | Decision necesaria |
|---:|---|---|---|
| 1 | ¿La licencia ObjectARX permite restauracion automatizada en CI alojado? | Legal | Revision competente antes de adopcion definitiva |
| 2 | ¿Permite caching efimero del paquete en el runner? | Legal | Confirmar alcance del uso temporal |
| 3 | ¿Permite caching persistente mediante `actions/cache`? | Legal / decision del dueño | Mantenerlo prohibido hasta resolverlo |
| 4 | ¿Permite conservar hashes y metadata sin conservar DLL? | Legal / tecnica | Validar que la evidencia no redistribuya contenido restringido |
| 5 | ¿NuGet es un canal suficientemente autorizado pese a `verified=false`? | Decision del dueño / tecnica | Definir el umbral de procedencia aceptable |
| 6 | ¿Debe exigirse revision legal interna? | Decision del dueño | Identificar responsable y evidencia de cierre |
| 7 | ¿Debe prohibirse `actions/cache` hasta cerrar la revision? | Decision del dueño / legal | Conservar la prohibicion provisional actual |
| 8 | ¿Debe mantenerse nuget.org como unica procedencia Autodesk? | Tecnica / mantenimiento | Definir fuentes permitidas y comportamiento fail-closed |
| 9 | ¿Que ocurre si Autodesk retira o sustituye el paquete? | Mantenimiento / tecnica | Definir contingencia, hashes, alertas y rollback |
| 10 | ¿Como se valida una actualizacion a AutoCAD 2026 o 2027? | Mantenimiento / tecnica | Repetir protocolo experimental y validacion runtime para cada version |

El gate debe registrar quien acepta el riesgo, el alcance exacto autorizado, fuentes y caches
permitidos, evidencia de licencia/procedencia revisada, politica ante retirada o sustitucion y
criterio de renovacion anual. Hasta entonces no se afirma aprobacion legal ni autenticidad oficial
de los paquetes.

## 32. Condiciones de adopcion definitiva

La clasificacion solo podra pasar a A cuando se cumplan todas estas condiciones:

- licencia y procedencia aceptadas por los responsables correspondientes;
- excepcion a la politica cero NuGet aprobada expresamente;
- ADR aprobado;
- implementacion reducida y mantenible en una rama limpia;
- CI verde sobre esa rama limpia;
- ausencia de DLL Autodesk fuera del cache comprobada;
- rollback documentado y probado en proporcion al cambio;
- proceso de actualizacion anual definido;
- integracion autorizada conforme a WORKFLOW;
- evidencia experimental conservada y limpieza completa ejecutada solo despues del merge.

## 33. Impacto sobre I-09, I-10 e I-16

I-09, I-10 e I-16 podran comenzar con mayor seguridad una vez que exista un build provisional
disponible: CI detectara errores de compilacion del Plugin antes de la validacion manual. Este gate
no las declara abiertas automaticamente ni sustituye las autorizaciones y dependencias de ROADMAP.

El build tampoco sustituye AutoCAD. Todo cambio runtime, de bloques, comandos, transacciones,
persistencia embebida o interaccion WPF sigue requiriendo `NETLOAD` y pruebas funcionales con el
host real. La revision legal puede separarse del trabajo preparatorio para no bloquearlo
innecesariamente, pero cualquier dependencia permanente del mecanismo debe esperar el gate de
licencia/procedencia, la excepcion NuGet y la promocion autorizada.

## 34. Estado de cierre

I-13 **todavia no esta cerrada**. E2 tiene resultado tecnico aceptado y decision provisional, pero
quedan pendientes:

- revision de licencia y procedencia;
- decision sobre la excepcion a cero NuGet;
- decision y autorizacion de una promocion limpia;
- posible ADR y su aceptacion o rechazo;
- implementacion provisional reducida;
- CI de la rama de promocion;
- integracion autorizada;
- rollback o limpieza de la implementacion experimental en el momento correcto;
- conclusion final A/B/C/D.

Hasta resolver esos puntos, la clasificacion es B, la rama experimental se conserva como evidencia
y ningun resultado de esta seccion debe interpretarse como integracion o adopcion definitiva.

## 35. Gate de licencia, procedencia y cero NuGet

Fecha de auditoria: 2026-07-20. Este gate separa hechos verificables, interpretaciones tecnicas,
riesgos y preguntas que requieren decision competente. No constituye asesoramiento ni aprobacion
legal. La clasificacion tecnica de I-13 permanece **B — viable con restricciones y decision
adicional**.

### 35.1 Fuentes primarias consultadas

- licencia `LICENSE.txt`, metadatos, manifiestos, contenido y firmas incluidos en los tres paquetes;
- paginas oficiales de [AutoCAD.NET 25.0.1](https://www.nuget.org/packages/AutoCAD.NET/25.0.1),
  [AutoCAD.NET.Core 25.0.0](https://www.nuget.org/packages/AutoCAD.NET.Core/25.0.0) y
  [AutoCAD.NET.Model 25.0.0](https://www.nuget.org/packages/AutoCAD.NET.Model/25.0.0);
- [licencia ObjectARX publicada por Autodesk](https://aps.autodesk.com/developer/overview/autocad-objectarx-sdk-licensing),
  [pagina oficial del SDK](https://aps.autodesk.com/developer/overview/objectarx-autocad-sdk),
  [componentes de la API .NET de AutoCAD 2025](https://help.autodesk.com/cloudhelp/2025/PLK/OARX-DevGuide-Managed/files/GUID-8657D153-0120-4881-A3C8-E00ED139E0D3.htm),
  [compatibilidad Managed .NET de AutoCAD 2025](https://help.autodesk.com/cloudhelp/2025/ENU/AutoCAD-Customization/files/GUID-A6C680F2-DE2E-418A-A182-E4884073338A.htm)
  y [tutorial oficial APS](https://get-started.aps.autodesk.com/tutorials/design-automation/prepare-plugin/);
- documentacion NuGet sobre
  [propietarios](https://learn.microsoft.com/nuget/nuget-org/publish-a-package#manage-package-owners-on-nugetorg),
  [prefijos reservados y el indicador `verified`](https://learn.microsoft.com/nuget/nuget-org/id-prefix-reservation),
  [PackageReference, assets y lock](https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files),
  [paquetes firmados](https://learn.microsoft.com/nuget/reference/signed-packages-reference),
  [limites de confianza](https://learn.microsoft.com/nuget/consume-packages/installing-signed-packages),
  [verificacion](https://learn.microsoft.com/dotnet/core/tools/dotnet-nuget-verify) y
  [unlisting/eliminacion](https://learn.microsoft.com/nuget/nuget-org/policies/deleting-packages);
- documentacion GitHub sobre
  [runners alojados](https://docs.github.com/actions/reference/runners/github-hosted-runners),
  [cache de dependencias](https://docs.github.com/actions/reference/workflows-and-actions/dependency-caching)
  y [artifacts](https://docs.github.com/actions/tutorials/store-and-share-data);
- `AGENTS.md`, `docs/ARCHITECTURE.md`, `docs/HANDOFF.md`, `docs/ROADMAP.md`,
  `docs/WORKFLOW.md`, los context packs aplicables, guias de despliegue, ADR vigentes,
  configuracion NuGet/MSBuild y archivos de E1/E2 de este repositorio.

No se usaron foros, blogs ni paquetes de terceros para sustentar las conclusiones.

### 35.2 Inventario exacto

| Campo | AutoCAD.NET | AutoCAD.NET.Core | AutoCAD.NET.Model |
|---|---|---|---|
| ID | `AutoCAD.NET` | `AutoCAD.NET.Core` | `AutoCAD.NET.Model` |
| Version | 25.0.1 | 25.0.0 | 25.0.0 |
| Propietario mostrado | Autodesk | Autodesk | Autodesk |
| Autor del nuspec | `AutoCAD Team` | `AutoCAD Team` | `AutoCAD Team` |
| `owners` del nuspec | `Autodesk, Inc.` | `Autodesk, Inc.` | `Autodesk, Inc.` |
| Verified owner | `false`, observado en la busqueda V3 de E1 | `false`, observado en la busqueda V3 de E1 | `false`, observado en la busqueda V3 de E1 |
| Fecha de publicacion | 2024-03-29 | 2024-03-27 | 2024-03-27 |
| TFM | `net8.0` | `net8.0` | `net8.0` |
| Dependencias | `AutoCAD.NET.Core [25.0.0]` | `AutoCAD.NET.Model [25.0.0]` | Ninguna |
| Ubicacion de assemblies | `lib/net8.0`, diez archivos | `lib/net8.0`, un archivo | `lib/net8.0`, dos archivos |
| Licencia incluida | `LICENSE.txt`, ObjectARX | La misma | La misma |
| URL oficial | Pagina NuGet enlazada arriba | Pagina NuGet enlazada arriba | Pagina NuGet enlazada arriba |
| SHA-256 de E1 | `b629f09e10bb7f414460e1ad47e4efa6d24d2815d00def388a64b10570ccd4c1` | `167a3b003d30230197cc150911080815bd5299e8e28fa411c64498e8e830ea53` | `06779a73f5da2eed6a98c063ea13cde4eb07056b088772fca91c93ecdc770283` |
| SHA-512/contentHash | Recalculado y coincidente con catalogo/lock; no se reproduce el valor codificado | Igual | Igual |
| Scripts incluidos | `tools/install.ps1` | `tools/install.ps1` | `tools/install.ps1` |
| Assets runtime | `lib/net8.0`; elegibles por defecto, sin carpeta `runtimes` | Igual | Igual |
| Assets compile | `lib/net8.0`; no hay carpeta `ref` | Igual | Igual |

Los assemblies no son reference assemblies formales: estan en `lib`, no en `ref`, y E1 encontro
implementacion significativa. Los scripts heredados solo ponen `CopyLocal=false` al instalarse en
el modelo antiguo de Visual Studio; E1/E2 no los ejecutaron. En el candidato de RackCad,
`ExcludeAssets=runtime` evita seleccionar esos archivos como assets runtime y `PrivateAssets=all`
evita que la dependencia se propague a consumidores. Ninguno de esos metadatos altera el contenido
del paquete ni convierte los assemblies en stubs.

### 35.3 Auditoria de la licencia incluida

Los tres textos son byte-a-byte equivalentes para el alcance de esta auditoria y su SHA-256 es
`df90a4dd078e9674a5f2b7be32664c85ec4bc516a415cfc06080e8cb69df1709`. El texto corresponde al
acuerdo ObjectARX para AutoCAD 2025, 2024, 2023 y 2022 incluido en los paquetes. La pagina vigente
de Autodesk conserva las mismas clausulas materiales para AutoCAD 2025.

| Tema | Sentido verificable | Interpretacion tecnica | Incertidumbre |
|---|---|---|---|
| Uso para desarrollo | Concede licencia limitada, no exclusiva, para usar y copiar el software; las copias entregadas deben usarse para desarrollar aplicaciones sobre productos Autodesk basados en AutoCAD, con exclusiones expresas | Compilar un plugin de AutoCAD encaja en el proposito de desarrollo | No menciona CI ni runners de terceros |
| Copias | Permite instalacion en una ubicacion, uso mediante servidor de archivos/red, backups y copias ilimitadas sujetas a proposito y avisos | Una copia de trabajo puede ser necesaria para compilar | No define cache efimero, cache persistente ni multiplicidad de VMs alojadas |
| Redistribucion del software | Permite dar copias a personas o entidades solo para el desarrollo indicado y conservando acuerdo y avisos | Transferir el paquete completo es materialmente distinto de restaurarlo para un job | No autoriza de forma especifica artifacts publicables, Git o un feed privado |
| Samples, headers, libraries y assemblies | El acuerdo usa el termino global `Software`; no separa derechos por tipo de archivo | Deben tratarse como parte del software licenciado | No existe una concesion separada para assemblies dentro de los paquetes |
| Sublicencia | No se localizo concesion expresa de sublicencia; solo la copia condicionada anterior | No debe inferirse una facultad general de sublicenciar | Requiere interpretacion legal si un proveedor de CI interviene |
| Modificacion | El termino software incluye versiones modificadas licenciadas por Autodesk; no se localizo una concesion general para modificar | RackCad solo los consume para compilacion | No debe confundirse esa definicion con permiso de alterar binarios |
| Aplicaciones desarrolladas | Autoriza copias para desarrollar aplicaciones, pero no regula de forma expresa la distribucion de la aplicacion resultante | RackCad puede evitar distribuir componentes Autodesk y depender de AutoCAD en runtime | Debe confirmarse el derecho de distribuir RackCad bajo ese modelo |
| Componentes redistribuibles | No enumera componentes ni designa estos assemblies como redistribuibles | `Copy Local=False` y el bundle limpio son consistentes con no redistribuir | No se localizo lista primaria que convierta estos archivos en redistribuibles |
| Infraestructura remota | No menciona servicios alojados, GitHub Actions, contenedores ni imagenes | Un runner crea al menos una copia de desarrollo en infraestructura de un tercero | Se requiere decision legal sobre proveedor, control y territorio |
| Terminacion | El acuerdo termina automaticamente por incumplimiento | Las guardas reducen el riesgo de una copia fuera del alcance acordado | El efecto concreto ante un error de CI es materia legal |
| Propiedad intelectual | Autodesk y sus licenciantes retienen titularidad; solo existen derechos expresos y el codigo/estructura se trata como secreto comercial | Hashes y metadata no transfieren el codigo; binarios y paquetes si contienen el software | No se debe inferir derecho alguno por disponibilidad publica en NuGet |
| Territorio y producto | Impone controles de exportacion de EE. UU.; excluye AutoCAD LT, DWG TrueConvert y DWG TrueView del desarrollo autorizado | El destino declarado de RackCad es AutoCAD 2025 completo | No aclara geografia del runner ni necesidad de asiento AutoCAD para build |
| Licencias adicionales | Menciona derechos de Autodesk y sus licenciantes, y el acuerdo completo; no identifica licencias adicionales por componente | La licencia incluida es la unica encontrada dentro de estos paquetes | No demuestra que no existan obligaciones externas aplicables al usuario o servicio |

Hecho contractual: existe permiso condicionado para uso y copias de desarrollo. Interpretacion
tecnica: el restore compile-only parece mas cercano a ese proposito que cualquier publicacion o
redistribucion. Riesgo: el texto no describe GitHub-hosted runners, persistencia administrada por
GitHub, containers ni feeds. Pregunta legal: si esas modalidades quedan dentro de la copia de
desarrollo permitida y bajo que avisos, controles y licencias.

### 35.4 Matriz de escenarios

| Escenario | Estado tecnico | Estado de licencia | Politica RackCad | Recomendacion |
|---|---|---|---|---|
| A. Restore efimero en runner GitHub-hosted | Probado por E2 | Aparentemente alineado con desarrollo, pero ambiguo por infraestructura alojada; requiere decision legal | Excepcion de producto/build aun no aprobada | Permitir solo provisionalmente en rama no integrable |
| B. Cache durante un unico job | Probado por E2 mediante `NUGET_PACKAGES` aislado | Misma ambiguedad; copia descartable del job | Mismo conflicto | Permitir provisionalmente, borrar con el runner y auditar salidas |
| C. `actions/cache` persistente | Tecnicamente posible, no probado en E2 | Ambiguo: conserva copias entre runs y amplia lectores posibles | No autorizado | Prohibir por defecto hasta decision legal y del dueño |
| D. Descarga desde NuGet en cada run | Probado por E2 | Aparentemente compatible con uso de desarrollo, con ambiguedad del runner | Excepcion pendiente | Preferir a cache persistente durante la fase provisional |
| E. Hashes y metadata en Git | Probado y sin bytes de assemblies | No se localizo restriccion; sirven a integridad | Compatible con documentacion/evidencia | Permitir, sin codificar contenido ni incluir paquetes |
| F. `packages.lock.json` | Probado temporalmente en E2; mecanismo oficial | Contiene IDs, grafo y contentHash, no assemblies; no tratado por licencia | Estrategia permanente no decidida | Permitir solo al adoptar locked mode y revisar su lugar correcto |
| G. Assemblies Autodesk como artifact | Tecnicamente posible; E2 lo prohibio | No autorizado por evidencia para este publico y modalidad | Prohibido | No publicar |
| H. Paquetes Autodesk como artifact | Tecnicamente posible; E2 lo prohibio | Redistribuye el software completo; no autorizado por evidencia | Prohibido | No publicar |
| I. Assemblies Autodesk en bundle | Tecnicamente evitable y ausentes en E1/E2 | Autodesk indica que los archivos ya acompañan al producto; no hay concesion especifica para este bundle | Prohibido por despliegue | Mantener guarda fail-closed |
| J. Feed privado interno | Tecnicamente posible, no probado | La licencia permite ciertas copias para desarrollo con avisos, pero no demuestra que cualquier feed cumpla | No autorizado | No asumir que resuelve la licencia; requiere revision legal y diseño de acceso |
| K. Assemblies Autodesk en Git | Tecnicamente posible | Copia persistente y distribuida; no autorizada por evidencia para este uso | Prohibido | No versionar |
| L. Runner autohospedado con AutoCAD | Tecnicamente posible, no probado en CI | Puede acercarse al baseline local, pero no resuelve asiento, host ni automatizacion | Requeriria nueva decision y operacion | Evaluar solo como alternativa con revision de licenciamiento AutoCAD |
| M. Contenedor o imagen con assemblies | Tecnicamente posible, no probado | Copia persistente y replicable no descrita | No autorizado | No usar |
| N. Compile-only; AutoCAD requerido en runtime | Probado por E2 y por bundle limpio | Es el caso de menor redistribucion y mas cercano al desarrollo permitido; aun ambiguo para CI alojado | Excepcion NuGet pendiente | Candidato condicionado; nunca presentar los paquetes como runtime |

No se recomienda versionar o publicar assemblies, publicar paquetes, incluir material Autodesk en el
bundle ni considerar un feed privado como solucion automatica de licencia.

### 35.5 Procedencia NuGet por capas

| Capa | Evidencia | Estado |
|---|---|---|
| 1. Nombre y propietario mostrado | Las tres paginas de nuget.org muestran propietario `Autodesk` | Demostrada como propiedad de galeria; el propietario puede administrar/publicar, no equivale por si solo a identidad corporativa criptografica |
| 2. Autor/owner del nuspec | `AutoCAD Team` y `Autodesk, Inc.` | Demostrada como metadata declarada; NuGet aclara que `authors`/`owners` del nuspec no determinan la propiedad de galeria |
| 3. SHA-256 | Fijados en E1 y revalidados | Demostrada para los bytes auditados |
| 4. SHA-512/contentHash | Coincidencia entre paquete, metadata y lock temporal | Demostrada para integridad/reproducibilidad; no es prueba independiente de autoridad editorial |
| 5. Firma del paquete | Los tres contienen firma de autor y de repositorio; `dotnet nuget verify --all` termino con codigo 0 | Demostrada. La firma de autor identifica `Autodesk, Inc.` y la de repositorio `NuGet.org Repository by Microsoft`; no se publican certificados ni huellas completas |
| 6. Verified owner | `verified=false` observado por E1 | Ausente. NuGet define `verified` como indicador de prefijo reservado; `false` significa que no se demostro esa reserva, no demuestra por si solo que el paquete sea no oficial |
| 7. Licencia incluida | Texto ObjectARX identico en los tres, hash fijado, y concordante materialmente con la pagina Autodesk | Demostrada como contenido incluido; su aplicacion al runner sigue siendo ambigua |
| 8. Reconocimiento Autodesk | La documentacion oficial del SDK ofrece las bibliotecas para desarrollo y el tutorial APS usa `AutoCAD.NET.Core`/`Model`; no se localizo una pagina Autodesk no-blog que enlace especificamente la version 25.0.1 | Parcial; falta un enlace oficial no-blog versionado al paquete exacto |

NuGet no permite normalmente borrar permanentemente una version porque rompería restores, y una
version no listada sigue disponible por version exacta. Existen excepciones de eliminacion, entre
ellas contenido dañino o infractor; por ello disponibilidad futura no es garantia absoluta. El lock
oficial fija el cierre del grafo y `--locked-mode` restaura exactamente ese grafo o falla ante
cambios. Las firmas protegen integridad y aportan evidencia de origen; NuGet permite endurecer la
confianza con `signatureValidationMode=require` y `trustedSigners`. Esa politica no esta implantada
todavia en RackCad y no debe agregarse fuera de una promocion autorizada.

### 35.6 Respuestas de documentacion oficial Autodesk

| Pregunta | Respuesta verificable |
|---|---|
| ¿Autodesk documenta los paquetes AutoCAD.NET? | Parcialmente. El tutorial APS oficial instruye instalar `AutoCAD.NET.Core` y `AutoCAD.NET.Model`; no se localizo documentacion no-blog versionada para los tres paquetes 25.0.x |
| ¿Autodesk enlaza oficialmente las paginas NuGet exactas? | No localizado en fuentes primarias no-blog para 25.0.1/25.0.0 |
| ¿ObjectARX presenta estos assemblies como redistribuibles? | No. La ayuda los presenta como referencias disponibles con AutoCAD o el SDK, y la licencia regula copias de desarrollo; no los etiqueta como componentes redistribuibles |
| ¿Las bibliotecas simplificadas se recomiendan solo para build? | La ayuda recomienda referenciar las versiones simplificadas del SDK; no usa la expresion `solo para build` ni las identifica con el contenido de estos paquetes |
| ¿Autodesk exige `Copy Local=False`? | Si. La ayuda de AutoCAD 2025 dice que debe ser falso porque los archivos ya se entregan con el producto y copiarlos puede causar resultados inesperados |
| ¿AutoCAD 2025 usa .NET 8 oficialmente? | Si. La tabla oficial de compatibilidad vincula AutoCAD 2025/SDK 25.0 con .NET 8.0 |
| ¿El SDK puede descargarse separadamente de AutoCAD? | Si. Autodesk ofrece ObjectARX como descarga sujeta a su licencia |
| ¿Que componentes declara redistribuibles? | No localizado en las fuentes primarias consultadas para estos assemblies |
| ¿Existe guia oficial para CI con estos paquetes? | No localizado en fuentes primarias |
| ¿Existe soporte oficial para GitHub-hosted runners? | No localizado en fuentes primarias |

### 35.7 Modalidades de almacenamiento y caching

| Modalidad | Bytes, duracion y acceso | ¿Redistribuye/publica? | Evidencia de licencia y riesgos | Recomendacion provisional |
|---|---|---|---|---|
| Cache de proceso | Buffers/descargas transitorias durante el proceso; acceso del proceso/job | No se publica | No mencionado; riesgo tecnico bajo y riesgo legal residual | Permitir dentro del job |
| `NUGET_PACKAGES` del job | Paquete, assemblies extraidos, metadata y hashes hasta destruir la VM; acceso de pasos y cuenta del job | No se publica, pero crea copia en infraestructura GitHub | La licencia permite copias de desarrollo pero no nombra CI; verificar que nada salga de la VM | Permitir de forma aislada y efimera |
| Persistencia entre jobs/runs | El mismo contenido conservado fuera de la VM para reutilizacion | Se almacena para otros jobs/runs | Caso no mencionado; amplia duracion y sujetos con acceso | No permitir sin revision |
| `actions/cache` | Archiva las rutas elegidas; accesible por alcances de rama/base/default branch; entradas sin acceso por mas de 7 dias se eliminan, sujeto a limites | No es artifact, pero GitHub conserva y sirve copias; lectores de PR pueden alcanzar caches base segun el modelo oficial | Cache no firmado/verificado por GitHub y con riesgo de poisoning; licencia silenciosa | Prohibir para material Autodesk |
| Artifact | Archiva archivos despues del job con retencion configurable y descarga autorizada | Si, publica el archivo como salida del workflow dentro del alcance del repositorio | No existe autorizacion demostrada para assemblies o paquetes Autodesk | Prohibir material Autodesk |
| Feed privado | Copia completa persistente servida a usuarios/runners internos | Redistribuye internamente a sus consumidores | Podria caer bajo copias de desarrollo condicionadas, pero no esta demostrado y exige avisos/control | No usar sin revision legal |
| Git | Copia completa, historica e indefinida para toda audiencia del repositorio | Si, distribuye con clones/fetch | NuGet recomienda omitir paquetes del control de versiones; RackCad lo prohibe | Prohibir assemblies y paquetes; admitir solo hashes/metadata/lock aprobados |

GitHub documenta que cada job Windows estandar corre en una VM alojada, mientras `actions/cache`
conserva archivos entre runs y permite lecturas segun alcance de rama, incluso desde ciertos PR.
Tambien distingue cache de artifacts: el primero reutiliza dependencias y el segundo conserva
salidas del job. Por ello se confirma la recomendacion predeterminada: cache solo efimero dentro del
job, sin `actions/cache`, artifacts, feed ni almacenamiento externo para material Autodesk.

### 35.8 Conflicto con cero NuGet

| Categoria | Aplica a AutoCAD.NET | Fundamento |
|---|---|---|
| Dependencia runtime | No | AutoCAD instalado debe proporcionar los assemblies; `ExcludeAssets=runtime` y las guardas impiden copiarlos |
| Dependencia de producto distribuido | No, en el diseño probado | Bundle E1/E2 sin material Autodesk |
| Dependencia de compilacion | Si | El Plugin compila contra los assets `lib/net8.0` |
| Dependencia CI | Si | El runner restaura los paquetes para compilar |
| Dependencia de tests | No | Los tests no consumen estos paquetes de forma directa |
| Herramienta de build | No estrictamente | Son referencias/API de compilacion, no un ejecutable que transforme el build |
| Componente redistribuido | No | Debe permanecer fuera de outputs, bundle y artifacts |

La politica escrita es **A dentro de los proyectos de producto**: cero paquetes NuGet en codigo de
producto; solo el proyecto de tests tiene la excepcion expresa. No existe distincion escrita entre
runtime y compile-only para el producto, de modo que un `PackageReference` condicional en
`RackCad.Plugin` sigue siendo una excepcion. La dependencia privada de tests no es precedente
equivalente: es precisamente la excepcion nombrada. El dueño puede aceptar o rechazar la excepcion;
una excepcion permanente requiere decision explicita y ADR aceptado antes del merge.

| Politica posible | Ventajas | Riesgos/consecuencia |
|---|---|---|
| Estricta: cero NuGet en producto y build | Regla simple; elimina dependencia del canal y este gate | Descarta la solucion E2 y conserva CI del Plugin dependiente de instalacion/otra provision |
| Excepcion de build limitada | Resuelve el build con condicion explicita, compile-only, versiones fijadas, cero runtime/artifacts y guardas | Crea excepcion estrecha que exige dueño, ADR, mantenimiento anual, procedencia y gate legal |
| Categoria general de dependencias de build | Permite gobernar futuras referencias/herramientas con criterios comunes | Amplia superficie de supply chain y puede normalizar excepciones mas alla de I-13 |

La alternativa proporcional a la evidencia actual es evaluar la excepcion limitada, no redefinir
todavia una politica general. Esta auditoria no la aprueba.

### 35.9 Preguntas cerradas para revision legal

| # | Pregunta | Por que importa y evidencia disponible | Riesgo si si | Riesgo si no | Decision tecnica resultante |
|---:|---|---|---|---|---|
| 1 | ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted? | E2 lo probo; el acuerdo permite copias de desarrollo pero no nombra CI | Adoptar sin controles suficientes de proveedor/territorio | Perder esta ruta de build alojado | Si: definir controles; no: descartar GitHub-hosted |
| 2 | ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida? | Es el almacenamiento minimo de E2 | Conservar bytes mas alla del alcance autorizado por error | Ningun restore alojado seria admisible | Si: aislar/destruir/auditar; no: cambiar de runner |
| 3 | ¿Puede GitHub actuar como proveedor de infraestructura para ese uso? | GitHub aloja la VM y procesa los bytes | Omitir condiciones contractuales aplicables al proveedor | Aun un restore efimero alojado quedaria bloqueado | Si: documentar proveedor; no: usar infraestructura autorizada o abandonar mecanismo |
| 4 | ¿Puede usarse `actions/cache`? | GitHub conserva copias entre runs con alcance de ramas/PR | Ampliar retencion y lectores de material Autodesk | Mayor latencia y dependencia de disponibilidad NuGet | Si: limitar acceso/retencion; no: mantener prohibicion |
| 5 | ¿Puede conservarse `packages.lock.json`? | Solo contiene grafo y contentHash | Tratar como libre metadata que pudiera estar sujeta a condiciones | Perder locked mode como control directo | Si: versionarlo en su lugar correcto; no: usar otra guarda reproducible |
| 6 | ¿Pueden conservarse contentHash y hashes? | No contienen los assemblies y soportan integridad | Exponer metadata que un criterio legal considere restringida | Debilitar deteccion de sustitucion de bytes | Si: conservarlos; no: fijar versiones y verificar por otro medio |
| 7 | ¿Es admisible que el paquete contenga assemblies de implementacion? | Estan en `lib`, no `ref`; la firma valida esos bytes | Aplicar controles insuficientes por tratarlos como stubs | Los paquetes no servirian como referencias CI | Si: reforzar guardas; no: descartar paquetes |
| 8 | ¿NuGet es un canal autorizado por Autodesk? | Propietario, firma de autor Autodesk, licencia y tutorial oficial general; falta enlace no-blog exacto | Confiar en una autoridad editorial insuficientemente documentada | Bloquear un canal firmado y tecnicamente probado | Si: permitir solo nuget.org; no: exigir confirmacion Autodesk/SDK u otro canal |
| 9 | ¿`verified=false` requiere validacion adicional? | Es ausencia de prefijo reservado, compensada parcialmente por firma | Aceptar una brecha de identidad de galeria sin compensacion suficiente | Crear costo de validacion adicional o bloquear adopcion | Si: definir evidencia adicional; no: aceptar firma+hash+documentacion como umbral |
| 10 | ¿Puede usarse un feed privado? | Implicaria copia persistente y entrega interna | Redistribuir sin avisos, alcance o retencion adecuados | Depender de nuget.org en cada run | Si: diseñar controles; no: descargar cada run o descartar solucion |
| 11 | ¿Puede usarse un runner autohospedado? | Alternativa mas controlada, aun con copia/licencia AutoCAD | Operar una maquina sin aislamiento/licencias suficientes | Perder la principal alternativa al runner alojado | Si: gobernar operacion/licencias; no: mantener hosted condicionado o no compilar Plugin en CI |
| 12 | ¿Hay obligaciones de avisos o atribucion? | Las copias entregadas deben conservar acuerdo y avisos | Incumplir por omitir avisos | Sobrecargar el flujo con avisos innecesarios | Si: preservar avisos donde corresponda; no: no añadir atribucion inventada |
| 13 | ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado? | Autodesk exige `Copy Local=False`; E1/E2 demostraron bundle limpio | Distribuir una aplicacion bajo un supuesto contractual incompleto | El modelo actual de despliegue requeriria revision | Si: documentar runtime y validar NETLOAD; no: bloquear distribucion hasta rediseño |
| 14 | ¿Difiere uso interno de distribucion externa? | El acuerdo habla de personas/entidades, no del modelo comercial de RackCad | Aplicar la respuesta al publico equivocado | Limitar innecesariamente uno de los modelos | Si: separar politicas; no: aplicar un unico criterio conservador |
| 15 | ¿Se requiere una licencia AutoCAD por cada entorno de build? | E2 no instalo ni ejecuto AutoCAD; el acuerdo ObjectARX no resuelve asientos del producto | Hacer inviable o costoso el CI alojado | Omitir una licencia que realmente fuera necesaria | Si: usar entorno licenciado/controlado; no: documentar que el build SDK no consume asiento |

### 35.10 Recomendacion, promocion y gate

Clasificacion independiente del gate:

> **L2 — ambiguedad material; el merge debe permanecer bloqueado.**

No se encontro una prohibicion expresa de usar las copias para desarrollar un plugin, y las firmas
fortalecen sustancialmente la procedencia. Sin embargo, las fuentes no autorizan inequivocamente el
uso en runners GitHub-hosted ni la persistencia administrada por GitHub, y la politica interna aun
prohibe el `PackageReference` en el Plugin. Por ello la recomendacion no legal es:

> **D + A condicionada**: registrar o abrir un gate separado de licencia/procedencia y permitir que,
> solo con autorizacion del dueño, se prepare una promocion limpia provisional; no integrar hasta
> cerrar licencia, procedencia, excepcion NuGet y ADR.

Puede crearse una rama limpia para implementacion tecnica y validacion, pero el merge queda
bloqueado. Conforme a los prefijos reales de `WORKFLOW.md`, el nombre propuesto es
`architecture/referencias-autocad-ci`, creada desde la punta vigente de `origin/main` y con un
reclamo nuevo. No se crea en esta ejecucion. La rama experimental se conserva como evidencia y no
debe ser la fuente directa de merge.

Condicion de merge: revision de licencia/procedencia cerrada por responsable competente, excepcion
de build aprobada por el dueño, ADR aceptado, diff limpio y reducido, CI verde, cero material
Autodesk fuera del cache efimero, rollback documentado y proceso anual de actualizacion.

### 35.11 Decisiones pendientes del dueño

Responder si/no; ninguna decision se ejecuta desde este gate:

1. ¿Autoriza crear una rama limpia de promocion desde `main`?
2. ¿Autoriza implementar la solucion reducida en esa rama?
3. ¿Confirma que el merge seguira bloqueado hasta cerrar licencia/procedencia?
4. ¿Autoriza una excepcion provisional de build a cero NuGet?
5. ¿Prohibe `actions/cache` para paquetes Autodesk?
6. ¿Desea revision legal interna o externa?
7. ¿Autoriza crear un ADR provisional despues de validar la rama limpia?
8. ¿Desea abrir una iniciativa separada para licencia/procedencia?

Este gate no cierra I-13, no cambia su clasificacion B, no adopta NuGet, no autoriza un merge y no
declara que el uso evaluado sea legal o ilegal.
