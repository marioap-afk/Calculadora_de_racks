# Revisión adversarial independiente de F4-T1-DRAFT-CORE, intento 3

- **Revisor:** subagente independiente (modelo Opus), sin conclusiones previas, de solo lectura.
- **Encargo:** `independent-review-prompt.md`, junto a este archivo.
- **Rangos revisados:** `d737b485..5a27ac8b..ce73ceb7`.
- **Texto:** sin cambios.

---

No refuto el intento 3: corrige N-1 sin salirse de la excepción F4-E10 ni del contrato del intento 3. No hay hallazgos bloqueantes ni importantes; dejo dos menores, ninguno de los cuales obliga a rehacer nada.

Trabajé en solo lectura: el worktree y el checkout principal siguen limpios, con HEAD = ce73ceb7. Exporté RED y GREEN con git archive a %TEMP%\n1rv, corrí allí las pruebas y la sonda de regex, y borré ese directorio al terminar.

**1. Alcance: limpio.**
- `git diff --name-only` devuelve solo `tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs` en los tres rangos (Base..Current, Base..Red y Red..Current).
- La rama es lineal: d737b485, luego 5a27ac8b, luego ce73ceb7.

**2. RED: limpio.**
- Solo inserta líneas completas en una única posición: 587-632 del RED, antes de la llave final de la clase. La región de la regla queda en 158-159, así que no se solapan.
- Lo comprobé a mano, sin usar la herramienta: el RED sin las líneas 587-632 es idéntico byte a byte al archivo de BaseSha.
- Compila, y la suite Core completa da 13 fallos de 16838. Los 13 son pruebas del bloque, todos por `Assert.Matches() Failure` y ninguno por excepción:
  - en la prueba de texto del núcleo: BOM, BOM.Build, BOMLines, DraftBOM, GetBOM, draftbom y DRAFTBOM;
  - en la prueba de nombres de miembro: BOM, BOMLines, DraftBOM, GetBOM, DRAFTBOM y draftbom.
- La CI del SHA exacto 5a27ac8b coincide: el job Tests (Domain + Application) termina en failure con los mismos 13 fallos.
- Los 13 casos coinciden con lo que predije analizando la regla de BaseSha. Los casos que ya detectaba (Bom, bom, bomCount, GetBom, BomBuilder, DraftBomLines y _bomCount) pasan, como se espera.

**3. GREEN: limpio.**
- El diff Red..Green solo cambia las líneas 158-159 por 158-161: el comentario contiguo y la declaración de `BomInAnyIdentifier`. No toca pruebas, aserciones, datos ni otras familias de guarda.
- `n1_hashes.py` es la versión commiteada en c31854f8 (mismo SHA-256 del archivo) y da Pass true:
  - H1 Red = Current = 96A696F1…2E62
  - H2 Red = Current = 9BA25558…DF67
  - H3: el RED sin el bloque = Base = 843DD9F1…88C5
  - el bloque no se solapa con la regla
- Rehíce H1 y H2 cortando líneas a mano, y también salen idénticos.
- La suite Core completa pasa (16838 de 16838). El filtro `RackCad.Tests.Workspace` selecciona 1300 pruebas (el mínimo es 1273) y todas pasan.
- La CI de ce73ceb7 tiene los cuatro jobs en success.

**4. Regla reparada: limpio.**
- Usé las regex reales, sacadas por reflexión de los ensamblados compilados: `I63Concepts` y `ForbiddenMemberNames`, ambas con IgnoreCase. Su patrón coincide con el texto esperado tanto en RED (regla de 09955667) como en GREEN.
- La batería tiene 3528 cadenas: las 8 combinaciones de mayúsculas de "bom" con 21 contextos a la izquierda y 21 a la derecha (inicio y fin de cadena, guion bajo, dígitos, punto, paréntesis, mayúsculas y minúsculas). Hay 0 casos que alguna regla anterior detectaba y la nueva no. Comparé con 90a81104 (texto e identificadores), con 09955667 y con la regla del padre `Bom\b`.
- La nueva detecta 802 casos más en el texto de código y 660 más en nombres de miembro.
- Detecta todos los nombres pedidos: DraftBomLines, BomBuilder, _bomCount, BOMLines, DraftBOM, GetBOM, draftbom, DRAFTBOM, GetBom, BOM.Build y bOm.
- No trata ninguna cadena literal como caso especial: son alternativas generales.
- El código de `src/RackCad.Application/Workspace/Drafts` no contiene "bom" en ninguna forma, ni en texto crudo ni en nombres de miembro, y pasa todas las guardas.

