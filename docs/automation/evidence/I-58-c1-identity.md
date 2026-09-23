# I-58 C1 — identidad Cantilever: evidencia y limite de inferencia

PROBE_VALIDATION_SHA: 5d69a0670a6576fe344a983c45c6187a5bb280ec. SDK 8.0.423.
Arbol limpio antes/despues de probe y de ambas corridas focales. No es Candidate ni cierre de gate.
Log [I-58-c1-probe-observed.txt](I-58-c1-probe-observed.txt). Comando y alcance en
[I-58-c1-probe/README.md](I-58-c1-probe/README.md). 4 seleccionados, 0 fallos, exit 0, 18.8648212 s.
Es diagnostico de comportamiento ACTUAL, no GREEN de AUTH-13 ni suite F1.

## Procedencia

- MEASURED (ejecucion): RackCantileverWindow real STA, LoadNew/LoadDesignForNew/LoadExisting, controles,
  RequestDraw/request; stores de Application, AUTH09 y CantileverLineEditorAssembler actuales.
- RECONSTRUCTED (ejecucion de extraccion literal): BuildCantileverPayload, CantileverKindHandler.RestampDesign
  y RackEnvelopeRestamp.RestampEnvelope(Guid). Harness verifica texto exacto contra fuente antes de empezar;
  dispatch de registry sustituido por llamada Cantilever directa. No se carga Plugin/AutoCAD.
- MEASURED (inspeccion de fuente, NO test conductual): I-55 membership/scan/PrepareExisting y AUTH10.
- INFERENCE: la discrepancia exterior/interior es irrelevante SOLO para igualdad authored dentro de conjunto
  exterior coherente bajo F-03 V2. Propuesta B pendiente, nunca observacion de un comparator implementado.

GUID interior generado por constructor REAL en esta corrida: 7722e18c-f448-471a-bfc3-5be5d4031755 (L).
Los outer de A/B/D son valores fijos del harness; verificar desigualdad/equivalencia no depende de su azar.

| Escenario | Envelope.Id exterior | Line.Id interior | Exterior/interior | Id interior entre hermanas del mismo rack |
|---|---|---|---|---|
| A nueva | 10000000-0000-0000-0000-000000000001 | L | distintos | igual; segunda vista sintetizada por writer extraido |
| B desde biblioteca | 20000000-0000-0000-0000-000000000002 | L, conservado desde plantilla | distintos | igual; segunda vista sintetizada por writer extraido |
| C existente reabierta + nueva hermana | 20000000-0000-0000-0000-000000000002 | L | distintos | igual; LoadExisting + InsertPlanta real conserva exterior e interior de B |
| D duplicacion/restamp reconstruido | 40000000-0000-0000-0000-000000000004 | 40000000-0000-0000-0000-000000000004 | iguales | igual en los dos payloads restampados con mismo nuevo GUID |

B usa proyecto de biblioteca serializado/reabierto real y la entrada LoadDesignForNew que llama
CantileverEditorModule.OpenFromLibrary. No se abrio el menu/library storage UI. C usa entrada LoadExisting
que llama RackCantileverCommands.EditCantilever; no ejecuto comando RACKEDITAR ni inserto bloque en DWG.
D ejecuta cuerpos puros exactos con dispatch limitado; no ejecuto RACKDUPLICAR ni su atomicidad.

| Escenario | Identidad membership I-55 (fuente) | AUTH09 ejecutado | AUTH10 (fuente) | Impacto esperado en authored equivalence V2 |
|---|---|---|---|---|
| A | outer 100...001 | CantileverLineDesign con L; assembler una llamada | resolved tipado; sin parametro outer Id | diferencia exterior/interior no impide Single entre hermanas iguales |
| B | outer 200...002 | CantileverLineDesign con L; assembler una llamada | resolved tipado; sin parametro outer Id | compartir L con plantilla/A NO vuelve A y B hermanas; conjunto exterior mezclado Unreadable |
| C | mismo outer de B | CantileverLineDesign con L; assembler una llamada | resolved tipado; sin reconciliar identidades | hermana nueva preserva authored interior; F-03 V1 la rechazaria por outer!=inner |
| D | nuevo outer 400...004 | CantileverLineDesign con nuevo GUID; assembler una llamada | resolved tipado; sin restamp | hermanas de copia comparables; copia y original no se mezclan |

