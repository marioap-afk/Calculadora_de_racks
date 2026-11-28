# Disposición para la verificación de F4B-PBT4-FULL-READ-PATH tras la parada de R20261120T211710Z-a468

**Autoridad:** la decisión del Owner del 2026-11-20, «Reverificar con aclaración (Recomendado)». Es la pregunta y la respuesta literales del registro `owner-pbt4a-verify-stop-decision.md`, que la sesión custodia con el cierre de PB-T4a.

**El punto:** la comprobación Authority (16.3) sobre `git diff --name-only AuthorityRevision BaseSha`.

**La aclaración:**
1. La regla 16.3 define tres clases: UNIT_DOC, UNIT_CHANGE y EXTERNAL, la última como «toda otra autoridad». Para ese diff solo exige que no contenga rutas EXTERNAL ni rutas con secciones UNIT_CHANGE.
2. Una ruta del diff que **no figura en Authorities** no es una autoridad del contrato. No pertenece a ninguna de esas clases: no es EXTERNAL ni UNIT_CHANGE, y la regla la admite. Eso no es un hecho «no verificable». Lo verificable es que no esté en Authorities ni como EXTERNAL ni como UNIT_CHANGE, y que el diff no contenga ninguna otra ruta de esas clases.
3. En esta delegación esa ruta es el generador del contrato que custodia el commit BASE de la emisión: `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-READ-PATH/coordinator/make_pbt4a_contract.py`. Las emisiones de F4B custodian así su generador desde PB-T2a, y las verificaciones de PB-T2a a PB-T3c lo admitieron con el mismo hecho.

**Lo que no cambia:**
- comprueba todo lo demás de 16.3 tú mismo con git, como siempre:
  - que AuthorityRevision es ancestro de BaseSha;
  - las cuatro rutas del diff y que ninguna es EXTERNAL ni tiene secciones UNIT_CHANGE;
  - las autoridades UNIT_CHANGE del contrato y su lectura en AuthorityRevision;
  - las EXTERNAL frente a MainSha;
- cita en la Evidence de Authority lo que midas;
- esta disposición no relaja ninguna otra comprobación.
