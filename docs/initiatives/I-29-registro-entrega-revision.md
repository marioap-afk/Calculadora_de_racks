# I-29 — Registro de entrega y revision interna

## 1. Identificacion

| Campo | Valor |
|---|---|
| Iniciativa | I-29 — Licencia y procedencia de referencias AutoCAD para CI |
| Rama | `docs/licencia-procedencia-autocad-ci` |
| Claim-Id | `526b69aa-a56e-4da4-acd7-b96d0d8d1409` |
| Commit del paquete P1 entregado | `195cc8b26e58e191eeb4c3f5af8fa325ad43a77d` |
| Fecha de entrega interna | 2026-07-20 |
| Estado | Revision preliminar P3 completada; decision formal pendiente |
| Decision | Pendiente |
| A/B/C/D | Sin seleccionar |

## 2. Objeto de la entrega

Registrar que el paquete P1 fue puesto a disposicion del responsable del proyecto para iniciar una
revision interna de riesgo, licencia y gobernanza. La entrega no contiene una decision, no cambia la
politica cero NuGet y no autoriza el merge de I-13.

## 3. Documentos entregados

- [Contrato principal I-29](I-29-licencia-procedencia-autocad-ci.md).
- [Paquete de decision interna](I-29-paquete-decision-interna.md).
- [Plantilla de decision](I-29-plantilla-decision.md).

## 4. Identificacion del receptor

| Campo | Valor |
|---|---|
| Nombre | Mario Pérez |
| Cargo | Coordinador de Desarrollo de Proyectos |
| Organizacion | Industrias Montilla |
| Funcion de recepcion | Owner e internal licensing reviewer |

## 5. Roles acumulados

| Rol | Persona | Cargo | Organización | Estado |
|---|---|---|---|---|
| Owner | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Identificado |
| Technical preparer | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Identificado |
| Internal licensing reviewer | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Identificado |
| Final approver | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Identificado; decisión pendiente |

## 6. Declaracion sobre concentracion de funciones

La misma persona ocupa los roles de Owner, technical preparer, internal licensing reviewer y final
approver. Esta concentracion reduce la independencia de la revision y debe considerarse al evaluar
la suficiencia de la futura decision. El Owner acepta esta concentracion para I-29.

La revision interna no constituye asesoria legal profesional. Cuando una pregunta requiera
interpretacion juridica especializada, la salida D permanece disponible. Ninguna decision interna
sustituye asesoria legal profesional cuando esta sea necesaria.

## 7. Alcance de la revision interna

La revision interna puede:

- examinar la evidencia tecnica, de integridad y procedencia presentada;
- evaluar riesgos operativos y de gobernanza para RackCad;
- identificar fuentes que deban revalidarse;
- proponer respuestas, restricciones, controles y rollback; y
- determinar si alguna pregunta requiere asesoria externa.

La revision debe mantener separadas la viabilidad tecnica, integridad, procedencia, politica interna
y autorizacion legal o contractual.

## 8. Limitaciones de la revision

- Mario Pérez no se presenta como abogado, asesor legal ni profesional con competencia legal
  certificada.
- No existe independencia entre preparer, reviewer y approver.
- La recepcion del paquete no constituye aprobacion, firma ni decision suficiente.
- No se ha proporcionado firma, fecha de firma, registro corporativo ni autoridad delegada adicional.
- No se ha declarado la existencia o ausencia de conflictos de interes.
- La revision no puede convertir firmas, hashes, owners o builds verdes en autorizacion legal.

## 9. Estado de las quince preguntas

Las quince preguntas exactas cuentan con evaluacion y respuesta interna propuesta en la
[hoja de revision interna](I-29-hoja-revision-interna.md) y trazabilidad ampliada en la
[matriz maestra](I-29-matriz-evidencia-evaluacion.md). Cada respuesta es preliminar y permanece
pendiente de decision formal de Mario Pérez.

## 10. Evidencia revalidada

P3 revalido independientemente el 2026-07-20:

- licencia incluida en los paquetes;
- nuspec y metadata declarada;
- `verified=false` observado por I-13;
- SHA-256, SHA-512 y contentHash;
- firma de autor; y
- firma de repositorio.

La composicion observada es mixta: siete assemblies con `ReferenceAssemblyAttribute` y seis sin el
atributo. Las tres referencias principales no contienen el atributo y contienen cuerpos. CI #54/#55
continua registrado como declarado, no verificado independientemente por P1/P2/P3.

## 11. Resultado pendiente

No se selecciono A, B, C o D. No existe decision firmada ni declaracion de suficiencia. ADR-0003
permanece propuesto, la excepcion cero NuGet no esta vigente e I-13 continua bloqueada.

## 12. Proximo paso P4

P4 permanece bloqueada hasta que Mario Pérez revise las propuestas, seleccione formalmente A/B/C/D,
registre firma, fecha, autoridad, vigencia y efecto sobre ADR-0003, cero NuGet, I-13 y el merge. P3
no ejecuta ni autoriza esas acciones.

## 13. Registro de recepcion

Mecanismo de entrega:

> Entrega interna mediante publicación versionada del paquete documental en la rama canónica de
> I-29, identificada por commit y push remoto.

| Campo | Valor |
|---|---|
| Receptor | Mario Pérez |
| Cargo | Coordinador de Desarrollo de Proyectos |
| Organizacion | Industrias Montilla |
| Fecha de recepcion | 2026-07-20 |
| Estado de recepcion | Recibido para revision interna |
| Evidencia | Este documento, commit de P2 y push de la rama canonica |

Este registro no afirma envio por correo, Teams, carta ni entrega de un documento firmado.

## 14. Campos pendientes de aprobacion

| Campo | Estado |
|---|---|
| Respuestas a las quince preguntas | Pendiente |
| Fuentes finales revisadas | Pendiente |
| Opcion A/B/C/D | Sin seleccionar |
| Restricciones | Pendiente |
| Autoridad corporativa adicional | No proporcionada |
| Firma | No proporcionada |
| Fecha de firma | No proporcionada |
| Vigencia y revision | Pendiente |
| Registro corporativo | No proporcionado |
| Conflictos de interes | No declarado |
| Efecto final sobre ADR-0003 e I-13 | Pendiente |
