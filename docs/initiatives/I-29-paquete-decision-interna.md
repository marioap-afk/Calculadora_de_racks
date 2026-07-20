# I-29 — Paquete de decision interna

Este paquete solicita una decision interna sobre el uso de referencias Autodesk durante la
compilacion de RackCad. No ofrece asesoramiento legal, no responde en nombre del reviewer y no
selecciona una salida. Los detalles tecnicos completos permanecen en I-13; aqui se presenta solo la
informacion necesaria para decidir.

Version del paquete: P1, 2026-07-20. Estado: decision humana pendiente; gate L2 abierto.

## 1. Resumen ejecutivo

RackCad es un complemento de AutoCAD 2025. Para detectar errores antes de integrar cambios, se busca
compilar tambien su componente `RackCad.Plugin` en GitHub Actions. Hoy el build local obtiene las
referencias de una instalacion de AutoCAD; un runner alojado por GitHub no tiene esa instalacion.

I-13 probo una alternativa: descargar desde nuget.org tres paquetes que contienen las APIs de
AutoCAD, usarlos unicamente como referencias de compilacion y evitar que sus DLL formen parte del
producto entregado. E1 demostro el mecanismo en aislamiento local y E2 declaro un build exitoso en
un runner Windows sin AutoCAD. La promocion reducida conserva controles de versiones, origen,
referencias y outputs.

La prueba tecnica no determina si la licencia permite las copias en infraestructura alojada, si
GitHub puede procesarlas, si pueden conservarse locks/hashes o que obligaciones aplican a uso
interno y distribucion externa. Tampoco modifica la politica interna que hoy prohibe paquetes NuGet
en codigo de producto.

Por eso el merge sigue bloqueado. Se solicita que personas identificadas y con autoridad suficiente
revisen las fuentes, respondan las quince preguntas y seleccionen exactamente una salida:

- A. Aprobado.
- B. Aprobado con restricciones.
- C. Rechazado.
- D. Requiere asesoria legal externa.

Este paquete no favorece ninguna salida. ADR-0003 permanece propuesto, la excepcion cero NuGet no
esta vigente e I-13 no esta cerrada.

## 2. Arquitectura tecnica propuesta

`RackCad.Plugin` necesita tipos de AutoCAD para compilar. La propuesta mantiene dos caminos
mutuamente excluyentes:

```text
Desarrollo y runtime local
  AutoCAD instalado -> referencias locales -> compilacion -> AutoCAD ejecuta RackCad

CI condicionado
  paquetes Autodesk -> referencias de compilacion -> RackCad.Plugin.dll
                                         |
                                         +-> DLL Autodesk NO se copian
```

En CI, el `PackageReference` se activa solo mediante una propiedad explicita. `PrivateAssets=all`
evita propagar la dependencia a consumidores y `ExcludeAssets=runtime` excluye sus assets runtime.
La resolucion observada exige `Private=false` y `CopyLocal=false`. Una guarda revisa que ninguna DLL
Autodesk entre en `ReferenceCopyLocalPaths`, outputs o bundle.

Compilar no equivale a ejecutar: AutoCAD instalado sigue siendo obligatorio para `NETLOAD`, comandos,
transacciones, WPF dentro del host, bloques reales y validacion funcional.

## 3. Material Autodesk utilizado

| Paquete | Version | Finalidad tecnica | Material observado | Estado de revalidacion |
|---|---:|---|---|---|
| `AutoCAD.NET` | 25.0.1 | Referencia directa; aporta `AcMgd` y otros compile assets | Assemblies bajo `lib/net8.0` | Evidencia heredada de I-13, pendiente de revalidacion independiente |
| `AutoCAD.NET.Core` | 25.0.0 | Dependencia transitiva; aporta `AcCoreMgd` | Assembly bajo `lib/net8.0` | Evidencia heredada de I-13, pendiente de revalidacion independiente |
| `AutoCAD.NET.Model` | 25.0.0 | Dependencia transitiva; aporta `AcDbMgd` y `acdbmgdbrep` | Assemblies bajo `lib/net8.0` | Evidencia heredada de I-13, pendiente de revalidacion independiente |

I-13 observo trece DLL administradas en total y concluyo que no son reference assemblies formales:
estan en `lib`, no en `ref`, y contienen implementacion significativa. Ese hecho tecnico no decide
por si solo el permiso aplicable. Las versiones son especificas; cualquier cambio requiere nueva
revision tecnica, de procedencia y de licencia.

