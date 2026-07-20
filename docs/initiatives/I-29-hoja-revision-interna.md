# I-29 — Hoja de revision interna

## Identificacion de la revision

| Campo | Valor |
|---|---|
| Nombre | Mario Pérez |
| Cargo | Coordinador de Desarrollo de Proyectos |
| Organizacion | Industrias Montilla |
| Rol | Internal licensing reviewer y Final approver |
| Fecha de inicio | 2026-07-20 |
| Fecha de evaluacion P3 | 2026-07-20 |
| Estado | Evaluacion preliminar completada; decision formal pendiente |
| Recomendacion preliminar | D. Requiere asesoria legal externa; no seleccionada formalmente |
| Decision | Pendiente |

La revision concentra preparacion, revision y aprobacion en la misma persona. No es asesoria legal
profesional. La [matriz maestra](I-29-matriz-evidencia-evaluacion.md) contiene hechos, inferencias,
clasificaciones, confianza, fuentes y limites completos. Esta hoja no firma ni selecciona A/B/C/D.

En las quince respuestas rige la marca: **Propuesta preliminar; pendiente de decision formal de
Mario Pérez.**

## Pregunta 1

| Campo | Contenido |
|---|---|
| Número | 1 |
| Categoría | Licencia e infraestructura |
| Pregunta exacta | ¿La licencia permite descargar y usar estos assemblies en runners GitHub-hosted? |
| Evidencia revisada | E2, licencia ObjectARX, ciclo de vida GitHub-hosted y matriz §9.1. |
| Evaluación interna | Requiere asesoria externa; la licencia no nombra CI alojado ni terceros. |
| Riesgo identificado | Copia o procesamiento fuera del alcance contractual. |
| ¿Requiere asesoría externa? | Si. |
| Respuesta propuesta | No autorizar el merge hasta confirmacion escrita competente. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Restore efimero, compile-only, runner sin AutoCAD, sin persistencia ni redistribucion. |
| Fuente de la respuesta | Matriz §§4, 8 y 9.1; Autodesk ObjectARX; GitHub runners. |

## Pregunta 2

| Campo | Contenido |
|---|---|
| Número | 2 |
| Categoría | Licencia y caching |
| Pregunta exacta | ¿La copia efimera en `NUGET_PACKAGES` es una copia de desarrollo permitida? |
| Evidencia revisada | Cache aislado E2, licencia ObjectARX y matriz §9.2. |
| Evaluación interna | Finalidad alineada con desarrollo, modalidad no nombrada; requiere asesoria externa. |
| Riesgo identificado | Copia temporal no cubierta o sin avisos suficientes. |
| ¿Requiere asesoría externa? | Si. |
| Respuesta propuesta | Mantener bloqueada salvo confirmacion conjunta con la pregunta 1. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Fuera del checkout, acceso del job, destruccion con VM y auditoria de salidas. |
| Fuente de la respuesta | Matriz §§5, 8 y 9.2; licencia ObjectARX. |

## Pregunta 3

| Campo | Contenido |
|---|---|
| Número | 3 |
| Categoría | Infraestructura de terceros |
| Pregunta exacta | ¿Puede GitHub actuar como proveedor de infraestructura para ese uso? |
| Evidencia revisada | Documentacion GitHub-hosted, licencia ObjectARX y matriz §9.3. |
| Evaluación interna | No resuelta; proveedor, control y territorio no estan tratados. |
| Riesgo identificado | Omitir condiciones aplicables al tercero. |
| ¿Requiere asesoría externa? | Si. |
| Respuesta propuesta | No reconocer a GitHub como autorizado sin criterio escrito competente. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | VM efimera, permisos minimos, sin secrets ni persistencia Autodesk si se confirma. |
| Fuente de la respuesta | Matriz §§4 y 9.3; Autodesk ObjectARX; GitHub runners. |

## Pregunta 4

