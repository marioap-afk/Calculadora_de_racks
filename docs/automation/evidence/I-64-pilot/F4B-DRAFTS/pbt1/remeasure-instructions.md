# Encargo: remedición del paquete de PB-T1 v2.2 sobre el producto final de PB-TC2 (v2.3, el texto de emisión de PB-T1a)

Lo da la sesión principal al redactor.

**Dónde está la rama:**
- PB-TC2 está **CLOSED / PASS WITH LIMITATIONS**: GREEN `b5c09f0b`, custodia del cierre `cd47523b` (F4B-E12).
- La rama está en `cd47523b`, y `git diff b5c09f0b cd47523b -- src tests` está vacío.
- **Ningún Worker trabaja ahora en el worktree:** ya puedes leerlo, siempre en solo lectura.

**La orden del Owner (D2, F4B-E11, literal):** «Re-measure it against the final PB-TC2 product before emission. If that remeasurement changes no material premise: issue the first PB-T1 task normally.»

**Las reglas:**
- solo lectura en el worktree;
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc;
- `gh`, solo de lectura.

## Qué haces

1. Copia la v2.2 a `I-64-f4b-pbt1-gate-contract-package-v23.md` y no toques la v2.2.
2. **Ejecuta la remedición R-1 a R-13** del §8.4 sobre `cd47523b`, con el producto de `b5c09f0b`:
   - las anclas de las zonas que tocó PB-TC2 (`Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, `ReadCellValues` y `OptionalVal`; `Val` ya no existe; los miembros nuevos son `KeepOptional`, `MixedBoth` y `KeepCell`);
   - los textos de los 15 mutantes de PB-T1a;
   - las anclas de D-PBT1-13 y D-PBT1-03, y la pasada reentrante de F-03;
   - que `RackHeaderHeightBox` ya no redondea, y el mapa de controles;
   - la línea base de la CI, con el último run verde de la rama (mídelo con `gh`), y los recuentos;
   - los alcances 59/117/59;
   - CT-PBT1A-11 frente a la agrupación del BOM por valor exacto (D1);
   - las reglas de G-L1, sin coincidencias hoy;
   - el miembro de la lista del informe de módulos;
   - la huella de CT-PBT1B-16;
   - las asignaciones de `Name` en el Plugin.

   **Registra cada medición** en un apartado nuevo de `pbt1-measurements.md`.
3. **Actualiza la v2.3** con las anclas y los recuentos medidos. Añade al principio un registro de cambios v2.2 → v2.3.
4. **Clasifica cada diferencia** como «no material» (anclas, recuentos, textos) o «**material**». Es material lo que cambia una premisa del diseño, una decisión del Owner, una de sus 10 condiciones de L-1, una guarda de T1B (OB-T1B.3), un efecto observable o la división en subtareas, según tu definición L-6.
5. **Si encuentras algo material,** dilo claramente: la emisión se para y vuelve al Owner.
6. **Prepara la emisión de PB-T1a:**
   - la línea de estado de la v2.3, que diga que PB-T1a se emite por F4B-E09 y F4B-E11 tras la remedición y la comprobación acotada;
   - el generador del contrato de PB-T1a, `pbt1/make_pbt1a_contract.py`, siguiendo a `pbtc2/make_pbtc2_contract.py`. `TaskId` `F4B-PBT1A-LOSSLESS-STATE-NET`, con su `AllowedWriteScope`, el `ForbiddenWriteScope` por el método de PB-TC2, los `RequiredTests`, los invariantes, `ExpectedEvidence` y las paradas (con las estándar, las de [DP] transcritas literales como en PB-TC2, C04, C15 y C17), las celdas e `IssuedBy`.
   - Ejecuta solo `python make_pbt1a_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`, que no escribe nada.

**Tu respuesta final** es corta:
- la ruta;
- la salida del `--dry`;
- la lista de diferencias de la remedición con su clase;
- **cualquier diferencia material**;
- lo no medido.