## 4. Flujo de bytes

```text
nuget.org
   |
   | descarga durante el job
   v
runner GitHub-hosted efimero
   |
   v
NUGET_PACKAGES aislado del job
   |
   | lectura como referencias de compilacion
   v
compilador .NET -> DLL RackCad
   |
   v
validacion de referencias, outputs y bundle
   |
   v
fin del job -> destruccion del runner y de su cache local
```

Nuget.org conserva el paquete publicado. GitHub aloja y procesa la copia de trabajo en la VM. El
job puede leerla durante la compilacion. Bajo el diseño solicitado, RackCad no la sube como artifact,
no la guarda en Git ni en un feed y no la copia al bundle. La eliminacion del runner corresponde al
ciclo de vida de la infraestructura alojada; la autorizacion contractual para ese flujo es una de
las preguntas pendientes.

## 5. Material que no se conserva

### Estado tecnico observado

E1/E2 y la promocion declaran cero DLL Autodesk en `ReferenceCopyLocalPaths`, `bin`, bundle o
artifacts del job de Plugin. E2 uso un lock temporal y un cache de paquetes aislado.

### Restricciones cuya adopcion se solicita

- no versionar DLL Autodesk;
- no versionar archivos `.nupkg`;
- no publicar artifacts que contengan material Autodesk;
- no incluir material Autodesk en el bundle de RackCad;
- no usar feed privado;
- no usar `actions/cache` sin autorizacion especifica;
- no conservar la cache del runner entre jobs o runs; y
- no redistribuir assemblies Autodesk con RackCad.

Estas son restricciones del diseño propuesto, no una conclusion de P1 sobre lo que la licencia
permite o prohibe.

## 6. Guardas tecnicas

| Guarda | Funcion | Estado/evidencia |
|---|---|---|
| Versiones exactas | Evitar actualizaciones flotantes | 25.0.1/25.0.0/25.0.0 en promocion |
| Fuente unica | Limitar restore a nuget.org | Configuracion temporal fail-closed |
| Lock temporal/locked mode | Confirmar el grafo exacto | Probado en E2; permanencia pendiente de decision |
| SHA-256 y contentHash | Detectar cambio de bytes | Evidencia heredada de I-13 pendiente de revalidacion |
| Firma de autor | Aportar integridad y procedencia tecnica | Evidencia heredada, atribuida en I-13 a Autodesk, Inc. |
| Firma de repositorio | Aportar integridad del canal | Evidencia heredada, atribuida a NuGet.org Repository by Microsoft |
| `Private=false` | Evitar copia local de referencias resueltas | Verificado por el script propuesto |
| `CopyLocal=false` | Evitar que DLL Autodesk entren al output | Verificado por el script propuesto |
| Inventario de outputs | Fallar ante DLL Autodesk fuera de cache | Script de promocion |
| Bundle limpio | Exigir solo cuatro DLL RackCad | Script de promocion |
| Rollback coordinado | Retirar proyecto, CI y documentacion juntos | Definido en ADR propuesto y en la seccion 12 |

Las firmas, hashes, owners y builds verdes son controles tecnicos. Ninguno concede por si mismo
autorizacion legal o contractual.

## 7. Politica cero NuGet y excepcion propuesta

La regla vigente de RackCad es cero paquetes NuGet en codigo de producto; solo los proyectos de
pruebas tienen la excepcion expresamente escrita. Como `RackCad.Plugin` es producto, incluso un
`PackageReference` condicional y compile-only constituye una excepcion.

La excepcion solicitada seria estrecha:

- solo `RackCad.Plugin`;
- solo compilacion CI condicionada;
- solo los tres paquetes y versiones de la seccion 3;
- sin runtime assets, propagacion, copia local o distribucion;
- sin extenderse a otro proyecto, paquete, version o uso; y
- con revision al cambiar AutoCAD, .NET, runner, fuente o guardas.

La excepcion no esta vigente. Requiere una decision L2 suficiente y ADR-0003 aceptado por el dueño
cuando corresponda. No crea una categoria general de dependencias de build.

## 8. Casos de uso cuya autorizacion se solicita

