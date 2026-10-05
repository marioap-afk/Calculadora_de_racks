# I-63 — Proposal V3 · Amendment A-3: resultado diagnóstico concreto de INV-22

```text
Amendment          = A-3 (tercera de la secuencia; append-only)
Tipo               = Coordinator-only, preautorizada por la orden de autorización escalonada de G3 (decisiones de I-63 §2,
                     «Autorización escalonada de G3»); la materializa la sesión principal sin volver, dentro de sus límites
Freeze aplicable   = Proposal V3 congelada: docs/initiatives/I-63-proposal-v3.md
                     commit de Freeze f61d0aca859a11b15cbe1797069a83cba873ba95, blob 4d5dedce15363fa6378e460c5b63005fe41b858b
Amendments previas = A-1 docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md (commit 658b35ad498520ba3c832a96eea4389df16dfa60)
                     A-2 docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md (commit 669d8a391f208e1077f136a406fd89058bc0ce6e)
Applies-to         = all
Origen             = Proposal V3 §13 (D-16.6), §20 (INV-22) y AQ-02: «El código concreto se fija en el commit RED de G3, con una A-n
                     solo del Coordinator»
RED que lo observó = 637dce7e5e7811992330b3c305358d9a7542b9f6 (G3-T1, WorkRunId R20261002T184507Z-9800;
                     verificación R20261002T194212Z-73b1, RED acreditado: RedPart = pass en Ci y en Tests)
Materialidad       = ninguna de M-01..M-08 (§6)
```

## 1. Qué es y qué no es

- **Es** la enmienda que el Freeze reservó para fijar el resultado diagnóstico concreto de INV-22. El resultado se observó en el RED de
  G3; no se eligió por inspección estática.
- **No** crea códigos de diagnóstico: usa el catálogo V6 vigente de I-49 (`ExpressionDiagnosticCatalogTests`, sin cambios).
- **No** cambia D-16, el lexer, el parser, `projectVariable` ni el alcance. No edita la Proposal V3, la A-1 ni la A-2: se lee junto a ellas.
- Condición de la preautorización cumplida: bastan códigos existentes del catálogo, así que C-10 no se activa.

## 2. Resultado fijado (INV-22)

Para un `SymbolId` del namespace `rack` **ausente** de la tabla, el formatter produce `Rack.#{<token>}` (D-16.6). Para el oráculo de
INV-22, `Rack.#{zzz}`, el resultado de analizar ese texto es:

| Aspecto | Resultado fijado |
|---|---|
| Código | **Uno solo**: `InvalidQualifier` (valor 11 del catálogo V6, clase `SyntaxAndLimits`), con `Limit` nulo |
| Span | Empieza en el desplazamiento 5, justo después de `Rack.`, y cubre el lexema `#{zzz}`: inicio 5, longitud 6 |
| Árbol sintáctico | Ninguno: el análisis no tiene éxito y leer `Syntax` lanza `InvalidOperationException` |
| Árbol enlazado | Ninguno: sin árbol sintáctico no hay nada que enlazar; el binder no se alcanza |
| Determinismo | El mismo resultado en análisis repetidos y con operadores a continuación (`Rack.#{zzz} * 2`, `Rack.#{zzz}+1`) |

El mismo resultado vale para cualquier clave cuyo contenido entre llaves no sea un GUID que `Guid.TryParse` lea; entre ellas están todas
las claves del catálogo cerrado de métricas de G1 (`frentes`, `frentesVacios`, `totalRacks`, `rackCount`, `totalFrentes`,
`totalFrentesVacios`). El span va del desplazamiento 5 con longitud igual a la del token más 3; para `Rack.#{frentes}` es 5+10.

**Observación del RED** (sonda desechable de la sesión de G3-T1 contra el código de `637dce7e`; salida literal):

```text
TEXT: Rack.#{zzz}
  parse.Succeeded=False
  parse diagnostics: InvalidQualifier@5+6
    InvalidQualifier class=SyntaxAndLimits span=5+6 limit=
TEXT: Rack.#{zzz} * 2
  parse.Succeeded=False
  parse diagnostics: InvalidQualifier@5+6
    InvalidQualifier class=SyntaxAndLimits span=5+6 limit=
TEXT: Rack.#{zzz}+1
  parse.Succeeded=False
  parse diagnostics: InvalidQualifier@5+6
    InvalidQualifier class=SyntaxAndLimits span=5+6 limit=
TEXT: Rack.#{zzz} parse.Succeeded=False diags=InvalidQualifier@5+6
    InvalidQualifier class=SyntaxAndLimits span=5+6
  Syntax throws InvalidOperationException
TEXT: Rack.#{frentes} parse.Succeeded=False diags=InvalidQualifier@5+10
    InvalidQualifier class=SyntaxAndLimits span=5+10
  Syntax throws InvalidOperationException
```

