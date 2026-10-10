# I-65 — Paquete de revisión del Architect, ronda 2 (Proposal V2)

## 1. Encargo

Re-revisión de la **versión completa** Proposal V2 ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5): el delta es foco mínimo, no
límite. Ronda 1: `CHANGES REQUIRED` sobre `0f4bc884` (custodia en
[architect-r1-verdict.md](../automation/evidence/I-65-f0/architect-r1-verdict.md)). Solo quien emitió un `REQUIRED` puede rebajarlo.

**Límite conocido de esta ronda:** A65-V1-01 no puede cerrarse sin ejecutar P1 (requiere la autorización del Owner, Q-O4). V2 lo
convierte en precondición dura del Freeze (§0) y marca **[P1]** cada valor por medir. Se pide al Architect que diga si A65-V1-01 queda
`OPEN — BLOCKED BY P1` (correcto por diseño) y que dictamine todo lo demás, incluida la suficiencia y seguridad de **P1 v2**, para que
la sonda que se ejecute sea la definitiva.

## 2. Entradas (versión completa)

| Entrada | Ruta |
|---|---|
| Proposal V2 (completa, autocontenida) | `docs/initiatives/I-65-proposal-v2.md` |
| Borrador ADR-0050 | `docs/adr/0050-interoperabilidad-analisis-estructural.md` |
| Sonda P1 v2 (no ejecutada) | `docs/automation/evidence/I-65-f0/ram-probe-p1.py` |
| Discovery (con adenda §13) | `docs/initiatives/I-65-discovery.md` |
| Veredicto ronda 1 | `docs/automation/evidence/I-65-f0/architect-r1-verdict.md` |
| Mandato, contrato, decisiones | sin cambios desde la ronda 1 |
| Proposal V1 (sustituida) | `docs/initiatives/I-65-proposal-v1.md` |

El commit exacto revisado se fija en la orden de revisión.

## 3. Delta explícito V1 → V2

1. §0 nuevo: precondiciones FP-01..04 y regla STOP para longitud, fuerza y posición de carga (sustituye la «exclusión de magnitud»).
2. D-01: alcance y tokens de G1; `CreateRamBuildPackage` y el lector de volcados pasan a la carpeta del proveedor; G2 incluye
   `Systems/* ↛ Providers/*`.
3. D-02 recortado: fuera momentos nodales, cargas locales y distribuidas, Cw, masa, peso propio y el punto de extensión de traducción de
   cargas; secciones con A, Ix, Iy, J informativas.
4. D-03 reescrito: almacenamiento en **pulgadas y lbf** (ADR-0005), tabla de propietarios por dimensión, propietario único nuevo de lb y
   gₙ con `StructuralSectionUnits` extendido bit a bit, G4 acotada y con lista cerrada, reglas de exactitud y par provisional `LB_IN` [P1].
5. D-04: origen = datum local de la estación; tabla de transformación de apoyos; vector de peralte con rotación y espejo; cobertura de P1.
6. D-05: reglas por clase de nudo IDL-01..10, tolerancias, familias doblemente simétricas, `REF-CANT-01`, golden a mano.
7. D-06/D-07: fuente de propiedades como decisión Q-O2 con recomendación (b) y fila ±0.5 %; materiales sin valores en RackCad con (b).
8. D-08: escritura atómica, puerta de capacidad medible (`__version__` + funciones del puente), modelo vacío [P1].
9. D-09: ocho reglas congeladas de la plantilla (imports, literal JSON ASCII, ruta embebida, archivo exclusivo, categorías como texto,
   límites de carga, punto cardinal y demás, puerta antes de escribir); sin marcas de tiempo en todo el paquete.
10. D-10: SUP-02 = opción A; supuestos con limitación; ASM-PROP-01.
11. D-10b nuevo: canal de cargas con unidad explícita, en la huella.
12. D-11: comparador rechaza `FAILED`, `STALE` y emparejamientos ambiguos.
13. D-12: RackId = Id del sobre; `SourceFingerprint` sobre los bytes persistidos (conservador).
14. D-13: separación neutral/proveedor y guarda del Plugin.
15. D-14: F6 excluye `member_design` y ratios.
16. D-15 + borrador de ADR-0050 con cláusula de compatibilidad (ADR-0005/0021/0040/0017/0009).
17. §3: RED en todas las filas; INV-15 (unidad obligatoria) e INV-16 (propietario único) nuevas.
18. §4: filas de propiedades, materiales y valores por defecto de miembro; sin filas de comportamiento.
19. §5: OV-I65-05/06 de inyección de fallo; repetición sobre el Candidato final; Candidato de hito en F4.
20. P1 v2: puerta antes de la primera escritura (sin `SystemExit`), análisis primero, orientaciones de la rebanada, entorno del script.

## 4. Disposición por hallazgo

La tabla de la Proposal V2 §13 da la disposición de A65-V1-01..19. Ninguno se considera cerrado por el Coordinator: el Architect decide.

## 5. Preguntas de la ronda 2

| ID | Pregunta |
|---|---|
| AQ2-01 | ¿Almacenar en pulgadas/lbf (D-03) cierra A65-V1-07 junto con la cláusula de ADR-0050? |
| AQ2-02 | ¿El propietario único de lb y gₙ con `StructuralSectionUnits` extendido bit a bit cierra A65-V1-05 sin activar M-08 sobre ADR-0021? |
| AQ2-03 | ¿La puerta de P1 v2 (nudos y miembros = 0, `OUT_DIR` válida; casos solo registrados) es suficiente, dado que la línea base de casos de File > New es justamente lo que P1 mide? |
| AQ2-04 | ¿Las reglas IDL-01..10 dan valores golden determinables a mano para `REF-CANT-01`? |
| AQ2-05 | ¿Las filas §4 y OV-I65-05/06 eliminan los oráculos ciegos de A65-V1-08 y A65-V1-12? |
| AQ2-06 | ¿Queda algún `REQUIRED` que impida congelar V3 una vez que P1 entregue valores y el Owner responda Q-O1..Q-O3 y Q-O7? |

## 6. Formato del veredicto

El mismo de la ronda 1 (paquete v1 §5), con `PRIOR_FINDINGS: A65-V1-nn → CLOSED | OPEN | OPEN — BLOCKED BY P1 | DOWNGRADED(motivo)`.