| Caso | Autorizacion solicitada | Controles | Respuesta pendiente |
|---|---|---|---|
| Descarga desde nuget.org | Obtener las tres versiones exactas durante el job | Fuente unica, versiones y grafo fijados | Reviewer y approver |
| GitHub-hosted runner | Procesar el material en una VM Windows alojada por GitHub | Runner sin AutoCAD, permisos minimos | Reviewer y approver |
| Copia efimera | Mantener el paquete extraido solo durante el job | `NUGET_PACKAGES` aislado y destruccion al finalizar | Reviewer |
| Compilacion interna | Usar assemblies como referencias compile-only | `Private=false`, `CopyLocal=false`, runtime excluido | Reviewer y approver |
| Hashes/contentHash | Conservar metadata de integridad sin conservar assemblies | Sin bytes del paquete en Git | Reviewer |
| `packages.lock.json` | Conservar o no un lock permanente para locked mode | Ubicacion y alcance por definir | Reviewer y approver |
| Distribuir RackCad sin material Autodesk | Entregar solo DLL RackCad, catalogos y manifiesto | Inspeccion recursiva y bundle limpio | Reviewer y approver |
| Uso interno | Usar el build dentro de la organizacion | Audiencia y responsables por definir | Reviewer y approver |
| Distribucion externa sin assemblies Autodesk | Entregar RackCad que depende de AutoCAD instalado | No redistribuir DLL Autodesk; avisos por definir | Reviewer y approver |

## 9. Casos expresamente prohibidos o no solicitados

El paquete no solicita autorizacion para:

- versionar DLL Autodesk o `.nupkg`;
- publicar artifacts o bundles con material Autodesk;
- conservar paquetes en un feed privado;
- usar caching persistente o `actions/cache`;
- redistribuir assemblies Autodesk;
- instalar o ejecutar AutoCAD en CI;
- sustituir, compartir o evitar licencias de AutoCAD;
- usar AutoCAD 2026/2027 u otras versiones de paquetes;
- aplicar el mecanismo a proyectos distintos de `RackCad.Plugin`; o
- ampliar automaticamente la excepcion a futuras dependencias.

Una decision futura podria tratar alguno de estos casos, pero tendria que hacerlo de forma expresa;
el silencio no los incorpora al alcance solicitado.

## 10. Preguntas que requieren decision humana

| Nº | Categoria | Pregunta | Evidencia disponible | Rol que debe responder | Estado |
|---:|---|---|---|---|---|
| 1 | Licencia/infraestructura | ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted? | Licencia ObjectARX heredada, E2 y documentacion GitHub | Reviewer y approver | Pendiente de respuesta humana |
| 2 | Licencia/caching | ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida? | Flujo E2 y licencia heredada | Reviewer | Pendiente de respuesta humana |
| 3 | Infraestructura | ¿Puede GitHub actuar como proveedor de infraestructura para ese uso? | Ciclo de vida de hosted runners | Reviewer y approver | Pendiente de respuesta humana |
| 4 | Caching | ¿Puede usarse `actions/cache`? | Documentacion oficial de caching | Reviewer y approver | Pendiente de respuesta humana |
| 5 | Integridad | ¿Puede conservarse `packages.lock.json`? | Lock temporal E2 | Reviewer y preparer | Pendiente de respuesta humana |
| 6 | Integridad | ¿Pueden conservarse `contentHash` y hashes? | Hashes heredados de I-13 | Reviewer | Pendiente de respuesta humana |
| 7 | Material/licencia | ¿Es admisible que el paquete contenga assemblies de implementacion? | Inventario PE heredado de E1 | Reviewer | Pendiente de respuesta humana |
| 8 | Procedencia | ¿NuGet es un canal autorizado por Autodesk? | Owner, firmas y tutorial APS; enlace exacto incompleto | Reviewer y approver | Pendiente de respuesta humana |
| 9 | Procedencia | ¿`verified=false` requiere validacion adicional? | Prefijo no verificado; firmas/hashes heredados | Reviewer y approver | Pendiente de respuesta humana |
| 10 | Caching/redistribucion | ¿Puede usarse un feed privado? | Escenario no probado; copia persistente | Reviewer y approver | Pendiente de respuesta humana |
| 11 | Infraestructura | ¿Puede usarse un runner autohospedado? | Alternativa no probada | Reviewer, owner y operaciones | Pendiente de respuesta humana |
| 12 | Obligaciones | ¿Hay obligaciones de avisos o atribucion? | Clausulas y avisos descritos en evidencia heredada | Reviewer | Pendiente de respuesta humana |
| 13 | Distribucion | ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado? | Bundle limpio y `CopyLocal=false` | Reviewer y approver | Pendiente de respuesta humana |
| 14 | Uso interno/externo | ¿Difiere uso interno de distribucion externa? | Audiencias no resueltas por evidencia tecnica | Reviewer y approver | Pendiente de respuesta humana |
| 15 | Licencias AutoCAD | ¿Se requiere una licencia AutoCAD por cada entorno de build? | E2 no instalo ni ejecuto AutoCAD | Reviewer | Pendiente de respuesta humana |