## 3. Relación con D-16.6

- D-16.6 congela el comportamiento: `Rack.#{<token>}` se muestra, nunca enlaza y es un error de sintaxis. Además añadía como
  `[RECONSTRUCTED]` un mecanismo: `NamespaceReferenceSyntax` sin miembro seguida de un cualificador suelto
  (`ExpressionSyntaxParser.cs:224-228`).
- El RED observa que, para el oráculo, el rechazo ocurre antes, en el lexer. Según P3.9, el contenido de `#{...}` tiene que ser una clave
  válida de `projectVariable`: un GUID que `Guid.TryParse` lea (`SymbolId.IsValidProjectVariableKey`). Si no lo es, todo el lexema es
  **un** token `InvalidQualifier` (`ExpressionLexer.cs`, `ScanBracedKey`). Ese código es de la clase `SyntaxAndLimits`, es decir, un
  error de sintaxis.
- El comportamiento congelado se cumple tal cual. Esta enmienda solo fija el código concreto que el Freeze dejó a INV-22. Para el oráculo,
  la explicación reconstruida queda sustituida por el hecho observado.

## 4. Límite de lo fijado: clave `rack` legible como GUID

- La regla de clave `rack` que fija el RED (`^[a-z][A-Za-z0-9]*`, ASCII) excluye guiones y llaves. Aun así, admite 32 dígitos
  hexadecimales que empiecen por `a`-`f`, que `Guid.TryParse` lee en forma N.
- Para una clave así, el lexer produce un `Qualifier` válido y el parser sigue el mecanismo reconstruido de D-16.6: un
  `NamespaceReferenceSyntax` sin miembro seguido de un cualificador suelto. El resultado es **un** `UnexpectedToken` (valor 3, clase
  `SyntaxAndLimits`) sobre el cualificador, sin árbol y determinista. También es un código existente y un error de sintaxis.
- **Observación de la sesión principal** (sonda desechable fuera del worktree, contra el `RackCad.Application.dll` Debug del worktree,
  SHA-256 `20f6ef4b6747e64808d98506b2b219fd74774f037256709efc26aac5cbcd7918`). `src/RackCad.Application/Expressions/` es idéntico en
  `95690c28` (main) y en `637dce7e`. Salida literal:

  ```text
  Rack.#{zzz} | Succeeded=False | InvalidQualifier(11)/SyntaxAndLimits@5+6 | Syntax throws InvalidOperationException
  Rack.#{frentes} | Succeeded=False | InvalidQualifier(11)/SyntaxAndLimits@5+10 | Syntax throws InvalidOperationException
  Rack.#{abcdef0123456789abcdef0123456789} | Succeeded=False | UnexpectedToken(3)/SyntaxAndLimits@5+35 | Syntax throws InvalidOperationException
  Rack.#{abcdef0123456789abcdef0123456789} + 1 | Succeeded=False | UnexpectedToken(3)/SyntaxAndLimits@5+35 | Syntax throws InvalidOperationException
  ```

- Ninguna clave del catálogo cerrado de G1 tiene esa forma, y ninguna prueba del RED cubre este caso. Por eso esta enmienda **lo
  declara y no lo fija como resultado de INV-22**. Cualquiera de las dos rutas cumple lo congelado: se muestra, nunca enlaza y es un
  error de sintaxis con un código del catálogo vigente.

## 5. Pruebas que lo fijan (RED `637dce7e`, protegidas)

En `tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsFormattingTests.cs`:

- `INV22_TheDiagnosticForm_NeverParses_ItIsOneInvalidQualifierAndNoTree`: tres filas, verdes desde el RED porque fijan el estado
  vigente del analizador.
- `INV22_AnAbsentRackId_IsShownAsRackDotHashBraceToken` e `INV22_FormatThenParse_OfAnAbsentRackId_GivesTheFixedDiagnostic_AndNeverATree`:
  rojas en el RED; las pone en verde G3-T2.

## 6. Materialidad

| ID | Estado | Razón |
|---|---|---|
| M-01..M-08 | **No activados** | Fija el código concreto que el Freeze reservó a INV-22, dentro del catálogo vigente, sin cambiar comportamiento, DTO, contratos consumidos, semántica de fallo, mecanismos ni ADR |

## 7. Evidencia

- Orden de autorización escalonada de G3, con la A-3 preautorizada: decisiones de I-63 §2.
- Entrega y verificaciones de G3-T1, y sondas de INV-22: evidencia de I-63 §41.
