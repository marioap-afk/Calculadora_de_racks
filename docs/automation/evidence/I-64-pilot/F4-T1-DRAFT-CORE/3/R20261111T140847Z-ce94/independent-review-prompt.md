Revision adversarial independiente (I-64, F4-T1-DRAFT-CORE, intento 3, solo N-1). Solo lectura: no edites, no hagas commit, no ejecutes AutoCAD. Puedes ejecutar git, python y dotnet test en el worktree indicado.

Worktree: <worktree-de-I-64>
Rangos (rama architecture/workspace-persistente-rackcad): BaseSha d737b4853b204d6b8cc6adcf2abe97d96d7ce88e; RED 5a27ac8b172efe29aab4f9c15de0ba64dfa9adf3; GREEN ce73ceb719cbc08ee4b1ee0f989ac2e5b4258649.
dotnet: usa <perfil-de-usuario>\AppData\Local\Microsoft\dotnet\dotnet.exe (el del PATH no sirve). Si ejecutas pruebas o programas de sondeo, ponlos solo bajo el directorio temporal del sistema, nunca dentro del worktree, y borralos al terminar.

Fuentes que mandan (leelas en el worktree):
- Excepcion del Owner: docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1-attempt3-exception.md, y F4-E10 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md.
- Contrato: docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/gate-contract-attempt3.json (INV-F4T1-22 y INV-F4T1-25 a 27; AllowedWriteScope de una ruta).
- Hallazgo N-1 que motiva el intento: docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/2/R20261110T194129Z-0d3b/independent-review.md.
- Herramienta de hashes: docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/n1_hashes.py (ejecutala desde el worktree con BaseSha RedSha CurrentSha).

Tu trabajo es intentar REFUTAR que el intento 3 corrige N-1 dentro de los limites de la excepcion. Con evidencia concreta (archivo:linea, salida de ordenes):
1. Alcance: git diff --name-only BaseSha..CurrentSha y BaseSha..RedSha y RedSha..CurrentSha. Solo puede aparecer tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs.
2. RED: solo inserta el bloque entre los marcadores, como lineas completas y fuera de la region de la regla; compila; con la regla de BaseSha fallan por asercion pruebas del bloque y ninguna otra.
3. GREEN: solo cambia la region de la regla (comentario contiguo y declaracion de BomInAnyIdentifier). Ejecuta n1_hashes.py y comprueba H1, H2 y H3. Busca cualquier cambio de pruebas, aserciones o datos.
4. La regla reparada: comprueba con la regex real (I63Concepts y ForbiddenMemberNames, ambas con IgnoreCase) que es un superconjunto de la regla de 90a81104 (texto: Bom al inicio de palabra; nombres de miembro: Bom al final; sin distinguir mayusculas) y de la de 09955667, probando una bateria amplia (BOM, Bom, bom, bOm en prefijo, sufijo, mitad de identificador, guion bajo, digitos, inicio y fin de cadena; DraftBomLines, BomBuilder, _bomCount, BOMLines, DraftBOM, GetBOM, draftbom). Busca casos que antes se detectaban y ahora no. Comprueba que no es un caso especial de cadenas literales y que el codigo real del nucleo (src/RackCad.Application/Workspace/Drafts) sigue pasando todas las guardas.
5. Pruebas del bloque: que de verdad prueben lo que dicen (no tautologicas) y que cubran BOM, Bom y bom en prefijo, sufijo y nombre de miembro.

No cuentes preferencias cosmeticas como defectos. Clasifica cada hallazgo como BLOQUEANTE (viola la excepcion, el contrato o deja N-1 sin corregir), IMPORTANTE o MENOR, con id, invariante, archivo:linea, escenario concreto y correccion minima. Si un apartado queda limpio, dilo explicitamente.

Responde en espanol, en texto plano, con una tabla final (N-1 corregido si/no; alcance; RED; GREEN; hashes) y un recuento por clase.