## 11. Alternativas

| Alternativa | Ventaja | Desventaja | Impacto en I-13 | Evidencia adicional requerida |
|---|---|---|---|---|
| Mantener build del Plugin solo local | Conserva politica y flujo actuales | CI no detecta errores del Plugin | Promocion no se integra | Disciplina y evidencia de builds locales |
| Runner autohospedado autorizado | Mayor control de maquina y retencion | Operacion, aislamiento y licencias propias | Requiere rediseñar la promocion | Politica de operacion y licenciamiento del host |
| Canal Autodesk distinto | Procedencia potencialmente mas explicita | Aprovisionamiento y actualizacion nuevos | Requiere repetir E1/E2 | Confirmacion del canal, licencia, hashes y guardas |
| CI parcial sin compilar Plugin | Mantiene tests y UI actuales | Persiste la brecha de compilacion | I-13 se descarta o queda historica | Aceptacion explicita del riesgo |
| Abandonar la promocion | Elimina la excepcion NuGet | No resuelve compilacion alojada | Activa rollback y rechazo documental | Decision atribuida y registro de rollback |
| Solicitar asesoria externa | Obtiene revision especializada | Tiempo, costo y coordinacion | Merge sigue bloqueado | Preguntas, contrato, fuentes y respuesta escrita |

## 12. Rollback

Si la propuesta se rechaza, se revoca o una guarda deja de sostenerse, una fase tecnica autorizada
debe retirar coordinadamente:

1. el `PackageReference` condicional y su propiedad de seleccion;
2. el job o paso de CI que restaura y compila con esos paquetes;
3. `eng/ci/verify-autocad-references.ps1`;
4. la excepcion documental y cualquier instruccion que la presente como vigente.

Domain/Application y UI deben continuar con sus validaciones cuando corresponda. El build del Plugin
regresa al mecanismo local basado en AutoCAD instalado o queda excluido de CI hasta disponer de un
canal autorizado. Si se detecta material Autodesk en un output, la distribucion afectada permanece
bloqueada hasta recuperar un bundle limpio.

El rollback debe registrar responsable, fecha, causa, commits, validaciones y efecto sobre I-13 y
ADR-0003. P1 solo define este procedimiento; no lo ejecuta.

## 13. Decision solicitada

El final approver debe seleccionar exactamente una salida en la
[plantilla de decision](I-29-plantilla-decision.md):

- A. Aprobado.
- B. Aprobado con restricciones.
- C. Rechazado.
- D. Requiere asesoria legal externa.

La respuesta debe indicar expresamente:

| Tema | Efecto que debe registrar la decision |
|---|---|
| ADR-0003 | Si puede avanzar, debe cambiar, debe rechazarse o sigue propuesto |
| Politica cero NuGet | Si existe una excepcion, su alcance; de lo contrario, continuidad de la regla |
| I-13 | Si avanza a cierre, requiere trabajo adicional o activa rollback |
| Merge de promocion | Autorizado solo bajo condiciones expresas o permanece bloqueado |

P1 no selecciona ni recomienda una opcion y no autoriza el merge.

## 14. Firmas y aprobaciones

| Rol | Nombre | Cargo | Organizacion | Autoridad | Fecha | Firma/mecanismo verificable |
|---|---|---|---|---|---|---|
| Owner |  |  |  |  |  |  |
| Technical preparer |  |  |  |  |  |  |
| Legal/licensing reviewer |  |  |  |  |  |  |
| Final approver |  |  |  |  |  |  |

Vigencia: ____________________

Fecha de revision: ____________________

Registro corporativo: ____________________

Conflictos de interes declarados: ____________________

Fuentes revisadas y version del paquete documental: ____________________
