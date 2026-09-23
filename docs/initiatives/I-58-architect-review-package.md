# I-58 — Paquete Architect pre-implementation V1

Review solicitado: FOUNDATION EVOLUTION / revision adversarial obligatoria (lifecycle §5).
Review mode solicitado: SEPARATE SESSION; revisor=autor: NO (pendiente de asignacion real).
Este archivo prepara la revision; NO es un veredicto ni afirma que un Architect haya actuado.
Coordinator pre-review: PENDING; agrupacion/materialidad/EXP requieren su revision formal.

## Entrada completa y version

Identidad exacta de esta entrega, de todos los blobs y de la base: [evidencia](../automation/evidence/I-58-evidence.md).
Leer version completa de [Freeze draft](I-58-freeze-draft.md), [Discovery](I-58-discovery.md) y
[characterization](I-58-characterization.md); anexo de inventario y probe referidos desde evidencia.
Los acuerdos deben citar las tres identidades congelables, sin referencias mutables indirectas.
Fuentes integradas: ADR-0034/0044, I-57 V5, R3 y recibo integration/I-57; no se editan.
Delta vs I-57: pasar cuatro kinds de unsupported a autoridad tipada; Cama y Selective intactos.
Delta de esta entrega: SOLO documentos y probe diagnostico, no implementacion de ese comportamiento.
Findings de revisiones previas I-58: NONE (primera entrega); no heredar veredictos de I-57.

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

## Puntos que requieren respuesta expresa

1. Confirmar ownership AUTH-13 y FOUNDATION EVOLUTION; M-02/04/05/06 activos.
2. Coordinator: revisar DC-01..09 y EXP-01..09; autorizar/cerrar expansiones positivas; preguntar cual se omitio.
3. Aprobar/rechazar forms tipadas y carrier con raw evidence; revisar que no obliga a cambiar persistencia.
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

Coordinator = PENDING
Architect = PENDING
IMPLEMENTATION AUTHORIZATION = NO
