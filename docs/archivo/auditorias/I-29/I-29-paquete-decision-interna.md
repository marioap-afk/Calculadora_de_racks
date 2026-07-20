> **Archivo de auditoria; evidencia historica, no fuente operativa vigente.**
>
> - Naturaleza: evidencia historica del paquete de decision interna.
> - Fecha de corte: 2026-07-20.
> - Estado: archivado.
> - Decision final: B. Aprobado con restricciones.
> - Nota posterior: I-13 fue integrada y cerrada despues de estos documentos; ADR-0003 fue aceptado
>   posteriormente y la excepcion limitada a cero NuGet quedo vigente bajo su alcance.
> - Fuentes vigentes: [decision final](../../../automation/decisions/I-29.md) y
>   [contrato canonico](../../../initiatives/I-29-licencia-procedencia-autocad-ci.md).
>
> Las afirmaciones de este documento describen el estado de la revision P1-P4 al corte; no deben
> interpretarse como estado actual de I-13, ADR-0003 ni la promocion.

# I-29 — Paquete de decision interna

Este paquete solicita una decision interna sobre el uso de referencias Autodesk durante la
compilacion de RackCad. No ofrece asesoramiento legal, no responde en nombre del reviewer y no
registra una decision juridica. Los detalles tecnicos completos permanecen en I-13; aqui se presenta
la evidencia que sustento la decision interna de riesgo del Owner.

Version del paquete: P4 final, 2026-07-20. Estado: **B. Aprobado con restricciones** como decision
interna de gestion del riesgo; gobernanza tecnica e integracion bloqueadas.

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

Por eso el merge sigue bloqueado. El Owner reviso las fuentes, las quince propuestas y la
recomendacion preliminar D de P3, y selecciono:

- **B. Aprobado con restricciones.**

La decision es interna, limitada a RackCad, uso interno, las versiones auditadas y compile-only. No
afirma autorizacion expresa de Autodesk ni constituye interpretacion legal definitiva. ADR-0003
permanece propuesto, la excepcion cero NuGet no esta vigente e I-13 no esta cerrada.

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
| `AutoCAD.NET` | 25.0.1 | Referencia directa; aporta `AcMgd` y otros compile assets | Diez assemblies bajo `lib/net8.0` | Revalidado independientemente en P3 |
| `AutoCAD.NET.Core` | 25.0.0 | Dependencia transitiva; aporta `AcCoreMgd` | Un assembly bajo `lib/net8.0` | Revalidado independientemente en P3 |
| `AutoCAD.NET.Model` | 25.0.0 | Dependencia transitiva; aporta `AcDbMgd` y `acdbmgdbrep` | Dos assemblies bajo `lib/net8.0` | Revalidado independientemente en P3 |

La revalidacion independiente de P3 conto trece DLL con composicion mixta. Siete contienen
`ReferenceAssemblyAttribute`: `AcCui`, `AcDx`, `AcMr`, `AcSeamless`, `AcWindows`, `AdUIMgd` y
`AdUiPalettes`. Seis no contienen el atributo: `AcMgd`, `AcTcMgd`, `AdWindows`, `AcCoreMgd`,
`AcDbMgd` y `acdbmgdbrep`. Las tres referencias principales de RackCad pertenecen al segundo grupo
y contienen cuerpos de metodos. El atributo no concede licencia y su ausencia no prueba
redistribuibilidad ni naturaleza runtime. Las versiones son especificas; cualquier cambio requiere
nueva revision tecnica, de procedencia y de licencia.

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
job puede leerla durante la compilacion. Bajo el diseño aprobado internamente, RackCad no la sube
como artifact, no la guarda en Git ni en un feed y no la copia al bundle. La eliminacion del runner
corresponde al ciclo de vida de la infraestructura alojada; la falta de autorizacion contractual
expresa localizada para ese flujo permanece como riesgo residual aceptado por el Owner.

## 5. Material que no se conserva

### Estado tecnico observado

E1/E2 y la promocion declaran cero DLL Autodesk en `ReferenceCopyLocalPaths`, `bin`, bundle o
artifacts del job de Plugin. E2 uso un lock temporal y un cache de paquetes aislado.

### Restricciones aprobadas

- no versionar DLL Autodesk;
- no versionar archivos `.nupkg`;
- no publicar artifacts que contengan material Autodesk;
- no incluir material Autodesk en el bundle de RackCad;
- no usar feed privado;
- no usar `actions/cache` sin autorizacion especifica;
- no conservar la cache del runner entre jobs o runs; y
- no redistribuir assemblies Autodesk con RackCad.

