> **Archivo de auditoria; evidencia historica, no fuente operativa vigente.**
>
> - Naturaleza: evidencia historica de entrega y revision interna.
> - Fecha de corte: 2026-07-20.
> - Estado: archivado.
> - Decision final: B. Aprobado con restricciones.
> - Nota posterior: I-13 fue integrada y cerrada despues de estos documentos; ADR-0003 fue aceptado
>   posteriormente y la excepcion limitada a cero NuGet quedo vigente bajo su alcance.
> - Fuentes vigentes: [decision final](../../../automation/decisions/I-29.md) y
>   [contrato canonico](../../../initiatives/I-29-licencia-procedencia-autocad-ci.md).
>
> Las afirmaciones de este documento describen el estado de la entrega y revision P1-P4 al corte;
> no deben interpretarse como estado actual de I-13, ADR-0003 ni la promocion.

# I-29 — Registro de entrega y revision interna

## 1. Identificacion

| Campo | Valor |
|---|---|
| Iniciativa | I-29 — Licencia y procedencia de referencias AutoCAD para CI |
| Rama | `docs/licencia-procedencia-autocad-ci` |
| Claim-Id | `526b69aa-a56e-4da4-acd7-b96d0d8d1409` |
| Commit del paquete P1 entregado | `195cc8b26e58e191eeb4c3f5af8fa325ad43a77d` |
| Fecha de entrega interna | 2026-07-20 |
| Estado | P4 completada; iniciativa documental cerrada |
| Decision | B. Aprobado con restricciones |
| A/B/C/D | B seleccionada por el Owner |

## 2. Objeto de la entrega

Registrar que el paquete P1 fue puesto a disposicion del responsable del proyecto para iniciar una
revision interna de riesgo, licencia y gobernanza. La entrega no contiene una decision, no cambia la
politica cero NuGet y no autoriza el merge de I-13.

## 3. Documentos entregados

- [Contrato principal I-29](../../../initiatives/I-29-licencia-procedencia-autocad-ci.md).
- [Paquete de decision interna](I-29-paquete-decision-interna.md).
- [Decision final](../../../automation/decisions/I-29.md).

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
| Final approver | Mario Pérez | Coordinador de Desarrollo de Proyectos | Industrias Montilla | Identificado; decision B registrada |

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
[matriz maestra](I-29-matriz-evidencia-evaluacion.md). Cada respuesta se conserva como propuesta
historica de P3 y fue sustituida como decision de gestion por B en P4.

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

## 11. Resultado final P4

El 2026-07-20 Mario Pérez selecciono **B. Aprobado con restricciones** como decision interna de
gestion del riesgo para RackCad. La instruccion escrita del Owner, este registro y el commit P4 son
el mecanismo verificable. La recomendacion D de P3 se conserva como antecedente y fue sustituida por
B. No existe autorizacion expresa localizada para GitHub-hosted runners; ese punto permanece como
riesgo residual aceptado internamente. ADR-0003 permanece propuesto, la excepcion cero NuGet no esta
vigente, I-13 continua abierta y el merge sigue bloqueado.

## 12. Efectos y trabajo posterior

I-29 queda cerrada documentalmente. Una iniciativa o sesion posterior, con autorizacion expresa,
podra decidir ADR-0003, la excepcion cero NuGet, I-13 y la promocion. P4 no ejecuta esas acciones.

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

## 14. Registro de aprobacion interna

| Campo | Estado |
|---|---|
| Respuestas a las quince preguntas | Registradas en plantilla y hoja; incertidumbres aceptadas como riesgo interno |
| Fuentes finales revisadas | Matriz P3 y paquete P4 |
| Opcion A/B/C/D | B seleccionada |
| Restricciones | Catorce restricciones obligatorias y simultaneas registradas |
| Autoridad corporativa adicional | No proporcionada |
| Firma/mecanismo verificable | Instruccion escrita del Owner incorporada al registro y commit P4 |
| Fecha de decision | 2026-07-20 |
| Vigencia y revision | Vigente desde 2026-07-20; cambio material o maximo 2027-07-20 |
| Registro corporativo | Documentos I-29 y commit P4 de la rama canonica |
| Conflictos de interes | No declarado |
| Efecto final sobre ADR-0003 e I-13 | ADR propuesto; I-13 y merge bloqueados; sin cambios |
