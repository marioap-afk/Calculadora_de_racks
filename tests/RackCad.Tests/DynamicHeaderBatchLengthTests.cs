using RackCad.Application.Systems.Dynamic;
using Xunit;
using F = RackCad.Tests.DynamicHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G6 — longitud y <c>IsManualOverride</c> (ADR-0037; Proposal V2 §7.3, §7.8; matriz §13.4: D-14 y D-15).
    ///
    /// <para>
    /// <c>IsManualOverride</c> es SOLO longitud manual; la personalizacion de la cabecera es
    /// <c>UseCalculatedHeaderConfiguration = false</c>. DISTRIBUTE no toca la longitud (D-12, en MUTATE). EDIT la toca solo
    /// por la regla existente de la ventana: si el fondo editado difiere de la longitud mas de 0.0001, la longitud pasa a ser
    /// ese fondo y queda manual; si no, todo queda como estaba.
    /// </para>
    /// </summary>
    public class DynamicHeaderBatchLengthTests
    {
        // ===== D-14 — DISTRIBUTE y despues otra tarima ===================================================================

        [Fact]
        public void D14_DISTRIBUTE_Y_CAMBIO_DE_TARIMA_LA_LONGITUD_SIGUE_AL_LAYOUT_Y_LA_PERSONALIZADA_SE_CONSERVA_ADAPTADA_E_INFORMADA()
        {
            var editor = new F.Editor(F.Design(palletsDeep: 9, palletDepth: 48.0));
            editor.Customize("M1", 141.0);
            F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", editor.Explicit("M3")))));
            var destino = editor.Module("M3");
            Assert.False(destino.IsManualOverride);                                // premisa: sin longitud manual
            Assert.Equal(48.0, destino.Length, 6);
            var receta = F.Recipe(destino.AssociatedFrameConfiguration);

            var result = editor.Rebuild(palletDepth: 40.0);

            var reconstruido = editor.Module("M3");
            Assert.Equal(40.0, reconstruido.Length, 6);                             // sigue al layout nuevo
            Assert.False(reconstruido.IsManualOverride);
            Assert.True(reconstruido.IsCalculated);
            Assert.False(reconstruido.UseCalculatedHeaderConfiguration);
            Assert.Equal(receta, F.Recipe(reconstruido.AssociatedFrameConfiguration));
            Assert.Equal(40.0, reconstruido.AssociatedFrameConfiguration.Depth, 6);
            Assert.Contains("M3", result.Reconciliation.Preserved);
            Assert.Contains("M3", result.Reconciliation.Adapted);
            Assert.False(result.Reconciliation.LostAnything);
            Assert.Contains("M3", result.Describe());
        }

        // ===== D-15 — EDIT y la regla existente del fondo editado =======================================================

        [Fact]
        public void D15_EDIT_CON_UN_FONDO_EDITADO_DISTINTO_DE_LA_LONGITUD_LA_FIJA_COMO_LONGITUD_MANUAL()
        {
            var editor = new F.Editor(F.Design());
            Assert.False(editor.Module("M5").IsManualOverride);
            var resultado = F.Custom(151.0, depth: 60.0);

            var preparation = editor.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(resultado, "M5")));
            var plan = F.Prepared(preparation);
            Assert.Equal(DynamicHeaderBatchOperation.Edit, preparation.Operation);
            Assert.Equal(new[] { "M5" }, F.Ids(plan.Targets));
            Assert.Empty(plan.Omitted);
            Assert.Equal(48.0, editor.Module("M5").Length, 6);                     // PREPARE no aplica el escalar

            F.Committed(editor.Apply(preparation));

            var modulo = editor.Module("M5");
            Assert.Equal(60.0, modulo.Length, 6);
            Assert.True(modulo.IsManualOverride);
            Assert.False(modulo.IsCalculated);
            Assert.False(modulo.UseCalculatedHeaderConfiguration);
            Assert.NotSame(resultado, modulo.AssociatedFrameConfiguration);
            Assert.Equal(151.0, modulo.AssociatedFrameConfiguration.PanelClear);
            Assert.Equal(60.0, modulo.AssociatedFrameConfiguration.Depth, 6);
            Assert.Equal(modulo.EndX, editor.Module("M6").StartX, 6);               // posiciones recalculadas
            Assert.Equal(456.0, editor.System.TotalLength, 6);                       // 444 + 12
        }

        [Theory]
        [InlineData(false, 48.0)]
        [InlineData(false, 48.00005)]
        [InlineData(true, 50.0)]
        public void D15_EDIT_SIN_CAMBIO_DE_FONDO_DEJA_LA_LONGITUD_Y_ISMANUALOVERRIDE_COMO_ESTABAN(bool manual, double fondoEditado)
        {
            var editor = new F.Editor(F.Design());
            if (manual)
            {
                editor.ManualLength("M5", 50.0);
            }

            var longitud = editor.Module("M5").Length;
            var calculada = editor.Module("M5").IsCalculated;

            F.Committed(editor.Apply(editor.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(F.Custom(152.0, depth: fondoEditado), "M5")))));

            var modulo = editor.Module("M5");
            Assert.Equal(longitud, modulo.Length);
            Assert.Equal(manual, modulo.IsManualOverride);
            Assert.Equal(calculada, modulo.IsCalculated);
            Assert.False(modulo.UseCalculatedHeaderConfiguration);
            Assert.Equal(longitud, modulo.AssociatedFrameConfiguration.Depth, 6);
        }

        [Fact]
        public void EDIT_CAPTURA_EL_RESULTADO_DEL_CONFIGURADOR_AL_PREPARAR_Y_NUNCA_INSTALA_ESA_INSTANCIA()
        {
            var editor = new F.Editor(F.Design());
            var resultado = F.Custom(153.0);
            var preparation = editor.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(resultado, "M3")));
            F.Prepared(preparation);

            resultado.PanelClear = 999.0;                                          // el configurador sigue vivo

            F.Committed(editor.Apply(preparation));
            var instalada = editor.Module("M3").AssociatedFrameConfiguration;
            Assert.NotSame(resultado, instalada);
            Assert.Equal(153.0, instalada.PanelClear);
        }
    }
}
