# I-65 F1 — revisión de conformidad del Architect (custodia)

- Rol: Architect de I-65, `SAME-SESSION ROLE` (subagente de contexto independiente del Coordinator), solo lectura.
- Fecha: 2026-10-11. Alcance: `git diff 9d0c6619..667a3991 -- src tests` contra la Proposal V3.1 (Frozen), ADR-0050, la orden del Owner
  «Conditional GO for Freeze and F1», CD-I65-F1-01..19 e `I-65-f1-ref-cant-01.md`. Rama en `0319012422e42721919cd1efa9216d8f9ef6d2e9`
  (`git diff 667a3991..HEAD -- src tests` vacío). Sin compilar ni ejecutar pruebas.
- Texto: transcripción fiel del informe devuelto por el subagente (agentId `a6a865a4d1f444f47`), traducida al español en los pasajes
  narrativos; identificadores, rutas y cifras sin cambios.

## Veredicto

**CONFORMANT WITH OBSERVATIONS.** Ningún hallazgo REQUIRED; nada bloquea el cierre de F1.

## Respuestas a las preguntas de revisión

1. **Freeze, ADR-0050, S-02 y fuga de proveedor:** conforme. Capas: neutral en `Application/StructuralAnalysis/`, fuente en
   `Systems/Cantilever/`; Domain, UI, Plugin, `.csproj` y `Providers/` intactos. Los tipos nuevos llenan hipótesis de nombre que el Freeze
   dejó abiertas; nada es arquitectura fuera del Freeze. Sin tokens de RAM en el código neutral, la fuente ni `Units/`. ADR-0050: decisiones
   1, 3, 6 y 9 cumplidas; la 5 solo en parte (A65-F1-07).
2. **Unidades:** propietarios únicos (`MassUnits.cs:12`, `ForceUnits.cs:32`), libra-fuerza derivada por multiplicación;
   `StructuralSectionUnits.cs:26` idéntico bit a bit; cantidades sin constructor ni conversión desde `double`; conversión única por
   `ForceUnits.ToPoundsForce`; sin literales de G4 en `StructuralAnalysis/**` (el `0.45359237` restante está en comentarios de
   `StructuralSectionUnits.cs`, excluidos por la guarda).
3. **Materiales:** solo por el perfil explícito verificado (`StructuralSourceMember` no tiene material; `StructuralModelBuilder.cs:335-346,
   396-397`); nadie lee `MaterialGrade`; el producto no trae filas. CD-I65-F1-06 es coherente con D-07 solo si en F3 el perfil neutral es
   proyección pura de la única tabla verificada del proveedor (A65-F1-01).
4. **Fallo cerrado:** solo `Valid` expone el modelo (`StructuralExportOutcome.cs:62-68`); un diagnóstico bloqueante no puede implicar
   `Valid` (`StructuralDiagnostic.cs:33-41`); el archivo de cargas inválido corta antes de consultar la fuente; precedencia por orden del
   enum (CD-I65-F1-04). Hueco residual: las excepciones se propagan en lugar de convertirse en diagnóstico (A65-F1-09).
5. **Idealización y golden:** coinciden con D-05 y §15; números rederivados (J = 20.498672113564037 − 6.35·7/192 = 20.26716169689737;
   distancia rígida 6.35/cos θ = 6.354218840504721; punta (36.22609804415268, 21.810300688090436); columna partida en cada J; apoyos SUP-01 y
   SUP-03 `[T,F,F,F,T,T]`; tolerancias de IDL-08). «Estación bloqueada → Unsupported» es literal de IDL-10.
6. **Provenance:** conforme a D-12 (huella de fuente con prefijos de longitud; huella de entrada con modelo canónico, hashes y versión del
   adaptador; forma canónica determinista). Riesgo residual real de CD-I65-F1-03: objeto de diseño y bytes llegan por separado (A65-F1-08).
7. **Guardas:** G1, G2, G3, G4, INV-16, INV-11 e INV-14 conformes; fricción prevista para F3/F4 (A65-F1-04, 05, 06).
8. **Supuestos como hipótesis (Q-O3):** ninguno afirma validación (una prueba prohíbe «validated», «certified», «approved», «safe»); faltan
   varias «limitaciones declaradas» de D-10 y el texto de ASM-PROP-01 es inexacto (A65-F1-07).
9. **Faltantes o desborde de alcance:** nada falta para la fila F1; sin código de F3-F6 ni de interfaz.

## Hallazgos (todos OPTIONAL)

