# I-62 — Disposición del Owner y del Coordinator sobre la revisión formal del Architect de la Proposal V13 (registro)

```text
Emisores:         Owner y Coordinator exclusivo de I-62 (disposición de la revisión formal y orden de la corrección final V14)
Naturaleza:       disposición de una revisión y orden de corrección; no es dictamen del Architect, Freeze, decisión de OD-6 ni autorización de implementación
Fecha:            2026-10-02 (UTC)
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — PROPOSAL V14 / BOUNDED NORMATIVE DEPENDENCY CLOSURE»);
                  sin archivo de origen. Procedencia: transcripción literal recibida (9 425 bytes en UTF-8, SHA-256
                  346f14a0b48b4f9eb07fda1c254068605e74a2f3292780b6d0c76e326c6f7130), custodiada fuera del repositorio con la transcripción de la sesión
Objeto:           revisión formal final del Architect de la Proposal V13 (commit b337a59fa6879893229096f6e423d441b3c97bc6, blob
                  949c6403a04570dfa7b5dcd1a3509271819dc696); registro I-62-architect-review-v13.md; custodia en f51eda39d56a498a4ec56b4604acbe3a9be25746
Disposición:      A62-V11-01 = ACCEPTED REQUIRED, residuo estrecho; todo lo demás CLOSED o SUPERSEDED sigue igual; la revisión de V13 sigue siendo VÁLIDA;
                  Proposal V14 autorizada como corrección final
Estado:           Frozen: NO · OD-6 PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 vigente hasta que I-62 se integre
```

Registro redactado por la sesión responsable por orden del Owner y del Coordinator. La respuesta está en la [Proposal V14](I-62-proposal-v14.md) y en el
[paquete V14](I-62-architect-package-v14.md) §§3-4.

## 1. Disposición (resumen fiel)

- **A62-V11-01 = ACCEPTED REQUIRED**, residuo estrecho.
- Todo lo demás CLOSED o SUPERSEDED sigue igual. No se reabre ninguna otra parte de la arquitectura.
- **La revisión de V13 sigue siendo válida.** Sus clausuras de dependencias calculadas mecánicamente que terminaron en UNKNOWN son **evidencia del defecto
  residual**, no un motivo para invalidar la revisión entera.
- **No se fabrica de forma retroactiva** una clausura completa para la revisión de V13. Se registra que:
  - el hallazgo directo del Architect sigue aceptado;
  - la clausura mecánica de V13 es UNKNOWN bajo la regla sustituida;
  - V14 cambia el diseño en adelante.

## 2. Dirección exigida (resumen fiel)