Estas restricciones son obligatorias y simultaneas. Ademas, la aprobacion solo cubre RackCad, uso
interno, las tres versiones auditadas y compilacion; exige `Private=false`, `CopyLocal=false`, hashes
y origen fijados, rollback documentado y nueva revision ante cualquier version o escenario distinto
o documentacion Autodesk incompatible. No son una conclusion sobre lo que la licencia permite.

## 6. Guardas tecnicas

| Guarda | Funcion | Estado/evidencia |
|---|---|---|
| Versiones exactas | Evitar actualizaciones flotantes | 25.0.1/25.0.0/25.0.0 en promocion |
| Fuente unica | Limitar restore a nuget.org | Configuracion temporal fail-closed |
| Lock temporal/locked mode | Confirmar el grafo exacto | Permitido como metadata sin bytes Autodesk; diseño tecnico posterior requerido |
| SHA-256 y contentHash | Detectar cambio de bytes | Revalidados y coincidentes en P3 |
| Firma de autor | Aportar integridad y procedencia tecnica | CMS valida de Autodesk, Inc., revalidada en P3 |
| Firma de repositorio | Aportar integridad del canal | CMS valida de NuGet.org Repository by Microsoft, revalidada en P3 |
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

La decision B satisface el registro interno L2, pero la excepcion no esta vigente: requiere una
fase tecnica posterior autorizada y ADR-0003 aceptado por el dueño cuando corresponda. No crea una
categoria general de dependencias de build.

## 8. Casos de uso aprobados internamente

| Caso | Alcance aprobado | Controles | Resultado P4 |
|---|---|---|---|
| Descarga desde nuget.org | Obtener las tres versiones exactas durante el job | Fuente unica, versiones y grafo fijados | Aprobado con restricciones |
| GitHub-hosted runner | Procesar el material en una VM Windows alojada por GitHub | Runner sin AutoCAD, permisos minimos | Riesgo aceptado internamente; sin autorizacion Autodesk expresa localizada |
| Copia efimera | Mantener el paquete extraido solo durante el job | `NUGET_PACKAGES` aislado y destruccion al finalizar | Aprobada con restricciones |
| Compilacion interna | Usar assemblies como referencias compile-only | `Private=false`, `CopyLocal=false`, runtime excluido | Aprobada con restricciones |
| Hashes/contentHash | Conservar metadata de integridad sin conservar assemblies | Sin bytes del paquete en Git | Aprobado |
| `packages.lock.json` | Conservar metadata para locked mode | Sin bytes Autodesk; versiones y origen fijados | Aprobado condicionalmente |
| Distribuir RackCad sin material Autodesk | Uso interno del bundle limpio | Inspeccion recursiva y bundle limpio | Aprobado solo para uso interno |
| Uso interno | Usar el build dentro de Industrias Montilla | Mario Pérez como responsable | Aprobado con restricciones |
| Distribucion externa sin assemblies Autodesk | Fuera del alcance aprobado | No redistribuir DLL Autodesk | No autorizada por P4 |

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
la decision B no los incorpora.

## 10. Preguntas revisadas por el Owner

| Nº | Categoria | Pregunta | Evidencia disponible | Rol que debe responder | Estado |
|---:|---|---|---|---|---|
| 1 | Licencia/infraestructura | ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted? | Licencia ObjectARX heredada, E2 y documentacion GitHub | Owner | Riesgo aceptado internamente; sin afirmacion juridica |
| 2 | Licencia/caching | ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida? | Flujo E2 y licencia heredada | Owner | Tratada como uso interno aceptable solo bajo restricciones |
| 3 | Infraestructura | ¿Puede GitHub actuar como proveedor de infraestructura para ese uso? | Ciclo de vida de hosted runners | Owner | Riesgo aceptado para el flujo efimero aprobado |
| 4 | Caching | ¿Puede usarse `actions/cache`? | Documentacion oficial de caching | Owner | No autorizado |
| 5 | Integridad | ¿Puede conservarse `packages.lock.json`? | Lock temporal E2 | Owner | Permitido como metadata sin material Autodesk |
| 6 | Integridad | ¿Pueden conservarse `contentHash` y hashes? | Hashes revalidados en P3 | Owner | Permitido como evidencia de integridad |
| 7 | Material/licencia | ¿Es admisible que el paquete contenga assemblies de implementacion? | Composicion mixta revalidada | Owner | Aceptado solo para compile-only y sin redistribucion |
| 8 | Procedencia | ¿NuGet es un canal autorizado por Autodesk? | Owner, firmas y tutorial APS; enlace exacto incompleto | Owner | nuget.org aceptado internamente como fuente unica; incertidumbre residual |
| 9 | Procedencia | ¿`verified=false` requiere validacion adicional? | Prefijo no verificado; firmas/hashes revalidados | Owner | Controles P3 obligatorios y repetibles |
| 10 | Caching/redistribucion | ¿Puede usarse un feed privado? | Escenario no probado; copia persistente | Owner | No autorizado |
| 11 | Infraestructura | ¿Puede usarse un runner autohospedado? | Alternativa no probada | Owner | Fuera de alcance; requiere nueva revision |
| 12 | Obligaciones | ¿Hay obligaciones de avisos o atribucion? | Clausulas y avisos revisados | Owner | Conservar acuerdo y avisos en toda copia permitida |
| 13 | Distribucion | ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado? | Bundle limpio y `CopyLocal=false` | Owner | Solo uso interno aprobado; distribucion externa fuera de alcance |
| 14 | Uso interno/externo | ¿Difiere uso interno de distribucion externa? | Audiencias separadas en P3 | Owner | Si para P4: uso interno aprobado; externo no autorizado |
| 15 | Licencias AutoCAD | ¿Se requiere una licencia AutoCAD por cada entorno de build? | E2 no instalo ni ejecuto AutoCAD | Owner | Sin conclusion; runner aprobado no instala ni ejecuta AutoCAD |

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

