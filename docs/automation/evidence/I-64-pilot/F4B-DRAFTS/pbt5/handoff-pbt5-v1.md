# Traspaso del autor del paquete v1 de PB-T5 (por si se corta el contexto)

Estado final: paquete y preguntas escritos en esta carpeta (I-64-f4b-pbt5-package-v1.md y owner-questions-v1.md). Este traspaso queda como registro de lo leído; los sha256 los da el informe al Coordinator (sha256.txt).

## Lo leído (solo por objetos git en 7da361eb, desde el checkout principal)
- Paquete de F4B v3.1: D-PB-01 a D-PB-10, §4, §5 (PB-T5 = «CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B»), §6.3 (reglas de operación del bloque M), §6.5 (error del operador, 2 repeticiones), §7.2 (las reglas de F4 no se heredan), §8 (R-PB-01 a 11), §9 (criterios de cierre).
- f4b-dispositions.md E01 a E60. Lo que deja al Smoke-PB: E48 (borrado real; OWNER-1 a 3 de PB-T3c), E51 (relectura en el origen, coste, OWNER-01 origen a Primary), E53 (OWNER-01 nombre tras Conservar, OWNER-04 segundo Sí, OB-03.2, Actualizar, Conservar, borrado real), E55 (OWNER-02 recarga, OWNER-03 BOM y biblioteca, OWNER-04 pregunta de Cerrar, Modificado, aviso M-04; modal, foco, eventos del puente), E59 (tres caminos de BuildCore), E60 (PB-T4e; R-T4-39 como información junto al paso 6). E06, E08 y E12: correcciones declaradas de RACKEDITAR (D-PB-06 a/b, D-PB-08, D-PB-11, H-31, OWNER-1 de PB-TC2). E23: BOM y biblioteca en WORKSPACE. E09: pb-21-cola fuera.
- Paquete de PB-T4 v5.6: §5.5 (lista v3 + riesgos de PB-T3c + los 8 pasos de PB-T4e), §9, §10, D-PBT4-05/08/09/19/20.
- Precedente F4: F4-DRAFTS/gate-close.md; F4-E71 a F4-E81; F4-SMOKE-2/prep (dll.md, dll-c2.md, selection.md) y run (run-log-2, integrity); owner-f4-smoke2-decisions.md, owner-smoke2-session-operator.md, owner-f4-smoke2-operator-instruction.md; el commit 5ccaa770 (cambios de I-64.yml y de I-64-evidence.md §230).
- PB-T0 bloque M: block-m-analysis.md, run-log-m.md, coordinator/pbt0-block-m-procedure.md (NETLOAD «Load Once», funciones LISP tecleadas, N-I1, segunda instancia si el Owner tiene AutoCAD abierto).
- Guía validacion-manual-autocad.md (§1 a §3, §6, §7, §7.1, §7.2), WORKFLOW §4 y §6, AGENTS «definición de terminado» y «reutilización», I-64.yml, HANDOFF (no se toca en un gate-close de fase).
- src en 7da361eb: UiSupport.PromptSaveToLibrary usa un SaveFileDialog con la carpeta de la biblioteca como inicial (se puede elegir otra).

## Decisiones de redacción ya tomadas
- 52 pasos SPB-01 a SPB-52 en bloques 0, A a H y Z, más 8 precondiciones P-01 a P-08.
- Preguntas Q-PBT5-1 a 6 y una información (R-T4-39).
- La ciudad de la hora del Owner se redacta (no está en el repo); se da la frontera en UTC.

## Lo que falta (fuera de este encargo)
- La revisión independiente del paquete v1.
- Completar lo marcado [T4e] cuando cierre PB-T4e.