AUTH09 de I-57 es un adapter generico: no tiene politica de RackId. En el probe se compone con assembler
existente, que hace snapshot y resolve una vez. AUTH10 prepara la vista del resultado tipado y no recibe
el envelope para elegir identidad. En main no hay wiring productivo AUTH13 de estos cuatro kinds.
La tabla documenta el seam verificado/propuesto, no anuncia ID18 funcionando.

## Fuentes exactas leidas

Codigo propio en PROBE_VALIDATION_SHA (identico al inicial para src/):
CantileverLineDesign.Id/DeepCopy; RackEditorIdentity.EnsureId; RackEditorSession.Complete;
RackCantileverWindow.LoadNew/LoadDesignForNew/LoadExisting/RequestDraw; CantileverEditorModule.OpenFromLibrary;
RackCantileverCommands.EditCantilever/BuildCantileverPayload; CantileverKindHandler.RestampDesign;
RackEnvelopeRestamp.RestampEnvelope; CantileverLineEditorAssembler.Build; RackResolveAdapter.Resolve;
RackViewPreparationAdapter.Prepare. Inventario de blobs en I-58-c1-source-identities.txt.
Decision I-37 §12.46 vinculante permanece intacta: un GUID por linea, compartido por vistas.

I-55 solo lectura en 08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d:
RackSiblingScan.Scan rellena RackId= envelope.Id y ProbeId= envelope.Id o RackEnvelopeIdProbe.
RackSiblingMembership.Classify compara esos valores con identidades exteriores atribuibles; no lee Line.Id.
RackProductPreparer.PrepareExisting entrega comparison.Authored a resolve, y source.Id al compose.
No se copio su codigo al harness ni se ejecuto/modifico su worktree.

## Deuda D58-CANT-ID y clasificacion

Defecto preexistente: nueva/biblioteca pueden separar outer Id de CantileverLineDesign.Id pese a I-37 §12.46.
La biblioteca puede conservar el mismo inner en racks distintos; corregirlo no pertenece a I-58.
No se cambia la decision historica, ni se repara identidad, migra o restampa persistencia real.

Propuesta B SOLO para I-58: AUTH13 compara intencion dentro de pertenencia exterior acreditada;
Line.Id valido comun entre hermanas basta como campo authored. F-03 V2 no exige igualdad outer/inner.
Si inner valido difiere ENTRE hermanas, Divergent; si invalido/vacio/ausente, Unreadable. No ignorarlo.
La propuesta no declara el defecto inocuo para otros consumidores ni verifica el corpus DWG del Owner.
Los casos existentes fuera del alcance probado siguen sin validacion manual; ningun hecho ausente se rellena.

Estado efectivo: EXP-01 = CLASS A OPEN / STOP. Rebaja propuesta B = PENDING COORDINATOR + ARCHITECT
CONFIRMATION. EXP-08 positiva. Si estos limites impiden confirmar irrelevancia, mantener A y requerir
revision; I-58 no puede arreglar el producto para desbloquearse. IMPLEMENTATION AUTHORIZATION = NO.

## Intentos descartados y pruebas complementarias

Todos los intentos se iniciaron/terminaron con arbol limpio; no se atribuye una corrida a su hijo:
- b6fd1796df79f6464ee54f773bc1f32661147cd4: falta using System.IO, build fallo; exit 1, 59.3520525 s, seleccion 0.
- 746357e26aafc0631569d76a51d5dd6efdf67882: catalogos no copiados por proyecto; setup fallo; exit 1, 33.6504187 s, seleccion 0.
- 0924587826d6c1c8806f581b9d99aae9b8f94f30: fixture sin completar controles no produjo request; setup fallo; exit 1, 18.7044184 s, seleccion 0.
Son FALLOS, no RED de comportamiento ni evidencia positiva. El harness se corrigio sin tocar producto.

Sobre PROBE_VALIDATION_SHA limpio, SDK 8.0.423:
- Core focal: SharedViewFoundationF5Tests + SharedViewFoundationF6Tests + CantileverPersistenceAndViewTests:
  66 seleccionados/PASS, 0 FAIL/SKIP, exit 0, wall 24.7457075 s.
- UI focal: CantileverEditorWindowTests + RackEditorSessionTests:
  28 seleccionados/PASS, 0 FAIL/SKIP, exit 0, wall 17.6615607 s.
Logs I-58-c1-core-observed.txt / I-58-c1-ui-observed.txt contienen comandos seleccionados por clases
registrados en evidencia canonica. No Full Core+UI ni Candidate. No se propagan al commit documental V2.
