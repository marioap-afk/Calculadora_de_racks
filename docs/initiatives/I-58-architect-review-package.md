# I-58 — Paquete Architect pre-implementation V2

Review solicitado: FOUNDATION EVOLUTION / revision adversarial obligatoria (lifecycle §5).
Review mode solicitado: SEPARATE SESSION; revisor=autor: NO (pendiente de asignacion real).
Este archivo prepara la revision; NO es un veredicto ni afirma que un Architect haya actuado.
Coordinator pre-review: CHANGES REQUIRED ON V1 / REVIEW REQUIRED ON V2; agrupacion/materialidad/EXP requieren su revision formal.

## Entrada completa y version

Identidad exacta de esta entrega, de todos los blobs y de la base: [evidencia](../automation/evidence/I-58-evidence.md).
Leer version completa de [Freeze draft](I-58-freeze-draft.md), [Discovery](I-58-discovery.md) y
[characterization](I-58-characterization.md); anexo de inventario y probe referidos desde evidencia.
Los acuerdos deben citar las tres identidades congelables, sin referencias mutables indirectas.
Fuentes integradas: ADR-0034/0044, I-57 V5, R3 y recibo integration/I-57; no se editan.
Delta vs I-57: pasar cuatro kinds de unsupported a autoridad tipada; Cama y Selective intactos.
Delta de esta entrega: SOLO documentos y probe diagnostico, no implementacion de ese comportamiento.
Findings previos: CR58-01/02 REQUIRED de C1 comunicado por Owner, ambos CORRECTION PROPOSED,
pendientes de disposicion de su emisor. Ver tabla individual en Discovery; no heredar veredictos I-57.
Delta V2: retirar outer==inner como precondicion propuesta; EXP-01 A/STOP con propuesta B pendiente;
separar canonical interno de cuatro TAuthored dominio; probar salida y seam exacta, no solo equality.
Este paquete NO solicita ni emite un veredicto en nombre del Architect.

## Ataques requeridos

| ID | Ataque minimo | Artefacto/obligacion que debe resistir |
|---|---|---|
| AR58-01 | ToDesign/ToDomain o store introducen effective/normalizacion antes de acreditar authored | Discovery DC-03; F-03/04/08/09; CT58-03, MM-D/P/H |
| AR58-02 | campo conocido no copiado o nested listo vacio deja Single falso | inventario, F-07; mutar CADA rama y nullables, no solo root |
| AR58-03 | 1.1/2.99 sin unknown visible pasa IsReadable | F-10; CT58-07 a cada nivel, sin editar SchemaVersionPolicy |
| AR58-04 | ExtensionData root se descarta como metadata | F-11; CT58-15 envelope/wrapper/payload |
| AR58-05 | unknown nested se pierde antes del carrier final | F-03/11; CT58-16 todo cierre; probar original raw intacto |
| AR58-06 | equality JSON/reflection disfraza falta de semantica | F-06/07; prueba conductual ademas de guardias; no hashes de JSON |
| AR58-07 | primer sibling/majority/early Divergent elige autoridad | F-05/12; permutar A/B/X y comprobar null y output reconstruido |
| AR58-08 | Foundation decide remedio, vista, batch, Insertar o RACKMIRROR | F-01/02/09; comparar con seam I-55 solo lectura |
| AR58-09 | se cambia persisted schema/DTO/store para facilitar unknown scan | F-02/10/11; scope guard de diff y fixtures schema byte-preservation |
| AR58-10 | API generic<TInput,TAuthored> parece soportar todo pero ignora entrada | F-02; overload concreto propuesto, incompatibles siguen fail-closed |
| AR58-11 | retorno Single comparte instancia o sintetiza Guid/default inestable | F-12; CT58-19/21, Cantilever.Id |
| AR58-12 | exclusiones Bfr/IntervalCount/margenes retirados ocultan authored real | F-08; comprobar writer/read path exacto y contraprueba de unknown vecino |
| AR58-13 | normalizar nullable/listas hace equivalentes dos intenciones diferentes | F-09; especialmente SideB holes, inactive manual values, trailing null vs index |
| AR58-14 | preparacion consumidor recompone desde source no acreditado | DC-05; carrier completeness y misma lectura; sin implementar producto aqui |
| AR58-15 | prohibir future/unknown dentro de lectura de autoridad se vende como cambio global de compatibilidad | DC-09 M-02/04; mantener stores existentes, exponer fallo solo por port nuevo |
| AR58-16 | outer RackId vs Line.Id: rechazar A/B/C o unir dos racks biblioteca por inner | DC-02..06 C1, F-03, CT58-23/24; atacar suficiencia de evidencia reconstruida para B, conservar A ante duda |
| AR58-17 | documentos existentes rechazados por una nueva precondicion ajena al soporte pedido | M-03/08, EXP-01/08; no reparar identidad ni reescribir I-37 para ocultar contradiccion |
| AR58-18 | canonical DTO escapa como TAuthored publico y obliga I-55 a reconvertir | F-06/12/13, CT58-25: cuatro tipos de dominio exactos, no Marker |
| AR58-19 | AUTH13->AUTH09 implica segundo resolve o resolver universal | F-13/CT58-25: autoridad real una vez; fallos cero, AUTH10 sin re-resolve |
| AR58-20 | canonicals iguales producen Single con dominio distinto por mapper con perdida | CT58-26/28, inspeccionar TODOS los valores retornados y aislamiento; no basta outcome o DTO |
| AR58-21 | ToDomain/ToDesign borra authored antes o despues del gate | F-06/09, CT58-26/27: raw intacto, materializacion fiel o fallo/finding, no default silencioso |

