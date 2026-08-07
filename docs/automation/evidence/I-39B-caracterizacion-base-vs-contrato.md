# I-39B — Las tres pruebas de caracterización que cambiaron, y por qué

> La caracterización previa es **inmutable**. Tres de las doce describían la base anterior a la mitad
> observable de I-39B y no pueden pasar contra el código nuevo. **No se reescribieron**: se conservan
> intactas, con su texto original, marcadas `Skip` como evidencia versionada, y el contrato nuevo vive
> en una clase separada.

La transición se lee entera en tres sitios:

1. **`b61182f`** — «I-39B: caracterizacion de las seis ventanas del arquetipo A», donde las tres
   corrían **en verde contra la base**.
2. **ADR-0029 D7 y D8**, aceptados, que autorizan el cambio.
3. **`RichEditorCloseContractTests`**, que prueba el comportamiento nuevo.

Ninguna prueba que describía el comportamiento antiguo fue convertida para fingir que siempre esperó
el nuevo.

## Diferencia 1 — Escape en Push Back

| | |
|---|---|
| **Base** | `RichEditorCharacterizationTests.FiveOfTheSixCloseWithEscapeAndPushBackDoesNot` |
| **Contrato** | `RichEditorCloseContractTests.TheSixCloseWithEscape` |
| **Autoriza** | ADR-0029 **D7**: «Escape no puede provocar pérdida silenciosa de cambios» y el cierre por las cuatro rutas atraviesa la misma política |

**Aserción base**, literal y conservada:

```csharp
// INCUMPLIMIENTO CARACTERIZADO, no corregido: Push Back es la unica sin IsCancel, asi que Escape NO
// la cierra. Es tambien la unica con un ambito dirty explicito, y por eso anadirle IsCancel sin
// politica de cierre convertiria Escape en un descarte silencioso.
Assert.Null(Cancel(PushBack()));
```

**Aserción nueva:** las **seis** tienen un botón `Cerrar` con `IsCancel`.

**Qué lo hace legítimo:** el `IsCancel` se añadió **después** de que existiera `OnClosing` con el
ámbito declarado. El orden inverso es exactamente lo que la aserción base advertía que no se hiciera.

## Diferencia 2 — Intercepción del cierre

| | |
|---|---|
| **Base** | `RichEditorCharacterizationTests.NoRichEditorInterceptsItsClosing` |
| **Contrato** | `RichEditorCloseContractTests.OnlyTheEditorsWithADeclaredScopeInterceptTheirClosing` |
| **Autoriza** | ADR-0029 **D7** y **D8** |

**Aserción base:** ninguna de las seis declara `OnClosing`; el botón, Escape, `Alt+F4` y el botón de
sistema **no atraviesan ninguna política**.

**Aserción nueva:** lo declaran **exactamente dos** —Push Back y Cabecera, las que tienen ámbito
transaccional— y **no** lo declaran las otras cuatro.

**Qué lo hace legítimo:** D8 admite «no aplicable» como valor legítimo de dirty, así que las cuatro sin
ámbito siguen cerrando directo. La prueba nueva es **más fuerte** que la base: comprueba las dos
direcciones, no solo la ausencia.

## Diferencia 3 — El ámbito declarado y el cierre

| | |
|---|---|
| **Base** | `RichEditorCharacterizationTests.NeitherDeclaredScopeIsConsultedWhenTheWindowCloses` |
| **Contrato** | `RichEditorCloseContractTests.ClosingWithoutPendingWorkStillAsksNothing` y `EditorClosePolicyTests` |
| **Autoriza** | ADR-0029 **D8**: dirty pertenece a un ámbito y la política de cierre lo agrega |

**Aserción base:** el ámbito existe (`ModuleSession`, `HasUnsavedManualEdits`) y **el cierre no lo
consulta**; cerrar con cambios pendientes los descarta sin preguntar.

**Aserción nueva:** el cierre **sí** lo consulta. Sin trabajo pendiente no aparece ningún diálogo —eso
es lo que fija la prueba de contrato— y el caso **con** trabajo pendiente, confirmando y rechazando, lo
cubre `EditorClosePolicyTests`.

**Qué lo hace legítimo:** la protección ya existía en la Cabecera y el cierre la evitaba; reutilizarla
es lo que el contrato de I-39B pide, no un modelo nuevo.

## Las nueve restantes

`TheDefaultActionOfEachRichEditorIsWhatItIsToday`, `ClosingARichEditorNeverMaterialisesAnything`,
`OnlyTwoRichEditorsDeclareATransactionalScope`, `FourRichEditorsAreComposedOverTheSharedShellAndTwoAreNot`,
`TheFourShellEditorsShareTheArchetypeSizeContractAndTheOtherTwoDoNot`, `AllSixDeclareCenterOwner`,
`FiveDeclareAnInitialFocusAndTheHeaderConfiguratorDoesNot`, `NoRichEditorDeclaresAnExplicitTabOrder` y
`NoRichEditorConsumesTheSharedActionOrStatusInfrastructure` **no se tocaron** y siguen ejecutándose en
verde: son la prueba de que el resto de la base no se movió.

## Diferencia 4 — Foco inicial de la Cabecera

| | |
|---|---|
| **Base** | `RichEditorCharacterizationTests.FiveDeclareAnInitialFocusAndTheHeaderConfiguratorDoesNot` |
| **Contrato** | `RichEditorContractTests.TheSixDeclareAnInitialFocus` y `TheHeaderConfiguratorFocusesItsModelTreeAndNothingDestructive` |
| **Autoriza** | ADR-0029 **D9**: «el foco inicial y el orden de tabulación son deterministas y no recaen en una acción destructiva ni bloqueada» |

**Aserción base:** cinco declaran `FocusManager.FocusedElement` y la Cabecera **no**, ni por
`FocusManager` ni por una llamada a `Focus()`.

**Aserción nueva:** las **seis** lo declaran; el destino de la Cabecera es su árbol de modelo, que es un
`TreeView` y no un `Button`.

**Qué lo hace legítimo:** D9 pide el «selector o lista principal» cuando no hay campo requerido
incompleto, y el árbol es el control principal de esa ventana. No mueve ningún flujo de producto.

## Nota sobre el método

Las cuatro diferencias se caracterizan **leyendo la fuente** allí donde el hecho es una declaración
(`IsCancel`, `FocusManager.FocusedElement`, `OnClosing`). Es el idiom de guarda del repositorio y evita
que la aserción dependa de que un `Binding` por `ElementName` se haya resuelto, cosa que exige mostrar
la ventana y activar su ámbito de foco.