| Id | Dirección |
|---|---|
| Problema | (1) Una dependencia de un documento entero puede tratarse como terminal aunque contenga otra dependencia de control. (2) Quitar esa regla sin más expande el grafo sin control: miles de líneas, muchos documentos y referencias ambiguas o sin resolver. (3) La expansión heurística de referencias en texto libre no es un mecanismo de autoridad suficientemente determinista. V14 debe definir un grafo de dependencias normativas **acotado y canónico** |
| Unidades normativas canónicas | definir `NormativeUnitRef {Document, UnitId, Anchor, Revision}`: la unidad canónica más pequeña que lleva el significado de control completo (párrafo; elemento con su padre; fila con sus cabeceras; invariante; enumeración o definición; bloque de algoritmo o de máquina de estados; cláusula de autoridad o precedencia). No identificar unidades solo por subcadenas. Los nombres finales pueden variar |
| Resolución canónica | toda arista usada para acreditar se resuelve de forma determinista a una o más `NormativeUnitRef`, con orden explícito: (1) referencia calificada → unidad exacta; (2) sección del mismo documento → solo ese documento; (3) sección propuesta mapeada → unidad de diseño en la Proposal o mapa de materialización; (4) identificador definido → su unidad de definición; (5) referencia sin calificador con más de un destino → AMBIGUOUS_REFERENCE → UNKNOWN; (6) referencia inexistente → UNRESOLVED_REFERENCE → UNKNOWN. Sin conjetura semántica |
| Secciones propuestas | referencias como AUTOMATION_PLAN §16.13 deben resolverse durante la revisión del diseño mediante el mapa de materialización congelado de la Proposal (`ProposedNormativeTarget {FutureDocument, FutureAnchor, DesignSource}`); tras materializar, rige el documento y el ancla reales. Que una sección aún no exista en `main` no la hace colgante si la Proposal define su destino |
| Documentos enteros | no son terminales de forma automática, ni se expanden por defecto. Son acreditables solo con (A) un conjunto de entrada acotado y determinista, (B) una regla compuesta exacta de una autoridad vigente, o, si no, (C) WHOLE_DOCUMENT_UNBOUNDED → independencia UNKNOWN → hallazgo no acreditado. No resolver C leyendo todas las frases normativas del documento |
| Aristas de dependencia | seguir solo las dependencias que pueden cambiar el significado o la aplicabilidad de la unidad seleccionada («solo si §X», «salvo lo previsto en Y», definiciones o enumeraciones usadas de forma normativa, precedencia, transiciones que dependen de un invariante). No seguir enlaces informativos, citas históricas, ejemplos, referencias de procedencia ni referencias sin relación de la misma sección o documento |
| Manifiesto o índice | no hacer que el tiempo de ejecución infiera las aristas del lenguaje natural. Diseñar una representación determinista y pequeña del grafo, preferiblemente un artefacto de Nivel A (manifiesto o índice versionado, generado o revisado con la Proposal o la materialización): `NormativeDependencyEntry {Source, DependsOn[]}`. Solo las unidades que necesitan el protocolo y las superficies del Freeze; sin ontología global; acotado a las superficies normativas de I-62 y a sus dependencias de compatibilidad. La representación exacta la diseña V14 |
| Clausura del grafo | (1) unidades de control directas del envoltorio; (2) cola; (3) solo aristas declaradas o resueltas; (4) visitados; (5) parar en nodos sin dependencias de control; (6) los ciclos terminan por los visitados; (7) los duplicados se funden por identidad canónica. Sin límite de profundidad si el grafo finito es canónico, pero el grafo debe estar acotado y ser determinista |
| Nodo terminal | solo con cero dependencias de control sin resolver; no por ser documento entero, de autoridad, externo o grande. Metadatos incompletos → UNKNOWN |
| Fidelidad | un hallazgo se acredita solo si sus envoltorios directos son fieles, toda `NormativeUnitRef` de la clausura fue visible fielmente para el revisor, toda dependencia de control se resolvió de forma determinista y ningún nodo requerido cruza un tramo degradado o no visto. Una unidad requerida que el revisor no vio → INVALID_PREMISE o UNKNOWN según la semántica existente. «Presente en events.jsonl» no equivale a visible |
| Principio de acotación | el objetivo no es «leer todo lo alcanzable desde cada referencia textual», sino «probar la fidelidad del conjunto finito de unidades normativas canónicas que controlan de verdad el hallazgo». Invariante crítico, con una guarda explícita: un enlace o una referencia sin relación no amplía la clausura |
| C-42 | casos mínimos: (1) A → documento B → B1 → C degradada → INVALID_PREMISE; (2) documento B con conjunto de entrada {B1}, B2 sin relación y degradada → B2 excluida, válido si A y las dependencias de B1 son fieles; (3) documento B sin conjunto de entrada ni regla compuesta → UNKNOWN, sin expandirlo; (4) A → §16.13 propuesta → unidad de diseño exacta; (5) «§16.3» sin calificador con dos documentos candidatos → AMBIGUOUS_REFERENCE / UNKNOWN; (6) «§16.3» del mismo documento → se resuelve localmente; (7) ciclo A → B → C → clausura finita; (8) cita histórica o enlace sin relación en B → no entra; (9) el manifiesto dice B → C, pero C falta → UNKNOWN; (10) todos los nodos fieles → hallazgo acreditado |
| GAP-10 y GAP-11 | conservar: la salida visible para el revisor es la autoridad de fidelidad; el truncamiento es degradación observable; la compactación es evidencia de runtime, no invalidación automática; un resumen cifrado o no observable no es evidencia. Mitigación operativa: lecturas acotadas de archivos grandes, sin congelar números de líneas de un proveedor |

## 3. Entrega ordenada y siguiente revisión formal

**Entrega**, producida de forma autónoma:
1. Proposal V14 completa, `Frozen: NO`;
2. paquete del Architect V14;
3. este registro;
4. decisiones, evidencia, estado y contrato;
5. delta V13 → V14 exacto;
6. commit y blobs exactos;
7. CI de publicación exacta.

Límites: sin implementación, ediciones normativas compartidas, pilotos, OD-6 ni Freeze. Tras la CI exacta en verde: STOP e informe único.

**Siguiente revisión:** V14 tendrá **una** revisión final del Architect, con el `EffectiveInputClosure`, la evidencia de fidelidad visible para el revisor,
lecturas acotadas y la misma exención de `dotnet test` si hace falta. Su alcance:
- el residuo final de A62-V11-01;
- la arquitectura de dependencias canónica y acotada;
- la confirmación de los cierres anteriores.

La revisión **no** debe exigir expandir mecánicamente todos los documentos alcanzables por la prosa. Con cero REQUIRED: el Owner decide OD-6, el Coordinator
da AGREED sobre la V14 exacta, y llega el Consensus Freeze.

## 4. Estado declarado por la disposición

```text
V13 = CHANGES REQUIRED
OPEN REQUIRED:   A62-V11-01 final residual
OD-6 = PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 REMAINS ACTIVE UNTIL I-62 IS INTEGRATED
```