**5. Pruebas del bloque: no son tautológicas.**
- La prueba de texto inyecta cada caso sobre código real del núcleo y comprueba antes que ese código, solo, no coincide. Ningún caso coincide por otra alternativa de la regex (Metric, Provider, etc.).
- Cubren BOM, Bom y bom en prefijo, sufijo y nombre de miembro.
- Mutaciones de la regla:
  - las pruebas detectan quitar `_bom`, quitar `Bom(?![a-z])`, quitar `bom\b` sin distinguir mayúsculas, quitar `\bbom` sin distinguir mayúsculas y volver a la regla de 09955667;
  - otras tres pasarían sin que fallara ninguna prueba (detalle en m-1).

**Hallazgos**

- **m-1 — MENOR — INV-F4T1-25 y 26 (cobertura).** Bloque en `WorkspaceDraftBoundaryTests.cs:589-634` del GREEN. Tres comportamientos de la regla no los fija ninguna prueba:
  - Si se quita la alternativa `BOM(?![a-z])`, todo sigue pasando y se dejan de detectar DraftBOMLines y RACKBOMTOTAL.
  - Si `\bbom` deja de aceptar una minúscula detrás, todo sigue pasando y se pierden Bomb, bomba y BOMlines, que 90a81104 sí detectaba. Es decir, ninguna prueba fija que la regla sea un superconjunto de 90a81104.
  - En la prueba de nombres de miembro falta un "bomCount" sin guion bajo. Hoy lo cubre solo la prueba de texto, porque las dos regex comparten la constante.
  - La regla actual es correcta en los tres casos; el riesgo es solo ante cambios futuros.
  - Corrección mínima: añadir esos casos. No cabe en este intento, porque el GREEN no puede añadir pruebas y no hay intento 4; propongo registrarlo como seguimiento.
- **m-2 — MENOR — precisión del comentario**, `WorkspaceDraftBoundaryTests.cs:158-160` del GREEN.
  - El comentario dice "whatever its case … or inside it before an uppercase letter, a digit or an underscore". Dentro de un identificador solo se detectan las formas exactas Bom, BOM y _bom.
  - No se detectan, por ejemplo, DraftbOmLines, DraftBoMLines, DraftbomLines ni Draft_bOm_Lines.
  - No es una regresión: ninguna regla anterior los detectaba y el contrato no lo exige.
  - Corrección mínima: cambiar la redacción dentro de la región de la regla, o registrarlo.

Además, dos observaciones que no cuento como defectos:
- Una regla que tratara literales como casos especiales también pasaría estas pruebas. Ninguna prueba puede impedirlo, y la regla real no lo hace.
- La regla nueva vuelve a marcar Bomb y bomba al inicio de palabra, que 09955667 excluía a propósito. Es consecuencia obligada de exigir el superconjunto de 90a81104, y el núcleo no se ve afectado.

| Punto | Resultado |
|---|---|
| N-1 corregido | Sí |
| Alcance | Limpio (una sola ruta en los tres rangos) |
| RED | Limpio (compila; 13 de 16838 fallos, todos del bloque y por aserción; CI en failure por esas pruebas) |
| GREEN | Limpio (solo la región de la regla; 16838 de 16838 superadas; filtro 1300 ≥ 1273; CI 4/4 en success) |
| Hashes | Pass true (H1, H2 y H3 iguales; recalculados a mano con el mismo resultado) |

Recuento por clase: BLOQUEANTE 0, IMPORTANTE 0, MENOR 2 (m-1 y m-2).
