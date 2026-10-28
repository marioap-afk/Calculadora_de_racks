I-65 — REVISION INDEPENDIENTE DEL ARCHITECT — F7 (conformidad, FOUNDATIONS, guia de uso, ADR-0050, plan de READY)
Rol: Architect de I-65, SAME-SESSION ROLE (subagente de contexto independiente del Coordinator), SOLO LECTURA.
Repositorio: rama architecture/interop-analisis-estructural; worktree C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural.
HEAD bajo revision (exacto): 8b2117c3f71feb99cf9395f1e780ea31e2073e02 (entregables de F7, decisiones §100). Producto = arbol de 51fc82f1
(Candidato de hito F5 bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109; F5 GATE PASS sobre 45aaad6e, aceptado por el Owner en §99). Base de main:
bb0d5522. Tus revisiones previas: docs/automation/evidence/I-65-f5/architect-f5-final-conformance.md (A65-F5F-01..08) y anteriores.

REGLAS DE OPERACION (obligatorias; controles del Owner, decisiones §41 y §99):
- No escribas, no edites, no crees ni borres NINGUN archivo en ningun lugar; no hagas commit, push, fetch, checkout, stash ni
  `git merge-tree --write-tree` (escribe objetos): solo lectura.
- No compiles ni ejecutes pruebas, AutoCAD, RAM Elements ni ningun interprete (python, python3, py).
- Tu directorio de proceso es el checkout principal del Owner (D:/Documentos/Codex/Calculadora de racks): NO lo leas ni lo uses. Lee
  SOLO con rutas absolutas del worktree de arriba o con git -C <worktree> show <sha>:<ruta> / diff / log / rev-parse / ls-tree / cat-file /
  grep.
- No leas carpetas de D:/r65-smoke ni D:/r65-m05c.
- PRIVACIDAD (decisiones §90): tu informe se versiona en un repositorio PUBLICO: sin nombres de personas, cuentas, rutas con nombre de
  usuario ni datos de terceros; secretos: solo tipo y alcance.
- Primera accion: informa pwd y anotalo en tu informe.

Autoridades:
- Freeze docs/initiatives/I-65-proposal-v3.1.md (§2 D-01..D-15, §3 INV, §4 matriz, §5 OV, §6 gates —fila F7: «Conformidad, regresion,
  entrada de FOUNDATIONS redactada, guia de uso, ADR-0050»—, §7 Fundaciones, §9 no-objetivos) y docs/adr/0050-interoperabilidad-analisis-estructural.md.
- docs/INITIATIVE_LIFECYCLE.md §4.1 (FOUNDATIONS), §7, §8, §9; docs/FOUNDATIONS.md (esquema de entrada); docs/WORKFLOW.md §8 y §11.4-§11.5;
  AGENTS.md («Pruebas — definicion de terminado»).
- Orden del Owner: docs/automation/decisions/I-65-owner-disposition-f5-acceptance-f7-authorization.original.txt; decisiones §99-§100.
- Entregables de F7: docs/initiatives/I-65-f7-preparation.md (AC-F7-01..12, presupuesto, STOP), I-65-f7-conformance-review.md,
  I-65-foundations-entry.md, docs/guias/interoperabilidad-ram-elements.md, el diff de README.md y I-65-ready-final-plan.md.