| Campo | Contenido |
|---|---|
| Número | 4 |
| Categoría | Caching persistente |
| Pregunta exacta | ¿Puede usarse `actions/cache`? |
| Evidencia revisada | Modelo oficial de caching, E2 sin cache y matriz §9.4. |
| Evaluación interna | Resuelta por politica interna: no solicitado y no autorizado. |
| Riesgo identificado | Retencion y audiencia ampliadas; poisoning. |
| ¿Requiere asesoría externa? | No para mantener la prohibicion. |
| Respuesta propuesta | No usar `actions/cache` con material Autodesk. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Cualquier solicitud futura requiere evaluacion separada. |
| Fuente de la respuesta | Matriz §§4 y 9.4; GitHub dependency caching; ADR propuesto solo como contexto. |

## Pregunta 5

| Campo | Contenido |
|---|---|
| Número | 5 |
| Categoría | Integridad y lock |
| Pregunta exacta | ¿Puede conservarse `packages.lock.json`? |
| Evidencia revisada | Lock temporal E2, formato oficial NuGet y matriz §9.5. |
| Evaluación interna | Respuesta condicionada; es metadata, pero el diseño permanente no esta aprobado. |
| Riesgo identificado | Gobernar mal los dos grafos o presentar metadata como permiso. |
| ¿Requiere asesoría externa? | No como decision tecnica; incluir si se exige certeza contractual completa. |
| Respuesta propuesta | Si, solo tras aprobar excepcion y diseño de lock permanente. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Sin bytes, revision de cambios, locked mode y ubicacion aprobada. |
| Fuente de la respuesta | Matriz §§4 y 9.5; Microsoft/NuGet PackageReference y lock. |

## Pregunta 6

| Campo | Contenido |
|---|---|
| Número | 6 |
| Categoría | Integridad |
| Pregunta exacta | ¿Pueden conservarse `contentHash` y hashes? |
| Evidencia revisada | Hashes revalidados, documentacion NuGet y matriz §9.6. |
| Evaluación interna | Resuelta por politica interna como evidencia textual, no autorizacion. |
| Riesgo identificado | Falsa confianza si se equiparan con licencia. |
| ¿Requiere asesoría externa? | No para el alcance interno propuesto. |
| Respuesta propuesta | Si, solo como evidencia de integridad. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Solo valores y contexto; ningun paquete, DLL o contenido codificado. |
| Fuente de la respuesta | Matriz §§5 y 9.6; Microsoft/NuGet. |

## Pregunta 7

| Campo | Contenido |
|---|---|
| Número | 7 |
| Categoría | Material y licencia |
| Pregunta exacta | ¿Es admisible que el paquete contenga assemblies de implementacion? |
| Evidencia revisada | Inventario 13, composicion 7/6, cuerpos en principales y matriz §§6-7, 9.7. |
| Evaluación interna | Requiere asesoria externa; la propiedad tecnica no decide permiso. |
| Riesgo identificado | Tratar como stubs material con cuerpos ejecutables. |
| ¿Requiere asesoría externa? | Si. |
| Respuesta propuesta | Tratar conservadoramente las seis DLL hasta criterio competente. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Nunca ejecutar, cachear persistentemente, publicar o redistribuir. |
| Fuente de la respuesta | Matriz §§5-9.7; ayuda Autodesk de componentes .NET. |

## Pregunta 8

| Campo | Contenido |
|---|---|
| Número | 8 |
| Categoría | Procedencia y canal |
| Pregunta exacta | ¿NuGet es un canal autorizado por Autodesk? |
| Evidencia revisada | Owner, firmas CMS, hashes, tutorial APS y paginas NuGet; matriz §9.8. |
| Evaluación interna | Procedencia tecnica robusta; autorizacion exacta del canal no resuelta. |
| Riesgo identificado | Confiar en evidencia editorial sin confirmacion primaria directa. |
| ¿Requiere asesoría externa? | Si, salvo confirmacion directa de Autodesk. |
| Respuesta propuesta | Solicitar confirmacion escrita antes del merge. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | nuget.org unico, versiones/hashes exactos, firmas obligatorias y fail-closed. |
| Fuente de la respuesta | Matriz §§4-5 y 9.8; Autodesk APS; NuGet oficial. |

