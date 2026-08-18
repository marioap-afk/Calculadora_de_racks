# I-39D — Caracterización base frente al contrato nuevo

> Evidencia de I-39D ([contrato](../../initiatives/I-39D-dialogos-y-utilitarias.md),
> [ADR-0029](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md)). Registro factual; no incluye
> conteos de pruebas ni hashes, que viven en `docs/HANDOFF.md` §12.

## 1. Por qué existe este documento

**La caracterización previa es inmutable.** Cuando un cambio contradice a propósito un comportamiento
caracterizado, la prueba original **no se reescribe**: se conserva intacta con `Skip` como evidencia
versionada, y el comportamiento nuevo se prueba en una clase separada. Así la transición se lee entera
en el historial —*comportamiento anterior caracterizado → cambio autorizado por ADR-0029 → comportamiento
nuevo probado*— en vez de aparecer como una prueba que siempre hubiera esperado lo nuevo.

El commit **`f6c4d12`** es la versión en que toda la caracterización de I-39D corría en verde contra el
árbol **sin tocar**.

## 2. Las cuatro que se conservan con `Skip`

| Prueba | La base decía | El contrato dice | Autoriza | Lo prueba |
|---|---|---|---|---|
| `NingunaDeLasDieciseisAplicaUnEstiloDeVentanaCompartido` | ninguna de las dieciséis aplica estilo de ventana | **nueve** aplican `DialogWindowStyle` | D9, D11 | `LasNueveAplicanElContratoDeVentanaDelArquetipoC` |
| `SafetyDefensaEsLaUNICAQueNoAsignaChromeYSuDeltaREALEsElFONDO` | abre en **blanco liso**, sin el fondo compartido | abre con el mismo `#F4F6F9` que sus nueve hermanas | D9 | `SafetyDefensaYaAbreConElFondoCompartidoComoSusNueveHermanas` |
| `NingunaDeLasDieciseisConsumeEditorActionNiEditorActionBar` | cero consumidores de la fábrica común | las **dos de almacén** construyen su barra con ella | D11, decisión 26 | `LasDosDeAlmacenConstruyenSuBarraConLaFabricaComun` |
| *(ver §3)* | | | | |

## 3. Las dos que **no** pueden conservarse con `Skip`, y por qué

`RackDialogWindowNoTieneNiUnaSolaSubclaseProductiva` y `RackDialogWindowAsignaChromeComoVALORLOCAL`
caracterizaban al tipo que I-39D **retira**. Una prueba omitida sigue teniendo que **compilar**, y ya no
hay tipo al que referirse, así que `Skip` no es una opción. Su cuerpo se transcribe aquí palabra por
palabra y sigue siendo ejecutable en `f6c4d12`.

```csharp
[Fact]
public void RackDialogWindowNoTieneNiUnaSolaSubclaseProductiva()
{
    var derived = typeof(RackCad.UI.Controls.RackDialogWindow).Assembly
        .GetTypes()
        .Where(t => typeof(RackCad.UI.Controls.RackDialogWindow).IsAssignableFrom(t)
            && t != typeof(RackCad.UI.Controls.RackDialogWindow))
        .Select(t => t.FullName)
        .ToList();

    Assert.Empty(derived);
}

[Fact]
public void RackDialogWindowAsignaChromeComoVALORLOCAL()
{
    StaTestRunner.Run(() =>
    {
        // El obstaculo estructural medido: en precedencia WPF un valor local GANA al setter de un estilo, asi
        // que mientras la base asigne fondo y tipografia en su constructor, ningun estilo de ventana del
        // arquetipo C podria cambiarlos en una subclase suya.
        var dialog = new RackCad.UI.Controls.RackDialogWindow();

        Assert.Equal("Segoe UI", dialog.FontFamily?.Source);
        Assert.NotNull(dialog.Background);
        Assert.Equal(WindowStartupLocation.CenterOwner, dialog.WindowStartupLocation);

        var source = UiSource("Controls", "RackDialogWindow.cs");
        Assert.Contains("FontFamily = new FontFamily(\"Segoe UI\")", source, StringComparison.Ordinal);
        Assert.Contains("WindowStartupLocation = WindowStartupLocation.CenterOwner", source, StringComparison.Ordinal);
    });
}
```

Lo que dicen sigue siendo cierto sobre la base: el tipo nunca tuvo un adoptante, y su forma de asignar el
chrome era el obstáculo estructural que impedía dárselo. El contrato nuevo lo prueban
`WindowCensusGuardTests.RackDialogWindowYaNoExiste` —el tipo no puede reaparecer sin que alguien lo
note— y `DialogWindowContractTests`, que fija dónde vive cada una de sus dos mitades.

## 4. Lo que se revisó y **no** se cambió

- **La ubicación de las cinco ventanas sin padre WPF posible.** Declaran `CenterOwner` sin que pueda
  existir `Owner`, lo que en WPF degrada silenciosamente a `CenterScreen`. Cambiarlo a `CenterScreen`
  explícito **mueve cinco ventanas de producción validadas** sin corregir ningún defecto observado: el
  producto lleva meses en uso y la ubicación nunca se reportó. Queda **caracterizado y documentado con su
  motivo**, que es exactamente lo que D9 pide, y la decisión de moverlas es del Owner, aislada.
- **La paleta de estado.** Siete archivos pintan su aviso con `Firebrick` en vez del `#B00020` de
  `EditorStatusPalette`. Unificarla cambia el color de siete ventanas de golpe y **no corrige ningún
  defecto**: es una decisión estética del Owner, no una violación de contrato.
- **Los cuatro mapeos `SafetySide` → `ComboBox`.** Dos no ofrecen «Ninguno» **a propósito** —la presencia
  la decide la matriz— y colapsan los valores intermedios. Es regla de producto (§4.2 de la auditoría).
- **Las etiquetas Todos/Ninguno frente a Todas/Ninguna.** Concordancia de género: regla de producto.
- **El foco inicial de las cuatro rejillas.** Su primer control de captura **es la matriz**, que no es
  enfocable como unidad, y apuntar el foco al par Aceptar/Cancelar lo pondría sobre una acción. El arreglo
  correcto exige que `SelectionMatrix` acepte foco, que es un cambio de **control** y no de arquetipo.
  Medido y registrado, no diferido en blanco.

## 5. Trazabilidad

- Base en verde sobre el árbol sin tocar: commit `f6c4d12`.
- Caracterización: `tests/RackCad.UI.Tests/DialogWindowCharacterizationTests.cs`.
- Contrato nuevo: `tests/RackCad.UI.Tests/DialogWindowContractTests.cs` y
  `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`.
- Auditoría de apertura: [`I-39D-auditoria-dialogos-y-utilitarias.md`](I-39D-auditoria-dialogos-y-utilitarias.md).
