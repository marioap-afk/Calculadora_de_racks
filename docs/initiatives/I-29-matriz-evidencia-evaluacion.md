# I-29 — Matriz maestra de evidencia y evaluacion

Fecha de ejecucion: 2026-07-20. Estado: P3 preliminar completada; decision formal pendiente.

Este documento es una evaluacion interna de riesgo, licencia y gobernanza. No constituye asesoria
legal profesional, no selecciona A/B/C/D y no autoriza cambios en I-13, ADR-0003, la politica cero
NuGet ni el merge de la promocion.

## 1. Alcance

Se evaluan los paquetes `AutoCAD.NET` 25.0.1, `AutoCAD.NET.Core` 25.0.0 y
`AutoCAD.NET.Model` 25.0.0 exclusivamente para restauracion efimera y uso como referencias de
compilacion de `RackCad.Plugin` en un runner Windows alojado por GitHub, sin AutoCAD instalado ni
ejecutado. Se excluyen caching persistente, feeds privados, artifacts con material Autodesk,
redistribucion de assemblies, otras versiones y cualquier conclusion sobre licencias de producto.

## 2. Metodo

1. Se verifico Git contra los hashes de continuidad y se leyeron las normas y documentos exigidos.
2. Se contrastaron E1/E2, el proyecto, el workflow y las guardas de la promocion en modo lectura.
3. Se registro la revalidacion independiente ya ejecutada sobre los tres nupkg exactos, sin volver a
   descargarlos ni conservar binarios o temporales.
4. Se revisaron fuentes primarias de Autodesk, NuGet/Microsoft y GitHub vigentes el 2026-07-20.
5. Para cada pregunta se separaron hechos, inferencias, limites, riesgo y decision pendiente.

## 3. Fuentes internas

| ID | Fuente | Uso |
|---|---|---|
| I1 | [Contrato I-29](I-29-licencia-procedencia-autocad-ci.md) | Alcance, preguntas y gates |
| I2 | [Paquete de decision](I-29-paquete-decision-interna.md) | Flujo de bytes, guardas y rollback |
| I3 | [Registro de entrega](I-29-registro-entrega-revision.md) | Roles y limites de la revision |
| I4 | Evidencia I-13 en `experiment/refs-autocad-ci` | E1, E2, hashes, licencia, firmas e inventario |
| I5 | Promocion en `architecture/referencias-autocad-ci` | Proyecto, CI, script fail-closed y ADR propuesto |
| I6 | `AGENTS.md`, `docs/ARCHITECTURE.md` y `docs/HANDOFF.md` | Politica cero NuGet vigente |

I4 e I5 se consultaron sin modificar sus ramas. CI #54/#55 permanece como evidencia declarada, no
reverificada por ejecucion manual durante P3.

## 4. Fuentes externas

