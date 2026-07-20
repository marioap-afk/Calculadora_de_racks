# I-29 — Plantilla de decision interna

Formulario neutral para registrar la decision sobre referencias AutoCAD de compilacion en CI. No
debe firmarse hasta revisar el [paquete de decision](I-29-paquete-decision-interna.md) y responder
las quince preguntas. Marcar exactamente una opcion.

## Identificacion

| Campo | Valor |
|---|---|
| Iniciativa | I-29 — Licencia y procedencia de referencias AutoCAD para CI |
| Fecha |  |
| Version del paquete documental |  |
| Fuentes revisadas |  |
| Owner |  |
| Technical preparer |  |
| Legal/licensing reviewer |  |
| Final approver |  |
| Registro corporativo |  |
| Mecanismo verificable de aprobacion |  |

## Salida seleccionada

- [ ] A. Aprobado.
- [ ] B. Aprobado con restricciones.
- [ ] C. Rechazado.
- [ ] D. Requiere asesoria legal externa.

## Campos obligatorios para A o B

| Campo | Respuesta |
|---|---|
| Alcance |  |
| Restricciones |  |
| Paquetes y versiones cubiertas |  |
| GitHub-hosted runners |  |
| Copia efimera |  |
| Obtencion desde nuget.org |  |
| `NUGET_PACKAGES` |  |
| `actions/cache` |  |
| `packages.lock.json` |  |
| Hashes/contentHash |  |
| Artifacts |  |
| Feeds privados |  |
| Uso interno |  |
| Uso externo |  |
| Distribucion de RackCad |  |
| Redistribucion de material Autodesk |  |
| Avisos y atribuciones |  |
| Evidencia que debe conservarse |  |
| Vigencia |  |
| Fecha de revision anual |  |
| Responsable de cumplimiento |  |
| Condiciones de revocacion |  |
| Rollback |  |
| Efecto sobre ADR-0003 |  |
| Efecto sobre I-13 |  |
| Autorizacion o bloqueo del merge |  |

Para B, cada restriccion debe ser verificable y debe identificar responsable, fecha y consecuencia
del incumplimiento. Para A, los campos siguen siendo obligatorios: una aprobacion sin alcance no es
suficiente.

## Campos obligatorios para C

| Campo | Respuesta |
|---|---|
| Causa |  |
| Fuente contractual o politica |  |
| Alcance del rechazo |  |
| Rollback requerido |  |
| Alternativa tecnica requerida |  |
| Efecto sobre I-13 |  |
| Responsable de ejecutar el rollback |  |
| Fecha objetivo |  |

## Campos obligatorios para D

| Campo | Respuesta |
|---|---|
| Tipo de asesor requerido |  |
| Documentacion que se entregara |  |
| Responsable de coordinacion |  |
| Fecha objetivo |  |
| Registro previsto para la respuesta |  |
| Confirmacion de que ADR-0003 continua propuesto |  |
| Confirmacion de que I-13 y su merge continúan bloqueados |  |

Preguntas exactas a remitir; marcar solo las que formaran parte de la consulta y copiarlas sin
reformular en el registro enviado:

- [ ] ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted?
- [ ] ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida?
- [ ] ¿Puede GitHub actuar como proveedor de infraestructura para ese uso?
- [ ] ¿Puede usarse `actions/cache`?
- [ ] ¿Puede conservarse `packages.lock.json`?
- [ ] ¿Pueden conservarse `contentHash` y hashes?
- [ ] ¿Es admisible que el paquete contenga assemblies de implementacion?
- [ ] ¿NuGet es un canal autorizado por Autodesk?
- [ ] ¿`verified=false` requiere validacion adicional?
- [ ] ¿Puede usarse un feed privado?
- [ ] ¿Puede usarse un runner autohospedado?
- [ ] ¿Hay obligaciones de avisos o atribucion?
- [ ] ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado?
- [ ] ¿Difiere uso interno de distribucion externa?
- [ ] ¿Se requiere una licencia AutoCAD por cada entorno de build?

## Matriz de respuestas a las quince preguntas

Esta matriz debe completarse para A, B o C. En D puede registrar que la respuesta se difiere a la
consulta externa, junto con la referencia exacta al expediente enviado.

| Nº | Respuesta atribuida | Fuente | Responsable | Restricciones o acciones |
|---:|---|---|---|---|
| 1 |  |  |  |  |
| 2 |  |  |  |  |
| 3 |  |  |  |  |
| 4 |  |  |  |  |
| 5 |  |  |  |  |
| 6 |  |  |  |  |
| 7 |  |  |  |  |
| 8 |  |  |  |  |
| 9 |  |  |  |  |
| 10 |  |  |  |  |
| 11 |  |  |  |  |
| 12 |  |  |  |  |
| 13 |  |  |  |  |
| 14 |  |  |  |  |
| 15 |  |  |  |  |

## Declaraciones de control

- [ ] La persona aprobadora esta identificada y su autoridad fue comprobada.
- [ ] Las fuentes revisadas y su fecha quedaron registradas.
- [ ] La decision distingue evidencia tecnica, integridad, procedencia, politica interna y
  autorizacion legal/contractual.
- [ ] El alcance interno/externo, caching, artifacts, distribucion y redistribucion quedaron
  respondidos expresamente.
- [ ] Los efectos sobre ADR-0003, cero NuGet, I-13 y el merge quedaron definidos.
- [ ] La vigencia, revision, cumplimiento, revocacion y rollback quedaron definidos.

## Firmas

### Owner

Nombre: ____________________  Cargo: ____________________

Organizacion: ____________________  Alcance de autoridad: ____________________

Fecha: ____________________  Firma o mecanismo verificable: ____________________

### Technical preparer

Nombre: ____________________  Cargo: ____________________

Organizacion: ____________________  Declaracion de completitud: ____________________

Fecha: ____________________  Firma o mecanismo verificable: ____________________

### Legal/licensing reviewer

Nombre: ____________________  Cargo: ____________________

Organizacion: ____________________  Competencia/autoridad: ____________________

Fecha: ____________________  Firma o mecanismo verificable: ____________________

### Final approver

Nombre: ____________________  Cargo: ____________________

Organizacion: ____________________  Alcance de autoridad: ____________________

Fecha: ____________________  Firma o mecanismo verificable: ____________________

Vigencia: ____________________  Proxima revision: ____________________

Registro corporativo: ____________________

Conflictos de interes: ____________________