- Codigo (solo lectura): src/RackCad.Application/StructuralAnalysis/** (incluido Providers/RamElements/), src/RackCad.Application/Units/,
  src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs, src/RackCad.Application/Systems/Cantilever/CantileverStructural*.cs,
  src/RackCad.Plugin/RackRamElementsCommands.cs, src/RackCad.UI/RackCommandReference.cs, tools/RackCad.StructuralAnalysis.RamTool/**,
  tests/RackCad.Tests/StructuralAnalysis*.cs, assets/structural-analysis/ram-elements/section-material-map.json.

Preguntas (responde cada una con evidencia concreta: archivo, lineas, sha, blob, decision):
1. Identidad: ¿entre bd1fcf8a y HEAD solo cambian docs/ y README.md? ¿src, tests, tools, assets, Directory.Build.*, global.json y
   RackCad.sln iguales a bd1fcf8a y a 51fc82f1? ¿Freeze (blob 4e6e1feb…) y ADR-0050 (blob c18def42…) intactos? ¿Algun A-n o EXP-01?
2. Criterios: ¿AC-F7-01..12 son los criterios correctos y suficientes para la fila F7 del Freeze y la orden §99? Evalua cada uno (CUMPLE /
   NO CUMPLE / PENDIENTE POR CONSTRUCCION): AC-F7-03 (regresion Full), AC-F7-11 y AC-F7-12 quedan pendientes hasta el cierre; di que
   falta exactamente. ¿La regresion Full definida (Core Full + UI Full locales + builds Debug de UI, Plugin y herramienta + CI exacto 4/4)
   es la que exige AGENTS.md para un punto que exige Full?
3. Revision de conformidad del Coordinator: verifica INDEPENDIENTEMENTE cada fila (gates, D-01..D-15, INV-01..16, §4, §5, §7, §9 y las
   Decisiones 1-9 de ADR-0050 con su clausula de compatibilidad) contra el codigo y la evidencia. ¿Alguna afirmacion incorrecta o sin
   evidencia? ¿La ausencia de huella en el volcado frente a la Decision 8 del ADR es una limitacion conocida (A65-F4F-04) o una EXP-01 de
   clase A? Razona con LIFECYCLE §4 y §9.
4. Entradas de FOUNDATIONS: ¿esquema de diez campos completo? ¿Cada afirmacion (autoridad, persistencia, contrato de mutacion, punto de
   extension, fuente de decision, pruebas que protegen, limitaciones) es factual contra el codigo de bd1fcf8a? ¿Las clases de prueba
   citadas existen y protegen lo que se dice? ¿Es correcto NO publicarlas en docs/FOUNDATIONS.md en F7 (LIFECYCLE §4.1, WORKFLOW §11.4) y el
   estado propuesto? ¿Cubren el «Introduces» y el «Extends» del Freeze §7?
5. Guia de uso y README: contrasta con el codigo cada hecho (prompts y resultados de RACKRAMPAQUETE, regla de carpeta, archivos del paquete,
   esquema, unidades y limites del archivo de cargas, puertas y mensajes del script de construccion, script de volcado y sus opciones,
   comparador: opciones, salidas 0-5 y filas, inyeccion de fallos). ¿Alguna afirmacion falsa, ambigua o que prometa mas que el Freeze
   (validacion de ingenieria, analisis, resultados)? ¿La fila del README es conforme a WORKFLOW §8?
6. Plan de READY final: ¿cubre los siete puntos de la orden §99 (Candidato final propio, repeticion de OV-I65-01..07, OV-I65-08, Architect,
   hallazgos materiales, conflicto con I-64, presupuesto) y READY-01..09 en orden? Verifica la afirmacion del conflicto con I-64 SIN
   merge-tree: compara `git diff --name-only bb0d5522 origin/architecture/workspace-persistente-rackcad` con
   `git diff --name-only bb0d5522 HEAD` (archivos tocados por ambos). ¿Las recomendaciones respetan el Freeze y no convierten OPTIONAL en
   REQUIRED sin base contractual?
7. Hallazgos residuales: ¿A65-F4F-04, A65-F4F-08, A65-F5F-03/04/05/08 y C-5 quedan conservados y evaluados para READY sin promocion
   indebida? ¿F7-OBS-01 (RACKAYUDA sin RACKRAMPAQUETE) esta bien clasificada (NON-MATERIAL, fuera de F7)? ¿Alguna otra superficie de
   documentacion o ayuda que WORKFLOW §8 exija actualizar en esta rama y falte (p. ej., docs/guias/despliegue.md, glosario)?
8. LIFECYCLE §7 para el cierre de F7, punto por punto (CUMPLE / NO APLICA / PENDIENTE POR CONSTRUCCION), y que SHA debe ser el SHA
   documental de cierre de F7 segun el precedente de F4/F5 (CD-I65-F4-219/221, CD-I65-F5-59/61).
9. Privacidad de los documentos nuevos.

Informe (texto, en espanol): veredicto CONFORMANT, CONFORMANT WITH OBSERVATIONS o NOT CONFORMANT sobre los entregables de F7 en
8b2117c3; respuestas 1-9; matriz AC-F7-01..12; tabla de hallazgos nuevos con id A65-F7-nn, ubicacion, hallazgo, severidad (REQUIRED u
OPTIONAL; REQUIRED solo con base contractual) y recomendacion; lista «no comprobado sin ejecutar»; pwd informado. NO declares GATE PASS
ni READY: esas determinaciones son del Coordinator y del Owner.