| ID | Enunciado | Evidencia | Disposición recomendada |
|---|---|---|---|
| A65-F1-01 | El hash de filas de mapeo cubre solo la proyección neutral; en F3, cambiar nombres de RAM o valores esperados no cambiaría `AnalysisInputFingerprint` y abriría dos autoridades de material | `StructuralMaterialProfile.cs:95-101`; `StructuralFingerprints.cs:62-78, 101-121` | Contrato de F3: una sola tabla verificada del proveedor; el perfil neutral es su proyección; se hashea la fila completa |
| A65-F1-02 | `CanonicalText` une campos con `\|` sin escape ni prefijo de longitud: dos filas distintas pueden dar el mismo hash | `StructuralMaterialProfile.cs:95-101` | Prefijos de longitud, junto con A65-F1-01 en F3 |
| A65-F1-03 | IDL-09 está escrito como lista de permiso y el código es lista de rechazo (C, L): una familia futura pasaría como `Valid` | `StructuralModelBuilder.cs:325-328` | Lista de permiso W, HSS-RECT, S o prueba que fije el enum |
| A65-F1-04 | INV-14 exime solo su propio archivo; las pruebas de plantilla de F3 (INV-10) necesitan mencionar `ramelements` | `StructuralAnalysisSourceGuardTests.cs:400-403, 490-505` | El contrato de F3 nombra por ruta exacta los archivos eximidos |
| A65-F1-05 | Ediciones de guarda que F3/F4 deben hacer: la prueba afirma que `Providers/` no existe y que la lista de permiso de G1 está vacía | mismo archivo `:76`, `:83` | Listarlas en los contratos de F3/F4 como ediciones autorizadas |
| A65-F1-06 | INV-11 busca subcadenas sin límites de palabra en todo `src/**` | mismo archivo `:396-398` | Revisar alcance o límites de palabra en un gate posterior |
| A65-F1-07 | Faltan limitaciones declaradas de D-10 en los enunciados (SUP-02, CON-01, ASM-PROP-01, LOD-01) y la advertencia de «solo equivalencia»; ASM-PROP-01 dice que las propiedades de sección llegan por el mapeo de material | `CantileverStructuralModelSource.cs:464-473` | El manifiesto de F3 lleva las limitaciones de D-10 y la advertencia (ADR-0050 §5); corregir ASM-PROP-01 |
| A65-F1-08 | Objeto de diseño y bytes son entradas independientes; las pruebas hashean un diseño re-serializado | `CantileverStructuralModelSource.cs:57-61`; `StructuralModelBuilder.cs:16-27` | F4: una entrada neutral lee el sobre una vez, deserializa esos bytes y los hashea intactos; prueba de cambio solo de espacios |
| A65-F1-09 | Excepciones de la fuente o del resolver se propagan; el constructor acepta modelo vacío, apoyos duplicados por nudo e ids de supuesto ausentes | `StructuralModelBuilder.cs:156, 255-262` | El comando de F4 las captura como `Unavailable`; endurecer el constructor con una segunda fuente |
| A65-F1-10 | `Perpendicular` re-ortogonaliza en silencio el vector de peralte | `CantileverStructuralModelSource.cs:444-462` | Acotar la componente eliminada (p. ej. sen 0.01°) y rechazar lo mayor |
| A65-F1-11 | Un bloqueo de línea completa se informa como bloqueo de estación `station-N` | `CantileverStructuralModelSource.cs:94-97` | Redacción del diagnóstico en F4 |
| A65-F1-12 | La fuente toma solo la columna y los brazos `PositiveY`; un miembro estructural futuro en el plano se ignoraría en silencio | `CantileverStructuralModelSource.cs:112-142` | Si el producto lo añade, fallar cerrado ante miembros desconocidos (disparador de A-n) |

## CD-I65-F1-14: ¿material?

**No material; no requiere A-n.** La lectura literal de IDL-10 (toda estación con placas, separadores, arriostramientos, barras redondas o
cartelas es `Unsupported`) haría contradictorio al Freeze: toda línea Cantilever del producto los tiene por defecto; la fila F1 exige un
`REF-CANT-01` `Valid`; IDL-06, IDL-02, SUP-02 y SUP-03 presuponen esas piezas; OV-I65-03 e INV-05 no las listan. La lectura del Coordinator
es la única coherente (el argumento que la sostiene es la coherencia interna, no la redacción de V1). Se informa al Owner y se registra que
aplica solo a piezas fuera del plano o absorbidas por SUP-02/IDL-06, nunca a un miembro estructural futuro en el plano (A65-F1-12).

## Entradas y riesgos para F3/F4

1. Una tabla verificada del proveedor (D-07) con el perfil neutral como proyección; fila completa hasheada con prefijos de longitud
   (A65-F1-01, 02).
2. Ediciones de guarda de F3/F4 escritas en sus contratos: exención de INV-14 para las pruebas de plantilla, retirar la aserción «Providers
   no existe», ruta exacta de la lista de permiso de G1 en F4 (A65-F1-04, 05).
3. El manifiesto de F3 lista cada supuesto con su limitación de D-10 y la advertencia de solo equivalencia; ASM-PROP-01 corregido
   (A65-F1-07).
4. El comando de F4 une bytes y diseño de una sola lectura del sobre, captura excepciones como `Unavailable` y mantiene neutrales los tipos
   de lectura de vuelta del comparador (A65-F1-08, 09).
5. Pendientes de CD-I65-F1-19: transformación de D-04 del orden Tx..Rz al orden de RAM (INV-08) y prioridad de fusión de la columna.
6. IDL-09 como lista de permiso antes de que llegue una familia nueva (A65-F1-03).
