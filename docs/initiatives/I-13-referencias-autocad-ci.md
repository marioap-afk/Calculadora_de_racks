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