## Pregunta 9

| Campo | Contenido |
|---|---|
| Número | 9 |
| Categoría | Procedencia |
| Pregunta exacta | ¿`verified=false` requiere validacion adicional? |
| Evidencia revisada | Definicion de prefijo reservado, firmas y hashes; matriz §9.9. |
| Evaluación interna | Resuelta tecnicamente: no implica falsedad; requiere controles compensatorios. |
| Riesgo identificado | Confundir el indicador o los controles con licencia. |
| ¿Requiere asesoría externa? | No para interpretar `verified`; la pregunta 8 sigue abierta. |
| Respuesta propuesta | Si requiere validacion adicional; P3 aplico firma, hashes, owner y licencia. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Repetir controles en cada version. |
| Fuente de la respuesta | Matriz §§4-5 y 9.9; Microsoft/NuGet prefix reservation. |

## Pregunta 10

| Campo | Contenido |
|---|---|
| Número | 10 |
| Categoría | Persistencia y redistribucion interna |
| Pregunta exacta | ¿Puede usarse un feed privado? |
| Evidencia revisada | Escenario no probado, licencia y matriz §9.10. |
| Evaluación interna | Resuelta por politica interna: fuera de alcance y no autorizado. |
| Riesgo identificado | Copia persistente y entrega interna sin controles suficientes. |
| ¿Requiere asesoría externa? | No para mantener la prohibicion. |
| Respuesta propuesta | No usar feed privado. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Evaluacion contractual, acceso, avisos y retencion separados para cualquier solicitud futura. |
| Fuente de la respuesta | Matriz §§8 y 9.10; paquete de decision. |

## Pregunta 11

| Campo | Contenido |
|---|---|
| Número | 11 |
| Categoría | Infraestructura alternativa |
| Pregunta exacta | ¿Puede usarse un runner autohospedado? |
| Evidencia revisada | Alternativa no probada, SDK/AutoCAD local y matriz §9.11. |
| Evaluación interna | Respuesta condicionada; requiere diseño, operacion y licenciamiento propios. |
| Riesgo identificado | Persistencia o asiento de producto incorrectos. |
| ¿Requiere asesoría externa? | Si si usa copias Autodesk o AutoCAD instalado. |
| Respuesta propuesta | No autorizar dentro de P3; evaluar por separado. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Inventario, usuarios, aislamiento, retencion, licencias y auditoria. |
| Fuente de la respuesta | Matriz §§4, 8 y 9.11; Autodesk ObjectARX y SDK. |

## Pregunta 12

| Campo | Contenido |
|---|---|
| Número | 12 |
| Categoría | Avisos y atribucion |
| Pregunta exacta | ¿Hay obligaciones de avisos o atribucion? |
| Evidencia revisada | `LICENSE.txt`, clausulas de copias y matriz §9.12. |
| Evaluación interna | Resuelta contractualmente para copias del Software: conservar acuerdo y avisos. |
| Riesgo identificado | Omitir avisos o inventar atribucion del producto final. |
| ¿Requiere asesoría externa? | No para la obligacion expresa; si cambia la distribucion. |
| Respuesta propuesta | Conservar acuerdo/avisos en toda copia Autodesk permitida; no añadir material al bundle. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Paquete intacto en cache efimero; redistribucion prohibida. |
| Fuente de la respuesta | Matriz §§4 y 9.12; licencia ObjectARX. |

## Pregunta 13

| Campo | Contenido |
|---|---|
| Número | 13 |
| Categoría | Distribucion de RackCad |
| Pregunta exacta | ¿RackCad puede distribuirse sin material Autodesk y depender de AutoCAD instalado? |
| Evidencia revisada | Bundle limpio, `Copy Local=False` y matriz §9.13. |
| Evaluación interna | Tecnicamente viable; autorizacion contractual de la aplicacion resultante no demostrada. |
| Riesgo identificado | Distribuir bajo un supuesto contractual incompleto. |
| ¿Requiere asesoría externa? | Si para confirmacion material. |
| Respuesta propuesta | Mantener modelo tecnico, sin presentar P3 como autorizacion de distribucion. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Cero material Autodesk, runtime documentado, inspeccion recursiva y AutoCAD licenciado. |
| Fuente de la respuesta | Matriz §§4, 7 y 9.13; ayuda Autodesk .NET. |