## 13. Recomendacion preliminar historica y decision final

P3 recomienda preliminarmente **D. Requiere asesoria legal externa**, con confianza media. No se
marca formalmente esa opcion. El fundamento es que las fuentes primarias permiten copias para
desarrollo, pero no responden de forma suficientemente directa si un runner GitHub-hosted puede
recibir y procesar esas copias ni si nuget.org es el canal autorizado para las versiones exactas.
La composicion mixta eleva la cautela sobre el material principal, pero no invalida E1/E2 ni es por
si sola motivo de rechazo.

P3 razono que, mientras se obtenia esa respuesta, debian mantenerse las restricciones de las
secciones 5, 7, 8 y 9, y que B solo seria proporcional con todas las guardas. Descarto
provisionalmente A por las ambiguedades materiales y C porque no se encontro prohibicion expresa y
E1/E2 seguian validos. Este razonamiento se conserva como antecedente, no como decision vigente.

El Owner reviso esa recomendacion y el 2026-07-20 decidio **B. Aprobado con restricciones**. La
decision acepta internamente el riesgo residual de no haber localizado autorizacion expresa para
GitHub-hosted runners. Se limita a RackCad, uso interno, Mario Pérez como mantenedor, las versiones
auditadas y el flujo compile-only. Cualquier cambio de proyecto, audiencia, versiones, fuente,
runner, caching, artifacts, distribucion o documentacion aplicable exige nueva revision.

La [decision final](../../../automation/decisions/I-29.md) registra B y estos efectos:

| Tema | Efecto P4 |
|---|---|
| ADR-0003 | Permanece propuesto y sin modificar |
| Politica cero NuGet | Permanece vigente; la excepcion tecnica no esta activa |
| I-13 | Permanece abierta, bloqueada y sin modificar |
| Merge de promocion | Permanece bloqueado y no autorizado por P4 |

P4 selecciona B exclusivamente como decision interna de riesgo. No acepta ADR-0003, no activa la
excepcion cero NuGet, no cierra I-13 y no autoriza el merge.

## 14. Firmas y aprobaciones

| Rol | Nombre | Cargo | Organizacion | Autoridad | Fecha | Firma/mecanismo verificable |
|---|---|---|---|---|---|---|
| Owner | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Gestion interna de RackCad | 2026-07-20 | Instruccion escrita del Owner registrada en P4 |
| Technical preparer | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Preparacion tecnica de RackCad | 2026-07-20 | Registro versionado P1-P4 |
| Internal licensing reviewer | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Revision interna de riesgo; no juridica profesional | 2026-07-20 | Instruccion escrita del Owner registrada en P4 |
| Final approver | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Decision interna de gestion del riesgo de RackCad | 2026-07-20 | Instruccion escrita del Owner registrada en P4 |

La misma persona ocupa los cuatro roles y el Owner acepta esa concentracion para I-29. No existe
independencia entre preparer, reviewer y approver. La revision interna se limita a riesgo, licencia
y gobernanza del proyecto; no constituye asesoria legal profesional. La salida D sigue disponible
cuando se requiera criterio juridico externo.

Vigencia: desde 2026-07-20 mientras se mantengan simultaneamente alcance y restricciones.

Fecha de revision: ante cualquier cambio material y, como maximo, 2027-07-20.

Registro corporativo: documentos I-29 y commit P4 de la rama canonica.

Conflictos de interes declarados: no declarados en la instruccion P4.

Fuentes revisadas y version del paquete documental: matriz P3 y paquete P4 final.