| ID | Fuente oficial | Aporta | No demuestra |
|---|---|---|---|
| E1 | [Licencia ObjectARX publicada por Autodesk](https://aps.autodesk.com/developer/overview/autocad-objectarx-sdk-licensing) | Permiso limitado de uso y copias para desarrollo; acuerdo y avisos en copias entregadas | GitHub-hosted runners, NuGet exacto o asientos AutoCAD |
| E2 | [ObjectARX SDK](https://aps.autodesk.com/developer/overview/objectarx-autocad-sdk) | SDK oficial para desarrollar extensiones AutoCAD | Autorizacion del canal NuGet exacto |
| E3 | [Componentes .NET de AutoCAD 2025](https://help.autodesk.com/cloudhelp/2025/PLK/OARX-DevGuide-Managed/files/GUID-8657D153-0120-4881-A3C8-E00ED139E0D3.htm) | DLL principales, SDK y `Copy Local=False` | Redistribuibilidad de los paquetes auditados |
| E4 | [Tutorial APS para plugin](https://get-started.aps.autodesk.com/tutorials/design-automation/prepare-plugin/) | Uso oficial de Core/Model en desarrollo APS | Endoso versionado de los tres paquetes exactos |
| E5 | Paginas oficiales NuGet de [AutoCAD.NET](https://www.nuget.org/packages/AutoCAD.NET/25.0.1), [Core](https://www.nuget.org/packages/AutoCAD.NET.Core/25.0.0) y [Model](https://www.nuget.org/packages/AutoCAD.NET.Model/25.0.0) | Owner mostrado, versiones y metadata de galeria | Autorizacion contractual de Autodesk para CI |
| E6 | [Prefijos reservados de NuGet](https://learn.microsoft.com/nuget/nuget-org/id-prefix-reservation) | Significado limitado de `verified` | Autenticidad o falsedad por si sola |
| E7 | [PackageReference y lock](https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files) | Assets, `packages.lock.json` y locked mode | Permiso contractual sobre bytes Autodesk |
| E8 | [Paquetes firmados](https://learn.microsoft.com/nuget/reference/signed-packages-reference) | Alcance tecnico de firmas de autor y repositorio | Licencia de uso |
| E9 | [Runners alojados](https://docs.github.com/actions/reference/runners/github-hosted-runners) | VM alojada y ciclo de vida administrado | Permiso Autodesk para entregar copias al proveedor |
| E10 | [Caching](https://docs.github.com/actions/reference/workflows-and-actions/dependency-caching) y [artifacts](https://docs.github.com/actions/tutorials/store-and-share-data) | Persistencia y diferencias operativas | Autorizacion para material Autodesk |

## 5. Revalidacion independiente de paquetes y assemblies

| Elemento | Resultado | Método | Alcance | Limitación |
|---|---|---|---|---|
| SHA-256 | Coincidente con E1 | Recalculado desde nupkg exacto | Integridad del archivo auditado | No concede autorización |
| SHA-512 | Coincidente con catálogo NuGet | Comparación con fuente oficial | Integridad de distribución | No acredita permiso contractual |
| LICENSE.txt | Idéntico | Hash y comparación de contenido | Texto incluido en paquetes | Requiere interpretación competente |
| Firma de autor | Autodesk, Inc. | Verificación CMS | Procedencia e integridad | No define todos los usos permitidos |
| Firma de repositorio | NuGet.org Repository by Microsoft | Verificación CMS | Integridad del repositorio | No sustituye licencia |
| Owner | Autodesk | Página oficial NuGet | Control de galería observado | No equivale por sí solo a autorización |
| verified | false | Metadatos NuGet | Ausencia de prefijo reservado | No implica falsedad |
| Assemblies | 13 DLL | Inventario de paquetes | Composición técnica | No determina licencia |
| ReferenceAssemblyAttribute | 7 presentes, 6 ausentes | Inspección de metadatos | Naturaleza técnica mixta | No determina permiso de uso |
| Referencias principales | AcMgd, AcCoreMgd y AcDbMgd sin atributo y con cuerpos | Inspección técnica | Material usado por RackCad.Plugin | No resuelve autorización de CI |

Los paquetes y temporales usados por la revalidacion fueron eliminados. No se agregaron nupkg, DLL,
caches, certificados ni logs completos al repositorio.

## 6. Inventario de paquetes y assemblies

| Paquete | Version | Assembly | `ReferenceAssemblyAttribute` | Observacion |
|---|---:|---|---|---|
| `AutoCAD.NET` | 25.0.1 | `AcCui` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AcDx` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AcMgd` | Ausente | Referencia principal; contiene cuerpos |
| `AutoCAD.NET` | 25.0.1 | `AcMr` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AcSeamless` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AcTcMgd` | Ausente | Contiene cuerpos; no es referencia principal |
| `AutoCAD.NET` | 25.0.1 | `AcWindows` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AdUIMgd` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AdUiPalettes` | Presente | Compile asset bajo `lib/net8.0` |
| `AutoCAD.NET` | 25.0.1 | `AdWindows` | Ausente | Contiene cuerpos; no es referencia principal |
| `AutoCAD.NET.Core` | 25.0.0 | `AcCoreMgd` | Ausente | Referencia principal; contiene cuerpos |
| `AutoCAD.NET.Model` | 25.0.0 | `AcDbMgd` | Ausente | Referencia principal; contiene cuerpos |
| `AutoCAD.NET.Model` | 25.0.0 | `acdbmgdbrep` | Ausente | Contiene cuerpos; no es referencia principal |

Total: trece DLL; siete con atributo y seis sin atributo.

## 7. Composicion mixta e impacto

Caracterizacion que gobierna P3:

> Los paquetes auditados contienen una composicion mixta: siete assemblies estan marcados mediante
> `ReferenceAssemblyAttribute` y seis no contienen esa marca. Las tres referencias principales
> utilizadas por `RackCad.Plugin` —`AcMgd`, `AcCoreMgd` y `AcDbMgd`— no contienen
> `ReferenceAssemblyAttribute` y contienen cuerpos de metodos.

| Tema | Impacto |
|---|---|
| Compilacion de `RackCad.Plugin` | No cambia: E1/E2 ya demostraron resolucion y compilacion con los bytes exactos |
| `Private=false` | No cambia; es metadata efectiva de resolucion, independiente del atributo |
| `CopyLocal=false` | No cambia; sigue siendo obligatorio y fue observado para las trece referencias |
| Riesgo de material ejecutable | Aumenta la cautela para las seis DLL con cuerpos, especialmente las tres principales; no prueba que deban ejecutarse |
| Redistribucion | No se amplian derechos; la ausencia del atributo no prueba redistribuibilidad ni naturaleza runtime |
| Caching | No autoriza persistencia; se mantiene solo cache efimero dentro del job |
| Artifacts | No cambia la prohibicion de DLL, nupkg, cache o contenido extraido Autodesk |
| Procedencia | No cambia hashes, firmas, owner ni canal observados |
| Integridad | No cambia la coincidencia criptografica de los archivos auditados |
| Preguntas contractuales | No resuelve ninguna por si sola; `ReferenceAssemblyAttribute` no equivale a licencia |
| Recomendacion | Refuerza el tratamiento conservador, pero no convierte automaticamente la salida en C |
| I-13 | Debe corregir posteriormente la generalizacion sobre las trece DLL, con trazabilidad a P3 |

E1 y E2 no quedan automaticamente invalidadas: sus pruebas operaron sobre estos mismos paquetes.

## 8. Limites de la evidencia

- Los hechos criptograficos prueban integridad y aportan procedencia, no autorizacion contractual.
- El owner de galeria y `verified=false` no sustituyen la identidad de la firma ni la licencia.
- La licencia permite copias condicionadas para desarrollo, pero no nombra GitHub Actions,
  infraestructura de terceros, caches administrados, feeds o artifacts.
- No se localizo una fuente Autodesk no-blog que enlace las tres versiones exactas de NuGet.
- No se localizo una fuente primaria que clasifique estas DLL como redistribuibles.
- E1/E2 no instalan ni ejecutan AutoCAD y no responden requisitos de licencias de producto.
- La concentracion de Owner, preparer, reviewer y approver reduce independencia.

## 9. Evaluacion de las quince preguntas

Cada respuesta conserva esta marca: **Propuesta preliminar; pendiente de decision formal de Mario
Pérez.**

### Pregunta 1

| Campo | Contenido |
|---|---|
| Número | 1 |
| Categoría | Licencia e infraestructura |
| Pregunta exacta | ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted? |
| Hechos técnicos comprobados | E2 compilo en una VM GitHub-hosted efimera sin AutoCAD; cero material Autodesk salio del cache del job. |
| Política interna aplicable | Cero NuGet en producto; excepcion y merge no autorizados. |
| Fuente contractual u oficial | E1, E9 e I4/I5. |
| Inferencias necesarias | Que la copia en infraestructura de GitHub queda dentro del permiso de copias para desarrollo. |
| Evidencia en contra o limitaciones | La licencia no nombra CI alojado, terceros ni GitHub. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Entregar/procesar copias fuera del alcance autorizado. |
| Riesgo si se rechaza | Perder compilacion alojada del Plugin. |
| Respuesta interna propuesta | No autorizar el merge hasta obtener confirmacion escrita competente sobre el runner alojado. |
| Restricciones propuestas | Solo restore efimero, compile-only, sin AutoCAD, cache persistente, artifacts ni redistribucion. |
| ¿Requiere asesoría legal externa? | Si. |
| Justificación | Cuestion contractual material sin fuente primaria suficientemente directa. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 2

| Campo | Contenido |
|---|---|
| Número | 2 |
| Categoría | Licencia y caching |
| Pregunta exacta | ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida? |
| Hechos técnicos comprobados | Es la copia minima usada solo durante E2 y destruida con la VM. |
| Política interna aplicable | Solo se contempla almacenamiento aislado dentro del job; excepcion pendiente. |
| Fuente contractual u oficial | E1 e I4. |
| Inferencias necesarias | Que una copia automatizada y efimera equivale a copia para desarrollo. |
| Evidencia en contra o limitaciones | El acuerdo permite copias de desarrollo, pero no define caches de CI. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Media. |
| Riesgo si se acepta | Copia no cubierta por el acuerdo o sin avisos suficientes. |
| Riesgo si se rechaza | GitHub-hosted restore deja de ser viable. |
| Respuesta interna propuesta | Mantenerla bloqueada salvo confirmacion conjunta con la pregunta 1. |
| Restricciones propuestas | Ruta fuera del checkout, acceso del job, borrado con VM y auditoria de salidas. |
| ¿Requiere asesoría legal externa? | Si. |
| Justificación | La finalidad parece alineada, pero la modalidad no esta nombrada. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 3

| Campo | Contenido |
|---|---|
| Número | 3 |
| Categoría | Infraestructura de terceros |
| Pregunta exacta | ¿Puede GitHub actuar como proveedor de infraestructura para ese uso? |
| Hechos técnicos comprobados | GitHub aloja la VM y procesa los bytes durante el job. |
| Política interna aplicable | Ningun proveedor esta autorizado para este material por politica vigente. |
| Fuente contractual u oficial | E1 y E9. |
| Inferencias necesarias | Que el proveedor puede recibir o custodiar la copia condicionada. |
| Evidencia en contra o limitaciones | No se localizo clausula sobre proveedores, subprocesadores o territorio del runner. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Omitir condiciones contractuales aplicables al tercero. |
| Riesgo si se rechaza | Requiere runner autorizado o build local. |
| Respuesta interna propuesta | No reconocer a GitHub como proveedor autorizado sin criterio escrito competente. |
| Restricciones propuestas | Si se confirma, permisos minimos, VM efimera, sin secrets ni persistencia Autodesk. |
| ¿Requiere asesoría legal externa? | Si. |
| Justificación | La licencia no resuelve el papel de GitHub. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 4

| Campo | Contenido |
|---|---|
| Número | 4 |
| Categoría | Caching persistente |
| Pregunta exacta | ¿Puede usarse `actions/cache`? |
| Hechos técnicos comprobados | No se uso en E2; GitHub conserva y reutiliza las rutas cacheadas entre runs. |
| Política interna aplicable | Caso no solicitado y no autorizado. |
| Fuente contractual u oficial | E10, I2 e I5. |
| Inferencias necesarias | Ninguna para prohibirlo internamente. |
| Evidencia en contra o limitaciones | No existe autorizacion contractual especifica. |
| Clasificación preliminar | Resuelta por política interna. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Mayor retencion, audiencia y riesgo de poisoning o copia no autorizada. |
| Riesgo si se rechaza | Mayor latencia y dependencia de nuget.org. |
| Respuesta interna propuesta | No; mantenerlo prohibido para material Autodesk. |
| Restricciones propuestas | Ninguna excepcion implicita; una solicitud futura exige evaluacion separada. |
| ¿Requiere asesoría legal externa? | No para mantener la prohibicion; si para solicitarlo en el futuro. |
| Justificación | La politica conservadora resuelve el alcance actual sin afirmar prohibicion legal. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 5

| Campo | Contenido |
|---|---|
| Número | 5 |
| Categoría | Integridad y lock |
| Pregunta exacta | ¿Puede conservarse `packages.lock.json`? |
| Hechos técnicos comprobados | Contiene IDs, versiones, grafo y `contentHash`, no assemblies; E2 uso uno temporal. |
| Política interna aplicable | No hay lock permanente aprobado para la excepcion propuesta. |
| Fuente contractual u oficial | E7 e I4. |
| Inferencias necesarias | Que metadata de integridad no constituye una copia del Software. |
| Evidencia en contra o limitaciones | La licencia no trata locks; el proyecto alterna dos grafos. |
| Clasificación preliminar | Respuesta condicionada. |
| Nivel de confianza | Media. |
| Riesgo si se acepta | Presentar como libre metadata no evaluada o gobernar mal el grafo local. |
| Riesgo si se rechaza | Pierde locked mode y reproducibilidad directa. |
| Respuesta interna propuesta | Si, solo tras aprobar la excepcion y un diseño de lock permanente correcto. |
| Restricciones propuestas | Solo metadata; sin bytes; revision de cambios; locked mode; ubicacion aprobada. |
| ¿Requiere asesoría legal externa? | No como decision tecnica interna; incluirla en la consulta si se busca certeza contractual completa. |
| Justificación | NuGet recomienda locks para CI, pero eso no decide Autodesk. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 6

| Campo | Contenido |
|---|---|
| Número | 6 |
| Categoría | Integridad |
| Pregunta exacta | ¿Pueden conservarse `contentHash` y hashes? |
| Hechos técnicos comprobados | Son digestos unidireccionales; no contienen DLL ni nupkg y detectan sustitucion. |
| Política interna aplicable | Evidencia textual de integridad permitida; binarios prohibidos. |
| Fuente contractual u oficial | E7, E8 e I4. |
| Inferencias necesarias | Que conservar un digesto no redistribuye el Software. |
| Evidencia en contra o limitaciones | No otorgan licencia ni autentican autoridad editorial por si solos. |
| Clasificación preliminar | Resuelta por política interna. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Falsa confianza si se presentan como autorizacion. |
| Riesgo si se rechaza | Debilita la deteccion de cambios de bytes. |
| Respuesta interna propuesta | Si, como evidencia de integridad y nunca como permiso. |
| Restricciones propuestas | Conservar solo valores y contexto; no contenido codificado ni paquetes. |
| ¿Requiere asesoría legal externa? | No para el alcance interno propuesto. |
| Justificación | La separacion entre hash y bytes es tecnica y la politica permite la evidencia. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 7

| Campo | Contenido |
|---|---|
| Número | 7 |
| Categoría | Material y licencia |
| Pregunta exacta | ¿Es admisible que el paquete contenga assemblies de implementacion? |
| Hechos técnicos comprobados | Composicion mixta: 7 con atributo y 6 sin el; las 3 principales no lo tienen y contienen cuerpos. |
| Política interna aplicable | Deben permanecer compile-only y fuera de outputs, caches persistentes y distribucion. |
| Fuente contractual u oficial | E1, E3 y revalidacion P3. |
| Inferencias necesarias | Que el permiso sobre el Software cubre el uso compile-only de esas DLL con cuerpos. |
| Evidencia en contra o limitaciones | El atributo no es licencia; ausencia no prueba runtime o redistribuibilidad. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Aplicar controles de stubs a material con cuerpos ejecutables. |
| Riesgo si se rechaza | Los paquetes dejan de ser canal viable de referencias CI. |
| Respuesta interna propuesta | Tratar conservadoramente las seis DLL como material completo hasta criterio competente. |
| Restricciones propuestas | Nunca ejecutar, copiar, cachear persistentemente, publicar o redistribuir. |
| ¿Requiere asesoría legal externa? | Si. |
| Justificación | La propiedad tecnica corrige precision, pero no concede uso. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 8

| Campo | Contenido |
|---|---|
| Número | 8 |
| Categoría | Procedencia y canal |
| Pregunta exacta | ¿NuGet es un canal autorizado por Autodesk? |
| Hechos técnicos comprobados | Owner Autodesk; firma CMS Autodesk, Inc.; firma de repositorio NuGet.org; hashes coincidentes. |
| Política interna aplicable | Solo nuget.org seria fuente tecnica permitida si se aprueba la excepcion. |
| Fuente contractual u oficial | E2, E4, E5, E8. |
| Inferencias necesarias | Que la publicacion firmada y el tutorial general equivalen a autorizacion del canal/versiones exactos. |
| Evidencia en contra o limitaciones | No se localizo enlace Autodesk no-blog a los tres paquetes exactos. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Confiar en evidencia editorial sin confirmacion primaria directa. |
| Riesgo si se rechaza | Exigir SDK u otro canal y repetir E1/E2. |
| Respuesta interna propuesta | Solicitar confirmacion escrita de Autodesk o asesor competente antes del merge. |
| Restricciones propuestas | Versiones y hashes exactos; nuget.org unico; firmas obligatorias; fail-closed. |
| ¿Requiere asesoría legal externa? | Si, salvo confirmacion primaria directa de Autodesk. |
| Justificación | Procedencia robusta no equivale a autorizacion de canal. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 9

| Campo | Contenido |
|---|---|
| Número | 9 |
| Categoría | Procedencia |
| Pregunta exacta | ¿`verified=false` requiere validacion adicional? |
| Hechos técnicos comprobados | NuGet usa `verified` para prefijo reservado; `false` no afirma que el paquete sea falso. |
| Política interna aplicable | P3 ya exige firma de autor/repositorio, hashes, owner, nuspec, licencia y versiones. |
| Fuente contractual u oficial | E5, E6 y E8. |
| Inferencias necesarias | Que las capas compensatorias alcanzan el umbral interno de procedencia tecnica. |
| Evidencia en contra o limitaciones | No resuelven por si mismas la autorizacion contractual del canal. |
| Clasificación preliminar | Resuelta técnicamente. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Confundir validacion tecnica adicional con licencia. |
| Riesgo si se rechaza | Bloqueo basado en una interpretacion incorrecta del indicador. |
| Respuesta interna propuesta | Si requiere controles compensatorios; los aplicados son suficientes para procedencia tecnica, no legal. |
| Restricciones propuestas | Mantener verificacion CMS, hashes y owner en cada cambio de version. |
| ¿Requiere asesoría legal externa? | No para interpretar `verified`; la pregunta 8 sigue abierta. |
| Justificación | Microsoft define de forma expresa el indicador. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 10

| Campo | Contenido |
|---|---|
| Número | 10 |
| Categoría | Persistencia y redistribucion interna |
| Pregunta exacta | ¿Puede usarse un feed privado? |
| Hechos técnicos comprobados | No fue probado; conservaria y serviria copias completas. |
| Política interna aplicable | Fuera de alcance y no autorizado. |
| Fuente contractual u oficial | E1, I2 e I5. |
| Inferencias necesarias | Ninguna para prohibirlo internamente. |
| Evidencia en contra o limitaciones | La licencia no autoriza especificamente esta modalidad. |
| Clasificación preliminar | Resuelta por política interna. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Copia persistente, audiencia ampliada y avisos/retencion insuficientes. |
| Riesgo si se rechaza | Restore depende de nuget.org o de otra provision autorizada. |
| Respuesta interna propuesta | No usar feed privado. |
| Restricciones propuestas | Cualquier solicitud futura requiere evaluacion contractual, acceso, avisos y retencion separados. |
| ¿Requiere asesoría legal externa? | No para mantener la prohibicion actual. |
| Justificación | No forma parte del caso solicitado. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 11

| Campo | Contenido |
|---|---|
| Número | 11 |
| Categoría | Infraestructura alternativa |
| Pregunta exacta | ¿Puede usarse un runner autohospedado? |
| Hechos técnicos comprobados | No fue probado; puede usar SDK o AutoCAD instalado bajo control organizacional. |
| Política interna aplicable | Requiere iniciativa y autorizacion separadas. |
| Fuente contractual u oficial | E1, E2, E3 e I4. |
| Inferencias necesarias | Que la maquina, usuarios, copias y licencias de producto cumplen sus acuerdos. |
| Evidencia en contra o limitaciones | No se conocen operacion, aislamiento, retencion ni licencias del host. |
| Clasificación preliminar | Respuesta condicionada. |
| Nivel de confianza | Baja. |
| Riesgo si se acepta | Trasladar la ambiguedad a una maquina persistente y posiblemente licenciada de forma incorrecta. |
| Riesgo si se rechaza | Pierde la principal alternativa al runner alojado. |
| Respuesta interna propuesta | No autorizar dentro de I-29/P3; evaluar como alternativa independiente. |
| Restricciones propuestas | Inventario de software, identidad de usuarios, aislamiento, retencion, licencias y auditoria. |
| ¿Requiere asesoría legal externa? | Si, si el diseño usa copias Autodesk o AutoCAD instalado. |
| Justificación | La topologia concreta no existe y puede activar acuerdos distintos. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 12

| Campo | Contenido |
|---|---|
| Número | 12 |
| Categoría | Avisos y atribucion |
| Pregunta exacta | ¿Hay obligaciones de avisos o atribucion? |
| Hechos técnicos comprobados | Los paquetes incluyen `LICENSE.txt` identico y avisos; E2 no modifico ni redistribuyo paquetes. |
| Política interna aplicable | No retirar avisos ni inventar atribuciones; no distribuir material Autodesk. |
| Fuente contractual u oficial | E1, clausulas de copias y copyright. |
| Inferencias necesarias | El restore intacto preserva los avisos dentro del paquete. |
| Evidencia en contra o limitaciones | No se determino un aviso adicional para distribuir solo RackCad. |
| Clasificación preliminar | Resuelta por fuente contractual explícita. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Omitir acuerdo/avisos en una copia entregada. |
| Riesgo si se rechaza | Agregar atribucion innecesaria o confusa. |
| Respuesta interna propuesta | Conservar acuerdo y avisos en toda copia Autodesk permitida; no añadir material al bundle RackCad. |
| Restricciones propuestas | Paquete intacto en cache efimero; prohibida redistribucion; revisar avisos de RackCad por separado si cambia el modelo. |
| ¿Requiere asesoría legal externa? | No para la obligacion expresa; si se pretende redistribuir o definir avisos del producto final. |
| Justificación | El acuerdo exige conservar acuerdo y avisos en copias entregadas. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 13

| Campo | Contenido |
|---|---|
| Número | 13 |
| Categoría | Distribucion de RackCad |
| Pregunta exacta | ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado? |
| Hechos técnicos comprobados | E1/E2 y la guarda produjeron bundle con solo cuatro DLL RackCad; Autodesk exige `Copy Local=False`. |
| Política interna aplicable | Distribucion de RackCad sin DLL Autodesk; AutoCAD requerido en runtime. |
| Fuente contractual u oficial | E1, E3 e I4/I5. |
| Inferencias necesarias | Que la licencia de desarrollo permite distribuir la aplicacion resultante que no contiene el Software. |
| Evidencia en contra o limitaciones | La licencia consultada no regula expresamente esa distribucion ni todos los acuerdos de producto. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Media. |
| Riesgo si se acepta | Distribuir bajo un supuesto contractual incompleto. |
| Riesgo si se rechaza | Bloquear el modelo actual aun con bundle limpio. |
| Respuesta interna propuesta | Mantener el modelo tecnico, pero no usar P3 como autorizacion contractual de distribucion. |
| Restricciones propuestas | Cero material Autodesk, dependencia runtime documentada, inspeccion recursiva y validacion en AutoCAD licenciado. |
| ¿Requiere asesoría legal externa? | Si para una confirmacion material de distribucion. |
| Justificación | La limpieza del bundle no concede el derecho sobre la aplicacion resultante. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 14

| Campo | Contenido |
|---|---|
| Número | 14 |
| Categoría | Uso interno y externo |
| Pregunta exacta | ¿Difiere uso interno de distribucion externa? |
| Hechos técnicos comprobados | El flujo de build puede ser identico; la audiencia y transferencia del producto difieren. |
| Política interna aplicable | Ambos casos deben excluir material Autodesk; no existe autorizacion externa especifica. |
| Fuente contractual u oficial | E1 e I2. |
| Inferencias necesarias | Que el acuerdo aplica igual o distinto a cada audiencia y modelo comercial. |
| Evidencia en contra o limitaciones | Las fuentes consultadas no resuelven expresamente esa diferencia para RackCad. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Baja. |
| Riesgo si se acepta | Extender una conclusion interna a una audiencia externa no cubierta. |
| Riesgo si se rechaza | Restringir innecesariamente un modelo permitido. |
| Respuesta interna propuesta | Separar ambos escenarios y no autorizar distribucion externa por analogia. |
| Restricciones propuestas | Alcance, destinatarios, avisos, soporte y ausencia de material Autodesk documentados por separado. |
| ¿Requiere asesoría legal externa? | Si. |
| Justificación | La evidencia tecnica no decide audiencias contractuales. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

### Pregunta 15

| Campo | Contenido |
|---|---|
| Número | 15 |
| Categoría | Licencias AutoCAD |
| Pregunta exacta | ¿Se requiere una licencia AutoCAD por cada entorno de build? |
| Hechos técnicos comprobados | E2 no instalo ni ejecuto AutoCAD; solo restauro paquetes ObjectARX. |
| Política interna aplicable | Compilar, instalar AutoCAD, ejecutar AutoCAD y usar servicios Autodesk son casos distintos. |
| Fuente contractual u oficial | E1, E2, E3 e I4. |
| Inferencias necesarias | Que un build SDK sin producto no consume asiento, o que si lo consume por otro acuerdo. |
| Evidencia en contra o limitaciones | La licencia ObjectARX no responde licenciamiento de producto por entorno. |
| Clasificación preliminar | Requiere asesoría legal externa. |
| Nivel de confianza | Alta. |
| Riesgo si se acepta | Coste u operacion innecesarios si se exige sin base. |
| Riesgo si se rechaza | Incumplir un requisito de licencia aplicable no identificado. |
| Respuesta interna propuesta | No concluir si o no; solicitar confirmacion expresa antes de adoptar el entorno. |
| Restricciones propuestas | Runner sin AutoCAD ni servicios; no usar credenciales o licencias de producto hasta resolver. |
| ¿Requiere asesoría legal externa? | Si. |
| Justificación | Es una cuestion de producto distinta del permiso ObjectARX. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |

## 10. Riesgos transversales

- Interpretar integridad, owner, firma o `ReferenceAssemblyAttribute` como autorizacion.
- Crear persistencia accidental mediante cache, artifact, feed, Git, imagen o logs con contenido.
- Relajar `Private=false`, `CopyLocal=false`, `ExcludeAssets=runtime` o la inspeccion de outputs.
- Cambiar versiones, SDK, TFM, runner o fuente sin repetir revision.
- Confundir compilacion verde con runtime, redistribucion o licencia AutoCAD.
- Tomar una decision sin independencia, autoridad documentada, vigencia o mecanismo verificable.

## 11. Recomendacion preliminar

Opcion recomendada para decision humana: **D. Requiere asesoria legal externa**. Confianza: media.
No se marca formalmente la opcion.

Fundamento: E1/E2 prueban que el mecanismo compila y puede mantener limpio el bundle; la licencia
ObjectARX concede copias condicionadas para desarrollo. Sin embargo, las fuentes primarias no
responden de forma suficientemente directa si GitHub puede alojar la copia ni si nuget.org es el
canal autorizado para las versiones exactas. Esas brechas afectan el caso central, no solo una
variante opcional.

Razones para descartar provisionalmente:

- A: la ambiguedad de infraestructura/canal y la excepcion cero NuGet impiden aprobacion irrestricta.
- B: podria ser resultado futuro, pero hoy presupone resueltas las preguntas 1, 2, 3, 7 y 8.
- C: no se encontro prohibicion expresa; la licencia permite copias para desarrollo y E1/E2 siguen
  tecnicamente validos.

Si la revision externa confirma el caso central, B seria la alternativa proporcional bajo todas
las restricciones: copia efimera, compile-only, runner sin AutoCAD, nuget.org y versiones fijadas,
sin `actions/cache`, feeds, artifacts ni redistribucion, bundle limpio, guardas fail-closed y
rollback coordinado.

## 12. Elementos pendientes de decision formal

- seleccion formal A/B/C/D;
- respuestas atribuidas de Mario Pérez a las quince preguntas;
- firma, fecha de aprobacion, autoridad, registro corporativo y conflictos;
- vigencia, revision y responsable de cumplimiento;
- aceptacion, cambio o rechazo de ADR-0003;
- excepcion expresa a cero NuGet;
- autorizacion o bloqueo final del merge de I-13;
- cierre o rollback de I-13.

Hasta entonces ADR-0003 permanece propuesto, cero NuGet sigue vigente, I-13 sigue abierta y
bloqueada y P4 no puede ejecutarse.

## 13. Deuda documental de I-13

> La documentación heredada de I-13 contiene una generalización sobre la naturaleza de los trece
> assemblies que debe corregirse posteriormente. La corrección deberá realizarse en P4 o en una
> ejecución documental específica autorizada, manteniendo trazabilidad con la revalidación de I-29.

| Campo | Registro |
|---|---|
| Afirmacion heredada | Las trece DLL no son reference assemblies formales o son assemblies de implementacion de naturaleza homogenea. |
| Caracterizacion corregida | Siete contienen `ReferenceAssemblyAttribute`; seis no; las tres principales estan entre las seis y contienen cuerpos. |
| Archivos potencialmente afectados | Documento I-13 en `experiment/refs-autocad-ci` y documentos derivados en `architecture/referencias-autocad-ci`. |
| Por que no se corrigen en P3 | Esas ramas son estrictamente de solo lectura y el alcance autoriza solo documentacion I-29. |
| Condicion futura | P4 o ejecucion documental especifica autorizada, sin alterar evidencia tecnica y con trazabilidad a esta matriz. |