## Pregunta 14

| Campo | Contenido |
|---|---|
| Número | 14 |
| Categoría | Uso interno y externo |
| Pregunta exacta | ¿Difiere uso interno de distribucion externa? |
| Evidencia revisada | Audiencias, licencia ObjectARX y matriz §9.14. |
| Evaluación interna | No resuelta por fuentes primarias; deben separarse ambos escenarios. |
| Riesgo identificado | Extender una conclusion a la audiencia equivocada. |
| ¿Requiere asesoría externa? | Si. |
| Respuesta propuesta | No autorizar distribucion externa por analogia con uso interno. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Alcance, destinatarios, avisos y soporte documentados por separado. |
| Fuente de la respuesta | Matriz §§8 y 9.14; licencia ObjectARX. |

## Pregunta 15

| Campo | Contenido |
|---|---|
| Número | 15 |
| Categoría | Licencias AutoCAD |
| Pregunta exacta | ¿Se requiere una licencia AutoCAD por cada entorno de build? |
| Evidencia revisada | E2 sin AutoCAD, licencia ObjectARX y matriz §9.15. |
| Evaluación interna | No resuelta; el acuerdo ObjectARX no decide asientos de producto. |
| Riesgo identificado | Exigir costos sin base o incumplir una licencia aplicable. |
| ¿Requiere asesoría externa? | Si. |
| Respuesta propuesta | No concluir si o no; solicitar confirmacion expresa. Propuesta preliminar; pendiente de decision formal de Mario Pérez. |
| Restricciones propuestas | Runner sin AutoCAD, servicios ni credenciales de producto hasta resolver. |
| Fuente de la respuesta | Matriz §§4, 8 y 9.15; Autodesk ObjectARX y SDK. |

## Matriz de completitud

| Requisito | Estado |
|---|---|
| 15 preguntas evaluadas | Completo preliminar |
| Fuentes primarias identificadas | Completo preliminar |
| Revalidacion 13/7/6 y principales | Completa |
| Caching | `actions/cache` no autorizado; copia efimera pendiente de asesoria |
| Artifacts | Material Autodesk prohibido; RackCad solo con guarda de ausencia total |
| Redistribución | Material Autodesk prohibido; RackCad condicionado y pendiente |
| Uso interno/externo | Separado; respuesta contractual pendiente |
| Obligaciones de atribución | Acuerdo y avisos expresos para copias Autodesk |
| Necesidad de asesoría externa | Determinada: recomendacion preliminar D |
| A/B/C/D seleccionado | Pendiente; ninguna casilla marcada |
| Decisión firmada | Pendiente; no existe firma |
| Fecha de aprobacion y vigencia | Pendientes |
| ADR-0003 | Propuesto |
| Excepcion cero NuGet | No vigente |
| I-13 y merge | Abiertos y bloqueados |

## Recomendacion preliminar

P3 recomienda **D. Requiere asesoria legal externa**, confianza media, sin seleccion formal. Las
preguntas materiales 1, 2, 3, 7, 8, 13, 14 y 15 carecen de respuesta primaria suficientemente
directa; la 11 tambien requiere revision si se adopta esa alternativa. A es insuficiente por esas
brechas; B puede ser una salida futura si se resuelven y se mantienen todas las restricciones; C
no se sostiene porque no se encontro prohibicion expresa y E1/E2 permanecen tecnicamente validos.

P4 permanece bloqueada. Mario Pérez debe revisar la matriz, decidir si solicita criterio externo y,
solo despues, seleccionar A/B/C/D y registrar autoridad, firma, fecha, vigencia y efectos de
gobernanza.
