# Entrada factual propuesta para el cierre I-58

Draft para conformidad, NO publicado en docs/FOUNDATIONS.md. Lifecycle §4.1 / WORKFLOW §11.4.
Su publicacion corresponde al cierre, despues de conformidad; no afirma que I-58 ya este integrada.

Name: Shared View Foundation — AUTH-13 authored comparators

Status: extension implementada en I-58; integracion pendiente.

Authority: `RackAuthoredComparatorPorts` conserva la autoridad neutral AUTH-13. Selective delega a
`SelectiveAuthoredAuthority.Resolve`; Dynamic, PushBack, Cantilever y Cabecera disponen de puertos
concretos que acreditan todas las hermanas raw antes de comparar y materializar. Cama y factories
genericas arbitrarias permanecen unsupported/fail-closed.

Persistence: no cambia formatos, DTOs, serializers ni stores. `RackAuthoredInput` es un carrier runtime
con pertenencia exterior, lista completa, source, raw original del envelope/Design y evidencia de
completitud. El reader local rechaza forma, schemas y unknowns no acreditables antes de cualquier mapper.

Public outputs: `Single`, `Divergent`, `Unreadable`; failures llevan Authored=null. Nuevos Singles de
dominio: DynamicRackDesign, PushBackDesign, CantileverLineDesign y RackFrameConfiguration respectivamente.
Selective conserva su contrato historico `SelectivePalletDesignDocument`; I-58 no lo migra.

Mutation contract: igualdad tipada sin resolver/catalogo; cada nuevo Single es profundo e independiente.
AUTH-09 recibe una copia de trabajo del authored y delega una vez a la autoridad vigente; AUTH-10 prepara
desde el effective tipado sin re-resolver. El effective nunca sustituye la autoridad authored. La composicion
esta demostrada por integracion pura; I-58 no agrega consumidores visibles ni escritura AutoCAD.

Extension point: consumir los puertos concretos y las seams existentes AUTH-09/10. El consumer acredita
la captura completa, conserva metadata del envelope y aplica sus gates propios de persistencia y policy.
No existe un registry/conversor universal ni permiso para reinterpretar DTOs o duplicar igualdad.

Decision source: Consensus Freeze V3 I-58 F-01..13 y Characterization V3, sobre AUTH-13 de I-57/ADR-0044.

Protecting tests: I58F1CharacterizationTests, I58F1OracleChecks, I58F2ReaderTests, I58F2BoundaryTests,
I58F2MaterializationTests, I58F3SeamTests, SharedViewFoundationF5Tests/F6Tests y SelectiveAuthored*.

Known limitations: global PostPeralte=0 conserva y compara cada Header persistible completo, por orden,
modulo y provenance; puede producir Divergent conservador aunque el peralte resuelto hoy coincida.
CustomProperties conserva autoridad separada. Cantilever distingue membership exterior de Line.Id interior;
no corrige D58-CANT-ID/I-37. Cama no se acredita. Captura de AutoCAD, remedios de divergence y consumo
I-55 G12 no se implementan ni desbloquean aqui. AUTH-15 y RACKMIRROR quedan fuera.

Last changed by: I-58 (publicar solo en el cierre autorizado).