## Puntos que requieren respuesta expresa

1. Confirmar ownership AUTH-13 y FOUNDATION EVOLUTION; M-02/04/05/06/08 activos; revisar M-03 con A/B/C y limite V2.
2. Coordinator: revisar DC-01..09 y EXP-01..09; autorizar/cerrar expansiones positivas; preguntar cual se omitio.
3. Aprobar/rechazar separacion de forms internas y autoridad publica tipada y carrier con raw evidence; revisar que no obliga a cambiar persistencia.
4. Resolver si todas las normalizaciones F-09 conservan intencion; cualquier ambiguedad material es REQUIRED,
   no un detalle que el implementador pueda completar en silencio. La primera suite F1 es prueba de ese contrato.
5. Confirmar todas las exclusiones F-08, incluyendo retired margins y getter IntervalCount, con evidencia.
6. Revisar viabilidad del plan de gates y asignacion OV; no exigir UI ficticia para comparator no cableado;
   tampoco retirar OV que active el diff final. I-55 OV-ID18 no se acredita por pruebas de I-58.
7. Confirmar que ningun PASS del probe Unreadable se considera prueba de reader seguro; exigir futuros RED
   por desactivar schema/unknown validators, con conteo >0 y sin source guards como sustituto.

## Formato de salida esperado (PROMPT_TEMPLATES C por referencia)

Declarar modo real y revisor=autor; version completa, commit/ruta/blob y anexo.
AGREED POINTS; DISAGREEMENTS; MATERIAL RISKS; REQUIRED CHANGES; OPTIONAL IMPROVEMENTS;
CONSENSUS STATUS ligado a identidad exacta. Cada hallazgo con ID estable, REQUIRED/OPTIONAL,
clausula y evidencia; disposicion de hallazgos solo por su autor conforme lifecycle §5.
No aprobar con REQUIRED abiertos. Una version revisada se entrega completa con delta y disposiciones.

Coordinator = CHANGES REQUIRED ON V1 / REVIEW REQUIRED ON V2
Architect = PENDING
F1 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO

EXP-01: no aceptar B por ausencia de edits en I-37. Exigir evidencia DC-02..06 de irrelevancia SOLO para
comparar authored con pertenencia exterior ya acreditada. La deuda subsiste para otras rutas y la
confirmacion requiere Coordinator + Architect. Ningun probe autoriza resolver C1 en nombre del emisor.
